# Map 3 Controlled Admission Contract

- Status: **Proposed**
- Evidence date: 2026-08-20
- Scope: implementation-neutral admission of the accepted controlled Map 3 start into the first
  exploration boundary, without requiring the original Witch/New lifecycle in the product, inventing
  a natural route, or starting Phase 4 implementation

## Judgment Boundary

This contract begins with the accepted controlled result recorded by
`sf2-map3-admitted-start-runtime-v1`. It ends when a validated product scenario has constructed the
exact accepted logical admission projection and setup/init result and is ready to admit the first
exploration action. It does not own the natural route into Map 3 or any behavior after that boundary.

- **Confirmed controlled evidence**: one original-runtime case reaches the first original
  `WaitForEvent` after the controlled Witch/New, new-game, save-service, main-loop, exploration-loop,
  setup-selection, and selected-init chronology recorded by the fixture. The reached result contains
  a complete bounded non-time scenario state, the source-shaped main-loop handoff, the selected
  default setup/init result, one observed RNG state, and no guarded program request before the
  boundary.
- **Accepted product direction**: ADR 0010 option 1A permits the remake to construct that accepted
  result directly. Product admission requires result equivalence at the first exploration boundary;
  it does not require call, program-counter, menu, naming, save-service, or chronology equivalence.
- **Unknown**: a natural player-visible New or load flow; the raw original time-counter values;
  whether another natural or controlled start has the same RNG state; natural Map 3 movement,
  dialogue, menu, event, transition, or battle admission; all later battle and endpoint behavior;
  complete presentation, audio, timing, and hardware observations; and save persistence.

The accepted fixture remains the golden. This document names bounded field categories and comparison
rules instead of copying its values into a second, weaker source of truth.

## Sole Executable Owner

The sole executable owner consumed by this contract is
`sf2-map3-admitted-start-runtime-v1` in
[`map3-admitted-start-v1.json`](../../../tests/fixtures/h3/map3-admitted-start-v1.json). Its verifier is
[`map3_admitted_start.py`](../../../src/sf2tool/h3/map3_admitted_start.py), and its evidence explanation
is [Map 3 Admitted-Start Evidence](../../research/map3-admitted-start.md).

The fixture directly binds seven research records through 17 bindings. Direct evidence binding and
semantic Design association are deliberately different denominators:

| Research record | Direct fixture bindings | Role in this contract | Proposed association |
| --- | ---: | --- | --- |
| `gameflow.main-loop` | 1 | source-shaped handoff provenance and logical admitted result | yes |
| `gameflow.exploration.loop` | 2 | first exploration and `WaitForEvent` boundary | yes |
| `scripting.map.mapsetupsfunctions-1` | 5 | selected setup/init dispatch and completed handoff | yes |
| `map.data.ms-map3-initfunction` | 1 | selected Map 3 init identity and bounded result | yes |
| `stats.new-game` | 1 | original controlled chronology provenance only | no; retain the new-game owner |
| `screens.witch.new-game-lifecycle` | 5 | original Witch/New chronology provenance only | no; retain the save-system owner |
| `tech.services.sram-actions` | 2 | original check/save service provenance only | no; retain the save-system owner |

The proposed semantic association subset is therefore exactly four records witnessed by nine direct
bindings. The remaining three records and eight bindings explain how the controlled original run was
reached; they do not require or expose those services as product admission behavior.

Phase 1 does not edit the research index. A later registration phase may add this contract only to the
four rows marked `yes`, after separate review. It must not bulk-associate the 26 aggregate Map 3 rows,
the other setup choices, generic state records, or adjacent lifecycle/service records.

## Original Evidence and Product Admission

The original controlled chronology and the product admission path have different obligations:

| Surface | Required meaning | Explicit non-claim |
| --- | --- | --- |
| original-evidence receipt | preserves and compares the complete fixture-owned H3 closure, including the source-shaped handoff, raw D0-D4 values, PCs, addresses, callback identities, chronology, provenance, boundary, and normalization capability | does not make those evidence fields authoritative product state or require the original route in the remake |
| product-admission result | identifies the validated scenario/package and proves the exact accepted logical map/start-position/facing/flow, force/session, setup/init, no-program-request, and exploration-readiness projection | does not store or synthesize D0-D4, PCs, RAM/ROM addresses, callback identities, or original lifecycle execution |

