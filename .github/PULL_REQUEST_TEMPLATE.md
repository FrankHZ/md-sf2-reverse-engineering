## Scope

- Issue: <!-- Fixes #NNN only if merge satisfies the whole Issue; otherwise Refs #NNN -->
- Lane: <!-- Research / Design / Remake / Tooling / Governance -->
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
- [ ] Committed planner and scope-appropriate checks are reported with actual outcomes and skip reasons.
- [ ] Actual public CI/check state is recorded; required-check configuration remains main-gate owned.
- [ ] Staged paths and cached diff were reviewed.
- [ ] No ROM, patch, save, trace, extracted asset, downloaded tool, or generated/private artifact is included.
- [ ] `uv run sf2 verify --full` was run only if this is a milestone, shared-harness, or release-readiness change.

<!-- Use remake/docs/development-and-verification.md#scope and the owning research route:
docs use direct checks; engine uses behavior unit tests and affected direct verification;
research uses its normal and owning narrow gates. Do not run normal/full suites just to fill this template. -->

## Handoff

- Current Issue handoff: <!-- executor task, preserved failures, process/retention needs and stopping condition -->
- Independent acceptance: <!-- main-gate; stop at Review / Verify, before merge or archival -->
