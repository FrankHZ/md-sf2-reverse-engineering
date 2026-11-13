# Host and Input Verification

Startup, logical input and accessibility checks observe the actual ordinary Godot host.

The [composed opening-loop acceptance](../evidence/opening-loop-acceptance.md) joins the entry,
field preparation, Search, Church, Battle01 outcome and restart recipes below. It owns the bounded
end-to-end reproduction, retained child evidence and final state/resource comparison.
Its [current human play preparation](../evidence/opening-loop-acceptance.md#current-human-play-preparation-1456)
is the ordinary launch/short-route guide. Historical probe recipes below retain their original
observed boundaries; preparing #1456 does not rerun them or establish user acceptance.

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
startup ContentError at the title before creating a session. They cannot select legacy play.

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

## Shared visual menu pilot (#1453)

The [Stage1 visual agreement](../godot/presentation.md#shared-visual-rules-and-finite-consumers-1453)
changes only shared disabled text and the existing battle list/navigation colors. Reuse the
[menu pointer recipe](#menu-pointer-observation), its prepared `local/issue1408/battle.json`
and keyboard/pointer settings. This input already supplies four admitted long spell names,
available/unavailable MP offers, empty items and available/out-of-range targets. No new gameplay
fixture, media acquisition, scene setter or whole-route replay is needed.

After loading the existing local input/tool selections, build the affected adapter and necessary
Debug native assembly using the [locked SDK workflow](../development-and-verification.md#locked-net-workflow).
Launch ordinary Main at 1920×1080 with that authored package. Extend the existing
`engine_menu_mouse_observation.gd` only in ignored Issue output to read actual Label font colors,
font sizes, modulate, text, visibility, focus/mouse filters and rectangles, plus frame style values.
Read named action samples rather than frames. Give each serial run fresh worktree-local writable
directories and a 45-second process bound; retain all results under `local/issue1453`, at most
100 MiB. No screenshot, observer test or engine/full suite belongs to this styling check.

1. Pilot Magic: inspect available selected/unselected and unavailable selected rows using keyboard
   and pad. Expect font colors `ffe19a`, `f4f5fc` and `8e9bab` respectively, white modulate,
   30px rows, `›` only on the selected row and literal unavailable/Not enough MP explanation.
   Confirm on the unavailable row must retain revision, RNG, inventories and HP/MP.
2. On the same representative consumer, reuse the existing pointer battle sequence for long-name
   wheel scrolling, enabled/empty-item selection, target navigation, unavailable-target refusal,
   Back and explicit commit. Check enabled/unavailable Confirm colors, retained compact label,
   26px/44px navigation within the description and native viewport. A real secondary-window focus
   transfer while pressed must reject the orphan release; fresh input works. Keep drag-out,
   keyboard takeover, layer-replacement and double-release guards from the existing recipe.
3. Reuse the existing `emphasis-icons` short private case and its already admitted raster package
   for unchanged 24px icons at nearest 4×, selected/normal resource use, 288×224 diamond geometry,
   stable hit regions and normal focus emphasis. The authored menu reads actual 96px/4× terrain
   and fixed world zoom. Source diff review must exclude all diamond/art/animation/geometry logic.

**Confirmed, bounded executor observations:** the representative pilot and final menu sequence
pass in native 1080p, including actual colors/fonts/frame margins, long-name scrolling, key/pad
selection, unavailable reason/refusal with no resource mutation, complete pointer/focus/release
gestures and navigation bounds. The reused private icon case also passes. Exact heads, commands,
receipts, retained intermediate geometry finding and CI belong to the Issue/PR handoff.
These checks establish engineering behavior, not human readability/aesthetic approval or physical
controller release. #1456 retains those and actual 1080p-display/DPI checks. Existing WAV/PlaybackWAV
warning causes and original timing/media remain Unknown; #1447 remains deferred. Other consumers
are explicitly dispositioned in the presentation inventory.

### Finite consumer migration

The [current agreement](../godot/presentation.md#shared-visual-rules-and-finite-consumers-1453)
selects host New Game, Church/Prepare/Party, Give/Discard/Retreat results and the real auxiliary HUD.
Use the same SDK/project/input setup and a fresh `local/issue1453/stage2` run directory per process:
100 MiB total, serial native runs at most 45 seconds. Observe named input/semantic boundaries,
real node font/color/geometry/visibility and errors, without screenshots or frame dumps.

- Menu pilot: ordinary New Game using the already prepared opening/services input, then the existing
  Party/Prepare and actual Church caller. Read New Game's native button style, Party member marker/
  focus versus ordinary Prepare/Close accents, and unavailable Prepare/Church color plus marker and
  actual explanation/refusal. Keyboard/pad/pointer navigation and normal follow-up must preserve
  revision, party, gold and RNG on refusals and return usable field control. Retain #1454's separate
  wording/transient refusal boundary. Older controlled starts that omit product preparation or add
  duplicate palette definitions are not substitutes for this product-path observation.
- Result/HUD pilot: reuse `battle-give.json` and the existing Give observer's real movement, commands,
  result/reveal and state/error helpers. Observe current item/recipient/index/count, pad cycling,
  free provisional selection, shared 30px frame/body, focus/hide suspension, reveal/acknowledgement
  and exactly one transfer followed by control. The HUD now exposes `hudPanel` reading geometry;
  its retired label-stack/wheel geometry assertions are not the current acceptance interface.
- Long text: a disposable variation of the existing authored Give weapon name, within its admitted
  text boundary and deliberately containing line breaks, makes HUD and result exceed one reading
  area. Confirm must read without changing revision/resources; candidate/Target switches reset
  reading, Cancel stays available, and result acknowledgement waits for complete reading.
  Keep the authored variation and receipts local; it proves modern reading behavior, not original
  game text, natural reach or timing. No new content/parser/verification framework is needed.
- Distinct siblings only: `battle-discard.json` for protected refusal, duplicate-slot pad navigation,
  shared result and same-actor continuation; `battle-equipment.json` for current offer/ATT/range,
  provisional/cancel/confirm and usable movement; existing controlled Battle02 Egress inputs for
  shared Retreat text, hidden text during effect, retained cue/reduced-flash and same-session usable
  field return. Do not replay the whole opening or all old probe routes.

**Confirmed, bounded executor observations:** these finite native 1080p cases pass. The representative
menus and Give precede sibling checks; long-name/current-choice/result reading retains commands and
one-time mutations. Earlier build/input-admission failures remain in the local handoff. Adapter,
necessary Debug, direct document/source/private checks and committed plan/actual CI complete the
engineering evidence. Appearance stays deferred to #1463; human aesthetics/physical release/display/
DPI stay #1456. Existing warning records and original timing/media Unknowns remain; #1447 is deferred.

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

Current review [#1570](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1570) retains
**Standard** rigor: distinguish current shutdown-only retention from a demonstrated project owner
defect, consuming accepted F8 code. High-rigor state/save boundaries remain unchanged. Scope is the
existing audio observers and these two owning documents unless resource attribution establishes a
production repair. No gameplay, save, engine version or audio timing changes are admitted. Initial
pilot: at most six serial headless runs, each at most60s, total new reports at most15MiB. Native IDs,
actual child exit and retained stderr are acceptance evidence; counts or JSON success alone are not.
Use the transferred imported checkout/shared editor; do not control the reserved user play instance.

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

### Current exit attribution (#1570)

**Confirmed:** current production base `626eb6277db8af112c04cb91894c1492ce233ce0` includes F8.
The six serial runs below use the same installed Godot identity cited above, headless Dummy and
project-authored silence. The current reflection constructor signatures in `AudioLifetimeObservation`
remain valid; no constructor repair was necessary. Only existing observers and the audio/host
documents change. Production `SessionAudio`, `GameRoot`, Application and save behavior do not.

| Retained run under `local/issue1570/` | Native PID / ms / exit | Resource and stderr result |
| --- | --- | --- |
| `scene-01` |39128 /1550 /0 | F8 assertions pass; two leaked instances, last restored areaB WAV/playback, each refcount1. |
| `backend-stop-01` |36340 /247 /0 | Stop/clear/queue-free then immediate quit; exactly the standalone voice's WAV/playback remain, each refcount1. |
| `backend-runtime-01` |2744 /1363 /0 | Four natural Finished callbacks; each interrupted pair is initially alive then invalid; no retained targets or stderr. |
| `session-01` |18900 /2562 /0 | Four actual SessionAudio cycles, sixteen completions,54 released IDs in13 samples, active disposal and node removal; no stderr. `_ExitTree` calls Dispose again without error. |
| `scene-drain-01` |29812 /896 /0 | Same F8 stages; native-ID predicate observes all16 IDs invalid after disposal; no stderr. |
| `scene-immediate-final` |19044 /803 /0 | Final observer code, drain option unset: F8 passes, same terminal pair remains and exit warning returns. |

Each launch used the actual Godot `.exe` headlessly, not its `_console.exe` wrapper, with fresh
worktree-local APPDATA/LOCALAPPDATA/TEMP/TMP/log directories. Each `process.json` records PID,
elapsed time, `timedOut:false`, `stopped:true` and captured native exit0. All six processes stopped;
none was killed or left running. stdout, stderr and reports are preserved. Total new observation
data is below0.25MiB, well below15MiB; the longest launch took2562ms. No new native launch is needed
for document-only follow-up. The reserved play checkout/processes were inspected read-only.

The F8 probe records native IDs at initial music, scene selection, finite start and previous-music
restart, then queries validity after each `using` scope has disposed its actual SessionAudio.
Both immediate runs end with WAV `9223372064302302679` and playback `9223372064319079896`:
these match the **second** variant's `previous-restarted` IDs and the verbose leaked instances.
Only that pair is alive after the second disposal; the first variant's pair has already vanished
during normal processing. The standalone backend's leaked WAV `9223372063798986198` and playback
`9223372063815763415` likewise match its created voice. All have refcount1; no leaked player node or
other object class was reported. The accompanying orphan `Master` StringName is retained separately
in verbose output. IDs are process-local: equality across runs does not identify the same object.
GDScript prints signed64 IDs; compare modulo2^64 with Godot's unsigned verbose IDs using integer
arithmetic, never floating-point JSON numbers. The observations retain integers, not managed resource
wrappers; they never call C# `GodotObject.IsInstanceIdValid` or force GC.

Reproduce from the existing imported owning checkout after the protected SDK setup above and a
Debug build if observation sources changed. Use the existing bounded local launcher when retained:
`pwsh -NoProfile -File local/issue1570/run-native.ps1 -Case scene -Run <fresh-name>`.
Its other cases are `backend-stop`, `backend-runtime`, and `session`. The underlying portable
selection is the existing native argument-array recipe with `--headless --audio-driver Dummy`,
`--verbose`, and the following scripts/environment:

| Case | Script under `res://probes/` | Environment |
| --- | --- | --- |
| F8 immediate | `engine_scene_music_context_observation.gd` | `SF2_SCENE_MUSIC_OUTPUT=<fresh>/native.json`; unset `SF2_AUDIO_DRAIN_EXIT` |
| F8 release contrast | same | same output selection, `SF2_AUDIO_DRAIN_EXIT=1` |
| Backend immediate | `engine_audio_backend_observation.gd` | `SF2_AUDIO_CASE=stop-quit` |
| Backend repeated | same | `SF2_AUDIO_CASE=runtime` |
| SessionAudio repeated | `engine_audio_lifetime_observation.gd` | `SF2_AUDIO_OUTPUT=<fresh>/native.json` |

Launch the actual executable selected beside the configured console executable, retaining its exit:

```powershell
$nativeGodot = $env:GODOT_BIN -replace '_console\.exe$', '.exe'
# $arguments: the selected script/flags above, explicit project and fresh --log-file destination.
& $nativeGodot @arguments 1> (Join-Path $run 'stdout.log') 2> (Join-Path $run 'stderr.log')
$nativeExit = $LASTEXITCODE
```

Use the60s process bound and preserve timeout/termination as failure, not exit0. The retained launcher
uses `ProcessStartInfo.ArgumentList`, hidden/no-window background execution, redirected output and
`WaitForExit(60000)` on that actual process. Do not infer its status from a console wrapper.
The optional F8 release contrast advances ordinary process frames only until tracked native IDs
become invalid, with a2s failure deadline. It adds no fixed sleep, production drain, gameplay delay
or synthetic music completion. Default F8 completion and immediate quit are unchanged.

**Confirmed bounded disposition:** current F8 immediate-exit retention and independent backend
retention reproduce the pinned upstream deferred deletion limitation. The source seams cited above
were rechecked: Stop marks fade/deletion, the mixer schedules list erasure, main-thread cleanup
unrefs playback, and shutdown does not drain live entries. Real project disposal reaches its release
owner; no project lifecycle repair is justified by these observations. Keep the visible warning and
the upstream-version limitation. Debug and Release adapter builds pass; no new engine behavior tests,
full suite, emulator or campaign replay is warranted by observation/document changes.

**Inferred:** the older F8 `local/observations/scene-music-context/adapter-first` warning shares this
cause. **Unknown:** exact class/ID/owner of that original non-verbose two-instance record and the
retained #1540 `continue01` two-instance / #1539 `continue01` eight-instance stderr records. Those
small historical logs were read, not overwritten; counts alone cannot exclude different causes or
establish growth. Other older two/four/eight-instance records remain unclassified unless their own
verbose evidence identifies them. Current desktop close, other drivers/versions, whole-campaign and
unbounded runtime behavior are not newly observed. Preserve all #1410 failures above and later
failed aggregates, including the separate private admission probes addressed in#1574; this clean
bounded evidence neither reruns nor turns those historical failures into passes.

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

## Window boundary design observation (#1455)

This is Stage1 design evidence at accepted `c58d41ecb1c11ed93d8c13b29c9b4a608d726ee6`, using the
existing Debug host from accepted #1452; no production code, SDK/build or observer platform changed.
The [support/recovery proposal](../godot/presentation.md#window-support-and-recovery-1455)
was independently accepted as Stage1; the selected implementation and its receipts follow below.
An observer's exit 0 means its bounded observation completed;
recorded out-of-viewport controls are product findings, not a claim all geometry passed.

**Confirmed:** two serial native Godot 4.7.2 runs, no screenshots or frame capture:

| Receipt / reproduction seam | Actual result and limitation |
| --- | --- |
| `window` / ordinary title → Witch → New Game slot/name → settled field, Party, Prepare, Settings → mode restore | Entry 1280×720 uses logical 1920×1080/CanvasItems, final scale 2/3. Field 1280×720 uses native viewport/Disabled, pixelScale 4/cellSize 96. Party/Prepare retain revision 4 and the same session throughout resizing/menu cancellation. Close and Prepare lie entirely outside 720p; their 1080p action bounds fit. Settings remains reachable at 720p. |
| Same run, baseline hint bounds | PartyHint ends at x2503; Prepare Hint ends at x2208 in 1920×1080. Both use long installed default bindings. These are **failed containment findings**, even though navigation/cancellation completed. Bounds establish clipping risk and unreachable pointer areas, not human typography acceptance. |
| Same run, native Window metadata | Client 1920×1080 has decorated 1946×1151 on this host. Physical display readback 3840×2160; ordinary fullscreen becomes 3840×2160, scale 4/cellSize 96; returning windowed restores 1920×1080. The checkpoint name `fullscreen1080-display` is misleading: its recorded native size is 4K. No physical 1080p-display test occurred. |
| `battle` / accepted controlled Battle02 before/load/start → mapped intro skip → command menu → Egress → usable Map3 return | Native 1280×720 and restored 1920×1080 at settled battle/menu/returned-field checkpoints; command menu sampled controls fit 720p, mapViewport equals viewport, zoom 1. Existing actor/terrain readback retains 96px cells. Egress returns in the same session and accepts ordinary movement. This separate controlled segment is not NewGame→Battle01 completion or natural reach proof. |

Executor receipts are `local/issue1455/pilot-01/native.json` (1769ms) and
`local/issue1455/battle-01/native.json` (3342ms); report/log/script detail remains private/ignored.
Both complete with empty observer failures and no script/C#/engine errors; the window report records
no native pad interference, the battle report has no unavailable boundary, and both stderr logs are
empty. Earlier #1445 pilot-01's 720p Party failure, #1451 failed observers/stale-input findings, #1452's oversized binding failure and historical audio/focus failures remain
preserved in their own handoffs. These two clean exits do not resolve prior WAV/Playback warnings.

Reproduce the product seams using the existing configured Godot project/Debug host, fresh ignored
reports and disposable external save namespace. Load `local/issue1045/inputs.ps1` and
`local/issue1441/inputs.ps1` in the launching process. Use separate CLI arguments `--resolution`,
`1920x1080`, then actual Window.size 1280×720 and restore; read Window.size, visible rect, content-scale mode/size, final transform, decorated size, current screen
and usable rect, visible control GetGlobalRect and existing ReadObservationJson at each checkpoint.
Use ordinary mapped keys and transformed complete pointer clicks, never session setters. For the
separate battle segment, load `local/issue1047/inputs.ps1`, select its existing world and controlled
Battle02 party, then use `--private-exploration-start` with
`remake/reference/inputs/engine-battle02-egress-start.json`. Follow the existing
[region recipe](#unified-world-viewport-observation); insert only the 720p/restore samples around
settled battle/menu/returned-field boundaries. Do not duplicate private worlds/assets or replay a
full battle. Retained local wrappers:

```powershell
# Existing ignored wrappers resolve paths from their owning repository; use fresh Run names.
pwsh -NoProfile -File local/issue1455/run-native.ps1 -Run <fresh-window> -Phase window
pwsh -NoProfile -File local/issue1455/run-native.ps1 -Run <fresh-battle> -Phase battle
```

The pilot preceded battle expansion. O(visible controls × finite checkpoints), fewer than 100 samples,
20MiB new-output budget and serial 45-second launch bounds were sufficient; actual new local evidence
was about 3.7MiB after these runs, with about 2.8GB free on H. No SDK/cache/tool/media installation or
copy, broad runner repair, engine/adapter/research/full suite, acquisition or old-evidence cleanup.
Both owned processes exited; no Godot/SDK remains. Replan before exceeding the budget or adding cases.

### Selected implementation observation

**Confirmed, bounded Stage2 executor receipts:** actual adapter compilation and native observations,
using the existing project/install, accepted input selections and disposable external namespaces.
The existing Debug host was rebuilt because new adapter code had to be loaded; this is not a docs-only
Stage1 rebuild. New ignored evidence stays under `local/issue1455/stage2` with 100MiB total budget and
serial 45-second launches. No installation/cache/media copy, full route, screenshots or new platform.

| Receipt | Native result / stopping boundary |
| --- | --- |
| `pilot-04`, `pilot-final-01` |1923/1777ms. Final New Game at 720p refuses before save/mount; a native 1280×720 maximum constrains Restore, leaves recovery and a visible reason. Clearing that constraint permits normal Restore, retaining confirmation until fresh input. Dirty Settings and pending Save survive recovery→Quit→Cancel→restore; gameplay remains paused under the restored modal. |
| `field-standard-03`, `field-swapped-01` |3429/3306ms. Actual 1080p Party/Prepare hint bounds/text fit with installed standard/swapped mappings. Held Confirm/echo cannot leak across restore; pending replacement-name draft/editor focus survives. Native 2560×1440 edge placement does not pause/recenter; 4K fullscreen retains 96px/4×. A Save-button press before recovery cannot activate on orphan release after restore. |
| `restore-first-fullscreen-01`, `restore-fullscreen-rollback-01` |1119/925ms. Fullscreen startup has no known windowed borders. First constrained Restore reads actual 26×71 overhead and rolls back prior mode/size; the second case specifically returns to prior native 4K fullscreen when its windowed client is constrained below minimum. A fresh unconstrained Restore centers an outer rect contained by this 4K desktop. No New Game publication. These are controlled native size requests, not a physical 1080p-desktop fit test. |
| `quit-02` |1510ms to prompt. Existing dirty-progress confirmation is reachable by actual pointer input at 720p; prompt/control geometry, progress and saved bytes retained. A deliberate click on Quit ends the owned process with exit 0. Cancel preservation is separately covered by pilot. |
| `continue-02`, `backup-01` |1106/1064ms. Fresh-process Continue and explicit backup recovery both refuse 720p before mount/publication, retain form/files and require fresh confirmation after Restore. Backup bytes remain unchanged; only the explicitly confirmed admitted recovery publishes a primary. The tiny disposable backup fixture reuses the generated pilot save, not user progress. |
| `hold-02` |1732ms. Held wait/pad direction cannot advance while recovered or restart an old hold on return. Release then fresh wait works; saved bytes remain unchanged. Events use synthetic device 42, not a physically operated controller. |
| `battle-02` |3208ms. Accepted controlled Battle02 entry/skip/menu/Egress→usable Map3 return. Resize while skip is held pauses without continuation debt; battle/menu/return 720p requests show recovery, restore 1080p and retain the same session. Native 4K battle-menu bounds retain zoom 1, cell 96/pixelScale 4. No whole Battle01/campaign or original reach claim. |
| `availability-03` |1689ms. **Controlled presentation availability**, with both modes passed as unavailable to the existing view while the physical 4K screen is unchanged. Native 1280×720 body has 235px scroll range: keyboard/pad Left/Right reach bottom and return to 0, Up/Down skips disabled actions without losing reading position, and fixed Quit/navigation stay in bounds. Pointer Quit reaches the existing confirmation. This does not validate a real small display. |
| `diagnostic-01` |1225ms. Controlled start requested at 720p has no mounted view and retains a pending source behind recovery. Restore admits 1920×1080 and mounts that source once; no fabricated session/completion injection. |

Reports contain observer failures/geometry failures/unavailable lists; require their emptiness,
actual process completion and clean script/C#/engine logs together. The first-fullscreen run retains
the existing two-ObjectDB exit warning category; this slice does not resolve WAV/Playback lifetime.
Retain completed earlier attempts: Slot1/Slot2 fixture mismatch, held Confirm activating recovery
before the intended mouse click, incomplete nested-menu unwinding, an ambiguous underlying Quit
button query, a battle state-array versus terrain-readback lookup, and inherited GDScript helper
signature errors. Corrected observers use the same product boundaries; these failures and logs are
preserved in the local handoff, not relabeled as interrupted or hidden by a full-suite rerun.
The controlled-unavailable pilot also exposed a real recovery-layout failure: container minimum height
expanded past 720p. A finite manual panel layout now fixes actions/navigation and scrolls only the
explanation; availability-01/02 failures remain preserved, with 03 passing the same containment/readability boundary.

Reproduce with fresh Run names using `local/issue1455/stage2/run-native.ps1`: phases `pilot`, `field`
(optional `-Variant swapped`), `restore` (native fullscreen startup), `quit`, `hold`, `battle`, and
`continue`, `availability` (explicitly controlled view availability) and `diagnostic`. Continue uses an
existing disposable namespace via `-Namespace`; backup uses a fresh
`-Variant backup` namespace and the generated pilot input named in the local handoff. Read actual
Window client/mode/decoration values, `ReadWindowSupportObservationJson`, mounted state/geometry,
settings draft/slot state, files and errors. Keep all canonical media/inputs read-only. Script wrappers
are ignored, not production hooks; no test of an observer/helper is added.

Run `uv run sf2 verify adapter`, the existing native Debug build, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, and direct source/docs/diff/private-boundary
checks. This host/UI-only implementation needs no engine/research/full suite. The small-desktop
startup fallback, unavailable modes and actual 1080p physical fit/DPI cannot be promoted from 4K/source
observations to hardware acceptance; matching human conditions remain open below under #1456.

### Later physical-focus human checklist (#1456)

Reuse accepted #1451's actual secondary-window transfer/return and held-Apply correction; its key/pad
input was injected. Field walking/skip and pointer owners retain their prior bounded observations.
No person physically operated keyboard/controller for #1455; all physical-release claims remain
**Unknown**, including whether pad release/neutral events reach GameInput while another OS window
owns focus. An unrelated physical pad event in an old observer is not this deliberate check.

At a safe disposable session, use the user's already-owned keyboard/controller only:

- Hold an exploration direction; Alt-Tab away, release outside, return. No resumed walking or repeat
  is allowed until a fresh direction; then stop normally. Repeat for a stick returned to neutral.
- In an ordinary menu hold Confirm/direction, leave focus, release outside, return. No activation or
  extra row change; one fresh press must work. Check the existing held-Apply/remapped return once.
- At the existing eligible before/load/start seam, hold mapped skip/wait, leave focus, release outside
  and return. Existing supported continuation must regain fresh control without repeated skip/wait.
- On an actual 1920×1080 display, check ordinary startup/Restore/fullscreen, decorations/work-area
  fit and accessible controls. A 1080p viewport on a larger display is not this check. Below-minimum
  clients must retain the journey/modal in the reachable recovery shell rather than crop gameplay.
- If a mixed-DPI setup is available, move between its displays, return and use pointer/menu controls.
  Record display size/scaling/fullscreen conditions and any clipping or misaligned input; unavailable
  hardware stays Unknown. Read dialogue, Party/Prepare/Church hints, battle results and Save panels
  for comfortable reading and a clear next action, without screenshot capture.

Record device, physical action, focus owner, observed state and any error; stop on unexpected movement
or confirmation and preserve the receipt. Human readability/feel belongs to
[#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456); no new device matrix or
hardware-timing claim. Missing participation does not block independent window implementation.

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

### Complete portrait raster observation

**Confirmed; [#1414](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1414).**
The accepted `ui-layout-static-v1` fixture locates `layout_PortraitWindow` at75342 and its mirrored
layout at75502, each8×10 words. Direct reading of the registered, verified ROM shows a border
around six columns and eight rows, selecting atlas columns0–5 in all eight rows. The mirrored
layout reverses those columns and flips their tiles. Therefore the source interior is48×64;
the prior48×56 crop lost the final tile row. Sarah1/Chester2 each lost384 nontransparent pixels,
and Astral40 lost383. Astral looking normal to the user did not establish that its lower row was
empty. All three admitted64×64 atlases and eye/mouth records match direct ROM decoding exactly.
This is a presentation defect; no extracted resource is repaired or regenerated.

`engine_story_dialogue_observation.gd` case `private-portraits` compares **every RGBA byte** of each
newly observed mounted texture with private expected bytes, then checks the actual child/resource
identity,48×64 texture,192×256 mount,304px panel minimum, safe bounds, child order and separation
from text/hints. Ordinary typewriting/blinking supplies facial states. A pre-start diagnostic caller
opens Astral, Sarah and Chester once on each side, then closes and returns to fresh field movement.
It changes only controlled input metadata, uses admitted assets and normal mapped acknowledgements,
and makes no original natural-route claim. No screenshot or post-start session mutation is used.

Reuse the prepared world/party and environment above. The diagnostic world must remain beside the
selected prepared world, because additional encounter selections resolve relative to that document.
Budget one127.6MB source read, one controlled copy of similar size, less than0.5MB expected bytes,
one process at a time, at most75 seconds and5MiB per report. Reuse the existing environment/build;
do not prepare another asset set. Run `uv run sf2 rom verify` before the direct decode. The following
Python, run with `uv run python` from the owning checkout, creates the ignored inputs:

```python
import base64, json, os, struct
from pathlib import Path
from sf2tool.compression import decode_stack_compressed
from sf2tool.h2.portraits import _read_animation_entries
from sf2tool.texture_extract import decode_md_4bpp_tile, md_palette_color, palette_index_rgba

source = Path('local/issue1045/world-route-13.json')
out = Path('local/issue1414'); out.mkdir(exist_ok=True)
doc = json.loads(source.read_text(encoding='utf-8')); world = doc['world']
rom = (Path(os.environ['SF2_SHARED_INPUT_ROOT']) / 'roms/sf2-us.bin').read_bytes()
expected = {}
for pid in (1, 2, 40):
    record = next(r for r in world['presentation']['portraits'] if r['portrait'] == pid)
    address = int.from_bytes(rom[0x1C8004+pid*4:0x1C8008+pid*4], 'big')
    data = rom[address:]
    eyes, offset = _read_animation_entries(data, 0, 'eye')
    mouth, offset = _read_animation_entries(data, offset, 'mouth')
    palette = [md_palette_color(int.from_bytes(data[i:i+2], 'big'))
               for i in range(offset, offset+32, 2)]
    raw = decode_stack_compressed(data[offset+32:], expected_output_bytes=2048).output
    tiles = [decode_md_4bpp_tile(raw[i:i+32]) for i in range(0,2048,32)]
    atlas = bytes(c for y in range(64) for x in range(64)
                  for c in palette_index_rgba(palette, tiles[y//8*8+x//8][y%8*8+x%8]))
    assert atlas == base64.b64decode(record['raster']['data'])
    assert eyes == record['eyes'] and mouth == record['mouth']
    for mirror in (False, True):
        words = struct.unpack_from('>80H', rom, 75502 if mirror else 75342)
        mapping = [words[(y+1)*8+x+1] for y in range(8) for x in range(6)]
        assert all((word & 0x7ff)-0x7c0 == y*8+(5-x if mirror else x)
                   and bool(word & 0x800) == mirror
                   for y in range(8) for x,word in enumerate(mapping[y*6:y*6+6]))
        for eye in (False, True):
            for mouth_open in (False, True):
                changes = {e[1]*8+e[0]: e[3]*8+e[2]
                           for entries in (eyes if eye else [], mouth if mouth_open else [])
                           for e in entries}
                rgba = bytearray()
                for y in range(64):
                    for x in range(48):
                        word = mapping[y//8*6+x//8]; tile = (word & 0x7ff)-0x7c0
                        index = tiles[changes.get(tile,tile)][y%8*8+(7-x%8 if word & 0x800 else x%8)]
                        rgba.extend(palette_index_rgba(palette,index))
                expected[f'{pid}:{int(mirror)}:{int(eye)}:{int(mouth_open)}'] = base64.b64encode(rgba).decode()
(out/'expected.json').write_text(json.dumps(expected), encoding='utf-8')

world['texts'].append(dict(id=65000,text='Portrait observation. The speaker continues talking '
    'while the eyes and mouth use their admitted animation tiles. The complete lower row '
    'must stay visible throughout this sentence.'))
instructions = []
for pid,entity in ((40,'entity-142'),(1,'entity-1'),(2,'entity-2')):
    for flags in (0,192):
        if pid == 40: instructions.append(dict(op='sprite',entity=entity,sprite=213))
        instructions += [dict(op='open-portrait',entity=entity,flags=flags),
            dict(op='show-text',mode='single',speaker=entity,speakerFlags=flags,
                 explicitWindows=True,text=65000),dict(op='close-portrait'),dict(op='close-text')]
instructions.append(dict(op='end-map-script'))
next(p for p in world['programs'] if p['id']=='cs-5149a')['instructions'] = instructions
with source.with_name('issue1414-world.json').open('w',encoding='utf-8') as f:
    json.dump(doc,f,separators=(',',':'))
```

After the adapter and Debug build above, select `SF2_PRIVATE_EXPLORATION_CONTENT` as that controlled
`issue1414-world.json`, `SF2_PORTRAIT_EXPECTED` as the absolute `local/issue1414/expected.json`, and
`SF2_VIEWPORT_CASE=private-portraits`. Use the existing Zone8 start, adjustable text settings and
native launch above, with a fresh local report/log/TEMP directory. Inspect actual exit, stdout,
stderr, report failures and coverage; private bytes stay untracked. The current executable field
comparison in `sf2tool.remake_h4.field_projection` also expects48×64. Its resource-size expectation
changes with the delivered adapter; original evidence remains unchanged. This adapter-only
expectation correction requires no original-data or whole-route H4 acquisition. The current automatic
planner nevertheless classifies that Python path as research and selects `public-core` plus
`tooling-python` beside `adapter-build`; `--scope engine` rejects this path as a research/shared
input. Preserve both outputs as a classifier limitation, not a passing engine-scope plan. Main-gate
review accepted direct owner inspection, mounted-pixel observation and adapter compilation as
sufficient for this exact dimension-only correction under the user's engine policy. It changes no
research extraction, inputs or comparison algorithm beyond the delivered resource-size expectation.
Broad local Python/research suites remain intentionally **NOT RUN**; this is not an unresolved
policy or merge blocker, and no planner/check configuration change is needed.
Hosted CI independently selects its narrow `research-public` job for this path; record its actual
result separately from the unexecuted broad local selections.

The focused headless run passed in41.893 seconds:56 checkpoints,357,187 report bytes,24 distinct
pixel states (all eye/mouth combinations for each portrait and side), exact byte equality,304px
panels, safe non-overlapping mounts, actual close and fresh field movement. Astral has no eye-entry
replacements; its logical blink states still produce the correct unchanged eye pixels. No runtime
or script error occurred. The run retains the known two-ObjectDB shutdown warning separately
tracked under#1410; exit0 and pixel acceptance do not resolve that warning. This is executor
headless evidence, not independent windowed or aesthetic acceptance.

Preserved failed attempts in ignored `local/issue1414` include four private admission failures from
placing the controlled world away from its relative encounter inputs, one61-second observer deadline
and one eight-second observer stop from comparing signed GDScript IDs with unsigned C# ID strings.
Corrected observation compares the presenter's textual child/resource identities and directly reads
the actual child's image. These are completed input/observer failures, not interrupted runs or
evidence that the corrected portrait pixels failed. Retain their logs and the two/four-ObjectDB
shutdown warnings. Earlier shared-dialogue failures and Unknowns above remain preserved.

## Party details observation

The [Standard E7 agreement](../godot/presentation.md#party-details-agreement) owns the read-only
member-status extension in [#1416](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1416).
`probes/engine_party_details_observation.gd` extends the existing
`engine_battle_return_status_observation.gd` and its native focus/input/return helpers. It loads
ordinary Main, sends mapped keys/pad and complete pointer events, and reads compact inspected fields,
actual rectangles, persistent member navigation, aligned stat/value rows, independent magic/item scrolling, complete portraits
and carried resources. It never sets session state, constructs a battle
for inspection, captures screenshots or records frames. Budget: one process, 75 seconds and less than
5MiB/report; reuse prepared data/builds. The pilot reports were below0.5MiB and4 seconds for field-only
cases. No asset acquisition, extra world export, route replay or new environment is required.

After loading the owning `local/private-inputs.ps1`, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`.
Run `uv sync --locked`, `uv run sf2 verify adapter` and the configured absolute SDK's Debug adapter
build from `remake/`. The small actual engine behavior checks in `PartyDetailsQueryTests` cover
joined/future/reserve/dead/absent membership, authored fallback, growth/equipment after real rules,
slot identity, unknown versus empty loadout and unchanged snapshot identity. The shared Domain attack
extraction was checked by `uv run sf2 verify engine`:1651 passed,31 private-input skips,0 failures.
Later query-only corrections pass all4 owning tests directly; the complete earlier failure remains
below rather than being replaced by repeated aggregate runs. The clean committed scope plan and
actual CI belong to the PR handoff.

Create only small pre-start controlled variants, from the owning repository root:

```python
import copy, json
from pathlib import Path
repo = Path.cwd().resolve()  # Run from the owning repository root.
out = repo / "local/issue1416"
out.mkdir(parents=True, exist_ok=True)
a = json.loads((repo / "remake/content/authored/harbor-arrival.json").read_text())
b = a["battle"]
actor, start, placement = (copy.deepcopy(b["actors"][0]), copy.deepcopy(b["start"]["actors"][0]),
                          copy.deepcopy(b["encounters"][0]["placements"][0]))
enemy = (b["actors"][-1], b["start"]["actors"][-1], b["encounters"][0]["placements"][-1])
b["actors"], b["start"]["actors"], b["encounters"][0]["placements"] = [], [], []
spell = copy.deepcopy(b["spells"][0])
for name in ["gentle-restoration", "renewed-vigor", "restorative-light"]:
    row = copy.deepcopy(spell); row["id"] = name; b["spells"].append(row)
for i in range(14):
    row, initial, deployed = copy.deepcopy(actor), copy.deepcopy(start), copy.deepcopy(placement)
    row["id"] = ("long-authored-party-member-name-for-roster-and-details" if i == 0 else "companion") + f"-{i}"
    row["items"] = [] if i == 1 else [0,0,0,0] if i == 3 else [0,0,127,127]
    row["spells"] = [dict(id=s["id"], level=1) for s in b["spells"]]
    initial.update(actor=row["id"], hp=0 if i == 2 else 95, exp=i)
    deployed.update(actor=row["id"], x=1+i%7, y=1+i//7, processingOrder=i)
    b["actors"].append(row); b["start"]["actors"].append(initial)
    b["encounters"][0]["placements"].append(deployed)
for destination, row in zip([b["actors"], b["start"]["actors"], b["encounters"][0]["placements"]], enemy):
    destination.append(row)
b["items"] = [dict(id=0, name="Authored restorative herb with a deliberately lengthy readable label for checking wrapped inventory information in the details panel",
                   effect="consumable-healing", power=10, minimumRange=0, maximumRange=1)]
for m in a["world"]["maps"]:
    m.update(battle=None, onLoad=None, events=[])
(out / "authored.json").write_text(json.dumps(a), encoding="utf-8")
```

The `authored` recipe/command above is the current independent public entry and is directly rerun
for F5. It preserves held-input, orphan/drag release,14-member scrolling, duplicate/full/empty slots,
defeated-member, live value, independent pane, close/reopen and subsequent movement checks. The
opening event is explicitly flushed before checking pause; normal field time may resume after close.
The following private/victory setup and initial-layout reports are **historical reproductions at
[`6c2d0982`](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/6c2d098212efebb26d026d335eb45c369914cff4)**,
requiring those named retained inputs in that checkout. They are not a current mandatory replay queue.
Current complete-world field/source-name/portrait observations use the [F5–F7 command](../godot/field-and-shop.md#reproduce-the-bounded-observations).

The private start reuses the retained viewport Zone8 approach and admits three joined allies; the
world remains the unchanged `local/issue1045/world-route-13.json`. Load `local/issue1045/inputs.ps1`
and select `SF2_PRIVATE_CONTROLLED_START` as tracked `remake/reference/inputs/battle01-player-ready.json`,
not the later grown Battle02 party. Preserve relative encounter paths by selecting the existing world
in place. No world copy or source/input modification is needed. The retained scene-start can be
recreated using the [viewport recipe](#unified-world-viewport-observation).

For each case use a fresh ignored output directory with worktree-local APPDATA, LOCALAPPDATA,
TEMP/TMP, `SF2_VIEWPORT_OUTPUT=<run>/native.json` and `SF2_VIEWPORT_CASE` below. Launch the registered
Godot from the repository root with separate arguments:

```powershell
$repo = (Get-Location).Path
$entryOption = '--authored-package'
$selectedInput = Join-Path $repo 'local/issue1416/authored.json'
$env:SF2_VIEWPORT_CASE = 'authored'
$nativeArguments = @('--headless', '--path', (Join-Path $repo 'remake/game'), '--max-fps', '60',
    '--resolution', '1920x1080', '--script', 'res://probes/engine_party_details_observation.gd',
    '--', $entryOption, $selectedInput)
& $env:GODOT_BIN @nativeArguments
$nativeExit = $LASTEXITCODE
```

The retained launcher is `local/issue1416/run-native.ps1 -Case <case> -Run <fresh-name>`. Require exit0,
`passed:true`, no failures/Unavailable and no runtime/script errors. A private shutdown warning stays
separate from those assertions. Reports contain only semantic checkpoints.

| Case / ordinary entry | Bounded executor observation |
| --- | --- |
| `authored` / `--authored-package local/issue1416/authored.json` | Persistent14-member navigation beside the field identity/vitals/attribute sheet, magic/items parallel columns and four learned spells. Long admitted item labels, duplicate/full/empty inventory and defeated member; mapped focus/selection, member-column visibility, independent scrolling, held opening/direction, orphan/drag releases, close and fresh field control. |
| `private` / `--private-exploration-start local/issue1416/private-start.json` | Actual connected Bowie/Sarah/Chester names, EGRESS1/HEAL1, Wooden Sword/Wooden Rod/Wooden Stick and Medical Herb from admitted metadata; future seeded ally excluded by current joined flags. Reliably mapped complete48×64 portraits at192×256, carried kills/defeats availability, current resources/stats, four slots/equipped markers and unchanged revision/resources/flags/seeds while browsing. The earlier unchanged single-joined-member start also passes. |
| `victory` / `--authored-package local/issue1416/victory.json` | The real accepted attack, reward/outcome and return route changes leader HP10→40, EXP0→48 and gold73→84. Read returned details, defeated-companion recovery, one outcome/return, repeated menu use and fresh field movement. No terminal state is injected. |

**Confirmed executor, retained initial layout:** authored/private/victory passed in3573/3137/7234ms
with6/5/14 checkpoints before the user selected original grouping. Main-gate independently passed
that old layout windowed in3577/3179/7387ms with clean stderr. Those results retain their data/input
boundary, not acceptance of the current layout. Current source-grouped checks are recorded below.
Private retains the known timing-dependent two-ObjectDB shutdown warning under the
[audio lifetime boundary](#audio-resource-lifetime-observation), with no runtime/script errors. These
are bounded executor observations; independent windowed acceptance and integration are recorded by
[PR1417](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1417). Physical hardware release outside focus and broader human/aesthetic approval remain
Unknown. The prior E2 observations below retain their original one-card/paging boundary; E7's current
member-status grouping supersedes that display only.

The current source-grouped layout passes authored/private/victory in3939/3390/7465ms with7/6/15
checkpoints (366339/530577/115511bytes). Native projections check the left portrait/KD/gold summary,
four row pairings, nonoverlapping label/value columns, parallel magic/items panes, a hidden-by-default
member dropdown, arrows, long names and independent scrolling. Read-only input/resource assertions
and the actual changed victory-return route remain. A focused private accounting-width observation
also passes in3393ms: unavailable counts use24px, known numbers30px; actual native minimum widths
fit the152px value field and24px summary border inset. Authored/victory logs are clean; private retains
the known two-ObjectDB shutdown warning. Query, Domain and Content sources/tests are unchanged by
this layout correction, so their completed checks remain valid by dependency.

`ExplorationRasterImages` extracts the existing decoder/crop only. The unchanged cache ownership,
mirror flag,64 atlas tile indexes,eye/mouth substitutions and texture disposal remain with their
existing callers. Direct reuse of the [complete portrait witness](#complete-portrait-raster-observation),
`local/issue1414/run-native.ps1 -Run issue1416-raster-extraction`, passes in41792ms with55 checkpoints,
357258bytes and all24 mounted source-pixel states. It retains the known two-ObjectDB shutdown warning.
No new world, pixel acquisition, screenshot or whole-route suite was introduced. The source-backed
layout agreement records the fixture orientation discrepancy without modifying research metadata.

Preserve completed failures in `local/issue1416`: initial query-test compilation used mismatched
nullable assertions and omitted the map constructor argument; the first completed engine run had
1649 passes/31 skips and one failing `MissingLoadoutIsUnknownRatherThanEmptyAndBattleHasNoFieldQuery`
whose fixture actually declared empty slots. Corrected owning tests pass. Authored native attempts
`-01/-02/-03` hit existing equipment-standalone-only, missing-item and battle-outcome admission bounds;
the failed-admission observer also emitted existing startup-read/JSON errors. `-04` incorrectly
required overflow for a short member whose text fitted. Private `-01` selected incompatible grown
party definitions; later correct registered selections pass. Victory `-01` found authored party flags
without source member bindings hiding all members; the query now preserves the authored carried-ally
fallback and the owning behavior test covers it. These were completed failures, not interrupted runs
or unavailable ROMs. Earlier E2, portrait, device and audio failures remain unchanged.

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

**Current production UI:** the full-card/cardinal-target geometry described here is retained historical evidence at [the pre-#1686 implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes). Do not use its old layout assertions as current acceptance. The [current F4/F8/F9 recipe](../godot/battle-interaction-baseline.md#direct-production-reproduction) covers compact cards, legal targets, bilingual resources and inspection; underlying source/disclosure/resource identities retain their original scope.

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
explicit file also fails; it never silently restores defaults.
An explicit `--input-settings` file is a read-only startup override. Ordinary launches without it use
the integrated [basic Settings/Apply and preferences persistence](../godot/presentation.md#basic-preferences-agreement),
including Witch menu and safe settled-field access plus grouped Controls help. Active battle/story
settings access and capture/remapping UI remain excluded; progress saves stay separate.

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

**Current production UI:** the full-card/cardinal-target geometry described here is retained historical evidence at [the pre-#1686 implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes). Do not use its old layout assertions as current acceptance. The [current F4/F8/F9 recipe](../godot/battle-interaction-baseline.md#direct-production-reproduction) covers compact cards, legal targets, bilingual resources and inspection; underlying source/disclosure/resource identities retain their original scope.

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

## Title and Witch entry observation

The [Standard A agreement](../application/opening-session-lifecycle.md#title-and-witch-presentation)
owns the current entry. Select `SF2_PRIVATE_ENTRY_CONTENT` as an absolute prepared `entry.json`,
alongside the existing palette-complete world/battle/static configuration. No controlled start is
selected for this route. Load private/tool settings in-process, protect locked .NET selections,
compile the actual adapter, and use its existing Debug project/installation. A missing running
instance requires startup for actual input/state/render observations; it does not authorize a
new project/toolchain or screenshots.

The bounded reproduction retained in `local/issue1441/run-native.ps1` uses fresh local APPDATA,
LOCALAPPDATA, TEMP, stdout/stderr and a compact JSON report. Its `entry.gd` drives mapped keyboard,
Pad Start/South/D-pad and real GUI pointer events, then reads `ReadEntryObservationJson`, the existing
field/persistence/audio observations and actual node rectangles. The headless engineering observation
is separate from a windowed human visual/device review. Reproduce from the owning worktree with
`pwsh -NoProfile -File local/issue1441/run-native.ps1 -Run <fresh-name>` after preparing the selected
entry package under the lifecycle recipe. Supporting bounded scripts in that same retained folder
cover Continue/replacement, resources, controls and music; their inputs and exact results are named
in the PR/local handoff. The tracked lifecycle recipe defines preparation without requiring those
ignored reports as source evidence.

Required observations are cold title layers/source dimensions/integer scale → held Start does not
skip the greeting → adjustable reveal/next/skip and Witch head/eye/mouth projection → actual actions;
Continue slot selection/cancel and restoration of an existing journey; real New Game to Map3;
replacement default Cancel and failed content admission retaining identity; repeated fresh starts;
Quit cancel/actual Quit; missing/corrupt entry diagnostics; reduced-flash and pointer reading.
The slot/name/initial-save flow below follows New Game; Normal is fixed with no difficulty selector.
Admission must require a looping Witch cue.

**Confirmed bounded adapter observations:** these paths run with the palette-complete opening
world. The held Start remains at greeting216, the revealed/head states project actual frames, and
Map3 is a new session. Continue restores a retained Map3 save; three subsequent fresh starts retain
one playing field music owner plus the pre-existing inactive retreat-cue player. Old session music
pauses during entry and resumes on cancellation. All six entry texture native IDs become invalid
after entry removal. A42s real-time Witch observation crosses the35.4s first PCM end, wraps to its
6.34s loop start and remains playing. Mouse and reduced-flash resource observations pass without
native stderr after the backend has processed release. No game-state/completion injection or
original framebuffer capture is involved.

Preserved discovery: the first observer compared a string map ID with an integer; another counted
only `SessionMusic` names and missed Godot's auto-renamed candidate, then counted all audio nodes
without accounting for the existing inactive retreat player. The actual Save rectangle was469px
wide and intercepted New Game; its shortened label now leaves the long binding in a tooltip.
These are separate observer/product findings, not weakened assertions or original evidence changes.
Immediate-exit observations retain WAV/Playback pair warnings, independently identified by verbose
type output and the [accepted upstream teardown finding](#audio-resource-lifetime-observation).
No fixed sleep, GC or warning filter was added to production. Headless success does not close
human aesthetic approval, physical-controller behavior, natural source navigation, exact cadence,
render/CRAM timing, original DMA tails or sample-exact audio parity.

### Title lifecycle investigation (#1447)

The [investigation-only agreement](../application/opening-session-lifecycle.md#title-lifecycle-investigation-1447)
uses accepted base `a4b787068f1f0c2b05210949bc74df8c07ed7cdd`, unchanged production and the existing
Debug build/private package selected by `local/issue1441/inputs.ps1`. No SDK/build, emulator,
capture, screenshot, recording or asset acquisition is needed. `EntryBackdrop._Process/_Draw`,
`SessionEntryView._Ready/Advance/ShowMenu/ShowTitle`, `EntryPresentationAssets`,
`GameRoot.ShowEntry` and `SessionAudio.Play/ObservePlayback` own the inspected behavior.
The pinned source remains `c834c652b6862bc5679fd7f69a38a7093206efc6`: under
`disasm/data/sound/musicbank0/`, `music28.asm` (`Music_28`, C2 header, nine `channel_end`,
no main loops) and `music10.asm` (`Music_10`, C1 header, eight main loops and one ended channel)
support finite versus repeating score structure. The [special-screen owner](../../../docs/research/presentation/special-screens.md#logo-and-title)
retains original fade/scroll timing Unknowns. These are source facts, not natural original navigation.

Reproduce using the existing native SceneTree observer approach above and a fresh ignored output:

1. Load `local/issue1441/inputs.ps1` in the launch process; keep its current entry/world selections.
   Use fresh local APPDATA/LOCALAPPDATA/TEMP directories, an absolute save root outside all
   checkouts with a fresh disposable namespace, and explicit version1 adjustable40cps
   input settings, and the owned Godot/project/Debug installation. Instantiate ordinary `Main.tscn`
   at1920×1080, with no controlled start. Do not use fixed simulation time to infer audio duration.
2. Read `ReadEntryObservationJson` and actual `AudioStreamPlayer` stream length, loop mode/bounds,
   mix rate/stereo, position, volume, bus/mute and paused state. Record `IsVisibleInTree`, window
   visibility, process-frame count and first `RenderingServer.frame_post_draw`; no pixel capture.
   Sample title at roughly0.25/0.6/1.1/2s and after11s. Send mapped Enter to Witch, Enter reveal/next,
   E/Start to menu, Escape back to title; sample return at1.15s and after another10s, then Enter again.
3. Run one distinct reduced-flash case through title/Witch/return. Inspect the actual projected
   frames during reading/idle. Keep each native process under45s, serial, with a1GiB free-space
   floor and total new outputs below10MiB. A short initial pilot precedes the longer case.

The retained invocation is `pwsh -NoProfile -File local/issue1447/run-native.ps1 -Phase <pilot|normal|reduced> -Run <fresh-name>`
from the owning checkout. The ignored driver extends the existing `local/issue1441/entry.gd`
input/readback access, writes bounded marks rather than per-frame data, and stops only its own
process on the45s watchdog. The recipe above and named production/source owners define the
reproduction independently of the retained private reports.

**Confirmed bounded native observations:** Godot4.7.2 .NET, native OpenGL Compatibility/WASAPI;
pilot2.200s, normal25.682s, reduced6.522s at their final marks. All exit0, no timeout, empty
stderr and no software audio error. Before the first draw the ordinary entry is already visible;
normal first post-draw is216ms after observer start, followed by17/38/68/122 draws at the named
early title samples. There is no observed host visibility delay consuming the entire entrance.
The existing projection does not expose numeral alpha/offset: the0.9s curve and lack of return
reset are **Confirmed structure**, not measured pixels or a claim that a person noticed the fade.

Loaded title WAV:44,100Hz stereo,450,995 frames,10.226643991s, loop disabled. Playback progresses
from0.29896s to2.04916s; at the11s sample it has a natural `finished` receipt, is not playing and
remains on the visible title. Witch WAV:1,561,247 frames,35.402426304s, Forward loop279,419–1,561,247.
Its loop wrap is not re-observed in this shorter investigation; the earlier42s observation above
retains that evidence. Arrival/reading/open-head/mouth and normal idle blink project; reduced-flash
idle samples all project `witch`. Returning requests title at0.04354s and later records a second
natural finish; fresh Enter then starts Witch. The ordered receipts are title start/finish → Witch
start → Witch stop/title start → title finish → Witch start. Entering the Witch menu does not restart
its cue. These three cases have one entry music player at every mark, unpaused/processable,0dB player/Master volume,
Master unmuted, with no competing host-session player in this cold-entry route. Field-session
pause/resume/replacement keeps its earlier observation above; it was not rerun here.

Configuration correction: the initial three runs selected a worktree-local save root, which
`LocalSaveStore` would reject. Their explicit input-settings override bypassed preference storage,
and title/Witch did not construct a save store; retained entry messages contain no storage diagnostic.
That configuration is insufficient for a full ordinary storage-entry claim. The corrected runner
uses an external operator save root and a fresh namespace plus independent `-Run` output selection;
it cannot overwrite the earlier cases. One additional normal observation (27.564s, exit0, empty
stderr) actually opens Continue: two Empty slots, `continue-slots`, no storage error or slot writes.
The panel temporarily disables entry processing and pauses its player via `ApplyHostPause`; Back resumes the same Witch
cue without a new start receipt. Its first draw is219ms, title naturally finishes twice, and the
seven title/Witch receipts match the earlier sequence. The title/audio recommendation is unchanged;
the other unaffected observations are retained rather than rerun.

**Inferred:** ordinary static idle and silence after the finite title score can account for part of
the reported experience; this run supplies no confirmed startup playback defect. **Unknown:**
physical output/audibility, why the user heard silence, perceived entrance visibility, and whether
return replay is desired. No nonzero-sample or software-playing claim establishes human listening.
Recommend retaining production pending independent review. The prior WAV/Playback teardown warnings
remain preserved above and are not erased by these empty-stderr runs. New ignored output stays below
10MiB, with more than2.5GiB free; no general audio audit or user visual/listening approval is implied.

## Witch slot, naming and initial save

The [High lifecycle design](../application/opening-session-lifecycle.md#slot-name-and-initial-save-design-1443)
owns parameters, shared names, compatibility and commit order. Load the same private/tool selections
as title/Witch entry, configure an absolute external `SF2_SAVE_ROOT` and fresh disposable
`SF2_SAVE_NAMESPACE`, then launch the ordinary Main scene. Reuse the existing entry package,
palette-complete world, installation and imports; do not copy assets or use valuable progress.

1. Witch New Game shows both actual slot states. Choose either slot, then type/edit the name or use
   directions and Confirm on the character grid. Keyboard Backspace and grid DEL delete; END/Use name
   opens final confirmation. Supported input is seven ASCII letters/digits or `,.-!?#' ` characters.
   Empty retains the immutable default; spaces remain significant. Unsupported text and overlength
   stay visible with an error and cannot continue. The selected private font maps every allowed
   character to an admitted glyph.
2. Back from final confirmation retains the draft; Back from naming returns to slots. Cancel from
   slots returns to the Witch, whose replacement Cancel resumes the old paused session. Assert
   unchanged identity and slot bytes. Both occupied slots remain selectable; final wording names
   the target and explains backup replacement. No draft or provisional selection writes a file.
3. Final Start/save admits the content, initialized progress, detached initial record and required
   audio before publication. A prepublication storage rejection leaves the paused session and old
   primary/backup intact. Correct the disposable failure and Retry; the candidate/seeds are retained.
   Rejected content can be corrected and retried without replacing the old session. Success writes
   once, makes the old valid primary the backup, mounts one fresh session and disarms held input.
   `save-publication-uncertain` retains the paused session/files and offers Continue inspection, with
   no blind write retry. This exceptional result is an explicit code/store boundary, not a claimed
   deterministic native filesystem-failure observation.
4. Exit, start a distinct process and Continue the initial slot. Its `new-game` recipe retains the
   initialized party and post-growth seeds, then enters actual Map3 setup with fresh-map audio,
   rather than invoking settled-savepoint audio restoration or rerolling growth. Walk the ordinary
   house/Sarah/school/messenger route; observe original program joins, name tokens and flags. Save
   at the joined Map3 anchor, restart again, Continue, inspect Party Status, then follow normal
   castle/palace/Astral/tower inputs into Battle01. Read actual dialogue projection, command/target
   cards, battle messages and name state; IDs/source content remain unchanged. Old version1 records
   lacking `names` retain defaults; malformed present maps/names reject through typed admission.

**Confirmed bounded remake observations:** real keyboard text/Backspace, mapped controller grid/DEL,
Back/Cancel, both occupied slots, empty/default, unsupported/overlength input, held final Confirm,
content rejection, prepublication write failure/retry, exact previous-primary backup bytes, actual
initial Continue and joined-save Continue are exercised. The latter continues through named dialogue,
Party Status and Battle01. This is semantic input/state/projection acceptance, without screenshots,
teleports, terminal flags or injected completion receipts. Original NameAlly natural reach/timing,
hardware rendering/audio parity and human/aesthetic/physical-device approval remain **Unknown**.

Focused behavior is in `NewGameSetupTests`, `OpeningSaveTests` and `OpeningRestoreTests`; run the
selected engine tests after locked .NET/input setup, then `uv run sf2 verify adapter` and the native
steps above. Retained local drivers/results are under `local/issue1443`; they extend the existing
bounded input/navigation helpers. One serial process, existing ~128MB world and at most80 semantic
marks per run suffice; allow10MiB of output and150MB headroom, with a two-minute process bound.
No new export, acquisition or slow aggregate is required. The live-play selection is not the
fixed PlayerReady comparison selection: the legacy seed/first-actor checks require
`remake/reference/inputs/battle01-player-ready.json`, and the single-encounter/malformed-input
checks require the existing smaller single-encounter world. Selecting the live R1 party and expanded
world exposed six failures in a completed engine run; those same six nodes pass with only these
comparison selections corrected. Product startup still never reads comparison starts. The original
completed result and configuration-only targeted correction remain separate evidence.
Preserve initial test equality failures,
the first naming release-barrier finding and the initial-Continue `restore-audio-unavailable`
rejection with their focused corrections. Immediate-exit WAV/Playback warnings remain under the
[existing audio lifetime finding](#audio-resource-lifetime-observation); no production delay/GC/filter.

### Authored names without member-name metadata

Name overrides remain the shared actor-keyed authority. A valid content member name supplies the
default; public authored content without that metadata uses the readable stable actor identity.
Private missing evidence retains `Name unavailable` rather than inventing an identity label. An
admitted custom name wins over either fallback, including absent default metadata.

Reproduce the authored regression with the unchanged `jaro-victory.json` package: launch ordinary
`--authored-package`, acknowledge the invitation, attack the enemy, finish the actual victory/return
program, then open Party Status. The two joined records must show `Leader` and `Ally Jaro` distinctly.
This package does not admit field preparation; its normal Prepare path must still report unsupported.
The same actual party was also rendered through the existing target-list projection in a controlled
read-only UI observation, with no revision change or command submission. A controlled start through
ordinary `GameSession.Start` on the same fixture, supplying only an actor-keyed `Nova` override,
projects `Nova` in both the party and target list. This is a name projection check, not a claim that
the fixture gained field services or that the authored profile exposes the Witch name editor.

The local `NameProjectionProbe.cs`/`name-projection.targets` instrument only that direct projection
and controlled start during a Debug observation; standard Debug/Release builds exclude them. No
production observation API, fixture, golden or helper test is added. The existing actual save/restore
name test also exercises missing default metadata. Preserve the initial native regression and the
fixture's unsupported boundary separately from the passing corrected observations.

## Opening New Game observation

The [High opening lifecycle agreement](../application/opening-session-lifecycle.md#implemented-new-game-boundary)
and [content admission/preparation](../content/profiles-and-trust.md#opening-new-game-admission)
own this product entry. Launch `GODOT_BIN --path remake/game` without a session-entry argument.
After preparing/selecting the private world, entry package, six pinned battle/static inputs and optional
battle scenes, pass the title/Witch entry above and choose New Game. No controlled-start
selection is required. At settled field input, the New Game… button opens a confirmation defaulting
to Cancel. Confirm New Game to choose a real slot, edit the protagonist name and inspect a final
Start/save confirmation. New Game writes only at that final confirmation, using the existing
[Save/Continue store](#opening-save-and-continue).

The historical bounded `local/issue1420/newgame.gd` witness drove actual mapped keyboard/mouse input
through the ordinary Main scene and read native state. It retained selected boundary samples,
without screenshots or a per-frame trajectory, using worktree-local APPDATA/LOCALAPPDATA/TEMP and
output. Its direct pre-Witch title interaction is historical; use the current entry recipe above
before the same underlying game route. It deliberately unset `SF2_PRIVATE_CONTROLLED_START` and
supplied no game entry option. Existing fixture navigation contributed input directions only,
never gameplay state.

Required observations: title → first New Game → ordinary movement → host-button pointer isolation
and Cancel preserving session → invalid replacement content, visible typed error and Cancel retaining
old progress → second independent fresh start with old view released → ordinary house/Sarah/school/
messenger joins → castle/palace/Astral/tower → before/load/start → Battle01 control with ordinary
Confirm/Cancel. Record fresh resources/loadouts/roster/seeds, program/flag boundaries, actual native
errors and process exit. Fixed initial seed unit cases separately admit0 and full uint storage.

The first completed route exposed `full-fade-state` at the house exit because the retained world
had no embedded map palettes; it is preserved. The product now rejects such input during startup,
and the narrow helper preparation supplies admitted palettes before rerunning the affected route.
The initial observer's missing optional method argument and later unused palette/guard/text tracking
are driver failures, retained separately from product failure. No original fixture is weakened to
satisfy this product path. Exact candidate results, CI and process ownership belong in the PR handoff;
independent integration remains main-gate's responsibility.

The completed private witness reaches Battle01 `Movement`, opens `ActionChoice` and cancels back
through actual input with no native stderr. It also observes ordinary mapped Wait when a fresh
seed places an ambient NPC in the walking path; no saved NPC phase is supplied. The retained
`native-07` and `native-final-02` runs preserve those bounded results. Source-origin labeling is
explicit scenario assembly metadata, independent of trusted ally metadata presence. The small
`entry-controls.gd` cases separately cover missing product input/no practice fallback and retained
explicit authored/controlled startup. Fixed-seed engine cases and the affected private scenario
comparison pass; earlier private failures and their current-assertion corrections remain in the
handoff rather than changing original fixtures.

The final small entry controls pass for missing product content, explicit product origin, authored
battle control and explicit controlled exploration origin. A separate standalone
`--private-battle-start` with the selected scene sidecar retains
`battlefield-map-resource-unavailable`: it has no exploration map material to mount. That existing
standalone presentation boundary is preserved, not treated as a product pass or repaired by
fallback. The connected New Game route supplies the map material and reaches actual Battle01.

## Opening Save and Continue

Stable battle Movement uses the same slots through the separate
[battle Save/Continue verification owner](battle-save.md). Field recipes below retain their own
safe-anchor semantics; a battle record restores the saved actor/queue position instead.

The [High-rigor lifecycle owner](../application/opening-session-lifecycle.md#implemented-savepoint-boundary)
owns the field recipes, persistent state and exclusions. These are remake savepoints, not original
SRAM or exact-position suspension. Load the current ignored private/tool configuration in the same
PowerShell process as each launch, select the existing palette-complete New Game world and six
battle/static inputs, and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the owned installation
and project. Real process exit/relaunch is required here because restart is the behavior under test.

Choose an absolute H-local persistent root outside every checkout and a fresh disposable namespace
for acceptance. For example, with operator-selected locations:

```powershell
$env:SF2_SAVE_ROOT = 'H:\sf2-player-saves' # Explicit persistent root, never a worktree.
$env:SF2_SAVE_NAMESPACE = 'opening-acceptance' # Fresh namespace for a new independent run.
Remove-Item Env:SF2_PRIVATE_CONTROLLED_START -ErrorAction SilentlyContinue
& $env:GODOT_BIN --path remake/game
```

No default root or fallback is inferred. Records live under
`<root>/<namespace>/sf2-opening/slot1.json` and `slot2.json`, with one `.bak` and a writer `.lock`
per slot. Private state stays local. New Game reads both slot states and writes its detached initial
journey only after explicit final confirmation; occupied replacement retains the previous valid
primary as backup. Ordinary Save retains its stable-boundary rules. At settled field
input, click Save… or use the mapped Stay action (default Space / Pad LB); the panel describes the
restore destination. Select a slot, inspect the explicit save/overwrite confirmation (Cancel is
selected initially), then confirm Save. Success follows atomic publication and readback. Quit is
available by button, OS close or the save panel and warns about progress since the last save/load.

Native state observation uses `ReadPersistenceObservationJson` for session identity/revision,
availability, detached progress and bounded entity-work diagnostics; `SaveSlotView.ReadSaveObservationJson`
for panel state/message/choices; and the existing field/battle capture APIs with full paths disabled.
Inspect errors and logical/native music. No screenshot, checkpoint injection, teleport, fixed seed,
per-frame export or asset recopy establishes this boundary. A UI witness must assert the confirmation
panel **before** Cancel/Confirm, then its result; click actual control rectangle centers, not a guessed
edge coordinate. Otherwise a missed Quit click can falsely appear to test cancellation.

Reproduce the finite product route using ordinary input:

1. Title → New Game → ordinary Sarah/Chester joins and messenger → Map3 Save slot1. Existing
   `map3-battle01-natural-route-v1.json` supplies navigation inputs only. Fresh ambient walkers may
   require a deliberate mapped Wait and legal step retry; their checkpoint state is never installed.
2. Exit the process and launch again with the same selected content/root/namespace. Title Continue
   slot1 restores `(32,13)`, Up. Leave the church through the south door: Down4, Right3, Up8,
   Left2, Up3, Left2, Up reaches the established castle gate route. Continue actual castle/palace/
   Astral/tower input and Battle01. Existing `map3-castle-battle-unlock-static-v1.json` and
   `map3-battle01-player-ready-v1.json` supply input plans, not session values. Battle healing requires
   opening Magic, confirming the spell, choosing the target and confirming the action.
3. Win the actual battle, finish its program and Save slot2 at Map57. Exit/relaunch/Continue slot2,
   verify the dedicated outcome anchor and preserved party, then move in a passable direction.
   The anchor is the actual completed outcome landing, not a fixed coordinate from a walkthrough.
4. Separately Continue slot1, play an actual ordinary leader defeat, finish the Map3 return and Save
   slot2. Exit/relaunch/Continue slot2 and retry the tower battle. Repeat palace entry can stop at
   `(23,37)` instead of running the first-visit walk: step Down then Up to leave that warp. Confirm
   and cancel a real battle action after re-entry. This is not retreat or a terminal-state injection.
5. In disposable UI slots, test controller/mouse Save, initial/overwrite confirmation and cancel,
   success, unsaved Quit/Cancel, empty Continue, and explicit backup recovery with its own cancel.
   Corrupt only the designated disposable primary after a valid overwrite has created a backup;
   preserve its bytes and confirm that recovery does not replace the good backup with corrupt data.

The bounded native witness covers all four route stages above. New process identities differ.
Every captured persistent value agrees after victory and defeat loads; the joined Map3 load differs
only by clearing its ordinary map-entry flags256/260/261. Music starts fresh with saved logical
cue/history. Actual movement, battle commands and return prove usability beyond DTO equality.
Controller Save, overwrite/cancel, actual Quit confirmation/cancel, empty slot and explicit backup
recovery have separate native observations. All successful witness processes exit with empty stderr.
Human/aesthetic approval and device/power-loss survival remain Unknown. Exact run outputs and failed
driver attempts stay in the Issue/PR handoff; they do not become original-game evidence.

### Engine and storage controls

`OpeningSaveTests` / `OpeningRestoreTests` exercise actual capture, reconstruction and continuation:
three recipes, intermediate flags, varied victory anchors, live/dead members, retained resources and
full words, changed valid loadout-derived attack, no on-load/growth/reward/heal replay, unchanged RNG,
independent repeat loads, stale revisions, pending story/presentation rejection and ambient restart.
Run those plus affected `FieldPlayerReturnTests` and existing Battle02 consumers against the current
product world. For the product-only private tests, select the palette-complete world explicitly;
for source AI/player-ready comparisons, select the corresponding `battle01-player-ready.json` input.

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore `
  --filter 'FullyQualifiedName~OpeningSaveTests|FullyQualifiedName~OpeningRestoreTests|FullyQualifiedName~FieldPlayerReturnTests|FullyQualifiedName~Battle02'
uv run sf2 verify adapter
```

The ordinary `uv run sf2 verify engine` gate runs all currently selected optional private cases too.
A broad legacy input selection is not the public/no-private profile. The completed local selected
run retains nine historical controlled opening cases failing `full-fade-state`. The
[controlled-start display audit](../evidence/controlled-start-display-audit.md) maps every node to
its actual input and first-warp preflight: the R1 checkpoint omits display, and two Map21 helper
constructors also omit it. The historical world separately lacks map palettes; selecting the
palette-complete product world still leaves these diagnostic displays null. Ordinary New Game and
Continue explicitly construct display state, and accepted transfers/battle returns retain it.
No product correction or test retirement is established by these pre-battle failures. The
[corrected diagnostic inputs](../evidence/controlled-start-display-audit.md#corrected-initialization-and-nine-node-results)
now explicitly initialize R1 and a separate Map21 bridge, retain their displays in both constructors,
and use the existing palette-complete world only through explicit command selection. No embedded/start
palette duplication or global selection change is made. Map21 period/restored visibility are labelled
remake-only controls; original display values at that seam remain Unknown. The nine actual corrected-input
baseline was **3 pass/6 downstream fail/0 skip**. The
[downstream resolution](../evidence/controlled-start-display-audit.md#downstream-resolution) now runs
**9 pass/0 fail/0 skip** with actual typed text receipts, encounter-scoped diagnostic bindings and
ordered fade lifecycle checks. The R2a Active `[0,1]` assertion remains unchanged: field-control
return preserves the counted lists while independently deriving gameplay ActiveAllies from flags.
All six former failure nodes and the three dependency regressions pass; this is bounded engine
execution, not a new native/original-continuity or original-display observation. Exact commands,
preserved failures and scope are in the audit.
Retain the completed 1692 pass/20 fail/1 skip aggregate and these failures separately from
passing product/restart checks and exact-head public CI. Do not weaken comparisons or rerun the
entire aggregate merely to replace the recorded result.

Exercise `OpeningSaveJson.Read/Write` and the compiled `LocalSaveStore.Read/Write/Recover` directly,
without adding tests of a verification harness. For a local PowerShell control, load the Debug
Domain/Application/Content/Godot assemblies, create an `OpeningRestoreReader` from the same selected
inputs, and retain its accepted immutable definition. The store is internal; obtain its type from
the loaded `Sf2.Remake.Godot` assembly and invoke its constructor/methods with
`BindingFlags.Instance | BindingFlags.NonPublic`. Supply root, fresh namespace, absolute checkout
and `Func<OpeningSaveData,string>` validation that calls `GameSession.Restore`. A successful
PowerShell string delegate must return `[NullString]::Value`, since `$null` converts to an empty
string and incorrectly reports an error. Read progress from a genuine disposable native save.

Direct controls cover first save, overwrite sequences, byte-preserving writer lock/read-only/held
destination failures, ignored interrupted first-save temp, backup-only and corrupt-primary recovery,
invalid backups and incompatible primary without rollback. Hold `.lock` with `FileShare.None` to
exercise competing writers; hold the primary with `FileShare.Read` to deny atomic replacement.
Restore attributes/handles in `finally`. Compare primary/backup bytes before/after failure and
backup bytes before/after recovery. Never fault-inject into retained player slots.

For format controls, decode the envelope's base64 payload. Test truncation, size over1 MiB, duplicate
keys at both levels, unknown/missing/null fields, wrong digest, unknown format, out-of-range gold,
incompatible gameplay revision and missing music resource. To reach semantic admission, encode the
modified UTF-8 payload and recompute its SHA-256 with the production envelope spelling. Otherwise
the expected result is the earlier checksum rejection. A read never repairs a file. Same-volume
replace/readback success establishes only the tested filesystem behavior, not all physical crash cases.

## Opening field preparation

The [Standard #1422 boundary](../application/opening-session-lifecycle.md#implemented-field-preparation)
adds field transactions to the existing party and opening/save world. Original source policy remains
separate from the following **Confirmed remake observations**; no original field timing/reach or
all-device/human acceptance is claimed.

Load `local/issue1045/inputs.ps1`, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and select the existing
palette-and-Search-complete world as `SF2_PRIVATE_EXPLORATION_CONTENT`; use the
[current metadata supplement](../content/profiles-and-trust.md#opening-new-game-admission).
A palette-only historical world rejects before product session publication.
Use existing absolute SDK/Godot selections and the same project/imports. No new export, asset copy,
comparison checkpoint or screenshot is needed. Build the actual Debug host and run affected engine
behavior directly:

```powershell
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~FieldInventoryTests|FullyQualifiedName~FieldRecoveryTests|FullyQualifiedName~BattleEquipmentTests|FullyQualifiedName~BattleGiveTests|FullyQualifiedName~BattleDiscardTests|FullyQualifiedName~OpeningSaveTests|FullyQualifiedName~OpeningRestoreTests|FullyQualifiedName~PartyDetailsQueryTests'
```

The targeted selection passes130 cases with the correct product input, including detached private
New Game save/restore. Query/cancel/stale/unsupported cases, duplicate/full/empty slots, class and
Unarmed choices, both sides of exchange, dead inventory access, self-exchange, rare/unsellable Drop,
full-HP/dead status-cure targets, low/zero MP, payment on no effect and EXP/RNG preservation are covered.
Meaningful poison cure is engine-tested; no naturally acquired Antidote/DETOX native witness is claimed.

Native runs use the actual mapped input and `ReadCaptureState`/`ReadPersistenceObservationJson` on a
1920×1080 window. Party Status → Prepare is available through Confirm and the visible Prepare control;
E / Pad Start opens the member screen. Directional input selects, Confirm enters an action or commits,
Cancel returns without a command, and pointer press/release uses the actual observed row rectangles.
The panel's preparation observation includes stage, selected index, captured revision, visible title
and row rectangles. Inspect authoritative `party` values rather than treating provisional UI as state.

Reproduce with a fresh owned namespace under the explicitly configured save root outside worktrees:

1. New Game; open preparation with the controller and pointer. Preview Unarmed, Cancel and compare
   party/revision/seeds; then confirm Unarmed. Use Medical Herb through the pointer and assert the
   no-effect result with unchanged party/seeds. Close normally.
2. Follow ordinary legal opening inputs through Sarah/Chester joins and the messenger. Sarah's field
   HEAL reports no effect without payment. Give Chester's herb to Sarah to fill her pack; Give Bowie's
   herb with an explicit return of Sarah's Wooden Rod. Cancel Drop, verify unchanged state, then confirm
   dropping that rod. Close and explicitly Save slot1 at an eligible settled field boundary.
3. Exit and start a fresh OS process. Title Continue → slot1 restores all four retained ally records,
   gold/custody and seeds exactly. The session identity changes. Follow normal castle/palace/Astral/tower
   movement/programs into Battle01. Its three joined allies consume the saved full item/spell words,
   HP/MP/EXP/status/progression. Unarmed Bowie and Sarah use their saved base ATT; this is actual battle
   entry, not a constructed field-only example. No outcome/reward is replayed by Continue.

Retained executions are `local/issue1422/run-native.ps1` with `field-native.gd`: `smoke-01` (1,655ms),
`preparation-02` (36,618ms) and `continue-battle-01` (37,506ms), all exit0, passed and empty stderr.
`compare-native.py`/`continuity-01.json` compare saved/restored party and actual battle consumers.
Use fresh `-Run` and `-Namespace` values when reproducing; the latter two runs share one disposable
namespace. The driver reuses #1421's bounded input/navigation functions and records selected boundaries,
not per-frame acquisition. One native process at a time, existing ~126MB world, at most80 marks/run,
less than10MiB evidence and a two-minute run bound are sufficient. No slot/output cleanup is implied.

Preserved completed discovery: `preparation-01` exited2 because its driver matched display names with
wrong case and assumed a second Bowie herb; corrected inputs use actual labels/starting inventories.
No source state was injected or acceptance condition weakened. Initial compile/fixture failures and
their focused passing corrections remain in the Issue's local handoff. The completed #1421 broad
private aggregate (1692 pass/20 fail/1 skip) and nine remaining controlled-start full-fade-state failures
remain preserved; this slice does not rerun that aggregate or turn those diagnostics into passes.

## Finite gameplay feedback observation (#1454)

The [Standard presentation agreement](../godot/presentation.md#gameplay-feedback-and-recovery-wording-1454)
limits this check to actionable movement refusals, Church effect wording and transient refusal/focus.
Load the current ignored selections in the same launch process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, reuse the installed SDK/Godot, project/imports and short
inputs/observer helpers. Adapter and necessary Debug builds plus the existing `FieldServiceTests`
behavior group suffice locally; inspect the clean committed dependency plan and actual CI separately.
Do not add string/helper tests or replay the full opening/defeat route merely for a message change.

Use fresh `local/issue1454` outputs, serial native processes <=45 seconds, <=50 MiB cumulative output
and a 1 GiB free-space replan floor. Start with one representative short pilot before siblings.
Observe only named state/input/control boundaries, actual geometry and errors; no screenshot,
per-frame capture, new acquisition or mutation of canonical inputs.

1. Reuse the authored movement corridor: preview the occupied ally tile, Confirm and retain the
   typed occupied refusal, revision, actors/resources and both seeds. Move to a legal tile, request
   blocked movement, retain the range refusal and actionable visible message. Follow with legal
   action/commit and observe resumed play. Retain actual reason codes independently of player text.
2. Reuse ordinary New Game and the real Map3 counter/priest helpers from #1424/#1453. Focus Promotion
   with keyboard/pad, refuse it and compare revision/party/gold/seeds. Read the actual Hint Label:
   refusal plus mapped Choose/Select/Decline/Back must fit. Change focus and verify stale error
   clears; repeat pointer selection and normal service follow-up/return. Unavailable marker/color
   and all release guards retain the accepted shared behavior.
3. For paid treatment, use a short explicit authored initial-state variation of the existing harbor
   field package: a dead ally, exact affordable Raise and its ordinary Church instruction/interaction.
   Observe offer/decline or confirmation, one real payment/recovery, Continue and legal field input.
   This is authored presentation evidence, not natural original or product-opening reach. Existing
   FieldService behavior assertions cover poison-before-stun, masks, current cost, insufficient/stale/
   duplicate commands and preserved RNG; direct typed offer/outcome mapping covers removal wording.

The reproducible local entry is `local/issue1454/run-native.ps1 -Case <battle|field|treatment>
-Run <fresh-name>`, using the existing menu/Church observers. Poison/stun natural native acquisition,
human readability, physical devices, actual display/DPI and original media/timing remain **Unknown**.
Earlier controlled-town duplicate-palette/preparation failures and all prior accepted/failing records
remain preserved; those failed starts are not a route for this check. #1447/#1463 remain deferred.

**Confirmed:** remake observations. `pilot-battle-02` passes in 615 ms: both typed refusals keep
revision/resources/seeds, actionable text fits the actual 1080p viewport, and legal Stay reaches the
next actor. `field-01` passes in 7,221 ms: ordinary New Game/real Church, key/pad/pointer Promotion
refusal with unchanged party/gold/seeds/revision, muted marker, actual three-line Hint text measured
inside its rectangle, cleared focus error, no-target Raise and fresh field input. `treatment-02`
passes in 467 ms: the explicit authored Raise offer spends 10→0 gold exactly once, HP 0→100 with other
party data and both seeds retained, then Continue/Leave and a legal field step. All three processes
exit0 with clean native error logs. These are finite engineering observations, not human approval.

Retained observer failures: `pilot-battle-01` assumed an object for the existing string failure-code
readback and reached its observer deadline after a script error; the corrected pilot reads the real
code. `treatment-01` completed with `authored-legal-step:timeout`: inherited product navigation only
recognizes `entity-0`, whereas this authored package uses `traveler`. The corrected short case uses
bounded ticks and actual settled-input readback, without changing product code/content rules or
replaying the opening. Both failed receipts/logs remain local; no suite was interrupted or rerun.

## Opening Church services

The [#1424 agreement](../application/opening-session-lifecycle.md#implemented-opening-church) owns
service legality and exclusions. [Caller inventory](../content/opening-branch-inventory.md#opening-service-callers)
keeps excluded F609 shops separate from the admitted default/F501 priest. Reproduce the direct
source boundary with `uv run sf2 h2 common-menus --upstream-path <pinned-source> --output-path <ignored-output>`;
that source selection needs its matching retained H1 listing. The pinned read-only retained source/
listing may be selected instead of rebuilding or copying it. Missing default selection/listing is
not a ROM or gameplay failure. No original H3 observation is added by this slice.

After same-process environment setup and the [documented metadata supplement](../content/profiles-and-trust.md#opening-new-game-admission),
select the supplemented world for `SF2_PRIVATE_EXPLORATION_CONTENT`. Run `uv run sf2 verify engine`
with optional private assertions unselected, `uv run sf2 verify adapter`, and the affected private
behavior group directly with that world. `FieldServiceTests` covers source-order offers, changed
current progress/content, exact/insufficient funds, decline/retry, dead/status combinations,
missing accounting, stale/duplicate confirmation and attributed real-caller failure. It does not
test a verifier, UI helper or reference route. Preserve completed failures and use focused corrections.

The retained native driver is `local/issue1424/run-native.ps1 -Run <fresh-output> -EntryMode <phase>
-Namespace <disposable-name>`, with `church-native.gd` extending existing #1423/#1422 mapped-input
navigation. Outputs stay under the owning worktree; saves use the configured external disposable
namespace. One existing Godot project/installation, serial processes and no asset exports suffice.
Expected input/output is one125.8MB world derivative plus compact semantic reports below10MiB;
allow at least150MB output headroom and replan below1GB free. A run is bounded to two minutes;
no per-frame capture, screenshot, teleport or later-state injection is used.

- `pilot` uses ordinary New Game, actual counter/priest entry, no-applicable Raise/Cure, keyboard,
  controller and mouse selection, caller return, service Save, slot confirmation and actual Quit.
- `raise` uses ordinary New Game/joins and actual Battle01 commands. It earns a real enemy reward,
  lets Sarah and then the leader fall, and runs ordinary defeat return. The priest offers Sarah's
  level1 Raise at10 gold. Decline retains30 gold/HP0; retry confirms30→20 gold and HP0→11. Cure has
  no applicable target. Cancel returns playable field; service Save waits for caller cleanup, then
  writes the existing joined Map3 savepoint and quits through the normal confirmation.
- `continue` launches a distinct process against that same disposable namespace. The entire detached
  saved progress record matches, including party/progress/items/status, gold, receipts/custody, flags,
  both RNG channels and save recipe/music. A legal resumed step works; reopening Raise has no target
  and takes no payment. A held click interrupted by view hide and Confirm during pause cannot submit
  after resumption. A fresh mapped action still works, then actual Quit closes the process.

The accepted local runs (`pilot-03`, `raise-02`, `continue-guard`) have empty stderr. The source-world
supplement was exercised from the accepted #1423 world, including removal of stale unavailable keys.
Positive naturally arising poison/stun is **Unknown** for the opening; its cost/status behavior is
covered by changed-state engine assertions. Original admission/return timing, service music/window
cadence, exhausted-sprite parity, empty-shop-chest access and later campaign remain **Unknown** or
outside admission. This does not repeat #1423's already accepted victory/Search/restart route or
close #1425. Initial discovery/build/driver failures and inherited controlled-start fade diagnostics
remain in the Issue's local handoff; no aggregate rerun replaces them.

## Localization pilot

After loading this worktree's `local/private-inputs.ps1`, reuse the selected Godot installation and
project. The first checkout/import of the new PO/font resources needs the ordinary editor import;
changed C# needs the existing Debug build, with absolute `DOTNET_BIN`, shared `DOTNET_CLI_HOME` and
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Keep native APPDATA/LOCALAPPDATA/TEMP and logs in the owned
ignored runtime directory and reuse its caches. Configure `SF2_SAVE_ROOT` outside all checkouts and a
unique `SF2_SAVE_NAMESPACE`; language belongs to `sf2-preferences/input.json`, not game slots.

```powershell
$env:SF2_DISPLAY_CATALOG = (Resolve-Path -LiteralPath 'remake/content/authored/localization-pilot.zh-CN.json').Path
& $env:GODOT_BIN --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/localization-pilot.json').Path
```

The real field program shows the captured Traveler name with W1/CLEAR/N/D1/W2, then returns control.
At settled field input press Escape for Menu, then choose Settings. The fifth row is Language; Confirm changes only
its draft endonym. Apply (seventh row) stores/publishes it; Cancel retains the old preference. Talk
repeats the authored dialogue in the selected language. The Witch menu Settings entry also
refreshes. Relaunch the same namespace to reload the preference. For a dynamic battle sample use
`healing-rule-demo.json` through the same `--authored-package` route: Confirm opens the diamond, H
opens Magic, Confirm selects its spell/self target and another Confirm commits. Result text uses
captured name/HP amount. Source work, resources and seeds do not depend on display length/reveal.

`remake/game/probes/localization_observation.gd` drives these actual consumers using ordinary keys.
Set `SF2_LOCALIZATION_OUTPUT` to an explicit ignored JSON destination. Default case covers English,
draft Cancel, Chinese Apply, same session/revision and repeat dialogue; `SF2_LOCALIZATION_CASE=heal`
uses a battle package with Chinese settings, `heal-en` expects English, `heal-slow` expects adjustable
Chinese. Cases `field-slow`, `field-instant` and `restart` observe speech/acknowledgements and loaded
preference. `SF2_LOCALIZATION_LONG=1` additionally requires ReadMore and measured glyph/geometry
checks; select a separately authored long overlay with the same source controls.

Direct native failure cases `ui-fallback`, `required-ui`, `required-font`, `write-failure`, `read-only`
exercise loaded resource mutations before composition/Apply or actual settings access; mutations are
process-local and never edit PO/font files. Read-only requires an explicit `--input-settings` file;
write-failure uses `SF2_LOCALIZATION_PREFS` only as the observer's external conflict destination.
Required resources retain distinct compiled English recovery controls and refuse Apply. Exact row
metadata distinguishes Chinese missing/invalid templates and retains English fallback. Existing
specific storage recovery remains visible; no English-message parsing supplies error semantics.

Bounded Content admission is exercised directly, not by tests of its reader: valid reordered names,
partial/malformed/duplicate/wrong-identity/oversized overlays, invalid parameter/control rows, literal
arguments, missing producer bindings and `A{CLEAR}B{W1}C{W2}`. Both English routes project B then BC;
Chinese projects its corresponding segments, with exactly two source acknowledgements. Behavior
unit tests live in `LocalizationDisplayTests`; affected existing FieldText/scene tests retain their
real Application assertions. Use `uv run sf2 verify engine`, `uv run sf2 verify adapter`, the committed
planner selection and actual CI; completed failures and targeted corrections belong in the handoff.
No screenshots, per-frame capture, emulator, world export or full-opening replay is needed. Actual
observations establish this finite engineering boundary, not translation/typography approval or
whole-epic coverage. Private overlays remain private; Chinese naming/IME remains unsupported.


## Localized Church and Search

Use the [localization pilot launch](#localization-pilot), selected installed Godot, current Debug
build, existing imported font and worktree runtime. The original pilot keeps its opening dialogue.
After it returns, face south at the starting cell and Confirm to Talk to the practice priest; Raise
then Cure exercise one payment at a time, poison before stun, declining, no-applicable and the
independent unavailable curse notice. Promote has a readable refusal. Save closes the caller before
the existing persistence flow (the public sample has no opening-save policy). The chest is at (3,1):
walk north to (1,0), east twice to (3,0), then face south against the blocked chest and Confirm. Dismiss and repeat to see
empty; delivery and visual redraw cannot grant again. Default names/item labels remain untranslated.

For short actual host observations, prepare finite variants **before launch** by changing a copy of
`localization-pilot.json` under the owning ignored report directory. Keep its original art, package
identity, programs, catalog and input route; remove `start.program` for settled input. Church starts
at (1,1), facing 3. Search starts at (2,1), facing 0, with the diagnostic NPC entities/events removed.
Set `SF2_LOCALIZATION_CASE` and `SF2_LOCALIZATION_OUTPUT`, then launch the ordinary authored host with
`--script res://probes/localization_observation.gd -- --authored-package <absolute-variant>`.
Use existing version1 English/zh-CN preferences through `--input-settings` except `search-switch`,
which needs a fresh writable external preferences namespace and no read-only override.

| Case | Pre-start authored difference / actual observation |
| --- | --- |
| `service-en`, `service-zh` | Base party/gold; English/Chinese Raise, Cure, decline, no-applicable, curse, Promote, Save close, keyboard and pointer press/release. |
| `service-low` | Gold 9; second ally status `poison-stun`. Insufficient Cure skips the remaining same-family members and retains resources. |
| `service-fallback` | Base Church variant, Chinese preferences. Observer removes loaded Chinese paid/refusal rows and supplies a longer original English refusal before composition; complete fallback/status and measured footer growth remain operable. |
| `search-ordinary` | Unmodified pilot including opening dialogue; ordinary north/east/east/south movement reaches the blocked chest, followed by acquisition and empty repeat. |
| `search-en`, `search-zh` | Chest item 0, space available; typed single-revision grant/flag/open block then empty repeat. |
| `search-full`, `search-invalid` | Each actor's four authored item slots holds 0; full refusal retains party, flag and closed chest. `search-invalid` changes only the loaded Chinese full template to invalid arguments before composition and observes whole English fallback. |
| `search-field-only` | Chest item 113 (`Practice Token`), defined only in the admitted field catalog. Real Search and Party's read-only projection retain the label; no battle Discard or field Use is enabled. |
| `search-gold` | Chest itemWord null, gold 25; reward and receipt then empty repeat. |
| `search-description` | Search starts at (1,1), facing 1; admitted description IDs 425/900 and the literal source-name binding. |
| `search-fallback` | Base Search, selected overlay omits row 403. Original source row remains English with visible fallback. |
| `search-switch` | Base Search and fresh English preferences; acquire, dismiss, ordinary Settings → Language → Apply, then Chinese repeat without another reward/revision change from Apply. |

Connected authored `battleDiscard.items` is a field-only catalog using existing complete property and
loadout validation. Same-ID label conflicts reject. Battle Discard stays null; no field effect/spell
is guessed. Finite connected ally status strings are `none`, `stun`, `poison`, `poison-stun`, `curse`,
`stun-curse`, `poison-curse`, `poison-stun-curse`. Their carry does not enable unsupported battle effects;
standalone statuses and the explicit `battlePoison` restriction retain their existing admission.

The native observer records state/choices, actual glyphs, measured title/hint bounds and input/resource
outcomes, with a short external process deadline. The selected engine/adapter checks and clean
committed planner/actual CI remain required; no screenshots, private story export, full opening or
new validation framework is needed. Preserve initial startup/driver failures and narrow corrections
in the slice handoff. These observations establish the finite implementation boundary; human
translation/typography and opening-wide bilingual coverage are not claimed.

## Localized entry and save

[#1474](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1474) owns this bounded screen
group under the [localization agreement](../application/localization-design.md#implemented-m2b-entry-and-save-boundary).
Reuse the admitted entry/opening selections from [entry](#opening-new-game-observation) and
[save/Continue](#opening-save-and-continue), the owned Godot installation/project/imports and local
private-input configuration. Use disposable external save/preference namespaces, serial short runs
at 1920×1080, a 50-second external deadline and no screenshots. New evidence budget is 15 MiB;
do not copy/rebuild the world or reset old failure records. Necessary changed C# builds and PO imports
use the existing absolute SDK/shared CLI home with `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`.

The retained local entry is `local/issue1474/run-native.ps1 -Run <case> [-Namespace <existing>]`;
it extends the existing entry/save input driver rather than introducing another probe framework.
`catalog.ps1` directly exercises the real Content reader; neither driver nor reader gains unit tests.
From the owned checkout, load the current private selections in-process, build the existing Debug
adapter only for changed C#, and use `-Run po-import -Import` only when native PO resources changed.
Choose fresh attempt/namespace names for new writes. Record actual native/SDK exits and semantic
results; a process exit alone does not establish a successful observation.

Reproduce the normal route with these actual host actions in each language:

1. Start at the title and advance through greeting rows 216–220 using reveal/ReadMore. At Witch
   menu row 221, open Settings, change the language draft, then Cancel. Confirm the same menu,
   texture identities and audio start receipts. Without an overlay, Chinese UI retains the original private
   English rows with `en` / `overlay-unavailable`, visibly separate from UI labels.
2. On the settled menu Apply English↔zh-CN. Keep page, selection, source/failure identity, textures
   and audio receipts; do not restart music. New Game → slot → naming must reject eight characters
   and Han input, allow empty/default, and retain a literal seven-or-fewer ASCII draft through Back.
   Confirm a literal `Bowie` remains an override, not an instruction to translate a default name.
3. Final confirmation defaults to Back and writes nothing. Prepare a disposable *file* as an invalid
   save-root destination before startup, attempt the real write, inspect the localized failure, then
   retry with the valid root. The same prepared candidate must be written before mounting. No live
   session/party/result injection is permitted. Cancel and failed writes preserve prior progress.
4. At ordinary settled opening field input Apply another language; compare session ID/revision,
   complete save capture and slot bytes. Existing New Game/Save/Quit buttons refresh in place.
   Save → occupied slot → Cancel must keep bytes; confirm Save retains the previous valid backup.
   Reopen New Game and cancel the occupied-slot path, retaining the paused session. Quit through
   the real confirmation. Start a new OS process with the same namespace, Continue that slot and
   perform a legal field step. Compare saved/restored party, names, flags, resources and both seeds.
5. In another disposable namespace prepare a truncated primary and a compatible genuine backup.
   Continue shows recovery explicitly; Cancel leaves both files unchanged. Confirm recovery, verify
   the restored name/progress and normal control. UTC remains explicit on occupied/recovery rows.

Optional entry display selection is an absolute `SF2_ENTRY_DISPLAY_CATALOG`, independent of the
world's `SF2_DISPLAY_CATALOG`. This complete original sample translates no extracted script:

```json
{
  "package": "private-local-entry",
  "locale": "zh-CN",
  "source": {
    "repository": "ShiningForceCentral/SF2DISASM",
    "commit": "c834c652b6862bc5679fd7f69a38a7093206efc6",
    "romSha256": "9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9"
  },
  "texts": [{"namespace": "entry.text", "id": 216, "text": "原创入口，欢迎开始新的旅程。"}]
}
```

This binds to the actual admitted entry profile, with IDs 217–221 explicitly missing. Run direct
missing-row, unexpected `{W1}` / `{NAME}`, duplicate row, wrong namespace/package/source and one-bad-row
controls. A semantic row defect falls back only that row; wrong identity/structural input rejects
the overlay. Retain effective locale/reason/diagnostics without recording private paths or prose in
public reports. `overlay-valid`, `overlay-invalid` and `overlay-source` use normal host entry input.

For `long-overlay`, give all six real IDs finite original Chinese prose, repeating one short sentence
about 30 times. Read to 221. A pointer Continue and keyboard Confirm must read first, without opening
slots or changing page. The visible Read more control finishes the body; then the original command
works. Apply English after a partial scroll, then Chinese: changed bodies reset scroll/read-through,
with the menu page/selection/audio retained. A long original resource-error explanation exercises
the same reading/reset boundary. Resource-error and actual failed-start menus must refresh their
localized explanation while retaining the original asset error or failure/code/field/retry values.

With a valid short entry overlay, erase Chinese `entry.new` and give `save.offer` an invalid
parameter before startup. Entry and Save show their complete English fallback and reason. In the
name editor, enter an invalid character to see the validation message; a valid draft clears it.
Inspect loaded glyphs, control/panel viewport bounds, allocated text height and native errors.
Scrollable dialogue content may extend beyond its clipped reading viewport; its viewport and footer
must fit. No source-control acknowledgement, session command, seed or write implements reading.

Run affected engine/adapter gates and the clean committed planner; record actual CI and preserved
startup/driver/geometry discoveries in the handoff. Missing private translation, title/numeral image
lettering, other menus, Unicode names/IME, export and human bilingual typography approval remain
outside this result. Do not close the localization epic from this screen-group acceptance.

## Private Witch-entry translation pilot

[#1486](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1486) owns the finite
[M3 suggestion/review boundary](../application/localization-design.md#m3-private-witch-entry-review-sample),
exactly `private-local-entry` / `entry.text/216–221`. Keep all original and translated dialogue,
bilingual review and absolute private paths local. The persistent external content owner has
`entry.zh-CN.json`, `review.zh-CN.md` and `README.md`; its data survives worktree retirement. The
private README and ignored `local/issue1486/HANDOFF.md` name its location and read-only source identity.
Neither a complete entry package nor a new content repository is created.

From the assigned existing checkout, load its current private-input/entry selection in the same
PowerShell process. Select the optional overlay with an absolute `SF2_ENTRY_DISPLAY_CATALOG` and use
ordinary `zh-CN` preferences/settings; `SF2_DISPLAY_CATALOG` still selects the separate world overlay.
Reuse the existing adapter, imports, Godot installation and owned runtime directories. The finite
local entry points are:

```powershell
pwsh -NoProfile -File ./local/issue1486/catalog.ps1
pwsh -NoProfile -File ./local/issue1486/run-native.ps1 -Run <fresh-attempt>
```

The reader check uses the existing compiled Content assemblies without an SDK launch. It validates
source package/repository/commit/ROM identity, exact six IDs, resolved text/effective locale/reason,
empty diagnostics and literal newline counts `1,2,1,1,1,0`. These are literal newlines, not added
`{N}` controls. Source size/mtime remain unchanged. The native launcher uses a fresh disposable
external preference namespace for this Issue, existing runtime directories, headless 1920×1080,
a 50-second outer deadline, no focus notification injection and no screenshots. Budget: private
three-file text below 16 KiB, new local reports below 2 MiB, H free space above 1 GiB. Replan before
exceeding those limits; no broader acquisition, world copy, environment/cache or new probe framework.

The ordinary input route starts at title, confirms each row to menu 221 with alternating pointer and
keyboard continuation, Applies English, returns to title and reads the same six English rows, then
Applies Chinese again. Keep only the six paired key windows and necessary menu/input/process state.
For every row compare the actual displayed body with the real reader result/source, require the
expected locale with no fallback, inspect visible control glyph coverage and text/viewport geometry,
and require fully read bodies. On Apply compare the state after ordinary Settings activation with
that after Apply: opening Settings legitimately selects that menu item. Page/selection, textures and
audio receipts then remain unchanged. Exercise menu Up/Down, New Game to empty slot selection and
Cancel back to menu, without naming, campaign start or any gameplay-save write. Persistence observation
is a nonempty envelope even with no session; check its null session/progress fields explicitly.

**Confirmed:** the corrected finite observation resolves all six rows in both languages, has no
input/text/glyph/geometry failures and no session/presenter errors; menu/slot Cancel works, no gameplay
session/save is created, and both locale changes retain textures/audio/page/selection. The native
process exits 0 before its deadline. Its stderr retains one shutdown warning for two ObjectDB instances,
an unattributed lifecycle **Unknown** alongside earlier observations; this slice does not investigate
or resolve it. Headless glyph/geometry checks do not establish physical rendering or human reading
comfort. Existing long-text, invalid-parameter, naming and independent-process Save/Continue acceptance
is reused rather than replayed.

The first completed local attempt has no glyph/geometry failure but failed two caller assumptions:
selection was compared before the pointer selected Settings, and the empty-session persistence
envelope was incorrectly required to be an empty object. Its result remains retained. Corrected
observation compares from Settings activation and checks null session/progress fields. No production
behavior, source input, fixture or accepted comparison was changed to satisfy it.

Direct document/link/scope/private-content checks and the clean committed planner own this two-document
tracked diff; record the selected hosted CI in the handoff/PR. No fresh SDK, engine, normal research,
full, H3, campaign or save-compatibility suite is required by this content sample. Main-gate independently
reviewed the suggestion's semantics, including the connected 219–220 condition and Chinese phrase
breaks in 218–219. **The user approved this sample's text tone on 2026-11-03; in-game typography
and actual reading review remain pending** in the private bilingual artifact and epic #1468. This
speaker's tone approval does not prescribe other characters' voices. Closing this engineering sample
does not approve a glossary, expand
the script batch, establish full-opening Chinese or grant private-content publication rights.

## Localized Party and field Prepare

The [M2c consumer agreement](../application/localization-design.md#implemented-m2c-party-and-field-prepare-boundary)
owns English/zh-CN labels, inventory/status, choices, confirmations, NoEffect and recovery. No engine,
source admission, save or naming rule changes are included. Use the installed Godot, current Debug
adapter, existing imported font and the ordinary [opening Prepare route](#opening-field-preparation).
Load the current ignored input configuration in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Keep fresh reports local and use an explicitly configured
external disposable save namespace; preserve original slots. No screenshot or world copy is needed.

At 1920×1080, Continue an existing eligible opening save (or reach the same settled field normally):

1. Open Party with E / Pad Start. Read class availability, all six aligned stat values, HP/MP,
   kills/defeats/gold, learned magic and inventory. Valid source/custom names remain literal.
   Confirm the stat nodes retain `LEVEL`, `ATTACK`, `DEFENSE`, `AGILITY`, `EXP`, `MOVE` names.
   Navigate members, cycle focus, scroll each list, open the roster and select a member with a
   complete pointer press/release. Compare party, revision, gold, flags and both seeds unchanged.
2. Open Prepare by Confirm or its visible control. Preview equipment and Cancel; compare the full
   state again. Explicitly confirm one supported equipment change and observe one publication and
   the projected attack/loadout. Redraw the result without repeating the command. Choose an actual
   unsupported weapon Use and check localized refusal without changes. An already carried Medical
   Herb's ordinary NoEffect confirmation/result keeps its item/resources and both seeds; this is
   not an unsupported effect. Do not acquire new private effects for this screen check.
3. Close, Apply the other language through the existing Settings menu, then reopen Party/Prepare.
   Compare session/resources/revision/seeds across Apply. Settings is unavailable while Party is open.
   Reopen/close the panels repeatedly and verify ordinary field control returns. Read text and actual
   font/geometry, including wrapped hints and return controls; human typography approval is separate.

For a genuine callback failure, use a small pre-start authored variation of
`remake/content/authored/localization-pilot.json` under the owning ignored report directory. Remove
`start.program`, place the player at `(2,1)` facing0, replace the existing field-only item113 with
already admitted rare/droppable item61 in `battle.battleDiscard.items` and the first chest's
`itemWord`, keeping its original authored label and setting `unsellable=false`. Leave item0 and all
other definitions unchanged; omit `battle.start.itemCustody` as in the original. This is an input
variation of supported Drop, not a new Use/spell/map effect or reader capability. Launch through
`--authored-package <absolute-variant>`, Search the chest normally, dismiss its result, then open
Party → Prepare → Drop → item → Confirm. The actual engine returns
`discard-custody-unavailable` / `party.itemCustody`: the item is kept, localized custody advice precedes
exact diagnostics, and Back lies below the measured body and returns to action selection. Repeat in
English and Chinese and compare complete old party/revision/seeds. No result is injected into the UI.
The source diagnostic remains English intentionally.

A private diagnostic controlled-start through the existing `PrivateExplorationReader` does not bind
product opening preparation. Its real `field-preparation-unsupported` query exercises unavailable
recovery separately. Use an existing bounded start whose palettes are supplied by the selected world
rather than duplicated in `start.mapPalettes`. Both pointer Back and Cancel must return to the member
screen, then Close returns to field control, with the complete state unchanged. Retrying Prepare
refreshes the query; it must not trap the player in the same unavailable result.

Missing-row observation uses only the native resource cache before host construction: erase Chinese
`party.stat.attack` and `prepare.confirm-equip` in that observer process. Open the actual consumers;
ATTACK and the whole equipment confirmation use English, while other rows stay Chinese. Their
existing row metadata must say effective `en` / `missing-translation`, and the hint must show the
key/reason. Disk resources and source content stay unchanged. Check glyph presence against each
actual label's font, minimum/wrapped height against its allocation, scroll-pane bounds and the
1080p canvas. Display refresh retains option index, typed command, actor/slot/word and captured revision.

Bounded native observations cover these English/Chinese transactions, cancel/readonly navigation,
actual custody/query failures, fallback, redraw and return controls with no geometry/glyph failures
or runtime warnings/errors. Prepare's title and choices measure wrapped body height; hints reserve
room for untranslated names and fallback. Supported triangle markers replace the prior missing small
triangle glyphs without changing node/action identity. The ordinary opening/retained Prepare drivers
are reused; successful checks and original completed failures remain in the Issue's local handoff.
Adapter compilation, direct catalog argument/coverage checks, the clean committed dependency plan and
actual selected CI complete this boundary. There are no new engine rules or tests of probes/readers.

Antidote/DETOX acquisition, private terminology/dialogue translation, other battle menus/results,
Unicode names, original natural reach/timing, public exports and human translation/typography approval
remain outside this check. Do not enlarge the field-only catalog's effect admission to test them.

## Localized battle commands targets and status

**Current production UI:** the full-card/cardinal-target geometry described here is retained historical evidence at [the pre-#1686 implementation](https://github.com/FrankHZ/md-sf2-reverse-engineering/tree/83a5d47adcabab295420ef15a54848529180cd2e/remake/game/probes). Do not use its old layout assertions as current acceptance. The [current F4/F8/F9 recipe](../godot/battle-interaction-baseline.md#direct-production-reproduction) covers compact cards, legal targets, bilingual resources and inspection; underlying source/disclosure/resource identities retain their original scope.

The [M2d Standard agreement](../application/localization-design.md#implemented-m2d-battle-menu-and-status-boundary)
is a Godot consumer slice. Compile with `uv run sf2 verify adapter`; build the actual Debug adapter
from `remake/` using the existing absolute `DOTNET_BIN`, `DOTNET_CLI_HOME` and forced
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Changed PO resources need one necessary existing-editor
`--headless --editor --import` scan. Reuse existing imports/project/SDK; serialize all SDK/native
launches. No engine rules change, so no new engine tests, research/full/H3/H4 or opening replay.

The retained `local/issue1478/run-native.ps1 -Run <fresh-case>` and `battle-native.gd` extend the
existing `engine_menu_mouse_observation.gd` input/focus/receipt helpers. They use Main.tscn, real
mapped keys and mouse motion/press/release, actual state/labels/glyphs/geometry and emitted session
results. No state setters, screenshots, new probe framework or private acquisition is involved.
Each native process has a 45-second wall limit; input is fixed60 at1920×1080. Reuse the transferred
`local/issue1471/field-01` APPDATA/LOCALAPPDATA/TEMP and fresh external `issue1478` save namespaces.
Read-only English/Chinese settings derive from the existing `local/issue1408/keyboard-pointer.json`:
keep ordinary keys, unbind only its already-unbound movement axes, select instant text and language.
The user still chooses product language through settings before battle.

| Fresh case prefix | Existing input and bounded claim |
| --- | --- |
| `heal-en`, `heal-zh` | Small pre-start `healing-rule-demo.json` variant: medic HP5; guard positionOverride(7,4). Open Magic/MEND1/self-target, release-click NextTarget to the out-of-range guard, reject Confirm without revision, Back to the same spell list, reopen and commit one real heal; subsequent ordinary control. Acting/target low HP and zero-MP values/resources remain truthful. |
| `heal-fallback` | Same variant, Chinese settings. Before host construction erase `battle.choose-target`; replace Chinese `battle.target-help` with invalid `{9}` and extend that complete English template with a longer explanation retaining exactly parameters0/1/2. Verify missing/invalid effective-locale metadata, whole English fallback, Chinese neighboring rows and measured bounds without changing command meaning. |
| `equip-en`, `equip-zh` | Unchanged `battle-equipment.json`: preview/cancel without mutation, one committed selected Slot, unchanged RNG, Slot4 Unarmed, empty magic and literal Healing Herb. Actual AuxiliaryHud/current choice and reading hints are localized; diagnostic-only spell/item dumps need no translation claim. |
| `literal-zh` | Same equipment package with item0 name `Empty`, weapon50 name `Unarmed`, actor0 items[2]=127. Real names remain English while actual empty slots and Slot4 translate; no English Label matching is allowed. |
| `defeated-en`, `defeated-zh` | Unchanged `stone-court.json`: ordinary Attack, target and Confirm, then actual lethal Reaction target card HP15→0 and localized defeated cue. This does not acquire new game data. |
| `refusal-en`, `refusal-zh` | Retained `local/issue1395/corridor.json` from the movement-range owner. Actual occupied-destination refusal retains revision/resources/RNG, shows code-based recovery, then legal movement/action input clears it. |
| `stopped-en`, `stopped-zh` | Small public variant below: menu physical-definition Unsupported remains cancellable. Stay then actual automatic decision produces Unsupported with selection cleared; global feedback must direct relaunch, retain code/target.physical, and reject later Confirm/Cancel without mutation. |
| `startup-zh` | Existing registered standalone battle01-player-ready input lacks exploration map resources. Its actual adapter startup failure must display stopped recovery and battlefield-map-resource-unavailable, retain diagnostics, and accept no gameplay command. This is an expected negative case, not a valid private battle entry. |
| `private-en`, `private-zh` | Reuse the coherent retained original-icon recipe below; actual source textures retain command/node/resource identities, 96px squares and untranslated raster lettering. Chinese heading translates. Pointer press alone does not activate command:3; release chooses Stay; idle/redraw does not submit/restart emphasis; Cancel returns usable actions. |

For the stopped input, reproduce the existing actual-engine case
`TargetSelectionTests.MissingRequiredCandidateDataOrLateSettlementRejectsWholeAction` (`physical`)
without adding a test of this observer. Starting from `stone-court.json`, set mainSeed=(55<<16)|0x1234,
thinkingSeed=0x00EF0042. For actors/start rows0/1/2 set current/max HP500, DEF4, ATT18/18/30;
physical critical chance one-in-32/one-in-32/one-in-16 and damageBonus half/half/quarter. Set classes0/1
to unpromoted-warrior/unpromoted-swordsman; placement2 aiStrategy=attack-then-approach and actor2
move1; place actor1 one tile left of placement2. Finally remove actor0.physical. This is admitted
content followed by a real AI fault, not a failure message injected into a Label or synthetic session.

The private-icon selection must stay coherent: load `local/issue1045/inputs.ps1`, select retained
`local/issue1045/world-route-13.json`, tracked `battle01-player-ready.json` for controlled party,
`local/issue1412/main-gate-retained-icons/scene.json` for the already transferred immutable rasters,
and `--private-exploration-start remake/reference/inputs/battle01-to-battle02-start.json`. Use the
existing bounded eligible Cancel skip before battle control; do not replay the full opening.
Standalone private battle lacks field resources; old Battle02 world/scene selections lack these
command rasters. Neither is evidence that mounted icon identity passed. Do not rebuild/copy the
world or regenerate assets to repair a mismatched selection.

Require actual exit0, `passed:true`, empty failure/Unavailable arrays and clean runtime stderr.
Inspect actual visible labels for bundled glyphs, viewport/allocated wrapped-text bounds and stable
navigation IDs; check current/max/drawn HP/MP resource names and values. Compare actor/target/round/
revision/loadout/seeds before and after idle, hover, cancellation and rejected confirmation; record
real commit/result receipts. Long fallback must preserve readable target/Back/Confirm geometry.
Directly check both PO catalogs' unique keys/exact parameters, typed consumers, stable identities,
source sizes, relative document links, private boundary and exact owned diff. On a clean committed
head inspect `uv run sf2 verify plan --base origin/main --head HEAD` and actual selected CI.

Keep this slice's reports <=15MiB and H free >=1GiB; peak native observations are around1.1GB, not a
universal memory bound. Prefer compact semantic checkpoints and only relevant localization metadata.
If completed JSON/logs need lossless gzip archival, compare decompressed bytes with the originals
before replacing only this slice's files; preserve all failures/process exits. Do not clean older
slices or recreate caches. The first necessary PO scan can recreate untracked script UID sidecars from
cache; retain them under ignored slice output before freeze, without committing generated files.

**Confirmed, bounded remake observations:** English/Chinese keys and pointer release cover these
command/target/equipment/status cases, long whole-template fallback, real stop versus choice-level
Unsupported, actual lethal cards and mounted original command rasters. These do not claim human
translation/typography acceptance, physical gamepad/DPI coverage, source-image translation, private
terminology/default-name translation, new effects or battle-wide localization. Give/Discard/Retreat,
other dynamic scene/results and seven-character ASCII naming retain their existing boundaries.


## Localized battle transactions and retreat

Use the [M2e agreement](../application/localization-design.md#implemented-m2e-battle-transactions-and-retreat-boundary)
and [presentation owner](../godot/presentation.md#localized-battle-transactions-and-retreat-display).
Load the existing ignored private-input configuration in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the transferred H project, SDK/Godot/imports and
`local/issue1471/field-01` APPDATA/LOCALAPPDATA/TEMP. Run `uv run sf2 verify adapter`, then the actual
Debug build from `remake/`: `& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.
A changed-PO `--headless --editor --import` scan is necessary; do not create another environment.
Native input uses the selected Godot with `--rendering-method gl_compatibility --fixed-fps 60
--resolution 1920x1080 --path remake/game --script <retained-driver> -- <startup arguments>`.
Settings use existing `textMode: adjustable`, language `en`/`zh-CN`, reduced flash and unbound movement
axes, selected via `--input-settings`; no mid-battle language switch is added.

The retained `local/issue1480/run-native.ps1 -Run <fresh-name> -Case <case>` selects the following
finite cases. Its local drivers reuse existing real-input/state helpers, not a new production probe.
Each process is serial and bounded to 45 wall seconds, with named checkpoints and no screenshots.

| Case | Initial inputs and actual observation |
| --- | --- |
| `give` | `battle-give.json`, existing Give observer path: movement/preview, item/recipient/confirm Cancel, normal transfer and full-pack exchange. Both loadouts/ATT, RNG and queue remain stable during selection/reveal; each result commits once and acknowledgement advances to receiver control. |
| `discard` | `battle-discard.json`: duplicate slot identity, equipped/rare/protected wording, real protected refusal without custody change, confirm Cancel, ordinary and rare discard. Actual result acknowledgement preserves actor/action, queue/round, movement preview and RNG; outer Cancel returns movement. |
| `reading` / `fallback` | Reuse the bounded authored long-Give variation from the finite consumer migration above. HUD/result ReadMore precedes selection/ack without changing revision, inventory or tokens; switching candidates resets reading. Before composition, remove Chinese give-title and invalidate give-result parameters in native Translation resources. Observe whole English templates and key/reason/effective-locale diagnostics, then one transfer and subsequent control. |
| `literal` | An ignored `battle-give.json` variation replaces the admitted Practice blade name consistently with `Empty {0} Bowie`. English-looking words and placeholder-like characters remain literal in real choices/results, rather than becoming empty-state or name-substitution instructions. |
| `retreat` | Reuse `local/issue1047/world-01.json`, tracked `battle02-outcome-party.json` and `engine-battle02-egress-start.json` with current issue1047 input selection. Ordinary STAY reaches Bowie, spell selection/Cancel retains MP, actual EGRESS pays 8→0 once before localized text, then cue/effect/close and same-session usable field return/step. |
| `wing` | Reuse the existing accepted `local/issue1343/world-01.json` unchanged with `battle02-angel-wing-party.json` and `engine-battle02-angel-wing-start.json`. Ordinary Chester control, Item list/second Wing/self-target, Cancel then confirm. Payment retains slot2/word4, MP0 and source name Angel Wing; one payment precedes localized text, effect/close and usable return. No world copy or new acquisition. |

`-Case retreat -Run retreat-long-zhNN` selects the long variation. It removes the Chinese spell row and supplies a complete multiline English
baseline template in memory, before composition. Reveal-all and each ReadMore must retain the Text
token/revision/MP/inventory; only a fresh final Confirm starts Effect. Effect/Close tokens remain
separate and return uses the same session. Inspect native visible labels/glyphs, actual panel/body/
hint geometry and wrapped line heights, SessionResultObserved receipts and stderr. Long content is
clipped within the retained reading area; its frame/hint stay inside 1080p. Compare bilingual semantic
checkpoints for actor/target/slots, resources/RNG, queue/round and accepted continuations.
Compare revision/sequence/token deltas from the common prepayment boundary when the retained intro
completed naturally in one run and used the existing entry skip in another; retain both raw histories.
Custom member names retain the unchanged CharacterNames source/custom resolver; no new name-entry,
Unicode/IME or private terminology translation is admitted.

Keep this slice's reports <=15 MiB and H free >=1 GiB, retaining all failures/process exits. Reuse
only existing immutable inputs, keep fresh save namespaces outside checkouts if needed, and preserve
necessary import-generated UID sidecars in ignored output before freeze. No full opening, research,
full/H3/H4/emulator or engine/probe unit tests are needed for these display-only changes. On the
clean committed head run `uv run sf2 verify plan --base origin/main --head HEAD` and compare selected
CI to its adapter-only scope. Main-gate owns independent acceptance and merge.

**Confirmed, bounded remake observations:** English/Chinese transactions and both retreat payment
branches use native input and actual tokens/resources/geometry/errors; long reading and whole-row
fallback retain once-only work. This does not establish human translation/typography approval,
original reach/timing/audio, source/default-name translation, source-image lettering, other dynamic
attack/growth/status/results or opening-wide bilingual coverage. These remain outside M2e; canonical
execution, game-save identity and seven-character ASCII naming are unchanged.


**Unknown, retained exit warning:** one passing Chinese Wing observation emitted one exit warning listing two ObjectDB
instances. The existing [#1410 boundary](#audio-resource-lifetime-observation) remains separate; no shared cause is established. One same-case `-Case wing -Run wing-zh-verboseNN -VerboseNative` observation completed
with empty stderr and no leaked-object type output. The original warning is preserved; its owner
(observer-held references versus existing host/resource lifecycle) is not established. The changed
consumer code adds string formatting/catalog rows, with no new Godot node/resource creation or
lifecycle changes. This does not establish that the original warning is resolved or attributable to
an independent issue; main-gate retains that acceptance boundary. No broader audio investigation or
repetition of passing scenarios is implied.

## Localized story controls, choices and return notices

Use the [M2f Standard agreement](../application/localization-design.md#implemented-m2f-story-controls-and-return-notices-boundary)
and [presentation boundary](../godot/presentation.md#localized-story-controls-and-return-notices).
Reuse the installed SDK/Godot, owning project/imports and existing ordinary-input observers. After
loading current ignored private-input configuration in the launching process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run `uv run sf2 verify adapter`, then build the actual Debug
adapter with the selected absolute DOTNET_BIN. A changed assembly needs a new process to load it;
changed PO files need the ordinary editor import. Stop SDK build servers before native work.

The retained finite launcher is `local/issue1481/run-native.ps1 -Run <fresh-name> -Case <case>`.
It uses 1920x1080, fixed60, existing `local/issue1471/field-01` writable runtime directories and fresh
external save-validation namespaces. It emits semantic checkpoints, relevant label rectangles,
line heights, missing glyphs and catalog status only; no screenshots, frame capture or world copy.
The local caller extends `engine_story_dialogue_observation.gd` for actual-host input/state/geometry,
and uses the existing menu-mouse press/release recipe for choices. Return reuses the bounded EGRESS
caller from the preceding [transaction group](#localized-battle-transactions-and-retreat).

| Case / finite existing input | Required observation |
| --- | --- |
| `story`, English/Chinese | Reuse `local/issue1393/control.json` from the [shared-story recipe](#shared-story-dialogue-observation), derived from public `harbor-arrival.json` with the next battle omitted. Ordinary Talk reveals the English body with localized hint/fallback; Cancel preserves token/flags/party/seeds. Reveal-all, continue, stable default Yes and keyboard Right/Cancel=No; reopen, complete right-click No, reopen, left press alone does not answer, full release Yes makes the actual flag10/yard-map branch. Nodes remain Yes/No and selected labels use supported `>`. |
| `long`, English/Chinese | Reuse existing `local/issue1393/overflow.json`. Reveal-all, then read every measured page before a fresh acknowledge. ReadMore/idle preserve revision, token, flags, party, cursor and seeds; new windows reset reading. The same finite choice semantics remain. |
| `story` with `fallback-zh` run name | Before composition erase Chinese `story.continue` and give `story.choice-help` invalid `{9}`. Observe complete English templates, effective locale/reasons/diagnostics and the independently identified English body. No canonical work is altered. |
| `battle`, English/Chinese | Unchanged public `jaro-conversion.json`; ordinary two Up previews and STAY reach two battle-story windows and actual camera waits. Check camera slot/focus, real tokens/completions, visible body fallback only in Chinese, readable hints/geometry, close and fresh movement. |
| `skip`, English/Chinese | Unchanged public `harbor-arrival.json`; choice excludes skip, later eligible intro shows actual mapped Cancel, held skip reaches handoff but cannot become combat input, release admits battle and fresh Confirm/Cancel works. |
| `settings`, Chinese | Existing public `localization-pilot.json` with its validated overlay, ordinary text acknowledgement then safe Settings entry. Inspect the short partial-support notice, actual glyphs/measured bounds and unchanged canonical state on open/close. |
| `return`, English/Chinese | Existing controlled Battle02 EGRESS input/world/party from the preceding transaction recipe. Ordinary STAY reaches Bowie; selection Cancel retains MP, EGRESS pays8→0 once, staged effect/close reaches same-session field. Read actual pending then ready typed retreat notice; first movement dismisses it. Compare resources/seeds and event order rather than translated strings. |

Require exit0, passed=true, empty failures/Unavailable and no script/runtime errors, with shutdown
warnings retained separately. Directly compare both PO key/argument sets, six complete return rows,
all new glyphs and source/input boundaries. The remaining Victory/Defeat event/lifetime variants are
already retained by the [battle-return owner](#battle-return-and-party-status-observation), including
`local/issue1398/victory-party-final` and `defeat-02`; inspect them directly instead of replaying battles.
The representative fresh return path is EGRESS, not a new claim of fresh victory/defeat execution.
Use the clean committed dependency planner and actual selected CI; no engine unit, research/full/H3,
probe unit tests, complete opening replay or new private acquisition follows from this display change.

**Confirmed executor boundary:** bilingual normal/long dialogue, keyboard and pointer choices,
intro skip, two battle-story windows/camera waits and EGRESS pending/ready/dismissed notices pass.
Controls/glyphs fit the actual viewport; shared panel body/hint/choice rectangles are disjoint.
Redraw/Cancel/ReadMore preserve applicable canonical state; explicit Yes changes the branch and the
already committed retreat payment is retained. The Chinese EGRESS entry observed one additional
`selection-cancelled` event before the common payment boundary, giving revision/token offsets of1;
subsequent semantic events and resources agree. Actual field movement advances the source random-walk
seed and is not counted as redraw invariance. Missing/invalid UI rows use complete English fallback.
The initial unsupported Continue glyph (`▾`, code9662) caused resource rejection; font inspection
identified it and supported `>` replaced it. One long caller initially waited for natural reveal
past its bounded deadline; ordinary reveal-all corrected the caller. The first settings caller
selected a no-overlay package, which correctly showed the distinct overlay-unavailable notice; the
existing pilot/overlay exercises the intended partial-support notice. Original completed failures and
import warnings remain under `local/issue1481`. Passing native logs have empty stderr; this does not
resolve the separate [ObjectDB lifetime Unknown](#audio-resource-lifetime-observation).

Other dialogue bodies, names, human translation/typography, original
reach/timing/audio, Unicode naming, export and full-opening Chinese acceptance remain outside this
slice. Existing FieldTextWait overlays and all game-save/engine semantics remain unchanged.

## Localized semantic battle feedback

The [M2g display agreement](../application/localization-design.md#implemented-m2g-semantic-battle-feedback-boundary)
is presentation only: typed Chinese functional feedback, preserved English source-ID display,
shared card/reading keys and complete English row fallback with a current body/hint key/reason notice.
The source mapping is reviewed directly for all existing finite phases, action/reaction kinds,
Critical/side, seven GrowthNotice kinds and 306/354 sleep-turn values. Spell growth uses SpellRef.Level;
stat growth uses Amount. There is no engine change or new engine/probe test.

Use the existing absolute SDK/Godot selections and serialized H environment/imports. Load the current
ignored configuration in the launching process, force DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false, run
`uv run sf2 verify adapter`, and build the existing Debug host as in the
[locked workflow](../development-and-verification.md#locked-net-workflow). Keep worktree-local runtime
state, fresh external save namespaces and relevant-field reports under the 15MiB budget; no new
world copy, environment, cache, SDK or screenshots. The executor's ignored `local/issue1483/run-native.ps1`
accepts a fresh Run and a finite Case (`physical`, `drop`, `aura`, `poison`, `heal`, `literal`, `private`).
Its local caller composes ordinary Main, uses existing read-only observations and actual key events;
no session setters or invented focus events supply a result. The tracked production probes are unchanged.

**Confirmed — bounded headless native observation at 1920×1080:** the headless DisplayServer reports
focus, and ordinary production input/suspension checks remain active. Existing fixed-fps60 launch
runs in the background without a physical game window. Catalog glyph coverage, native Label metrics,
clipped reading-area/card/hint geometry, current message text and canonical state are read directly.
These observations do not prove physical rendering, desktop focus transfer or human reading quality.
No foreground run was needed for this slice. The
[background execution preference](../development-and-verification.md#scope)
applies to future suitable checks too.

| Case | Actual finite boundary |
| --- | --- |
| physical, English/zh-CN | Existing progression-outcome-victory: ordinary interaction/entry, physical attack/damage/death, EXP, level and five stat gains, gold, end and field removal. Existing English wording remains; Chinese uses typed functional templates. Both retain reward/resource/RNG and phase-token order. |
| drop, English/zh-CN | Existing battle-drops: ordinary target selection reaches enemy; actual dropped/received Taros Sword, death, EXP/gold and custody publication. No private source text enters a PO row. |
| aura, English/zh-CN | Existing aura-healing: ordinary AURA4 selection, six ordered target recoveries (including full HP), one MP payment, capped EXP and next control. |
| poison, English/zh-CN | Existing poison-turn: ordinary Stay, fixed2 nonlethal HP loss and next actor, then lethal poison/exhaustion and field cleanup. Short Chinese notices fit the measured panel with the unchanged automatic0.5-second policy. Wider reading comfort remains Unknown. |
| heal, English/zh-CN | Existing healing-rule-demo: ordinary HEAL action/recovery, logical work through retirement and next control. Retained M1 work/fairy evidence stays authoritative; this checks affected display/hints directly. |
| literal, English/zh-CN | Before composition vary only the public drop item's admitted label to include `{N}`, `{ITEM}`, `{D1}`, `{CLEAR}`, `{NAME}` and `{0}`. The actual committed drop/receipt shows all tokens literally, including English source-template substitution. CharacterNames/default/custom resolution remains source-equal to accepted main. |
| fallback, Chinese | Erase attack and invalidate damage parameters before composition. Each actual message uses a complete authored English row; its visible hint names only the current key and missing/invalid reason. An unrelated missing poison row cannot contaminate this screen. Missing Continue similarly falls back visibly with its own key/reason. Blank phases do not display an erroneous body notice. |
| long, Chinese | Lengthen the existing action row before composition. The actual scene reaches multiple ReadMore pages; each retains token/revision/resources/RNG, and acknowledgement follows reading. The clipped body and measured hint remain disjoint at1080p. |
| private, English/zh-CN | Reuse the accepted controlled Battle02 world/party/start recipe from issue1398. Ordinary intro skip/Stay reaches Sarah and actual full-HP self HEAL, one MP payment and EXP, then next control. The original private inputs stay read-only. No natural original reach/timing claim follows. |

Direct comparison retains typed actor/resource/seed/custody state, applicable phase tokens/revisions
and semantic event outcomes across each bilingual pair. The private intro can have a constant
revision/token offset; compare the common control boundary and subsequent deltas, retaining that
difference explicitly. Direct catalog checks require every key and exact argument set in both PO
files; every added public row is project-authored functional UI text. English source-ID selection,
CharacterNames precedence, canonical HEAL work and all input/continuation/animation code are checked
against accepted main. Clean committed planner and actual CI select adapter-build, not engine units,
research/normal/full/H3 or whole-campaign replay.

Completed failed attempts remain in local/issue1483: the first headless pilot read a stale field
node after battle mounting; a subsequent caller double-connected its observation signal on reparent;
drop selection initially chose another target; the first long sample fit the enlarged panel and a
later check incorrectly required the scroll content label itself to fit the viewport. Corrected
callers read the active node, connect once, select the intended admitted target and measure the
clipped reading area. None changes production timing or input. Early private attempts used the
Battle01 menu fixture, whose missing EXP correctly refuses HEAL with `unspecified-exp`; the accepted
Battle02 recipe supplies the required operand. No golden/schema/input was weakened. Import regenerated
missing UID sidecars from the existing cache; the originals are retained locally after scope review.
Reports were narrowed to this Issue's Project record and losslessly compacted with semantic equality,
preserving every failure/result within budget. No old evidence or environment was removed.

Public headless passes have empty stderr. Private HEAL exits0 without session/presenter errors but
reports one shutdown warning for eight ObjectDB instances; failed private attempts also retain their
own warning counts. This remains an unattributed lifecycle **Unknown**, alongside the earlier issue1480
one-warning/two-instance observation; no new audio investigation or claim of resolution is made.
Original/private prose and terminology translation, source-image lettering, Unicode naming, export,
human translation/typography and full-opening Chinese acceptance remain outside this slice.


## Display layer boundary observation

The [Standard scope and fixed canvas contract](../godot/presentation.md#display-layer-boundaries-1517)
own #1517. Reuse the transferred H worktree, private inputs, SDK/Godot installation, project/imports
and existing ordinary-input probes. Load `local/issue1441/inputs.ps1` in the launching process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run `uv run sf2 verify adapter`, then build the existing
Debug adapter from `remake/` with the configured absolute `DOTNET_BIN` and `--no-restore`.
No engine rule changed; no behavior-test addition or normal/full research suite is selected.
On a clean committed head inspect `uv run sf2 verify plan --base origin/main --head HEAD`.

The retained launcher is `local/issue1517/run-native.ps1 -Run <fresh-name> -Case <case>`.
It extends the existing scene/story/return and world-viewport observers with finite node facts;
there is no production observer API or new test framework. Use the cases below serially, with
`--headless --fixed-fps 60 --resolution 1920x1080`, isolated ignored report/save namespaces and the
existing worktree-local runtime directories. Target <15 MiB retained reports and >=1 GiB H free;
losslessly compress repeated diagnostic logs when necessary, preserving their failed process result.

| Case | Controlled consumer and actual acceptance |
| --- | --- |
| `private` | Existing admitted Battle01 entry in `local/issue1491/battle-start.json`, current services world and `map3-opening-party.json`. Ordinary intro skip/Confirm, Bowie STAY, then Sarah HEAL. Capture map sprites/terrain and weapon/ally/fairy/message/card intersections, message/result/reward input and return to map. Before/after canonical event arrays match exactly, including RNG and rewards. |
| `physical` / `aura` | `progression-outcome-victory.json` attack/messages/results/death/rewards and `aura-healing.json` magic. Ordinary input and existing same-token redraw/ack checks; authored placeholder art is not proof of private source art. |
| `battleStory` | `jaro-conversion.json`: camera wait, map/story dialogue, close and fresh movement. Existing story observer; normal-reading settings from issue1481. |
| `story` | Existing `local/issue1393/control.json`: field reveal/read/choice/continue/cancel with ordinary input and token/state invariants. Use normal-reading settings, not the scene observer's instant-text selection. |
| `field` | Small controlled initial Map3 priest seam derived from the existing admitted start: player32,13 facing Up, current world/party, no mutation after start. Party open/close; parent Hide/Show and process-disable; synthetic window focus signals; native pointer Settings/Save open, input blocking, recovery cover/size restoration and Church unavailable choice/close; fresh movement and full owner retirement. |
| `return` | Existing `engine-battle02-egress-start.json`, `battle02-outcome-party.json` and the matching issue1047 world. EGRESS cancel/payment/text/effect/closure, same-session usable field return and movement. Hidden battle and every canvas descendant are invisible; field remains active; retiring the host leaves neither canvas. Do not substitute the services world's different party definition. |

For each relevant visible node retain its path, nearest CanvasLayer/canvas identity, relative Z,
ancestor Z accumulation until a canvas/non-CanvasItem boundary, tree index and inherited visibility.
Use actual Sprite2D rect transformed by `get_global_transform_with_canvas()` and actual Control
rects for intersections; a node's own Z alone is insufficient. Inspect existing map terrain Z2
against actor Z1 as well as UI canvas order. Scene cards/messages are canvas1 over intersecting
sprite/effect canvas0; field dialogue/menus are canvas2; host windows are canvas3. At scene end,
check map visible and scene UI hidden; at field return check the entire hidden battle subtree,
not just its root. Verify viewport roots, unchanged consumer fonts/rects and explicit themes.
Modal input is independently checked by actual pointer/key events and unchanged revision, tokens,
party/gold/flags/RNG while covered, then fresh control after close.

**Confirmed — bounded headless Godot observation:** all matrix cases above complete exit0 with no
session/presenter/script error. Private HEAL reproduces the pre-fix overlap, and the corrected
composition retains that geometry with higher UI canvas order. The pre-fix and corrected HEAL
semantic receipts are identical. Field Hide/Show, owner process inheritance, scene reparenting,
EGRESS return and host retirement are directly checked. World plane/entity algorithms are untouched;
these observations do not replay every wall/roof or certify final raster pixels.

Retain the initial implementation failure: freeing `SceneUi` on ExitTree disposed still-live message
nodes during ordinary battle reparenting. The correction keeps that UI with the battle session and
resubscribes visibility on tree entry; the same private consumer passes. Earlier attempts also retain
an invalid private party selection, a GDScript type error, a wrong return-world party definition and
an instant-versus-normal-reading observer mismatch. These are failed attempts, not successful gates.

**Unknown / not applicable:** headless has no real desktop fit or final raster proof. Its restore and
fullscreen buttons are correctly unavailable; clicking disabled restore and using actual recovery
reading input cannot submit gameplay, and a supported-size window event restores the covered Save
modal before field control. Physical restore-button execution and physical focus are unobserved;
focus events in the lifecycle check are explicitly synthetic. No screenshots, per-frame collection,
foreground window, all-device pass or original natural route is claimed. Public authored cases and
EGRESS exit with empty stderr; private HEAL/field checks retain shutdown ObjectDB warnings (six/four
instances respectively), without attributing or resolving the existing shutdown Unknown. Earlier
issue1491 `1801 PASS / 6 FAIL / 1 SKIP`, issue1492 observer failures and school03 exit2 remain retained
in their original handoffs; this slice does not rerun or relabel them.

## System menu observation

The [Standard agreement](../godot/presentation.md#system-menu-agreement-1518) owns #1518. Reuse the
transferred worktree/environment, `local/issue1441/inputs.ps1`, locked SDK, existing Godot/project
imports and `local/issue1471/field-01` runtime directories. Load config in the actual launch process;
force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Use separate issue1518 reports and external disposable
save namespaces. Single instance, reports<=15MiB, leave>=1GiB on H; measured native peak<1GiB.
No screenshots, frame acquisition, full-route replay or broad engine/research suite is required.

Retained launcher: `local/issue1518/run-native.ps1 -Run <fresh-name> -Case <case>`. It uses headless,
fixed60,1920×1080 and ordinary `Main.tscn`; reports contain state/input/geometry/error observations,
process exit/time/peak and complete stdout/stderr. The local observer is not production or a new
verification framework. For an equivalent fresh observation, use ordinary mapped `Input.parse_input_event`
key/pad/axis and complete mouse gestures against actual Control rectangles; read existing host/view
observation methods and native geometry, without setters or manufactured gameplay receipts.

| Case / fixture | Required finite observation |
| --- | --- |
| `field` | Existing #1517 initial priest seam using current #1441 world and Map3 party. Only one SystemMenuEntry is visible; F10 opens, Resume is selected. Settings and existing New Game replacement open, Cancel returns Menu; Quit defaults Cancel. Controlled-start Save remains disabled with an explanation. Party/Equipment and Party Cancel retain their own responsibilities. |
| `physical` | Existing `progression-outcome-victory.json`, ordinary Confirm to player Movement. Same Menu/button geometry; Save/Settings/New Game disabled, no activation on click. Battle Confirm/Cancel continue their existing gameplay responsibility after resume. |
| `product` | Copy #1443 route-01 slot1 read-only into the fresh issue1518 save namespace, then ordinary title→Witch→Continue. Restored product savepoint permits Save. Confirm-save Back→slots Back→Menu; cancel every write. Compare the disposable slot bytes with the original read-only source afterward. Never use the original namespace as writable output. |
| `story` / `battleStory` | Existing #1393 `control.json` / authored `jaro-conversion.json` and ordinary #1481 adjustable text settings. Jaro uses ordinary Up twice, Stay and round progression to reach `story.kind=DialogueWait`. Open Menu at text, confirm all three conditional actions disabled, verify reveal/token/resources pause, Resume does not acknowledge, then accept fresh text input. |
| `prefs` | Fresh disposable ordinary preferences with explicit Menu=F9/RightX− and copied product slot. Help reports Menu; Help Back retains Settings; Language Apply refreshes Chinese Menu without session changes; Defaults/Apply restores English and preserves all custom Menu bindings. |
| `bindings-*` | Existing authored battle plus version1 configs: default; old Attack=F10+RightX+ (Menu becomes F9+RightX−); old Up adds RightX±/RightY− so all eight signed axes are occupied (Menu keyboard only, no Guide, visible reason and working mouse); explicit Menu=F9+RightX−; swapped Confirm/Cancel; invalid explicit duplicate F10 (typed invalid-input-settings before session). Inspect actual InputMap, hints and injected input, not only JSON parsing. |

For field/battle/product, repeat English/zh-CN: read every visible menu Label/Button, required font
glyphs, native minimum sizes, bounds and selected/disabled states. The800px panel fits1080p and
2560×1440 using native coordinates and the existing world scale. Shrink to1280×720: recovery hides
the same Menu, Cancel opens existing recovery Quit, Cancel returns recovery, and an actual supported
size event restores the same Menu still paused. This is recovery at unsupported gameplay size,
not a new720p gameplay claim. Headless Restore/Fullscreen physical execution remains **Unknown**.

Use keyboard, pad right-stick open/East Back, mouse entry/Resume/right-click. Hold a Quit mouse
press across recovery cover/restore: its old release cannot activate confirmation. While the opening key
and another Confirm key are held, no activation crosses the existing all-release barrier; only a
fresh Confirm resumes. Also check no old host button exists, entry is hidden under modals, hidden
Menu is process-disabled, and retirement frees HostUi. A mapped Menu toggles only Menu itself;
subpages retain their own Back rules. Actual desktop delivery of pad axes and physical focus remain
**Unknown**; injected native events establish host routing, not all hardware.

Final finite observations pass with no script/session/presenter errors. Private field/product and
invalid-input teardown retain ObjectDB warnings (two instances); prior #1517 private/field/return
6/4/2 warnings remain unattributed. Initial observer parse failure, recovery Cancel/geometry
assumption failures, a misnamed config, and an omitted Jaro trigger were corrected in the observer,
with failed reports preserved. The misnamed config also exposed uninitialized window-signal
disconnect errors: the narrow subscription guard corrects that real host early-exit fault; explicit
invalid binding now retains typed failure without those errors. No claim of general leak closure.
Historical #1491 engine1801PASS/6FAIL/1SKIP and school03 exit2 remain completed evidence and were
not rerun or reclassified. Headless facts do not establish final raster, physical focus or human
approval. Run affected `uv run sf2 verify adapter`, then obtain `verify plan` on the clean committed
head and compare actual CI with that plan.

## Church and shared Save calibration

The [Standard agreement](../godot/presentation.md#church-and-shared-save-agreement-1519) owns #1519
inside the existing High save/recovery contract. Reuse the transferred worktree, SDK/Godot/project/
imports, `local/issue1441/inputs.ps1` and `local/issue1471/field-01` runtime directories. Load selections
in the actual launch process and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reports belong in
`local/issue1519`, at most15MiB; keep at least1GiB free on H. Use one native instance, headless fixed60,
1920×1080, bounded45s; a selected Church/common-slot sample also checks2560×1440. No screenshots,
frame acquisition, environment recreation, large derivatives or full route are needed.

The retained launcher is `local/issue1519/run-native.ps1 -Run <fresh-name> -Case <case>`. Its small
local observer reuses #1518's actual-host input and observation helpers and #1424/#1443/#1454 fixtures;
it is not another production verifier. For equivalent reproduction, instantiate ordinary `Main.tscn`,
send mapped keyboard/pad inputs and complete mouse gestures via `Input.parse_input_event`, and read
`ReadObservationJson`, `ReadPersistenceObservationJson`, `ReadSaveObservationJson`, native Control
rectangles, minimum text sizes, bundled-font glyphs and errors. Never set session state or manufacture
a transaction receipt.

| Case / existing input | Finite acceptance |
| --- | --- |
| `field`: #1517 Map3 priest-position diagnostic start and current #1441 world/party | Ordinary Confirm reaches the real priest/counter and `ChurchWait`. Inspect all services and bilingual rows; unavailable Promotion refuses without changing resources and focus clears its message. Healthy Raise/Cure show no applicable member. Leave completes the caller. Church Save closes the service, then the controlled profile correctly refuses saving; Back restores usable exploration. This initial-position seam does not establish natural reach or save eligibility. |
| `field` pause boundary | Hold a Church-row mouse press, open and close the real system Menu, then release the old press. No command/payment occurs. A fresh complete row click after neutral input works. This reproduced the old stuck release flag; checking only keyboard continuation would miss it. |
| `product`: disposable copy of #1443 route-01 slot1 | Ordinary title/Witch/Continue restores the real product savepoint at the priest counter. Menu Save and Church Save have exactly equal slot labels, destination and Cancel-first overwrite choices. Confirm Back returns slots; Menu slots Back returns Menu. Church cancellation returns exploration with unchanged save bytes and no service/event wait. Check selected-row help in both languages and1080p/1440p geometry. |
| English `product`, then `storage` | Perform one explicit Church overwrite, retain the original valid backup, and close success with held Confirm; it cannot reopen services or leak gameplay input. Reuse that disposable namespace for `storage`: after opening confirmation, retain its ordinary lock file, temporarily substitute a directory at that disposable lock path, then confirm. The real write reports failure, preserving primary/backup bytes. Restore only that test obstacle, close failure, reenter priest/Leave, and verify fresh legal movement without a second write. |
| `continue` | In a fresh process, ordinary Continue from that same issue1519 disposable save restores playable exploration without `ChurchWait`; fresh legal movement works. Original source save bytes remain unchanged. |
| `raise` / `insufficient` / `cure` | Reuse #1454's small authored treatment package; vary only disposable starting gold/status for insufficient funds and poison+stun+curse. Inspect real offer target/cost and mapped Decline, Confirm, results and return. Exact10-gold Raise restores100HP once; zero funds changes no resources; poison10 then stun20 clear only those bits, preserve dead HP and curse4, and state that curse removal is unavailable. Preserve MP, other members, progress and RNG; no duplicate payment on return. These are controlled service states, not proof of naturally acquired opening statuses. |

Use distinct fresh reports and issue1519 disposable save namespaces; for `storage`/`continue`, select
the already-saved product namespace rather than copying the original again. The original #1443 input
is read-only. Preserve complete stdout/stderr, exit/time/peak, actual observations and failed attempts.
The finite runs retain ObjectDB teardown warnings (two or four instances), without leak attribution.
Physical focus/real-device delivery, final pixels, original timing and human acceptance remain
**Unknown**. Preserve #1491's completed1801PASS/6FAIL/1SKIP and school03 exit2; none is relabeled or
rerun for this UI slice.

The first glyph check failed on the old `▸`; the Church uses supported `>` after correction. The
first product attempt completed its overwrite but stopped because the observer assumed no persistent
lock file. `storage` retains that file before substituting its disposable obstacle; no extra product
write is needed. Initial authored resource comparisons mixed integer expected values with JSON
floating-point values; typed numeric normalization corrects the observer without changing gameplay.
The fresh-click-after-pause failures reproduce the actual Church release defect and remain retained
alongside the passing correction; keyboard-only intermediate passes do not close that failure.

Run affected `uv run sf2 verify adapter`, the necessary existing Debug host build for native
observations, and inspect `uv run sf2 verify plan --base origin/main --head HEAD` on the clean committed
candidate. This is an adapter/UI correction, with no new engine behavior or helper tests and no broad
research/engine/H3/full-suite rerun. Direct ownership, source-size, document/link/catalog and private
boundary checks complete the slice. Record actual CI independently from the selected plan; queued is
not passed. Freeze a Draft PR for main-gate's independent review and merge.


## Church diamond observation

The [Standard current agreement](../godot/presentation.md#church-and-shared-save-agreement-1519)
owns #1587. Use `probes/engine_church_diamond_observation.gd` with the actual Debug adapter,
selected existing Godot installation/project and same-process private/tool configuration. The
observer submits ordinary mapped keys, pad buttons and pointer events; it reads real state, labels,
rectangles and errors. No screenshots, post-start state setters, synthetic successful commands,
whole-route run or tests of the observer are involved.

Small controlled input: copy `content/authored/localization-pilot.json` into a fresh ignored output;
remove `start.program`, set position(2,2), facing2 (West), keeping its real church interaction,
programs, party, text/font and presentation. Initial gold100 and dead/status-bearing medic provide
Raise10 and Cure poison10/stun20. For the `low` case change only initial gold to5. Do not mutate
live state. `public` uses no church rasters. `private` mounts only the eight privately prepared
`churchMenu` entries into that same input. `bad-png` changes only `cure.selected` to a33-byte PNG
header claiming24×24, with correct digest/declared shape but invalid image data. This deliberately
passes header admission and exercises the actual native decoder/failure boundary.

```powershell
$env:SF2_CHURCH_CASE = 'public' # private, low, bad-png, or real
$env:SF2_CHURCH_OUTPUT = Join-Path $freshIgnoredRun 'native.json'
# Resolve these paths before launch; APPDATA/LOCALAPPDATA/TEMP remain in this fresh owning run.
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1920x1080 --path $project `
    --audio-driver Dummy --script res://probes/engine_church_diamond_observation.gd -- `
    --authored-package $controlledPackage --input-settings $preferences
```

Preferences are ordinary version1 English/Chinese, instant text. The observer sets the headless
viewport to1920×1080 before mounting the ordinary host. Run with a55s process deadline and stop the
owned process after completion; retain logs, failures and process exit/stopped state. Reimport only
changed translation resources if required, and keep importer output out of the diff.

**Confirmed:** English and Chinese no-art samples fit the short96px button captions and full
selected text. Four cardinal icons plus independent Leave replace the five-row menu. Mapped
keyboard Promotion refuses without mutation; a new focus clears its reason. Holding a mouse press
through actual system Menu/Back rejects its old release; a fresh complete icon click reaches Raise.
Decline takes no gold; accept charges10 once while held Confirm cannot advance/recharge. Pad Cure
preserves ordered poison refusal then stun20/payment/result/menu. Healthy Raise and the separate
5-gold insufficient case preserve resources. Save help wraps at1080p/1440p. Authored Save completes
its caller and uses the shared `save-profile-unavailable` panel; closing it returns usable field.
The private-art sample observes all mounted normal/selected church resources. Bad PNG reports one
native decoder error, `church-menu-resource-invalid`, a hidden service and stable revision after
idle/fresh input; no repeated decode or payment occurs.

`real` uses the existing admitted private world with only these new resources, the same controlled
initial party and an explicit Map3 priest-counter start(32,13), facing1 (North), with the current
opening flags/display/text settings. Select it through `--private-exploration-start` rather than
`--authored-package`. Ordinary Confirm enters the actual original Church program/event; healthy
Cure returns no-applicable with gold60 unchanged. Leave completes the caller; a fresh interaction
reopens it. Save then completes the actual caller/portrait/finite settlement and reaches the same
shared eligibility failure with no `ChurchWait` or payment. This controlled diagnostic seam does not
claim natural reach or grant product save eligibility; retained
[shared Save calibration](#church-and-shared-save-calibration) owns successful slot/write/restart
coverage, whose host/format is unchanged here. No user slots/backups are read or written by this seam.

The representative authored run completed in<2s; real source observation in<2s after startup. The
single prepared private world preserves its selected encounter dependency files. Retain failed
startup/window/fixture setup observations and their corrections; they are not interrupted runs.
Existing eight-instance ObjectDB shutdown warnings in the real-source seam remain **Unknown**.
Physical hardware delivery, original menu animation/window pixels and human visual approval remain
outside these observations; #1456 owns the latter. Run affected adapter compilation, direct resource
preparation/provenance and document checks, committed planner selection and actual exact-head CI.

## Exploration gameplay menu observation

The [current F5–F7 agreement](../godot/field-and-shop.md) owns the compact list and
field/service reading layout. The [gameplay agreement](../godot/presentation.md#exploration-gameplay-diamond-agreement-1588) retains source mapping, pause and admission. `probes/engine_field_menu_observation.gd` drives
actual Godot input and reads the ordinary session/presenter state. It never injects gameplay commands,
completions or post-start state and captures no screenshots. Reuse the selected installation/project;
a new native instance is necessary when no owned one remains or changed adapter binaries need loading.

Prepare one small controlled authored start in an explicit ignored directory from
`remake/content/authored/localization-pilot.json`: remove `start.program`, set its position to(2,2)
and facing2. Keep the original world programs/events and controlled party. Write instant-text
version1 settings with language `en` or `zh-CN`. For private original icons use the existing
[world-menu publisher](../content/presentation-assets.md#private-exploration-menu-icons), and use its
resulting `world.json` as the authored package. After same-process configuration and Debug build:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:SF2_FIELD_MENU_OUTPUT = <absolute-new-ignored-report-path>
$env:SF2_FIELD_MENU_CASE = 'public'
& $env:GODOT_BIN --headless --max-fps 60 --resolution 1920x1080 `
    --audio-driver Dummy --path remake/game --script res://probes/engine_field_menu_observation.gd `
    -- --authored-package <absolute-controlled-package> --input-settings <absolute-settings>
```

Use the existing worktree-local TEMP/cache and fresh APPDATA/LOCALAPPDATA/report selections. No user
save slot is read or written. The observer exits after its finite steps; operator wrappers retain
exit/error output and stop at55s. `public` observes root geometry, selected help at1080p/1440p,
Member/Magic/Item consumers, empty spells, all free pages and the actual flushed closing input edge,
complete mouse entry, held Confirm, focus-exited signal invalidation and fresh gesture recovery,
system/Save exclusion, pad return/Party shortcut and actual Search. Delivering the Godot focus signal
checks the installed handler; it does not claim an OS window-focus transition. Independent visual
and human usability acceptance remains #1456.

Finite variants use the same scene and observer: omit the map `search` to verify disabled Search;
`remap` settings assign Confirm F5/South and Cancel F6/East; `swapped` uses `confirmCancel: swapped`.
`transaction` and `shortcut` add two item0s to the first authored actor, then compare gameplay Item
versus E/Prepare Drop. Both submit exactly one confirmed Drop through the same consumer while held,
preserve gold, restore the second item focus on pointer Back and return to field control. No new
field effect/rule is inferred from this observation. `real` uses a controlled Map3 private start and
an admitted world lacking FieldPreparation: Member remains available, Magic/Item are disabled,
Search reaches the existing field Search wait even while facing a priest. It does not prove private
normal-route reach or support absent from that content. `bad-png` replaces magic.selected by a
valid-dimension/digest but undecodable PNG and observes one stopped failure through further input.

**Confirmed:** bounded observations pass for these inputs and actual adapter projection/errors.
The four commands use 528px-wide rows at least80px tall, full wrapped captions and separate Back.
Original normal/selected resources use24px images at3× nearest; description scrolling does not move
the command hit regions. Up/Down traverses the list; mapped Left/Right reads its explanation. Menu idle and provisional navigation preserve revision, simulation tick,
party and gold; closing edge submits no command, and ordinary field time resumes afterward. Final
menu and shortcut Drop results are equal. The initial focus scenario exposed a neutral-release
reset located below the new modal early return; moving it before modal dispatch restores fresh
pointer input. Earlier failed observer assumptions and unsupported-content attempts remain in the
local slice handoff. **Unknown:** the private process retains the existing two-instance ObjectDB exit
warning; expected malformed-PNG decoder errors are a separate deliberate failure observation.

Gates are affected Content admission behavior plus existing FieldInventory/Recovery/Search tests,
adapter Release and necessary Debug build, direct source/docs/private checks and clean committed
planner/actual CI. The unchanged source-path limitation in normal toolchain verification and completed
old aggregate failures remain failures; this UI change does not require another full aggregate run.

## Held battle rate (#1619 B/C)

The [presentation owner](../godot/presentation.md#held-battle-acceleration-1619-bc) defines the
clock and reset boundaries. `engine_battle_rate_observation.gd` uses actual mapped host keys,
existing state/result signals and natural cue completion. It records selected endpoints and
semantic counts, not frames, screenshots, asset data or a new comparison platform. Additional
state is bounded; service cost is O(min(due,4) × existing consumer cost). The serial pilot used
39 launches including failed setup/observer attempts, each with a 45 s deadline and 100 KiB report
cap, within 2 MiB total semantic reports. No owned instance remained at takeover; changed Debug
binaries required fresh finite observer processes in the retained project/installation.

Load the existing ignored SDK/private configuration in the same process; build Debug once using
the [locked workflow](../development-and-verification.md#locked-net-workflow). Use fresh ignored
report/TEMP/APPDATA/LOCALAPPDATA and an external disposable `SF2_SAVE_NAMESPACE`. For an authored
case, select version1 instant-text input settings (or adjustable 15 cps for `reading`):

```powershell
$env:SF2_BATTLE_RATE_CASE = 'heal' # movement, reading, story, appearance, private-egress
$env:SF2_BATTLE_RATE_FACTOR = '1.5' # repeat matched work at '1'
$env:SF2_BATTLE_RATE_OUTPUT = <absolute-new-ignored-report-path>
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 60 --resolution 1920x1080 `
    --path remake/game --script res://probes/engine_battle_rate_observation.gd `
    -- --authored-package <absolute-selected-package> --input-settings <absolute-settings>
```

`heal` uses `remake/content/authored/healing-rule-demo.json`; run each factor at 30/60/120 fps.
`movement` and `reading` use `stone-court.json`. `appearance` uses `hidden-spawns.json` and
performs real movement, Stay, two appearances, an attack, death message and field exit.
`story` uses an ignored copy of `jaro-conversion.json`, inserting
`{"op":"wait-ticks","ticks":60}` at the beginning of program `pledge`; other content is unchanged.
It uses the existing region program/camera/conversion and both real dialogue acknowledgements.
Retain stdout/stderr and require both a passing report and no `SCRIPT ERROR`/`ERROR:`: an observer
exception can otherwise produce an incomplete report whose own `passed` value is misleading.

For `private-egress`, retain the registered complete-world/scene selections. Omit
`--authored-package` and set `SF2_BATTLE_RATE_START` to a fresh ignored copy of
`remake/reference/inputs/engine-battle02-egress-start.json`, changing initial map to `map-3`,
position to(1,26), facing2 and adding
`start.partyInput = "remake/reference/inputs/battle02-outcome-party.json"`.
The existing startup-only `StartPreVoyageLocalizationObservation` admits the complete world,
then the controlled party through the existing reader/initialization. This optional parameter
accepts a nonempty string resolving beneath `remake/reference/inputs/`, an existing file ≤64 KiB,
and a valid controlled battle input. Other paths, nonstrings, absent files and oversize files are
rejected as `observation-party-input`; production readers and active snapshots are unchanged.
Derived initial progression carries the validated controlled resource caps and loadout; seeds stay
those declared by the input. No post-start teleport/reseed/actor setter is used. Hold Left from
the real Map3 field across its west warp to Map66/Battle02; ordinary Stay advances the queue to
Bowie, real learned EGRESS pays 8 MP once, then actual presentation returns to usable Map3.
This is controlled initial state, not proof of natural spell learning or chronology.

**Confirmed:** the six HEAL rate/FPS combinations and final focused repeat match the accepted-A
1× 60 fps baseline exactly in all 208 mandatory HEAL operations, semantic counts/phase order,
final actor/resources/status/inventory, queue and both seeds. HP 30→40, MP 8→5, EXP 10;
final main seed 247534132 and thinking seed 3203334210. A warmed 0.20 s movement segment takes
212 ms at 1× and143 ms at 1.5×; first-load timing is excluded. Release during movement returns1×;
undo/inspection preserves seeds/resources and spends no action. The 60 tick story wait takes 1000 ms
versus 665 ms; story and appearance/death matched endpoints, semantic counts and phase order agree.
15 cps reading stays unscaled and Shift cannot acknowledge or draw RNG. Focus signals stop service
and cannot rearm a held key; a 120 ms rendering stall leaves at most 4 serviced opportunities and
only fractional debt. This exercises installed handlers, not a physical OS-focus transition.

Both private rates pass the actual Map3→Battle02→EGRESS→Map3 route. Field 1.5× is observed before
the warp, battle entry and field return are 1×, same-session identity survives, and fresh Down
moves the returned player. One MP payment and one field handoff occur. The actual retreat player
emits `finished` once with pitch 1; it is not playing when close is requested. Its natural finish
is 300 ms after effect request at 1.5× (335 ms at 1×); visual close follows at 351/468 ms. Music/other
sample pitch is never changed by the implementation. The unchanged existing Speed transaction
observer separately passes all four presets, custom 47.5 cps, rollback/Apply and paused fight state;
no settings/suppression/rule implementation is changed.

Retained failed attempts are discovery evidence: an observer incorrectly awaited HEAL delivery
on RewardMessage; raw complete-world startup hit `encounter-selection-limit`; controlled party
resources initially hit `numeric-range`; observers read prepared/retired/null/freed views before
actual attach; two blocked Map3 starts hit `map-entry-position`; the appearance observer wrongly
expected its selected killed actor to respawn; the full cumulative audio ledger hit the report cap.
Fixes selected the actual consumers, validated initial caps and playback endpoints; none weaken
production admission or golden state. Earlier movement first-load timings remain noncomparative.
Exact attempts, errors and retained reports are in the local Issue handoff. Existing private
ObjectDB exit warnings remain **Unknown**, separate from native script/engine errors (none in the
final successful cases). Human comfort, all devices, physical battle focus and all-route timing
remain **Unknown**. No new campaign, full-route or emulator observation is claimed.

Validation is adapter Release/Debug compilation, direct source/docs/locale/private checks,
committed dependency planner and exact-head scoped CI. Domain/Application/Content are unchanged;
there are no new engine unit tests or tests of this observer, and no aggregate rerun is required.