`OriginalEvidenceReceipt` and `ProductAdmissionResult` are prospective design labels for these two
surfaces. They are not fixture IDs, executable schemas, C# types, or new ownership of Research data.
Future implementation may choose different names while preserving the separation.

When an accepted adjacent owner establishes the semantic result of a register-shaped observation,
the product projection maps that result to the already-named logical field. It MUST NOT retain a
duplicate register-shaped product field. A raw handoff field without an accepted semantic mapping
remains evidence-only.

An H4 report MUST keep both results visible. A product-admission success cannot overwrite or relabel
the original evidence receipt, and an original H3 success cannot prove that a product package was
validated or admitted.

## Accepted Evidence and Product Projections

This contract defines two comparison projections over the fixture evidence. `OriginalEvidenceReceipt`
MUST preserve and compare the complete source-shaped closure on the original H3 side.
`ProductAdmissionResult` MUST validate only the exact logical admission projection whose meanings are
already established by accepted owners. The latter projection contains these product categories:

| Category | Original evidence surface | Product admission obligation |
| --- | --- | --- |
| map handoff | current/egress map plus the complete source-shaped main-loop handoff, including raw register facts | validate accepted logical map, start-position, facing, and flow-state fields only; unmapped raw handoff facts remain evidence-only |
| player and session state | player entity state, gold, and difficulty state | preserve the exact admitted values and accepted field widths/ranges through typed state |
| force state | every joined/active fact and the complete accepted ally-record fields | construct the full bounded force snapshot; omitted or default-filled records fail admission |
| setup/init result | selected default setup identity, selected init identity, dispatch return seams, and completion before the first exploration wait | validate the selected logical setup/init IDs and prove their admitted result is complete |
| guarded program result | the accepted absence of a guarded program request before the boundary | expose the same bounded result; a missing capability is not equivalent to an accepted no-request result |
| exploration readiness | arrival at the first original exploration wait | admit the first semantic exploration action only after all preceding result checks pass |

Original register names and values, RAM/ROM addresses, source symbols, callback identities, and PCs
belong to the complete evidence receipt and importer provenance. They MUST NOT become authoritative
gameplay state or leak into the plain domain model. The product mapping uses engine-neutral logical
IDs and typed values and MUST be complete for the accepted logical projection, versioned, and
diagnosable against the fixture-owned categories. Product success MUST NOT require an intentionally
excluded raw evidence field.

Equality is exact within each projection: the original evidence receipt compares the complete
fixture-owned closure, while product admission compares the complete accepted logical projection.
Product result equivalence does not require the remake to run the original setup code, selected init
code, or original main/exploration loops. A future product may construct an already-resolved snapshot
or execute independently maintained typed programs, provided the logical admitted result and
capability report remain identical at this boundary.

## Time and RNG Separation

The fixture records four distinct state spans: one RNG span and three original VInt-maintained time
spans. They do not share one normalization rule.

### Raw original time

The observer reads each raw original time counter at the first original `WaitForEvent` using its
source-faithful width. The accepted fixture does not publish those raw values as stable expectations.
They are therefore **Unknown/unpublished**, excluded from original-state equality, and unavailable as
product clock defaults.

### Post-boundary harness normalization

Only after the original `WaitForEvent`, and outside every callback, the R1 observer normalizes the
three time spans and verifies restoration. The fixture label
`post-boundary-controlled-zeroed-vint-counters` identifies that harness transformation.

An original-evidence receipt that claims reproduction of the R1 profile MUST report this
normalization capability and label. It MUST NOT describe the normalized values as original snapshot
facts. The product-admission result MUST NOT inherit a zero clock merely because the evidence harness
uses this transformation.

### Future product clock

The product deterministic clock is a separate Application/H4 input under ADR 0011. Its epoch, first
step, and mapping to later accepted timing evidence remain future contract work. Until that work is
accepted, this contract admits no original-time equality and chooses no product clock value.

### Controlled RNG observation

