# Battle Scene Command and Presentation-Data Contract

- **Confirmed original behavior:** the 21-command scene interpreter, scene initialization and
  selector order, complete 32-slot spell setup/update dispatch, 208 actor-animation sequences, and
  the complete background, actor-sprite, weapon, ground, spell, invocation, status, and transition
  graphics container boundaries described below.
- **Unknown original behavior:** exact command and frame timing, VInt/VDP effects, palette-transition
  appearance, visible layer composition and placement, the 512-byte invocation-transfer tails,
  natural reachability of every selector/index combination, and rendered frame parity.
- Remake status: implementation-neutral Phase 3 contract; no renderer, animation graph, asset format,
  frame-rate policy, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract owns the static boundary between an ordered battle-scene command stream and the data,
selectors, loaders, and dispatch tables used to present that stream. It defines:

1. the command-buffer interpreter and command identities;
2. scene initialization, actor/weapon/background selection, and layer-load order;
3. ally/enemy animation selection and sequence data;
4. background, actor-sprite, weapon, and ground container/load contracts;
5. spell-animation setup/update dispatch and battle-effect graphics corpora;
6. the state that a future compatibility adapter must expose without prescribing a renderer.

It does not own action choice, target choice, action construction, damage or spell arithmetic,
persistent command replay, battle outcome, input/UI policy, or rendered timing. Those remain with
[Battle AI decision](battle-ai-decision.md),
[battle action construction](battle-action-construction.md),
[combat resolution](combat-resolution.md), [spell resolution](spell-resolution.md), and
[battle control/lifecycle](battle-control-lifecycle.md).

The executable owners are:

- `sf2-battle-scene-engine-static-v1` in
  `tests/fixtures/h2/battle-scene-engine-static-v1.json`;
- `sf2-battle-scene-animations-static-v1` in
  `tests/fixtures/h2/battle-scene-animations-static-v1.json`;
- `sf2-battle-background-decode-v1` in
  `tests/fixtures/h2/battle-background-decode-v1.json`;
- `sf2-battle-sprite-decode-v1` in
  `tests/fixtures/h2/battle-sprite-decode-v1.json`;
- `sf2-battle-sprite-animation-static-v1` in
  `tests/fixtures/h2/battle-sprite-animation-static-v1.json`;
- `sf2-battle-weapon-ground-decode-v1` in
  `tests/fixtures/h2/battle-weapon-ground-decode-v1.json`;
- `sf2-battle-effect-graphics-decode-v1` in
  `tests/fixtures/h2/battle-effect-graphics-decode-v1.json`.

The research owner is [Battle Scene Engine](../../research/battle-scene-engine.md), with compression
and loader context in [Technical Graphics and Decompression Services](../../research/technical-graphics.md).

## Pre-Contract Evidence Audit

This slice checked the owning research prose, all seven fixture payloads and exact IDs, their H2
verifiers, the research-index bindings, and the focused reproduction commands before synthesis. The
owners agree on table sizes, stream counts, decoded/transfer units, selector order, and the static
presentation boundary.

Two scope corrections govern this contract:

- `sf2-battle-scene-replay-v1` observes persistent HP and EXP mutation after two battle-scene
  commands. It is evidence for [combat resolution](combat-resolution.md), not evidence for rendered
  scene behavior, so it is not registered here.
- source names such as `TintScreen`, `VInt_UpdateBattlesceneGraphics`, and animation setup/update
  labels identify static dispatch and call structure. They do not confirm visible color, timing,
  composition, or frame output.

No accepted runtime presentation matrix exists. This contract therefore makes no fresh H3 claim and
does not convert the research runtime queue into implied behavior.

## Canonical Scene State

An original-fidelity adapter MUST preserve these distinguishable domains:

| State domain | Contract role |
| --- | --- |
| scene command stream | ordered command words and parameters constructed by battle actions |
| scene interpreter state | command cursor, dead-combatant list, actor/target switches, waits, and return state |
| selection state | actor sides, sprite/palette IDs, weapon, ground, background, animation index, mirror and variant bits |
| decoded resource state | background, actor, weapon, ground, spell, invocation, status, and transition payloads |
| sequence state | actor frame entries, offsets, weapon fields, spell triggers, and hold/default flags |
| presentation-driver state | palettes, layouts, optional layers, VInt registrations, DMA requests, fade and phase/toggle state |
| persistent battle state | HP, MP, status, EXP, gold, items, death, and battle outcome owned by other contracts |

