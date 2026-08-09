# Startup Control Flow Contract

- Status: **Confirmed static startup routing and bounded initialization extents**
- Evidence date: 2026-08-09
- Scope: implementation-neutral reconstruction of the original `Start`, `InitializeSystem`,
  `InitializeGame`, intro/title handoff, and region-gate control surface, without converting
  source-static loop extents into hardware timing or importing asset, audio, new-game, input,
  presentation, persistence, or platform-lifecycle semantics

## Judgment Boundary

This contract begins at the source-shaped `Start` entry. It follows the optional initial setup block,
the common DMA-busy wait, the system and game initialization handoffs, the bounded intro/title return
routes, and the region admission branch. It ends at each downstream subsystem handoff or source-local
terminal branch.

- **Confirmed**: the initial setup block is conditionally skipped by the original `CTRL1`/`CTRL3`
  tests; when executed, its accepted static write and loop extents are 24 VDP register writes, 38 Z80
  bootstrap bytes, 65,536 RAM bytes, 128 CRAM bytes, 80 VSRAM bytes, and four PSG writes; both setup
  routes reach the source's DMA-busy wait and then `InitializeSystem`; system initialization preserves
  the ordered `InitializeVdp`, `InitializeZ80`, `InitializeVdpData`, `InitializeGame` handoffs and 19
  maintained VDP entries; game initialization preserves the ordered `LoadBaseTiles`, `CheckRegion`,
  `NewGame`, `DisplaySegaLogo` handoffs; a nonzero logo result bypasses the intro; `GameIntro` stores
  its continuation pointer and clears it on the ordinary helper-return path before the title handoff;
  the title result separates a
  nonzero Witch handoff from the zero-result `InitialStack`/`p_Start` reset route; `CheckRegion` masks
  `HW_Info` with `0xC0`, accepts `0x80`, and otherwise reaches its source-local infinite loop.
- **Inferred**: none. Hardware lifecycle, player intent, and visible startup meaning are not inferred
  from register tests, source comments, branch names, or helper identities.
- **Unknown**: whether the `CTRL1`/`CTRL3` values reliably distinguish cold boot, soft reset, or any
  other platform lifecycle; reset and TMSS variants; Z80 bus and VDP/DMA cadence; the state produced
  by the static write loops on different hardware; exact controller samples that produce logo or
  title results; debug-route reachability; real-hardware region values and compatibility; rejected
  region rendering; intro, title, Witch, logo, fade, audio, and input timing; persistence; malformed
  or injected state; and player-visible presentation.

The contract describes source-shaped control and metadata, not a requirement that a modern engine
emulate Mega Drive startup hardware. An original-fidelity adapter may expose that hardware-facing
surface; a portable remake may replace it with validated platform services while retaining the
accepted route and handoff facts.

## Evidence Owner and Source Audit

