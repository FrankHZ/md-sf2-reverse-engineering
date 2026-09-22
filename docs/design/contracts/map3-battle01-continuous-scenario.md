# Map 3 to Battle 01 Continuous Scenario Contract

- Status: **Accepted comparison definitions**; executable binding and H4 execution are OPEN.
- Accepted evidence base: `58c5a94a4c5349fd221a130a0970222962715528`, including PR #504 at
  `5102804b` (accepted observation source `9c3ea03ac5f5b467ee744f1ac624870da2408443`).
- Product: [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md),
  `1A + 2A + 3A + 4A + 5B + 6A + 7C + 8D + 9A + 10A`.
- Scope: one controlled admission, natural mandatory route, winning battle and post-victory field
  boundary. This composes existing owners; it registers no fixture, schema or research association.

## Evidence and result rules

Original expected values come from the [admission contract](map3-controlled-admission.md),
[Research field audit](../../research/map3-battle01-audit.md#accepted-evidence-and-exact-field-mapping)
and [accepted final acquisition](../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness).
The latter owns the private successful prepared-68..86 lineage, 18 verified parent pairs, actual
input/checkpoint/observer records and read-only reproduction commands. Its source is pinned above;
ROM SHA-256 is `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, upstream
SF2DISASM is `c834c652b6862bc5679fd7f69a38a7093206efc6`, emulator is BizHawk 2.11.1 / Genesis Plus GX.
Private records supply exact per-step values; this document does not manufacture a public golden.
Earlier losing branches and failed requests are excluded from the winning input stream, but their
failures and costs remain with Research. Prepared-86 is terminal/nonresumable; the earlier
first-player-ready terminal is also not a continuation parent.

Each comparison result must identify layer, assertion, original owner/Git object/record location,
logical checkpoint, expected and actual semantic values, input ordinal, remake observation sequence,
and result/reason. Private payloads stay local. Public results expose only safe identities and
bounded differences. The field names below are comparison vocabulary, not a new wire schema.

- `PASS`: both sides exist and the stated assertion holds at its named boundary.
- `FAIL`: an observed mismatch, forbidden transition, runtime error, unsupported required action,
  missing required host completion in an otherwise observable execution, or attributed timeout.
- `Unavailable`: required private input/provenance, original expected field or actual observation
  capability is absent. Identify which side and field; never substitute zero, an empty list or remake
  output. A definition whose original value remains **Unknown** is OPEN, and its execution is
  `Unavailable`, not skipped/PASS. Preserve observed FAILs alongside unavailable assertions.

Every layer is required. Whole-run PASS requires all required assertions and accessibility variants
to pass; report evidence readiness, definition review and actual H4 execution separately. A session
restart, injected story state, reseed, direct battle entry or after-program shortcut after admission
fails continuity. Preserve one session identity and monotonic observations through the complete run.
Rejection/error records must not be discarded. No frame, pixel, waveform, chip or hardware equality
is required; gameplay-affecting timing, causal order and input blocking remain in scope.

## Admitted state and ordered route

Use R1's fixture `sf2-map3-admitted-start-runtime-v1`, case `controlled-new-map3-default`,
at first `WaitForEvent`, through the admission contract's logical projection. Compare map, location,
facing, flow, party/roster, gold, flags, stats, items and spells at their documented widths and meanings.
The [projection limits](../../research/map3-battle01-audit.md#what-the-admitted-projection-does-not-contain)
are binding: public R1 omits status and does not contain four complete two-byte item slots; normalized
time is not raw time. Obtain the winning lineage's inherited status, complete equipment, RNG/copy
and relevant NPC phase from accepted private readback. Do not assume reset POISON=0, use R2d's seeded
actor/order, or impose `0x1234` as the natural seed. Missing readback is OPEN.

The selected order is:

1. Controlled Map3 opening → real messenger/prompt acceptance → follower-ready closure/F603 →
   gate `cs_51652`/F604 → north warp and Map19 init return.
2. Map19 input at `(26,30)` → actual displacement `(26,29)` → royal/guard programs and returns →
   natural castle/tower Maps21/40/57. Preserve each intermediate source operation/warp checkpoint
   from the selected lineage, not just the final map list. Optional interactions/FieldMenu are outside
   this reference route; no mandatory dialogue/choice/trigger may be omitted.
3. CheckBattle/new-battle branch → before `bbcs_01` → LoadBattle → start `ms_Empty`/F451 →
   activation/region/spawn/order generation → actual first actor **2**, `ControlBattleEntity` after
   WaitForVInt/before input. Script `loadMapFadeIn` scene changes are not extra player warps.
4. Selected successful logical decisions through round **14**, turn **103**: movement, STAY,
   physical attacks, HEAL 1 and Medical Herb use, with actual target, resource, scene/effect and
   after-turn results from that lineage. Earlier herb/Heal/defeat branches prove only their own
   values; they must not be spliced into this winning expected stream. One cancel/reselect path
   remains a 4A requirement; an unobserved original branch (including herb cancel) stays OPEN.
5. Natural victory → `abcs_battle01` → shared tail/enclosing return → F401 clear → F501 set →
   BattleLoop D4=1 → SwitchMap return → ExplorationLoop → Map57 `ms_Void` → stable field boundary.

The final original chain confirms victory at frame 58657, after-program return at 60945 and
stable frames 61009/61010. These locate evidence; they are not remake clock targets. There are
**67 matched reached operation pairs**, not 80 dispatcher iterations: the static 80-record corpus
includes embedded entity actions/data. Static R2b/R3/R4a explain rules/structure; only the selected
observations establish natural reach. Savestate-linked verified original continuity does not claim
uninterrupted wall time or a natural New/load flow.

## Ten H4 comparison layers

Inputs to every row are the declared admission, selected successful logical input stream and that
row's accepted original records. Equality applies to semantic values, not object layout or PCs.
The mappings in the next section identify actual existing surfaces and missing coverage.

| Layer | Fields and original expected source | Required assertion against actual remake observations |
| --- | --- | --- |
| 1 — admission | R1 projection plus selected lineage readback: map, player position/facing, party/joined/active lists, flags, gold, each actor's level/HP/MP/stats/status/items/spells, RNG and relevant continuation phase; ROM/source/configuration/parent identities | Exact logical start equality before first user command; validate provenance and complete slots/status separately. Report controlled construction as 1A. Fixed seed must be the accepted lineage seed; unmapped RNG/time dependencies are OPEN. |
| 2 — input/route | Ordered successful input records: checkpoint, logical action, actor, direction, target, choice, accept/reject and resulting movement/transition | Same logical decisions and causal handoffs in the route above. Compare accepted destination/trigger and cancel/reselect effects, not held frame counts. Confirm/Cancel must be user-operable on every ally turn. Missing logical decoding of a physical request is OPEN, never guessed. |
| 3 — world/story | R2/R2a and selected lineage: map/setup selection, program/operation entry-return, dialogue ID/speaker/choice, entity position/facing/visibility, roster/followers, flag before/after | Equal reached state mutations and relative causal order; map load, entity motion and dialogue must finish at their required waits. Compare selected operations, including branches; static corpus membership alone is insufficient. Real YesNoPrompt return is evidence, shimmed DisplayText consumption is not. |
| 4 — natural encounter | Selected CheckBattle/load/start/first-control records; R2c/R2d field shapes only: battle ID, before/start programs, F88/F451, region flags 90–105, party/combatants, position/stats/status/equipment, activation/spawn, turn scores/order/cursor, first actor and readiness guards | Natural route creates Battle01 and completes programs before manual control; compare actual actor 2 and exact selected initialized values. Clear blocking script/modal/transfer/action/target/scroll state. Original window count 2/palette mode 5 are allowed nonblocking presentation, not mandatory host byte values. |
| 5 — battle | Selected action/checkpoint/scene records and R3a–R3d/local rules: round/order/actor/control, movement origin/path/destination, action/resource/slot/target, AI choice/memory, RNG before/range/value/after, follow-up kind, per-target HP/MP/status/death, item removal, EXP/level/stats/spells/gold/drop, after-turn and outcome | Match every reached decision and consumed effect in order, including resource costs and RNG-driven results. Compare HP before WriteBattlesceneScript with consumed EndBattlescene, not temporary script-calculation HP. Pair each RNG draw/effect where evidenced; gaps in draw mapping remain OPEN even if endpoint HP matches. Do not hardcode round 14 or actor history as gameplay legality. |
| 6 — victory/return | Selected final segment plus R4a: winning condition, eligible-party healing, reached after-program operations/effects, joins, F401/F501, controller result, transfer/setup selection | Require natural victory and full reached operation entry/return pairing, shared tail before enclosing return, then clear/set flags, D4=1 equivalent and exploration handoff. `ms_Void` at source `0x477E8` is Map57's exact fallback selection. One completed return cannot replace after-program consumption. |
| 7 — endpoint | Exact bounded values below, pending consumers/readiness and RA-12 ordinary-input evidence | Compare all scenario state and no pending battle/script/modal/transfer; observe settled player/camera across two host update boundaries without inventing original-frame equality. Then accept the independently evidenced nonneutral logical input and compare its effect. Current original nonneutral effect is **Unknown / OPEN**, so bounded neutral readiness cannot PASS full 5B. |
| 8 — save/7C | 6A restart rule; private asset inventory and ROM/source/extraction provenance for every reached original scene dialogue/map/sprite/portrait/animation/music/SFX identity and binding | No user save/load/suspend/checkpoint surface; restart reconstructs layer 1. Every consumed original scene resource resolves to admitted original private content. Missing private input is Unavailable; an authored substitute for required original scene content fails 7C when observed. MUSIC_JOIN/MUSIC_SAD_JOIN chord loops and host mute do not satisfy original audio. Public distribution remains outside scope. |
| 9 — 8D presentation | Reached program/operation and scene/dialogue/animation/audio resource identities, dispatch/consumer/ack boundaries, blocking and resulting state from accepted source plus bounded observations | Match semantic identity and causal order; observe actual host use and completion/ack as defined below. Request/mailbox pairs, program return or a counter alone cannot PASS delivery. Missing original consumption evidence is OPEN; missing host evidence is Unavailable. No screenshots. |
| 10 — deviations | ADR0010 1A/2A/4A/6A/9A/10A; inventory below | Emit a separate named result for every accepted deviation and its expected behavior, even when PASS. No implicit exclusions, missing-input waiver or newly invented deviation. |

Layer 8 distinguishes original scene content from modern interface resources. Under the existing
[presentation owner](../../../remake/docs/presentation-and-assets.md#fonts-theme-and-input-glyphs)
and 9A boundary, modern HUD/theme, semantic input glyphs and fonts are checked for their accepted
authorship/license, admitted asset binding and configured input/accessibility behavior. They need
not be ROM-original fonts or UI resources. This applies the existing boundary, adds no deviation,
and does not waive original scene dialogue, graphics, animation, music or SFX provenance.

## Exact observed endpoint

**Confirmed**, bounded to the accepted final original segment: map **57**, player tile **(5,12)**,
raw position **(1920,4608)**, facing **3/DOWN**, battle sentinel **255**, F401=false, F501=true;
party/joined/active roster **[0,1,2]**, gold **420**. All three status-effect words are zero.

| Actor | Level | HP current/max | MP current/max | Raw item slots | Raw spell slots |
| --- | ---: | --- | --- | --- | --- |
| Bowie / 0 | 1 | 12/12 | 8/8 | `[199,127,127,127]` | `[10,63,63,63]` |
| Sarah / 1 | 2 | 12/12 | 12/12 | `[213,127,127,127]` | `[0,63,63,63]` |
| Chester / 2 | 2 | 12/12 | 0/0 | `[184,127,127,127]` | `[63,63,63,63]` |

Decode item identity with mask `0x7F`, retaining equipped state; 127 is empty. Preserve spell
identity/rank and empty 63 through the existing source decoder, not string/name equality. No herbs
remain. Remaining stats/flags/records must be compared from the accepted private terminal, not
filled from this summary. Player field entity position is not ally battle-record position.
RNG bytes `[188,203,0,0]`, copy 188 and raw time `(frame=170, seconds=674, secondsFrames=40)`
are provenance/readback values: require a justified semantic RNG mapping, not raw host-clock equality.

Movement/camera are settled, map-event word/typewriting/pending returns/active consumers are zero,
and no map program/battle return/transfer/modal remains. Original input polls occur on both neutral
frames; final player poll `0x4FF8` reads 0. Generic window-state byte 1 does not denote a blocking
dialogue. **Unknown:** acceptance and state effect of the next nonneutral input (RA-12); no remake
movement observation can supply that original expected result.

## Mapping to existing actual observations

These read-only mappings describe accepted implementation surfaces, not original evidence or a
claim of complete observer coverage. Reuse them before adding machinery:

| Existing surface | Comparison use and limit |
| --- | --- |
| [SessionContract.cs](../../../remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs) | `CommandEnvelope` SessionId/ExpectedRevision/Actor/Command; `SessionResult` Failure/StopReason/Observations; `SessionSnapshot` Mode/Active/Story/Selection. `SessionObservation` Sequence/Revision/Kind/Actor/Target/Before/After/From/To/RandomRange/RandomValue/Program supplies ordered semantic changes. Match by source meaning; sequence numbers need only be monotonic, not equal original callback counts. Capture every result, not only the latest snapshot. |
| [ExplorationSessionView.ReadObservationJson](../../../remake/game/src/Exploration/ExplorationSessionView.cs) | `sessionId`, `map`, `party`, `partyLists`, `gold`, `flags`, `mainSeed`, `entities` position/facing/moving/busy, `cursor`, `wait`, `token`, `stop`, `battleMounted`, `textId`, `speaker`, `speakerFlags`, `visibleCharacters`, `totalCharacters`, `observations`, failure fields. Normalize `map-57` to source map 57 using content identity. Story wait/cursor absence plus settled entities and released battle view support readiness; they do not alone prove input delivery or all stats. |
| [BattleSessionView.ReadObservationJson](../../../remake/game/src/Battles/BattleSessionView.cs) | `round`, `turnOrder`, `queueCursor`, `actor`, `stage`, `target`, `spell`, `previewX/Y`, `actors`, `mainSeed`, `thinkingSeed`, `gold`, `regionFlags`, `aiMemory`, `observations`, failure fields. Use typed snapshot state for values omitted by host projection. Do not fabricate missing complete inventory, per-draw or scene-consumption records. |
| [ExplorationPresentation](../../../remake/game/src/Exploration/ExplorationPresentation.cs) and exploration projection | `presentation.activeCue`, `completedCueToken/Kind`, sprite request/ready fields, gesture/fade/mosaic counters, `soundStarts/Fades`, `error`. Pair token/kind with the real `CompletePresentation`, actual resource/node and state transition. Aggregate counters do not identify a cue or prove full playback/asset provenance; missing correlations remain OPEN implementation work. |
| [Existing outcome probe](../../../remake/game/probes/engine_battle01_outcome_observation.gd), [9A owner](../../../remake/docs/development-and-verification.md#native-9a-observation) | Reuse actual input, single-session, wait/token, after-program, return and movement observation methods. Their previous PASS is bounded implementation evidence, not this winning original trace or continuous H4 PASS. Physical driver/hot-plug and complete export remain unverified. |

## Presentation and accessibility assertions

For every reached cue, compare `(program, operation, resource, subject, occurrence)` in causal order.
Keep request → actual consumer start/use → required completion/ack → resumed program/input edges.
Different independent concurrent cues need no invented total hardware order. Unknown dependencies
remain explicit. A presentation wait may end only with its matching token/kind and required consumer
completion; a scene's committed HP/resource effect must appear at the corresponding resolution edge.

- Dialogue: compare text ID, speaker/portrait and choice result, actual displayed private content
  binding, reveal completion, acknowledgement and wait release. The [Research presentation audit](../../research/map3-battle01-audit.md#presentation-sufficiency-under-8d)
  records `display-text-rts` at `0x6260`; IDs/returns do not prove unshimmed original acknowledgement.
  That missing original boundary stays OPEN even when the remake Label works.
- Animation/scene: compare source entity/resource/action identity, visible host consumer and required
  motion/gesture/fade completion before its dependent program or input resumes. A culled/absent
  subject must follow an evidenced semantic rule, not silently count as rendered completion.
- Audio: bind command namespace/resource to the private original asset and actual player start;
  require reached replacement/fade/stop/resume edges. Persistent music continues until its required
  replacement/stop; no fictitious track-ended event. The final segment's **106 dispatch/mailbox
  pairs** establish that bounded source seam only, not complete consumption or 7C provenance.

Battle scenes require their own consumer evidence. For each reached action/target/follow-up, bind
the accepted `InitializeBattlescene` → `ExecuteBattlesceneScript` → `EndBattlescene` observations
to the [battle-scene contract](battle-scene-presentation.md): scene occurrence, actor/target,
background/actor/weapon/spell-animation resource IDs, ordered scene command and operands, dialogue
identity where reached, HP/MP/resource effect edge, and blocking/wait/completion boundary. Actual
host evidence must identify the corresponding mounted scene/resources, started and completed
animation/dialogue consumers, matching completion token or equivalent occurrence identity,
committed effect and return to battlefield input. Assert initialization before command consumption,
each required wait before dependent progression, consumed effects before EndBattlescene-equivalent
release, and no input release while blocking scene work remains. Missing original resource/order
bindings remain OPEN; missing actual observation is Unavailable, while observed omission of a
required scene is FAIL. The current [BattlePresentation](../../../remake/game/src/Battles/BattlePresentation.cs)
projects board markers/status/roster only: it does not prove battle-scene animation or dialogue
consumption. This is an explicit layer-9 implementation gap, independent of the exploration cue
services, battle-state correctness and private-audio work. No original pixels or frame durations
are required to close it.

9A runs report separate variant identities/settings and compare the same layer 2/3/5/7 gameplay
decisions/state against the baseline: default and remapped keyboard/gamepad, standard and swapped
Confirm/Cancel, reduced-flash and normal, instant and adjustable text. Report any unexecuted required
variant as Unavailable. Swapping changes physical bindings, not command meaning. Reduced-flash must
suppress the white overlay yet complete the same token/kind and state effect. Adjustable text's
first Confirm reveals unfinished text without consuming the wait; subsequent Confirm acknowledges;
instant text still requires acknowledgement, and choices preserve Yes/No. Original device cadence
and text duration are not equality fields. Observe actual host settings/input/projection, not merely
direct SessionCommand injection. Existing authored paired observations do not stand in for continuous
variant execution.

Layer 10 separately reports: controlled construction (1A/layers1–2); excluded optional interactions
with mandatory route retained (2A/layers2–3); fixed evidenced seed/logical trace with manual agency
and no live reseeding (4A/layers1,2,5,7); absent save surfaces and restart equivalence (6A/layers1,8);
all 9A variants and their acknowledgement/state equivalence (layers2,3,5,7,9); and each explicitly
identified out-of-domain safe/Unsupported behavior (10A/affected layer). Out-of-domain safety cannot
waive an in-domain required action. Private-only 7C handling is a product boundary, not a deviation.

## Remaining acceptance work

**Unknown / OPEN original fields:** complete selected logical trace/seed/draw-to-effect projection
where not yet bound to comparison records, missing status/continuation fields, cancel/reselect,
unshimmed required dialogue and other incomplete 8D consumer boundaries, and RA-12 nonneutral effect.
This is a field-specific evidence/adaptation requirement, not permission for another native run.
Accepted [PR #519 source preparation](../../research/map3-messenger-acceptance.md#post-victory-ordinary-input-preparation-issue-515) supplies collector capability, not a native RA-12 result. Nonneutral acceptance/effect remains OPEN; future Research #515 observations enter only after independent acceptance on main.

**OPEN content/implementation:** 7C audio and complete reached asset provenance, manual required
Medical Herb input (the inspected `SessionAction` exposes only Stay/Heal/PhysicalAttack), any missing
snapshot/cue correlation, actual battle-scene consumers above, continuous comparator and all
applicable actual host/9A executions. Audio/host work (#517) and Medical Herb (#518) are separate
implementation owners; their unmerged results do not close these entries.
Do not use current remake limitations to remove reached actions from the expected contract.
Independent review accepts these comparison definitions with their precise open boundaries; this
does not establish complete definition readiness or milestone readiness.
The [readiness ledger](../synthesis/map3-battle01-readiness.md) tracks closure; main-gate independently
reviews acceptance. No executable H4 implementation, native acquisition or suite runs occur here.
