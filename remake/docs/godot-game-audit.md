# Godot Game Host Audit

Status: G1/G2/G5 are resolved at the [ordinary/reference host boundary](./architecture.md#ordinary-and-reference-godot-hosts).
The findings below describe the inspected Git object. G3/G4 remain with their actual consumers. G6 is addressed by the common spell/level selection below.

This is a static review of accepted commit `7a95585282a7bdbdccc929ad9874d10e6ca60f92`, tree
`0abc464b6c8a325333428e3ebd857abc85cbdd34`. Its game and engine sources are identical to #413,
`1244b8bd856f1ce5e32599bd287bff46f2f32fff`. Active, unmerged source-AI work is outside this review.
No new Godot process, screenshot, engine suite, or ROM observation was used. **Confirmed** below
means the named tracked source directly establishes the structural or control-flow claim; native
symptoms were not independently reproduced in this audit.

The [earlier engine audit](./architecture-audit.md), [current architecture](./architecture.md),
[reference inventory](../reference/README.md), and
[ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md) remain the owning
decisions. This review examines their Godot consumer boundary. It does not adopt a framework or
authorize interruption of the current gameplay slice.

## Assessment

The common battle view has the intended inward dependency and state flow. Most of the architectural
debt is in the surrounding legacy host, which still shares the executable, entry scene, and source
directory. Engine isolation therefore does not yet establish isolation of the complete game host.
The two current common-entry defects below are separate from that known migration debt.

## Findings

### G1 — Default entry changes when unrelated user arguments are present

**Confirmed; current common startup, correction priority.**
[Map3Root](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7a95585282a7bdbdccc929ad9874d10e6ca60f92/remake/game/src/Map3Root.cs), lines 38–84, selects the default authored battle only when the
user-argument array is empty, or the exact `--authored-package` option exists. Every other argument
list without the exact private-start option reaches the legacy parser. That
[parser](../reference/game/src/Map3RuntimeProfileSelection.cs), lines 238–242 and 290–298, ignores unknown
options and defaults to PublicSynthetic when no recognized private/profile option exists.

For example, user arguments `--authored-pakcage example.json` select the legacy synthetic route;
the misspelled package option does not produce a content/startup error. Likewise, supplying only
the observation script's `--observation-output` option changes the host selected by an otherwise
default launch. This is a source-level reproduction, not a claimed native run. Duplicate
`--authored-package` arguments also silently use the first occurrence, while private-start duplicates
are rejected at lines 49–52.

Use one small startup selection owner with an explicit default, consistent duplicate handling, and
explicit legacy selection, retaining the documented legacy smoke switch where needed. Give diagnostic
arguments a deliberate owner; unknown arguments must not choose a different game. No generic command
framework is required.

### G2 — The scene and game assembly still couple common play to the reference host

**Confirmed; existing migration debt with a build and entry boundary still to close.**
[Main.tscn](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7a95585282a7bdbdccc929ad9874d10e6ca60f92/remake/game/Main.tscn), lines 3–6, always instantiates Map3Root.
[The Godot project](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7a95585282a7bdbdccc929ad9874d10e6ca60f92/remake/game/Sf2.Remake.Godot.csproj), lines 8–14, directly references both production
projects and `Sf2.Remake.Reference`. The current architecture explicitly acknowledges that reference
consumer, so this is not a new dependency secretly introduced by #413. It does mean that the common
game executable still requires the reference implementation to build.

The root file is 1,014 lines and [PrivateMap3Composition](../reference/game/src/PrivateMap3Composition.cs) is
another 1,149-line part of the same root class. The root also has a partial declaration in
[PrivateBattle01Composition](../reference/game/src/PrivateBattle01Composition.cs). The root source directory
contains 17 C# files; only the three common battle files have a responsibility directory. These are
descriptive counts at the inspected object, not acceptance targets.

Create a neutral game composition entry and move the still-needed reference consumer to an explicit
reference host boundary. Reuse the existing Godot installation and worktree. The eventual normal game
build should have no Reference or verification dependency. A folder rename or another giant partial
does not establish that boundary; direct project references and actual callers do. Retire each old
consumer as its capability migrates, following the existing inventory rather than preserving a second
engine indefinitely.

### G3 — The legacy Godot adapter still owns gameplay dispatch and comparison policy

**Confirmed; legacy private route, already identified broadly by the engine audit.**
[PrivateBattle01Composition](../reference/game/src/PrivateBattle01Composition.cs), lines 17–55, selects named
Sarah/Chester/leader comparison presets and calls preparation, initialization, round creation, and
first-control entry itself. Its `DispatchNext`, lines 199–280, loops over the turn buffer, creates a
round, chooses enemy behavior and pursuit/attack/standby operations, and advances to player control.
Lines 158–160 even select action-help text using the Sarah comparison identity, actor index, and
round 13. These are concrete reasons the class is not merely an input adapter despite its comment.

Do not carry this dispatcher or its history conditions into the common view. Keep actual turn and
action advancement behind the common Application session; put controlled starts and expected histories
in external reference verification. Preserve real map/story/return behavior until its named common
implementation replaces it. This review does not ask the current common source-AI slice to restore
or rewrite legacy behavior just to keep every old comparison running.

### G4 — Presentation code combines asset provenance, fixed case admission, and rendering

**Confirmed; legacy presentation/content boundary.**
[PrivateLocalPresentationAssetCatalog](../reference/game/src/PrivateLocalPresentationAssetCatalog.cs), lines
85–151, hardcodes map-specific asset identities, an asset repository commit, manifest digest, and
individual payload digests. Mount methods at lines 219–435 bind those particular assets;
`MountRaster`, lines 590–710, also checks package/receipt identity and delegates payload admission
back to the Content reader. The file additionally contains a HUD Control implementation.

[PrivateOriginalMapBaseViewport](../reference/game/src/PrivateOriginalMapBaseViewport.cs) is 2,032 lines spanning
pixel decoding/raster generation, nearest replication and Scale2x processing, texture binding, live
actor glyph projection, and a Node2D view. Lines 925–1028 embed the Entity142 diagnostic's exact entity
identity, coordinates, sprite, opaque bytes, and accepted-population checks alongside presentation.
This ties rendering to one accepted reference case and turns resource-pack changes into code changes.

Separate the existing responsibilities: content owns selected resource definitions and trust admission;
presentation owns texture/node projection and animation; external reference verification owns exact
case predicates. Retain justified integrity checks and pinned provenance at their owner. This is not
a request to delete digest validation or genuine story/entity definitions. Existing offline asset
processing should be reused where it already supplies the needed raster; do not add a second pipeline.

### G5 — Old smoke scenarios are callable from the normal root and compiled with the game

**Confirmed; verification placement debt.**
[PublicSyntheticMap3SmokeDriver](../reference/game/src/PublicSyntheticMap3SmokeDriver.cs) and
[PrivateMap3SmokeDriver](../reference/game/src/PrivateMap3SmokeDriver.cs) contain 726 and 677 lines of scripted
commands, expected case results, and process/pass/fail behavior inside `game/src`.
Map3Root lines 512–522 and PrivateMap3Composition lines 1027–1045 invoke them. The Godot project has
no separate build boundary for these classes. The existing
[export preset](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/7a95585282a7bdbdccc929ad9874d10e6ca60f92/remake/game/export_presets.cfg), lines 9–11, also has no resource exclusion for probes;
actual exported contents were not inspected, so their packaging remains **Unknown** here.

Move still-useful scripted comparisons out of the production compile path and remove obsolete smoke
callers with their owning legacy route. Preserve necessary reference observations, without replacing
every retired test. New verification must consume normal input/commands and read-only state.

The newer [engine observation script](../game/probes/engine_battle_observation.gd) already separates
expected values from the view, drives real input at lines 22–33, and reads `ReadObservationJson` at
line 13. Its existence or size is not evidence that gameplay has leaked into the view. Keep this
approach where useful and keep its scripts/expected cases out of the eventual ordinary game package.

### G6 — Authored spell choices exist in content but cannot all be selected in the UI

**Addressed for the common input surface.** H cycles the current actor's ordered learned spell
references and levels; the HUD identifies the accepted selection and an unavailable attempt.
`SelectSpell`, `SelectTarget` and `Confirm` retain engine authority for legality, range, MP and effects.
Changing an accepted spell clears its previous target; rejected choices retain the previous snapshot.
The direct `spell-selection` native observation selects and casts a second spell at level2 without
reordering JSON. The private reader also exposes lower learned levels. EGRESS and wider unsupported
effects remain visible, attributed failures. This minimal interface does not claim a complete modern
battle UI or original presentation; semantic InputMap modernization remains later interface work.

## Boundaries already working

- The common view, presentation projection, and map viewport are 196, 37, and 110 lines respectively.
  The view submits session envelopes and projects published results; it does not mutate HP, positions,
  turn order, or RNG. `_targetCandidate` explicitly retains UI attempt feedback rather than a second
  gameplay selection authority.
- `BattleSessionView._Process` only submits `AdvanceSimulation` when Application reports
  `SimulationWait`. The Application advancer owns actual automatic progression; this is a valid host
  pump, unlike the legacy `DispatchNext` loop.
- The presentation projection is pure and the map nodes are disposable projections. A read-only node
  and session observation endpoint is appropriate diagnostic support; expected rounds, seeds and case
  assertions belong in the external probe.
- The common private entry selects Content inputs and uses the same session/view as authored play.
  This audit found no Map3/Sarah/Chester comparison history gate in those three common battle files.

## Remaining follow-up and acceptance

The ordinary host entry and explicit reference compile boundary now implement G1/G2/G5; current
startup/build commands and remaining caller ownership are linked above. The inspected source record
below those links does not establish completion of the remaining game migration.
Carry G3 with existing battle migration and G4 with actual content/map/presentation migration; remove
each old caller at that boundary. The bounded G6 spell/level selection is implemented; full modern
battle UI and presentation remain later work.

Each implementation needs an explicit caller/dependency boundary and the affected actual-input/state
observation. Startup changes should observe default, explicit common/private, duplicate/conflicting,
and unknown-option behavior. Spell selection should demonstrate a second admitted spell without
editing JSON order. Content/presentation migration should demonstrate a different admitted map/entity
or resource binding without another scenario-specific class. Use appropriate existing engine behavior
unit tests only when engine behavior changes; do not add architecture, probe, or planner test suites.

Follow the existing [verification owner](./development-and-verification.md): reuse sufficient completed
observations and the same Godot installation/project/instance. A necessary startup check or code/import
restart can justify a bounded restart. No screenshots, speculative debug server, environment copy, or
full legacy-suite replay is required by this audit. This documentation change itself uses direct
document/scope checks, the committed planner, and actual CI results.
