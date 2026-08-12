# ROM Header Data Contract

- **Confirmed original structure:** a 64-entry vector-table boundary, the accepted HInt/VInt level
  anchors and named-trap range summary, and the exact product, stored-checksum, ROM-end, declared-SRAM,
  and region metadata listed below.
- **Inferred original behavior:** only the platform-facing intent suggested by the source header field
  labels; this contract promotes no boot, interrupt, trap, save, region, or hardware behavior from
  those labels.
- **Unknown original behavior:** the complete per-vector target mapping and ABI, boot/reset use,
  interrupt and trap runtime behavior, checksum generation or platform acceptance, SRAM enablement and
  persistence, region compatibility, and all player-visible outcomes.
- Remake status: implementation-neutral Phase 3 provenance/import contract; no emulator header API,
  persistence backend, region policy, or distributable original payload has been selected.
- Evidence date: 2026-08-12
- Source baseline: `ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

## Contract Boundary

This contract owns two related but distinct static structures:

1. metadata about the 64-entry vector table that precedes the console header;
2. selected literal console-header fields accepted by the executable owner.

The executable owner is fixture id `sf2-remaining-core-static-v1` in
[`tests/fixtures/h2/remaining-core-static-v1.json`](../../../tests/fixtures/h2/remaining-core-static-v1.json).
This contract consumes only `expected.headerFacts`. The research owner is
[ROM Header, Window Engine, and Special Debug Flows](../../research/remaining-core.md), and the
bounded source owner is `code/romheader.asm` at the pinned baseline.

The research-index provenance record is `core.rom-header`, bound to source symbol `InitialStack` at
ROM address `0`. That symbol/address pair is an evidence anchor only. It does not establish the
initial stack value, stack setup, reset semantics, or any caller-visible behavior.

The vector table and console header share one source file and one contract, but they MUST remain
separate logical structures. A consumer cannot replace the vector-table summary with console-header
metadata or infer vector targets from the product, memory-range, checksum, or region fields.

## Pre-Contract Evidence Audit

The dedicated owner was reproduced from the accepted baseline:

```text
sf2-remaining-core-static-v1
SHA256 E786D25EF9EABCC12A997227AF4B3CA32E29F2D79E4DB77EE45417ADB1B39224
Files 5 / WindowSlots 8 / DebugActions 7 / RuntimeQuestions 2 / PASS
```

The fixture directly binds exactly five research-index records:

| Record | Existing design owner |
| --- | --- |
| `core.rom-header` | none; exact future association for this contract |
| `core.window-engine` | [window-system](window-system.md) |
| `debug.battle-test` | [debug-control-flow](debug-control-flow.md) |
| `debug.configuration` | [debug-control-flow](debug-control-flow.md) |
| `debug.battle-actions` | [debug-control-flow](debug-control-flow.md) |

Only `core.rom-header` is unassociated. The other four records retain their existing contracts and
semantics. This contract does not consume `expected.windowFacts` or `expected.debugFacts` merely
because those sections share the fixture.

The accepted header facts agree with the independent H0 identity rail summarized in the repository
[README](../../../README.md). That agreement is corroboration, not a merged evidence owner: the
stored header checksum in this contract remains distinct from H0's independently computed checksum
verification.

## Vector-Table Metadata

**Confirmed static:** the source contains exactly 64 longword vector entries before the console
header. Within the accepted summary:

- the horizontal-interrupt anchor is at interrupt level 4;
- the vertical-interrupt anchor is at interrupt level 6;
- `namedTrapRange` is exactly `[0, 9]`.

`namedTrapRange` is a range summary, not a claim that every trap slot in that interval has a distinct
named implementation, a proven transport ABI, or an accepted runtime path. The executable fixture
does not publish a complete ordered vector-target table. This contract therefore does not require or
claim:

- the exact target identity of each of the 64 entries;
- that every entry is nonzero, unique, callable, or reachable;
- per-vector register, stack, return, or condition-code behavior;
- interrupt priority, cadence, acknowledgement, or device behavior;
- exact per-trap service mapping or inline-operand decoding.

The count and three accepted summary fields are sufficient for public traceability. A private source
audit may inspect the ordered vector words, but those words do not become public fixture payload or an
H4 parity requirement under this contract.

## Console-Header Metadata

**Confirmed static:** the selected literal header fields are:

| Field | Accepted value | Preserved boundary |
| --- | ---: | --- |
| product code | `GM MK-1315 -00` | exact stored identifier; no platform-acceptance claim |
| stored checksum | `35105` (`0x8921`) | stored word only; no algorithm or validation claim |
| ROM end address | `2097151` (`0x1FFFFF`) | declared end value only |
| SRAM start address | `2097153` (`0x200001`) | declared odd start retained exactly |
| SRAM end address | `2113535` (`0x203FFF`) | declared end value only |
| region code | `U` | exact stored code; no compatibility or player-region claim |

The SRAM interval is header metadata. Its odd start MUST NOT be normalized, rounded, corrected, or
silently replaced by an implementation's preferred address. Preserving it does not prove that SRAM
is enabled, accessible, byte- or word-addressable in a particular way, persistent, powered, or used
by any accepted save path.

Likewise, the stored checksum is not the checksum algorithm. It does not prove how the original
value was generated, when a platform validates it, which bytes participate, or what happens after a
mismatch. H0 may independently compute the same value without changing this contract's narrower
stored-field boundary.

## Implementation-Neutral Import Model

The minimum public model keeps vector metadata and console-header metadata separate:

```text
RomHeaderContract {
  provenance: RomHeaderProvenance
  vectorTable: VectorTableMetadata
  consoleHeader: ConsoleHeaderMetadata
}

