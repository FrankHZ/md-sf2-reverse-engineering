# Battle31 outcomes and Battle32 admission

Evidence date: 2026-10-20. **Confirmed (static):** Battle31 selects empty
defeated/after scripts, but their wrappers still queue leader cleanup and
call JoinForce0. Unchanged victory clears431, sets531 and returns to fresh
Map2 exploration. It neither joins Frayja nor sets432. Battle32 requires a
later Map74 admission scan; preheld427 can select27 first. Defeat/Egress
instead select the Map36 castle savepoint. **Unknown:** natural outcome,
route reach, incoming state, completed services/control, presentation and
persistence. No Battle31/32 runnable-content claim follows.

## Agreement, provenance and method

Standard rigor for [Issue #1080](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1080)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect conditional outcome, returned state, separate travel and subsequent
admission. Accepted base is `5388b22c310a7ce46bb231300fbf3de9fdd867e4`,
including [activation/first dispatch](./battle31-activation-and-first-turn-dispatch.md)
and [opening/initialization](./battle31-initialization-and-opening.md).
Owned writes are this document and one predecessor navigation link. No
shared-path/stacked dependency; UI692 is independent. Begin after a returning
ExecuteIndividualTurn; stop at conditional BattleLoop32 handoff, before its
initialization, Frayja recruitment, Jaro or Moun story. Alternative returns
stop at their named exploration/battle entry. No action/AI/loot simulation,
whole-town resurvey, engine, tool/schema/fixture, extraction or runtime work.

Reuse the shared [Battle30 outcome lifecycle](./battle30-outcomes-and-mitula-scene.md),
[Battle29 returns](./battle29-outcomes-and-shrine-exterior-route.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[control lifecycle](../design/contracts/battle-control-lifecycle.md),
[cutscene routing](../design/contracts/battle-cutscene-routing.md),
[request state](./map-event-request-state.md) and
[request consumption](./map-event-request-consumption.md). Detailed town
population/interactions remain with [Pacalon entry](./battle26-return-through-pacalon-entry.md),
[town exits](./pacalon-interactions-and-castle-handoff.md),
[Frayja/castle offer](./pacalon-return-frayja-and-battle31-admission.md) and
[departure/Map74](./pacalon-departure-to-battle27-admission.md).

Original paths below are relative to `disasm/` at clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Existing evidence identifies USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no fresh ROM/H1 equality is claimed. `sf2enums.asm`/`sf2const.asm` own
constants and RAM. Fnnn means flag nnn; ordinals are zero-based; extents are
half-open. `battleloop/` and `cutscenes/` abbreviate children of
`code/gameflow/battle/`; `map/` means `code/common/scripting/map/`.

Minimum evidence is selected source/table/consumer joins and existing
fixtures/schemas. Reuse the unchanged18,285,979-byte map-init/script pair
and accepted0.083s provenance at `local/issue1005/pair.txt`, without extraction.
Linear bounded reads/checks budget less than one minute/512MiB, with small
ignored receipts in `local/issue1080/`. No assets/corpus acquisition or API
processing. Replan conflicting evidence or material supporting machinery.
ADR0004's branch/width/order/alias/provenance review applies proportionally;
no maintained parser or tests of an audit program are introduced.

## Conditional post-turn terminal processing

**Confirmed:** `code/gameflow/battle/battleloop_1.asm:BattleLoop`
(`$23A84..$23BB4`) resumes from ExecuteIndividualTurn, optionally executes
the debug kill-all branch, then calls the defeated wrapper,
ProcessKilledCombatants and CountRemainingCombatants in that order. Ally
count0 loses before enemy count0 wins. If neither terminates, it reads the
actual current turn-buffer byte, runs ProcessAfterTurnEffects, processes
deaths and repeats the same defeat-before-victory tests. There is no second
defeated hook in that suffix. Only a surviving suffix advances the byte
offset by2. Escape instead unwinds before this continuation.

The complete selected tables, not script names alone, establish:

| Table / complete extent | Battle31 selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot31/byte62 at `$47C26`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte31 at `$47CAD`: -1, a nonzero cleanup selector.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot31/byte62 at `$47D32`: ms_Empty.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte31 at `$47D89`:0, an actual JoinForce operand.|
|`data/battles/global/enemyitemdrops.asm`,30 four-byte rows plus word terminator, `$BE52..$BECC`|No Battle31 row.|

`cutscenes/battleendcutscenesstart.asm` requires Bowie0 HP.w nonzero and
enemy128 HP.w zero. It checks actual500+CURRENT_BATTLE: clear531 dispatches
ms_Empty, held531 skips dispatch. Both continue to
`battleendcutscenesend.asm:loc_47C48`, reread the battle and select leader
presence. Unchanged31's -1 scans all32 enemies128–159, appending every
nonzero-HP slot to the existing death list, incrementing its word length
and replacing the next sentinel. No X/membership/activation check and no
initial list clear occur. Fifteen declared enemies do not shorten the scan.
The separate afterbattlepositions table contains only battle5, no31 entry.

`map/ms_empty.asm` is one FFFF word. ExecuteMapScript still changes its
loading scratch and clears SKIP_CUTSCENE_TEXT; its end conditionally waits
for view scroll when DIALOGUE_WINDOW_INDEX is nonzero, clears scrolling
speed and restores registers. Skipping dispatch avoids these interpreter
effects, but neither wrapper tail. An empty script is no proof of a completed
service or a side-effect-free wrapper.

`battleloop/processkilledcombatants.asm` performs spin/effect/sound waits,
increments ally defeats or enemy kills attributed to supplied
BATTLESCENE_FIRST_ALLY, writes X/Y=-1, clears status/recomputes stats and
moves the mapped entity to `$7000,$7000`. It does not set HP0, clear the
list length or invoke the action-side drop transaction. Thus queued living
enemies can disappear from the count through X alone. Existing stale or
duplicate entries/mappings and interrupted prefixes remain supplied state.
No Battle31 row exists in the drop table. Separately, all27 spriteset
placement item operands are NOTHING; this does not mean empty inventory.
`battleloop/initializecombatants.asm:InitializeEnemyStats` first copies the
full56-byte enemy definition. InitializeEnemyItems returns on ITEM_NOTHING
before AddItem, without clearing those copied items. Thus no placement item
is added, while definition equipment remains: fresh enemy128 Devil Soldier
retains GREAT_AXE|EQUIPPED from `data/stats/enemies/enemydefs.asm`, definition23.
The [opening inventory owner](./battle31-initialization-and-opening.md#full-spriteset-and-fresh-construction)
retains the other selected definitions. No dedicated drop row is distinct
from retained equipment, other action-side rewards or arbitrary inventory.

`countremainingcombatants.asm` counts signed X.b>=0 and HP.w!=0 across
all30 allies/all32 enemies, without Y, membership, neutrality, activation
or commandset tests. Bowie HP0 forces ally count0 even if others survive;
simultaneous exhaustion loses. High-bit nonzero HP passes. Off-map enemies
can allow victory without the leader hook; a second-pass poison kill of128
does not run that hook again there. F88 supplied entry does not manufacture
a fresh roster/list/selector. Changed CURRENT_BATTLE between guard, script,
leader and outcome reads can select different flags/tables; no snapshot or
arbitrary-state safety is supplied.

## Victory seed, join tail and independent flags

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies over30 slots, without membership filtering.
Peter7/Lemon28 heal even at HP0; other nonzero-HP allies heal to max HP/MP.
Other dead allies stay dead. It retains existing STUN/POISON/CURSE, clears
other status bits and recomputes stats. Battle44's timer branch is not31.

Before the after wrapper, victory saves CURRENT_MAP.b to PARAM2, calls
UpdateForceAndGetFirstBattlePartyMemberIndex, reads that member's actual
combatant X/Y and byte-adds crop X/Y into PARAM3/4, resolves its mapped
entity facing into PARAM5, then writes PARAM1=0. It does not post an event
type. UpdateForce rebuilds the joined/active/reserve counted prefixes and
word counts from joined0–29/active32–61 flags in member-ID order; uncounted
tails remain. The helper reads the first active byte without a count/HP
check. This need not be Bowie, a living/placed member or the first turn actor.
With unchanged Map2/crop `(0,0,48,32)`, the seed is map2 and those actual
coordinate bytes/facing, with byte wrapping, not a fixed victory tile.

`battlefunctions/battlefunctions_0.asm:GetEntityIndexForCombatant` returns
the lookup byte unchecked. Only zero mapping aliases physical0.
PositionBattleEntities uses FF for an unplaced ally; victory shifts it by5
and reads facing at `ENTITY_DATA+$1FE0+$10`. It applies neither LoadBattle's
negative fallback nor a3F mask. FF is not zero alias or proof of a valid
entity. Changed mapping bytes and retained empty-list bytes remain distinct
limitations. The return seed is already saved before JoinForce0 can change
membership or lists.

`afterbattlecutscenesstart.asm` checks actual500+CURRENT_BATTLE: clear531
executes ms_Empty, held531 skips it. Both reach EndAfterBattleCutscene,
reread the battle and call JoinForce with byte0. JoinForce sets F0, rebuilds
lists, then sets F32 only if the resulting active count is below12. At
capacity it does not clear a supplied F32. A newly set F32 is absent from
the already-built active prefix until another rebuild. There is no stats
initialization, join ceremony or correction of the earlier saved seed.

After wrapper return, BattleLoop rereads CURRENT_BATTLE, clears400+index,
then sets500+index: unchanged31 clears431 then sets531. It clears d0–d3
**words**, copies PARAM2–5 bytes, sets d4=1 with moveq and returns. Upper
register halves are not established by clr.w. PARAM1=0 differs from the
returned success indicator d4=1. A changed selector can change the final
flag pair independently of the guard/join selections.

| State authority | Effect of intact selected31 victory |
| --- | --- |
|431 /531 /481|Clear unlock431, then set completion531; intro481 remains supplied. Held531 skips only script dispatch, not join/final writes.|
|82 /811|No write. Follower declaration82 and castle-offer latch811 retain their independent history.|
|22 /54|No write to Frayja's joined22 or active54 flags. Rebuilds may reflect their supplied values in cached lists.|
|432 /532|No write. Battle32 unlock/completion are not Battle31's outcome flags.|
|530 /527 /427|No write. Restored-map substitution, earlier-battle completion and stale earlier unlock remain independent.|

Interrupted healing, seed, empty-script end wait, JoinForce,431-clear or
531-set leaves only its reached mutations; no atomic group, rollback,
usable control or save persistence is established. Changed flags/party
between rebuilds can change lists without retroactively repairing PARAM2–5.

## Defeat, Egress and castle exploration entry

**Confirmed:** BattleLoop_Defeat first runs UpdateBattleUnlockedFlag: clear431
only if531 is already held. It clears dialogue name1, requests sad music,
text363/close, restores **Bowie0** HP=max, halves unsigned gold, selects
egress and returns d4=-1. Battle4's losable upgrade is not31. Text363's
LEADER token, if normal parsing reaches it, calls symbol_leader →
UpdateForceAndGetFirstBattlePartyMemberIndex → j_UpdateForce before name
selection. Thus cached lists/counts can rebuild before HP/gold recovery;
the displayed name need not be Bowie. Before parsing reaches the token,
that rebuild has not occurred. It writes neither joined22 nor active54.

`battlefunctions/executeindividualturn.asm` branches admitted Egress/Angel
Wing to `battlefunctions_1.asm`. Egress deducts selected spell MP cost
before text274/cast flash; Angel Wing removes the selected item slot (with
stats refresh) before text275 and that common tail. The tail closes text,
updates unlock, unwinds the action frame, selects egress and clears d4.w.
It bypasses the post-turn terminal pass, defeat's gold/Bowie recovery and
after/join tail. Neither path newly sets531 or changes481; reached earlier
combat/cost/item writes persist. No service is guaranteed to finish.

`battleloop/getegresspositionforbattle.asm` checks actual battle31 and uses
moveq MAP_PACALON_CASTLE36, ignoring EGRESS_MAP on this selected branch.
GetSavepointForMap scans the complete23-row savepoint table: row22 is
`36,(40,5),Up1`. BattleLoop set399 on both fresh/F88 entry, so unchanged
reached defeat/escape returns that exact low-word seed. Changed selectors
can select other special cases or live EGRESS_MAP instead; this is not an
arbitrary-state guarantee. Direct F399-clear entry returns initial3
`(56,3),Down3` before raft handling. With399/64 held, Map36 matches the
four-row raft-reset table's default: RAFT_MAP69, X42,Y15; clear64 leaves
raft state unchanged. F64 is not cleared by that reset.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls SwitchMap then
ExplorationLoop for every completed battle return: **no d4 sign test and
no intervening CheckBattle**. The full switch table is3/F640→4,
71/F530→78,58/F999→59. Neither explicit2 victory nor36 escape/defeat is
substituted. The latter stops here at Map36 exploration entry; no church,
save, revival, dialogue or castle service completion is claimed.

## Successful town return and the existing outbound route

**Confirmed:** explicit2 victory enters
`exploration/explorationfunctions_2.asm:ExplorationLoop`: clear pending
word, subtract20000 from STEP_COUNTER with signed bge/zero fallback,
heal living/immortal allies again, fade, set CURRENT_MAP2/CURRENT_BATTLE255,
load tilesets, rebuild entities/mapsprites, clear temporary256–383, set80,
load map/view and invoke selected init before WaitForEvent. Full town
population and control guards remain with the linked entry owner. F530
selects normal/flag530 populations; both use the RTS ms_map2_InitFunction.
ClearEntities replaces battle actors/lookups/walking scratch; flag followers
are reconstructed. F82 may declare Frayja22 without joining her;22/54 and
other persistent flags survive. There is no Caravan prefix on town2.
Pending zero installs mode-selected control/view0; a new pending event
dispatches first. Entry is not proof of usable control.

The rechecked route joins are:

| Separate reached boundary | Selected request / lifetime |
| --- | --- |
|Map2 south warp row3, `$96A74`|Source `(255,31)`, no-scroll71 `(22,16)`, Down3. Earlier castle rows retain precedence. No531/811/82/22/54 predicate.|
|MainLoop after that no-scroll return|Switch71 to78 iff F530 is held when read. Neither71 nor78 has a battle row; fresh explicit exploration rebuilds actors, clears temporary flags and resets CURRENT_BATTLE255.|
|Map71/78 north warp row2, `$C06A4`/`$C69F4`|Source `(255,0)`, scroll Up `$11` to74 `(255,62)`. North edge differs from town entrances at Y15.|
|Scroll consumer|Idle main; set CURRENT_MAP74; transition blocks/layout/rectangle; retain actual X/XDEST for FF; set Y/YDEST62*384; sample main, DeclareRaftEntity; return within existing exploration.|
|Separately admitted Map74 zone|Ordinary layout-kind$1400 producer posts6 plus candidate X/Y words. Row `(255,61)` selects Map74_ZoneEvent0; default returns. No scroll-to-callback call.|

Map71/78 have **no MapSetups record and no own s6_initfunction**.
GetCurrentMapSetup returns ms_Void/FFFF: fixed-list sentinel/init skip,
while shared main/follower/raft construction still runs. As overworlds,
F65 can add Caravan before ordinary flag followers. The north scroll does
not rebuild these actors/mappings/scripts/scratch, clear temporary flags,
heal, subtract20000, reset CURRENT_BATTLE, run init or enter MainLoop.
Main's action pointer changes; DeclareRaftEntity can relocate followers
according to mode and update the F64 raft. Population retention does not
mean all actor fields stay unchanged.

Map74's single ms_map74 setup has an empty fixed list and RTS init; a
scroll invokes neither. Later zone selection resolves actual CURRENT_MAP
and live setup. Full area/view metadata refresh is also separate from
the scroll's rectangle read. Stale non255 CURRENT_BATTLE can affect block/
layout cropping; ordinary fresh71/78 has255. Actor/layout/request details
remain with the [accepted departure owner](./pacalon-departure-to-battle27-admission.md#ordered-north-scroll-and-retained-state).
Rows and metadata prove no traversable continuous path, natural contact or
completed rendering. No binary geometry is decoded here.

## Map74 callback, partial request and Battle32 handoff

**Confirmed:** `data/maps/entries/map74/mapsetups/s3_zoneevents.asm`
contains the complete local predicate/write order below. Zone selection
tests low bytes with exactFF wildcard, after the ordinary movement/layout
admission. Posting a zone need not prove completed movement to that cell.

| State when actually read | Ordered callback effects |
| --- | --- |
|527 clear|Set427; longword reload write; set428; set429; set430; return.|
|527 held,811 clear|Return without these writes.|
|527 held,811 held,532 held|Return without these writes.|
|527 held,811 held,532 clear|Set432; longword reload write; return.|

There is no531/82/22/54/530 test, completion write, JoinForce, random check
or battle call here. No-write branches preserve existing427/432. The
[castle offer](./pacalon-return-frayja-and-battle31-admission.md#independent-state-lifetimes-and-interrupted-prefixes)
sets811 then82 only after its scene returns; its three unlocks are431/433/434.
LEADER parsing can earlier rebuild lists, and following can skip on Bowie's
high-byte HP precheck. Neither that offer nor Battle31 victory supplies
membership22/54 or432. F811 is a predicate, not proof that every earlier
service/follow effect completed;531 is not a prerequisite for this callback.

The reload is `move.l #MAP_EVENT_RELOADMAP,((MAP_EVENT_TYPE-$1000000)).w`,
constant `$000100FF`: event word1 at `$FFA84A`, PARAM1=0 at `$FFA84C`,
PARAM2=CURRENT255 at `$FFA84D`. It leaves PARAM3/4/5, padding and WARP_SFX
untouched. With the intact ordinary zone producer's Y word003D, PARAM3/4
become later warp **X byte0/Y byte61**, not coordinate-preserve sentinels.
PARAM5 may retain the north warp's Up1 only if no writer replaced it.
Changed high Y bytes can independently change later X despite a61 match.
Flag-before-request and later428–430 writes are non-atomic prefixes;
another producer may replace this single mutable slot.

After callback return the zone wrapper closes portrait/text, waits a VInt
and waits for main's actual coordinates to equal destinations. It retains
the new pending request. Only if those services return can WaitForEvent
consume it before installing control/A/C input. ProcessMapEvent clears
stored type; the no-scroll warp submits/clears WARP_SFX, idles main, removes
the exploration return address, samples actual main words/facing, then
applies live PARAM3/4 nonnegative **low-byte** replacements and PARAM5.
Negative bytes preserve sampled words; high bytes survive nonnegative
replacement. Ordinary small coordinates yield CURRENT255 `(0,61)` with
retained PARAM5 and d4=0, a register seed rather than immediate actor motion.
Every field uses its actual consumption-time value.

MainLoop @Start calls SwitchMap (no255 rule), then CheckBattle. The latter
resolves CURRENT from actual CURRENT_MAP, scans all45 seven-byte records,
tests map/unlock then wildcard/exact trigger bytes, writes crop, and only
then checks completion. Exactly two rows select74:

| Scan state / candidate | Result before BattleLoop |
| --- | --- |
|427 held, either432; row27 `$7AF3`|Choose27 first; crop `(2,16,32,48)`; held527 clears427 but still returns27 now.|
|427 clear,432 held; row32 `$7B16`|Choose32; same crop; held532 clears432 but still returns32 now.|
|427/432 both clear|No match, d7=-1, no new crop; restore original map operand255.|

Both rows' triggers are255/255. Neither outgoing `(0,61)`, callback reach,
being inside the crop nor F531 is an extra scan predicate. Hence the
811-held callback can set432 while stale427 still wins27. A532-held no-write
callback can coexist with preheld432 and an independently produced request
that admits32 once. Changed CURRENT_MAP redirects the scan; other maps can
select their own battles. CheckBattle does not write CURRENT_BATTLE.
Success retains resolved d0=74 and returns d7; MainLoop copies d7 to d1 and
calls j_BattleLoop. **Stop at this call for32 or alternative27**, before
initialization, recruitment, regions, turns or later story.

No-match instead enters CURRENT exploration: pending/counter/heal/fade and
main/raft updates, retained actors/lookups/scratch/temporary flags and
CURRENT_BATTLE, LoadMap's CURRENT resource-retaining reload, area/view
refresh and selected RTS init. Stop at this named alternative; no promised
control follows. A supplied explicit map takes fresh construction instead.

## Actual Content, Domain, Application and Godot boundaries

**Confirmed about the accepted repository:** production consumes prepared
Content. Expressible rules, admission guards, missing bindings and fidelity
assessment are separate. The [capability owner](../../remake/docs/capability-status.md)
controls runtime scope; no package or running instance is created here.

| Actual consumer | Existing mechanism | Guard, missing binding or unassessed fidelity |
| --- | --- | --- |
|[PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Encounter data, outcome routes/flags/after/defeated/join/egress/return programs have shapes.|Pin admits1/2/3, rejects31/32. Private outcome-source additionally requires EnemyLeaderPresent=false; selected31 is true. defeated-program requires only End/Return. Empty script does not admit31 leader cleanup. Missing selected world/encounter/party/policy/material bindings.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Placed living actors, leader-loss precedence, defeated gate and outcome observations exist.|Actor set is not original raw30/32 slots/death-list capacity; current hook observation says enemy-defeated-program-none. No accepted31 leader-presence sweep, supplied death-list/selector/interruption parity.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Save return before after program; completion skip; Join then clear unlock/set completion; flag-derived lists preserve JoinForce's pre-active-write list. Defeat HP/gold and heal policy exist.|Source policy requires active.First()/bound actor/position, unlike unchecked empty-list/FF reads. Nonzero status refresh and defeat absent399/held64 or640 refuse. No31 Map36 override/return binding, general retreat or live-selector race acceptance.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [map preparation](../../src/sf2tool/remake_exploration_maps.py), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Flag branches/writes/jumps, End/Return and text/party operations are representable; native-call becomes UnsupportedInstruction and reached program-opcode refusal.|Direct Map74 lowering retains branches/writes but its two move.l reloads are native-call; no selected partial pending-slot implementation follows. Scrolling/retained-coordinate warps emit warp-frontier. No preparation or runtime execution is implied by lowering.|
|[MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [BattleEntry](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Candidate/marker-selected source zones, wrapper waits, setup/population, preserve/rebuild and ordered route selection exist. Private completed admission can clear unlock yet select now; F88 refuses suspended-battle-entry.|Loaded Definition.Events is not proof of original live setup reselection after retained scroll; mutable byte request, word/sentinel behavior and route27-before32 need bindings/assessment. SourceRandomBattleRunner has a reload-like BattleRequest consumer, but Map74's deterministic callback is not bound to it and does not call the random helper.|
|[ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|LEADER can rebuild PartyLists from PartyFlags; logical field text/view/entity/presentation consumers exist.|No selected363/return/zone execution. Actor/portrait/bleep/map/audio/material binding, exact token time, frame waits, rendering/control/persistence remain unassessed. Passing the opening's Map2 metadata guards does not supply those bindings.|

Preserve the [opening's corrected nested refusal](./battle31-initialization-and-opening.md#actual-preparation-and-consumer-gaps):
lowered15 ac_animSpeedX2 can refuse in enabled entity service before top-level
cameraSpeed18. Activation's commandset/ENHANCED/MAGE/equipment guards remain
separate from Pin rejection. Outcome research does not erase these prerequisites
or establish later Battle32 initialization/Frayja recruitment.

## Reproduction, changed states and stopping boundary

Load ignored configuration in every tool-launch process. Direct bounded audit:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1080/review.py <configured-source-disasm-root>
git diff --check
```

Check complete selected tables and existing fixtures/schemas; post-turn,
death/count/heal, party/text/lookup, empty interpreter/wrapper/tail, ordered
flags, defeat/escape/savepoint/raft, MainLoop/switch and source route/request
joins; actual Content/Domain/Application/Godot consumers and direct Map74
lowering. Review all callback branches, preheld/completed flags, changed
selectors/party/HP/mappings, F88, skipped/not-reached LEADER, interruption,
partial payload/high bytes and stale CURRENT_BATTLE separately. Then check
full prose, UTF-8/LF, links/anchors, private boundary, exact two owned paths
and predecessor navigation-only diff. On clean committed HEAD run
`uv run sf2 verify plan --base origin/main --head HEAD`; record actual
exact-head scoped CI and receipts in the Issue/PR handoff. Documentation
scope skips blanket normal/full Python/.NET/Godot/H3/H4, audit-program tests
and old comparisons.

Retain #1078 author0.332s/independent0.326s/CI39486144367 and its diagnostics;
#1076's missed nested-action refusal and focused correction; #1074's LEADER,
#1070's N==V, #1068's high-byte HP and #1066's geometry corrections with
their preserved failure/Unknown owners. The unchanged pair and twelve
completed old private comparison failures remain outside this rerun queue.

Stop at a clean current-main-based committed/pushed Draft PR, Project Review /
Verify, exact head/tree/base/checks/CI and successful main-gate callback,
with writer/process stopped. No merge, cleanup, archive or next Issue.

**Unknown:** natural victory/defeat/escape and route/contact reach; supplied
party/HP/items/flags/list tails/death entries/crop/aliases/lookup/EGRESS_MAP/
selectors/mode/pending bytes; completed text/action/fade/scroll/cleanup,
interrupt interleavings/capacity, precise rendering/audio/timing, usable
control and persistence. These are not an automatic runtime queue. Original
evidence remains separate from remake design and acceptance.
