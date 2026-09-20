# Map 3 Messenger Acceptance

- Status: **Confirmed** for the one admitted R2a continuation only
- Fixture: sf2-map3-messenger-acceptance-runtime-v1
- Case: natural-map3-messenger-accept-to-follower-ready-wait
- ROM: USA retail SHA-256 9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9
- Source baseline: ShiningForceCentral/SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6

## Boundary

This is a continuation of the accepted R1 admitted-start and R2 natural-opening fixtures. It begins
at the original ExecuteMapScript callback at ROM 0x4712C, with A0 = cs_5149A (0x5149A), before
the first script word is interpreted. It ends only after Map3_ZoneEvent8 sets flag F603 at 0x50EE4,
returns at 0x50EE8, and the original WaitForEvent at 0x2591C is stable.

The public fixture retains the accepted R1 and R2 fixture bytes and projections as prelaunch and
golden-boundary guards. It does not promote either prefix again. At R2 entry, F603=false and the
field menu remains **NotReached**.

## Confirmed source and runtime facts

**Confirmed:** scripts_1.asm:cs_5149A is ROM 0x5149A..0x51651 (440 bytes, SHA-256
01C2ACC81830937BDD6510F88F9FA4E4BF67D6E8F1E49A6693BAEF19B88068AA). The accepted
original-default path contains 116 parsed operations. Its prompt path is csc11_promptYesNoForStoryFlow
at 0x47490 → YesNoPrompt at 0x15284 → SetFlag F89 → csc0C_jumpIfFlagSet at 0x47418 →
cs_51614; the original default-zero return is the accepted choice. Decline/re-prompt is
source/H1/ROM-only, not a runtime claim.

**Confirmed:** source/H1/ROM guards fix the accepted order F600, F66, csc08_joinForce selector 128,
Sarah then Chester JoinForce at 0x9956, the two followentity commands, guards 138/139, csc_end at
0x51650, and the Zone Event 8 commit. The observer requires service and return callbacks; it never
infers completion from callee entry.

**Confirmed:** the one runtime case records 17 reached map-script text commands with IDs 517–531 and
535–536, plus csc08 join text 447. Public data contains IDs, raw source-compatible speaker operands,
and a control-shape hash only. A speaker operand is the source macro modifier/entity word: the Sarah
portrait form is 0xC001, not a normalized character ID. Dialogue prose, captures, assets, and audio
remain private or unobserved.

**Confirmed:** runtime callbacks observe the prompt acceptance, F89 branch, joins of Sarah (1) and
Chester (2), UpdateForce/JoinBattleParty completion, Sarah→Bowie and Chester→Sarah follower links
(distance 2), endpoint Map 3 / 43,10 / Down, Zone Event 8 F603, and the stable wait. The endpoint and
active-party result are fixture/model-owned observed facts, not source-only goldens. Character aliases
for guard selectors 138/139 are resolved through GetEntityAddressFromCharacter and the entity-index
list before physical entity readback.

## RA effect and exclusions

- **RA-03 Confirmed:** extends through this accepted messenger body and follower-ready wait only.
- **RA-04 Confirmed:** only the post-messenger launch state above; Castle, Maps 19/20/21/40/57,
  CheckBattle, Battle 01, and all before/start cutscenes remain **Inferred** or **Unknown**.
- **RA-08 Confirmed:** field-menu **NotReached** remains limited to this extended prefix.
- **RA-09 Confirmed:** reached command/control/text-ID/speaker/prompt/join chronology only.
  Rendered prose, speaker/window presentation, and timing are **Unknown**.
- **RA-11 Confirmed capability inventory:** reached dialogue windows, Yes/No UI, entity/camera
  animation, join music/audio, and VInt/DMA/CRAM/VDP surfaces are private immutable inputs or H4
  questions. No tolerances, audio/pixel claim, or complete 8C closure is made.

