# Documentation Index

> See [`README.zh-CN.md`](./README.zh-CN.md) for the Chinese reading copy. The English version is the
> canonical index.

Documentation is layered as "evidence → contract → implementation choice" to keep original-game
facts, inferences, and remake preferences separate.

## Continuity Without External Memory

The repository is the durable project record. A fresh contributor or agent should be able to resume
without a previous chat transcript or external memory store:

1. read the root [`README.md`](../README.md) for scope, baseline, current phase, aggregate evidence,
   and the active frontier;
2. read [`research/source-coverage.md`](./research/source-coverage.md) for the exact coverage
   denominators, verification cadence, and current subsystem direction;
3. use this index to open the closest research, design, or decision owner;
4. inspect `git status` and recent commits before assuming the active slice is complete or the
   worktree is clean;
5. reproduce counters from tracked manifests and commands instead of copying a stale progress note.

Topic documents own detailed findings and unknowns, decision records own durable tool/architecture
choices, and Git history owns completed-slice chronology. External agent memory is neither required
nor authoritative, and it is disabled for routine work on this repository. Do not read or update a
personal/global memory store to resume the project. If the user explicitly requests a one-time
migration audit, move only still-valid, project-specific facts into the appropriate tracked owner and
then stop synchronizing against the external store. When a session discovers a durable fact or
changes the frontier, update the owning repository document in the same slice.

The one-time migration audit on 2026-07-19 found no `md-sf2` project entry in the disabled external
memory index. The durable chat-era decisions were already owned here: autonomous Phase 2 slice and
commit cadence in `AGENTS.md`, Python/uv and focused commit verification in ADR 0002, static-first
batched simulation in ADR 0003, and reproducible coverage counters/frontiers in
`research/source-coverage.md`. The remaining implementation-only Lua syntax-preflight rule was
migrated into ADR 0001 during this audit. No continuing memory synchronization is required.

## Research

`research/` stores reproducible reverse-engineering findings. Each entry must carry Confirmed,
Inferred, or Unknown labels, and provide a ROM hash, an upstream commit, an address/symbol or runtime
observation, and reproduction commands.

- [`reproducible-original.md`](./research/reproducible-original.md): ROM H0, the pinned toolchain, and
  the bit-perfect H1 baseline.
- [`static-core-data.md`](./research/static-core-data.md): character slots, classes, items, and spells:
  ROM ranges, ROM byte packing, dual-path parity, and semantics awaiting verification.
- [`ally-growth.md`](./research/ally-growth.md): growth curves, class-growth projections, cross-ally
  class-block scanning, spell-learning and inheritance control codes, and post-level-up current/derived
  stat refresh.
- [`runtime-rng-and-battle-math.md`](./research/runtime-rng-and-battle-math.md): base/debug-override
  RNG, growth math/complete level-up, projection/level-cap/spell-inheritance boundaries, and Battle 01
  turn order, AGI 127/128 boundary, region activation, and physical damage from terrain/archer bonuses
  through dodge, critical, spread, double/counter, and death/distance/status/side/special-enemy
  follow-up validation with the complete double validator; HP/EXP construction, kill level
  differences, final EXP halving/randomization/minimum, EXP 200 saturation/single-threshold command,
  gold 9,999,999/carry saturation, enemy-item rare/guaranteed/duplicate flags, persistent replay, 99
  EXP natural level-up, the BLAZE 2 four-tier FIRE resistance matrix, DAO/APOLLO/NEPTUN/ATLAS
  four-index target-count division, attack-spell EXP, HEAL 1, the SLEEP/SLOW 1 four-tier STATUS
  resistance matrices, the DESOUL four-tier success and multi-target kill reward, the SPOIT
  target/caster MP boundary matrix, BOOST 1 first/cast-again behavior, the DISPEL/SILENCE consumption
  chain, and the post-turn status expiry/continuation H3 dynamic fixtures.
- [`enemy-promotions.md`](./research/enemy-promotions.md): the five-section promotion mapping, 103 enemy
  names, 56-byte enemy definitions, and the 30-enemy-drop/103-enemy-gold-word source/ROM contracts,
  plus drop termination/flag/RNG special cases, the 69-word unused tail after the gold table, and the
  static church/enemy-initialization consumers.
- [`battle01-placement.md`](./research/battle01-placement.md): the first story battle's map link,
  Stack-compressed terrain, background/EXP/win-loss global rules, nine entity records, three region
  polygons, and primary/secondary AI activation semantics.
- [`indexing.md`](./research/indexing.md): the machine-readable symbol → ROM/RAM address → fixture →
  document/design-contract index, its validation rules, and the landing flow for new findings.
- [`source-coverage.md`](./research/source-coverage.md): the current coverage denominators, 98.45%
  code-file reach, 100% data-ASM H2 inventory, the 60.18% domain-aware data-file reach boundaries, and
  the static-first, batch-run H3 subsystem cadence.
- [`battle-ai.md`](./research/battle-ai.md): the complete battle-AI source inventory, action filter,
  attack, healing, support, final action/target selection, terrain/swarm/special-attacker control, and
  the first grouped 14-case H3 launch plus the follow-up question matrix.
- [`battlefield-pathfinding.md`](./research/battlefield-pathfinding.md): the complete static
  inventory of the 17 battlefield/pathfinding files, movement/range/target/move-string contracts, and
  the single-launch five-case propagation and boundary H3 matrix.
- [`battle-loop.md`](./research/battle-loop.md): the complete static inventory of the 18 battle-loop
  files and nine top-level control files, plus main-loop, win/loss, roster, terrain, spawn, death
  cleanup, and post-battle recovery contracts.
- [`battle-actions.md`](./research/battle-actions.md): the complete static inventory of the 29
  battle-action files, the action pipeline, physical-branch order, item breakage, the Taros special
  case, and target-sorting contracts.
- [`battle-functions.md`](./research/battle-functions.md): the seven shared battle-function files
  inventory, individual-turn control, Kiwi flame breath, EGRESS/Angel Wing, battle load, move SFX, and
  the static state-machine contracts for six player-control/cursor/menu entry points plus equip,
  give/drop, and chest outcomes.
- [`battle-scene-engine.md`](./research/battle-scene-engine.md): the twelve battle-scene root-engine
  files and 55 animation-implementation files, 21 scene-script commands, initialization/selectors,
  32×2 setup/update pairs, and the complete 87-ally/121-enemy battle-sprite sequence and 421-frame-entry
  contracts.
- [`battle-cutscenes.md`](./research/battle-cutscenes.md): the ten-file routing for pre-battle/
  opening/post-battle/enemy-defeat/region cutscenes, flag admission, leader-death position prep, and
  map-script scheduling.
