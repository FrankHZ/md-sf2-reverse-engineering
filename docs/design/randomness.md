# Randomness Services

- Evidence date: 2026-07-20
- Source baseline: `ShiningForceCentral/SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract

**Confirmed:** the main generator advances the 16-bit `RANDOM_SEED` as
`(seed * 13 + 7) & 0xFFFF`, preserves the caller's `d6` range register, and
scales from the upper product word after doubling that range. The observed base
behavior is recorded by `tests/fixtures/h3/rng-v1.json`
(`sf2-rng-generate-random-number-v1`), while the complete static service shape
is recorded by `tests/fixtures/h2/tech-services-static-v1.json`
(`sf2-tech-services-static-v1`).

**Confirmed:** debug mode checks directions in Right, Up, Left, Down priority and
returns 0, 1, 2, or 3 without advancing the base seed; disabled debug mode or no
direction falls back to the base generator. The observed override/fallback and
register boundary is `tests/fixtures/h3/debug-rng-v1.json`
(`sf2-rng-debug-override-v1`).

**Confirmed:** the thinking-AI byte path uses `RANDOM_SEED_COPY`; its source
label `GenerateRandomValueSigned` sign-extends a low byte before an unsigned
multiplication by 541 plus 12345, masks and stores the low byte. The bounded
`GenerateRandomNumberUnderD6` service returns zero immediately for low-byte
ranges 0, 1, and 128--255; for 2--127 it retries until an unsigned byte is in
0..range-1. The upstream comment says the accepted lower bound is 2, which does
not match the static comparison. Existing action-choice observation for the
range-two branch is `tests/fixtures/h3/battle-ai-action-choice-v1.json`
(`sf2-battle-ai-action-choice-runtime-v1`).

**Unknown:** retry iteration counts/distribution, caller-visible timing, and
whether the seed-copy state is isolated across text, menu, and AI scenarios.
They remain one grouped H3 matrix rather than new one-case fixtures.

## Implementation Boundary

Keep the main seed and seed-copy state separate, make debug overrides explicit
test controls, and expose range-zero behavior separately from bounded sampling.
Do not encode the upstream comment's lower bound of 2 as a returned-value rule.
