## Scope

- Lane: <!-- research / design-synthesis / tooling / governance -->
- Base commit:
- Owned paths:
- Shared integration files:
- Branch dependencies and merge order: <!-- none, or name the stacked branch -->

## Change

<!-- What changed, why it changed, and its user/developer impact. -->

## Evidence and contract impact

- Evidence labels/provenance changed: <!-- no, or summarize Confirmed/Inferred/Unknown changes -->
- Fixture/schema/design-contract impact:
- Remaining questions or deliberate non-goals:

## Validation

- [ ] Branch updated onto current `main` before final acceptance.
- [ ] Public tracked-input checks pass.
- [ ] Owning narrow H2/H3 command passes, or this is a design/tooling/governance-only change.
- [ ] `uv run sf2 verify` passes in the local integration worktree.
- [ ] Staged paths and cached diff were reviewed.
- [ ] No ROM, patch, save, trace, extracted asset, downloaded tool, or generated/private artifact is included.
- [ ] `uv run sf2 verify --full` was run only if this is a milestone, shared-harness, or release-readiness change.
