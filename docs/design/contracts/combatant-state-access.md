# Combatant State Access Contract

- Status: **Confirmed static combatant selector, getter, mutation-wrapper, clamp-helper, distance,
  and unused type-encoding surfaces; Confirmed runtime behavior for nine bounded clamp operations**
- Evidence date: 2026-08-08
- Scope: source-shaped access to the 56-byte combatant-entry domain without assigning higher-level
  battle, roster, persistence, presentation, or balance meaning

## Judgment Boundary

This contract defines the original low-level combatant state-access boundary. It does not define
when a combatant exists, joins a party, enters a battle, acts, dies, persists, or appears to a player.

- **Confirmed**: the selector-to-entry source route and derived 56-byte stride; 31 getter entries;
  53 mutation wrappers; seven clamp helpers and their static caller inventory; nine observed clamp
  operations; the two-selector static distance function; and the separately bounded, source-marked
  unused combatant-type encoding.
- **Inferred**: caller-visible intent of the invalid-selector route only. The source writes an error
  code, disables VInt through a trap argument, and loops, but no accepted runtime owner closes what a
  caller or player observes.
- **Unknown**: caller-dependent getter and mutation outcomes outside the nine-operation matrix;
  `DecreaseAndClampWord`, both Long helpers, decrease-current-ATT, indirect helper reachability,
  selector-160 runtime use, distance edge behavior, roster and battle lifecycle, save persistence,
  UI and presentation, and balance intent.

The [party-roster contract](party-roster-state.md) owns membership and active-party commands. The
[battle-control contract](battle-control-lifecycle.md) owns battle admission, turns, cleanup, and
outcomes. [Level-up](level-up.md), [combat resolution](combat-resolution.md), and
[battlefield navigation](battlefield-navigation.md) own higher-level consumers. Those contracts may
mutate or read this state, but their lifecycle semantics are not folded into this low-level ABI.

## Evidence Owners

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the
dedicated H2 owner. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md). The owner retains complete
instruction and local-label corpora, source/H1 addresses, field-width and offset use sites, helper
ABIs, caller identities, and exact order for the bounded surfaces used here.

`sf2-stat-clamp-boundaries-v1`
([`stat-clamp-boundaries-v1.json`](../../../tests/fixtures/h3/stat-clamp-boundaries-v1.json)) is the
bounded H3 owner for one Slade/THIF level-up case containing nine controlled wrapper operations. It
does not confirm every wrapper or helper. Its existing association with the level-up contract remains
valid; this contract consumes the same fixture only for the low-level clamp outcomes it observes.

The sibling flags, party, caravan, Deals, item-inventory, name/item/spell services, new-game, and
unused-null records in the common-stats aggregate are deliberately excluded from this contract's
research-index boundary.

## Selector and Entry-Address Topology

**Confirmed static:** `GetCombatantEntryAddress` consumes the low byte of the selector and derives a
56-byte entry address. The source routes selectors as follows:

| Source predicate | Route | Address-domain consequence |
| --- | --- | --- |
| selector below `128`, at most `31` | ally | masked selector is multiplied by 56 |
| selector below `128`, above `31` | error | writes the source error state, invokes the VInt trap boundary, then self-loops |
| selector at least `128`, at most `160` under the source `bhi` comparison | enemy route | subtract `96`, mask, then multiply by 56 |
| selector above `160` | error | same static error route |

The comparison against `COMBATANT_ENEMIES_SPACE_END=160` uses `bhi`, so the accepted source-shaped
predicate admits selector `160`. That would produce adjusted slot index `64`; this contract does not
reinterpret it as a valid ordinary enemy slot. Natural reachability, allocated-state safety, and
runtime behavior for selector `160` remain **Unknown**. An implementation must preserve the source
predicate for fidelity diagnostics while exposing a separate validated application-level selector.