The RNG span is observed before the post-boundary time normalization and is not transformed by that
normalization. Its exact fixture value is admitted only as the state of this one controlled case.
It MUST NOT be generalized into a universal natural-new-game seed, an all-starts rule, or permission
to use a hidden or time-derived random stream. H4 may compare the controlled case exactly while other
starts and the later reached RNG call sequence remain separately owned.

## Import and Admission Rules

A future scenario package for this seam MUST be admitted before a gameplay session exists. At minimum
the admission boundary requires:

- the accepted fixture/scenario identity and content digest;
- a supported contract and package version;
- the complete accepted logical product projection listed above;
- validated logical map, setup, init, actor, class, item, and spell references as applicable;
- explicit provenance class and required capabilities;
- separate original-evidence and product-admission result fields;
- an explicit deterministic-clock capability without an invented original-time value;
- the controlled RNG field scoped to this one case;
- no unsupported or unresolved field silently replaced by a default.

Admission fails closed on a missing field, digest mismatch, unsupported version, duplicate or
unresolved logical ID, incomplete force record, wrong setup/init result, unexpected program request,
unsupported provenance/capability, or ambiguous evidence-to-product mapping. Diagnostics identify the
scenario phase, field/category, expected fixture owner, actual result, and responsible boundary.

The private 7C profile may resolve its original evidence only through ignored local inputs whose
identity and provenance are validated. A public-synthetic profile may exercise the same package and
admission shape with redistribution-safe project-authored data, but it makes no original-state,
original-asset, 7C, or 8C claim. Neither profile authorizes public release.

## H4 Acceptance Surface

A future H4 adapter passes this proposed contract only when it can:

1. validate a named original-evidence receipt against the accepted R1 fixture identity, including its
   controlled chronology, private provenance, observation boundary, and post-boundary normalization
   label, while leaving original-runtime reproduction with the H3 owner;
2. admit a product package through the ordinary validation path without invoking or claiming the
   original Witch/New, new-game, save-service, or main-loop chronology;
3. compare the complete fixture-owned source-shaped closure inside the original-evidence receipt,
   including raw handoff registers, PCs, addresses, and callback identities on that side only;
4. compare the exact accepted logical product projection without requiring D0-D4, PCs, RAM/ROM
   addresses, callback identities, or duplicate register-shaped product fields;
5. prove the selected setup/init result, no-program-request result, and first-exploration readiness
   without treating source addresses or callback PCs as domain state;
6. keep raw original time **Unknown/unpublished**, harness-normalized time evidence-only, and product
   deterministic-clock state separately labeled;
7. compare the fixture RNG state only for the one controlled case and reject an unexpected RNG field
   or hidden random source;
8. report original-evidence and product-admission failures independently, with no fallback counted as
   exact success;
9. preserve the fixture as the golden instead of copying selected values into an engine-specific
   snapshot;
10. avoid claiming that all 26 aggregate Map 3 records, alternative setup rows, or later route events
   were reached;
11. keep complete 8C pixels, palettes, frame cadence, animation, audio, VInt/DMA/CRAM/VDP observations,
    ordering, chronology, and tolerances outside this local admission contract.

These checks define future observation seams only. They do not constitute an H4 implementation or a
Phase 4 start.

## Adjacent Owners and Exclusions

| Adjacent owner | Boundary retained |
| --- | --- |
| [New-Game State Initialization](new-game-state-initialization.md) | owns original new-game initialization order and state mutations; this contract consumes only the reached admitted result |
| [Save System](save-system.md) | owns Witch/New and SRAM service behavior; its calls remain original evidence provenance and are absent from milestone product composition under option 6A |
| [Exploration Control Flow](exploration-control-flow.md) | owns generic MainLoop/ExplorationLoop control order; this contract owns only the admitted handoff/result at the selected first exploration boundary |
| [Map Entry Routing State](map-entry-routing-state.md) | owns generic map/battle routing helpers; natural Map 3 routing and Battle 01 admission remain R2 |
| [Map Setup Data](map-setup-data.md) | owns the private setup topology/import surface; this contract references only the accepted selected setup/init result |
| [Map and Exploration](map-exploration.md) | owns generic selector, init, entity, camera, map-script, and exploration rails; this contract does not absorb their broader fixtures or behaviors |
| state/data contracts | retain flag, roster, party, combatant, item, spell, class, and content field semantics; this contract composes one fixture-owned snapshot without redefining those fields |
| [Story Progression](../synthesis/story-progression.md) | may explain accepted progression architecture but cannot turn this controlled seam into a natural story route |
| [Map 3 to Battle 01 Readiness](../synthesis/map3-battle01-readiness.md) | remains the separate **NOT READY** ledger and owns no evidence through this document |