- [`common-scripting.md`](./research/common-scripting.md): the 29-file entity/map/text/credits
  inventory, the 90/80-slot interpreters, the complete 255-entry/86-tree/1,536-leaf context-Huffman
  corpus, the 17-bank/4,267-string/152,679-symbol static decode, the 80-glyph variable-width
  font/256-entry ASCII map data path, the six map-script dialogue commands' 2,883 ordered
  program-reference/handler/text-line/sprite-dialogue consumer contracts, the five map-script
  transition commands' 146 ordered program-site/handler/caller contracts, the six map-script
  roster/death commands' 43 ordered program-site/handler/caller contracts, the two map-script
  block-copy commands' 208 ordered program-site/handler/cursor/helper/caller contracts, the four
  map-script entity population/reload commands' 96 ordered program-site/handler/caller contracts, the
  single source-named `cloneEntity` command's nine ordered program-site/handler/caller contracts, the
  seven map-script control/audio forms' 2,336 ordered source-site/static-control contracts with a
  single-launch six-case wait/skip/no-op/sound/subroutine/jump/end H3 boundary, the three map-script
  camera-control commands' 415 ordered program-site/handler/caller/service contracts, the four
  map-script map-lifecycle commands' 108 ordered program-site/handler/caller contracts plus a
  single-launch five-case handler-return/direct-H1-JSR-site-order/map-camera-word/two-marker H3
  contract, the three map-script camera-control commands' single-launch seven-case
  target-branch/destination-word-transfer/speed/wait H3 contract, the four map-script entity-placement
  commands' single-launch seven-case alive/dead-cursor, record-word/facing, 31-flash-loop/shared-tail,
  and destination-wait/bypass H3 contract, the two source-named map-script trigger commands' eight
  ordered program-site/handler/caller/table-boundary contracts, the four source-named map-script
  entity-placement commands' 2,288 ordered program-site/handler/caller contracts, the six map-script
  to-entity-action bridge commands' 3,256 ordered program-site/payload/handler/caller contracts, the
  eight source-named entity lifecycle/presentation commands' 464 ordered
  program-site/handler/caller contracts, the seven source-named entity gesture/relationship/motion
  commands' 545 ordered program-site/handler/caller contracts, the twelve source-named
  screen/map-presentation commands' 459 ordered program-site/handler/caller contracts, and the complete
  entity-action static chain plus the single-launch 13-case/20-tick entity movement H3.
- [`common-maps.md`](./research/common-maps.md): the seven shared map-engine files, map switch, battle
  trigger, egress/savepoint, 8 KiB layout-decompression boundary, and VInt gates.
- [`common-stats.md`](./research/common-stats.md): the 20 shared stats files, flags/party/inventory,
  field-item dispatch, spell learning, new-game order, getter/mutation/clamp static contracts, and the
  un-included alternate-source boundary.
- [`common-menus.md`](./research/common-menus.md): the 42 shared menu files, prompt/text control,
  field-item dispatch, the complete static shop/church/caravan/blacksmith service state machines,
  diamond/yes-no compressed graphics, the complete icon storage/copy/highlight contracts, the 27 leaf
  UI layouts/2,394 VDP words, the spell-level pointer and diamond-border contracts, and alternate
  boundaries.
- [`technical-interrupts.md`](./research/technical-interrupts.md): VInt, DMA, fade, input repeat,
  wait/sleep handshake, trap routing, and hardware timing awaiting batched verification.
- [`technical-graphics.md`](./research/technical-graphics.md): decompression, display initialization,
  sprite/palette, parallax, the complete battle terrain/background/sprite/weapon/ground/portrait/
  special-sprite Stack corpora, flash scripts and rendering-parity boundaries, plus the 720-slot
  regular map-sprite Basic-compression corpus, nine special-screen and eight base/menu UI Stack streams
  with the seven-icon uncompressed main menu, the 56 battle-effect and 115 map-tileset
  Stack-compression corpora, the 208 battle-sprite animation tables, the 163-slot icon
  storage/menu-copy/highlight contracts, the 19 vanilla-built UI-layout owners and the 5,614-byte static
  corpus, the 80-glyph variable-width font/ASCII map/loader and the complete context-Huffman contract,
  the witch-menu 16-color palette/12-frame bubble table, the 12-resource/8,832-byte special-screen
  palette/layout corpus, the four-stream/32,768-byte unused-cloud and two unused-base palettes, the 16
  map palettes/79-map usage with the effective color-zero contract, and the 46/46 direct named
  compression-consumer owner inventory.
- [`technical-interfaces.md`](./research/technical-interfaces.md): the complete static routing table
  of 331 jump stubs and 60 longword pointers.
- [`technical-services.md`](./research/technical-services.md): resource incbin, byte copy, input,
  SRAM, variable-width font, context-Huffman and witch-menu direct payloads, the 68000 sound bridge,
  RNG, and the standalone Z80 driver build chain; the RNG range-low-byte retry and controlled
  source-shaped-copy single-launch matrix; the cloud/base payload boundary with no symbolic consumer;
  and the executable 20/20 technical-incbin to 8-deep-H2-owner attribution audit.
- [`gameflow-core.md`](./research/gameflow-core.md): cold start, system initialization, the main loop,
  battle/exploration routing, map events, interaction, and item handoff.
- [`special-screens.md`](./research/special-screens.md): the 19-file special-screen boundaries across
  logo/title, witch save (the four-row New/Load/Delete/Copy dispatcher, page selector, 118
  source-use provenance records, SRAM action routing, and the single-launch nine-service/two-Load-branch
  and four-case New/core-replay runtime matrices), suspend/reset, and ending, plus all nine compressed
  tile streams, DMA transfer/tail, the choice palette and 4×3 bubble animation, and the seven-palette/
  five-layout contracts.
- [`remaining-core.md`](./research/remaining-core.md): the final main-code boundaries of the ROM
  header/vector, window engine, battle test, configuration, and debug actions.

Data-side directory inventory and ROM parity:

- [`battle-global-data.md`](./research/battle-global-data.md): the 18/18 H2 inventory of global battle
  data and 17 H1-bound canonical tables.
- [`ally-data-inventory.md`](./research/ally-data-inventory.md): the 42 directly or transitively
  included ally/class files, and the reuse relationship with the existing growth/spell-learning rails.
- [`core-stats-data-inventory.md`](./research/core-stats-data-inventory.md): the 19 items/spells/enemies
  source files with table dimensions, plus the 9-range deep source/H1/ROM contracts for shops/debug
  shop/chest gold/break messages/mithril/Caravan/field items/weapon graphics and the 166-row enemy
  map-sprite normal-vs-tail reachability contract.
- [`battle-cutscene-data.md`](./research/battle-cutscene-data.md): the 61 battle-cutscene data files,
  59 in-build scripts, and two explicit exceptions.
- [`battle-spriteset-data.md`](./research/battle-spriteset-data.md): the 46-file spriteset
  pointer/include graph, header ranges, and combatant-macro counts.
- [`battle-routing-data.md`](./research/battle-routing-data.md): cutscene slots, region routes, terrain
  aliases, the complete decode/ROM parity of the 43 Stack-compressed terrain payloads, unused joins,
  and old aggregate boundaries.
- [`map-data-inventory.md`](./research/map-data-inventory.md): the complete 1,390-file map ASM build
  graph, 727 internal H1 bindings, 662 include-site-only bodies, 64+66 setup-selection rows, ROM
  parity of the 126 six-pointer setup tables, 125 entity-list sources/980 physical records with suffix
  fallthrough, the complete 263 entity/zone/item-event sources/1,134 physical records, 915
  source/H1 target profiles, 684 entity-event, 150 zone-event, and 80 item-event target programs (plus
  one raw-expression exclusion), 3,579 non-comment operations (54 mnemonics, nine source-faithful
  families, 34 macro/engine definition joins, and four Map 21 action-payload contexts), 493 direct
  numeric flag source uses (316 read / 169 set / 8 clear, 151 operands, 316 immediate conditional
  consumers) with 469 instructions and effective target identities per category, 147 direct `script`
  source references (138 instruction labels, 135 effective map-script owners; complete zero-count
  target domains of 348/304), and 1,006 direct `TEXTBOX` source/H1 references (981 numeric
  line-references, 25 `$FFFF` sentinels; complete zero-count table of 914 callers and 4,267 declared
  text-line IDs, without decoding text), 378 pointer-table and 390 selector-route category joins, the
  map44 raw-target boundary, nine first-match selection cases, 75 description targets/227 physical
  entries, the `d6` condition on the normal call chain, 84 init sources/90 callable entries, and 47
  standalone scripts/8,058 statements; the ten-case selector and six-case init-dispatch single-launch
  H3 matrices additionally confirm missing/default, last-set-flag-wins, alias route, and
  active/scripted/direct-return init calls.
