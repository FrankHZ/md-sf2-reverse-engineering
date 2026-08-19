# Godot AI-Probe

A disposable Godot C# probe project used to evaluate agent-driven ("AI development
mode") Godot workflows: how far an LLM agent can get authoring a Godot C# project
directly (no editor plugin, no MCP) with only CLI gates, and whether an up-to-date
documentation index such as Context7 adds value.

Everything here is experiment material, not a Phase 4 `remake/` slice. Phase 4 is
still gated by ADR 0009 and requires a separate explicit start action. This probe
is a candidate "same disposable project" for the ADR 0008 MCP bakeoff.

## Layout

- `project.godot`, `probe.csproj`, `Main.tscn`, `src/` — the probe project
  (deterministic turn-based domain in pure C#, thin Godot adapter, bounded
  60-frame self-quit smoke).
- `run_probe.py` — the reproducible gate: `dotnet build` + Godot headless editor
  import + two bounded headless runs with asserted stdout and determinism.
- `.godot/` — generated import/build state, gitignored.

## Environment (2026-08-18)

| Item | Value |
| --- | --- |
| Godot | `4.7.2.stable.mono.official.ed1daf0bf` (machine: `G:\Godot_v4.7.2-stable_mono_win64\`) |
| .NET SDK | 10.0.204 (single SDK on the machine) |
| .NET runtime | 8.0.30 present; hostfxr used is `10.0.8` |
| Godot.NET.Sdk | 4.7.2 (via NuGet) |
| Target framework | net8.0 |

Note: ADR 0008 pins Godot 4.7.1 for the remake; 4.7.2 was a release candidate at
the investigation date and needs the documented compatibility recheck before any
adoption. This probe ran on 4.7.2 because that is the machine's current editor.

## Reproduce

```powershell
$env:GODOT_BIN = 'G:\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe'
python tools/godot-ai-probe/run_probe.py
```

Expected pass output includes `PROBE_READY seed=42` and
`PROBE_DONE frames=60 x=5 y=0 score=364` twice (identical), exit 0.

## Findings (DeepSeek Flash, one-shot authoring without docs lookup)

The whole project was authored from the model's built-in knowledge with no
reference lookup, then iterated against the gates below. Two real knowledge gaps
were found and fixed; both are cheap to avoid with an up-to-date docs index.

### Gap 1: project.godot comment syntax

**Symptom:** with `#` comment lines at the top of `project.godot`, the run failed
with `Can't run project: no main scene defined` even though `run/main_scene` was
present. Switching the comments to `;` fixed it.

**Root cause (confirmed in Godot 4.7 source):** ConfigFile parsing delegates to
`core/variant/variant_parser.cpp`; `if (c == ';') { //comment }` is the only
comment branch. `#` lines are not comments; a leading `#` corrupts parsing.

**Lesson:** Godot config files use `;` comments only. The official docs describe
the file as "INI/win.ini format" but do not state the `;`-only rule directly;
the tscn format page does state it for `.tscn` files.

### Gap 2: [dotnet] project/assembly_name is required

**Symptom:** with a hand-authored `project.godot`, Godot reported
`.NET: Failed to load project assembly` and every C# script failed with
`Cannot instantiate C# script because the associated class could not be found`,
even though `dotnet build` produced the assembly at
`.godot/mono/temp/bin/Debug/probe.dll`.

**Root cause:** the editor normally writes `[dotnet] project/assembly_name="..."`
into `project.godot` when a C# project is created. A hand-written project file
must include it; without it the runtime cannot resolve the project assembly.
Confirmed by comparing against the working `Project-Mech-Strike` project
(`game/project.godot` has `[dotnet] project/assembly_name="Mech Strike"`).

**Lesson:** when scaffolding a Godot C# project by hand (the CLI-first path),
remember the `[dotnet]` section. `dotnet build` succeeding is NOT enough.

### Environment notes

- The machine's hostfxr is .NET 10 (`10.0.8`). A net8.0 Godot project loads fine
  through it once `assembly_name` is set. The known Godot issue
  [godotengine/godot#111246](https://github.com/godotengine/godot/issues/111246)
  ("Failed to load project assembly" with .NET 10) is about targeting `net10.0`,
  which this probe does not do.
- PowerShell quirk: `& <godot.exe>` returns immediately (GUI subsystem app);
  use `Start-Process -Wait -PassThru` (or `subprocess`) to capture exit codes.
- Godot 4.6.2 .NET (the ADR 0008 audit version) reproduces the same failures and
  the same fix — the gaps are project-file content, not the engine version.

## Context7 evaluation

Context7 indexes Godot documentation:
`/godotengine/godot-docs` (official docs, branch 4.5, refreshed 2026-08-04) and
`/godotengine/godot` (engine source, versions through 4.6-stable, refreshed
2026-08-09). Querying the public API
(`GET https://context7.com/api/v2/context?libraryId=/godotengine/godot-docs&query=...`):

- "project.godot [dotnet] project/assembly_name" returned the
  `class_projectsettings.rst` dotnet/project settings domain and
  `c_sharp_basics.rst` (C# project file generation) — would have led to Gap 2
  quickly.
- "project.godot comments" returned the INI/win.ini format descriptions and the
  tscn semicolon-comment rule — indirect for Gap 1; the engine-source library
  (variant_parser.cpp) would state it directly.
- The docs library tracks 4.5, not the 4.7.x line; API and project-file docs are
  stable across those versions, so this matters little for this probe, but a
  future compatibility check should confirm index freshness.

**Verdict:** Context7 would have saved the two fix iterations (roughly half the
debugging) by pointing at the owning doc pages. It is a developer-experience aid,
not a project dependency: the durable contract here is the probe project plus
`run_probe.py`, which reproduce and verify everything without any external
service. Keep machine paths (GODOT_BIN) out of tracked files.
