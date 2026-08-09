# Debug Control Flow Contract

- Status: **Confirmed static debug-routing and bounded state-write contract**
- Evidence date: 2026-08-09
- Scope: implementation-neutral reconstruction of the original battle-test, configuration, and
  debug battle-action control surface, without promoting source labels or helper identities into
  normal-player reachability, UI, runtime effects, persistence, audio, or battle-resolution meaning

## Judgment Boundary

This contract begins at three source entries: `DebugModeBattleTest`, `CheatModeConfiguration`, and
`DebugModeActionSelect`. It preserves their accepted initialization writes, branch predicates,
prompt bounds, relative routes, handoff identities, and helper-local stack writes. It ends whenever
control passes to a menu, battle, sound, input, flag, party, stat, display, or other subsystem.

- **Confirmed**: three source files contain eight accepted global entries at exact ROM addresses;
  battle-test setup writes two toggles, submits the ordered 29 non-Bowie ally identities to the join
  helper, applies source value 99 through eight named Bowie stat setters, registers the window VInt
  pointer, and stores a 30-length generic-list declaration beside the exact byte sequence `0..31`;
  its prompt and service handoffs preserve the accepted source order, including a church-call block
  that is statically unreachable under the preceding result test and branches; configuration
  preserves its Start, Up, completed-bit, and configuration-toggle gates plus four ordered choice
  writes; debug battle-action selection preserves a seven-route relative table, bounded operands,
  target selection, and four `seq` writes to named stack aliases.
- **Inferred**: source labels, comments, and route shape strongly suggest developer tooling. This
  inference does not establish how a player reaches it or what any prompt looks like.
- **Unknown**: natural or normal-player admission; exact controller samples and frames; prompt
  rendering, cancellation meaning beyond the checked branches, and user-visible results; whether
  each zero-direct-caller entry is reached indirectly; battle, menu, sound-test, stat, party, flag,
  save, item, and action effects after each handoff; persistence; display/audio timing; malformed or
  injected state; emulator and hardware behavior; and intentional remake exposure of debug tools.

The original debug surface is evidence, not a requirement that a public remake ship a player-facing
debug menu. A modern implementation may isolate these routes behind development-only tooling while
retaining the accepted control and data boundaries for original-fidelity testing.

## Evidence Owner and Association Audit

`sf2-remaining-core-static-v1`
([`remaining-core-static-v1.json`](../../../tests/fixtures/h2/remaining-core-static-v1.json)) is the
sole executable owner consumed by this contract. Its verifier is
[`remaining_core.py`](../../../src/sf2tool/h2/remaining_core.py), and the owning explanation is
[ROM Header, Window Engine, and Special Debug Flows](../../research/remaining-core.md). This
contract consumes only `expected.debugFacts` and the following three research records:

- `debug.battle-test`;
- `debug.configuration`;
- `debug.battle-actions`.

The fixture directly binds five research records in total. `core.window-engine` retains its existing
[window-system](window-system.md) contract. `core.rom-header` remains unassociated. Neither record
gains this contract by sharing an executable owner.