The command stream is not a rendered frame, and decoded bytes are not proof of visible pixels. A
remake MAY use different internal structures, but an H4 adapter must still expose the confirmed
identities, order, selectors, sizes, and aliases at this boundary.

## Scene Command Interpreter

**Confirmed static:** `ExecuteBattlesceneScript` reads word commands from the buffer at `0xFF0000`,
clears the dead-combatant list, seeds its first entry with `0xFF`, stops on word `0xFFFF`, dispatches
through a 21-entry relative jump table, and returns zero.

The command identities cover:

- enemy/ally action animation and sprite movement;
- enemy/ally idle and actor switching;
- enemy/ally reaction;
- actor-idle/end, end, and sleep;
- EXP award;
- message display, no-wait message display, text-box close, and player-input wait;
- one null command.

The command numbers and handler order are compatibility facts. The labels `sleep`, `wait`, and
`displayMessageWithNoWait` do not establish elapsed time, accepted input cadence, audio behavior, or
player-visible completion. Those effects remain **Unknown** until a bounded runtime owner exists.

The scene interpreter consumes the stream produced by
[battle action construction](battle-action-construction.md). Persistent reaction and reward effects
are separately evidenced by combat/spell fixtures; this contract does not infer every mutation from
the static handler names.

## Initialization and Selector Order

**Confirmed static order:** scene initialization clears its scene-data block, resolves enemy and ally
graphics, resolves weapon and background selectors, clears existing VInts, loads palette/layout
state, conditionally loads enemy, ally, ground, and weapon layers, adds
`VInt_UpdateBattlesceneGraphics` and `VInt_UpdateWindows`, loads status-animation tiles, applies
status-animation state, and then reaches the fade-in seam.

Selector boundaries are equally specific:

- weapon graphics are ally-only; an invalid weapon yields sprite/palette `(-1, -1)`;
- background selection prioritizes Zeon, then a battle-specific override, then terrain;
- initialization prefers the enemy actor and then the ally actor for background context;
- when no current background actor exists, the selector uses the saved actor and then combatant 0;
- ally selection defaults to regular attack, uses a separate dodge block, and remaps KNTE, PLDN, and
  PGNT spear/javelin attacks to direct special entries.

This order is not an engine-module prescription and does not prove what a player sees between calls.
The exact VInt, DMA, palette, fade, and optional-layer effects remain **Unknown**.

## Actor Sprite Containers and Animation Sequences

### Sprite containers and frame loading

The separate ally and enemy pointer tables contain 32 and 54 entries. All 86 payload identities and
their source/ROM bytes are closed by the fixture. The payload corpus contains 167 palettes and 408
compressed frames:

| Side | Pointers/payloads | Palettes | Frames | Decoded bytes |
| --- | ---: | ---: | ---: | ---: |
| ally | 32 | 59 | 153 | 705,024 |
| enemy | 54 | 108 | 255 | 1,566,720 |

Property loading stores the animation-speed word and following status-icon X/Y bytes, resolves the
palette relative to header word 2, clears destination color 0, and copies the remaining 15 palette
words. Frame loading resolves a self-relative word beginning at header byte 6 and Stack-decodes the
selected stream before DMA. Fixed DMA lengths are `0x900` words for ally frames and `0xC00` words for
enemy frames, matching decoded frame sizes 4,608 and 6,144 bytes.

These are container and transfer boundaries. Header offsets do not establish on-screen coordinates,
palette appearance, sprite-layer placement, or timing.

### Animation sequence tables

The sequence corpus contains 208 entries: 87 ally and 121 enemy animations. Across both sides there
are 421 frame entries and 334 played attack-frame entries.

- ally headers and entries are eight bytes each; entry zero also serves as optional idle frame two,
  so attack playback skips it and consumes 147 later entries;