The address calculation masks to one byte, shifts by three twice with an intervening copy, subtracts
the first shifted value from the second, then adds the result to `COMBATANT_DATA`. This proves the
56-byte stride. It does not prove that every byte in each entry has one universal semantic view.

## Getter-Exposed Record Views

**Confirmed static:** the getter surface contains 31 ordered entries from `GetCombatantName` through
`GetDefeats` in the 586-byte interval `0x82D0..0x851A`. Ordinary byte reads clear the 16-bit result
before loading; word reads preserve a 16-bit field. X and Y getters sign-extend their stored bytes.

The following table is the getter-exposed projection, not a claim that unlisted bytes are unused:

| Offset | Width | Getter-facing view |
| ---: | ---: | --- |
| `0..9` | bounded byte sequence | ally name storage inspected by `GetCombatantName`; enemy names follow a separate enemy-index/table path |
| `10`, `11` | byte each | class, level |
| `12`, `14` | word each | maximum HP, current HP |
| `16`, `17` | byte each | maximum MP, current MP |
| `18..25` | byte each | base/current ATT, DEF, AGI, and MOV pairs |
| `26`, `28` | word each | base and current resistance |
| `30`, `31` | byte each | base and current prowess |
| `44` | word | status effects |
| `46`, `47` | signed-extended byte each | X and Y |
| `48` | byte | current EXP |
| `49` | byte | high-nibble move type and low-nibble AI commandset views |
| `50` | word | move-order high/low byte split; ally view also exposes kills here |
| `52` | word | activation bitfield |
| `54` | byte or word by getter | trigger-region nibble split; ally view also exposes defeats as a word |
| `55` | byte | enemy identity for enemy selectors |

The offset-50 and offset-54 aliases are type- and getter-dependent. A remake must not replace the
56-byte state with one flat semantic struct that makes move orders and kills, or trigger regions and
defeats, independently writable at the same time without an explicit overlay rule.

`GetCombatantName` is also nonuniform: allies use entry-local bytes with a nine-character bound,
while enemies use the stored enemy index and the separate enemy-name table. `GetEnemy` returns `-1`
on its non-enemy route and reads offset 55 on its enemy route. These are access facts, not claims
about display localization, identity lifecycle, or normal caller inputs.

## Mutation-Wrapper Surface

**Confirmed static:** the mutation surface contains 53 ordered entries from `LoadAllyName` through
`DecreaseCurrentMov` in the 1,046-byte interval `0x855A..0x8970`:

| Wrapper class | Count | Contract meaning |
| --- | ---: | --- |
| ally-name load | 1 | distinct bounded copy form |
| direct set | 27 | source-specific byte/word or packed-field writes |
| increase | 16 | wrapper supplies field offset, width, and clamp arguments to a lower helper |
| decrease | 9 | wrapper supplies field offset, width, and clamp arguments to a lower helper |

The fixture retains each wrapper's selector/value or delta width, field use site, lower-helper ABI,
register roles, preservation/terminal order, and direct/effective caller identity. Direct setters,
packed move-order and trigger-region merges, guarded kills/defeats, and current HP/MP maximum reads
remain distinct operations. They must not be normalized into an unconstrained generic setter before
their width, read-modify-write order, and guard behavior are preserved.

Static wrapper structure alone does not confirm successful mutation for every selector, caller, or
value. In particular, no global rule such as "all combatant values clamp to 0..200" can be derived
from the wrapper inventory.

## Clamp-Helper Algorithms

**Confirmed static:** seven helpers occupy the 268-byte interval `0x9312..0x941E` in this exact order:

1. `IncreaseAndClampByte`;
2. `IncreaseAndClamp7Bits`;
3. `DecreaseAndClampByte`;
4. `IncreaseAndClampWord`;
5. `DecreaseAndClampWord`;
6. `IncreaseAndClampLong`;
7. `DecreaseAndClampLong`.

