# Caravan landing through Creed: engine and save audit

**Decision: retain the current engine and typed story programs. Repair A1's audio selection before
expanding ordinary Stage2 play; make A2's live-room persistence part of the first mine slice.**
The accepted bilingual Stage1 histories remain valid. No evidence here calls for a general script
VM, save framework, engine rewrite or more audio capture. A3 concerns older readers of populated
Creed scene saves, not current-version Continue or omitted scene fields. A4 concerns documentation
routing. Main-gate owns repair dispatch and independent integration; this audit neither implements
fixes nor starts Stage2.

## Agreement and evidence boundary

High rigor under [#1885](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1885),
parent [#1881](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1881) and the
[campaign agreement](../application/remaining-campaign-risk-baseline.md): membership, source
aliases, room progress and save compatibility outlive a chapter. The decision is whether to reuse
the current boundaries from Caravan landing through Creed and what must precede their next consumer.
Owned paths are this report and the short stage conclusion/navigation in that agreement. Production,
tests, research, schemas, global indexes and other chapter owners are read-only.

Inspected accepted commit **`f3b1e3847309aed0de59e9bcac0c08fc9c8e55a3`**, tree
**`07c2bdb68692edb1c014dabdc0b6ed52aeb5d1ee`**, including necessary current callers and earlier
audit dispositions. Reproduce source statements with `git show f3b1e384:<named-path>`; use
`git show f3b1e384 -- <path>` for recruitment additions. **Confirmed** below means inspected source,
the explicitly repeated offline comparison, or the named retained observation at its own boundary.
Reading a test establishes its assertions, not a new test PASS. **Inferred** marks consequences not
executed here; **Unknown** remains unobserved.

Original behavior comes from the linked research owners, pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` and their registered USA ROM identity. Remake code and
native reports do not prove original natural reach, hardware timing, acoustic or human visual fidelity.
The [recruitment owner](../application/creed-recruitment.md), accepted
[PR1886](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1886), registered
`handoffs/issue1876/HANDOFF.md` and `ROOT-REVIEW.md` locate retained ordinary observations.
Adjacent #1870–1875 results remain owned by the service/restoration/consultation documents below.

## Findings and disposition

### A1 — P1: additive finite audio can disable a previously usable context

Current repair and bounded acceptance: [explicit finite defaults (#1888)](../godot/audio.md#explicit-finite-defaults).
The original finding and failed observations below remain historical evidence.

**Confirmed.** [SessionAudio.Select](../../game/src/Audio/SessionAudio.cs) selects an exact
command/current-music Timer B pair; without one it accepts only a unique finite candidate for that
command. Adding a second finite variant changes a previously successful fallback into
`audio-command-ambiguous`. A third exact variant restores only that timer. This is a resource-selection
regression mechanism, not evidence that every sound needs another recording under every song.

The actual retained `en-fight04/audio-failure.json` has `error=audio-command-ambiguous`,
`musicCue=MUSIC_MAIN_THEME`, `timerB=194`. Receipts show ATTACK/198, hit83/198, then restored
MAIN_THEME/194 before the death116 request. `en-fight03/04` are failed observations; `en-fight05`
passes after admitting116/194. This audit independently read the failure fields and receipt order;
it did not reenact combat. The current accepted recording fixes that reached case. Other unmatched
timers still take the ambiguous branch whenever a command has multiple finite candidates.

Impact exceeds a missing cosmetic tail: `PlayEffect` latches `Error`, and
[ExplorationPresentation.Update](../../game/src/Exploration/ExplorationPresentation.cs) returns
without further completion delivery while that error is present. Named `Play(resource)` also calls
the selector on a timer mismatch. Changing only the integer-command caller is insufficient.
Which additional future ordinary routes hit an unmatched timer is **Unknown**, not a capture queue.

**Owner / minimum repair.** The [audio contract](../godot/audio.md), `SessionAudio.Select/Play`,
[ExplorationAudio](../../src/Sf2.Remake.Application/Content/Scenarios/ExplorationDefinition.cs),
[ExplorationAssetReader](../../src/Sf2.Remake.Content/Scenarios/ExplorationAssetReader.cs)
and the existing audio producer admission own selection. Add one optional explicit fallback designation
on an existing finite resource for each command needing fallback. Validate at most one designated
finite fallback per command, a real same-command cue and no looping fallback. Retain exact match
first, then the designated fallback, then legacy unique-finite behavior for an old package with no
designation. Missing/ambiguous undeclared content remains an error. Do not choose by dictionary
order, infer pitch, borrow another command or overwrite the recorded Timer B. Apply the same
decision to named playback; keep actual and requested timers separate in receipts. This needs no
new assets, acquisition service, cache or save migration. Defaults are remake presentation choices
over existing PCM, not newly confirmed source-driver behavior.

**Acceptance / stop condition.** Exercise the actual selector's exact, absent-exact/default, old
unique, absent, duplicate-default and ambiguous-without-default cases. Add an unrelated timer
variant and reverse enumeration order: the declared fallback must remain stable. Consume the
existing death seam and a different unmatched-timer command through the real adapter with existing
audio; verify completion, receipts and subsequent input, including named cue playback. Keep old
cue IDs, PCM, music/history and slot Continue intact. Use small behavior checks and a bounded
observation, not tests of an observer or a sound×Timer B sweep. **Stage2 ordinary expansion should
wait for this correction.** HEADQUARTERS39 was a genuinely new reached music track;116/194 was an
additional recording used to escape selector ambiguity. Treat those costs differently.

### A2 — P2: the next mine needs live-room persistence, not a wider Creed whitelist

**Confirmed boundary, not a defect in accepted Stage1.**
[Sf2CreedRestore](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2CreedRestore.cs) captures only
Map33 with F784/785/786/787 and admits the F784/F786 room definitions. It retains the actually loaded
population, layout deltas, physical actors, aliases, current list and following. Its local graph
requires unique current-list slots, no follow cycle and no leader on physical0. Those are finite
Creed admission conditions, not universal original-game invariants.

The next [Map30 source owner](../../../docs/research/chapters/southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md#complete-map30-population-and-init)
already provides a concrete second consumer. After a successful F761 gift, the live F785 room still
has five dwarves and the digging scene's 2×2 layout copy. Fresh F761 entry has no residents and init
also performs a 4×4 copy. Rebuilding solely from flags would change the saved room and interactions.
With the accepted F68-clear/F67+F69 history, absent FOLLOWER_B31 aliases physical0; recovery's
B→Peter request can affect Bowie and append physical0. With F68 set, B is distinct. Do not reject
or normalize that source alias merely because the Creed snapshot disallows it. These are static
source consumers; completed remake mine execution remains **Unknown**.

**Owner / minimum extension.** The Stage2 Map30 slice under #1818 owns its recipe and acceptance;
[Sf2OpeningRestore](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs),
`Sf2CreedRestore` and [SavedFieldScene](../../src/Sf2.Remake.Application/Runtime/Persistence/SavedFieldScene.cs)
are persistence dependencies. Reuse the payload where it expresses settled progress, but keep
context-specific admissibility in SF2 policy. Extract only the responsibility both real consumers
need. Do not simply add `map-30` to Creed checks or store executable continuations. Determine the
source state at the intended settled save boundary, including whether physical0 still has a follow
relation; the earlier request alone does not prove its final state. No new capture is needed to
establish the live/fresh distinction.

**Acceptance / stop condition.** Before accepting a Map30 post-gift save, compare live save→cold
Continue with a separate exit→fresh reentry: each must preserve its own population/layout semantics.
Cover full Bowie pack (no gift/F761, but zone F760/set and F69/clear), later retry through resident128
and successful custody; retain the source helper's absence of a universal F761 one-shot guard.
Check inherited F68-clear aliasing and a changed F68-set state at their actual continuation boundary.
Retain old Creed saves unchanged. **This belongs in the first Stage2 deliverable, not a general
all-map persistence project or a blocker to unrelated design.**

### A3 — P2: older readers classify a new optional save field as corruption

**Confirmed source classification and payload shape; Inferred user-facing loss path.**
[OpeningSaveData.FieldScene](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSaveData.cs)
uses `JsonIgnore(WhenWritingNull)`: normal field-v1 writes omit the property when no scene is captured.
Independent decoding of the retained English Fairy and bilingual recruited Map33 slots found
populated `fieldScene` objects; both languages' outside Map67 and mine-approach Map68 slots omit it.
The older-reader risk below applies to the populated Creed records. At pre-recruit accepted `8b559f14`,
[OpeningSaveJson](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs) has no such property,
disallows unmapped members and translates the resulting `JsonException` to `save-malformed` before
compatibility validation. The current reader has the same general classification for future
unknown fields. [SlotRead.CanRecover / LocalSaveStore.Recover](../../game/src/Persistence/LocalSaveStore.cs)
permit `save-malformed` recovery and replace the primary with a valid older backup, preserving the
backup but not the superseded primary. Reopening the same namespace with an old binary can therefore
offer corruption recovery for valid newer progress. This needs an old readable backup and explicit
user recovery; it is not automatic current-version data loss. Explicit `fieldScene: null` is not a
normal writer output: current `CheckKeys` rejects that constructed input as `save-null-value`.
No recovery was executed on user slots.

**Owner / minimum correction.** Content save-envelope classification and Godot slot recovery, owned
by [Save/Continue](../application/battle-save-and-continue.md), must distinguish unsupported shape
from damaged bytes before offering destructive repair. For future incompatible writes, use the
existing early format/kind protection when required; a later recipe revision alone cannot protect
an older reader that fails deserialization first. Separately make future unknown-shape diagnostics
nonrecoverable in the maintained reader. Already shipped old binaries cannot be retroactively fixed:
keep their namespaces separate and preserve original slots when publishing or reverting packages.
Backward loading by old binaries is not promised; preserving valid newer bytes is a distinct concern.

**Acceptance / disposition.** In disposable slots, pair an old readable backup with an actual newer
Creed primary containing a populated `fieldScene`. Use actual outside/mine records with the property
omitted as wire-shape controls, keeping any separate roster/recipe compatibility rejection distinct.
Also inspect a deliberately constructed non-null unknown future field and a future format version;
these are synthetic compatibility cases, not current writer outputs. Inspect old/current diagnostics
and recovery availability. Valid unsupported primary bytes must survive; real checksum corruption
may still offer explicit backup recovery. **Nonblocking for Stage2 in isolated forward-only
namespaces; fix before supporting shared-namespace binary rollback
or distributing that recovery promise.** No new migration database is needed.

### A4 — P3: navigation and scope prose lag the accepted consumer

**Confirmed.** [Architecture](../architecture.md) still describes the connected campaign through
Battle13; the [recruitment chapter](../application/creed-recruitment.md) calls itself a candidate
requiring independent acceptance, although PR1886 is merged. The bottom of
[Caravan services](../application/caravan-services-design.md#acceptance-and-reproduction-boundary)
excludes transport UI access and calls the future Creed service route Unknown, while its current
section records the accepted actual vehicle caller. These can misroute a later executor into
repeating completed work; they do not undo the evidence.

Owners are those documents and [capability-status](../capability-status.md#campaign-feature-trace).
Minimum correction is concise current-scope wording and links, without milestone histories or
duplicated requirements. Check chapter/feature/architecture links against the accepted PR and this
audit's limits. This audit changes only its declared stage navigation; main-gate should assign the
other shared owners together. **Nonblocking; prose does not require engine/build/runtime gates.**

## Reviewed boundaries without a new blocking finding

The following paths are relative to `remake/src/` unless named otherwise. `Application`, `Content`
and `Domain` abbreviate `Sf2.Remake.Application`, `Sf2.Remake.Content` and `Sf2.Remake.Domain`.
Conclusions follow actual callers and changed states, not an identifier scan or assumed test success.

| Boundary and actual owner | Assessment and variation reviewed |
| --- | --- |
| Chapter conditions and publication | Source selection/compiler produces F784 consultation, four offers, join-before-F787, Fairy and Oddler callers. `Application/Runtime/Exploration/ProgramRunner.cs` and `Runtime/GameSession.cs` retain one snapshot publication path. Chapter IDs/flags in `Application/Gameplay/Sf2/Sf2OpeningRestore.cs` are selected-game policy; codec/Godot invent no recruitment prerequisite. `CreedRecruitmentTests` varies four choices, No/Yes,10 versus12 active and controlled F22. F787/F22 refusal coverage remains controlled. |
| Joined / deployed / following | `Domain/Maps/MapPartyMembership.cs`, Application `CaravanService.cs`, `SceneEntities.cs` and `FollowerMotion.cs` separate authorities. A thirteenth joined member stays reserve; current-room following does not deploy it. Service publication rebuilds party lists and active actors while retaining resources/custody. `CaravanServiceTests` covers different relief members, dead confirmation, Bowie refusal, stale commands and later callback failure. No duplicate join/reward authority found. |
| Depot and Item transactions | `Domain/Battles/Rules/CaravanInventory.cs` prepares immutable pack/custody before publication; Application `CaravanDepotService.cs` checks index/word/capacity/curse; `CaravanItemService.cs` reuses `FieldInventory/FieldRecovery` with an explicit caller. Actual engine assertions cover full/self-Give, exchange, cancel, ordered duplicates, rare Deals saturation and unsupported Use without consumption. `BattleItemCustody`, field/battle wires and continuation retain depot. Absent old depot means empty only in known old formats. Greeting/portrait/return may consume main RNG: resource preservation is not whole-session RNG equality across elapsed presentation. |
| Vehicle caller and return | `Application/Runtime/Exploration/CaravanRuntime.ServiceTarget` uses walking/overworld population, visible physical1 sprite62 and overlap; `ExplorationDispatcher` starts `CaravanPrograms.Service` with field-action return. The vehicle stays while actors shrink/park/grow and camera/following/music restore. `CaravanWorldServiceTests` varies sprite, visibility and overlap. Ordinary full12 relief now uses this caller. #1870–1872 acceptance used Map22/26 NPC callers and existing allies; this caller/new palettes were explicit later chapter dependencies, not proven by those earlier services. |
| Current actor/class/equipment appearance | `Application/Content/Scenarios/BattleSceneDefinition.TryAllyVisual` reads current actor/class/equipped item; optional actor rows precede class fallback. Content `BattleSceneContentReader` binds source members; `PrivateWorldEncounterReader.CampaignScenes` shares immutable frames across selected encounters. `BattleSceneAllyAppearanceTests` varies identity, class, armed/unarmed, missing weapon and legacy null bindings. The producer includes initial/admitted equipable weapons; host hides weapon drawing when unarmed. Retained Eric uses ally2-palette2/weapon75. Missing combinations refuse; promotion/new classes still need content acceptance, not join-loadout caching. |
| Shared graphics / palette / loops | `ProgramRunner` changes slot resources without replacing actors; `PaletteColorTransition` owns BASE/COPY and interpolation; Godot palette view projects it. `EntityActionRunner` skips X≥0x7000 before motion/actions. Repository `src/sf2tool/remake_exploration_actions.py:cyclic_entry_actions` lowers the supported shared backward cycle from each entry; named source-family admission is finite. `SharedSceneGraphicsTests` varies slots, hidden/reactivated loops, partial COPY restore and new-palette versus CURRENT255 passage. #1875 retains eight four-destination orbits,15 graphics requests and exact18-program comparison. New branch shapes need explicit admission, not silent truncation; no fixed itinerary selects engine behavior. |
| Size/layer and old roster | `Application/Runtime/Persistence/OpeningSave` rejects busy/riding/unsettled states; layer0/2 and actor render sizes are separate payloads. `LayeredFieldSaveTests` varies offsets/positions and invalid anchors. `Gameplay/Sf2/Sf2HistoricalRoster` revisions for7/9/10/11/12/16 records initialize missing unjoined actors, replay per-expansion growth seeds and preserve existing resources/live RNG. Legacy recipe1 Battle23 is recognized from its twelve-member roster. `HistoricalSaveRosterTests` covers that old battle and direct/staged upgrades; no arbitrary-roster or SRAM promise follows. |
| Finite current-room Continue | `OpeningSave.Query` rebuilds a detached candidate before offering Save; `OpeningRestore` passes optional `fieldScene` to SF2 policy. Map33 F785/786/787 without it rejects; old pre-recruit records rebuild without it. The writer omits null scene values; actual outside/mine records have no scene property, while the inspected Creed records contain objects. Fairy130 in the loaded F784 room, later Fairy158 and Oddler130 on fresh F786 entry remain distinct. Settled scene progress restores without replaying rewards; ordinary ambient actions restart. F22 and unfinished restoration remain excluded. A2 prevents promoting this finite success to all-map persistence. |

Application service/action filenames above are under `Sf2.Remake.Application/Runtime/Exploration/`;
Content scene readers are under `Sf2.Remake.Content/Scenarios/`; tests are under
[`remake/tests/Sf2.Remake.Engine.Tests`](../../tests/Sf2.Remake.Engine.Tests).
The named assertions and retained chapter observations in [Caravan services](../application/caravan-services-design.md),
[restoration](../application/creed-restoration.md), [consultation](../application/evil-spirit-consultation.md)
and [recruitment](../application/creed-recruitment.md) bound these conclusions. They are not exhaustive
fuzzing, all-opcode conformance, every weapon/class combination or a new full journey.

## Independent checks, resources and retained limits

This audit repeated decoded-data comparison for all16 pairs in registered
`issue1876/cold-comparison.json`: fourteen equal; both reentry pairs differ only in `mainSeed` and
`fieldScene`, matching their retained150-tick ambient interval. This checks existing bytes, not a
new cold process. It also independently extracted A1's audio error/context. Local
`issue1885/check_receipts.py` and `receipt-check.json` retain the exact read-only procedure/results;
`issue1885/field-scene-presence-review.json` records the seven decoded payload-shape checks in A3.
Inputs are the registered handoff, never copied worlds or modified slots. Load the checkout's
ignored private-input configuration and run that script with its existing Python environment.

Earlier findings were reconciled against current source: [the Polca audit](polca-raft-engine-save-audit.zh-CN.md)
#1749/#1750/#1751/#1752 corrections cover roster migration, raft berth, birdman AI, carried enemy
items and recovery policy; [engine/script review](engine-script-boundary-review.md) records F6–F8's
admitted-defeat, follower-list and shared music-context fixes. Current source retains those mechanisms.
A1 is multiplicity selection, not a reopened view-area or audio-lifetime finding. Stage1's secondary
layer save correction is present. No duplicate repair ticket is proposed.

Preserve #1876's completed2782-pass/45-skip/3-failure aggregate and its exact three obsolete
`OpeningRestoreTests.MansionSaveRebuildsFirstVisitWithoutReplayingStory` cases; affected108-case
correction and exact-head CI are separate passing evidence. Do not rerun the aggregate to erase it.
Private stable Play language/recruit-override contamination was corrected and independently rechecked
without a product commit. The whole-service RNG equality assumption was corrected, not an expected
value weakened. Navigation failures, defeat and audio failures remain distinct.

**Unknown / excluded:** post-consultation physical departure without recruitment is still unproved;
earlier mansion departure is not that witness. No F787 gate follows from obstructed navigation.
Later natural F22, Rick, Fairy Woods and F762 village services remain separately owned optional
content. #1716's native finalizer/heap diagnosis, ObjectDB warnings and public-release risk stay
open; this audit runs no dump, Godot change or shutdown experiment. Human art/experience, original
natural timing and full-campaign/credits acceptance remain outside Stage1.

Resource choice is bounded text/source review and small JSON: linear time in inspected bytes,
one small receipt/pair at a time, below2 MiB new output. Entry disk headroom was approximately426 MiB G
/1.66 GiB H. Private audit output is on the designated H root. No build, import, runtime, SDK launch,
media acquisition,100 MB world copy,449 MB bundle or1.47 GB dump. The accepted100,352,152-byte world
remains under the128 MiB reader bound; external Town WAV and immutable shared scene frames are
existing mechanisms. Further media needs a bounded estimate before preparation, not a reader-limit
increase or package platform. No full-size package was parsed by this audit.

Acceptance is direct UTF-8/link/scope checks, `git diff --check`, declared-path/private-boundary
review, clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual applicable
CI. Exact candidate/results/stopped-process state belong to the handoff. A1 precedes ordinary
Stage2 expansion, A2 belongs at Map30 acceptance, and A3/A4 retain explicit owners; green documentation
checks do not close those product boundaries.