- [`map-content.md`](./research/map-content.md): the complete source/H1/ROM parity of 79 46-byte map
  entries, 662 source-form content sections, and 154 private blocks/layout payloads, the canonical
  Python decode of 77 bitstream pairs, the 1,859-resource/79-map engine-neutral import (including 64
  routes, 126 setups, 178 standalone, and 201 init-source programs), record/consumer rules, and the
  upstream `MAPDATA_OFFSET_LAYOUT` constant defect.
- [`auxiliary-data-inventory.md`](./research/auxiliary-data-inventory.md): the 65-file
  graphics/scripting/technical/sprite-dialogue boundary, 80 indexed records/79 distinct symbols
  across 63 source files, two alternates, the 56-slot/52-payload portrait header, animation metadata,
  palette, and Stack-graphics decode contracts, the 30-slot/27-payload battle-background dual-tileset
  decode contract, 86 battle-sprite containers/408 graphic frames, 208 animation sequences/421 frame
  entries, weapon/ground graphics and palettes, 670 regular map-sprite payloads,
  six special-sprite streams, nine
  special-screen tile streams, eight base/menu UI streams, 56 spell/invocation/status/transition
  streams, 115 map-tileset streams, 163 assembled icons with four source-only icon exceptions, 27 UI
  layouts/16-slot spell pointers/four borders/four direct tile payloads, and complete parity of 16 map
  palettes/79 header references; it also closes the 119-row map-sprite/portrait/speech-SFX property
  table with the `0xFFFF` sentinel, first-match, and fallback consumption rules at complete
  source/H1/ROM parity, the five sprite-write sites, 81 script assignments, 20 property-update callers,
  and the ally/enemy derivation-domain 237-250 exclusion audit, plus the three shared entity-action
  corpora (2,864 bytes, 118 labels, 732 commands, 38 relative branches, 61-entry external-reference
  graph) and continued closure of 75 distributed sources, 361 inline programs, 11 standalone ROM
  ranges, 1,472 commands, and 17 named entries, and the 80-slot dispatcher's 37 filler/43 handler
  slots, 40 macro-reachable opcodes, three handler-only branch opcodes, and the `$8080` inline
  terminator boundary; the handler catalog further classifies 11 read/17 write accesses over 18 entity
  fields, 10 read/5 write over 15 global states, parameter-read widths, eight handler families, 22
  entity bit accesses, 46 script-pointer actions, and the complete/low-byte/skip classification of 46
  macro parameters/86 bytes to handler reads; it closes `ac_branch`'s out-of-macro relative
  displacement, the three handler-only 6-byte layouts, the complete source-use boundary for 35/43
  handlers, and the full handler-flow outcome classification of 39 redispatch/11 yield/7 dual, with
  the 10 signed/20 unsigned/15 boolean/1 ignored interpretations of all 46 parameters and the seven
  dual predicates bound to source evidence; the follow-on `UpdateEntityData` closes 560 bytes, 190
  instructions, nine movement phases, 15 fields, five bit accesses, and the 16-byte facing table at
  ROM parity; the four update helpers close 434 bytes, 135 instructions, 22 callers, the
  destination-conflict CCR, sprite fallthrough, and the map-offset hash formula; the map-script engine
  closes the 90-slot dispatcher's 82 valid opcodes/8 fillers, 83 unique handlers, 82 primary
  macros/8 aliases/3 specials, and 13,515 invocations across 169 source files, 955 handler
  statements, 16 entity fields, 25 global states, and 62 direct-call targets; the ABI closes 133
  primary macro parameters/operand fields, 234 operand bytes, 2/4/6/8-byte width distributions, and the
  77 sequential/1 absolute/4 conditional/1 inline cursor-flow classification; the complete source
  further attributes 304 programs/348 labels, 303 `csc_end` terminations plus one jump termination,
  and resolves 42 same-program and 20 cross-program script jumps plus 122 68000 subroutine calls; the
  full 2,077 code/data-ASM reference graph further distinguishes 187 cross-file-referencable, 110
  same-file-only, and seven zero-reference programs, with 347/348 labels referenced. It further closes
  the 89-program story-state surface: 51 conditional reads over six flags, 53 direct writes, 22 prompt
  writes, and 20 battle-unlock writes over 56 flags, with the read/write domains intersecting only at
  71/76/89. The same fixture fixes the seven story-state branch/prompt forms (146 sites) with a
  304-row zero-count program corpus, the primary `csc10` and `setF`/`clearF` alias physical layouts,
  five handler guards, and the sole H3 `story-state/branch-prompt-persistence-matrix`. The same
  304-row zero-count corpus fixes four entity population/reload forms (96 sites), four
  cursor/VInt/call/constant handler guards, and the zero-inclusive caller map retaining the direct
  `j_InitializeMapEntities` identity versus the resolved `InitializeMapEntities`; the single-launch
  12-case H3 fixture now fixes handler-local callback/cursor/list/record results (including three
  selected `newEntity` index seeds, a direct table, identity-list-selected reload, and seven map-setup
  inputs); the remaining queue covers capacity beyond the observed high-water, normal-story/save/
  map-reload persistence, player-visible rendering/animation/VDP timing, and collision/pathfinding
  consumer effects. The same corpus fixes four map-lifecycle forms (108 sites), four named handlers'
  cursor/probe, VInt, branch, and call/fall-through guards, and a five-target zero-inclusive caller
  map; the H3 five-case/single-launch fixture fixes handler return, direct-H1-JSR-site order,
  post-handler map/camera words, and two nonasset markers, while the remaining queue covers
  layout/collision/pathfinding, entity reload/player placement, presentation/fade/hardware timing, and
  story reachability/persistence. The same corpus fixes two source-named trigger forms (8 sites), two
  named handlers' A6 word-read, `MAP_TILE_SIZE` use-sites, call/return guards, a two-target
  zero-inclusive caller map, and the independently parsed 94-step/114-roof table boundary; the H3
  six-case/single-launch fixture fixes Map 02 record-0 hit, terminator miss, busy/battle gate,
  direct-H1-JSR-site, D0/D1 words, hash/table boundary, post-handler words, and two markers, with the
  remaining queue covering full layout/collision/pathfinding, presentation/audio/timing/hardware, and
  persistence/story reachability. The same corpus fixes eight source-named entity
  lifecycle/presentation forms (464 sites), eight named handlers' cursor/branch/callback/return
  guards, and a nine-target zero-inclusive caller map; the H3 11-case/single-launch fixture fixes all
  eight handler entries, the live/zero-HP start/stop boundary, the controlled second `waitIdle`
  compare, both sprite-selector sides, the priority byte, the complete remove-shadow callback chain,
  and source-backed temporary/restored sprite-size words plus the flags-B record; the remaining queue
  covers normal-story reachability, full entity-state/callback effects, and player-visible
  presentation/timing/collision/persistence. The same corpus fixes seven source-named entity
  gesture/relationship/motion forms (545 sites), seven named handlers' A6 cursor,
  source-operand/literal, branch/loop/call/return guards, and a ten-target zero-inclusive caller map;
  its single-launch 17-case H3 covers seven handlers' controlled callback/state seams (including
  shiver's three temporary/restore cycles, the follow high-byte zero-HP boundary, the face/move word
  boundary, both fly sides, and the above-register record); the remaining queue covers normal-story
  reachability, full entity-state/callback effects, and player-visible
  presentation/timing/collision/persistence. The same corpus fixes twelve source-named
  screen/map-presentation forms (459 sites), twelve named handlers' A6 cursor, immediate/operand,
  branch/loop/call/return guards, and a five-target zero-inclusive caller map preserving seven
  PC-relative `LaunchFading` targets; its single-launch 22-case H3 fixes handler-local entry/return,
  cursor, direct-call/target/return, quake write, slow-counter, and flash-loop seams, while four
  Unknown queues remain for visible/palette/VDP/timing/service body/persistence/reachability. The
  same corpus fixes three source-named entity-presentation-FX forms (177 sites), three named handlers'
  A6 cursor, immediate/operand, branch/function-chunk/loop/call/return guards, and a nine-target
  zero-inclusive caller map; the single-launch ten-case H3 fixes handler-local
  entry/operand/branch/loop/callback/return records and two direct entity-byte-write seams, with four
  Unknown queues for normal-story reachability, player-visible output/timing/completion/repeat,
  bypassed-service/`WaitForVInt` effects, and persistence/map-entity interactions. The same corpus
  fixes three source-named UI primary forms (5 sites, including the zero-use `menu`), three named
  handlers' A6 cursor, immediate/operand, branch/stack/call/return guards, and a provenance-joined
  portrait-helper plus seven-target alias-aware caller map; the single-launch 11-case H3 verifies
  source-row input, busy/sentinel handler return, hide chronology, and the menu selector/A6/stack
  boundary, with four Unknown queues for normal story, complete window/VDP timing, real choice/service
  side effects, and persistence. The same corpus fixes the single source-named `cloneEntity` `$25` form
  (9 sites), the complete `csc25_cloneEntity` two A6 word reads/lookup, and the single-byte
  `ENTITYDEF_OFFSET_ENTNUM` transfer without promoting it to a whole-record copy/span; the
  single-launch nine-case H3 fixes handler entry/RTS, the A6 4/8 cursor boundary, two word/lookup PC
  chronologies, the offset-18 byte before/after, and adjacent-byte sentinels, with three Unknown
  matrices remaining for neutral state, external consumers, and context.