Excluded: persistence; optional Map 3/menu content; decline/re-prompt runtime; R3/R4; Phase 4,
Godot, MCP, remake or product changes; and redistribution of private payloads.

## Reproduction

    uv run sf2 h3 map3-messenger-acceptance --timeout-seconds 300

The verifier validates all three closed schemas, source/H1/ROM derivations, retained R1/R2 projection
digests before launch and at the golden boundary, then requires a typed-clean callback status. A failure
removes output, restores the declared scope, clears all callbacks, and returns nonzero. The disposable
session ROM is deleted; canonical ROM bytes are rechecked unchanged.

## Unshimmed Map3 candidate (preparation only)

**Confirmed capability; bounded diagnostic failed:** Issue #456 adds `prepare_map3_observation_candidate` and
`run_map3_observation_candidate` to the existing Python owner, with an opt-in `candidate` mode in
the same observer. The old command, default R2a cases, schemas and observed fixture remain unchanged.
Neither Python function is wired into the CLI. Preparation performs no original emulator launch;
execution requires an independent method and lineage/budget disposition. The single separately
admitted diagnostic below failed and exhausted its permission. A successful API return would be
`OBSERVATION-COMPLETE-UNREVIEWED`, never a public golden or H4 verdict.

The controlled prefix still uses the accepted CheckSram return redirect, checkpoint/menu thunk,
NewGame→SaveGame→MainLoop→default Map3 setup/init path. At the first `WaitForEvent`, before
original player control installation, the candidate checks the observed R1 player/ally/item-byte/
spell/party/difficulty fields, gold, idle event word and source-proved initial route guards. It reads
all 30 **16-bit** status words at `COMBATANT_DATA + id * COMBATANT_DATA_ENTRY_SIZE +
COMBATANT_OFFSET_STATUSEFFECTS` and reports POISON using the source mask, without asserting its
omitted value. Entity records and index mappings are private raw readbacks for active-NPC comparison.
RNG, its copied byte and raw VInt time are captured; R1's post-boundary normalized time is not imposed
on this continuation. The [admitted-start owner](map3-admitted-start.md) supplies those boundaries.

Before starting its input clock, it restores **all three** session patches (menu alias, name alias,
DisplayText) and verifies both cartridge and bus bytes. It also restores generated checkpoint/thunk
RAM. The original CheckSram return redirect has already been consumed; the original main-loop calls
own the live stack, so the pre-bootstrap stack is not written over it. Retained setup mutations are
the original controlled NewGame/SaveGame/default-map state, inherited status/NPC/RNG/raw time and
the in-process bootstrap lineage. This is not natural New/load or passive reset replay.

After that boundary, the candidate does not call the adaptive route driver, write automation markers,
inject MAP_EVENT, bridge flags/positions/RNG, force entity completion, or advance PC. Only original
execution changes game state until terminal/failure cleanup restores the saved core/scope. Ordinary
joypad input comes from the already existing `set_messenger_input` boundary and a prebound frame
array. The epoch is selected once at the admitted wait; subsequent selection depends only on elapsed
frames. Callbacks can record, fail or stop, but cannot reschedule input. The generic scenario facade
is a data-only descriptor check and cannot supply a movie/start; the disabled replay materializer's
fixed 33-row recording is unsuitable. Neither is revived or silently substituted.

### Source binding and observation boundary

The canonical ROM and pinned source revision at the top of this document remain required. Material
preparation validates the retained R1/R2/R2a projections, the actual H1 listing, original source
sections, final ROM operands, and Lua syntax. Newly consumed source files must match their exact
pinned Git objects. H1's unresolved PC-relative listing words are **not** final ROM operands:
`byte_50E32` binds the `chkFlg 604` branch and original LEA/script macro; ROM displacement decoding
binds `cs_51652`. The original F604 trap is `0x50E3E`, followed by the original RTS at `0x50E42`.

