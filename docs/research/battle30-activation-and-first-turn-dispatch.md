# Battle30 activation and first-turn dispatch

Evidence date: 2026-10-20. **Confirmed:** all twelve declared formation
positions miss all four Battle30 regions under the integer consumer. Fifteen
initialized enemies qualify for sixteen turn records: Zalbard's current
AGI180 retains the extra-turn bit. Hidden Harpy Queen143 has triggerFF and
cannot enter the unchanged region-gated spawn path; absent144–159 are a
different state. Stop at the first ExecuteIndividualTurn call, before its
body. **Inferred:** the formation stages contact before region entry.
**Unknown:** natural reach, incoming state/RNG, actual first actor, completed
services, presentation, usable control and persistence.

## Agreement and provenance

Standard rigor for [Issue #1066](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1066)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
separate geometry, checked bits, persistent flags, activation, spawn admission
and turn eligibility. Accepted dependency is #1064/PR1065 at
`f46bd54c6295e6722821b2cdb1a5bd899fa12489`. This document and one navigation
link from the [initialization/opening owner](./battle30-initialization-and-opening.md)
are the exact owned paths. No shared index, schema, fixture, tool, remake,
design-guide or translation change. No acquisition, comparator, seed search,
AI/action body, combat, outcome or shrine aftermath work.

Fresh scope begins at BattleLoop:@Start after LoadBattle and the start wrapper
return; F480 is a latch set before the empty start dispatch, not completed
opening evidence. F88 supplies a separate buffer/offset boundary. Reuse
[Battle29's shared organization](./battle29-activation-and-first-turn-dispatch.md),
[Battle27](./battle27-activation-and-first-turn-dispatch.md),
[Battle26's activation/spawn/turn derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [placement](./battle01-placement.md),
[spritesets](./battle-spriteset-data.md), [RNG/stat/turn evidence](./runtime-rng-and-battle-math.md),
[cutscenes](./battle-cutscenes.md), [lifecycle](../design/contracts/battle-control-lifecycle.md)
and [routing](../design/contracts/battle-cutscene-routing.md). Their fixtures
retain their named seams; they are not Battle30 runtime observations.

All original source paths below are relative to `disasm/` at clean pinned
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence identifies the USA retail ROM by SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Constants/RAM are `sf2enums.asm`/`sf2const.asm`; Fnnn means flag nnn.
`battleloop/` and `cutscenes/` abbreviate `code/gameflow/battle/` children.
These are conditional instruction/data deductions, with no new ROM run.

Minimum evidence is selected source, existing fixture/schema/consumer joins
and48 formation/region checks. Reuse the unchanged18,285,979-byte private
init/script pair and its0.083s provenance from `local/issue1005/pair.txt`.
Reads/checks are linear, budgeted below one minute/512MiB with small receipts
in ignored `local/issue1066/`; no extraction, network corpus or model/API
processing. Replan contradictory evidence or material supporting machinery.

## Ordered prefix and supplied-state dispatch

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) orders:

1. ActivateEnemies, including the extra helper invocation160 below.
2. j_ExecuteBattleRegionCutscene.
3. DEBUG_MODE_TOGGLE.b: zero skips PrintAllActivatedDefCons; nonzero calls it.
4. j_PopulateTargetsListWithSpawningEnemies. Read TARGETS_LIST_LENGTH.w
   into d7; zero skips spawning. Otherwise decrement d7, point a0 to the byte
   TARGETS_LIST and clear d0.w before each byte read/SpawnEnemyWithCamera call;
   DBF preserves list order.
5. GenerateBattleTurnOrder, then @IndividualTurns_Loop.
6. Clear d0.w, read CURRENT_BATTLE_TURN.b into d0, use that **byte offset**
   into BATTLE_TURN_ORDER, replace d0.b with its combatant byte. FF branches
   back to @Start; any other byte reaches ExecuteIndividualTurn. Stop there.

The prefix is not atomic. Interruptions retain reached region/checked/flag,
activation/deals, reset/list, seed and buffer writes. Services must return
before the next phase. Debug output makes sixteen flag checks90–105;
PrintActivatedDefCon writes region index to DIALOGUE_NUMBER.l and requests
text463 only for held flags, then increments d1.w; the caller closes text.
No diagnostic appearance or timing was observed.

F88's accepted supplied path restores seconds, clears88/AI memory/view target,
calls LoadBattle and branches directly to @IndividualTurns_Loop. Its following
bra @Start is unreachable. It does not activate, spawn, generate or clear the
supplied turn offset before that first read. Odd offsets can select score
bytes; offsets128–255 read beyond the128-byte buffer. FF can then re-enter
preparation using actual current selectors/state. No natural suspend caller,
arbitrary-state safety or cross-process persistence is established.

## Geometry, checked bits and persistent flags

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) offers
128–159 using32 DBF iterations. UpdateEnemyActivationBitfield rejects
negative signed X.b, zero current HP.w, then nonnegative d0.b. It has no Y,
membership, neutral, order or commandset predicate. Thus initialized128–142
pass; hidden143 and absent144–159 fail X. A high-bit HP word is nonzero here.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies` saves
d0–a6/combatant index and selects the **current** battle's region subsection.
Zero count returns before scan/activation. Count4 supplies indices0–3.
For each unset NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`) bit, it calls
IsRegionTriggered, sets F90+index only on nonzero d1.b, then sets the checked
bit on both hit and miss. A held bit suppresses geometry. Neither this prefix
nor ActivateEnemies clears checked bits or persistent flags. Fresh
initialization cleared F90–105; later AI's checked-word clear is outside scope.