- [`sound-data-inventory.md`](./research/sound-data-inventory.md): the 41-file Z80 music include graph,
  canonical ROM parity of the two 32 KiB banks, 37 song range/address bindings, the 29-macro/
  39,290-invocation static command corpus, and the single-launch 4-command/12-checkpoint/120-channel-
  snapshot Z80 live-state H3 matrix; the same rail also closes the driver-embedded 56-entry SFX
  command/header domain and the complete 786-token/7-counted-loop static control flow of all 66 active
  streams.

## Design

`design/contracts/` contains evidence-bound implementation-neutral subsystem contracts, while
`design/synthesis/` contains cross-subsystem or player-facing explanations that consume accepted
evidence. Shared governance remains at the `design/` root. A design document cannot be used to
"prove" a reverse-engineering conclusion backwards.

zh-CN localization proceeds from the English canonical source in dedicated batches: the glossary
[`glossary.md`](./design/glossary.md) is the single binding source for English-to-Chinese terminology,
and mirrors live under `design/zh-CN/`, preserving each canonical English source's relative path (the
English file remains the review baseline).
The translation index is tracked in `manifests/zh-translation-index.json` and maintained with
`uv run sf2 zh-meta test` (strict verification) and `uv run sf2 zh-meta update` (regenerate while
preserving accepted anchors). After reviewing a changed mirror against its English source and the
current glossary, repeat `--reanchor-source docs/design/<category>/<file>.md` for each reviewed
document whose anchors may be updated.

- [`glossary.md`](./design/glossary.md): the accepted English-to-Chinese glossary and rules for zh-CN
  localization; fixed evidence-label translations, preserved source identifiers, one-term-one-
  translation, proper nouns kept in English, and `design/zh-CN/` mirror conventions.
- [`documentation-roadmap.md`](./design/documentation-roadmap.md): three-layer evidence/explanation/
  modernization boundaries, the English authoring baseline, near-term synthesis order, long-term
  directions, reusable authoring structure, and collaboration governance; it is neither evidence of
  original behavior nor a remake product decision.
- [`gameplay-overview.md`](./design/synthesis/gameplay-overview.md): synthesizes player verbs, top-level state
  flow, local loops, and subsystem handoffs from accepted gameflow, map, input, dialogue,
  party/roster, service, battle, growth, and save contracts while retaining campaign, experience,
  balance, and upper-layer design Unknown/decision boundaries.
- [`tactical-battle-loop.md`](./design/synthesis/tactical-battle-loop.md): synthesizes a bounded tactical battle
  loop from accepted battle control, player/AI control, movement/target, action construction,
  combat/spell resolution, state replay, and outcome evidence while retaining tactics, balance,
  presentation, and general-simulation Unknown/decision boundaries.
- [`progression-and-economy.md`](./design/synthesis/progression-and-economy.md): connects action-local EXP,
  persistent EXP and level-up, stat refresh, gold, enemy drops, item destinations, source-static
  service exchanges, and save boundaries after an adversarial owner/fixture audit, while retaining
  balance, campaign reachability, service-runtime, and end-to-end persistence Unknown boundaries.
- [`story-progression.md`](./design/synthesis/story-progression.md): maps the bounded top-level route, ordered
  setup/event selection, script graph, story-state, dialogue, roster, transition, and save handoffs
  after an adversarial owner/fixture audit, while retaining plot chronology, choice consequences,
  normal-save reachability, full persistence, and presentation as Unknown boundaries.
- [`map-design-principles.md`](./design/synthesis/map-design-principles.md): synthesizes map definitions,
  geometry/resource identity, ordered setup variants, interaction selection, and mutable working
  layouts into evidence-bounded structural principles after an adversarial owner/fixture audit,
  while retaining route quality, pacing, collision/pathfinding, reachability, visible presentation,
  and authorial intent as Unknown boundaries.
- [`map3-battle01-readiness.md`](./design/synthesis/map3-battle01-readiness.md): records the evidence,
  design-contract, and product-decision closure ledger for the ADR 0009 Map 3 through Battle 01
  milestone, preserving its explicit NOT READY state and the separate user action required before
  Phase 4 implementation may begin.
- [`ally-definition-data.md`](./design/contracts/ally-definition-data.md): the implementation-neutral
  contract for the 30 named ally identities, two independent 32-slot start/pointer domains, 32 class
  definitions, promotion tables, five growth curves, 59 ally/class growth records, spell-list
  inheritance, and presentation references, while retaining runtime roster state, level-up mutation,
  story availability, balance, and presentation fallback as separate-owner or Unknown boundaries.
- [`enemy-definition-data.md`](./design/contracts/enemy-definition-data.md): the implementation-neutral
  contract for 103 enemy identities and fixed spawn baselines, their ordered battle/map presentation
  references, the distinct 63-row NPC map-sprite tail, and the unchecked map-sprite lookup boundary,
  while retaining nonstandard tail reachability, runtime transforms, encounter design, AI, combat,
  rewards, presentation, and balance as separate-owner or Unknown boundaries.
- [`item-definition-data.md`](./design/contracts/item-definition-data.md): the implementation-neutral
  contract for 128 item identities and fixed definitions, shop/debug/chest/break/mithril/Caravan/
  field-use catalogs, weapon-graphics references, and bounded lookup rules, while retaining runtime
  transactions, effects, RNG, persistence, presentation, economy balance, and replacement content
  as separate-owner, Unknown, or deliberate-design boundaries.
