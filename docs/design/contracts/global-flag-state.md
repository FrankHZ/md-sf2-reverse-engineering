# Global Flag State Contract

- Status: **Confirmed static flag-addressing shape and bounded flag-trap inventory**
- Evidence date: 2026-08-08
- Scope: implementation-neutral storage and operation identities for the original global flag state,
  without assigning campaign, persistence, caller, presentation, or balance meaning

## Judgment Boundary

This contract defines the lowest accepted global-flag boundary. It does not define what an individual
flag means, when a story or battle path reads it, whether it survives a save/load cycle, or what a
player sees when its value changes.

- **Confirmed**: flag indexes pass through an accepted masking step; eight flags share each byte;
  bit selection begins at bit 7; Check, Set, and Clear share the same `GetFlag` resolution path; the
  interrupt owner reports four flag-trap wrappers, identifies `Trap4_CheckFlag` at address `5888`,
  and groups traps 1 through 4 as wrappers for Check, Set, and Clear operations.
- **Inferred**: none. Higher-level intent is deliberately not inferred from the storage and wrapper
  shapes.
- **Unknown**: the exact usable flag-domain size beyond the accepted masking fact; names and meanings
  for individual flags; natural and runtime caller reachability; save/load persistence; ordering
  across systems; caller-visible results or condition-code use; inline trap-operand decoding;
  return-address movement; UI and presentation; debugging routes; and balance or campaign intent.

The [save-system contract](save-system.md) owns accepted save structures and actions, not global-flag
persistence unless an evidence owner closes that join. The
[story-progression synthesis](../synthesis/story-progression.md) may explain accepted consumers, but
it must not turn this low-level storage contract into a story-state taxonomy.

## Evidence Owners

