# Inspect an Existing Godot .NET Session

## Scope and Ownership

Use this procedure when a player reports a current state failure and restarting would lose the
reproduction or unsaved progress. The existing Standard engineering scope applies: preserve the
player's instance, obtain bounded state evidence, and route a production correction separately.
The maintained entry is
[`Sf2.Remake.LiveInspection`](../../remake/tools/Sf2.Remake.LiveInspection/Program.cs).
It reads managed exploration fields from a Windows Process Snapshotting (PSS) copy. It does not
evaluate target methods, send input, advance time, change saves, or implement `QuerySave` again.
Its completion means a receipt was collected, not that saving was accepted or the defect repaired.

`DEBUG` in a window title does not establish a connected debugger or a command endpoint. First
inspect the actual launch arguments and existing observation channel. Prefer the session's existing
state/input/projection probe when one is already loaded and can answer the question. Do not restart
to install a probe, rebuild the loaded game, or assume a diagnostic named pipe is a Godot RPC API.
This fallback handles the current managed exploration field layout only; it is not a general debugger,
battle inspector, or supported interface for arbitrary Godot builds.

## Identify and Preserve the Target

1. Derive Git HEAD, tree, dirt and worktrees. Identify the game's owning project from its native
   process command line, together with executable, PID, start time and window title. A console
   wrapper PID is not the native game PID. Inspect the specific owned processes rather than all
   users' command lines. Retained headless probes in other worktrees are separate owners.
2. Record the current build's provenance from its launch/build record. Preserve the loaded Debug
   artifacts. Repository HEAD can advance while an older binary remains loaded. The tool compares
   loaded module MVIDs with this checkout's `remake/game/.godot/mono/temp/bin/Debug` artifacts;
   matching MVIDs prove those module identities agree, **not** that they were built from current HEAD.
