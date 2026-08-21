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