The source audit used pinned upstream commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`. Its three selected source files contain eight accepted
entries:

| Source file | Entry | ROM address |
| --- | --- | ---: |
| `battletest.asm` | `DebugModeBattleTest` | `0x769C` / 30,364 |
| `battletest.asm` | `LoadAllyStatsDecimalDigits` | `0x78BC` / 30,908 |
| `battletest.asm` | `LevelUpWholeForce` | `0x7920` / 31,008 |
| `battletest.asm` | `GetDecimalDigits` | `0x7930` / 31,024 |
| `configurationmode.asm` | `CheatModeConfiguration` | `0x7E3A` / 32,314 |
| `debugmodebattleactions.asm` | `DebugModeActionSelect` | `0x9A9A` / 39,578 |
| `debugmodebattleactions.asm` | `DebugModeSelectTargetEnemy` | `0x9B44` / 39,748 |
| `debugmodebattleactions.asm` | `DebugModeSelectHits` | `0x9B58` / 39,768 |

The exact addresses, ordered identities, bounds, labels, and branches below are public metadata.
Original text strings, graphics, audio, ROM bytes, live RAM, and captured frames remain private or
separate-owner material.

## Battle-Test Setup

**Confirmed static:** `DebugModeBattleTest` starts by writing `-1` to `DEBUG_MODE_TOGGLE` and
`SPECIAL_TURBO_TOGGLE`. It then invokes `j_JoinForce` for the fixture's exact ordered 29-label
non-Bowie roster. This contract preserves that ordered symbolic sequence without inferring the
resulting active party, counted-list synchronization, map placement, or later roster state. Those
belong to the [party-membership state contract](party-membership-state.md) or remain **Unknown**.

The source next selects `ALLY_BOWIE`, loads the source value 99, and calls these setters in order:

1. `j_SetBaseAgi`;
2. `j_SetBaseAtt`;
3. `j_SetBaseDef`;
4. `j_SetMaxHp`;
5. `j_SetCurrentAgi`;
6. `j_SetCurrentAtt`;
7. `j_SetCurrentDef`;
8. `j_SetCurrentHp`.

This is a static call-and-operand contract. It does not redefine clamping, selector validity,
derived-stat consistency, or caller-visible results owned by
[combatant-state access](combatant-state-access.md).

After other source handoffs outside this contract, the function registers `VInt_UpdateWindows`
through the source `VINT_FUNCTIONS`/`VINTS_ADD` form and calls `InitializeWindowProperties`. The
pointer and handoff identities are Confirmed static. Interrupt cadence, callback execution, window
composition, DMA, and visible presentation are outside this contract.

The generic-list setup deliberately preserves two different accepted facts:

- the stored length declaration is `COMBATANT_ALLIES_NUMBER`, whose accepted value is 30;
- the backing bytes written by the source are exactly the ordered values `0..31`.

The 30-length declaration must not be normalized to 32 merely because 32 bytes are present. The two
trailing stored bytes are retained as source data, not promoted into counted membership or a roster
cardinality rule.

## Battle-Test Prompt and Service Route

**Confirmed static:** after `CheatModeConfiguration` returns, the battle-test loop preserves these
source-shaped decisions and handoffs:

1. a number prompt admits `0..49`;
2. a negative result branches to the member/level-up route;
3. a nonnegative result is retained while a second `0..1` prompt is obtained;
4. a nonzero second result sets the selected battle index plus
   `BATTLE_INTRO_CUTSCENE_FLAGS_START` through the flag helper;
5. the follower-flag helper receives `FLAG_INDEX_FOLLOWERS_ASTRAL`;
6. battle-map coordinate addressing uses source stride
   `BATTLEMAPCOORDINATES_ENTRY_SIZE_FULL`, whose accepted value is seven;
7. control hands off in source order to `j_BattleLoop`, `j_ChurchMenu`, a `0..100` shop prompt and
   `CURRENT_SHOP_INDEX` write, `j_ShopMenu`, `j_FieldMenu`, and `j_CaravanMenu`;
8. the source returns to the battle-test prompt loop.

This sequence owns only predicates, operands, writes, and handoff order. It does not claim that a
battle completes successfully, that church or shop actions occur, that the selected indexes are
valid content, or that any service returns a particular result. Battle lifecycle, service behavior,
map-coordinate meaning, flags, presentation, and player intent remain separate.

The negative-result route first builds the stat-display source buffer, then calls the member-summary
handoff. The accepted result structure is exact:

- `tst.b d0` is followed by a nonzero branch back to the battle prompt;
- the fallthrough value is therefore zero;
- the subsequent `bpl` reaches `LevelUpWholeForce`;
- a church-call block exists textually between the branches but is statically unreachable under
  this preceding `tst`/`bne`/`bpl` structure.

The church block remains part of the source inventory. It is not a runtime route, negative-result
meaning, hidden service rule, or fidelity requirement to execute dead control flow.

`LoadAllyStatsDecimalDigits` loops over 30 ally selectors. For each selector it stores six packed
decimal words at offsets 0, 2, 4, 6, 8, and 10 of a 16-byte record. The accepted call sequence reads
level, maximum HP, maximum MP, base attack, base defense, and base agility; it writes current HP and
MP through their setters after the corresponding maximum getters. `LevelUpWholeForce` separately
submits 30 selectors to `j_LevelUp`. Formatting, stat mutation results, level-up rules, visible member
screens, and buffer lifetime are not owned here.

## Configuration Gates and Writes

**Confirmed static:** `CheatModeConfiguration` returns immediately unless the Start input bit is
set. If Start is set, the source tests Up and then completed save-flag bit 7. When both source tests
select that edge, `bne.w j_SoundTest` performs a direct transfer without pushing a return address.
This contract owns only that target identity and transfer form. It does not own the target
implementation, return behavior, sound enumeration, audible output, or presentation.

When the sound-test edge is not taken, a zero `CONFIGURATION_MODE_TOGGLE` returns. Otherwise the
source processes four choices in this exact order:

| Text identity | Zero-result write | Nonzero-result route |
| ---: | --- | --- |
| 450 | write `-1` to `SPECIAL_TURBO_TOGGLE` | continue without that write |
| 451 | write `-1` to `CONTROL_OPPONENT_TOGGLE` | continue without that write |
| 452 | write `-1` to `AUTO_BATTLE_TOGGLE` | continue without that write |
| 455 | set save-flag bit 7 | clear save-flag bit 7 |

These are source-static result tests and writes. They do not establish prompt button meaning,
initial toggle values, mutual exclusion, persistence, save validity, accessibility, localized text,
or the runtime effects suggested by the toggle names. The
[special-screen control-flow contract](special-screen-control-flow.md) owns the separate Sega-logo
debug-sequence and configuration-handler boundary; it does not transfer that admission evidence to
this contract.

## Debug Battle-Action Construction

**Confirmed static:** `DebugModeActionSelect` obtains a value in the source range `0..6`, compares
the returned byte with `-1`, and returns without writing the selected action when that comparison is
equal. Otherwise it writes the selected word and dispatches through this exact ordered relative
table:

| Index | Relative target | Accepted helper-local write shape |
| ---: | --- | --- |
| 0 | `Attack` | one target word |
| 1 | `Magic` | one packed spell word, then one target word |
| 2 | `Item` | item word, target word, value word |
| 3 | `EndTurn` | no additional word in this helper |
| 4 | `BurstRock` | no additional word in this helper |
| 5 | `Muddle` | no additional word in this helper |
| 6 | `PrismLaser` | write source-labelled battle value |

`Magic` prompts for `1..4`, subtracts one, shifts the result left by six, and adds it to a second
prompt result in `0..42` before storing the packed word. `Item` obtains `0..127`, then an enemy
target, then `0..3`. `DebugModeSelectTargetEnemy` admits source values `128..159`.

The ranges and packing steps are not validation of spell, item, target, or battle semantics. They do
not establish that every admitted number denotes content, that cancellation is handled after each
nested prompt, or that the downstream battle engine accepts the resulting words. The ordinary
[battle-action construction contract](battle-action-construction.md), item and spell data contracts,
and runtime battle evidence remain separate.

`DebugModeSelectHits` performs four prompt/test/`seq` groups in source order and writes to these
stack-frame aliases:

| Order | Source alias | Stack offset |
| ---: | --- | ---: |
| 1 | `debugDodge` | -23 |
| 2 | `debugCritical` | -22 |
| 3 | `debugDouble` | -21 |
| 4 | `debugCounter` | -20 |

This contract preserves the aliases, offsets, order, and source `seq` operation. It does not claim a
particular prompt response, probability, attack outcome, dodge/critical/follow-up behavior, or
caller-visible effect. The [randomness contract](randomness.md) owns separate debug RNG evidence; it
does not supply evidence for these manual stack writes.

## Direct Caller Inventory

The bounded comment-stripped scan finds exactly these external direct caller occurrences for the
eight selected entries:

- `battleactionsengine_1.asm` contains one direct site each for `DebugModeActionSelect` and
  `DebugModeSelectHits`;
- `witchstart.asm` contains two direct sites for `CheatModeConfiguration`.

The accepted direct-call counts for `DebugModeBattleTest`, `LoadAllyStatsDecimalDigits`,
`LevelUpWholeForce`, `GetDecimalDigits`, `DebugModeSelectTargetEnemy`, and the other helper-local
entries are zero outside their bounded source files. A zero direct count never establishes dead code
or unreachability: computed transfers, source-local branches, alternate linkage, modified builds, and
runtime admission are outside this inventory. No external longword-pointer occurrence was found in
the bounded scan.

## Cross-System Separation

The debug route orchestrates many adjacent systems without owning them:

- [party-membership state](party-membership-state.md) owns accepted join and counted-prefix behavior;
- [combatant-state access](combatant-state-access.md) owns stat access, selector, and clamp boundaries;
- [global-flag state](global-flag-state.md) owns flag storage and wrapper structure;
- [window-system](window-system.md) owns window allocation, motion, VInt composition, and DMA-call
  boundaries;
- [special-screen control flow](special-screen-control-flow.md) owns Sega-logo and Witch control
  surfaces;
- [battle-action construction](battle-action-construction.md) owns ordinary action construction;
- [item-definition data](item-definition-data.md) and [spell-definition data](spell-definition-data.md)
  own static identities and packed definitions;
- [randomness](randomness.md) owns base/debug RNG behavior rather than manual debug prompts.

Menu internals, service transactions, battle outcomes, input sampling, audio, text resources,
presentation, map data, saves, and story state remain with their dedicated owners or **Unknown**. No
adjacent research record gains this contract merely because a handoff is named.

## Implementation-Neutral Control Model

The minimum logical model is a metadata and control projection:

```text
DebugControlSurface
  evidenceOwner: sf2-remaining-core-static-v1.expected.debugFacts
  sourceCommit
  sourceFiles[3]
  entries[8]:
    symbol
    romAddress

  battleTest:
    initialToggleWrites[2]
    orderedJoinLabels[29]
    bowieStatValue: 99
    orderedStatSetters[8]
    windowVIntPointerIdentity
    genericList:
      declaredLength: 30
      storedBytes: ordered 0..31
    battlePromptRange: 0..49
    negativeRoute: memberAndLevelUpControl
    cutscenePromptRange: 0..1
    battleCoordinateStride: 7
    orderedServiceHandoffs
    statDisplay:
      selectorCount: 30
      recordStrideBytes: 16
      wordOffsets: [0, 2, 4, 6, 8, 10]
    unreachableChurchBlockRetainedAsSourceStructure: true

  configuration:
    startGate
    upAndCompletedBitDirectSoundTestTransfer
    configurationToggleGate
    choices[4]:
      textIdentity
      zeroWrite
      nonzeroRoute

  battleActions:
    topLevelRange: 0..6
    cancelComparison: returnedByte == -1
    orderedRelativeRoutes[7]
    magicPacking
    itemOperands
    enemyTargetRange: 128..159
    orderedHitWrites[4]:
      alias
      stackOffset
      operation: seq

  externalDirectCallerOccurrences
  directCallerZeroesRetainedWithoutReachabilityClaim
