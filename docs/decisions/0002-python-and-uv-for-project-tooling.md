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
- phase milestones, release/merge readiness, shared harness changes, and explicit parity requests run
  `uv run sf2 verify --full`;
- related runtime cases share one generated case table and one BizHawk launch unless their setup or
  observation seams cannot safely be shared.

On the 2026-07-19 Windows research workstation, the default gate completed in roughly 3-6 seconds
after the research index changed from one full H1-listing scan per record to one symbol-map build per
run. The complete H1-H3 gate completed in 958.1 seconds. These are observed diagnostics, not pass/fail
thresholds; automation wrapping the full profile should allow at least 20 minutes rather than treating
a 15-minute caller timeout as a parity failure.

## Consequences

- New project tooling and tests go under `src/sf2tool/` and `tests/python/`.
- Root verification, index queries, and ROM checks no longer expose PowerShell commands.
- `uv run ruff check src tests/python` and `uv run pytest` are required narrow gates.
- `uv run sf2 verify` is the default ordinary-commit profile: it runs those Python gates plus
  design/index, ROM identity, and toolchain provenance. A changed runtime/extractor slice adds only
  its owning narrow command. `uv run sf2 verify --full` is the explicit
  milestone/release/shared-harness gate, not an every-commit default. The old `--quick` spelling is
  accepted as a hidden compatibility alias for the default profile.
- A fresh environment can validate tracked Python contracts without a ROM; full H0-H3 still needs
  the ignored local evidence and, during migration, PowerShell 7 for legacy rails.
- The compatibility layer may be removed only after its final caller is migrated.
