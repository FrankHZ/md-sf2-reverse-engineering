# Battle Encounter Definition Contract

- **Confirmed original structure:** the 45-slot battle spriteset, map-coordinate, background,
  leader, and terrain-selection surfaces; the complete spriteset header totals and ranges; the
  supporting neutral-entity, laser, random-battle, movement-matrix, after-battle-position, and
  special-membership table dimensions; and the separate 48-slot battle-cutscene route tables
  described below.
- **Inferred original behavior:** caller-dependent use of several named global tables and admission
  into non-empty battle-cutscene routes.
- **Unknown original behavior:** complete row-level semantics outside deeper owners, slot selection
  by story state, hidden and delayed spawn transitions, neutral-entity presentation, cutscene flag
  lifecycle, and product-level encounter balance or intent.
- Remake status: implementation-neutral Phase 3 contract; no encounter-authoring format, editor,
  simulation model, or deliberate compatibility deviation has been selected.
- Evidence date: 2026-08-08
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract defines how accepted original data partitions a tactical encounter into stable
identity and configuration domains. It owns:

1. battle-slot identity for the 45 source-form spriteset and terrain entries;
2. the structural shape of ally placements, enemy placements, AI regions, and AI points;
3. battle-indexed map coordinates, trigger overrides, backgrounds, enemy-leader flags, and other
   bounded global metadata;
4. terrain-payload selection and its two confirmed aliases;
5. the fact that battle-cutscene routing uses a separate 48-slot namespace.

It does not own combatant runtime lifecycle, AI decisions, player action construction,
pathfinding, damage or EXP formulas, battle-scene presentation, story progression, or cutscene
execution. Those remain with adjacent contracts, research owners, or **Unknown**.

The executable evidence owners are:

- `sf2-battle-spriteset-data-static-v1` in
  [`tests/fixtures/h2/battle-spriteset-data-static-v1.json`](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json);
- `sf2-battle-global-data-static-v1` in
  [`tests/fixtures/h2/battle-global-data-static-v1.json`](../../../tests/fixtures/h2/battle-global-data-static-v1.json);
- `sf2-battle-routing-data-static-v1` in
  [`tests/fixtures/h2/battle-routing-data-static-v1.json`](../../../tests/fixtures/h2/battle-routing-data-static-v1.json).

The research owners are [battle spriteset data](../../research/battle-spriteset-data.md),
[global battle data](../../research/battle-global-data.md), and
[battle routing and terrain data](../../research/battle-routing-data.md).

## Pre-Contract Evidence Audit

This synthesis was checked against the three research owners, their tracked fixtures, and their H2
verifiers. The audit preserves four important limits:

- the spriteset and global-data fixtures retain counts, addresses, hashes, selected indexes, and
  structural expectations rather than redistributing the source rows;
- complete canonical rows are generated only under ignored `local/derived/` output from the lawful
  private research input;
- Battle 01 has a deeper placement/runtime owner and MUST NOT be generalized to all 45 battles;
- the routing owner closes terrain pointer selection, aliases, and decompression, but not cutscene
  admission or story-state meaning.

The tracked evidence is therefore sufficient for an implementation-neutral shape and join
contract. It is not sufficient for a complete encounter database in the public repository or for a
claim that every field's runtime meaning is closed.

## Identity Domains

An implementation MUST keep these identities separate:

| Domain | Confirmed original boundary |
| --- | --- |
| battle slot | indexes 0 through 44 for the source-form spriteset table and terrain table |
| spriteset payload | `BattleSpriteset00` through `BattleSpriteset44`, one source file per slot |
| terrain payload | 43 unique compressed payloads selected by 45 battle slots |
| cutscene route slot | indexes in four independent 48-slot relative-pointer tables |
| map identity | 33 distinct maps referenced by 45 seven-field battle-map rows |
| combatant placement | source macro records within one selected spriteset, not a global roster slot |
| AI region and point | source geometry/target records within one selected spriteset |

The 48 cutscene-route slots MUST NOT be truncated to, joined positionally with, or described as the
same namespace as the 45 battle slots without new caller evidence. Likewise, a map identity is not a
battle identity: multiple battle rows can reference the same map.

## Forty-Five-Slot Encounter Backbone

The accepted source inventory establishes a common 45-entry backbone across these configuration
surfaces:

| Surface | Confirmed shape | Contract use |
| --- | --- | --- |
| spriteset entries | 45 ordered longwords, `BattleSpriteset00` through `BattleSpriteset44` | select placement and local AI-geometry payload |
| battle-map rows | 45 rows, seven fields each, 33 distinct map identities | select map coordinates and bounded trigger metadata |
| custom backgrounds | 45 battle-wide entries, 24 distinct values | retain the original selected value |
| enemy-leader flags | 45 entries, 28 marked present | retain membership as encounter metadata |
| terrain entries | 45 pointers to 43 unique payloads | select the initial terrain grid payload |