Each12-byte region is type4, unknown0, ordered vertices below and two zeros.
IsRegionTriggered tests triangle1/2/4, then3/2/4 only if the first has no
eligible ally. AreAnyAlliesInTriangle scans slots0–29, requiring positive
**signed** current HP.w and nonnegative signed X.b/Y.b, independently of
membership. IsAllyInTriangle returns0 inside; its caller converts a match to
d6.w=-1, otherwise0, copied to d1.w. Living placed nonmembers can trigger;
dead/negative-HP/offscreen allies cannot. Enemies are outside this scan.

| Region / flag | Ordered vertices1,2,3,4 |
| --- | --- |
|0 /90|`(0,0),(0,21),(23,30),(23,0)`|
|1 /91|`(0,0),(0,29),(23,20),(23,0)`|
|2 /92|`(0,0),(0,18),(23,18),(23,0)`|
|3 /93|`(0,0),(0,16),(23,16),(23,0)`|

The actual edge function sign-extends coordinate bytes, chooses the larger
absolute delta (horizontal on equality), multiplies/divides signed with
truncation toward zero and adds the projection to **endpoint B**. Triangle
edges are1->2,3->1,3->2. Result1 immediately accepts; otherwise compare the
side with the opposite vertex using the actual bit0 exception branches.
Negative projection uses `move #1,ccr` and flag synthesis, yielding2 rather
than the comment's3. Selected coordinates are nonnegative, triangles
nondegenerate, and arithmetic fits. This does not replace the consumer with
a floating-point or cross-product polygon rule. Its index comparison is
`cmp.b d1,d7` / `ble`, not strict index<count; FF separately returns false.
Normal iteration supplies0–3. Trigger15 is a sentinel, not a fifth polygon.

| Ordinal | X/Y | Region0 | Region1 | Region2 | Region3 |
| --- | --- | --- | --- | --- | --- |
|0|`(11,28)`|outside|outside|outside|outside|
|1|`(11,29)`|outside|outside|outside|outside|
|2|`(10,29)`|outside|outside|outside|outside|
|3|`(12,29)`|outside|outside|outside|outside|
|4|`(11,30)`|outside|outside|outside|outside|
|5|`(10,30)`|outside|outside|outside|outside|
|6|`(12,30)`|outside|outside|outside|outside|
|7|`(11,31)`|outside|outside|outside|outside|
|8|`(9,30)`|outside|outside|outside|outside|
|9|`(13,30)`|outside|outside|outside|outside|
|10|`(12,31)`|outside|outside|outside|outside|
|11|`(10,31)`|outside|outside|outside|outside|

Let trunc mean signed truncation toward zero. Compare each projected axis
with the same coordinate of the selected point. Region1's triangle1 edge4->2
has absolute deltas X23/Y29, so it takes the vertical path: at `(11,28)`,
Xproj=`trunc(-23*(28-29)/29)=0`, below X11. For Y31, Xproj=-1 instead takes
the negative-projection return2. All other entries below project Y, below
the selected Y. First triangle rejects at edge4->2; second at3->2, with
opposite vertex on the other side. Earlier edges do not immediately accept:

| Region | Triangle1/2/4 projection | Triangle3/2/4 projection |
| --- | --- | --- |
|0|Y: `21+trunc(-21*X/23)`|Y: `21+trunc(9*X/23)`|
|1|X: `trunc(-23*(Y-29)/29)`|Y: `29+trunc(-9*X/23)`|
|2|Y: `18+trunc(-18*X/23)`|Y: 18|
|3|Y: `16+trunc(-16*X/23)`|Y: 16|

These are formation ordinals, not twelve fixed living identities. Unchanged
fresh placement uses a subset for fewer/dead members; other allies have X=-1.
Consequently F90–93 remain clear regardless of incoming checked bits;
F94–105 also remain clear. The first passing enemy produces checked word
`incoming OR $000F`, not necessarily exactly15. Changed living position
`(11,10)` is inside all four under this consumer; declared vertices/edges
also have direct checks. A previously checked miss still prevents rescan.
Changed flags, coordinates/HP or selectors invalidate the fresh deduction.

## Complete activation rows and extra invocation160

**Confirmed:** GetTriggerRegions splits the stored byte into primary high
and secondary low nibbles. Both15 returns without write. A held non15 primary
wins and sets bit0; only absent/clear primary permits held non15 secondary
to set bits0/1. A miss preserves the word. Primary preserves any existing
bit1 and all unrelated bits; the consumer does not deactivate an enemy.

`data/battles/spritesets/spriteset30.asm` supplies all16 rows below. All
secondary orders are NONE, filler0. Rows0–14 have NOTHING/STARTING0;
row15 has MITHRIL|UNUSED_ITEM_DROP/HIDDEN2. Words are hexadecimal. The last
row's type/position/orders/item are declarations, not fully initialized state.

| Row / ID | Type | X,Y | Commandset | Primary order | P/S | Fresh word | Held relevant flag |
| --- | --- | --- | --- | --- | --- | --- | --- |
|0 /128|Zalbard|11,7|LEADER|MOVE_TO\|0|3/15|`1000`|F93 -> `1001`|
|1 /129|Master Monk|12,6|HEALER3|FOLLOW_ENEMY\|0|3/15|`3000`|F93 -> `3001`|
|2 /130|Dark Knight|12,8|ATTACKER3|FOLLOW_ENEMY\|0|3/3|`2000`|F93 -> `2001`, primary wins|
|3 /131|Gargoyle|8,7|ATTACKER3|FOLLOW_ENEMY\|4|3/15|`2000`|F93 -> `2001`|
|4 /132|Dark Knight|9,10|ATTACKER3|FOLLOW_ENEMY\|5|3/15|`2000`|F93 -> `2001`|
|5 /133|Orc Lord|7,12|SENTRY|NONE|15/15|`2000`|no activation write|
|6 /134|Harpy Queen|11,11|ATTACKER3|MOVE_TO\|0|15/15|`2000`|no activation write|
|7 /135|Wizard|17,10|ATTACKER3|FOLLOW_ENEMY\|8|15/15|`2000`|no activation write|
|8 /136|Dark Knight|16,12|SENTRY|NONE|15/15|`2000`|no activation write|
|9 /137|Harpy Queen|2,14|ATTACKER1|FOLLOW_TARGET\|0|1/15|`2000`|F91 -> `2001`|
|10 /138|Wyvern|18,16|ATTACKER1|FOLLOW_TARGET\|0|0/15|`2000`|F90 -> `2001`|
|11 /139|Dark Knight|7,18|SENTRY|NONE|15/15|`2000`|no activation write|
|12 /140|Dark Knight|17,21|SENTRY|NONE|15/15|`2000`|no activation write|
|13 /141|Orc Lord|5,22|SENTRY|NONE|15/15|`2000`|no activation write|
|14 /142|Wizard|16,21|ATTACKER3|FOLLOW_ENEMY\|12|15/15|`2000`|no activation write|
|15 /143|Harpy Queen|19,15|ATTACKER1|NONE|15/15|`0200`|fails activation X/HP guards|

MOVE_TO0 references point0 `(11,13)`, distinct from trigger region0.
FOLLOW_ENEMY payloads are local enemy ordinals; FOLLOW_TARGET0 is not a
fixed AI point. No unchanged row takes a secondary-only path: row2 has equal
triggers, the others secondary15. Changed trigger bytes can take it. With
fresh flags all selected words remain unchanged. Priority/LEADER/HEALER/
SENTRY labels do not prove a chosen tactic or control ownership.

