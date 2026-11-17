# Input/Quit reduction

Project-authored, asset-free diagnostic for [the InputEventKey shutdown incident](../../docs/verification/map3-commission-save.md#standalone-inputquit-reduction).
This is a candidate reduction, not a reliable crash reproducer or a product gate. It has no
remake project references, reference inputs, private content, editor plugins or test framework.
Production projects do not reference it. Godot owns the borrowed `_Input` argument; the callback
records primitive values without retaining or explicitly disposing the event.

## Boundary

Both modes deliver Left down, Left up, Enter down to the same C# callback. After the third
expected edge, C# prints a terminal receipt and immediately calls `SceneTree.Quit()`.
The ordinary mode uses the main scene without a driver script. The synthetic mode constructs
native GDScript `InputEventKey` objects through `Input.parse_input_event`; its frame awaits allow
input delivery between edges and do not delay shutdown. There is no forced collection, lifecycle
sleep, artificial destruction, allocation-pressure loop or explicit process exit.

`INPUT_QUIT` JSON lines identify readiness, each input's type/key/physical key/native pointer,
the managed assembly MVID/runtime, and terminal reach. They are observations, not assertions that
shutdown succeeded. Record the native process status separately. Pointer values are process-local.
OS physical key values may differ from synthetic values; compare logical edges and terminal reach.

## Prepare outside the checkout

Use the registered Godot 4.7.2 .NET installation and SDK 10.0.204 selected by the checkout's
[`local/private-inputs.ps1`](../../../docs/operations/local-private-inputs.md).
No ROM or extracted assets are needed. Godot is MIT licensed; restore the pinned packages using
the installed distribution's `GodotSharp/Tools/nupkgs` and NuGet.org. Do not upgrade the shared tools.

The following PowerShell recipe accepts explicit absolute roots. Choose a fresh scratch directory
outside removable worktrees with at least 200 MiB available for packages/builds/logs, plus normal
runtime headroom. All paths below derive from those roots, independently of the caller's directory.

```powershell
$checkout = (Resolve-Path -LiteralPath '<checkout>').Path
$scratch = [IO.Path]::GetFullPath('<fresh diagnostic output directory>')
$project = Join-Path $scratch 'project'
New-Item -ItemType Directory -Path $project -ErrorAction Stop | Out-Null
. (Join-Path $checkout 'local/private-inputs.ps1')
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:MSBUILDDISABLENODEREUSE = '1'
$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
$env:NUGET_PACKAGES = Join-Path $scratch 'nuget/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $scratch 'nuget/http-cache'
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $scratch 'nuget/plugins-cache'
$env:NUGET_SCRATCH = Join-Path $scratch 'nuget/scratch'
$env:TEMP = Join-Path $scratch 'temp'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Path $env:TEMP -ErrorAction Stop | Out-Null
$source = Join-Path $checkout 'remake/diagnostics/input-quit'
foreach ($file in 'Sf2.InputQuit.csproj','packages.lock.json','project.godot',
                  'Main.tscn','InputQuit.cs','input.gd') {
    Copy-Item -LiteralPath (Join-Path $source $file) -Destination $project
}
Copy-Item -LiteralPath (Join-Path $checkout 'remake/global.json') -Destination $project
$localPackages = Join-Path (Split-Path -Parent $env:GODOT_BIN) 'GodotSharp/Tools/nupkgs'
$escaped = [Security.SecurityElement]::Escape($localPackages)
$nugetConfig = Join-Path $scratch 'NuGet.Config'
@"
<configuration><packageSources><clear/>
<add key="godot-local" value="$escaped"/>
<add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
</packageSources></configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8
Push-Location $project
try {
    & $env:DOTNET_BIN restore Sf2.InputQuit.csproj --locked-mode --configfile $nugetConfig
    if ($LASTEXITCODE -ne 0) { throw 'Locked restore failed' }
    & $env:DOTNET_BIN build Sf2.InputQuit.csproj --configuration Debug --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
} finally { Pop-Location }
```

Keep the configured absolute `DOTNET_BIN` and shared SDK-only `DOTNET_CLI_HOME`; other writable
state stays in scratch. The copied project has no dependency on its original checkout. Build
outputs go under its `.godot/mono/temp`; no editor import launch is required for these resources.

## Bounded observation

Run one ordinary case, collect its exit, then run one synthetic case. Use separate case directories
for APPDATA, LOCALAPPDATA, TEMP/TMP and logs. For example, after selecting `$case` and creating those
directories:

```powershell
$env:APPDATA = Join-Path $case 'appdata'
$env:LOCALAPPDATA = Join-Path $case 'localappdata'
$env:TEMP = Join-Path $case 'temp'
$env:TMP = $env:TEMP
$native = $env:GODOT_BIN -replace '_console\.exe$', '.exe'
$arguments = @('--verbose','--audio-driver','Dummy','--max-fps','60',
               '--resolution','640x360','--path',$project,'--log-file',
               (Join-Path $case 'native.log'))
# Synthetic case only:
# $arguments += @('--script','res://input.gd')
& $native @arguments | Out-Host
$nativeExit = $LASTEXITCODE
```

In ordinary mode, focus the window after the `ready` receipt, press and release Left, then press
Enter. Collect the process exit before the next launch; disappearing windows alone do not prove
exit. A Windows automated controller may instead target that native PID's Engine window with
`WM_KEYDOWN`/`WM_KEYUP`, using a private desktop without switching the user's desktop. Match both
PID and the `Engine` class, not a count of all visible windows; retain the window inventory,
message results and sent-edge count even on timeout. Record this
as posted OS messages, not a physical keyboard observation. A controller timeout/termination is a
preparation failure if the terminal was never reached; it is not a Quit crash.

Keep a finite 60-second per-process cap and stop after the pair. Do not rerun to obtain green results.
If input delivery fails, preserve it and review the preparation boundary before authorizing another
sample. Capture no new dump: the incident already has a sufficient private heap capture. A single
successful reduction does not disprove the intermittent original fault. Results, protocol deviations,
upstream applicability and open questions belong to the linked incident owner.