- [`spell-definition-data.md`](./design/contracts/spell-definition-data.md): the implementation-neutral
  contract for 44 spell identities and element rows, 89 fixed eight-byte definitions, packed
  identity/level and animation fields, and the radius-3 data exception, while retaining resolution,
  targeting/geometry, effects, MP transactions, presentation, balance, localization, and content as
  separate-owner, Unknown, or deliberate-design boundaries.
- [`spellbook-state.md`](./design/contracts/spellbook-state.md): the implementation-neutral contract
  for raw learned-spell entries, raw definition-miss fallback, caller-selected-slot access with an
  independent learned count, and known-before-empty `LearnSpell` mutation/results, while retaining
  numeric packing/cardinality, malformed inputs, runtime acquisition, persistence, UI, resolution,
  presentation, and balance as Unknown or separate-owner boundaries.
- [`name-table-lookup-service.md`](./design/contracts/name-table-lookup-service.md): the
  implementation-neutral service contract for the `GetClassName` frontend, length-prefixed
  `FindName` traversal, bounded valid-table/index result relation, word-stack preservation, and
  three direct caller seams, while retaining table contents, caller-specific selection, malformed
  inputs, localization, rendering, runtime use, and presentation as private, separate-owner, or
  Unknown boundaries.
- [`stats-null-return-service.md`](./design/contracts/stats-null-return-service.md): the
  implementation-neutral provenance and compatibility contract for the bounded `nullsub_9482`
  source identity and its sole immediate-return instruction, while retaining caller admission,
  runtime reachability and result, stack/register/CCR behavior, timing, and gameplay meaning as
  Unknown or separate-owner boundaries.
- [`audio-system.md`](./design/contracts/audio-system.md): the implementation-neutral contract for
  music/SFX command identities, two-bank slot and target aliases, headers and channel roles, the
  static macro/note/sample/instrument/SFX domains, and the bounded four-command playback-state
  matrix, while retaining audible output, timing, transition policy, and asset licensing as Unknown
  or deliberate-design boundaries.
- [`combatant-state-access.md`](./design/contracts/combatant-state-access.md): the
  implementation-neutral contract for the source-shaped combatant selector and 56-byte entry ABI,
  typed getter overlays, mutation wrappers, clamp helpers, the bounded nine-operation runtime matrix,
  distance helper, and unused type encoder, while retaining higher-level lifecycle, persistence,
  presentation, caller-visible edge behavior, and balance as separate-owner or Unknown boundaries.
- [`global-flag-state.md`](./design/contracts/global-flag-state.md): the implementation-neutral
  contract for masked flag-index resolution, eight-flags-per-byte storage, bit-7-first selection,
  shared Check/Set/Clear addressing, and the bounded four-wrapper flag-trap inventory, while retaining
  exact capacity, trap ABI, runtime reachability, persistence, campaign meaning, and presentation as
  Unknown or separate-owner boundaries.
- [`graphics-service-state.md`](./design/contracts/graphics-service-state.md): the
  implementation-neutral contract for bounded decompression entry ABI, display initialization,
  sprite-link and palette-transition state, exact special-sprite routing, fixed flash words, and
  graphics helper inventory, while retaining camera logic, codec micro-implementation, rendered
  presentation, hardware timing, copyrighted assets, and forced invalid-state behavior as
  separate-owner, private, or Unknown boundaries.
- [`special-sprite-graphics-data.md`](./design/contracts/special-sprite-graphics-data.md): the
  implementation-neutral private-import contract for ten ordered special-sprite pointers, five
  initial payload owners, five aliases, six source resource definitions, exact corpus accounting,
  and aggregate decode/parity metadata, while retaining original payloads, route/service behavior,
  runtime admission, animation, transfer, presentation, and replacement policy as private,
  separate-owner, or Unknown boundaries.
- [`special-screen-asset-data.md`](./design/contracts/special-screen-asset-data.md): the
  implementation-neutral private-import contract for nine Stack-compressed special-screen
  resources, twelve uncompressed palette/layout resources, and the witch choice/bubble table,
  while retaining transfer-tail contents, screen engines, VInt/DMA/CRAM cadence, visible timing,
  rendered presentation, and copyrighted payloads as Unknown, separate-owner, or private
  boundaries.
- [`special-screen-control-flow.md`](./design/contracts/special-screen-control-flow.md): the
  implementation-neutral contract for bounded Sega-logo/title control, witch entry/action-page/
  dispatch/menu seams, source-static suspend counters and reset handoff, and ending operation-owner
  identities, while retaining save-action internals, assets, runtime input/timing, presentation,
  persistence, and copyrighted payloads as separate-owner, Unknown, or private boundaries.
- [`portrait-graphics-data.md`](./design/contracts/portrait-graphics-data.md): the
  implementation-neutral private-import contract for 56 ordered portrait slots, 52 source payload
  owners, four aliases, counted eye/mouth records, palette/stream partition, and aggregate
  decode/parity metadata, while retaining original payloads, selection, loading, transfer,
  rendering, timing, and replacement policy as private, separate-owner, or Unknown boundaries.
- [`portrait-window-state.md`](./design/contracts/portrait-window-state.md): the
  implementation-neutral contract for combatant/ally portrait selection, canonical portrait-data
  consumption, portrait and name-window state, callback lifetime, exact load/DMA call order, and
  bounded eye/mouth update operations, while retaining layouts, global VInt/DMA semantics, visible
  timing, localization, accessibility, and copyrighted payloads as separate-owner, Unknown,
  private, or deliberate-design boundaries.
- [`caravan-and-deals-state.md`](./design/contracts/caravan-and-deals-state.md): the
  implementation-neutral contract for Caravan status-bit normalization, full-add ignore, ordered
  removal compaction/tail clearing, and packed Deals count saturation/zero-removal boundaries, while
  retaining capacities, numeric masks/mappings, runtime/service outcomes, persistence, UI, and balance
  as separate-owner or Unknown boundaries.
- [`new-game-state-initialization.md`](./design/contracts/new-game-state-initialization.md): the
  implementation-neutral contract for the two accepted top-level initialization order edges,
  complete original ally-entry coverage, empty spell state, class-to-initial-to-derived stat order,
  flags/Deals/Caravan clears, and message-speed default, while retaining constants, cardinality,
  runtime reachability, persistence, UI, presentation, and balance as Unknown or separate-owner
  boundaries.
- [`party-membership-state.md`](./design/contracts/party-membership-state.md): the
  implementation-neutral contract for separate joined/active membership flags, the three
  `UpdateForce` counted prefixes, set-joined-before-rebuild `JoinForce` chronology, and distinct leave
  operation identities, while retaining post-helper list synchronization, capacity values, runtime
  outcomes, persistence, roster-choice UI, presentation, and balance as Unknown or separate-owner
  boundaries.
- [`combat-resolution.md`](./design/contracts/combat-resolution.md): the implementation-neutral contract from
  physical attacks through dodge, terrain/countering, critical, spread, and double/counter to
  temporary HP, reaction replay, EXP booking, and level-up connection, plus the shared fixture
  boundary for a future H4.
- [`exploration-control-flow.md`](./design/contracts/exploration-control-flow.md): the
  implementation-neutral contract for `MainLoop` exploration handoffs, within-iteration map-event
  precedence, player-action priority, interaction admission, and the bounded area, refill, flag, and
  update inventories, while keeping runtime timing, downstream effects, and private table content
  separate or Unknown.
