# Name-Table Lookup Service Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded entry identities, length-prefixed traversal,
  output shape, and direct caller inventory described below
- Modernization: **Allowed** for engine-native indexed resources behind a private import-equivalence
  adapter
- Unknown: malformed or out-of-range indexes, runtime admission, caller-visible register/CCR
  dependence beyond the stated word preservation, encoding, localization, rendering, timing, and
  player-visible meaning

## Purpose

This contract defines the smallest implementation-neutral lookup service supported by the accepted
`findname.asm` evidence. It preserves the original length-prefixed row traversal and the
`GetClassName` frontend without taking ownership of any class, item, spell, ally, or enemy name
content.

The source service returns a pointer into a caller-selected private table and the selected row's
length. A remake may instead return an engine-native resource reference and length, provided a
private compatibility adapter can reproduce the accepted indexed result. The original table bytes,
addresses, pointers, and strings are not distributable contract payloads.

## Judgment Boundary

**Confirmed static:** the accepted common-stats fixture binds `GetClassName` to ROM address
`0x8970` (`35184`) and identifies `findname.asm` as its source. The H1 listing places the six-byte
frontend at `0x8970..0x8976`, followed immediately by `FindName` at `0x8976`; the annotated source
interval ends at `0x898E`. `GetClassName` loads `p_table_ClassNames` into `a0` and falls through into
`FindName`.

Within a caller-owned valid table and an admitted index in `0..rowCount-1`, `FindName` treats each
row as one unsigned length byte followed by that many payload bytes. It skips exactly the requested
number of preceding rows, returns `a0` immediately after the selected row's length byte, and clears
then loads that byte into `d7.w`. The source saves and restores exactly `d0.w` through one balanced
two-byte stack slot.

The accepted complete common-stats source inventory has exactly three direct `FindName` call sites:
one each in `combatantstats_1.asm`, `itemstats.asm`, and `spellstats.asm`. The `GetClassName`
fallthrough is a distinct frontend and is not counted as a fourth direct call.

**Inferred:** none. Source identities such as “class,” “item,” “spell,” and “name” describe the
original storage/caller vocabulary. They do not establish player-facing meaning, a localization
policy, or a required public API name.

**Unknown or excluded:** negative, out-of-range, wrapping, or otherwise malformed indexes;
truncated rows; missing length bytes; invalid pointers; cross-address-space behavior; caller
validation; runtime reachability and result use; caller-visible `d1`, CCR, or register behavior beyond
the stated `d0.w` save/restore; text encoding; glyph selection; localization; rendering; window
layout; timing; persistence; and replacement-content policy.

## Evidence Contract

This contract consumes only the following public surface from
[`sf2-common-stats-static-v1`](../../../tests/fixtures/h2/common-stats-static-v1.json):

- `function.classNameAddress`;
- `expected.representativeSymbols["findname.asm"]`;
- `upstreamCommit` and `romSha256` provenance.

