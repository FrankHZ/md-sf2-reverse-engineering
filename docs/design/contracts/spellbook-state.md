# Spellbook State Contract

- Status: **Confirmed static spellbook lookup and learning boundaries**
- Evidence date: 2026-08-08
- Scope: implementation-neutral reconstruction of stored learned-spell entries, selected-slot and
  learned-count access, definition-miss fallback, and `LearnSpell` result/mutation order, without
  importing runtime acquisition, persistence, UI, battle resolution, presentation, or balance meaning

## Judgment Boundary

This contract begins at the source-shaped helpers in `spellstats.asm`. It defines a bounded mutable
spellbook service and one definition-lookup handoff. It does not define when a character earns a spell,
how a player sees it, or how the spell behaves in battle.

- **Confirmed fixture-owned facts**: a definition lookup miss falls back to the first definition entry;
  `LearnSpell` returns `0` for static success, `1` when the same or a higher level is already known,
  and `2` when no room remains; a higher incoming level replaces the known entry.
- **Confirmed direct source review**: `GetSpellDefinitionAddress` compares the raw incoming `d1.b`
  against definition-entry identity bytes and does not apply `SPELLENTRY_MASK_INDEX` itself;
  `GetSpellAndNumberOfSpells` returns the entry from the caller-selected slot while separately scanning
  every source slot for the learned count; and `LearnSpell` completes its known-base scan before any
  empty-slot search.
- **Unknown**: numeric mask, shift, slot-count, Nothing, and definition-count constants; malformed or
  duplicate stored-entry behavior beyond the bounded source form; invalid combatant, slot, or
  definition inputs; runtime caller reachability and mutation outcomes; transaction/rollback behavior;
  save/load persistence; acquisition policy; UI, name/localization, MP use, targeting, range, effects,
  battle-scene presentation, audio, and balance intent.

The [spell-definition-data contract](spell-definition-data.md) owns immutable spell identity and record
packing. [Level-up](level-up.md) owns its accepted caller route, [spell resolution](spell-resolution.md)
owns battle execution, and [combatant state access](combatant-state-access.md) owns generic combatant
entry access. The [new-game initialization contract](new-game-state-initialization.md) owns only its
accepted symbolic empty-slot initialization fact. This contract does not borrow those adjacent
contracts' research-index associations.

## Evidence Owner and Source Audit

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the sole
executable owner consumed by this contract. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md).

The fixture binds the `spellstats.asm` service surface at representative `GetSpellName` decimal
address `37318` and owns exactly five `expected.statsFacts.spells` facts. Those five facts do not
constitute runtime observation and do not establish every helper's complete API.

A separate read-only audit of pinned upstream commit
`c834c652b6862bc5679fd7f69a38a7093206efc6` reviewed
`code/common/stats/spellstats.asm` from `GetSpellName` through `GetSpellCost`. That audit supplies the
bounded helper chronology below. It does not promote source comments, symbolic constants, or adjacent
resource lookups into player-facing semantics.

Active common-maps Issue #80, queued tech-graphics Issue #81, and all aggregate fixtures are
deliberately excluded. Spell-definition records, combatant setters, new-game, level-up, battle-action,
menu, save, text, and gameflow records are outside this contract's research-index association
boundary. The sole future association is `stats.spell-stats`.

## Stored Entry Boundary

The source-shaped spellbook stores ordered raw entries. Helpers may derive a base spell identity and a
level code from an entry through symbolic mask/shift operations, but this contract does not import
numeric bit positions or a numeric Nothing encoding.

Masking is helper-specific rather than a universal precondition:

- `GetSpellName` applies the symbolic base-index mask before its name lookup;
- `GetSpellAndNumberOfSpells` applies the mask while deciding whether each scanned slot is Nothing;
- `LearnSpell` applies the base-index mask and level shift to compare stored and incoming entries;
- `GetSpellDefinitionAddress` does **not** apply the base-index mask before comparison.