3. Reuse the verified installation and owning checkout. Load its current ignored environment using
   [Local Private Inputs](local-private-inputs.md) and the
   [locked .NET workflow](../../remake/docs/development-and-verification.md#locked-net-workflow).
   `DOTNET_BIN`, `DOTNET_CLI_HOME` and `GODOT_BIN` must select the registered absolute paths;
   force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Keep NuGet, TEMP and outputs in this worktree.
4. Name the needed fields and resource budget before capture. Do not force a save, mutate state,
   close menus, or send movement just to make the report easier to read. A failed diagnostic does
   not authorize restarting the player. Preserve stderr and existing failed receipts.

The target PID/start pair and configured native executable are checked before attachment. Operators
must still verify the owning project: multiple sessions can share the same Godot executable. The
tool resolves its repository from its own built location, not the caller's working directory, and
allows only a new output below that repository's ignored `local/` without reparse-point ancestry.

## Build and Capture

After loading the owning environment in this same PowerShell process, run from that checkout's
`remake/`. Build only this standalone tool; it has no game project references. The root's `.NET`
selection and pinned NuGet lock apply. The example uses an operator-selected PID from the preflight,
not a persisted PID from a previous run.

```powershell
$ErrorActionPreference = 'Stop'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$inspectionProject = 'tools/Sf2.Remake.LiveInspection/Sf2.Remake.LiveInspection.csproj'
& $env:DOTNET_BIN restore $inspectionProject --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'Inspection restore failed' }
& $env:DOTNET_BIN build $inspectionProject --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Inspection build failed' }

$inspectionPid = [int](Read-Host 'Verified native game PID')
$inspectionProcess = Get-Process -Id $inspectionPid
$inspectionStart = $inspectionProcess.StartTime.ToUniversalTime().ToString('O')
$inspectionRoot = [IO.Path]::GetFullPath((Join-Path $PWD '..'))
$inspectionDir = Join-Path $inspectionRoot 'local/live-inspection'
New-Item -ItemType Directory -Force -Path $inspectionDir | Out-Null
$inspectionStem = Join-Path $inspectionDir ([DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fffffff'))
$inspectionDll = 'tools/Sf2.Remake.LiveInspection/bin/Release/net8.0/Sf2.Remake.LiveInspection.dll'
& $env:DOTNET_BIN $inspectionDll $inspectionPid $inspectionStart "$inspectionStem.json" 2> "$inspectionStem.stderr.log"
$inspectionExit = $LASTEXITCODE
if ($inspectionExit -ne 0) { throw "Inspection failed ($inspectionExit); retain stderr" }
```

The native executable is the configured `GODOT_BIN` with a possible `_console.exe` suffix replaced
by `.exe`. The inspector uses the target's local `coreclr.dll` sibling `mscordaccore.dll` explicitly;
it does not need symbol-server credentials, a downloaded DAC, or target method evaluation. Dispose
releases the snapshot before JSON is written. Output contains private runtime state and addresses:
keep JSON, logs, binaries, restored dependencies and any derived evidence local; do not attach them
to public issues. No screenshot or full memory dump is part of this procedure.

## Budgets and Failure Boundaries

Microsoft documents that Windows PSS briefly suspends the target while taking the snapshot, then
allows inspection of the copy. This is read-only observation with a possible transient pause, not
zero-impact sampling. See the official
[ClrMD guide](https://github.com/microsoft/clrmd/blob/2e7e5d74cddb2be0ac523aedead10b90a95e71c3/doc/GettingStarted.md)
and [security guidance](https://github.com/microsoft/clrmd/blob/2e7e5d74cddb2be0ac523aedead10b90a95e71c3/SECURE-CONFIGURATION.md).

The initial budget is one snapshot at a time, no persisted dump, at most 2 MiB JSON, and 200 MiB
for newly restored tool packages before replanning. PSS cost depends on the target's virtual memory
and copy-on-write activity; a small JSON receipt does not bound snapshot memory. The tool refuses
targets above 8 GiB private memory or less than 2 GiB system commit headroom, but those admission
checks are not a memory reservation. Check free storage and host pressure before capture. Stop and
replan if measured memory or runtime materially exceeds the estimate.

The heap walk is O(live/retained heap objects), capped at five million enumerated objects or thirty
seconds checked between objects. The latter is not a hard timeout for an OS/ClrMD call. Projection
limits are 128 entities, 1,024 actions per entity, 128 fields per type, four nested value levels,
128-character strings and 32 MiB metadata per module. Referenced objects are summarized rather than
recursively dumped. Unhandled primitive kinds appear as `unread:*`; never treat those as zero/false.

One observed approximately 1.15 GiB-private target produced an 84,607-byte receipt in 745 ms of
inspection, with a 330 ms snapshot call and approximately 61 MiB inspector peak working set.
System commit samples were approximately 37.3 / 38.2 / 38.1 GiB before/during/after. These host-wide
samples include unrelated activity and do not establish attributable or peak snapshot memory.
**Unknown:** exact target suspension duration; snapshot-call duration is only an upper bound.
Reuse a sufficient receipt; do not repeat captures merely to improve reporting.

Wrong PID/start/executable/output path, insufficient resource headroom, missing local DAC, unsupported
CLR/field layout, no unique live `GameRoot`, non-exploration mode and budget failures exit nonzero.
Keep that evidence distinct from a gameplay failure. A missing disk assembly fails inspection;
`diskMatchesLoaded=false` is retained as evidence rather than silently accepted. Before interpreting
fields against source, require identities for Godot, Domain, Application and Content, all matches,
and the independent build-to-source record. Missing identities, unexpected fields or `unread` data
leave the affected claim **Unknown**; do not substitute current source or rebuild the target to hide
the mismatch. Adapt the bounded tool in its own owned change if a newer layout is needed.

## Interpreting Save Refusal

Read the selected live `GameRoot._session._current`, not an arbitrary retained snapshot in the heap.
The receipt includes revision, story/wait/caller state, logical text/view/display, active map, entity
motion/follower/action state and save-panel fields. Compare the specific receipt with the matching
source owners:

- [`OpeningSave.RequireStable`](../../remake/src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs)
  owns save-boundary rejection; later location/restore checks can also fail.
- [`SessionSnapshot.CanWaitAtInput`](../../remake/src/Sf2.Remake.Application/Runtime/SessionContract.cs)
  defines player input availability.
- [`EntityMotion`](../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs) defines unfinished movement
  by position differing from destination.
- [`ExplorationSessionView`](../../remake/game/src/Exploration/ExplorationSessionView.cs) owns host
  field-time advancement and pause at input.

The Church idle-save investigation tracked in
[#1580](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1580) supplies a bounded example:
**Confirmed:** successive field reads retained revision 16550 / simulation tick 9647, closed text and
portrait windows, no script wait/cursor, no view destination, a settled player, and visible follower
entity 2 at Y=5824 with destination Y=5760, without actions or sprite wait. The source movement rule
therefore classifies that follower as moving, and `RequireStable` rejects visible moving followers.
**Inferred:** player-input field-time pause leaves that follower unfinished and explains the observed
idle-save refusal. This is field/source diagnosis, not a target-side `QuerySave` invocation or proof
that all other save checks pass. The production correction must settle legitimate motion and verify
save/restore behavior; bypassing the guard or mutating this player's state is outside inspection.

## Dependency and Verification Ownership

`Microsoft.Diagnostics.Runtime` is pinned exactly to NuGet `4.2.751201` with transitive versions in
`packages.lock.json`. Its official NuGet metadata declares MIT and repository
`microsoft/clrmd` commit `2e7e5d74cddb2be0ac523aedead10b90a95e71c3`. Dependencies are restored, not
vendored. Preserve package licensing notices when distributing dependency binaries; this source
change does not authorize publishing runtime captures or private game artifacts.

Public CI's `live-inspection-build` only restores the locked tool and compiles it. It never attaches
to a process or receives private input. Verify tooling changes directly with a controlled build and,
only when necessary, an owned live observation; do not add tests of the inspector. Record process
identity, exit/errors, report fields, loaded/disk identity comparison and resource measurements in
the private handoff. Public handoffs retain only the minimum redistributable diagnosis and limitations.
