# Map-Sprite Assignment Surface Contract

- Status: **Confirmed static assignment identities, source-domain counts, and joined built-input exclusion**
- Evidence date: 2026-08-14
- Scope: original entity map-sprite assignment origins and the bounded domain they admit

## Judgment Boundary

This contract describes the source-static surface that can write or derive an entity's map-sprite
identity. It does not define sprite graphics, presentation, story placement, or arbitrary runtime
mutation.

- **Confirmed**: five complete entity map-sprite writer identities; five indexed helper/writer entry
  identities; 81 built script assignments across three macro forms; all 20 direct callers of
  `UpdateEntityProperties`; the accepted ally, enemy, and initial-entity domain summaries used as
  external witnesses; and the dedicated fixture's joined conclusion that its accepted original
  built input domains contain no map-sprite IDs `237..250`.
- **Inferred**: no behavior is normative at this evidence level. Terms such as `player`, `raft`,
  `ally`, `vehicle`, `regular-backed`, `routed-special`, and `blue-flame NPC` retain source or
  verifier classification vocabulary without establishing story purpose, rendering, or design
  intent.
- **Unknown**: whether deliberately malformed scripts, direct RAM mutation, corrupt combatant state,
  or a debug-only injection route can introduce IDs `237..250`; consumer behavior after such an
  injection; loader failure or sentinel behavior; queue saturation; caller-visible results; runtime
  frequency; timing; DMA; rendering; presentation; persistence; and replacement policy.

`originalBuiltDomainsContainReservedIds=false` is a **Confirmed static joined conclusion** owned by
the dedicated fixture over its accepted input domains. It is not a claim that arbitrary runtime
state can never contain those values, a guarantee for malformed data, or independent ownership of
the ally, enemy, or initial-entity corpora used as witnesses.

## Evidence Owner and Consumed Surface

The sole executable owner consumed by this contract is
`sf2-map-sprite-assignments-static-v1`
([fixture](../../../tests/fixtures/h2/map-sprite-assignments-static-v1.json),
[verifier](../../../src/sf2tool/h2/map_sprite_assignments.py),
[schema](../../../schemas/h2-map-sprite-assignments-static-fixture.schema.json), and
[manifest](../../../manifests/extractions/map-sprite-assignments-static.json)). Its source-backed
prose owner is [Common Scripting](../../research/common-scripting.md).

The contract consumes these tracked fixture fields:

- the five selected indexed entry identities and addresses from `table`;
- the aggregate counts in `summary`;
- the five bounded rows in `writerSites`;
- `scriptAssignmentFacts`, including macro/domain counts, the empty reserved-ID set, the highest
  regular value, and routed-special value identities;
- `updateCallerFacts`, including the complete input-kind count partition;
- the range and exclusion summaries in `derivedDomainFacts`; and
- the two retained `runtimeQuestions` as explicit Unknowns.

The verifier also generates complete `scriptAssignments[81]` and `updateCallers[20]` catalogs under
ignored `local/derived/map-sprite-assignments-static.json`. Those catalogs are private verification
inputs. They are deliberately absent from the public fixture and are not published by this contract.

The aggregate `sf2-common-scripting-static-v1` fixture is not consumed. Neither are the ally-data,
enemy-definition, map-entity, map-sprite decode, special-sprite decode, map-data, or runtime H3
fixtures. Their accepted results appear only through explicit separate-owner boundaries below.

## Direct Binding and Association Boundary

The dedicated fixture directly binds six research-index records. Five are the exact future
association candidates for this contract; the sixth remains solely with its existing data owner.

| Record ID | Source identity | ROM entry | Contract treatment |
| --- | --- | ---: | --- |
| `scripting.entity.declarenewentity` | `DeclareNewEntity` | 280,010 | new association candidate |
| `scripting.entity.esc17-setspritenumber` | `esc17_setSpriteNumber` | 23,420 | new association candidate |
| `scripting.entity.getallymapsprite` | `GetAllyMapsprite` | 281,030 | new association candidate |
| `scripting.entity.updateentityproperties` | `UpdateEntityProperties` | 24,658 | new association candidate |
| `scripting.map.csc1a-setentitysprite` | `csc1A_setEntitySprite` | 289,352 | new association candidate |
| `ally.data.map-sprites` | `table_AllyMapsprites` | 281,182 | unchanged; owned only by [Ally Definition and Growth Data](ally-definition-data.md) |

No other `scripting.entity.*`, `scripting.map.*`, `ally.data.*`, `enemy.*`, `map.entity-population.*`,
`map.data.*`, `tech.graphics.*`, or auxiliary-data record is associated by this contract.

## Complete Writer Identity Surface

The verifier scans the complete pinned source tree for writes to the entity map-sprite field. It
finds four offset-form writes plus one direct player-field write, and proves there is exactly one
source occurrence of each bounded instruction shape.