- [`startup-control-flow.md`](./design/contracts/startup-control-flow.md): the implementation-neutral
  contract for the source-shaped conditional initial setup, bounded write/loop extents, ordered
  system and game handoffs, logo/intro/title return routing, and region admission branch, while
  retaining platform lifecycle, hardware timing, callee internals, input generation, presentation,
  persistence, and compatibility as separate-owner or Unknown boundaries.
- [`standalone-map-script-program-data.md`](./design/contracts/standalone-map-script-program-data.md):
  the implementation-neutral private-import contract for 47 standalone map-script source files,
  178 non-empty programs, 8,058 ordered operations, their resolved target and lexical-reference
  topology, and the bounded init-target ownership join, while retaining original program content,
  runtime admission/effects, story persistence, presentation, and timing as private, separate-owner,
  or Unknown boundaries.
- [`debug-control-flow.md`](./design/contracts/debug-control-flow.md): the implementation-neutral
  contract for battle-test setup and service handoffs, configuration gates and writes, seven debug
  battle-action routes, and four helper-local stack writes, while retaining admission, input, UI,
  downstream effects, persistence, timing, and product exposure as separate-owner or Unknown
  boundaries.
- [`rom-header-data.md`](./design/contracts/rom-header-data.md): the implementation-neutral
  provenance/import contract for the distinct 64-entry vector-table summary and selected console-
  header metadata, while retaining per-vector mappings and ABIs, boot/reset behavior, checksum
  algorithms, SRAM behavior, region compatibility, hardware outcomes, and raw header payload as
  separate-owner, Unknown, or private boundaries.
- [`map-area-description-routing.md`](./design/contracts/map-area-description-routing.md): the
  implementation-neutral private-import and routing contract for 126 ordered setup references, 75
  callable targets, 37 wrappers, 38 direct-return stubs, 227 physical entries, first-match selection,
  and the bounded normal-path `d6=1` condition rule, while retaining complete tables, text/function
  graphs, nonstandard reachability, effects, persistence, and presentation as private, separate-owner,
  or Unknown boundaries.
- [`map-camera-update-control-flow.md`](./design/contracts/map-camera-update-control-flow.md): the
  implementation-neutral contract for the bounded `VInt_UpdateViewData` target branch, destination-
  request seam, counter routes, scroll-speed precedence, and four exact word-width parallax update
  paths, while retaining camera commands, area inputs, callee behavior, callback scheduling, VDP
  output, timing, and presentation as separate-owner or Unknown boundaries.
- [`map-entity-data.md`](./design/contracts/map-entity-data.md): the implementation-neutral private-
  import contract for 126 setup-pointer references to 125 entity-list roots, 980 physical records
  producing 987 ordered list references, exact suffix-sharing and terminator topology, bounded record
  encodings, and the initial map-sprite domain, while retaining runtime population, movement,
  persistence, presentation, and complete original rows as separate-owner, Unknown, or private
  boundaries.
- [`map-entry-routing-state.md`](./design/contracts/map-entry-routing-state.md): the
  implementation-neutral contract for ordered flag-switched map selection, battle-candidate
  admission and bounded state writes, and savepoint/raft-reset selection, while retaining private
  table corpora, story and persistence meaning, downstream battle/map lifecycle, presentation, and
  malformed inputs as excluded, separate-owner, or Unknown boundaries.
- [`unused-mapload-control-flow.md`](./design/contracts/unused-mapload-control-flow.md): the
  implementation-neutral archival contract for the source-named unused mapload entry, its four
  ordered RNG request operands and staging flow, post-helper-tested VDP/VInt request loop, and
  four-word signed-store helper, while retaining reachability, callee effects, camera/display
  behavior, liveness, timing, and presentation as Inferred, separate-owner, or Unknown boundaries.
- [`map-exploration.md`](./design/contracts/map-exploration.md): the 79-map import boundary, shared
  block/layout ownership, 64x64 geometry, the executable canonical import, area/event/item/animation
  ordering, working-layout mutation, the two source-faithful map-script block-copy forms, the four
  source-shaped entity population/reload forms, the single source-faithful `cloneEntity` form, the
  three source-faithful map-script camera-control forms, the four source-faithful map-lifecycle forms,
  the two source-named trigger forms, the four source-named entity-placement forms, the six
  source-named entity-action bridge forms, the eight source-named entity lifecycle/presentation forms,
  the seven source-named entity gesture/relationship/motion forms, the twelve source-named
  screen/map-presentation forms, and the original-fact/Unknown/modernizable boundary with a modern
  renderer.
- [`map-layout-data.md`](./design/contracts/map-layout-data.md): the implementation-neutral private-
  import contract for 77 unique block/layout payload owners serving 79 map references, both exact
  shared-owner aliases, complete decoded block and 64x64 layout shapes, aggregate decoder-family
  counters, and source/ROM parity, while retaining original words and hashes as private data and
  leaving working-layout mutation, collision, transitions, and presentation to their existing owners.
- [`map-palette-data.md`](./design/contracts/map-palette-data.md): the implementation-neutral private-
  import contract for sixteen ordered source/effective map-palette identities, 79 ordered private map
  references, exact public usage/parity metadata, and the source-shaped first-word-clear rule, while
  retaining original words, payloads, hashes, full assignments, rendering, timing, and replacement
  policy as private, separate-owner, or Unknown boundaries.
- [`map-sprite-assignment-surface.md`](./design/contracts/map-sprite-assignment-surface.md): the
  implementation-neutral contract for five map-sprite writer origins, one indexed derivation helper,
  the private 81-assignment and 20-caller catalogs, and the joined accepted built-input exclusion of
  IDs 237..250, while retaining external corpus ownership, arbitrary runtime injection, rendering,
  timing, persistence, and replacement policy as separate-owner or Unknown boundaries.
- [`map-sprite-graphics-data.md`](./design/contracts/map-sprite-graphics-data.md): the implementation-
  neutral private-import contract for 720 ordered regular map-sprite source slots, 670 payload
  identities, the complete alias relation, 669 fixed-size Basic-decoded forms, and the shared sentinel
  identity, while retaining original assets, full aliases, runtime reachability, DMA, and presentation
  as private, separate-owner, or Unknown boundaries.
- [`map-setup-data.md`](./design/contracts/map-setup-data.md): the implementation-neutral private-
  import contract for 64 ordered map rows, 66 ordered flag rows, 130 setup references, 126 six-slot
  definition identities, four exact aliases, and the bounded entity-list access seam, while retaining
  the complete graph as private and selector, dispatcher, runtime, story, and presentation behavior
  with their existing owners or as Unknown.
- [`map-tileset-data.md`](./design/contracts/map-tileset-data.md): the implementation-neutral private-
  import contract for 115 ordered compressed/decoded map-tileset identities, the private 79-map and
  32-animation reference relations, and bounded public usage/parity metadata, while retaining original
  payloads, decoded art, hashes, complete assignments, runtime lifecycle, and rendering as private,
  separate-owner, or Unknown boundaries.
- [`battle-ai-decision.md`](./design/contracts/battle-ai-decision.md): the implementation-neutral
  contract for AI spell/item filters, priority/healing/support scores, final action/target choice,
  Move and Move Order, temporary terrain, commandsets, activation/swarm/special/standby control, and
  the bounded 14-case runtime matrix, while retaining caller-visible queued cases, natural path
  choice, complete multi-turn behavior, fairness, intent, balance, and presentation as Unknown.