The observer retains messenger text/prompt/join/follower callbacks and checks R2's messenger entry
and R2a's follower-ready state. It records actual script/text/close-window entry and stack-matched
return, text wait1 and acknowledgement input reads, controller reads and accepted movement. A
single physical-PC dispatcher handles shared return sites. Callback failures use the shared failure
prefix/nonzero exit with private `kind=map3-candidate-callback-failure` JSON; JSONL preserves actual
checkpoints and the last checkpoint on failure. Candidate phase/role names are not claimed to satisfy
the closed legacy R2a failure schema. That schema and the default rail remain unchanged.

Gate admission requires R2a, original `Map3_ZoneEvent4`, `cs_51652` return, original F604 trap/readback,
then the north warp. `csc14_setEntityActscriptManual` (`0x46950`), `loc_46966` and `loc_46970` bind
action installation and the original idle wait. Entity 138 is **not awaited**; its sampled state is
not declared complete because entity 139 or the program returned. Entity 139 must actually have
`eas_Idle` at its awaited command return. The first Map19 wait requires original warp/init and
`cs_53104` returns with no tracked program/text/close return pending. The terminal is the first
original player movement acceptance at `loc_52E8` after that wait, with Map19, no pending event,
no typewriting and F604 set. No royal/tower, battle or 5B continuation is included.

