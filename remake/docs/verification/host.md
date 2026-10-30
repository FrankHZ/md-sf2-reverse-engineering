# Host and Input Verification

Startup, logical input and accessibility checks observe the actual ordinary Godot host.

Load the owning worktree's private-input configuration and protected SDK/Godot environment from
[development](../development-and-verification.md#locked-net-workflow) in the same launching process.
Unless a recipe explicitly changes directory, commands run from the repository root. Reuse the
existing instance when suitable and choose fresh ignored outputs. These are focused recipes, not
an aggregate checklist. No screenshots; no new tests of probes/helpers. Read completed failure and
Unknown boundaries before reproducing a claim. Full historical receipts remain in the
[pre-reorganization guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/development-and-verification.md).

## Ordinary Host Startup

After loading the retained worktree SDK/Godot environment, ordinary play uses:

```powershell
& $godotBinary --path remake/game
& $godotBinary --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/garden-watch.json').Path
```

The same project accepts `--private-battle-start` and `--private-exploration-start` with the selected private inputs described below. These and `--authored-package` select mutually exclusive session entries.
An optional `--input-settings <path>` selects the versioned product settings below, independently of
the entry and in either argument order. Each option takes one path and may appear only once.
Unknown/positional, duplicate, conflicting entry and missing-path arguments report
startup ContentError in the common view before creating a session. They cannot select legacy play.

The external observer owns `SF2_OBSERVATION_CASE`, `OUTPUT`, `FOLLOWUP_SHAPE`, `TARGET_SHAPE`,
`ENEMY_SHAPE`, `CONTINUATION_SHAPE`, `LONG_PATH` and `REVERSE_KILLS` (all with the same prefix).
Unset/empty CASE runs the default observation; flags use1. Former `--observation-*`/shape options are
not game arguments. Each example assigns its case/output; optional shapes belong to that named case.
Diagnostics do not affect ordinary launches without the script. Direct startup observation uses:

```powershell
$env:SF2_OBSERVATION_CASE = 'startup'
$env:SF2_OBSERVATION_EXPECT_FAILURE = ''
$env:SF2_OBSERVATION_EXPECT_ORIGIN = 'public-authored-controlled-start'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'startup.json'
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd
```

For a rejected real-argument launch, set EXPECT_FAILURE to `unknown-startup-option`,
`duplicate-startup-option`, `conflicting-startup-options` or `missing-startup-path` and append the
corresponding arguments after Godot’s `--`. Private success uses EXPECT_ORIGIN
`private-local-controlled-start`. The script observes actual Main/GameRoot/BattleSessionView and
published startup state; it does not set engine state. Preserve process output and require passed:true.

The legacy `remake/reference/game` host, its `--map3-smoke`/profile arguments and their observations
were retired at M5. Ordinary export configuration excludes `probes/*`; complete ordinary export and
package contents remain unverified.

## Menu pointer observation

The [Standard D agreement](../godot/presentation.md#menu-pointer-agreement) uses the ordinary Main
scene and `probes/engine_menu_mouse_observation.gd`. It reuses the viewport/status observer's native
input, bounded focus, semantic-result and error helpers. It supplies real Godot mouse press/release,
motion and mapped key events and reads current session/control state. No session setters,
screenshots, bulk frames or tests of observers are involved. Each run has the existing 75-second
deadline and a fresh ignored output directory; selected cases take seconds, private story cases
about 47 seconds. Reuse the installation, project, imports and prepared private inputs. Rebuild Debug
only when changed adapter code needs a new native load; Release uses `uv run sf2 verify adapter`.

Prepare these small public variations from tracked packages, once under ignored `local/issue1408`:

```python
import copy, json
from pathlib import Path
repo = Path(__file__).resolve().parents[2]  # Save as local/issue1408/prepare-inputs.py.
out = repo / "local/issue1408"
out.mkdir(parents=True, exist_ok=True)
b = json.loads((repo / "remake/content/authored/practice-yard.json").read_text())
b["items"] = [dict(id=0, name="Recovery leaf", effect="consumable-healing", power=10,
                   minimumRange=0, maximumRange=1),
              dict(id=6, name="Small remedy", effect="consumable-healing", power=4,
                   minimumRange=0, maximumRange=2)]
b["actors"][0]["items"] = [256, 6, 255, 383]
b["actors"][1]["items"] = [0, 127, 127, 127]
b["start"]["actors"][0]["hp"] = 60
# Four admitted long labels require scrolling without expanding spellbook capacity.
b["spells"] = [dict(copy.deepcopy(b["spells"][0]), id="m"+"w"*62+str(i),
                    mpCost=3 if i < 3 else 25) for i in range(4)]
b["actors"][0]["spells"] = [dict(id=s["id"], level=1) for s in b["spells"]]
(out / "battle.json").write_text(json.dumps(b), encoding="utf-8")
a = json.loads((repo / "remake/content/authored/harbor-arrival.json").read_text())
a["start"]["program"] = dict(program="invitation", instruction=0)
a["world"]["texts"][0]["text"] = "Will you accept the invitation? " * 10
a["world"]["programs"][0]["instructions"] = [dict(op="text-cursor", text=100),
    dict(op="show-text", mode="single", speaker=None), dict(op="yes-no", flag=10),
    dict(op="yes-no", flag=11), dict(op="close-text"), dict(op="end")]
(out / "authored.json").write_text(json.dumps(a), encoding="utf-8")
settings = dict(formatVersion=1, textMode="adjustable", charactersPerSecond=40, bindings={
    name: dict(keys=keys, buttons=[button], axes=[]) for name, keys, button in [
        ("up", ["Up","W"], "DpadUp"), ("right", ["Right","D"], "DpadRight"),
        ("down", ["Down","S"], "DpadDown"), ("left", ["Left","A"], "DpadLeft")]})
(out / "keyboard-pointer.json").write_text(json.dumps(settings), encoding="utf-8")
```

Use `uv run python -X utf8 local/issue1408/prepare-inputs.py`. The explicit ordinary input configuration removes
only the interfering movement-axis bindings. It preserves keyboard and pad buttons, plus the
existing Drop axis. The observer checks the installed `sf2_*` actions: unbound raw axis events remain
diagnostic; mapped external axis/button input still reports Unavailable. This changes neither
default product settings nor user devices/registry. An attempted preset removing Drop's only pad
axis was correctly rejected; the maintained preset leaves it intact.

After loading `local/private-inputs.ps1` in the launching process and forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, select one case/input below. Give that run new worktree-local
APPDATA, LOCALAPPDATA, TEMP and TMP directories, and set `SF2_VIEWPORT_CASE` plus
`SF2_VIEWPORT_OUTPUT=<fresh-run>/native.json`. Launch from the repository root:

```powershell
$repo = (& git rev-parse --show-toplevel)
$arguments = @('--path', (Join-Path $repo 'remake/game'), '--max-fps', '60',
    '--resolution', '1920x1080', '--script', 'res://probes/engine_menu_mouse_observation.gd',
    '--', $entryOption, $selectedInput, '--input-settings',
    (Join-Path $repo 'local/issue1408/keyboard-pointer.json'))
# Add --headless only for the table's headless cases. Focus cases require native windows.
& $env:GODOT_BIN @arguments
$nativeExit = $LASTEXITCODE
```

| Case / ordinary entry | Bounded observation |
| --- | --- |
| `battle` / `--authored-package local/issue1408/battle.json`; headless | All four commands, long-label wheel scrolling, unavailable spell/empty item, available item, unavailable ordered target, previous/next, explicit confirmation guard, layered Back/right click, Stay, keyboard handoff, drag/hide/menu-creation release isolation; no rejected/canceled resource or RNG mutation. |
| `physical` / `--authored-package remake/content/authored/stone-court.json`; headless | Click Attack into its admitted target flow, then a separate click Confirm. |
| `heal` / retained E1 `local/issue1397/heal.json`; headless | Selection remains provisional; explicit click plus double-click tail reaches the actual healing consumer, one MP cost and HP recovery, then next actor. Reuses the existing scene observer. |
| `focus-battle` / `--authored-package local/issue1408/battle.json`; windowed | Actual native Window focus transfer/return during a press, drag-out/re-entry cancellation, fresh click after return and keyboard replacement of a pressed list layer. |
| `authored` / `--authored-package local/issue1408/authored.json`; headless | Unread text rejects click; text acknowledgement attaches a new row without adopting an old press; hover No→Confirm answers No; a second question rejects stale release/double click; direction takes focus back to Yes and Confirm answers Yes. |
| `authored-clicks` / same authored input; headless | Deliberate clicked Yes, followed by right-click No on the next question; normal close and usable field return. |
| `authored-focus` / same authored input; windowed | Actual native focus transfer/return, drag-out, fresh No answer, newly attached next-question release isolation, right-click No and field return. |
| `private-no`, `private-yes` / `--private-exploration-start local/issue1391/scene-start.json`; headless | Existing Zone8 source sequence through the real question, held-entry Opening/Release and rejected click, authoritative hover/directional focus, deliberate clicked answer, retained Closing, deletion/result flag and existing program continuation. Stops before later JOIN work. |

Private cases reuse `local/issue1045/inputs.ps1`, `world-route-13.json`, and the tracked
`remake/reference/inputs/battle01-player-ready.json` controlled party selection. Resolve these
within the owning checkout; retain their original identities. No new extraction or original-reach
claim is authorized. The owning checkout's ignored `local/issue1408/run-native.ps1` records exact
launches, settings, fresh directories, PIDs and stdout/stderr alongside the JSON reports.

**Confirmed, bounded executor results:** battle, physical, actual healing cost, authored mixed and
clicked answers, source No/Yes, and both native window focus cases pass. The source runs retain one
ObjectDB warning listing six leaked instances at exit; they are not a clean-shutdown claim. The
[audio lifetime investigation](#audio-resource-lifetime-observation) distinguishes this from runtime
accumulation. Small actual
Application tests cover explicit source answer selection, stale/wrong token, Entry/Opening/Release,
Closing/result delay and next-question guards, and unchanged authored direct answers. Independent
main-gate review/merge remains separate.

Completed discoveries remain in `local/issue1408`: initial adapter CS0266 notification constants
(corrected narrowly); `pilot-01` hover failure (**Unknown** cause); `battle-01` external Axis0=-0.51
and `battle-window-06` mapped Button1 interference (**Unavailable**). The successful initial window
pilot did not yet have input receipts; later explicit-settings focus witnesses retain those receipts.
`battle-headless-02/-03` used an inadmissible nine-entry spellbook or expected nine offers from three
learned entries; `-04` clicked a still-clipped row fragment. Those fixture/observer failures led to
four legal long labels and sufficient local scrolling. `battle-headless-05` found stale container
geometry moving navigation on the next input; native layout now settles without input. `-07` is the
rejected Drop preset above. `authored-headless-01` expected unread content under instant text;
`-02` reached the correct mixed answers but its controlled program omitted close-text and failed
the return wait. Corrected inputs retain ordinary unread/close behavior. Passing finite runs do not
erase `heal-resource-headless-01`'s observer baseline-shape error and completed 75-second timeout;
the reused scene witness now receives its actual initial actor baseline. They also do not
erase the other results, resolve raw device interference, prove physical release outside the focused
window, or establish whole-game/mouse-only, all-device, original-timing or aesthetic acceptance.

## Diamond focus emphasis observation

The [bounded B7 agreement](../godot/presentation.md#diamond-focus-emphasis) extends the existing
`probes/engine_menu_mouse_observation.gd` witness, reusing ordinary native key/mouse input, focus
transfer, resource/RNG and error helpers. It records a few actual draw offsets and timers around
focus changes and settle; no screenshots or per-frame state capture. The native observer retains
the 75-second deadline; each emphasis case takes about three seconds and emits about 91 KiB.

Use the [menu-pointer launch recipe](#menu-pointer-observation), unchanged project/installation and
retained `local/issue1408/battle.json` plus `keyboard-pointer.json`. Set `SF2_VIEWPORT_CASE=emphasis`
and a fresh ignored `SF2_VIEWPORT_OUTPUT`. Rebuild Debug only for changed adapter code; Release
uses `uv run sf2 verify adapter`. A second run uses case `emphasis-reduced` and an ordinary settings
copy with `reducedFlash=true`. Both require native windows for actual focus loss/return. The owning
checkout's `local/issue1412/run-native.ps1` records exact launches, output roots, PIDs and logs.

**Confirmed, bounded executor results:** native fallback emphasis passes in 2,732 ms; reduced-flash
passes in 2,089 ms, both exit0 without warning/error output. Actual drawing advances −4→−2→0 through
the short emphasis; quick changes select only the new item. Pointer/keyboard focus, idle/current
direction non-retrigger, fixed hit rectangles, unchanged revision/resources/RNG, hide/show, native
focus loss/return, close/reopen and actual host-scene retirement pass. Reduced-flash stays at zero.
The existing headless `battle` witness passes in 4,815 ms, retaining disabled inspection,
press/release capture, scroll/list/target/cancel and explicit confirmation behavior. These are
injected native inputs with the retained axis-adjusted settings; all-device release remains Unknown.

**Confirmed, mounted-original-icon executor result:** the same short native witness with case
`emphasis-icons` passes in 4,633 ms, exit0 without warning/error output. It checks all four mounted
24×24 textures, the actual `.normal`/`.selected` resource, nearest 4× sampling, integer advancing
and settled draw offsets, fixed hit rectangles and the same lifecycle/resource/input boundaries.
It uses the existing controlled `--private-exploration-start` with
`remake/reference/inputs/battle01-to-battle02-start.json`, #1045 world/party selections from the
menu recipe, and the existing eligible Cancel entry skip; it does not replay the opening.

Select `SF2_PRIVATE_BATTLE_SCENE_CONTENT` from the worktree-local
`local/issue1412/main-gate-retained-icons/scene.json`. Main-gate explicitly transferred only the
frozen accepted #689 `original-icons-02` scene and provenance after confirming its old owner stopped;
the executor does not access the other checkout. The scene is 150,016 bytes, provenance 1,839 bytes;
all eight existing resource digests and 24×24 RGBA8 payload sizes match. Provenance pins SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the registered USA retail ROM and `tiles_MainMenu` in
`data/graphics/tech/menus/mainmenutiles.bin`. The ordinary Content reader supplies runtime admission.
The retained `icon-transfer-check.json`, `TRANSFER.md` and exact launcher under `local/issue1412`
record selection, provenance and input ownership; generated/private pixels remain ignored.

Preserve the initial input-selection failure: the configured #1045/#1047 and #1397 scene variants
exist but lack `command-menu.*`. Narrow preparation with the accepted helper was authorized, but
its configured local upstream checkout/H1 listing was absent; the registered ROM verifier passed.
No preparation, installation, source rebuild or substitute art occurred. `icon-input-check.json`
and `icon-preparation-check.json` retain these distinct setup results, resolved for this observation
by the explicit immutable-artifact transfer. Earlier menu/audio failures and warnings remain intact.
Original cadence, physical-device release and final human/aesthetic approval remain Unknown.

## Audio resource lifetime observation

The Standard agreement in [#1410](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1410)
owns a bounded lifetime investigation, not an audio redesign. Production `SessionAudio`,
`GameRoot` and gameplay remain unchanged. Two explicit observations isolate the backend and actual
adapter; neither is loaded by ordinary startup. No private audio, screenshots, emulator, forced GC,
sleep-based repair or new engine tests are needed. Reuse the existing installation/project and
Debug build. `uv run sf2 verify adapter` covers compilation; use the committed dependency plan and
applicable CI. Scope and current policy belong to the [audio owner](../godot/audio.md#resource-lifetime-and-shutdown).

### Reproduce

From the owning checkout, load `local/private-inputs.ps1` and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Build Debug from `remake/` using
`& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore` when the
observation source changes. For each launch, select a fresh ignored directory and direct APPDATA,
LOCALAPPDATA, TEMP, TMP, stdout and stderr there as in the menu-pointer recipe. Set
`SF2_AUDIO_OUTPUT=<fresh-run>/native.json` for the adapter observation. Launch with argument arrays
from the repository root:

```powershell
$repo = (& git rev-parse --show-toplevel)
$arguments = @('--verbose', '--path', (Join-Path $repo 'remake/game'), '--max-fps', '60',
    '--script', 'res://probes/engine_audio_lifetime_observation.gd')
# Dummy: prepend --headless. WASAPI: omit it and verify the actual driver in the report/log.
& $env:GODOT_BIN @arguments
$nativeExit = $LASTEXITCODE
```

`AudioLifetimeObservation.cs` calls the real `SessionAudio` owner. Reflection constructs only
immutable diagnostic definitions containing 8 kHz mono PCM16 silence: loop/finite music and finite
commands67/83. It bypasses content admission explicitly; it is not a source-content or gameplay test.
Four cycles observe music replacement/finite completion, natural effect completion, same-slot
interruption and disjoint completion. Active disposal follows. It tracks IDs without holding
resource references, bounds each release/completion at two seconds, and records thirteen compact
samples. Every tracked release must invalidate its native IDs; disposed player nodes must also be
gone. A failed predicate exits1 with its label and completed samples in the logs. A successful JSON
does not override any stderr or later shutdown error.

For the independent backend reproduction, select
`res://probes/engine_audio_backend_observation.gd` instead. Set `SF2_AUDIO_CASE`:

| Case | Evidence |
| --- | --- |
| `runtime` | Four natural-finish/interruption cycles with weak references; interrupted objects are initially alive and become invalid during ordinary processing. Exit1 if resources remain or real Finished callbacks are missing. |
| `active` | Exit while one looping WAV is active; retain verbose ObjectDB warning details even when exit0. |
| `stop-quit` | Stop, clear stream, queue-free the player, then quit immediately; distinguishes deferred backend cleanup from forgotten project release. |

This GDScript case uses authored 0.1-second silence and no project classes. For a native ordinary
host comparison, launch the Main scene without `--script`, using the retained menu-pointer private
entry/settings. Send normal window close to the actual Godot child process, whose parent matches
the launched console PID. The console wrapper has no window. Retain the launch, close result,
stdout/stderr and actual process completion; do not substitute a forced kill as normal exit.

### Findings and limits

**Confirmed:** installed `4.7.2.stable.mono.official.ed1daf0bf`, upstream commit
`ed1daf0bf001b61586d9930840f2f1394092c079`. In
[`AudioServer::stop_playback_stream`, `_delete_stream_playback_list_node`, `_cleanup_lists`, `finish` and destructor](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/servers/audio/audio_server.cpp),
Stop marks playback for fade/deletion. Mixer processing schedules deletion, and main-thread cleanup
releases the playback reference. Shutdown stops drivers and clears buses; it does not enumerate
remaining playback entries. The destructor only cleans already-erased entries through
[`SafeList::maybe_cleanup`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/core/templates/safe_list.h).
This explains retained WAV/playback pairs when teardown outruns deferred deletion; no game code
is needed to reproduce it.

| Completed observation | Result and boundary |
| --- | --- |
| Preserved menu-pointer private No, Dummy | Gameplay PASS, 46,556ms, exit0; six instances (three WAV/playback pairs), each refcount1. This old warning remains evidence. |
| Menu-pointer private No, WASAPI | Gameplay PASS, 46,625ms, exit0, no stderr or exit warning; same controlled choice/closing/result continuation. A clean native run does not invalidate the headless failure or prove every exit clean. |
| Backend `active` and `stop-quit`, Dummy | Exit0, two retained instances each: one WAV and one playback, refcount1. Immediate Stop/clear/queue-free is insufficient. |
| Backend `runtime`, Dummy and WASAPI | PASS; four genuine Finished callbacks, zero remaining weak-reference targets after each release, no exit warning. |
| Actual SessionAudio, Dummy and WASAPI | PASS; four cycles, sixteen natural completions, 54 tracked resource/playback IDs released, final active disposal and player-node release, no exit warning. No runtime accumulation in this bounded sequence. |
| Ordinary Main window close, WASAPI | Normal close of the verified child succeeded and process stopped. One run had no warning; another retained one WAV/playback pair. Exit-code receipts were not captured for these manual closes. Counts are timing-dependent, not evidence of growth. |

**Confirmed diagnostic defect:** C#
[`GodotObject.IsInstanceIdValid`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/glue/GodotSharp/GodotSharp/Core/Extensions/GodotObjectExtensions.cs)
calls `InstanceFromId`. Polling it after disposal can recreate a managed RefCounted wrapper while
native deletion is pending. Initial adapter observations therefore retained one music WAV with no
playback; the same failure reproduced without SessionAudio. Native GDScript `is_instance_id_valid`
avoids this observer-created reference. Only the diagnostic changed; no production workaround or
forced collection was applied.

Retained local evidence is under `local/issue1410`; the inherited warning is under
`local/issue1408/shutdown-verbose-no-01`. Completed failures include the initial backend check that
mistook `Playing == false` for the queued Finished callback, C# validity-polling disposal failures,
one nullable diagnostic build failure, and ordinary-close launchers that targeted the console or
checked window readiness too early. The verified native child windows were subsequently closed
normally; these launcher failures were not converted to passed automated checks. The corrected
observations use actual Finished and native identity queries. A first WASAPI choice run passed its
gameplay assertions in46,600ms but reported a shader-cache path error; its output selection contained
`..`. Canonicalizing the new run's output path yielded the clean native result above. Preserve both
results and all original logs.

**Inferred:** the retained source-observer pairs share the demonstrated upstream teardown cause;
their exact historical IDs were not instrumented before exit. **Unknown:** exact retention count
under other timing/drivers/versions and unbounded-session behavior. No whole-game, hardware or
audible-fidelity claim follows. An upstream shutdown repair is the appropriate remedy; the warning
remains an explicit limitation on the pinned version. Do not suppress it or add a product delay.

## Unified world viewport observation

The [Standard presentation agreement](../godot/presentation.md#unified-world-viewport-agreement)
owns 1920×1080,96px cells/ordinary frames and 4× nearest sampling. Build the actual Release adapter
with `uv run sf2 verify adapter` and Debug adapter with the configured absolute SDK after loading
`local/private-inputs.ps1`; force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the registered
Godot installation and owning project. A changed adapter binary requires a new native load; no
SDK, project or asset copy is needed. Screenshots and full/H4 replay are excluded.

`engine_world_viewport_observation.gd` loads ordinary `Main.tscn`, submits mapped keyboard events,
waits on actual input/delivery state and reads existing camera/layout/terrain observations. It
requests native focus before input and permits one bounded recovery, then reports Unavailable.
Select `SF2_VIEWPORT_CASE`, a fresh `SF2_VIEWPORT_OUTPUT` and the corresponding ordinary startup
argument. With the selected private inputs or authored package loaded, run from the repository root:

```powershell
$env:SF2_VIEWPORT_CASE = 'room'
$env:SF2_VIEWPORT_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'viewport-room.json'
& $env:GODOT_BIN --path remake/game --max-fps 60 --resolution 1920x1080 `
    --script res://probes/engine_world_viewport_observation.gd -- `
    --private-exploration-start $selectedStart
```

Require exit 0, `passed:true`, empty failure/unavailable lists, and no Godot script,
C# or draw errors. An output file or zero process status alone is insufficient. Reports retain
only a few semantic checkpoints, never a full scene per frame. A 75-second observer deadline and
small-MB reports bound each short case; inspect the room/entry pilot before adding other cases.

| Case | Existing input / bounded observation |
| --- | --- |
| `room` | Retained private connected Map3 world, initial player `(56,3)`, flags already compatible with ordinary field input. Actual 11×10 legal area gives 1056×960 centered at `(432,60)`. A small requested layout strip crossing the room's left edge contains only legal source draws; ordinary step/turn/release keeps 96px frames. |
| `warp` | Same room input. Two ordinary Left steps use the existing packed-room warp to town. Require the player visible immediately, new area bounds and full world rectangle; do not pan through unopened packed storage. |
| `entry` | Existing connected Battle01 start `battle01-to-battle02-start.json`, selected retained world/party. Cancel the eligible intro, release, wait for ordinary battle control. Read real map textures/sprites; open/cancel menus, move the preview and cycle item targets. Older private packages retain their explicit Unsupported Egress instead of changing gameplay to satisfy the observer. |
| `region` | Accepted Battle02 outcome world/party with `engine-battle02-egress-start.json`, from the [Egress owner](field-programs.md#battle02-egress-and-walking-retry). Actual map-region origin `(16,19)` maps terrain/actors into the shared viewport. Short ordinary Egress reaches usable Map3 and subsequent movement; no full battle is replayed. |
| `scene` | Same retained Map3 world, controlled initial player `(42,9)` and flags `0,32,256,260,261,601,602`, with the current explicit palette/text start fields. Down then Right naturally enters Zone8 at `(43,10)`. Existing caller runs `cs-5149a`; ordinary reveal/acknowledge reaches text525. Actual teacher ink gets high-priority wall fragments after the 96px sprite draw, with independent plane origins. Stop at that boundary. No direct text/program-state injection or private world duplication. |
| `far` | Small pre-start variation of authored practice yard: 32×20 open terrain, injured `guard-a` at `(29,17)`, MEND range 48. Move preview, choose self then distant target, cancel. World zoom remains 1 (96px board cells), the target pans inside legal bounds, and a corner description avoids the edge-clamped subject. Repeat with medic at `(1,1)` and require mutually disjoint visible panels around that subject. |

Create the small controlled starts before startup, preserving the tracked input's explicit palette,
text and display fields; this writes no private world or asset copy:

```powershell
$startData = Get-Content remake/reference/inputs/battle01-to-battle02-start.json -Raw | ConvertFrom-Json -AsHashtable
$startData.start.map = 'map-3'
$startData.start.position = @{ x=56; y=3 }
$startData.start.facing = 3
$startData.start.flags = @(0,32,256,260,261,601,602)
$selectedStart = Join-Path $env:SF2_RUN_OUTPUT 'room-start.json'
$startData | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $selectedStart -Encoding utf8
# For scene: change only the initial position to @{ x=42; y=9 } and save another start.
```

For `far`, use `--authored-package $package`; all other cases use their selected private start.
Case recipes share the explicit native options `--max-fps 60 --resolution 1920x1080`.
Private payloads, start variants, reports and logs remain in the owning ignored output directory.
No new observation establishes original hardware timing, all-map coverage, physical-device release
while another OS window owns focus, or final aesthetic quality. Preserve #689's entry timeout,
#1386's exit warning and #1389's focus-release Unknown; source geometry updates do not erase them.
Static-room runs can retain the existing two-ObjectDB exit warning; successful active cases must
still be checked for errors. Original icon namespace selection remains with the
[private icon owner](../content/presentation-assets.md#private-command-menu-icons); these map/sprite
checks do not claim a new original-icon acquisition or asset redistribution right.

## Shared story dialogue observation

The [Standard agreement/current treatment](../godot/presentation.md#shared-story-dialogue-agreement)
owns this bounded modern UI. Reuse the registered Godot, configured absolute SDK/shared CLI home,
existing project and prepared private content. Load `local/private-inputs.ps1` in each launching
process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run `uv sync --locked`, Release
`uv run sf2 verify adapter`, and Debug `dotnet build remake/game/Sf2.Remake.Godot.csproj --no-restore`
using `$env:DOTNET_BIN`. A changed adapter requires a new native load; no asset/toolchain export or
copy is involved. No engine unit, full, research, H4 or screenshot suite is selected.

`engine_story_dialogue_observation.gd` reuses the actual-host focus/input/report helpers from the
viewport observer. It loads ordinary Main and supplies mapped key events only. It reads native
panel/body/portrait/choice/hint rectangles, full shaped line counts, visible characters, reading
coverage, real session tokens/choice phases and errors. A portrait must be a live textured child
after the background, not merely a valid rectangle behind it. Reports are semantic checkpoints;
they do not retain scene frames. Budget: one process, 75-second outer deadline, private loop at
most 60 seconds, under 5 MiB per report. Stop and retain any failed/unavailable run; a passed flag
with runtime/script errors is not accepted. Private source cases retain the observed two-ObjectDB
shutdown warning; its cause is **Unknown**, not proven harmless.

Create these small **pre-start authored inputs** in ignored `local/issue1393`, using Python from
the owning checkout. They change no production content. The harbor copy omits its next battle
so the existing Yes/automatic-transfer consumer returns to field control. The original package
is still used by `skip`; the existing Jaro package supplies the battle-story consumer.

```python
import copy, json
from pathlib import Path
out = Path("local/issue1393"); out.mkdir(parents=True, exist_ok=True)
base = json.loads(Path("remake/content/authored/harbor-arrival.json").read_text())
base["world"]["maps"][1]["battle"] = None
def write(name, value):
    (out / (name + ".json")).write_text(json.dumps(value), encoding="utf-8")
write("control", base)
for mode in ("adjustable", "instant"):
    write(mode, dict(formatVersion=1, textMode=mode, charactersPerSecond=40))
long = copy.deepcopy(base)
long["world"]["texts"][0]["text"] = "\n".join(
    f"Passage {n}: The harbor keeper checks every ticket before the ferry leaves. "
    "Read each account and choose whether to travel." for n in range(1, 36))
write("overflow", long)
repeat = copy.deepcopy(long)
repeat["world"]["programs"][0]["instructions"][2:2] = [
    dict(op="text-cursor", text=100), dict(op="show-text", mode="single", speaker=None)]
write("repeat", repeat)
failed = copy.deepcopy(base)
failed["world"]["programs"][0]["instructions"][2:] = [dict(op="native-call",
    symbol="unavailable-dialogue-service", source="authored-failure-boundary")]
write("failure", failed)
top = copy.deepcopy(base)
top["start"]["position"]["y"] = 5
top["world"]["maps"][0]["entities"][0]["position"]["y"] = 5
write("top", top)
```

For `private`, `private-choice` or `private-yes`, use the retained selected Map3 world and controlled
party from the [viewport recipe](#unified-world-viewport-observation): source player `(42,9)`, same
explicit palette/text/display fields and flags `0,32,256,260,261,601,602`. Set
`SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_CONTROLLED_START` through the existing prepared
selection (`local/issue1045/inputs.ps1` and its retained `world-route-13.json`/tracked
`battle01-player-ready.json`); do not duplicate the private world or assets. Down then Right
naturally enters Zone8 and runs the existing caller. This is controlled modern-host reach, not
original natural timing. Missing retained inputs are a prerequisite, not acquisition authority.

Choose a fresh run directory and selected absolute start/package path, then run from the owning
checkout. Authored cases select `--authored-package`; private cases select
`--private-exploration-start`. Settings are `instant.json` only for `instant`, otherwise
`adjustable.json`. Case names and inputs are in the table below.

```powershell
$repo = (Get-Location).Path
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$case = 'repeat'
$run = Join-Path $repo 'local/issue1393/reproduce-repeat'
if (Test-Path -LiteralPath $run) { throw 'Choose a fresh run directory' }
New-Item -ItemType Directory -Path $run | Out-Null
foreach ($name in @('appdata','localappdata','temp')) {
    New-Item -ItemType Directory -Path (Join-Path $run $name) | Out-Null
}
$env:APPDATA = Join-Path $run 'appdata'; $env:LOCALAPPDATA = Join-Path $run 'localappdata'
$env:TEMP = Join-Path $run 'temp'; $env:TMP = $env:TEMP
$env:SF2_VIEWPORT_CASE = $case
$env:SF2_VIEWPORT_OUTPUT = Join-Path $run 'native.json'
$package = Join-Path $repo 'local/issue1393/repeat.json'
$settings = Join-Path $repo 'local/issue1393/adjustable.json'
& $env:GODOT_BIN --headless --path (Join-Path $repo 'remake/game') --max-fps 60 `
    --resolution 1920x1080 --script res://probes/engine_story_dialogue_observation.gd -- `
    --authored-package $package --input-settings $settings
# Retain stdout/stderr alongside native.json and inspect both, plus the actual exit code.
```

| Case / selected input | Bounded observation |
| --- | --- |
| `authored`, `instant` / `control.json` | Hidden field dialogue; legal Talk/Wait; no invented name/portrait; incremental/reveal-all or instant; fresh acknowledgement; authored direct Yes/Cancel=No; same-session automatic transfer/close and fresh field movement. |
| `top` / `top.json` | Same consumer with a bottom-edge subject; top placement stays stable through reveal, parent hide/show and question. |
| `overflow`, `repeat` / matching JSON | 4,295 characters, 70 full shaped lines at 30px. Panel expands to 1440×1016; overlapping reading pages retain the token. Choice attachment preserves already-read coverage. `repeat` opens identical text in a new window and proves reading resets to the beginning. |
| `failure` / `failure.json` | Acknowledge real authored text into existing unsupported native-call boundary; readable stopped wording, retained diagnostic/state, no automatic retry or response to later Confirm/Cancel. |
| `private` / Zone8 start | Real portrait resources, side/mirror/eye/mouth selector, actual child order and safe native bounds; source lines through text525. |
| `private-choice` / same start | Continues through531, retains default Yes, changes focus, holds Cancel=No without entering skip, releases, closes/deletes choices, naturally returns through532/533 to fresh field control. |
| `private-yes` / same start | Holds Confirm=Yes through source choice, releases, observes deleted choice and next genuine text535 input; stops before later JOIN work. |
| `battle` / authored `jaro-conversion.json` | Actual movement/stay trigger; two shared battle-story windows, retained physical camera slot/focus, real acknowledgements, correct close and fresh movement. |
| `skip` / original authored `harbor-arrival.json` | Choice offers no skip; later eligible intro shows actual binding; held Cancel stops at handoff, release admits battle, fresh Confirm/Cancel operate its ordinary menu. |

All executor cases below ran with `--headless`. They exercise actual Godot controls, layout,
process/draw callbacks, mounted resources and injected mapped input, but do **not** establish
windowed compositor delivery, OS window focus or physical-device behavior. Headless root-focus
flags are only observer preconditions. Independent main-gate owns the bounded windowed checks;
those results remain separate from these executor records. No executor windowed pass is claimed.

**Confirmed at the candidate boundary:** the private portrait/Zone8 pilot and authored question/return
pilot passed before expansion. Private No and Yes took about50/46 seconds, with24/22 compact samples
and about122/115 KiB reports. Reached source question531 uses three shaped lines; its full binding
hint measures71px and expands the panel to327px high, all within the safe area. The final repeated
long-text case took about1.9 seconds with eight samples/54 KiB; top, instant, failure, battle-story
and skip cases took about1–3.3 seconds each. Body/portrait/choice/hint bounds are disjoint and the
world rectangle/96px scale remain unchanged. Exact head/build/check identities belong to the Issue
and Draft PR freeze handoff; these are executor observations pending independent acceptance.

**Independent main-gate windowed validation:** on the unchanged `7066720c` adapter
(`fd037ee1` changed documentation only), main-gate reports authored, private-choice, repeat, battle,
top, skip, failure and instant all **PASS**: eight processes exited0,64 semantic checkpoints,
65.734 seconds observer time and289,323 bytes total. Its stdout/stderr checks found no runtime/script
errors, focus Unavailable or shutdown warnings; its independent Release adapter build also passed
with zero warnings/errors. Reproduce the same cases/installation/project/prepared inputs above by
omitting `--headless`; keep the windowed logs/receipts separate. Results are retained in ignored
`local/issue1393/main-gate-*` and `main-gate-window-results.json`. These supplied independent results
do not erase executor headless shutdown warnings or establish physical-device/original fidelity.

Retained failed attempts are in ignored `local/issue1393`: observer parse/type, numeric flag check,
insufficient reveal/movement sequencing and the original harbor case's automatic battle entry
were observer/input-plan failures. Product discoveries were the already-read authored question
restarting reveal, lost reading coverage after choice-row resize, and the source choice hint
exceeding its fixed footer. Their corrections passed focused reruns. The first repeated-window
fixture accidentally advanced text100 to101; corrected explicit text-cursor selection proves the
identical-window boundary. Completed failures/logs remain, including a four-ObjectDB warning on
the failed first source-choice run. No completed failure is relabeled interrupted or erased.

**Unknown/limits:** physical device release outside OS focus, original timing/audio/hardware fidelity,
all-map or multi-resolution coverage and final aesthetic quality. Temporary source `HideWindows`
retains identity by direct ownership review; parent hide/show is natively observed. Do not infer a
new natural full-opening claim. Earlier A unsuitable-start/probe failures and missed-skip timing
Unknown, B focus-loss Unavailable/physical-release Unknown, #689's entry timeout and #1386 exit
warnings remain preserved. No broader original comparison or cleanup authority follows.

## Battle return and Party Status observation

[E2's agreement](../godot/presentation.md#battle-messages-return-and-party-status) owns this bounded
presentation change. `probes/engine_battle_return_status_observation.gd` extends the existing viewport
witness: ordinary `Main.tscn`, mapped keyboard events, 75-second deadline and semantic checkpoints.
It reads actual message panels/cards/draw counters, carried resources and return events. No post-start
state mutation, synthetic completion, screenshots or tests of the observer are used.

Prepare two small authored variants before launch from `content/authored/leader-cleanup.json`:
set `start.map` to `arena`, append flag401, and replace the `before` program with one `end` instruction.
Save this as `victory.json`. For `defeat.json`, additionally set the leader's starting HP to1, the enemy's
HP to its admitted maximum, and that enemy deployment's `aiStrategy` to `attack-then-approach`.
The existing real rules then perform each outcome; no result is assigned. Retain the dead companion
and zero maximum MP. These controlled starts exercise actual authored behavior, not original reach.
Use `{"formatVersion":1,"textMode":"instant"}` for the input-settings JSON.

Load `local/private-inputs.ps1` in the launcher, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run
`uv run sf2 verify adapter`, and build the actual Debug adapter from `remake/` with the selected SDK:
`& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.
For each case set `SF2_VIEWPORT_CASE=victory` or `defeat` and `SF2_VIEWPORT_OUTPUT` to a fresh ignored
JSON path. Launch the existing `$env:GODOT_BIN` with separate arguments:
`--headless --path remake/game --max-fps 60 --resolution 1920x1080 --script res://probes/engine_battle_return_status_observation.gd -- --authored-package <variant> --input-settings <settings>`.
Give each run worktree-local APPDATA, LOCALAPPDATA and TEMP/TMP. The retained executor wrapper is
`local/issue1398/run-native.ps1 -Case <case> -Run <fresh-name>`; outputs are under that issue directory.

`failed` reuses the ordinary host recipe's `battle-failure` pre-start package; `startup-failed` uses
an empty JSON object, exercising actual content admission rejection. Both require visible “stopped”
feedback and stable revision after input. `ContextHint.Arrange` shows nonempty feedback even after
`RetireWindows`; these cases cover that visibility boundary. Token-local `ResetReading` runs before
projecting each new battle message; same-token resource refresh retains reading position.

**Confirmed executor, headless only:** final authored victory and ordinary defeat pass real messages,
one outcome and one completed return, old-window retirement, busy status exclusion, readable live
party cards and gold, paging, hide/resume, consumed close input, unchanged party/revision while viewing,
and fresh field movement. Final runs take about6 seconds each and emit under1MB each. Startup failure
and reached program failure pass visible stopped feedback and immobility in under1 second each.
Private HEAL reuses the E1 `private` case, retained issue1047 world/inputs and its ordinary controlled
Battle02 entry: actual private names, card/draw, cost/recovery phases and continued control pass in
about9 seconds (1.05MB). This verifies the changed shared message consumer with admitted private
content; it does not replay private Battle01's full victory/defeat routes. Their unchanged engine,
reward/outcome ordering and after-program evidence remain at the existing
[Battle01 outcome owner](../application/session-and-programs.md#battle01-outcome-after-program-and-return).

Release and Debug adapter builds pass with zero warnings/errors. No engine behavior changed, so no
engine unit suite or historical aggregate was launched. Clean-head planner/CI results belong to the
PR handoff. No GameRoot or Application interface change was needed; existing handoff wiring suffices.

**Retained failure/Unknown:** `defeat-01` completed at75 seconds because the initial authored fixture
kept the enemy's Stay policy; it could not defeat the player. The corrected pre-start existing attack
policy passes `defeat-02`; no runtime rule was changed. `victory-01` predates the ally-only display
filter and is superseded for roster scope by `victory-party-final`. `private-heal-01` passes but emits
two ObjectDB shutdown warnings, preserving the existing shutdown Unknown. External joypad/focus and
physical release issues remain; no windowed E2 pass or full-game/original timing claim is made.
No native process is retained. Main-gate independently accepted the following bounded headless
observations at `c9410809` (earlier victory/defeat evidence retains unchanged dependencies):

| Independent case | Result / duration / semantic samples |
| --- | --- |
| Victory / ordinary defeat | PASS, 5786ms /14; PASS, 5665ms /12; clean stderr |
| Private close-up visibility | PASS, 9221ms /29, 1048267 bytes; two retained ObjectDB warnings |
| Physical close-up visibility | PASS, 3831ms /17, 505218 bytes; clean stderr |
| Startup / reached-program failure | PASS, 286ms /1 /25744 bytes; PASS, 388ms /1 /88851 bytes |
| Final victory return | PASS, 5915ms /14, 832299 bytes; clean stderr |

Reports remain in `local/issue1398/main-gate-*`. These results establish only their named boundary;
windowed input/focus, original timing and ObjectDB Unknowns remain unchanged.

The independent review also found duplicate IllegalCommand feedback: the global bottom panel could
cover the menu's own reason/help. The correction suppresses global recoverable feedback only while
the command menu is visible. Fatal/startup and errors without a menu retain global feedback.
For the focused witness, use `SF2_VIEWPORT_CASE=illegal` with unchanged `stone-court.json` and the
same return/status observer and launch above. Ordinary Up reaches a real rejected movement; the
witness checks the menu reason/help, hidden global feedback, then Cancel/Confirm recovery into
ActionChoice. Executor `illegal-feedback-01` passes (3186ms,3 samples,338908 bytes,clean stderr), with
Debug/Release adapter builds passing zero warnings/errors. Main-gate independently reviewed the
predicate and repeated the actual illegal-move/recovery observation at `8d7f3225`:
`main-gate-illegal-01` PASS, exit0, 2601ms,3 samples,338904 bytes, clean stdout/stderr.
No victory/defeat/private rerun is needed for this predicate-only correction. Engineering acceptance
is complete; [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400) owns integration.

## Battle status-card observation

[E1's Standard agreement](../godot/presentation.md#battle-status-card-agreement) owns the outcome.
`engine_battle_status_observation.gd` reuses the existing viewport observer's ordinary Main.tscn,
mapped input, focus recovery, 75-second deadline and output helpers. It reads actual session actors,
native name bounds/line counts, card/resource values, cropped-fill draw operands, menu/range geometry
and scene phases. It never sets gameplay state, completes a presentation synthetically or captures
images. Six serial finite cases use a few dozen semantic checkpoints each; the pilot reports were
below 2MB per case. No whole-battle replay, bulk capture, external acquisition or new comparison
platform is required. Retain failures; do not repeat an unrelated full suite.

Load `local/private-inputs.ps1` in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Run `uv run sf2 verify adapter`; use the configured absolute
SDK from `remake/` to build `game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore` for
native load. The first load of the new PNGs needs one existing-editor import:
`& $env:GODOT_BIN --headless --path remake/game --editor --import --quit`. Reuse the same installation,
project, imports and caches; a code build needs a new native load, not another installation/project.
No engine/data change in E1 requires unit tests. On the clean committed head, inspect
`uv run sf2 verify plan --base origin/main --head HEAD`; adapter-build is the affected product gate.

Prepare only small pre-start authored variants in a fresh ignored output directory:

```python
import json
from pathlib import Path
out = Path("local/issue1397")
out.mkdir(parents=True, exist_ok=True)
d = json.loads(Path("remake/content/authored/practice-yard.json").read_text(encoding="utf-8"))
name = "wandering-medic-of-the-northern-watch"
d["actors"][0]["id"] = name
d["start"]["actors"][0].update(actor=name, hp=25)
d["encounters"][0]["placements"][0]["actor"] = name
d["encounters"][0]["placements"][1].update(x=3, y=4)
d["start"]["actors"][1]["hp"] = 2
(out / "heal.json").write_text(json.dumps(d), encoding="utf-8")
long_name = "w" * 64
d["actors"][0]["id"] = long_name
d["start"]["actors"][0]["actor"] = long_name
d["encounters"][0]["placements"][0]["actor"] = long_name
(out / "long.json").write_text(json.dumps(d), encoding="utf-8")
```

Set `SF2_VIEWPORT_CASE`, `SF2_VIEWPORT_OUTPUT` to a new `native.json`, and select the corresponding
ordinary startup input below. Give each run its own ignored APPDATA/LOCALAPPDATA/TEMP/TMP directories.
For example, from the repository root with `$selectedInput` resolved to the selected authored file:

```powershell
$env:SF2_VIEWPORT_CASE = 'heal'
$env:SF2_VIEWPORT_OUTPUT = Join-Path $runRoot 'native.json'
& $env:GODOT_BIN --path remake/game --max-fps 60 --resolution 1920x1080 `
  --script res://probes/engine_battle_status_observation.gd -- --authored-package $selectedInput
$nativeExit = $LASTEXITCODE
```

Adding `--headless` selects a headless actual-host check; label it accordingly. Require actual exit0,
`passed:true`, empty failure/Unavailable arrays, and inspection of both logs for script/C#/draw errors.
JSON success does not erase shutdown warnings or prove original pixels/hardware timing. Scene waits
continuously check focus; one bounded recovery is permitted by the existing helper. Explicit fresh
menu-page waits prevent an observer from treating an unprocessed input as a selected action.

The status observer subscribes before Main startup, records its requested keyboard edges separately
from received native keyboard/joypad and focus events, and retains startup failures and key session
boundaries. Neutral axis noise is summarized; input/semantic receipt limits fail explicitly rather
than dropping required records. An active native joypad edge during this keyboard-controlled witness
records **Unavailable** and stops, preserving the event instead of filtering its gameplay effects.
The gamepad itself and ordinary product bindings are unchanged. Keep physical devices neutral for
an independent keyboard run. The receipt sample includes input-stage/revision and Skip request/stop
facts. Resource-event assertions still begin at first battle control, separate from startup work.

Entry follows the existing viewport observer's maximum of two eligible fresh Cancel requests within
the same deadline. Each request records before/after state; only an actual EntrySkip, pending handoff
or battle presenter counts as acceptance. If a request is unaccepted, or an accepted Skip is later
stopped before handoff, the observer does not retain a stale “skipped” flag. It can use the remaining
eligible request. No session setter, input suppression, larger deadline or general retry is added.

| Case | Entry | Finite acceptance boundary |
| --- | --- | --- |
| `heal` | Prepared `heal.json`, `--authored-package` | Two-line readable name, quarter-HP warning, full MP, image ratios, Magic/self/other target, layered Cancel with unchanged resources, fresh selection and actual heal/cost phases. HP remains old until Reaction; MP remains old until SpellCost. Next actor has its own card and no stale target. |
| `long` | Prepared `long.json`, `--authored-package` | The valid 64-character identity stays complete in two native lines at >=22px, with a wider safe-edge card, visible numeric/image rows and the same command/heal path. |
| `damage` | Tracked `content/authored/stone-court.json`, unchanged | Readable Swordsman/Raider, enemy target, full/zero-max-MP tracks, lethal ordinary physical action, HP15→0 at Reaction and the explicit Defeated cue; subsequent usable control. |
| `laser` | Tracked `content/authored/prism-laser.json`, unchanged | Existing two off-ray Stays and one Up+Stay per ally trigger one actual automatic shot. Every reached TargetEnter/Reaction/ResultMessage/DeathMessage card matches `laser.target`; Reaction cards traverse companion → leader → sentry, including both factions/deaths, then return control. |
| `aura` | Tracked `content/authored/aura-healing.json`, unchanged | Ordinary Magic list selects AURA4. Shared cards follow a-edge → b-center → n-neutral → m-full → z-priest → f-far, including full-HP recovery and one MP cost. |
| `private` | Same retained issue1047 private selections/start/party as [B8 private-region](#battle-movement-range-observation), `--private-exploration-start remake/reference/inputs/engine-battle02-egress-start.json` | Real Battle02 entry/skip, Chester name, ordinary Stay to Sarah, actual HEAL list/self/unavailable-other target and Cancel, then real private HEAL scene. Sarah HP11 stays11 while MP10→7 at SpellCost; no fictitious HP event is required for full-HP recovery. Existing scene rasters remain local. |

Private selection reuses `local/issue1047/inputs.ps1`, explicitly selects its `world-01.json` and the
tracked `reference/inputs/battle02-outcome-party.json`, resolved within the owning checkout. It copies
no canonical ROM, world or asset pack. All private payloads/starts/logs/reports remain ignored.
No original enemy/name/art acquisition is implied. Selected scene-name metadata is projected as
admitted; the host adds no party rename operation.

The targeted `names` / `overflow` cases use that same retained world and
`--private-exploration-start remake/reference/inputs/battle01-to-battle02-start.json`. Select a small
ignored copy of the already-admitted Battle01 scene JSON through `SF2_PRIVATE_BATTLE_SCENE_CONTENT`;
change only its first three `memberNames` before startup. For `names`, use
`"Northern\n\r\nWatch\t Keeper\nThree"`; require the complete displayed name
“Northern Watch Keeper Three”. For `overflow`, use three newline-separated 400-character `W`
strings; require the normalized 1202-character observation, 800px frame and two visible native lines.
Only these small metadata fixtures are copied (the selected scene is about 122KB); the existing
127.6MB world/assets remain read-only and are not copied. These cases stop at the first battle menu.
They directly check the finite width/ellipsis correction; they do not invalidate unrelated completed
damage/heal/MP observations or require another full case run.

**Confirmed executor boundary:** six windowed source-final cases passed with actual exits0, empty
failure/Unavailable arrays and clean stdout/stderr (about 51 seconds and 4.93MB total reports).
The later finite-name correction passed the two targeted windowed cases above, about 6.4 seconds
and 132KB, also with exits0 and clean logs. Debug/Release adapter compilation passed. Prior private
runs retained two ObjectDB exit warnings; clean final runs do not establish that historical issue
resolved. No Godot process is retained. Independent acceptance has the separate boundary below.

### Independent E1 acceptance

**Confirmed:** main-gate independently accepted E1 at `03f1e9525effcb69b5b46f443ad618f98116c6e9`
after code, documentation, asset and private-boundary review and the following actual-host checks.
`local/issue1397/main-gate-native-summary.json` retains the run identities and results. All five runs
exited 0 with empty failure/Unavailable arrays and clean stdout/stderr.

| Run | Mode | Elapsed ms | Samples | Report bytes |
| --- | --- | ---: | ---: | ---: |
| `main-gate-heal-01` | Windowed | 9500 | 25 | 654961 |
| `main-gate-private-01` | Windowed | 9680 | 23 | 993569 |
| `main-gate-names-01` | Windowed | 14524 | 1 | 37077 |
| `main-gate-damage-headless-02` | Headless state/draw | 4142 | 17 | 483667 |
| `main-gate-overflow-headless-02` | Headless state/draw | 3180 | 75 | 1353146 |

The three windowed checks cover heal, private-local presentation and normalized names. The two
headless checks cover damage and exceptional-name state/draw in the actual Godot scene. They do
not establish independent windowed damage/overflow passes or resolve native input interference.
Earlier window failures, uncontrolled joypad events and shutdown warnings retain their meanings
below; these five clean logs do not erase them. Broader physical-device/all-map/aesthetic approval
remains outside E1's accepted boundary.

Main-gate's direct review confirmed 22 owned paths, 141 local links, source-size limits and diff/private
boundaries. [CI40830350947](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/40830350947)
passed scope/adapter-build at the accepted head; engine-unit/research-public/h4-comparison-build were skipped by scope.
No acceptance blocker remains within this bounded E1 scope. Integration remains with main-gate in
[Draft PR1399](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1399); this documentation-only
closure changes no implementation and requires no repetition of completed build/native checks.

Completed failures remain in ignored `local/issue1397`: an initial JSON float/string expectation;
first-show wrapped-card height1402 (fixed using native minimum-size notification); a helper-name
collision caught by Debug compilation; private-case assumptions about name casing, terrain reach,
target order and an HP event on full-HP healing; and a long-name case initially routed to an
unsupported physical action by the observer. Earlier window runs also missed a page/failed to
complete ActionMessage; explicit page waits and continuous focus checks precede the final cases.
The extra name-fixture attempts selected Battle02's scene instead of the changed Battle01 names,
then exposed the direct private-battle start's unavailable battlefield-map resource, then missed the
eligible Battle01 intro Skip. Corrected cases reuse the admitted world and its ordinary eligible Skip.
Those window-input anomalies are not declared resolved merely by a later passing run. These are preserved results,
not interrupted suites or waivers. Prior #689/#1386/#1389/#1393/#1395 focus/release/timing failures
and ObjectDB shutdown Unknowns remain. Independent main-gate review owns integration and final
acceptance; broader physical-device, all-map and aesthetic approval are not established here.

Independent `main-gate-damage-01` failed at589ms/revision3 with Movement, unchanged actor positions
and `movement-range` before any scripted key; the original report contains no startup input receipt.
Independent `main-gate-overflow-01` timed out at23958ms in field text work, without Skip/input history.
Their specific causes remain **Unknown**. The later damage diagnostic passed with ordinary input
receipts; that does not identify the earlier event. Overflow diagnostic-01 confirmed Skip requested
at revision24 and stopped at363, followed by hundreds of native joypad events, including active
left-stick axes around±0.7; it ended at the input receipt budget. Diagnostic-02 caught a GDScript type
inference error before load; corrected diagnostic-03 stopped **Unavailable** at1093ms on actual native
L1. These later events establish interference in those attempts, not proof of the original causes.
All reports/logs remain under ignored `local/issue1397`. Independent heal/private/names PASS results
remain valid; only damage/overflow received the declared follow-up. No engine behavior changed, and
no passed case group was rerun to hide the failures. On main-gate's direction, the same overflow
scene was also observed **headless**: correction-headless-02 passed state/draw assertions at3156ms,
exit0, no failures/Unavailable or script errors, with two retained ObjectDB shutdown warnings. It
does not establish a windowed pass or resolve physical input. Correction-headless-01's successful
JSON still had duplicate signal-connection errors during view reparenting; it is not a passed gate.
The node-added subscriber now checks an existing connection before attaching again. Windowed
overflow remains unverified at the retained interference boundary; independent headless acceptance
above does not resolve that physical-input question.

## Logical Input and Accessibility (ADR 0010 9A)

`GameRoot` loads one `InputSettings`/`GameInput` owner before starting a session, installs its
`sf2_*` Godot InputMap actions, and dispatches each physical event once to the current view.
Exploration, dialogue/choice, battle and return use the same semantic actions. Defaults are:

| Action identity | Keyboard keys | Standard gamepad buttons / axes |
| --- | --- | --- |
| `up` | Up, W | DpadUp, LeftY- |
| `right` | Right, D | DpadRight, LeftX+ |
| `down` | Down, S | DpadDown, LeftY+ |
| `left` | Left, A | DpadLeft, LeftX- |
| `confirm` | Enter, KpEnter, Z | South |
| `cancel` | Escape, X | East |
| `attack` | F | West |
| `spell` | H | North |
| `item` | I | Back |
| `target` | Tab | RightShoulder |
| `stay` | Space | LeftShoulder |

Confirm talks, acknowledges dialogue, answers Yes, or selects/commits in battle. Cancel answers No
or backs out one menu level in battle; it does not acknowledge dialogue. Spell and Item open their
lists from the diamond; Confirm selects a row, initially targeting self when the offer permits it.
Target cycles the selected action's offered candidates. The [diamond recipe](#diamond-menu-observation)
owns the current battle interaction; older immediate-selection shortcuts in probes are retained
historical recipes, not acceptance of this revised UI.
All accepted commands still use Application's existing legality and state. F replaces the former X
attack binding; former C/Y/N exploration shortcuts are replaced by Confirm/Cancel. View help and
spell/item hints read effective bindings, including after a convention swap.

### Diamond menu observation

The first #438 production iteration uses default keyboard A and live semantic offers. After the
locked environment and Debug adapter build, prepare a small variation of the existing authored
practice yard in a fresh ignored output directory. This is controlled content, not a session setter:

```powershell
$package = Join-Path $env:SF2_RUN_OUTPUT 'menu-package.json'
$data = Get-Content remake/content/authored/practice-yard.json -Raw | ConvertFrom-Json
$data | Add-Member -NotePropertyName items -NotePropertyValue @(
    [pscustomobject]@{id=0;name='Recovery leaf';effect='consumable-healing';power=10;minimumRange=0;maximumRange=1},
    [pscustomobject]@{id=6;name='Small remedy';effect='consumable-healing';power=4;minimumRange=0;maximumRange=2}) -Force
$data.actors[0].items = @(256,6,255,383)
$data.actors[1].items = @(0,127,127,127)
$data.start.actors[0].hp = 60
$spell = $data.spells[0] | ConvertTo-Json -Depth 10 | ConvertFrom-Json
$spell.level = 2; $spell.mpCost = 25
$data.spells += $spell
$data.actors[0].spells += [pscustomobject]@{id='mend';level=2}
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $package -Encoding utf8
$env:SF2_DIAMOND_CASE = 'menu'
$env:SF2_DIAMOND_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'diamond-menu.json'
& $godotBinary --headless --path remake/game --max-fps 120 `
    --script res://probes/engine_diamond_ui_observation.gd -- --authored-package $package
```

The probe checks cardinal focus, disabled spell/item/target feedback, nested cancel without resource
use, real healing and another actor's item consumption. Current acceptance uses the existing
1280×720 review window, including disabled-list and target descriptions; older multi-size receipts
are not a requirement to expand multi-resolution support. Check the 216×168 command cluster and
72×72 icons, full-window map rectangle, and unchanged map rectangle/zoom/preview framing when opening
the main menu or a list. Gameplay target/movement changes can still move the camera. No claim of
final visual quality follows from geometry alone.

For physical attack, set `SF2_DIAMOND_CASE=physical`, a fresh output, and use the existing
`remake/content/authored/stone-court.json` package. For a bounded private menu observation, load
registered complete private selections, set CASE=`private`, a fresh output, and use
`--private-exploration-start remake/reference/inputs/battle01-to-battle02-start.json` with its
accepted connected world and party. The probe waits for the ordinary tower entry's skip hint, sends
default-keyboard Cancel (Esc), releases it and waits for ordinary battle control through the accepted
[battle-entry skip](../godot/battle-entry-skip.md). Its entry deadline is ten seconds; a missing or
failed skip does not silently fall back to waiting through the entire intro. A legacy standalone
battle input lacking map-region/field resources is
not a substitute. That case checks menu navigation and cancel without committing an action; it is
not another continuous opening/outcome comparison.
For original-art acceptance, prepare/select the [private command rasters](../content/presentation-assets.md#private-command-menu-icons)
and set `SF2_DIAMOND_ORIGINAL_ICONS=1`. Require square command rectangles at the review size, mounted
24×24 textures with nearest sampling, and actual draw readback selecting each normal/selected raster
as focus moves. The observation has a 150-second deadline. These checks establish resource use and
geometry, not human visual approval.
Original-icon observations also require no extra drawn frame or visible per-icon label. The compact
main hint must name the focused command at every cardinal position, hide keyboard prose and retain
the edge placement; unavailable feedback remains scoped to that selected command.
Cases `equipment`, `give` and `discard` use the corresponding existing
`remake/content/authored/battle-<case>.json` packages. They check that each supplementary shortcut
opens its existing presenter and Cancel returns to the diamond without spending resources. The
private pilot's older selected content does not supply every supplementary capability.
For a partial-resource negative check, copy the prepared private scene into another ignored input,
remove one command-menu raster and select that copy. Set `SF2_DIAMOND_EXPECT_FAILURE` to
`command-menu-resource-required`; require the field-owned admission failure before battle control.
Require exit 0, `passed:true`, empty failures and clean Godot errors. Preserve failed reports when
correcting a layout. The public Release adapter build and engine behavior tests are separate gates;
no tests of this probe or screenshot audit are added.

Keyboard echo is ignored. A stick deflection crossing magnitude0.5 submits one action; it must return
below that threshold or change sign to submit another. Unrelated axes and releases cannot repeat a
held direction. All standard Godot-mapped gamepads are accepted. This is product input behavior,
not a claim about original hardware scancodes or repeat cadence; physical controller-driver mapping
and hot-plug behavior are outside the injected native-event observations.

For example, save this UTF-8 JSON in a local settings file and pass its absolute path:

```json
{
  "formatVersion": 1,
  "confirmCancel": "swapped",
  "reducedFlash": true,
  "textMode": "adjustable",
  "charactersPerSecond": 40,
  "bindings": {
    "attack": { "keys": ["R"], "buttons": ["West"], "axes": [] }
  }
}
```

```powershell
& $godotBinary --path remake/game -- --input-settings $settingsPath
```

`formatVersion` is required and currently1. Other fields default to `standard`, false, `instant`,
40 and an empty binding override map. `confirmCancel` accepts `standard` or `swapped`; swapping
exchanges the **whole effective Confirm and Cancel binding sets after overrides**, across both
keyboard and gamepad, without changing their semantic roles. `textMode` accepts `instant` or
`adjustable`; `charactersPerSecond` must be finite in1–240 even in instant mode.

Each binding override replaces one complete action and requires all three arrays (`keys`, `buttons`,
`axes`), at least one key and at least one button or axis. Key identities are case-sensitive Godot
`Key` enum names (for example `I`, `F1`, `Space`, `Escape`); `None` and standalone modifier keys
`Shift`/`Ctrl`/`Alt`/`Meta` are rejected, and key chords are not part of version1. Button identities
are the names in the table plus `Start`, `LeftStick` and `RightStick`. Signed axis identities
are `LeftX-`, `LeftX+`, `LeftY-`, `LeftY+`, `RightX-`, `RightX+`, `RightY-`, `RightY+`.
Unknown fields/actions/identities, conflicting bindings, missing device access and invalid values
produce `ContentError: invalid-input-settings` before a session is published. A missing or unreadable
explicit file also fails; it never silently restores defaults. No file selects documented defaults.
Settings are loaded once at startup; there is no in-game settings UI or automatic settings save.

Adjustable text uses the actual Label's visible-character count. Time reveals characters but never
submits a gameplay command. Confirm while text is incomplete reveals the rest and preserves the real
wait token; a subsequent Confirm acknowledges. A choice with incomplete text likewise reveals first
and still requires a semantic Yes/No input. Instant text displays everything and still awaits its
normal acknowledgement and choice. A completed reveal cannot complete a presentation cue.

Reduced-flash keeps white FadeOut/FadeIn overlay opacity at0 for the same service duration. It still
submits `CompletePresentation` with the reached token and cue kind. Black fades and other services
retain their existing behavior. This intentional visual deviation, logical remapping and modern text
progression belong to ADR0010 9A/10A; they do not establish original visual/timing parity or 8C/H4.

### Native 9A observation

After the locked environment and Debug adapter build, select a fresh ignored output directory. The
bounded external observer writes an authored variation/settings into the specified destinations,
then instantiates ordinary `Main.tscn` with those actual startup arguments. It never sets session
state. It adds white presentation cues, a second spell, ordinary physical definitions/rewards and
an adjacent durable opponent to the selected existing authored world.

```powershell
$env:SF2_INPUT_CASE = 'keyboard' # gamepad, remapped-keyboard, remapped-gamepad
$env:SF2_INPUT_VARIANT = 'harbor' # hill exercises another world, actor and spellbook
$env:SF2_INPUT_RATE = '20'       # also observe80 with the hill variation
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'input.json'
$package = Join-Path $env:SF2_RUN_OUTPUT 'input-package.json'
$settings = Join-Path $env:SF2_RUN_OUTPUT 'input-settings.json'
$arguments = @('--headless', '--path', 'remake/game', '--fixed-fps', '60',
    '--script', 'res://probes/engine_input_accessibility_observation.gd', '--',
    '--authored-package', $package)
if ($env:SF2_INPUT_CASE.StartsWith('remapped-')) { $arguments += @('--input-settings', $settings) }
& $godotBinary @arguments
```

The remapped cases replace every action binding, move stick input to the right stick, swap the
Confirm/Cancel convention and select reduced-flash/adjustable text. Require `passed:true`, empty
failures and clean process errors. Observe actual field movement, decline/re-prompt/acceptance,
partial/full text reveal with retained waits, both real completed white cue tokens, the same session
at first battle control, movement/cancel, two spells, another allied target, actual HEAL, another
actor's Stay, next-round attack and binding-derived help. Gamepad cases also check repeated axis
values, unrelated trigger events and release edges against the actual session revision/preview.
Compare default keyboard/gamepad and remapped harbor outputs at the matching semantic checkpoints:
flags, actor, round, resources, seeds and white token/kind completion must agree; only the intended
presentation/settings differ. The hill case checks independent valid content rather than a fixed
receipt sequence. Default white opacity reaches1; reduced-flash stays0.

For the current private continuous outcome/return observation, reuse the existing private selection,
R1 start and opening navigation setup from [exploration reproduction](field-programs.md#reproduction).
Select `SF2_INPUT_CASE=private-remapped-gamepad`, `SF2_BATTLE_OUTCOME_CASE=victory`, and the new observer
script, with `--private-exploration-start <selected-R1-start> --input-settings <ignored-settings-path>`.
It reuses the existing actual-input outcome driver, including its single-frame input cadence, with
all commands mapped to the remapped/swapped gamepad. This case uses instant text and ordinary flashes;
the authored cases own the paired accessibility comparison. Require the existing main/admission/
outcome reports to pass through usable return movement, retained session identity and valid node
projection. The settings startup matrix uses the existing `engine_battle_observation.gd` startup
case with EXPECT_FAILURE=`invalid-input-settings`; require no session/actors/RNG on rejected input.
Each launch tests an actual startup configuration in the retained project/installation; no editor,
project copy, screenshot, original emulator, reference replay or test of the observer is needed.

## Battle Movement Range Observation

[B8 agreement](../godot/presentation.md#battle-movement-range-agreement) owns semantics and layering.
`engine_movement_range_observation.gd` reuses the existing world-viewport observer's input, focus,
deadline and output helpers. It performs no post-start state injection. Its four cases are finite
checkpoints, not a whole-battle or opening replay. Reports retain actual state, range draw destinations,
board transform/order, actor nodes and lifecycle boundaries. No images or frame captures are produced.

Load `local/private-inputs.ps1`, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and build the actual
adapter Debug project from `remake/` with the existing absolute SDK. Reuse its Godot installation.
For each run choose a new ignored output directory, with APPDATA/LOCALAPPDATA/TEMP/TMP beneath it.
Set `SF2_VIEWPORT_CASE` to a case below and `SF2_VIEWPORT_OUTPUT` to its `native.json`, then launch
from the repository root (replace the selected entry option/input with the table's entry):

```powershell
& $env:GODOT_BIN --headless --path remake/game --max-fps 60 --resolution 1920x1080 `
  --script res://probes/engine_movement_range_observation.gd -- --authored-package $selectedInput
$nativeExit = $LASTEXITCODE
```

Read both logs and the actual exit status. `passed:true` alone does not excuse script errors or
shutdown warnings. Omitting `--headless` selects a bounded windowed integration check; do not claim
windowed or pixel acceptance from a headless report.

| Case | Pre-start input | Named boundary |
| --- | --- | --- |
| `corridor` | Ignored variant of `practice-yard.json`: all terrain rows barriers except row3 all `g`; guard at(4,3), dummy at(1,3); all other fields unchanged. | Medic origin(3,3) has exactly(2,3),(3,3),(5,3),(6,3),(7,3),(8,3). Enemy blocks left travel, ally can be crossed but not stopped on, open budget ends at8. Mapped Right twice retains the same range during animation and after crossing ally; actions/Magic/Item/commit hide; Cancel restores through return animation; Stay refreshes guard's origin and occupied tiles. |
| `automatic` | Ignored variant of `stone-court.json`: actor2 move3; placement2 strategy `attack-then-approach`, x7; placement3 x6,y5; all other fields unchanged. | Mapped Stay reaches actual Automatic movement with range absent. |
| `outcome` | Tracked `progression-outcome-victory.json`, unchanged. | Mapped interaction enters battle, one normal physical command defeats the one-HP enemy; range clears at target/commit/scene, remains absent during field-owned outcome dialogue on the retained battle backdrop and after usable return in the same session. |
| `private-region` | `--private-exploration-start remake/reference/inputs/engine-battle02-egress-start.json`, with retained `local/issue1047/inputs.ps1`, `SF2_PRIVATE_EXPLORATION_CONTENT=local/issue1047/world-01.json` and `SF2_PRIVATE_CONTROLLED_START=remake/reference/inputs/battle02-outcome-party.json` resolved to absolute checkout-local paths. | Existing controlled Battle02 entry/skip reaches actual terrain, foreground and sprite projection at region(16,19); actions/Magic/self-target/retreat clear the overlay; same-session usable field return detaches the battle range. This is not natural original reach. |

Generate the authored variants before startup using Python and fresh ignored paths; no production
fixture change is required. For example, from repository root:

```python
import json
from pathlib import Path
out = Path("local/issue1395")
out.mkdir(parents=True, exist_ok=True)
d = json.loads(Path("remake/content/authored/practice-yard.json").read_text(encoding="utf-8"))
d["terrains"][0]["rows"] = ["gggggggggggg" if y == 3 else "############" for y in range(9)]
p = d["encounters"][0]["placements"]
p[1].update(x=4, y=3)
p[2].update(x=1, y=3)
(out / "corridor.json").write_text(json.dumps(d), encoding="utf-8")
d = json.loads(Path("remake/content/authored/stone-court.json").read_text(encoding="utf-8"))
d["actors"][2]["move"] = 3
p = d["encounters"][0]["placements"]
p[2].update(aiStrategy="attack-then-approach", x=7)
p[3].update(x=6, y=5)
(out / "automatic.json").write_text(json.dumps(d), encoding="utf-8")
```

**Confirmed:** executor headless cases corridor-final, automatic-01, outcome-04 and private-region-final
passed with actual exits 0 and no script errors. They total about 14 seconds and 0.75MB of checkpoint JSON.
Private region retains two ObjectDB shutdown leaks; authored cases have clean logs. No process remains.
Engine behavior tests cover weighted/impassable terrain, exact budget, map edges, occupancy,
read-only immutable results, preview/return anchoring, stage/story/round/field clearing and actor refresh.
The selected engine run passed 1642, skipped 31 private-input cases, failed 0; adapter Release and Debug
builds passed with zero warnings/errors. Native private success is separate from those skips.

Preserved failures under ignored `local/issue1395`: movement-tests-01/02 expected a pause after Stay,
although the real turn flow advances immediately; corrected query tests pass. corridor-01 compared
JSON float arrays to integer literals; private-region-01 guessed terrain node names; corrected
checks use numeric operands and actual nodes. story-01 assumed an initial wait at an input start and
was retired from the selected coverage. outcome-01 missed DialogueWait input; outcome-02 missed
field-owned after-program presentation; outcome-03 incorrectly required its retained battlefield
backdrop to be hidden. Corrected observation checks range visibility on that backdrop and after
return. Earlier reports/logs remain intact. These observer failures are not product or evidence waivers.

Independent Release movement/query checks passed 15/15 with no skips, using the existing build.
**Confirmed:** independent windowed corridor, private-region, automatic and outcome observations
passed on the same production code with the observer phase waits/receipts below. All four actual
exits were 0, with no failures, Unavailable results, runtime/script errors or warnings. The retained
`main-gate-window-results.json` records 30 checkpoints, 29.407 seconds and 1,088,375 bytes. This
includes legal tile sets, fixed geometry/layers, preview/cancel, next actor, automatic clearing,
private nonzero-region projection, targeting, retreat, outcome story and usable field return.

The first independent windowed corridor run (`main-gate-corridor-01`) failed its Cancel-restoration
check: earlier legality/preview/menu checks passed, but the final receipt remained ActionChoice at
revision12 with preview(5,3), no movement and no range. Exit1 and clean stderr are preserved.
The cause is **Unknown**. A windowed diagnostic recorded the expected first Cancel to ActionChoice
and second Cancel to Return movement without a production change. The observer now explicitly
waits for those two phase boundaries and retains menu/command receipts; a bounded windowed check
passed. There is no extra Cancel, automatic retry or weakened restoration expectation. A passing
diagnosis does not explain or erase the initial failure.

**Unknown:** complete fixture/full-game coverage, original range pixels/cadence, the initial
nonreproduced windowed Cancel sequence, earlier physical focus-release boundaries and the private headless shutdown
warning cause. The earlier A/B/C failures and Unknowns remain in their owners. This slice adds no
research/H3/H4 comparison or screenshot requirement.
