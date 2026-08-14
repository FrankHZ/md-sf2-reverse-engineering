# Stats Null-Return Service Contract

- Status: **Draft evidence-bound contract**
- Original fidelity: **Confirmed static** for the bounded source/file/entry identity and sole
  immediate-return instruction described below
- Modernization: **Allowed** to erase, inline, or retain an engine-native no-op compatibility seam
- Unknown: natural reachability, caller admission, runtime result, original stack/register/CCR
  behavior, timing, gameplay meaning, and whether a remake needs a callable endpoint

## Purpose

This contract preserves the smallest complete source owner in the accepted common-stats inventory:
`unusedsub_9482.asm`. The file contains one global entry, `nullsub_9482`, and one parsed statement,
`rts`. The contract records that source identity without treating the upstream word “unused” as proof
of dead code or universal runtime unreachability.

The evidence is useful as a provenance and compatibility boundary. It closes the two-byte interval
between the combatant-distance helper and the level-up source, and it lets a future private fidelity
adapter explain why an original entry exists even if a modern engine chooses not to expose it.

## Judgment Boundary

**Confirmed static:** `sf2-common-stats-static-v1` binds `nullsub_9482` to ROM address `0x9482`
(`38018`) and identifies `unusedsub_9482.asm` as its representative source. The pinned source and H1
listing bound the entry to `0x9482..0x9484`, with the sole body instruction identity `rts` at
`0x9482`. The accepted source inventory records 13 source lines, one parsed statement, one global
label, zero local labels, zero outgoing direct calls, and zero indirect call sites for the file.

The interval boundaries are exact and non-overlapping: `GetDistanceBetweenCombatants` ends at the
exclusive address `0x9482`; `nullsub_9482` occupies `0x9482..0x9484`; the next included `LevelUp`
source begins at `0x9484`.

**Inferred:** the source filename, symbol, and comment classify the entry as an unused null
subroutine. The one-instruction body is compatible with an engineering no-op return seam. These are
source vocabulary and engineering interpretation only, not observed runtime reachability or intent.

**Unknown or excluded:** natural or debug reachability; indirect calls or address-table use; caller
admission; why the entry and its jump-interface witness exist; observed runtime execution and result;
caller-visible stack, return-address, register, or CCR behavior; invalid return contexts; interrupts,
cycle counts, and timing; persistence; gameplay, UI, presentation, or accessibility meaning; and
whether a remake requires any public or runtime endpoint corresponding to this source identity.

## Evidence Contract

This contract consumes only the following public surface from
[`sf2-common-stats-static-v1`](../../../tests/fixtures/h2/common-stats-static-v1.json):

- `function.unusedAddress`;
- `expected.representativeSymbols["unusedsub_9482.asm"]`;
- `upstreamCommit` and `romSha256` provenance.

The owning [common-stats research](../../research/common-stats.md), executable
[`stats.py`](../../../src/sf2tool/h2/stats.py), and extraction
[`manifest`](../../../manifests/extractions/common-stats-static.json) retain the complete source
inventory and accepted output digest. The digest-bound generated row for
`code/common/stats/unusedsub_9482.asm` records:

| Inventory field | Accepted value |
| --- | ---: |
| source SHA-256 | `1E729E16223F79D95DB1B77D4FB5E0C369E4DF52CF8B687FB85C31F1FA97EF7A` |
| source lines | `13` |
| parsed statements | `1` |
| global labels | `1` |
| local labels | `0` |
| outgoing direct calls | `0` |
| indirect call sites | `0` |

