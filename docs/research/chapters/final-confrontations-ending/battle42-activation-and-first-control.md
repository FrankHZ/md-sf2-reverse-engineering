# Battle42 activation and first control

Evidence date: 2026-10-22. **Confirmed (static):** the twelve fresh
formation points miss all four sloped regions. Four HIDDEN slots can
instead initialize from held region flags and unoccupied starts. Neutral
Elis, combat159 and script alias31 have distinct roles. Galam's refreshed
rawAGI190 contributes two queue entries. These are state-dependent source
contracts, not a natural first actor or a playable Battle42 remake.
Stop before a concrete movement, spell, item or attack decision.

Continue at [Battle42 outcomes, Galam aftermath and Zeon admission](./battle42-outcomes-and-zeon-admission.md).

## Agreement, provenance and method

Standard rigor for [Issue #1172](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1172)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted base is `91f5a70ab5814da596af76f0a17e7fcf84814885`, tree
`42e5b19331e0da2d2d1a1abe39dd82a13d6b52e8`. The outcome is a reusable
activation, hidden-spawn, queue and first-control contract for later
content/engine work. The four slopes, four hidden declarations, neutral
alias and Galam refresh require one coherent boundary. Own this document
and successor navigation only in
[Battle42 initialization](./battle42-initialization-and-opening.md).
No index/schema/fixture/extractor/engine changes or unmerged dependencies.
Damage, drops, outcomes, Galam aftermath and ending content are excluded.

Reuse [Battle38 hidden activation](../moun-nazca-grans/battle38-activation-and-first-turn-dispatch.md),
[Battle40 regions](./battle40-regions-terrain-and-first-control.md),
[Battle41 first control](./battle41-opening-activation-and-first-control.md),
[battle loop](../../battle/battle-loop.md), [stats](../../party/common-stats.md) and the
predecessor's full initialization/neutral provenance. Selected42 operands
and changed legal inputs are checked here; no earlier opening is lowered.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`. sf2const.asm, sf2enums.asm
and the placement macros own widths/constants. Fnnn denotes a story flag;
addresses/packed words are hexadecimal, ordinals zero-based and intervals
half-open. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality, decoded tactical terrain or runtime claim.

Method: linear selected source and actual consumer inspection, plus four
128×128 nonnegative signed-byte coordinate checks:65,536 region queries.
Keep only counts and a few counterexamples, not a full coordinate corpus.
The accepted small integer arithmetic is reused; cross-product comparison
is diagnostic, not the native oracle. No all-state Cartesian product,
map/terrain export, new comparator, pair reload/regeneration or lowering.
The unchanged18,285,979-byte static pair's existing provenance is
`local/issue1005/pair.txt`; it is not needed to evaluate these instructions.
Ignored recipe/results belong to `local/issue1172/`. Direct analysis is
seconds-scale, selected evidence budget<2MiB and total including CI/
coordination<4MiB; peak-memory target512MiB is **unmeasured**. Reassess
before runtime, new machinery or materially larger work.

## Round prefix and live selectors

**Confirmed:** `battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) continues after fresh LoadBattle and the independent
F492 empty start hook:

1. ActivateEnemies scans128..159, then falls into its helper once for160.
2. ExecuteBattleRegionCutscene rereads CURRENT_BATTLE.
3. Nonzero DEBUG_MODE_TOGGLE.b requests PrintAllActivatedDefCons.
4. PopulateTargetsListWithSpawningEnemies rebuilds TARGETS_LIST_LENGTH.w.
   Each admitted byte ID is widened from a cleared word and passed to
   SpawnEnemyWithCamera in ascending scan order; each call must return.
5. GenerateBattleTurnOrder fills/sorts the queue and clears its cursor.
6. The individual loop clears d0.w, reads CURRENT_BATTLE_TURN.b as a
   byte offset and reads the actor byte. FF returns to step1; otherwise
   ExecuteIndividualTurn runs, with a fresh HP check.

`battle/cutscenes/regionactivatedcutscenes.asm`
(`$47E82..$47EC8`) reads the complete table in
`data/battles/cutscenes/regionactivatedcutscenes.asm`:32/0/F386 and
32/1/F386→rbcs_battle32;40/0/F387→rbcs_battle40_1;
40/1/F388→rbcs_battle40_2; FFFF terminates. Live42 misses before
played/region checks: no Battle42 region program exists to lower.
For a changed32/40 selector, played checking precedes region checking;
SetFlag precedes MAPSCRIPT. No snapshot or rollback protects selector,
flag, combatant or service changes between calls.

Debug printing visits flags90..105, requests txt463 with region number
0..15 for each held flag, then closes. It does not create a story region
scene. Unchanged fresh42 requests none; independently held undeclared
flags can still print. No new text lowering or timing observation.

