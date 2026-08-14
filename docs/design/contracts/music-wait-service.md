# Music-Wait Service Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded entry identity, ordered source macro
  operands, and post-sleep predicate loop described below
- Modernization: **Allowed** for engine-native asynchronous completion, events, futures, or
  callbacks behind a source-compatibility trace
- Unknown: caller admission, command transport and acceptance, sound-side flag lifecycle, audible
  completion, transition behavior, scheduling, elapsed time, failure handling, and presentation

## Purpose

This contract defines the smallest implementation-neutral service shape supported by the accepted
static evidence for `PlayMusicAfterCurrentOne`. It preserves the original source order and retry
predicate without turning the Mega Drive sound-command trap, `Sleep`, VInt scheduling, Z80 state, or
audible playback into evidence owned by this document.

The contract is deliberately a control-boundary contract. The two `sndCom` statements are ordered
source command-request/macro-operand identities. They are not proof that either request was
transported, queued, accepted, or completed by the sound side. Likewise, the tested byte is a named
source predicate. Its player-visible or audible meaning remains outside this evidence.

## Judgment Boundary

**Confirmed static:** the accepted fixture binds `PlayMusicAfterCurrentOne` to ROM address `0x16BE`
(`5822`) and records a source sleep argument of `3`. The pinned source places the following forms in
order:

1. `sndCom SOUND_COMMAND_WAIT_MUSIC_END`;
2. `sndCom SOUND_COMMAND_GET_D0_PARAMETER`;
3. `moveq #3,d0`;
4. `bsr Sleep`;
5. `tst.b WAIT_FOR_MUSIC_END`;
6. `bne @Wait`;
7. `rts` on the zero path.

The loop is post-sleep and post-request: it does not test the predicate before the first `Sleep(3)`
request. If the first observation is zero, the source-shaped trace still contains exactly one wait
request. If `k` observations are nonzero before the first zero observation, the trace contains
exactly `k + 1` wait requests.

**Inferred:** the source symbol, command names, and comments suggest a utility intended to defer a
requested music change until a current-music condition finishes. This is engineering intent only. It
does not confirm normal caller meaning, the sound driver's interpretation, or anything a player
hears.

**Unknown or excluded:** the admitted caller set; the source comment's `d0.w` music-index and `$FB`
previous-music meanings as a supported public API; command numeric encoding, trap transport,
disabled-sound behavior, acceptance, queueing, and processing; which component sets or clears
`WAIT_FOR_MUSIC_END`; whether the byte corresponds to audible completion; fade, transition, resume,
or silence behavior; `Sleep`/VInt cadence; elapsed time; interrupt latency; scheduling; reentrancy;
concurrency; cancellation; deadlock and recovery; register or CCR behavior beyond the visible source
writes and calls; UI, scene, story, persistence, accessibility, and presentation effects.

## Evidence Contract

This contract consumes only the following surface from
[`sf2-tech-services-static-v1`](../../../tests/fixtures/h2/tech-services-static-v1.json):

- `function.PlayMusicAfterCurrentOne`;
- `expected.serviceFacts.musicWaitSleepFrameCount`;
- `upstreamCommit` and `romSha256` provenance.

The fixture field name contains “FrameCount,” but this contract interprets its value only as the
source argument supplied to `Sleep`. Exact frame cadence and wall-clock duration are not established
by this fixture.