- [`battle-action-construction.md`](./design/contracts/battle-action-construction.md): the implementation-neutral
  contract for action-family routing, target and per-target order, physical early exits, item
  use/break routing, Taros and Burst Rock gates, message-command records, and the complete static
  54-site message corpus, while retaining caller reachability, timing, presentation, unmodeled
  sub-routes, action choice, and downstream resolution as Unknown or separate-owner boundaries.
- [`battle-cutscene-routing.md`](./design/contracts/battle-cutscene-routing.md): the
  implementation-neutral contract for four independent 48-slot route tables, 59 built cutscene
  programs, intro/completion/leader/region gates, join and dead-list tails, and the corrected
  leader-position X/HP ranges, while retaining route targets, MAPSCRIPT effects, presentation,
  persistence, natural reachability, and story meaning as separate-owner or Unknown boundaries.
- [`battle-encounter-definition.md`](./design/contracts/battle-encounter-definition.md): the
  implementation-neutral contract for the 45-slot spriteset, map/global, and terrain-selection
  backbone, placement and local AI-geometry shape, supporting battle metadata, terrain aliases, and
  the separate 48-slot cutscene-route namespace, while retaining runtime admission, AI, pathfinding,
  resolution, presentation, story selection, and balance as separate-owner or Unknown boundaries.
- [`battle-functions-control-flow.md`](./design/contracts/battle-functions-control-flow.md): the
  implementation-neutral contract for fixture-bounded individual-turn routing, battle-load and
  move-command selection, cursor and target-list control, battle and battlefield menu branches, and
  ordered equipment/item/chest requests, while retaining callee completion, persistence, input
  timing, presentation, and natural scenario reachability as separate-owner or Unknown boundaries.
- [`battle-background-graphics-data.md`](./design/contracts/battle-background-graphics-data.md): the
  implementation-neutral private-import contract for the 30 ordered background slots, 27 payload
  owners, three aliases, exact pre-stream prefix/palette/two-stream structure, and aggregate
  decode/parity metadata, while retaining original payloads, selection, loading, transfer,
  rendering, timing, and replacement policy as private, separate-owner, or Unknown boundaries.
- [`battle-effect-graphics-data.md`](./design/contracts/battle-effect-graphics-data.md): the
  implementation-neutral private-import contract for 23 spell, 30 invocation, one status-animation,
  and two battle-transition stream identities, their private container/palette/offset relations,
  and aggregate decode/parity metadata, while retaining original payloads, selection, loading,
  transfer tails, rendering, timing, and replacement policy as private, separate-owner, or Unknown
  boundaries.
- [`battle-sprite-graphics-data.md`](./design/contracts/battle-sprite-graphics-data.md): the
  implementation-neutral private-import contract for separate 32-slot ally and 54-slot enemy
  tables, 86 source payload owners, header/palette/frame-stream structure, and aggregate decode/parity
  metadata, while retaining original payloads, selection, animation, loading, transfer, rendering,
  timing, and replacement policy as private, separate-owner, or Unknown boundaries.
- [`battle-weapon-ground-graphics-data.md`](./design/contracts/battle-weapon-ground-graphics-data.md):
  the implementation-neutral private-import contract for the 23-slot weapon table, 42 weapon-palette
  entries, 30-slot/27-header ground table and aliases, 33 source stream owners, heterogeneous
  palette/header accounting, and aggregate decode/parity metadata, while retaining original
  payloads, selection, loading, transfer, rendering, timing, and replacement policy as private,
  separate-owner, or Unknown boundaries.
- [`battle-scene-presentation.md`](./design/contracts/battle-scene-presentation.md): the
  implementation-neutral contract for the 21-command scene interpreter, initialization and selector
  order, background, actor-sprite, weapon, ground, spell, invocation, status, and transition
  selection/loader/presentation seams, 208 actor-animation sequences, and spell setup/update
  dispatch, while retaining command/frame timing, VInt/VDP effects, palette/layer composition,
  invocation transfer tails, reachability, and rendered output as Unknown.
- [`battle-control-lifecycle.md`](./design/contracts/battle-control-lifecycle.md): the implementation-neutral
  contract for new/resumed battle entry, round activation/spawn/turn scheduling, combatant death
  worklists and cleanup, bounded Battle 01 region and turn-order runtime behavior, double faction
  checks around one-step after-turn processing, and static victory/defeat result mutations, while
  retaining persistence, special-case, multi-round, presentation, and campaign boundaries as Unknown.
- [`battlefield-navigation.md`](./design/contracts/battlefield-navigation.md): the implementation-neutral
  contract for the 48x48 battlefield grids, terrain/occupancy state, weighted movement propagation,
  Manhattan range and target admission, attack-position selection, move strings, and the bounded
  five-case original-runtime movement matrix, while retaining natural-map reachability, unsafe-read
  effects, arithmetic edges, tactics, and presentation as Unknown.
- [`level-up.md`](./design/contracts/level-up.md): growth-curve randomized gains, minimum-growth pity, the battle
  EXP threshold entry, the complete level-up order, post-projection fixed growth, class level caps,
  cross-ally class-block scanning, current/derived stat and equipment refresh, stat
  clamp/underflow bounds, enemy curse suppression, inherited spell upgrades, the Karna/HEAL 3 complete
  prowess high-nibble matrix, the `LEVELUP_ARGUMENTS` result contract, and the original-fact and
  remake-choice boundary for the TORT effective-level defect.
- [`spell-resolution.md`](./design/contracts/spell-resolution.md): the implementation-neutral contract for
  attack-spell elemental resistance bit fields, integer damage adjustment, promoted power, DAO
  target-count division, spell critical, the shared downward spread, attack-spell EXP, HEAL 1 healing
  and healing EXP, SLEEP/SLOW 1 status resistance and immunity, DESOUL success/instant-death/
  multi-target kill EXP/gold, SPOIT MP absorption with boundary truncation, BOOST 1
  attribute/recast timing, the SILENCE cast gate, and the temporary-status post-turn lifecycle and
  persistent-scenario replay boundary.
- [`byte-copy-service.md`](./design/contracts/byte-copy-service.md): the implementation-neutral contract
  for the overlap-preserving `CopyBytes` result over its bounded positive-length domain, the original
  signed direction choice and exact `d7`/`a0`/`a1` preservation seam, and permitted platform
  `memmove` modernization, while retaining unsupported ranges, callers, timing, atomicity, and
  hardware effects as Unknown or separate-owner boundaries.
- [`music-wait-service.md`](./design/contracts/music-wait-service.md): the implementation-neutral
  contract for the two ordered source command-request identities, the post-`Sleep(3)` predicate
  shape, and the `k + 1` wait-request compatibility trace, while retaining transport acceptance,
  sound-side flag lifecycle, audible completion, timing, callers, and presentation as Unknown or
  separate-owner boundaries.
- [`service-interactions.md`](./design/contracts/service-interactions.md): the shop, church, caravan/depot, and
  blacksmith action order, cancel boundaries, and static resource mutation contracts, with explicitly
  retained persistence/timing unknowns.
- [`save-system.md`](./design/contracts/save-system.md): the two-slot SRAM, interleaved byte layout, checksum,
  occupied flag, the save/load/copy/delete static contracts, the single-launch in-process service
  matrix, and the cross-process persistence and power-loss boundaries still left to H3.
- [`input-system.md`](./design/contracts/input-system.md): two-port raw sampling, VInt current/repeat filtering,
  input-wait helpers, and the controller/timing Unknown boundary.
- [`text-and-font-system.md`](./design/contracts/text-and-font-system.md): the implementation-neutral
  contract for the 255-entry context-Huffman table, 86 reachable trees, 17 banks and 4,267 records,
  complete aggregate symbol replay, 80 variable-width glyph records, and 256-entry ASCII map, while
  retaining control effects, presentation, localization, and copyrighted content as Unknown or
  deliberate-design boundaries.
