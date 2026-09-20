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
| `toolchains/sf2disasm-c834c652/tools` | `sf2disasm-c834c652/tools` | Pinned H1 binaries |

The toolchain manifest retains fixed upstream/binary provenance. The JDK digest is the existing
POSIX-relative-path ordinal inventory: `PATH<TAB>SIZE<TAB>UPPERCASE_SHA256`, LF-separated without a
trailing LF, then UTF-8 SHA-256. Java's executable identity and version are also checked. BizHawk's
archive, executable and Lua identities are checked; runtime preparation compares release files
directly with the pinned archive and copies only those files, excluding any old installation saves
or configuration. Manifest `localJavaPath`, `localArchivePath` and `localExecutablePath` describe
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
| `sf2 toolchain verify` / `Test-Toolchain.ps1` | Verify local checkout provenance and shared JDK, five H1 tools, BizHawk archive/executable/Lua |
| `Invoke-Sf2Rebuild.ps1` | `sf2 toolchain paths` supplies all five shared binaries; source, cwd, builds and TEMP stay local; build algorithm unchanged |
| Ordinary Python H3 and Lua syntax | Compile with shared Lua; prepare a fresh local BizHawk runtime copy for native launch |
| `Observe-H3Battle01TurnOrder.ps1` | `sf2 toolchain bizhawk-materialize` supplies executable, config, cwd and TEMP; observation Lua unchanged |
| Debug bridge | Same local runtime preparation, retaining its explicit output, paused startup and bounded process lifecycle |
| Disabled original-reference replay | Shared archive/installation resolution feeds its local contained runtime; original disabled admission and budget remain unchanged |
| .NET / Godot | Shared complete installations, explicit process selection; owning project, imports, outputs and caches remain local |

The initialization/verification PowerShell scripts are thin forwarding entrypoints. Their manifest,
upstream, Java and Defender-scan parameters still propagate. `JavaPath` may explicitly select only
the registered shared executable; another executable fails rather than bypassing the shared selection.
`sf2 init --skip-defender-scan` preserves the explicit scan opt-out. A supplied upstream checkout must
be under the owning worktree's `local/`; H1 cannot write into another task's source tree.

## BizHawk Runtime Copies

Configuration, SaveRAM and movie contamination can change observations. The accepted simple policy
is to supply a clean local runtime copy from the verified shared installation. This exception to
direct shared execution is intentional. Unrelated preference settings do not require additional
isolation machinery. There is no hardlink or cross-volume strategy.

For pinned BizHawk commit `bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5`, Windows
[PathUtils](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Common/Extensions/PathExtensions.cs)
uses the executable's application base for both installation and data directories, ignoring
`BIZHAWK_DATA_HOME`.
[GameDBHelper](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Emulation.Cores/GameDBHelper.cs)
places the user database beneath that data directory;
[Config](https://github.com/TASEmulators/BizHawk/blob/bdddf4a58aa1a022afb11dc73294a81a5aa7bbd5/src/BizHawk.Client.Common/config/Config.cs)
places controller defaults beside the executable. Thus changing cwd and `--config` alone is
insufficient. Copying the complete release makes those defaults local as well.

The local executable, explicit local `--config`, cwd, save/movie directories and child `TEMP`/`TMP`
share one fresh local runtime directory. TEMP is selected before startup because BizHawk starts its
temporary-file manager before loading configuration. Failed preparation and existing observations
remain available; preparation does not clean prior runs. No whole-local junction or shared writable
emulator directory is used.

**Confirmed:** source resolution and direct materialization can establish selected paths, fixed
release bytes and independent local copies. **Unknown:** actual Windows startup/write behavior of
the new launch preparation remains pending a separately authorized native observation. Preparation
is not H3 acceptance and does not enable replay or reset diagnostic budgets.

## Local State and Promotion

Keep writable source/build checkouts, Python/uv/NuGet environments and caches, TEMP, emulator state,
movies, traces, assets, reports and exports in their owning worktree. Only SDK-owned CLI state may
use the shared CLI home. Do not replace the whole `local/` directory with a shared writable tree.

Configure every retained worktree's ignored environment when selecting shared installations. Reuse
verified existing installations. Where an authorized promotion is needed, copy a complete verified
installation into an empty destination, verify the copy, and promote without overwrite. Preserve
old installations, configuration backups, gate receipts and completed failures. Deletion and cleanup
require separate authorization. A mismatch is a failure, never permission to overwrite an input.