Bounded chronology is reviewed directly in the pinned
[`music.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/sound/music.asm)
and summarized by the owning
[`technical-services.md`](../../research/technical-services.md). The executable static verifier is
[`services.py`](../../../src/sf2tool/h2/services.py).

The verifier resolves the H1 entry identity and checks the named source fragments under the accepted
ROM SHA provenance. It does not establish byte-for-byte H1/ROM parity for this function body, macro
expansion, or instruction encoding. Those stronger comparisons are not Confirmed facts or H4
requirements here.

This contract does **not** consume `expected.resourceFacts`, `expected.soundDriverFacts`,
`expected.inputFacts`, `expected.randomServicesFacts`, `expected.sramFacts`, any other
`expected.serviceFacts` field, or `expected.runtimeQuestions`.

### Exact research-index denominator

The accepted fixture is linked directly to ten research records. This contract changes the semantic
association of exactly one:

| Record | Design ownership after this contract |
| --- | --- |
| `tech.services.music-wait` | this contract; currently unassociated before registration |
| `tech.services.byte-copy` | remains with [`byte-copy-service`](byte-copy-service.md) |
| `tech.services.resource-icon` | remains with `ui-graphics-asset-data` |
| `tech.services.resource-graphics` | remains with `text-and-font-system` |
| `tech.services.resource-text-trees` | remains with `text-and-font-system` |
| `tech.services.resource-title` | remains with `unused-technical-asset-data` |
| `tech.services.resource-base` | remains with `unused-technical-asset-data` |
| `tech.services.input` | remains with `input-system` |
| `tech.services.sram` | remains with `save-system` |
| `tech.services.thinking-rng` | remains with `randomness` |

Sharing the aggregate fixture does not transfer any sibling fact or association to this contract.

## Source-Shaped Control Sequence

### Ordered command-request identities

The two leading `sndCom` forms retain this source order:

1. `SOUND_COMMAND_WAIT_MUSIC_END`;
2. `SOUND_COMMAND_GET_D0_PARAMETER`.

These are macro-operand identities at the 68000 source seam. “Request” in this contract means only
that the source invokes the macro with that operand. It does not mean that the trap transported the
request, that a mailbox changed, that the Z80 accepted it, or that an audio queue exists.

The numeric command values, inline trap encoding, parameter payload, and the source comment's input
domain are not part of this contract's runtime model. A modern implementation may use typed commands
or direct service calls while retaining an abstract compatibility trace with the same two identities
in the same order.

### Post-sleep predicate order

After the two source requests, every source-shaped loop iteration has this order:

1. replace `d0` with the source wait argument `3`;
2. call `Sleep`;
3. observe `WAIT_FOR_MUSIC_END` as a byte;
4. repeat when that observation is nonzero;
5. return when that observation is zero.

The predicate is never a precondition for entering the first wait. This yields the exact abstract
trace relation:

| Observed predicate sequence | Wait requests | Source-shaped result |
| --- | ---: | --- |
| `[0]` | 1 | return after the first wait and zero observation |
| `[nonzero, 0]` | 2 | one retry, then return |
| `k` nonzero observations followed by `0` | `k + 1` | return only after the final zero observation |

An observation stream without a zero has no confirmed return in this bounded model. The contract
does not invent a timeout, cancellation, error, or fallback policy.

### Separate-owner wait semantics

The [interrupt/DMA/trap contract](interrupt-dma-and-trap-state.md) owns the accepted static behavior
of `Sleep` and `WaitForVInt`: a positive source argument repeats its wait handshake. This contract
retains only the call order and argument `3`. It does not consume the interrupt fixture, define VInt
cadence, or convert three source wait iterations into wall-clock time.

Likewise, the [audio-system contract](audio-system.md) owns sound-driver data, command selection,
bounded playback state, channel behavior, and the existing audible/timing Unknowns. This contract
does not reinterpret its predicate as audible completion.

## Implementation-Neutral Model

A conforming import may expose the static evidence as:

```text
MusicWaitServiceEvidence
  identity
    sourceSymbol = PlayMusicAfterCurrentOne
    sourcePath
    h1ResolvedEntryAddress = 0x16BE
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  orderedSourceCommandRequests[2]
    macroName = sndCom
    operandIdentity

  retryControl
    waitArgument = 3
    order = WAIT_REQUEST_THEN_PREDICATE_TEST
    predicateIdentity = WAIT_FOR_MUSIC_END
    repeatCondition = NONZERO
    returnCondition = ZERO
    waitRequestCountForKNonzeroThenZero = k + 1

  excludedRuntimeMeaning
    commandTransport
    commandAcceptance
    soundSideFlagLifecycle
    audibleCompletion
    elapsedTiming
```

This model distinguishes imported evidence from a remake runtime. Complete or exact source-body text
and dumps, full macro-expanded or instruction bytes, private ROM excerpts, and other non-public
round-trip verification material remain private inputs. The bounded symbol, H1 entry, macro-operand
names, wait count and order, hashes, and provenance listed in the public projection below remain
public metadata. After verification, a remake may use engine-native command objects, references,
events, futures, promises, or callbacks. It is not required to reproduce Mega Drive address space,
trap encoding, the original polling loop, VInt scheduling, or Z80 memory.

An engine-native implementation that waits on an event can still provide a compatibility adapter
that emits the abstract ordered command-request and wait/predicate trace. Such a trace proves model
equivalence only; it does not claim that the original used events or that either implementation has
the same real-time duration.

## Public and Private Projection

The public contract may retain:

- the source symbol and H1-resolved entry address;
- the two ordered macro-operand identity names;
- the source wait argument `3`;
- the post-sleep predicate order and `k + 1` trace relation;
- fixture identity, hashes, upstream revision, and bounded provenance.

The public form MUST NOT publish original source-body bytes, instruction encodings, macro-expanded
body bytes, private ROM excerpts, captured sound state, music data, decoded audio, emulator traces,
or copyrighted audiovisual content. Complete or exact source-body comparison and instruction-byte
comparison may be used privately by a future stronger verifier, but they are not accepted parity
evidence here.

## Cross-System Separation

- [`audio-system`](audio-system.md) retains sound commands, Z80 driver/channel state, bounded playback
  observations, and all audible, transition, fade, resume, priority, mixing, and timing boundaries.
- [`interrupt-dma-and-trap-state`](interrupt-dma-and-trap-state.md) retains sound-trap transport facts
  and `Sleep`/`WaitForVInt` control semantics. This contract does not consume that fixture.
- Menu, service, battle, map, story, and special-screen owners retain their own callers and scene
  meaning. Direct-caller inventory and natural reachability are outside this contract.
- `byte-copy-service`, input, randomness, SRAM, text/font, UI graphics, and unused technical assets
  remain sibling contracts whose facts and associations do not change.
- Sound-disabled behavior, command loss, queue pressure, reentrancy, cancellation, and failure
  recovery remain **Unknown** rather than being inferred from the source loop.

## H4 Acceptance Surface

A future implementation satisfies this contract when:

1. the accepted fixture identity, upstream commit, ROM SHA provenance, source symbol, and H1-resolved
   entry remain traceable;
2. the imported static model retains exactly the two ordered source command-request/macro-operand
   identities without claiming successful transport, queueing, or acceptance;
3. the compatibility trace preserves `WAIT_REQUEST_THEN_PREDICATE_TEST` order;
4. an immediate-zero predicate sequence produces exactly one wait request, never zero;
5. `k` nonzero predicate observations followed by zero produce exactly `k + 1` wait requests and
   return only after the zero observation;
6. every source-compatible wait request carries the source argument `3`, without converting that
   count into wall-clock or audible-duration parity;
7. synthetic cases cover immediate zero, one nonzero then zero, and multiple nonzero observations
   before zero, and report the ordered abstract trace;
8. an engine-native event/future/callback implementation is permitted and need not reproduce the
   original trap, polling, `Sleep`, VInt, Z80, address, or instruction microimplementation;
9. command processing, flag lifecycle, caller admission, audible completion, timing, failure,
   register/CCR behavior, and presentation remain separate evidence or explicit **Unknowns**;
10. public reports remain metadata-only and respect the private/copyright boundary.

This acceptance surface is intentionally silent on an endless nonzero predicate stream. A remake may
adopt a timeout or cancellation policy as a documented modernization, but it MUST NOT report that
policy as Confirmed original behavior.

## Evidence Matrix

| Claim | Evidence level | Owner | Remaining boundary |
| --- | --- | --- | --- |
| entry `0x16BE` / `5822` and wait argument `3` | **Confirmed static** | `sf2-tech-services-static-v1` | no function-body byte parity |
| two ordered `sndCom` operand identities | **Confirmed static source** | pinned `music.asm` plus technical-services owner | transport, queueing, acceptance, numeric encoding |
| post-sleep test and nonzero retry / zero return | **Confirmed static source** | same bounded source chronology | flag producer, lifecycle, runtime reachability |
| immediate zero gives one wait; `k` nonzero then zero gives `k + 1` | **Confirmed static derived control relation** | direct consequence of the accepted source order | real elapsed duration and scheduler behavior |
| “play requested music after current music” engineering role | **Inferred** | source symbol, operands, and comments | sound-side and audible confirmation |
| caller meaning, audio output, timing, transitions, failure, UI | **Unknown / separate owner** | audio, interrupt, and caller contracts | requires bounded evidence or deliberate design |

## Reproduction

```powershell
uv run sf2 h2 tech-services
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated outputs remain under ignored `local/derived/`. No ROM, source-body dump, audio capture,
music payload, emulator state, or other private/generated artifact belongs in the public contract.
