# Local game debug port

Standard rigor applies to the adapter, ordinary input and Windows export boundary.
The ordinary Godot main scene owns a C# endpoint in both builds. Enable it in the game
process by setting `SF2_DEBUG_PORT` to a decimal TCP port from 1 to 65535. An empty or
unset value leaves the listener disabled. Invalid values and bind conflicts produce a
Godot warning and allow the game to start. Bind is exclusively to IPv4 127.0.0.1.
Any local process able to connect can read private state and send player input.
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

From a second shell use Python 3 and the checkout's standalone client. The game
itself requires neither Python nor a client checkout. Keep the game window focused
during input. For interactive use, start a short delayed command and return focus to
the game before the first press. Reads work with the game in the background.

```powershell
python remake/tools/debug_client.py 42332 hello 'observe host'
Start-Sleep 3
python remake/tools/debug_client.py 42332 'tap E' 'observe host'
```

For the representative opening skip, title music and Witch Back control, start at
opening or title with the default key bindings and complete Witch entry content:

```powershell
python remake/tools/debug_title_round_trip.py 42332
```

Closing the client disconnects it. The next client takes ownership after key release.
Clear `SF2_DEBUG_PORT` before the **next launch** to disable it. Exiting the game closes
the listener and releases any remaining synthetic keys.

## Wire contract (version 1)

Send one printable ASCII command followed by LF over TCP. A CR immediately before
LF is accepted. Maximum line content is 64 bytes including optional CR. Read one
UTF-8 JSON line per request. Replies including LF are limited to 1 MiB, the adapter's
existing single observation record budget. The client must read each reply before
sending the next command. Complete a version-1 `hello` before submitting input. This
keeps a waiting TCP client from sending keystrokes before the game serves it. The
example client performs this handshake on connect. A connection has 60 seconds to finish a command and read
its reply. The listener serves one client at a time with a TCP backlog of one.

| Request | Successful `data` |
| --- | --- |
| `hello` | Protocol version, OS process ID, adapter assembly module ID and exported-build flag |
| `observe host` | Window/focus, opening and entry projections (including their audio), synthetic held keys, mapped held/release barrier and system-menu visibility |
| `observe state` | Active opening, exploration or battle view's existing `ReadObservationJson` object |
| `observe scene`, `observe audio` | Existing battle-scene or gameplay-session audio projection |
| `observe settings`, `observe slots`, `observe name` | Present settings, save-slot or name-entry view |
| `down Enter`, `up Enter` | One ordinary Godot keyboard edge using an exact C# Godot Key name such as `E`, `Right` or `Escape` |

Success returns `{"ok":true,"frame":123,"data":...}`. An absent view returns JSON null.
The frame identifies the main-thread sample or dispatch. An input receipt means Godot
flushed the event through ordinary input handling; sample the relevant view to see
whether the current game/UI accepted it. `tap KEY` is client shorthand for a completed
down and up on the same connection. No action is automatically retried. If a
connection drops after sending a command, reconnect and inspect state before acting.

An error returns `{"ok":false,"error":"..."}`. Unknown commands/keys and duplicate
downs or unowned ups leave held state alone. A framing error closes the connection.
The worker handles sockets, framing and response bytes. One pending request crosses
to the game thread, which handles at most one per frame, including disconnect release.
Readouts and input run against the current host on that thread. Keep detailed state
samples sparse. A large projection can take a frame to serialize. The endpoint
performs no periodic capture.

Key-down requires focus. Release and observation remain available after focus loss.
Debug input follows the current key bindings, barriers, UI, pause and save rules.
Use a single input operator while holding keys; the OS keyboard and synthetic keys
feed the same Godot input map. The background-continuation preference belongs to #2331.

## Migration boundary

For general observation, use `observe state` in place of finding a view and calling
`ReadObservationJson` from GDScript. Use `down`/`up` in place of a probe's
`Input.parse_input_event` keyboard helper. The title round-trip client consumes the
existing opening/entry/audio readouts and verifies both a held Start and its release.
The production assembly includes the endpoint in ExportRelease. No script loader,
editor API, scene instantiation by the client or dynamic method lookup is involved.

| Remaining specialized consumer | Owner and purpose |
| --- | --- |
| `engine_opening_startup_observation.gd`, `engine_opening_hd_observation.gd` | [Presentation assets](../content/presentation-assets.md), including focus manipulation, retired callbacks and HD evidence |
| `host_ui_observation.gd`, settings, save and pointer probes | [Host verification](host.md), finite layout, pointer, persistence and replacement cases |
| `engine_exploration*_observation.gd` and field/story subclasses | [Field programs](field-programs.md), scenario inputs, signal timing and route assertions |
| `engine_battle*_observation.gd` and combat-specific probes | [Verification routing](../development-and-verification.md), battle projections and scenario assertions |
| `engine_h4_observation.gd` and `ObservationCapture` | Per-event/resource evidence capture with its own queue, writer and completion contract |
| Windows PSS live inspection | [Existing-session inspection](../../../docs/operations/godot-live-inspection.md), bounded read-only fallback for an already running instance |

Use `git ls-files remake/game/probes` for the full scenario inventory. The small network
protocol exposes selected readouts and keyboard edges. It has no file, save-edit,
script execution, arbitrary node-call or system-management operation.
