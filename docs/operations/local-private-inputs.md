# Local Private Input Layout

This guide applies [ADR 0006](../decisions/0006-parallel-worktrees-and-topic-branch-integration.md)
to machine-private inputs. It does not make private files distributable or turn a shared local tree
into repository state.

## Operational Now

After the shared-ROM resolver is merged, `SF2_SHARED_INPUT_ROOT` may name one explicit absolute,
machine-private directory. The only registered shared identity is:

```text
roms/sf2-us.bin
```

The CLI resolves that file beneath the configured root for its default `--rom-path`. It rejects an
empty, relative, drive-relative, traversing, unregistered, missing, or post-resolution escaping path.
It does not discover sibling directories, create a junction, copy a file, or read the ROM payload.
The existing ROM verifier remains responsible for the accepted size and hashes.

When the variable is absent, the default remains the worktree-local ignored
`local/roms/sf2-us.bin`. An explicit `--rom-path` still overrides either default. Configure a shell
with a machine-local value, never a tracked absolute path:

```powershell
$env:SF2_SHARED_INPUT_ROOT = '<absolute-machine-private-shared-root>'
uv run sf2 rom verify
```

Do not include the private root's absolute machine path or the ROM filename supplied by the user in
a public handoff. Report the registered repository-owned identity and its allowed provenance instead.

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

- shared JDK and pristine BizHawk archive resolution;
- contained per-launch BizHawk extraction and emulator state;
- an isolated H1 scratch/copy workflow that can consume a read-only upstream source; and
- deletion or deduplication of any existing worktree-local copy.

The primary repository is therefore not yet self-contained for normal or full verification merely
because the shared ROM is configured. Those profiles still require their currently owned upstream
and toolchain state. Full verification, H1, H3, Godot, and emulator execution are unchanged.

For every later migration, use this order: copy into a temporary destination, verify the accepted
identity, promote without overwrite, switch one bounded consumer, rerun its gates, and only then seek
separate authorization to remove an old copy. A shared-path mismatch stops the migration; it is not a
reason to overwrite a known input or redirect writable output.
