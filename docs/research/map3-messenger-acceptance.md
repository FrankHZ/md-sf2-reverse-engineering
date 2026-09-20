# Map 3 Messenger Acceptance

- Status: **Confirmed** for the accepted R2a continuation; Issue #475 acquisition reached Map19, pending independent acceptance.
- Fixture: sf2-map3-messenger-acceptance-runtime-v1
- Case: natural-map3-messenger-accept-to-follower-ready-wait
- ROM: USA retail SHA-256 9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9
- Source baseline: ShiningForceCentral/SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6

## Bounded interactive acquisition

Issue [473](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/473) implements an explicit
interactive acquisition mode for the existing candidate. That implementation slice was **offline
preparation only, zero emulator launches**. After PR #474 was independently accepted and merged,
[Issue #475](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/475) separately admitted
and consumed exactly one acquisition. Its [result](#single-admitted-interactive-acquisition-result)
reached the Map19 terminal and remains unreviewed; no execution allowance remains. [ADR 0015](../decisions/0015-original-reference-replay-and-h4-boundary.md#distinguish-interactive-acquisition-frozen-replay-and-remake-h4)
separates this operation from frozen replay and remake H4. The
[bridge protocol](../operations/bizhawk-debug-bridge.md#map3-interactive-composition-offline-only)
owns communication, serial requests and the single process. This observer retains source-bound R1
admission, checkpoints, typed failures and final restoration. No new CLI, debugger service, fixture,
schema, planner, route policy or verification-helper test is introduced.

**Confirmed (source/implementation boundary):** preparation checks the pinned SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, H1 listing/ROM callback bytes, retained R1/R2/RA
contracts and installed tool identities. The explicit mode uses an empty input declaration; it does
not load, convert or execute #471's frozen house-neutral input. The old default API retains its
frozen frame-table semantics. New mode preparation binds the shared bridge Python/Lua/JSON sources
in addition to existing candidate execution identities. Preparation grants no execution permission.

The readback and terminal stay as described below: controlled NewGame/SaveGame/default Map3 state,
allies (including inherited POISON), full live entity/index state, RNG/raw time and service/scratch
restoration at the first R1 `WaitForEvent`. Nothing normalizes those inherited values after that seam.
The first original Map19 movement acceptance must follow the real gate program, original F604 trap,
north warp, Map19 init/program returns, original wait/controller installation and closed pending
consumers. The terminal callback is retained once; remaining batch frames are skipped. The current
frame may finish before final restoration, and the reply distinguishes those two observation times.

### Consumer observations for explicit operator decisions

Existing `DisplayText`, `CloseDialogueWindow`, script and prompt entry/return events remain source
observations. Interactive frame-end snapshots additionally report pending returns, active consumer
counts, and the most recent source consumer poll with its observer/emulator frame and PC:

| Poll | Pinned source owner and meaning |
| --- | --- |
| `text-wait1` | `code/common/scripting/text/textfunctions_1.asm`, `loc_65B4`: current-input read after `WaitForVInt` in `symbol_wait1` |
| `text-wait2-loop` | Same source, `loc_6472`: wait2 loop before animation/VInt/input processing; not the later acknowledgement read itself |
| `WaitForEvent-action` | `code/gameflow/exploration/explorationfunctions_2.asm`, `loc_2593C`: field loop A/C dispatch; another C may invoke an entity/area action or FieldMenu |
| `YesNoPrompt-release` | `code/common/menus/yesnoprompt.asm`, `loc_1530C`: wait for release of player-one input |
| `YesNoPrompt-choice` | Same source, `loc_15314`: choice loop; B selects No, A/C accepts the current choice |

These named symbols are bound to H1 and canonical ROM bytes during preparation. A last poll can be
stale; its timestamp and active consumers must be read together. `CURRENTLY_TYPEWRITING=0`, a script
return or a past poll alone is **not** readiness proof. Lua never converts these facts into a button
decision. FieldMenu remains a typed stop, not a condition to repair with injected state. The #475
result below records the reached native boundary. Other availability, timing and downstream reach
remain **Unknown**.

### Preparation and operator protocol

The following describes capability use, not remaining launch permission. The sole reviewed #475
material was retained `local/issue473/prepared-02`; `prepared-01` is stale and forbidden for execution.
Do not reprepare or invoke either after the consumed attempt.

Load the current worktree's `local/private-inputs.ps1` in the launching shell. Use installed `uv`
dependencies and shared Lua. The existing API prepares into a fresh ignored directory without a
runtime copy or process:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import UPSTREAM, prepare_map3_observation_candidate
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path

report = prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    output_directory=Path("local/issue473/prepared-01"),
    proposed_timeout_seconds=1800, interactive=True,
)
```

Outputs are `candidate.json`, `config.json`, syntax-checked `config.lua` and `input.json`. The last
file identifies the explicit mode/clock and contains zero preselected frames; its digest is **not an
actual input recording**. The report retains `CANDIDATE-PREPARED-NOT-ADMITTED`, zero launches, the
1800-second wall bound, 28634-frame total, 2048 batches and 120-frame maximum batch. It records
historical controlled starts 2 and future ordinal 3 without claiming that ordinal was executed.

After independent merge and separate main-gate admission, #475 invoked this same runner with the
explicit matching mode. This records the consumed invocation; **do not repeat it**:

```powershell
uv run python -c "from pathlib import Path; from sf2tool.h3.map3_messenger_acceptance import run_map3_observation_candidate; from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path; run_map3_observation_candidate(private_input_path(ROM_INPUT_IDENTITY), Path('local/issue473/prepared-02'), interactive=True)"
```

After the admitted R1 frame finishes, hello prints its paused state. Enter one JSON array per line:
`["state"]`, `["step", 1, "C"]`, `["step", 1, "neutral"]`, or `["abort"]`. Each button is held for
the requested 1–120 frames; neutral requires its own explicit step to advance a release frame. Paused
queries consume no original frames. The operator chooses from original observations; no fixed pulse
schedule or automatic B replacement is supplied. Inspect the private callback records as needed.

### Actual input, clocks, order, and failure boundary

Each `runtime/actual-inputs.jsonl` object has a shared monotonic `order`, observer `frame`,
`emulatorFrame`, `r1Epoch` and `r1EmulatorEpoch` (false before admission). Checkpoints share this order;
their `boundary` distinguishes `callback-time` from `host-loop` diagnostics. Input log kinds are:

| Kind | Meaning |
| --- | --- |
| `command` | Validly framed command ID and fields received; not proof of application |
| `applying` | Button submitted, `beforeFrame`, next `inputFrame`, command ID (0 for bootstrap); a crash here does not prove completion |
| `frame` | `emu.frameadvance` returned; actual button, `beforeFrame`, `afterFrame`, R1-relative `inputFrame`, command ID and bootstrap marker |
| `result` | Command outcome, actually advanced count, source snapshot labelled `frame-end`, terminal callback separately and error if any |

Record every completed frame, including neutral and the declared automatic Start/neutral bootstrap.
The R1 admission callback may occur inside a bootstrap frame; retain both its callback emulator clock
and that frame's completion instead of relabelling it an operator frame. JSONL gaps in `order` are
expected because `checkpoints.jsonl` shares the sequence. No requested batch substitutes for completed
`frame` records. Physical input-consumption evidence remains the original controller/consumer callbacks.

The first error or terminal ends remaining advances. Unsupported buttons, wrong arity, malformed
counts, budget overflow, disconnect and abort fail closed. At 28634 total frames without terminal the
session fails; at most 2048 accepted input batches are allowed. The 1800-second wall limit starts at
the owned process launch and includes startup and all paused time. Host containment, short exchange
timeouts, callback exceptions, restoration and final session-ROM deletion keep their distinct statuses.
EOF graceful cleanup remains **Unknown** at the native boundary; a killed process is not proof of Lua
restoration. The #475 result below includes independent owned-process and canonical-identity checks. An acquisition completion remains `OBSERVATION-COMPLETE-UNREVIEWED`, not replay/H4 PASS.

### Issue #473 offline verification and retained limitations

Use Python AST/compile, targeted Ruff, the shared Lua syntax compiler and bounded direct protocol,
input-log/budget/error-path checks. API stubs can check delivery ordering and skipped batch remainders;
they cannot establish actual emulator input, core behavior, natural reach or native EOF cleanup.
Record the clean committed planner and actual public CI, but shared-path fanout does not authorize
local normal/full/H1/H2/H3 runtime queues for this slice. No bridge smoke or warmup is permitted.

Preserve #460's raw-MAP_CURRENT failure, #465's FieldMenu failure, #469's source/trace audit, #471's
pre-API CRLF assertion failure/correction and its unexecuted fourteen-neutral replacement candidate.
At #473 preparation the controlled historical count was **2**. The separately admitted #475
acquisition below consumed the one additional start, making the total **3**. There is no remaining
call, reset, extra smoke/replay/retry or fourth start. Disabled original replay ordinals 1/2 and
their restrictions remain separate. Arrival timing, NPC/RNG effects, prompt/gate/natural continuity,
complete 8D and H4 stay **Unknown** until their own required evidence and independent acceptance.

## Single admitted interactive acquisition result

**Confirmed observation, pending independent acceptance:** Issue [475](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/475)
consumed its sole API call/native start on 2026-09-20 UTC (2026-09-19 America/Chicago launch date).
The unchanged accepted base was `9cb14a55c2777b515a18450e98fc37ca90350292`, tree
`63659d8c0d16f662279727dc1301f641ef0a3d4c`. This separately admitted interactive controlled-R1
acquisition superseded preparation's NOT-ADMITTED disposition only for that attempt. The return was
`OBSERVATION-COMPLETE-UNREVIEWED`, not frozen replay, a public golden, complete 8D or H4 PASS.
Historical controlled starts are now **3**; #460/#465 completed FAIL, #471's pre-API CRLF assertion
and unexecuted house-neutral preparation, stale #473 prepared-01, and disabled replay ordinal 1/2
failures/restrictions remain preserved. No retry, fourth start, follow-up replay or cleanup is authorized.

### Frozen material and actual transport

Read-only Git/ownership/process checks found a clean transferred Research worktree, no open PR and
no EmuHawk. The retained `local/issue473/prepared-02/runtime` did not exist. Current private-input
configuration and narrow ROM verification passed; all report source/H1/fixture/helper/tool identities
matched without preparation or material changes. The pre-call note and full identity check are retained
under ignored `local/issue475/`. Exact SHA-256 identities were:

| Material | SHA-256 |
| --- | --- |
| `candidate.json` | `73AE17BEA9290DF305A313063F139429E329114BDEE3DBD578B3695DEB591F84` |
| `config.json` | `B0C0D8BB16F3440BB64F9851257DAEFD81288C7E1C8782C322F0EE15B8E38A80` |
| `config.lua` | `C58438B4B3A830C00A145774EBFEFD2C102269F71A38EDF039D0F69B76CBC2DB` |
| Empty mode declaration `input.json` | `05562B5546BEBE81C0EA462E2285268CA6CB943091F7B5A01536CD6D94C4AA95` |
| Observer | `33766BE3A243EE782AF0F156DD6B48EE15BF8CEF0D20D285AC8CB66A68C063A3` |
| Runner | `37FAF15B03745C984316456051C6C7CA242A54E59078CC1B281F8D7A4927CC27` |

The existing `run_map3_observation_candidate(..., interactive=True)` ran once through a persistent
PTY with `uv run python -X utf8 -c`; stdin remained available for one JSON array plus carriage return
per command. No transport drycheck, warmup, smoke, replay or second API call occurred. All 200 commands
were explicit `step` requests; there were no B/A inputs, state/ping probes, abort or disconnect.
Button/release choices used current coordinates, source route topology, active/pending consumers and
fresh polls. The post-join choice also used text 447's actual return plus the existing source chain
`csc08_joinForce` → `FadeOut_WaitForP1Input` → `WaitForPlayerInput`. That wait has no dedicated poll
in this observer: its exact live entry was **Inferred**, not established by the stale text poll.
After C at frame 8495 the original window-close callback ran, then the messenger returned.

### Reached source boundaries

Frames below are observer frames; callback `emulatorFrame` is one lower in this run. They are
provenance for this acquisition, not gameplay timing requirements or a reusable input schedule.
All PCs/symbols refer to the pinned source/H1/ROM identified above.

| Boundary | Actual observation |
| --- | --- |
| Controlled R1 service/scratch restoration | Frame 355 / emulator 354, `WaitForEvent` `0x2591C`; patches/scratch restored before operator input. All 30 retained ally status words read 0, hence no inherited POISON in this attempt. Live entity/index and raw RNG/time readbacks remain private; raw time was not normalized. |
| House, Sarah and Astral | House `cs_5145C` entry/return 501/1015; Sarah `cs_513D6` 2025/2052; Astral exit `cs_5148C` 3541/3543. Original house and school stair handlers reached; F601/F602 became true. No FieldMenu callback occurred. |
| Messenger and prompt | `cs_5149A` entry 3881, YesNoPrompt entry 7267, fresh choice poll 7291, C 7292, original return D0=0 at 7297 (`0x47498`). Accepted-path text IDs 517–531, 535–536 and join text 447 reached their real DisplayText consumers. |
| Follower-ready return | Messenger return 8514; follower-ready wait 8515 at `(43,10)`, Down, F600/F66/F603 true, F604 false. All tracked script/text/prompt/close consumers returned. |
| Gate `Map3_ZoneEvent4` / `cs_51652` | Entry 8903 at the source-selected gate edge. Six real text IDs 537–542 completed. Script return at 10032 has order 20952, followed by F604 false before trap at `0x50E3E` (20953) and true at `0x50E42` (20954). Guard readbacks are retained; no unobserved entity motion is inferred from script return alone. |
| North warp | Frame 10197, original handler `0x25978`, source `(28,2)` to target `(28,1)`, operands `[0,19,26,30,1]`; effective destination Map19. |
| Map19 setup and consumers | `ms_map19_InitFunction` `0x530EA` entry, `cs_53104` entry/return, init return all at 10259, orders 21446–21449. First wait `0x2591C` at 10285 precedes original controller installation. |
| Sole terminal | Frame 10329 / emulator 10328, original movement acceptance `0x52E8`, Up input 1, Map19 `(26,30)`, F604 true, event word 0, pending returns 0 and every tracked consumer count 0. The completed frame remains `(26,30)`; acceptance of movement does not prove next-tile arrival. |

### Complete logs, process and cleanup

**Confirmed:** PID 39748 ran BizHawk 2.11.1 / Genplus-gx, startup 9.433 seconds and total owned
elapsed 1778.575 seconds including paused operator time, within 1800. Exactly 10329 applying/completed
frame pairs cover emulator frames 0→10329 without gaps: 355 bootstrap frames (214 Start, 141 neutral)
and 9974 operator frames (8351 neutral, 302 Left, 597 Right, 325 Down, 361 Up, 38 C). The 200 batches
all contain 1–120 completed frames, below 2048 batches and 28634 total frames. No partial batch or
skipped remainder was exercised because the last request was one Up frame. The 582 checkpoints and
input records share all 21640 unique ordered records; each command/result agrees with the host receipt.

The terminal callback precedes final frame completion. Its checkpoint reports emulator 10328 and
its snapshot has raw frame-counter 206; the preserved frame-end snapshot reports emulator 10329 and counter 207. Final
restoration then reloads the declared bootstrap core state: the enclosing last `result` log metadata
reports emulator 214 while its nested acquired snapshot remains 10329. This is a cleanup boundary,
not a missing input-frame interval or permission to replace the acquired terminal with restored state.

Host/API exit was 0, `timedOut=false`, `forcedTermination=false`, process terminated and bridge
Lua status `closed`. Candidate status ends with `callbacks-cleared:0` / `observer-finished`, without
failure lines. Every declared restoration field is true; `outputRemoved=false` preserves the result.
The runner deleted the session ROM and rechecked canonical identity unchanged. Independent OS
inspection after completion found no PID/runtime-copy survivors and zero EmuHawk; no containment
or extra cleanup was needed. Host stdout/stderr slots are null in this composition; actual native
output remains in the bridge process log, while `local/issue475/api-console.txt` retains API stdout.

Private runtime evidence stays in `local/issue473/prepared-02/runtime/`:

| Artifact | SHA-256 |
| --- | --- |
| `actual-inputs.jsonl` | `21A9422DA146189FE8C46CAAAE141E7694EF24F67C6B135AB077F896C514E021` |
| `checkpoints.jsonl` | `E7D50E264F73DECB6129240067F5E36FCBB177954903F8634E829224054748C2` |
| `host-status.json` | `9716B8BC9FCAC809E6AC688B2D9B188556C7C198846748EAE50D6584E89A0E46` |
| `observer.observed.json` | `EA6EE99767C49F4B9E16CDD48C91AAC9C4634B1BC03E307FCBCBBAAE69EAB8D4` |
| `bridge/receipt.json` | `1C0B1158A20F9D24CA1D68B6807DF7078D08387696B856D0370C4763AEDF5F15` |

### Verification and remaining boundary

Read-only result reproduction loads those existing JSON/JSONL/status files: pair `applying`/`frame`
by ID/button/beforeFrame, require consecutive completed frames, merge checkpoint/input `order`, join
all 200 commands/results against `bridge/receipt.json`, and inspect the named callback sequence,
terminal/restoration/host status and independent OS result. The retained direct command is
`uv run python -X utf8 local/issue475/check_result.py` (with current private configuration loaded);
it does not invoke or import the acquisition runner. Its exact assertions and summary remain local.

The core retained-record integrity check can also be reproduced without that local analysis script:

```python
import json
from pathlib import Path
root = Path("local/issue473/prepared-02/runtime")
inputs = [json.loads(line) for line in (root / "actual-inputs.jsonl").read_text().splitlines()]
checkpoints = [json.loads(line) for line in (root / "checkpoints.jsonl").read_text().splitlines()]
frames = [row for row in inputs if row["kind"] == "frame"]
assert [row["afterFrame"] for row in frames] == list(range(1, 10330))
assert sorted(row["order"] for row in inputs + checkpoints) == list(range(1, 21641))
terminal = [row for row in checkpoints if row["kind"] == "map19:first-original-movement-acceptance"]
assert len(terminal) == 1 and terminal[0]["frame"] == 10329
assert terminal[0]["state"]["pendingReturns"] == 0
```

Preserved local analysis failures: identity checker 01 compared raw CRLF bytes to normalized source
text hashes; checker 02 hashed the returned projection wrapper instead of its `projectionSha256`.
The corrected checker 03 matched existing preparation semantics. Result checker 01 incorrectly
required a gap between final frame order and result order; corrected checker 02 accepts adjacent
records and passed on the same completed run. No material, source, input or native result was repaired,
and no native invocation followed any analysis correction. One tool-output formatter failed before
sending a controller command; it was corrected without an emulator restart or extra input.

Direct result/material checks and 64 document links/anchors passed. Acceptance also records the
clean committed planner and actual Public CI on the Draft PR. No normal/full/H1/H2/H3 queues or new verification-helper tests are authorized.
**Unknown:** unobserved prompt branches, native EOF/abort recovery, dedicated long-idle thresholds,
partial-batch termination, repeatability/frozen replay, downstream tower/battle/5B continuity and
complete 8D/H4. Paused snapshots and ordinary controller delivery are observed only for this run;
no general backend or presentation compatibility follows. Independent main-gate acceptance is required
before these bounded facts become an accepted downstream contract. Stop with a clean pushed Draft PR.

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
execution requires an independent method and lineage/budget disposition. The two separately
admitted diagnostics below failed and exhausted their respective permissions. A successful API return
would be
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
map sentinel. The R2 source builder's comment records this same no-scroll distinction; its
executable behavior is unchanged. A nonzero scroll-mode path writes the map byte directly and
must not receive the same sentinel interpretation.

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

## Corrected candidate single-run result

**Confirmed failure, 2026-09-19 project date (2026-09-20 02:42 UTC):**
[Issue #465's exact main-gate disposition](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/465)
admitted only the retained corrected candidate once, from accepted commit
`07dce82ac1f157e626521549aa30b62c0a064165`, tree `dff51195c746619ba4953c73a2dc71a255367399`.
The accepted MAP_CURRENT correction above was the sole capability change from the failed #460
attempt. Under ADR 0015's further-work condition this was the **second actual native process start
of the controlled-observation method**. It consumed #465's entire permission; no third controlled
start or frozen acceptance is reserved. The disabled replay's separate ordinal 1/2 consumption,
ordinal-2 timeout/cleanup **FAIL**, missing genuine receipts/ledger, ordinal-3 prerequisites and
no nominal reset/fourth-attempt rule remain unchanged. PID 42464 / frame 499 / input 145 / exit 1
from #460 remains **FAIL**, with all original artifacts and cleanup records preserved.

The retained `candidate-02` preparation used the same 23,234 input frames as #460, a 28,634-frame
watchdog and a 600-second hard timeout. Before the one `run_map3_observation_candidate` call,
read-only checks verified the accepted Git tree, no competing writer/emulator, absent runtime,
canonical ROM, complete retained source/helper/bootstrap/tool/fixture/listing identities and limits.
The ignored invocation note was written first. No material was regenerated, warmed up or repaired;
no old H3/replay command or alternative input was run. Frozen SHA-256 identities were:

| Material | SHA-256 |
| --- | --- |
| Input | `37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145` |
| Python runner | `BC0A62798DDDC4128C719E35A3C504F08FD9D634B64F9AE682614980872BAEB5` |
| Lua observer | `264C75201CCE8C4A1DAB1F2D63057395E0359EE51C7A4CAEB1C5C8327E23679D` |
| Configuration | `30E80814C4A6D240484E515253BFD98835112E2ED2976422C29AC478374B5682` |

**Confirmed bounded observations:** the 72 retained checkpoints include the original first warp
handler at `0x25978`, frame 499 / input 145: no-scroll raw map 255, observed current/effective map 3,
source `(55,3)`, movement target `(54,3)`, destination `(3,3)` / Right. Unlike the old failed attempt,
execution continued to the original controller read at frame 643 / input 289 with Map 3 `(3,3)`,
Right and no pending map event. This establishes post-warp control for this house transition;
the effective-map calculation alone would not establish it. The same frame records movement
acceptance and zone dispatch toward `(4,4)`, followed by `cs_5145C` (`0x5145C`) entry and its
stack-matched return at frame 1167 / input 813. These are reached callbacks, distinct from the
static school/north warp expectations above; neither school warp was reached.

**Confirmed failure boundary:** the original `FieldMenu` entry at `0x2127E` triggered
`candidate:unexpected-field-menu` in phase `candidate-route`, frame **1749 / input 1395**.
The observer's expected and actual PC agree. The last checkpoint before the failure record is
`field-menu:reached`; readback is Map 3 `(4,4)`, F601 set, F603/F604 clear, map-event word 0,
typewriting 0 and zero tracked pending returns. The frozen house-exit acknowledgement block had
supplied C at input frames 1393–1394; the original controller read at 1393 records that C, and
1395 is already neutral. Thus this fixed trace actually reaches the forbidden menu after the
opening script returns. It does not establish that the original route is impossible or that
any particular alternate timing would succeed. No menu choice/return/effect, corrected entity142
interaction, messenger/R2a boundary, gate/F604, north warp or Map19 terminal was observed.

**Confirmed process and cleanup:** native PID **26448** started once and exited **1**, with
`timed_out=false`, `process_terminated=true` and `timeout_tree_killed=false`. Host status is
`FAIL` / `started-failure`. Lua reports callback count zero, output removal and every recorded
scope/session restoration flag true. The host records session ROM deletion and unchanged canonical
ROM. An independent post-exit OS scan found neither the owned PID nor a process attributable to
this runtime copy; no extra process/tree termination was needed. Runtime copies, generated config,
checkpoints, typed status and full stdout/stderr remain private and retained. No terminal observation
exists. The API interval 02:42:04.211612–02:42:40.001038 UTC includes preparation/cleanup and is not
an exact native-runtime measurement.

Read-only reproduction uses `local/issue465/invocation-note.json`, `result-review.py`,
`result-summary.json` and `process-survival.json`, with the retained
`local/issue463/candidate-02/runtime/host-status.json`, `observer.status.txt` and `checkpoints.jsonl`.
Load the current ignored private-input configuration, then run
`uv run python -P -X utf8 local/issue465/result-review.py` with a fresh summary output name.
Inspect the named accepted Git object and unchanged input for the source
expectations and callback checks; do not invoke the candidate again. Direct result, material,
document/scope checks and the committed planner validate this report, not successful gameplay.

**Unknown:** later fixed-input timing/reach, the original gate/F604/north transition, first Map19
control, general callback compatibility and the continuous milestone remain open. No public golden,
full 8D or H4 result is promoted. Normal/full/H1/H2/H3 queues and new tests are excluded from this
result-only scope. The diagnostic stops here regardless of further hypotheses; independent
main-gate review of this result does not authorize another launch or complete #437.

## Fixed acknowledgement schedule: source and retained-trace review

**Confirmed, zero-launch review (Issue #469):** the frozen house block treats C as a generic
acknowledgement for longer than the opening dialogue lasts. C is also an exploration action.
The completed #465 failure therefore contradicts that scheduling assumption, not the original
route's legality. This section consumes the result above and the complete pinned source; it changes
no input, observer, fixture, golden or launch permission.

### Opening dialogue, return and subsequent actions

**Confirmed source:** `scripts_1.asm:cs_5145C` first waits for entity 128's initialization and
movement, then executes `textCursor 510`, two `nextText` commands (510, 511), `textCursor 483`,
one `nextSingleText` (483), a final `setActscriptWait 128,eas_Init`, and `csc_end`.
The pinned `gamescript.txt` gives 510/511 one `{W2}` each and 483 one `{W1}`. These are three
text commands, not twelve acknowledgement requirements. The complete script has no Yes/No prompt.

`sf2cutscenemacros.asm` maps `nextText` to command 2 and `nextSingleText` to command 0.
`mapscriptengine_2.asm:csc02_displayTextbox` calls `DisplayText` and increments the cutscene
cursor; `csc00_displaySingleTextbox` additionally closes portrait/dialogue windows and sleeps
10 ticks before returning. `textfunctions_1.asm` implements `{W2}` at
`ParseSpecialTextSymbol:@wait2/loc_6472` and `{W1}` at `symbol_wait1/loc_659C/loc_65B4`.
Both wait for ordinary directional/A/B/C input through `CURRENT_PLAYER_INPUT`; both temporarily
clear `CURRENTLY_TYPEWRITING` while waiting. Thus typewriting 0 alone does not mean input-ready.
`HandleDialogueTypewriting` in `textfunctions_2.asm` can also shorten character delay when
`PLAYER_1_INPUT` is nonzero; a supplied pulse is not necessarily a completed acknowledgement.

**Confirmed retained observations:** all ordinals below are the existing candidate's one-based
input clock, with observer frame = input + 354. Entry/return pairs are the observer's original-PC,
stack-matched callbacks, not reconstructed timings.

| Input ordinal | Observed boundary |
| --- | --- |
| 289 | `ExecuteMapScript` entry for `cs_5145C`; zone target `(4,4)` |
| 339–553; 557–673 | `DisplayText` entry/return for 510, then 511; returns coincide with C pulses |
| 677; 790; 793 | Text 483 entry; `symbol_wait1`; actual `loc_65B4` acknowledgement read `0x20` (C), then text return |
| 793–802; 813 | Single-text dialogue close; script return at `0x58C`; the zone wrapper's extra close also enters/returns at 813 |
| 913; 916–1033; 1042 | Controller reads C; a new trap-based text 483 enters/returns at `0x6260`/`0x574`; its dialogue close returns |
| 1153; 1156–1273; 1282 | Controller reads C; another trap-based text 483 and dialogue close complete |
| 1393; 1395 | Controller reads C; `FieldMenu` entry `0x2127E` fails the candidate, although the input table is neutral by 1395 |

The observer instruments `{W1}`'s read, not `{W2}`'s loop. The first two text returns and concurrent
C pulses are **Confirmed**; their exact internal `{W2}` read times are **Unknown**. The later
`text:wait1` records carry cutscene cursor 484, but the paired `DisplayText` target is 483:
trap-based `txt` does not set that cutscene cursor. Those records do not establish text 484.

**Confirmed source:** after `cs_5145C`, `s3_zoneevents.asm:Map3_ZoneEvent6` calls
`MakeEntityWalk` for selector 128 with `(5,6,1)`, sets F601 and returns. `MakeEntityWalk` and
`entityfunctions_2.asm:SetWalkingActscript` install walking work without waiting for its completion.
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` closes windows, waits a VInt and waits for the
player to stop before returning to exploration. Script return is therefore not the whole caller's
return or a proof of completed NPC movement. The observed F601 readback changes after script
return; the first retained subsequent nonzero controller read is 913. The exact first ready frame
between those boundaries is **Unknown**, since neutral controller reads are not logged.

`explorationfunctions_2.asm:WaitForEvent/loc_2593C` tests A/C after pending map events;
`ExplorationLoop/loc_2587E` dispatches `ProcessPlayerAction`. In `explorationvints.asm`, that
function saves `PLAYER_1_INPUT` in D7 before waiting for player/view movement. A goes to
`loc_25BCC`; ordinary C goes through `GetActivatedEntity`, entity-event dispatch, then
`CheckArea` if no entity was selected. If neither yields an action, control falls through to `j_FieldMenu`
(`s05_jumpinterface.asm` resolves it to `FieldMenu`). Releasing C after dispatch need not cancel
the saved action. `esc02_controlCharacter` at `0x4FF8` separately chooses `PLAYER_1_INPUT` when
D7 is nonzero, matching the observed D7=48/value=32 reads; it is not itself a menu-entry callback.

**Inferred:** the two post-opening displays are re-interactions with entity 128: the F602-clear
`s2_entityevents.asm:Map3_EntityEvent2` displays 483, matching the two trap-based calls, player
position/facing and the source's departing NPC. The final C's ordinary entity/area fallback
explains the later menu entry. **Unknown:** the exact selected entity/negative lookup result,
NPC position and area-check result at each action were not recorded by this candidate. Do not
promote the plausible explanation that the NPC moved out of interaction range to an observed fact.
The confirmed repeated text and menu callbacks already establish that these extra pulses were
not harmless opening-dialogue acknowledgements.

### Later blocks: complete source, no later runtime claim

**Confirmed construction/source; Unknown reach and timing:** direct expansion of the documented
recipe equals all 23,234 frozen input frames. Each listed block repeats 2 C + 118 neutral; ranges
include its final neutral tail. Later rows were never reached by #465. They share the same risk
whenever C outlasts the intended consumer; source command counts cannot prove which frame crosses
that boundary or justify replacing the pulse count with a text count.

| Block / input range / pulse count | Complete owning flow and acknowledgement boundary |
| --- | --- |
| House / 313–1752 / 12 | `cs_5145C`: 510, 511, 483 as above. Pulses start at 313 + 120k, k=0..11; seven start after the observed script return. |
| Sarah classroom / 3433–4872 / 12 | Intended first `Map3_EntityEvent0`, F602/F603/F256 clear: `txt` 512 (`W2`), 480 (no explicit W token), 481 (`W1`), then complete movement-only `cs_513D6` and F256. Later interactions take the flag-dependent branch; the entity-event wrapper closes windows. |
| Astral introduction / 5161–6600 / 12 | `Map3_ZoneEvent7`, F602/F603 clear: portrait and `txt` 513 (`W1`), then return and wrapper close. No twelve-acknowledgement loop. |
| Entity 142 / 6937–9816 / 24 | After the frozen Left-facing step and actual C interaction: `Map3_EntityEvent15` conditionally displays 500 (`W2`) and sets F261, then 501 (`W1`) and F602. Repeating C can select later interactions; F602-related re-init `cs_513A0` only positions Sarah, with no text. |
| Astral zone / 9985–11424 / 12 | `Map3_ZoneEvent7`, F602 set and F603/F260 clear: 514 (`W2`), 515 (`W2`), 516 (`W1`), then complete positioning-only `cs_5148C`, F260 and wrapper close. |
| Messenger / 11881–19080 / 60 | Complete `cs_5149A` through its branches: accepted Yes path uses 517–531, 535–536, a real Yes/No prompt/F89 branch, plus `csc08_joinForce` text 447 and `FadeOut_WaitForP1Input`. The decline path uses 532–533 instead of the accepted join ending. Entity movement/waits separate text commands; the caller sets F603 after script return. |
| Gate / 19825–22704 / 24 | Complete `cs_51652`: six `nextSingleText` commands, 537–542 (each `W1`), between two guard-action groups; entity 139 is awaited, 138 is not. `Map3_ZoneEvent4` commits F604 after script return, then the zone wrapper returns. |

The messenger block cannot be treated as text acknowledgements alone: `YesNoPrompt` initializes
choice zero, may wait for release of entry input, handles left/right selection and C/A confirmation,
and treats B as No. Join text 447 has no explicit W token, but its caller separately waits for
player input. Replacing every C with B or assigning one pulse per displayed command is therefore
not a source-supported correction for the entire route. The gate's six real text commands remain
required; the withdrawn no-dialogue assertion is not revived. None of these static branches proves
the proposed later arrival state, completion frame, selected prompt result or absence of extra C
actions in the unshimmed candidate.

### Bounded correction proposal and reproduction

**Inferred correction proposal, not executed:** preserve the house prefix through the final
observed acknowledgement at 793–794 and replace only the seven remaining house C pairs
(913–914, 1033–1034, 1153–1154, 1273–1274, 1393–1394, 1513–1514, 1633–1634) with neutral frames.
Keep the block length and subsequent ordinals unchanged. This removes the demonstrated extra
actions at their cause; neutralizing only 1393 would retain the two unintended conversations and
later surplus pulses. It adds no adaptive input or observer behavior. This is a concrete candidate
correction for independent review, not an established successful replacement: altered interaction
and RNG/NPC evolution can change later state, and the downstream blocks have no measured safe
cutoff. No complete fixed-frame route replacement can be established from the retained failure.

Reproduce the static review against `ShiningForceCentral/SF2DISASM` at
`c834c652b6862bc5679fd7f69a38a7093206efc6`, with paths relative to `disasm/`:

- `data/maps/entries/map03/mapsetups/{scripts_1,s2_entityevents,s3_zoneevents,s6_initfunction}.asm`
  supplies the complete programs/callers above; `scripts_1.asm` blob is
  `5e9b260b9e07dd22387a0ab1ab3b6666d1ab05e1`.
- `data/scripting/text/gamescript.txt` blob `d1f5c1fa20ff2a2d442408d71d2dcfbffc2cb7bd`
  supplies hex-indexed text control tokens. Retain IDs/tokens only, not dialogue prose.
- `code/common/scripting/map/{mapscriptengine_2,mapsetupsfunctions_1}.asm`,
  `code/common/scripting/text/textfunctions_{1,2}.asm`,
  `code/common/scripting/entity/{entityscriptengine_2,entityfunctions_2}.asm`, and
  `code/gameflow/exploration/{explorationfunctions_0,explorationfunctions_2,explorationvints}.asm`
  supply the named consumers, return order, action dispatch and input masks.
- `sf2cutscenemacros.asm`, `code/common/tech/interrupts/trap5_textbox.asm`,
  `code/common/menus/yesnoprompt.asm`, and
  `code/gameflow/battle/battlefunctions/battlefunctions_0.asm:FadeOut_WaitForP1Input`
  resolve macro/trap/prompt/join behavior; the latter calls `code/common/tech/input.asm:WaitForPlayerInput`.

Use `git -C local/upstream/SF2DISASM show <pinned-commit>:disasm/<path>` for these source sections.
Read the unchanged `local/issue463/candidate-02/input.json` (identity in the preceding result) and
its `runtime/checkpoints.jsonl`, `host-status.json`, `observer.status.txt`; filter checkpoint kinds
`script:*`, `DisplayText:*`, `CloseDialogueWindow:*`, `text:*`, `input:original-controller-read`
and `field-menu:reached`. The read-only local `local/issue469/audit.py` compares the reviewed source
files to pinned Git objects, expands the existing recipe in memory and reports only IDs/control
facts from the retained 72 checkpoints. Run with
`.venv/Scripts/python.exe -P -X utf8 local/issue469/audit.py`; it creates no candidate or runtime.
The [observer](../../tools/bizhawk/map3_messenger_acceptance_observer.lua)'s `install_candidate`
owns the recorded fields and callback limitations; H2 projections are not substitutes for these
complete scripts.

Acceptance is direct source/trace, document/link/scope and `git diff --check` review, then the
clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual public CI.
The planner's generic `public-core` selection does not authorize local normal/full/H1/H2/H3,
warmups, materialization or tests of verification programs for this documentation-only slice.
Both controlled starts remain consumed, both completed failures and the separate disabled replay
ordinal-1/2 restrictions remain preserved. This proposal grants no third start, runtime permission,
natural-continuity claim, full 8D/H4 acceptance or completion of #437.

## Prepared house-neutral correction (not executed)

**Confirmed preparation only, 2026-09-19 project date:**
[Issue #471](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/471) materializes the
[bounded proposal above](#bounded-correction-proposal-and-reproduction) from accepted commit
`ddd17b2442455ac4ab9864418ed2f30f4837fda6`, tree `d7e8d6b06f7f9537e6d72e55388dcccc328c771c`.
The original `local/issue463/candidate-02/input.json` remains unchanged at SHA-256
`37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145`.
Only one-based ordinals **913–914, 1033–1034, 1153–1154, 1273–1274, 1393–1394,
1513–1514 and 1633–1634** change from C to neutral (empty string) in the new private copy.
Direct comparison confirms exactly 14 changed entries, identical remaining frames and metadata,
23,234 total frames and no insertion, deletion or ordinal shift. The prefix through 793–794,
the remaining house tail and every later block retain their original positions and contents.

The fresh input is `local/issue471/house-neutral-input.json`; the prepared output is
`local/issue471/candidate-01/`. The existing `prepare_map3_observation_candidate` completed its
first and only call for this Issue with `CANDIDATE-PREPARED-NOT-ADMITTED`, `EmulatorLaunches=0`,
23,234 input frames, proposed timeout 600 seconds and watchdog 28,634 frames. Its source/H1/ROM
binding and Lua syntax checks passed without building H1 or starting an emulator. The ROM's narrow
identity verifier also passed. The candidate has no `runtime/` directory; an OS process check found
zero EmuHawk processes. No launch API, runtime materialization or no-ROM warmup was invoked.

| Prepared material | SHA-256 |
| --- | --- |
| New input (standalone and candidate copy) | `4BA1E4738877A72866BCC0B6A644F75479FB5A9E21CC761E7D99FEC24403A76A` |
| `candidate.json` | `B7D8D6116490DD5A1DB122F23275603273695A66B32D2C0CA30C7211A63D28BF` |
| `config.json` | `47D7A3C287B3DEB0ABDF429DFA0C782BB8873826A4CBAC5310F5B3014B89A405` |
| `config.lua` | `19BFFE2BB3AC0B1D90698436E283A70714A97CFAD86077ADF477C1670A775A3A` |
| Current `src/sf2tool/h3/bizhawk.py` | `832966617D63CA97B341C09F684F0E7021B16CAF4578A4D27B77F82AD8779B64` |
| Shared BizHawk executable | `F8CDB93551A544F680BF3876D9D8D72643859E7A44A23B04E1A25B92E48F80CD` |
| Shared Lua library | `4786E0DF4CAF120E3BEDF0B6DDA260525DF2187C66DED220A21A53ACE76B0501` |

The report binds USA ROM `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`,
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, listing
`FA21225556FC916ED287A42E8D2451A15326B4FBF02816DDE7D82C6D4354E982`, every selected source,
retained fixture and execution helper. Runner and observer identities equal those in the
[completed #465 result](#corrected-candidate-single-run-result). The current helper includes the
accepted default mute change; no existing runtime configuration was changed. A parsed comparison
with the retained candidate-02 configuration finds only the 14 frames, their input identity and
fresh output/checkpoint/status paths changed. All other observation configuration is identical;
no adaptive input, callback semantics or downstream schedule was added.

Reproduce the input transformation below only into a fresh ignored destination. The original has
CRLF line endings; the new copy uses the same observed serialization. Neither parsing nor
comparison rewrites the original. The accepted source rationale remains the fixed-schedule review
above; these assertions verify the transformation, not successful gameplay.

```python
import json
from copy import deepcopy
from hashlib import sha256
from pathlib import Path

original = Path("local/issue463/candidate-02/input.json").read_bytes()
assert sha256(original).hexdigest().upper() == (
    "37544D8C526F41A60D13B79971744D0A99303BCB08E407FBB1AE01C8AD7F1145"
)
old = json.loads(original)
new = deepcopy(old)
changed = [n for start in (913, 1033, 1153, 1273, 1393, 1513, 1633)
           for n in (start, start + 1)]
for n in changed:
    assert old["frames"][n - 1] == "C"
    new["frames"][n - 1] = ""
assert len(old["frames"]) == len(new["frames"]) == 23234
assert {k: v for k, v in old.items() if k != "frames"} == {
    k: v for k, v in new.items() if k != "frames"
}
assert [i + 1 for i, (a, b) in enumerate(zip(old["frames"], new["frames"], strict=True))
        if a != b] == changed
new_bytes = (json.dumps(new, indent=2) + "\n").replace("\n", "\r\n").encode("utf-8")
assert sha256(new_bytes).hexdigest().upper() == (
    "4BA1E4738877A72866BCC0B6A644F75479FB5A9E21CC761E7D99FEC24403A76A"
)
with Path("local/<fresh-review>/house-neutral-input.json").open("xb") as stream:
    stream.write(new_bytes)
```

Load `. ./local/private-inputs.ps1` in the launching PowerShell process, run `uv run sf2 rom verify`,
then use `uv run python -P -X utf8` and the existing
[preparation API example](#reproduce-preparation-without-execution), substituting that new input,
an absent `local/<fresh-review>/candidate-01` output and `proposed_timeout_seconds=600`.
The report/config identities above name the retained Issue #471 destination; another destination
changes generated path-bearing configuration and its identity. Read the retained candidate for
review instead of overwriting or regenerating it.

The actual invocation and PASS output are retained as
`local/issue471/preparation-command-02.txt` and `preparation-output-02.txt`, with
`preparation-summary.json`, `rom-verify.txt` and `process-check-02.json`. The initial
`preparation-command.txt` / `preparation-output.txt` and `failure-state.json` preserve a completed
local inspection **FAIL**: an extra assertion required original bytes to equal LF-only JSON
serialization despite the original's CRLF. It failed before any new input or preparation API call.
Main-gate independently checked that boundary and authorized removal of this extra assertion in
the same task; the original was never normalized or written back. This was not a preparation API,
ROM, toolchain or game failure, and no native attempt was retried.

**Unknown:** downstream arrival, timing, NPC/RNG evolution, prompt choices, later C consumers,
gate/F604/north/Map19 reach, callback/cleanup compatibility and natural continuity remain unobserved.
The transformation removes demonstrated surplus actions from the input; it proves no successful
route or original-game outcome. Full 8D/H4 and #437 remain incomplete.

Acceptance is direct input/source/preparation and document/link/scope/whitespace checks, followed
by the clean committed planner and actual public CI. Local normal/full/H1/H2/H3 queues and tests
of verification programs are **NOT RUN**, outside this preparation-only scope. The completed
#460 and #465 **FAIL** results and original artifacts remain preserved. Controlled-method actual
starts remain **2 with no remaining permission**; disabled replay ordinal-1/2 consumption,
timeout/cleanup failure, missing genuine receipts, ordinal-3 prerequisites and retry/reset/fourth-
launch prohibitions remain separate and unchanged. This Issue, new identity, preparation PASS or
independent PR review grants no third controlled start or frozen acceptance. Stop at the Draft PR
for independent main-gate review; there is no launch, merge or cleanup authorization.
