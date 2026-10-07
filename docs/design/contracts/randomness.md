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

### Initial byte versus live caller state

**Confirmed (static source):** `code/common/scripting/text/textfunctions_1.asm`
`ParseSpecialTextSymbol:@wait2/loc_6472` and `symbol_wait1` write the result of main
`GenerateRandomNumber(range=256)` into the base-address byte of `RANDOM_SEED_COPY` before waiting.
`code/common/menus/diamondmenu.asm:ExecuteDiamondMenu/@loc_16` makes the same write.
These main-generator draws depend on `RANDOM_SEED`, not the previous seed-copy byte. Therefore
an executed poll kills dependence on the old high byte, while retaining the low byte; subsequent
thinking draws still depend on the newly written value. This is overwrite-before-read, not a
null-to-zero normalization rule.

The [admitted W2 caller evidence](dialogue-system.md#w2-composed-semantic-acceptance) places
Map3 opening text510/511 and before-battle text2292/2299/2303 on the selected route before battle
AI. The original thinking service's direct callers are the battle-AI standby, action-choice,
target-priority and special-attacker paths. For an ordinary inactive enemy,
`ExecuteIndividualTurn:@Call_StartAiControl` enters `StartAiControl:@NonSwarmAi`, then
`DetermineAiStandbyMovement`, whose first draw is range8. Those entry routines do not reseed the
copy. Player diamond-menu polls between a text wait and that AI can replace the text value again.
Consequently the opening byte's death does not establish which later writer supplied the first
AI draw, nor equality of two runs' complete AI history.

An initial-byte H4 diagnostic exemption requires the overwrite-before-first-effective-read
argument on **both** the original and the actual consumer path. A helper's correct arithmetic,
a text-only latch observation, or the existence of AI tests does not supply that binding.

**Confirmed (modern engine behavior):**
`SourceEnemyAiTests.FieldTextCopyReplacesInitialAiByteBeforeEntryAndAiKeepsItsUpdatedByte` starts a
declared field scenario and executes a before-battle W1/W2, normal battle entry, player Stay and
automatic SourceOrders AI. Changed initial high bytes `$03/$12` with identical remaining input
both become `$4E` at the text-copy commit. AI then consumes range8/2/2 values0/1/0, moves to(4,6),
records memory`$13`, and leaves high byte`$00`; each variant's other24 bits survive. These are
controlled engine inputs/results, not the original natural route's first-AI golden. Before the
copy binding, the same lawful W2 flow retained `$03/$12` into AI and produced respectively
Stay at(5,5)/memory`$00` and movement to(6,5)/memory`$34`. That counterexample rules out accepting
the old separate-latch behavior merely because text and RNG helper checks passed.

The [modern text owner](../../../remake/docs/application/session-and-programs.md) defines `ThinkingSeed` as
the live image and the nullable story copy as a last-text-write diagnostic. The actual current
byte is derived even in a standalone battle with no preceding text; null diagnostic evidence
must not hide an explicitly configured/executed modern value or be coerced into an original zero.
Retained complete-route captures from before this correction remain historical consumer evidence;
their seed/AI trajectories do not describe the corrected executable. Applying a diagnostic
exemption to H4 still requires binding the selected actual evidence to the corrected path.

**Unknown:** the selected original route's last effective copy producer before its first AI
draw. Modern ActionChoice's admitted sound edges do not implement original diamond-menu polling
or its RNG opportunities. No menu poll/CPU clock is synthesized by this text-copy correction.
The grouped lifetime Unknown above remains open for those callers; this bounded overwrite proof
does not establish complete menu/AI chronology or permit comparison against a whole historical
action sequence with different draw opportunities.
