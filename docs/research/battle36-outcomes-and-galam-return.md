# Battle36 outcomes and Galam return

Evidence date: 2026-10-21. **Confirmed (static):** Battle36 has an empty
defeated hook and a nonzero leader byte. With Bowie alive, enemy128's death
can remove the remaining living enemies before the count. Battle Sword66
is a separate action-side drop from that Executioner, not a victory reward.
Clear536 runs the Geshp after scene; held536 skips it. Both paths join
Bowie through the common tail, clear436 and set536. The scene loads Map53
and replaces entities, but supplies no warp to overwrite the pre-scene
return seed. Fresh exploration rebuilds population at the saved member
position, not necessarily the scene's11,20. Map53 exits lead to77; Battle37
admission remains a later independent request. Natural reach, completed
services, usable control, presentation and persistence remain **Unknown**.
This is not a playable36 claim.

## Agreement, provenance and method

Standard rigor for [Issue #1134](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1134)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted36 combat evidence to outcomes, the Geshp scene and Galam
return for later content integration. Accepted base
`cc2649e5ae78ca0efe5a0fc23d2a24c36a0b1505` includes
[initialization](./battle36-initialization-and-opening.md),
[activation](./battle36-activation-and-first-turn-dispatch.md),
[laser rules](./battle36-prism-flower-laser.md) and
[Map53/77 admission](./roft-to-galam-and-battle36-admission.md).
Own this document and navigation only in the laser owner; no shared index,
schema, fixture, registry, design or implementation changes, stacked
dependency or competing writer.

Reuse [Battle35's common outcomes/drop/return](./battle35-outcomes-and-roft-return.md),
[Battle34's corrected death accounting](./battle34-outcomes-and-sky-orb-handoff.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[global data](./battle-global-data.md), [maps](./common-maps.md),
[request producer](./map-event-request-state.md),
[consumer](./map-event-request-consumption.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md).
Start at the returning individual-turn suffix and action drop/retreat seams.
Stop at Map77 exploration and its already defined next encounter entry.
Battle37 initialization, full Galam/Dwarven Village content and whole-battle
execution are excluded.

Original paths are relative to clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battleloop/`, `battleactions/` and `cutscenes/` abbreviate children of
`code/gameflow/battle/`; `map/` means `code/common/scripting/map/`.
sf2const.asm, sf2enums.asm and macros own constants/widths. Fnnn means
story flag nnn; ordinals are zero-based; intervals half-open. The USA ROM
SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality or original runtime observation is asserted.

Minimum evidence is complete selected tables/scene, actual helpers and
production consumers, with fixed changed-state examples. Reuse the
immutable18,285,979-byte init/script pair and `local/issue1005/pair.txt`
provenance. Linear selected reads and one existing-compiler lowering have
a one minute/512MiB budget per command; peak memory is unmeasured. Lowering
registers the selected source and uses existing source helpers, requiring
no canonical map export. Small unique receipts stay in ignored
`local/issue1134/`. No acquisition/export, new parser/platform/fixture,
verification-tool test, old comparison or runtime gate. Reassess changed
prerequisites or material contradictions before expanding supporting work.

## Terminal selection, leader cleanup and accounting

**Confirmed:** `battleloop_1.asm:BattleLoop` returns from
ExecuteIndividualTurn, optionally performs debug kill-all, then calls
ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and
CountRemainingCombatants. Zero allies selects defeat before zero enemies
selects victory. A nonterminal pass applies ProcessAfterTurnEffects to the
actual turn-buffer actor, processes deaths and counts again with the same
priority, without another defeated hook. Only a second nonterminal result
increments the byte cursor by2. Admitted retreat unwinds before this suffix.

| Complete selected table | Battle36 result |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot36/byte72 at `$47C30`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte36 at `$47CB2`: -1, nonzero cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot36/byte72 at `$47D3C`: abcs_battle36.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte36 at `$47D8E`:0, passed to JoinForce.|
|`data/battles/global/afterbattlepositions.asm`, `$47E6C..$47E82`|Only battle5 has a row; no36 override.|

`cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated`
requires Bowie0 HP.w!=0 and enemy128 HP.w=0. Here128 is the selected
Executioner at8,27, not a Flower or the later Geshp scene actor. Clear536
dispatches ms_Empty (FFFF); held536 skips that interpreter call. Both reach
`battleendcutscenesend.asm:loc_47C48`, reread CURRENT_BATTLE.b and test its
leader byte. Unchanged36 sweeps all32 enemies128–159, appending each
nonzero-HP ID to the existing death list and updating word length/sentinel.
It neither clears the initial list nor tests coordinates, membership,
neutral/activation state or spriteset size.

ProcessKilledCombatants requests death presentation/waits, then increments
defeats for the listed ally's own ID. Only the enemy branch temporarily
substitutes BATTLESCENE_FIRST_ALLY for kills, then restores the listed enemy
ID. It sets listed X/Y=-1, clears status, refreshes stats and moves the
aliased physical entity to$7000,$7000. It does not set HP0, clear list
length or call DropEnemyItem. A living enemy queued by leader cleanup can
therefore disappear from counts through X without an action kill/drop.
Prism Flower friendly fire or FIRST_ALLY accounting alone does not prove
an ally-earned reward.

CountRemainingCombatants scans30 allies/32 enemies using signed X.b>=0
and HP.w!=0, ignoring Y, joined/active membership, neutral/activation state
and declared count. High-bit HP is nonzero here. Bowie HP0 finally forces
ally count0. ApplyPositionsAfterEnemyLeaderDies has the same Bowie/enemy128
gate but a separate word-battle table lookup; the actual after-battlescene
listener calls it, misses36 and returns without repositioning. It does not
revive128 or arrange the after scene.

| Supplied state at the named seam | Result boundary |
| --- | --- |
|Fresh unchanged20 STARTING enemies, before deaths|All20 positioned/nonzero-HP slots count even with activation bits clear.|
|Bowie alive;128 HP0; other19 enemies nonzeroHP|Leader sweep can queue all19; returning cleanup removes their X from the count. No need to destroy every Flower separately.|
|128 alive/positioned; all eight Flowers gone|128 and other eligible enemies keep the count nonzero; Flowers alone are not the terminal predicate.|
|128 alive but every enemy fails X/HP count|Victory can occur without the defeated hook's leader gate.|
|Bowie HP0, another living ally, no enemies|Wrapper skips; forced zero allies selects defeat. Both counts0 also selects defeat.|
|All allies off-map, Bowie HP nonzero|Zero allies still selects defeat.|
|128 reaches HP0 only in the second after-turn pass|No second defeated hook; other eligible enemies can keep battle running.|
|536 already held|Defeated script skips, but gated leader cleanup/count priority remain.|

Helpers reread their own selector/state; the original has no immutable36
snapshot or transaction. Death-list duplicates, stale aliases and F88
retained state are not repaired by this description.

## Battle Sword: actual inventory, drop bit and delivery

**Confirmed:** `data/battles/global/enemyitemdrops.asm` has30 four-byte
records plus a word terminator (`$BE52..$BECC`). Ordinal25 at `$BEB6` is
VERSUS_PRISM_FLOWERS / enemyEntity0 / BATTLE_SWORD / droppedFlag25.
The macro emits0+128=128. `spriteset36.asm` enemy0 is Executioner28,
LEADER AI, placement item NOTHING; definition28 supplies66|EQUIPPED plus
three NOTHING slots. Placement NOTHING does not erase that weapon. Only
one selected36 drop row exists; Flower129–136 and the scene's Geshp128
are not alternative drop owners.

Reuse the [drop/inventory/Deals consumer](./battle35-outcomes-and-roft-return.md#heat-axe-declaration-actual-inventory-and-deals):
`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem` has three
direct action-engine calls. It requires an allied actor, enemy target,
targetDies nonzero, live battle/target match and an actual matching item
in the target's four slots. GetItemSlotContainingIndex masks index bits
and picks the first match. An equipped/broken matching entry can match;
a missing item prevents the drop.

66 is not Taros Sword, Iron Ball or Counter Sword: it goes directly to
DropItem without the range32 draw. The later upgrade-probability block is
unreachable. BSET tests/sets bit25 at
`ENEMY_ITEM_DROPPED_FLAGS+$3=$FFF7A7`, mask$02. This is not story F25,
436,486 or536. Already set returns before removal/award. First admission
removes/arranges the selected enemy slot and refreshes stats, then checks
the actual allied actor's HP. A living actor's AddItem tries only that
actor's four slots; success adds bare66 unequipped and requests396 then395.
HP0/full inventory instead attempts Deals because66 is RARE, without those
success messages. Deals saturates the item's nibble at15; it does not
search reserves/Caravan or roll back bit/removal.

The complete item66 definition is HERO|BRN|BDBT|RDBN equip flags,
range1..1, price9200, WEAPON|RARE, useSpell NOTHING, effects
INCREASE_ATT35, NONE0, NONE0. It is not BREAKABLE/CURSED. A changed broken
slot's flags are not transferred: AddItem receives the masked bare index.

| Changed state / interruption | Consequence |
| --- | --- |
|128 owns66, bit25 clear, allied actor alive with room|Conditional delivery of one unequipped66 after preceding calls return.|
|Same, actor full or HP0|Deals attempt; Deals15 can remain15 after bit/removal.|
|128 lacks66, or target129 owns66|No selected match; no newly set bit25/delivery from this row.|
|Bit25 held|No removal/new award, even with a fresh66 or536 clear.|
|Enemy laser kills128, or leader cleanup removes a living enemy|Enemy actor fails drop admission; cleanup is not a drop caller.|
|Interrupt after BSET/removal/award|Reached state can precede messages/536; no rollback/save guarantee.|

Victory/after scene neither award66 nor require its possession;436/536
tail writes do not reset the independent drop field.

## Pre-scene seed, complete Geshp scene and completion tail

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` heals living allies
and Peter7/Lemon28 even at HP0, retaining other dead allies and
STUN/POISON/CURSE. It saves CURRENT_MAP.b→PARAM2, rebuilds force lists,
takes the first active-list byte and saves its actual X/Y plus
BATTLE_AREA_X/Y byte additions→PARAM3/4. Facing comes through that
member's physical alias→PARAM5. PARAM1=0 is written without posting an
event type. The [common seed owner](./battle35-outcomes-and-roft-return.md#victory-seed-empty-after-wrapper-and-completion-tail)
retains unchecked empty/stale list/alias and upper-word limits.

Unchanged36 map53/crop0,0 saves that member's actual byte coordinates,
not necessarily Bowie or the last actor. Supplied member position8,27
produces53/8,27 with actual aliased facing; it is not forced to the
Executioner's initial8,27, mainEntity11,20 or view6,17. Zero crop validates
neither position nor alias. The wrapper checks500+CURRENT_BATTLE: clear536
runs abcs_battle36; held536 skips the whole scene. Its source is
`data/battles/entries/battle36/cs_afterbattle.asm`, `$4D9A4..$4DA66`.
The complete command order is:

| Ordered block | Selected commands / boundary |
| --- | --- |
|Entry|textCursor2953; loadMapFadeIn53,6,17; loadMapEntities ce_4DA46.|
|Setup|setActscriptWait Bowie,eas_Init; Peter,eas_Init; setPos Peter10,19,Down; FOLLOWER_B,eas_Init; setPos FOLLOWER_B12,19,Down.|
|Only branch|jumpIfFlagClear76→cs_4D9F0. Held76 instead runs Zynk,eas_Init with wait, setPos Zynk63,62,Down, then falls through. Tests76, not joined26/active58.|
|Arrival|animEntityFX128,MOSAIC_OUT; fadeInB; animEntityFX128,MOSAIC_IN; setActscriptWait128,eas_Jump; csWait5.|
|Reaction|setActscript128,eas_2xRightLeft without wait; csWait120; setFacing128,Down; nextText flags0/speaker128→2953; nextSingleText flags0/speaker128→2954.|
|Departure|setFacing128,Up; nextSingleText flags0/speaker128→2955; animEntityFX128,MOSAIC_OUT.|
|Follow|followEntity129,Bowie,2; followEntityPeter,129,1; followEntityFOLLOWER_B,129,3; csc_end.|

The full ce_4DA46 table is mainEntity11,20,Down; Peter10,19,Down/eas_Init;
Geshp11,23,Down/eas_Init; Caravan11,19,Down/eas_Init; cscEntitiesEnd.
`map/mapscriptengine_1.asm:csc42_loadMapEntities` calls
InitializeMapEntities then LoadEntityMapsprites with display/VInt services.
`map/mapfunctions.asm` clears entities, creates flag-selected followers
first, processes the table, then declares main physical0. An existing
Peter alias can suppress the explicit duplicate record; Geshp/Caravan
receive scene references128/129. New physical aliases are unrelated to
the Executioner/Flower combatant resources. The table does not explicitly
declare FOLLOWER_B or Zynk; their flag-selected presence/aliases are inputs.

The common follower table can select31 for Astral/Oddler via separate
flags and26 via76 independently of joined/active/HP. Do not assume a
unique follower from the requested FOLLOWER_B position. Map53 is not an
overworld map, so65 alone does not prepend Caravan to this scene's
ordinary followers; the explicit Caravan row is separate. Accepted alias-zero/
overwrite and F65-overworld-prefix limits stay with the
[population owner](./roft-to-galam-and-battle36-admission.md).
GetEntityAddressFromCharacter reads the actual alias without first
rejecting zero/FF. The byte-ID setPos/setFacing precheck skips an ordinary
ally HP0 and bypasses that HP gate for pseudo30/31 and enemy-bit128/129.
Peter just
received immortal healing; a dead Zynk can still be created as a follower
and receive eas_Init through csc15, while its setPos command skips.

`data/scripting/entity/eas_actions.asm` owns the three actions. eas_Init
sets acceleration factors0, speed32, acceleration/deceleration off,
obstruction on, entity/map-uncollidable off, auto-facing on, orientation
up, immersed off, global size24 and sprite update, then idle. As the accepted
[action-semantics owner](./battle36-initialization-and-opening.md#action-hp-alias-and-wait-semantics)
records, eas_Init clears flagsA bits5/6, disabling the respective entity/map
collision checks at their consumers. eas_Jump disables auto-facing,
requests relative0,-1, waits5, relative0,1, waits5, relative0,0, enables
auto-facing and enters idle. eas_2xRightLeft sets Right/Left/Right/Left,
refreshing sprite and waiting30 after each, then idle. Its nonblocking
installation plus csWait120 is not measured completion or travel.

csc2C_followEntity instead prechecks the high byte of its word follower
operand. All three selected IDs are below256, so that byte is0: Bowie
HP0 skips the command, while a live Bowie permits following even for a
dead follower. This reuses the corrected
[word/byte boundary](./battle34-outcomes-and-sky-orb-handoff.md#entity-aliases-leader-and-following).
It then resolves both aliases and installs AddFollower with
signed pixel offsets: mode2=(-24,0) for Caravan relative to Bowie,
mode1=(0,-24) for Peter relative to Caravan, mode3=(0,24) for31 relative
to Caravan. AddFollower copies42 bytes, writes leader-slot/offset words
and adds the follower slot to EXPLORATION_ENTITIES if absent. It installs
motion without waiting for formation or changing party membership.
No Zynk follow command occurs at the tail.

loadMapFadeIn enters csc37, starts out-to-black and falls through csc48:
detach view, load tilesets, wait fade, deactivate VInts, LoadMap53 using
packed6,17 scaled for the view, enable display/VInts and wait once. It
does not invoke fresh exploration population/init or CheckBattle;
loadMapEntities is separate, as is later fadeInB. Neither load command
writes PARAM2–5. There is no warp/reset-return command in the complete
scene, so the seed is not replaced by6,17,11,20 or follower positions.
A hypothetical interleaving request can change shared bytes; static order
does not rule that out or prove completed services.

After csc_end, ExecuteMapScript conditionally waits for view when dialogue
is open, clears view scrolling speed and restores registers. Both run
and held536 skip paths reach EndAfterBattleCutscene, reread selector36
and call JoinForce0: set Bowie's joined0, rebuild lists, set active32 only
below12 active members. It does not join Zynk or insert a newly set32
into the earlier list snapshot. Victory rereads CURRENT_BATTLE, clears436
then sets536, clears d0–d3 words, copies PARAM2–5 bytes and returns d4=1.
PARAM1=0 differs from returned d4=1. F486,76/26/58,437/537 and drop bits
are untouched at this tail. Interrupted heal/seed/scene/join/clear/set
prefixes are not atomic.

## Defeat, retreat and Map53 to77

**Confirmed:** defeat updates unlock first (clear436 only if536 was
already held), clears name1, requests sad music/txt363/close, restores
Bowie HP=max, halves current gold by longword logical shift and selects
egress with d4=-1. No36-specific losable override sets536/486. At admitted
retreat, Egress consumes the selected spell's actual MP cost before274;
Angel Wing removes the actual used item slot and refreshes stats before275.
Both request sound/flash/close, update unlock, unwind the action frame,
select egress and clear d4.w. They bypass terminal suffix, defeat's gold/
Bowie recovery and after/join/completion tail. Reached resource/drop/flag
changes are not rolled back; common exploration healing is later.

`battleloop/getegresspositionforbattle.asm` has no36 override: it reads
actual EGRESS_MAP. GetSavepointForMap tests399; clear399 returns initial
3/56,3/Down before raft logic. Held399 uses23 savepoint rows; unmatched
53/77 retain their map with default1,1/Up. Examples38 Roft→21,27/Right
and31 Moun→20,8/Up remain different destinations. The
[shootdown owner](./roft-to-galam-and-battle36-admission.md#map53-default-setup-and-battle36-scan)
records an earlier Dojo egress write, not the actual later value after
visits/saves. Held64 separately resets raft coordinates for the chosen
destination. Battle36 does not mandate Galam or a church return.

MainLoop returns from BattleLoop through SwitchMap to ExplorationLoop
without CheckBattle or d4 branch. No flag-switched row replaces53/77.
Exploration clears pending event, adjusts STEP_COUNTER.w using source
signed-word subtraction/overflow, heals, fades and distinguishes explicit
destination from CURRENT255. Ordinary victory returns explicit53 even
after the scene loaded53: reset CURRENT_BATTLE255, load resources,
rebuild player/followers/fixed population, clear temporary256..383, set80,
load map/base VInts and run live setup init. Supplied CURRENT instead
retains entities/temp state; it is not unchanged36's return. WaitForEvent
installs controlled script/view0 only without pending event; that call
does not prove usable input or completed presentation.

The [Map53 setup owner](./roft-to-galam-and-battle36-admission.md#map53-default-setup-and-battle36-scan)
finds no53 route: ms_Void suppresses local init/entity/item/description/
zone callbacks. Common population is independent. Fresh return replaces
Geshp/Caravan scene records and resamples follower flags. No local53 zone
automatically reopens36. A later ordinary53 scan rejects36 with436 clear;
supplied436+536 admits36 now while clearing436, not completion-first
rejection.

Both complete53 warp rows are no-scroll/Down:255,0→77,57,32 and
255,31→77,57,35, with no536/537/party condition. Natural marker reach still
needs actual input/collision/request state. Reuse the
[Map77 zone/scan owner](./roft-to-galam-and-battle36-admission.md#complete-map77-setup-and-connection-tables):
clear537's invoked default zone sets437 and posts longword RELOADMAP;
held537 returns without clearing preheld437. The request writes only
type/PARAM1/PARAM2, retaining coordinate/facing bytes. A later explicit77/
CURRENT MainLoop scan needs437, selects37/crop16,32/48×32 with FF/FF
triggers, and held537 clears437 while still admitting37 this time.
Neither producer nor scanner tests536. Scroll, scene load, battle return,
zone request and admission are distinct entries. Ending36 does not
automatically initialize37. Stop here.

## Text and actual current consumers

**Confirmed:** `data/scripting/text/gamescript.txt` supplies complete
ordered tokens below. Only IDs/authored short paraphrases are public.
The scene requests use flags0 and speaker128, advancing from cursor2953.
Shared drop/retreat modifiers use actual actor/target, not Geshp.

| ID / authored role | Complete ordered tokens |
| --- | --- |
|2953 / Geshp notices Flowers|`["{W2}"]`|
|2954 / reacts to their loss|`["{W1}"]`|
|2955 / threatens return with Red Baron|`["{LEADER}","{N}","{N}","{W1}"]`|
|363 / exhaustion before recovery|`["{LEADER}","{W1}"]`|
|274 / actor's retreat spell/level|`["{NAME}","{N}","{SPELL}","{#}"]`|
|275 / actor's retreat item|`["{NAME}","{N}","{ITEM}"]`|
|396 / target128 drops66|`["{NAME}","{N}","{ITEM}","{D1}"]`|
|395 / allied receiver gets66|`["{NAME}","{N}","{ITEM}"]`|

LEADER rebuilds the active list and need not name hardcoded Bowie.
Requests do not establish display, acknowledgment, timing or completion.

One bounded lowering by unchanged
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) returned
no error: abcs_battle36 has15 instructions, cs_4D9F0 has29;
eas_Init/eas_Jump/eas_2xRightLeft have14/9/14 actions. No native-call
occurs in these instructions/actions. Branch76/continuation, scene-map53/
view6,17, entity population, motion/positions, mosaic/fade/text/portrait/
waits and three signed follow offsets are typed. This is source lowering
only: no map import, Content admission, runner, Godot service or original
execution. Preserved event-enabled53 import UnboundLocalError/zero partial
programs is neither rerun nor repaired by this successful lowering.

**Confirmed about accepted production code, by direct read:** production
consumes prepared Content; [capability status](../../remake/docs/capability-status.md)
owns runnable claims. Generic capacity is not selected36 binding.

| Actual consumer | Expression and selected36 boundary |
| --- | --- |
|[Preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Outcome hooks/join/battleOrigin/configured egress have shapes. Pin admits1/2/3, not36. Private outcome admission requires EnemyLeaderPresent=false and End/Return-only defeated program;36 satisfies empty hook but fails leader condition. No prepared36 map/material/party/outcome is supplied.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleDeathBatch](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Defeat priority and correct dead-ally/enemy-FIRST_ALLY accounting exist. Batch admits distinct positiveHP→0 changes, skips nondead/off-map and cannot reproduce arbitrary native lists/living-enemy leader sweep. No66/drop-bit/remove/AddItem/Deals service is bound here.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Pre-scene return, completion skip and join→unlock→complete exist. Active.First()/bound position/validated anchor differ from raw list/unchecked alias reads; nonzero statuses can refuse return-status-refresh. Egress reads configured route rather than arbitrary story.EgressMap and refuses absent399 or held64/640. No source retreat MP/item/unwind follows. Validation precedes tail publication, unlike native interrupted prefixes.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Typed branches/motion/position/follow/scene load exist. Scene load retains actors/program; separate entity reload waits for sprites. Bound text/display requires black/palette/closed-window context. Original byte-ID position HP-skip and high-byte/Bowie-HP follow precheck are absent from the unconditional lowered operations. Alias-zero can address physical0; F65-prefix omission remains. No admitted76/31/26 state is established. ReturnBattleMap uses OutcomeReturn; Apply can check admission for NewBattle, unlike original immediate return.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Mosaic/fade/text/entity/sprite consumers exist; generic mosaic timing is not original VInt parity. No36 material binding, completed scene/control/return observation or37 play is supplied.|

Future selected integration needs admitted36 content/material/outcome,
leader/drop semantics, exact scene population/HP/return bindings and
completed service/control observations. These gaps do not authorize
implementation, an engine-ready label or a runtime queue.

## Reproduction, retained failures and Unknowns

Load current ignored selections in the same process and use a new receipt.
The one lowering is retained in `local/issue1134/observe-01.json`; do not
repeat merely to change that receipt's head identity.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1134/review.py <configured-source-disasm-root> --receipt local/issue1134/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument ends in disasm and contains sf2enums.asm, code/ and
data/. The ignored recipe checks complete selected tables/scene/action/
branch/follow order, drop/item/state prefixes, common outcome/return
predicates, exact tokens/consumers and two-path scope/navigation-only,
UTF8/LF, links/private boundary. Tracked source/prose own findings;
recipes/receipts are coordination. Apply ADR0004 evidence/polarity/order/
width/provenance checks; parser/schema/fixture mutations and H3 callbacks
are N/A. Use clean committed planner and actual exact-head scoped CI.
Normal/full is SKIPPED by docs-only agreement, not executed PASS. No old
comparison, SDK/.NET/Godot/emulator/H3/H4/screenshots or acquisition/export.

Preserve #1132 original RNG hand-calculation/regex diagnostics and corrected
author0.2234s/main0.2198s PASS, CI39545472070, static laser boundary;
#1130 author/main0.332s PASS, CI39543645357. Preserve #1128's three prose
corrections (clear bits5/6 disable entity/map collision checks, moving a0's bit5, nonzero
leaderFF), #1126 Map53 event-enabled UnboundLocalError/zero partial and
earlier named alias/collision/follow/statistics/import failures. Twelve
completed old comparison failures remain discovery evidence; none is
closed, rerun or called interrupted here.

Freeze clean committed/pushed Draft PR, exact base/head/tree/checks/CI/
failures/Unknowns/process state and ignored handoff, Project Review / Verify,
notify main-gate once and stop writer. Independent acceptance/merge remains
main-gate-owned; no next Issue, merge, cleanup/archive or private upload.

**Unknown:** natural outcome/retreat/warp/zone reach; actual selectors,
flags/party/HP/MP/status/equipment/items/drop bits/Deals/gold/death list/RNG,
positions/facing/aliases/F88 retained state/EGRESS_MAP; service interleaving/
interruption and completed sprite/DMA/death/scene/view/text/portrait/audio/
fade/load/init/input, VInt/presentation timing, usable exploration and
save persistence. Broader malformed states/resource decoding remain open.
Battle37 initialization, full Galam and Dwarven Village remain excluded.