Unexpected programs, warps or FieldMenu entry, ordering/state drift, exhausted fixed input, frame
budget, host timeout and cleanup failure remain failures. A fresh `runtime` directory is mandatory;
the execution composition reserves a fresh attempt before identity checks and refuses to overwrite it.
It copies the canonical ROM to a disposable session, preserves checkpoint/status/host diagnostics,
and independently attempts session deletion and the canonical-identity check even after failure.
The accepted [shared-tool mechanism](../operations/local-private-inputs.md#bizhawk-runtime-copies)
resolves the reviewed EXE/Lua identities from the shared installation and copies verified release
files into `runtime/observer/bizhawk-*` before any separately admitted launch. Its executable,
configuration, cwd and TEMP/TMP stay in that local copy; observer outputs remain beside the
`runtime/observer` stem. The candidate opts into `run_observer`'s diagnostic callbacks, which use
`run_native_bizhawk_process` and `NativeProcessResult`. Callers without the optional result callback
retain their existing process/timeout/status behavior; the native helper's existing consumers also
retain their default behavior.
Preparation itself creates only `input.json`, `config.json`, `config.lua` and `candidate.json` in an
explicit fresh worktree-local ignored directory. Missing input is `FileNotFoundError`/unavailable;
bad input/source binding is rejected before materialization, never `PRELAUNCH-PASS`.

### Private process diagnostics

Issue #460 retains one `runtime/host-status.json` per attempted composition, including identity or
preparation failure before process creation. `process.started=false` distinguishes that boundary;
PID is written immediately after creation, before waiting. Exit code, stdout/stderr, timeout,
termination and tree-kill result come from the native helper, never exception-message matching.
Timeout is persisted before termination is attempted, so a failed kill cannot erase that fact.
Unavailable results remain JSON null: a PID alone does not prove termination, and an interrupted
helper with no final result leaves exit/output/termination unknown. `timeout_tree_killed` retains
the helper's existing meaning (including its on-started-error cleanup); it is not an independent
residual-process scan. Original callback compatibility and residual process state still need an
admitted observation.

The private report retains the reviewed candidate identities, actual runtime-copy path/command,
settings path, cwd/TEMP/TMP, and exact runtime EXE/Lua/settings/generated observer-config identities.
Preparation also binds the process helper, bootstrap descriptor/library and tool resolver. Execution
rejects helper/bootstrap drift and copied EXE/Lua drift before creation. The generated config is
retained beside the observer stem; it derives from the frozen JSON with only the existing bootstrap
and attempt-local output/status/checkpoint paths supplied by the reviewed helpers. These private
paths and payloads must not be pasted into public PR/Issue output.

Lua status, checkpoints and observation files remain intact when the process, callback or terminal
checks fail. Host cleanup records session deletion and canonical ROM comparison independently;
a primary exception and cleanup errors both survive in the diagnostic. This is an opt-in host
report, not a new replay ledger, fixture or launch allowance. A failed attempt is preserved and
requires independent disposition; a fresh directory is not permission to retry.

### Reproduce preparation without execution

After loading the current ignored private-input configuration, this API prepares a supplied trace:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import UPSTREAM, prepare_map3_observation_candidate
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path

prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    input_path=Path("local/issue460/diagnostic-input-proposal.json"),
    output_directory=Path("local/issue460/review-candidate"),  # must not exist
    proposed_timeout_seconds=600,  # proposal for review, not a launch allowance
)
```

Run through `uv run python -X utf8` in the owning worktree. The closed input object has
`clock="first-r1-wait-next-frame"`, `provenance="diagnostic-parameters"` and `frames`: 1–36,000
explicit single-button strings (`Up`, `Down`, `Left`, `Right`, `A`, `B`, `C`, or empty for neutral).
No conditions, coordinates, executable expressions or unknown timing values are accepted as input.
The report binds ROM/source/listing/observer/configuration/input identities and retains Unknowns.

The private review trace's reproducible **diagnostic** recipe uses R2
`expectedObservation.records[0].logicalInputTrace` in its complete order and R2b
`static.routeGraph.segments[0].inputs` / `[2].inputs`; these provide logical directions, not observed
timing. Start with 120 neutral frames; give each logical input 2 pressed + 22 neutral frames. After
the last input of each contiguous waypoint group, append 120 neutral, except:

- For `map3-house-exit-zone`, `map3-sarah-classroom`, `map3-astral-zone-introduction` and
  `map3-astral-zone`, append 12 repetitions of 2 C + 118 neutral instead.
- Append no block after the first `map3-entity142` navigation group or `map3-entity142-face`.
  Consume the trace's Left facing input and then its actual C interaction first. After that final
  `map3-entity142` C, append 120 neutral and 24 repetitions of 2 C + 118 neutral. This relocates
  the earlier proposal's premature 12-confirm block and facing wait after interaction while
  preserving all its pulse counts and durations.

Then append 60 C/neutral repetitions for the messenger, segment 0 inputs (2 pressed + 22 neutral
each), 24 C/neutral repetitions for the gate, segment 2 inputs at the same duration, 240 neutral,
2 Up, and 120 neutral. This separate proposal still has 23,234 frames, a proposed 600-second host
timeout and a 28,634-frame watchdog (input plus retained bootstrap/controlled-prefix margins).
Neither limit is an allowance or a reset of historical consumption. All pulse counts, waits and
reach remain **Unknown**; moving the blocks after interaction does not establish that they finish
before control resumes or avoid FieldMenu. No block may be rescheduled from live state.

**Confirmed input-construction defect, not an original-game observation:** the retained Issue #456
candidate-06 expands each contiguous waypoint group, placing 12 C pulses at frame ordinals
6769–8208 before the trace's entity142 Left-facing step and actual C interaction at `(55,17)`.
The complete trace contains a navigation group, a separate facing row and the final interaction
under the repeated `map3-entity142` name. The new proposal only reorders frames 6769–9816 to
preserve this logical order; old candidate/input material remains unchanged. Source
`code/gameflow/exploration/explorationfunctions_0.asm:GetActivatedEntity` uses player facing for
the target block. `explorationvints.asm:ProcessPlayerAction` sends C through entity/area selection,
with FieldMenu as a fallback. Thus premature C is not a harmless generic wait; the actual selected
action and timing are still **Unknown**.

**Confirmed source correction:** the full pinned
`data/maps/entries/map03/mapsetups/scripts_1.asm:cs_51652` through `csc_end` contains six
`nextSingleText` commands between the two guard-action groups. The H2 `programs/cs_51652/operations`
array is a selected control-effect projection, not the complete script. The initial Issue #460
analysis incorrectly inferred no gate dialogue from that subset; that finding is withdrawn.
The gate's 24-confirm block remains unchanged. Source confirmation uses the pinned commit named
above and blob `5e9b260b9e07dd22387a0ab1ab3b6666d1ab05e1`; it does not prove the proposed timing.

Direct acceptance consists of source/H1/ROM binding, materialization and rejection checks,
Python lint/compile, Lua compilation and the clean committed planner. The Issue #463 correction
uses the narrower zero-launch checks below; it does not rerun normal/full/H1 or runtime gates.
Issue #460 directly exercises the opted-in process handoff and error paths with synthetic process
doubles only, including pre-process failure, PID persistence, timeout/failed termination, callback
failure, missing output and independent cleanup. These checks launch no native process and are not
original evidence. Ordinary planner-selected runtime gates remain **NOT RUN**; the separately admitted candidate
diagnostic and its failed result are recorded below. The local
environment needed an explicitly authorized pinned checkout, verified tool copies and one real
bit-perfect H1 build; its kept listing/log precede preparation and are not runtime evidence. Consuming
the accepted shared-tool mechanism requires direct preparation/copy checks, not another H1 build or
normal suite solely because the base changed. Local-copy EXE/Lua bytes must equal the candidate's
reviewed identities. These checks do not establish native startup or original-game compatibility.

Old R2b/replay ordinal-2 timeout **FAIL**, cleanup failure, missing genuine receipts/ledger,
retry/reset prohibition and completed #431/#434 failures remain preserved by the
[audit dossier](map3-battle01-audit.md#first-necessary-original-observation-dossier).
This candidate grants no launch allowance and supplies no natural continuity, full 8D or H4 PASS.

## Single admitted controlled-start diagnostic result

**Confirmed failure, 2026-09-19 project date (2026-09-20 02:07 UTC):** the
[one-run main-gate disposition](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/460#issuecomment-5746916798)
was executed once from accepted commit `4386c0148362e44d8d9256cf183671832f7c10e2`, tree
`3f0aeda35c7715f6e7002baf3f3021b4e78fe7c3`. This was a controlled-R1-start diagnostic, not
scenario acceptance or passive reset/New/load replay. The disposition invoked ADR 0015's accepted
further-work condition after independent acceptance of the source/admission, observer-restoration
and diagnostic-capability corrections. It did not reset historical consumption, grant a two-plus-one
sequence or satisfy the disabled replay's ordinal-3 prerequisite. Its single process allowance is
now exhausted. No retry, repair launch, longer limit or follow-on H3 is authorized.

The existing `run_map3_observation_candidate` API consumed the retained `candidate-proposal-03`
once after an invocation note and read-only identity/process checks. Frozen input SHA-256 was
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`, runner
`C6EDA0CCBD4BBEFE1732883F31B51B0736EF4DA7BC5D7B39CE4C969432509F14`, configuration
`8495B3D379970403AD4672E321E1D87F688903700DF470B07F2B1D027FCDCD07`, and observer
`335148A5D50383CA4152E14C4E18498AE1CAE2FC615E830421DCB29D108DB028`.
The reviewed report's helper/bootstrap, EXE/Lua, pinned source, retained fixtures, H1 listing and ROM
identities matched before invocation. Limits stayed 23,234 frozen input frames, 28,634 watchdog
frames and 600 seconds. No preceding native warmup or old H3 command ran.

