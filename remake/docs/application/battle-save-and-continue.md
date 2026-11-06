# Battle Save and Continue / 战斗保存与继续

## Agreement and delivery boundary

**High rigor; [#1618](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1618).**
The authorized outcome is explicit Save during a battle, process exit, then Continue in that same
battle with equivalent subsequent gameplay. Silent loss, duplicate payment/rewards and a plausible
but different RNG continuation are unacceptable. High rigor controls recovery and staged acceptance;
it does not require a snapshot platform. This document owns the accepted staged implementation
contract. A implements finite in-memory continuation, B the versioned codec/shared store, and C
connects ordinary host Save/Continue with the bounded native observations below. Independent
integration remains main-gate's responsibility. The current
[opening lifecycle](opening-session-lifecycle.md#implemented-savepoint-boundary)
continues to own delivered field savepoints and the common storage/entry rules.

The admitted boundary is **an unfinished battle at settled player Movement input, at the actor's
committed origin, after all prior work has completed**. Save preserves the current queue entry;
Continue returns to that actor's Movement input, not battle entry or the beginning of the round.
Use the existing two manual slots. Offer Save and return to play, or explicitly Save and Quit;
Continue is reusable, not a consumable suspend file. This is a remake UX choice within the user's
authorization. There is no save during arbitrary animation, autosave, cloud, original SRAM import,
new slot manager, event log, predictive simulation, or later campaign admission.

The implementation target is the currently admitted connected campaign's battle/return routes.
Stage the first vertical proof in Battle01, then admit the remaining existing routes after the same
restoration checks; do not hardcode a particular round, actor or reference sequence as eligibility.
Standalone diagnostic battles with unknown progress or no field return are not silently converted
to product saves. Additional gameplay capabilities remain owned by their existing issues.

Design acceptance requires the boundary, reader-backed state disposition, compatibility/failure
policy, fresh-process acceptance matrix and bounded implementation ownership below. Implementation
acceptance requires the checks below to pass; serializing successfully is insufficient. Main-gate
owns review, implementation dispatch and integration. No unresolved product choice blocks this design.

## Implemented Domain/Application boundary (A)

**Confirmed in the remake; [#1636](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1636).**
`GameSession.QueryBattleSave`, revision-bound `CaptureBattleSave` and detached `RestoreBattle`
consume [BattleSaveData](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSaveData.cs) and
[BattleContinuationData](../../src/Sf2.Remake.Domain/Battles/State/BattleContinuationData.cs).
The existing field v1 API remains separate. Battle recipe revision1 uses the existing package,
gameplay, rules and field-compatibility identity plus a distinct battle recipe revision.
The immutable admitted scenario owns encounter definitions and the map/ordinal return route;
the payload carries live actors, queue/cursor, both seeds, AI/region state, death credit, custody,
semantic physical aliases and the retained party/story/music context needed by future readers.
Nullable typed operands distinguish absence or unknown from zero. B must encode explicit presence
without relaxing the disk contract's prohibition on JSON null.

Query/capture validate a detached candidate without consuming input, RNG, region work or rewards.
Restore validates the saved finite values against admitted definitions, reconstructs them directly,
and compares them against the reconstructed values to reject implicit constructor normalization.
Only pure movement preview is recomputed. No new-battle initializer, queue generation, story entry,
reward replay or speculative next action runs during restore. A fresh session ID, revision and
observation sequence replace process-local identities; the saved current queue entry remains current.
The reconstruction and comparison are specific to this contract, not a general snapshot system.

Admission requires a connected New Game route, complete active/retained roster and accounting,
known required AI/return resources, completed prior work and known ordinary looping battle music.
The saved player binding must support the intermediate battle map, resolved live egress (including
the declared defeat fallback) and special defeat destination. Population allocation requires
`entity-0`; plain authored return maps retain their supplied player ID. Only these destination maps
and their declared setup alternatives constrain the binding, because outcome programs and entry
can change setup flags before rebuilding. Admission does not execute those programs or inspect
unrelated maps. Invalid bindings reject before a replacement session is published.
Unknown death credit stays distinct from known-no-ally; a poison-capable encounter requires known
credit. Semantic visual aliases/old physical slots are retained when present; absence is not
replaced with actor projections. The [defeated field staging consumer](pre-voyage-campaign.md#galam-escape-through-battle06-first-control)
builds its own entities from combatants/population and permits absent battle-story visual tables;
supplied tables and actual battle-story alias readers retain their validation.

Reproduce actual behavior with `uv run sf2 verify engine`, or the narrow engine-test filter
`FullyQualifiedName~BattleSave|FullyQualifiedName~BattleEndPositionTests|FullyQualifiedName~DefeatedContextTests`
using the [locked SDK/environment workflow](../development-and-verification.md#locked-net-workflow).
`BattleSaveContinuationTests`, `BattleSaveEligibilityTests`, `BattleSaveProgressTests`,
`BattleSaveRegionTests` and `BattleSaveReturnTests` cover pure/stale/busy admission, real movement
return/undo delivery, changed actor/order, duplicate/dead queue entries, zero/high-bit seeds,
next AI/round draws, promoted orders, paid healing/items, committed equipment/discard, kill/drop/EXP,
poison credit, region-script once flags, visual aliases and repeated restore. Focused legal victory,
defeat and Angel Wing suffixes compare carried resources/return operands and use the existing field
v1 save/restore consumer at the supported returned recipe. `BattleEndPositionTests` runs defeated
Begin/Resume/death/outcome with and without a battle visual table; invalid supplied aliases still fail.

With registered private inputs, `BattleSaveProfileTests` reads the existing seven-encounter world
once, uses actual initialized definitions and explicitly known settled story/music context, then
compares the next legal turn. This is engine admission/continuation evidence, not natural campaign
reach or native restart. The local representative run covers 9–17 actors and 0–4 retained allies;
capture plus detached restore measured 24.72 ms for the first case and 0.39–0.83 ms thereafter,
excluding content admission, encoding and disk/host work. No media copies, traces or new acquisition
are needed; the existing world reader dominates memory. B records measured encoded size and peak memory
with the real codec/store under the resource limits below.

Entering a region script invalidates music history. `BattleSaveRegionTests` submits C's typed
`ObserveBattleMusic` only after actual program completion, then saves/restores and continues the
next round without replaying the region. Unknown history still rejects. Finite music/interpreter
continuation, later campaign content and arbitrary saves remain outside admission.

## Implemented versioned codec and shared storage (B)

**Confirmed in the remake; [#1637](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1637).**
[SaveRecord](../../src/Sf2.Remake.Content/Persistence/SaveRecord.cs) has finite field and battle
variants. `OpeningSaveJson.ReadRecord`/`WriteRecord` dispatch these records; the original
`Read`/`Write` API remains field-only. Version1 keeps its exact original property order, payload,
checksum and missing-`names` compatibility. Reading never rewrites a file. Version2 has required
`formatVersion`, `slot`, `sequence`, `savedAt`, `kind: "battle-player-input"` and `data` keys.
[BattleSaveWire](../../src/Sf2.Remake.Content/Persistence/BattleSaveWire.cs) maps only A's finite
battle/story progress, with no CLR type discriminator, path, definitions or computed view property.
All keys are required, including names. Every nullable operand is explicitly
`{"present":false}` or `{"present":true,"value":...}`; unknown death credit, known-no-ally credit,
zero and absence remain distinct. No JSON null is admitted. Custody flags must already be distinct
and sorted; duplicate deal identities reject before their constructor could normalize them.

The existing base64/SHA-256 envelope checks the exact decoded payload bytes. The 1 MiB encoded-file
and depth32 limits, duplicate/unknown/missing-key rejection and UTC/slot/positive-sequence header
checks apply to both versions. After envelope/checksum validation, the untyped top-level version
and v2 kind are classified before typed payload binding or null/key inspection of that payload.
Unknown versions/kinds produce `save-version`/`save-kind`, even with unfamiliar/null payload data;
they do not become a malformed old payload eligible for downgrade. Invalid JSON or excessive depth
still rejects at the common bounded JSON reader.

`SaveRecordRestore.Restore` dispatches to the existing field restore or A's `RestoreBattle`, using
only the caller's admitted definitions/rules. Reuse `OpeningRestoreReader` for configured private
content: its `initializeParty:false` path calls the existing `PrivateBattleContentReader.Definitions`,
not growth initialization. No battle entry, AI, round generation or route program runs. The payload
selects no input file or rule implementation. Failed admission returns no mountable session.

[LocalSaveStore](../../game/src/Persistence/LocalSaveStore.cs) accepts the same two typed variants
and a record validator. Both use one slot lock, sequence calculation, same-directory temp,
WriteThrough/flush, validated temp readback, atomic Move/Replace and validated final readback.
Readbacks also compare exact bytes, in addition to sequence; this is an inexpensive transaction
check, not a content-comparison framework. Recovery's second backup read is bounded as well.
`SlotRead.CanRecover` is shared by store and host: only missing or structurally corrupt primary
permits explicit repair from a valid backup. Semantic/content rejection, unknown versions/kinds
and unavailable readers preserve both files and forbid downgrade. Repair preserves the good backup
byte-for-byte. No operation promotes an interrupted `.tmp` residue. A failure after publication is
attempted returns `save-publication-uncertain`, including failed final validation after bytes were
published; callers must inspect the slot before deciding on a later operation, never retry or exit
as though the attempt certainly failed. Only the operation's own unpublished temp is disposable.

`OpeningPersistenceHost` consumes both typed variants. The shared recovery decision still forbids
downgrade behind unsupported versions/kinds or semantic/content rejection. Valid battle records
use their own labels and restore path; they are never converted to v1 field progress.

Direct acceptance invokes the real codec, `SaveRecordRestore` and linked production store from a
disposable console driver, with A's small `BattleSaveFixture.Create`/`Capture` and existing
`OpeningRestoreTests.Fixture` supplying inputs. Reproduce a deterministic roundtrip by writing
`new BattleSaveRecord(1, 2, DateTimeOffset.UnixEpoch, data)`, reading with `ReadRecord`, restoring
against the fixture's definition, recapturing through the returned `GameSession`, and requiring
identical encoded bytes with the same header. Envelope mutations recompute the existing checksum
so missing/duplicate/unknown keys, malformed presence, bad coordinates/queue/roles/loadout,
required unknown accounting, wrong compatibility/route and size/depth failures reach their real
owners. No codec/probe test suite or production fault platform is added.

Under an explicitly configured **new external disposable root/namespace**, directly call store
Write(v1), Write(v2), Write(v1), inspect sequences and both bytes, corrupt/remove primary and call
Recover. Open `slot1.lock` with FileShare.None for lock contention; hold primary with FileShare.Read
without Delete sharing to reach a real Windows replacement-attempt failure. The existing validator
delegate can admit the temp, then use an incompatible configured definition at final readback to
exercise uncertain publication after commit; rejecting at temp validation proves pre-publication
preservation. Both mixed-version backup directions, exact backup repair, interrupted-temp residue,
future version/kind, unavailable content and the field-only host policy were checked. These are
bounded direct consumers, not evidence of physical power-loss durability. Local reproduction commands,
driver source, disposable namespace and all completed failures/results belong in the executor's
ignored `local/issue1637/HANDOFF.md`; this document owns the API recipe and acceptance boundary.

The representative run reused the existing seven-encounter input read-only, following the same
initialized encounter/known settled context construction as `BattleSaveProfileTests`, then restored
through separately admitted no-init definitions. It copied no world/music. For 9–17 actors and 0–4
retained allies, encoded records measured 18,386–22,358 bytes. No-init content admission took 834 ms;
codec + detached restore + recapture/re-encode took 68 ms cold and 2.8–4.5 ms afterward; actual store
writes including flush/readbacks took 9.9–11.5 ms. Combined per-case managed allocation (codec cycle
and store) was 2.75–4.00 MB, process peak working set 644 MB including content, within the existing
3 GiB admission budget. These are small local samples, not latency guarantees. The cold metadata/JIT
cost merits C's ordinary-input observation and use of the existing detached `LoadEntry` work where
needed; there is no size pressure, new cache, checksum scheme or limit increase. Fresh-process native
equivalence, loading feedback, music and outcome mounting use C's native verification below.

Validation uses `uv run sf2 verify engine`, `uv run sf2 verify adapter`, selected consumer builds,
and clean committed `uv run sf2 verify plan --base origin/main --head HEAD` under the locked
environment workflow. No normal/full research suite or Godot restart is selected solely for B.

## Implemented ordinary host boundary (C)

**Confirmed in bounded remake observations; [#1638](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1638).**
The existing system menu offers the same two slots at admitted player Movement, with localized
encounter/round/actor/time labels and explicit overwrite consent. Confirmation offers Save and
return or Save and Quit; only verified publication marks the captured revision clean or exits.
The saved acknowledgement returns directly to play. Nonzero preview, pending movement/scene,
selection and story work refuse without cancellation or a queued save. Closing the menu resumes
the real pending consumer. Existing modal pause, ownership and release barriers remain shared.

`SessionAudio.ObserveBattleHistory` reads its actual battle map/area/current cue and bounded history,
including native attack/defence scene pushes. `ObserveBattleMusic` validates stable battle input,
route and admitted looping resources; it never calls the field-only observation. A changed native
previous stack replaces the last observation because battle scenes push music between player inputs;
a known current cue cannot disagree, and an existing logical generation/clock is retained. Finite,
fading, skipped, unknown or incomplete helper contexts do not become saveable by this observation.

Continue reuses asynchronous `LoadEntry`, the no-init definition reader and `SaveRecordRestore`.
Battle audio and the actual disabled candidate battle view prepare before releasing the old session;
failed preparation disposes the candidate and retains the previous owner. Mount does not Start or
Advance. Saved music starts at its beginning with a new generation and reconstructed history.
Cancel rebinds the old session's display catalog. Capture/store work runs detached while gameplay
callbacks remain frozen. Publication uncertainty survives a close request even if that request
discards the completed entry operation's result; another Save requires read-only Continue inspection.

The [native verification owner](../verification/battle-save.md) records real Save/return, owned
process exit/new PID Continue, exact pre-input semantic/RNG comparisons and matched subsequent
AI/round draws. All seven existing battle shapes mount/save/continue by the same mechanism.
Controlled focused Battle01 victory/defeat and admitted Battle02 Egress return to usable field,
save v1 and Continue repeatedly without repeating outcomes. Engine A/B evidence remains authoritative
for the wider state/codec/storage matrix; this is not a seven-battle natural campaign replay.
Physical focus/OS-close device acceptance and arbitrary candidate resource corruption are not newly
claimed by these headless observations. Exit warnings remain explicit in the verification owner.

## Evidence boundary and current consumers

**Confirmed, accepted original evidence:** the [battle interaction baseline](../godot/battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
records the USA manual and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`:
player Movement B returns the preview before free inspection; empty-ground options include Quit;
`BattlefieldMenu` excludes Battle00, confirms suspension, records elapsed seconds, sets flag88,
saves the selected slot and normally transfers to `WitchSuspend`. The battle loop has suspended
entry. Battle saving is an original feature. This design consumes that accepted evidence without
claiming a new original observation, physical power-loss guarantee or arbitrary-action snapshot.
The modern two-slot UI and the serialization/restoration boundaries here are separate decisions.

The current source responsibilities are the implemented A/B/C boundaries above. Field v1 remains
owned by `OpeningSave`/`OpeningRestore`; battle v2 by `BattleSave`/`BattleRestore`. Both use the same
strict envelope, typed record dispatch and atomic `LocalSaveStore`. The host consumes their
admission results rather than weakening a field predicate or inventing a last-safe snapshot.
## Stable commit and request UX

Application owns eligibility. The host may explain it and pause input; it cannot declare a safe
snapshot from an animation frame. Reuse session ID/revision checking for query → confirmation →
capture. All copied arrays/maps must be detached. Query/capture/validation must not advance gameplay,
RNG, region tests, simulation opportunities, actions or receipts.

| Live phase | Required request behavior |
| --- | --- |
| Settled Movement at committed origin | Allow only the predicate below; slot confirmation says “Continue resumes this actor's turn here.” Display encounter, round, actor, slot, character and timestamp. Save success resumes the same boundary; Save and Quit exits only after verified publication. |
| Nonzero movement preview | Refuse with “Cancel movement before saving.” Do not discard a preview as a side effect of Save. The ordinary Cancel path must finish its actual return delivery; then the player requests Save again. Moving back to origin is also eligible once the current preview is canonical zero-move and delivery has ended; no movement-history flag is needed. |
| Action, spell/item/equipment/Give/Discard or target/commit selection | Refuse with “Return to movement and cancel the move before saving.” Existing Back/Cancel semantics remain authoritative. Already committed equipment/discard changes survive cancellation; they are not an undo transaction. |
| Preview or undo delivery in flight | Refuse with “Wait for movement to finish.” A delivered final position is required, not just an empty command queue in the host. |
| Committed attack/heal/item, result/growth/drop/death, after-turn poison/sleep/silence | Refuse with “Finish this action before saving.” HP/payment/RNG/reward publications can precede the final delivery; never capture a partial scene or rerun it on load. |
| Automatic actor, round construction, appearance/region program, battle entry, retreat, victory/defeat return, story choice/wait, fault/Unsupported | Refuse with the specific busy/unavailable reason. No deferred save intent, timeout fast-forward or “save last safe state” fallback. Resume normally and request again at player input (or an existing eligible field savepoint). |
| Read-only inspection/options over eligible Movement | May open the same Save flow after ordinary undo has completed. The inspection cursor/map is not durable. Cancel returns through the real menu parent; Continue closes menus and focuses the acting unit. |
| Window-close or ordinary Quit | Keep explicit unsaved-progress warning and Cancel. A requested Save and Quit uses this eligibility and write result; failure stays in game. Explicit Quit without saving remains possible. Closing during an active publication waits for its result, as current entry operations do. |

The first implementation deliberately has **no pending-save queue**. Rejection leaves the same live
snapshot and choices intact, and dismissing its message does not deliver a gameplay confirmation.
Busy menus must allow returning to play; they cannot pause a scene forever while waiting for its
completion. Repeated Save, focus recovery, held controls and session replacement follow the existing
input-release and owner-lifetime rules. Only one save operation is active. Freeze simulation and
presentation callbacks that could mutate semantic state throughout the captured revision's flow.

The stable predicate extends, rather than reuses unchanged, `SessionSnapshot.HasBattleControl` in
[SessionContract](../../src/Sf2.Remake.Application/Runtime/SessionContract.cs):

1. ActiveBattle, `StopReason.PlayerInput`, no outcome, and the current queue entry is a living,
   placed, player-controlled actor that agrees with `Selection.Actor`. No terminal outcome already
   follows from the state. Automatic control and sleeping skipped entries are not player boundaries.
2. Selection is Movement with a fresh zero-move preview at that actor's authoritative position;
   no chosen action/target/item/equipment/exchange operands. Verify the preview/path, not just equality
   of endpoints. No Scene, Movement, Retreat, Round, Give or Discard continuation is retained.
3. Story has no cursor/callers/wait/event caller, defeated program, warp/script warp, entry skip,
   battle request or pending outcome return. A retained old `OutcomeReturn` value alone is not
   pending work: the current continuation determines that. Entry completed to `ProgramContinuation.FieldInput`;
   retain the connected `EnteringBattle` route and `FieldPlayer` binding (unlike field saves).
   Text/portrait windows and logical text are closed, no typewriting/scroll/fade work remains, and
   semantic display state is settled and admitted. Unknown required context is an error.
4. The current profile supports restoring the encounter, party, region/AI state, content resources
   and future outcome route without executing entry. This includes all state in the next section.
   Native motion and sprite delivery must have acknowledged their actual owned work before capture.

`BattleActionCommitter.Publish` is **not** itself this boundary. It can start defeated/death and
status continuations before `BattleTurnFlow.ConsumeEntry`; `BattleRoundContinuation` can publish
region flags and appearances before generating its queue. Inspect the actual final snapshot.

## State ownership and reconstruction proof

The DTO is a finite value contract, not serialization of `SessionSnapshot`/Godot objects. The table
identifies durable groups and their consuming code; implementation should reuse existing value
owners and avoid maintaining a second field-by-field schema inventory in prose.

| Authority and real later reader | Durable treatment / reconstruction obligation |
| --- | --- |
| [EngineBattleState](../../src/Sf2.Remake.Domain/Battles/State/EngineBattleState.cs); [BattleTurnFlow](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) `QueuedActor`, `ConsumeEntry`, `GenerateQueue` | Save encounter, round, the entire ordered queue including nullable-actor sentinel slots and altered agility, and cursor. Extra entries and dead entries are legal; do not deduplicate, AGI-sort, remove consumed entries or regenerate the round. Current actor is derived from queue/cursor and cross-checked, not a second authority. |
| BattleActorState; [BattleMovement](../../src/Sf2.Remake.Domain/Battles/Rules/BattleMovement.cs), [Sf2PhysicalAction](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), growth/status rules | Preserve every current encounter actor, including dead, unplaced, hidden/uninitialized and converted records: identity, position presence and coordinates, facing, current HP/MP/status, initialization, progress/accounting and full loadout. Live `Attack` must be retained/validated; difficulty initialization and growth/equipment can distinguish it from the immutable definition. Restore cannot respawn a dead enemy, refill MP, refresh status or rebuild from encounter starts. |
| [SourceEnemyAi.Resolve](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SourceEnemyAi.cs), [SourceMoveOrderAi](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SourceMoveOrderAi.cs) | Preserve ActivationWord, AiMemory, PrimaryOrder, SecondaryOrder and LastTarget with their present/absent meanings. Standby consumes ThinkingSeed and memory; dead-follow promotion changes orders; LastTarget affects pursuit radius. Immutable strategy/control/deployment/processing order rebind from the compatible content; SourceAllyControl also reads the live activation word. No AI decision is run during restore. |
| [BattleActivationRules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleActivationRules.cs), [BattleSpawnRules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleSpawnRules.cs), [BattleRoundContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRoundContinuation.cs) | Retain region Flags and Tested, activation/spawn state and story region-program once flags. Rebind region geometry/program definitions; do not re-test regions, populate, display appearances or set played flags while loading. |
| EngineBattleState seeds; turn generation, SourceEnemyAi, action/growth and [BattleAfterTurnContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAfterTurnContinuation.cs) | Preserve both complete uint MainSeed and ThinkingSeed, including zero/high/low bits. They are the gameplay RNG authority. No wall-clock reseed, startup growth, AI preflight or idle simulation on restore; historical observations/draw receipts are not state to replay. |
| [Sf2BattleProgressionRule](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2BattleProgressionRule.cs), [BattleActionCommitter](../../src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs), [BattleItemCustody](../../src/Sf2.Remake.Domain/Battles/State/BattleItemCustody.cs) | Save attained EXP/level/stats/spells, kills/defeats, gold, item words (including equipped/broken bits), drop once-flags and Deals counts as already committed. Preserve DeathCredit's unknown versus known-no-ally versus known-ally distinction: it survives turns without a scene and controls later death accounting. Do not infer it from the acting unit. Completed reward messages are not pending rewards. |
| [BattleEntry](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs), [BattleOutcome](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs) | Retain connected route identity, FieldPlayer identity/speed and encounter-excluded RetainedAllies, story flags/names, egress register, step counter and live start-policy gameplay options. Rebind route programs, battle origin and return definitions from admitted content. BattleOutcome concatenates retained allies; return selects live flags/egress and first active member's position/facing. Do not choose a route by encounter alone where ambiguous, recompute player identity, discard nondeployed members or perform healing/gold-halving/join/completion on load. |
| [StoryState](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs), ProgramRunner and text/music consumers | Retain TextCursor, TextSettings, settled display/palette operands, semantic speaker/service/window-fix state and known music cue/history/context where later commands read them. Preserve explicit unknown/presence states when legal; refuse a required unknown rather than turning it into zero/false/empty. PartyLists may rebuild from flags only after equality with the live lists is checked; a mismatch rejects capture. SaveAnchor remains an optional durable prior field anchor; ReturnAnchor/OutcomeReturn may be omitted only under the admitted no-pending-return invariant and their overwrite-before-read recipe. |
| [BattleVisualState](../../src/Sf2.Remake.Domain/Battles/State/BattleVisualState.cs), [BattleStoryOperations.Visual/Convert](../../src/Sf2.Remake.Application/Runtime/Battles/BattleStoryOperations.cs) | These are semantic physical-slot identities, not disposable nodes. Preserve admitted aliases and visual records (appearance/position/facing); conversion can share a slot and leave an old physical record. Rebuilding every alias or pose from logical actor position is invalid. Definitions/resources rebind; Godot nodes, sprite handles and animation clocks rebuild. A recipe without such state must prove it is unneeded rather than fabricate mappings. |
| [BattleMovementContinuation](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMovementContinuation.cs), selection queries, host view | Provisional selection, target lists, movement grid/path, camera, menu focus, inspection cursor and input latches are rebuilt at the saved actor's origin. Preserve committed equipment/discard effects. Use the pure selection/range construction, not `BattleAdvancer.Start/Advance` to arrive at a convenient boundary. |

Retained nondeployed allies use the existing complete carried-party representation, including absence
and initialization; the battle actor set is not the entire campaign party. Reuse existing loadout,
item identity and learned-spell validators where their assumptions apply. The field `SavedAlly`
validator requires initialized growth-bearing allies; applying it unchanged to enemies or hidden
actors would reject valid states or invent defaults. New battle admission must validate those actual
roles, negative/out-of-board coordinates, permitted absence and accounting knowledge explicitly.

Session ID, revision, observation sequence, wait tokens, process/native resource handles and input
state are fresh after restore. They authorize one process, not saved gameplay. No pending wait/token
is allowed in this profile. `SimulationTick` and the old text-poll `RandomSeedCopy` have no independent
future gameplay authority at this seam (live seed copy projects ThinkingSeed); keep them out of
semantic equality and never use them to seed RNG. Later consumer changes must revisit that claim.

Music is not automatically disposable: [ExplorationMusicRunner](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationMusicRunner.cs)
reads history, context and finite completion state. First battle admission permits a known ordinary
looping battle cue/history and no pending finite music helper; bind native knowledge through a
battle-context counterpart of the existing observation, without drawing RNG. Rebuild native playback
at its beginning with a fresh generation. Do not call field-only `ObserveFieldMusic` on ActiveBattle.
A finite cue whose logical step/completion can affect future waits is outside this first stable
profile and rejects explicitly; admitting it requires preservation of those semantic operands and
an equivalence check, not copying the field-save policy of resetting all clocks. PCM position and
cosmetic phase are not promised. Post-battle field cue selection remains its existing owner.

## Format and detached restoration

Recommend a **version2 battle record**, with an explicit kind `battle-player-input`, under the
existing checksum envelope and slot directory. Keep the v1 field record/parser/writer unchanged;
new code reads both supported versions, dispatches to one typed payload, and validates before use.
Do not add nullable battle fields to `OpeningSaveData`, export arbitrary CLR type names, or store
content paths. Share reusable party values/validation only where semantics match. Proposed names
here describe responsibilities; no JSON schema or production file is created by this design.

Version2 retains format/slot/sequence/UTC timestamp plus the existing profile/package, gameplay,
rule and recipe compatibility identities and pinned source identities. Add a battle restoration
recipe revision, separate from v1 field recipe interpretation. The configured source supplies
trusted content and rules; a save cannot select an executable, file, private root or alternate rule.
Use the existing revision mechanisms, not a new manifest or content hashes for change detection.
Adding the battle format alone must not change v1's `SaveCompatibility` values. A genuine shared
gameplay/content change still follows explicit compatibility rejection; this design adds no migration.
Do not set original suspend flag88 to route through `BattleEntry`: that entry currently rejects it,
and executing it would initialize a new battle rather than restore this one.

Use explicit finite presence variants for nullable/sentinel values (position, queue actor,
DeathCredit and legitimately unknown optional operands). The strict JSON reader rejects null today;
these variants must preserve meaning without relaxing v1 or accepting arbitrary missing keys.
Unknown required transaction/return/AI data rejects. Reject unsupported version/kind, incompatible
content/rules, missing or extra keys, duplicate identities, bad bounds and cross-field conflicts.
The checksum detects accidental corruption, not authenticity or semantic validity.

A complete restoration transaction is:

1. Read the selected slot under current byte/depth limits; validate envelope, version and header.
   Admit independently configured content with `OpeningRestoreReader`'s no-party-initialization
   approach. Resolve exact encounter and route by stable content-owned identity (e.g. owning map
   plus route key/ordinal under gameplay revision); require uniqueness and encounter agreement.
2. Validate actor identity/roles, legal progress/loadout/status, positions, region arrays, queue
   shape/sentinel/cursor and the actual current actor. Allow repeated extra-turn actors and consumed
   dead entries. Validate membership, retained-party partition, custody, options and return resources.
   Rebind immutable definitions/rules without invoking start initialization. Restore StartPolicy's
   gameplay operands needed by future spawn/control; diagnostic provenance is not a gameplay default.
3. Construct a detached EngineBattleState and connected StoryState, with the saved queue/seeds and
   a fresh Movement selection. Build visual/audio resources and validate the same stable predicate.
   Compare the captured semantic values with the reconstructed candidate before offering Save.
   No entry/start/load program, round generator, `BattleOutcome.Begin`, action preparation, region
   test or reward consumer runs merely to mount. In particular, no `GameSession.Start(saved start)`.
4. Publish the candidate session/view only after validation and required resource preparation pass.
   [GameRoot.MountSession](../../game/src/GameRoot.cs) already prepares candidate audio before
   releasing the old session; extend the correct battle mount path and its failure cleanup. Until
   success the old session remains paused and usable. Discard old intents/callbacks and require
   input release. Initial rendering must not tick automatic work past the saved actor.

Neither a valid checksum nor data that could produce a *new* battle proves valid continuation.
Check saved invariants without executing future actions speculatively. If valid original-live state
cannot pass the proposed restore validator, correct the owning validator/contract; do not normalize
the save until it resembles a favored test case.

## Publication, failure, compatibility and rollback

Reuse the existing per-slot lock, detached validation, bounded serialization, flush, temporary
readback, atomic replace/move and final readback. Generalize only the record/validator boundary in
LocalSaveStore so v1 field and v2 battle records share it, including backups of a different supported
kind/version. Sequence remains monotonically selected from compatible primary/backup under the lock.
Do not duplicate the filesystem transaction in a battle-specific store.

| Failure or compatibility case | Required behavior |
| --- | --- |
| Busy/stale/unknown or candidate validation/resource failure | No disk publication and no live-session replacement. Explain reason; preserve current battle, preview and last good files. |
| Write/flush/temp reread failure before publication | Keep primary/backup intact; permit an explicit later retry from a freshly checked revision. No success/clean marker and no automatic exit. |
| Replacement attempted, exception or failed final readback | Preserve `save-publication-uncertain`; no blind retry or claim that the old primary survived. Keep session paused, offer read-only Continue/slot inspection and Cancel; reconcile the actual slot before another write. |
| Process interruption | Before publication: last good primary wins; leftover temp is never Continue input. After publication: validate actual primary; if missing/corrupt offer the compatible backup explicitly. Do not infer commit from temp/mtime alone. No universal disk-controller/power-loss guarantee is claimed. |
| Corrupt primary, valid compatible backup | Show backup kind/destination/actor/time and require explicit recovery. Reuse Recover's temp/replace path preserving backup bytes; never rotate corrupt primary over the last good backup. |
| Incompatible/future-version primary or unavailable required content | Preserve files and report matching build/content required. Do not silently load an older backup, downgrade to field return, restart battle, delete a slot or overwrite it to unblock Save. Retain current recovery exclusions. |
| Existing v1 field/new-game save, including absent names | Continue through unchanged v1 semantics; do not reinterpret it as a battle or rewrite on read. New field saves stay v1. |
| Old executable sees v2 primary | It rejects the unsupported record; supported new code can still read the prior v1 backup. The existing reader deserializes v1 before checking version, so a differently shaped v2 can report `save-malformed` and expose old backup recovery instead of `save-version`. Do not use that old recovery to overwrite v2 during rollback. Preserve user-owned bytes locally before operator-directed rollback; no automatic downgrade. New version dispatch must classify unsupported versions before typed payload parsing. |

Overwrite consent remains explicit, including replacing a field save with a battle save. Mark saved
session/revision only after verified publication; after mounting Continue mark the new session's
revision, not the serialized old identity. If capture succeeds but host mount fails, report the
mount failure separately from the disk result. Repeated Continue restores the same progress; it
must not append rewards, charges or new save sequences merely by reading. No game-state rollback
is performed when Cancel closes a save UI.

## Minimum implementation acceptance

Use small tests of actual Domain/Application behavior and direct codec/store invocations. Do not
add tests of probes, comparison drivers or report generators. Native checks read state/input/view
projection/errors, never screenshots. Use disposable slot namespaces outside removable worktrees;
registered immutable content is read-only. Existing user primary/backup and retained diagnostic
processes are not test inputs. The following is the integrated required acceptance matrix.
A/B engine and direct-store checks are recorded above; C's actual native coverage and explicit
limits are recorded in the linked verification owner. This matrix remains the acceptance boundary,
not a claim that every failure was observed on physical hardware.

| Group / minimum representative seam | Pass condition |
| --- | --- |
| Eligibility and purity | Engine behavior: query/capture at ordinary Movement; nonzero preview versus moving back to a canonical origin preview; Action/Target/Commit; undo in flight; scene/reward/status; AI; appearance/region program; outcome/retreat; fault; stale ID/revision. Only the stable predicate succeeds; rejected requests preserve all state and seeds. Capture is detached and repeatable. Include already committed non-turn equipment/discard where that capability is admitted. |
| Exact continuation in a new process | Save after at least one committed action, with a partially consumed queue, damaged units and consumed MP/item. Terminate the owned disposable process, launch the same configured content in a genuinely fresh process and Continue through the normal slot UI. Before first input compare semantic state and both uint seeds. Apply the same legal command sequence and presentation completions to live-control and restored branches: compare next AI decision/action/resource results, queue consumption and next-round draws. Session IDs/revisions/tokens and cosmetic clocks are intentionally different. |
| Alternate valid state | Repeat a small engine continuation with a different actor/order and zero/high-bit seed values, status/AI memory/promoted order changes and an absent/dead actor. Exercise a region activation/appearance once-flag and semantic visual alias state with an existing small authored case where admitted; this does not claim those diagnostics are product save profiles. No fixed reference trajectory as legality. |
| Rewards and accounting | Save at the next stable player input after a kill/drop/EXP or growth award, then Continue twice independently. Each restore starts with exactly the saved amounts/flags/items/kills/defeats; later actions award only new rewards. Include poison/death-credit continuation after a no-scene turn and a retained nondeployed ally. No completed message/action is replayed. |
| Victory, defeat and retreat return | From one saved connected boundary, run separate short legal endings for victory, ordinary defeat and admitted Egress/Angel Wing. Compare positions/facing/map, leader recovery, gold payment/halving, roster/join/once flags, carried inventory and next field usability with the corresponding uninterrupted branch. Save/Continue the resulting supported field recipe and confirm no duplicate outcome processing. Use a focused suffix, not the whole campaign. |
| Strict data and old files | Directly invoke real codec/store/restore with disposable v1 field/new-game (with/without names), valid v2, wrong version/profile/revision/route, checksum corruption, truncated/oversize data, missing/duplicate/unknown keys, bad queue/cursor, mismatched membership, invalid loadout and required unknown. No failed candidate is mounted or silently defaulted; mixed v1/v2 backup rotation remains valid. |
| Storage and cancellation | Direct bounded failures at pre-publication and post-attempt/readback, lock contention and missing/corrupt primary with valid backup. Inspect actual bytes/sequence and recovery through the real consumer. Use a temporary path/controlled I/O seam only if needed to reach the particular failure; no general fault platform. Test Cancel, rejected overwrite, duplicate confirmation, focus loss, window-close and session replacement; exit only after the selected successful Save and Quit. |
| Actual host mounting | Native ordinary input → Save and return; Save and Quit → fresh Continue. Correct encounter/round/actor slot label, matching board/resources, usable Movement, music, no stale token/held-input action and no engine/presentation/audio errors. After busy refusal, closing the message resumes the pending action normally. |

Acceptance evidence names the saved boundary, content/rule identity, semantic differences, actual
process termination/restart and command results. Same-process DTO roundtrip cannot substitute for
fresh-process proof. Original hardware timing, exact PCM resume and full-game coverage remain
Unknown/outside the claim. A required failure case not reached is recorded as unavailable, not PASS.

## Bounded implementation ownership and resources

The serial slices follow; main-gate creates implementation Issues after design review.
Every slice starts from accepted main. Do not dispatch overlapping writers in the shared session,
store, entry or battle input files. Consume the accepted movement/camera and handoff boundary in
[#1607's presentation owner](../godot/presentation.md#ordinary-movement-camera-continuity-1607).
That accepted change does not alter persistence semantics; no in-flight #1607 dependency remains.
Inspection implementation [#1633](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1633)
consumes the accepted #1622 baseline and depends on #1615/#1617. Its options surface can later host
Quit, but inspection is not a persistence dependency: host Menu can expose the same safe Save/Save
and Quit first. No unrelated shared navigation expansion is needed.

| Slice / accountable code owner | Bounded output and stopping acceptance | Estimate |
| --- | --- | --- |
| A: Domain/Application continuation | Existing `Domain/Battles/State`, narrow restore admission beside it, `Application/Runtime/Persistence`, `GameSession` facade and actual engine behavior tests. Add typed battle progress, stable eligibility and detached reconstruction with reader-backed party/route/AI/queue/visual invariants. Reuse pure movement selection. No disk/UI availability yet. Stop at equivalent next actions/RNG and connected return behavior on small engine cases. | 2–4 engineer-days |
| B: Content and shared store | `Content/Persistence/OpeningSaveJson` dispatch plus a bounded battle codec, existing Content compatibility admission and `game/src/Persistence/LocalSaveStore`. Preserve v1 and generalize the existing store's record boundary. Directly validate strict/mixed-version/corrupt/interrupted files. No ordinary Save option until host integration passes. Depends on accepted A. | 1–2 days |
| C: Host and integrated acceptance | `game/src/Entry/OpeningPersistenceHost`, existing entry loading/mount/audio and system/battle-menu consumers, localized save labels; narrow Application music-observation hook if needed. Preserve the accepted movement/handoff boundary and serialize ownership with active inspection/input work. Native safe/busy UX, fresh-process Continue, outcome suffixes, disposable failure checks; update only affected capability/evidence owners when implemented. Depends on accepted A/B. | 2–3 days |

The inferred total is **5–9 engineer-days**, including focused verification and independent review;
this is an estimate, not an approved spending cap or a reason to omit a correctness boundary. Highest
risk is return/retained-party completeness, unknown AI/death/visual state, then mixed-version storage
and host callback lifetime. Reassess after A's first Battle01 continuation and before exposing C.
If a required reader needs a live interpreter stack, an unsupported finite music continuation or
new gameplay content, stop that profile's admission, record the precise dependency and replan;
do not expand to arbitrary snapshots or hide a gap behind another default.

Expected state is tens of actors, one 64-slot queue and small region/custody/story sets. Payload
copy/encoding is O(actors + queue + flags + retained progress); current cross-reference checks can
scan the bounded actor/content sets repeatedly. Reuse existing lookups and measure rather than
claim linear end-to-end validation. Content admission/resource preparation is additional work;
no history/frames/traces are retained. Keep the 1 MiB encoded-file
cap. One save uses at most primary + backup + one unpublished temp (at most 3 MiB per active slot),
with bounded encoded/decoded/candidate copies in memory; configured content dominates memory.
Two existing slots plus one active temp remain a small fixed disk budget. No new network/model/API
cost or content duplication is required. Measure one representative serialized battle and fresh
restore latency/memory in A/B; target comfortably below the cap (review if over 256 KiB or if saving
visibly blocks input). Use existing detached entry work if resource preparation needs it, never a
background mutation of the live session. A cap increase or repeated resource failures require
review of actual payload/consumer costs, not a new compression/cache subsystem by default.

Release only when v1 compatibility and the vertical restart/return matrix pass. If a later slice
fails, keep battle saving unavailable while field v1 remains usable, and preserve evidence/files.
Reverting code must not delete or reinterpret v2 files; the compatibility row above governs recovery.
Keep new handwritten responsibilities below the repository's source-size limit, extracting only a
touched oversized responsibility where necessary. A uses scoped engine behavior verification,
adapter compilation for the shared facade/return consumer, direct private profile admission, a clean
committed planner and actual CI disposition. Documentation-only corrections use direct source/link,
scope and private-boundary inspection; they do not select research or native suites by themselves.