Every helper receives a selector, field offset, delta/result register, and caller-supplied minimum
and maximum. The byte, word, and Long forms preserve their own operand widths and branch order; they
are not interchangeable host-language arithmetic. Increase-byte checks carry before maximum and
minimum convergence. Increase-word and Increase-Long use their source negative-result branch before
the range comparisons. Decrease forms copy the delta, read the stored field, subtract, then apply
their source-specific underflow/minimum/maximum sequence.

`IncreaseAndClamp7Bits` is a separate algorithm. It preserves the stored `0x80` bit, masks the
working value with `0x7F`, adds and clamps the low portion, ORs the preserved bit back, writes the
byte, and normalizes the returned word to `0..255`. A generic unsigned-byte clamp would lose this
state split.

The complete static caller inventory finds 25 direct sites in the mutation-wrapper file:

| Helper | Direct wrapper sites |
| --- | ---: |
| `IncreaseAndClampByte` | 10 |
| `IncreaseAndClamp7Bits` | 2 |
| `DecreaseAndClampByte` | 8 |
| `IncreaseAndClampWord` | 4 |
| `DecreaseAndClampWord` | 1 |
| `IncreaseAndClampLong` | 0 |
| `DecreaseAndClampLong` | 0 |

Zero direct sites do not prove runtime unreachability through aliases, data-driven dispatch, or
unindexed code. Both Long helpers therefore remain preserved static forms with **Unknown** runtime
use.

## Bounded Runtime Clamp Matrix

**Confirmed runtime:** the accepted single-case H3 fixture observes exactly nine operations:

| Operation family | Cases | Accepted boundary examples |
| --- | ---: | --- |
| increase byte | 4 | ATT/DEF/MOV cap at 200; byte carry from `250 + 50` also yields 200 |
| increase word | 1 | maximum HP `65535 + 2` wraps to `1` before the source branch sequence completes |
| increase seven bits | 1 | agility `227` plus `2` preserves `0x80` and produces `228` with low-seven-bit cap 100 |
| decrease byte | 3 | DEF `3-5`, MOV `1-2`, and AGI `5-10` each produce 0 |

This matrix does not observe `DecreaseAndClampWord`, either Long helper, decrease-current-ATT, all 53
wrappers, invalid selectors, or indirect reachability. Those remain explicit H3 expansion gates.

## Distance Helper Boundary

**Confirmed static:** `GetDistanceBetweenCombatants` is the 100-byte interval `0x941E..0x9482`.
It receives two 16-bit selector values, preserves `d0-d1/d3-d5`, and returns a 16-bit result in `d2`.
It obtains actor X/Y and target X/Y in order. After each getter it compares the low byte with `-1`;
any match takes the source `d2=-1` path. Otherwise it subtracts each axis, conditionally negates on
the source carry-clear/no-borrow branches, then adds the two word intermediates.

The static inventory finds two direct callers and no calls through the jump-interface alias. No H3
fixture currently observes this function. Invalid-coordinate return behavior, word-wrap boundaries,
coordinate interpretation, and caller-visible use remain **Unknown**; the source-shaped operation
must not be promoted into a universal geometry rule.

## Unused Combatant-Type Encoding

**Confirmed static:** the source-marked unused `GetCombatantType` service returns the enemy index for
an enemy selector. For an ally it sets bit 15 and combines class type, ally-count scaling, and ally
index. The raw encoding is a separately preserved compatibility surface.

The upstream unused label and lack of a runtime owner mean normal reachability and gameplay meaning
are **Unknown**. A remake may omit this value from ordinary domain APIs, but an original-fidelity
adapter must retain a reproducible encoder rather than reassigning its bits.

## Implementation-Neutral State Model

