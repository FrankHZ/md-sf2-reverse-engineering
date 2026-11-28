# Local Private Inputs and Shared Tools

Load the owning worktree's ignored `local/private-inputs.ps1` in the same PowerShell process as
its command. Keep actual machine paths untracked. No user/machine PATH changes are required.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
# Research checkouts need the pinned writable upstream before toolchain verification.
# On first research use, run: uv run sf2 init
uv run sf2 toolchain verify
```

## Fresh worktree setup

Keep the machine configuration template outside all worktrees. Register its absolute path once
with `git config --local sf2.privateInputsTemplate <absolute-template-path>` in the owning repo.
Linked worktrees share that setting. From the root of a checkout with no `local/`, restore just
the configuration before running the commands above:

```powershell
$template = git config --path --get sf2.privateInputsTemplate
if (!$template -or !(Test-Path -LiteralPath $template -PathType Leaf)) { throw 'Missing private input template' }
New-Item -ItemType Directory -Path ./local/tmp -Force | Out-Null
Copy-Item -LiteralPath $template -Destination ./local/private-inputs.ps1
```

The restored script selects stable installations and inputs and derives writable paths from its
new checkout. Register a template for each host/clone that uses private inputs.

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
it neither copies release files nor imports installation saves or configuration. Manifest
`localJavaPath`, `localArchivePath` and `localExecutablePath` describe legacy layouts only;
maintained consumers no longer select them.

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

### Maintained Default Boundary

**Confirmed:** the default CLI prepares no executable copy, and ordinary `sf2 h3 rng`
uses the registered installation successfully. A default `DebugBridge.start` run
reuses the retained 1,030-frame/229-request short comparison with no launcher
substitution: final 68K RAM, reported registers and emulator frame match the
retained continuous endpoint; journal reconstruction equals the final receipt.
SaveRAM is local. Shared file size/mtime inventory and backed-up settings bytes
remain unchanged, with all 450 release members matching the pinned archive.

**Confirmed:** two fresh acquisition segments in distinct directories save at the
house-exit and Sarah-classroom checkpoints, advancing cumulative delivered frames
from 1,030 to 2,080. Raw configuration hashes differ; explicit continuation
identities match. Both sealed pairs read successfully. Actual sound-setting drift,
additional unknown settings and unrelated absolute strings change compatibility
identity; an escaped path, mismatched settings and historical raw identity are
rejected before process start. Missing/null/unknown identity markers cannot bypass
segment checks. The [bridge identity owner](./bizhawk-debug-bridge.md#configuration-identity-across-segments)
defines this contract; historical pairs remain unchanged.

**Confirmed:** all selected H3 commands completed successfully with the new default:

| Partition | Completed commands |
| --- | --- |
| `h3-battle01` | 31 |
| `h3-map-debug` | 25 |
| `h3-direct-seam` | 11 |
| `h3-witch` | 8 |
| `h3-sound` | 1 |
| `h3-original-reference` | 2, both `--preflight-only`; no replay launch |

The serial `local/issue510/run_gates.py` recipe reads the committed plan and invokes
the real CLI through `h3_entry.py`, which redirects only generated H3 output paths
into fresh ignored directories. It does not replace launch preparation, executable,
config, core or process execution. The earlier completed ordinary RNG result is
retained rather than repeated. `gates/results.jsonl` and `gate-summary.json` record
each result and the unchanged installation inventory after each command.

**Confirmed:** controlled ordinary-observer timeout and guarded controller-input
callback failure remain visible; the latter exits 1 and retains its typed terminal
failure status. Native `on_started` and `on_timeout` exceptions retain an error in
`NativeProcessResult` and terminate/reap the owned process. A failed timeout
diagnostic callback no longer skips the result/reap step. Bridge disconnect retains
its known forced-termination boundary; idle timeout exits 1 without forced
termination and clears its callback. Both failure receipts reconstruct exactly
from the journal. Reproduce with `observe_native_failures.py` and the maintained
bridge module's `--mode disconnect` / `--mode idle-timeout`, using new local outputs.
No owned EmuHawk remains afterward. Final ROM verification, all 450 release members,
shared file metadata and backed-up settings bytes pass. No additional restoration
was needed beyond the existing instrumented-H3 database cleanup, and no emulator
release copy was created.

Verification accounting remains explicit: the normal gate passed (148 tests),
and the direct H3/preflight results above are complete. The attempted full profile
was interrupted during Python verification when the volume filled; its partial
failure stream has no completed node summary and it never reached H1/H2/H3. It was
not blindly restarted. Focused checks exposed stale preparation mocks and a
hardcoded ROM path, corrected in the existing affected tests. The stale callback-owner
source-shape helper and its two coupled mutation cases
are retired under the current verification-tool test policy. It incorrectly put
optional `candidate:*` registrations in the ordinary failure-role enum. The retained
configuration test checks output exclusion and separate extension ownership;
production `map3_messenger_acceptance._assert_lua_roles()` checks required roles
and shared-PC dispatch, while `_failure_diagnostic()` validates emitted ordinary
failures against the unchanged schema. These checks do not claim the retired
helper's exact source-text closure for extension roles/phases. Run the affected
file with `uv run --locked pytest tests/python/test_map3_messenger_acceptance.py -q`
and the production role check directly after same-process private-input setup.
[#512](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/512) retains the
original failure and node disposition; the actual messenger H3 command already
passed with unchanged Lua/schema. No schema/golden expansion is needed.
The planner's H1/H2 fanout comes from CLI help text;
their execution semantics, source and inputs are unchanged. This is not a claim
that the interrupted full profile passed.

Reproduction uses the maintained preparation, observer, bridge and segment APIs
after same-process private-input configuration. The ignored `local/issue510/`
recipes retain `run_short.py` (the prior short input recipe without its launcher
substitution), `segment.py parent prepare/run`, `segment.py child prepare/run`,
`read_segments.py`, `check_settings.py` and `check_marker.py`. The segment input
comes from the retained house-exit/classroom request records, not a full Battle01
acquisition; use fresh output names and serial installation ownership when
reproducing. `short-report.json`, `segment-report.json`, settings/marker reports
and the original native receipts retain the results. Config/state/input payloads
remain private and are not public fixtures. These checks establish launch and
continuation compatibility, not new original-game or H4 acceptance.

### Earlier Isolation Probe

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

## Single-volume local layout

[#2370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2370) defines the target for
the Windows operator host. Consolidation uses the configured project volume. The configured
selections and their owners still describe current operation until each cutover is verified.
Keep machine paths in ignored configuration and receipts. The
[asset pipeline workflow](../../remake/docs/content/asset-pipeline-workflow.md#directory-responsibilities)
owns media subdirectories, publication write ownership and the pilot-to-cutover sequence.

| Location relative to the operator's project root | Owner and contents |
| --- | --- |
| `local/shared/roms/` | Registered immutable ROM selection, with its existing identity check |
| `local/shared/sources/<role>/` | Durable private corpora and independent asset, graphics or audio repositories. Each retains its own history, writable ownership and private boundary. |
| `<main-checkout>/private-media/` | Canonical private image/audio payload and the closed world, scene, entry and catalog files that select it. The main repository tracks the [asset metadata](../../remake/docs/content/asset-inventory.md#shared-package-retention). |
| `local/shared/media` | The single Windows NTFS junction pointing to the canonical checkout's `private-media/`. All worktrees read the same payload through this entry. |
| `local/shared/packages/<role>/<version>/` | Closed Windows delivery and other operator publications outside the canonical media root. |
| `local/shared/toolchains/` and `local/shared/dotnet-cli-home/` | Selected installations, required distributions and SDK-owned CLI state. Keep the existing root contracts and update their configured selections together. |
| `local/player-data/` | Operator-owned writable save roots and their backups. Keep real namespaces, settings and backups separate from installed builds and disposable saves. |
| `local/runs/<owner>/<run>/` | Explicit external namespaces for probes and disposable saves that must sit outside Git. The checkout or operator holding the run owns their cleanup. Other checkout scratch stays in that checkout. |

Use an existing role directory when possible. Keep independent repositories outside removable
worktrees, preserve Git metadata and local-only files, and verify the expected commit and working
state before redirecting a producer. Existing selectors may remain temporarily at their old roots
during staged cutover. Preserve separately designated media projects, source recordings and masters
at their operator-owned paths.

Use the ignored checkout-local `local/private-inputs.ps1` to select the shared root via
`SF2_SHARED_INPUT_ROOT`. The [tracked runtime selection](../../remake/docs/content/metadata/selection.json)
resolves all four runtime inputs through the verified `media` junction. Use the
[reader-input block](../../remake/docs/content/asset-inventory.md#pre-voyage-package-locations)
for direct Godot launches. Keep machine launcher overrides under their operator's ownership. The canonical checkout's `private-media/` is Git-ignored
durable data. Keep each package's image and audio paths relative to its content documents. The
`media` junction is the only linked entry in this layout. Use real directories for checkout
scratch, `local/shared` itself and independent Git repositories. Keep authored and licensed
resources used by the public project tracked with their provenance.

Before switching this entry, verify the linked target is the intended real `private-media/`
under the canonical checkout. Keep the junction's location and resolved entity out of normal
scratch cleanup. A cleanup of the shared parent must skip reparse points. To retire the junction,
verify that exact link and unlink the entry only. Retirement of the entity requires a separate
decision for the canonical media directory.

### Player profiles, fixtures and runs

| Relative to the operator project root | Writer and lifetime |
| --- | --- |
| `local/player-data/saves/<profile>/` | The player owns one writable namespace across builds. Numbered slots, sidecars and ordinary settings travel together. Choose the same existing directory when changing checkout or installation. |
| `local/player-data/backups/` | Keep a complete independent copy before migration or repair. Label it with the source profile, compatible build or content selection, reason and recovery window in private operator records. The store already rotates a slot's previous primary into its adjacent `.bak` on ordinary replacement. |
| `local/player-data/fixtures/<scenario>/slotN.json` | Custodian-owned, read-only private seed for a named current replay that needs serialized history. The [scenario table](../../remake/docs/application/pre-voyage-campaign.md#showcase-starts-and-continuity) records intent and provenance. A controlled campaign-field start supplies reconstructible flags, actors and position. Retain a seed when the claim depends on saved party resources, a battle snapshot, legacy compatibility or the original reported state. |
| `local/runs/<owner>/<run>/saves/<namespace>/` | One disposable writer per journey. Copy a numbered fixture or a profile into a fresh namespace before testing. Cold Continue reuses only that journey's saves. Each process gets its own logs and TEMP. |

**Manual selection.** From the intended checkout, set `$profileDirectory` to the absolute existing
`saves/<profile>` directory in the operator's ignored manual-play configuration, then run:

```powershell
& ./remake/tools/select_save_profile.ps1 -ProfileDirectory $profileDirectory
```

The [selector](../../remake/tools/select_save_profile.ps1) checks the existing directory and
sets both `SF2_SAVE_ROOT` (its parent) and `SF2_SAVE_NAMESPACE` (its basename) in this process.
Choose this profile explicitly for ordinary Debug or Release. An installation launcher that clears
development variables must pass those two selected values into its child environment after clearing.
Keep build-specific executable and media selections independent. The tracked selector reads no slots
and creates no profile. An unconfigured export retains its [standalone storage default](../../remake/docs/windows-local-candidate.md#build-and-run).
[Automated Debug journeys](../../remake/docs/verification/debug-port.md#development-default) create their
own namespace even when launched from a shell with an active manual selection.

**Confirmed.** The operator's ordinary Debug and Release launchers now read the same manual
profile selection. Their resolved values and child-environment selection were checked without
starting either player launcher. An independent checkout's Debug build continued and saved on a
complete disposable profile copy. The prior primary became its byte-identical slot backup, and
the installed Release restored the saved field in a fresh process. All files in the real
namespace, including preferences and backups, matched the pre-run baseline byte for byte.
A separate fixture journey restored the registered Battle01 snapshot to first-round Movement.
The cold battle process quit normally and Godot reported two ObjectDB instances at shutdown.
The original fixture bytes remained unchanged. These observations cover the selected save
formats and builds. Other content versions retain their ordinary admission checks.

**Recovery.** Stop all writers to the selected namespace. Preserve its current contents, including
preferences and the newest progress, before opening a backup. Copy the backup into a fresh run;
use the intended build/content and the ordinary slot list, recovery confirmation and Continue.
Keep the original slot number. The existing slot lock, compatibility checks, rotating backup and
incompatible-record preservation remain in force. If a launcher or executable must roll back,
first try the previous build on a copy of the newest progress. Switch only after comparing the
results and resolving incompatible or divergent progress with the player. A saved backup predating
the switch can be retired by its owner after recovery is verified, newer progress is reconciled
and the agreed rollback window ends.

**Cleanup.** Main-gate reviews exact completed run paths. Require an idle writer, resolved
containment within the designated runs root, no reparse points in the target or ancestors, and
release of any sibling cold-Continue dependency. Exclude player data, fixture sources, video
projects and independent repositories. An old handoff may contain a historical seed or other
unique work. Retire only the reviewed seed file after a byte comparison and consumer switch.
Keep a still-consumed seed even when its originating Issue has closed.

### Cut over a local selection

Plan space for the largest concurrent copy, validation scratch and rollback copies. Enumerate the
actual consumers, including launchers, templates, the tracked asset inventory, build inputs, save
namespaces and package-relative resources. Record which version or location each change replaces.

With the owning writer quiesced, copy into a fresh destination and verify the complete selection.
Use the existing selection or package checker and byte comparison where required. For player
storage compare relative files and bytes, including settings, then exercise read/Continue and a
write/reload on a disposable copy. Keep a versioned backup and one authoritative writable namespace.
Switch the owner-approved launchers and configuration, then validate the affected ordinary entry
point with the old source unavailable to that launch. Record the tested selection and fallback.
If new saves were written after the switch, stop the writer and reconcile them with the retained
backup before reverting the launcher. Retire old sources only after acceptance and the authorized
[cleanup checks](github-project-governance.md#worktree-selection-and-retirement).

## Local State and Promotion

Keep writable source/build checkouts, Python/uv/NuGet environments and caches, TEMP, emulator state,
movies, traces, assets, reports and exports in their owning worktree. Only SDK-owned CLI state may
use the shared CLI home. Do not replace the whole `local/` directory with a shared writable tree.

Configure every retained worktree's ignored environment when selecting shared installations. Reuse
verified existing installations. Where an authorized promotion is needed, copy a complete verified
installation into an empty destination, verify the copy, and promote without overwrite. Preserve
the current rollback selection and decisive acceptance or failure evidence. Retire superseded
installations and generated copies after current consumers pass from the new selection. Deletion
and cleanup require separate authorization. A mismatch is a failure, never permission to overwrite an input.