The bounded source shape is reviewed directly in pinned
[`unusedsub_9482.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/unusedsub_9482.asm).
The accepted H1 listing independently resolves the entry and the next exclusive address. This
contract does not claim byte-for-byte H1/ROM parity for the instruction body or publish the encoded
instruction bytes.

This contract consumes no `expected.statsFacts` subtree. Party, flags, Caravan, Deals, combatant
access, name lookup, spells, new-game, item inventory, item stats, and other sibling behavior remain
outside this source/file boundary.

### Exact research-index denominator

The accepted fixture is linked directly to twelve research records. This contract changes exactly
one future semantic association:

| Record | Design ownership after this contract |
| --- | --- |
| `stats.unused-null` | this contract; currently unassociated before registration |
| `stats.caravan` | unchanged: `caravan-and-deals-state` |
| `stats.deals` | unchanged: `caravan-and-deals-state` |
| `stats.combatant-setters` | unchanged: `combatant-state-access` |
| `stats.combatant-type` | unchanged: `combatant-state-access` |
| `stats.flags` | unchanged: `global-flag-state` |
| `stats.names` | unchanged: `name-table-lookup-service` |
| `stats.new-game` | unchanged: `new-game-state-initialization` |
| `stats.party` | unchanged: `party-membership-state` |
| `stats.spell-stats` | unchanged: `spellbook-state` |
| `stats.item-inventory` | remains unassociated and outside this contract |
| `stats.item-stats` | remains unassociated and outside this contract |

Sharing the aggregate common-stats fixture does not transfer any sibling fact or association to this
contract.

## Static Source Boundary

### Complete owner file

The complete source body has this bounded shape:

```text
entry nullsub_9482 at 0x9482
  immediate-return instruction identity
exclusive end at 0x9484
```

“Immediate return” is a static source identity in this contract. It means there is no separate
source statement, local branch, explicit domain-memory access, or outgoing call before `rts`. It does
not mean that the routine was invoked in an accepted runtime case, that its caller supplied a valid
return context, or that all architectural state is unchanged.

The generated row's zero outgoing-call count describes calls made by this file. It says nothing
about incoming calls, jump stubs, indirect reachability, or address-table references.

### Adjacent source intervals

The entry is also a durable ownership boundary:

| Interval | Owner boundary |
| --- | --- |
| ending at `0x9482` | `GetDistanceBetweenCombatants`, owned by [`combatant-state-access`](combatant-state-access.md) |
| `0x9482..0x9484` | `nullsub_9482`, owned by this contract |
| beginning at `0x9484` | `LevelUp`, owned by the [`level-up`](level-up.md) contract and progression evidence |

The shared boundary does not extend the combatant-distance ABI into the null entry and does not make
the null entry part of level-up behavior.

### Separate jump-interface witness

Pinned `s02_jumpinterface.asm` contains `j_nullsub_9482` at `0x81F4`, whose source body jumps to
`nullsub_9482`. The [technical-interface research](../../research/technical-interfaces.md) retains
that aggregate interface source. Its presence is an external source witness, not evidence that a
normal caller invokes the entry.

This contract does **not** consume `sf2-tech-interfaces-static-v1`, does not own the S02 interface,
and does not associate `tech.interfaces.jump-s02`. That record remains unchanged and unassociated.
The witness is recorded precisely to prevent “unused” from being rewritten as “no exported seam.”

## Implementation-Neutral Model

A private evidence model may retain:

```text
StatsNullReturnEvidence
  identity
    sourceSymbol = nullsub_9482
    sourcePath
    h1EntryAddress = 0x9482
    exclusiveEndAddress = 0x9484
    sourceSha256
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  staticBody
    statementCount = 1
    instructionIdentity = IMMEDIATE_RETURN
    localLabelCount = 0
    outgoingDirectCallCount = 0
    indirectCallSiteCount = 0

  separateWitness
    sourceSymbol = j_nullsub_9482
    sourceAddress = 0x81F4
    consumedFixture = NONE
```

This notation is a provenance model, not a required engine type or callable interface. A remake may
erase the entry, inline it as no work, or retain a typed no-op compatibility endpoint. If a private
compatibility adapter exposes such an endpoint, project-authored synthetic state may verify that the
adapter performs no domain-state mutation before returning.

That abstract check does not reproduce or assert original stack movement, return-address reads,
register preservation, CCR behavior, instruction encoding, cycle timing, or invalid-call behavior.
Those architectural/runtime questions remain outside this static evidence contract.

## Public and Private Projection

The public contract may retain:

- fixture ID and accepted provenance;
- source path and source SHA-256;
- `nullsub_9482`, entry `0x9482`, and exclusive end `0x9484`;
- the sole immediate-return instruction identity without its encoded bytes;
- the bounded source inventory counts;
- the separate `j_nullsub_9482` witness identity/address and explicit non-consumption boundary.

The public form MUST NOT publish raw source/H1/ROM bodies, encoded instruction bytes, stack or memory
captures, emulator traces, or private ROM excerpts. This slice owns no original text, graphics,
audio, map, or other content payload.

## Original Fidelity and Modernization

Original-fidelity tooling preserves the source/file/entry identity, interval, one-statement shape,
and provenance. It reports the upstream “unused” classification without promoting it to a proven
runtime fact.

A modern engine is not required to allocate an address or callable service for an entry with no
accepted consumer contract. Retaining a named no-op adapter for diagnostics is equally allowed.
Whichever choice is made, it is a modernization/architecture decision and MUST NOT be described as
evidence that the original entry was reachable or unreachable.

## H4 Acceptance Surface

A future adapter satisfies this contract when:

1. the fixture identity, source path/SHA, upstream commit, accepted ROM provenance, entry address,
   and exclusive end remain traceable;
2. the imported static model contains exactly one global entry, no local labels, one immediate-return
   statement, no outgoing direct calls, and no indirect call sites for the complete owner file;
3. the `0x9482` and `0x9484` boundaries remain disjoint from the adjacent combatant-distance and
   level-up owners;
4. an optional engine-native compatibility endpoint may pass a project-authored synthetic no-domain-
   mutation check without reproducing 68000 call/return mechanics;
5. omission, erasure, or inlining of the runtime endpoint remains allowed until a separate accepted
   caller contract requires it;
6. no test claims dead-code status, natural unreachability, caller admission, all-register/CCR
   neutrality, stack equivalence, timing, or invalid-return behavior;
7. `j_nullsub_9482` remains a separate interface witness and neither its fixture nor
   `tech.interfaces.jump-s02` is consumed or associated here;
8. public output remains bounded metadata and contains no raw source, listing, ROM, or runtime dump.

H4 does not need original copyrighted payloads. Synthetic adapter state is sufficient for the
optional logical no-op check.

## Cross-System Separation

- [`combatant-state-access`](combatant-state-access.md) ends its distance helper at `0x9482` and
  retains combatant getters, setters, clamps, distance, and type encoding. This contract adds no
  combatant ABI.
- [`level-up`](level-up.md) and progression owners retain all behavior beginning at `0x9484`.
- [Technical-interface research](../../research/technical-interfaces.md) retains the S02 jump table
  and `j_nullsub_9482` witness. No technical-interface fixture is registered here.
- [`graphics-service-state`](graphics-service-state.md),
  [`debug-control-flow`](debug-control-flow.md), scripting owners, and battle owners retain their own
  nominally-unused or null entries. Similar labels do not create shared behavior or ownership.
- `stats.item-inventory` and `stats.item-stats` remain unassociated. Their large source files and
  caller-dependent behavior are not implied by this complete one-statement slice.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| `nullsub_9482` identity/address and representative source | **Confirmed static** | `sf2-common-stats-static-v1` | body byte parity and runtime invocation |
| exact `0x9482..0x9484` interval and sole immediate-return statement | **Confirmed static source/H1** | pinned source, H1 listing, and common-stats owner | stack/register/CCR/runtime behavior |
| 13/1/1/0/0/0 source-inventory counts and source SHA | **Confirmed static inventory** | digest-bound common-stats generated row | incoming and indirect reachability |
| “unused null subroutine” engineering classification | **Inferred** | upstream filename, symbol, and comment | dead-code status and design intent |
| `j_nullsub_9482` exported interface witness | **Separate-owner static source** | technical-interface source/research | caller admission and runtime use |
| gameplay, persistence, UI, presentation, timing | **Unknown / separate owners** | future caller/runtime evidence | complete observed outcome |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```

Generated inventories remain under ignored `local/derived/`. No source-body dump, listing dump, ROM
excerpt, runtime capture, or other private/generated artifact belongs in the public contract.
