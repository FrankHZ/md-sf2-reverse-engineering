# Local game debug port

Standard rigor applies to the adapter, ordinary input and Windows export boundary.
The ordinary Godot main scene owns a C# endpoint in both builds. Enable it in the game
process by setting `SF2_DEBUG_PORT` to a decimal TCP port from 1 to 65535. An empty or
unset value leaves the listener disabled. Invalid values and bind conflicts produce a
Godot warning and allow the game to start. Bind is exclusively to IPv4 127.0.0.1.
Any local process able to connect can read private state and operate the selected game instance.
Use a dedicated instance, disposable save root and fresh namespace.

## Launch and operate

### Development default

Use `remake/tools/debug_dev.py` after loading the checkout-local private-input configuration
and building the Debug adapter. It requires one explicit target and a fresh disposable run
directory outside Git checkouts. All paths supplied to the launcher resolve from the caller's working directory;
the Godot project resolves from the launcher's checkout. Each run owns its stdout/stderr. The default also owns a fresh save namespace and settings. The launcher stays attached until the game exits.

On Windows, launch defaults to a separate, unshown native desktop: it does not activate a
window on the user's desktop and still runs the real renderer and semantic port. Ordinary
background scenario launches enable `runInBackground` in a per-run settings copy.
Use `--foreground` to launch visibly for visual/input review or focus-pause testing with
the selected preference. This is a
launch choice, not a second scene or a headless substitute for native presentation checks.
Other operating systems currently require `--foreground`.
Drive the hidden instance through the semantic client. Use its ordinary system-menu Quit
to exit cleanly; interrupting the launcher stops only its own process tree. Close failures
and rejected startup remain in the run's logs; an unshown window is not evidence of success.

```powershell
. ./local/private-inputs.ps1
python remake/tools/debug_dev.py --port 42332 --run-dir G:/md-sf2/disposable-saves/debug-save-01 --save $seedCopy
# Or start directly at an authored E2E scenario:
python remake/tools/debug_dev.py --port 42332 --run-dir G:/md-sf2/disposable-saves/debug-scenario-01 --scenario remake/content/authored/harbor-arrival.json
```

