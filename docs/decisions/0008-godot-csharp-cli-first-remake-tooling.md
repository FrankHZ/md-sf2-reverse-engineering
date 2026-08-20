# ADR 0008: Godot C# with a CLI-First Remake Toolchain

- Status: **Accepted**
- Decision date: 2026-08-14
- Baseline amendment date: 2026-08-20
- Investigation date: 2026-08-14
- Scope: prospective Phase 4 engine and development-tool boundary
- Experiment source: local `G:\Codes-godot\Project-Mech-Strike`

## Decision

Use **Godot 4.7.2 .NET** with C# as the accepted Phase 4 engine/tooling baseline for the prospective
desktop 2D remake. The user explicitly amended the original 4.7.1 selection after Godot 4.7.2 became
stable on 2026-08-18. This decision pins 4.7.2 rather than floating to whichever stable release exists
when Phase 4 is separately authorized. Any later version change requires an explicit compatibility
recheck and accepted decision update before project creation. Keep the maintained build and
acceptance path independent of an editor plugin:

1. restore and build C# with an explicit .NET SDK and locked packages;
2. import and run the project with the official Godot command line in headless mode;
3. keep deterministic domain tests outside the scene tree where practical;
4. treat MCP as an optional, replaceable development adapter for scene-tree inspection, runtime
   observation, log capture, input injection, and screenshots;
5. require the same build, test, and smoke gates to pass after the MCP adapter is removed.

This decision does **not** start Phase 4, create a `remake/` project, select a distributable asset
strategy, or accept a particular MCP implementation. Those actions require their own accepted
scope; Phase 4 implementation remains subject to a separate explicit start transition.

## Why This Investigation Exists

The earlier `Project-Mech-Strike` prototype showed that a Codex agent could implement a small Godot
C# game directly. Before carrying that workflow into this repository, the investigation separated
what the tracked project proves from what only existed in ignored machine-local configuration, then
re-ran its documented build and a bounded headless scene smoke.

The comparison matters because an MCP adapter can make editor feedback more convenient without being
a sound project toolchain. The remake needs a reproducible CLI contract first; editor automation is a
developer-experience layer on top of that contract.

## Project-Mech-Strike Audit Input

The audit used this exact local state:

| Item | Audited value |
| --- | --- |
| Repository | `https://github.com/FrankHZ/Project-Mech-Strike.git` |
| Local branch | `main`, clean, four commits ahead of `origin/main` |
| Local head | `eb4583234f85c19f85b5a6e3214cf2ac1a820ec7` |
| Local tree | `3bcbf6d9a63f5c85a6e7bc781298747bc46a8489` |
| Remote baseline | `origin/main` at `6f768e5b8784e884cda635f8faae7c82adb42243` |
| Local-only work | targeting, missiles, overdrive, and HUD commits |
| Godot contract | 4.6.2 Mono/.NET, C#, `net8.0` |
| Tracked implementation | 16 C# files and 9 `.tscn` scenes |

Reproduce the identity and tracked-surface audit with:

```powershell
$repo = 'G:\Codes-godot\Project-Mech-Strike'
git -C $repo status --short --branch
git -C $repo rev-parse HEAD 'HEAD^{tree}' origin/main 'origin/main^{tree}'
git -C $repo log --oneline --reverse origin/main..main
git -C $repo ls-files
```

The local-only commits are relevant because they contain four of the prototype's later gameplay
features. They are collaboration evidence, not accepted remote history.

## What the Prototype Actually Did

### Tracked, reproducible workflow

The tracked project documents and history show a simple direct-file workflow:

- C# scripts own behavior and use Godot lifecycle and node APIs directly.
- Small, targeted `.tscn` edits wire scripts, nodes, exported values, resources, and transforms.
- Each phase is a narrow commit that changes the owning scripts and scenes together.
- The documented command-line gate is only:

  ```powershell
  dotnet build 'game\Mech Strike.csproj'
  ```

- `game/project.godot` selects Godot 4.6, C#, Forward Plus, Direct3D 12, and Jolt 3D physics. Those
  renderer and physics choices belong to the 3D prototype and are not requirements for a 2D remake.

There is no tracked test project, CI workflow, Node package manifest or lockfile, Godot addon, export
gate, or scripted headless smoke. All 16 C# files depend directly on Godot; the prototype does not
demonstrate a separately testable domain layer.

### Machine-local MCP configuration

The prototype root contains an ignored `.codex/config.toml` created at 2026-05-02 02:48:06 UTC
(2026-05-01 21:48:06 in America/Chicago). It configures:

```toml
[mcp_servers.godot]
command = "npx"
args = ["@coding-solo/godot-mcp"]
env = { GODOT_PATH = "<absolute-machine-path-to-Godot-4.6.2>", DEBUG = "true" }
```

Only version/project discovery and project run/stop tools have explicit per-tool approval entries.
The project `.gitignore` excludes the entire `.codex` directory, so collaborators and later agents
cannot reproduce that configuration from the repository. The command also floats the npm package
instead of pinning a version and embeds a machine-specific executable path.

As of the investigation date, npm reports `@coding-solo/godot-mcp` only at `0.1.1`, published on
2026-02-03, with no npm release lock in the prototype. Its upstream repository is active and has a
large user signal, but publishes no GitHub releases or tags; its 2026-04-16 head includes a fix for
arbitrary GDScript instantiation through scene/node tools. Relevant sources are the
[`@coding-solo/godot-mcp` npm package](https://www.npmjs.com/package/@coding-solo/godot-mcp), the
[`Coding-Solo/godot-mcp` repository](https://github.com/Coding-Solo/godot-mcp), and the audited
upstream head
[`1209744`](https://github.com/Coding-Solo/godot-mcp/commit/1209744fad78f3998f98c7394fd0f6ef50da5281).

The repository does not retain agent transcripts or MCP invocation logs. Therefore:

- **Confirmed:** MCP was configured locally before the implementation commits, and the configuration
  could discover, launch, stop, and read output from Godot.
- **Inferred:** the agent primarily authored C# and small scene changes directly, using MCP or Godot
  execution as a feedback loop. This matches the tracked change shape and project instructions.
- **Unknown:** which individual commits or scene edits actually invoked an MCP tool. Git history cannot
  distinguish a direct text edit from an MCP-mediated edit.

The useful result is not that MCP generated the game. It is that direct C# and bounded scene editing
were sufficient, while optional runtime feedback kept the project runnable.

## Independent Reproduction on 2026-08-14

The investigation re-ran the local prototype with the configured Godot executable:

| Check | Result |
| --- | --- |
| `dotnet build 'game\Mech Strike.csproj'` | passed, 0 warnings and 0 errors |
| Godot `--version` | `4.6.2.stable.mono.official.71f334935` |
| headless editor/import smoke | exit 0; emitted a non-fatal `Scan thread aborted` shutdown warning |
| main-scene 120-iteration headless smoke | exit 0; emitted the expected damage diagnostic |
| Git status after validation | still clean; generated `.godot/` state remained ignored |

The first attempted `dotnet build --no-restore` failed with `NETSDK1127` after the installed .NET SDK
changed. A subsequent normal documented build succeeded, and the build remained stable afterward.
This is evidence that `--no-restore` is not a standalone reproducible gate without an explicit
restore/SDK/package-lock contract.

The bounded Godot commands were:

```powershell
$godot = '<absolute-path-to-Godot-4.6.2-.NET.exe>'
$project = 'G:\Codes-godot\Project-Mech-Strike\game'

& $godot '--headless' '--path' $project '--editor' '--quit-after' '1'
& $godot '--headless' '--path' $project '--quit-after' '120'
```

## Practices to Carry Forward

- Keep feature slices small and keep the project runnable after each slice.
- Put behavior in focused C# classes and use scenes for composition.
- Permit direct `.tscn` editing only for reviewable structural changes; use the editor or a validated
  adapter for complex resources, animations, signals, and visual tuning.
- Pair compile checks with an actual Godot import/run smoke. A successful C# build does not prove that
  scenes load, resources resolve, or runtime callbacks succeed.
- Keep generated `.godot/`, logs, captures, and imported assets outside the tracked boundary.

## Practices Not to Carry Forward

- Do not ignore the only durable description of the project's agent/tool integration.
- Do not use floating `npx <package>` as a maintained gate.
- Do not commit absolute executable paths or credentials.
- Do not make MCP availability a prerequisite for build, test, import, run, or export.
- Do not couple all deterministic game rules to Godot nodes when they can be tested as ordinary C#.
- Do not treat a zero-exit headless launch as gameplay parity; assertions and bounded state outputs are
  still required.

## Current Godot and MCP Boundary

The initial 2026-08-14 investigation selected Godot 4.7.1 while 4.7.2 was still a release candidate.
The official release archive now records Godot 4.7.2-stable on 2026-08-18, while 4.8 remains a
development line. The official editor CLI already owns project selection, import, headless
execution, fixed-frame runs, movie/frame output, and export. Godot C# requires the .NET-enabled editor
and a separately installed .NET SDK. See the official
[`Godot release archive`](https://godotengine.org/download/archive/),
[`Godot 4.7.2 maintenance release`](https://godotengine.org/article/maintenance-release-godot-4-7-2/),
[`command-line tutorial`](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html),
and [`C# prerequisites`](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html).

That date-stamped release observation does not create a floating-version policy. The accepted
baseline is 4.7.2; any later upgrade still requires a bounded compatibility recheck and explicit
accepted decision update.

The recent MCP ecosystem has capable candidates, but all remain optional evaluation targets:

| Candidate | Useful boundary | Adoption concern |
| --- | --- | --- |
| [`IvanMurzak/Godot-MCP`](https://github.com/IvanMurzak/Godot-MCP) | C# editor addon, CLI installation, scenes, build, runtime, logs, screenshots | cloud is the default mode; addon, NuGet, and server dependencies enlarge the trusted surface |
| [`tugcantopaloglu/godot-mcp`](https://github.com/tugcantopaloglu/godot-mcp) | broad Godot 4.7 and C#/.NET runtime/editor tool set | Node/TypeScript server plus game-side integration is a larger maintenance surface |
| [`beckettlab/beckett-godot-mcp`](https://github.com/beckettlab/beckett-godot-mcp) | no Node sidecar, C# build diagnostics, scene/runtime/log/screenshot loop | young implementation with limited independent adoption evidence |
| [`regiellis/godot-mcp-go`](https://github.com/regiellis/godot-mcp-go) | CLI-first Go binary, discovery, C# build, runtime/input/frame loop | only reached 0.9.0 during this investigation and is too new for a project default |
| [`Godot MCP Native`](https://store.godotengine.org/asset/yurineko73/godot-mcp-native/) | large native tool surface | publisher currently marks the release unstable |

The initial experiment should compare IvanMurzak and tugcantopaloglu because they currently provide
the strongest C# fit and broader adoption evidence. Beckett remains a useful low-dependency control.
No candidate should be vendored or added to the remake project before the experiment passes.

## Required Plugin Bakeoff Before MCP Adoption

Before adopting any MCP candidate, run each candidate against the same disposable project on the
accepted Godot 4.7.2 .NET baseline in an isolated worktree. Do not substitute a newer stable release
without the compatibility recheck and decision update required by this decision:

1. discover the exact Godot version and project root;
2. inspect an existing scene and C# type without changing files;
3. add one node and one C# script, save, then review the exact text diff;
4. compile C# and return structured diagnostics for an intentional error and its correction;
5. run a bounded scene, read stdout/stderr, inspect the remote tree, and capture one screenshot;
6. inject one declared input and verify one deterministic state change;
7. stop the game/server and prove that no process, callback, token, generated file, or export payload
   remains outside the declared scratch boundary;
8. remove the adapter and rerun the official CLI gates unchanged.

Reject a candidate if it requires a cloud account, cannot bind only to localhost, cannot pin all
executed artifacts, bypasses action approval, edits scenes outside Godot UndoRedo without a stable
diff, leaks development tooling into exports, or makes the official CLI path fail when removed.

## Phase 4 Baseline

A separately authorized implementation slice will own the engine project and its gates. That later
slice should:

- pin the evaluated Godot 4.7.2 .NET baseline and the selected .NET SDK in a tracked toolchain
  manifest; do not silently float to a newer stable release;
- use a desktop-oriented 2D renderer rather than copying Mech Strike's Forward Plus/D3D12/Jolt 3D
  choices;
- keep deterministic exploration and battle rules in a plain C# project referenced by a thin Godot
  adapter;
- add ordinary C# unit tests, a Godot import smoke, a bounded scene-state smoke, and an export smoke;
- consume canonical project-owned contracts and fixtures rather than private extracted assets;
- keep any MCP entry project-local and reviewable, but store machine paths and credentials outside
  Git;
- use an exact package/release/hash rather than a floating npm or Git default branch.

The first implementation acceptance profile should be CLI-only. MCP bakeoff results may improve the
developer loop later, but cannot define completion.

## Consequences and Remaining Decisions

The decision accepts one lesson from Project-Mech-Strike: modern Codex models can implement and
iterate on Godot C# directly without an editor plugin owning the project. It rejects the prototype's
non-reproducible local integration as a durable project contract.

Acceptance of this ADR selects the engine/tooling boundary but does not authorize the Phase 4
transition. [ADR 0009](./0009-first-phase4-playable-slice.md) selects the first playable milestone
and its pre-entry gap gate; Phase 4 still requires a separate explicit start action after that gate
closes. Asset licensing and replacement, save/UI scope, visual parity targets, and any optional MCP
winner remain separate decisions or acceptance slices.
