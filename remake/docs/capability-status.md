# Remake Capability Status

## Battle item-drop implementation agreement

**High rigor:** physical construction, scene playback and cross-mode item custody share one
`GameSession` publisher. The bounded outcome is an authored allied physical kill that removes an
actually held, explicitly mapped enemy item and delivers it to the living acting ally, or uses the
source rare-item Deals fallback. Ordinary outcome return and walking reentry retain live party
inventory, separate drop-once flags and per-item Deals stock while rebuilding fresh enemies.

The current implementation uses finite immutable definitions/candidates and existing scene/return
mechanisms. It adds no service, transaction runner or second mutable inventory authority. Missing
custody rejects before affected publication. Three hard-coded chance identities are separate from
the RARE attribute. Direct receipt has ordered dropped/received messages; Deals fallback is silent
and observable as committed state. [Rules](domain/gameplay-rules.md#finite-physical-action-item-drops),
[custody](application/session-and-programs.md#battle-item-custody-and-receipt-playback) and
[admission](content/profiles-and-trust.md#authored-item-drop-binding) own the exact boundaries.

Acceptance covers source-order first/second/counter construction, failed/held chance, actual slots,
status normalization, full/dead recipient, independent saturated stock, sweep exclusion, stale waits,
later failure, and same-session receipt/full-pack return/reentry through ordinary commands. Use
actual engine behavior tests, engine/adapter builds, the two
[bounded native inputs](development-and-verification.md#battle-item-drop-authored-observations),
committed verification selection and exact-head CI. Each native run is bounded to180s/5MiB; no
new assets or capture platform is required. Current milestone is implementation and native validation
complete, with independent main-gate acceptance pending in
[Issue #1349](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1349).

Full/private Battle18, wider action families, shops/Deals purchase, Caravan/depot, equip interaction,
save/load, original natural acquisition and hardware parity remain outside scope. The twelve #1047
comparison failures and prior completed failures remain preserved. Reassess before any wider action,
service or persistence expansion; this bounded state lifetime does not authorize one.


## Leader cleanup implementation agreement

**High rigor:** #1347 crosses retained accounting custody, resumable field death and outcome return.
The bounded outcome is an ordinary authored attack defeating the explicitly bound enemy128 gate,
followed by living-enemy removal through the existing field phases, genuine killed-list bookkeeping,
and usable exploration. The finite source binding maps Bowie and enemy slots independently of
physical leader flags and Taros enemy definitions. Action deaths precede positive-HP survivors in
source-slot order; removal preserves their HP and adds no damage EXP, gold or drops.

Acceptance covers held completion, dead/live gates and Bowie, changed slots/definitions/order,
unplaced survivors, zero-HP exclusions, stale waits, retained accounting after a later failure,
real scene/no-scene first-ally custody, and no second defeated hook after after-turn. Use focused
engine behavior tests, current engine/adapter gates, one ordinary-key native scenario and exact-head
CI. Native runs are bounded to <180s and <5MiB; prior failures and twelve unresolved #1047
comparisons remain retained. Bounded leader cleanup is implemented; the item-drop agreement above
owns its separate action-drop extension. No full Battle18/private import,
nonempty defeated programs, Caravan,
new assets, persistence, general hook platform or hardware parity. Missing first-ally custody is
explicitly Unsupported before affected cleanup publication; it is never replaced by the current actor.

## Taros physical-effectiveness implementation agreement

**Standard rigor:** a bounded physical-action extension uses the existing immutable Content,
SF2 construction, reaction delivery and sole session publisher. The player outcome is an authored
controlled battle: ordinary movement/ATTACK/target/confirm gives explicit ineffective feedback
for the wrong actor or live weapon, while the bound Bowie identity with equipped Achilles damages
the mapped Taros definition. This is not private Battle18 admission or complete encounter parity.

The finite encounter binding maps logical actors to source enemy-definition identities independently
of deployment order and binds Bowie plus weapon item types. Compute the ineffective operand once
at physical construction, retain it for doubles and clear it for counter construction. The separate
Taros-origin counter prohibition remains. Live inventory wins over starting equipment/possession.
Dodge/critical precede the damage gate; ineffective skips variance, HP and damage/kill/gold credit.
Surviving eligible ally end EXP still draws twice and awards minimum1. The source's later supported
sleep ailment can still execute; an ineffective result does not invent status immunity.

Acceptance is focused engine behavior tests (identity/live loadout/lifetime/RNG/reward/status,
queries/cancel, real scene wait/stale/failure prefixes), current engine and adapter gates, direct
source/document/private/ownership checks, and bounded actual Godot keyboard delivery of both
results through usable control. Native runs stay below180s and5MiB; no new assets, large world,
emulator, H4 or retired/full aggregate. Independent main-gate integration remains required.

Full Battle18 AI/private leader admission, wider loot/service bindings, aftermath/Caravan, all-action immunity, equipment
acquisition/equip interaction and hardware/natural original trajectories remain Unsupported or
Unknown at their existing owners. Preserve #1047's twelve completed unresolved comparisons and
all completed #1341/#1343 results; they are not rerun or replaced by this slice. The
[special-mechanic design](domain/special-mechanics-design.md#taros-effectiveness-cleanup-loot-and-acquisition)
retains the wider staged proposal. Coordination and exact candidate/check identities belong to
[Issue #1345](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1345) and its Draft PR.

## Battle02 Angel Wing implementation agreement

Issue [#1343](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1343) retains **High** rigor:
selected-slot payment, delivery, party return and fresh encounter entry cross runtime layers. A living
actionable player ally uses the normal held-item/self-target path, consumes exactly the selected Wing
before item feedback, returns to usable exploration and walks into fresh Battle02 in the same session.
No MP, known EGRESS or Bowie restriction applies. Initial custody/resources are controlled inputs.

Acceptance distinguishes provisional query/cancel, live actor/packed-slot/target/envelope rejection,
selected-slot compaction with survivor bits/order and empty tail, exactly-once payment before text,
matching staged completion, paid-prefix retention on failed return, and carried inventory after
ordinary healing/reentry. Small engine behavior tests cover duplicate Wing IDs/varied slots,
malformed prepared deltas, held/clear completion failure prefixes and Herb/EGRESS regressions.
The actual native observer uses MP0/unlearned Chester and two held Wings, cancels then selects slot2,
reads item feedback/delivery, tests duplicate completion and focus/process suspension, and walks back
with consumed inventory and fresh enemies/queue. No post-start injection, restart or reseed.

Scope reuses the finite spell/item payment and the accepted retreat continuation/return. Private
admission is Battle02 with F399 held and F64/F640 clear, admitted live savepoint and status-free party.
Ordinary outcomes remain Victory/Defeat. Wider map/status support, acquisition/drop/Deals/shop/depot,
equipment transactions, Battle0/session end, save/suspend, later mechanics and UI redesign remain
Unsupported or out of scope. PR692 is independent; only selection/confirm semantics overlap.
The [payment/source owner](application/session-and-programs.md#paid-retreat-and-field-return),
[gameplay rules](domain/gameplay-rules.md#finite-egress-action) and
[native recipe](verification/field-programs.md#battle02-angel-wing-and-walking-retry) own details.
The source join is **Confirmed** static; native observations prove modern behavior only. Natural
acquisition/chronology and original timing/media remain **Unknown**. Feedback reuses the accepted
authored tone/pulse/fade without new media. Preserved earlier comparisons/failures are unchanged.

## Battle02 EGRESS implementation agreement

Issue [#1341](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1341) uses **High** rigor:
spell admission, payment, actual delivery, return/recovery and fresh entry cross four runtime layers.
The bounded implementation connects those layers through the existing common session.
The outcome is a living, player-controlled Bowie selecting learned EGRESS1, confirming self, paying
8 MP once, receiving feedback, returning to usable Map3 exploration and walking into fresh Battle02
in the same session. The starting learned spell/party/resources are explicitly controlled inputs.

Acceptance separates query/cancel/MP7 rejection; MP8 payment before text; matching text/effect/close;
ordinary exploration recovery/setup; and fresh encounter entry carrying party identity, growth,
loadout and accounting. The engine tests include alternate admitted live savepoints, ordinary dead
versus immortal recovery, clear/held completion and a failed return retaining payment and closure.
The actual host observer reads input/state/projection/errors, including duplicate completion and
focus/suspension. [Session authority](application/session-and-programs.md#paid-retreat-and-field-return)
and the [reproduction recipe](verification/field-programs.md#battle02-egress-and-walking-retry) own details.

In scope: existing common session, finite EGRESS spell action/retreat continuation, admitted savepoint
bindings, existing field-return program and fresh entry. No new framework or second state publisher.
Private admission binds EGRESS only for Battle02, with F399 held and F64/F640 clear. A live egress
writer overrides the declared initial map and rejects an unadmitted destination. The selected party
has no outstanding status effects; broader status refresh remains Unsupported. Other-map EGRESS/
Angel Wing, Battle0/session end, suspend/save, later special mechanics, campaign expansion and menu
redesign remain out of scope. PR692 is independent; only existing semantic input/projection sites overlap.

Feedback is the explicitly authored `authored-egress-tone-v1` plus text, soft pulse and close fade;
original EGRESS media/timing is not claimed. Original natural learning/route and hardware fidelity
remain **Unknown**. The source join is **Confirmed** static at the named owners; modern tests and native
observations establish implementation behavior only. No original acquisition or bulk comparison ran.
The twelve unresolved #1047 comparisons and completed failures remain preserved at their existing
[evidence route](evidence/retained-comparisons.md).

## Reading This Matrix

This matrix groups coherent runtime capabilities of the common engine. It is not a pull-request
chronology and does not create evidence for the original game. The legacy reference matrix was retired
with the reference runtime at
[M5](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m5-implementation); its
last state is [preserved in Git](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/8a581a82e297ea2947cc9837e661752163d2806d/remake/docs/capability-status.md#legacy-reference-matrix).
[Current M1](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m1-implementation)
provides common-session authored admission and connected HEAL/STAY battles;
[M2 physical](../../docs/decisions/0019-state-and-content-driven-remake-engine.md#current-m2-ordinary-physical-implementation)
adds the ordinary physical/death/reward chain. Completed old checks remain evidence of their named
scope, not obligations to preserve their tests. The current private Map 3-through-Battle 01 victory
and usable-return milestone has [independent composed acceptance](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
under keyboard A, the modern deterministic clock and 7C/8D/10A semantics. Historical executable
reports retain their original verdicts; no fresh full H4 report was generated. Hardware 8C,
full-game parity and public distribution remain outside the accepted boundary.

## Common Authored Battle

| Capability | Status and boundary |
| --- | --- |
| Configurable start | **Implemented:** the format-v7 authored packages use the same Content/session path; immutable definitions/deployments are separate from explicit start resources/accounting/seeds and optional placement overrides. One definition can support independent starts with validated references/bounds and fresh runtime actors. The separately selected private initializer is bounded below. |
| Encounter faction and order | **Implemented:** deployments own explicit ally/enemy faction and unique integer processing order; source numeric slots no longer define side or identity. Round/AI/UI ordering is stable under JSON array reordering, and queues resolve `ActorRef` with a separate sentinel. Supported 30-ally/32-enemy and control-side limits remain; ally AI and player-controlled enemies are Unsupported. |
| Semantic terrain | **Implemented:** row references resolve explicit surface/protection definitions; one terrain-rule owner serves placement/start, preview/commit, AI pursuit/scoring and target/counter land reduction. Dynamic blocking is a separate cost projection. Original movement adapters decode source operands into the same kernel; unresolved/unsupported terrain rejects. Required private Centaur/hovering/Flying profiles and bounded initialization are implemented below; broader profiles remain unsupported. |
| Movement and selection | **Implemented:** weighted preview, occupancy and provisional destination; finite player, decreasing-cost Cancel and retained AI routes deliver one token per adjacent segment before action construction/commit. Friendly traversal does not authorize stopping. Godot projects actual intermediate positions, facing and walking halves and starts private79/BD once per new segment. No movement-delivery RNG is added. Original interrupt timing and arbitrary equipment remain Unknown/Unsupported. [Movement boundary](godot/battle-scenes.md#battlefield-movement-and-walking-audio), [verification](verification/battles.md#battlefield-movement-consumer). Logical maps do not wrap at row edges. |
| Ordinary priest HEAL and STAY | **Implemented:** learned spell, MP/living ally/range checks, atomic preparation/rejection, staged MP/HP/EXP and live scene RNG; awards consume admitted growth when supplied; missing growth and unsupported effects reject before publication. |
| Replaceable battle action algorithms | **Implemented bounded seam:** immutable per-session C# composition, the same rule's spell/target query and confirm, finite preparation validation, and the existing actual presenter. Default SF2 and authored capped/award A versus injured-only half-missing/no-award B use the same Content/session/view. Herb still uses one shared target/scalar owner independently of the spell variant. Source unknown accounting remains rejected when preparation reaches it. Later growth failure retains committed stages and its live token. The same finite action query/confirm/preparation now covers physical, Herb and Stay, with semantic inventory/target choices and structural staged validation. The selected physical calculator reaches player and existing AI estimate/preflight/post-movement execution. Automatic and progression/outcome replacement are implemented below; conditional-story/source-policy replacement is described in the following row. [Architecture](domain/gameplay-rules.md#implemented-heal-seam), [action verification](verification/battles.md#semantic-action-selection-observation), [HEAL demonstrations](verification/battles.md#replaceable-heal-observation). |
| Replaceable conditional story and source compatibility | **Implemented bounded seam:** public v8 A/B programs each execute both live flag branches, two nested calls, real text/tick waits, different entity/flag effects and usable returned movement through one unchanged generic runner. Selected SF2 story policy owns the exact Map3 scratch guards and actual retirement; explicitly authored compositions reject its typed source family. Finite candidates cannot replace PC/stack/waits/party/aliases or publish revisions. Expected/invalid/throwing policy failures preserve prior effects and reached caller/PC context. Source-context behavior tests establish only their seam. Fixed refresh/activation/control, movement-cost and field/scene source-service replacement remains deferred; full-game/no-compile algorithm authoring and private natural fidelity are not claimed. Independent final-slice and finite Epic acceptance remain main-gate-owned. [Architecture/author guide](application/session-and-programs.md#source-story-policy), [content guide](application/session-and-programs.md#authoring-conditional-programs), [verification](verification/field-programs.md#replaceable-story-observation), [#438 semantic handoff](godot/presentation.md#semantic-interface). |
| Numerical agility and extra round action | **Implemented:** numerical agility0–127 and explicit boolean eligibility generate one or two entries independently. Ordinary/extra draws, truncated basis, signed sentinels, capacity, dead/unplaced omission and natural queue consumption remain source-equivalent. Physical double/counter rules are separate; arbitrary action counts and wider speed formulas are unsupported. |
| Independent control and AI policy | **Implemented:** encounter deployments select player/automatic control separately from an immutable strategy reference resolved against the chosen composition; actor definitions remain reusable across deployments. Content/start reject missing or incompatible combinations; session start resolves all current/later/dead deployments and rejects unknown/duplicate bindings, never silently Stay. Ally/player and enemy/automatic remain the supported pairings; wider control/status/AI is unsupported. |
| Replaceable automatic decisions | **Implemented bounded seam:** source approach/standby/set6/set7 and authored FirstLegal/LowestHp bind once before start through `IBattleDecisionRule`/`BattleStrategyRef`. The same admitted state selects different physical targets, moves and presents them without scheduler/view policy switches. Finite candidate validation preserves earlier commits and failed queue entries; thinking/memory prepare before movement, actual main construction follows it, and completion does not repeat decisions. Source priority/class/tie/zero-target/loadout/status bounds remain. No wider AI platform, action category or runtime selector is admitted. [Architecture](domain/gameplay-rules.md#implemented-automatic-decisions), [actual consumer recipe](verification/battles.md#replaceable-automatic-decision-observation). |
| Automatic battle continuation | **Implemented:** dead queued entries skip; sleeping entries use the staged after-turn notice; the selected decision executes through the same movement/action consumer, sentinel starts the next generated round, and Application stops at player input or a bounded simulation tick. |
| Replaceable progression and ordinary outcomes | **Implemented bounded seam:** selected award/gold/counters, credit, live-seed growth, outcome/hook and program-return policies retain existing publication stages. SF2 keeps source caps/learning/equipment/recovery; an authored factory changes eligible rewards and defeat cost without changing continuation machinery. Two small public v8 packages and four native factory/outcome cases cover real entry, messages, accounting, ordered join/flags, return and movement. Authored growth excludes equipment/spell learning; nonempty defeated programs, unbound/private enemy leaders and wider outcomes remain unsupported; the explicit authored cleanup binding above is implemented. Private route/native HEAL limitations are not closed by this demonstration. [Architecture](domain/gameplay-rules.md#implemented-progression-and-outcome-policies), [verification](verification/battles.md#replaceable-progression-and-outcome-observation). |
| Ordinary host boundary | **Implemented:** ordinary GameRoot/Main compiles only with production projects, with consistent authored/private-common startup diagnostics. The reference host and its smoke code were retired at M5. Diagnostic environment options belong to the external probe; normal export configuration excludes it. Original natural continuity and complete ordinary export remain outside this boundary. |
| Godot input and projection | **Implemented:** shared remappable keyboard/standard-gamepad actions, configurable Confirm/Cancel convention and effective HUD hints across exploration, battle and return. Versioned startup settings also select reduced white flashes and instant/adjustable text while preserving real wait/cue completion. [Native 9A observation](verification/host.md#native-9a-observation) covers default/remapped/swapped devices, alternate authored content and the private continuous return. These are intentional modern-interface deviations; physical controller drivers are not observed by injected events. Authored export packaging and original presentation remain unclaimed. |
| Private initialized entry | **Implemented subset:** actual trusted encounter plus pinned selected enemy/class/item/spell exports and external controlled PlayerReady party/start enter the common session. New-battle heal/effective ATT, initial AI/region state, activation/program/spawn admission, carried first queue and candidate control lead to common movement/cancel. Required regular/healer/Centaur/hovering/Stealth/Flying movers work. Unknown accounting stays null; source baseline/loadout/words/provenance remain retained. |
| Private source enemy continuation | **Implemented subset:** actual common player commands reach inactive anchor-relative standby, evolving occupancy/memory, region activation and source set6/set7 no-action pursuit, then the next live player. Source NONE orders, activation/tested words, last target and both RNG channels remain intact. Standby/pursuit calculations have one production owner; one common movement/action publisher commits results. Reached physical operands use the shared action binding below; failures remain atomic. Nonempty move-order/action categories and broader status/neutral behavior remain Unsupported. |
| Private actions and spell selection | **Implemented subset:** actual class prowess/adjacent ATT-only equipment, source HEAL1–3 and pinned enemy gold bind to common physical/reward/death/after-turn. Hovering/Flying share airborne dodge and priority; only Flying ignores ground land protection. The logical Spell action (default H / Pad North) cycles learned spells and levels without content reordering; engine owns legality/resources. Explicit external accounting enables actual enemy/player/kill/HEAL continuation; natural counters and seeds remain Unknown. Selected Battle02 EGRESS/Angel Wing is implemented at the agreement above; wider effects remain Unsupported. The connected private world binds growth and terminal programs below; broader M2 capabilities and hardware 8C remain incomplete; current milestone acceptance is recorded below. |
| Ordinary physical attack and rewards | **Implemented subset:** regular-ground, status-free, no equipment, explicit paired critical chance/bonus definitions (1/32 plus half or1/16 plus quarter); source-ordered first/second/reversed counter, dodge/critical/spread, per-action EXP/gold, ordinary enemy/ally cleanup, capped kills/defeats and next living control. Both authored physical packages support either legal kill order. |
| Physical battle scene execution | **Implemented bounded consumer:** typed construction, animation/reaction, result/death, EXP/growth/gold and end waits retain input and turn ownership. Damage reactions advance shared-main RNG by 24 source draws each; growth uses the carried seed. Godot consumes privately prepared actor/weapon/environment frames and source music2/5, hit81/83 and level102 SFX through actual audio. Normal/reduced-flash observations preserve gameplay/RNG. HEAL has the separate bounded consumer below; modern text/fade timing and controlled native entry do not prove original timing, final pixels or H4. [Scene owner](godot/battle-scenes.md#physical-battle-scenes). |
| HEAL spell scenes | **Implemented bounded consumer:** HEAL1–3 MP-before-cast, one/one/two source-rule fairies, shared-main RNG, self/other HP recovery, stop/cleanup before rewards/input, private rasters and actual77/113 audio. Function comparison under externally supplied opportunities/control passes; normal/reduced native semantic events match. **Unknown:** continuous original opportunity admission; recovery bsc0B is the first observed divergence and produces intermediate RNG differences. The accepted selected HEAL/scene composition supplies the current milestone obligation; this historical timing divergence remains with its owner. [Exact boundary](godot/battle-scenes.md#heal-spell-scenes). |
| Medical Herb battle scenes | **Implemented bounded consumer:** live inventory consumption during construction, deferred mode2 HP recovery without MP cost, single-ally display/target switch/restoration, EXP/growth and end-of-scene input release. Uses source item animation NONE/Nothing and SFX113, with no fairy or physical recoil draws. Wounded controlled entry and full-HP source-world normal/reduced observations cover different boundaries; natural timing and whole-scene HEAL parity remain unfinished. [Scene owner](godot/battle-scenes.md#medical-herb-scenes). |
| Enemy physical decision | **Implemented subset:** already-active, physical-only ATTACK1/script3, admitted source movers with MOV1–63. Reverse candidate/RNG order, signed raw-priority cohort and capped result, Regular class table for named classes when needed, largest-movement/later-collected tie, shared physical/reward/death/control path. Missing reached class/physical capability remains atomic Unsupported; earlier commits and the failed queue entry survive. |
| Zero-target commandset06 continuation | **Implemented subset:** ATTACK1, unavailable HEAL1 and SUPPORT return failure; MOVE1 selects a living target from complete raw costs0–127, walks fixed cost4 then corrects the station with the legal MOV grid. Movement or origin Stay returns success and ends the action; no same-turn attack, RNG/resource or last-target change. High/incomplete target costs remain atomic Unsupported. |
| Remaining battle behavior | **Unsupported beyond admitted capabilities:** nonempty defeated programs/private or unbound enemy-leader cleanup, battle-region programs, ally automatic control, nonzero spawn, laser action6, unadmitted EGRESS/Angel Wing and broader equipment/spell/status families. Growth and ordinary victory/defeat/return are implemented for the bound content above and below; Battle03 Flying/physical sleep is implemented below. Missing bindings or unsupported reached effects still reject at their owning boundary, preserving any prior committed prefix. The [Proposed special-mechanics design](domain/special-mechanics-design.md) separates missing content, rules and state/interface extensions; it does not enable these mechanisms. |

## Common Exploration and Programs

[The current owner](application/session-and-programs.md) records exact semantics, Content/start formats,
source provenance and reproduction.

| Capability | Current boundary |
| --- | --- |
| Connected authored exploration | **Implemented:** two distinct format-v8 packages execute interaction/choice, real entity motion, call/return, flag/tick/text waits, map loading and both intro hooks into existing battle control through one session. |
| Program transactions and state | **Implemented:** PC/call stack/wait token, continued text window, actual simulation ticks, source-order entity actions and global sprite size. A failing native instruction/action retains the completed prefix and its own cursor. |
| Private original audio | **Implemented resource/consumer subset; current milestone composition accepted:** six music resources and 25 finite SFX timer variants are privately admitted. Actual WASAPI checks cover persistent loops, Town/JOIN replacement/finite wait/previous restart, representative speech replacement/Finished in CB/D2 contexts, one ordinary R1 warp and the physical scene subset above. Map music, source speaker metadata, field warp/door and modern UI confirmation/selection bindings consume original PCM. Source-population obstruction now precedes door copy/audio using current and reserved positions; actual controlled Map3 input observes one92/CB, completion, traversal and no replay on an opened door. [Door boundary](godot/audio.md#ordinary-door-obstruction-and-audio). Successful ActionChoice entry/exit consumes65 instead of generic66/67 on that boundary; paired actual two-actor observations cover entry, cancellation, Stay/target, rejection and same-stage behavior. This does not reproduce every original window. Instant/reveal-all speech omission is an accepted 9A/10A deviation: skipped characters add no speech, reveal preserves existing tails subject to later legitimate replacement, and normal source-specific confirmation remains. Accepted settings equivalence, C reveal-only state records, D normal speech and new A instant/confirmation receipts supply bounded coverage; the historical C synchronous reveal-tail audio interval remains unobserved; Option A has separate accepted controlled tail evidence. [Fast-text policy](godot/audio.md#accepted-fast-text-speech-policy). Disjoint and partially overlapping finite SFX play as whole PCM clips together; equal/fully covered groups replace. This accepted modern approximation keeps Herb tails from blocking menu input and bounds repeated same-group requests. Unknown classifications and missing/ambiguous content remain explicit. Finite SFX prefer exact command/timer matches, then permit only a unique same-command finite PCM unchanged; receipts distinguish requested and recorded timers. Actual physical/Herb consumers cover the bounded overlap rule. Exact hardware masking and partial-channel mixing are outside the current audio goal, not milestone blockers; #554 stem/core work was abandoned as unnecessary. The bounded modern fade ramps music/noise, terminal-stops tone3 and preserves type-2 tails; actual group/interruption and authored SoundFade input/completion observations cover it. Original253 is asynchronous; the explicit host service wait and modern duration are not original timing or natural-route evidence. [Fade boundary](godot/audio.md#sound-fade-request-and-effect-lifetime). Bound JOIN now uses the accepted modern finite profile19/end505 plus matching actual completion, previous restart and plain input through first ordinary Zone return; original sound/VInt phase remains Unknown; reached field/UI bindings use the accepted current composition below. [Clock policy](../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy). HEAL77/113 is covered by the bounded scene consumer; original continuous opportunity timing remains Unknown; selected logical opportunities and consumers have composed acceptance. Missing content fails explicitly; synthesized JOIN chords are removed. [Audio boundary](godot/audio.md#audio-boundary) owns the exact scope and asset pins. |
| Original Map3 opening | **Confirmed, bounded:** the common session consumes the R1 start, R2 input route and complete R2a messenger acceptance through F603 and stable field input. Real source entity allocation/aliases, initialization, ordered events, doors/roofs/same-map preservation, joins and follower motion execute. Decline/re-prompt acceptance and an extra-follower slot shift exercise alternate state. |
| Original castle/palace/Astral/tower | **Confirmed bounded group:** same live opening session runs castle gate, Map19/20 init, full palace, royal repeat/return, Astral refusal/re-prompt/acceptance, tower warps and actual guard release. Fresh source135 resolves to player slot0; sprite completion precedes F401 and caller F256. Repeat interaction and stable field input pass at60/30 FPS. The bound modern opening/JOIN additionally reaches gate/F604, source255 first palace/F605 and stable Map19 return under instant and adjustable/reveal delivery; general init windows retain real callers and use D1 neutral service and sourceFF silent narration. Zero-autoscroll field view also admits main-plane unity/origin0 with secondary per-axis128/256 parallax and offsets0–63, signed-word limits, independent plane scroll and main-plane actor projection. The bound same-session continuation reaches Astral refusal/re-prompt/acceptance, guard release/repeat and Map40(14,13) field approach with actual unequal-plane projection. Later bound battle consumers are covered by the current composition; original cadence remains Unknown; [execution owner](application/session-and-programs.md#castle-palace-astral-and-tower). |
| Bound before-battle scene/windows | Closed-window black scene replacement preserves explicit origin, source counter, service policy and base/current distinction. Real BeforeBattleFinished text/portrait/view services retain route/callers and expose only ordinary W1/W2 input; the native boundary is natural Map40 exit through Map57 to Astral2292 W2 confirm and2293 W1 readiness. Visible/transitioning/open-window loads and missing palette reject. Null detach preserves axes/counter/RNG without time advance and is explicitly lowered before source fade waits; standalone fades retain following. One explicit post-load service precedes entity replacement. Bound drawing uses the selected A/B area even with the retained player outside every new area. Physical target tracking is admitted by the bound target rule below; bound white is described below; complete bound entry is described below; original cadence remains Unknown; [owner](application/session-and-programs.md#bound-before-battle-scene-and-windows). |
| Bound physical camera tracking | Existing bound field/map-init/real-before contexts resolve a logical selector once to LogicalView.TargetSlot and follow its live physical motion. Zero-time installation/retarget preserves axes/counter/services; active scroll finishes before follow resumes, with existing main-plane and secondary128/256 profiles. Valid slot0/default aliases and invisible allocated targets remain valid; missing/removed/out-of-table/cursor and incomplete/outcome contexts reject. Ordinary W1/window services retain route/callers. Independent actual readback reaches tracked motion and three later W1s, then the first white Unsupported; raw probe FAIL from two expected-stop assertions is preserved separately from semantic readback PASS. That historical stop is superseded by the bound white capability below; complete bound entry is described below. [Execution rule](application/session-and-programs.md#bound-physical-camera-tracking). |
| Bound white palette | Finite source map-white out/in uses one base/current authority, forced period1, seven offsets, terminator/extra service and prior-period restoration. Matching actual delivery joins logical completion. Existing common service honors script entity override and registered portrait; warp context remains deliberate. Ordinary six-helper continuation is bounded at Chester2297 W1; authored mixed black/white normal/reduced projection verifies modulation/alpha reset. Whole-viewport half-second delivery is modern; raw palette RAM and source map-only pixels/natural cadence remain Unknown; bound effects/entry are described below; original LoadBattle opportunities remain Unknown. [Owner](application/session-and-programs.md#bound-white-palette), [verification](verification/field-programs.md#bound-white-palette-observation). |
| Bound before-body/battle-loader ownership | Field helper requires ActiveExploration; ActiveBattle loader keeps retained field settings/frozen facade and existing modern delivery-only black waits. Explicit non-field FullBlack rejects atomically. Bound generic effect/loader waits use existing focus/visibility suspension without catch-up. **Confirmed bounded remake:** same-session ordinary remainder delivers mosaic/five shivers and windows/joins, suspends/resumes one generic effect, mounts/loads/starts the battle and exercises first Movement/Confirm/Cancel without action or turn commit; unchanged prior checkpoints. Meaningful engine cases and actual normal observation pass. Original LoadBattle opportunities/pixels remain Unknown; winning/return and current H4 composition are accepted below. [Owner](application/session-and-programs.md#bound-before-body-and-battle-entry), [verification](verification/field-programs.md#bound-before-body-and-battle-entry-observation). |
| Battle01 entry and presentation | **Confirmed bounded group with accepted current text/view composition:** continuous R1/castle/Map21/40 route executes complete `bbcs_01`, scene/entity replacement, source initialization, load/start and first real battle input in one session. Live membership packs source formation; resources/equipment/accounting/RNG retain their owners. White fades, mosaic, shiver and load mount are performed modern services. The external H3 seed/actor1 comparison stays separate. Original hardware timing, broader setups/roster/status refresh and F88 resume remain **Unknown/Unsupported**; [entry owner](application/session-and-programs.md#battle01-admission-and-first-input). |

| Battle01 growth and complete outcomes | **Confirmed bounded group:** real actions/deaths select victory or ordinary defeat in the same session. Content supplies source class growth, one EXP threshold, five-stat/equipment refresh and learned spell upgrades. Victory executes the full after-program and Map57 return; ordinary defeat restores the leader, halves gold and uses explicit Map3 egress. Source-ordered flags/join/reset (compared with the accepted R4a static spine), conditional inactive-window scratch retirement and actual field movement are covered by engine and native 60/30 FPS observations. Broader status/effects/outcome families, natural original inputs and hardware/presentation remain Unknown or Unsupported; [execution owner](application/session-and-programs.md#battle01-outcome-after-program-and-return). |

| Post-action battlefield death batch | **Confirmed bounded engine and enemy/counter-death host:** newly dead members retain cells through twelve turns and three effect63 directions; one116/BD starts for the entire nonempty batch. Cleanup/stat caps, counter casualties, no-death, outcome blocking and ordered multi-member domain cleanup are tested. The explicit authored leader binding additionally presents a damaged gate plus living survivor and returns to usable exploration. Private map sprites reuse selected world resources; only63/field selectors are added to selected scene content. Both actual normal/reduced pairs preserve ordered semantic events and gameplay state. The preceding counter reaction reuses unique81/CC under requestedC6 through the accepted modern finite-PCM policy; it does not claim hardware-exact audio. After-turn death producers, natural multiple deaths, special entity32 and exact interrupt timing remain Unsupported/Unknown. [Presentation boundary](godot/battle-scenes.md#battlefield-death-batches), [verification](verification/battles.md#battlefield-death-consumer). |

## Milestone and Deferred Surfaces

| Capability | Owner | Status | Retained boundary |
| --- | --- | --- | --- |
| Continuous Map 3-through-Battle 01 milestone | Research, design contracts and common Application/Domain/Godot | **Accepted current private milestone** | Existing continuous opening, Battle01 victory and usable 5B return evidence, accepted rule/resource/consumer compositions, unchanged dependencies, checked parent coverage/shared joins and explicit deviations establish the current keyboard A/modern-clock/7C/8D/10A boundary. The [readiness owner](../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone) records independent acceptance; the [review route](evidence/retained-comparisons.md#accepted-composition-review) locates retained supporting inspections. Historical A10/default/matrix reports remain Unavailable and scoped reports retain `milestonePass=false`; no fresh full report or corrected whole-A trajectory is claimed. Historical missing fields and original timing retain their owners. Ordinary defeat is a separate bounded capability. |
| Save/load and persistence | future milestone and save-system owner | **Unsupported / deferred** | no player-facing save, checkpoint, suspend, migration, or persistence adapter |
| Hardware 8C reference parity | future layered observation owners | **Unknown** | exact reached frame, palette, audio, hardware chronology, capture conditions, and accepted tolerances remain open |
| Legacy public-synthetic and private-local profiles | none | **Retired at M5** | the public-synthetic Map 3/battle package and export smoke, legacy private traversal views, project-authored battle bridge, legacy local asset-pack Godot consumers and controlled Battle01 receipt chains were removed with the reference runtime; the common host renders prepared private worlds instead |
| Item acquisition, area descriptions and setup-event selection | none | **Unsupported** | their only consumers were legacy; accepted contracts and research remain for a future reached consumer |
| Special encounters, retreat and transport extensions | [Staged engineering design](domain/special-mechanics-design.md) | **Authored Taros physical effectiveness and selected Battle02 retreat implemented; wider consumers proposed/unsupported** | The finite identity/live-weapon gate delivers explicit ineffective feedback in controlled authored content. Authored leader cleanup and usable return are implemented with explicit source-slot binding. Authored physical receipt/Deals and session custody are implemented; private Battle18/natural Taros loot, Jaro conversion, Prism laser, hidden/reappearing enemies and trial records, wider retreat/special caller return, and raft/Caravan travel still need their owned extensions. Existing flags, following, region activation and ordinary outcomes are reusable; acquisition flags alone do not implement vehicles or services. |

Future diagnostic facts should extend one bounded inspector or observation model. They do not receive a
new capability, receipt, or matrix row unless they independently cross a trust, authority, versioned-port,
or stable-observation boundary under
[ADR 0017](../../docs/decisions/0017-heavy-boundaries-light-internals.md).

## Current Engineering Frontier

### Battlefield map rendering agreement

Issue [#1051](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1051) uses **Standard** rigor:
bounded integration of admitted resources into the existing battle board, with one shared decoder
and no second gameplay/map authority. **Implemented and merged** in [PR #1052](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1052):
Battle01–03 draw their selected map regions with one cell/actor/preview transform, tile-priority
passes and an accepted-target outline. Content preserves per-encounter visual map/origin through
growth/outcome binding. Authored practice keeps its intended fallback; missing private maps fail
visibly and retain the pending BattleLoad. The
[presentation owner](godot/battle-scenes.md#battlefield-maps) and
[native recipe](verification/battles.md#battlefield-map-observation) own current behavior and acceptance.

Acceptance covers the Battle01 pilot, different source origins, movement/cancel, target rejection
and cancellation, real action/death/next-player presentation, desktop/small-window clipping,
exploration handoff, Battle02 retry, different-map rebuild and final disposal. Reuse prepared worlds;
no screenshot/emulator acquisition, broader renderer platform, menu redesign or additional gameplay
is in scope. UI/UX remains independently owned by #689/PR692. Human aesthetic approval and original
pixel/timing parity remain **Unknown**; the twelve historical comparison failures remain unchanged.

### Yeel guide through Battle03 implementation agreement

Issue [#1049](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1049) retains **High** rigor:
ordered field events, party continuity, encounter initialization and staged status/control changes
cross existing state owners. **Implemented and merged** in [PR #1050](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1050): an explicitly
controlled accepted Battle02 return walks through early Yeel, Kazin's offer/following, exit/reentry
with one Kazin, and a real Map66 zone request into playable Battle03 in one session. A separate initial
zone approach with F72/F502 clear proves that neither Kazin nor Battle02 completion is a prerequisite.
Actual player movement/cancel, committed Stay, enemy continuation and the next usable player boundary
are observed. Human play remains pending; original natural chronology/timing remains **Unknown**.

The accepted [route synthesis](../../docs/design/synthesis/battle02-outcome-through-battle03.md) and
pinned `c834c652b6862bc5679fd7f69a38a7093206efc6` sources own original facts. Earlier matching
Map66 Y9/10/28 zone rows suppress the default even when their program returns. The default requests
Battle03 deterministically before completion; completed-repeat counter/RNG semantics remain distinct.
Ordered encounter admission retains earlier Battle02 eligibility. The request owns no fresh X/Y/facing.
`StoryState` owns pending request, nullable signed step counter and the reached Egress assignment.
`bbcs_03` writes Yeel only under the F453-clear intro gate; empty start sets F453. Egress gameplay and
Battle03 outcomes are outside scope. Twelve source ally rows are formation capacity, not recruitment.

| Selected content | Implemented boundary | Remaining boundary |
| --- | --- | --- |
| Bowie / Sarah / Chester / Jaha | SDMN/PRST/KNTE/WARR, ordinary equipment/growth/accounting and HEAL1; live return resources, including dead/unplaced Chester; no F4 recruitment | Selected Battle02 EGRESS/Angel Wing is implemented above; wider spell/equipment families remain Unsupported. |
| Huge Rat / Ooze | Existing Stealth/Regular movement, ordinary physical, source set6/priority and scene paths | No wider enemy action family. |
| Huge Bat | Source Flying movement/land/dodge/AI priority; unarmed sprite23/palette0; sleep critical and full action/after-turn recovery lifecycle | Other ailments, spells/equipment and source animation/timing fidelity remain outside scope. |
| Field caller/presentation | Source layouts, doors, ordered tables, texts784–787 and portrait/speaker distinctions; typed synchronous narration and CLEAR; one following Kazin after reentry | Later Yeel setups and village services are not implemented. |
| Encounter notification | Explicitly authored 240ms Boost109/C2 cue with actual start/Finished callbacks; four white pulses over32 modern ticks and restored overlay | ReducedFlash suppresses opacity while completing normally. No original audio/hardware equivalence. |

Source `InitializeEnemyItems` calls `AddItem`: mask `$807F` changes the bat placement word `$6000`
to ordinary Herb0. Raw placement metadata is retained separately; the physical-only AI admits only
this selected inert ordinary Herb or empty slots, rejecting unknown/equipped/AI-enabled items.
Flying uses source table5 costs for terrain0–8, zero land reduction, airborne dodge8 and existing
priority rules. HighSky7 stays blocked to Hovering; padding/barriers, bounds and occupancy remain
blocked. Preview, committed movement and AI share the existing terrain kernel.

Physical sleep follows source operands: `criticalhitdefs.asm` row10 is chance4/factor0;
`attack.asm` skips ailment on lethal hits. `inflictailment.asm` emits a separate status reaction;
`bsc0A`/`bsc0B` publish constructed status operands during playback. First/second/counter ordering
therefore preserves old-status writes, including a later damage operand replacing newly inflicted
sleep. Initially sleeping targets skip dodge and counter; ordinary hits have no blanket wake rule.
After-turn draws with range `status & $C0`, tests `draw & $C0` and otherwise subtracts `$40`.
The adjacent source comment's chance fractions disagree with this arithmetic; code operands control.
Only actor.Status is authoritative; pending notices/reactions contain uncommitted operands.
RNG precedes the real notice, status follows actual completion, and one queue entry is consumed once.
Wrong/stale input cannot repeat the transition. A separate explicit INITIAL HP200 party variation
naturally reaches bat sleep, skipped action, wake and restored player control without later injection.

The selected private world uses Map5/66 and Battle02/03 with a finite `audioCues` selection. Its
94,006,868-byte document fits the unchanged128MiB bound; the largest observed game process peaks at
760,791,040 resident bytes within the2GiB budget. Preparation/load remain linear in selected content,
with one producer/native process and worktree-local outputs. No cache/loading infrastructure, shared
asset write, new emulator acquisition or bulk frame capture is added. Older runnable packages and
private evidence remain intact. Exact private inputs and outputs live only in ignored configuration.

Acceptance uses direct producer execution, new engine behavior tests, affected existing behaviors,
the actual adapter build, normal/reduced-flash guide observations, independent skipped-guide and
sleep variants, a clean-head engine-scoped dependency plan and CI. The
[execution owner](application/session-and-programs.md#yeel-guide-zone-request-and-sleep-continuation),
[trust owner](content/profiles-and-trust.md#selected-third-encounter) and
[reproduction recipe](verification/field-programs.md#yeel-guide-through-battle03) define the boundary.
The two commit groups separate required Flying/sleep from field integration. PR692's UI/session
contract files remain separately owned. Independent review and merge belong to main-gate.

Preserve all completed discoveries and #1047's twelve unresolved old comparisons: nine old-start
full-fade-state cases, selected seed, first actor and activation rejection. They are not passes;
no aggregate rerun replaces them. Battle03 outcomes, Hawel/recruitment, Battle04, village services,
save/load, balancing, later Yeel and full-game parity remain outside this slice.

### Battle02 outcome implementation agreement

**Implemented bounded group:** ordinary actions reach victory, the empty after-program/JoinForce(0)
tail, F402 clear/F502 set and usable Map66 return at the first active member's final position plus
`(16,19)` and live facing. Ordinary defeat restores the leader, halves current gold and returns to the
explicitly selected Map3 savepoint; incomplete F402 remains set. Church-door movement and west-exit
entry initialize fresh enemies while retaining the same party/session. Grown actors can consume Herbs
without losing growth or inventory consistency. Native win and defeat/retry observations establish
implementation behavior. [Execution](application/session-and-programs.md#battle02-outcomes-and-live-facing)
and [verification](verification/field-programs.md#battle02-outcomes-and-retry) own the detailed boundary.

Issue [#1047](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1047) retains **High**
rigor for caller-dependent return coordinates, party recovery and continuous retry. The bounded
outcome is actual Battle02 victory or ordinary defeat, usable field control, and a same-session
walk back into Battle02 after defeat. Controlled initial Battle02 starts with four members are
explicit inputs; later state injection, restart or reseeding cannot establish retry acceptance.
Research/design batches remain paused. Yeel, Battle03, save/load, revival/shop UI and Egress gameplay
are outside this slice. Original natural reach/timing and human-play acceptance remain Unknown/pending.

Owned implementation files are `remake_exploration_battles.py`, `ExplorationDefinition.cs`,
`ExplorationContentReader.cs`, `Sf2OutcomeReturnPolicy.cs` and `BattleOutcome.cs`; approved shared
extensions are `EngineBattleState.cs`, `BattleMovementContinuation.cs`, `BattlePresentation.cs`,
`BattleActionResolution.cs`, `IBattleProgressionRule.cs`, `BattleSceneContinuation.cs`,
`BattleActionCommitter.cs` and `ExplorationViewRunner.cs`. Existing movement/session/healing-rule
and private-initialization tests replace only stale movement identity expectations with unchanged
resource/accounting/seed and explicit facing assertions. Focused acceptance uses
`Battle02OutcomeTests.cs`, `battle02-outcome-{programs,start,party}.json` and
`engine_battle02_outcome_observation.gd`. Documentation changes stay in this capability owner,
the session/program owner and the field verification recipe. There are no unmerged dependencies.

Acceptance covers varying final positions/facing, living/dead allies, completed/incomplete defeat,
movement cancellation, unchanged accounting/resources except source-owned recovery, and fresh enemies
on retry. Run the affected producer, engine behavior tests, adapter build, clean-head dependency plan,
scoped CI and two native input/state runs (win; defeat plus retry). Existing failed comparisons remain
individually classified. The selected world is 127,597,616 bytes under the 134,217,728-byte bound.
This selection adds small route/program metadata, no new media, with unchanged linear preparation/load
cost. Runtime outputs and resource measurements remain in the Issue handoff. Independent integration
is complete in [PR #1048](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1048); human play
remains pending. Existing failed comparisons are not reclassified by this bounded acceptance.

### Battle01 return through Battle02 implementation agreement

Issue [#1045](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1045) uses **High**
rigor: second-encounter admission, shared party resources and cross-map story execution cross
existing Content/Application/host boundaries. The intended outcome is one ordinary private
`GameSession` executing real Battle01 victory, tower return, castle exorcism/commission, Jaha's
join and the Granseal west exit into usable Battle02 rounds. Research and design-production batches
are paused. Accepted source contracts remain evidence; remake execution establishes implementation
behavior only.

Acceptance is staged: first a separately controlled Battle02 pilot (terrain, six enemies, Jaha,
movement/cancel, physical action and enemy continuation); then the uninterrupted ordinary route
from actual Battle01 victory; finally native input/state/projection evidence and human-play launch
instructions. The pilot is not continuity acceptance. No checkpoint substitution, flag injection,
teleport, session replacement, screenshots or bulk frame capture is allowed for that final route.
Source completion/unlock behavior must preserve intentionally different authored policy. Party
resources, equipment, growth, accounting and seeds retain their live owners across encounters.

The endpoint is Battle02 actions and return to player control, excluding Battle02 resolution,
Battle03, save/load, UI redesign, Chinese, full-game import and new original runtime acquisition.
The measured candidate is 127,596,681 bytes, admitted under a
128 MiB (134,217,728-byte) bound checked before allocation and after read. A native load measured
721,465,344 bytes resident and 1,007,714,304 bytes peak working set; these are one observed load,
not a general performance guarantee. Existing resources and producers are reused. Eight explicitly
approved finite sound captures total 28,260,736 raw bytes; acquisition is closed. MainTheme1 uses
existing unchanged PCM with an inferred modern loop. Ordinary defeat uses a clearly authored
local chord asset, preserving the prior accepted presentation choice without an original-audio claim. Scope/ownership expansion and a need for a general
campaign platform return to main-gate. Actual reached failures remain explicit, never no-op success.

Validation uses actual engine behavior tests, affected producer execution, `uv run sf2 verify engine`,
`uv run sf2 verify adapter`, the clean-head dependency plan, scoped CI and native semantic checkpoints.
The bounded implementation, scoped tests, native continuous route and independent implementation
review are complete. Integration is tracked in [PR #1046](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1046);
human-play acceptance remains pending. These results do not establish original-game fidelity or
full-game acceptance, or extend the accepted Battle01 milestone beyond its existing boundary.

Current mandatory acceptance uses default keyboard A under the
[corrected product scope](../../docs/decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope).
Existing C/D observations complete the winning/return route with intact bounded capture and unchanged
inputs. C is supplemental; B/D are historical and excluded from the current gate. D's offline job was
cancelled by scope, with partial evidence retained. The thirteen formerly remaining H4 children now have independently accepted bounded
compositions; the [acceptance map](../../docs/design/synthesis/map3-battle01-readiness.md#current-required-comparison-boundary)
owns their current scope. Independent parent/dependency/shared-join review accepts the complete
current private milestone. Retained A10/matrix and scoped report verdicts remain unchanged. The
separate #517 Option A speech choice is accepted with its controlled reveal-tail evidence. See the
[C/D capture and comparison boundary](evidence/retained-comparisons.md#current-cd-settings-capture),
including preserved failed attempts and the completed local toolchain-provenance failure.

Bound victory/defeat/return programs now share field text/portrait readiness with their genuine
outcome route, anchor and continuation. New outcome worlds initialize their own logical view and
closed windows, preserving carried party/display/RNG. Outcome engine cases cover changed winning
positions, living/dead membership, flag/join ordering, nested map init and usable movement. The
ordinary bound native baseline reaches victory, the complete after/return and settled field movement
through live adaptive commands. Target-range rejections during TAB browsing retain accepted selection,
resources and RNG under the existing menu contract; they remain visible evidence. Executable H4
report verdicts remain distinct from the accepted composition.

The common state-driven session is the only runtime; M5 removed the legacy host and its smoke
drivers. Startup selection stays inline in `GameRoot` because it joins distinct ports and results.

The current product boundary is the common state-driven session described by
[Exploration and programs](application/session-and-programs.md). Live opening-to-castle/palace/Astral/tower
execution replaces the old endpoint transitions as a group. Source identity mapping follows the
accepted default-zero/removed-FF lifecycle. The complete Battle01 before/init/load/start group now
reaches first battle input through the same session, and the selected Battle01 outcome group returns to
usable exploration. Natural original runtime/hardware continuity, broader initialization and other
outcome families remain explicit boundaries. First/repeat visits, choice alternatives and
follower/flag variants are compared through real common commands; native observations use actual
ordinary-host input and a world prepared with the local presentation assets.
See [Architecture](./architecture.md).