Hidden143 has max/current HP0, X=-1, triggerFF and activation0200. It retains
other old fields until full initialization. Absent144–159 instead have
HP0/X=-1, activation0 and trigger3F from the **unoffset first row**, not FF.
They are not hidden Harpy Queens. Changing only their X/HP can expose those
3/15 triggers to activation; no neutral filter protects a changed slot.

After32 iterations d0.w=160/d7.w=FFFF. There is no RTS, branch or data before
the following helper label, so it executes once more and its RTS returns to
BattleLoop. The accepted [address/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
is checked through jump aliases and actual getters/setter: address lookup's
`bhi` admits160, mapping `(160-96)*56+$FFE800` to CURRENT_GOLD `$FFF600`.
Pseudo-X `$FFF62E` packs quantities84/85; pseudo-HP `$FFF60E` packs ring20–23;
triggers `$FFF636` pack100/101; activation `$FFF634` packs96–99. Nonnegative
pseudo-X, nonzero pseudo-HP and a held non15 trigger flag90–104 can OR bit0
or bits0/1 into Goddess Staff99's count nibble, preserving other bits. This
is conditional, not shop addition, saturation or persistence evidence.
Unchanged Battle30 sets no such region flags, so there is no deals write
even if pseudo-X/HP pass. Changed state can differ; earlier flag writes are
visible to this call. No Battle29-specific flag result is assumed.

## Region hook, hidden admission and actual spawn consumer

**Confirmed:** the complete `data/battles/cutscenes/regionactivatedcutscenes.asm`
table has four8-byte records and a word terminator:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

ExecuteBattleRegionCutscene advances8, tests terminator, then compares
CURRENT_BATTLE.b. With30 throughout it skips every row without checking
played/region flags, setting played flags or dispatching MAPSCRIPT. Changed
current32/40 can match: played flag precedes region check, set flag precedes
script dispatch, and the wrapper returns after that script. Interrupted
scripts can leave flags held. There is no Battle30 region script here to
repair143's FF triggers. Debug text, if enabled, follows this return.

`battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`) scans128–159,
masking activation with0F00. Mode0 skips HP/region/reset/item work. Thus
all fifteen STARTING rows and sixteen absent slots skip; active bits0/1
do not alter that mode. Hidden143 enters mode0200, but
UpdateEnemyActivationIfDead requires HP.w=0 **and a held non15 primary or
secondary flag**. Both triggers15 cause failure even if F90–105 are held,
positions change or its active bits are already set. It never reaches the
max-HP/reset test under unchanged triggers. List length becomes0; stale list
bytes are not consumed. No fresh spawn/camera/text/item effect follows.

Changed state must satisfy the actual branches:

- Mode0200: dead/trigger helper succeeds, then max HP.w must be0 before reset.
- Mode0100: HP.w=0 admits reset without a region requirement.
- Mode0300: dead/trigger helper succeeds, with no additional max-HP test.
- Other nonzero masked modes append nothing.

Primary trigger wins; secondary success sets both active bits. This helper
has no X/Y guard and can set active bits **before** max-HP or reset rejection.
Changing143's trigger to a held non15 flag with HP/maxHP0 can admit reset;
changing only flags, checked word or its position cannot repair FF. Mode0100
is another conditional path. These are supplied-state deductions, not an
observed way to reveal this enemy naturally.

ResetSpawningEnemyStats uses the current battle's enemy count OR80 as the
exclusive bound: Battle30's16 gives144, so143 is a real row but144–159 cannot
reset from it. It resolves the actual selected row and tests its declared
X/Y against all30 allies and32 enemies, with no HP, neutral or self exclusion.
An actor left at its own starting coordinates can block itself. Unchanged
143's `(19,15)` is distinct from all other declared positions and hidden X=-1;
changed living/dead occupant coordinates can still block it. Rejection writes
max/current HP0, X=-1 and carry, with no append. Success calls full
InitializeEnemyStats and restores the saved **full activation word**, then
returns clear carry through its setter. Restored triggers/orders/type/position
come from that row, not from a separately cached Battle30 identity. A changed
CURRENT_BATTLE can change count, row and random-upgrade selection; each
consumer uses its own current selection, without a snapshot guarantee.