```

The model contains no original strings, layouts, graphics, music, code bytes, ROM, save, RAM dump,
trace, or emulator state. Public fixtures and reports retain symbolic identities and structural
metadata only. A private fidelity adapter may reconstruct source-shaped buffers from licensed or
user-provided inputs without publishing their payloads.

## Original Fidelity and Modernization

Original-fidelity testing preserves the accepted initialization operands, the `30` declaration plus
stored `0..31` bytes, source branch structure, prompt bounds, route order, handoff identities, and
four stack writes. It does not invent a normal-player route, execute the statically unreachable
church block, or infer downstream effects.

A modern engine may expose typed development commands instead of reproducing the original prompts,
may omit the tools from release builds, and may validate identifiers before dispatch. Such choices
are modernizations. Compatibility tooling should still be able to emit the accepted source-facing
control trace and report intentional deviations separately.

## H4 Acceptance Gates

A future debug-control adapter passes this contract only when:

1. all three source identities, eight entry identities, and exact ROM addresses remain traceable;
2. battle-test setup preserves the two toggle writes, ordered 29 join labels, source value 99, eight
   setter identities, and window VInt pointer handoff without claiming their runtime results;
3. the generic list retains a declared length of 30 beside all 32 stored bytes `0..31`, without
   normalizing either fact;
4. the battle-test prompt ranges, cutscene-flag operand, seven-byte coordinate stride, and service
   handoff order remain reproducible without importing downstream battle, menu, or service behavior;
5. the member-result branch keeps the church block as statically unreachable source structure and
   never requires it to execute;
6. configuration preserves Start/Up/completed-bit/configuration-toggle gates, direct SoundTest
   transfer identity, four choice identities, and exact writes without assigning UI or persistence;
7. the seven relative action routes, bounded operands, magic packing, target range, and four ordered
   stack writes remain reproducible without claiming battle effects;
8. direct caller occurrences and zeros remain exact, while zero counts never become unreachability
   claims;
9. the association boundary remains exactly the three `debug.*` records; `core.rom-header`,
   `core.window-engine`, and all adjacent records remain semantically unchanged;
10. public artifacts contain only metadata and synthetic state, never ROM, source payload, text,
    graphics, audio, live memory, save, trace, or emulator-state content.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| three sources, eight entries, labels, addresses, and direct caller inventory | **Confirmed static** | `sf2-remaining-core-static-v1` ([`remaining-core-static-v1.json`](../../../tests/fixtures/h2/remaining-core-static-v1.json)) | Indirect reachability, normal admission, modified builds |
| battle-test initialization, generic-list split, prompts, branch structure, and handoffs | **Confirmed static** | same `expected.debugFacts` owner | Party/stat/battle/menu effects, UI, timing, persistence |
| configuration gates, direct SoundTest transfer, and four choice writes | **Confirmed static** | same `expected.debugFacts` owner | Input frames, prompt meaning, sound implementation, save outcome |
| seven action routes, operand packing, target range, and four stack writes | **Confirmed static** | same `expected.debugFacts` owner | Nested cancellation, action validity, battle resolution, presentation |
| developer-tool purpose | **Inferred** | source labels, comments, and route shape | Player-facing product intent and release policy |
| runtime behavior and visible experience | **Unknown / separate owner** | future grouped runtime evidence and adjacent contracts | Do not infer end-to-end behavior from static control |

## Reproduction

```powershell
uv run sf2 h2 remaining-core
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated JSON remains under ignored `local/derived/remaining-core-static.json`.
