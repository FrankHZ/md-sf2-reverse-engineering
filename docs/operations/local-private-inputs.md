# Local Private Input Layout

This guide applies [ADR 0006](../decisions/0006-parallel-worktrees-and-topic-branch-integration.md)
to machine-private inputs. It does not make private files distributable or turn a shared local tree
into repository state.

## Operational Now

`SF2_SHARED_INPUT_ROOT` may name one explicit absolute, machine-private directory. The operational
registered shared identities are:

```text
roms/sf2-us.bin
toolchains/jdk-17.0.19+10
```

The resolver treats the ROM as a file and the JDK as a directory. It rejects an empty, relative,
drive-relative, traversing, unregistered, missing, wrong-type, or post-resolution escaping path. It
does not discover sibling directories, create a junction, copy an input, or read the ROM payload.
The existing ROM verifier remains responsible for the accepted ROM size and hashes.

When the variable is absent, the default remains the worktree-local ignored
`local/roms/sf2-us.bin`. An explicit `--rom-path` still overrides either default. Configure a shell
with a machine-local value, never a tracked absolute path:

```powershell
$env:SF2_SHARED_INPUT_ROOT = '<absolute-machine-private-shared-root>'
uv run sf2 rom verify
```

Do not include the private root's absolute machine path or any private input filename supplied by the
user in a public handoff. Report the registered repository-owned identity and its allowed provenance
instead.

The Python `verify_toolchain` default validates the complete registered JDK tree before invoking its
existing version check. The durable tree digest uses POSIX relative paths in ordinal order. Each row
is `PATH<TAB>SIZE<TAB>UPPERCASE_SHA256`; rows are joined with LF and no trailing LF, then the UTF-8
manifest is SHA-256 hashed. The tracked toolchain manifest owns the exact count, size, digest, Java
relative path, and executable identity. An explicit Python `java_path` remains an override and does
not consult the shared root.

The frozen PowerShell bootstrap and H1 toolchain checks remain worktree-local. This slice does not
redirect them to the shared JDK or change their execution semantics.

## Per-Worktree Writable State

Only immutable inputs may be considered for shared read-only resolution. Keep these isolated beneath
each owning worktree's ignored local state:

- upstream checkouts used by H1, because rebuild and split steps write into them;
- extracted emulator installations and their configuration, save-RAM, movie, and capture state;
- derived ROMs, extracted assets, reports, gate receipts, and other generated output; and
- H3 scratch, instrumentation, and launch state.

Do not replace a worktree's complete `local/` directory with a junction or shared writable root.

## Proposed or Deferred

The following are prepared or possible follow-ups, not operational consumers of this resolver:

- shared pristine BizHawk archive resolution;
- contained per-launch BizHawk extraction and emulator state;
- an isolated H1 scratch/copy workflow that can consume a read-only upstream source; and
- deletion or deduplication of any existing worktree-local copy.

The primary repository is therefore not yet self-contained for normal or full verification merely
because the shared ROM and Python JDK are configured. Those profiles still require their currently
owned upstream and BizHawk archive/executable state. Full verification, H1, H3, Godot, and emulator
execution are unchanged.

For every later migration, use this order: copy into a temporary destination, verify the accepted
identity, promote without overwrite, switch one bounded consumer, rerun its gates, and only then seek
separate authorization to remove an old copy. A shared-path mismatch stops the migration; it is not a
reason to overwrite a known input or redirect writable output.
