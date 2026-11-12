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

**Unknown:** Whether either leaked object participated in the original fault, and whether ordinary
player exits can trigger it. Keep #1716 open. A fresh finalizer fault or unexpected native exit on
ordinary Quit warrants collecting its build, path through the UI, process status, logs and matching
local dump. A crash during play or lost progress changes the severity.
