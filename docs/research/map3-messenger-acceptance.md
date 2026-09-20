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

**Inferred / not runtime-admitted:** Issue #456 adds `prepare_map3_observation_candidate` and
`run_map3_observation_candidate` to the existing Python owner, with an opt-in `candidate` mode in
the same observer. The old command, default R2a cases, schemas and observed fixture remain unchanged.
Neither Python function is wired into the CLI. Preparation performs no original emulator launch;
the execution composition is for a later independently admitted method and lineage/budget decision.
Its result is `OBSERVATION-COMPLETE-UNREVIEWED`, never a public golden or H4 verdict.

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
single physical-PC dispatcher handles shared return sites. Callback failures reach the existing
typed status and nonzero exit; JSONL preserves actual checkpoints and the last checkpoint on failure.

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
the execution composition refuses to overwrite an attempt, uses the shared process helper with a
contained absolute output stem, copies the canonical ROM to a disposable session, preserves
checkpoint/status/host diagnostics and checks canonical identity after deleting that session.
Preparation itself creates only `input.json`, `config.json`, `config.lua` and `candidate.json` in an
explicit fresh worktree-local ignored directory. Missing input is `FileNotFoundError`/unavailable;
bad input/source binding is rejected before materialization, never `PRELAUNCH-PASS`.

### Reproduce preparation without execution

After loading the current ignored private-input configuration, this API prepares a supplied trace:

```python
from pathlib import Path
from sf2tool.h3.map3_messenger_acceptance import UPSTREAM, prepare_map3_observation_candidate
from sf2tool.private_inputs import ROM_INPUT_IDENTITY, private_input_path

prepare_map3_observation_candidate(
    private_input_path(ROM_INPUT_IDENTITY), UPSTREAM,
    input_path=Path("local/issue456/diagnostic-input.json"),
    output_directory=Path("local/issue456/review-candidate"),  # must not exist
)
```

Run through `uv run python -X utf8` in the owning worktree. The closed input object has
`clock="first-r1-wait-next-frame"`, `provenance="diagnostic-parameters"` and `frames`: 1–36,000
explicit single-button strings (`Up`, `Down`, `Left`, `Right`, `A`, `B`, `C`, or empty for neutral).
No conditions, coordinates, executable expressions or unknown timing values are accepted as input.
The report binds ROM/source/listing/observer/configuration/input identities and retains Unknowns.

The private review trace's reproducible **diagnostic** recipe uses R2
`expectedObservation.records[0].logicalInputTrace` in order and R2b
`static.routeGraph.segments[0].inputs` / `[2].inputs`; these provide logical directions, not observed
timing. Start with 120 neutral frames; give each logical input 2 pressed + 22 neutral frames. At each
R2 waypoint's last input, add 12 repetitions of 2 C + 118 neutral for `map3-house-exit-zone`,
`map3-sarah-classroom`, `map3-astral-zone-introduction`, `map3-entity142`, `map3-astral-zone`; add 120
neutral for other waypoints. Then add 60 C/neutral repetitions for the messenger, segment 0 inputs,
24 C/neutral repetitions for the gate, segment 2 inputs, 240 neutral, 2 Up, and 120 neutral.
This produces 23,234 frames. These deliberately unvalidated hold/release/wait parameters may exhaust
or enter FieldMenu; neither reachability nor compatibility is claimed. They must be reviewed before
any launch and may not be adapted to live state. The material is an inspectable method candidate,
not an accepted playback recording or proof that this input reaches Map19.

Direct acceptance consists of source/H1/ROM binding, materialization and rejection checks,
Python lint/compile, Lua compilation, normal research verification and the clean committed planner.
Selected original-runtime gates are **NOT RUN / pending independent launch admission**. The local
environment needed an explicitly authorized pinned checkout, verified tool copies and one real
bit-perfect H1 build; its kept listing/log precede preparation and are not runtime evidence. Shared
tool consumer migration is separately owned; it does not invalidate that completed H1 result.

Old R2b/replay ordinal-2 timeout **FAIL**, cleanup failure, missing genuine receipts/ledger,
retry/reset prohibition and completed #431/#434 failures remain preserved by the
[audit dossier](map3-battle01-audit.md#first-necessary-original-observation-dossier).
This candidate grants no launch allowance and supplies no natural continuity, full 8D or H4 PASS.