- [`ui-graphics-asset-data.md`](./design/contracts/ui-graphics-asset-data.md): the
  implementation-neutral private-import contract for nine shared UI resource/pointer identities,
  the exact eight-Stack/one-uncompressed payload split, heterogeneous menu routes, 167 source icon
  paths and exact 163-slot vanilla assembly, and bounded copy/highlight transformations, while
  retaining runtime selection, palette/DMA timing, rendering, localization, accessibility, and
  copyrighted payloads as Unknown, separate-owner, or private boundaries.
- [`ui-layout-data.md`](./design/contracts/ui-layout-data.md): the implementation-neutral static-data
  contract for 27 ordered UI layout grids, the first-class 16-entry spell-level pointer table and its
  aliases, four diamond-border variants, four direct assets, and exact non-overlapping source/ROM
  coverage, while retaining runtime window behavior, presentation, localization, accessibility, and
  copyrighted payloads as separate, Unknown, or deliberate-design boundaries.
- [`unused-menu-constant-write-control-flow.md`](./design/contracts/unused-menu-constant-write-control-flow.md):
  the implementation-neutral contract for the bounded `sub_15268` constant-write chronology, its
  exact fourteen-longword admitted memory projection, and static zero-symbolic-caller inventory,
  while retaining machine state, runtime reachability, memory meaning, UI behavior, and timing as
  Unknown or separate-owner boundaries.
- [`unused-technical-asset-data.md`](./design/contracts/unused-technical-asset-data.md): the
  implementation-neutral private-import contract for one nominally unused 5,694-byte container with
  four ordered Stack streams, two ordered palettes, the separate palette-pointer identity, and the
  bounded symbolic-reference inventory, while retaining original payloads, runtime reachability,
  rendering, hardware timing, and source-label meaning as private, separate-owner, or Unknown
  boundaries.
- [`window-system.md`](./design/contracts/window-system.md): the eight-slot window entry, layout
  allocation/reclamation, packed-coordinate addressing, VInt composition/DMA call order, and the
  presentation-timing Unknown boundary.
- [`dialogue-system.md`](./design/contracts/dialogue-system.md): the physical layout of the six map-script
  dialogue commands, the static cursor/name-index/portrait consumer order, the 21-case single-launch
  handler-local H3 contract, and three explicit presentation/runtime Unknown boundaries.
- [`sprite-dialogue-property-data.md`](./design/contracts/sprite-dialogue-property-data.md): the
  implementation-neutral contract for 119 ordered map-sprite dialogue-property records, their
  independent terminator, distinct key/portrait/speech-SFX metadata, and signed/zero-extended
  first-match lookup results, while retaining caller reachability, rendering, playback, timing,
  localization, and raw copyrighted rows as Unknown, separate-owner, or private boundaries.
- [`party-roster-state.md`](./design/contracts/party-roster-state.md): the physical layout of the ten map-script
  roster/death and active-party/AI/follower source forms, named-handler branch/mutation/call order,
  alias-aware caller identity, and two grouped H3 runtime boundaries.
- [`randomness.md`](./design/contracts/randomness.md): the static/runtime contracts for the main RNG, debug
  directional override, AI byte RNG, bounded sampling, helper-return state, and controlled
  source-shaped copy, plus the retry and seed-copy isolation boundary.
- [`interrupt-dma-and-trap-state.md`](./design/contracts/interrupt-dma-and-trap-state.md): the
  implementation-neutral contract for VInt scheduling gates and ordered stages, contextual slots,
  wait/sleep and DMA control state, fade predicates, and bounded trap transport, while retaining
  device timing, queue failure behavior, downstream effects, presentation, and reachability as
  separate-owner or Unknown boundaries.

## Decisions

`decisions/` records durable engine, emulator, data-format, and toolchain choices. A decision record
is created only when a real disagreement appears and the choice constrains later implementation.

- [`0001-bizhawk-for-h3-runtime-observation.md`](./decisions/0001-bizhawk-for-h3-runtime-observation.md):
  pins BizHawk 2.11.1 and records the measured Genesis Plus GX register-write boundary.
- [`0002-python-and-uv-for-project-tooling.md`](./decisions/0002-python-and-uv-for-project-tooling.md):
  the Python/uv toolchain, the stable CLI, and the frozen migration boundary of the existing
  PowerShell rails.
- [`0003-static-first-batched-runtime-research.md`](./decisions/0003-static-first-batched-runtime-research.md):
  Phase 2 first audits static batches, then concentrates questions that cannot be decided statically
  into single BizHawk matrices.
- [`0004-single-terra-worker-with-root-acceptance.md`](./decisions/0004-single-terra-worker-with-root-acceptance.md):
  the workflow boundary where a single Terra worker completes a Phase 2 evidence slice and the root
  thread independently reviews, verifies, scans, and commits.
- [`0005-remake-value-driven-driver-freeze.md`](./decisions/0005-remake-value-driven-driver-freeze.md):
  preserves existing evidence and verification while freezing low-remake-value driver/hardware
  exactness, redirecting the Phase 2 main line to events, maps, UI/save, and implementation-neutral
  content contracts.
- [`0006-parallel-worktrees-and-topic-branch-integration.md`](./decisions/0006-parallel-worktrees-and-topic-branch-integration.md):
  the collaboration boundary of serialized `main` integration, research/design dual-worktree lanes,
  short-lived topic branches, shared-file ownership, and tracked-only remote checks.
- [`0007-schema-contract-composition-and-migration.md`](./decisions/0007-schema-contract-composition-and-migration.md):
  audits large schemas for golden/shape duplication, prescribes a local `$ref` registry, structural
  contracts, and exact fixture layering, and migrates common-stats, common-menus, map-events, and
  map-script/H3 in order without weakening negative gates.
- [`0008-godot-csharp-cli-first-remake-tooling.md`](./decisions/0008-godot-csharp-cli-first-remake-tooling.md):
  audits the earlier Project-Mech-Strike Godot/C# and machine-local MCP workflow, reproduces its build
  and headless run boundary, and accepts Godot 4.7.1 .NET/C# as the fixed CLI-first Phase 4 baseline
  with a plain-C# domain layer, thin Godot adapter, and optional removable MCP; acceptance does not
  start remake implementation.
- [`0009-first-phase4-playable-slice.md`](./decisions/0009-first-phase4-playable-slice.md):
  accepts one continuous Map 3-through-Battle 01-completion scenario as the first playable milestone
  and requires independent Research and Design gap audits, accepted gap closure, main-gate readiness,
  and a separate explicit Phase 4 start action before implementation.
- [`0010-map3-battle01-product-acceptance.md`](./decisions/0010-map3-battle01-product-acceptance.md):
  accepts the private-local Map 3-through-Battle 01 product profile, including natural playable
  continuity, original private assets with no public redistribution, frame/audio/hardware-exact
  parity, modern accessible controls, and the remaining Research/H4 gates before Phase 4 may start.

## Evidence Vocabulary

- **Confirmed**: reproduced by a script/test, or directly supported by specific disassembly locations
  and observed runtime behavior.
- **Inferred**: the evidence is strong but not yet reproduced independently.
- **Unknown**: an open question that still needs experiments; convenient assumptions are not allowed.

The root [`README.md`](../README.md) is the source of truth for scope and route; the root
[`AGENTS.md`](../AGENTS.md) is the working constraint; this directory owns research and design
content.