**Confirmed observed boundary:** 13 private checkpoints record the first R1 wait and reported
service/scratch restoration at observer frame 354, followed by the first two accepted Left inputs.
At observer frame 499 / input frame 145, `ProcessMapEventType1_Warp` entry `0x25978` reached
`candidate:warp`. Current map was 3; the raw destination-map operand was `255` (`MAP_CURRENT`).
The candidate asserted `warp beyond bounded Map3/19 route` in phase `candidate-route`. The failure
record's expected and actual PC both equal `0x25978`; this was not an unexpected callback address.
It stopped at handler entry, before observing completion of that warp. Messenger, entity142, gate,
F604, north warp and first Map19 control were not reached by this diagnostic. The proposed input
reordering was therefore not exercised at its corrected interaction.

**Confirmed observer/source mismatch:** at that accepted Git object,
`tools/bizhawk/map3_messenger_acceptance_observer.lua:2425` permits only raw map 3 for a prefix
warp, besides its separately guarded raw map 19 branch. Pinned `sf2enums.asm:MAP_CURRENT` defines
255 as reloading the current map. `data/maps/entries/map03/6-warp-events.asm` gives the house stair
warp at `(54,3)` that sentinel and destination `(3,3)`. In
`code/gameflow/exploration/explorationfunctions_2.asm`, `ProcessMapEventType1_Warp` reads
`MAP_EVENT_PARAM_2`, while `ExplorationLoop` branches on byte `-1` to `@MapIndexNotProvided`
instead of replacing `CURRENT_MAP`. The observer rejects this valid source representation at the
first opening warp. This explains the reported assertion; it is not an original route failure or
proof that the warp completed. The executed Python/Lua identities and attempted material remain preserved. The separate
zero-launch correction below changes the opt-in observer, without reclassifying that failure.