For a successful unchanged-row143 reset, Harpy Queen stats become HP53/MP20,
X19/Y15, type47, FLYING/ATTACKER1, ordersFFFF/triggersFF and definition
equipment/spells/status before the ordinary refresh. Saved0201/0203 may
survive even though the row restores FF. Battle30 has no random upgrade.
InitializeEnemyItems passes declaration407B (Mithril123|4000) to AddItem;
its four empty definition slots permit insertion. `code/common/stats/itemstats.asm:AddItem`
masks with807F, storing **007B**, stripping4000 and any equip bit. The caller's
equipped test is false. Fresh hidden143 has not received this item; this
conditional inventory initialization proves no drop, award or loot outcome.

Each successful slot appends once in ascending ID order (word length/byte
IDs). Later mode comparisons use returned full d1, not a remasked mode;
fall-through does not duplicate append. With a nonempty list,
`battleloop/spawnenemy.asm:SpawnEnemyWithCamera` clears d1 and calls
SpawnEnemy. `entityfunctions_1.asm:SpawnEnemyEntity` reads actual X/Y/movetype,
masks coordinates3F and scales by384, chooses aerial layer for FLYING/HOVERING,
facing3/eas_Idle and the current enemy mapsprite (Harpy Queen111).
GetEntityEvent scans the64 lookup bytes for the highest signed mapping and
allocates max+1, stores that mapping and declares an entity; there is no
separate capacity check here. With N unchanged allies and fifteen existing
enemies, hypothetical143 uses physical N+15 (27 at N=12); changed mappings
need separate assessment. Sprite loading/DMA and its wait must return.

SpawnEnemy then resolves the mapping, sets cursor radius0/view target,
moves the cursor, waits for cursor/view scrolling, hides it, performs twelve
orientation/UpdateEntityProperties/Sleep3 iterations, requests SFX_SPAWN and
text397/close. Outer d7/a0 are preserved. These service requests and return
dependencies prove neither measured timing, visible appearance nor natural
control; failed/interrupted entity services do not roll back earlier stats
or list mutations. Unchanged list0 never calls this consumer.

## Eligible records, AGI transformations and first call

**Confirmed:** `battleloop/turnorderfunctions.asm:GenerateBattleTurnOrder`
(`$25544`, helper `$255A4`) clears64 two-byte records at `$FFF71A` to FFFF.
It offers allies0–29, then enemies128–157 (BATTLE_ENEMY_ENTITIES_COUNTER29),
not158/159. Admission requires nonnegative signed X.b and nonzero HP.w;
no Y, membership, neutral, activation, region, order or commandset test.
Fresh128–142 qualify despite clear active bits;143–157 fail X. A successfully
reset143 would qualify in that same preparation after spawn services return.

The predecessor's complete eight definitions/status/equipment join is checked
against actual current-stat consumers. InitializeCurrentStats copies base
AGI to current AGI. ApplyStatusEffectsOnStats calls both IncreaseCurrentAgi
and DecreaseCurrentAgi even for status NONE, with zero amounts; STUN's extra
decrease is skipped. In `combatantstats_2.asm`/`combatantstats_3.asm`, increase
uses IncreaseAndClamp7Bits: preserve80, mask7F, add/clamp0..127, OR80 back.
DecreaseAndClampByte then subtracts0 and caps at200. Zalbard180 becomes
`(52|128)=180`, remains below200, and is **not** clamped to127 or read as-76.
GetCombatantByte clears d1.w before reading; GetCurrentAgi therefore returns
unsigned180 for the helper's signed **word** comparison with128. Clear
status and the four equipped weapons' ATT-only effects leave all listed
AGIs unchanged. Difficulty changes ATT, not this AGI join. Changed statuses,
items or refreshed stats require their own actual current-byte calculation.

| ID | Type | Current HP | Current AGI | A=AGI&127 | A>>3 | Records |
| --- | --- | --- | --- | --- | --- | --- |
|128|Zalbard|80|180|52|6|2|
|129|Master Monk|47|31|31|3|1|
|130|Dark Knight|50|40|40|5|1|
|131|Gargoyle|29|21|21|2|1|
|132|Dark Knight|50|40|40|5|1|
|133|Orc Lord|46|37|37|4|1|
|134|Harpy Queen|53|43|43|5|1|
|135|Wizard|37|44|44|5|1|
|136|Dark Knight|50|40|40|5|1|
|137|Harpy Queen|53|43|43|5|1|
|138|Wyvern|46|36|36|4|1|
|139|Dark Knight|50|40|40|5|1|
|140|Dark Knight|50|40|40|5|1|
|141|Orc Lord|46|37|37|4|1|
|142|Wizard|37|44|44|5|1|