- enemy headers and entries are four bytes each; all 187 entries are attack frames;
- 43 entries use frame value 15 to retain the previous battle-sprite frame;
- seven headers request a default spell animation;
- every terminate-spell flag is zero in the shipped corpus.

Normal attacks use the combatant base animation index. Dodge adds 40 for allies or 60 for enemies.
Indices at or above 80 for allies or 118 for enemies are direct special entries. The ally spear
remap uses direct indices 80 through 82 for KNTE, PLDN, and PGNT.

Reachable base-index combinations, frame duration, frame-15 appearance, weapon
flip/layer/offset interpretation, and spell-trigger timing remain **Unknown**.

## Background, Weapon, and Ground Containers

### Backgrounds

The background table has 30 pointers resolving to 27 unique payloads. Backgrounds 21 and 22 alias
payload 12; background 29 aliases payload 13. Each container supplies three relative-offset words,
two Stack-compressed tilesets, and one 32-byte palette.

`LoadBattlesceneBackground` decodes both tilesets with the second destination `0x1800` bytes after the
first. Every stream decodes to 6,144 bytes; the complete 54-stream corpus decodes to 331,776 bytes.
The loader clears palette word 0 and copies the remaining 15 words. This contract preserves the two
stream identities, offset, palette rule, and aliases, but not their visible arrangement.

### Weapons and ground

The combined corpus has 53 pointers: 23 weapon entries and 30 ground entries. All 23 weapon graphics
decode to 8,192 bytes and each payload contains four 64-tile views. The contiguous weapon-palette
entry supplies the final two colors of the ally battle-sprite palette.

The 30 ground pointers resolve to 27 headers and ten unique graphics payloads. Grounds 21/22 alias
ground 12 and ground 29 aliases ground 13. A ground header supplies three palette words followed by a
self-relative tileset word; each unique graphics stream decodes to 1,536 bytes and requests
`0x300` DMA words.

These counts and load units do not establish weapon angle selection, ground/background composition,
tilemap placement, palette priority, or visible layer order.

## Spell Animation Dispatch

Setup and update each expose 32 dispatch slots. The setup side has 32 unique targets across 29 files;
Buff and Debuff setup files are shared. The update side has 28 unique targets across 26 child files
plus two root-owned targets:

- Absorb is reused at slots 10 and 24;
- Buff is reused at slots 8 and 25;
- Debuff is reused at slots 9, 27, and 28;
- `spellanimationUpdate_Nothing` and `spellanimationUpdate_Absorb` are root-owned.

Every child animation file is reached by at least one setup/update slot. Setup preserves the mirror
bit and stores the decoded variant as one-based; disabled setup and index `-1` return without
dispatch. Update requires its toggle and phase state before dispatch.

Slot identity, reuse, and gating are **Confirmed static**. Per-frame state changes, setup/update
cadence, simultaneous effects, interrupt behavior, palette transitions, and visible animation
results remain **Unknown**.

## Battle-Effect Graphics Corpus

The battle-effect owner closes 56 Stack-compressed streams:

| Family | Confirmed corpus |
| --- | --- |
| spell | 23 graphics containers |
| invocation | 4 containers, 15 frames, 30 streams |
| status | 1 stream |
| battle transition | 2 streams |

Together the streams decode to 200,992 bytes. Each invocation stream decodes to 4,096 bytes, while
both invocation consumer paths transfer 4,608 bytes. The 512-byte tail per transfer is therefore an
explicit consumer boundary: its bytes, stability, and visibility are **Unknown** and MUST NOT be
invented from adjacent data.

The corpus proves resource, pointer, table, decode-size, and transfer-size relationships. It does not
prove palettes, layer ordering, frame timing, transition composition, or rendered output.

## Fidelity and Modernization Boundary

An original-fidelity adapter MUST preserve:

- command word identities, 21-entry dispatch order, terminator, dead-list initialization, and return
  state;
- scene initialization and selector chronology at the accepted static seam;
- separate actor-sprite containers, sequence tables, spell dispatch tables, and presentation-driver
  state;
- exact pointer/payload counts, aliases, palette rules, decoded sizes, DMA/transfer units, and
  setup/update reuse;
