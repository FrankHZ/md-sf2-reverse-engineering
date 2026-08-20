# Godot AI Probe

This disposable Godot C# project tests whether an agent can author and verify a
small project with CLI gates, without an editor plugin or MCP. It exercises a
plain-C# turn-based domain object through a thin Godot adapter.

This is dated experiment/tooling material, not a `remake/` implementation or a
Phase 4 restore contract. It does not close the ADR 0009 pre-entry gap gate,
change readiness counters, select assets, install an MCP, or start Phase 4.

## Accepted baseline and probe boundary

[ADR 0008](../../docs/decisions/0008-godot-csharp-cli-first-remake-tooling.md)
accepts Godot 4.7.2 .NET/C# as the Phase 4 engine/tooling baseline while keeping
the implementation transition separate. This probe therefore requires the
exact evaluated editor build:

`4.7.2.stable.mono.official.ed1daf0bf`

The runner executes `Godot --version` and rejects any other output before it
creates a scratch directory, builds, or imports the project. That proves this
dated capability check against the accepted editor build. A future Phase 4
restore workflow still needs separately authorized, tracked toolchain inputs
that pin the editor artifact, .NET SDK, packages, and restore/build commands.

## Layout and output policy

- `project.godot`, `probe.csproj`, `Main.tscn`, and `src/` are immutable probe
  inputs during a run.
- `run_probe.py` copies only those nine declared inputs into a new ignored
  directory under `local/`, then builds, imports, and runs there.
- Generated `.godot/`, `bin/`, and `obj/` state therefore belongs to the scratch
  copy, not to the tracked probe source.
- An explicit `--work-dir` must be a nonexistent child of this repository's
  ignored `local/` root. Existing, outside-local, or colliding paths are rejected
  and are never deleted by the runner.

Without `--work-dir`, each invocation creates a unique directory below
`local/derived/godot-ai-probe/`. Scratch results remain local for inspection;
the caller decides when to remove them.

## Reproduce

```powershell
$env:GODOT_BIN = '<path-to-Godot-4.7.2-.NET-editor.exe>'
uv run python tools/godot-ai-probe/run_probe.py
```

An explicit fresh destination can be used when a stable diagnostic path helps:

```powershell
uv run python tools/godot-ai-probe/run_probe.py `
  --work-dir local/derived/godot-ai-probe/manual-20260820-1
```

The gate performs, in order:

1. exact Godot version preflight (15-second wall-clock timeout);
2. `dotnet build` in scratch (120 seconds);
3. Godot headless editor import in scratch (60 seconds);
4. two headless game runs (60 seconds each).

Each game self-quits after 60 `_Process` frames. `--quit-after 120` is retained
as an independent Godot 120-iteration safety cap. A timed-out process triggers
a bounded process-tree termination attempt (Windows uses
`taskkill.exe /PID ... /T /F` without a shell), followed by a direct-process
fallback and a separately bounded pipe reap. If cleanup still cannot finish,
the runner closes its capture handles and fails instead of waiting indefinitely.
stdout/stderr diagnostics remain bounded to their final 2,000 characters.

Expected game markers are identical across both runs:

```text
PROBE_READY seed=42
PROBE_DONE frames=60 x=5 y=0 score=364
```

## Reproduced environment (2026-08-20)

| Item | Reproduced value |
| --- | --- |
| Godot editor | `4.7.2.stable.mono.official.ed1daf0bf` |
| Godot executable SHA-256 | `45336315eb6f1a52a8923bc4f2ce8079a03dc4939dcb7d531047890f1f7cdfab` |
| .NET SDK | 10.0.204 |
| Installed .NET runtimes present on the host | 8.0.30 and 10.0.8 |
| `Godot.NET.Sdk` | 4.7.2 through NuGet |
| Probe target framework | `net8.0` |

On this one host, .NET host tracing showed hostfxr/CoreCLR 10.0.8 loading the
probe runtime configuration that requests .NET 8. That is a bounded machine
observation, not evidence that every .NET 10 host can run every net8 project and
not a Phase 4 compatibility policy. The related Godot
[#111246](https://github.com/godotengine/godot/issues/111246) report concerns a
different `net10.0` target scenario.

The probe uses `System.Random(seed)`. Its repeated result is asserted for the
controlled runtime above only. .NET does not promise the same algorithm across
runtime generations, so this is neither a long-term deterministic format nor a
normative Phase 4 gameplay RNG choice.

## Project-file observations

### Semicolon comments

The pinned Godot source recognizes `;` in the parser's comment branch
([`variant_parser.cpp` at commit `ed1daf0bf`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/core/variant/variant_parser.cpp#L1910)).
During the original one-shot experiment, replacing the leading semicolon lines
in `project.godot` with `#` lines coincided with failure to discover the main
scene; returning to semicolons restored the run. The failing variant is not
retained as an executable negative fixture, so this README does not generalize
that observation beyond the pinned parser fact and this probe.

### `assembly_name` for this probe

The probe's application name is `Godot AI Probe`, while `dotnet build` produces
`probe.dll`. The tracked `project/assembly_name="probe"` reconciles that specific
name/DLL mismatch. In the pinned 4.7.2 source,
[`Path::get_csharp_project_name()`](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/modules/mono/utils/path_utils.cpp#L232-L258)
reads the setting and otherwise falls back to the application name. This probe
therefore does not claim that every hand-authored C# project universally needs
an explicit setting; it verifies the explicit value required by this naming
choice.

## Context7 evaluation boundary

Context7 was consulted only as an optional documentation-navigation experiment.
On 2026-08-18, these exact public queries were tried against
`/godotengine/godot-docs`:

- `project.godot [dotnet] project/assembly_name`
- `project.godot comments`

The recorded responses were broad and incomplete navigation material. They did
not directly establish either pinned-source fact recorded above, and the indexed
documentation was not the accepted 4.7.2 source tree. The result is not proof
that Context7 would have prevented either iteration or that it is required for
this workflow.

Context7 remains optional and removable. The exact editor preflight, source
links, probe inputs, and local runner provide a bounded, repeatable, dated local
capability check. They are not a locked restore or supply-chain guarantee, and no
external documentation service is needed to execute the check.