Reuse the checked [RNG/score/signed-sort derivation](./battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch).
First score is `A+R(A>>3)-R(A>>3)+R(3)-1`. Current raw AGI>=128 adds an
immediate second record using B=floor(A*5/6), score
`B+R(B>>3)-R(B>>3)`. Word arithmetic stores the low score byte. Every direct
GenerateRandomNumber call updates seed `(seed*13+7)&65535`, including range0;
for selected ranges R(n)=floor(seed*n/65536). Singles consume3 sequential
draws, doubles5, rejects0. The calls use direct RNG, not its debug override;
earlier reached services determine the actual generation seed.

Zalbard appends both records before129: A52/range6, then B43/range5. Their
conservative score bounds46..58 and39..47 are positive signed bytes, not
guaranteed independently attainable extremes. All other fresh enemy scores
are also positive below128. Enemy contribution is16 records/47 draws after
actual ally draws. With N eligible initialized allies and T having current
raw AGI>=128, total is16+N+T, at most40 for the twelve-position formation.
Fewer/dead members reduce N. Hypothetically spawning only143 adds one record/
three draws, yielding at most41 under those premises. Changed placements/
membership/AGI do not inherit the twelve-member bound: all30 ally slots are
offered, and158/159 remain outside the enemy turn scan.

Sorting is62 passes of63 adjacent comparisons across all64 records, using
`cmp.b`/signed `ble`; swap only when the right score is strictly greater.
Ties preserve append order, including allies before enemies and Zalbard's
first before second on equal scores. Padding255 is-1; wrapped135 is-121.
The existing rawAGI128 score0/255 and rawAGI127 score135 boundary fixture
proves its seam, not incoming30 state. The original has no append-capacity
guard; changed many/doubled candidates can overrun64, and62 passes do not
prove complete sorting for arbitrary64 entries. Fresh maximum40 fits and
positive enemy scores guarantee a non-sentinel front within these passes,
conditional on all earlier services returning.

Generation clears CURRENT_BATTLE_TURN.b (`$FFF79A`, immediately after the
buffer), so the fresh prefix reads offset0 and reaches the first call.
Actor identity depends on actual party/current stats and coupled RNG draws;
AGI180/LEADER is no proof that Zalbard acts first. Empty eligible sets leave
FF and repeat preparation; changed scores below-1 can sort behind padding.
No selected actor's AI, movement, action or completed turn is inspected.

## Actual representations and consumer gaps

**Confirmed about the accepted repository:** research, prepared Content and
Application/Godot execution are separate. The
[capability owner](../../remake/docs/capability-status.md) controls runtime claims.

| Surface | Actual consumer / expression | Missing binding, rejection or unassessed fidelity |
| --- | --- | --- |
|Research/preparer|Spriteset fixture/schema retains summary/address facts and rowContentTracked=false; full pinned source supplies exact30 rows. Existing map-script corpus/preparer handles its admitted scene instructions.|No extraction or new30 package. Another battle's observed geometry/RNG fixture cannot establish natural30 state or spawning.|
|Content declarations|[BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs) expresses coordinates/order/item/spawn expressions, regions and point. [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) preserves non-STARTING raw placements without declaring deployed geometry.|Actual Pin admits1/2/3 and rejects30. Full30 encounter/start/party/enemy/policy/material binding is absent. Expressible HIDDEN is not an executing spawn.|
|Scenario assembly|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs) separates numerical AGI&127 from ExtraRoundAction, useful for raw180.|Also admits region-program table1/2/3, requires STARTING (`spawn-expression`), limited placement items (`placement-item-refresh`), commandsets (no HEALER3/SENTRY), and enemy REGULAR power (`enemy-derived-state`). Mithril407B is outside its item cases. These are actual guard gaps beyond the first30 pin rejection, not claims of a reached runtime failure.|
|Round activation/spawn|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs) runs before turn generation through [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs); it admits RegionProgram.None and SpawnMode0.|Nonzero spawn modes reject `spawn-mode`; there is no hidden reset/entity-spawn consumer. Geometry uses cross products/local tested0, not truncated projection/supplied checked word; actors replace raw slots, with no160/deals alias. Fresh all-miss agreement proves no general changed-state parity.|
|Stats/turns|[TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs) consumes numerical AGI<=127 plus separate extra action, retains sequential draws/64 slots/signed62x63 sort and rejects overflow. Initialization rules require bounded enemy equipment/status refresh.|Zalbard52+extra action is expressible; all eight30 definitions and equipment/status transformations need binding. Equipped enemies reject `enemy-equipment-refresh`; source-loadout only0/127 does not admit Mithril123. The engine's validation and queue cursor are not original unsafe buffer/offset evidence.|
|Application/Godot|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) emits region/round/RNG observations. [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) consumes fresh hooks/membership. The predecessor checked actual view/terrain consumers: all three Map1 plane profiles pass current guards.|F88 NewBattle rejects `suspended-battle-entry`; save/suspend Unsupported. Map1 material/crop/area fallback/animation and full30 scene remain separate gaps; flashScreenWhite has no lowering. Passing Map1 profiles does not bind the hidden spawn's entity/camera/text/audio sequence or establish exact presentation.|