**Confirmed process and retained cleanup:** native PID 42464 started once, returned exit code 1,
reported `timed_out=false` and `process_terminated=true`; timeout tree-kill was not used. The host
result is `FAIL` / `started-failure`. Full stdout/stderr remain private. Lua emitted the typed failure,
reported callback count zero, scope/session restoration and output removal. No terminal observation
file remains. The host independently recorded session ROM deletion and unchanged canonical ROM.
A separate post-exit operating-system check found neither that PID nor a process executing from this
attempt's runtime directory alive; no additional process cleanup was needed. Runtime copies, status,
checkpoints and all prior results are retained.

For read-only reproduction of the finding, inspect the retained `candidate-proposal-03/runtime/`
`host-status.json`, `observer.status.txt` and `checkpoints.jsonl`, together with the Issue #460
invocation note and post-exit process check. Compare the exact executed observer and pinned source
sections named above; do not invoke the candidate again or rematerialize it around collision refusal.
The failure reaches the host exit contract and bounds the native cleanup result for this attempt.
It establishes no general callback compatibility, successful gate transition, natural continuity,
full 8D or H4. The intended caller-dependent gate question and later timing/reach remain **Unknown**.
Historical R2b/replay failures and missing genuine receipts remain intact; documentation of this
failed diagnostic does not replenish any allowance.

## MAP_CURRENT correction and bounded warp admission

**Confirmed source/code correction, 2026-09-19 project date:** Issue #463 corrects only the opt-in
candidate warp admission. `MAP_CURRENT` is a raw operand, not map 255. On the no-scroll path
(`MAP_EVENT_PARAM_1=0`), `ProcessMapEventType1_Warp` passes the raw map byte in D0 back to MainLoop;
`ExplorationLoop` compares it with byte -1 and branches to `@MapIndexNotProvided`, retaining
`CURRENT_MAP`. `UpdatePlayerPosFromMapEvent` reads coordinates/facing; it does **not** resolve the
map sentinel. The inherited R2 builder's comment attributing resolution to that helper is inaccurate;
that default-rail comment is outside this correction's owned code boundary. A nonzero scroll-mode
path writes the map byte directly and must not receive the same sentinel interpretation.

Preparation binds `sf2enums.asm`, `sf2mapmacros.asm`, the complete Map3 `6-warp-events.asm`, and
`explorationfunctions_2.asm` to their pinned Git contents. Existing map-content encoding/decoding
checks the complete nine-row table at H1 symbol `Map03s6_WarpEvents` (`0x978F0`) against the canonical
ROM, including its terminator. Source guards retain no-scroll dispatch and current-map branch order;
H1/ROM binds the `cmpi.b` at `0x257E4` to the parsed equate, the final ROM BEQ at `0x257E8` to
`0x25828`, and the BNE.W at `0x2597C` to `loc_259CC`. No H1 rebuild is performed.

