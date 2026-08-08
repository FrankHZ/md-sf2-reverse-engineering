# New-Game State Initialization Contract

- Status: **Confirmed static NewGame phase and bounded state-initialization order**
- Evidence date: 2026-08-08
- Scope: implementation-neutral reconstruction of the seven accepted `NewGame` initialization facts,
  without importing cold-start routing, numeric content constants, runtime outcomes, persistence, UI,
  presentation, or balance meaning

## Judgment Boundary

This contract begins at the source-shaped `NewGame` routine. It does not establish how a player
reaches that routine, what precedes it in platform startup or title flow, or when its mutations become
durable or visible.

- **Confirmed**: game settings initialize before ally entries; every original ally entry is
  initialized; starting gold is assigned before Bowie is joined; ally spell slots receive the owner's
  Nothing/empty state; each ally's class data loads before initial stats, which precede derived-stat
  refresh; settings clear flags, Deals, and Caravan; and default message speed is set to `2`.
- **Inferred**: none. Story intent and caller-visible meaning are deliberately not inferred from the
  static initialization sequence.
- **Unknown**: the numeric starting-gold and empty-spell constants; ally cardinality, identity content,
  and iteration order; ordering among the accepted settings clear targets; settings fields outside the
  seven accepted facts; natural/runtime caller reachability; caller-visible return and partial-failure
  behavior; cold boot, system initialization, title/intro routing, and new-game UI/cancellation;
  save/load persistence; presentation, audio, localization, and balance intent.

The [ally-definition-data contract](ally-definition-data.md) owns ally identities, definitions, and
growth data without supplying this routine's runtime meaning. The
[combatant-state-access contract](combatant-state-access.md) owns the low-level combatant entry access
surface, not new-game lifecycle. [Party-roster state](party-roster-state.md),
[global flags](global-flag-state.md), and [Caravan/Deals state](caravan-and-deals-state.md) own their
respective state boundaries; this contract records only the accepted initialization handoffs.

## Evidence Owner and Source Audit

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md).

The fixture binds `NewGame` at decimal address `38710` and owns exactly seven
`expected.statsFacts.newGame` facts. A read-only audit of the pinned source confirms the same bounded
shape: the settings call precedes the ally loop; starting gold assignment precedes the Bowie join;
the ally initializer contains the accepted empty-spell assignment and the accepted
class/initial/derived-stat stage order; and the settings routine contains the three accepted clear
targets plus message speed `2`.

The source contains additional constants, writes, and low-level loop details. They are not promoted by
this contract because the executable owner does not expose them as accepted `newGame` facts. In
particular, source inspection is not used here to invent an ally count, numeric gold value, numeric
empty-spell encoding, internal clear ordering, a complete four-stage top-level order, or a complete
settings-reset corpus.

The common-scripting and battle-functions aggregates are deliberately excluded. Gameflow startup and
main-loop records, sibling party/flags/Caravan/Deals records, definition records, service/menu records,
and save records are also outside this contract's research-index association boundary.

## Top-Level Partial Order

**Confirmed static:** `NewGame` has two accepted order constraints plus one coverage fact:

1. game settings initialize before ally-entry initialization;
2. every original ally entry is initialized;
3. the symbolic starting-gold value is assigned before Bowie is joined.

The fixture does not add an order edge between completion of all ally initialization and starting-gold
assignment. It also does not expose a numeric ally count, loop direction, gold constant, caller, or
final runtime observation. “Join Bowie” preserves the source operation identity; it does not establish
party-capacity behavior, story availability, visible roster composition, or player choice.

| Phase | Accepted state fact | Deliberate boundary |
| --- | --- | --- |
| settings | completes before ally initialization | internal clear/write order and unlisted fields are **Unknown** |
| ally entries | every original ally entry is initialized | count, content, and iteration order are **Unknown** |
| starting resource | gold is assigned before the Bowie join | numeric gold value and economy intent are **Unknown** |
| starting member | Bowie join follows gold assignment | runtime party result, capacity, and presentation are **Unknown** |

## Per-Ally Initialization Boundary

**Confirmed static:** each ally initialization includes the owner's Nothing/empty spell-slot state and
the following ordered stat stages:

1. load ally class data;
2. initialize ally stats;
3. refresh derived combatant stats.

The accepted order prevents a remake importer from flattening class-derived fields, initial stats,
and derived-stat refresh into unordered writes. It does not define the full combatant record, the
number or packing of spell slots, class/stat formulas, growth curves, equipment effects, or the final
runtime value of any field.

The [spell-definition-data contract](spell-definition-data.md) owns fixed spell identities and
definitions, not mutable spell-slot storage. [Level-up](level-up.md) owns its accepted growth and
spell-learning route. Neither contract supplies a numeric empty-slot encoding to this initialization
contract.