Production consumes prepared Content, not research/disassembly on demand.
No engine, adapter, Godot or emulator was launched. Reusable mechanisms do
not establish30 readiness, and missing30 binding does not make every action,
text, turn rule or plane profile universally Unsupported.

## Changed states, reproduction and stopping boundary

| Changed premise | Required distinction |
| --- | --- |
|F480/opening interrupted|A latch is not completion; fresh endpoint requires returned services.|
|F88/current selectors/offset/buffer|Supplied first dispatch bypasses round preparation; FF re-enters using actual state.|
|Party/HP/X/Y|Formation ordinal is not identity; geometry requires signed positive HP/Y, activation/turn need nonzero HP and no Y.|
|Checked word/flags|Checked misses suppress later scans; F90–105 are separate persistent flags.|
|Trigger/order/active bits|Primary wins, sentinel15 skips, bit1 persists; order payload does not identify a trigger.|
|Hidden143|FF blocks region spawn even with held flags; changed trigger/mode/HP/maxHP can reach reset.|
|Absent144–159|Mode0/trigger3F is not hidden0200/FF; actual guards and reset bound144 differ from turn bound158.|
|Occupancy/mappings|Dead/self occupants can block reset; changed lookup/capacity affects entity allocation and services.|
|Deals/pseudo-fields|160's X/HP/trigger gates govern any count-bit write.|
|CURRENT_BATTLE between reads|Region/hook/reset row/upgrade consumers use actual selectors; no atomic Battle30 snapshot.|
|AGI/status/items/RNG|Current raw byte controls second record; score wrapping/capacity/order need actual inputs.|
|Interrupted prefix|Reached flags/stats/items/list/seed/buffer writes persist without rollback or implied completion.|

Load ignored `local/private-inputs.ps1` in each launching process. Reproduce
the bounded direct review:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1066/review.py <configured-source-disasm-root>
git diff --check
```

It joins the complete28 placements/16 enemy states/eight definitions, all48
integer results and changed edges/positions, actual activation/alias/region
wrapper/spawn/reset/item/entity/AGI/turn/RNG use sites, existing fixtures/
schemas and current Content/Domain/Application consumers. Review all prose
and changed-state rows, UTF-8/LF, relative links/anchors, private boundary,
exact two-path/navigation-only diff. On clean committed HEAD use `uv run sf2
verify plan --base origin/main --head HEAD`; actual receipts/diagnostics and
exact-head scoped CI belong in the Issue/PR handoff. This documentation scope
skips blanket normal/full Python/.NET/Godot/H3/H4 and tests of review programs.

Preserve #1064's independent0.206s/CI39454491309 and Unknowns, #1062's original
and P2 mapping correction receipts/diagnostics and the linked earlier failures/
Unknown chain. The unchanged pair and twelve completed old remake comparison
failures remain outside this slice's repair/rerun queue. Freeze a clean
current-main-based committed/pushed Draft PR, Project Review / Verify, exact
identity/checks and successful main-gate callback with stopped writer/process
state. No merge, cleanup, archive or next Issue.

**Unknown:** natural reach/admission, actual membership/classes/items/HP/
positions/flags/checked word/deals/current/saved selectors/seed, natural hidden
reveal, entity capacity/mappings and service returns/interleavings,
rendering/audio/timing, usable control, persistence and first actor. No
AI/action body, turn result, battle outcome or post-battle story is established;
these Unknowns are not an automatic acquisition queue.