Held F88 follows the predecessor's retained-entry boundary: restore
seconds, clear88/AI memory/view target, LoadBattle with incoming d0 and
jump directly to the retained individual loop. It skips initial
activation/spawn/generation. Incoming d0.w=59 reads alias offset59 at
FFB17B, inside the64-byte table:enemy155, not ally59. Fresh155 is absent/aliasFF; retained
position/neutral changes can populate it. Negative alias falls back to
physical0 for coordinates; another alias lacks a49-record guard. This
is not a safe installed view selector or a suspend observation.
Queue/cursor/RNG/checked word and live combatants remain inputs. A
retained FF may later reach the prefix; no valid restored battle is assumed.

F542 has two independent owners: Map59's flag-layout copy and neutral
creation during entity packing. Neither is a Battle42 region hook.
The layout copy transfers28 words/56 content bytes in the working map;
BattleTerrain42 is loaded separately. No tactical mutation or collision
shape follows from the layout table. Those facts remain with the predecessor.

## Four regions and original integer geometry

**Confirmed:** `data/battles/spritesets/spriteset42.asm` has12 allies,
19 enemies,4 regions and1 point. Region records are12 bytes each;
type4/padding0, ordered vertex pairs, trailing0,0. Point0 is two bytes.

| Region / flag / address | V1 | V2 | V3 | V4 |
| --- | --- | --- | --- | --- |
|0 /90 /1B6A28|0,0|0,31|25,22|25,0|
|1 /91 /1B6A34|0,0|0,21|25,32|25,0|
|2 /92 /1B6A40|7,15|7,18|11,20|18,15|
|3 /93 /1B6A4C|11,15|11,20|18,18|18,15|
|Point0 /1B6A58|13,18|—|—|—|

`battle/battleloop/triggerregions.asm:IsRegionTriggered`
(`$1AC9FC..$1ACF30` contains the complete helpers) tests triangle1/2/4,
then3/2/4 only after a miss. AreAnyAlliesInTriangle scans all30 ally
slots, requiring signed currentHP.w>0 and signed X.b/Y.b≥0. Membership,
active force, enemies, neutral, terrain/collision and occupancy are not
predicates. HP8000..FFFF, HP0 or either coordinate128..255 excludes
that ally; another eligible ally may still trigger. A living positioned
nonmember can trigger. Coordinates are battle-relative; no world/camera
offset is added.

The segment helper sign-extends coordinate bytes. For segment A→B and
test C it chooses horizontal when |Bx−Ax|≥|By−Ay|, including equality:

- Horizontal: `Yproj = By + trunc0((By−Ay)×(Cx−Bx)/(Bx−Ax))`.
  A zero X denominator falls through to vertical.
- Vertical: `Xproj = Bx + trunc0((Bx−Ax)×(Cy−By)/(By−Ay))`.
- MULS/DIVS are signed; trunc0 is toward zero. Projection is based at
  **B**, not A. The selected products/quotients fit the relevant widths
  throughout the checked domain; this does not validate arbitrary bytes.
- Negative projection synthesizes2 despite the source comment's3.
  Equal projection within the inclusive dominant-axis segment bounds
  returns1, an immediate triangle hit. Equal but unbounded returns2;
  otherwise side is2 if projection<test coordinate,0 if greater.
- Triangle edges1→2,3→1,3→2 compare C's side against the opposite
  vertex's side unless the immediate bounded-edge result already accepted.

The native region-index guard rejects FF separately but uses signed
cmp.b/ble against count4, admitting equality4 and other signed cases.
Ordinary iteration is only0..3. Do not interpret the guard as a safe
public table-index API or read beyond the selected table.

| Formation ordinal | X,Y | Native hit regions |
| --- | --- | --- |
|0|13,32|none|
|1|14,33|none|
|2|13,33|none|
|3|12,33|none|
|4|11,33|none|
|5|14,32|none|
|6|12,32|none|
|7|11,32|none|
|8|14,34|none|
|9|13,34|none|
|10|12,34|none|
|11|11,34|none|

These are formation inputs; actual living/active party placement may be
a subset. The finite selected-domain check gives:

| Region | Native hits /16,384 | Differences from exact inclusive cross-product triangles |
| --- | --- | --- |
|0|727|24|
|1|703|0|
|2|48|5|
|3|43|6|

All differences are native inclusions absent from the exact polygon.
Examples are R0:(1,31),(3,30); R2:(13,19),(14,18);
R3:(12,20),(15,19). R0's sloping bottom at X1 projects from
B=(0,31) in triangle3/2/4:31+trunc0(−9/25)=31, accepting(1,31),
although the exact bottom is30.64. R2 triangle3/2/4's second edge
(region V4→V3) at(13,19) uses B=(11,20):20+trunc0(−5×2/7)=19.
These rounded edge hits are not
evidence for a rectangular area or generic polygon parity.

