# ADR 0002: Python and uv for Project Tooling

- Status: **Accepted**
- Decision date: 2026-07-17
- Scope: maintained extractors, verifiers, command-line tools, and root harness

## Decision

Use Python 3.12 or newer as the maintained project-tooling language. Use Astral `uv` for dependency
resolution, the committed lockfile, environment creation, and command execution. The stable public
entry point is:

```powershell
uv sync --locked
uv run sf2 verify
```

`pyproject.toml` owns direct runtime and development dependencies; `uv.lock` owns the complete
resolution. Do not add `requirements.txt`, Poetry, Conda, or an independently managed virtual
environment contract.

The initial lock was produced with `uv 0.11.18` and Python `3.13.14`; `.python-version` selects the
Python 3.13 line while `requires-python >=3.12` keeps the package contract explicit.

## Why

The initial evidence harness grew to 42 PowerShell files and 5,621 lines. PowerShell was useful for
bootstrapping old Windows-native assemblers and BizHawk, but JSON/schema work, binary parsing,
cross-platform path handling, test reuse, and a growing command graph are materially easier to
maintain and review in Python. A package and CLI also give the eventual remake pipeline one stable
tool API rather than a collection of filename-level entry points.

`uv` is already available in the working environment and provides a reproducible lock plus fast,
isolated command execution. The project currently needs only `jsonschema` at runtime; Ruff and
pytest are development dependencies.

## Migration Boundary

The first migration moves these owners into `src/sf2tool/`:

- CLI dispatch and root harness orchestration;
- design-contract traceability;
- research-index schema and relationship validation/querying;
- H0 ROM identity/header/checksum validation;
- pinned toolchain provenance checks.

The proven H1 build, H2 extractors, and H3 BizHawk observers remain temporarily executable through
the single `sf2tool.legacy.run_powershell` adapter. This is compatibility isolation, not a second
implementation strategy. No new PowerShell file or aggregate PowerShell line growth is accepted;
`tests/python/test_native_harness.py` enforces the current post-growth-migration ceiling of 36 files
and 4,813 lines. Each later migration must port a complete rail, compare its outputs with the existing
fixture, lower the ceiling, and delete the superseded script.

BizHawk observers are tracked Lua templates controlled by Python, not Python string rewrites. The
completed migrations are the base/debug-aware RNG and stat-gain/complete-level-up rails under
`src/sf2tool/h3/`, with tracked observers under `tools/bizhawk/`. Lua remains appropriate inside
BizHawk; host orchestration and validation belong in Python. Before Python opens EmuHawk, it compiles
both the tracked observer and generated configuration with BizHawk's pinned `lua54.dll`; syntax
failures therefore stop at the command line instead of surfacing in the interactive Lua Console.
Tracked observers also have a repository test that rejects Lua reserved words used through dot-field
syntax (for example, `.function`). Such keys must be renamed or accessed with bracket syntax. This
specifically prevents an observer template from reaching EmuHawk with the NLua
`<name> expected near 'function'` failure that prompted the guard.

## Verification Profiles and Observed Cost

The profile split is an operational contract, not merely a convenience:

- ordinary commits run `uv run sf2 verify` plus the one narrow H2/H3 command that owns the change;
  `verify` always runs Ruff across `src` and `tests/python`, then the shared critical target
  `tests/python/test_native_harness.py`; it is not a broad Python regression suite;
- `uv run pytest` runs the complete Python suite in one process when that broader check is required;
- phase milestones, release/merge readiness, shared harness changes, and explicit parity requests run
  `uv run sf2 verify --full`, which runs the complete Python suite with four process-isolated pytest
  workers before H1/H2/H3. `loadfile` scheduling keeps every test module and its module-scoped
  fixtures on one worker, and worker crashes fail instead of being silently restarted;
- related runtime cases share one generated case table and one BizHawk launch unless their setup or
  observation seams cannot safely be shared.

On 2026-08-01, the ordinary commit profile completed in about 6.8–7.2 seconds on the Windows research
workstation. The milestone full profile completed its 866-test Python suite in 1,450.80 seconds
(24:10) and the complete gate in 2,511.858 seconds (41:52). These are observed diagnostics, not
pass/fail thresholds. Automation wrapping the full profile must allow the complete Python suite plus
H1-H3; a 15-minute caller timeout cannot be treated as a parity failure.

Tracked schema files and their validators are immutable inputs during one Python worker process, so
the local-only schema registry and root validators are cached once per worker. Temporary schemas and
callers supplying an explicit registry remain uncached; mutation and registry-resolution tests
therefore continue to observe every on-disk change. The full profile reports its 25 slowest pytest
durations so later performance drift remains visible without turning wall-clock timing into a flaky
pass/fail threshold. Developers can reproduce only the parallel Python portion with:

```powershell
uv run pytest -n 4 --dist loadfile --max-worker-restart 0 --durations 25
```

Large composed contracts may keep complete mutation coverage without repeatedly copying and validating
their entire logical fixture. When a root schema first validates the accepted baseline and executable
tests prove its exact local `$ref` inventory, a mutation test may copy only the containers on the changed
path and validate the mutated object through the owning referenced section schema. Schema-valid semantic
mutations must still reach the complete domain verifier, and the test must prove that its shared baseline
was not modified. This is a traversal optimization, not permission to sample mutations, bypass the root
composition guard, or weaken missing/extra/boundary coverage. Applying that rule to the 24 schema-invalid
and ten schema-valid map-events mutations reduced the observed test call from 159.55 seconds to 29.27
seconds on 2026-08-03 while retaining every case.

## Consequences

- New project tooling and tests go under `src/sf2tool/` and `tests/python/`.
- Root verification, index queries, and ROM checks no longer expose PowerShell commands.
- `uv run ruff check src tests/python` remains the direct full Ruff gate, and `uv run pytest` remains
  the direct single-process complete Python-suite gate. `uv run sf2 verify` is the default
  ordinary-commit profile:
  it runs the full Ruff scan and only the shared critical `tests/python/test_native_harness.py`
  target, then design/index, ROM identity, and toolchain provenance. A changed runtime/extractor
  slice adds only its owning narrow command. `uv run sf2 verify --full` runs the full Python suite
  then H1-H3 as the explicit milestone/release/shared-harness gate, not an every-commit default. The
  old `--quick` spelling is accepted as a hidden compatibility alias for the default profile.
- A fresh environment can validate tracked Python contracts without a ROM; full H0-H3 still needs
  the ignored local evidence and, during migration, PowerShell 7 for legacy rails.
- The compatibility layer may be removed only after its final caller is migrated.