RomHeaderProvenance {
  fixtureId = "sf2-remaining-core-static-v1"
  sourcePath = "code/romheader.asm"
  sourceAnchorSymbol = "InitialStack"
  sourceAnchorAddress = 0
  upstreamCommit
  romIdentityReference
}

VectorTableMetadata {
  entryCount = 64
  horizontalInterruptLevel = 4
  verticalInterruptLevel = 6
  namedTrapRange = [0, 9]
}

ConsoleHeaderMetadata {
  productCode = "GM MK-1315 -00"
  storedChecksum = 35105
  romEndAddress = 2097151
  declaredSramStartAddress = 2097153
  declaredSramEndAddress = 2113535
  regionCode = "U"
}
```

This is a provenance and import-validation model, not a hardware abstraction. It deliberately has no
stack state, vector target collection, trap dispatcher, SRAM device, save backend, checksum service,
region switch, or presentation state.

The public projection MAY retain the exact bounded metadata above, fixture identity, pinned-source
provenance, and pass/fail diagnostics. It MUST NOT redistribute raw vector words, the raw console
header, title strings, copyright strings, memo fields, or other original header payload. Private
verification may read those bytes from a user-owned original input without publishing them.

## Cross-System Separation

This contract does not own:

- `InitialStack`/`p_Start` boot and reset chronology, which remains with
  [startup-control-flow](startup-control-flow.md);
- interrupt registration, VInt/HInt handling, DMA queues, fade control, device-facing intent, or
  trap-runtime seams, which remain with
  [interrupt-dma-and-trap-state](interrupt-dma-and-trap-state.md);
- flag storage and the bounded flag-trap grouping, which remain with
  [global-flag-state](global-flag-state.md);
- sound-command transport and driver state, which remain with [audio-system](audio-system.md);
- save layout, initialization, SRAM persistence, suspend behavior, or power-loss recovery, which
  remain with [save-system](save-system.md);
- region admission, platform compatibility, checksum rejection, emulator policy, or hardware timing;
- window or debug semantics from the other sections of the shared remaining-core fixture.

An implementation may consume the metadata here while using different runtime abstractions. Such a
choice is conforming only when it does not relabel runtime behavior as evidence supplied by this
static contract.

## Judgment Boundary

### Confirmed

- fixture/source provenance through `sf2-remaining-core-static-v1` and `core.rom-header`;
- 64 vector entries before the console header;
- HInt level 4, VInt level 6, and `namedTrapRange=[0,9]` as bounded summary fields;
- exact product code, stored checksum, ROM-end, declared-SRAM-range, and region-code values;
- public metadata/private raw-payload separation.

### Inferred

- the source labels indicate platform-facing vector and header intent, but no runtime behavior is
  promoted from that intent.

### Unknown

- the full per-vector target map, exact trap mapping, ABIs, reachability, and runtime results;
- boot/reset stack behavior and the relationship between all vector entries and live execution;
- interrupt cadence, priority effects, DMA/device behavior, and visible or audible outcomes;
- checksum generation, validation, rejection, and compatibility behavior;
- SRAM enablement, accessibility, persistence, hardware behavior, and save-system integration;
- region compatibility, localization selection, and any player-visible result;
- malformed, modified, expanded, or replacement-header admission policy.

## H4 Acceptance Contract

A remake-facing H4 adapter passes this contract only when it can:

1. identify fixture `sf2-remaining-core-static-v1`, the pinned upstream commit, source path, and
   `InitialStack` address-zero provenance anchor without interpreting that anchor as stack behavior;
2. represent vector-table metadata and console-header metadata as distinct structures;
3. reproduce vector entry count 64, HInt level 4, VInt level 6, and named-trap range `[0,9]` without
   inventing a complete per-vector mapping, non-null rule, or ABI;
4. preserve product code `GM MK-1315 -00` exactly;
5. preserve stored checksum 35105 separately from any independently computed checksum result or
   checksum algorithm;
6. preserve ROM end 2097151 and declared SRAM range 2097153 through 2113535 exactly, including the
   odd start, without treating them as proven runtime memory behavior;
7. preserve region code `U` without assigning compatibility or localization behavior;
8. detect missing fields, changed literals, structure conflation, normalized addresses, and invented
   vector semantics through synthetic metadata tests;
9. keep raw vector words, raw header bytes, title/copyright strings, and other original payload out of
   public fixtures and reports;
10. report boot, interrupt, trap, checksum, SRAM, save, region, hardware, and presentation behavior
    through their separate owners or as **Unknown**.

H4 acceptance does not require the remake to reproduce an original hardware header internally. It
requires a private import/provenance adapter to retain the accepted metadata faithfully and to report
intentional replacement or omission explicitly.

## Evidence Matrix

| Contract surface | Evidence label | Exact owner | Preserved boundary |
| --- | --- | --- | --- |
| provenance anchor | **Confirmed static** | `sf2-remaining-core-static-v1`; [fixture](../../../tests/fixtures/h2/remaining-core-static-v1.json) | `InitialStack` / address 0 is provenance only; no stack or reset semantics |
| vector-table summary | **Confirmed static** | same fixture; [remaining-core research](../../research/remaining-core.md) | 64 entries, HInt 4, VInt 6, range `[0,9]`; no full mapping, ABI, or reachability |
| console-header literals | **Confirmed static** | same fixture | exact product/checksum/ROM-end/SRAM-range/region fields; no hardware or platform behavior |
| stored-checksum corroboration | separate-owner evidence | repository [H0 summary](../../../README.md) | computed verification remains distinct from stored-field ownership and algorithm semantics |
| boot/reset control | separate-owner evidence | [startup-control-flow](startup-control-flow.md) | `InitialStack` and `p_Start` live chronology is not duplicated here |
| interrupt and trap behavior | separate-owner evidence / **Unknown** | [interrupt contract](interrupt-dma-and-trap-state.md); [flag contract](global-flag-state.md) | accepted bounded seams stay with their owners; unobserved mappings, ABIs, timing, and results remain open |
| SRAM/save behavior | separate-owner evidence / **Unknown** | [save-system](save-system.md) | declared range is retained; enablement, persistence, power loss, and integration are not claimed |
| public payload | **Unknown** / excluded | no consumed redistribution owner | metadata and provenance only; raw copyrighted header/title payload remains private |

## Open Questions

1. Should a future platform-adapter contract validate a privately read complete vector table without
   adding its raw targets to public fixtures?
2. Which checksum algorithm and platform acceptance rules should a distributable import tool support,
   and how should intentional modified-ROM values be reported?
3. How should a remake preserve the declared SRAM range as provenance while using a platform-neutral
   save backend?

## Reproduction

```powershell
uv run sf2 h2 remaining-core
uv run sf2 design-contracts test
uv run sf2 research-index test
```

Generated output remains under ignored `local/derived/remaining-core-static.json`. Public acceptance
uses fixture metadata and provenance, not raw original header payload.