`--save` uses the ordinary seed reader and restore validation, without mounting the opening,
title or Witch. Select compatible complete content first; the launcher does not edit the seed
or repair its story state. `--field` and `--battle` select the existing controlled-start files.
Cancelling a direct seed load or leaving its error panel quits that instance; there is no
underlying Witch or gameplay session to return to. Successful restore mounts normally.
Use the [progression table](#preparing-a-progression-start) to choose a meaningful predecessor.
Missing/conflicting targets are rejected before Godot starts. No target is inferred from a
player namespace or latest slot. With explicit `--save-output`, Save publishes to a new
`staged-save.json` beside this run's logs. The seed remains read-only.

Audio defaults to Godot's `Dummy` driver: playback logic and audio observations still run,
but the instance is inaudible. Add `--audio` only for an explicit listening/audio-device check;
this does not override saved volume preferences. Add `--settings` for an existing read-only
settings override when a scenario needs a particular input setup.
The endpoint still uses the same focus and clock rules below. For an effects-only recording,
apply Music 0% in ordinary Settings or seed equivalent editable journey preferences before
adding `--audio`. [Volume and recording steps](../godot/audio.md#effects-only-recording).

Opening/title/Witch validation is a deliberate exception: select `--entry` explicitly, and
add `--audio` only if listening is part of acceptance. For example:

```powershell
python remake/tools/debug_dev.py --port 42332 --run-dir G:/md-sf2/disposable-saves/debug-entry-01 --entry --foreground --audio
```

To inspect a single opening act, select the complete registered opening package with
`SF2_PRIVATE_OPENING_CONTENT` and the ordinary entry selections. Use the same checkout-local
configuration and Debug build as above.

```powershell
python remake/tools/debug_dev.py --opening-act 2 --port 42332 --run-dir G:/md-sf2/disposable-saves/issue2346-rework/storm-act2
python remake/tools/debug_client.py 42332 "observe host" "observe state"
```

Acts 1 through 4 are indexed in their ordinary sequence order. Each launches with the
ordinary opening music and black initial state, plays the selected act and releases to
the title on completion or Skip. `observe host` reports the selected act and outcome.
The default uses the unshown Windows desktop and silent audio driver. For hands-on Shift
and Skip checks, add `--foreground`. For the ordinary Intro1-to-Intro2 comparison, use
`--entry` in a fresh run directory. [Projection evidence](../application/opening-session-lifecycle.md#storm-actor-fade-and-reload-2346).

The existing showcase client includes the full entry journey as part of its recording check;
select `--entry` for that journey. A direct save start is for checks beginning at the restored
state, not for claiming that the skipped entry journey was verified.

For the [showcase journey](../evidence/opening-loop-acceptance.md#pre-voyage-showcase-e2e-2357),
`--entry --journey` seeds editable preferences inside the fresh disposable run namespace.
The settings input is copied and becomes ordinary local preferences for that journey. After
the first game exits, select the same run with `--entry --continue-journey` and a fresh sibling
`--run-dir`. This reuses the saves and editable preferences while keeping each process's logs.
Default background Continue keeps the playback clock running for this launch, even if
the journey's saved Run in Background preference is Off. An explicit `--foreground`
Continue follows the saved preference. The override belongs to the debug launch and
never participates in Settings Apply or save publication.
`--movie --audio` records to the fresh run's `movie.ogv` at 60 fps, with a 54,000-frame
limit. The unshown Windows desktop can record the native renderer and real audio driver.

**Confirmed (#2357 review).** Start an isolated `--entry --journey --foreground`
with format-1 `runInBackground: false`, choose New Game and publish slot 1. Quit, then
use that same journey with fresh run directories for a default Continue and a
`--foreground` Continue. Move actual native focus to a second window on the test
desktop. In the default launch, Continue restored Map3 and Left completed
(56,3) → (55,3) with no frame grant. In the foreground launch, the restored field
paused on focus loss and a submitted Left completed during an explicit
`clock run 120` grant. In both launches Settings displayed Off and was editable.
The original preference and slot files matched byte for byte after both continuations.

### Direct endpoint and export setup

From the checkout, after [development setup](../development-and-verification.md#locked-net-workflow)
and a Debug adapter build, load the usual content selections (including Witch assets
for the title example). For an export, use the [relocated candidate](../windows-local-candidate.md).
In the **game launch shell**, select a fresh non-Git save location; keep it distinct from
the install and any existing player roots. For example, in PowerShell:

```powershell
$env:SF2_SAVE_ROOT = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('Temp/sf2-debug-' + [guid]::NewGuid())
$env:SF2_SAVE_NAMESPACE = 'debug-control'
$env:SF2_DEBUG_PORT = '42332'
& $env:GODOT_BIN --audio-driver Dummy --path remake/game -- --load-save $seedCopy
# For the relocated export, keep the variables and pass the same arguments:
# & $exportExe --audio-driver Dummy -- --load-save $seedCopy
```

From a second shell use Python 3 and the checkout's standalone client. Both builds
serve the same commands while another application owns the foreground.

```powershell
python remake/tools/debug_client.py 42332 hello 'observe host'
# Only for an explicitly selected --entry scenario:
# python remake/tools/debug_title_round_trip.py 42332
```

The round trip starts at opening or title, takes the ordinary skip to the title,
checks title music, enters the Witch greeting, skips it and takes Back to title.
Select complete Witch and opening content. Exit the game to close the listener.
Clear `SF2_DEBUG_PORT` before the next launch to disable it.

## Wire contract (version 2)

One client at a time sends printable ASCII lines of at most 64 bytes before LF,
including an optional final CR. Read one UTF-8 JSON line (at most 1 MiB including
LF) per request. The maintained client checks `hello` for version 2 on each
connection. The worker enforces the existing 60-second exchange deadline, awaits
each main-thread reply and never resubmits a request. A framing error closes the
connection. A successful reply contains `ok`, the main-thread `frame` and `data`.
An error has `ok:false` and an `error` code.

| Request | Preconditions and result |
| --- | --- |
| `hello` | Version, PID, adapter module ID and exported-build flag. |
| `observe host` | Focus, effective `runInBackground`, `backgroundClockForced` for the current debug launch, remaining authorized frames, physical gameplay hold and release barrier, opening, entry and system-menu selection. |
| `observe state`, `observe scene`, `observe audio` | Live opening/field/battle projection, battle scene or gameplay audio. An absent view returns null. Large projections are limited to the reply budget. |
| `observe slots`, `observe name`, `observe settings` | Read the matching modal projection. |
| `observe layout Y` | At field input, read one existing layout region of 64×4 cells. The row is a multiple of four from 0 to 60. |
| `name draft NAME` | At the New Game name prompt, update one word of up to seven valid ASCII characters. Read the draft, then use the ordinary Confirm and Start confirmation. |
| `opening skip`, `witch skip` | Finish the active opening or the Witch's initial greeting using its existing skip. |
| `menu confirm`, `menu back`, `menu up/down/left/right` | Route one choice to the topmost entry or slot page or system menu. Use the individual direction, e.g. `menu down`. Title Confirm starts the Witch. Continue uses the selected Witch row, reading, slot checks and farewell. A name page accepts Confirm with its validated current draft or Back when offered. Settings supports Back outside binding capture/review, directional focus in Root/Display, and ordinary activation of Display, Language, Graphics and Apply. A covered field routes these actions to Member/Prepare, the field menu or Church when one owns input. |
| `menu open` | Open the ordinary system menu from settled field input or the base battle movement decision. Save and Load use the ordinary menu, slot store and validation. |
| `field up/right/down/left`, `field interact`, `field back` | At ordinary exploration input, request one move, the current interaction/search or the ordinary field menu. Movement arms no direction hold. Sample `entities`, `wait` and `canWaitAtInput` to observe travel or obstruction. |
| `story confirm/back/up/right/down/left` | Route a single reading or choice action to the active field wait. Its owner handles reveal, page, delay, token and acknowledgement rules. Check `token`, `wait`, text and choice projections for completion. |
| `battle confirm/back/up/right/down/left`, `battle attack`, `battle stay` | At the current player decision, use a single selection/navigation or an offered main action. During a reading boundary use Confirm. Back can return from inspection. Read actor, stage, preview, decision and receipts. |
| `clock run N` | While the background preference is Off and focus pauses playback, schedule 1–120 ordinary process frames. `data.status` is `scheduled`. `observe host` reports `runFramesRemaining`; zero ends the grant. Recovery and quit overlays refuse the request. Existing menu, loading and story waits keep their own rules. |

A semantic success (`data.status = dispatched`) means the named controller received
one action in its live context. Selection, reveal or starting an asynchronous save
can each be the immediate effect. Inspect the owner's view for completion, blocked
travel, slot errors or a new reading boundary. A handler can leave a selected row
or position unchanged. Separate connections can observe a dispatched action.
If a reply is lost, inspect state on reconnect before sending another action.
Resending the line is a new user operation. The port has no held input.

`action-unavailable` covers a missing or covered owner, a blocked action or a verb
outside that context. `window-modal` identifies window recovery/quit. Unsupported
top-level commands return `unknown-command`. Invalid frame counts, overlapping runs
and attempts to grant already-running playback return `frame-budget`,
`run-in-progress` and `clock-not-focus-paused` respectively.

Dispatch runs between process callbacks. A field or battle operation can use its
active presentation controller synchronously while passive playback is paused.
Its normal pause policy is restored before child clocks run. For a move, queued
text delivery or asynchronous menu default, authorize playback separately if needed.
A clock grant enables the existing clocks and audio for its bounded frame count,
including ordinary background world activity during that interval. It can stop
partway through a movement or wait. Inspect state before granting further time.
Connection, observation and disconnect grant zero frames. The frame budget and a
dispatched token-bound reading intent survive disconnect and physical focus loss.

## Preparing a progression start

Use the [four campaign stops and dock example](../application/pre-voyage-campaign.md#proposed-non-battle-debug-starts)
to select the predecessor and expected landing. Prefer a short ordinary transition from a
registered, provenance-checked predecessor in a fresh disposable namespace. For the docks, the
[observed post-tower castle visit](../application/pre-voyage-campaign.md#castle-to-quake-handoff-from-a-player-save-2167)
already has royal followers. Check its setup, flags, party and live bindings before walking to
Map4 and taking its east exit. Copy only the registered selection into disposable storage for
ordinary Continue. A loadable slot by itself establishes neither its story lineage nor its cast.

The [showcase starts](../application/pre-voyage-campaign.md#showcase-starts-and-continuity)
name two campaign scene definitions and the independent records. For a scene definition, use
`debug_dev.py --campaign-field <path>` with the same private package selections as New Game.
The start uses the existing `formatVersion`, `controlledBoundary` and `start` shape with an
explicit `textSettings` object to admit the campaign's field text services. Leave program,
display and entity phase overrides absent so the selected map loads normally. Joined and active
membership flags select the initial roster from the ordinary source-initialized party. The
controlled battle origin preserves diagnostic persistence eligibility and routes battles through
their normal field entry. [Scene commands](../evidence/opening-loop-acceptance.md#slade-join-and-castle-quake-2364).

For the read-only records, run the [semantic pilot](../evidence/opening-loop-acceptance.md#pilot-and-recording-boundary)
on one fresh instance per record. The Galam check consumes a restored v6 visit through ordinary
Continue and uses the south exits on Map16 and Map18.

For repeatable development checks without a suitable slot, the existing pre-session
[controlled exploration start](../../game/probes/PreVoyageLocalizationObservation.cs) admits the
selected world and constructs a fresh exploration input. Choose the predecessor map, player,
tile, facing, speed, explicit story/membership flags and compatible party inputs. The field
probe derives active IDs from flags 32–61. Its default party, gold, initial egress and seeds are
controlled values. A carried-resource or natural-reach claim needs an ordinary predecessor.
Omit the explicit program override so the [selected setup and its OnLoad](../application/session-and-programs.md#exploration-start-setup)
own initial population, aliases, working layout, camera, continuation and scene effects through
normal loading and playback. Observe first-entry latches after their writers run. On a settled
[v6 Continue](../application/opening-session-lifecycle.md#unified-settled-field-snapshots),
restore the captured visit without replaying init. Editing flags or aliases in a snapshot loses
the connection between the chosen setup and that captured visit.

**Proposed follow-up slices**

1. Prepare one named dock predecessor using the existing controlled start and registered content
   in development. Drive the exit with v2 field/story commands. Verify the selected setup,
   actual roster, royal bindings, settled control and errors. Vary Kiwi membership once to check
   that the royal cast is independent. Save only at an eligible settled visit and cold Continue
   the disposable slot to compare bindings, layout and one-time flags. Add another named start
   only when its distinct transition needs checking. The wire vocabulary needs no setter.
2. Check natural F74 portraits separately in the specific relocated Release, starting with the
   registered naturally reached castle predecessor compatible with that export. Record the
   handshake/build and slot provenance, use ordinary transitions through the first quake, then
   inspect the dock at reading waits. At 1009/1013 expect Astral portrait 31, across 1015/1018/1019 expect the
   Minister's panel hidden with null texture and the King's portrait 32 when each speaks. Compare logical speaker and portrait
   request with the drawn ID and actual panel visibility/texture. Exercise No before Yes, then
   check F74 clears in boarding, F640 clears on the Map73 request, and F700 is set at first
   Map14 (40,8) Left control. Use bounded clock grants when paused. If identity, bindings or
   projections disagree, retain that boundary and stop. A controlled entry or an observation
   in another build cannot close this export-specific **Unknown**.

## Native acceptance and reproduction

**Confirmed (#2354):** the development launcher mounted authored `harbor-arrival.json` and
the registered `handoffs/issue2245/map16/slot2.json` seed with `issue2315/aqua-v1` content at
settled exploration input. Both reported no entry host and inactive opening over the v2 port.
Use the launch commands above with a settings override containing `formatVersion: 1` and
`runInBackground: true`, then read `observe host` and `observe state`. An invalid `{}` seed
displayed the ordinary restore error; `menu back` exited with code 0. An explicit `--entry`
with opening `issue2324/opening-hd-v1` mounted the opening. Missing and conflicting targets
returned code 2 before launch. The default launch command selected `--audio-driver Dummy`.
Background launches retained the same user-desktop foreground window for the authored,
valid seed, invalid seed and entry checks. A separate default-settings authored run accepted
`field down`, advanced revision 10 to 62 and exited through `menu open`, two `menu down`,
`menu confirm`, then the ordinary Quit confirmation (exit 0). The launch settings reported
`runInBackground: true`. Minimize-without-activation alone did not work reliably with this
Godot installation; the separate native desktop replaced it. One initial background seed
attempt exited 0 before the client's ready condition without a reported native error;
its cause remains **Unknown**. Two subsequent isolated seed launches reached settled Map16.
Successful observation instances were stopped by the operator after readback, so those runs
do not claim graceful exit or a full campaign replay. Real-device listening remains **Unknown**;
`--audio` omits the Dummy override without changing playback logic or preferences.

The first local check supplied incorrectly capitalized settings keys and failed before port
startup; corrected camelCase settings passed. Its client also lacked a deadline on successful
connections that were not ready; the local observation was corrected before the completed runs.

**Confirmed:** product commit `3077da05978fa7e415d41534f90393a284016561` served v2
from the development main scene and a freshly built, relocated Windows x86_64 Release.
The Release build used the [registered packaging recipe](../windows-local-candidate.md#build-and-run),
196 inputs (408,278,969 bytes) and a clean source tree. Build time was 50.29 seconds,
67.30 seconds including ZIP. The relocated process ran from a separate cwd with
OS-only PATH and developer content/SDK variables cleared. Each handshake matched the
native PID, module ID and build kind.

Use fresh disposable namespaces for each run. Select Off or On in the ordinary
format-1 preferences (`runInBackground`). Verify the game once owns the foreground,
then move focus to a separate window and minimize the game. Require the native
foreground PID to differ from the handshake PID and `observe host.focused` to be
false. Repeat these three short cases in each build with each preference.

| Case | Reproduction and actual result |
| --- | --- |
| Title and connection lifetime | Run the title round trip, take `menu confirm`, `witch skip`, wait for slot discovery, then select Continue. Acknowledge text 225. The empty disposable history opened `continue-slots` and Back returned to Witch. At menu index 0 send one complete `menu down` line and disconnect before reading its reply. Reconnect and read twice. Index 1 persisted. Restore focus and press ordinary Down once. Index 2 persisted with both gameplay hold and release barrier false. |
| Field | Launch with `-- --authored-package <absolute path to remake/content/authored/harbor-arrival.json>`. Read state twice about 0.4 s apart. With Off the Release tick held at 53 with equal revision and random seed copy. With On it advanced 96→124. From quay (1,1), `field down` settled at (1,2). With Off grant 40 frames and wait for the budget to expire. A single `field up` submitted before a focus loss settled back at (1,1). Open the system menu, confirm Settings, return twice. Field movement under the menu was refused. Face the ferryman with `field right`, then `field interact`. Two separate `story confirm` requests took the dialogue to a choice and the selected choice to the next dialogue with flags 10,12,13,14,15. Grant bounded playback between waits with Off. |
| Battle | Use the same authored option with `early-ranged.json`. Allow ordinary playback to the lancer's Movement decision (60 frames with Off). `field interact` was refused. `battle confirm` reached ActionChoice at revision 4. `battle stay` and ordinary playback reached the archer's Movement decision at revision 6. |

**Confirmed:** the minimized Release also completed New Game and cold Continue with
Off. Select the fresh Witch New Game row, confirm the new slot, confirm the valid
default name, then choose Start in the ordinary confirmation. After Map3 attached,
restart against that isolated namespace. Continue selected the saved row. Confirm
the slot and farewell texts 226 and 224. The second process projected Map3 at
revision 4 with no field failure while its window remained minimized.

Focus preflights that relied on launch style, visibility or an unverified native
click failed. At startup Godot could still report focus with a different Windows
foreground PID. The verified transfer above established the background condition.
An early cold Continue reached Map3 but failed its final focus assertion. A separate
attempt exhausted one upfront clock grant before asynchronous slot discovery was
published. The title client now spaces two-frame grants during slot discovery with a
120-frame total and a 30-second deadline. An earlier client returned with its run
still active, so a second grant correctly returned `run-in-progress`. The current
client waits for the budget to expire. These controls and the successful cold run
use the same Release assembly.

**Unknown:** passive startup behavior for a never-activated hidden window, natural
voyage/dock state and unexercised battle or story contexts. The previous endpoint's
focused keyboard, disabled-listener, bind-conflict and framing results, including
the original CS0234 build failure, are preserved with the
[version-1 owner](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/38f2b7bafb45c92e47733ebb2d3b7db01b3abb2f/remake/docs/verification/debug-port.md#runtime-acceptance).

## Migration and specialized consumers

Version 1 synthesized mapped keyboard edges and required focus for key-down.
Version 2 retires `down`, `up` and the client's `tap` shorthand. The supported
vocabulary is independent of installed bindings and OS foreground ownership.
Device remapping, typed names, pointer gestures and specialized scenario setup
retain their ordinary or scenario-specific owners.

| Remaining specialized consumer | Owner and purpose |
| --- | --- |
| `engine_opening_startup_observation.gd`, `engine_opening_hd_observation.gd` | [Presentation assets](../content/presentation-assets.md), including focus manipulation, retired callbacks and HD evidence |
| `host_ui_observation.gd`, settings, save and pointer probes | [Host verification](host.md), finite layout, pointer, persistence and replacement cases |
| `engine_exploration*_observation.gd` and field/story subclasses | [Field programs](field-programs.md), scenario inputs, signal timing and route assertions |
| `engine_battle*_observation.gd` and combat-specific probes | [Verification routing](../development-and-verification.md), battle projections and scenario assertions |
| `engine_h4_observation.gd` and `ObservationCapture` | Per-event/resource evidence capture with its own queue, writer and completion contract |
| Windows PSS live inspection | [Existing-session inspection](../../../docs/operations/godot-live-inspection.md), bounded read-only fallback for an already running instance |

Use `git ls-files remake/game/probes` for the full scenario inventory.