A remake MUST NOT normalize every incoming spell value through one shared masking step. In particular,
definition lookup preserves the raw incoming `d1.b` comparison boundary even if another helper accepts
the same packed entry type.

The source contains a symbolic slot counter and symbolic entry constants. Their numeric values remain
outside this contract. The logical model must retain every imported slot without hard-coding a
cardinality inferred from another document.

## Definition Lookup Boundary

**Confirmed static:** `GetSpellDefinitionAddress` compares raw incoming `d1.b` against the identity
byte at each source definition entry. If no comparison matches across the source-bounded scan, it
returns the first definition entry.

The routine itself does not mask level bits or otherwise normalize `d1.b`. Importing the masked-name
or spellbook-count behavior into this lookup would change the accepted source boundary.

The first-entry fallback is an address-selection rule, not proof that entry zero is semantically safe,
equivalent to Nothing, visible to a player, or appropriate for malformed remake data. Definition-table
cardinality, duplicate identities, corrupted tables, and caller-visible error policy remain
**Unknown** or explicit modernization decisions.

## Selected Slot and Learned Count

`GetSpellAndNumberOfSpells` has two separate outputs:

1. it reads and returns the raw entry at the caller-selected slot;
2. it independently scans every source slot, masks each scanned entry to its base identity, and counts
   entries whose masked identity is not the symbolic Nothing value.

The selected return entry is not necessarily the first learned spell. This contract deliberately does
not repeat the upstream comment's “first spell entry” phrasing because the instruction path indexes the
slot supplied by the caller.

The learned count does not reorder, compact, or mutate the stored entries. Numeric slot cardinality,
invalid selected-slot behavior, duplicate base identities, and the runtime meaning of “known” remain
outside the accepted static contract.

## `LearnSpell` Chronology

The source-reviewed mutation order is:

1. retain the incoming raw entry while deriving its symbolic base identity and level code;
2. scan existing slots for that base identity before searching for an empty slot;
3. when a matching base identity is found, compare the stored level with the incoming level;
4. if the same or a higher level is already stored, return result `1` without performing the
   empty-slot search;
5. if the incoming level is higher, replace that known entry and return result `0`;
6. only when the complete known-base scan finds no match, scan for the symbolic Nothing entry;
7. write the incoming raw entry to the source-selected empty slot and return `0`, or return `2` when
   the empty-slot scan finds no room.

The early result-`1` path is authoritative: an available empty slot does not permit a duplicate lower,
equal, or otherwise non-upgrading copy of an already-known base identity. Empty-slot search is reachable
only after no known base identity was found.

Direct source review observes the known-base scan from the source end toward the start and the later
empty-slot scan from the start toward the end. A fidelity adapter preserves that order, but this
contract does not promote malformed duplicate-base behavior into a supported gameplay rule. Runtime
partial writes, interruption, concurrent mutation, and caller-visible transactions remain **Unknown**.

The numeric results `0`, `1`, and `2` are fixture-owned static return identities. They do not prove how
any caller presents success/failure, whether a message is shown, or whether surrounding state persists.

## Adjacent Helper Separation

`GetSpellName` and `GetSpellCost` remain source operation identities in the audited file, but they do
not expand this mutable-state contract:

- the name helper's masked lookup does not establish player-facing text, localization, or asset
  licensing;
- the cost helper delegates through definition lookup and reads the source cost field, but MP
  affordability, deduction timing, enemy rules, UI display, and balance belong to other owners.

Similarly, `LearnSpell` does not define why a spell is awarded. Level-up, scripted rewards, debug
routes, or other callers must retain their own reachability and ordering evidence.

## Cross-System Separation

Spellbook storage is not the complete spell system:

- immutable name/element/definition tables remain with spell-definition data;
- combatant entry layout and generic field access remain with combatant-state access;
- new-game owns its symbolic empty initialization but not later learning outcomes;
- level-up owns accepted gain/prowess/spell-caller chronology, not the general storage service;
- targeting geometry, MP transactions, status/effect dispatch, combat resolution, battle-scene
  presentation, and AI spell choice require their own evidence;