`sf2-gameflow-core-static-v1`
([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`gameflow.py`](../../../src/sf2tool/h2/gameflow.py), and its source-backed explanation is
[Startup, Main Loop, and Exploration Core](../../research/gameflow-core.md). This contract consumes
selected `expected.startupFacts` and the following five record identities:

- `gameflow.start.cold-start`;
- `gameflow.start.system-init`;
- `gameflow.start.game-init`;
- `gameflow.start.intro`;
- `gameflow.start.region`.

The fixture's complete source-membership set contains 14 records. Thirteen records have direct
`sf2-gameflow-core-static-v1` evidence. The exact contract partition is:

- the five startup records selected above;
- two directly fixture-linked startup records that retain their existing contracts;
- six main-loop and exploration records already owned by
  [exploration-control-flow](exploration-control-flow.md);
- one membership-only map-block record whose H3 evidence and
  [map-exploration](map-exploration.md) association remain unchanged.

`gameflow.start.base-tiles` remains with
[UI graphics asset data](ui-graphics-asset-data.md), while `gameflow.start.z80-init` remains with the
[audio system](audio-system.md). This contract names `LoadBaseTiles` and `InitializeZ80` only as
ordered handoff identities. It does not add this contract to either research record.

The accepted source audit checked the pinned upstream commit
`c834c652b6862bc5679fd7f69a38a7093206efc6` at these representative entries:

| Selected record | Representative entry | ROM address |
| --- | --- | ---: |
| `gameflow.start.cold-start` | `Start` | `0x2DE` / 734 |
| `gameflow.start.system-init` | `InitializeSystem` | `0x200` / 512 |
| `gameflow.start.game-init` | `InitializeGame` | `0x70D2` / 28,882 |
| `gameflow.start.intro` | `j_GameIntro` | `0x71C0` / 29,120 |
| `gameflow.start.region` | `CheckRegion` | `0x7EC6` / 32,454 |

The audit reviewed `gamestart.asm`, `systeminit.asm`, `gameinit.asm`, `gameintro.asm`, and
`regioncheck.asm`. Source review confirms the bounded branch and call chronology below. It does not
promote comments or helper names into runtime, hardware, or presentation conclusions.

The map-data aggregate and every `map.data.*` table record are outside this contract's consumed
evidence. A correction to that aggregate does not automatically expand this contract.

## Source-Shaped Initial Setup

**Confirmed static:** `Start` first tests a longword at `CTRL1`. If that test is zero, it tests a word
at `CTRL3`. The initial setup block executes only through the source path on which both tests leave a
zero result; a nonzero result from either tested value reaches `@SkipSetup`.

This is a branch fact, not a platform detector contract. The two addresses are retained as source
identities. Their values are not redefined as a reliable cold-boot, reset, console-model, or hardware
health signal.

When the initial setup block executes, the accepted fixture and source loops establish these exact
static extents:

| Setup operation | Accepted extent | Evidence meaning |
| --- | ---: | --- |
| initial VDP register writes | 24 | source loop iterations |
| Z80 bootstrap copy | 38 bytes | source byte-copy iterations |
| 68000 RAM clear | 65,536 bytes | source longword-loop extent expressed as bytes |
| CRAM clear | 128 bytes | source longword-loop extent expressed as bytes |
| VSRAM clear | 80 bytes | source longword-loop extent expressed as bytes |
| PSG writes | 4 | source byte-write iterations |

These are not elapsed cycles, guaranteed device completion, audio silence duration, visible blanking,
or proof of identical effects on every console or emulator. The public contract retains counts and
operation identities, not the original bootstrap bytes or other private payloads.

After the conditionally executed block, both paths reach the shared continuation. The source reads
the VDP control port, repeats while the accepted DMA-busy bit test remains nonzero, and then branches
to `InitializeSystem`. The branch order is Confirmed static; polling cadence, hardware completion,
and failure behavior remain **Unknown**.

## System Initialization Handoffs

**Confirmed static:** `InitializeSystem` preserves this exact source order:

1. call `InitializeVdp`;
2. call `InitializeZ80`;
3. call `InitializeVdpData`;
4. tail-jump to `InitializeGame`.

The `InitializeVdp` source loop consumes 19 maintained VDP initialization entries. This is a table
and loop cardinality, not a portable display specification or a claim about visible state.

`InitializeZ80` is only a handoff identity in this contract. The generated sound-driver payload,
copy length interpretation, bus/reset protocol, live Z80 state, first command, audible result, and
failure behavior remain with [audio-system](audio-system.md) or **Unknown**.

`InitializeVdpData` is likewise a handoff identity. Queue layout, scroll buffers, palettes, sprite
tables, DMA processing, controller port configuration, completion, and rendering are outside this
contract. A modern implementation may replace the three platform helpers with one validated service,
provided its compatibility trace can reproduce the accepted ordered handoffs when original-route
parity is requested.

## Game Initialization Handoffs

**Confirmed static:** `InitializeGame` preserves this exact top-level call order:

1. `LoadBaseTiles`;
2. `CheckRegion`;
3. `NewGame`;
4. `DisplaySegaLogo`.

This contract does not consume `baseTileCount` or `baseTileCompressionMode`. The source operand
`4096`, compressed resource identity, decoder behavior, transfer form, destination, and rendered
result remain with [UI graphics asset data](ui-graphics-asset-data.md) and its dedicated evidence.
`LoadBaseTiles` is only the ordered handoff named above.

The [new-game state initialization contract](new-game-state-initialization.md) owns the accepted
`NewGame` mutations and their internal order. This contract owns only the fact that the handoff occurs
after the region helper returns and before the logo handoff. It does not define save creation,
campaign state, persistence, or player-visible new-game behavior.

After `DisplaySegaLogo` returns, the accepted source branch sends a nonzero result directly to
`AfterGameIntro`. A zero result continues to the debug-toggle and intro-routing source. The
[special-screen control-flow contract](special-screen-control-flow.md) owns logo internals and its
bounded input-sequence surface. This contract does not claim how a Start press or any exact input
sample creates the result, nor that the branch is visible on a particular frame.

The remaining debug-mode branches are not promoted into a complete debug contract. Their natural
reachability, input timing, map/battle test semantics, and downstream state are **Unknown** or belong
to separate owners.

## Intro and Title Return Routing

**Confirmed static:** `j_GameIntro` branches to the `GameIntro` chunk. That chunk:

1. stores the current stack pointer in the source backup location;
2. stores `AfterGameIntro` in the intro continuation-pointer location;
3. hands off to the intro/end-cutscene helper;
4. clears the continuation pointer after the helper returns through the ordinary source path;
5. later reaches the `StartTitleScreen` handoff.

The accepted chronology does not prove how the cutscene helper uses the pointer on every route, when
the pointer becomes visible to interrupts, whether an alternate jump bypasses the clear, or what the
player sees. Those remain separate runtime questions.

After `StartTitleScreen` returns, the source distinguishes two routes:

- a nonzero result branches to `StartWitchScreen`;
- a zero result raises the source interrupt mask, reloads the stack from `InitialStack`, loads the
  target through `p_Start`, and jumps to it.

The second path is preserved as the source-shaped start-vector reset route, not proof of a hardware
reset or complete platform reinitialization. Title loops, input polling, Witch admission, save actions,
fades, music, and visible transition behavior remain with
[special-screen-control-flow](special-screen-control-flow.md), other dedicated contracts, or
**Unknown**.

## Region Admission Branch

**Confirmed static:** `CheckRegion` reads `HW_Info`, applies `0xC0`, compares the masked byte with
`0x80`, and returns on equality. The non-equal branch reaches the source's warning work and then its
local infinite loop.

The accepted contract is only the mask, accepted comparison value, return-versus-local-loop split,
and representative helper identity. It does not define:

- the set of values produced by all real or emulated hardware;
- a general region taxonomy;
- compatibility with other releases or console revisions;
- the warning text, font, layout, colors, transfer completion, or rendered result;
- recovery, timeout, accessibility, localization, or user-facing error policy.

A modern engine may replace the original gate with an explicit platform/content compatibility check.
Any deliberate change must be documented as modernization rather than evidence about original
hardware behavior.

## Cross-System Separation

The startup route is an orchestrator. A named handoff does not transfer ownership of the callee:

- [UI graphics asset data](ui-graphics-asset-data.md) owns base-tile resource and transfer metadata;
- [audio-system](audio-system.md) owns the sound-driver and live audio boundary;
- [new-game state initialization](new-game-state-initialization.md) owns accepted `NewGame` state
  facts;
- [special-screen control flow](special-screen-control-flow.md) owns logo, title, and Witch internals;
- [input-system](input-system.md) owns accepted controller sampling and wait-helper behavior;
- [exploration-control-flow](exploration-control-flow.md) owns the later main-loop and exploration
  surface.

Map-data tables, map loading, working-layout mutation, battle outcomes, save persistence, dialogue,
menus, presentation assets, and story meaning remain outside this contract. No adjacent research
record gains this contract by implication.

## Implementation-Neutral Control Model

The minimum logical model is a control-and-provenance projection, not a hardware emulator:

```text
StartupControlPlan
  sourceIdentity
  sourceCommit
  selectedEntries:
    Start
    InitializeSystem
    InitializeGame
    j_GameIntro
    CheckRegion

  initialSetupAdmission:
    firstTest: CTRL1 longword
    secondTestWhenFirstZero: CTRL3 word
    executeSetupWhenBothZero: true
    otherSourcePathsSkipSetup: true

  initialSetupExtents:
    vdpRegisterWrites: 24
    z80BootstrapBytes: 38
    ramClearBytes: 65536
    cramClearBytes: 128
    vsramClearBytes: 80
    psgWrites: 4

  commonContinuation:
    pollVdpDmaBusyBit
    handoff: InitializeSystem

  systemHandoffs:
    - InitializeVdp
    - InitializeZ80
    - InitializeVdpData
    - InitializeGame
    maintainedVdpEntryCount: 19

  gameHandoffs:
    - LoadBaseTiles
    - CheckRegion
    - NewGame
    - DisplaySegaLogo

  logoReturn:
    nonzero: AfterGameIntro
    zero: debugAndIntroRouting

  introRoute:
    continuationTarget: AfterGameIntro
    storeBeforeIntroHandoff: true
    clearOnOrdinaryReturn: true
    titleHandoff: StartTitleScreen

  titleReturn:
    nonzero: StartWitchScreen
    zero:
      reloadStackFrom: InitialStack
      jumpTargetFrom: p_Start

  regionGate:
    sourceByte: HW_Info
    mask: 0xC0
    acceptedValue: 0x80
    equalRoute: return
    otherRoute: sourceLocalInfiniteLoop
```

This model preserves identities, ordered handoffs, branch predicates, and loop extents. It does not
store original code, bootstrap bytes, region-warning content, graphics, audio, or other copyrighted
payloads. Public fixtures and reports retain only metadata and synthetic test state.

The model deliberately does not include a base-tile count, compression mode, sound-driver length,
input frame, hardware clock, rendered screen, save state, or campaign state. Those facts either have
another owner or remain **Unknown**.

## Original Fidelity and Modernization

Original-fidelity mode preserves the source-shaped optional setup branch, accepted static extents,
common DMA-wait handoff, system and game call order, nonzero logo bypass, intro continuation-pointer
chronology, title result split, and region mask/comparison/terminal split. Hardware timing and visible
results remain explicit separate tests rather than silent assumptions.

A modern engine may initialize graphics and audio through host APIs, omit hardware bootstrap work,
replace the region gate, use typed scene results, and route title choices through a state machine. Such
changes are allowed design choices. A compatibility adapter must still be able to emit or validate the
accepted source-facing route facts, and intentional deviations must be recorded separately.

## H4 Acceptance Gates

A future remake startup-control adapter passes this contract only when:

1. it preserves the source-shaped `CTRL1`/`CTRL3` conditional setup admission without presenting the
   two values as a universal cold-boot or reset detector;
2. original-fidelity metadata retains the exact 24, 38, 65,536, 128, 80, and four static write/loop
   extents without converting them into duration, completion, or visible-result claims;
3. both initial routes reach the common DMA-busy wait and `InitializeSystem` handoff, while hardware
   cadence and failure behavior remain separately tested or **Unknown**;
4. the system handoff order and 19 maintained VDP entries remain reproducible, with `InitializeZ80`
   retained only as an ordered identity under this contract;
5. the game handoff order remains reproducible, while base-tile data/transfer and `NewGame` mutations
   stay with their dedicated owners;
6. a nonzero logo result bypasses the intro without assigning controller sampling, logo internals, or
   visible timing to this contract;
7. the intro continuation-pointer store/ordinary-return clear, title handoff, nonzero Witch route,
   and zero-result `InitialStack`/`p_Start` route remain distinct and ordered;
8. the region mask `0xC0`, accepted value `0x80`, equal return, and non-equal local-loop route remain
   reproducible without generalizing hardware compatibility or warning presentation;
9. the exact association boundary remains the five selected startup records; base-tiles, Z80 init,
   six exploration records, the map-block membership record, map-data, and all other sibling records
   remain semantically unchanged;
10. public artifacts contain only structural metadata and synthetic state, never ROM, bootstrap,
    graphics, audio, text, trace, emulator-state, or save payloads.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| conditional initial setup and exact write/loop extents | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Hardware lifecycle meaning, cadence, completion, visible state |
| common DMA-busy wait and system handoff | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Hardware timing, lockup and failure behavior |
| system order and 19 maintained VDP entries | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Driver data, queues, bus/reset, display/audio results |
| game initialization handoff order and logo-return bypass | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Asset payload, NewGame mutations, input generation, debug routes |
| intro continuation pointer and title result routes | **Confirmed static plus pinned source chronology** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Title/Witch internals, reachability, reset hardware effect, presentation |
| region mask, accepted value, return/local-loop split | **Confirmed static** | `sf2-gameflow-core-static-v1` ([`gameflow-core-static-v1.json`](../../../tests/fixtures/h2/gameflow-core-static-v1.json)) | Real hardware domains, compatibility, warning output, recovery |
| base-tile and Z80 startup records | **Separate-owner Confirmed static** | [UI graphics asset data](ui-graphics-asset-data.md) and [audio-system](audio-system.md) | Handoff identities only here; no new association |
| runtime, hardware, persistence, input, debug, asset, audio and presentation meaning | **Separate owner / Unknown** | Adjacent contracts and future runtime work | Do not infer a complete boot experience from static control |

## Reproduction

```powershell
uv run sf2 h2 gameflow-core
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated JSON remains under ignored `local/derived/gameflow-core-static.json`.
