# Battle32 activation, Jaro side change and first-turn dispatch

Continue with [Battle32 outcomes, Jaro recruitment and Moun entry](./battle32-outcomes-and-moun-entry.md).

Evidence date: 2026-10-20. **Confirmed (static):** all twelve declared
formation positions miss both regions under the original integer consumer.
Unchanged fresh placement therefore does not admit the Jaro region scene.
When region0 or1 is held and flag386 is clear, the wrapper sets386 before
the scene converts enemy143's position into ally23's position and membership.
This does not copy enemy HP/stats or finish physical services. Stop at the
first ExecuteIndividualTurn call, before its body. **Unknown:** natural
reach, incoming state/RNG, first actor, completed presentation, control and
persistence. Battle32 is not established as runnable in the remake.

## Agreement and evidence route

Standard rigor for [Issue #1084](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1084)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
provide source-backed activation, Jaro conversion and turn-admission rules
for later content/engine decisions. Accepted dependency is
[Battle32 initialization/opening](./battle32-initialization-and-opening.md),
#1082/PR1083, base `2653df79f44839c17fcdb1d54dfb4369b4018f9e`.
Owned writes are this owner and one predecessor navigation link. No shared
index or stacked dependency; UI692 is independent. No implementation,
extraction, schema/fixture/tool change, acquisition, runtime, seed hunt,
individual-turn AI/action/combat, outcome/Moun, translation or design-guide work.

Reuse [Battle31](./battle31-activation-and-first-turn-dispatch.md),
[Battle30](./battle30-activation-and-first-turn-dispatch.md),
[Battle26 shared derivations](./battle26-activation-and-first-turn-dispatch.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[script data](./battle-cutscene-data.md), [RNG/math](./runtime-rng-and-battle-math.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md). Shared fixtures
prove their named seams, not a Battle32 observation.

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The accepted USA retail ROM identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no ROM is run or rebuilt. `sf2const.asm`/`sf2enums.asm` own constants/RAM;
Fnnn means flag nnn; ordinals are zero-based; intervals are half-open.
`battleloop/` and `cutscenes/` abbreviate `code/gameflow/battle/` children.

Minimum evidence is complete selected source/consumer joins, 24 finite
formation-region evaluations, the complete18-command scene and changed
state review. Reuse the unchanged18,285,979-byte init/script pair and
accepted0.083s provenance in `local/issue1005/pair.txt`. Bounded linear reads
and small arithmetic are budgeted below one minute/512MiB per direct run,
with small ignored receipts in `local/issue1084/`. No network acquisition,
binary decoding or model/API data processing. Reassess contradictions or
material supporting machinery before expansion.

## Prefix order and supplied-state entry

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) enters @Start after fresh LoadBattle and the start wrapper
return. The predecessor owns F482, fresh flag clearing/healing/placement,
the empty start script, Map74/crop and physical construction. F482 was set
before start-script execution; it is no completion receipt. At @Start:

1. ActivateEnemies scans128–159, then falls through once for160.
2. j_ExecuteBattleRegionCutscene may run one admitted script to completion.
3. Nonzero DEBUG_MODE_TOGGLE.b calls PrintAllActivatedDefCons. Held flags
   among90–105 request text463 with region index in DIALOGUE_NUMBER.l;
   the caller then closes text. Clear flags request none.
4. j_PopulateTargetsListWithSpawningEnemies rewrites the word length.
   Nonzero length drives a DBF byte-ID scan and SpawnEnemyWithCamera calls;
   zero skips the list, whose stale byte tail is not cleared.
5. GenerateBattleTurnOrder builds/sorts the buffer and clears the offset.
6. @IndividualTurns_Loop clears d0.w, reads CURRENT_BATTLE_TURN.b as a
   **byte offset** into BATTLE_TURN_ORDER, then replaces d0.b with that byte.
   FF branches to @Start; otherwise ExecuteIndividualTurn is called. Stop here.

There is no transaction or rollback across these phases. Every prerequisite
service must return before later phases. F88's supplied-state entry instead
restores seconds, clears88/AI memory/view target, calls LoadBattle and
branches directly to @IndividualTurns_Loop; its following bra @Start is
unreachable. It does not generate or clear the supplied offset. Odd offsets
read score bytes,128–255 exceed the128-byte buffer, and supplied FF can
re-enter @Start using actual live state. This does not establish a natural
suspend caller, safe arbitrary state or cross-process persistence.

## Integer geometry, checked word and persistent flags

**Confirmed:** `battleloop/activateenemies.asm` (`$2550C..$25544`) offers32
slots128–159. UpdateEnemyActivationBitfield rejects negative signed X.b,
zero current HP.w, then nonnegative d0.b. It has no Y, membership, neutral,
order or commandset predicate. High-bit HP is nonzero here. Initialized
128–142 and unskipped143 pass; absent144–159 fail X. Skipped143 uses its
retained fields, not the JAR definition.

`battleloop/triggerregions.asm:TriggerRegionsAndActivateEnemies` saves
d0–a6 and selects the **live** battle's region subsection. Zero count returns
before geometry or activation; selected32 has count2. For each unset bit
of NEWLY_TRIGGERED_BATTLE_REGIONS.w (`$FFB20C`), IsRegionTriggered is called;
a nonzero result sets F90+index. Both hit and miss set the checked bit.
Held checked bits suppress geometry. This prefix clears neither checked
word nor persistent flags. Fresh initialization cleared F90–105, not F386.

`data/battles/spritesets/spriteset32.asm` supplies two12-byte records: type4,
unknown0, four ordered coordinate pairs and two trailing zeros:

| Region / flag | Vertices1,2,3,4 |
| --- | --- |
|0 /90|`(0,0),(0,34),(31,34),(31,0)`|
|1 /91|`(1,0),(1,47),(31,17),(31,0)`|

IsRegionTriggered tests triangle1/2/4, then3/2/4 only on a miss. Its
AreAnyAlliesInTriangle scans0–29, requiring positive **signed** HP.w and
nonnegative signed X.b/Y.b, without membership tests. IsAllyInTriangle
returns0 inside; the caller returns d1.w=-1 for a match or0 for no match.
Placed living nonmembers can trigger; enemies, dead/negative-HP or negative
coordinate allies cannot. This differs from the activation/turn HP guard.

Reuse the source-backed [integer edge derivation](./battle30-activation-and-first-turn-dispatch.md#geometry-checked-bits-and-persistent-flags):
signed byte coordinates, dominant absolute delta (horizontal on equality),
signed multiplication/division truncating toward zero, projection added to
endpoint B. Triangle edges are1->2,3->1,3->2, comparing the point's side with
the opposite vertex. Flag1 immediately accepts; bit0 exceptions follow the
actual branches. Negative projection returns2 through `move #1,ccr` and
flag synthesis, despite its comment. No floating-point/cross-product
substitute is used. The region-index guard uses `cmp.b d1,d7` / `ble`, not
strict index<count; FF separately returns false. Normal iteration supplies0/1.

| Ordinal | X/Y | Region0 | Region1 |
| --- | --- | --- | --- |
|0|`(15,44)`|outside|outside|
|1|`(14,45)`|outside|outside|
|2|`(15,45)`|outside|outside|
|3|`(16,45)`|outside|outside|
|4|`(14,46)`|outside|outside|
|5|`(15,46)`|outside|outside|
|6|`(16,46)`|outside|outside|
|7|`(17,46)`|outside|outside|
|8|`(17,45)`|outside|outside|
|9|`(13,45)`|outside|outside|
|10|`(14,44)`|outside|outside|
|11|`(16,44)`|outside|outside|

Every first triangle rejects at edge4->2; every second at edge3->2, with
earlier edges not immediately accepting. For region0's first triangle,
vertical Xproj=`trunc(-31*(Y-34)/34)` is negative at every formation point;
the opposite vertex is on the other side. Its second triangle has Yproj34,
below every selected Y. Region1's first triangle uses vertical
Xproj=`1+trunc(-30*(Y-47)/47)`, below every selected X. Its second edge uses
horizontal Yproj=`47+trunc(-30*(X-1)/30)=48-X`, below every selected Y.
All selected coordinates are nonnegative/nondegenerate and arithmetic fits.

Changed checks preserve thresholds: `(15,34)` hits0 only, `(15,35)` hits
neither, `(8,31)` hits both, `(1,47)` hits1 only; `(16,32)`/`(17,31)` hit
both but `(16,33)`/`(17,32)` hit0 only. These are coordinate predicates,
not proof a character can naturally reach or stand there.

Formation ordinals are no fixed party. Fresh placement uses a living subset
when fewer/dead members are present; other ally X values are-1. Thus unchanged
fresh placement sets no region flag, even if143 was skipped. The first
eligible initialized enemy produces checked word `incoming OR $0003`, not
necessarily exactly3. F90–105 remain clear and no Jaro hook is admitted.
A previously checked miss can suppress a later moved ally; independently
held flags can activate enemies/admit the hook even when geometry is skipped.

## All activation rows and the extra call160

**Confirmed:** GetTriggerRegions splits primary high/secondary low nibbles.
Both15 returns without writing. A held non15 primary wins and ORs bit0;
only absent/clear primary permits held non15 secondary to OR bits0/1.
Misses preserve the word. Existing secondary bit1 and unrelated bits survive
a primary write; these are not deactivation or AI-action rules.

The predecessor owns all orders/items/positions. All16 rows are STARTING0,
NOTHING/filler0; the selected activation consequences are:

| Row / ID | Type | Primary/secondary | Initialized word | Held relevant flag |
| --- | --- | --- | --- | --- |
|0 /128|Bow Master|15/15|`2000`|no activation write|
|1 /129|Necromancer|15/15|`2000`|no activation write|
|2 /130|Master Monk|15/15|`3000`|no activation write|
|3 /131|Pegasus Knight|15/15|`2000`|no activation write|
|4 /132|Devil Soldier|15/15|`2000`|no activation write|
|5 /133|Cerberus|15/15|`2000`|no activation write|
|6 /134|Pegasus Knight|15/15|`2000`|no activation write|
|7 /135|Devil Soldier|15/15|`2000`|no activation write|
|8 /136|Cerberus|15/15|`2000`|no activation write|
|9 /137|Master Monk|0/1|`3000`|F90 -> `3001`; only F91 -> `3003`|
|10 /138|Pegasus Knight|0/15|`2000`|F90 -> `2001`|
|11 /139|Pegasus Knight|0/15|`2000`|F90 -> `2001`|
|12 /140|Devil Soldier|0/15|`2000`|F90 -> `2001`|
|13 /141|Cerberus|0/0|`2000`|F90 -> `2001`, primary wins|
|14 /142|Necromancer|0/0|`2000`|F90 -> `2001`, primary wins|
|15 /143|JAR|0/1|`2000`|F90 -> `2001`; only F91 -> `2003`|

With both flags held,137/143 take primary. Row137 has FOLLOW_ENEMY9 on
both sides, while143 has NONE on both: identical orders do not collapse
triggers or suppress activation. Trigger15 is a sentinel, not a third polygon.
The143 row/word is conditional on actual initialization; joined23 skips
that slot before clearing/copying, retaining old HP/position/type/items/
orders/triggers/activation. A stale nonneutral143 can still have a physical
entity. The predecessor also preserves HasJaroJoinedTheForce's empty-count
case: an unguarded stale-byte/DBF scan can return a match, so no joined
members does not reliably imply that143 was initialized. This is distinct
from absent144–159's partial clear: max/current
HP0, X=-1, activation0, triggerFF from the still-unoffset first row; other
fields remain. Fresh flags leave all initialized words unchanged.

After32 iterations there is no RTS before UpdateEnemyActivationBitfield:
d0.w=160/d7.w=FFFF falls through once, then its RTS returns to BattleLoop.
The [accepted RAM/deals derivation](./battle26-activation-and-first-turn-dispatch.md#the-extra-fall-through-and-its-selected-ram-alias)
checks the s02 getter/setter aliases and GetCombatantEntryAddress's `bhi`
admission of160 at CURRENT_GOLD `$FFF600`. Pseudo-X `$FFF62E` packs item
counts84/85; HP `$FFF60E` packs20–23; trigger `$FFF636` packs100/101;
activation `$FFF634` packs96–99. Only nonnegative pseudo-X, nonzero pseudo-HP
and a held non15 trigger flag90–104 permit OR1/OR3 into Goddess Staff99's
count nibble. Clear fresh flags imply no write. This is neither an
unconditional shop addition nor visible/persistent inventory evidence.

## Region-hook admission and complete Jaro scene

**Confirmed:** `data/battles/cutscenes/regionactivatedcutscenes.asm`
contains four8-byte records followed by a word FFFF:

| Battle | Region | Played flag | Script |
| --- | --- | --- | --- |
|32|0|386|rbcs_battle32|
|32|1|386|rbcs_battle32|
|40|0|387|rbcs_battle40_1|
|40|1|388|rbcs_battle40_2|

`cutscenes/regionactivatedcutscenes.asm:ExecuteBattleRegionCutscene`
(`$47E82..$47EC8`) advances8, checks terminator, then compares live
CURRENT_BATTLE.b. On a match it loads d1.w=played flag, calls j_CheckFlag,
and skips held flags. It clears d0.w, reads region byte, and calls
j_CheckTriggerRegionFlag. That helper saves/restores d0–a6, computes90+region
in its private d1 and returns flag CCR. Thus d1 is still386 at the wrapper's
j_SetFlag: **386, not90/91**, is set before the MAPSCRIPT trap. CheckFlag
and SetFlag also preserve d0/d1/a0. The wrapper restores its saved d0/d1/a0
and returns after one script, with no rescan. Both held regions select the
first row; only F91 selects the second. Shared386 suppresses either on
subsequent calls. There is no HP, Jaro-membership or completion test here.

The scene's final setF386 repeats the write. The complete pinned progression-
flag references are the two table operands and this script write; the other
numeric386 occurrence is title-screen tile data. Fresh initialization does
not reset386. Interrupted script execution can leave386 held before any
conversion, text, AI-bit write or final setF. Clearing386 externally permits
another admission with the then-current slots; no idempotent conversion guard
exists. Natural occurrence and persistence remain Unknown.

`data/battles/entries/battle32/cs_regiontriggered_1.asm:rbcs_battle32`
starts at `$4CD10`: **18 top-level commands/70 encoded bytes**, including
the terminator;17 commands precede it. Four nextSingleText commands normally
consume keys2849–2852 (`0B21`–`0B24` in gamescript.txt) in order. The complete inventory is:

| Index | Command / operands | Effect boundary |
| --- | --- | --- |
|0|textCursor2849|Set CUTSCENE_DIALOG_INDEX.w.|
|1|executeSubroutine csub_JaroJoinForce|Run the logical conversion below before camera/text.|
|2|cameraSpeed `$38`|Write VIEW_SCROLLING_SPEED.w=56.|
|3|setCameraEntity23|Read copied ally lookup into VIEW_TARGET_ENTITY.b.|
|4|csWait60|Request Sleep60 unless debug text-skip is active.|
|5|nextSingleText0,23|Key2849; Jaro rejects fighting for devils; `{N}`, `{W1}`.|
|6|setCameraEntity128|Target the current Bow Master lookup, not an intro-scene alias.|
|7|csWait60|Same conditional wait.|
|8|nextSingleText0,128|Key2850; surprise addressed to `{NAME;23}`; `{W1}`.|
|9|setCameraEntity23|Return view target to copied lookup.|
|10|csWait60|Same conditional wait.|
|11|setFacing23,UP|Facing byte1 and sprite update, subject to ally HP helper.|
|12|nextSingleText0,23|Key2851; Jaro's reply; `{W1}`.|
|13|setFacing23,DOWN|Facing byte3 and sprite update, same HP helper.|
|14|nextSingleText0,23|Key2852; declares his new side; two `{N}`, `{W1}`.|
|15|joinForceAI23,FFFF|Set AI-control bit2 and call JoinForce, below.|
|16|setF386|Repeat shared played-flag write.|
|17|csc_end|End script; successful epilogue clears scrolling speed.|

`sf2cutscenemacros.asm` and `code/common/scripting/map/mapscriptengine_1/2.asm`
are the actual consumers, not the macro comments. csc0A reads a long
subroutine pointer and preserves a0 across JSR. Camera target consumes a
word ID, maps enemy IDs by subtracting96, reads a lookup byte and writes the
view byte without a living-ally guard. csc23 facing instead calls
AdjustScriptPointerByCharacterAliveStatus: ally23 with HP.w=0 skips its two
operand bytes; nonzero HP, including a high-bit word, proceeds. It writes
the existing physical entity's facing and calls UpdateEntitySprite_0.

nextSingleText's modifier0 and speaker byte are not text variables.
csc00 requests portrait/speech properties from the physical entity's current
mapsprite, waits for view scrolling, calls DisplayText, increments the text
cursor, closes portrait/text and requests Sleep10. There is no speaker-HP
test here. Normal JAR mapsprite52/JARO_SPECIAL maps to portrait JARO and
DIALOG_BLEEP_5. Bow Master127 has no matching dialog-properties row and
uses PORTRAIT_DEFAULT/DIALOG_BLEEP_6. Retained143 can change Jaro's physical
sprite/properties. `{NAME;23}` is an explicit name token, not membership.
`{N}` and `{W1}` require text consumers; no completed rendering/input is proved.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry; debug mode plus P2 START
can then hold it. Each csWait skips Sleep and csc00 skips its operand without
incrementing the cursor while held. Therefore2849–2852 is the normal sequence,
not a guarantee under changed debug input. Other commands, including side
conversion/AI/flags, still execute. FFFF epilogue optionally waits for view
when a dialog window remains, clears scrolling speed and restores d0–a6.
There is no scripted map reload, music/join jingle, new scene population or
physical sprite allocation in these18 commands.

## Logical conversion, membership and physical aliases

**Confirmed:** csub_JaroJoinForce has no entrance membership/HP/capacity or
lookup guard. Its exact mutation sequence, following s02 jump aliases to
`code/common/stats/combatantstats_1/2/3.asm` and `battleparty.asm`, is:

1. d0.w=143; GetCombatantX/Y sign-extend stored bytes to d1.w, saved in d7/d6.
2. SetCurrentHp writes enemy143 HP.w=0. SetCombatantX/Y write bytesFF/FF.
   Enemy maxHP, MP, definition, stats, equipment, orders, activation and
   physical lookup/entity remain unchanged.
3. d0=23; SetCombatantX/Y copy the saved low coordinate bytes into ally23.
   Ally HP/maxHP/MP/class/stats/items/status/activation are untouched here.
   It does not copy the enemy's59 HP, heal or initialize Jaro.
4. JoinForce23 sets joined F23, calls UpdateForce, then conditionally calls
   JoinBattleParty if rebuilt active count is **below12**. JoinBattleParty
   sets F55 only. It neither checks capacity nor rebuilds lists.
5. The subroutine explicitly calls JoinBattleParty23 again, regardless of
   that capacity branch, so F55 is set even with12 other active members.
6. `move.b (ENTITY_INDEX_ENEMY_JAR).l,(ENTITY_INDEX_ALLY_JARO).l` copies
   `$FFB16F` to `$FFB157`, then RTS. It does not clear the source lookup.

UpdateForce rebuilds joined TARGETS_LIST, active BATTLE_PARTY_MEMBERS,
reserve RESERVE_MEMBERS and three word lengths from joined/active flags
for IDs0–29 in ascending order. It clears no tails and writes no flags.
Its snapshot precedes JoinForce's optional active write and the explicit
JoinBattleParty: immediately after the subroutine the cached active/reserve
prefixes can lag F55. No claim that all lists are already synchronized follows.

At scene command15, **csc54_joinForceAi** reads23 and its activation word,
reads the second wordFFFF into d2 and branches on nonzero: OR `$0004` into
d1, call j_JoinForce23, then j_SetActivationBitfield writes the preserved
word. JoinForce saves/restores d1, so the OR survives its list rebuild.
It sets AI control; the macro comment's `-1:off` is contradicted by the
callee. Zero instead ANDs FFFB, does not call JoinForce, and writes the word.
This is not csc08's recruitment music/text service. Successful command15
rebuilds lists **after** F55 was set, so their counted prefixes now include
active Jaro; the later spawn-list builder overwrites TARGETS_LIST/count again.
Battle-party/reserve lists have separate storage. Other activation bits survive.

| Reached state / changed premise | Consequence before generation |
| --- | --- |
|Unjoined, live143 at8,31; ally23 HP nonzero|Enemy becomes HP0/X=Y=-1; ally23 takes8,31; F23/F55 held; command15 adds bit2. Ally uses its own retained stats/HP.|
|Twelve other joined active members|Explicit F55 can create13 active flags; command15's rebuild counts13. No member is evicted or formation recut. This is not a general capacity-safety guarantee.|
|Already joined23|Initializer may have skipped143 entirely. Admitted script still reads that old143 and overwrites ally23's current position, even with negative bytes. No already-joined early return exists.|
|Dead ally23|No revival. Camera/text can still use the copied physical lookup; facing skips on HP0 and the turn helper rejects it.|
|Dead or offscreen143|No eligibility guard at scene entry; saved negative coordinates can hide ally23 logically. Enemy HP still becomes0.|
|Retained/nonstandard143 activation/type/lookup|Logical side mutations do not normalize those fields. Later spawn eligibility and physical aliases depend on actual values.|
|Interruption after wrapper/subroutine/before command15|F386 and reached logical writes can remain; active lists may lag flags and AI bit may remain unchanged. Wrapper retry is suppressed by386.|

The physical copy makes two logical IDs alias one physical slot; it does
not replace its entity number, action script, sprite, position, animation,
priority, or enemy lookup, nor free any previous ally23 entity. Camera23
uses the copied index immediately. GetEntityAddressFromCharacter uses the
lookup and32-byte stride without a49-slot/FF validity check; a missing or
stale source index is not repaired by recruitment. Normal freshly built
JAR's mapsprite52 can already match Jaro's appearance, but that is no
physical migration/reload or service-completion proof. Existing entity
services and later reloads are separate; no new sprite allocation occurs here.

## Spawn selection, including retained143

**Confirmed:** `battleloop/populateenemyspawns.asm` (`$1ACF30..$1AD090`)
scans128–159, masks activation with0F00, and zero skips HP/trigger/reset/item
work. Initialized2000/3000, even with active bits0/1, mask to0. Absent144–159
also mask0. Thus **with unskipped initialized143**, all32 skip, the length
becomes0 and SpawnEnemyWithCamera is bypassed. The region scene's enemy HP0
and ally AI-bit4 do not themselves create an enemy spawn mode.

Do not extend that empty-list result to a **skipped retained143** without
checking its mode. Reuse the [shared spawn/reset derivation](./battle26-activation-and-first-turn-dispatch.md#region-hook-and-initial-spawn-admission):
0100 requires HP0;0200 requires HP0 plus a held non15 trigger and maxHP0;
0300 requires the dead/trigger helper without the extra maxHP test. Other
modes append nothing. Primary wins; activation bits can be written before
a later maxHP/reset rejection. There is no X/Y filter in the dead helper.

ResetSpawningEnemyStats uses live enemy count16 OR80 as exclusive bound144;
143 is a real selected row,144–159 are not. It tests that row's original
starting8,31 against all30 allies/32 enemies, without HP/neutral/self
exclusion. Rejection writes max/current HP0 and X=-1, sets carry and prevents
append. Success calls **InitializeEnemyStats directly**, then restores the
saved full activation word; it does not call the Jaro joined-check wrapper.
Thus changed retained143 mode can permit reset despite joined23. If conversion
placed ally23 at8,31 it blocks that reset even if dead; with other retained
coordinates and an unoccupied start the result can differ. No arbitrary-state
safe removal or guaranteed single-sided Jaro follows from the scene name.

Successful candidates append once in ascending ID order. The subsequent
mode comparisons use returned full d1. Every actual SpawnEnemyWithCamera/
SpawnEnemy call must finish SpawnEnemyEntity, sprite/DMA, view/cursor, spin,
sound and text397 services before generation. Flags alone do not supply
those services or change an initialized mode0. No such service was executed.

## Eligibility, AGI/RNG and first call

**Confirmed:** `battleloop/turnorderfunctions.asm` (`$25544..$25610`) clears
64 two-byte records to FFFF, then offers allies0–29 and enemies128–157 in
ascending order;158/159 are outside this constructor. Only negative X.b or
zero HP.w rejects: no Y, membership, neutral, activation, commandset or
LEADER predicate. Alive inactive enemies still receive turns. For unskipped
unchanged fresh32, all16 declared enemies qualify; absent144–157 fail X.
Skipped143's retained state and possible reset need their own eligibility.

The predecessor checks all seven enemy definitions and five equipped weapon
effects. InitializeEnemyStats copies base AGI to current stats via
`code/common/stats/updatecombatantstats.asm`; initial status NONE and ATT-only
equipment preserve the raw AGI bytes below. Difficulty changes ATT, not AGI;
32 has no random-upgrade entry. Placement NOTHING retains copied inventory,
not an empty loadout. Current changed status/equipment can alter these facts.

| Type / eligible initialized IDs | HP | Base = current raw AGI | A = raw&127 | A>>3 | Records each |
| --- | --- | --- | --- | --- | --- |
|Bow Master /128|46|40|40|5|1|
|Necromancer /129,142|47|51|51|6|1|
|Master Monk /130,137|47|31|31|3|1|
|Pegasus Knight /131,134,138,139|65|47|47|5|1|
|Devil Soldier /132,135,140|60|43|43|5|1|
|Cerberus /133,136,141|51|45|45|5|1|
|JAR /143|59|46|46|5|1|

Reuse the [RNG/score derivation](./battle26-activation-and-first-turn-dispatch.md#turn-buffer-randomized-agility-and-first-dispatch):
first score `A+R(A>>3)-R(A>>3)+R(3)-1`. Current **raw** AGI>=128 adds a
second record using `B=floor(A*5/6)` and `B+R(B>>3)-R(B>>3)`; priority,
commandset and AI-control bit do not select that extra record. Each direct
draw updates `seed=(seed*13+7)&65535`, even range0; here
`R(n)=floor(seed*n/65536)`. Singles use3 sequential draws, doubles5,
rejections0. Direct generation bypasses the debug RNG override; preceding
reached services determine the actual incoming seed.

All initialized enemy AGI bytes are below128: **16 records/48 draws** after
actual ally draws. Conservative first-score bounds by table order are35..45,
45..57,28..34,42..52,38..48,40..50,41..51. Coupled draws do not guarantee
independently attainable extremes. With N eligible placed fresh allies and
T of those rawAGI>=128, total `16+N+T` is at most40 for twelve formation slots.
This bound is conditional on unskipped143 and unchanged construction, not
an arbitrary-state queue-capacity claim.

If the Jaro scene completes **before generation**,143's HP0/X=-1 removes
its record unless a changed-mode reset recreates it. Ally23 is offered in
the ally scan, before enemies, if its own X is nonnegative and HP nonzero;
it retains its own raw AGI/extra-turn eligibility. Joined/active/AI flags
neither ensure nor suppress its record. With initialized other enemies,
mode0 and no other mutations, remaining enemies give15 records/45 draws;
ally23's presence, previous eligibility and current AGI determine the net
change in records and sequential draws. There is no guaranteed one-for-one
replacement, copied46 AGI or deterministic first actor. If HP0 it adds none;
if already eligible, conversion can change position without adding an actor.

Sort performs62 passes of63 adjacent signed-byte comparisons across64
slots, swapping only when the right score is strictly greater (`cmp.b`/`ble`).
Ties retain append order, including allies before enemies; padding255 is-1.
The existing rawAGI128 score0/255 and rawAGI127 score135 seams remain relevant.
There is no original append-capacity guard, and62 passes do not guarantee
complete sorting for arbitrary64 inputs. Selected fresh positive enemy
scores put a non-sentinel at the front, conditional on prior return. Generation
clears CURRENT_BATTLE_TURN.b (`$FFF79A`), so the first read uses offset0 and
reaches ExecuteIndividualTurn. Actor identity depends on actual party/stats
and sequential RNG. Empty eligibility repeats @Start; supplied scores below-1
can sort behind padding. No action, movement, AI tactic or outcome is evaluated.

## Actual lowering and accepted consumers

**Confirmed about accepted code:** production consumes prepared Content;
research shape, admission, runtime and fidelity are distinct. The
[capability owner](../../remake/docs/capability-status.md) controls running scope.

[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) actually
lowers rbcs_battle32 to38 instructions: text cursor0; call1 into
csub_JaroJoinForce; native cameraSpeed2; camera/wait/text/facing3–34;
native joinForceAI35; set-flag36; end-map-script37. The called subroutine
lowers to16 instructions: **15 native calls then end**, starting with
unsupported `jsr` for j_GetCombatantX at source index1. Immediate register
tracking is not a combatant mutation. Recursive inspection covers both
programs:17 native instructions total, no entity-action/motion stream.
If independently admitted with required program/text context and no earlier
failure, the call reaches subroutine instruction0's `program-opcode` refusal
**before** cameraSpeed2, rather than performing Jaro conversion. The two
top-level native instructions remain later separate gaps. No selected runtime
or absolute first-error claim follows from this lowering alone.

| Actual consumer | Representable / implemented | Specific gap or different boundary |
| --- | --- | --- |
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Call stack, text/camera/face/wait/flag forms; native-call becomes UnsupportedInstruction; reached execution reports program-opcode.|No selected csub coordinate/HP/lookup or joinForceAI implementation. Failure preserves reached snapshot. Known action forms do not make this native subroutine supported.|
|[PrivateBattleEncounterReader.Pin](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEncounterDefinition](../../remake/src/Sf2.Remake.Application/Content/Scenarios/BattleEncounterDefinition.cs)|Placement/region/point/scene/terrain declarations exist.|Pin admits1/2/3 and rejects32. Missing32 party/encounter/stat/material bindings remain separate from expressible coordinates and source corpus.|
|[PrivateBattleScenarioReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Both triggers/orders, STARTING/NOTHING and separate numerical AGI/ExtraRoundAction shapes exist.|region-program-table admits1/2/3 only; commandsets omit HEALER3/SENTRY; enemy-derived-state requires REGULAR while all seven selected types are ENHANCED; source-mover omits ARCHER and MAGE. These are inspected guards beyond first Pin rejection, not reached32 errors.|
|[BattleActivationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleTurnFlow](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)|Activation before generation, primary/secondary OR precedence and zero spawn-mode handling.|Non-None region program rejects region-program; nonzero spawn rejects spawn-mode. No32 hook/shared386/Jaro conversion/reset consumer. Geometry uses cross products and local tested=0, not original truncated edges/supplied checked word/raw-slot160 alias. Fresh misses do not prove changed-state parity.|
|[MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)|Joined/active/reserve rebuild and conditional Join flags exist; scene allocation/aliases have consumers.|Generic Join alone does not implement explicit forced F55, subsequent AI-control/list rebuild, retained143 or dual lookup alias. Modern scene allocator's49-slot guard does not prove original stale-lookup safety.|
|[BattleInitializationRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs), [TurnOrderRules](../../remake/src/Sf2.Remake.Domain/Battles/Rules/TurnOrderRules.cs)|Numerical AGI<=127 plus ExtraRoundAction, sequential draws, signed62x63 sort/64 slots.|Enemy equipment refresh admits item words0/127 only, rejecting selected equipped weapons as enemy-equipment-refresh. Current-state/party mapping is missing; modern queue-capacity validation differs from unsafe original appends.|
|[BattleTurnFlow.ValidateDeployment](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs), [BattleAdvancer](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Round observations and fresh-entry continuations exist.|control-side requires ally iff Player, so automatic ally Jaro is not admitted by this battle policy. region-program-none observation is no executed Jaro scene. F88 rejects suspended-battle-entry; no original first actor/control claim.|
|[ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs)|Existing view/sprite/window/effect consumers; predecessor checks Map74 layer254/zero-origin/256-parallax/zero-autoscroll metadata profiles.|Native speed56 remains unsupported; nonzero admitted ViewSpeed separately refuses field-view-speed. Selected Map74/Terrain27/party/sprites/portraits/text/audio/resources, physical migration, exact service/timing/control/persistence are unassessed.|

No adapter, Godot or emulator was launched. Existing supported mechanisms
are not erased by missing32 bindings, and source-specific unsupported
operations are not silently treated as successful no-ops.

## Reproduction, retained failures and stopping condition

Load ignored selections in the same launching process. The narrow direct
review checks selected source/fixture joins; all24 integer results and
changed thresholds; complete activation/hook/scene/text/csub/flag/register/
membership/lookup/spawn/AGI/RNG/dispatch semantics; recursively lowered calls
and actual Content/Domain/Application/Godot boundaries; and all prose tables.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1084/review.py <configured-source-disasm-root> --committed
git diff --check
```

On clean committed HEAD separately compare exact accepted-base-to-HEAD
owned paths and clean status; require navigation-only predecessor change,
UTF-8/LF, valid relative links/anchors and private boundary. Then run
`uv run sf2 verify plan --base origin/main --head HEAD` and retain actual
exact-head scoped CI in the Issue/PR handoff. Documentation-only scope skips
blanket normal/full Python/.NET/Godot/H3/H4, tests of audit programs and old
comparisons. No schema/parser change creates new mutation-test obligations.

Preserve #1082 accepted independent0.222s/CI39491194567; #1080 original
author0.302s/CI39488100152 missed inventory claim, independent clean-head
`review-main-gate.log` AssertionError[] and focused independent0.107s/
CI39489091772 correction; #1076 nested-action correction independent0.123s/
CI39484532491; earlier LEADER/width/HP/geometry/N==V findings and Unknowns.
Twelve completed old comparison failures remain discovery evidence, not a
rerun queue. Keep every new failed receipt and focused correction in this
Issue's ignored handoff; do not simulate a clean committed scope with dirt.

Stop with exact owned diff, clean committed/pushed frozen Draft PR, Project
Review / Verify, head/tree/base/checks/CI/handoff and successful main-gate
notification; writer/process stopped. Independent main-gate accepts/merges.
No merge, environment cleanup/archive or next Issue is performed here.

**Unknown:** natural admission/region-scene reach, actual flags/checked word,
party/classes/equipment/HP/positions/deals, joined/active/list/retained143/
lookup/capacity state, selectors, RNG/first actor, interrupted prefixes and
service interleavings, completed text/portrait/camera/sprite/sound/waits,
exact presentation/timing, usable control and persistence. These are not an
automatic runtime-acquisition queue. Individual-turn actions/combat, outcomes
and later Moun story remain outside this slice.