This alignment permits a remake importer to expose one battle-indexed encounter record with typed
references to independently owned payloads. It does not prove that every table is read at the same
time, that one story route selects all fields together, or that every field affects every encounter.

## Spriteset Structural Shape

Across the 45 spriteset headers, the confirmed source totals are:

| Record kind | Total | Per-battle range |
| --- | ---: | ---: |
| ally placements | 500 | 3-13 |
| enemy placements | 627 | 6-20 |
| AI regions | 133 | 0-14 |
| AI points | 37 | 0-2 |

The parser independently counts 500 `allyCombatant` macros, 627 `enemyCombatant` macros, and 1,127
each of `combatantAiAndItem` and `combatantBehavior`, rejecting disagreement between header counts
and macro bodies.

These numbers describe source records. They MUST NOT be reinterpreted as:

- the player's roster-choice capacity;
- the number of combatants simultaneously alive or admitted by the runtime controller;
- proof that every source placement is immediately visible;
- proof of a specific AI policy, item effect, follow-target transition, or region activation rule.

Runtime combatant admission and cleanup belong to the
[battle-control contract](battle-control-lifecycle.md). AI interpretation belongs to the
[battle-AI decision contract](battle-ai-decision.md), and player-issued commands belong to the
[battle-action construction contract](battle-action-construction.md). Hidden/delayed spawn state
outside accepted bounded runtime cases remains **Unknown**.

## Map Coordinates and Trigger Metadata

The global owner confirms 45 seven-field battle-map rows over 33 maps. Battle indexes 11, 25, and
41 have a non-default trigger coordinate. A fidelity importer MUST retain:

- the battle index;
- all seven source-derived fields in their canonical order and type;
- the distinction between map identity and battle identity;
- the explicit default versus non-default trigger representation.

The trigger rows establish stored encounter metadata, not the caller's admission test, normal-story
reachability, collision behavior, camera placement, or when a transition becomes visible. Map
movement and collision remain with the [map-exploration contract](map-exploration.md); campaign
selection remains outside this contract.

## Terrain Selection Boundary

The terrain pointer table has 45 battle slots backed by 43 unique compressed payloads. Two aliases
are confirmed:

- battle slot 4 reuses terrain payload 3;
- battle slot 32 reuses terrain payload 27.

`LoadBattleTerrainData` indexes the pointer table with `CURRENT_BATTLE * 4`, writes to the fixed
battle-terrain array, and invokes the accepted Stack decoder. Each unique payload produces one
48-by-48 grid of 2,304 bytes.

This contract owns the selected payload identity and alias preservation only. Terrain values,
movement cost, obstruction, adjacency, reachability, route construction, and cursor interaction are
owned by the [battlefield-navigation contract](battlefield-navigation.md). An implementation MUST
preserve aliases as equivalent selected content; it MAY deduplicate storage internally.

## Background, Leader, and Membership Metadata

The global tables also establish these bounded battle-indexed or category-indexed facts:

| Table | Confirmed static shape | Semantic boundary |
| --- | --- | --- |
| custom battle backgrounds | 45 entries, 24 distinct values | selected values confirmed; visible composition/timing separate |
| enemy background switches | 30 flags; indexes 3, 11, 18, and 27 enabled | membership confirmed; visible orientation **Unknown** |
| terrain backgrounds | 16 entries, 7 distinct values | mapping shape confirmed; rendering separate |
| enemy leaders | 45 flags, 28 present | membership confirmed; defeat/outcome use belongs to battle control |
| halved-EXP membership | one battle entry | membership confirmed; EXP arithmetic belongs to combat resolution |

Battle backgrounds and switches do not define battle-scene sequencing, palette timing, or asset
transfer. Those remain with the [battle-scene presentation contract](battle-scene-presentation.md).
The halved-EXP row MUST NOT be used as independent proof of the calculation or award timing; those
belong to the [combat-resolution contract](combat-resolution.md).

## Movement Matrix Boundary

The global-data owner confirms a 13-by-16 land-effect/move-cost matrix containing 208 entries, 127
of which are obstructed. This contract retains the matrix as a referenced encounter-support domain,
not as encounter-authored row content.

Movement-class meaning, cost lookup, obstruction, occupancy, reachability, path tie-breaking, and
map dimensions are owned by [battlefield navigation](battlefield-navigation.md). No encounter tool
should duplicate that matrix into an independently editable balance table unless a later design
decision explicitly creates a derived authoring view with one canonical source of truth.

## Neutral, Laser, Random, and Special Metadata

The accepted global inventory confirms the following additional shapes:

- 11 battles contain 17 neutral entities using four entity-action scripts;
- three laser battles carry per-enemy facing rows of 24, 16, and 12 entries, with 8, 2, and 2 active
  laser facings respectively;
- 11 battles are in the random-battle table;
- enemy upgrades use five categories, including an intentionally empty airborne exclusion list;
- one after-battle position table contains three four-byte entity placements.