| Supplied eligible ally point | Hit region IDs | Adjacent point | Hit region IDs |
| --- | --- | --- | --- |
|0,31|0|0,32|none|
|25,22|0,1|25,23|1|
|24,22|0,1|24,23|0,1|
|0,21|0,1|0,22|0|
|25,32|1|25,33|none|
|24,31|1|24,32|none|
|7,18|0,1,2|7,19|0,1|
|11,20|0,1,2,3|11,21|0,1|
|12,20|0,1,3|12,21|0,1|
|18,18|0,1,3|18,19|0,1|
|17,18|0,1,3|17,19|0,1,3|
|9,17|0,1,2|17,17|0,1,3|
|13,18|0,1,2,3|11,19|0,1,2,3|

These supplied states establish arithmetic predicates, not movement,
walkability, natural reach, party identity or independent probabilities.

## Checked word, activation and every selected assignment

**Confirmed:** ActivateEnemies (`$2550C..$25544`) requires signed X.b≥0,
HP.w nonzero and negative actor d0.b. It has no Y, membership, neutral,
already-active or priority gate. Thus HP8000 passes this guard, unlike
ally geometry. Fresh15 STARTING enemies pass; hidden132..135 and absent
147..159 fail. Elis159 is positioned but still fails HP0.

TriggerRegionsAndActivateEnemies reads NEWLY_TRIGGERED_BATTLE_REGIONS.w
at `$FFB20C`. A clear region bit permits geometry; a hit sets90+ID.
**Both miss and hit mark the checked bit.** Later enemies reuse the word
and held flags. Checked does not mean triggered; a held flag can activate
an enemy with its checked bit set. ClearAiMemory writes48 last-target
bytes and48 memory bytes ending before this word. Fresh clearing of
90..105 and F88 clearing do not clear it. Enemy StartAiControl's
@NonSwarmAi clears the word before the activation guard; selected AI has
no SWARM15. Ally AI bypasses that enemy reset. A player move or a nominal
round boundary alone does not clear checked storage.

With only eligible formation positions and freshly clear90..105, incoming
checked C ends as C OR000F with no held selected flag. Fresh STARTING
enemies ensure a passing caller. With no passing enemy/pseudo160, neither
geometry nor activation runs. With a caller but no eligible ally, misses
still set checked bits and never erase flags. Moving to(11,19) while
checked000F/flags clear does not trigger; checked0 allows90..93.

GetTriggerRegions splits high primary/low secondary nibbles. Both15
preserves the whole activation word and later bypasses the PRIMARY AI
guard. Otherwise held non15 primary wins and ORs1; only its miss permits
held secondary to OR3. Misses never clear bits. For current W the outputs
are W OR1, W OR3 or W; a new primary hit preserves old SECONDARY.
Definition priority, AI commandset and PRIMARY are separate fields.

| Ordinal / logical | Kind / commandset | Primary order / region | Secondary order / region | Fresh word and applicable change |
| --- | --- | --- | --- | --- |
|0 /128|Galam /LEADER14|40 /0|FF /1|1000;90→1001;only91→1003|
|1 /129|Blue Shaman /HEALER5=4|80 /0|FF /1|3000;90→3001;only91→3003|
|2 /130|Demon Master /ATTACKER3=9|80 /0|FF /1|2000;90→2001;only91→2003|
|3 /131|Devil Griffin /ATTACKER1=6|FF /15|FF /15|2000 unchanged;unconditional|
|4 /132|Devil Griffin /SENTRY8|FF /3|FF /15|0200;dead helper93→0201|
|5 /133|Reaper /HEALER1=0|FF /3|FF /15|0200;dead helper93→0201|
|6 /134|Evil Beast /SENTRY8|FF /2|FF /15|0200;dead helper92→0201|
|7 /135|Horseman /SENTRY8|FF /2|FF /15|0200;dead helper92→0201|
|8 /136|Reaper /SENTRY8|FF /15|FF /15|2000 unchanged;unconditional|
|9 /137|Evil Beast /SENTRY8|FF /15|FF /15|2000 unchanged;unconditional|
|10 /138|Demon Master /ATTACKER3=9|8C /0|FF /15|2000;90→2001|
|11 /139|Evil Beast /ATTACKER3=9|8C /0|FF /15|2000;90→2001|
|12 /140|Chaos Warrior /ATTACKER1=6|FF /0|FF /15|2000;90→2001|
|13 /141|Horseman /ATTACKER3=9|8F /1|FF /15|2000;91→2001|
|14 /142|Reaper /ATTACKER3=9|8F /1|FF /15|2000;91→2001|
|15 /143|Chaos Dragon /ATTACKER1=6|FF /1|FF /15|2000;91→2001|
|16 /144|Reaper /ATTACKER1=6|FF /15|FF /15|2000 unchanged;unconditional|
|17 /145|Chaos Warrior /ATTACKER1=6|FF /15|FF /15|2000 unchanged;unconditional|
|18 /146|Horseman /ATTACKER1=6|FF /15|FF /15|2000 unchanged;unconditional|

