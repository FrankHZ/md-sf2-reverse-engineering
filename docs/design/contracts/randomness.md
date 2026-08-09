# Randomness Services

- Evidence date: 2026-08-09
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

**Confirmed:** the thinking-AI byte path uses `RANDOM_SEED_COPY`; its H2 source
shape reads one byte at that base address, sign-extends it before an unsigned
multiplication by 541 plus 12345, masks the result to one byte, and writes one
byte back at the same base address. The bounded
`GenerateRandomNumberUnderD6` service returns zero immediately for low-byte
ranges 0, 1, and 128--255; for 2--127 it retries until an unsigned byte is in
0..range-1. The upstream comment says the accepted lower bound is 2, which does
not match the static comparison. Existing action-choice observation for the
range-two branch is `tests/fixtures/h3/battle-ai-action-choice-v1.json`
(`sf2-battle-ai-action-choice-runtime-v1`). The independent ten-case runtime matrix in
`tests/fixtures/h3/random-services-v1.json`
(`sf2-random-services-matrix-runtime-v1`) confirms those range-low-byte early exits, the unsigned
range-two three-step retry, and the thinking exact-seed 57-step retry. It also resolves the byte lane:
the base-address byte is the big-endian seed-copy word's high byte. The original bounded helpers return
`d7=0` while retaining their helper-return seed-copy states (`$53C2` and `$985D` in the early rows).
Only the controlled source-shaped probe copy that follows each helper writes that returned byte into the
high byte, yielding `$00C2` and `$005D`; source-context text and diamond rows likewise preserve their
low byte. Neither helper changes `RANDOM_SEED`. The same accepted matrix enters the exact
`symbol_wait1` and Diamond-menu preambles, observes each source RNG call, copy, register restore, and
one `WaitForVInt` return, then diverts to its controlled continuation. It confirms those bounded seams,
not the surrounding original caller loops. The natural Battle Test route is setup-only for that probe;
it does not establish battle, UI, text/menu, timing, or story behavior.

**Unknown:** caller-visible timing, retry distribution outside the exact matrix seeds, normal full
text/menu/AI caller flow outside the two observed seams, and seed-copy lifetime or overwrite behavior
across caller families. They remain the one grouped
`random-services-unobserved-caller-context-and-seed-copy-lifetime` queue rather than new one-case
fixtures.

## Implementation Boundary

Keep the main seed and seed-copy state separate, make debug overrides explicit
test controls, and expose range-zero behavior separately from bounded sampling.
Do not encode the upstream comment's lower bound of 2 as a returned-value rule.