- the distinction between decoded payload, requested transfer, command/replay state, and visible
  presentation;
- every itemized **Unknown** boundary rather than filling it from labels or modern conventions.

A future remake MAY use decoded modern assets, a different renderer, typed scene commands, a modern
animation graph, asynchronous resource loading, higher frame rates, skippable effects, accessibility
options, or redesigned transitions. These are product decisions. A deliberate deviation requires an
explicit decision and an H4 expected-deviation fixture; it is not a newly discovered original rule.

## H4 Acceptance Surface

A future H4 adapter should consume the seven fixtures named by this contract and compare:

1. command-buffer initialization, ordered command dispatch, termination, and return state;
2. selector inputs/results and scene initialization event order;
3. actor sprite/palette/header fields and decoded frame byte counts;
4. all 208 sequence identities, frame records, hold/default flags, and selector-index rules;
5. background, weapon, and ground pointer aliases, palette operations, decoded sizes, and transfer
   requests;
6. all 32 setup/update slot identities, reuse, disabled/minus-one gates, mirror/variant values, and
   update toggle/phase admission;
7. all 56 battle-effect stream identities, decoded lengths, invocation transfer lengths, and the
   declared unknown tail boundary.

H4 MUST compare canonical records rather than original RAM/ROM addresses when the remake has no
equivalent memory layout. Rendered pixels, frame pacing, VDP/VInt chronology, palette appearance,
audio/input timing, natural reachability, and persistent combat results require separate accepted
owners.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| command interpreter, initialization, selectors, spell setup/update tables | **Confirmed static** | `sf2-battle-scene-engine-static-v1` ([`battle-scene-engine-static-v1.json`](../../../tests/fixtures/h2/battle-scene-engine-static-v1.json)) | Runtime command cadence, VInt/VDP/fade effects, rendered result |
| complete setup/update source pairing | **Confirmed static** | `sf2-battle-scene-animations-static-v1` ([`battle-scene-animations-static-v1.json`](../../../tests/fixtures/h2/battle-scene-animations-static-v1.json)) | Per-frame state and visible animation |
| background containers, aliases, decode/load units | **Confirmed static** | `sf2-battle-background-decode-v1` ([`battle-background-decode-v1.json`](../../../tests/fixtures/h2/battle-background-decode-v1.json)) | Tile arrangement, layer composition, visible palette |
| ally/enemy sprite containers, palettes, frames, DMA units | **Confirmed static** | `sf2-battle-sprite-decode-v1` ([`battle-sprite-decode-v1.json`](../../../tests/fixtures/h2/battle-sprite-decode-v1.json)) | Placement, timing, rendered frames |
| 208 actor-animation sequences and selector rules | **Confirmed static** | `sf2-battle-sprite-animation-static-v1` ([`battle-sprite-animation-static-v1.json`](../../../tests/fixtures/h2/battle-sprite-animation-static-v1.json)) | Reachable base-index combinations, timing, weapon-field interpretation |
| weapon/ground containers, palettes, aliases, decode/load units | **Confirmed static** | `sf2-battle-weapon-ground-decode-v1` ([`battle-weapon-ground-decode-v1.json`](../../../tests/fixtures/h2/battle-weapon-ground-decode-v1.json)) | Angle selection, placement, composition |
| spell/invocation/status/transition graphics streams | **Confirmed static** | `sf2-battle-effect-graphics-decode-v1` ([`battle-effect-graphics-decode-v1.json`](../../../tests/fixtures/h2/battle-effect-graphics-decode-v1.json)) | Invocation tail bytes, palettes, layer/timing/transition composition |
| persistent HP/EXP scene-command replay | **Separate confirmed runtime subset** | [combat-resolution contract](combat-resolution.md) | Not evidence for rendered presentation |
| rendered pixels, frame timing, VInt/VDP effects, presentation intent | **Unknown** | No accepted executable owner | Requires a grouped runtime presentation matrix or future product decision |

## Reproduction

```powershell
uv run sf2 h2 battle-scene-engine
uv run sf2 h2 battle-scene-animations
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 battle-sprite-animations
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 battle-effect-graphics
uv run sf2 design-contracts test
uv run sf2 research-index test
```