This contract explicitly excludes R2 natural Map 3 movement, dialogue, menu, event, transition, and
battle admission; R3 Battle 01 playthrough; R4 victory, after-program, return, and stable endpoint;
complete 8C presentation/audio/hardware evidence; save/load/checkpoint/suspend/persistence; complete
private asset or capture payloads; malformed natural-game recovery; route or content design; and any
claim that the 26 aggregate Map 3 rows were all reached.

Under option 6A, restarting the milestone may rebuild this controlled admitted state through the
scenario admission path. Such reconstruction, test setup, or H4 reset is not a player save, load,
checkpoint, or suspend feature.

## Phase and Distribution Boundary

This Proposed contract does not make the readiness ledger ready, authorize `remake/`, start Phase 4,
select an MCP workflow, run Godot, define engine scenes, or change product code. It does not authorize
publication of the ROM, SRAM, traces, captures, extracted assets, or other private inputs. Godot and
the future composition root remain outer consumers of the accepted Application admission result.

Any later continuous Map 3-to-Battle 01 scenario contract must consume this local admission seam as
one bounded owner. It must not weaken its golden, silently replace its fixture, or use it as proof of
R2, R3, R4, or complete 8C closure.

## Evidence Matrix

| Contract statement | Evidence label | Exact owner | Remaining boundary |
| --- | --- | --- | --- |
| controlled original chronology reaches the first Map 3 exploration wait | **Confirmed bounded runtime** | `sf2-map3-admitted-start-runtime-v1`; [research owner](../../research/map3-admitted-start.md) | chronology is provenance, not a natural route or product requirement |
| complete source-shaped handoff, PCs, addresses, callbacks, non-time state, and setup/init result | **Confirmed bounded runtime evidence projection** | same fixture | raw evidence fields remain in `OriginalEvidenceReceipt` and outside authoritative product state |
| exact logical admission result | **Accepted product projection** | same sole fixture; accepted adjacent contracts define semantics only | map/start-position/facing/flow, force/session, setup/init, no-request, and readiness values stay golden-owned; product mapping and H4 implementation are future work |
| direct evidence denominator 7 records / 17 bindings | **Confirmed indexed relationship** | accepted [research index](../../../manifests/research-index.json) | only four records / nine bindings form the proposed semantic association subset |
| raw original time values | **Unknown/unpublished** | R1 observer reads source-faithful spans | normalized public values are not original snapshot facts |
| post-boundary time normalization | **Confirmed controlled harness policy** | same fixture and verifier | does not choose the product deterministic clock |
| controlled RNG state | **Confirmed for one case** | same fixture | not a universal natural-start seed or later RNG trace |
| direct product construction | **Accepted product direction** | [ADR 0010](../../decisions/0010-map3-battle01-product-acceptance.md) option 1A | implementation and H4 execution require the separate Phase 4 start |
| authoritative state, deterministic clock/RNG, content admission, and layered receipts | **Accepted architecture constraint** | [ADR 0011](../../decisions/0011-phase4-remake-runtime-architecture.md) | exact product clock and later scenario facts remain future accepted inputs |
| natural route through endpoint and complete 8C | **Excluded / Unknown** | [Research audit](../../research/map3-battle01-audit.md) and later R2-R4/H4 owners | readiness remains **NOT READY** |

## Reproduction

The accepted controlled evidence and unchanged relationship counters are reproduced with:

```powershell
uv run sf2 h3 map3-admitted-start
uv run sf2 design-contracts test
uv run sf2 research-index test
uv run sf2 verify
```

These commands validate the accepted evidence and repository relationships. They do not execute a
remake, prove a natural route, or authorize Phase 4.
