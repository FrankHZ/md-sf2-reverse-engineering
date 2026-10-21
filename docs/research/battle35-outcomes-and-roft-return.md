# Battle35 outcomes and Roft return

Evidence date: 2026-10-20. **Confirmed (static):** Battle35's defeated and
after scripts are empty, but its nonzero leader-table entry still removes
living enemies after Bowie survives and enemy128 dies. Victory saves a
Map76 return seed before the after wrapper, joins Bowie through the common
tail, clears435 and sets535. Heat Axe40 is a separate action-side drop
from combatant133, not a victory award. Defeat and admitted retreat use
the actual EGRESS_MAP; neither has a Battle35-specific Roft destination.
Map76's town warp and Roft's savepoint are distinct entry seeds. Stop at
the existing [Roft entry owner](./nazca-to-roft-and-battle35-admission.md#the-separate-roft-warp-and-fresh-population).
Natural reach, actual state, completed services, usable control,
presentation and saved persistence remain **Unknown**. This is not a
playable35 or completed town-services claim.

## Agreement, sources and bounded method

Standard rigor for [Issue #1122](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1122)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect selected35 outcomes, item delivery and exploration return for later
content integration. Accepted base `ef934b0dad579cdccfad0c196b26bcf9b530d84a`
includes [initialization](./battle35-initialization-and-opening.md),
[activation/first dispatch](./battle35-activation-and-first-turn-dispatch.md)
and [Nazca/Roft admission](./nazca-to-roft-and-battle35-admission.md).
Own this document and a navigation-only link in the activation owner.
There is no stacked dependency or competing writer. Shared index, registry,
schema, fixture, design and engine paths are excluded.

Begin at the suffix of a returning individual turn and the selected
action-side drop/retreat seams. Individual-turn AI/combat algorithms,
Roft's full town/services/recruitment, Map77 internals and subsequent
encounters are excluded. Reuse [Battle34's shared outcome flow and corrected
accounting](./battle34-outcomes-and-sky-orb-handoff.md), [battle loop](./battle-loop.md),
[cutscenes](./battle-cutscenes.md), [global data](./battle-global-data.md),
[common maps](./common-maps.md), [request producer](./map-event-request-state.md),
[request consumer](./map-event-request-consumption.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md). Shared seams do
not substitute for selected35 tables, actual item slots or current consumers.

Original paths below are relative to clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2const.asm`, `sf2enums.asm` and source macros own constants and widths.
`battleloop/`, `battlefunctions/` and `cutscenes/` abbreviate children of
`code/gameflow/battle/`; `map/` means `code/common/scripting/map/`.
Fnnn means story flag nnn; ordinals are zero-based; intervals half-open.
The USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equality, decoding or runtime observation is asserted.

Minimum evidence is complete selected wrapper/table/helper joins, current
Content/Domain/Application/Godot consumers and small changed-state predicates.
Reuse the immutable18,285,979-byte init/script pair and its0.083s provenance
at `local/issue1005/pair.txt`. One pair load and linear selected reads have
a one minute/512MiB budget per direct audit; peak memory is unmeasured.
Small unique receipts stay in `local/issue1122/`. Empty selected hooks need
no forced scene lowering. No acquisition, regenerated export, new parser,
fixture, environment/platform, old comparison or runtime gate. Reassess a
material contradiction or supporting-tool expansion before increasing scope.

## Terminal selection and the selected leader cleanup

**Confirmed:** `battleloop_1.asm:BattleLoop` (`$23A84..$23BB4`) returns
from ExecuteIndividualTurn, optionally performs the debug kill-all, then
calls the defeated wrapper, ProcessKilledCombatants and
CountRemainingCombatants. Zero allies selects defeat **before** zero enemies
selects victory. A nonterminal first pass applies ProcessAfterTurnEffects
to the actual turn-buffer actor, processes deaths and repeats the same
count priority, without another defeated hook. Only a nonterminal second
pass increments the byte cursor by2. Admitted retreat unwinds before this
suffix. This does not describe any particular turn's action or natural result.

| Complete selected table | Battle35 result |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, `$47BE8..$47C48`|slot35/byte70 at `$47C2E`: ms_Empty.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, `$47C8E..$47CBB`|byte35 at `$47CB1`: -1, nonzero cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, `$47CF4..$47D54`|slot35/byte70 at `$47D3A`: ms_Empty.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, `$47D6A..$47D9E`|byte35 at `$47D8D`:0, passed to JoinForce.|
|`data/battles/global/afterbattlepositions.asm`, `$47E6C..$47E82`|Only battle5 has a row; no35 reposition override.|

`cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated`
requires Bowie0 HP.w!=0 and enemy128 HP.w=0. The selected formation's128
is **Bow Rider**, not the temporary Geshp scene actor128. Clear535 dispatches
ms_Empty; held535 skips it. Both reach `battleendcutscenesend.asm:loc_47C48`,
reread CURRENT_BATTLE.b and test its leader byte. Unchanged35's -1 sweeps
all32 enemies128–159, appending every nonzero-HP slot to the **existing**
death list and updating its word length/next sentinel. There is no X/Y,
membership, neutral, activation or spawned-appearance gate and no initial
list clear. Nonzero off-map HP can therefore be appended as well.

`ApplyPositionsAfterEnemyLeaderDies` (`$47D9E..$47E6C`) has the same
Bowie/enemy128 HP gate but a separate word-battle lookup. Its after-battlescene
listener call is in ExecuteIndividualTurn; the complete one-row positions
table misses35 and returns at the terminator. It does not relocate35,
revive128 or supply a special post-battle party arrangement. This is a
selected listener boundary, not an expansion into the individual-turn body.

ms_Empty is just FFFF in `map/ms_empty.asm`. ExecuteMapScript still writes
its scratch pointer, clears text-skip, conditionally waits for view when
dialogue is open and clears view scrolling speed on return. Skipping a hook
avoids those interpreter effects; it does not skip the leader sweep or
after-wrapper JoinForce tail. No selected after dialogue, reward scene or
new scene population exists here.

### Death processing and hidden counts

`battleloop/processkilledcombatants.asm` (`$24518..$24642`) requests death
spins, sprite/VInt waits, death sound/effects and sleeps, then accounts for
each listed ID. A dead ally's defeats belong to **that listed ally**;
only the enemy branch temporarily loads BATTLESCENE_FIRST_ALLY for kills,
then restores the dead enemy ID. With Jaha3 listed and FIRST_ALLY=Bowie0,
3 receives the defeat; a listed enemy credits0's kill. This preserves the
accepted #1104 correction. It writes listed X/Y=-1, clears status,
refreshes stats and puts the mapped physical entity at `$7000,$7000`.
It does not set HP0, clear death-list length or call DropEnemyItem.
Living enemies queued by the leader sweep can disappear from counts through
X alone; duplicate/stale entries and mappings are not repaired here.

`CountRemainingCombatants` (`$23C58..$23CBA`) scans all30 allies and32
enemies, accepting signed X.b>=0 and HP.w!=0. It ignores Y, joined/active
membership, neutrality, activation and declared spriteset count. High-bit
HP is nonzero and passes. A final Bowie0 HP0 overrides the ally count to0.
This differs from the activation geometry's positive signed HP condition.

| Supplied state at the actual count/hook | Result boundary |
| --- | --- |
|Fresh unchanged13 STARTING, hidden141–145 HP0/X−1|Thirteen enemy slots count before any deaths, even if activation bits are clear.|
|Unspawned five remain HP0/X−1; other enemies gone; eligible ally and Bowie survive|They do not delay victory or force a hidden spawn before return.|
|One successfully reset hidden slot has nonnegative X and HP55|It counts until HP0 or off-map; maxHP or appearance completion is not the count predicate.|
|128 HP0, Bowie alive; five hidden already have nonzeroHP|Leader sweep appends them too; cleanup can remove them by X without an action-side kill/drop.|
|128 HP0, Bowie alive; five hidden still HP0|Sweep skips those five; they are not killed or initialized by the sweep.|
|Bowie HP0, even with another living ally and no enemies|Defeated wrapper skips; ally count forced0, defeat wins.|
|Both counts0|Defeat wins. All allies off-map also loses even if Bowie HP is nonzero.|
|128 alive but every enemy fails X/HP count|Victory can occur without the leader hook.|
|128 first reaches HP0 in the second after-turn pass|No second defeated wrapper in this suffix; other eligible enemies can keep battle running.|
|535 already held|Defeated script skips, but its gated leader sweep still runs; counts/priority are unchanged.|

Spawn success, death-list admission, cleanup, terminal selection and service
completion are separate events. Interruption retains the reached prefix.
F88 supplies retained queue/slots/aliases rather than fresh guarantees.
Each wrapper/service rereads its own CURRENT_BATTLE; if changed, table,
completion flag, cleanup and later return decisions can select differently.
No single immutable35 selector snapshot is imposed on original code.

## Heat Axe: declaration, actual inventory and Deals

**Confirmed:** the complete `data/battles/global/enemyitemdrops.asm` has
30 four-byte rows plus a word terminator (`$BE52..$BECC`). Ordinal13 at
`$BE86` is `TO_ROFT / enemyEntity5 / HEAT_AXE / droppedFlag13`.
`sf2macros.asm:enemyEntity` emits5+128=**133**, not combatant5 or all
Dragonewts. The selected base spriteset has Dragonewts131/133/136;133 is
the row at23,26. Their definition26 copies Heat Axe40|EQUIPPED plus three
NOTHING items. Placement's NOTHING adds nothing; it does not erase that
weapon. Only133 has this selected table entitlement. Geshp is not its owner.

`battleactions/dropenemyitem.asm:battlesceneScript_DropEnemyItem`
(`$BD24..$BE52`) has three direct action-engine calls. At this seam:

1. Actor's enemy bit must be clear, target's enemy bit set, and
   targetDies must be nonzero. Match the live battle byte and full target
   combatant byte against the ordered table.
2. GetItemSlotContainingIndex scans four actual target slots, masks item
   indices and returns the first match or−1. The table declaration alone
   cannot grant a missing item. A broken/equipped matching entry can match.
3. The three random-drop special items are Taros Sword, Iron Ball and
   Counter Sword; Heat Axe is none of them. Its matching row goes directly
   to DropItem, without the range32 draw. The later IsBattleUpgradable
   block is unreachable and does not add a repeat-battle probability.
4. BSET tests/sets the independent ENEMY_ITEM_DROPPED_FLAGS bit13:
   `$FFF7A4+floor(13/8)=$FFF7A5`, mask20 hexadecimal. An already set bit
   returns **before removal/award**. It is not story F13,435,485 or535.
5. On first admission, remove the matched slot and arrange/refresh the
   target's stats. Then read the actual allied actor's HP. HP0 bypasses
   inventory award. Otherwise AddItem tries that actor's four slots;
   failure routes to the rare-item fallback.
6. Success stores the masked item index40, without EQUIPPED, and requests
   dropped396 then received395 messages. It does not scan the whole force,
   reserves or Caravan for room. The failed/dead-actor branch adds a RARE
   item to Deals and does not emit these two success messages.

Heat Axe40's complete definition is GLDT|BRN|RDBN equip flags, range1..1,
price7200, WEAPON|RARE|BREAKABLE, useSpell BLAZE|LV2 and equip effects
INCREASE_ATT32, NONE0, NONE0. AddItem's mask retains index/broken bit,
but the drop passes bare40; it does not transfer the target's equipped or
broken word. AddItemToDeals uses one nibble per item and increments until
count15; at15 it dumps the addition. A full inventory with Deals at15 can set
the drop bit/remove the weapon without increasing either inventory or Deals.

| Changed state / interruption | Consequence |
| --- | --- |
|133 owns40; drop bit clear; living allied actor has room|One unequipped40 can be added, conditional on all preceding calls returning.|
|Same, actor full or HP0|RARE fallback attempts Deals; no mandatory discard or party-wide recipient search.|
|133 lacks40 after changed/upgraded initialization|No matching item slot; bit13 is not newly set and nothing is awarded.|
|131 or136 owns40|Their weapons do not match133's row; no generic Heat Axe drop from this table.|
|Bit13 already set, even after defeat/reentry/new weapon copy|No removal or new award at this drop seam.535/485 alone do not reset it.|
|Leader sweep removes a living133 / victory without reaching drop caller|Cleanup does not attempt the drop; victory does not repair it.|
|Interrupt after BSET, before removal or award|Bit may already suppress retry although no item reached the actor/Deals. No rollback is supplied.|
|Interrupt after removal/award, before message or completion535|Inventory and drop state can precede story completion; no save/load result is proved.|

Completed TO_ROFT is an upgrade-enabled random battle. Reuse the accepted
[live upgrade boundary](./battle35-initialization-and-opening.md#to_roft-and-completed-battle-upgrades):
535, Bowie's effective level/class, current selector, movement category,
bounded draws/exclusions/retry and the airborne underflow matter. A
reinitialized133 may have another current loadout. Check the actual slots;
do not promise40 from the original definition, or award all equipped axes.
The independent drop bit can remain set across a battle return/retry; no
save persistence or reset outside these seams is inferred.

## Victory seed, empty after wrapper and completion tail

**Confirmed:** `battleloop_2.asm:BattleLoop_Victory` (`$23CBA..$23D98` file)
first calls HealLivingAndImmortalAllies. It visits30 slots, heals living
allies and Peter7/Lemon28 even at HP0, retains other dead allies, retains
STUN/POISON/CURSE and refreshes stats. Battle44's timer special case is not35.

Before the after wrapper it saves CURRENT_MAP.b→PARAM2, rebuilds force
lists and reads the **first active-list byte**. It adds BATTLE_AREA_X/Y
bytes to that combatant's actual X/Y into PARAM3/4 and reads facing through
its physical alias into PARAM5. It writes PARAM1=0 without posting an
event type. UpdateForce orders joined/active/reserve prefixes by member ID;
it does not require this first member alive/positioned or the list nonempty,
and does not clear stale tails. It need not be Bowie or the last actor.
Unchecked alias0 can name physical0; FF shifts into a facing read at
`ENTITY_DATA+$1FE0+$10`, without LoadBattle's negative fallback/3F mask.

Unchanged map76/crop16,12 saves76 and byte-wrapped `memberX+16,memberY+12`.
For supplied member45,8 this is61,20;250,250 wraps to10,6. These are
arithmetic examples, not measured battle-ending positions or traversal.
The seed is not the opening scene's43,9, a hidden spawn, a camera location,
the reload request's X0, or a fixed town/church point.

`ExecuteAfterBattleCutscene` checks actual500+CURRENT_BATTLE: clear535
dispatches ms_Empty; held535 skips it. Both reach EndAfterBattleCutscene,
reread the selector and call JoinForce with selected byte0. For unchanged35
this sets Bowie's joined flag0, rebuilds lists, then sets active32 only
when active count<12. A preexisting32 remains at capacity; a newly set32
is absent from the list snapshot just rebuilt. It joins neither Zynk26
nor a follower pseudo actor. Even the completed-repeat path runs this tail.

After wrapper return, reread CURRENT_BATTLE, clear400+index then set
500+index: unchanged35 clears435 then sets535. F485 is untouched, as are
Roft910/911/921, Zynk76/26/58 and the item-drop field at this tail.
Clear d0–d3 **words**, copy PARAM2–5 bytes into them, return d4=1.
Upper longword halves are not established by clr.w; PARAM1=0 is a separate
value from returned d4=1. Parameter bytes and current selectors remain
mutable across calls. Interrupted seed/empty-script/join/clear/set prefixes
are not one transaction; no reward-possession condition gates535.

## Defeat, Egress and Angel Wing: actual destination state

**Confirmed:** defeat first calls UpdateBattleUnlockedFlag: clear435 only
if535 is **already** held. It clears dialogue name1, requests sad music,
txt363 and close, restores Bowie0 HP=max, halves current gold with a
longword logical shift, calls GetEgressPositionForBattle and returns d4=-1.
It newly sets neither535 nor485. Battle4's losable upgrade/map branch is
not35. The subsequent common exploration healing is another operation.

At the admitted retreat seam, Egress deducts the actual selected spell's
MP cost before txt274. Angel Wing removes the selected inventory slot and
refreshes stats before txt275. Both request cast sound/flash, close text,
update unlock, unwind the action frame, select egress and clr.w d4.
They bypass terminal/death suffix, defeat's Bowie/gold recovery, after
wrapper, JoinForce and completion tail. Prior MP/item/flags/drop writes
remain reached state; common exploration healing can refill surviving
allies later if it returns. No resource/input completion is asserted.

`GetEgressPositionForBattle` (`$23E50..$23EB0`) has destination overrides
16/17/13/31/32 and unlock clears38/39, **no35 case**. Unchanged35 reads
EGRESS_MAP.b. `code/common/maps/egressinit.asm:GetSavepointForMap`
(`$75EC..$764E`) tests399, ordinarily set at BattleLoop entry. Supplied
clear399 returns initial3,56,3,Down before raft logic. Held399 scans the
complete23-row savepoint table; unmatched maps retain their map byte and
default1,1/Up. Selected examples:

| Actual EGRESS_MAP, with399 held | d0–d3 destination bytes |
| --- | --- |
|38 Roft|38 /21,27 /Right0. Distinct from the Map76 town warp.|
|31 Moun|31 /20,8 /Up1.|
|2 Pacalon|2 /41,10 /Up1.|
|36 Pacalon Castle|36 /40,5 /Up1.|
|75 or76|No savepoint row: same map /1,1 /Up1.|

Held64 also resets raft coordinates using the chosen destination's row;
all these examples use the default69/42,15 row. Existing church save code
can write CURRENT_MAP to EGRESS_MAP before SaveGame, but this research
does not establish a prior save, last visited town or actual registered
destination. Saving, visiting Roft and finishing35 are different facts.

## Map76 exploration and the independent Roft connection

**Confirmed:** `code/gameflow/mainloop.asm` returns from BattleLoop to
alt_MainLoopEntry: SwitchMap, then ExplorationLoop, with **no CheckBattle
or d4 branch** at that return. The complete switch table has only
3/F640→4,71/F530→78,58/F999→59; unchanged76 and38 are not substituted.

`explorationfunctions_2.asm:ExplorationLoop` first clears pending event,
subtracts20000 from STEP_COUNTER.w and branches BGE using signed N==V;
otherwise clears the word. This is source word/overflow behavior, not an
unbounded mathematical clamp. It heals living/immortal allies, fades,
then distinguishes explicit map byte from CURRENT255:

| Supplied return operand | Population, temporary flags and resource/init boundary |
| --- | --- |
|Unchanged35 victory d0=76, or ordinary explicit egress map|Fresh explicit entry even when destination equals current map. Write CURRENT_MAP and CURRENT_BATTLE255, load tilesets, wait fade, clear VInts, rebuild map entities/mapsprites, clear256..383 and set80. Initialize sprite frame counter, load map around physical0, install base VInts and run live setup init.|
|Changed/supplied d0=255|Retained path: wait fade, update main properties/raft; no fresh population/temp clear or current-battle reset in this branch. Then common LoadMap/base-VInts/live-init. This is not the ordinary unchanged35 victory.|
|Map76↔75/77 scroll edges|The predecessor's scroll path retains actors/temp state and skips this fresh initializer/battle scan. It is not the BattleLoop return path.|

Fresh76 therefore replaces battle entities with main/followers/raft. Reuse
the [complete Map76/38 population and resource owner](./nazca-to-roft-and-battle35-admission.md):
Map76 has no fixed setup entities, RTS init, no local item/description
service, layer254 and the declared tilesets/layout/animation. On this
overworld map F65 can prepend Caravan before ordinary followers; F76
can select Zynk independently of joined/active/HP. The original alias
overwrite and current missing65-prefix boundaries remain. An empty after
hook does not retain combatants or the opening's four scene Mud Men.
Fresh reconstruction differs from scroll/CURRENT reuse and can resample
flags/appearance/resources. No decoded map or completed resource load is proved.

After init, optional music/fade and event services must return. WaitForEvent
with no pending request selects view target0 and installs the controlled
entity script on physical0; PLAYER_TYPE was cleared at battle entry.
A pending request skips that installation. Reaching a control-script call
does not prove usable input, finished motion, display/audio or persistence.

Map76's complete default zone callback tests535. Clear535 sets435 and
posts the longword RELOADMAP; held535 returns without clearing an existing
435. The actual ordinary zone request's parameter overlap, stale facing,
X0/Y preservation and service cleanup remain with the predecessor. Only
a later no-scroll unwind reaches MainLoop's normal CheckBattle path.
Its complete45-row table has only one76 row:35, crop16,12/48×32,
triggerFF/FF. It requires435, writes crop and checks535; held535 clears435
but **still admits35 on this invocation**. A subsequent scan with435
clear does not. It never writes CURRENT_BATTLE itself.

| State after return / later request | Consequence |
| --- | --- |
|Victory completed,435 clear/535 held|Immediate exploration bypasses scan; later76 zone does not manufacture reload/unlock.|
|Defeat/retreat with535 clear|Update unlock leaves435 as supplied; later clear535 zone can set435/reload again. F485 can still skip the prior opening.|
|Defeat/retreat with535 held|Update unlock clears435; zone remains suppressed.|
|Changed435 held/535 held at a later explicit76/CURRENT scan|35 admits now and clears435; completion is not an early scan rejection.|
|535 cleared independently|Zone behavior can change; clearing535 alone does not set435 before a scan.|
|Direct return to38 from actual egress state|Fresh Roft entry, no38 battle-coordinate row; no Map76 traversal is implied.|

The complete three Map76 warps are8,47→no-scroll38,14,30,Up;
left edge0,255→scroll77,62,255,Left; right edge63,255→scroll75,1,255,Right.
The town row has no435/535/910/921/recruitment condition. Roft's outer
exit255,31 independently requests no-scroll76,8,48,Down. An explicit76
request on that normal MainLoop route can scan35 if435 is held; completing35
does not create a mandatory town entry or forbid entering before combat.
Natural access between markers and actual collision remain Unknown.

Both the warp seed38,14,30,Up and egress seed38,21,27,Right reach the
same fresh38 setup boundary, conditional on services. Reuse the existing
[Roft init branches](./nazca-to-roft-and-battle35-admission.md#independent-zynk-creation-and-greeting-branches):
921 held first sets911, then only joined26 clear/temp260 clear calls
cs_5DD8E and sets260 after return. Independently, clear910 calls cs_5DD9C
and sets910 then258 after return. Fresh entry cleared260/258 first;
retained CURRENT did not. Battle35's empty after/tail writes none of these
story flags. Neither incoming point guarantees it remains the final point
after init. Stop at those already owned entry programs, without redoing
the greeting, Petro scene, services, recruitment or Map77 interior.

## Selected text identity and ordered tokens

**Confirmed:** `data/scripting/text/gamescript.txt` supplies these complete
token orders. There is no35 after dialogue. The shared txt requests use
the ordinary dialogue path, not a selected fixed scene portrait/speaker.
Name/item/spell modifiers below are set at the named call sites; raw text
stays private and prose is authored paraphrase.

| ID / request / modifiers | Authored meaning | Complete ordered tokens |
| --- | --- | --- |
|363 /txt; name1 cleared0, LEADER rebuilds active list|Announces exhaustion before Bowie recovery.|`["{LEADER}","{W1}"]`|
|274 /txt; name1=actual retreat actor, name2=masked spell, number1|Announces retreat spell/level.|`["{NAME}","{N}","{SPELL}","{#}"]`|
|275 /txt; name1=actual actor, name2=masked used item|Announces retreat item use.|`["{NAME}","{N}","{ITEM}"]`|
|396 /displayMessage; target133, item40, number0|Announces the target's dropped item.|`["{NAME}","{N}","{ITEM}","{D1}"]`|
|395 /displayMessage; allied receiver, item40, number0|Announces the received item.|`["{NAME}","{N}","{ITEM}"]`|

396 precedes395 only on successful AddItem. Message construction and
requested waits/flash/close do not establish actual display, speaker
appearance, player acknowledgment or timing. LEADER363 can name the first
active member rather than the hardcoded Bowie0 recovered next. Full
inventories route to Deals without these two success requests. No town
dialogue is newly selected or reproduced here.

## Actual current consumers and missing Battle35 binding

**Confirmed about accepted code:** production consumes prepared Content,
not disassembly at runtime. [Capability status](../../remake/docs/capability-status.md)
owns runnable claims. These are direct static consumer checks; no SDK,
adapter, Godot or engine behavior run is supplied.

| Actual owner | Existing expression / selected gap |
| --- | --- |
|[Encounter preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Outcome hooks, joinMember, battleOrigin and configured outcomeEgressMap/savepoint have shapes. Pin admits1/2/3, rejects35; selected35 map/battle/party/terrain/material/outcome bindings are absent. A configured egress route is not the original live arbitrary EGRESS_MAP.|
|[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Private outcome-source requires EnemyLeaderPresent=false; defeated program must contain End/Return only.35's empty defeated hook satisfies only the latter, not its nonzero leader sweep. No35 outcome admission follows.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleDeathBatch](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Defeat priority, positioned living actors, defeated gate and correct dead-ally/enemy-kill ownership exist. Death batch admits distinct new positiveHP→0 changes and skips nondead/off-map entries; it does not reproduce arbitrary native lists or living-enemy leader cleanup. Actor positions replace raw30/32 slot/X/HP reads. None of these consumers executes the selected Heat Axe drop-bit/remove/add/Deals service.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Pre-scene return, completion skip, join→unlock→complete and defeat recovery have shapes. Active.First()/bound positioned actor, validated return anchor and return-status-refresh refusal differ from unchecked bytes/aliases and original status refresh. Egress refuses absent399 or held64/640 and uses configured route; no selected source retreat MP/item/unwind flow is established. Policy validation precedes publication, unlike arbitrary interrupted native prefixes.|
|[Story/return runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [map preparation](../../src/sf2tool/remake_exploration_maps.py)|Flags, SetEgressMap, ReturnBattleMap/Rebuild, init/zone/warp/population/resource shapes exist. SetEgressMap updates story.EgressMap, while the selected Sf2 return policy reads route.EgressMap. ReturnBattleMap calls Apply, which can check battle admission when NewBattle is supplied; original immediate alt_MainLoopEntry skips CheckBattle. This needs deliberate selected binding, not a claim of universal return parity. Actual entry/display/traversal/loaded setup guards and65prefix gap remain.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Board/entity/effect/text/audio/view services have consumers. No35 death/drop/return scene/resource/input lifecycle is bound or observed; existing visuals do not prove native timing or usable return.|

The predecessor's cameraSpeed native refusal, F88 scratch/queue overread,
65prefix omission,45,23 integer-geometry difference, retained checked word,
hidden reset/spawn/appearance, equipment/current-stat/upgrade and airborne
Unknowns remain. Battle01–03 mechanisms do not establish35 playability.
This research identifies the necessary content/policy boundaries without
authorizing implementation, a new comparison platform or a runtime queue.

## Reproduction, preserved failures and Unknowns

After loading the owning worktree's current ignored private selections in
the same process, choose an unused receipt and run:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1122/review.py <configured-source-disasm-root> --receipt local/issue1122/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored direct recipe checks pinned clean source/unchanged pair,
complete selected hooks/leader/positions and30-row drop table, target133/
actual item/flag/order/full inventory/Deals, corrected accounting,
count/heal/seed/join/completion/retreat/return widths and predicates,
savepoint/zone/warp/init joins, selected text tokens and actual consumers.
It checks two-path ownership, navigation-only predecessor, UTF-8/LF,
links, authored text/private boundary and committed cleanliness. Source
and this prose own the evidence; receipts/recipe are coordination state.
Apply ADR0004's applicable provenance/polarity/selector/order/width/state
checklist. New parser/schema/fixture/H3 obligations are N/A.
Use clean committed planner and actual exact-head scoped CI. Normal/full
Python, old comparisons, SDK/.NET/Godot/emulator/H3/H4/screenshots,
new export or fixture and tests of verification tools are excluded.

Preserve the named #1118/#1120 F88/65/cameraSpeed/equipment/upgrade/
airborne and geometry/checked/hidden boundaries. #1116's original e26968a9
author0.500s/CI39529528549 PASS missed collision polarity; accepted
c3f0c96a focused correction/CI39530664106 did not rerun its whole recipe.
[Dojo](./dojo-exploration-and-sheela-recruitment.md) retains Lemon28,
follow offset2=(−24,0), original missed checks and eas_5FFC4/5FFC8 /
compile_maps45 refusals. [Post-crash](./post-crash-ship-and-nazca-overworld-return.md)
retains complete-import @Return, crew/RNG/mapLoad/ReloadScene limits.
#1104's dead-ally defeats/enemy-FIRST_ALLY correction and all other named
predecessor failures remain with their owners. Twelve completed old
comparison failures remain discovery evidence, not interrupted runs or
an automatic rerun queue.

Freeze a clean committed/pushed Draft PR, exact base/head/tree/checks/
actual CI/failures/Unknowns and ignored handoff; move Project to
Review / Verify, notify main-gate and stop writer/process. Independent
acceptance/merge belongs to main-gate; no next Issue, merge, cleanup,
archive or private input publication follows.

**Unknown:** natural combat/retreat/town/zone reach; actual selectors,
party/list/HP/status/class/equipment/items/drop bits/Deals/gold/flags,
coordinates/facing/aliases/death lists/EGRESS_MAP/raft/step state;
upgraded definitions and airborne underflow, F88 scratch/restored queue;
decoded collision/terrain/material bindings; interruption/interleaving,
completed sprite/DMA/death/camera/flash/audio/text/fade/load/init/input
services, VInt/render timing, usable exploration and save/load persistence.
Full Roft interactions/recruitment, Map77 internals and further battles
remain excluded. No Unknown is a passed gate or an acquisition queue.