- save/load, UI, localization, audio, accessibility, and balance remain deliberate design or separate
  runtime boundaries.

## Implementation-Neutral State Model

```text
StoredSpellEntry
  rawEntry
  derivedBaseIdentity: apply symbolic base mask only in helpers that do so
  derivedLevelCode: apply symbolic level shift only in helpers that do so

SpellbookState
  orderedSlots: source-bounded symbolic cardinality

lookupDefinition(rawQueryByte)
  compare rawQueryByte directly with definition identity bytes
  masking: none
  onMiss: firstDefinitionEntry

getSelectedEntryAndLearnedCount(selectedSlot)
  selectedEntry: orderedSlots[selectedSlot]
  learnedCount:
    count each slot whose symbolically masked base identity is not Nothing

learnSpell(incomingRawEntry)
  scan known base identities before empty slots
  if sameOrHigherKnown:
    return 1
  if knownLevelIsLowerThanIncoming:
    replace known entry with incomingRawEntry
    return 0
  if emptySlotExists:
    write incomingRawEntry to source-selected empty slot
    return 0
  return 2
```

This is a logical parity model, not a required engine memory layout. A remake may use typed entries,
validated collections, result enums, or transactions. Its fidelity adapter must preserve raw-vs-masked
helper boundaries, selected-slot/count separation, known-before-empty chronology, replacement rule,
and the three accepted result identities.

## Original Fidelity and Modernization

Original-fidelity mode preserves the five fixture-owned facts, the directly reviewed helper order,
and the representative service identity/address. It reports runtime, malformed-input, persistence, and
presentation questions rather than treating the static return codes as a complete player experience.

A remake may reject malformed entries, enforce unique base identities, expose typed failure reasons,
resize spellbooks, or use a different definition-miss policy. Those are explicit product decisions. An
original-fidelity adapter must still reproduce the accepted source boundary or record the deviation.

Public parity fixtures need structural metadata, symbolic identities, and synthetic entry values; they
do not require copyrighted spell names, descriptions, graphics, audio, or dialogue.

## H4 Acceptance Gates

A future remake spellbook adapter passes this contract only when:

1. raw stored entries remain lossless while base identity and level code are derived only in helpers
   whose accepted source paths apply the corresponding symbolic operations;
2. definition lookup compares raw `d1.b` and falls back to the first definition entry on a miss,
   without importing `SPELLENTRY_MASK_INDEX` from another helper;
3. selected-slot return and all-slot learned counting remain separate outputs, and the selected entry
   is not mislabeled as the first learned spell;
4. `LearnSpell` completes the known-base scan before empty-slot search, returns `1` early for a same or
   higher known level, replaces a lower known level with result `0`, writes an empty slot with result
   `0` only when no base match exists, and returns `2` when no room remains;
5. numeric constants/cardinality, malformed inputs, runtime reachability, persistence, acquisition
   policy, UI, localization, MP/target/effect resolution, presentation, audio, and balance remain
   separately tested or explicitly **Unknown**;
6. adjacent definition, combatant, new-game, level-up, resolution, save, and presentation contracts
   remain independently testable rather than being collapsed into this state layer;
7. public fixtures use structural metadata and synthetic values rather than copyrighted content.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| definition-miss fallback; result `0`/`1`/`2`; higher-level replacement | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Runtime callers, presentation, persistence |
| raw definition comparison without helper-imported masking | **Confirmed static source review** | pinned `spellstats.asm:GetSpellDefinitionAddress` | Invalid/corrupt table behavior and caller policy |
| caller-selected entry plus independent all-slot count | **Confirmed static source review** | pinned `spellstats.asm:GetSpellAndNumberOfSpells` | Numeric slot count and invalid selection |
| known-before-empty learning chronology | **Confirmed static source review** | pinned `spellstats.asm:LearnSpell` | Runtime mutation/transaction edges and malformed duplicates |
| acquisition, save/load, UI, localization, resolution, presentation, audio, balance | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer the complete spell experience |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