`sf2-common-stats-static-v1`
([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) is the
dedicated H2 owner for the flag-addressing facts. Its verifier is
[`stats.py`](../../../src/sf2tool/h2/stats.py), and its source-backed explanation is
[Common Stats and Inventory Services](../../research/common-stats.md). This contract consumes only
the owner's `flags` facts: masked index, eight bits per byte, bit-7-first selection, and the shared
`GetFlag` resolution path.

`sf2-tech-interrupts-static-v1`
([`tech-interrupts-static-v1.json`](../../../tests/fixtures/h2/tech-interrupts-static-v1.json)) is the
dedicated H2 owner for the bounded trap inventory. Its verifier is
[`interrupts.py`](../../../src/sf2tool/h2/interrupts.py), and its source-backed explanation is
[Technical Interrupts](../../research/technical-interrupts.md). This contract consumes only
`flagTrapCount=4`, the representative `Trap4_CheckFlag` identity/address, and the owner prose's
Check/Set/Clear wrapper grouping.

The common-scripting and battle-functions aggregates are deliberately excluded. Their queued owner
corrections are neither evidence dependencies nor merge dependencies for this contract. Map-script,
battle, menu, and debug consumers also remain outside the research-index association boundary.

## Flag Reference Resolution

**Confirmed static:** the accepted storage resolver applies a mask to the incoming flag index before
selecting storage. It then addresses a domain with eight flags per byte and begins bit selection at
bit 7. Check, Set, and Clear all share this resolver.

The executable owner does not expose the numeric mask value or a complete flag-storage cardinality.
This contract therefore requires the normalization step and byte/bit topology without inventing a
maximum flag ID, byte-array length, or set of valid semantic names.

An import or compatibility adapter must retain both the caller-supplied index and the normalized
storage reference. Collapsing them into one unchecked application boolean would hide aliasing caused
by the accepted mask and make source-facing diagnostics impossible.

| Resolver property | Accepted contract | Deliberate boundary |
| --- | --- | --- |
| incoming identity | preserve the raw flag index | caller provenance and semantic name are **Unknown** |
| normalization | apply the accepted flag-index mask before addressing | numeric mask and usable domain size are **Unknown** |
| byte selection | eight flag positions share one byte | total byte count is **Unknown** |
| bit selection | begin at bit 7 within the selected byte | no higher-level priority or meaning is implied |
| operation reuse | Check, Set, and Clear share `GetFlag` resolution | caller-visible result and condition semantics are **Unknown** |

## Operation Identity Boundary

**Confirmed static:** Check, Set, and Clear are distinct operation identities that converge on the
same reference-resolution path. A fidelity-facing representation must preserve the requested
operation kind and the resolved byte/bit reference separately.

This is not a runtime transaction contract. The accepted owners do not close interleaving,
concurrency, interruption, rollback, persistence, or notification behavior. They also do not prove
how callers consume a Check result. A modern implementation may provide typed query and mutation
APIs, but it must not silently replace the three original-facing identities with a single toggle or
assign new caller-visible semantics to the wrapper layer.

## Flag-Trap Inventory Boundary

**Confirmed static:** the interrupt owner reports `flagTrapCount=4`. It identifies
`Trap4_CheckFlag` at decimal address `5888` and describes traps 1 through 4 as wrappers around flag
Check, Set, and Clear operations. These are inventory and grouping facts only.

This contract does not decode inline operands, specify how any trap changes a saved return address,
map every trap number to one exact operation, define returned values or condition codes, or claim
runtime reachability. Those details remain **Unknown** until a dedicated static or runtime owner
accepts them. The representative Trap 4 identity must not be generalized into a complete four-entry
ABI table.

## Implementation-Neutral State Model

```text
FlagStore
  storageBytes[]

FlagReference
  rawIndex
  normalizedIndex
  byteIndex
  bitMask

FlagOperation
  kind: check | set | clear
  reference: FlagReference

FlagTrapInventory
  wrapperCount: 4
  representativeIdentity: Trap4_CheckFlag
  representativeAddress: 5888
  groupedOperationKinds: check | set | clear
```

This is a logical parity model, not a required engine memory layout. `storageBytes` intentionally has
no contract-level length, and the derivation of `normalizedIndex` intentionally has no invented
numeric mask. The separate raw and normalized indexes preserve the accepted addressing boundary
without assigning validity or story meaning.

The trap inventory is metadata, not an executable ABI specification. A remake may implement ordinary
gameplay consumers without machine traps while retaining this inventory in an original-fidelity
adapter or diagnostic layer.

## Original Fidelity and Modernization

Original-fidelity mode preserves the masked-index step, eight-flags-per-byte topology, bit-7-first
selection, shared resolver, three operation identities, and bounded trap inventory. Unknown domains
and caller behavior remain visible rather than being filled with guessed names or lifecycles.

A modern engine may expose named flags, typed campaign facts, immutable queries, explicit mutation
commands, event logs, and a separate persistence policy. Those are legitimate design choices only
when their mapping to raw and normalized original-facing references is explicit and intentional
deviations are recorded separately.

Original flag names, dialogue, and other copyrighted content are not needed for this public contract.
Public fixtures retain structural metadata, identities, counts, and addresses only.

## H4 Acceptance Gates

A future remake global-flag adapter passes this contract only when:

1. the caller-supplied flag index and its normalized storage reference remain separately observable;
2. the accepted mask-before-addressing step is preserved without substituting an invented domain
   size;
3. eight flag positions share each byte and bit selection begins at bit 7;
4. Check, Set, and Clear remain distinct operation identities that use one shared resolution path;
5. the trap inventory preserves the count of four, representative `Trap4_CheckFlag` identity/address,
   and bounded Check/Set/Clear grouping without inventing a complete trap ABI;
6. inline operand decoding, return-address movement, caller-visible result/condition behavior,
   runtime reachability, persistence, presentation, and campaign meaning remain separately tested or
   explicitly **Unknown**;
7. public parity artifacts contain structural metadata rather than original copyrighted content.

## Evidence Matrix

| Contract area | Evidence label | Executable owner | Remaining boundary |
| --- | --- | --- | --- |
| masked flag index, eight-per-byte storage, bit-7-first selection | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Numeric mask, total capacity, semantic flag domain |
| shared Check/Set/Clear reference resolution | **Confirmed static** | `sf2-common-stats-static-v1` ([`common-stats-static-v1.json`](../../../tests/fixtures/h2/common-stats-static-v1.json)) | Runtime outcomes, ordering, caller-visible Check semantics |
| four flag traps and representative `Trap4_CheckFlag` at `5888` | **Confirmed static inventory** | `sf2-tech-interrupts-static-v1` ([`tech-interrupts-static-v1.json`](../../../tests/fixtures/h2/tech-interrupts-static-v1.json)) | Complete per-trap mapping, inline ABI, runtime reachability |
| traps 1-4 grouped around Check/Set/Clear | **Confirmed owner prose** | [Technical Interrupts](../../research/technical-interrupts.md) plus `sf2-tech-interrupts-static-v1` | Operand decoding, return movement, results and conditions |
| campaign consumers, persistence, UI, debug routes, balance | **Separate owner / Unknown** | Adjacent contracts and future runtime/synthesis work | Do not infer higher-level meaning from storage or wrapper shape |

## Reproduction

```powershell
uv run sf2 h2 common-stats
uv run sf2 h2 tech-interrupts
uv run sf2 design-contracts test
uv run sf2 verify
```