| Source owner | Source path | Confirmed write shape |
| --- | --- | --- |
| `DeclareNewEntity` | `code/common/scripting/entity/entityfunctions_1.asm` | `d4` byte to the entity-definition map-sprite field |
| `esc17_setSpriteNumber` | `code/common/scripting/entity/entityscriptengine_2.asm` | script byte `3(a1)` to that field |
| `UpdateEntityProperties` | `code/common/scripting/entity/entityscriptengine_2.asm` | `d3` byte to that field |
| `csc1A_setEntitySprite` | `code/common/scripting/map/mapscriptengine_1.asm` | `d0` byte to the selected entity field |
| `direct-player-raft-write` | `code/common/scripting/map/followersfunctions_2.asm` | named `MAPSPRITE_RAFT` byte to the direct player field |

`GetAllyMapsprite` is a separately indexed derivation helper, not a sixth writer. The public writer
rows record source identities and bounded instruction summaries; they do not publish complete source
bodies or establish that every writer is naturally reached during ordinary play.

## Built Script Assignment Domain

The complete pinned source scan finds 81 assignments in three macro families:

| Macro family | Assignment count |
| --- | ---: |
| `setSprite` | 56 |
| `newEntity` | 18 |
| `ac_setSprite` | 7 |
| **Total** | **81** |

Those assignments use 40 distinct numeric map-sprite values. The verifier classifies 76 assignments
as `regular-backed` and five as `routed-special`. All five routed-special assignments use value
`255`. The highest regular value in this script surface is `230`. No script assignment uses a value
in `237..250`, so `scriptReservedCount=0` and `reservedIdsPresent=[]`.

These are source-corpus counts, not execution counts. Repeated source assignments do not imply
runtime frequency, and the `regular-backed`/`routed-special` categories do not define decode,
presentation, or replacement behavior.

## `UpdateEntityProperties` Input Partition

Every direct source caller of `UpdateEntityProperties` has a bounded preceding `d3` input. The
complete 20-caller catalog closes as:

| Input classification | Caller count | Contract meaning |
| --- | ---: | --- |
| preserve existing | 12 | the source passes the preserve sentinel rather than a replacement identity |
| ally-table derived | 5 | the bounded caller window derives an ally map-sprite value |
| ally or literal vehicle | 1 | the caller window contains the accepted ally/vehicle split |
| literal map sprite | 2 | the caller window supplies a named literal identity |
| **Total** | **20** | every direct caller is classified |

The table records only input provenance. It does not claim the call succeeds, that another state
field is unchanged, that a graphical load completes, or that the selected sprite becomes visible.
The complete caller paths, line numbers, and expressions stay in the private derived catalog.

## Joined Accepted Built-Input Conclusion

The dedicated verifier joins four accepted source-static domains:

1. the 81 built script assignments summarized above;
2. the 20 classified inputs to `UpdateEntityProperties`;
3. 980 accepted initial entity records from the separate [Map Entity Data](map-entity-data.md) owner;
4. the externally owned ally and enemy definition tables.

The tracked witness summaries are:

- the 980 initial records contain no sentinel-regular or unbacked-special ID;
- the 30-row ally table has values in `1..58`;
- the 166-row enemy table has values in `52..229`;
- ally derivation only subtracts from accepted table values or uses named blue-flame NPC fallbacks;
- all 20 property-update callers are classified; and
- the built script surface itself has zero IDs in `237..250`.

Together these yield `originalBuiltDomainsContainReservedIds=false` for the fixture's accepted joined
domains. The conclusion does not redefine the 980 records, 30 ally rows, 166 enemy rows, or their
complete contents. It also does not extend to raw RAM, malformed scripts, corrupt state, unpublished
mods, or future remake data.

## Implementation-Neutral Logical Model

A complete private import may use a model equivalent to:

```text
MapSpriteAssignmentSurface {
    provenance {
        fixtureId
        upstreamCommit
        romSha256Identity
        verifierOutputSha256
    }
    writerIdentities[5] {
        logicalWriterId
        sourcePath
        boundedWriteForm
        optional indexedRecordRef
    }
    indexedDerivationHelpers[1] {
        logicalHelperId = GET_ALLY_MAPSPRITE
        indexedRecordRef
    }
    privateScriptAssignments[81] {
        sourceOriginRef
        macroFamily
        expressionIdentity
        logicalMapSpriteValue
        sourceDomainClass
    }
    privateUpdateCallers[20] {
        sourceCallerRef
        inputClassification
        privateExpressionIdentity
    }
    externalDomainWitnesses {
        initialEntityRecordOwnerRef
        allyTableOwnerRef
        enemyTableOwnerRef
        acceptedRangeAndExclusionSummaries
    }
    reservedIdRange = 237..250
    originalBuiltDomainsContainReservedIds = false
}
```

The original addresses, source paths, expressions, and catalogs are provenance and private
round-trip inputs. After verifying them, a conforming remake may use engine-native entity and sprite
references. It is not required to reproduce Mega Drive RAM offsets, byte-field placement, macro
encodings, instruction forms, ROM addresses, or the original writer microimplementation.