```text
CombatantSelector
  rawValue
  sourceRoute: ally | enemy | error
  sourceAdjustedIndex
  applicationValidated: boolean

CombatantEntry
  rawBytes[56]
  fieldViews[]
  allyOverlay
  enemyOverlay

FieldView
  name
  offset
  widthBits
  signednessOrPacking
  applicableOverlay

GetterDefinition
  sourceIdentity
  selectorRule
  fieldViewOrCustomPath
  returnShape

MutationDefinition
  sourceIdentity
  selectorRule
  fieldView
  directOrReadModifyWriteOrder
  lowerHelperRef
  guardAndReturnShape

ClampDefinition
  sourceIdentity
  operandWidth
  orderedArithmeticAndBranches
  preservedBits
  observedCases[]

DistanceDefinition
  selectorOrder
  coordinateReadOrder
  invalidComparison
  orderedAxisOperations
  returnShape
```

This is a logical parity model, not a required engine memory layout. The raw 56-byte entry and typed
views coexist so aliased offsets, packed fields, signed coordinate reads, and source-specific mutation
order remain diagnosable. Application code may expose safer typed state, but the compatibility layer
must not erase the distinction between source admission and application validation.

## Original Fidelity and Modernization

Original-fidelity mode preserves selector routing, the 56-byte stride, getter/mutation identities,
field widths and overlays, clamp branch order, the nine observed results, distance-operation order,
and the unused type encoding. It reports unobserved routes rather than silently assigning behavior.

A modern engine may use entity handles, separate ally/enemy components, explicit option/error values,
saturating numeric types, safer coordinate APIs, and event-sourced mutations. Those are legitimate
implementation choices only when an adapter can reproduce the accepted original-facing observations
and intentional deviations are recorded separately.

Player names and other original content referenced by runtime entries remain private input. Public
fixtures and this contract retain structural metadata, identifiers, counts, and bounded observations,
not distributable original content.

## H4 Acceptance Gates

A future remake combatant-state adapter passes this contract only when:

1. ally, enemy, gap/error, and selector-160 source predicates remain reproducible separately from
   safer application validation;
2. the derived 56-byte stride and all getter-exposed offsets, widths, signed reads, packed views, and
   ally/enemy overlays round-trip without flattening collisions;
3. all 31 getter and 53 mutation-wrapper identities remain traceable to their source order, custom
   path or field view, helper boundary, guards, and return shape;
4. all seven clamp helpers preserve exact operand widths and ordered branch/write behavior, including
   the seven-bit `0x80` preservation rule and zero-direct-site Long helpers;
5. the nine accepted H3 operations reproduce their exact before/amount/cap/after results without
   being generalized to unobserved helpers or wrappers;
6. the two-selector distance helper preserves read order, byte-sized `-1` tests, word arithmetic,
   and return shape while unobserved runtime edges remain reported as Unknown;
7. the unused combatant-type encoder remains reproducible without being treated as a required
   player-facing domain identity;
8. battle/roster lifecycle, persistence, UI, presentation, balance, and modernization are tested by
   their own owners or recorded as deliberate deviations.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| selector route, 56-byte stride, 31 getters, 53 wrappers | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Caller-visible invalid-selector behavior and complete runtime outcomes |
| seven clamp helpers and 25 direct wrapper sites | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Indirect reachability and zero-site helper use |
| nine bounded clamp operations | **Confirmed runtime** | `sf2-stat-clamp-boundaries-v1` ([`stat-clamp-boundaries-v1.json`](../../../tests/fixtures/h3/stat-clamp-boundaries-v1.json)) | Decrease-word, Long helpers, decrease-current-ATT, other wrappers/selectors |
| two-selector distance operation and two direct callers | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Runtime edges, coordinate meaning, caller-visible use |
| source-marked unused combatant-type encoding | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Natural reachability and gameplay meaning |
| membership, battle lifecycle, persistence, UI, presentation, balance | **Separate owner / Unknown** | Adjacent contracts and future H3/synthesis work | Do not infer higher-level meaning from access APIs |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 h3 stat-clamps
uv run sf2 design-contracts test
uv run sf2 verify
```
