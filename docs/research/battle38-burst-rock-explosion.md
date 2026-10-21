# Battle38 Burst Rock targeting, damage and immediate explosions

Evidence date: 2026-10-21. **Confirmed (static):** kind32's special AI
replaces its ally-only preparation with an **all-combatant** occupancy
array. Eligible enemies alone can therefore enable the explosion draw.
B_ROCK selects the twelve squares at Manhattan distance1–2, excluding
the origin. Action4 uses constant base18, independently of spell power18;
ordinary non-rock targets take14–18 after two variance draws, rocks take18
without variance. A lethal rock hit registers an immediate explosion only
when **d7.w=0**. Sorting places rocks last and bubbles a minimum-HP rock
to the final slot; it does not queue every dead rock. Script construction
temporarily kills actors/targets, then restores the HP snapshot before
playback applies reactions. This is supplied-state source evidence, not
a naturally reached Battle38 explosion or playable acceptance.

## Agreement, provenance and cost boundary

Standard rigor for [Issue #1148](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1148)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
continue accepted [activation and first dispatch](./battle38-activation-and-first-turn-dispatch.md)
at base `dc4515e17f71d70a74075e76de0a899f583c8176` through special target
selection, action construction, damage and immediate reactive selection.
The result supports a future special-action contract and identifies actual
consumer gaps. Own this document and navigation only in that predecessor;
no shared registry, fixture/schema/parser, design or remake change, competing
writer or stacked dependency. Whole-round continuation, victory/defeat,
after-battle story and next-map admission remain outside.

Reuse [Battle36 laser shared services](./battle36-prism-flower-laser.md),
[battle actions](./battle-actions.md), [battle AI](./battle-ai.md),
[RNG/math](./runtime-rng-and-battle-math.md), [battle loop](./battle-loop.md)
and the [action contract](../design/contracts/battle-action-construction.md).
The predecessor owns hidden reset/activation, nine initial rock positions,
control predicates and service admissions. A successful reset installs
kind32, HP/maxHP45, ATT0/DEF60/AGI43/MOV4, empty items/spells and0201.
Those inputs do not make ordinary ATTACKER1 a special explosion executor.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`ai/`, `battlefield/`, `battlefunctions/`, `battleactions/`, `battlescenes/`
abbreviate children of `code/gameflow/battle/`; `tech/` and `stats/` mean
`code/common/tech/` and `code/common/stats/`. Constants/RAM are owned by
sf2enums.asm/sf2const.asm. IDs are combatant byte IDs, not entity indices;
intervals half-open, hexadecimal values have `$` prefix. The registered USA
ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality or original-runtime observation is claimed.

Minimum evidence is selected instructions, relative tables, actual consumers
and a few supplied occupancy/HP/RNG deltas. At most62 eligible insertions,
12 coordinate visits and quadratic sorting of at most12 unique occupants
per ordinary valid-origin case are sufficient; no battle simulation or
seed enumeration is needed. Linear selected reads and tiny ignored receipts
target one minute/512MiB per command; peak memory is unmeasured. The existing
immutable18,285,979-byte pair with `local/issue1005/pair.txt` provenance is
available but not loaded. Ignored recipe/results stay in `local/issue1148/`.
No acquisition, repeated lowering, import/export, verification platform or
helper tests, normal/full/old comparisons, SDK/Godot/emulator/H3/H4 or
screenshots. Reassess before materially expanding evidence or tooling.

## Special AI, target replacement and the thinking draw

**Confirmed:** `ai/startaicontrol.asm:StartAiControl` calls
ProcessExploderAi only after the accepted enemy activation guard and kind32
check, then branches to Done before ordinary follow/order/commandset work.
Ally AI does not reach that enemy-kind branch. Forced AI/status and live
control predicates retain the predecessor's boundaries.

ProcessExploderAi saves/restores d0–a6; d5.w retains the incoming actor.
BuildTargetsArrayWithAllies preserves registers and populates ally occupancy,
but `PopulateTargetsListForSpell` compares d1.b to SPELL_B_ROCK=$19 and
**calls BuildTargetsArrayWithAllCombatants**, clearing and rebuilding it.
The ally-only array does not survive. Both actor d0 and spell d1 survive
the call; its output is TARGETS_LIST/LENGTH RAM, not a returned target ID.
The helper uses the actor itself as the center; no ally is chosen as center.

The AI reads length.w into d0. Zero goes directly to DoNotExplode without
this explosion RNG call. Nonzero sets d6.w=6, invokes
`j_GenerateRandomNumberUnderD6` (the pinned s13 jump alias), and compares
**d7.b=4**. Other results fall back. The wrapper in
`tech/thinkingairng.asm` (`$1AD090..$1AD0DC`) saves d0–d5/a0–a6,
copies d6.b to d1.b, and repeatedly calls GenerateRandomValueSigned until
unsigned d7.b<6. Its literal code admits **0,1,2,3,4,5**, despite the
comment describing0 or2..5. There is no exclusion of1 and no directional
debug override in this wrapper.

GenerateRandomValueSigned clears d7.w, reads the byte at RANDOM_SEED_COPY
`$FFDFB0`, sign-extends it, unsigned-multiplies by541, adds12345 as a word,
masks255 and writes back **that byte**. Thus its observable recurrence is
`b'=(541*b+12345)&255`, including signed input bytes because only the low
byte survives. Each rejection updates the copy; RANDOM_SEED `$FFDEA4` is
not advanced by this decision helper. Text parsing/diamond menu writers
can seed the copy; `tech/randomnumbergenerator.asm:GenerateRandomValueUnsigned`
is a separate unused word-based copy generator, not this byte helper.
The direct GenerateRandomNumber uses RANDOM_SEED. These callers do not
establish the byte at an actual rock turn.

| Supplied copy byte; nonempty target list | Generated byte prefix | Returned d7 / decision |
| --- | --- | --- |
|7|4|4 / explode after one update|
|51|0|0 / MOVE1 then STAY|
|104|1|1 / MOVE1 then STAY; disproves the comment's exclusion|
|Empty list; copy7|no call|MOVE1 then STAY; that command may use its own RNG|

The source branch is not evidence of an unconditional1/6 natural chance.
No uniform incoming-copy distribution, independence from prior UI work,
actual debug input, natural reach or seed frequency is established. The
main damage RNG below is a different path. No exhaustive seed sample is
used to manufacture a probability claim.

Success writes CURRENT_BATTLEACTION.type.w=4 (BATTLEACTION_BURST_ROCK),
offset2.w=$19 (item-or-spell), offset4.w=d5 (actor/center), then FF to
the **first byte** of BATTLE_ENTITY_MOVE_STRING and returns. It does not
move, spend MP, validate a learned spell or perform damage here.

Fallback restores d0.w=d5, sets d1=AI_COMMAND_MOVE1 and clears d7.w,
calls ExecuteAiCommand, then unconditionally writes type.w=STAY3.
`ai/executeaicommand.asm` maps MOVE1 to d1=0 → aiCommand_Move. The caller
ignores its result and does **not** replace its resulting move string with
FF or rewrite offsets2/4. Therefore STAY is the post-movement action;
it does not promise no movement. Route construction/pathfinding, terrain,
occupancy and MOVE1's own RNG remain a movement interface, not a simulated
trajectory or a second explosion search in this owner.

## B_ROCK geometry, filters and overwrite order

**Confirmed:** `data/stats/spells/spelldefs.asm` declares B_ROCK level1:
MP0, BURST_ROCK_EXPLOSION animation, TYPE_ATTACK, range0..0, radius2,
power18. `battlefield/populatetargetslist.asm:PopulateTargetsList` clears
length.w, obtains the definition, sets d2=radius+1=3 and points a1 just
after pointer2 in `data/stats/spells/spellranges.asm` (`$C590..$C5D6`).
B_ROCK decrements d2 to2, so predecrement traversal visits **Range2 then
Range1**, never Range0's0,0. Exact relative X,Y order:

| Table | Ordered offsets |
| --- | --- |
|SpellRange2, count8|0,−2; −1,−1; −2,0; −1,1; 0,2; 1,1; 2,0; 1,−1|
|SpellRange1, count4|0,1; 1,0; 0,−1; −1,0|

For valid coordinates these are exactly `1 <= abs(dx)+abs(dy) <= 2`.
Origin exclusion is spatial, not an ID filter: every actor stacked on the
origin is excluded, while the center need not be eligible for occupancy.
This is not an ordinary cast-range/LOS/terrain query. d5.b=0 means the
relative-coordinate helper bypasses its terrain/movable-grid block.

BuildTargetsArrayWithAllCombatants (`buildtargetsarray.asm`, `$C7F2..$C87A`)
clears2304 bytes toFF, visits0–29 ascending, then128–159 ascending without
another clear. Each slot requires HP.w **nonzero**, neutral activation bit3
clear, unsigned sign-extended Y.w<48 and X.w<48. Negative coordinates and
48+ reject. HP$8000–FFFF is not excluded as signed-negative; there is no
party membership, joined/active-force, primary-active, kind, defense,
status, class, terrain or selected32×32 crop predicate. A later eligible
occupant overwrites the earlier byte on the same square: higher ally ID,
then enemies over allies, then higher enemy ID. An ineligible later slot
does not erase a valid earlier insertion. Stacked actors are not all hit.

`buildactionrangegrids.asm:ApplyRelativeCoordinatesListToGrid` rejects
center X exactly−1, then obtains center X/Y as sign-extended bytes. It adds
each offset with **ADD.B** into the low byte of a word and unsigned-tests
the resulting word against48 before reading occupancy. For normal centers
0..47, offsets below0 wrap to254/255 and reject, and48/49 reject; the
radius clips to48×48 storage, not the battle crop. This is not a general
signed-coordinate clipping API for malformed centers. It rereads occupant
HP and appends only nonFF/nonzero-HP occupants. Length and list are rebuilt
on every call; stale bytes beyond length are not targets. Unique valid
offsets bound this list to12. Getters/helper registers are restored.

All states below are supplied seam inputs, not movements or a battle replay;
unmentioned slots are ineligible. Rock139 is at14,15 unless named otherwise.
Lists are the **unsorted** geometry output, before scene sorting.

| Supplied state | List / boundary |
| --- | --- |
|Only139 at its origin|[]; no explosion draw|
|Ally2 at14,13, ally9 at14,16|[2,9]; allies enable the draw|
|Only enemy128 at14,13|[128]; enemy-only enables the draw too|
|Ally2 at14,13, enemy128 at15,16, ally9 at14,16|[2,128,9]; mixed sides|
|Add rock141 at15,15 to the mixed case|[2,128,9,141]; nearby rocks are targets|
|Ally2 and9, enemy128 and140 all at14,13|[140]; last eligible enemy overwrites all|
|Ally2 at14,13; overlapping140 dead or neutral|[2]; rejected overwrite leaves ally|
|Ally2 HP0 or neutral bit3 set (separately), no others|[]|
|Ally2 HP$8000, neutral clear at14,13|[2]; nonzero is sufficient here|
|Ally2 at16,16 (distance3), or stacked at14,15|[]; radius/origin exclusion|
|Center0,0; ally2 at0,2, ally9 at1,1, enemy128 at2,0|[2,9,128]; negative offsets clip|
|Center31,31; enemy128 at33,31|[128]; beyond32-wide crop remains in48 storage|
|Center47,47; ally2 at47,45 and9 at45,47|[2,9]; outward48/49 omitted|
|Only target X−1, Y−1, X48 or Y48 (separately)|[]; insertion refuses each|

With the nine declared successfully reset rock starts alone, the only
nearby pair is144↔145 (distance2); all other rock pairs are beyond2.
This conditional adjacency can enable an enemy-only AI draw,
but hidden/off-map/dead rows do not count, and spawning all nine is not
assumed. Fresh HP45 exceeds a single18 rock hit, so adjacency alone does
not cause a lethal chain. Current state, not starting positions, governs.

## Action handoff and rock-last sorting

**Confirmed:** `ExecuteIndividualTurn` can divert after AI preparation for
sleep/stun or changed control. If ExecuteAiControl is reached, its movement
services run **before** the action-type check: BuildMovementRangeGrid,
blocks/SFX, MoveBattleEntityByMoveString, SetCombatantX/Y from d2/d3, delete
blocks. FF-first success supplies no steps but still requires these services
to return. Fallback STAY can follow a nonempty MOVE1 route.

In `battlefunctions/battlefunctions_2.asm:ExecuteAiControl @Check_Explosion`,
action4 uses offset2 spell$19 for BuildSpellRangeGrid/definition radius,
waits for scroll, creates blocks, writes CURSOR_RADIUS=2 and copies
**BATTLEACTION_ITEM_OR_SPELL_COPY** (the offset4 alias `$FFB632`) into
itemOrSpellIndex(a6) for PerformAiTargetingVisualAct. Thus it focuses on
the actor/center, not the first nearby target or a second spell. Cursor,
facing, window, Sleep15 and validation-SFX calls are presentation interfaces.
The selected spell range0 is for this display/range preparation, not the
damage target list's radius2. No completed service or usable input follows
merely from selecting action4.

After its action checks, ExecuteIndividualTurn requests two WaitForVInt
calls, then `j_WriteBattlesceneScript`. In
`battleactionsengine_1.asm` (`$9B92..$9EC4`), initialization publishes actor
and actor-copy bytes, clears EXP/gold/attack-type and transient toggles,
including explode. DetermineTargetsByAction action4 loads **(a4) actor**
into d0.b, forces d1.w=$19, rebuilds all-combatant radius targets, then
sorts. It does not consult action offsets2/4 in this branch. A target that
moved/died/became neutral after decision can vanish; new eligible enemies
can appear. A later empty list does not change action4 to STAY.

`sorttargets.asm` (`$A9CC..$AA98`) returns immediately for length0/1.
For2+ targets it marks **enemy** kind32 IDs with bit6 ($40), compares
unsigned bytes in its nested sort, then makes **one left-to-right adjacent
HP pass** over marked entries. If current HP<next HP (unsigned), swap;
ties do not swap in this pass. Finally clear bit6 with$BF. Therefore
ordinary allies/enemies sort by ID before rocks; a minimum-HP rock reaches
the final slot, but the entire rock suffix is **not** necessarily ordered
by descending HP. Initial ID order breaks equal-HP cases; the earlier
unsigned sort swaps equal bytes, irrelevant for these unique occupants.

| Supplied unsorted targets / current rock HP | Final order |
| --- | --- |
|[141,2,128,140], rocks140=5,141=20|[2,128,141,140]|
|[141,140,139], HP139=5,140=20,141=10|[140,141,139]|
|[139,140,141], HP139=20,140=10,141=30|[139,141,140]; single pass, not full HP sort|
|[141,139,140], allHP18|[139,140,141]; highest ID last on equalHP|
|[140], HP18|[140]; no marker/HP pass, still final target|

InitializeBattlesceneProperties snapshots HP for30+32 slots and bypasses
ordinary muddle/same-side classification for action4. InitializeActors
initially displays the enemy actor alone (ally byteFF); SwitchTargets
later selects each target's side. The same-side target is not refused.
Initial status/sleep/control checks still matter before this interface,
but action4's construction does not enter attack curse/stun rolls or
cast-spell silence/MP/learned-spell setup.

## Power, HP writes and reaction requests

**Confirmed:** `battleactionsengine_2.asm:ApplyActionEffect` loads
BATTLEACTION_BURST_ROCK_POWER=18 into d6.w **for each target**, then calls
InflictDamage (`$ACEA..$AE32`) and, if targetDies, DisplayDeathMessage.
The source comment there says the rock dies, but (a5)/targetDies refers
to the **current target**; actor self-destruction is a different path.
The base constant is separate from spelldefs power18. Changing spell power
alone would not change this action's damage. No Attack/CastSpell call
occurs: ATT−DEF, terrain protection, elemental/status resistance,
enhanced spell power, dodge/critical/ailment/curse and double/counter
determination are not damage modifiers selected by action4.

InflictDamage retains these actual shared exceptions:

1. Taros plus ineffectiveAttackToggle can return without damage. The
   selected38 initializer leaves it clear; it is not a universal defense.
2. Attack-type COUNTER halves d6 first. Initial active explosion has type0;
   the reactive loop does **not** reset attack-type, so an inherited counter
   context would halve18 to9. No ordinary counter is determined for action4.
3. A kind32 **target** bypasses variance (zero damage would become1).
   Other targets call GenerateRandomOrDebugNumber twice with unchanged
   range `(18>>3)+1=3`, subtract both0..2 results, then require positive
   signed damage, minimum1. Ordinary base result is14..18. Directional
   debug Right/Up/Left/Down returns0/1/2/3 in that priority, without seed
   advance; held Down produces12, outside the ordinary result envelope.
   No direction/debug disabled uses the direct main RNG, not thinking copy.
4. CalculateDamageExp precedes HP subtraction; DecreaseCurrentHp clamps
   word subtraction to0..maxHP via DecreaseAndClampWord. For ordinary valid
   HP≤maxHP<$8000, after=`max(0,before−damage)`; malformed signed-high
   words do not inherit an unsigned-health guarantee. HP0 sets targetDies
   and calls AddExpAndGoldForKill before reactive registration.
5. Emit target-side executeAllyReaction/executeEnemyReaction with signed
   HP delta−damage, MP0, current status and flags1, then damage message.
   Calculated amount remains18/variance result even if HP is smaller.
   The pending-explosion exception below substitutes HP delta0.

Direct main RNG for range3 is `s'=(13*s+7)&65535`, output
`floor(s'*3/65536)`. Calls occur in sorted target order without reseeding.

| Supplied arithmetic seam | Damage / temporary HP / emitted delta |
| --- | --- |
|Ordinary targetHP20, seed0|0,0 draws; seed7 then98;18 /2 /−18|
|Next ordinaryHP20, same stream|seed1281 then16660,0,0;18 /2 /−18|
|OrdinaryHP20, seed$1234|seed$ECAB then$04B6,2,0;16 /4 /−16|
|Same seed, HP10|16 /0 /−16; target death message follows|
|Debug Down held, HP20|12 /8 /−12; main seed unchanged|
|RockHP45, any main seed, attack-type0|18 /27 /−18; no variance draw|
|Final rockHP18, attack-type0|18 /0 /0; pending explosion, death message still emitted|
|Final rockHP19, attack-type0|18 /1 /−18; no pending explosion|
|Inherited COUNTER, rockHP9|9 /0 /0 if d7=0; no variance draw|

Before the effect loop, `createbattlesceneanimation.asm:PerformAnimation`
selects the hardcoded BURST_ROCK_EXPLOSION animation ($1E), sets mirror
bit7 for an enemy ($9E), emits actor-side animation with
BATTLEANIMATION_ATTACK, then actor reaction HP$8000/MP0/
current status/flags1 and **SetCurrentHp0** for the actor. It does not
calculate self-damage, kill EXP/gold, drop or targetDies here. This temporary
self-removal precedes target damage, even for length0; animation/self-loss
are requested before the length-underflow branch reaches End.

Script construction shares native HP storage for these temporary writes;
it is not a separate immutable HP object. End restores all62 snapshot HP
words, including the self-killed actor, after restoring BATTLESCENE_ACTOR
from ACTOR_COPY. RNG, action-type changes, constructed commands and other
side effects are not rolled back. During later script playback,
`battlescenes/battlesceneengine_0.asm:bsc0A_executeEnemyReaction`/bsc0B
read the displayed side's current ID, consume signed HP/MP/status/flags,
call clamped HP helpers, set status and refresh stats. Displayed−1 skips
the8 operand bytes. HP$8000 is a special removal reaction: append that
displayed actor to the dead list, clear its scene slot and run removal
services. Ordinary reactions append on resulting death through their
own branch. Thus the zero-delta lethal-rock reaction keeps the rock visible
until its subsequent self-removal request; the earlier temporary0 is not
itself a persistent death or a completed playback.

These are ordered, nontransactional prefixes. If construction stops after
actor SetCurrentHp0 or a target subtraction but before End, the snapshot
has not yet been restored. A later failed playback/presentation service
does not imply that earlier emitted HP/status/dead-list writes were undone.
Completion requires every named call and the End restoration/playback path
to return; a constructed script or one reaction is not the whole action.

## Immediate reaction slot and its stopping boundary

**Confirmed:** InflictDamage's CheckExplode requires targetDies nonzero,
GetEnemy kind32, and **tst.w d7 / bne skip**. Only then set explode.b=FF,
explodingActor.b=d0 and d2.w=0. There is no queue, no check that the incoming
action was physical, and no check that explode was previously clear.
A later qualifying call overwrites the one actor byte; a nonqualifying
call leaves the pending slot untouched. The ordinary effect loop initializes
d7=length−1 and DBF decrements after each target, so only its final target
can qualify. Several lethal rocks in one list are not several registrations.
The marker/minimum-HP pass is what puts a killable rock in that position.

After **all** target effects/drop calls, MakeActorIdle, BreakUsedItem and
double/counter validation, CheckExplode reloads a4=actor/a5=list, tests
explode and either ends or:

1. Clears explode.b **before** running the new pass.
2. Writes action.type.w=4 and replaces BATTLESCENE_ACTOR.b with
   explodingActor. ACTOR_COPY remains the original actor. Offsets2/4
   are not rewritten and are not needed by the action4 target branch.
3. Emits makeActorIdleAndEndAnimation, rebuilds all-combatant radius
   targets around that rock's current position using temporary HP,
   sorts again, and branches to **Continue**, not the initializer.
4. Emits new action message/animation/self-removal and processes the
   new target list. A newly qualifying final dead rock can replace the
   slot and repeat. Empty list still emits the explosion/self-removal,
   then branches straight to End. Otherwise clear pending after the
   pass ends the loop. No natural turn/AI/RNG selection is required
   for this already-registered immediate action.

No second HP snapshot, actor initialization, EXP/gold reset, muddle/side
classification or attack-type reset occurs on this branch. Existing
context is inherited; do not treat it as a fresh ordinary turn. Selected
active action4 initializes follow-up flags0 and never determines them;
both validators retain debug overrides after their clear branches. The
following cases assume no forced debug hit flags and unchanged services.

| Supplied local seam | Pending/next-pass result |
| --- | --- |
|One hit kills rock140, d7=0|pending140; reaction delta0; next center140|
|Same hit, d7=1|no registration; ordinary−damage reaction/removal|
|Three sorted rocks139HP20,141HP30,140HP10 all receive18|139→2,141→12,140→0; only140 registers|
|Two equalHP18 rocks140,141 both receive18|140 dies with d7=1 and no registration;141 dies final and registers; rebuild omits temporary-dead140|
|A pending140 followed by a qualifying141 call supplied with d7=0|single byte becomes141; no retained140 queue entry|
|Explosion139 at14,15; rock141 moved to15,15 with HP18; no others|141 final dies/registers; next141 pass excludes self and temporary-dead139, length0, requests self-removal then End|
|Same, rock141HP45|141 survives27; pending remains clear, End after this pass|

These bounded deductions are not a general chain simulator. With fixed
valid slots and no external mutation, each exploding actor is set0 and
later occupancy rebuilds exclude it; a registered target is already0.
This supports finite consumption of that fixed living set (**Inferred**),
not an established arbitrary-chain safety bound under debug overrides,
interleaved state changes, malformed memory or command-buffer exhaustion.
No runtime chain length, hardware timing or naturally reached state is known.

Identity/accounting follows the current pointers: damage EXP, kill gold and
DropEnemyItem return early for an enemy actor. An initial ally killing the
rock can accrue the ordinary kill before registration and its drop call
still sees that original ally/target. Subsequent rock→target kills use the
rock actor and do not become extra ally awards. Actor self-removal has no
kill-award call. Before End the actor is restored from ACTOR_COPY, so any
eligible final giveEXP interface belongs to the original actor; its later
HP, counter context and the existing End gates still apply. No per-rock
reward total is inferred here. General EXP randomization/drop/statistics
remain with the shared action/loop owners.

Action4 requests message318 (authored role: actor explosion), damage285
for enemy actor, death290 for enemy target or291 for ally. Complete ordered
tokens checked in `data/scripting/text/gamescript.txt` are318:`{NAME}`,
285:`{NAME}{N}{#}`,290/291:`{NAME}{N}`. Native
`battlescenes/animation/burstrock.asm` clears animation properties, requests
the EXPLOSION tileset, starts a property entry, sets animation30/update
toggles and stores the palette. Its `animation/update/burstrock.asm` visits
16 property slots; an entry reaching counter2 draws direct main RNG with
ranges7,192,128 for graphics variation/position and requests
SFX_BATTLEFIELD_DEATH. Update-toggle0 branches to ReinitializeSceneAfterSpell.
These later presentation draws do not retroactively change already
constructed damage, but they can affect the next main-RNG consumer. No
total animation draw count, duration or rendered/audio completion is claimed.
No dialogue corpus is reproduced. Stop after construction/restoration and the named
reaction/dead-list playback interface. Reload, dead-list statistics,
whole-round continuation, battle result and later story are not executed.

## Actual Content, Domain and Application consumers

**Confirmed about current code:** production consumes prepared Content.
Direct reads establish these boundaries; no selected38 runtime was launched.
The predecessor retains missing38 pins/terrain/materials, HIDDEN reset,
region/control/order and F88 limits; none is silently closed here.

| Actual consumer | Representation, admission and execution boundary |
| --- | --- |
|[PrivateBattleDefinitionReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleDefinitionReader.cs), [scenario reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs), [encounter reader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Private spell shapes retain animation/properties/range/radius/power; definition selection collects party, enemy and item-use spells. Empty rock spell slots do not implicitly select B_ROCK. Scenario builds executable healing definitions only from HEAL with radius0 and the exact healing property expression. Selected38 pin/HIDDEN guards still refuse before a playable encounter.|
|[RuleCompositions](../../remake/src/Sf2.Remake.Application/Gameplay/RuleCompositions.cs), [SourceEnemyAi](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs)|source-orders binds the actual decision.6/7/9/14 and empty loadout can pass ordinary guards if earlier inputs are supplied, but there is no kind32 dispatch, all-combatant radius decision or compare4 thinking draw. It pursues/attacks opposite-faction targets. Ordinary ATTACKER1 admission can choose incorrect ordinary behavior rather than a named special-action refusal.|
|[State/action types](../../remake/src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs), [automatic/action result](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActionResolution.cs)|SpellRef and damage reactions are representable; BattleActionKind is Stay/Healing/Physical/Item. Automatic result has one optional opposing living target; Validate enforces opposing faction. No BurstRock action, center-versus-final-list policy, actor self-removal or pending explode/explodingActor state exists. Physical result validation is a bounded duel of at most3 reactions, not this radius/chain mechanism.|
|[BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [movement continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs), [Sf2PhysicalAction](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs)|Automatic target becomes Physical; absent target becomes Stay. Physical admission rejects same faction and uses ATT/DEF/terrain/physical rolls. It does not execute constant18 both-side area damage, rock variance exemption or last-target reactive selection. The existing generic thinking generator is capacity, not a kind32 caller.|
|[Command dispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs), [scene continuation](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleSceneContinuation.cs), [scene rules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleSceneRules.cs)|Commit uses prepared actions; TargetEnter applies each prepared reaction and records deaths; DeathMessage advances a finite prebuilt reaction list/reward/end. There is no HP-driven target rebuild, minimum-rock sort, pending-slot replacement or immediate self-explosion producer. Damage arrays/strings and common HP playback do not implement native action4 or animation30/message318.|

Future design must choose an explicit special-action/center/target/reaction
contract and retain construction-versus-playback identity/accounting. This
is an identified gap, not an implementation authorization or engine-ready
claim. No consumer, schema, test count or required-check policy is changed.

## Reproduction, preserved failures and Unknowns

The ignored recipe checks clean pinned source, named operands/branches/
aliases/order, exact tables, supplied occupancy/sort/thinking/main-RNG/HP/
pending deltas, selected adjacency, message tokens, actual consumer guards,
two-path navigation-only scope, UTF8/LF/links/fences and private publication
boundary. It is a local review receipt, not a maintained platform or proof
of runtime reach. Load current configuration in the same process and use
an unused receipt destination:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1148/review.py <configured-source-disasm-root> --receipt local/issue1148/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm containing code/, data/ and sf2enums.asm.
Record actual committed planner and exact-head scoped CI, not suggested
commands as passes. Normal `uv run sf2 verify` is **SKIPPED** by the explicit
documentation-only Issue agreement; full/old comparisons and SDK/runtime
remain excluded. Apply ADR0004's applicable evidence/date/provenance,
width/branch/order/selector/consumer/adversarial checks. Maintained parser/
schema/fixture/helper tests and H3 callback changes are N/A.

Preserve #1146's accepted source/consumer/direct/main-gate and CI results,
#1144's sole opening observation, earlier handoffs, Map53 event import
UnboundLocalError/zero partial programs, twelve completed old private
comparison failures, Battle37 y64 runner refusal, Map66 reload/cross-map
scroll and existing consumer gaps. No prior failure is erased, relabeled
interrupted, automatically repaired or rerun. The ignored handoff retains
scratch failures/corrections and actual command/resource results.

Freeze a clean committed/pushed Draft PR with base/head/tree, actual checks/
CI, failures/Unknowns and ignored handoff; independent main-gate acceptance
and merge remain pending. Stop writer/processes at handoff, without next
Issue, merge, cleanup/archive or private publication.

**Unknown:** natural Battle38 entry/activation/movement/explosion/chain;
live party/all30+32 slots, HP/status/neutral/overlap/positions, control/debug/
seed-copy/main seed, interleaving/selectors/aliases, actual command-buffer
headroom, completed movement/view/window/animation/text/audio/removal/DMA/
reload services, original timing/usable input/persistence and source/ROM
equality beyond existing evidence. Arbitrary malformed/forced-debug chains,
whole rounds, outcomes and after-story remain outside. These are explicit
limits, not a passed gate or automatic acquisition queue.
