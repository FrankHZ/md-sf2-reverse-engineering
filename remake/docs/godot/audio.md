# Audio Delivery and Source Boundaries

The ordinary `SessionAudio` adapter consumes admitted private audio and semantic session cues.
Application retains logical waits and completion authority. This owner records the reached audio
policies, source provenance and limits; [asset admission](../content/presentation-assets.md) owns
private payloads and [retained comparisons](../evidence/retained-comparisons.md#scoped-audio-consumer-comparison)
owns the bounded evidence.

## Audio Boundary

Music and sound effects follow the same explicit local-pack rule as graphics. Their source and accepted
runtime forms are versioned in the local asset repository. The product uses the user's current
material; it does not generate replacement tracks or substitute sound effects. Application emits
semantic audio cues, and the Godot audio adapter resolves those cues to admitted local asset IDs.

The private audio implementation uses the existing version-1 pack with `kind: audio` entries.
Each entry retains its original command, semantic cue, source capture identity, derivation identity,
and one PCM16 WAV runtime payload. `timerB` identifies the original scheduling context. Music commands
are unique; finite SFX may have multiple reviewed `(command, timerB)` variants. Cue IDs remain unique.
Preflight verifies the pinned local Git payload, SHA-256, rate, channel count, sample-frame count and
explicit loop range. Missing content is an error; the former synthesized JOIN chords are removed.

`python -m sf2tool.remake_audio --help` describes the offline candidate materializer. It accepts a
reviewed capture SHA-256, command/timer context, explicit inclusive/exclusive sample-frame interval,
and optional loop boundaries. It creates a new candidate under the worktree's ignored `local/`
directory. It neither launches an emulator nor promotes assets. Original captures, native observer
configuration, failed attempts and runtime WAVs remain private. The source capture and its observed
boundaries must be reviewed before the existing local asset admission transaction.

The compiler embeds the admitted PCM and metadata in the private world. Content checks its identity
again. The Godot session owner creates `AudioStreamWav` resources at their admitted rate and channel
layout, with no gain normalization or pitch change. Streams are resident; resource ownership survives
exploration/battle view replacement. Looping is enabled only when both admitted sample boundaries
are present. Natural `Finished` and deliberate replacement/stop remain distinct observations.
`GameRoot.ReadAudioObservationJson()` exposes the live players and the last 64 ordered start/stop/
finish receipts, including PCM identity, command/timer, loop range, session revision and wait token.
Inspectors must collect these during execution; sequence gaps are not complete playback evidence.

**Confirmed source structure:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`sounddriver.asm:Load_Music` writes the music header's Timer B value; `Load_SFX` initializes channel
records without replacing it, and `UpdateSound` advances them under that timer. Note-frequency
register writes use separate frequency tables. SFX variants therefore retain the reached timer
context; `pitch_scale` is not a tempo substitute because it would transpose the sampled sound.
The existing [sound inventory](../../../docs/research/sound-data-inventory.md) owns the ROM/source
parity, command identities, type-specific channels and stream termination facts.

**Confirmed in a bounded private host observation:** JOIN opens its text window, waits for the
actual finite stream to finish, reissues previous music, and then accepts acknowledgement. The
source `csc08_joinForce`, `FadeOut_WaitForP1Input`, `PlayMusicAfterCurrentOne` and
`ApplyFadingEffectAndZ80BusUpdate` support this order. The source previous operation reissues the
prior command; it does not restore a PCM offset. Map-area field/battle music selection uses imported
area music and `PlayMapMusic` substitutions. Application retains wait tokens and player input
ownership; Godot reports service completion.

The selected inventory contains six music resources and 27 finite SFX timer variants. The additive
continuous world is 103,329,576 bytes, within the existing 100 MiB reader limit. Other package limits
are unchanged. A WASAPI Godot run observed Town's natural forward-loop
wrap while `Playing` stayed true without `Finished`, then ordinary Map 3 inputs reached JOIN.
An early acknowledgement retained the sound wait token. The finite stream ended naturally;
previous Town restarted at its beginning before acknowledgement returned to the field with flag 603.
The ordered player receipts were start 8, stop 8, start 19, finish 19, start 8. Native exit was zero.
This checks real resource playback and input ownership in that bounded route; it does not establish
original waveform/timing equality. The selected private resource inventory is admitted at local asset
commit `293c9460818f4768905be809e83488d77f698fba`, tree
`70196dde88e94b52bc482ecd2a8c8662c743787b`, manifest SHA-256
`C3951D26CE7E05996584A959AC9177A79DC6FF933F249B3766651B337A36326E`;
checkout and export preflight passed. Complete reached 7C provenance remains an H4 obligation. Its tracked
`manifests/audio-town-join-provenance.json` retains original capture identities, sample cuts,
reproduction parameters, loop evidence and the host receipts. Source WAVs and capture observers
remain in that private repository. Use the explicit asset commit/tree/manifest pins with
`python -m sf2tool.remake_assets checkout`; the temporary world is not an implicitly selected pack.

`manifests/audio-reached-inventory-provenance.json` owns the remaining capture protocols, exact
sample cuts and actual host receipts. Music commands 2, 5, 34 and 38 passed actual forward-loop,
continued-playing and clean-release observations through explicitly controlled field-music selections.
That tests their resource consumption, not natural battle-action triggers. The finite SFX variants
passed original driver termination, native lifecycle, complete WAV and unchanged-installation checks.
Their cuts retain unchanged PCM, including 441 samples before and after the selected audible interval;
the measured quiet separation permits at most one PCM16 unit. No normalization, noise gate or waveform
rewrite is applied. Initial-context variants retain their explicit inferred Map 3/Town context.

**Confirmed exact speech additions:** command 70 (`SFX_DIALOG_BLEEP_2_TIMER_BD`) and command 73
(`SFX_DIALOG_BLEEP_5_TIMER_BD`) retain inherited Timer B 189 from original music 34. The isolated
finite protocol dispatched at frame 243, observed channel 8 termination at 249 and all 13 channels
inactive through the 60-frame tail ending at 309. Pinned source spans end with `FF 00 00`; observed
cursors 6022 and 6055 match that terminal. Independently reviewed cuts retain samples
`[179311,187805)` and `[179313,187892)`, respectively 8,494 and 8,579 stereo PCM16 frames at
44,100 Hz. Both candidates equal the original selected sample bytes; no resampling, gain or pitch
change. These two isolated exports cost 49.3339547 seconds and 2,400 dump frames; retained natural
route/RNG accounting is unchanged. They do not prove natural original timing or waveform phase.

The private admission appends only two source WAVs, two runtime WAVs and their records in the two
existing manifests; all prior resources, provenance and host history remain unchanged. The selected
continuous world adds only those two audio rows, with its other content and old PCM/modernEndStep
values directly equal. Completed A/B/C observations retain their earlier selection: their actual used
rows are unchanged and they did not invoke these previously unresolved pairs. Partial receipt windows
are not claimed as an exhaustive trace; the existing exact-or-unique selector would reject those
requests in the old catalog. The D selection explicitly contains the two additions.

**Confirmed bounded D consumer:** natural adjustable20/remapped-gamepad/swapped/reduced-flash D
completes the ordinary winning route and settled Left/Down return without an added audio Wait.
Actual receipts match requested BD, asset BD, format and PCM identity: command 70 has 23 starts,
22 legitimate replacements/stops and one `Finished`; command 73 has eight starts, seven replacements/
stops and one `Finished`. All 31 starts are paired, with no sampled sequence gaps. Replacement is the
existing whole-clip covered-slot policy; these results do not claim original per-channel mixing.
The earlier D attempt's `audio-command-ambiguous` failure remains preserved; selector/runtime policy
is unchanged. Unvisited playback and full 7C/H4 bindings remain open.

Ordinary accepted field warps publish `warp-started` before transfer; rejected destinations and script
transfers do not. `WarpIfSetAtPoint`, `ProcessMapEventType1_Warp` and the source `WARP_SFX` reset own
this distinction. Audio consumes that origin marker once, before selecting destination music.
An actual R1 input prefix observed command 89 starting once in Town's inherited Timer B context.
The existing `door-opened` result selects command 92. Reattaching a result does not replay its effects.

Adjustable text consumes the imported sprite speech identity on alternating non-space revealed
characters; acknowledgement and choices consume command 67. Accepted battle spell/item selection
and cancellation use 66, and confirmation uses 67. These are modern UI bindings, not reproduction of
every original menu/window edge. An authored two-text adapter fixture with the original Bowie
sprite/speech binding observed command 73 replacement and natural `Finished`, plus ordinary Enter's
67, under both CB and D2 timer contexts. It collected every audio receipt without sequence gaps.
The full inventory includes the original Woman sprite 195's 72/CB voice; a failed reduced fixture
omitted that entry while incorrectly assuming the entity was Sarah. That fixture failure does not
establish a missing reached asset. The complete pack and source-derived speaker metadata agree.

### Accepted fast-text speech policy

The user accepted [the 9A/10A fast-text omission](../../../docs/decisions/0010-map3-battle01-product-acceptance.md#accepted-fast-text-speech-omission):
instant text and reveal-all input skip speech for omitted character reveals. Reveal itself leaves
already-playing tails intact; later legitimate cue replacement still applies, including normal
source-defined confirmation replacement. Normal source-specific confirmation cues remain required,
without adding67 to every Ack. Reveal-only emits no Ack, confirmation cue, tick/RNG advance or wait
for omitted speech, and Ack does not wait for a tail to finish. Adjustable text retains the configured
modern reveal speed and imported speaker identity. These rules use the existing adapter and whole-PCM
replacement policy; they do not introduce another cue or audio scheduler.

**Confirmed bounded actual coverage:** the accepted A-D settings matrix compares the executed route's
semantic inputs, ordered logical observations and gameplay/RNG state. C contains84 synchronous
early Confirm inputs with unchanged token, revision, tick, RNG and input state, and no submitted
result; D confirms naturally delivered text. New A's instant session has a gap-free1049-receipt
audio stream with no speech commands70–74 and51 starts of67 (36 replacements/stops and15 natural
completions). These totals prove bounded consumption, not a cue-to-Ack binding for every occurrence.
D's62 speech receipts cover70/73 at BD:31 starts,29 replacements and two natural completions.
The [settings and same-session observation owners](../evidence/retained-comparisons.md#same-session-actual-observation)
retain the selected inputs and reproduction routes; these observations do not backfill old sessions.

**Unknown actual reveal-tail interval:** C's synchronous input records carry gameplay fields but no
before/after sound identity. Its sampled audio retains391 distinct receipts, including74 playback
and confirmation67;47 early inputs have a same-token/revision sample with a playing voice. Such samples
cannot prove that the same voice remained playing across reveal. A sparse later `Finished` is not
sufficient. That historical C claim therefore retains this observation limit: a
bounded actual reveal while speech is playing would need the same startSequence/voice before and
after, no reveal-induced stop/restart/new speech or67, and actual later completion or legitimate
replacement. No new acceptance family or route/settings rerun follows from this limit.

**Confirmed controlled reveal-tail mechanism:** the accepted
[consumer boundary review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/600#issuecomment-5957595317)
at commit `1c26b48640b1ef16401d23893e05a237a26ec9c5` independently read the real window-06 reveal:
the same speech voice startSequence2/cue/slots remains Playing before and after input, with unchanged
logical context/result range, no new dispatch receipt, and one later natural Finished at sequence3.
This disclosed controlled program establishes the mechanism; it does not fill C's natural interval.
Direct readback verifies the voice PCM against the current selected world. RevealText,
SpeakRevealedCharacters and HandleAction are unchanged since that accepted witness. Later capture,
resource and view observations do not alter those input/speech methods; SessionAudio's additions
record immutable receipts/session identity and expose state without changing replacement or callback
behavior. Reuse this evidence when those dependencies remain unchanged; a commit change alone does
not require another native observation.

The [continuous contract](../../../docs/design/contracts/map3-battle01-continuous-scenario.md)
reports the accepted omission independently of state equivalence and actual lifecycle coverage.
Normal-reveal speech, actual completion and7C provenance remain required where applicable; unobserved
coverage stays Unavailable. Historical CB/D2 speech consumption PASS is not state-equivalence evidence,
and the completed instant-fixture observation failure remains preserved. Full H4 remains incomplete.

Town's inferred PCM loop uses an exact recurrence of all ten original driver channel records at
frames 2757 and 8618 in the controlled export. The reviewed candidate loop is sample frames
1,894,900 through 6,208,289 (exclusive), at 44,100 Hz stereo PCM16. The export bridge's startup
buffer and possible adaptive resampling prevent claiming exact hardware sample phase. Reproduction
requires the retained private capture/configuration, candidate manifest and ordinary-input host
observation; neither the loop interval nor a waveform seam metric alone proves the host result.

### Ordinary door obstruction and audio

For source-populated exploration, entity obstruction is checked at the resolved candidate position
before door copying, ordinary warp dispatch or movement. The mover's `FlagsA` bit5 enables the
check; other host-visible entities obstruct only with bit7 set and either their current or reserved
position within strictly 256 fixed-point units on both axes. The existing `EntityMotion` predicate
owns that geometry. A rejected move updates facing but leaves the door layout unchanged and emits
no `door-opened`, so it cannot request sound92. A clear approach still copies the door before map
traversal, then selects any step event from the copied layout. Non-populated authored maps retain
their existing occupancy/warp policy.

**Confirmed source structure:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/common/scripting/entity/entityscriptengine_2.asm:esc02_controlCharacter`,
`loc_51A8` through `loc_5218` checks mover bit5, other bit7, current and reserved positions before
`loc_5278` calls `OpenDoor` for marker `0x0400`. `code/gameflow/exploration/exploration.asm:OpenDoor`
applies the matching copy before requesting92. The source scan itself uses flags and coordinates;
host `Visible` filtering remains the existing engine abstraction, not a claim of a separate original
visibility predicate. Removed entities are off-map; natural reach of a particular obstructed doorway
remains **Unknown**, while changed valid engine states are covered directly.

**Confirmed bounded consumer:** `engine_private_exploration_observation.gd` case `map3-door`
uses a controlled ordinary approach from Map3 `(4,7)` to the existing door at `(4,8)`, the unchanged
private audio pack and real key input. It observes one `door-opened`, one playing92 at inherited
TimerB `0xCB`, natural `Finished`, post-copy traversal/input availability and no replay on redraw,
repeated projection or revisiting the opened doorway. It does not run the full Map3-to-battle route
or establish original timing. Engine cases vary positions, current/reserved obstruction, the strict
distance threshold, disabled flags, hidden/retired entities, non-doors and repeat-open behavior;
blocked attempts preserve layout, gameplay time and RNG. SessionAudio's existing event consumer is
unchanged. This slice does not close other cue bindings, JOIN or gameplay-opportunity differences.

### Action-choice menu audio

A successful battle submit that enters or leaves `BattleSelectionStage.ActionChoice` plays command
65 once through the existing UI audio callback. This boundary takes precedence over generic 66/67
selection/confirmation sounds: entry, cancel back to movement, and selection of Stay or a target
action all use 65. Rejected submissions, accepted operations that remain in the same stage, and
redraws do not add a menu-boundary cue. Other existing UI bindings are unchanged. The rule uses
before/after selection state for any actor; it adds no engine event, window RNG or gameplay delay.

**Confirmed source structure:** pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`,
`code/gameflow/battle/battlefunctions/battlefunctions_2.asm:ProcessBattleEntityControlPlayerInput`
selects the outer action menu after movement/stop validation. Its `ExecuteDiamondMenu` caller in
`code/common/menus/diamondmenu.asm` uses `MoveWindowWithSfx` for both opening and A/C-confirm or
B-cancel closing. `code/common/windows/windowengine.asm:MoveWindowWithSfx` submits command 65
before moving the window; directional menu selection's 66 is a separate edge. The retained original
65 request PC `0x48F4` identifies this shared window routine, not all its parent callers. No original
request-count target or cue on every dialogue/window follows from it.

This is the current modern ActionChoice lifetime's source-backed audio mapping, not complete
original diamond-menu/window presentation, nested menu behavior, timing or input-polling parity.
The consumer uses the already admitted original 65 resource and current music timer context.
Set `SF2_BATTLE_SCENE_MENU_AUDIO=1` with the source-world scene observer below to exercise ordinary
entry, cancel/reentry, Stay, target selection, rejected and same-stage inputs for two actual actors.
The observer checks exact per-input starts, the actual 65/BD player, natural `Finished`, legitimate
same-slot replacement and no redraw replay or gameplay work during playback-tail collection.
Normal/reduced-flash runs preserve the same semantic inputs and ordered session observations;
this bounded check does not establish the separate instant/adjustable text or full host Option A
requirements. Missing or ambiguous SFX content remains an explicit error under the finite selection
policy below.

### Finite SFX overlap

The session audio owner uses a modern whole-PCM mixing policy: disjoint and partially overlapping
groups continue playing together; equal or fully covered groups replace the old clip. Partial
overlap preserves the old whole clip and starts the new one, so a Herb tail cannot block ordinary
menu input. Repeated requests replace their previous group instead of accumulating voices.
An admitted command without a
reviewed slot classification fails with `sfx-slots-unavailable`. Missing or ambiguous content remains
explicit. No extra action wait or gameplay tick is introduced for a cosmetic effect's tail.

Finite SFX selection prefers the exact command/current-music Timer B pair. If that pair is absent,
the host may reuse the unique admitted finite PCM for the same command. Zero candidates fail as
unavailable; multiple candidates fail as ambiguous. Named resource playback follows the same rule
when its timer differs. This accepted modern approximation changes neither music selection nor PCM,
cue identity, recorded Timer B, sample rate or pitch. Playback receipts retain the asset's `TimerB`
and separately record `RequestedTimerB`. It does not change the original `Load_SFX` inheritance fact.

**Confirmed source structure:** pinned `sounddriver.asm:Load_SFX` initializes only non-FF channel
entries, and its type-2 effects have separate temporary YM records. `StopMusic` preserves these
extra records. The existing H2 sound inventory's `sfxModel.entries.activeSlots` supplies the bounded
classification in `SessionAudio`; command names and type numbers alone are insufficient. Commands
83 and 102 occupy YM6 and PSG tone3 respectively; 113 occupies YM4/5 while 67 occupies PSG tone3.
The former single player truncated 83 on level sound 102 and recovery 113 on confirmation 67 in
retained ordinary controlled scene observations. These are independent cue-lifetime defects, not
waveform-fidelity requirements. Classification of PSG noise and tone slots expresses replacement
identity only; it does not prove acoustic independence or reproduce their chip coupling.

`ReadAudioObservationJson()` includes each live effect's cue, command, source slots, actual
`Playing`/position and start receipt sequence in `sounds`. Every voice retains its own actual
`Finished` callback. Already-ended voices await that callback; `Playing == false` alone is not
reported as completion. Replacement and disposal disconnect callbacks and release streams/nodes.
The receipt window remains bounded; inspectors must collect it without sequence gaps.

For the existing controlled physical reward and Herb source-world observation modes below, set
`SF2_BATTLE_SCENE_DISJOINT_AUDIO=1`. The observer samples actual simultaneous players and checks a
pair of independently completed instances (83/102 or 113/67). Rapid later tone input may replace a
tone; it must not truncate the independent reaction sound. After product input release, the observer
collects the playback tail and requires unchanged gameplay state. This is observation latency,
not a product wait. Pair normal/reduced-flash runs using the same semantic inputs and compare their
session observations; no full host Option A conformance follows from an audio overlap check.

**Confirmed, bounded actual consumer observations:** the controlled physical reward and Herb modes
each passed normal/reduced-flash WASAPI runs with concurrent players and independent natural
completion. Each settings pair produced identical ordered session observations and final shared RNG;
playback-tail collection left the input-ready gameplay state unchanged. An independent adapter-only
owner additionally observed reverse arrival, starts after completion, same-slot/full-coverage
replacement, preserved playback on unknown/missing rejection, duplicate-result
suppression and stream/callback disposal. It reused admitted PCM and did not mutate a game session.
PSG noise/tone coverage was a slot-lifetime check only. Earlier observations that ended before the
queued `Finished`, or required a rapid UI tone to survive legitimate same-slot replacement, remain
failed observations; no engine or source rule was changed to satisfy them.

**Accepted modern approximation:** source slots are inexpensive replacement groups, not hardware
output masks. Partial overlap can retain a source voice that the original would replace. Exact
channel masking, chip coupling, sample-perfect mixing and live timer changes within a PCM clip are
outside the current audio goal and do not block the playable milestone. The proposed #554 stem/core
investigation was abandoned as unnecessary under this policy, not delivered.

`SF2_BATTLE_SCENE_PARTIAL_AUDIO=1` observes ordinary Herb-to-menu overlap, actual completion and
unchanged gameplay during tail collection. Independent adapter observations cover partial overlap,
rapid bounded replacement, full/disjoint groups and disposal.

### Sound-fade request and effect lifetime

**Confirmed source structure:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`disasm/code/common/tech/interrupts/applyfadingeffectandz80busupdate.asm:ApplyZ80BusUpdates/@IsFadeOut`
forwards command253 asynchronously; the F0 `WAIT_FOR_MUSIC_END` gate is separate.
`disasm/code/common/tech/sound/sounddriver.asm:Fade_Out`, `UpdateSound` and `StopMusic` distinguish
shared music/type-1 records from the extra type-2 records. `PSG_ParseToneData:loc_DF2` excludes tone3
from incremental attenuation; `PSG_ParseNoiseData:loc_F88` attenuates noise. Type-2 YM/DAC paths bypass
fade attenuation and survive StopMusic. `Load_Music` stops shared records and resets fade state;
`Load_SFX` does not. Same-music suppression can prevent that reload. These named source seams establish
structure, not natural caller reach, original fade duration or a reproduced audible defect.

**Implemented bounded modern policy:** the existing half-second service ramps whole music/noise PCM,
leaves tone3 gain unchanged until terminal stop, and preserves type-2 lifetime. Admitted tone3 commands
are66/67/70/72/73/74/79/102, noise89, and type-2 commands65/77/81/83/92/113/116. New noise inherits active
fade gain. Repeated requests/progress cannot brighten playback; each actual begin records253 even
without playing music (asset metadata is then null). Actual new music cancels the old fade and stops
shared effects; suppressed same-music requests preserve it. Later canceled progress cannot stop the
replacement. Terminal stops do not manufacture `Finished`; already-ended players retain queued actual
callbacks. Existing whole-clip replacement and disposal rules remain in force.

The generic `SoundFade` presentation issues one begin per token and retains its existing completion
and input ownership. Battle entry/end retain their own existing service boundaries. Neither wait is
proof that original253 waits: `InitializeBattlescene` requests253 then performs background/palette
work, while `EndBattlescene` requests253 and closes windows. No asynchronous source-script mapping or
logical clock/RNG policy is added here.

**Confirmed bounded actual consumption:** independent Godot/WASAPI players with unchanged admitted
assets cover gain/terminal stop, effects started during fade, surviving type-2 natural completion,
no-music/already-ended cases, repeated begin, suppressed same music, replacement interruption, bounded
rapid replacement and disposal. The tracked `engine_private_exploration_observation.gd` case
`SF2_PRIVATE_EXPLORATION_CASE=sound-fade` takes an explicitly authored host resource through
`SF2_FADE_FIXTURE_HOST`: pending SoundFade, early ordinary input blocked, actual gain/stop, one completion
and one253 despite redraw. Its host reuses admitted content in memory with PresentCue(SoundFade),
WriteFlag(7,true), EndProgram. The existing host continues simulation ticks during presentation;
those observations remain explicit, and RNG is unchanged. This is authored service coverage;
no naturally reached generic SoundFade instruction was established in the admitted source world.

The [composed audio consumer binding](../../../docs/design/contracts/map3-battle01-continuous-scenario.md#composed-reached-audio-consumer-binding)
now associates the complete selected A playback inventory with legitimate replacement/fade causes
and the actual scene/JOIN releases. Source slot headers are read from the pinned original driver,
using the maintained sound inventory parser. A cue with one outstanding start has a unique actual
terminal association; current service tokens may change while its tail plays. No new playback ID is
needed for that selected channel. Ambiguous simultaneous same-cue starts remain Unavailable.
The terminal looping battlefield music needs no invented end. Finite effects retain real Finished
callbacks or legitimate covered-slot replacements, including tails surviving shared music release.

**Incomplete:** original hardware/per-channel timing, historical C reveal intervals and other H4
consumer families remain outside this A binding. The
[physical scene consumer](battle-scenes.md#physical-battle-scenes) implements a bounded battle-action music/SFX and
input-release subset; the bounded HEAL consumer is described below. The modern half-second service
is not source-timing evidence. Neither successful resource
requests, counters nor a build proves actual audible consumption or continuous 7C/8D/H4 acceptance.
JOIN uses the [accepted modern finite-music clock](../../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
in addition to actual PCM completion. Explicit `modernEndStep` metadata belongs to Application
content; SessionAudio binds actual playback to its semantic generation and cue. The host's finite
completion receipt never creates logical work. Previous music becomes logically eligible at the
gate sample but actually restarts after both the complete helper group and whole-PCM finish.
Duplicate current requests preserve playback/generation; changed active-helper playback reports an
error. No source clock claim follows from the profile or actual callback. Field-Wait eligibility and
PCM duration cannot supply an original clock binding. The accepted
[fast-text speech omission](#accepted-fast-text-speech-policy) retains its separate deviation report,
same-semantic-Wait/acknowledgement comparison and bounded actual coverage limits.