For128..130 both90/91 choose primaryOR1, not secondaryOR3. Existing
1003/3003/2003 remains so on a primary hit. The direct recipe checks all
16 selected flag masks at six assignment shapes and seven representative
existing words:672 supplied arithmetic cases. It is not an atomic flag
snapshot guarantee. For hidden rows the ordinary activation scan initially
skips; only their later dead helper can make the listed first write.

Absent147..158 retain HP/maxHP0,X−1,activation0,triggers0/1 from the
unadvanced Galam row; Y/kind/stats/orders may be stale.159 initially has
the same absent result before the neutral override. Changing HP/X can
admit147..157 to activation and queue without initializing definitions;
158/159 can pass activation but are never offered to the queue. Y−1 or
NEUTRAL can pass activation/queue while failing ordinary entity packing.
Do not turn supplied field changes into declared actors or arbitrary RAM fuzz.

The accepted [extra160/deals alias](../northern-travel-mitula/battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
also applies: GetCombatantEntryAddress admits160 and resolves storage to
CURRENT_GOLD `$FFF600`. Pseudo-X FFF62E packs deals84/85, pseudo-HP
FFF60E deals20..23, triggers FFF636 deals100/101 and activation FFF634
deals96..99. With the helper's X/HP gates and a held non15 trigger it can
OR1/3 into Goddess Staff99 count bits. Fresh formation-only42 sets no
region flags; changed held90..93 may matter.160 is not a new actor,
spawn/queue candidate, item award or persistence claim.

## Four HIDDEN resets, occupancy and appearance

**Confirmed:** `battle/battleloop/populateenemyspawns.asm`
(`$1ACF30..$1AD090`) scans128..159, masks initialization0F00 and
rebuilds the word list length. STARTING words mask0 even after OR1/3;
fresh hidden132..135 mask0200. For each0200 row:

1. UpdateEnemyActivationIfDead requires currentHP0, then a held non15
   primary, otherwise secondary flag. It ORs1/3 into the live full word
   and returns success. It has no X/Y/neutral/occupancy gate. Old active
   bits alone cannot replace a currently held trigger.
2. Only then test maxHP0. A dead already-initialized enemy can acquire
   active bits but fail this one-time HIDDEN guard. Nonzero HP fails
   before it. There is no generic "dead implies respawn" rule.
3. Read current full activation and call ResetSpawningEnemyStats in
   `battle/battleloop/initializecombatants.asm` (`$1B1272..$1B15F8`).
   Live count19 OR80 gives exclusive bound147;147..159 refuse before
   row lookup. Declared132..135 use their own12-byte rows.
4. IsEnemyStartingPositionOccupied scans all30 allies and32 enemies,
   comparing both coordinates with no HP, membership, neutral or self
   exclusion. Dead actors and the subject at its own start can block.
5. Refusal/occupied reset clears maxHP/currentHP, writes X−1, sets carry
   and skips append. Y, activation writes and other stale fields survive.
   There is no rollback of the helper's active bits. A later held-trigger
   retry can succeed after the blocker moves; self-blocking first removes
   the subject's X and can therefore change the next attempt.
6. Success calls InitializeEnemyStats, restores the saved **full** word,
   then appends the ID and increments length. Selected saved0201 is not
   remasked to0200. Subsequent0100/0300 comparisons do not append it again.

| Logical / own trigger | Restored X,Y / kind | Full definition only after success | Current ATT ordinary /SUPER; rawAGI |
| --- | --- | --- | --- |
|132 /3,15|17,17 /Devil Griffin54|HP89,MP0,DEF47,MOV7,FLYING,FLAME3,items NOTHING|120 /150;59|
|133 /3,15|16,18 /Reaper35|HP82,MP31,DEF49,MOV6,REGULAR,MUDDLE2,Battle Sword66 equipped|110 /128;57|
|134 /2,15|9,17 /Evil Beast36|HP84,MP0,DEF51,MOV6,REGULAR,no spell/items|122 /152;60|
|135 /2,15|8,18 /Horseman66|HP67,MP0,DEF57,MOV7,CENTAUR_ARCHER,no spell,Buster Shot52 equipped|110 /128;55|

These rows copy56 bytes/14 longwords from enemydefs at
`$1B1A66+56×kind`, fill HP/MP to maxima, position, placement AI,
kind, triggers/orders, placement item, activation, difficulty ATT and
UpdateCombatantStats. ENHANCED99 class, initial status0, definition
resistance/prowess and all four item/spell slots become real only then;
the predecessor owns their exact definitions. Nothing placement skips
AddItem and retains copied equipment. Difficulty's accepted truncated
5/4 adjustment precedes equipment. The temporary initialized activation
is2200, discarding definition BIT4/retaining priority2; restoration makes
the base result0201, **not2201**. Other saved priority/neutral/active bits
are preserved instead when supplied by changed state.

UpgradeRandomBattleEnemies checks the complete random-battle list
3,8,9,10,14,17,19,21,24,26,35. Live42 misses before completion542,
level/category/upgrade-RNG work. Holding542 does not upgrade these rows.
Changing the battle selector inside services is a different live input.

| Supplied state; other eligible allies at formation points | Successful appended IDs |
| --- | --- |
|Unchanged fresh placement,clear90..105,any checked C|none|
|Eligible ally9,17,checked0;all four starts otherwise clear|135;134 self-position blocked by ally|
|Eligible ally7,18,checked0;all starts clear|134,135|
|Eligible ally17,19,checked0;all starts clear|132,133|
|Eligible ally11,19,checked0;all starts clear|132,133,134,135|
|Same11,19;checked000F/flags clear|none|
|Original formation;checked000F and independently held92|134,135|
|Same with independently held93|132,133|
|11,19;dead or neutral slot at16,18|132,134,135;133 retains0201/HP0/maxHP0/X−1|
|11,19;132 dead with maxHP89;other three fresh|133,134,135;132 may become active but fails maxHP0|
|11,19;132 already alive;other three fresh|133,134,135;132 fails dead gate|
|11,19;all four starts occupied|none;all four keep active write and HP0/maxHP0/X−1|

The starts are distinct and avoid the15 STARTING positions. A successful
HIDDEN reset prevents another reset through maxHP, even after later death,
unless supplied state also restores maxHP0. Blocked rows retain0 and can
retry. Modes0100 and0300 have different dead/trigger/maxHP rules retained
in Battle38; none is declared here. Absent/stale slots do not acquire a
valid definition merely by editing spawn bits.

All successful stats exist before the first camera call. Earlier reset,
activation and alias writes survive later failure/interruption; a partial
definition copy or presentation call is not a transaction. The list is
ascending logical ID, not AGI sorted. No completed appearance is inferred.

SpawnEnemyWithCamera→SpawnEnemy (`$2448A..$24512`) calls
`code/common/scripting/entity/entityfunctions_1.asm:SpawnEnemyEntity`.
It reads live X/Y/mover/mapsprite, masks coordinates3F, scales by384,
sets facing3/eas_Idle and aerial layer for FLYING/HOVERING. Selected
mapsprites are118,99,100,130; only132 is flying. GetEntityEvent scans
64 aliases for a signed-byte maximum, appends max+1 and writes enemy
alias offset ID−96 (36..39), then declares the physical entity. It does
not choose the lowest free hole or guard the49-record capacity.

Subsequent required-return interfaces clear sprite scratch, submit VInt
DMA and wait; set cursor radius0/view target/destination; wait for cursor
and view; hide cursor; perform12 orientation/property/Sleep3 iterations;
request SFX_SPAWN, txt397 and close. Dialogue name is the current
combatant, not scene Galam or Elis. These requests prove neither service
completion nor frame timing, rendered output or usable input.

## Neutral Elis, packing, occupancy and queue differences

**Confirmed:** fresh PositionBattleEntities packs A eligible allies,
then15 STARTING enemies into physical A..A+14. Ordinary packing uses
signed nonnegative X/Y and excludes neutral enemies, without HP,
membership or PRIMARY tests. Hidden132..135 and absent147..159 start
off-map and get aliasFF. The scene aliases/population were rebuilt.

With clear542 the separate neutral producer changes backing159 to
HP/maxHP0,status0,X13/Y15,activation0008 and creates physical A+15
with ELIS204/eas_LyingLeft. Its literal predecrement writes
`$FFB15F`=offset31/script FOLLOWER_B; enemy159's offset63 staysFF.
Other159 fields can remain stale. Script31 is not combat159 and
GetEntityIndexForCombatant(159) still returnsFF. Held542 skips both
mutation and allocation:159 remains absent and alias31 fresh0. A
previous scene Elis is not retained by this reconstruction.

| Consumer | Clear542's backing159 / physical Elis interaction |
| --- | --- |
|Ally geometry|Not visited:only IDs0..29. Physical/script aliases never supply ally coordinates.|
|Enemy activation|159 visited;X13 passes but HP0 fails. NEUTRAL8 itself is not a gate.|
|Hidden collector|0008 AND0F00=0;skip. It is not a20th declared enemy or a HIDDEN row.|
|Spawn-start occupancy|159 participates at13,15 despite HP0/neutral. None of the four selected starts equals it; moving a valid occupant to a start can block.|
|Alias maximum for appearances|alias31=A+15 contributes the physical maximum. offset63=FF is signed−1 and does not contribute; its FF does not make physical Elis invisible to this scan.|
|Turn generator|Only128..157 offered;159 is excluded irrespective of HP/X/neutral changes.|

For unchanged normal packing let E=1 when542 clear and0 when held.
Initial physical count is A+15+E. If h of the four rows reset and their
appearance calls complete in list order, appended physical indices are
A+15+E through A+14+E+h, with h=0 meaning no interval. Rejected rows
leave no allocation hole. At A≤12,h≤4 maximum count32 with Elis,31
without, below49. Alias31 ensures the first spawn follows Elis instead
of overwriting her physical slot. Arbitrary retained aliases do not
inherit this capacity proof; repeated declarations are not deduplicated.

Changed159 HP/X can affect activation and occupancy but never ordinary
queue admission; a saved neutral bit can survive a successful changed
hidden reset and the spawn service still declares an entity. Absent
147..158 can carry stale Y/stats: their HP/X changes do not establish
safe packing, refresh or control. No single alive/present predicate
correctly combines these consumers.

## Live AGI, queue capacity and first dispatch

**Confirmed:** `battle/battleloop/turnorderfunctions.asm`
(`$25544..$25610`) clears64 two-byte records at FFF71A toFFFF,
offers allies0..29 then enemies128..157 ascending, and filters only
signed-negative X or zero currentHP. Y, membership, neutral, PRIMARY,
sleep/stun and signed-positive HP are not admission tests.158..160 are
never offered. All15 fresh STARTING enemies enter even while inactive;
hidden successes can enter without any additional definition-count test.

This path calls GetCurrentAgi, **not UpdateCombatantStats**. It does not
refresh Galam again. The predecessor's accepted refresh remains
raw208→zero Boost preserve208→zero Slow cap200→Demon Rod−10→190=BE,
numerical low62 plus high80. It is neither208 nor198. Successful hidden
resets separately execute their own normal stat refresh before generation.
Future live status/item/stat writes are inputs, not definition constants.

Let G=rawAGI AND127 and R(n) the next direct GenerateRandomNumber draw:

- First score `G + R(G>>3) − R(G>>3) + R(3) − 1`.
- Raw≥128 immediately appends a second record for the same actor.
  B=floor(5G/6); score `B + R(B>>3) − R(B>>3)`.
- Low score bytes are stored. Skipped candidates draw0, singles3,
  two-record candidates5. The word RNG advances
  seed=(13×seed+7) AND65535 and returns floor(seed×n/65536);
  range0 still advances. This is not the thinking RNG or debug override.

| Current kind / rawAGI | Numerical basis | First range / algebraic score envelope | Second basis / range / envelope |
| --- | --- | --- | --- |
|Galam /190|62|7 /55..69|51 /6 /46..56|
|Blue Shaman /51|51|6 /45..57|none|
|Demon Master /65|65|8 /57..73|none|
|Devil Griffin /59|59|7 /52..66|none|
|Reaper,Chaos Dragon /57|57|7 /50..64|none|
|Evil Beast /60|60|7 /53..67|none|
|Horseman /55|55|6 /49..61|none|
|Chaos Warrior /54|54|6 /48..60|none|

These are algebraic envelopes, not independent samples, joint extrema,
probabilities or a specific natural winner. Eligible allies draw earlier;
hidden appearances and any earlier source RNG work can change the incoming
seed. Prowess DOUBLE is an action mechanic, not this high-bit queue rule.

For unchanged living STARTING enemies and h successful hidden rows,
enemy entries=16+h and draws=47+3h. With N eligible allies and T of
those having high-bit rawAGI, totals are16+h+N+T entries and
47+3h+3N+2T draws. At N≤12,T≤N: fresh h0≤40 entries/107 draws;
all four h4≤44/119. A counts packed allies in the prior section; N
counts queue-eligible allies and need not equal A after changed state.
For general offered live candidates C and high-bit candidates D, use
C+D and3C+2D. The source has no overflow guard:60 high-bit offered
candidates could write120 records into64 slots. The normal bound does
not legalize arbitrary HP/X/stale states.

Sorting uses62 outer passes,63 adjacent comparisons each, descending
**signed** score bytes. Swap only if the next score is greater; ties
preserve insertion order:allies before enemies, lower IDs and first
before second record. FFFF's score−1 participates. Wrapped negative
scores may remain behind a sentinel and never dispatch; admission does
not ensure execution. Cursor resets0 and advances2 after an ordinary
completed turn. Retained odd offsets read scores as actors, and byte
offsets128..255 exceed the128-byte buffer. No safe arbitrary cursor API.

ExecuteIndividualTurn (`$23EB0..$24242`) rechecks HP0 and skips a dead
entry. Otherwise it prepares actor/target coordinates, alias/view target
and cursor. MUDDLE, AI_CONTROLLED bit2, enemy/control-opponent toggle
and ally auto-battle toggle select StartAiControl preparation. Ordinary
allies otherwise use player control; controlled opponents can too.
Cursor/view waits, targeting reset, mini-status/land-effect windows,
movement grid/pulsating blocks and cursor hide precede SLEEP/STUN action
gates. Those control predicates are reread before ExecuteAiControl or
ProcessBattleEntityControlPlayerInput. Sleeping AI may already have
prepared; PRIMARY, high AGI or a queue entry alone never grants input.

At enemy @NonSwarmAi,15/15 bypasses PRIMARY after clearing checked.
Other rows with clear PRIMARY call DetermineAiStandbyMovement and install
STAY; standby exposes movement preparation, not queue exclusion. With
only fresh formation points,128..130 and138..143 take that inactive
interface if reached unchanged.131,136,137,144..146 bypass it. Hidden
successes normally restore PRIMARY1. None of the selected kinds is Prism
Flower, Zeon Guard or Burst Rock; Galam LEADER is an ordinary commandset,
not a special-kind branch. Stale hidden kinds are not credited before reset.

For activated ordinary AI, live primaryFF skips follow work; bit40 means
point order, so Galam40 resolves point0=(13,18). Follow orders80→128,
8C→140 and8F→143 refer to combatants. A live target HP0 promotes
secondary to primary and writes secondaryFF; all selected secondary
orders areFF, so this yields FF/FF. This is a live death check, not a
flag90/91 selection or a scene/physical entity lookup. GetMoveOrderPosition
(`$1B15F8..$1B1630`) reads the target's current X/Y or point index
low nibble×2 without a generic point-count guard.

`data/battles/global/aicommandsets.asm` (`$E1AC..$E25B`) supplies:

| Selected set | Ordered command interface | Temporary pathfinding mode |
| --- | --- | --- |
|HEALER1=0|HEAL1,SUPPORT,ATTACK1,MOVE1,STAY|REGULAR|
|HEALER5=4|HEAL3,MOVE_ORDER5,SUPPORT,ATTACK2,MOVE3,STAY|BLOCK_AND_CARVE|
|ATTACKER1=6|ATTACK1,HEAL1,SUPPORT,MOVE1,STAY|REGULAR|
|SENTRY8|ATTACK1,HEAL1,SUPPORT,STAY|REGULAR|
|ATTACKER3=9|MOVE_ORDER3,ATTACK3,HEAL2,SUPPORT,MOVE_ORDER4,MOVE2,STAY|BLOCK_NON_MOVABLE|
|LEADER14→same list as13|MOVE_ORDER5,ATTACK2,HEAL3,SUPPORT,MOVE_ORDER2,MOVE3,STAY|BLOCK_AND_CARVE|

StartAiControl applies the selected temporary obstruction interface,
then executes commands until one succeeds; it clears temporary flags on
return. This is a terrain/pathfinding interface, not an F542 layout copy
or an export of BattleTerrain42. Stop at these command/input interfaces;
no concrete AI move/heal/target/attack or completed player input is derived.

## Actual Content, Domain, Application and adapter boundary

**Confirmed about production code at the accepted base:** these are
actual consumers, not promises established by typed primitives or tables.
No engine, importer, SDK, Godot or source-runtime command was executed here.

| Owner | Present support and selected42 limit |
| --- | --- |
|[Encounter preparation](../../../../src/sf2tool/remake_exploration_battles.py), [encounter reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Preparation admits2/3; pinned reader1/2/3.42 placements/regions/materials are not produced/admitted. Retaining an expression does not execute a spawn.|
|[Definition reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [scenario assembly](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Assembly restricts1/2/3,STARTING,REGULAR spell power and selected AI/definition forms.42's HIDDEN,ENHANCED,HEALER5/SENTRY/BIT4 and equipped refresh remain unsupported. Metadata admission alone is not native execution.|
|[Initialization](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs)|Requires roster-only/already-refreshed allies,difficulty0,bounded control/status/loadout. Enemy equipment refresh refuses. No native hidden HP0/X−1/stale-field lifecycle, neutral159 or alias31 reconstruction.|
|[Activation](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs)|Preserves held flags/OR1/OR3, but tests cross-product triangles and resets local tested=0 each BeforeRound. Uses supplied actor roster/Position/HP>0, not all30 signed slots or persistent native checked word. Rejects nonzero SpawnMode before publication; no selected hidden collector/reset/appearance. The finite slope differences above remain real gaps.|
|[Turn generation](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs), [turn flow](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Sequential RNG,second action,signed62-pass stable sort and sentinel have consumers. Uses numerical AGI≤127 plus explicit ExtraRoundAction and deployment processing order; it does not read native rawAGI or fixed offered IDs. Validates capacity≤64 instead of source overflow; no Galam rod refresh binding. Cursor is a slot index, not native byte offset.|
|[SourceEnemyAi](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [move orders](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs), [standby](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/AiStandbyRules.cs)|Consumes primary/secondary,15/15 bypass,checked clear,follow promotion,points and bounded standby. Active commandsets6/7/9/14 only; status,spells/equipment and selected heal/sentry paths refuse. Standby occupancy comes from living positioned supplied actors, unlike all-slot native spawn occupancy. Neutral opposing target words refuse. A Galam-shaped LEADER primitive is not bound Galam behavior.|
|[BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [BattleAdvancer](../../../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs)|Before→initialize→load→start and round/control continuation exist. Direct Start and field entry have different publication boundaries, detailed below. F88 and original cheat/control modes remain unsupported.|
|[Player admission](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleControlRules.cs), BattleAdvancer and SourceEnemyAi above|Sleep (status AND0xC0) skips to after-turn before AI decision, unlike native preparation order. Stun bit0 has no such skip: with StartPolicy present, player admission classifies Stunned and rejects as Unsupported; SourceEnemyAi rejects nonzero status. EnterPlayer without StartPolicy returns unchanged before classification.|
|[BattleMapViewport](../../../../remake/game/src/Battles/BattleMapViewport.cs)|Consumes session projection/selection for supported packages. No42 package,hidden camera/alias/DMA sequence,selected terrain/material or usable first-control observation was bound or run.|

Direct BattleAdvancer.Start with BattleStartInput and nonnull NewBattle
computes initialization, calls Advance and throws on its failure before
returning an initial session: that direct entry is a transaction through
supported initial control. Field entry instead commits initialization/
region-flag clearing and LoadProgram in BattleEntry.Continue, then
separately commits IntroFlag/StartProgram after load. ProgramRunner's
BattleStartFinished calls **Advance**, not Start. A start-hook refusal
or later first-AI refusal therefore retains the already committed field
prefix; it does not roll back the whole entry. Within a round,
BeforeRound validates and computes replacement activation state, and
GenerateRound computes the queue/RNG replacement before publication.
Failure there preserves the prior committed state; failure in a later
AI/control decision does not undo an already published round or field
prefix. These local publication boundaries do not establish native
instruction-by-instruction hidden/spawn prefix parity.

**Inferred:** future42 admission needs coordinated content, persistent
activation/cache, integer geometry, hidden/neutral lifecycle, actual stat
refresh and AI/control work. Merely widening an allowlist or importing
the table would expose unsupported behavior. This is a dependency
assessment, not authorization for an engine fix or a new comparison system.

## Reproduction, preserved results and stopping boundary

The ignored one-off recipe checks the pinned source and actual consumers,
selected full table/formation/assignment shapes, instruction order/
polarity/widths, fixed integer geometry, links/anchors/UTF8/LF, exact
owned diff and private boundary. It retains source snapshots once and
references them on later runs. It is not a maintained checker framework,
parser/schema/fixture test or original-runtime observation. Tracked
prose/named Git source objects own durable findings; receipts aid review.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1172/review.py <configured-source-disasm-root> --receipt local/issue1172/direct-reproduction-01.json --committed --snapshots local/issue1172/analysis-01.json
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

Each invocation requires a **new, nonexistent receipt path**; change the
example name after using it. The source argument ends in disasm. Reuse
the retained snapshots, not an existing receipt output filename. The
analysis-only receipt does not establish document acceptance. Recipe
availability is local; pinned source/prose remain the portable contract.
Clean committed planner and actual scoped CI results belong to the
frozen handoff. Normal/full Python, old comparisons, SDK/Godot,
emulator/H3/H4 and screenshots are **SKIPPED** by documentation-only scope.

Preserve [the predecessor's retained failures and Unknowns](./battle42-initialization-and-opening.md#reproduction-retained-failures-and-open-boundary),
including Battle41/40 geometry, Map54 final overwrite, Map53 failed
import/zero partial, twelve completed old comparison failures,
#1162 followEntity high-byte/BowieHP missed-finding PASS and correction,
and #1166/#1168 recipe/read failures. #1170's initially frozen receipt
example was not rerunnable because the recipe required a fresh output;
its accepted command-only correction and passing narrow review do not
turn the earlier direct/CI PASS into command-copyability coverage.
The old-head CI/watcher propagation race remains old-head evidence,
not current CI. This slice's original author/main-gate direct PASS and
scoped CI also missed the consumer row's conflation of Sleep and Stun
and its overgeneralization of direct Start atomicity to field entry.
The corrected consumer rows and publication paragraph follow the named
production paths; those earlier passes do not establish coverage of
either distinction.
Nothing is erased, reclassified as interrupted or rerun
without invalidation. This task's bounded path-lookup failures and
self-review corrections remain in its ignored handoff.

**Unknown:** natural incoming party/HP/status/items/coordinates/flags,
checked word/aliases/deals/queue/cursor/RNG, F88 retained state, actual
first actor, reachable movement/terrain/collision, service interleaving/
interruption and completed camera/sprite/DMA/text/audio/input, hardware
timing, source-ROM equality, save persistence and original/remake
playability. Static supplied states and counterexamples do not close
these questions or create an automatic runtime/repair queue.

Freeze committed/pushed Draft PR, exact head/tree, clean state, direct
checks/planner/actual CI and terminal process state. Main-gate retains
independent source/consumer review and merge. Stop at author delivery;
no merge, cleanup, archive, next Issue or concrete battle action.