The bounded chronology is reviewed directly in pinned
[`findname.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/findname.asm).
The owning [common-stats research](../../research/common-stats.md), executable
[`stats.py`](../../../src/sf2tool/h2/stats.py), and extraction
[`manifest`](../../../manifests/extractions/common-stats-static.json) retain the complete source
inventory, accepted output digest, and provenance. The generated inventory establishes the three
direct caller occurrences without publishing any name-table payload.

This contract does **not** consume any `expected.statsFacts` subtree. In particular, it does not
consume combatant getter/mutation/clamp/distance behavior, party, flag, Caravan, Deals, spell,
new-game, combatant-type, or inventory facts merely because they share the aggregate fixture.

The accepted evidence resolves the representative H1 entry and pinned source chronology under the
accepted ROM identity. It does not establish byte-for-byte H1/ROM instruction parity for the complete
function body. A future private instruction-byte comparison would be stronger evidence, not a
current H4 requirement.

### Exact research-index denominator

The accepted fixture is linked directly to twelve research records. This contract changes exactly
one future semantic association:

| Record | Design ownership after this contract |
| --- | --- |
| `stats.names` | this contract; currently unassociated before registration |
| `stats.caravan` | unchanged: `caravan-and-deals-state` |
| `stats.deals` | unchanged: `caravan-and-deals-state` |
| `stats.combatant-setters` | unchanged: `combatant-state-access` |
| `stats.combatant-type` | unchanged: `combatant-state-access` |
| `stats.flags` | unchanged: `global-flag-state` |
| `stats.new-game` | unchanged: `new-game-state-initialization` |
| `stats.party` | unchanged: `party-membership-state` |
| `stats.spell-stats` | unchanged: `spellbook-state` |
| `stats.item-inventory` | remains unassociated and outside this contract |
| `stats.item-stats` | remains unassociated and outside this contract |
| `stats.unused-null` | remains unassociated and outside this contract |

Sharing the common-stats fixture does not transfer any sibling fact, evidence subtree, or design
association to this service.

## Original Static Service

### Frontend and entry identities

The source interval contains two distinct identities:

| Identity | H1 address | Accepted role |
| --- | ---: | --- |
| `GetClassName` | `0x8970` (`35184`) | load the private class-name table pointer and fall through |
| `FindName` | `0x8976` (`35190`) | traverse the caller-selected length-prefixed table |

`GetClassName` has no separate return between its pointer load and `FindName`. This fallthrough is a
source control-flow fact. It does not make this service the owner of the class table, class identity
domain, or class display content.

### Admitted lookup domain

The common result contract is intentionally bounded to:

- a valid caller-owned ordered table;
- rows encoded privately as one length byte plus exactly that many payload bytes;
- a nonnegative index in `0..rowCount-1`;
- non-wrapping readable storage covering every skipped row and the selected row.

The source contains no local row-count argument or bounds check. Inputs outside this domain are not
normalized into a modern API guarantee. A remake may reject them, but that rejection is a documented
modern safety policy rather than Confirmed original behavior.

### Traversal chronology

For an admitted index `n`, the source-shaped chronology is:

1. save `d0.w` in one two-byte stack slot;
2. subtract one from `d1.w`;
3. when the result is negative, skip the row-scan loop, selecting row zero;
4. otherwise clear `d0.w`, read the current row length byte into `d0.b`, advance `a0` past the
   length byte, add the zero-extended length to `a0`, and repeat with `dbf` until exactly `n`
   preceding rows have been skipped;
5. clear `d7.w`, read the selected row length byte into `d7.b`, and advance `a0` once;
6. restore `d0.w` and return.

The resulting abstract relation is:

| Admitted input | Rows skipped | Logical output |
| --- | ---: | --- |
| index `0` | `0` | first row payload reference and first row length |
| index `1` | `1` | second row payload reference and second row length |
| index `n` | `n` | row `n` payload reference and row `n` length |

The source zero-extends each length byte before using or returning it. This is a storage and lookup
fact, not an assertion about character encoding, glyph count, displayed width, or localized length.

### Register and stack boundary

The source explicitly saves and restores the low word `d0.w`; it does not save a full longword copy
on the stack. This contract therefore states the source operation exactly and does not promote it to
an all-register ABI claim. The stack decrement and matching word restore are balanced on the admitted
return path.

`a0` and `d7.w` are outputs. `d1.w` is traversal state rather than a preserved input. CCR state and
any caller dependence on unlisted register portions remain outside the contract.

## Direct Caller Separation

The complete accepted common-stats source inventory contains these three direct `FindName` sites:

| Caller source | Caller-owned preparation | Separate design owner |
| --- | --- | --- |
| `combatantstats_1.asm` | enemy selection and enemy-name table choice | [`combatant-state-access`](combatant-state-access.md) and [`enemy-definition-data`](enemy-definition-data.md) |
| `itemstats.asm` | item-entry masking and item-name table choice | [`item-definition-data`](item-definition-data.md); broader item-service behavior remains separate |
| `spellstats.asm` | spell-entry masking and spell-name table choice | [`spellbook-state`](spellbook-state.md) and [`spell-definition-data`](spell-definition-data.md) |

The class-table frontend consumes the table owned by
[`ally-definition-data`](ally-definition-data.md). This contract does not duplicate any caller's
masking, selector, table cardinality, content fidelity, runtime admission, or display behavior.

## Implementation-Neutral Model

A conforming private import may use this logical shape:

```text
NameTableLookupEvidence
  identity
    getClassNameSymbol
    getClassNameH1Address = 0x8970
    findNameSymbol
    findNameH1Address = 0x8976
    sourcePath
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  privateTable
    orderedRows[]
      privatePayloadBytes
      byteLength

  admittedLookup
    validIndexRange = 0..rowCount-1
    resultPayloadRef
    resultByteLength

  sourceBoundary
    classFrontendFallsThrough
    d0WordSaveRestore
    balancedWordStackSlot
    directCallerOccurrences[3]
```

This is an evidence/import model, not a required engine class layout. Complete table contents,
original strings, raw source bodies, exact pointer values, complete addresses, instruction bytes, and
private ROM excerpts remain private verification inputs. The bounded symbols, two indexed entry
addresses, source path, row-format rule, direct caller summaries, hashes, and provenance are public
metadata.

After verifying the pinned-source chronology and H1 identities under the fixture's accepted ROM
provenance, a conforming remake may use arrays, localized resource IDs, immutable string tables, or a
typed lookup service. It need not reproduce Mega Drive addresses, big-endian pointer storage,
length-prefixed runtime memory, `dbf`, register allocation, or the original stack operations.

## Public and Private Projection

The public contract may retain:

- fixture ID, source path, source symbols, and the two bounded H1 entry addresses;
- the length-byte-plus-payload row rule and admitted valid-index domain;
- the abstract index-to-payload-reference-and-byte-length relation;
- the three caller source identities and their separation boundaries;
- upstream revision, accepted ROM identity/hash, output digest, and reproduction command;
- project-authored synthetic lookup cases containing no original names.

The public form MUST NOT expose original class, item, spell, ally, or enemy names; complete name-table
bytes; private row hashes; original pointer values; raw source or ROM bodies; emulator memory; or
localized/captured text. Original-compatible content may be imported and verified privately, while a
distributable remake uses replacement or separately cleared text resources.

## H4 Acceptance Surface

A future adapter satisfies this contract when:

1. the accepted fixture identity, source path, upstream commit, ROM SHA provenance, `GetClassName`
   address, and source-local `FindName` identity remain traceable;
2. the private importer preserves ordered length-prefixed row boundaries without publishing original
   payloads;
3. a valid index `n` selects exactly row `n`, returns a reference to its payload rather than its
   length byte, and returns the row's zero-extended byte length;
4. project-authored synthetic cases cover index zero, index one, and a later valid index, including
   rows with different lengths;
5. source-compatibility diagnostics retain the `GetClassName` pointer-load/fallthrough identity and
   the exact `d0.w` save/restore plus balanced word-stack boundary without claiming all-register or
   CCR preservation;
6. the three direct caller occurrences remain traceable while their masks, selectors, table content,
   and runtime meaning remain with their separate owners;
7. an engine-native implementation may return typed resource references and need not reproduce the
   original loop, pointers, address space, byte order, registers, or memory layout;
8. malformed/out-of-range behavior, encoding, localization, rendering, timing, and presentation are
   either rejected as outside this contract or covered by separate evidence/design policy;
9. public reports remain metadata-only and respect the private/copyright boundary.

H4 MUST NOT require original strings or tables in a public fixture. It tests the accepted relation
with synthetic rows and a private parity adapter when original-compatible inputs are available.

## Cross-System Separation

- [`ally-definition-data`](ally-definition-data.md), [`item-definition-data`](item-definition-data.md),
  [`spell-definition-data`](spell-definition-data.md), and
  [`enemy-definition-data`](enemy-definition-data.md) retain canonical table identities, order,
  cardinalities, and private content.
- [`combatant-state-access`](combatant-state-access.md) retains ally/enemy name selection and its
  bounded `GetCombatantName` behavior.
- [`spellbook-state`](spellbook-state.md) retains spell-entry masking and spell lookup caller
  chronology. Item-entry masking and inventory/equipment behavior remain outside this contract.
- [`text-and-font-system`](text-and-font-system.md) and
  [`dialogue-system`](dialogue-system.md) retain encoding, glyphs, text resources, rendering,
  dialogue substitution, windows, timing, and player-visible presentation.
- Party, flags, Caravan, Deals, combatant mutation, new-game, inventory, and unused-null siblings in
  the aggregate common-stats fixture retain their current owners or unassociated state.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| `GetClassName` identity/address and `findname.asm` representative source | **Confirmed static** | `sf2-common-stats-static-v1` | complete function-body byte parity |
| `GetClassName` pointer load/fallthrough and `FindName` traversal/output chronology | **Confirmed static source/H1** | pinned `findname.asm`, H1 listing, and common-stats owner | malformed indexes, runtime caller dependence |
| exactly three direct `FindName` caller occurrences | **Confirmed static inventory** | common-stats generated inventory and accepted digest | indirect/runtime reachability |
| class/item/spell/enemy table content and caller-specific selection | **Separate owners** | definition/state contracts listed above | replacement/localization policy |
| encoding, glyphs, windows, dialogue, visible length, presentation | **Unknown / separate owners** | text/font, dialogue, and presentation contracts | runtime and player-visible outcomes |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated inventories remain under ignored `local/derived/`. No original name table, string payload,
source-body dump, ROM excerpt, emulator state, or other private/generated artifact belongs in the
public contract.