The candidate reuses R2 `static.route.runtimeOpening.warps` and `navigation.inputPlan`, joining
source position, attempted movement target and raw warp destination separately. The source-bound
prefix is:

| Warp | Player source | Movement target | Raw map | Effective map | Warp destination / facing |
| --- | --- | --- | --- | --- | --- |
| House stairs down | `(55,3)` | `(54,3)` | 255 | 3 | `(3,3)` / Right |
| School stairs down | `(45,7)` | `(46,7)` | 255 | 3 | `(59,12)` / Left |
| School stairs up | `(58,13)` | `(59,12)` | 255 | 3 | `(46,7)` / Down |
| North castle entrance | `(28,2)` | `(28,1)` | 19 | 19 | `(26,30)` / Up |

These are static admission constraints, not newly observed transitions. School stairs up uses R2's
source-derived diagonal target. North uses the R2b navigation/warp segment and retained source row
whose trigger X is wildcard 255; the bounded route still requires actual target `(28,1)`. That trigger
wildcard is distinct from the map operand sentinel. The north callback retains original gate-return
and F604 requirements, checks no-scroll/raw map/destination/facing, and rejects subsequent warps.
The later original Map19 init/program/wait/input checks remain unchanged.

The Lua checkpoint preserves all five raw operands, observed current map, player source and movement
target, plus `effectiveDestinationMap`. Only no-scroll `MAP_CURRENT` uses current map for this
read-only calculation. Admission also requires the exact source row: literal map 3 cannot substitute
for raw 255 in the three prefix rows; raw 255 cannot substitute for north's explicit map 19. Wrong
map, scroll mode, coordinates, destination, facing, gate state, and later/out-of-scope warps fail.
The callback never assigns game state, simulates a warp, or selects input from live state.

### Zero-launch verification and stopping condition

Load `local/private-inputs.ps1` in the owning worktree and run `uv run sf2 rom verify`, then the
preparation API shown above with the unchanged `local/issue460/candidate-proposal-03/input.json`
and a fresh ignored output directory. Input SHA-256 remains
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`; all 23,234 frames,
600-second proposed limit and 28,634-frame watchdog are unchanged. The preparation report binds
exact source/listing/runner/observer/configuration identities. It remains
`CANDIDATE-PREPARED-NOT-ADMITTED` with no `runtime` directory.

Direct checks retained under `local/issue463/` use `uv run python -X utf8 local/issue463/direct.py`
(with a fresh output name on reproduction). They compile Lua without running the observer, then
exercise only the extracted admission callback in an in-process Lua library using immutable sample
reads: the three current-map rows and explicit north destination pass; malformed/out-of-scope
variants fail. In-memory source branch, ROM operand/branch/table, and retained-map mutations must
reject construction. These are checks of admission constraints, not original runtime observations;
no tracked validation-tool tests or new parser are added. Python lint/compile, document links/scope,
`git diff --check`, clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
public CI complete the preparation review. Exact identities and check outcomes belong in the frozen
Draft PR/Issue handoff; private payloads stay local.

**Unknown:** actual handler completion, later fixed-input reach/timing, caller-dependent gate/F604
transition, first Map19 control and general callback compatibility remain unobserved by this
correction. Selected runtime gates are **NOT RUN**. The frame499/input145/PID42464 diagnostic remains
**FAIL**, with its cleanup record intact. #460's permission is exhausted; no native/no-ROM startup,
candidate execution, old H3 or replay preflight is authorized. Normal/full/H1 results and prior
failures remain preserved without rerun. Independent main-gate review of this frozen correction
precedes any decision on further work; code/materialization PASS never triggers a retry or closes #437.
