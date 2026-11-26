# Local game debug port

Standard rigor applies to the adapter, ordinary input and Windows export boundary.
The ordinary Godot main scene owns a C# endpoint in both builds. Enable it in the game
process by setting `SF2_DEBUG_PORT` to a decimal TCP port from 1 to 65535. An empty or
unset value leaves the listener disabled. Invalid values and bind conflicts produce a
Godot warning and allow the game to start. Bind is exclusively to IPv4 127.0.0.1.
Any local process able to connect can read private state and operate the selected game instance.
Use a dedicated instance, disposable save root and fresh namespace.

## Launch and operate

From the checkout, after [development setup](../development-and-verification.md#locked-net-workflow)
and a Debug adapter build, load the usual content selections (including Witch assets
for the title example). For an export, use the [relocated candidate](../windows-local-candidate.md).
In the **game launch shell**, select a fresh non-Git save location; keep it distinct from
the install and any existing player roots. For example, in PowerShell:

```powershell
$env:SF2_SAVE_ROOT = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('Temp/sf2-debug-' + [guid]::NewGuid())
$env:SF2_SAVE_NAMESPACE = 'debug-control'
$env:SF2_DEBUG_PORT = '42332'
& $env:GODOT_BIN --path remake/game
# Or launch the relocated Sf2Remake.exe with these same three variables.
```

From a second shell use Python 3 and the checkout's standalone client. Both builds
serve the same commands while another application owns the foreground.

```powershell
python remake/tools/debug_client.py 42332 hello 'observe host'
python remake/tools/debug_title_round_trip.py 42332
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
| `observe host` | Focus, background preference, remaining authorized frames, physical gameplay hold and release barrier, opening, entry and system-menu selection. |
| `observe state`, `observe scene`, `observe audio` | Live opening/field/battle projection, battle scene or gameplay audio. An absent view returns null. Large projections are limited to the reply budget. |
| `observe slots`, `observe name`, `observe settings` | Read the matching modal projection. |
| `opening skip`, `witch skip` | Finish the active opening or the Witch's initial greeting using its existing skip. |
| `menu confirm`, `menu back`, `menu up/down/left/right` | Route one choice to the topmost entry or slot page or system menu. Use the individual direction, e.g. `menu down`. Title Confirm starts the Witch. Continue uses the selected Witch row, reading, slot checks and farewell. A name page accepts Confirm with its validated current draft or Back when offered. Settings supports Back outside binding capture/review. |
| `menu open` | Open the ordinary system menu from settled field input or the base battle movement decision. Save and Load use the ordinary menu, slot store and validation. |
| `field up/right/down/left`, `field interact` | At ordinary exploration input, request one move in the named direction or the current interaction/search. Movement arms no direction hold. Sample `entities`, `wait` and `canWaitAtInput` to observe travel or obstruction. |
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
