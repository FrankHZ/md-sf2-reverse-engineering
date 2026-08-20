# Map 3 Admitted-Start Evidence

## Scope and provenance

This owner records one **controlled** continuity seam for R1 of the ADR 0009
Map 3 through Battle 01 audit. It is not a claim that a player naturally
reaches Map 3 by this route, nor a substitute for R2's route evidence.

- Canonical private input: US ROM SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Source baseline: `SF2DISASM` commit
  `c834c652b6862bc5679fd7f69a38a7093206efc6`.
- Runtime: BizHawk 2.11.1 / Genesis Plus GX, one deterministic launch of
  `uv run sf2 h3 map3-admitted-start --timeout-seconds 180`.
- Public evidence: fixture state facts, source/ROM hashes, and trace shape
  only. The ROM, session ROM, SRAM, captures, and emulator artifacts stay
  ignored under `local/`; no asset or capture payload is tracked.

The fixture is
[`map3-admitted-start-v1.json`](../../tests/fixtures/h3/map3-admitted-start-v1.json).
Its verifier independently derives the source, H1, and ROM seams before
launch; the Lua config contains no accepted observation or golden corpus.

## Controlled admission boundary

**Confirmed:** one case, `controlled-new-map3-default`, reaches the first
original `WaitForEvent` after this exact observed chronology:

```text
CheckSram (0x006EA6)
→ witchMenuAction_New (0x007406)
→ NewGame (0x009736)
→ SaveGame (0x006F6A)
→ MainLoop (0x0075C4)
→ ExplorationLoop (0x0257C0)
→ RunMapSetupInitFunction (0x0474FC)
→ setup-resolution return (0x047504)
→ original indirect init call (0x047512)
→ ms_map3_InitFunction (0x051382)
→ original indirect init return (0x047514)
→ WaitForEvent (0x02591C)
```

The harness reaches the original `CheckSram` entry, replaces only that call's
post-return continuation with a generated work-RAM checkpoint, and then admits
one original Witch/New action. Its menu returns are controlled to
`initial=1`, `difficulty=0`, with save flags `0`; name/text waits are
session-only return seams. `NewGame`, `SaveGame`, `MainLoop`, `ExplorationLoop`,
the setup wrapper, and the selected init call/return remain original code and
are callback-observed. This is therefore **Confirmed** as a controlled seam,
while normal title/menu input, naming, text pacing, and natural story reachability
remain **Unknown**.

## Admitted snapshot and setup join

**Confirmed:** at the first observed exploration wait, `CURRENT_MAP` and
`EGRESS_MAP` are both `3`; the original MainLoop handoff registers are
`D0/D1/D2/D3/D4 = 3/56/3/3/1`. The fixture records the entire scenario-relevant
state vector: player entity `(x=0x5400, y=0x0480, facing=3)`, current gold,
difficulty flags, all 30 joined/active bits, all 30 ally records including
class/level/current-and-max HP/MP, and source-faithful current battle fields:
HP values are words, while MP, attack, defense, agility, and move are bytes;
it also records four item bytes, four spell bytes, and RNG/time state. `RANDOM_SEED`
is observed as its exact four-byte span. `FRAME_COUNTER` and
`SECONDS_COUNTER_FRAMES` are each one byte, while `SECONDS_COUNTER` is a
longword. The snapshot and restoration proof preserves those four spans
separately; it does not sweep the unrelated bytes between the frame counter
and RNG storage.

**Confirmed (controlled normalization):** VInt can advance its byte and
longword counters between otherwise identical launches. The observer reads the
three original spans at the first wait, then, outside every callback and only
after that original boundary, writes and readbacks zero to those three exact
spans before emitting the public fixture. The subsequent scoped core-state
restore proves the session returns to the pre-write state. The golden's zeroed
time values are therefore an explicit controlled normalization, not a claim
that the raw VInt values are fixed or a natural-game time snapshot. The pinned
`vint.asm` byte/long updates, longword timer-window read, and H1/ROM opcode
guards define the widths;
the original RNG generator's word update remains distinct from the separately
observed four-byte RNG span.
In this controlled snapshot only ally 0 is joined and active; the exact values
are fixture-owned rather than duplicated here.

**Confirmed:** the static sourcePath corpus contains exactly 26
`data/maps/entries/map03/*` records. The observed state has every guarded
selector flag clear, so the source-ordered Map 3 selection chooses the default
setup at `0x050AE8`; its original init pointer is `ms_map3_InitFunction` at
`0x051382`. During that selected init execution, callbacks fail closed on both
guarded script targets and the guarded `MoveEntityOutOfMap` helper entry; none
occur before the first `WaitForEvent` (`programRequest = "none"`). The verifier
checks the default and flag-506/543/609 selector rows from the static source,
but does not claim the latter rows were traversed.

**Unknown:** the narrative meanings and later effects of flags 506/543/609,
normal route selection, dialogue/menu/event chronology, persistence, rendered
output, audio/timing, and any map-to-battle consequence. R2 owns natural route
evidence; this slice neither associates the 26 Map 3 records with design
contracts nor promotes a controlled seam into natural reachability.

## Runtime integrity and private-reference foundation

**Confirmed:** callbacks use one deterministic registration per physical PC;
pre-admission bootstrap callbacks are explicitly diagnostic no-ops, while every
post-admission transition is phase-checked. Callback exceptions defer to one
structured failure status outside the memory-callback context. The success
status is one exact, unique ordered sequence through the snapshot, core-state
checkpoint, original call chain, init return, first wait, callback cleanup, and
terminal exit. The passing run ended with `callbacks-cleared:0`, no Lua Console
error, and a deleted disposable session ROM. Failure status is structurally
closed by phase/role, case, PC, callback-count/output cleanup, scoped-restoration
truth, and a typed first mismatch when restoration fails. Restoration is scoped
to game flags, the 30 complete ally records, map/battle bytes, player entity,
gold, the four independently scoped time/RNG spans, and the generated work-RAM
span; it makes no all-RAM, SRAM-persistence, or presentation claim.

This establishes only the reached RA-11 foundation: fixture provenance records
the private ROM hash, pinned source commit, BizHawk/core, controlled input
condition, and first-wait timing condition. It deliberately does not capture or
publish pixels, palette/VDP/DMA state, animation cadence, audio, or any private
payload. Those 8C fields remain **Unknown** until an accepted route/battle slice
defines private capture and comparison conditions.
