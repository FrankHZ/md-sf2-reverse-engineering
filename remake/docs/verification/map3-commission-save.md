# Map3 commission Save and Continue

The [High-rigor persistence agreement](../application/opening-session-lifecycle.md#implemented-savepoint-boundary)
owns this field recovery boundary. This observation concerns remake savepoints, not original SRAM.
Use the existing private content/tool selections, owning isolated worktree and a fresh `issue1711-*`
save namespace under the configured external save root. Preserve player saves, preferences, instances
and windows. The dedicated `engine_map3_commission_save_observation.gd` runs with `--headless` and
dummy audio; it never requests native focus or takes screenshots.

The finite consumer cases are `baseline`, `route`, `continue`, `church` and `legacy`:

1. At a declared controlled Map3 F603/F609/F610 `(32,13)` start, the accepted baseline rejects
   `save-location-unavailable` and disables system Save. This is the before-fix refusal witness.
2. `route` starts at an explicitly controlled pre-exorcism Map20 royal-room boundary, before any
   session exists. It then uses ordinary input through the zone event, exorcism, F609 transition,
   commission/F610, castle exit and Map3 church approach. This is a bounded consumer continuation,
   not a replay of New Game/Battle01 or proof of natural original reach. It opens system Save,
   asserts the slot and Cancel-first confirmation, and chooses Save and Quit. Require publication
   of the actual slot and an exited process with code 0; its pre-quit intent report alone is insufficient.
3. Start a genuinely new process with the same namespace for `continue`. Use Witch Continue and
   compare all captured progress to the saved payload, allowing only declared map-entry flag resets.
   Check the church anchor, F609/F610, selected NPC population, two followers and party. Move and
   reach Jaha's actual zone/join, confirming F3/F35 without replaying the commission.
4. A separate fresh `church` process continues the same retained slot, talks to the actual priest,
   selects Record, waits for the caller return, and writes slot2 through the common save entry.
5. `legacy` uses a byte-identical disposable copy of a pre-fix revision4 default-Map3 record.
   Continue and movement must work without rewriting that source copy. Older gameplay revisions
   still require their own compatible content; this slice adds no cross-revision migration.

Run with the locked host environment and existing editor; supply `SF2_HOST_UI_CASE`, the absolute
`SF2_HOST_UI_OUTPUT`, and, for controlled-start cases, `SF2_HOST_UI_START` under the owning worktree's
ignored `local/`. The observer's startup file uses the existing pre-session localization admission.
Use existing immutable private selections; no active-session setters or save-payload edits are allowed.
The application host observation exposes availability, detached progress and bounded entity work;
the slot view supplies actual panel states. Store terminal reports, stdout/stderr, process identity,
exit status, source-copy provenance and failed attempts in the ignored Issue handoff.

The controlled pre-exorcism start is the following project-authored consumer input, not a captured
original-game checkpoint. Save it under the owning worktree's ignored `local/`:

```json
{"start":{"map":"map-20","player":"entity-0","position":{"x":25,"y":50},"facing":1,"speed":32,"flags":[0,1,2,32,33,34,66,80,501,600,601,602,603,604,605,606,607,608]}}
```

After loading the existing host environment and selecting a fresh external save namespace, launch
from the repository root. Supply the actual local input/output paths; do not reuse an old report:

```powershell
$env:SF2_HOST_UI_CASE = 'route'
$env:SF2_HOST_UI_START = (Resolve-Path -LiteralPath 'local/issue1711/route-start.json').Path
$run = (New-Item -ItemType Directory -Path "local/issue1711/$([guid]::NewGuid())").FullName
$env:SF2_HOST_UI_OUTPUT = Join-Path $run 'native.json'
$env:APPDATA = (New-Item -ItemType Directory -Path "$run/appdata").FullName
$env:LOCALAPPDATA = (New-Item -ItemType Directory -Path "$run/localappdata").FullName
$env:TEMP = (New-Item -ItemType Directory -Path "$run/temp").FullName
$env:TMP = $env:TEMP
$native = $env:GODOT_BIN -replace '_console\.exe$', '.exe'
$argsForGame = @('--headless', '--verbose', '--path', (Resolve-Path remake/game).Path,
    '--audio-driver', 'Dummy', '--max-fps', '120', '--resolution', '1920x1080',
    '--script', 'res://probes/engine_map3_commission_save_observation.gd')
$p = Start-Process -FilePath $native -ArgumentList ($argsForGame | ForEach-Object { '"' + $_ + '"' }) `
    -WindowStyle Hidden -RedirectStandardOutput "$run/stdout.log" -RedirectStandardError "$run/stderr.log" -PassThru
$null = $p.Handle
$timedOut = !$p.WaitForExit(190000)
if ($timedOut) { $p.Kill(); $p.WaitForExit() }
@{ pid = $p.Id; nativeExit = $p.ExitCode; timedOut = $timedOut } | ConvertTo-Json |
    Set-Content -LiteralPath "$run/process.json"
if ($timedOut -or $p.ExitCode -ne 0) { throw "See the native process result in $run" }
```

Repeat the launch in a new process with case `continue` or `church` and a fresh output file, retaining
the same save namespace. For the before-fix `baseline` case, change the controlled start to Map3
`(32,13)`, Up, and append F609/F610; build the accepted pre-fix source. Keep that binary's exact Git
identity separate from the fixed binary. The `legacy` case needs an unedited copied default-Map3
slot1 from before this change, compatible with the selected world.

Affected engine behavior can be checked directly with the existing protected SDK launch:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore `
  --filter 'FullyQualifiedName~Map3SaveSetupTests|FullyQualifiedName~OpeningRestoreTests|FullyQualifiedName~OpeningSaveTests|FullyQualifiedName~PreVoyageSaveTests'
uv run sf2 verify adapter
```

The private family test must select the admitted world and six battle/static inputs. Public tests
use authored changed states and reject an unknown overriding setup. Follow the owner's recovery
authoring rule when story/content changes alter persistent effects, population or initialization;
the witness is not an all-maps comparator or a per-dialogue test requirement.

## Chinese Continue shutdown (#1716)

**Confirmed:** The first Chinese `continue` run on `182ea66832db5b98daaf8c9a39bcfb33c2557432`
wrote a passing 13-check report. At 2026-11-12 08:31:02 UTC, Windows Application events 1026
(.NET Runtime) and 1000 (Application Error) recorded termination of the native Godot process.
The matching local minidump for PID 18416 records a read at address zero, exception `0xC0000005`
in `coreclr.dll` at offset `0x356d4f`. ClrMD with the installed .NET 10.0.8 DAC identifies thread
15168 as the finalizer thread. Its managed frames, from caller to callee, are `GC.RunFinalizers`,
`GodotObject.Finalize`, `GodotObject.Dispose(Boolean)` and `godotsharp_internal_object_get_associated_gchandle`.
The concrete wrapper type and allocating owner remain **Unknown**. The first wrapper omitted the native
exit code. Its same-build rerun passed 13 checks and returned 0. [Incident #1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

The bounded comparison reused the original Debug assemblies and imported resources, Godot
`4.7.2.stable.mono.official.ed1daf0bf`, Dummy audio, a read-only Chinese settings override and the
disposable #1711 commission slot held open for shared reads only. Each row started the native
executable directly with its own logs, temporary files and application data.

| Exit path | Observed boundary | Native exit | Verbose ObjectDB output |
| --- | --- | ---: | --- |
| Headless ordinary title, no script | Engine frame limit (`--quit-after 120`) | 0 | None |
| Headless `continue` probe | 13 checks through Jaha, then probe `SceneTree.quit()` | 0 | One WAV/playback pair |
| Same probe and route, system menu Quit | Wait for Resume, select Quit, observe Cancel first, select 退出 and confirm through host input. 17 checks | 0 | One WAV/playback pair |
| Ordinary title, no script, isolated Windows desktop | Native window received `WM_CLOSE` | 0 | One WAV/playback pair |
| Ordinary Witch Continue, no script, isolated Windows desktop | Field movement input, then `WM_CLOSE`. An external read-only snapshot found Map3 and `confirm-quit` with selection 1 before Enter | 0 | One WAV/playback pair |

For the windowed control, start ordinary Main on a separate, non-displayed desktop. Deliver input
only to the window owned by that PID. After the terminating key-down, wait on the **process handle**.
A first driver attempted key-up after the window disappeared and forced termination with code 124.
A first system-menu observer sent Up before the menu settled and exited 1. Both failures remain in
the local investigation. The native receipts, stdout, stderr and snapshot are under ignored
`local/issue1716/`. Keep the incident dump private.

The crash did not recur in these comparisons. Verbose output identifies each warning pair as
`AudioStreamWAV` and `AudioStreamPlaybackWAV`, each with reference count 1.
[Audio lifetime observation](host.md#audio-resource-lifetime-observation)

### Later finalizer faults and ordinary Quit

The Map3 capture/restore fix shipped in [#1714](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1714).
The [first shutdown investigation](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1741)
separately established native-exit capture and the windowed controls above.

**Confirmed:** Three [East Shrine gate observations](../application/east-shrine-panel.md) on
2026-11-13 have the same four managed frames as the Map3 crash. The English
`admitted14` and Chinese `natural03` observers ended with “no battle”. The English
`desert-release` observer captured the expected `battle-entry-unavailable` result before the
fault. For PIDs 22864, 48040 and 38492 respectively, Windows events 1026/1000 and the matching
minidumps identify a read at zero at the same CoreCLR 10.0.8 offset. ClrMD 4.2.751201 with the
matching DAC places the fault on each finalizer thread. Each dump contains the stack root for
`GodotObject.Finalize()` and omits the target object's memory page. The exception code is
a Windows exception record. The gate handoff reports 0xC0000005, but there is no separate
per-sample process-handle receipt in those three directories. These cases extend the signature
to English and to a different map and outcome.

The [Battle15 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1831)
records ten cold observer runs with exit 0. Eight console logs warn of 2–10 live ObjectDB
instances. Those non-verbose logs give counts only. The observer queues the host for deletion,
waits six frames and calls `SceneTree.quit()`.

**Confirmed:** The following independent runs reused the #1814 Debug build
(adapter product revision `e03be159`, game and engine sources identical to accepted `0f274ac2`),
registered Godot 4.7.2 .NET and Dummy audio. Each loaded a fresh copy of its seed and left it
byte-identical. The three Map50 runs used the same post-Panel slot, package and Chinese settings.

| Entry and exit owner | Evidence before exit | Native exit | Verbose ObjectDB output |
| --- | --- | ---: | --- |
| Map50, headless observer teardown | Witch Continue reached a settled field with F152. Observer queued the host for deletion, waited six frames and quit. | 0 | One WAV/playback pair |
| Map50, headless host menu | Same Continue. F10 → Quit showed 取消 selected first. The observer selected 退出 and sent Enter. The game handled exit. | 0 | None |
| Map50, ordinary Main on a non-displayed Windows desktop, no script | PID-targeted keys drove Witch Continue. After `WM_CLOSE` and Down, a read-only process snapshot found Map50 and `confirm-quit` selection 1. Enter ended the native process. | 0 | One WAV/playback pair |
| Map61, headless gate observer | Continued the pre-battle elder slot with the retained 14-battle package, opened the gate and captured the unavailable Battle15 receipt before observer teardown. | 0 | One WAV/playback pair |

To repeat in the prepared checkout, run each line serially with a new sample name. The private
runner selects the registered native executable, isolates saves, settings, logs and temporary files,
and waits on the process handle. The desktop case requires a supported client size. An initial
1280×720 launch reached size recovery and exited 0 without establishing Map50. The first observer
launcher's slot comparison also failed to open the path. Direct binary comparison of the same
retained slot then passed. The failed checks remain with their samples.

```powershell
./local/issue1716/run.ps1 probe   map50-probe-new
./local/issue1716/run.ps1 menu    map50-menu-new
./local/issue1716/run.ps1 desktop map50-window-new
./local/issue1716/run.ps1 gate    map61-gate-new
```

**Unknown:** The concrete wrapper and its allocating owner, whether it was either audio object,
and whether an ordinary player exit can fault this way. Those Map50/61 controls reproduced the
gameplay boundaries and the WAV/playback warning. All four processes exited normally. The
[backend and SessionAudio comparisons](host.md#audio-resource-lifetime-observation) explain how a
WAV/playback pair can survive immediate audio shutdown. No sample identifies a production object
requiring a lifecycle change for the access violation.

**Confirmed:** Independent [Creed restoration](../application/creed-restoration.md) acceptance on
frozen `a56fe51c1f57ce7e5e3f13e7932fc7003bf4a6e6` reopened #1716. English shrine cold Continue,
its repeat from the same `basement-en` seed, and the post-round-trip cold case each completed
Continue, Save and movement, wrote `END`, then exited `-1073741819` (`0xC0000005`). The stack
again reaches `associated_gchandle` through `GodotObject.Dispose`, `Finalize` and `GC.RunFinalizers`.
The observer explicitly deletes its host, waits five process frames and quits. English restored-mansion
and all three Chinese cold controls exited 0. Complete decoded save-data comparisons passed separately.

Bounded controls reused that frozen runtime and the same English shrine seed in fresh namespaces:

| Exit owner | Retained observation | Native exit |
| --- | --- | ---: |
| Ordinary Main, no script, hidden separate desktop | PID-targeted input performed Continue → Save → close panel → Left → settled field → system-menu Quit. Read-only managed fields/MVIDs confirmed Map34, default Cancel0, then Quit1 before Enter. | 0 |
| Same cold observer, only its terminal action changed | The original route and `END` remained; actual menu Quit replaced explicit host deletion and quit. Cancel0 and Quit1 were observed before confirmation. The two-instance ObjectDB warning remains. | 0 |
| Independent repeat of the same menu observer, `root-menu1117` | Reached `END` and `MENU QUIT confirmation`, PID4248; no timeout. The original explicit host deletion was absent. | -1073741819 |
| Independent ordinary Main preparation, `root-desktop1117` | First `loaded` read-only snapshot failed with `Object is null`; controller status1, PID19852. Quit was not reached or confirmed. | 124, controller-forced; no player-exit result |

Saved data objects from the successful controls and root menu repeat matched the input completely.
The root desktop's retained copy also matched, without establishing that Save was reached.
An initial desktop-controller assertion wrongly
required Y28 after Left; actual tile(25,29) matched both root failure receipts. That preparation failure
ended with controller-forced124 before Quit, and is retained separately from game exits. The successful
repeat changed only that assertion. The private operator evidence under `local/runs/diagnostics/native-quit-v1/ordinary-controls`
contains the exact scripts, exit-only diff, native receipts, read-only snapshots and reproduction report;
the original failing driver, all three earlier root failures and both new root controls remain intact.
The root desktop preparation failure supplies no independent ordinary-Quit acceptance; the earlier
successful no-script control remains valid for its single observation. No further preparation retry
was undertaken. No product patch, GC workaround, lifecycle sleep, log suppression, screenshot or new
dump was used.

**Confirmed:** The independent menu-observer failure withdraws the earlier inference associating the
fault with explicit observer teardown. Product menu Quit does not exclude the failure. **Unknown:** the
faulting wrapper, exact cause and intermittent ordinary-player Quit risk. The successful controls
do not resolve [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716), which remains
open and linked to [long-term maintenance #1881](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1881).
Subsequent campaign observers use the existing product menu Quit entry to accept the player's exit
path, while retaining the original driver
for failure reproduction. Report terminal gameplay and native exit independently; do not add a lifecycle
patch solely to make an observer pass.

Keep the incident open. Prioritize a fault or unexpected native status after ordinary Quit/close,
or a new observer fault that identifies the wrapper. Preserve the build,
language, slot copy, exit action and owner, terminal gameplay result, native PID/status, separate
logs and matching private dump. A dump that includes the finalizing object's heap page can narrow
ownership. A fault during play or loss of saved progress warrants immediate escalation.

### Heap-backed InputEventKey shutdown diagnosis

This investigation uses **Standard** rigor: an intermittent cross-runtime failure needs a bounded
capture and independent review. Its decision is whether an application lifecycle correction is
supported. Scope is the frozen Creed menu-observer input, finalizing wrapper and native binding;
campaign replay, an engine upgrade and speculative cleanup are outside this slice. Acceptance is
an independently readable object/binding receipt and explicit limits on the original access violation.

**Confirmed:** After [#1880](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1880)
was accepted at `b329610976df64f992469530f3c4739c0fe00d4f`, two serial debugger acquisitions
on 2026-11-17 reused its unchanged `a56fe51c` product, English `basement-en` seed and
`finalizer-controls/verify-menu.gd`. Each reached Continue, Save, movement, `END` and
`MENU QUIT confirmation`; each complete decoded saved-data object equals the seed.

| Private sample | Capture boundary | Native process result |
| --- | --- | --- |
| `menu-heap01`, PID41372 | ProcDump native second-chance monitoring saw first-chance CLR events and `C000001D`, but no second-chance dump was produced. Retained failed acquisition. | `-1073741795` (`0xC000001D`), no controller termination |
| `menu-heap02`, PID44892 | Corrected first-chance filter admitted only `C0000005` or `C000001D`; one full-memory dump captured the latter. Ordinary first-chance CLR events were not collected or counted as the target fault. | `-1073741795`, no controller termination |

The second sample identifies a different native failure signature from the earlier `C0000005`.
Neither is a successful Quit. ProcDump 12.02 ran as a native debugger (`-g`), with
`-ma -e 1 -f C0000005,C000001D -n 1 -at 15`; the unchanged game was its `-x` target.
The recorded native process handle, rather than the debugger exit code, owns the game result.
Debugger timing/heap effects remain part of this observation's boundary.

The one private dump is 1,469,930,520 bytes; observed target private memory peaked near 1.12 GiB.
Its registered location is the operator’s private `local/runs/diagnostics/native-quit-v1/finalizer` tree.
Adjacent `matching-product` and `original-failures` hold the matching product modules, comparison seed
and original observations. Paths embedded in acquisition records describe the original host layout.
The two acquisitions plus small tools/receipts stayed below the 2 GiB new-output budget and left
over 2.2 GiB free on the output volume. No further native sample followed this capture.

**Confirmed:** Existing ClrMD NuGet `4.2.751201` (assembly `4.2.15.1201`) and the installed
.NET 10.0.8 DAC read the following fields from that dump. The CLR version is `10.0.826.23019`.
All four loaded remake modules and GodotSharp/GodotPlugins match the frozen disk modules by
metadata bytes and MVID; this does not substitute for the retained product build provenance.

| Boundary | Direct observation |
| --- | --- |
| Finalizer thread | TID37000, `IsFinalizer=true`; `GC.RunFinalizers → GodotObject.Finalize → Dispose → godotsharp_internal_refcounted_disposed` |
| Managed `this` | `0x169789103d8`, type `Godot.InputEventKey`; `NativePtr=0x188466522d0`, `_disposed=true`, `_memoryOwn=true` |
| Native fault | Godot executable RVA `0xd944c5`, `UD2` bytes `0f0b`; embedded assertion identifies `!rc_owner`, `_instance_binding_reference_callback`, `csharp_script.cpp:1264` |
| Binding record | `0x18846556ba0`; object key `0x188466522d0`, `inited=false`, GC handle zero and `owner=null` |

`GodotObject.Dispose` sets `_disposed` before entering the native call; that flag is not evidence
of an earlier application disposal or double-dispose.
The binding offsets are checked against the captured callback instructions and the pinned source
structure. They are not inferred from a preceding warning. From the private investigation root,
`pwsh -NoProfile -File ./read-target.ps1 -Checkout "<checkout>"` rereads the finalizer object and
module locations. The checkout path is absolute or relative to the caller's working directory;
its `local/private-inputs.ps1` selects the shared DAC, and its existing LiveInspection Release
build supplies ClrMD. It need not be the original checkout. If that build is absent, rebuild the
pinned `4.2.751201` dependency using the [standalone tool build instructions](../../../docs/operations/godot-live-inspection.md#build-and-capture);
retain the matching .NET 10.0.8 DAC for this dump. No game build or launch is required.
`python -X utf8 ./proof.py` rereads the same dump's assertion/binding bytes, compares module
metadata and decodes both saved payloads. `menu-heap02/proof.json` is the compact combined receipt.
These are offline reads, not a request to repeat the game. The first offline save comparison
incorrectly expected top-level `data`; the corrected reader decodes the existing base64 `payload`.
That inspection error did not change a save or rerun either sample.

**Confirmed:** The relevant upstream source is Godot
[`ed1daf0bf001b61586d9930840f2f1394092c079`](https://github.com/godotengine/godot/tree/ed1daf0bf001b61586d9930840f2f1394092c079).
[`CSharpLanguage::finalize`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/csharp_script.cpp#L140)
calls the managed disposable tracker, then releases native bindings and deletes the `GDMono` host.
The [tracker](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/glue/GodotSharp/GodotSharp/Core/DisposablesTracker.cs)
uses ordinary weak references and disposes targets still returned by `TryGetTarget`.
[`get_existing_instance_binding`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/csharp_script.cpp#L1355)
actually calls the binding-creating path. Its creation callback inserts a default record with null
owner; the separate setup path supplies the owner. The reference callback checks the owner before
checking whether the record was initialized. Those facts explain how an uninitialized binding can
reach the captured assertion; they do not establish when this particular record was created.

**Inferred:** A late managed finalizer overlapping native binding teardown can recreate a default
binding and then enter its reference callback. This is an upstream lifecycle candidate, supported
by the concrete object/record and source paths. The capture is not an allocation/free trace or a
native stack unwind, so the precise interleaving remains **Unknown**. The earlier `C0000005`
wrapper and whether both signatures share that cause also remain **Unknown**.

#### Input ownership and player risk

The retained menu observer inherits `key(code)` through `issue1867/verify.gd → issue1864/verify.gd
→ issue1862/verify.gd → issue1861/verify.gd → issue1804/campaign.gd → issue1804/direct.gd
→ issue1759/observe.gd`. That helper creates native GDScript `InputEventKey` objects and feeds
press/release edges through `Input.parse_input_event`. Godot's
[pinned input contract](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/doc/classes/Input.xml#L541)
routes those events to `Node._input`, including the product's
[`GameRoot._Input(InputEvent)`](../../game/src/GameRoot.cs).
Native-object conversion uses `VariantUtils.ConvertToGodotObject` and
[`InteropUtils.UnmanagedGetManaged`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/glue/GodotSharp/GodotSharp/Core/NativeInterop/InteropUtils.cs#L11)
to reuse or create a managed wrapper; `ScriptManagerBridge.CreateManagedForGodotObjectBinding`
constructs the managed type around the existing native pointer. Godot owns that bridge and its
reference release. The product input handler does not take exclusive disposal ownership of each
borrowed event. Disposing borrowed callback arguments is not an evidenced remedy.

**Confirmed:** Ordinary OS keyboard events also enter the typed product input callback; wrapper
conversion does not select a different ownership mechanism based on GDScript versus OS origin.
The product separately allocates binding templates in `GameInput.PrepareRows`. A postmortem type
alone cannot establish which allocation produced the captured object. **Unknown:** its exact
allocating call and whether a no-script player exit reproduces either failure. The previous ordinary
Quit success remains one observation, not proof that ordinary exits are immune. Theme/audio warning
identities and saved-data equality answer different questions from wrapper lifetime and safe exit.

#### Next useful boundary

No application patch or release-safety claim follows from this sample. Keep #1716 open. A useful
upstream reproducer would use only an authored C# `Node._Input` callback, transient native key
events and ordinary `SceneTree.Quit`, with no private content, explicit GC or lifecycle delay.
Compare a real OS-input/no-script exit with the synthetic-event case; accept the same binding
assertion or a separately identified access-violation object as evidence, not a generic nonzero exit.
The bounded reduction below is now available; upstream submission has not been performed.

An upstream correction needs to preserve binding lookup/creation and teardown ownership while
preventing late finalizers from using cleared or uninitialized records. Merely removing the assert,
forcing collection, sleeping or exiting the process is not that correction. Validate any candidate
on a pinned engine build with the reduced case and frozen original input, preserving all failed
receipts. An application fix instead requires an allocation/free trace identifying an application
owner that releases a resource incorrectly. Until one of those boundaries is satisfied, local
campaign progress can retain separate gameplay/save acceptance, but reliable ordinary shutdown
and readiness for a general player release remain unestablished.

### Standalone Input/Quit reduction

The [asset-free diagnostic](../../diagnostics/input-quit/README.md) continues **Standard** rigor.
Scope is official upstream applicability and a finite ordinary/synthetic input reduction, not a
product fix, engine cutover, stress campaign or upstream submission. Acceptance requires a locked
standalone build, explicit dependency/planner boundaries, and separate terminal/native-exit receipts.
Input consumption and native exit are required separately; a successful reduction does not establish
reliability or close the original incident.

#### Official upstream applicability

**Confirmed:** On 2026-11-17 the official latest stable release is
[`4.7.2-stable`](https://github.com/godotengine/godot/releases/tag/4.7.2-stable), the installed baseline.
The following accepted fixes are already ancestors of its pinned `ed1daf0bf001b61586d9930840f2f1394092c079`
source. GitHub compare results identify each merge as the merge base, with zero commits behind.

| Upstream fix | Accepted merge | Applicability |
| --- | --- | --- |
| [#78157](https://github.com/godotengine/godot/pull/78157), disposable-tracker shutdown ordering | `6afd320984cf14198368cc6c53752813a02169e3` | Tracker disposal already precedes binding/host teardown. |
| [#92201](https://github.com/godotengine/godot/pull/92201), clear parsed input events before language teardown | `be2c5e66f44df2a49a3238434c06ad33a212e544` | `Main::cleanup` already calls `Input::flush_frame_parsed_events`; a relevant synthetic-event retention fix, not a new candidate. |
| [#93172](https://github.com/godotengine/godot/pull/93172), clear binding callbacks during finalization | `3647bc33cdeaf26ef0b9bb1512fddbfbc7ba010e` | `CSharpLanguage::finalize` already frees each owner's instance binding. |

The inspected official master object is
[`bf828694fd29ccb0902cba2d2a8375cbaedd721f`](https://github.com/godotengine/godot/tree/bf828694fd29ccb0902cba2d2a8375cbaedd721f).
Direct source comparison found identical `CSharpLanguage::finalize`, binding create/free/reference
callbacks, `get_existing_instance_binding`, and the complete `DisposablesTracker.cs` between that
object and the installed pin. Both contain the parsed-event cleanup. This bounded review found no
new applicable correction; it does not prove all intervening changes are irrelevant or that master
has been runtime-tested. [#77362](https://github.com/godotengine/godot/pull/77362), proposed runtime-null
guard at head `3e437f2fbcc064ad8688542e23bfca95189484c9`, was closed **without merging** and is not
an accepted fix. A non-null GitHub test-merge field does not establish merge acceptance.

Godot's [MIT license at the baseline](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/LICENSE.txt)
also matches the inspected master. No upstream source/binary was vendored and no engine or SDK was
installed/upgraded. Candidate adoption remains separate work requiring an actual applicable change,
pinned provenance/license, and validation against both the reduction and frozen original failure.

#### Bounded pilot and its limits

**Confirmed:** The standalone Godot.NET.Sdk `4.7.2`/`net8.0` project has no project references and
no production project references it. Its copied source, SDK selection and package lock built outside
the checkout with SDK `10.0.204`, zero warnings/errors. Locked restore passed. The initial restore
failed because repeated source arguments were interpreted as a malformed local HTTPS path; a small
explicit NuGet configuration corrected the preparation. The failed log is retained.

The C# callback receives borrowed `InputEventKey` values, logs primitive observations, and quits
after Left down/up and Enter down. GDScript only supplies native synthetic events. No forced GC,
manual event disposal, shutdown delay, private content or allocation-pressure loop was added.
Both launches used the same registered native Godot executable, Compatibility renderer, Dummy audio,
640×360 window, 60 FPS limit, and managed MVID `794ea9f5-3e2e-4055-9707-85388ebceede`.
The observed CLR is .NET `10.0.8`; the target framework alone does not identify the loaded CLR.

| Retained sample | Terminal observation | Native result |
| --- | --- | --- |
| `os`, PID28024 | C# Ready, no input/terminal receipts. No driver script. | Controller's 60-second preparation cap terminated the process: `124`, forced. This is not a Quit observation. |
| `synthetic`, PID15652 | All three expected typed edges and `SceneTree.Quit` terminal receipt. | `0`, unforced. One successful reduced sample; no original fault reproduced. |
| `os-corrected`, PID11560 | After the bounded controller correction, ordinary Main received all three expected OS-message edges and the same Quit terminal, without a driver script. | `0`, unforced. One successful ordinary reduced exit. |

**Confirmed protocol deviation:** The synthetic process completed at 23:47:13 UTC while the ordinary
process remained alive until its 23:47:21 timeout. The controller was not awaited before the second
launch. These are not an equivalent serial pair; the overlap prevents a controlled origin comparison.
The ordinary controller also omitted the established `Engine` window-class filter and persisted its
sent-edge count only on normal completion. Its retained receipt therefore cannot distinguish a
window-selection failure from a posted first edge that was not delivered. Ready-line parsing is
correct. The pinned Windows key-event path does not require a window-focus flag before parsing key
events; adding speculative activation messages is not an evidenced correction.

**Confirmed:** Main-gate authorized one OS-only preparation correction; synthetic was not repeated.
The controller restored PID plus `Engine` class matching and persisted a bounded window inventory,
each message's target/parameters/result, and the sent-edge count even on timeout. The corrected PID
had three visible windows: `Engine`, `UAC_InputIndicatorOverlayWnd` and `UAC Input Indicator`.
The original all-visible-windows count predicate would reject that inventory. All three corrected
messages targeted the single Engine window; the C# receipts prove consumption, beyond message-post
success. Logical keys/pressed states, runtime and MVID match synthetic; OS physical key values are
Left/Enter rather than synthetic zero. No activation message or shutdown intervention was added.

**Inferred:** Auxiliary visible windows explain the first controller's zero-input observation.
Its missing window/sent-edge record prevents proving the exact original state retrospectively.
The corrected run had no other process from this investigation running. The independently owned
#1875 headless chapter observation was active with a different project/output root; main-gate
explicitly allowed that parallelism. No timing or hardware-equivalence claim follows. This does
not erase the first pair's protocol deviation or turn the samples into a clean serial comparison.

**Unknown:** whether either input origin reliably reproduces either original fault signature,
and whether a reduced successful exit predicts product shutdown safety.
The original failed receipts and heap diagnosis remain authoritative for their named seams. No new
dump was taken or copied, and neither this success nor the already-included upstream fixes closes
#1716. The first pair is preserved, not replaced by a retry-until-green result.

The original input observations and bounded controllers are retained under the private operator
`local/runs/diagnostics/native-quit-v1/input-comparison` directory. The tracked diagnostic project
and pinned upstream revision identify the source. The initial estimate was under 200 MiB total new output,
with one process at a time and O(three input events); observed package/build/source/log output was
about 140 MiB. Runtime concurrency deviated as recorded above. The project README gives standalone
preparation and manual ordinary/synthetic reproduction without private paths. Only the explicit
`remake/diagnostics/input-quit/` prefix receives no product partitions in the local planner; it
requires its direct locked build/observation instead. A direct comparison of 1233 existing paths
found no selection changes. The shared planner file itself still selects its existing broad gates;
the unchanged CI workflow also retains its broad `remake/*` fallback. The diagnostic is not run
by CI. No new test platform, CI runtime job or tests of verification tools were added.
