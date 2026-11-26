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

## Preparing a progression start

Use the [four campaign stops and dock example](../application/pre-voyage-campaign.md#proposed-non-battle-debug-starts)
to select the predecessor and expected landing. Prefer a short ordinary transition from a
registered, provenance-checked predecessor in a fresh disposable namespace. For the docks, the
[observed post-tower castle visit](../application/pre-voyage-campaign.md#castle-to-quake-handoff-from-a-player-save-2167)
already has royal followers. Check its setup, flags, party and live bindings before walking to
Map4 and taking its east exit. Copy only the registered selection into disposable storage for
ordinary Continue. A loadable slot by itself establishes neither its story lineage nor its cast.

The [showcase starts](../application/pre-voyage-campaign.md#showcase-starts-and-continuity)
name the two independently executable records. Run the [semantic pilot](../evidence/opening-loop-acceptance.md#pilot-and-recording-boundary)
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
