# Local Private Inputs and Shared Tools

Load the owning worktree's ignored `local/private-inputs.ps1` in the same PowerShell process as
its command. Keep actual machine paths untracked. No user/machine PATH changes are required.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
uv run sf2 toolchain verify
```

## Selections

`SF2_SHARED_INPUT_ROOT` selects the registered immutable `roms/sf2-us.bin`. With no selection,
the ROM default remains `local/roms/sf2-us.bin`; an explicit `--rom-path` overrides it. Before
reporting a ROM blocker, load the configuration and run `uv run sf2 rom verify` when that check
is authorized. Distinguish missing configuration, missing input, identity mismatch and later tool
failures. Do not copy a ROM merely to fill a default filename.

`SF2_TOOLCHAIN_ROOT` is the required absolute shared installation root for maintained research
tool consumers. Missing, empty, relative, escaping, missing-file and wrong-type selections fail;
they never trigger a local installation download or fallback. The existing resolver registers:

| Registered identity | Relative to tool root | Kind |
| --- | --- | --- |
| `toolchains/jdk-17.0.19+10` | `jdk-17.0.19+10` | Complete JDK directory |
| `archives/BizHawk-2.11.1-win-x64.zip` | `BizHawk-2.11.1-win-x64.zip` | Pristine release archive |
| `toolchains/BizHawk-2.11.1-win-x64` | `BizHawk-2.11.1-win-x64` | Installed release |
| `toolchains/sf2disasm-c834c652/tools` | `sf2disasm-c834c652/tools` | Complete pinned H1 installation |

The toolchain manifest retains fixed upstream/binary provenance. The complete H1 installation
includes the five pinned executables and `tools/asw/{as,cmdarg,ioerrs,p2bin,tools}.msg` from the same
pinned SF2DISASM checkout. `asw` cannot start without its adjacent message catalogs; checking only
EXE identities is insufficient. `buildSupportFiles` names these required nonempty files so shared
selection rejects an incomplete installation before a build starts. Promotion verifies support
file bytes against their existing pinned Git objects; no second checksum inventory is required.

The JDK digest is the existing
POSIX-relative-path ordinal inventory: `PATH<TAB>SIZE<TAB>UPPERCASE_SHA256`, LF-separated without a
trailing LF, then UTF-8 SHA-256. Java's executable identity and version are also checked. BizHawk's
archive, executable and Lua identities are checked; runtime preparation compares release files
directly with the pinned archive. Preparation creates fresh local configuration and writable paths;
it neither copies release files nor imports installation saves or configuration. Manifest `localJavaPath`, `localArchivePath` and `localExecutablePath` describe
legacy layouts only; maintained consumers no longer select them.

Select the existing shared .NET installation with absolute `DOTNET_BIN` and the accepted shared
SDK-only `DOTNET_CLI_HOME`. Select the existing Godot editor with absolute `GODOT_BIN` (ordinary
PowerShell examples use `$godotBinary = $env:GODOT_BIN`). Both installations belong under the chosen
shared tool root in local machine configuration. Preserve the pinned SDK/editor versions and complete
runtime dependencies. Their [environment owner](../../remake/docs/development-and-verification.md#locked-net-workflow)
retains forced `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` and worktree-local NuGet/build state.

## Maintained Consumers

| Consumer | Shared input and local writes |
| --- | --- |
| `sf2 init` / `Initialize-LocalResearch.ps1` | Validate shared tools; initialize only local writable SF2DISASM checkout; no tool downloads/extraction or redundant ROM copy |
| `sf2 toolchain verify` / `Test-Toolchain.ps1` | Verify local checkout provenance and shared JDK, complete H1 installation, BizHawk archive/executable/Lua |
| `Invoke-Sf2Rebuild.ps1` | `sf2 toolchain paths` checks the complete installation and supplies all five shared binaries; source, cwd, builds and TEMP stay local; build algorithm unchanged |
| Ordinary Python H3 and Lua syntax | Compile with shared Lua; execute the registered installation with fresh local writable state |
| `Observe-H3Battle01TurnOrder.ps1` | `sf2 toolchain bizhawk-materialize` supplies executable, config, cwd and TEMP; observation Lua unchanged |
| Debug bridge | Same direct launch preparation, retaining explicit local output, paused startup and bounded process lifecycle |
| Disabled original-reference replay | Registered executable with contained local launch state; original disabled admission and budget remain unchanged |
| .NET / Godot | Shared complete installations, explicit process selection; owning project, imports, outputs and caches remain local |

The initialization/verification PowerShell scripts are thin forwarding entrypoints. Their manifest,
upstream, Java and Defender-scan parameters still propagate. `JavaPath` may explicitly select only
the registered shared executable; another executable fails rather than bypassing the shared selection.
`sf2 init --skip-defender-scan` preserves the explicit scan opt-out. A supplied upstream checkout must
be under the owning worktree's `local/`; H1 cannot write into another task's source tree.

## BizHawk Direct Launch State

`materialize_bizhawk_launch` and `sf2 toolchain bizhawk-materialize` prepare writable
state only. Maintained consumers execute the registered, verified installation by
default; no release files are copied. The command returns executable, explicit
config, cwd and child TEMP/TMP. A fresh directory beneath this worktree's ignored
`local/` owns all of the latter. Preparation never cleans existing outputs.

The helper sets absolute `PathEntries.Paths` for the pinned Global_NULL and GEN
roles: base, ROM/firmware, movies/backups/macros, A/V, tools/Lua, watches/logs,
bundles/external tools, temporary files, Genesis savestates, SaveRAM, screenshots
and cheats. Launcher-owned entries replace caller path entries after merging
settings, preventing inherited installation defaults from returning. Other
settings retain caller overrides, including explicit `SoundEnabled=true`; default
host audio is muted, the core is Genplus-gx, and automatic save-slot loading/saving,
periodic SaveRAM and backups are disabled. Observers and bridge logs receive
separate explicit local destinations. TEMP is selected before native startup.

Native Python consumers validate the registered executable, local cwd/config/TEMP
and exact writable path roles before starting. The frozen replay consumer uses
the same direct preparation and explicit cwd; this interface migration does not
authorize replay or change its original admission/identity checks.

Only serial GEN/NULL research is supported. Before launch, inspect EmuHawk
processes and obtain exclusive installation ownership through the repository's
`bizhawk-original-runtime` scheduling boundary. Keep ownership through exit and
installation/settings verification. The preparation CLI does not reserve an
installation or attach to a process. A conflict stops launch; there is no silent
copy fallback or general Windows sandbox.

For pinned BizHawk commit `bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5`, Windows
[PathUtils](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Common/Extensions/PathExtensions.cs)
uses the executable's application base and ignores `BIZHAWK_DATA_HOME`.
[GameDBHelper](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/GameDBHelper.cs)
and [Config](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/config/Config.cs)
retain executable-base user-database/controller-default surfaces. Arbitrary UI
editing of these files, other cores and concurrent use remain unsupported.
Existing instrumented H3 owners temporarily register their ROM in the shared user
DB and restore the prior bytes/absence in `finally`; this remains an explicit
serialized exception, not relocated state. Preserve prior settings and verify
restoration after those owners run. No broad UI editing or database mutation is
authorized by launch preparation. Old copies/evidence and the two retained empty
Genesis directories remain untouched.

## Direct Installation Reuse Findings

**Confirmed:** a separately user-authorized investigation launched the registered
BizHawk 2.11.1 / Genplus-gx executable directly, without runtime copies. A paired
1,030-frame replay of retained input produced identical final 68K RAM, CPU registers
and emulator frame counts with and without the acquisition observer. Its
[performance result](./bizhawk-debug-bridge.md#acquisition-performance-boundary) is
independent of the installation-reuse decision.

The initial direct pair used local `--config`, cwd and child TEMP, with save-slot
autoload/autosave and periodic SaveRAM disabled. It still created a Genesis SaveRAM
file and backup **under the shared installation**. Thus `AutosaveSaveRAM=false` and
a separate config file do not by themselves isolate runtime writes. Those generated
files were retained outside the installation; original settings were restored.

A corrective 120-frame direct probe set existing `PathEntries.Paths` entries:

| System / type | Explicit absolute destination |
| --- | --- |
| `Global_NULL` / `Base` | This run's ignored output directory |
| `GEN` / `Base` | Its `Genesis` subdirectory |
| `GEN` / `Save RAM` | Its `SaveRAM` subdirectory |

**Confirmed:** the probe exited 0, wrote SaveRAM at the selected local destination
and added/changed/removed no shared-installation files. The three diagnostic
processes all terminated; all 450 release files still matched the pinned archive,
the ROM and retained parent/source segment pairs validated, and backed-up settings
matched their original bytes. Two empty generated directories remain after an
automatic policy rejection of their removal; no generated save file remains there.

**Inferred:** serial Genesis research can reuse the installed executable with
explicit per-run config, absolute writable paths, TEMP and disabled accidental
autoload. Copying roughly 155 MB of release files per launch is not intrinsically
required for this workload. Reuse must keep outputs separate from immutable inputs.
The maintained default now uses the direct-launch state contract above, including
the observed SaveRAM correction and explicit remaining Global/Genesis paths. No
new emulator tree or generic isolation framework is required for this workload.

**Unknown:** arbitrary UI/core/tool operations and concurrent users modifying the
same installation. Controller defaults and the user game database still have
executable-base paths, as established above; `PathEntries` does not relocate those
surfaces. Start with one installation owner, avoid modifying shared defaults/database
during a run, and stop on a conflict or reproduction needing unsupported isolation. The default
provides no automatic copy fallback; the existing instrumented H3 database
registration exception is explicitly described above.

The pinned [path defaults](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/config/PathEntryCollection.cs)
and [path resolution](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/config/PathEntryCollectionExtensions.cs)
own these configuration names. The private reproduction is
`uv run --locked python -X utf8 local/acquisition-speed-diagnostic/probe_paths.py`,
after same-process input configuration. It reuses the performance recipe with only
120 input frames and the three explicit path entries. The retained
`path-probe-report.json`, comparison report and `shared-backup/` record actual writes
and restoration. No generic environment-variable redirect or new binary was used.

## Local State and Promotion

Keep writable source/build checkouts, Python/uv/NuGet environments and caches, TEMP, emulator state,
movies, traces, assets, reports and exports in their owning worktree. Only SDK-owned CLI state may
use the shared CLI home. Do not replace the whole `local/` directory with a shared writable tree.

Configure every retained worktree's ignored environment when selecting shared installations. Reuse
verified existing installations. Where an authorized promotion is needed, copy a complete verified
installation into an empty destination, verify the copy, and promote without overwrite. Preserve
old installations, configuration backups, gate receipts and completed failures. Deletion and cleanup
require separate authorization. A mismatch is a failure, never permission to overwrite an input.
