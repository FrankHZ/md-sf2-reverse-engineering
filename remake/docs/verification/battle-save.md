# Battle Save/Continue verification

The [High-rigor persistence agreement](../application/battle-save-and-continue.md) owns the
product boundary. These observations support #1638: explicit save at settled player Movement,
real process exit, fresh title Continue at that turn and equivalent later play. They consume A's
engine and B's codec/store evidence; they do not replace those checks with another comparator.
Independent integration belongs to main-gate.

## Method and reproduction

Reuse the owning worktree, configured private inputs, existing imported Debug project and registered
Godot installation. Load private/tool configuration in the launching PowerShell process and apply
the [locked SDK workflow](../development-and-verification.md#locked-net-workflow). Required inputs
are the accepted eighteen-map/seven-encounter world, six battle/static inputs, first encounter's
accepted battle-scene resources and title presentation. `OpeningRestoreReader` admits immutable
definitions without initializing party/growth. No world, WAV, installation or player save is copied.

Select a **new external disposable** `SF2_SAVE_ROOT` and namespace. Never use a player's files.
The bounded [native observer](../../game/probes/engine_battle_save_observation.gd) instantiates the
ordinary host and sends mapped keyboard events through `Input.parse_input_event`. Normal title
Continue, menu Save, confirmation and actual presentation consumers do the work. The observer does
not install a live snapshot or submit synthetic presentation/audio completion. Synchronous attach
observation captures `ReadPersistenceObservationJson` before the first input or automatic callback.
It reads actual input/state/projection/audio/error observations; no screenshots are used.

After the current configuration and protected Debug build, use a fresh ignored output directory:

```powershell
$env:SF2_SAVE_CASE = 'resume'
$env:SF2_SAVE_REPORT = Join-Path $runRoot 'native.json'
# For pilot/vertical/boundaries only: an explicitly controlled field approach file.
$env:SF2_BATTLE_SAVE_START = $controlledFieldInput
& $env:GODOT_BIN --headless --path remake/game --audio-driver Dummy `
  --fixed-fps 60 --resolution 1920x1080 --log-file (Join-Path $runRoot 'godot.log') `
  --script res://probes/engine_battle_save_observation.gd -- --input-settings $inputSettings
```

Select English, instant text and reduced flash for indexed input steps; `shape` also runs with
Chinese UI keys. Headless checks do not take the user's foreground. The observer uses bounded wall
time while waiting for detached work/native audio; fixed simulation frames alone can expire before
a real worker or mixer completes. Each owned process is bounded to 55 seconds and launched serially.
All stdout/stderr and explicit Godot logs remain worktree-local. Before rerunning, inspect completion
and preserve failed results. Do not kill unrelated native processes or shared services.

Controlled preconditions are distinct from the ordinary input proof:

- `pilot`, `vertical`, `boundaries`: the existing `StartPreVoyageLocalizationObservation` admits
  a New Game party at Map57 `(8,14)`, facing3, speed32, flags
  `0,1,2,32,33,34,66,80,401,451`. Ordinary battle admission, waits, native audio and player input
  follow. This is a controlled field approach, not natural title-to-tower reach.
- Seven `shape` slots: use the accepted `BattleSaveProfileTests` construction with the actual
  initialized encounter/route, complete active and retained roster and known settled context.
  Capture with the real finite API, validate with no-init restore, then publish through the shared
  store before startup. Each native process uses normal Continue, Save/return and its next action.
- `resources`: on the same admitted Battle01 definition, set Sarah's initial HP1, Bowie's HP6 and a legal queue
  starting Sarah, Chester, Bowie, then the remaining enemy entries. Preserve immutable deployment
  order, accounting, source loadouts, seeds, regions, flags and retained party. The native UI casts
  HEAL on Sarah, uses Chester's herb and saves after those real payments and queue consumption.
- `victory`/`defeat`: validated detached Battle01 inputs put the final live enemy adjacent to the
  current player at HP1, or put a live enemy next to HP1 Bowie with the other allies defeated and
  the enemy next in the queue. Actual attack/AI, death, award/outcome programs and return consumers
  run normally. `retreat` uses the admitted Battle02 Egress capability with Bowie acting first.
  These are focused controlled endings, not natural whole-battle reach or new capability admission.

Other cases are `busy-scene`, `native-history`, `overwrite-cancel`, `storage-fail`, `uncertain`,
`uncertain-close`, `unknown`, `backup-cancel`, `backup-recover`, `save-quit`, and `field-continue`.
For recovery/failure cases, prepare only new disposable namespaces with production-valid records.
Keep a valid backup, replace primary with invalid bytes or a checksum-valid future-version payload,
then drive the real menu. Hold `slot1.lock` with FileShare.None for a pre-publication error; hold
primary read-only without Delete sharing for a real Windows Replace-attempt uncertainty. Release
only this observer's file handle after the owned process exits. `uncertain-close` calls the existing
WM-close notification handler during publication; it is not a physical OS-close claim.

Resource scope is bounded: one existing approximately125 MB world per content admission, serial
processes, no acquisition/network/model cost or content cloning. B's seven payloads are18–22 KB,
no-init admission834 ms and process peak644 MB, within the existing3 GiB content budget. The native
cases retain selected samples only. Including failed runs, the initial C JSON evidence measured
35.7 MB; keep the slice's observation/log headroom within64 MiB and reassess before adding more
cases. No frame history, full-campaign replay, cache or compression platform is needed.

## Confirmed observations and limits

| Boundary | Actual result |
| --- | --- |
| Ordinary Save/return | Battle01 controlled field approach enters the real battle. Menu binds actual battle-context history, labels encounter/round/actor/time, confirms the selected slot and returns to the same usable Movement boundary. No forced cancellation or queued save. |
| Paid resources and restart | Native HEAL changes Sarah HP1→11 and MP10→7; Chester's herb changes inventory `[184,0,127,127]`→`[184,127,127,127]`. Save occurs with queue cursor2 at Bowie's Movement, Bowie still damaged at HP6, and earned EXP22/1 for Sarah/Chester. The uninterrupted suffix and genuinely fresh Continue have exactly equal finite payloads before first input and after eight legal Stay turns, including AI, resources, queue and next-round seeds. |
| Real termination | The resource control process PID36116 performs Save/return. A separate ordinary Continue process PID14408 chooses Save and Quit, verifies publication and exits0. New PID41776 uses normal title Continue. Its pre-input and suffix payloads match the control; final seeds are1449438157 and44813807. Repeated independent Continue produces the same state without another payment or award. These are native PIDs, not console-wrapper PIDs. |
| Existing seven shapes | Battles01–07 all mount, save/return and reach their next player input through the same code. Controlled admission retains9–17 actors and0–4 nondeployed members. This does not claim a natural continuous seven-battle replay. |
| Return and v1 | Focused actual Battle01 victory and defeat, and admitted Battle02 Egress, finish in usable field. Each writes v1 and is Continued in two fresh processes with identical pre-input field payloads, including party, position/facing, gold, flags and resources. No outcome replays on mount. Wider kill/drop/growth/poison and Angel Wing assertions remain A's engine evidence. |
| Real music history | An actual enemy attack changes Bowie's HP12→9 and pushes `MUSIC_ENEMY_ATTACK` over battlefield music and the prior attack history. The next real menu binds and saves that native stack. Region completion uses the new typed engine command in `BattleSaveRegionTests`; finite helpers remain refused. |
| Busy/preview/input | Nonzero preview refusal preserves revision/preview; real undo remains frozen while covered and completes only after closing. A committed HEAL scene freezes under the menu and completes naturally after return. Held Menu blocks a Confirm until release. Overwrite Cancel preserves progress and file sequence. |
| Failure/recovery | Real lock and Replace denial do not exit or mark new progress clean. Uncertainty blocks another write until read-only inspection and allows cancellation to the same live session. This also holds after cancelling the deferred close during publication. Future-version primary does not offer an older backup; corrupt primary offers explicit backup recovery. Cancel preserves bad primary bytes; recovery preserves good backup bytes and mounts the battle. |
| Localization/latency | English and Chinese save labels/choices use UI keys. Loading feedback is published before detached admission/publication; it remains cancellable for reads. No private content/cache or synchronous world admission was added to a gameplay input callback. These samples do not establish a latency guarantee on other hosts. |

The selected engine gate passes2220 tests with43 optional private skips; adapter and affected
PrivateAdmission/LiveInspection builds pass without warnings/errors. Exact clean-head planner,
CI, commands, bytes, PIDs, failure ledger and process state belong to the frozen slice handoff.
Earlier compile/probe failures remain retained: an invalid test API call, paused `Playing=false`
misclassification, overly short fixed-frame worker/mixer waits, and unsupported Battle01 discard/
Egress attempts. Those attempts were not counted as successful product input or expanded into new
gameplay capabilities. Battle02 supplies the existing admitted retreat proof.

**Unknown / not newly exercised:** physical focus/device and OS-close behavior on this save flow,
arbitrary resource corruption during candidate-view preparation, original timing/PCM position,
unbounded sessions, later campaign admission and public redistribution. Existing shared modal/input
and A/B rejection evidence is retained; it does not turn these unavailable native observations into PASS.

## Exit warning attribution

**Confirmed, bounded current comparison:** verbose Save and Quit exits0 with exactly one
`AudioStreamWAV`/`AudioStreamPlaybackWAV` pair, each refcount1. The same `resume` input in separate
processes gives three such pairs both when retaining observer references (PID3088) and when clearing
`view`, immediately freeing the actual host and clearing `host` before Quit (PID44772).
Pre-input and suffix semantic payloads are identical in that pair. No Node is listed as leaked.
There is no GC, mixer delay, log filtering or delayed gameplay completion in this diagnostic.

**Inferred:** these current WAV/playback warnings are consistent with the existing
[F8 native audio teardown boundary](../godot/audio.md#resource-lifetime-and-shutdown), and do not
depend on the observer retaining host/view references. This narrow comparison did not establish a
new host-node lifecycle leak. It does not identify the native mixer ownership of every pair.
Earlier non-verbose2/6-instance logs and later variable4–12 counts retain **Unknown** per-object
attribution; do not retrospectively relabel them. All original logs remain. Exit0/semantic success
does not mean clean shutdown or leak-free long-term play; no broader cleanup audit was performed.