## Settings Initialization Boundary

**Confirmed static:** the settings stage clears these three logical stores:

- global flags;
- Deals counts;
- Caravan storage.

The same accepted settings boundary sets default message speed to `2`. The fixture does not establish
an internal order among the three clear targets or between those clears and the message-speed write.
It also does not expose every setting or memory field touched by the source routine.

These facts define initialization handoffs, not the full semantics of the stores. Flag addressing,
Caravan normalization/compaction, and Deals packed-count mutation remain with their own contracts.
Message speed `2` is an accepted stored default, not evidence for visible text cadence, accessibility
policy, input timing, or localization behavior.

## Cross-System Separation

The NewGame routine is not the complete new-game experience:

- cold start, system setup, region checks, base resources, title/intro flow, and top-level loop routing
  remain with gameflow and technical owners;
- ally definitions and combatant access describe data/state shapes, not runtime initialization success;
- roster membership owns party-state behavior after a join request;
- item, spell, flag, Caravan, and Deals owners retain their independent storage semantics;
- save evidence does not prove that the newly initialized state persists across every process or
  power-loss path;
- UI, confirmation/cancellation, audio, presentation, localization, and starting-balance policy are
  not derived from this static call order.

The [story-progression synthesis](../synthesis/story-progression.md) may place accepted handoffs in a
larger explanation, but it must not use this contract as proof of natural runtime reachability or
player-visible chronology.

## Implementation-Neutral State Model

```text
NewGameInitializationPlan
  operations:
    settingsStage
    allyInitializationStage
    startingGoldAssignment
    startingMemberJoin: Bowie
  acceptedOrderEdges:
    settingsStage -> allyInitializationStage
    startingGoldAssignment -> startingMemberJoin
  allyInitializationCoverage: everyOriginalAllyEntry

SettingsStage
  clearGlobalFlags
  clearDeals
  clearCaravan
  defaultMessageSpeed: 2

AllyInitializationStage
  coverage: everyOriginalAllyEntry
  perEntry:
    initializeSpellSlots: Nothing
    orderedStatStages:
      - loadClassData
      - initializeStats
      - refreshDerivedStats
    spellInitRelativeToStatStages: notContracted
```

This is a logical parity model, not a required engine memory layout. The model intentionally omits
ally count/order, raw content, numeric starting gold, numeric Nothing encoding, unaccepted settings
fields, return values, and persistence behavior.

A modern engine may construct immutable defaults, batch entity creation, use transactions, or expose
typed initialization results. Its compatibility adapter must still reproduce the accepted top-level
partial order, per-ally stat-stage order, three clear targets, and message-speed default.

## Original Fidelity and Modernization

Original-fidelity mode preserves the seven accepted static facts and the representative `NewGame`
identity/address. It reports caller, runtime, and persistence questions rather than treating the
source call order as a complete player-facing new-game flow.

A remake may choose different starting resources, roster composition, default settings, accessibility
defaults, error handling, or save creation policy. Those are explicit product decisions unless an
original-fidelity adapter reproduces the accepted boundary and records intentional deviations.

Original names and other copyrighted content are not required for public parity fixtures. Public
tests should use structural metadata, source identifiers, and synthetic values.

## H4 Acceptance Gates

A future remake NewGame adapter passes this contract only when:

1. settings initialization precedes ally-entry initialization;
2. every imported original ally entry receives an initialization pass without hard-coding an
   unaccepted contract-level count or content corpus;
3. each ally initializes spell slots to the symbolic Nothing state and independently preserves
   class-data load, initial-stat construction, and derived-stat refresh order, without inventing a
   contract-level relative order between those two accepted facts;
4. starting gold assignment precedes the Bowie join without inventing a numeric gold constant or an
   unaccepted order edge from ally initialization to gold assignment;
5. settings clear flags, Deals, and Caravan and set message speed to `2` without inventing an internal
   clear/write order or complete settings corpus;
6. gameflow reachability, caller/return behavior, runtime results, persistence, UI, presentation,
   localization, and balance remain separately tested or explicitly **Unknown**;
7. public fixtures contain structural metadata and synthetic values rather than copyrighted content.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| settings before all ally initialization | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Caller, ally cardinality/order, runtime outcome |
| starting gold then Bowie join | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Numeric gold, party result, story/player meaning |
| empty spell state; independent class → initial stats → derived refresh order | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Relative spell/stat-stage order, numeric encoding, formulas, final field values |
| flags/Deals/Caravan clears and message speed `2` | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Internal ordering, unlisted settings, visible/runtime effect |
| startup/title/UI/save/presentation/balance semantics | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer a complete new-game experience from static initialization |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