These are table and membership facts. Neutral action timing/presentation, laser control and visual
effects, random-battle admission, upgrade bounds and failure behavior, after-battle row selection,
and the source-marked ignored fourth position byte are not closed by this contract. An importer MUST
retain the source distinction between absent, empty, and populated categories; a simulator MUST use
the deeper owner or keep the behavior **Unknown**.

## Cutscene and After-Battle Routing Boundary

Four separate relative-pointer tables contain 48 slots each:

| Route table | Non-empty targets |
| --- | ---: |
| before battle | 27 |
| battle start | 1 |
| enemy defeated | 3 |
| after battle | 25 |

The routing inventory also confirms four region routes followed by a terminator and a 52-byte
source-marked unused after-battle join table whose every byte is zero.

The contract preserves table identity, slot count, empty versus non-empty status, and the unused
alternate's exclusion from active layout ownership. It does not claim:

- positional equivalence between the 48 route slots and 45 battle slots;
- cutscene admission, empty-slot fallback, repeatability, or flag lifecycle;
- story meaning, presentation order, rendered effects, or save persistence;
- active use of the unused all-zero after-battle join table.

Those behaviors remain **Inferred** or **Unknown** until a dedicated caller/runtime owner closes
them.

## Alternate and Legacy Data

The top-level `spritesetentries.asm` is a 45-payload binary aggregate that defines the same symbols
as the maintained source-form spriteset tree, but the original layout does not include it. It is an
explicitly hashed alternate, not the canonical editable source and not an additional encounter set.

Similarly, `data/battles/global/afterbattlejoins.asm` is the unused 52-byte all-zero alternate; the
layout-owned table is under `data/battles/cutscenes`. A remake importer MUST NOT double-count either
alternate or assign it borrowed runtime reachability.

## Fidelity and Modernization Boundary

An original-fidelity encounter definition MUST preserve:

- separate battle, map, terrain-payload, and cutscene-route identities;
- all 45-slot structural joins without inventing a forty-sixth battle;
- spriteset header counts and source-record ordering;
- terrain aliases 4-to-3 and 32-to-27;
- empty, absent, disabled, and populated metadata as distinct states;
- the 48-slot cutscene table shapes without forcing them into the 45-slot battle namespace;
- excluded alternate files as provenance, not active encounter content.

A future remake MAY normalize records into typed objects, deduplicate aliased payloads, expose an
encounter editor, add validation previews, or deliberately rebalance encounters. It MAY also create
new battles beyond the original 45. Those are product decisions and MUST be represented as new or
overridden content, never silently reported as original facts.

## H4 Acceptance Surface

A future H4 adapter should validate canonical imported state rather than original source syntax or
RAM addresses. For each lawful local extraction it SHOULD compare:

1. 45 ordered encounter identities and their spriteset, map-row, background, leader, and terrain
   references;
2. per-spriteset header counts and aggregate totals/ranges;
3. terrain selected-content equality for aliases 4-to-3 and 32-to-27;
4. the dimensions and membership indexes of bounded global tables;
5. four independent 48-slot cutscene route-presence vectors;
6. exclusion of the legacy spriteset aggregate and unused global after-battle join alternate.

The public tracked fixtures can validate counts, hashes, addresses, and selected indexes. Exact
row-by-row H4 parity requires canonical output generated from the lawful private input and MUST NOT
be approximated by inventing rows from aggregate fixture facts.

Encounter playthroughs, tactical choices, spawn evolution, route admission, story transitions,
rendering, and balance remain outside this adapter until their dedicated evidence or design
decisions exist.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| spriteset slots, headers, aggregate placement/AI geometry shape | **Confirmed static** | `sf2-battle-spriteset-data-static-v1` ([`battle-spriteset-data-static-v1.json`](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)) | Row-level semantics beyond deeper owners, non-Battle01 integration, hidden/delayed transitions |
| map rows, backgrounds, leaders, movement matrix, neutral/laser/random/special tables | **Confirmed static** | `sf2-battle-global-data-static-v1` ([`battle-global-data-static-v1.json`](../../../tests/fixtures/h2/battle-global-data-static-v1.json)) | Caller behavior, presentation, admission, selection, and balance intent |
| terrain selection/aliases and 48-slot cutscene-route table shapes | **Confirmed static** | `sf2-battle-routing-data-static-v1` ([`battle-routing-data-static-v1.json`](../../../tests/fixtures/h2/battle-routing-data-static-v1.json)) | Cutscene admission/fallback/flags; terrain behavior belongs to navigation |
| roster admission, turns, outcomes | **Separate owner** | [Battle-control lifecycle](battle-control-lifecycle.md) | Do not infer runtime lifecycle from source placement counts |
| AI, movement, resolution, presentation, and campaign meaning | **Separate owner / Unknown** | Dedicated contracts and future evidence | No aggregate encounter fixture closes these behaviors |

## Reproduction

```powershell
uv run sf2 h2 battle-spriteset-data
uv run sf2 h2 battle-global-data
uv run sf2 h2 battle-routing-data
uv run sf2 design-contracts test
uv run sf2 research-index test
```