The logical model MUST keep the assignment origin and selected logical sprite identity distinct.
It must also preserve the difference between a source assignment, a caller that preserves the
current value, a derived table value, and an externally owned data witness.

## Public and Private Projection

The public projection may retain only the bounded tracked metadata already represented by the
fixture and this contract:

- fixture identity, output hash, upstream revision, and accepted ROM identity;
- selected indexed symbols and entry addresses;
- the five bounded writer rows;
- aggregate assignment, distinct-value, macro-family, domain, and caller counts;
- the highest regular value and routed-special value identity;
- the empty reserved-ID summaries, external table ranges, and joined built-domain conclusion; and
- the two explicit runtime questions.

The public projection MUST NOT include the complete 81 assignment rows, the complete 20 caller rows,
full script programs, initial entity records, ally/enemy table rows, private expressions, original
sprite payloads or art, ROM excerpts, emulator traces, or captured presentation. Private importers
may verify those materials locally without making them redistributable contract payloads.

## H4 Remake Acceptance Surface

A future H4 implementation conforms when it can show that:

1. the five logical writer origins and one indexed derivation-helper identity are represented without
   merging their roles;
2. the complete private 81-assignment catalog imports deterministically and retains its exact
   `56 + 18 + 7` origin partition;
3. the complete private 20-caller catalog retains the exact `12 + 5 + 1 + 2` input partition;
4. values in the accepted built script surface retain their logical identities and the exact
   `76 regular-backed + 5 routed-special` classification;
5. external initial-entity, ally-table, and enemy-table witnesses remain references to their own
   contracts rather than copied ownership;
6. the joined accepted built-domain test reports no IDs `237..250` without generalizing that result
   to malformed, injected, or arbitrary runtime state;
7. engine-native references can reproduce the same assignment relation without requiring the
   original RAM, macro, instruction, or address layout; and
8. public reports expose only bounded metadata while complete catalogs and copyrighted payloads
   remain private.

H4 does not require the original write loop, call timing, graphics queue, DMA behavior, rendered
result, story occurrence, or treatment of unsupported injected IDs.

## Cross-System Separation

- [Ally Definition and Growth Data](ally-definition-data.md) remains the sole owner of the 30-row
  ally map-sprite table, its identity order, and its row contents.
- [Enemy Definition Data](enemy-definition-data.md) retains the 166-row enemy map-sprite table and
  the normal-versus-tail definition boundary.
- [Map Entity Data](map-entity-data.md) retains the 980 physical initial entity records, record
  packing, list topology, and initial map-sprite field.
- [Map-Sprite Graphics Data](map-sprite-graphics-data.md) retains the 720 regular source slots, 670
  payload identities, alias relation, Basic streams, and sentinel structure.
- [Graphics Service State](graphics-service-state.md) retains special-sprite pointer/dispatch and
  decompression service boundaries.
- [Map and Exploration](map-exploration.md) and Common Scripting retain runtime entity mutation,
  movement, action dispatch, lifecycle, camera, and presentation handoffs.
- [Sprite Dialogue Property Data](sprite-dialogue-property-data.md) retains map-sprite-to-portrait and
  speech-SFX property lookup.
- Story selection, save persistence, visible facing/animation, graphics loading, VInt/DMA cadence,
  localization, accessibility, and replacement policy remain separate-owner, Unknown, or deliberate
  remake design.

## Evidence Matrix

| Contract area | Evidence label | Owner | Remaining boundary |
| --- | --- | --- | --- |
| five writer identities and bounded write forms | **Confirmed static** | `sf2-map-sprite-assignments-static-v1` | Natural execution, result, queueing, timing |
| 81 assignments, macro/domain counts, zero reserved values | **Confirmed static** | same fixture | Runtime frequency, presentation, malformed inputs |
| 20 classified `UpdateEntityProperties` callers | **Confirmed static** | same fixture | Complete caller behavior and visible effect |
| initial/ally/enemy range and exclusion witnesses | **Separate-owner Confirmed static witnesses** | map-entity, ally-definition, and enemy-definition owners joined by the dedicated fixture | No transfer of corpus ownership |
| accepted joined built-domain exclusion of IDs `237..250` | **Confirmed static** | dedicated assignment fixture | Raw RAM, corrupt/debug injection, arbitrary runtime state |
| developer/story purpose of source labels | **Inferred, non-normative** | source vocabulary only | Product intent and player-facing meaning |
| injected-ID consumer failure, timing, rendering, persistence | **Unknown / separate owner** | future bounded runtime or presentation evidence | Not an H4 requirement here |

## Reproduction

```powershell
uv run sf2 h2 map-sprite-assignments
uv run sf2 design-contracts test
uv run sf2 verify
```

The generated complete assignment and caller catalogs remain under ignored
`local/derived/map-sprite-assignments-static.json`. They are reproducible private evidence, not
tracked or distributable contract content.
