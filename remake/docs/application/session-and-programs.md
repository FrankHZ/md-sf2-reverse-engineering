# Session and Resumable Programs

## Jaro victory program continuation

The [High-rigor agreement](../domain/gameplay-rules.md#authored-jaro-victory-recruitment) uses the
existing `GameSession`, `BattleOutcome` and `ProgramRunner`. Ordinary victory selects the return
anchor and heals eligible living actors before scene dispatch. The first-completion authored program
runs join23, visible acknowledgement and ten ticks, AI-bit clear, visible reply, leave23, then its
return. These are authored delivery waits, not an original music/jump/presentation timing claim.

`OutcomePartyOperations` admits only exploration with `VictoryProgramFinished`, an outcome route,
the source30/0/32/12 flag layout and a retained allied member in that encounter. Clear requires a
known activation word. Operations publish through the ordinary per-instruction commit and advance
the existing cursor; no second roster or broad workflow service is introduced. `JoinResources`
continues to preserve existing actor objects. Leave preserves the cached lists until common join0.
The tail clears432 then sets532 and executes the existing return program before field control.

Completed532 bypasses all recruitment instructions but retains that common tail. Retried selected
policy failures resume the failing cursor/continuation with prior joins, activation, leave and waits
committed. Stale revisions/tokens reject; unknown required state fails explicitly. No post-start
state injection is needed to run the package. Runtime raw combatant coordinates and physical aliases
are not retained by this outcome representation; source X-only removal is bounded by the rule owner.

## Poison after-turn continuation

The [finite High-rigor agreement and source provenance](../domain/gameplay-rules.md#authored-poison-after-turn)
cover standalone poison only. Normal action/scene publication and costs precede poison. The first
outcome pass skips poison for resolved victory/defeat or a dead/unplaced acting actor. Eligible
QueueOnly automatic standby also enters the existing publisher, retaining prior first-ally custody.

`BattlePoisonContinuation` waits on typed `PoisonDamageMessage` delivery before publishing clamped
HP. Survivors retain status2, complete after-turn once, then check outcome/consume one queue slot.
Lethal HP remains placed with status2 during `PoisonExhaustedMessage`; its completion starts the
existing field-death batch. Spin/exit/cleanup/settle preserve their order. The retained `AfterTurn`
return operand sends field-settle directly to the second outcome/count pass, never to action
publication, the defeated hook, rewards or another poison tick.

A later failure retains the accepted prefix. Missing death credit keeps HP0 and the exhaustion token;
accounting failure keeps the field-exit token; second outcome failure keeps `PoisonFinalize` after
nonlethal HP publication or the existing field-settle stage after cleanup. A matching completion
retries only that stage. Stale/wrong/duplicate completions reject without mutation. Sleep keeps its
existing continuation and RNG behavior.

A poison-enabled standalone encounter supplies the existing leader/enemy outcome binding. With no
exploration world, `BattleOutcome.Begin` records `ActiveBattle.Outcome` and `BattleEnded`; terminal
commands reject and the host displays Victory/Defeat. The same boundary handles an already-dead
leader at controlled start. This is a modern standalone terminal result, not an original return,
chapter progression or save/load implementation. Connected/private return programs are unchanged.

## Battle discard selection and completion

`BeginDiscard` is valid at ActionChoice and enters DiscardItemChoice. `SelectDiscardItem(slot)`
selects an exact live packed word, including duplicate identities in separate slots. Protected items
remain visible and Confirm returns typed `discard-unsellable` without changing the snapshot.
Confirm on a permitted item enters DiscardCommitReady; final Confirm revalidates actor, word,
metadata and required custody before publishing. Wrong-stage, empty, stale and wrong-actor inputs
return typed failures. Cancel from either Drop selection stage returns ActionChoice for free.
Selection and cancellation retain provisional movement and authoritative resources/turn/RNG.

`BattleDiscardContinuation` publishes the finite loadout/equipment/Deals change once and retains
ActionChoice plus its existing movement preview behind `BattleDiscardState`. Its authored text and
WaitToken require actual CompleteTextReveal followed by Acknowledge. Pending input, duplicate/stale
delivery and acknowledgement cannot repeat mutation or select the next compacted item. The wait
blocks battle control; acknowledgement releases the same actor's same action. GameSession bypasses
outcome/defeated hooks for this free interaction. There is no battle scene, STAY, movement commit,
after-turn or next-turn dispatch. Outer Cancel later returns to movement while retaining accepted
inventory/equipment/Deals, and an ordinary subsequent action uses the usual commit/turn lifecycle.
The [domain owner](../domain/gameplay-rules.md#authored-battle-discard) records source order and limits.

## Battle Give selection and completion

`BeginGive` enters `GiveItemChoice` from `ActionChoice`; `SelectGiveItem(slot)` changes only
selection. Confirm enters `GiveTargetChoice`, where `SelectTarget(actor)` chooses an adjacent
admitted teammate. Confirm selects normal `GiveCommitReady` or, for a full recipient,
`GiveExchangeChoice`. `SelectGiveReturn(slot)` selects the exact return word; Confirm reaches
`GiveCommitReady`. Final Confirm revalidates live actor/destination, both selected words and capacity.
Unknown targets return a typed rejection before lookup. Session/revision/actor envelope checks apply.

Cancel backs up one selection stage: confirmation to recipient/return item, return item to
recipient, recipient to donor item, donor item to actions. It preserves authoritative positions,
loadouts, resources, turn and RNG. Existing movement delivery may change facing/visuals; the move
remains provisional until the accepted action. Use/equip/transfer offers are separate.

`BattleGiveContinuation.Begin` publishes both loadouts, refreshed ATT and committed movement once.
Its `BattleGiveState` carries an authored result and `WaitToken`; no battle scene or rewards are
created. Actual `CompleteTextReveal` precedes `Acknowledge`. Acknowledgement resumes the existing
no-scene STAY suffix (`BattleActionCommitter` and `BattleAdvancer`), consuming the giver's queue
entry once. Stale/duplicate waits reject; a later suffix failure preserves the published transfer
and stops that continuation. Ordinary subsequent rounds retain their own RNG behavior.

The [Give owner](../domain/gameplay-rules.md#authored-battle-give-and-exchange) owns source order,
admission and limits. This is an authored standalone interaction; no original result dialogue,
hardware timing, field/service inventory or private persistence claim is made.

## Battle equipment selection

`BeginEquipment` is admitted at `ActionChoice`; `SelectEquipment(slot)` is admitted only within
`EquipmentChoice`. Both publish selection only. The query exposes eligible slot labels, current
equipped markers and prospective ATT/range without constructing an action or mutating battle state.
Initial focus is the currently equipped slot, Unarmed where offered, or the first weapon when four
eligible slots exclude Unarmed. A zero-weapon list refuses entry.

Cancel returns to the same action menu and movement preview with authoritative equipment/stats
unchanged. Confirm revalidates the current actor, status, class/slot eligibility, selected packed
word and live stats, then publishes equipment and derived ATT together exactly once. The existing
session envelope rejects wrong session, stale revision, duplicate submission and wrong actor.
Confirmation returns to `ActionChoice`, not the action committer/scene/turn advancer. A later
movement/action Cancel preserves confirmed equipment; it never restores a starting loadout.

This is a standalone authored weapon interaction. It introduces no item custody transfer, payment,
RNG, turn consumption or connected/private persistence. See the
[domain owner](../domain/gameplay-rules.md#authored-battle-weapon-equipment) and
[content boundary](../content/profiles-and-trust.md#authored-weapon-admission).

## Ordered AURA recovery

AURA uses ordinary spell/target/confirm commands and existing healing preparation and scene owners.
`ValidateHealing` requires the exact ordered eligible recipient list, valid deferred recoveries,
unchanged unrelated state and a valid construction RNG chain. Selection/cancellation, unavailable
options and stale commands do not publish resources or RNG.

Construction publishes movement and one award RNG computation with ordered contribution facts;
persistent HP/MP/EXP remain unchanged. One ActionMessage completion pays MP. SpellCost and one
ActionAnimation precede the first recovery. Each TargetEnter completion applies its recovery and
emits `healing-recovery`, including zero amounts. ResultMessage advances to the next exit/entry
without repeating cast/payment. The last result proceeds through idle/stop/restoration to one credit,
reward text and live-seed growth. End releases the existing after-turn/next-control path. Self
recovery preserves paid MP. Later credit/growth refusal retains the committed prefix, token and
queue; retry cannot replay earlier recipients/payment.

Authored AURA has no `HealingSceneCursor` fairy work. Finite actual delivery owns its waits;
the [view](../godot/battle-scenes.md#authored-aura-recovery) makes no original opportunity/RNG parity
claim. HEAL keeps its existing fairy continuation; physical/Herb/Prism retain their existing paths.
The [rule owner](../domain/gameplay-rules.md#authored-aura-group-healing) owns source provenance.

## Caravan boarding, travel and landing

`CaravanDefinition` supplies explicit authored bindings; `ExplorationState.TravelMode` and
`CurrentEntities` belong to the same live world as its physical actors and logical aliases.
`CaravanPrograms` constructs finite typed story/entity programs for numeric events 2/4.
`MapEventDispatcher` selects the walking producer or mode1 controller; no presenter toggles mode.
Walking reaches a 0x3800 marker only after source obstruction and shrunk-bound checks, requires F65
in exploration, and requests event 2 before raw blocking without moving or decrementing StepCounter.
The vehicle uses unshrunk bounds, current physical positions only, and no walking slope/zone/step tail.
It travels on 0x3800 even with raw blocking bits, requests event 4 on other admitted words below 0xC000,
and otherwise blocks. Both controllers compare the single admitted, loaded area without reselecting
it after scripted movement or raw physical 1 cloning outside its bounds; only the attempted direction
is bounded. Unsupported door/warp/copy markers and map replacement fail explicitly.

**Confirmed static provenance:** SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/scripting/map/vehiclefunctions.asm:MapEventType2/cs_45284/eas_452BA/byte_45316/MapEventType4/cs_45348/eas_4536C`,
`code/common/scripting/entity/entityscriptengine_2.asm:esc02/esc07/esc09/esc0D/esc0E/loc_55C8/esc03`,
`entityfunctions_3.asm:ApplyActscriptToFollowers/WaitForPartyEntitiesIdle`,
`code/common/scripting/map/followersfunctions_2.asm:InitializeFollowerActscripts`,
`data/scripting/entity/followers.asm` and `eas_main.asm`; see the
[accepted Caravan research](../../../docs/research/caravan-boarding-and-southwest-parmecia-travel.md).
These static sources do not establish natural original reach, timing or persistence.

Logical FOLLOWER_A, raw physical 0/1 and the current list suffix are separate operands. Facing-relative
motion reads current coordinates, bypasses collision and supports the source table's diagonal entries
created by scripted motion. Clone copies position/destination/travel/facing, not velocity, sprite,
resources or membership. Raw 0x6000 parking does not enlarge MapPosition's legal layout domain.
Resize uses per-actor flags and shared SpriteSize; each refresh retains its sampled RenderSize for
actual presentation. Warp-sound requests retain order; the authored package supplies no original audio.

Both transitions reuse script-idle, physical destination, sprite-ready and view waits. The final
`PhysicalEntityWait` waits only the last current-list suffix actor, with no invented join-all.
Empty suffixes return immediately. Missing logical bindings, zero speeds and undelivered sprites
retain earlier writes and current mode. External admission validates every current-list slot and
forbids population replacement; reached slot checks do not silently remove or repair entries.

Event 4 first clones raw 0 into the mapped vehicle, restores the admitted live player's appearance
using its current HP and fixed class binding, then grows/moves the player and current suffix from
current facing. Mode1 remains authoritative through those waits. Reinstall scans current source
flag rows, chooses normal versus F65/overworld pointer table and assigns scripts to consecutive slots
by selected-row ordinal. It does not consult or rebuild the current list. Pointer assignment preserves
motion; settings execute at the next entity service. Overworld scripts 2/3 retarget while moving;
ordinary scripts wait for destination. More than five selected rows or missing required slots are
explicitly Unsupported before reinstall; the already grown scene and mode1 remain committed.
Only successful reinstall commits mode0. Landing does not itself run a zone or decrement steps.

The source timer operands and modern sprite/camera completion are deterministic delivery boundaries,
not original frame durations. The public host pauses ticks and completions while hidden/unfocused.
`CaravanTests` exercises changed state/content, pending/stale completion, source list/lookup
counterexamples, current/destination obstruction and later ordinary walking through one session.
No saved vehicle placement, acquisition flow, generic vehicle platform or second publisher exists.

## Battle-context region programs

Physical position is updated at initial placement and acknowledged movement delivery, never by
blanket snapshot synchronization. Conversion's intermediate logical placement preserves the old
ally physical slot even when the later alias copy fails. Decision/action transaction invariants
preserve the visual state; selected rules cannot replace or drop its mappings.

The finite round order is activation → first eligible ordered region row → played flag → shared
ProgramRunner → spawn/reset/appearance → queue. Wrapper admission tests only region and played flag;
it adds no HP, joined-membership or conversion-completion guard. Both matching rows choose the first.
`BattleRoundState.Program` retains the round stage while `ActiveBattle` and the existing `StoryState`
cursor, callers and typed wait remain authoritative. It never creates an exploration world.

Standalone authored battle programs/texts live in `BattleStoryDefinition`; they use the same typed
instruction decoder, ProgramRunner and completion dispatcher as field programs. Battle-safe operations
are ordinary flag/branch/jump/call/return/end, text cursor/simple dialogue/close, ticks, semantic camera
target/CameraWait, facing, `ConvertBattleActor` and `JoinBattleForceAi`. Field-only operations reject
at the reached PC. Source map-script epilogues, entity-service calls, rich field text/media and other
presentation cues are not silently admitted. Story updates survive BattleAdvancer, selection commands
and GameSession publication; no outer incoming-story restore can erase the program's writes.

`BattleStoryOperations` commits conversion and JoinForce steps in source order. A late invalid alias
returns the last committed snapshot with unchanged conversion PC and the played flag held. Such an
Unsupported session remains stopped; it is not reset/retried from the wrapper. Wrong/stale completions
at a valid wait reject without advancing PC, RNG, conversion or queue. Actual matching acknowledgement
or presentation completion resumes once, including nested calls. The existing spawn phase runs only
after the program returns; queue generation sees the resulting live actors. A later battle-action
failure retains the ordinary committed movement/scene prefix.

`BattleVisualState` holds explicit logical-to-physical aliases and retained physical appearance,
coordinates/facing for the finite authored scene. Camera/text resolves that alias even for a dead
ally; facing skips the lookup for HP0 allies. Logical combat identity and resources stay with the two
actors. Godot projects the bound appearance and ordinary movement from the session, never deciding
faction/control from a sprite. The separate [authored victory suffix](#jaro-victory-program-continuation) is implemented. Full original physical entity services and private Battle32 victory recruitment
are separate unsupported consumers. See the [rule owner](../domain/gameplay-rules.md#jaro-conversion-and-source-ally-commandset6).

## Round prefix and appearance continuation

`BattleRoundContinuation` uses the existing `GameSession` publisher. Activation and the optional battle-context
program stage precede the ordered whole-list reset. One immutable `BattleRoundState` then holds
successful actor IDs, appearance cursor and completion token; live actors remain solely in
`EngineBattleState`. Both successful and refused reset facts are observable. No future battle or
parallel region store is retained.

Each actual `EntityEffect` completion advances one appearance. Wrong/stale/duplicate completions
reject without resetting actors, consuming RNG or generating turns. The complete list is loaded before
the first appearance, but Godot hides later pending actors. Only after the last appearance does
`GenerateQueue` consume live placement/HP/agility and main RNG. A later decision failure keeps the
already delivered appearance prefix and generated queue; it cannot replay reset. Story and item/death
custody survive the continuation. Outcome selection waits for appearance delivery and otherwise keeps
its existing precedence, including termination before a new round.

Authored encounters with initialization retain validated immutable encounter starts. The existing
`BattleEntry` overlays live party allies and custody on fresh enemy defaults; `BattleOutcome` carries
initialized state with ally vitals/loadout. Fresh mode2 may therefore appear again after an encounter
return/retry, without being eligible for same-encounter reappearance after an ordinary death. The
[capability agreement](../capability-status.md#hidden-enemy-lifecycle-agreement) owns acceptance and exclusions.

## Prism laser continuation

`BattleAutomaticAction.ChosenAction` carries the selected finite laser through the existing automatic
movement consumer. Ordinary target decisions retain their physical default; a laser focus is not
substituted for its live hit list. The laser rule rebuilds both factions at construction and retains
an immutable `PrismLaserShot` with origin, direction, old focus and source-ordered targets.

The existing `BattleSceneState` owns the candidate and token. Empty laser scenes expose no reaction
target or displayed ally, still pass Initialize → ActionMessage → ActionAnimation → End, then
resume the ordinary action continuation. Nonempty scenes announce/animate once and enter each
ordered target/reaction/result/death sequence without announcing another shot. HP/status and reaction
RNG publish only at the later reaction boundary. Godot completes the finite visual; no observation
or probe can advance it by claiming success.

`BattleDeathCredit` is one immutable accounting context in the live battle, retained through
no-scene turns. Scene initialization installs a known-none laser context or ordinary first ally.
The field continuation captures that context for its ordered death batch. Unknown context remains
an error; known-none permits enemy removal without kill credit, including a leader sweep. Ally
defeats remain required. Cleanup finishes before defeat-before-victory/outcome selection. A later
failure preserves already accepted HP/RNG, removals and token boundaries; retries do not reconstruct
the shot. Tests cover real automatic input/control, empty scenes, ordered friendly fire, sweep and
failure recovery. See the [rule/source accounting join](../domain/gameplay-rules.md#finite-prism-laser).


## Battle item custody and receipt playback

`BattleItemCustody` is one immutable value containing separate drop-once bits and per-item Deals
counts. The active `EngineBattleState` owns it in battle; the active exploration `Party` owns it in
field mode. `BattleOutcome` transfers it alongside live ally loadouts, progress, accounting and RNG;
`BattleEntry` consumes that party value when constructing a new battle. No StoryState flags, presenter
store, pending scene or encounter default becomes a competing live authority. Null means unknown,
not an empty store, and drop-enabled battle admission rejects it.

All existing party reconstruction paths retain the value: recovery/defeat/retreat, field RNG,
portrait/text polling, resource restoration, join and encounter entry. The return-policy boundary
rejects a policy that replaces it. Drop-enabled authored encounters retain immutable controlled
enemy starts. Reentry reconstructs those enemies but overlays the party's live ally inventory; a
fresh enemy may hold its initial item again while the retained once bit prevents another receipt.

Physical construction commits inventory/once/Deals exactly once with the accepted action. Each
reaction may retain a successful `BattleItemReceipt` for later display. After its death message,
`ItemDroppedMessage` then `ItemReceivedMessage` use the ordinary scene token/acknowledgement path,
before EXP/growth/gold playback. They do not mutate inventory. A rejected or stale completion cannot
replay construction; later growth/outcome failure preserves already committed inventory and stock.
Deals/discard fallback has no new player-facing message or wait. `item-to-deals`/`item-discarded`
observations and the active-mode `itemCustody` projection expose committed facts to inspection.

The [native recipe](../development-and-verification.md#battle-item-drop-authored-observations)
observes both variants through real waits and a second ordinary kill. Its authored retry trigger is
controlled content, not a claim about the original Battle18 encounter-entry/after-program route.

## Ordered leader cleanup and retained accounting

`BattleActionCommitter` owns the first post-action checkpoint for completed scenes and no-scene
Stay, sleeping, dead and automatic-standby turns in cleanup-enabled encounters. It evaluates the
existing defeated hook, combines ordinary deaths with the finite source-ordered survivor sweep,
and starts `BattleFieldDeathContinuation` before asking the outcome policy. A held completion
flag records the skipped empty program while still admitting the sweep.

`EngineBattleState.DeathCredit` starts unknown and is assigned by each real admitted physical,
HEAL or item scene; `FirstAlly` projects its optional ally. Laser assigns known-none as described above.
No-scene turns preserve the context; they do not substitute the current actor.
A reached nonempty cleanup without a known context fails `death-first-ally-context` before cleanup
or outcome publication. Controlled starts cannot invent prior source scene state. Unsupported
ranged enemy/spell callers remain outside this extension; the finite laser has its own admitted path.

The extracted continuation retains only caller/destination/first-ally operands and the immutable
ordered worklist. It has no synthetic `BattleActionResolution` and no second actor snapshot.
All listed actors pass through twelve spin deliveries, three exit deliveries and one settle
wait. Accounting/removal is one finite publication at the last exit. Stale/duplicate commands
reject against the live token. If accounting fails, the accepted earlier presentation remains;
if outcome selection fails after accounting, the settle token and cleaned state remain, so retry
cannot repeat kills or rewards. `GameSession` remains the sole snapshot publisher.

After settle the ordinary publisher performs defeat-first outcome selection, otherwise after-turn
work and queue consumption. Sleep completion does not repeat the defeated hook at its second
checkpoint; a later individual turn has its own first checkpoint. The existing outcome/after/
defeat/return programs carry party accounting back to usable exploration. See the
[rule/source owner](../domain/gameplay-rules.md#finite-enemy-leader-cleanup) and
[native recipe](../development-and-verification.md#leader-cleanup-authored-observation).

## Ineffective physical scene continuation

Taros physical construction uses the existing `BattleActionResolution` transaction and single
`GameSession` publisher. `Ineffective` is a finite reaction kind with amount0 and unchanged HP/MP;
validation rejects damage for a blocked initial strike, an unbound ineffective result, an
ineffective counter, inappropriate status writes or a Taros-origin counter. It does not execute
observation strings or accept arbitrary actor patches.

Construction publishes its carried RNG/movement and prepared reactions. `ineffective` is withheld
from construction observations and emitted once at actual reaction entry. `BattleSceneRules` adds
no damage recoil/RNG for this kind. The current result message/acknowledgement and subsequent
sleep, reward and end continuations retain their normal tokens; the supported sleep operand is
applied only at its later message completion. End EXP credit remains a distinct commit.

Queries/cancel do not prepare or spend. Stale/duplicate delivery cannot repeat the reaction or
release another wait. A later credit failure retains committed construction/movement, prior
feedback and the pending cursor/token; retry does not reroll or repeat feedback. The
[rule owner](../domain/gameplay-rules.md#finite-taros-physical-effectiveness) explains source ordering;
[TarosEffectivenessTests](../../tests/Sf2.Remake.Engine.Tests/TarosEffectivenessTests.cs) exercises
these real command and failure boundaries.

## Paid retreat and field return

`BattleActionKind.Retreat` and `SessionAction.Retreat` keep a learned retreat spell in the ordinary
semantic spell/target query. `GameSession.Submit` preserves envelope rejection and selects one finite
`BattleRetreatContinuation` while `ActiveBattle.Retreat` is present. No field program or ordinary
attack scene impersonates that pending action. `HasBattleControl` is false throughout delivery.

Confirmation revalidates the live action/self target/MP and publishes movement plus payment once.
`retreat-mp-paid` records the actual before/after MP before `retreat-text-requested`. The continuation
requires `CompleteTextReveal`, then acknowledgement, then matching effect and close completions.
Each service after text has its own token. Wrong/stale/duplicate commands and tokenless simulation
cannot pay or release a later wait. Return failure preserves the paid battle, Return phase/token and
any already-applied repeat unlock update. Ordinary outcome rules cannot manufacture a retreat exit.

Angel Wing stays `BattleActionKind.Item` / `SessionAction.Item` through the normal carried-slot query
and self-target selection. Selection retains the packed word; confirmation rejects a changed live
word, invalid actor/control or target before preparation. `ItemRetreatPayment` carries the selected
slot/word and admitted item identity/name; `SpellRetreatPayment` carries the learned spell/cost.
The validated Wing preparation removes only that slot, shifts whole surviving words and appends127.
Publishing that prepared state and `retreat-item-paid` precedes `retreat-text-requested` in the same
immutable transition. It does not charge MP, require EGRESS, construct a healing scene, award EXP or
consume RNG. The common continuation owns all later waits/return; failure retains the paid inventory.
Field healing and fresh battle entry carry the compacted loadout rather than recreating initial items.

After closure, held completion clears its corresponding unlock; clear completion preserves unlock.
Neither path newly sets completion, runs the defeated/after-turn suffix, performs a victory join,
revives the defeated leader or halves gold. `Sf2OutcomeReturnPolicy.Start(Retreat)` reads live
`StoryState.EgressMap`, or the explicit controlled initial route selection if no writer has run.
It resolves only admitted outcome savepoint bindings. Unknown maps and F399-clear or F64/F640-held
source branches fail explicitly. The selected private binding is Map3 `(32,13)`, Up; this is not a
universal fixed-map rule. Authored tests select two different live admitted anchors.

The caller then heals eligible living/immortal allies through the existing outcome recovery rule,
builds the field projection, and runs the admitted `ReturnProgram` under `OutcomeMapLoaded`.
The ordinary map load/setup/fade must finish before usable field control. Dead ordinary allies stay
dead; members 7/28 retain the existing immortal exception. No-status selected input is supported;
unimplemented status refresh remains an explicit failure. Later HP/MP recovery does not refund or
remove the earlier paid observation. Walking reentry uses `BattleEntry` and its fresh enemy/queue
initialization, living-member recovery and carried party/loadout/growth/accounting/RNG.

**Confirmed static join:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`battlefunctions_1.asm:ExecuteBattleaction_Egress/UpdateBattleUnlockedFlag`,
`executeindividualturn.asm`, `battleloop/getegresspositionforbattle.asm`,
`common/maps/egressinit.asm`, `data/maps/global/savepointmapcoords.asm` and
`exploration/explorationfunctions_2.asm:ExplorationLoop` establish payment, bypass, return selection
and caller healing-before-fade. `battleloop/heallivingandimmortalallies.asm` separates ordinary dead
members from 7/28 and refreshes after resource recovery. The accepted
[admission/payment owner](../../../docs/research/battle00-bonus-battle-and-reset.md#outcomes-escape-and-the-actual-reset-return)
and [supplied-map caller owner](../../../docs/research/battle18-outcomes-and-caravan-acquisition.md#victory-map12-control-or-warningretreat)
retain original evidence and Unknowns. Reproduce the bounded source inspection with `rg -n` on those
symbols in the pinned local checkout, and the modern behavior with the
[EGRESS recipe](../verification/field-programs.md#battle02-egress-and-walking-retry).

**Confirmed static Wing join:** at the same pin, `battlefunctions_2.asm` stores the selected item and
slot, calls `IsItemUsableByCombatant`, builds the item range and confirms a target before dispatch.
`common/stats/itemstats.asm:IsItemUsableByCombatant` admits non-equipment consumables without an
equippable-class restriction. `RemoveItemBySlot` reads that slot; `RemoveAndArrangeItems` shifts
subsequent complete words and writes `ITEM_NOTHING` at the tail. The wrapper also requests stats
refresh; the admitted consumable has no equip effects and this slice preserves existing gear/stat
authority. `data/stats/items/itemdefs.asm` item4 is Angel Wing, CONSUMABLE, EGRESS, range0 and no equip
effects. `ExecuteBattleaction_AngelWing` removes the slot before text275 (`{NAME}` / `{ITEM}`) and
branches to the shared exit tail. This proves static order, not natural acquisition or original
runtime delivery. The ordinary Battle02 caller remains distinct from the linked Battle0 caller.
Reproduce with `rg -n -A 48 'ExecuteBattleaction_AngelWing:'` in the pinned battlefunctions source,
and `rg -n -A 60 'RemoveAndArrangeItems:|IsItemUsableByCombatant:'` in itemstats. The
[Wing native recipe](../verification/field-programs.md#battle02-angel-wing-and-walking-retry) proves
the modern selected-item path only; all-map/status and original media/timing remain Unsupported/Unknown.

## Yeel guide, zone request and sleep continuation

The selected early Yeel program preserves texts784–787, source portraits and synchronous caller
return. `ShowText.Narration` defaults false and permits only a null explicit speaker without
`UseEventSpeaker`; reader and runtime reject conflicting selectors. The producer lowers only the
explicit close-text/close-portrait/copy-and-clear-speech sequence. Narration keeps normal W1/W2 and
caller ownership; it does not bypass missing portrait checks for ordinary speakers. `{CLEAR}` resets
the current window cursor/layout with one logical VInt, keeping the window and following real input.
The next portrait instruction restores Kazin. F72 is written after the offer returns; the existing
source population/follower path supplies one follower on reentry without adding F4 or battle members.

A map has an ordered `Battles` collection. The old optional `battle` document is the singleton form;
`battles` is the multiple-route form, with both simultaneously rejected. Entry scans eligible routes
in source order, preserving completed-repeat policy and one-time unlock consumption. Map66's ordered
zone dispatch remains a separate service: matching rows can suppress its default without an encounter.
`SourceRandomBattle` requests an incomplete encounter without RNG or step tests; a completed encounter
uses the nullable signed `StoryState.StepCounter`. Unknown repeat state rejects; nonzero defers; zero
uses range8, then range4+2 only on failure. Positive counters decrement once at successful field-move
launch, never during preview, blocked moves or observation.

A request sets its unlock and30000 counter, then runs the existing typed WaitView/Sound/FlashWhite
program and returns through the actual caller. Only return to the zone boundary consumes the pending
request and performs the ordered encounter scan. This in-place selection invents no position/facing
and imposes no battle-rectangle proximity predicate. The reached `SetEgressMap` instruction writes the
shared story owner; mode/automatic/scene continuations preserve it. The selected Battle03 intro gate
controls the Yeel assignment and F453; its empty start remains a real program boundary.

Physical sleep has two separate publication stages. Constructed damage/status operands remain in
`BattleActionResolution`; damage playback publishes its original status, and the following real
SleepMessage completion publishes the infliction. Construction and counter eligibility never read an
unplayed reaction as live status. Lethal damage omits infliction; sleeping targets omit dodge/counter.
At a sleeping queued entry, `BattleAdvancer` skips command selection and starts a typed after-turn
notice without fabricating an attack, reward or selection. `BattleAfterTurnContinuation` commits the
single main draw before text306/354, retains actor.Status while pending, then applies its operand and
consumes the queue entry on the matching actual completion. Dead entries bypass this work. Wrong or
stale commands cannot consume it twice. Other status families remain Unsupported. The Godot consumer
uses finite modern no-input notices over the field and the same completion protocol; it does not
freeze an attack/hurt animation or own logical status.

See [capability agreement](../capability-status.md#yeel-guide-through-battle03-implementation-agreement)
and [direct observation](../verification/field-programs.md#yeel-guide-through-battle03).

## Battle02 outcomes and live facing

The selected private world admits Battle02's empty after-program and the shared `JoinForce(0)` tail.
Victory saves the first active member by membership index, adds the battle-area origin to its final
combatant coordinates, and retains its current logical facing. The producer supplies `battleOrigin`
from the pinned battle-map table (Battle02 `(16,19)`). This explicit binding selects live source
coordinates/facing; existing authored and retained inputs without it keep their declared map-space
position/fixed-facing policy. The projection uses the same coordinate conversion before scripts run.
Victory clears F402 and sets F502, returns directly to field control and does not request Battle03.
An already completed battle skips only its after-program, retaining the join/flag tail.

Ordinary defeat restores the leader, halves current gold by integer division and refreshes living
members; dead ordinary allies remain dead. It clears the unlock only when completion already exists.
The selected controlled Egress map is Map3, yielding savepoint `(32,13)`, Up. This is an explicit
caller input, consistent with the initial witch-start assignment; neither Battle02's empty hooks nor
the selected intervening route writes Yeel. Church/Egress changes outside this selection remain
unsupported. Walking through the church door `(32,15)` and Granseal west exit invokes ordinary entry:
party inventory, growth, EXP, kills/defeats, money and RNG are carried, while enemies come from the
encounter defaults. Retry reuses the same `GameSession` and source living-member initialization.

Actor facing belongs to immutable battle state. Completed movement segments, including cancel-return
segments, update only the moving actor. Stationary selection preserves it. No-action completion sets
only the actor Down; actual scene completion reloads ordinary placed entities facing Down before
field-death/outcome processing. Transient movement/death drawing overrides remain presentation data.
The host projects facing and never supplies it back from sprite metadata. Action/automatic/progression
guards preserve facing unless the owning lifecycle boundary changes it. Herb consumption also keeps
`Progress.SourceLoadout` synchronized with the verified consumed inventory while preserving all other
growth fields; it does not relax non-item or other-actor invariants.

These rules consume pinned `c834c652b6862bc5679fd7f69a38a7093206efc6` source:
`battleloop_2.asm:BattleLoop_Victory/BattleLoop_Defeat`,
`battlefunctions_1.asm:UpdateBattleUnlockedFlag`,
`textfunctions_1.asm:UpdateForceAndGetFirstBattlePartyMemberIndex`,
`executeindividualturn.asm:@NoAction/@InitBattlescene`,
`loadBattle.asm:LoadBattle`, and `entityfunctions_1.asm:PositionBattleEntities`.
`witchstart.asm` assigns `GAMESTART_MAP` to `EGRESS_MAP`; `getegresspositionforbattle.asm` and
`egressinit.asm:GetSavepointForMap` consume the selected map. The accepted
[Battle02 outcome owner](../../../docs/research/battle02-to-battle03-content-route.md) retains the
original evidence and Unknowns; remake observations establish implementation behavior only.

Map66's source `data/maps/entries/map66/2-areas.asm` uses layer 254 with unit parallax and zero origins/
autoscroll. `mapload.asm` loads this byte unchanged; `camerafunctions.asm` selects Plane A for nonzero
values. The field view now admits 254 under the same main-plane constraints as 255. Other unknown layer
values and unsupported parallax/autoscroll remain rejected. Native acceptance reads the actual bound
camera/player projection after victory and field movement; no screenshot or hardware-timing claim
is made. See the [focused recipe](../verification/field-programs.md#battle02-outcomes-and-retry).

Application owns command admission, continuation and snapshot publication across exploration,
battle, scene work and return. [Domain rules](../domain/gameplay-rules.md) supply selected algorithms;
[Content](../content/profiles-and-trust.md) supplies admitted immutable definitions and explicit starts.

| Work | Read |
| --- | --- |
| State publication and failures | [Session authority](#session-authority) |
| Typed content and conditional authoring | [Definitions](#definitions-and-execution), [authoring](#authoring-conditional-programs), [source policy](#source-story-policy) |
| Dialogue and field input | [Portrait/plain wait](#portrait-lifecycle-and-plain-current-input-wait), [W1](#w1-in-a-suppressed-entity-event), [bound text](#bound-field-text-work), [yes/no](#source-bound-yesno-lifecycle) |
| Field services | [Camera](#source-bound-camera), [nod](#source-bound-nod), [music](#modern-finite-music), [raw text](#raw-field-display) |
| Connected source programs | [Opening](#original-map-3-opening), [castle/tower](#castle-palace-astral-and-tower), [battle entry](#battle01-admission-and-first-input), [outcome/return](#battle01-outcome-after-program-and-return) |
| Original limits | [Remaining source boundaries](#remaining-source-boundaries) |

## Session Authority

`Runtime.GameSession.Start` accepts either an `IScenarioSource` or admitted definition/start pair.
It reads external content once and binds `SessionRules` before creating live actors. One `Submit`
checks the command envelope and dispatches the explicit `ActiveBattle` / `ActiveExploration` mode.
`GameSession` alone assigns Current. `ExplorationDispatcher`, `ProgramRunner`, `EntityActionRunner`,
`MapTransfer`, `BattleAdvancer` and scene/movement/outcome continuations compute the next result.

`SessionSnapshot` carries the active state and shared `StoryState`; it does not store a reference
trajectory or scenario-specific receipt chain. Mode-specific access is explicit. Keep immutable
observations and preserve unrelated state when transitioning. A failed preparation retains the
previous state; later failure retains already published effects, current cursor/callers and waits.
`SessionFailure` distinguishes IllegalCommand, UnsupportedCapability, ContentError, InvariantFailure
and AdapterError. Selected-policy failures include identity/operation and source-story PC context.

The [field/program verification owner](../verification/field-programs.md) holds preparation and
actual-input/state recipes. Native projection belongs to [Godot](../godot/presentation.md), not to
these execution rules.

## Current capability

The ordinary `GameSession` executes exploration, resumable story programs and map transfer, then
hands the same session to the existing battle flow. `harbor-arrival.json` and `hill-passage.json`
exercise different maps, actor identities, dialogue, choices and motion through this path. Accepting
runs entity actions, a call/return and a timer, transfers maps, executes map init and both intro
hooks, initializes the encounter and reaches first player control. Declining returns field input.
Neither package identity nor an expected route/receipt admits a command.

Exploration and return consume the same [logical input/settings owner](../verification/host.md#logical-input-and-accessibility-adr-0010-9a) as battle. Confirm talks/acknowledges/accepts and Cancel declines. Text reveal stays in the adapter; W1 delivery reports its completion separately from player acknowledgement, including automatic completion of a trailing span. Reduced-flash suppresses the reached white overlay while completing its real token/kind through the existing presentation service, an intentional 9A deviation.

`GameSession` remains the sole snapshot publisher. `ExplorationDispatcher`, `ProgramRunner`,
`EntityActionRunner`, `SceneEntities`, `MapTransfer`, `BattleEntry` and `BattleOutcome` compute immutable results. The active payload is either
`ActiveExploration` or `ActiveBattle`; story flags, program PC/call stack, typed wait, text window,
simulation tick and observations survive the switch. The battle view attaches to the existing
session. It cannot restart it at the transfer endpoint. Startup and transfer use the same derived
`HasBattleControl` boundary: an active battle with a pending program or wait keeps the program view
until its dialogue, choice, timer or presentation service finishes, then exposes ordinary battle
control.

Before battle entry, `ExplorationState.Party` owns the explicit candidate resources, seeds and
accounting. Joined/active membership is separately owned by story flags and source-order counted
lists; follower links refer to physical map slots. `JoinForce` refreshes counted lists before its
active-flag write, so the immediately exposed list can lag that final write. This is intentional
source chronology. The opening input supplies the first three R1 allies' class, resources,
equipment and spells. Other initial ally appearances and names come from pinned source data;
this group does not claim a full travelling roster, mutable names, promotion/death appearance,
general field inventory operations or later battles. At Battle01 admission the sequencer rebuilds active membership
from live flags, so the earlier `JoinForce` list timing never forces a missing ally into battle.

## Definitions and execution

Format-v7 battle packages remain current. Format-v8 exploration packages contain `package`, an
embedded format-v7 `battle` definition/start, `world` and `start`. World data contains maps, ordered
events, programs and text; start data contains the selected map/player, position/facing/speed and
flags. An optional start `program` invokes instruction zero of a whole program in a fresh session.
It does not import a PC inside that program, a call stack, an active wait or any executed history.

The Content reader links all call/jump/branch targets, including untaken branches, and checks map
references, duplicate IDs, scalar domains and terminal fallthrough. Entity references that depend
on runtime population are checked when reached. Typed operations support flags, calls/returns,
conditional branches, text cursors, continued/single text, explicit close, yes/no, fixed-point
entity motion, facing/sprite/position/visibility, tick waits, presentation requests and map transfers.
Known unimplemented operations retain a source-attributed stop instruction.

A blocking instruction keeps its PC until its own wait completes. A stale token/revision cannot
release it. Continued text remains visible after acknowledgement until close or replacement;
legacy single text closes on acknowledgement. Source-produced text uses explicit window operations
described below. Speaker flag bytes remain available for speech/display, separately from the
persistent portrait gate. Real ticks advance background actions at legacy open-text consumers,
and increment `SimulationTick` only when executed. The admitted plain input-first consumer instead
requires explicit player Wait; delivery/reveal supplies none. A tick batch stops at a newly reached
wait boundary. The host batches elapsed 60 Hz ticks and subtracts only ticks the engine executed.
Unused time remains available for automatic continuation on the next frame; reaching paused input
clears the accumulator. Lower frame rates therefore retain elapsed ticks without draining a newly
reached dialogue or choosing an answer automatically. Existing field input does not truncate a
background action batch; completing a foreground wait still yields at the newly reached input or
program boundary before further ticks are submitted.

### Authoring conditional programs

Use the existing public v8 envelope and typed operations. Edit `world.programs[].instructions`,
link targets with `{program,instruction}`, and select the entry through an ordinary map event or
whole-program start. The reader admits all targets, including untaken branches, before creating a
session. No C# implementation name or package-name branch selects the executed instructions.
Content is read once: restart after a JSON edit; compiled rule edits also require the
[rule author workflow](../domain/gameplay-rules.md#rule-author-workflow-and-diagnostics).

[Story A](../../content/authored/story-rule-demo-a.json) and
[story B](../../content/authored/story-rule-demo-b.json) use the same quay, player, ferryman interaction,
marker and ordinary battle definition. A's `invitation` branches when live flag40 is set to
`repeat`; its initial fallthrough calls `first-a` and sets40. B reverses the condition: when40
is clear it branches to `first-visit-b`, calls `first-b` and sets40; its later fallthrough calls
`again-b` and clears40. A's repeat calls `again-a` and also clears40. The branch condition, targets,
called programs and effects are external data; both outcomes run through the same runner.
Each called program calls `pause-a` or `pause-b`, leaving two real return addresses on the stack.
The pause displays its package's text, waits for real acknowledgement, waits30 logical services,
then returns to the called program's later effects and finally to field input.

| Content/branch | Before dialogue/timer | After nested pause returns |
| --- | --- | --- |
| A, flag40 clear | Set41, hide marker at(4,3) | Set42, position(4,2), show marker, return and set40 |
| A, flag40 set | Set43, show marker at(4,2) | Set44, position(3,2), hide marker, return and clear40 |
| B, flag40 clear | Set51, hide marker at(4,3) | Set52, position(3,4), show marker, return and set40 |
| B, flag40 set | Set53, show marker at(3,4) | Set54, position(4,4), hide marker, return and clear40 |

For the compiled demonstration choose `RuleCompositions.AuthoredStory()` before startup. The two
files run unchanged through the same runner and composition, with real Confirm inputs followed by
West movement. Source-only compatibility is unsupported under that composition; ordinary programs
do not invoke it. The host's committed default remains SF2, which also executes ordinary authored
programs. [Verification](../verification/field-programs.md#replaceable-story-observation) gives the
short observer and exact recipe. This is project-authored replacement evidence, not original-game
reach, frame timing, private presentation completeness or a no-compile algorithm platform.

While `ShowText` or `WaitProgramTicks` is pending, its PC remains at that blocking instruction;
no later flag/entity effect executes. The engine releases the exact token, advances that PC and
returns each nested caller in order. Unsupported reached instructions or selected source-policy
errors retain prior effects, cursor and callers; malformed references reject before start.
Wrong, stale and duplicate completions cannot skip the wait or replay a later effect.

### Portrait lifecycle and plain current-input wait

`StoryState.PortraitWindow` is persistent Unknown, Closed or Open (portrait identity and packed
flags). `open-portrait` takes an entity and flags: an existing Open or Unknown gate is retained;
a null entity represents a skipped lookup and also preserves the gate. From Closed, the resolved
entity's current sprite metadata supplies the portrait, or a known absent portrait keeps Closed.
Missing metadata becomes Unknown. Only executed `close-portrait` establishes Closed. Text-only
close, raw text and JOIN preserve it; calls, returns and branches carry it. The Godot portrait
projection consumes this state independently of the text window and retains an opened identity
even if later text has another speaker. Close is an atomic engine lifecycle operation; it does
not claim original window-movement timing or its VInt work.

The real producer emits `explicitWindows: true` on `show-text`; its acknowledgement never closes
either window implicitly. `nextSingleText` lowers to portrait lookup, DisplayText acknowledgement,
portrait close, text close and mandatory Sleep(10). `nextText` has no close tail. Packed FFFF skips
the portrait lookup without closing an existing portrait. Raw `txt` only displays text; raw `clsTxt`
only closes text. `closeTxt` closes portrait then text. JOIN's emitted SoundWait → PreviousMusic →
`wait-text-input` → text close → Sleep(10) retains the incoming portrait gate. Bound execution stops
at SoundWait without a finite profile; see [modern finite music](#modern-finite-music). These rules are
implemented by the existing compiler/typed reader, with no selected-script eligibility exception.
Fresh output is required to gain these distinctions; old generated content cannot reconstruct them.

**Confirmed source:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`mapscriptengine_2.asm:csc00_displaySingleTextbox`, `csc02_displayTextbox`,
`csc08_joinForce`, `csc09_hideDialogueAndPortraitWindows`; `mapscriptengine_1.asm:csc1D_showPortrait`;
`trap5_textbox.asm`, `portraitwindow.asm:ClosePortraitWindow` and
`mapsetupsfunctions_1.asm:DisplayCurrentPortrait`. A source-wide cutscene text-skip mode is not
implemented; unsupported native operations remain stops. The FFFF lookup skip is distinct from it.

Initial/restored portrait provenance is Unknown. An initial source open cannot resolve a possibly
existing window, so it stays Unknown until a real close executes; no asset absence proves Closed.
Legacy `show-text` without explicit window operations retains its old display hint inside Unknown
state and its old single-text auto-close policy. That hint never admits the plain consumer. Thus
legacy content remains readable but cannot claim the new source lifecycle or natural initial portrait.

`WaitForTextInput` records the input-first consumer and its current program's established
`EntitiesRunning` setting. `WaitForText(token)` is admitted only in exploration presentation wait,
with an open text window, proven Closed portrait, field continuation, no pending battle entry,
no busy player and no pending entity sprite readiness. Calls may remain on the stack. One Wait
executes the existing entity service once when enabled, retaining physical-slot order and shared
RNG rules; a disabled service produces no entity draw. `Acknowledge` returns immediately with no
accepting-poll tick, then executes subsequent explicit operations. Generic `AdvanceSimulation`
is rejected at this admitted consumer. The [host contract and reproduction](../verification/field-programs.md#plain-text-input-gameplay-wait)
cover reveal, delivery and input rearm.

The [natural JOIN observation](../../../docs/research/map3-messenger-acceptance.md#natural-join-audio-and-input-boundary)
confirms its named helper entry/return. **Unknown:** joint logical audio/service progress, the complete
enabled VInt table and portrait counter chronology. The current program's entity setting is
the admitted engine service state, not a sampled original service table. W2, active/unknown portrait
consumers and preceding audio delivery retain their separate legacy boundaries. The bounded W1
consumer below does not establish whole-host Option A, 9A or H4 conformance.

### W1 in a suppressed entity event

The reader retains raw text for compatibility and an ordered token stream for execution: literal
spans, `{N}`, `{LEADER}`, `{NAME;n}` and each `{W1}` occurrence. Names are substituted as literal
data; their contents are never parsed again as controls. Any unsupported control, malformed brace
or unavailable name excludes the entire text from this capability. W2 cannot become W1.

`ShowText` admits this stream only with explicit source windows, a live `EntityEventContext`,
field continuation without battle entry, an established Closed portrait, resolved event-actor
sprite metadata explicitly declaring no portrait, and `EntitiesRunning=false` on the executing
program. No map, text, actor, seed or expected endpoint identity participates in admission. Without explicit bound field-text settings, other
consumers retain legacy display/acknowledgement and cannot claim this W1 contract.

`W1TextWait` keeps the displayed text ID, exclusive token endpoint, delivery state and whether
the endpoint is a W1 or the final trailing span. Every optional `WaitForText` and the accepting
`Acknowledge` executes the same ordered preamble: main-seed draw256, source copy-byte write, one
logical suppressed-service wait, then input decision. Observations expose those four stages.
The copy commit writes the result into bits31..24 of the party's `ThinkingSeed` image, retaining
its other24 bits. That image is the live authority carried into battle, updated by AI and carried
back through battle outcome. `StoryState.RandomSeedCopy` is only the last text-write diagnostic:
it is nullable until an admitted text write and survives ordinary copies, including later AI
changes. `SessionSnapshot.CurrentRandomSeedCopy` always derives the current byte from the active
party/battle image; a declared modern start supplies a value even before any text write. This does
not confirm the original game's initial byte. The host reports this live byte as `randomSeedCopy`
and the historical text write separately as `lastTextSeedCopy`. Entity actions/motion/followers and portrait
RNG receive no service in this admitted wait. The source wrapper's existing pre-facing service
still happens before admission; ordinary return/control and other programs keep their own rules.

`CompleteTextReveal` is an actual host-delivery receipt, consumes no tick/RNG, and cannot accept
a W1. Wait and Ack reject incomplete delivery. Each accepted W1 creates a fresh token for the
next span; the host retains already shown characters and reveals only up to that next endpoint.
The full projection remains mounted for at least one host frame before the delivery receipt.
A trailing span then completes automatically, with no invented manual Ack or logical tick.
Generic elapsed `AdvanceSimulation` is rejected for all W1 delivery/input states. Confirm can
reveal text without polling; accepting W1 does not play generic SFX67. Plain JOIN and choice
semantics remain with their separate owners.

**Confirmed (source):** the [reached W1 contract](../../../docs/design/contracts/dialogue-system.md#reached-w1-consumer-binding)
owns `textfunctions_1.asm:symbol_wait1/loc_659C/loc_65B4` and the distinct W2 tail at pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. The bounded ordinary caller is
`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` → `map03/mapsetups/s2_entityevents.asm:Map3_EntityEvent2`,
F602 clear → raw text483. Its actor128 uses sprite195/WOMAN with `PORTRAIT_NONE` in
`spritedialogproperties.asm`; the wrapper faces before suppressing entities, then closes/reactivates
after return. `WaitForVInt` includes its enable/handshake. Closed portrait excludes blink/mouth
service. Quake consumes RNG only for nonzero `QUAKE_AMPLITUDE`: the intro clears it and csc33 is
the source setter; this reached Map3 path contains no setter. The later retained prepared-68
`FFA80C=0` supports that ancestry, not a direct text483 service sample. No quake capability is added.

**Confirmed (remake):** ordinary physical facing/Interact after the existing three-step opening
setup reaches this caller and the required gates, then 0/1/3 optional Waits plus actual Ack give
1/2/4 polls. [Verification and limits](../verification/field-programs.md#w1-entity-event-input)
record the actual settings comparison and preserved failures. **Unknown:** original opening
W2/typewrite/entity timing, general portrait/service tables and whole-route 9A/H4 remain open.

Movement uses the existing `OriginalMapTraversal` area, collision and stair rules. The extracted
entity core uses 384 fixed units per tile, source signed-word arithmetic, acceleration/deceleration,
destination obstruction, facing/animation and arrival layer/immersed changes. Each entity's movement
runs before its action dispatch. Nonwaiting configuration actions execute in the same tick. Source
`ac_moveRel` installs a relative destination and redispatches immediately; shorthand `moveRight` and
its siblings additionally wait for arrival. Timed reversal can therefore replace an unfinished
destination, as in the palace's entity action streams. Replacing an action stream retains current physical motion. Source `setPos`
changes position, destination and facing without resetting unrelated speed/flags/action state.
`SPRITE_SIZE` is global. A later failing action preserves completed configuration and movement in
that tick and retains the failing action cursor. No destination assignment replaces a motion path.

Source motion installation has an optional `installation` field: `Preserve` (the authored
default and native MakeEntityWalk), `SlotTimer` (custom/named cutscene scripts), or
`SlotTimerClearCollision` (entityActions sequences). The latter two use the resolved physical
slot as the wait timer; only the last also clears flagsA bits5/6. Existing source aliases
determine the slot at runtime. No timer value is baked into compiled content.

A true source jump-to-idle compiles to `jump` plus terminal `idle`; entityActions gets its
implicit source idle tail. Inline bytes after an unconditional idle jump are consumed through
the delimiter without becoming executable. Plain authored Stop/exhaustion or Unsupported
does not imply idle. The terminal stays in the existing Actions/cursor, services wait1/branch
once per existing slot service, and contributes no active-script Busy. Direct installation
preserves its timer; the preceding source jump clears it before idle runs in that same service.
Source cutscene motion waits require script idle, so a following caller may run while physical
travel remains; ordinary field movement and authored default waits retain their physical/busy
completion rule. No active infinite idle loop or second state authority is involved.

The [idle/caller source owner](../../../docs/research/map3-controlled-start-egress-transition.md#source-idle-completion-and-caller-installation)
records the Zone6 leading-wait consumer and the boundaries. Ordinary player control still
supersedes its old action/follower continuation and establishes controlled motion settings after
movement at the next existing service. Follower installation clears Actions; source phase
startup still binds real walking wait cursors. General high-bit source wait fidelity and the
unimplemented waitIdle/native producers remain outside this bounded correction.

Source `MainLoop` selects an unlocked, uncompleted battle before `ExplorationLoop` initializes its map.
The before-battle program retains the previous field scene until its own load/entity calls.
Authored map-init routing remains explicit in its definitions. Entry runs
**before program → region/party/enemy initialization → battle load → start program → first round**.
Both hooks check the same intro flag; the start wrapper sets it. The internal `BeforeBattleRouted`
policy is produced only after Application completes that routing. External standalone starts still
require the existing explicit controlled skip. Battle region state and its activation rules retain
their existing Domain owner; general story access to battle-region flag aliases is outside the
selected private programs described below.

## Bound field text work

The explicit `start.textSettings` profile (`messageSpeed`, `mouthControl`, `viewSpeed`) enables
source text work for field programs. It requires a known Closed portrait with an explicitly
portrait-less speaker, or the registered entity/zone caller portrait described below, regular font
metadata, and a supported logical view. Names,
text/actor IDs, seeds, routes and receipt counts never select this capability. A start without
this profile retains its existing plain/W1/legacy consumer. An unsupported bound context stops;
it cannot silently use display latency as gameplay work. Battle continuation, unadmitted caller portraits,
scene camera entity changes, cursor targets, scrolling overrides, autoscroll and non-unity parallax
are outside this profile. Quake and pulsating fade variants have no admitted implementation.

`world.textFont` contains the existing source reader's 256 ASCII-to-symbol entries and 80 glyph
advances. The private producer reuses `build_variable_width_font_contract`: it checks the registered
USA ROM, existing H1 symbol listing, split-font bytes, font pointer and ASCII table parity. The
split font is a private binary, not a pinned Git blob. Only widths/mapping enter Content; no bitmap,
comparison fixture or runtime reference dependency is added. Area `view` metadata carries bounds,
foreground/background offsets, per-plane parallax/autoscroll and layer. The selected profile is
layer0, zero background offset, unity parallax and zero autoscroll. Authored width/name/area changes
execute through the same rules. Source IDs7C/7D bypass typewriting, but cannot occur in the admitted
normal symbol range1–80; other control/font families remain unsupported.

`FieldTextWait` separates the ordered glyph/control cursor, mandatory phase and actual delivery.
It preserves the current span and the first-regular-glyph bit across W1/W2 occurrences. DisplayText
resets that bit (source DIALOGUE_REGULAR_TILE_TOGGLE) and selects regular font1, while a reused
window retains X/Y/row. Names are substituted as literal glyphs, never reparsed as controls. LEADER uses the first active
member after rebuilding the source party flags; authored content without those flags names member0.
A fresh window performs two clear/DMA opportunities, creates the source29×8 window at(2,29),
and services its eight-step move to(2,19). Every glyph applies the first-glyph/automatic newline
rule, advances X by its symbol width, performs one cursor/DMA opportunity and then the source
speed0/1/2/3 delay of4/2/1/0 opportunities. X>204 wraps before the next glyph; a newline adds16
and Y>=48 performs two row-scroll waits plus a final wait before subtracting16. Row offset wraps
modulo6 for this bound event/black-bar style. Close services eight movement steps and the final
moving-bit clear observation. Reopening starts a fresh layout.

Neutral mandatory work supplies logical input0. Source nonzero input shortens the extra glyph
delay only when mouth control is0; the engine tests that rule, but exposes no new shortening
player action. Reveal-only Confirm and actual delivery neither supply this input nor consume
work. They cannot skip outstanding work; logical completion without delivery also cannot poll.
Each optional Wait and accepting Ack draws main RNG256, copies its byte, updates the W2 indicator
if applicable, services once, then decides input. Enabled NPC draws may change the main image
in that service; they cannot overwrite the copy. W2's20-step indicator is visible at counter>=7,
is hidden by view scrolling, and acceptance hides it and requests validation67. W1 adds no67.
Plain JOIN remains input-first with no accepting-poll preamble.

The shared field service executes the existing entity reducer before logical view/scroll/window
work and increments one simulation opportunity. Suppressed entity events still service view and
window work. Their wrapper restores facing first, retains context and the live entity-service flag
through portrait and `TextCloseWait`, then closes logical/projected windows together before clearing context and
returning control. The no-window and unbound legacy paths keep their own lifecycle.

`wait-view` represents the source helper before nextText/nextSingleText. It checks active axes,
services until settled, services and rechecks (that service can start scrolling), then performs
the final service. It is not a fixed two-tick delay. `LogicalView` carries both plane positions,
active-axis destinations/speeds, target slot and follow counter. Inactive destinations remain
absent/don't-care. LoadMap clamps/quantizes origins in source order. View data reads the target
**after** entity service, uses strict1536/2304 deadbands and area bounds, retargets by384, then
scrolls each active axis and clears it on completion. Speed is24 or32 when the signed follow
counter exceeds6; active scrolling preserves its speeds. Godot interpolation supplies no readiness.

**Confirmed (source):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, beneath
`disasm/`: `code/common/scripting/text/textfunctions_1.asm` (DisplayText, ApplyAutomaticNewline,
@line, symbol_wait1, @wait2/sub_64A8); `textfunctions_2.asm` (CreateDialogueWindow,
HandleDialogueTypewriting, HandleBlinkingDialogueCursor, sub_6AD2/sub_6AE0, CloseDialogueWindow);
`code/common/windows/windowengine.asm` (VInt_UpdateWindows, WaitForWindowMovementEnd);
`code/common/maps/camerafunctions.asm` (VInt_UpdateViewData, WaitForViewScrollEnd),
`animations.asm` (VInt_UpdateScrollingData); and `code/common/scripting/map/mapsetupsfunctions_1.asm`
(RunMapSetupEntityEvent/loc_476A8–loc_476D6). The base VInt order is map planes, entities, view,
scrolling, sprites, windows, map animations. Only the admitted gameplay effects are modeled;
no additional interrupt RNG or CPU-time padding is invented.

The [binding evidence](../../../docs/research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding)
separates source facts, later saved bytes and inferred opening ancestry. The
[verification owner](../verification/field-programs.md#bound-opening-field-text-observation)
records actual settings comparisons and unresolved boundaries. Pending #517 speech policy remains;
this consumer retains existing actual speech projection and grants no policy waiver.

### Bound entity-event portrait

The bound wrapper opens the live actor's portrait before its enabled facing service, then
suppresses entity service before the handler. Known absent portraits retain the closed path;
missing metadata, Unknown state and an unregistered open portrait stop admission. A supported
existing open portrait retains its identity, flags and counters. No actor, text, flag or route
identity selects this capability.

`OpenPortraitWindow.Work` owns movement, registration, blink and mouth state. A fresh open starts
blink20/mouth6, moves from source Y=-10 to1 in four steps and observes the final moving-bit clear
before registering. Close removes the service first, performs the reverse move and deletes the
window. Both movement requests deliver existing SFX65. Packed right/mirror flags and ROM eye/mouth
tile mappings select the existing64×64 raster; the host crops/mirrors the composed face and
publishes actual tile/layout metadata. Raster/audio extraction is independent of this metadata.

After the admitted entity/view/window services, the registered portrait decrements blink;
at3 it selects alternate eyes, at0 normal eyes and main RNG120+30. Typewriting independently
gates mouth decrement; at5 it selects alternate mouth, at0 normal mouth and main RNG5+10.
DisplayText preserves incoming typewriting throughout fresh dialogue clear/open work and sets it
only after CreateDialogueWindow returns, before token processing. A reused window returns without
creation work. Empty/W-only text sets then clears it without exposing a typing service opportunity.
When typewriting is clear, mouth<=5 resets/draws immediately, otherwise it holds. Blink draws
precede mouth draws. W tokens clear typewriting and acceptance restores it for subsequent glyphs;
mouth-control shortening remains separate. NPC and portrait draws after a poll never replace
that poll's copied byte. Delivery frames supply no portrait opportunities or RNG.

`call.activateEntities` expresses Trap6's live activation, which survives its return.
`end-map-script` distinguishes the source script end from native RTS: an open dialogue window
requires the existing view helper before returning; then view override clears. A closed window
returns without that wait. The wrapper restores facing, closes portrait then dialogue and
finally returns control with entities enabled. A no-script suppressed event remains suppressed
during its close; a Trap6 caller retains activation through its close.

**Confirmed (static source):** the pinned revision above, `code/common/menus/portraitwindow.asm`,
`portraitfunctions.asm:VInt_PerformPortraitBlinking/UpdatePortrait/LoadPortrait`,
`code/common/tech/interrupts/trap6_mapscript.asm`, `trap9_contextualfunctions.asm`, `vint.asm`,
and `code/common/scripting/map/mapscriptengine_2.asm:loc_47234` support this bounded service order.
The wrapper binds `mapsetupsfunctions_1.asm:loc_4765E/loc_476A8/loc_476C4`.
Trap9 appends the portrait after the installed base services; no global interrupt scheduler is
introduced. The [research binding](../../../docs/research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
and [native observation](../verification/field-programs.md#bound-portrait-entity-event-observation)
retain provenance and the actual caller-return boundary. The source-zone section below admits the first introduction; later camera/JOIN/battle consumers
and original hardware/DMA timing remain outside this admission.

### Source zone caller

Source-produced `source-zone` events carry validated init actions compiled from `eas_Init`.
Authored `step` events retain arrival-before-program behavior. For a source zone, the marker
request follows entity obstruction and precedes map passability. The complete producing entity
pass runs before installation of the init stream and immediate handler entry. Physical travel,
its timer and destination survive; subsequent enabled services execute init and the existing
sprite handshake.

`StoryState.EventCaller` is the sole live caller authority: `EntityEventContext` and
`ZoneEventContext` select their own entry/return rules. `EntityEvent` is an entity-specific
projection. A zone uses the existing registered portrait/text/view reducers and the carried
live entity-service flag, regardless of native program defaults. It does not impersonate an
entity interaction. Its return removes/closes portrait, closes dialogue, then owns one mandatory
`ZoneArrivalWait` opportunity followed by physical-coordinate equality checks. Control stays
with the caller until arrival; script-idle/Busy are not the return condition.

The current actual admission is the first Astral introduction at(58,13), including affected
opening Zone6 and Sarah from the original bound start. No route, text, speaker, flag value or
seed selects this runtime mechanism. The [source binding](../../../docs/research/map3-messenger-acceptance.md#first-introduction-source-zone-caller)
and [actual observation](../verification/field-programs.md#source-zone-caller-observation)
record its limits. Later text500/501/F602, the second zone branch, camera, choice, JOIN and battle
consumers remain separate. This extends no original runtime or distribution claim.

## Original Map 3 opening

The named `map3-opening-start.json` begins at R1's controlled Map3 initialization seam, at (56,3)
facing down with only Bowie joined/active. It is not natural title/menu reach. The ordinary host
consumes canonical layouts, ordered setup/entity/event tables, pinned native wrappers and complete
map-script bodies. No expected endpoint, route ID or reference receipt participates in execution.
The separately read R2 controller trace exercises house exit, both doors, Sarah's classroom
interaction, stairs, entity142, Astral's zone and the messenger trigger. The complete messenger
acceptance runs through its motion, text, choice, two nods, camera waits, join music/fade, membership,
follower installation and guard positions; the zone wrapper then sets F603 and returns field input.

**Confirmed:** common-session comparisons match the accepted R1 position/membership/start RNG,
R2 program order and route positions, and R2a text IDs/packed speaker operands, flags, guards,
follower links and (43,10)/down endpoint. Ordinary Godot input observations reach the same endpoint
and move down then back up through a follower-occupied tile. A second input run declines, verifies
that join/follower flags stay clear, then talks to Sarah and accepts. That alternate behavior is
supported by pinned source and reproduced in the remake; it is not an additional original H3 claim.
An independent live-flag start enables another follower and moves logical entity142 from physical
slot17 to18. Logical identity and source record identity do not depend on that slot number.

Population allocation preserves physical processing order, enabled source followers, source allies
that reuse those slots, and non-ally logical aliases. Source `eas_Init` configurations execute and
wait for a matching sprite mount. Source walking templates are lowered from their actual operations;
random movement uses the existing main RNG. Followers measure current separation, rotate source
offsets around the leader's destination, and apply terrain fallbacks. Player input checks the source
obstructable bit and both current/reserved positions; scripted motion keeps its distinct destination
check. Source populations start with all 64 identity entries mapped to physical slot0, then overlay
allocated ally/non-ally references. Numeric selectors use the source low-byte/signed encoding;
32–63 share entries with128–159. These are references to `AllEntities`, never duplicate entity state.
Hiding removes every reference to the hidden slot and its follower behavior while retaining the
physical record. Missing source-table keys represent FF tombstones and survive state copies and
same-map preservation; only rebuilding population initializes fresh zero entries. Out-of-table
selectors and references outside the 49 normal records stop as Unsupported. Authored maps without
source population keep strict named-ID resolution. The accepted
[lookup contract](../../../docs/design/contracts/map-exploration.md) owns the source boundary.

Ordered door copies run before traversal; flag copies run at rebuild; roof activation/restoration
uses the shared layout reducers and signed area overlays. Source map255 same-map reload preserves
entities and the working layout, updates player position/facing, performs roof-on-load and reruns
selected initialization. The original setup selector still refuses an unimplemented alternative
instead of publishing default entities. This group uses Map3's default setup; broader initialization,
healing/temp reset, dynamically promoted/dead allies and other setups are outside its start boundary.

## Castle, palace, Astral and tower

The same live opening session continues through Map3's castle gate, Map19, the first Map20 palace
visit, royal return, Astral's invitation and the west/middle tower. Preparation imports the complete
source bodies `cs_51652`, `cs_53104`, `cs_53996`, `cs_52F0C`, `cs_52F40` and `cs_53EF4`, including
their native event/init callers and entity action streams. All six complete in the connected group.

**Confirmed:** the common-session group comparison starts at R1 and uses the accepted
[castle H2 graph](../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json) only as test navigation
and expected facts. The engine reads actual source programs. Gate guards move out and back before
the caller sets F604. Palace execution returns Bowie to (23,39), minister131 to (20,39), removes
Astral130's aliases and then lets the caller set F605. A repeat visit runs the source repeat init
without replaying the palace. Astral refusal sets caller F607; accepting a later prompt moves him
to (63,63), sets F608 and releases the passage. With F608 set and F256 clear, Map21's actual guard
interaction moves128 to (6,16), resolves unassigned135 to the existing player slot0, faces it down
and waits for the mounted sprite service. Only then does the script set F401 and return to the caller
which sets F256. Repeating the interaction displays579 without another move. Ordinary input reaches
(5,15) and remains available. Original natural reach and cadence remain **Unknown**;
the H2 graph is static evidence, not an H3 timeline.

Actual map rebuilds select/populate using incoming flags, clear source temporary flags256–383,
set F80, apply layout flags and run the selected initializer. Same-map preservation keeps temporary
flags. Map20's native entry branch compares exact signed fixed-point X/Y against $2280/$3780;
other entrances do not run the royal scene. Physical slot allocation uses the live follower flags,
while membership remains a separate source flag range. The group tests cover first/repeat visits,
initial/revealed/departed Astral, zero/two/three followers, initial acceptance and refusal followed
by an actual Sarah re-prompt. All four F608/F256 guard branches are compared with each follower count,
including dialogue-only branches that never set F401 or move the guard. A direct castle approach with F600 clear still reaches the explicitly
unimplemented `cs_51454`/`moveNextToPlayer` branch; the tests do not manufacture F600.

Map19/20 reuse their registered shared atlas; Map21 uses its own registered atlas. `setPriority`
persists on the physical entity. The modern renderer draws priority entities after ordinary
entities, retaining its existing Y/slot ordering within a group. Palace `fadeInB` performs a black
to full-brightness transition over 0.5 seconds and completes only after the actual view updates.
These are bounded presentation mappings, not original VDP ordering, palette cadence or pixel parity.
Source `setFacing` requests a new sprite generation on its resolved physical slot and retains its
program PC until `EntitySpriteReady` returns for that slot/generation. Authored `face` can omit this
service. Actual ordinary-host observations at60 and30 FPS cover the connected group, repeat visits,
the player-facing wait, guard repeat interaction and stable field input without state injection.

### Bound map-initialization lifecycle

Bound text/view settings now admit ordinary map initialization with its real `MapLoaded`
continuation, no event caller, and a live cursor. Explicit portraits use the same movement,
registration, blink/mouth service and close lifecycle as field callers. Bound text retains its
window/view service and enables only eligible text Wait/Ack after logical work and delivery;
`CanWaitAtInput` still requires complete field return. A nested `end-map-script` waits for the
view when its text remains open, then resumes its actual caller. Only the outer init completion
restores field following. No map/program/text ID selects production admission.

D1 is a typed neutral22-service pause with saved/cleared/restored typewriting; nonzero mouth
control skips its waits. It preserves first-glyph/token order and uses the same live entity/window/
portrait services, with no poll/copy or fake acknowledgement. Held logical-input shortening remains
unadmitted; reveal/Ack cannot supply it. Other unbound controls remain Unsupported. Explicit
sourceFF/null/no-event-speaker narration is admitted without replacing a retained portrait or
inferring its absence; actual speech uses the null speaker and remains silent. Explicit closes
still own removal. [Dialogue provenance/limits](../../../docs/design/contracts/dialogue-system.md#bound-neutral-text-delay-and-narration).

The map-init camera profile includes source layer255 with zero foreground/background offsets,
unity parallax and zero autoscroll. It follows A rather than B, retaining identical initialized
A/B axes through destination/scroll/window service. The existing renderer uses that same origin
without a duplicate foreground. The field profile also admits main-plane unity/origin0 and secondary
per-axis128/256 parallax, offsets0–63 and zero autoscroll within nonnegative signed-word positions.
Initialization quantizes before scaling/offset; destination and speed transforms preserve independent
axes. Ordinary actors subtract the area main plane; entity Layer still controls window priority.
Foreground presence includes parallax differences. Other view profiles, outcome windows and camera-entity retain their explicit limits.
Before-battle scene/windows use the separate bounded lifecycle below.
[Source contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle).

Actual engine cases cover nested camera and open/closed text returns, portrait registration/close,
readiness and unsupported contexts, first/repeat flag branches and a distinct authored foreground
follow state. **Confirmed, bounded modern host:** instant and adjustable/reveal delivery execute the same
opening/JOIN, guard out/back/F604, Map19 init, full first palace/F605 and Map19 field return.
Source255 A/B origins and actual tile/actor projections agree through init/scroll; sourceFF narration
requests no speech. Matching semantic readbacks preserve the accepted JOIN prefix and gameplay
across delivery. The bounded first palace route is described in
[verification](../verification/field-programs.md#bound-map-initialization-observation). Original
natural timing and full modern Battle01/H4 acceptance remain **Unknown**.
**Confirmed, bounded modern host:** the retained opening/JOIN and first-palace checkpoint continues
through Astral refusal/re-prompt/acceptance, tower guard sprite completion before F401/F256,
repeat interaction and Map21 readiness to Map40(14,13). The existing navigation omits the final
Up into the marked battle exit. Actual independent A/B tile origins and main-plane actor geometry,
resources, clipping and signed window priority are read back throughout changing plane separation.
Instant and adjustable/reveal settings retain identical post-JOIN logical transitions and carried RNG.
This field endpoint does not admit scene-map, camera-entity or before-battle windows, and does
not close executable H4 or original natural-cadence acceptance.

## Presentation and private Content

Content validates private provenance, embedded raster identities/shapes, required map/sprite links
and portrait references before session creation. Preparation verifies the registered USA ROM and
pinned source/asset manifest, reuses the existing map atlas, and decodes required sprite directions
and portraits through maintained decoders. The generated world and all game text/raster bytes remain
ignored private output. No ROM, extracted asset, dialogue prose or private absolute path is tracked.

Godot draws the working block layout and source sprite frames, mounts requested entity sprites,
shows source portraits and resolves `{LEADER}`/`{NAME;n}` from configured source names.
The bound camera consumes logical view state as described below; unbound cues retain their
presentation-driven arrival wait.

### Source-bound camera

With explicit field settings, `SetCameraTarget` validates the admitted field context and installs
four destinations in `LogicalView`; nullable `TargetSlot` represents source FF/no-follow.
Positions and follow counter survive target changes. An equal active axis remains active until
scroll service; an equal inactive axis stays inactive. `CameraWait` uses the existing `ViewWait`
clear/recheck/final-service lifecycle. It needs no asset acknowledgement, wall-clock duration or
second camera state. Field Wait/Ack/movement and `CompletePresentation` cannot release it.

`ExplorationViewRunner` prepares24/32 speed from the inherited signed counter every no-follow
service, scrolls each axis independently and evaluates window hiding before scrolling. Existing
entity→view/window→portrait service carries the live entity flag, shared RNG and poll-copy byte.
Dialogue and nested returns retain the held camera. Actual ordinary field-control return restores
the player's physical slot without clearing pending scrolling, positions or counter. The
[source contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle)
and [static provenance](../../../docs/research/map3-messenger-acceptance.md#source-bound-camera-lifecycle)
own these rules and their source limits.

All bound draws use B logical positions/16 for base and layer0 actors, and A positions/16 for
foreground. A already includes its layout offset, so bound layer sampling adds no second offset.
Each8px tile is drawn using its retained priority bit. Base composition is low B, low A,
high B, high A, retaining alpha. Sprites keep their existing mutual order; entity layer versus
live window count determines display priority independently of `SetEntityPriority`. After a low
sprite, high B/A regions are restored only under that sprite's nontransparent ink. A high sprite
requires no repair. This preserves mixed-priority sprite order and transparent holes; no character,
text or room selects the rule. The existing sprite resource cache retains its row ink spans for
this mask (mosaic draws use their sampled block coverage), and all map regions reuse the existing block texture. Actual draw passes and masked
8px overlaps are included in `cameraProjection.occlusionDraws`.
The adapter applies no player-centering, viewport clamp or MoveToward to this profile. Modern
320×192 viewport/layout and existing textures remain. `cameraProjection` records actual draw
tick/token, plane origins/source offsets, first clipped tile/count and actor rectangles/resources/
culling; observation compares only matching source-state draws. Focus/visibility suspension reuses
the common clock with no accumulated debt. Unbound centering, clamping, smoothing and completion
remain unchanged.

[Native acceptance](../verification/field-programs.md#source-bound-camera-observation) covers the
whole accepted prefix and both Messenger destinations, stopping at text531 W1 before Wait/Ack.
The camera stays held; zone/script return and F603 remain pending. Physical tracking uses the
bound target rule below. Explicit speed, cursor/pulsating overrides, profiles outside the admitted
secondary parallax domain, autoscroll and word-wrap destinations remain unsupported; no hardware timing claim follows.

### Source-bound nod

With explicit field text/view settings, a nod validates the admitted field context and creates
`NodWait` for the resolved physical entity. AnimationFF precedes ten common services; elapsed10
selects the transformed sprite, elapsed30 restores normal selection, and elapsed40 waits for actual
completion before animation0 and caller continuation. Services retain the current entity flag and
entity→view/window→portrait order, shared RNG and prior poll copy. Adjacent nods retain their own
40 opportunities. This implements the [bounded source contract](../../../docs/design/contracts/map-exploration.md#bound-field-nod-lifecycle),
not a natural hardware-frame count.

The adapter mounts the existing normal/transformed sprite resources and draws the semantic phase,
without a wall-clock gesture timer or per-frame acknowledgement. The existing token/kind completion
joins logical completion; an early receipt cannot skip services and a late one cannot add them.
Focus loss or hidden view suspends automatic work and clears debt. `nodProjection` records the
actual actor, slot, sprite, facing, phase, texture selection, dimensions and viewport visibility.
The [native observation](../verification/field-programs.md#source-bound-nod-observation) reaches both
Messenger nods from the unchanged start and stops at text521 W1 before Ack with the zone still active.

Unbound nods retain their 40/60-second presentation timeline, altered band from10/60 to30/60,
and elapsed-time completion independent of culling. A culled actor or missed render phase does not
fabricate a draw; aggregate gesture counts alone are not visible-phase evidence.
`CompletePresentation` validates both wait token and kind; `EntitySpriteReady` validates slot and
request generation. Enter acknowledges dialogue only and cannot complete an unperformed service.

The audio contract permits an explicit project-authored presentation mapping: MUSIC_JOIN and
MUSIC_SAD_JOIN play a generated C-major/C-minor chord loop through `AudioStreamPlayer`, with an actual
fade and stop. This is a modern cue, **not original audio parity**. Sound and fade services continue
while the common story owns control, including after
the active state has become a battle with no exploration world. Camera and gesture cues require a
current exploration world; without it they report `AdapterError` and retain their wait instead of
using stale map state. Unbound camera smoothing, portrait crop,
text layout/full-line display and unbound cue duration remain bounded presentation choices.
The nod transform reuses the inspected source band mapping; original raster/DMA timing remains unproven. Outside the admitted field-text/entity-event profile, portrait blink/mouth/typewriter
RNG remains unbound. Original VRAM/DMA timing, speech SFX, door/warp music
and fade fidelity, waveform/tempo and hardware frame equivalence remain **Unknown or Unsupported**.
They are not silently reported as performed original services. An unbound explicit presentation cue
reports `AdapterError` and retains its wait.

### Bound before-battle scene and windows

A real `BeforeBattleFinished`/`EnteringBattle` program with bound text/view state, no event caller
and a live cursor can use ordinary explicit portrait/text/view services. This retains actual
nested callers, route and source text cursor; text Wait/Ack requires logical input and completed
delivery, while field movement remains closed. Nested `end-map-script` retains the real
continuation rather than manufacturing field input. Legacy and outcome contexts gain no admission.

Explicit black FadeOut/FadeIn cues without a period override use validated Display.Period;
supplied nonzero overrides restore the prior period after completion. csc37 and ordinary black
helpers share the existing finite fade service, without a new clock.
The source loadMapFadeIn lowering now explicitly emits camera-entity:null before that helper.
Bound null detach validates the same caller context, clears only logical tracking and preserves
axes/destinations/counter/services/RNG with no time advance. This represents the source FF store
before its first enabled fade wait, without claiming CPU-cycle ordering of fade-setting writes.
Standalone black fades retain following; production never infers composition from the next opcode.
After scene loading, source lowering emits one explicit `wait-ticks` service before returning to
subsequent instructions. This services retained entities under the existing enablement policy;
it is separate from pre-load fade services and precedes any `scene-entities` replacement.

Bound `scene-map` requires settled closed windows, no ordinary warp, a valid target base palette,
a settled black current display and the accepted view profile. Its explicit camera origin selects
the target area and independently transforms A/B; following and scroll speeds/mask clear, but the
source follow counter and entity service policy remain. Window state clears without resetting
party/RNG/music/callers or invoking OnLoad. Base changes while black Current remains unchanged;
only the later real fade restores new colors. Visible/transitioning/open-window/missing-palette
loads reject before any replacement. This restriction is a remake representation boundary,
not a statement about original visible csc48 behavior. Separate `scene-entities` replaces physical
aliases and mounts sprites without re-centering or reattaching the camera.
Bound rendering uses the selected logical area and both plane origins even when no target-map
area covers the retained player. Player-area and legacy overlay lookup apply only to unbound drawing.

**Confirmed (bounded remake observation):** the representative ordinary native run continues the existing same-session route from Map40 approach
through its last marked exit Up, Map57 scene/entity replacement, explicit destination(2,8) and
Astral135 text2292/W2 confirmation to2293/W1 input. These operands identify observation only.
No tick/seed, route endpoint or instruction index controls production legality. Bound white palette
services follow the rule below; the bound before-body/first-input acceptance is described below.
The older unbound full Battle01 admission is separate evidence; modern carried-party/RNG admission
and original natural cadence remain open. [Source/semantic contract](../../../docs/design/contracts/map-exploration.md#bound-black-scene-replacement-and-before-battle-windows).

### Bound physical camera tracking

`camera-entity` validates the existing bound field/map-init/real-before caller, resolves a logical
selector once through the normal physical entity resolver and installs only LogicalView.TargetSlot.
Valid slot0/default aliases and invisible allocated sprites remain valid. Missing/removed/out-of-table
references, reserved physical cursor slot63 and incomplete/outcome contexts reject before publication.
Installation/retarget/detach advances no tick or RNG and preserves independent axes, destinations,
speeds, source follow counter, area, windows, callers, service policy and party. Compatibility camera
entity/destination hints clear; actual bound projection continues to use LogicalView alone.

The existing common service moves enabled entities before updating the view/windows and registered
portrait. Following reads that live physical record, selecting B for layer0 or A for255. While any
axis is active it preserves prepared speed/counter and completes the existing scroll; later settled
following uses the existing deadband/clamp/counter rule and independent128/256 parallax. Null detach
retains axes/counter; explicit destinations clear tracking. Sprite visibility is independent of
identity removal. No new reducer, render smoothing, resource, clock or content lowering is needed.

Ordinary W1 Ack, portrait unregister/close, text close and source Sleep10 precede subsequent target
installation/motion. Later portrait/text opening and input retain the real before continuation and
route. Idle input presentation performs no camera service. Bound white palette uses the finite
service below; complete before-body,
later effects and battle admission are separate. [Observation](../verification/field-programs.md#bound-physical-camera-tracking-observation)
and [contract](../../../docs/design/contracts/map-exploration.md#bound-physical-camera-target) own this boundary.
Independent ordinary native readback reaches tracked motion and three later W1s before the white
Unsupported. Its raw observer FAIL from two expected-stop assertions is preserved separately from
semantic readback PASS; the corrected observer has not been executed.

## Battle01 admission and first input

**Confirmed, bounded:** the same R1 session continues from Map21 `(5,15)` through the accepted
46-input Map21/40 extension, the marked Map40 exit, complete `bbcs_01`, source new-battle
initialization, `LoadBattle` presentation, empty start hook and the first actual battle input.
The [admission evidence owner](../../../docs/research/map3-battle01-admission.md) and its H2/H3 fixtures
own the original facts. The pinned `mainloop.asm`, `battleloop_1.asm`, `loadBattle.asm`,
`cs_beforebattle.asm`, map script engine and map entity allocator supply executable content.
The ordinary program never reads those comparison fixtures.

`loadMapFadeIn` starts out-to-black and loads the scene layout/camera; the later `fadeInB` remains
separate. Scene loading does not run exploration init or replace entities. `loadMapEntities` then
rebuilds the physical records and identity table with live followers and custom main-entity coordinates.
Every new sprite must mount before the next instruction. Astral135 is now a real allocated record,
distinct from Map21's earlier unassigned135 alias to player slot0. Camera tracking resolves a physical
slot; explicit camera destinations clear tracking. Shiver saves/restores the animation counter and
global sprite size around its typed presentation wait.

Selection writes source F399 before the before-battle program. Initialization validates before
publishing battle state or clearing F90–F105. Living active allies consume source formation slots
in live membership order; dead ordinary allies remain unpositioned. An absent Sarah leaves no deployed Sarah and
Chester takes the next available slot. Living eligible allies heal, dead ordinary allies remain dead,
and Peter/Lemon follow the source immortal exception. Accounting, source equipment/spells and
main/thinking RNG survive; enemy initialization and region/AI reset use existing Domain rules.
This bounded baseline already has refreshed/equipped ally stats. Status requiring broader stat
refresh, F88 resume, other difficulty/control modes and a larger unadmitted roster remain Unsupported.

The load program holds control while Godot fades out, mounts the existing battle board and roster
in the same view/session, then fades in. Input and automatic turns stay closed during that service.
Only completion permits the start wrapper to set F451 and generate the first round. Seen-intro entry
skips both hooks but still initializes/loads. Completed selection clears its unlock and returns field control.

The independent H3 comparison deliberately supplies its external `0x1234` seed and three-member
roster, runs the whole before body and checks first actor1, RNG and turn order. These values are
never injected into the continuous R1 run. The native observer continues the same opening/castle
instance, observes white fades, mosaic and shiver draws, then presses Enter/Escape at battle control.
Original timing/pixels/hardware effects and natural title/menu reach remain Unknown.

White fades use an actual white overlay; black fades modulate the current scene. Both are blocking
modern half-second services, not original asynchronous palette cadence. Mosaic-in draws coarse
source-sprite samples through progressively finer blocks over half a second. Shiver draws three
alternating five-tick offsets, then restores the engine fields; original sprite DMA/bitfield waveform
is unclaimed. The renderer reports an adapter failure if an admitted service cannot be bound.

## Battle01 outcome, after-program and return

Bound outcomes initialize the newly built field world through `ExplorationTextRunner.Initialize`:
its view starts from that world, windows are closed and old entity-service/window work is cleared.
Display, party/resources/progress/loadout/RNG, route and captured return anchor are retained.
`BattleReturn` is shared by text admission, portrait admission and text readiness. It requires the
actual outcome continuation, route, anchor, cursor and initialized settings/view; it does not
rename the program as ordinary field input. Nested return-map initialization retains its callers
until completion. Existing reveal/input phases and portrait registration still control confirmation.

**Confirmed — bounded modern native route:** the unchanged opening party/world with the accepted
cumulative scene selection reaches live battle control, adaptive legal commits, victory and the
full bound after/return through actual keyboard inputs. Registered explicit windows retain the
victory continuation, source join/unlock/completed order precedes returned field input, and a real
Move settles afterward. The existing target-browse contract permits range rejection without changing
accepted target, party/resources or RNG; rejected attempts and subsequent legal confirmations remain
in the observation. Original natural cadence/pixels and executable H4 applicability are separate.

The connected private world binds the original outcome hooks and growth tables through Content.
Actual HEAL/item/physical awards consume one EXP threshold, grow the five base stats with carried main
RNG, refresh admitted ATT-only equipment and retain current HP/MP separately from their new maxima.
Learned spell upgrades update both the live spell choices and packed source spellbook. Class caps
consume the threshold without a growth draw. Missing growth or unsupported status/equipment/spell
effects remain explicit boundaries; growth metadata never supplies an action history or kill order.

Action publication orders the reached empty enemy-defeated hook, death accounting/cleanup and
outcome check before an ordinary after-turn/queue advance. Leader loss takes precedence over enemy
exhaustion. A terminal action does not consume another turn. The same session then owns the complete
outcome program; `battle-returned` is emitted only after its callers and return-map init finish.

Victory heals eligible living/immortal allies, executes all of `abcs_battle01`, applies the source
join-table tail (including member zero), clears F401, sets F501 and executes the return load. The
script's `resetForceBattleStats` separately restores all supplied allies, including ordinary dead
allies. Its displayed map is **Map57**, not Map40. The controller captures the first active ally's
battlefield position before the script; the script's mainEntity position does not replace that
return tuple. Reached action tails recreate the ally facing DOWN. Map57's void setup still creates
the player/followers; its F506 layout-copy branch remains a guarded frontier.

Ordinary defeat plays its own sound/text, restores the leader's HP, halves unsigned current gold,
then heals eligible living/immortal allies on exploration entry. It retains F401 and does not set
F501 or execute the after-program/join tail. The selection explicitly supplies egress Map3; F399
must be set and F64/F640 clear, selecting (32,13)/UP. This is a controlled egress choice, not evidence
for the original campaign's natural producer.

Normal Map3 reload retains the real `cs_513BA` hide and removed entity142 alias. At exactly
`byte_513A8:1`, the [accepted source contract](../../../docs/design/contracts/map-exploration.md) permits
the four writes into inactive window scratch to have no further map/entity/camera effect: windows
are empty, old presentation work has drained, and a new window is rebuilt before publication.
The common renderer has no emulated DMA queue. The selected
[`Sf2StoryPolicy`](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs) requires exactly
`byte-513a8:1`, Map3, MapLoaded/OutcomeMapLoaded, a closed window and F603. A still-live142 alias
receives the real `Hide` move-out, retaining aliases. A removed alias requires F1 and a real hidden
entity142 tombstone at X/Y0x7000. Missing records, other call sites and active windows are rejected.
The [typed source family and finite candidate](#source-story-policy)
leave PC/stack/wait/revision publication with the generic runner. Explicitly authored compositions
reject this operation at its PC; the private produced opcode remains unchanged.

Godot keeps the battle projection until the actual fade/load hands over to the scene, renders
mosaic-out and sprite replacement, and plays a distinct project-authored defeat cue. Sprite changes
wait for the exact slot/request completion. After source init and fade-in, ordinary movement uses
the existing exploration view and session identity. These are modern presentation services;
original waveforms, DMA/VInt behavior and natural original continuity remain Unknown.

## Remaining source boundaries

The accepted Map21 default-population contract resolves135 to slot0 under its stated initialization
conditions. Natural original call-time RAM, full sprite/hardware effects and timing remain **Unknown**;
remake continuation is not a new original H3 observation. The continuous remake route reaches
Battle01 victory/ordinary-defeat return; original natural continuity across the H3 bridge and
original presentation remain outside this claim. Other Map3 native branches such as ChurchMenu and
moveNextToPlayer retain executable source frontiers.

The post-F603 abstraction above is specific to inactive window scratch on normal reload. Active
windows, deferred DMA, other missing-entity helpers and later storage reuse remain Unknown; no
generic missing-entity no-op, player fallback or entity255 is introduced.

## Reference migration boundary

The ordinary source path has no Reference dependency. The legacy Sarah, zone601, entity142,
Astral-zone, messenger, castle gate, palace, Astral invitation, tower guard, castle/tower cross-map,
Map40 pending-admission and defeat-recovery/return/arrival executors were removed with their callers
during M3/M4. M5 then retired the remaining independent startup/action comparisons, frozen context
DTOs, legacy geometry/visual bindings, the reference host and the `map3-post-opening-reference-start.json`
input together with `SF2_REFERENCE_POST_OPENING_START`. Whole-flow comparisons use common commands and
actual native input. The separate private probe retains the Map21 guard and missing-presentation
comparisons. A1–A8 closure and 8C/H4 are not reported.

## Source-bound yes/no lifecycle

With the explicit field text/view profile, `ChooseYesNo` retains a `ChoiceWait` through entry,
opening, raw held release, conditioned input, closing and the caller's return delay. Legacy
unbound `ChooseDialogue(bool)` remains separate and cannot release a bound choice. The flag operand
is generic; no text, route, instruction index or seed selects legality.

`ChoiceHeldInput` captures mapped held state at entry. If held, opening must finish and genuine
release must occur before polling. Release adds zero semantic service under the admitted modern
input boundary; this does not assert zero original hardware interrupts. `PollChoice` accepts a
fresh conditioned semantic mask. Left, Right, Cancel, ConfirmC, ConfirmA have source precedence.
Left/Right select Yes/No even when reselecting; Cancel returns No; either Confirm accepts the
current selection. Up/Down alone are neutral polls. Initial selection is Yes. A neutral explicit
Wait or directional poll draws/decrements the logical animation counter and runs one common service;
confirmation/cancel starts closing without that loop tail. Host idle/reveal/focus latency does not
create opportunities or debt. Raw held bytes are never silently treated as conditioned input.

The own14×3 window opens from(32,17) to(12,17), closes to(-16,17), and advances at the common window
stage. Source length4 has four movement passes followed by busy clearing; completion reads every
represented moving window, including dialogue and portrait. Opening/closing refresh an existing
dialogue window for4; selection refreshes it for1 only when settled. Delete the own window before
writing the result flag, then run exactly ten common services before advancing the instruction.
The choice does not close the dialogue/portrait or increment dialogue/portrait presence.

`StoryState.WindowFixPending` carries the source post-scroll flag through waits, empty-window passes
and map initialization. At the common window stage, dialogue, portrait and choice advance their own
geometry/counters and record incoming busy status. A hidden pass sets the pending flag. An unhidden
pass consumes it only when a represented window exists, refreshing every geometrically settled window
to length1/counter0 while preserving the pass's busy result. Stationary animation can still be busy;
its counters cannot stand in for geometric equality. A four-step own move arriving on the first
unhidden pass after three hidden passes therefore completes on service6, not the usual5. No fixed pad
is applied. Dialogue tracks window coordinates separately from its glyph cursor; every close entry
uses its actual current position. Portrait geometry runs here once, with registered blink/mouth RNG
remaining later. Dialogue and portrait close helpers also wait for every represented moving window.

The fresh remake session admits a clear pending flag. **Confirmed (static source):** full system
initialization clears its RAM byte. **Inferred:** the retained controlled opening start has that value;
it does not capture the byte or establish natural cold-boot ancestry. Ordinary initialization does
not reset it. See the named initialization and postpass bodies in the source owner.

Common entity→view/scroll/window→registered portrait service uses live enablement, counters,
Typewriting, held/follow camera and RNG. There is no direct choice RNG draw or poll-copy write.
The animation begins at15, wraps1→20, uses the alternate variant at values>=10, and navigation
resets the old variant then draws the new choice at19. Actual Godot labels/selected color and panel
position consume this logical state; original menu tile rasters are not extracted. Remapped keys,
pad buttons/axes and swapped Confirm/Cancel use the installed input actions; entry release includes
all mapped gameplay actions except the separate modern Wait. Duplicate held pad packets and axis
values do not become fresh edges. Focus/hide requires a fresh release and resets clock debt.

Source callsite audio emits65 per move-window request (own and existing dialogue),66 per selection,
including reselection. Bound confirmation emits no generic67. Existing whole-PCM playback and
preemption remain authoritative for actual sound delivery; no sound duration controls entity ticks.

Admission is normal movement, gold absent, field continuation with logical text/view and Closed or
admitted EventCaller portrait. **Unknown:** incoming original global24/6 repeat/direction ancestry,
release-spin CPU interrupts, debug turbo, gold-window lifecycle, exact menu raster and hardware
DMA timing. These exclusions do not prevent both answers in the declared fresh semantic stream.
JOIN's audio/input/live-service coupling remains separate. See the
[source contract](../../../docs/design/contracts/map-exploration.md#source-bound-choice-consumer),
[original provenance](../../../docs/research/map3-messenger-acceptance.md#source-bound-choice-lifecycle)
and [observation owner](../verification/field-programs.md#source-bound-choice-observation).

## Raw field display

Bound explicit-window `ShowText` uses `FieldTextWait` even when `WaitForAcknowledgement=false`.
The existing token stream, substituted names/font advances, window/glyph/scroll work and
`FinishDelivery` own completion. Actual W1/W2 retain their input consumers and ordered poll/RNG
semantics. A final no-W span returns only after logical End and actual reveal; it neither invents
an Ack nor closes the window. Early reveal cannot skip mandatory work; late reveal adds no service
or clock debt. Unbound legacy execution is unchanged.

Only raw text with no explicit speaker and no event-speaker lookup admits intentional absence.
Missing event context and invalid explicit speakers still report `field-text-speaker`. Speakerless
host delivery uses the existing silent path, while an admitted registered portrait keeps its own
blink/mouth service and RNG. This does not decide ordinary speech delivery policy under#517.

Bound `PresentCue(SoundWait)` without a finite profile reports `field-music-progress-unbound` at `program.presentation`,
kind `UnsupportedCapability`, stop `Unsupported`. Completed text/window/entity/party/flag state
remains at that instruction. No PresentationWait is created and actual finite-audio completion
cannot release gameplay through previous-music, plain input or script return. Existing PCM playback
and unbound presentation observations retain their previous applicability.

The [source contract](../../../docs/design/contracts/map-exploration.md#raw-field-text-and-the-music-boundary)
and [provenance](../../../docs/research/map3-messenger-acceptance.md#raw-display-and-unbound-music-progress)
separate this supported raw-text rule from the unknown sound-to-service clock. The known505 driver
updates are not a duration in entity services. See [verification](../verification/field-programs.md#raw-field-text-observation)
for behavior and native expected-boundary acceptance.

## Modern Finite Music

The [accepted modern policy](../../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
uses optional finite audio metadata `modernEndStep`; the private lowerer emits505 for command19.
The reader permits a positive endpoint only on non-looping music. Ordinary playback needs no profile.
`MusicProgress` retains cue, semantic generation, previous stack, map/area, progress, actual completion
and logical previous eligibility. Duplicate current cues preserve it; earlier replacements start at0.
Ordinary unprofiled/looping `PreviousMusic` also pops this history and changes the semantic generation;
it does not require a finite helper. Restoring a profiled cue starts its progress at0, matching the
player restart. Unknown/empty history remains Unsupported.
`ExplorationMusicRunner` advances this state before each existing bound common service, regardless
of entity enablement. It never adds a second entity service or uses PCM duration.

`MusicWait` first arms, subsequently samples logical end, and returns only after a complete group
of3 services plus matching `CompleteMusic(generation,cue)`. Early receipts latch; late receipts
cause no tick/RNG/debt. The host observes actual finite completion and then the existing actual
previous-track restart. Wrong/stale/duplicate receipts reject; active-helper interference or a
changed bound map/area/battle-selection reports Unsupported, and changed host playback reports an explicit adapter error.
Missing profiles retain the raw-text boundary above. Looping/other unbound playback remains supported.

The common commit boundary invalidates history and finite phase on context changes outside a helper,
including an intermediate map/area change followed by a return. `HistoryBound` prevents that old
stack from becoming valid again merely because the final map matches. The first subsequent explicit
request establishes only its current cue with empty history/no finite profile; a later different
request can use that known cue as previous and admit finite progress. This avoids assuming that
SessionAudio consumed every intermediate map selection within one application result. No host
history or timing is copied back into gameplay; context changes add no extra service.

A subsequent external ordinary field input may establish only the current map cue when the
invalidated music context differs from the published map/area/battle context. The host consumes
that final result through `SessionAudio.Observe` before dispatching the input, and that changed
context selects typed map music. This clears the obsolete stack without reviving it. Valid explicit
script music remains untouched. A transfer and return to the old context in one result cannot
establish map music this way: the host may retain its explicit cue, so history stays unbound until
an explicit request establishes it. Binding advances neither service time nor RNG.

Plain input after the helper keeps the raw window without revealing it anew. A genuine Wait uses
the existing one-service input-first rule without a W1/W2 copy. Bound plain Ack is silent; only
accepted W2 requests validation67. Ack, close/Sleep10, follower/position
instructions and script/Zone return reuse their owners. No production rule names text447, cs-51614
or a selected seed. [Verification](../verification/field-programs.md#modern-finite-music-observation)
owns behavior cases and the bounded native JOIN return. The [current composed milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
has separate acceptance; this JOIN observation alone does not prove full-route or hardware parity.

### Bound white palette

With validated Display, typed `present` FadeOut/FadeIn resource `white` selects the source map-white
profile; existing lowering already supplies this cue. `FullFadeWait.Color` distinguishes black and
white, retaining token, countdown, entry, extra service and logical/actual completion. White uses
period1, seven base-derived positive offsets, terminator and one extra service, then restores the
stored period. Base is unchanged; White/BaseRestored/Black visibility is explicit even for endpoint
color equality. Black temporary overrides remain black-only; FlashWhite/RestorePalette and conflicting
FullBlack+white remain Unsupported. Starting from settled black/restored/white is valid; invalid or
transitioning state rejects. Startup JSON still admits only black/base-restored states.

Fade ticks reuse common service with current script EntityServices override, then program default;
ordinary warp deliberately retains enabled field entities. Existing music runs exactly once, followed
by entity, view/window and registered portrait. Logical completion suspends service while actual
delivery finishes; early delivery still waits for all logical work. Late/stale/duplicate receipts add
no opportunities. Renderer clears inherited black modulation before white and stale white before
black, preserving the half-second modern delivery and reduced-flash alpha0 policy. Its viewport
coverage is not source map-only palette pixel equivalence.

The white-helper observation ends at Chester2297 W1 after all six white helpers and their
intervening source waits. It does not establish the following bound mosaic/shiver, complete before
body, battle load/input or winning trace. The bound continuation is described below; the next genuine
Unsupported blocker is **Unknown** until observed. See [verification](../verification/field-programs.md#bound-white-palette-observation)
and [source/semantic rule](../../../docs/design/contracts/map-exploration.md#bound-map-white-finite-service).

### Bound before-body and battle entry

Field palette routing checks `ActiveExploration`, not merely retained Display. After the genuine
before-context completes, BattleEntry initializes `ActiveBattle`; its unprofiled black loader cues
use the existing `PresentationWait` modern delivery lifetime. Matching receipts retain ActiveBattle and its state
and frozen field Display/view/settings; they add no field opportunity. Explicit FullBlack in a
non-field context rejects as `full-fade-context` before helper publication. The old scene facade
remains available until the existing mount and handoff. Load/start/intro ordering and first queue
remain owned by BattleEntry and BattleTurnFlow; the final delivery can resume those owners and their
ordinary first-queue RNG draws without adding a field opportunity.

Bound generic `PresentationWait`, including mosaic/shiver and retained-profile loader delivery,
now uses the existing visibility/focus suspension and zero-debt resume. Effect completion/restoration
and unbound/music policy are unchanged. The [contract](../../../docs/design/contracts/map-exploration.md#bound-before-body-and-battle-loader-ownership)
distinguishes this modern loader policy from original LoadBattle opportunities. **Confirmed (bounded remake):** the ordinary full bound
before-body and first-input observation passes through the [verification route](../verification/field-programs.md#bound-before-body-and-battle-entry-observation).

## Source Story Policy

The existing public v8 reader admits [story A](../../content/authored/story-rule-demo-a.json) and
[story B](../../content/authored/story-rule-demo-b.json), including all untaken branch/call targets.
An ordinary interaction branches on live flag40, calls an external first/again program, then calls
a nested pause program. That program presents its own text and waits30 logical services before
returning. The called program then changes a flag and entity position/visibility; its caller toggles
flag40 and returns field control. Both outcomes execute in each package in one session. Package
names never choose runner algorithms. [The program owner](#authoring-conditional-programs)
describes the actual effects and content editing steps.

[`SourceStoryInstruction`](../../src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs) is a typed
source-only family. Only this family delegates to the selected
[`ISourceStoryPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/ISourceStoryPolicy.cs).
The SF2 [instruction](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryInstructions.cs) and
[policy](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs) own the existing retired Map3
operation. The reader maps the unchanged opcode; private produced bytes and schema are unchanged.
`ProgramRunner` knows neither this exact source operation nor its map/program/flag guards.

The policy returns only a `SourceStoryCandidate`: no active-state change, or one retired existing
physical entity record. `SourceStoryRules` checks that record against the existing `Hide` result
at its actual slot; it preserves every other record, aliases, layout, party and world metadata.
It does not resolve a record's other logical alias again. The candidate cannot contain story state,
PC, stack, wait, seed, revision or observations. The runner alone advances/commits the instruction.
All original exact context, live-alias hide and removed-alias tombstone guards remain in SF2.
This is the accepted conditional inactive-scratch abstraction, not a new visible scratch effect.

Every explicitly authored factory selects
[`AuthoredStoryPolicy`](../../src/Sf2.Remake.Application/Gameplay/Authored/AuthoredStoryPolicy.cs), which
rejects source-only instructions at the reached PC. Direct source-default starts and `ForGame`
remain SF2. Content profile does not automatically select executable policy; see
[trust/selection](../content/profiles-and-trust.md#story-content-and-source-policy-selection).
Ordinary branch/call/return/text/tick operations are independent of that policy and unchanged.
Existing callers already pass the same `SessionRules` through field and battle-outcome continuations.

Expected source errors retain code/field/category and add policy/operation/PC to the message.
Malformed candidates or unexpected exceptions use the existing InvariantFailure policy diagnostic
with the same context and no private exception detail. Earlier committed instructions, the reached
PC and stack remain. A rejected or duplicate completion cannot apply a later effect or return a caller.
[Behavior tests](../../tests/Sf2.Remake.Engine.Tests/StoryPolicyReplacementTests.cs) and
[short actual observations](../verification/field-programs.md#replaceable-story-observation)
cover content replacement, failure retention and usable control; source-context tests do not claim
private natural reach, original timing or measured performance.

### Selected rules during map transfer

Standard rigor applies to this bounded policy-propagation correction: preserve the session's bound
composition at initialized battle admission, with observable program ordering and retained failure
prefixes. It does not change source policy, Content admission, finite source guards or the available
instruction set. Terminal/poison and broader field-state continuity remain separate work.

`TransferToMap`, queued warp, delayed map load and battle return carry the selected `SessionRules`
through `MapTransfer` and `BattleEntry`. Policy-sensitive internal parameters are required; these
helpers no longer substitute SF2 when an argument is omitted. Explicit source-default session and
standalone comparison entries still select SF2.

With `NewBattle != null`, a completed/unlocked destination consults the selected
`EnterCompletedBattle` before map rebuild or destination on-load. Admission retains the current field
scene for the before-program, then initializes battle, runs load and runs the unseen intro/start
program in order. Refusal allows destination on-load, then ordinary map admission consumes the
completed route's unlock without entering battle. An uncompleted route does not ask replay policy.
Public JSON controlled starts have no `NewBattle`; they are not a reproduction of early initialized
selection, even if later map admission consults the same policy.

An unexpected policy exception reports `InvariantFailure`, `battle-policy-failure`, field `rules`,
and the selected identity/`completed-battle-admission` operation. It publishes no speculative map,
entry or battle. Earlier flags and the reached transfer PC remain committed. A typed simulation
retry performs its ordinary service and retries that PC without replaying earlier instructions.

[`MapTransferPolicyTests`](../../tests/Sf2.Remake.Engine.Tests/MapTransferPolicyTests.cs) uses
project-authored initialized definitions and actual `GameSession` commands to cover opposite replay
answers relative to each SF2 fallback, destination on-load, before/load/start waits and flags,
throw/retry, ordinary uncompleted admission, explicit source defaults, queued warp and a public JSON
controlled-start counterexample boundary. Its synthetic source
metadata only exercises the fallback discriminator; it proves no private provenance or original
route. After loading the existing ignored environment and forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run the locked engine project with
`--filter FullyQualifiedName~MapTransferPolicyTests`; use the
[locked workflow](../development-and-verification.md#locked-net-workflow) for scoped engine/adapter
gates and the committed planner. No native/emulator observation is required for this logical edge.

## Continuous second-encounter boundary

The selected private route keeps one `GameSession` from the controlled tower start through actual
Battle01 outcome, castle return, Jaha and Battle02. `ScenarioDefinition` owns immutable encounter
definitions, initial inputs and scene resources. `BattleEntry` selects the next definition and carries
the live ally records, gold and both RNG images; only new members/enemies use the selected baseline.
The source new-battle refresh still heals living allies and applies the admitted refresh rules.
Continuity does not mean bypassing that refresh or preserving provisional battle positions.

SF2 story policy preserves the source completed-and-unlocked warp behavior, including consuming the
unlock once and entering the selected encounter. The authored policy retains its one-shot completed
route behavior. Map setup selection uses the last satisfied source flag variant without replacing
the immutable layout. Motion waits can finish at the destination while retaining the installed
follower script; script-completion waits retain their separate meaning.

The source `csc08` join body compiles into existing program instructions and returns to its caller.
Member/class text captures the reached join member, while an explicit text ID leaves the shared text
cursor unchanged. Missing class content is a typed reached failure after any earlier committed join;
it does not run the caller's later unlock. The exact-position/no-op and already-joined source branches
remain in the selected program, rather than being replaced by a general join shortcut.

The host rebuilds the board only when its actual battle definition or scene resources change, before
publishing observations. Repeated attachment of the same state keeps the board. Scene cache keys
include the encounter, and field-death selection uses the active scene package. Field music chooses
the published map context after an outcome; a retained `EnteringBattle` anchor does not select battle
music during victory/defeat return. This also preserves valid explicit cues and rejects unbound
same-context music waits. [Direct acceptance](../verification/field-programs.md#battle01-through-battle02)
covers the actual cross-encounter consumer; Battle02 resolution remains outside this selection.
## Field-player binding across connected battles

**Standard scope:** retain the admitted field player's identity and applicable movement speed across
connected victory, defeat and retreat, including a connected map that enters battle during startup.
This is a bounded cross-continuation correction: one `GameSession`, existing outcome rules and
program waits remain authoritative. Battle legality, terminal
poison behavior, UI and new content capabilities are outside this agreement.

`BattleEntry` captures a `FieldPlayerBinding` containing only the live player ID and speed after the
before-program completes, in the same commit that publishes successfully validated battle
initialization. Legitimate pre-battle entity speed changes therefore apply. Failed initialization
keeps its accepted field/program prefix and publishes no speculative binding. Load/start programs
then run in battle mode; the binding survives those programs, battle turns and paid retreat delivery.
No old field snapshot, battle-leader inference or player registry participates in return.

For authored fields without source population, the binding retains the live player's `XSpeed`, the
same scalar used by ordinary map rebuilding, and rebuilds both movement axes from it. Source-population
fields retain the required logical `entity-0` / physical player slot and the established outcome
projection speed reset to32, even if the before-program used another speed. This is an explicit
source-population rule, independent of authored ID spelling: an authored `entity-0` also retains its
live speed. `SceneEntities.Build` continues to own source allocation, aliases and source record data.

Both outcome projection paths consume the binding. After/return programs resolve the same named
player, and ordinary subsequent transfers use the current returned field speed, including any later
legitimate program changes. The binding is cleared with `EnteringBattle` only after the complete
return program/map continuation finishes. A later failure retains the binding, reached cursor,
committed flags/resources and exact pending wait. Missing binding in an internally constructed
connected outcome is an explicit `field-player-binding` failure.

Acceptance is actual engine behavior with varied IDs/speeds, all three outcomes, pre-battle speed
changes, direct/transfer entry, named motion/text/presentation, source population, initialization and
later failure boundaries, plus one compact ordinary-host authored return/movement witness. The
[bounded reproduction](../development-and-verification.md#authored-field-player-return) owns commands
and the native observation's limits. Broader original-route and hardware parity remain Unknown.
