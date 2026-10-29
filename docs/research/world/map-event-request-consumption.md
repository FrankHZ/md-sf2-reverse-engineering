# Map-event request consumption

## Scope and provenance

**Confirmed (static):** `sf2-map-event-request-consumption-static-v1` is the
consumer-side complement to the retained `sf2-map-event-request-state-static-v1`
writer inventory. It reads exactly seven pinned source files from
`ShiningForceCentral/SF2DISASM` commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, joins their 18 physical H1/ROM
instruction anchors to the US ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, and
keeps the H1 normalized-text SHA-256
`F28FAF604DD8F37AE3EDAA819DD1C9A601863B0596F2C83602CA3D572BB8644D`.

Run the deterministic static check with:

```text
uv run sf2 h2 map-event-request-consumption
```

The fixture is public-safe: it records symbol, width, source-line, PC,
control-shape, H1/ROM instruction digest, and retained-owner identities, but
no source payload, ROM/H1 bytes, dialogue, or inferred runtime narrative.

## Confirmed consumer topology

The six source-defined fixed-RAM symbols resolve to `CURRENT_SHOP_INDEX`,
`MAP_EVENT_TYPE`, `EGRESS_MAP`, `RAFT_MAP`, `RAFT_X`, and `RAFT_Y`. Their
consumer contexts are bounded at the first selected access, local branch, or
direct handoff; no callee algorithm is copied or entered.

| Consumer context | Entry PC | Confirmed fixed-RAM access shape |
| --- | ---: | --- |
| `GetShopInventoryAddress` | `0x20852` | `CURRENT_SHOP_INDEX` byte read at `0x2085C`, then the local carry branch |
| `ExplorationLoop` | `0x257C0` | `MAP_EVENT_TYPE` word reset at entry |
| `WaitForEvent` | `0x2591C` | `MAP_EVENT_TYPE` word reads at `0x2591C` and `0x25934`, with their local polls |
| `ProcessMapEvent` | `0x2594A` | `MAP_EVENT_TYPE` word clear before the six local dispatch branches |
| `FieldMenu` | `0x2127E` | `EGRESS_MAP` byte read at `0x21384`, field-local gates, and direct savepoint handoff |
| `GetEgressPositionForBattle` | `0x23E50` | `EGRESS_MAP` byte read at `0x23EA6`, local fallback branch, and direct savepoint handoff |
| `DeclareRaftEntity` | `0x441AA` | `RAFT_MAP`, `RAFT_X`, and `RAFT_Y` byte reads at `0x441FC`, `0x44202`, and `0x44206`, local gates, and direct entity declaration handoff |
| `sub_44404` | `0x44404` | `RAFT_MAP`, `RAFT_X`, and `RAFT_Y` byte reads at `0x4442C`, `0x44434`, and `0x44438`, local gates, and direct entity declaration handoff |

**Confirmed counts:** 6 symbol definitions, 13 lifecycle accesses, 12
symbol/context relations, 21 contextual roles, and 18 unique physical anchors.
Three entry/access aliases account for the difference between contextual roles
and physical PCs: `0x257C0`, `0x2591C`, and `0x2594A` are both context entry
and the respective reset, entry-poll, or pre-dispatch clear access.

The retained writer owner is freshly rebuilt before this fixture joins it. Its
accepted boundary remains 39 positive and 875 zero program contexts, 262 local
operations, 45 write definitions, 67 handoff sites, and 69 handoff relations.
The fresh consumer join also verifies the existing common-menu, gameflow,
field-menu-control, battle-loop, common-scripting, and common-map owners.

## Boundary and H3 question queue

**Unknown (runtime/caller-dependent):**

- normal-story producer-to-consumer reachability and the selected producer or definition;
- actual consumer entry state, read value, map-event poll/clear timing, and dispatch path;
- actual shop selection/outcome, field Egress destination, and battle Egress fallback destination;
- raft presence and coordinates, cross-map/save-load persistence; and
- input, UI, map-transition, audio, and story meaning.

These are a grouped H3 question queue only. This static slice does not claim
that any producer reaches a consumer, that an observed value has a particular
meaning, or that a source branch is naturally traversed.
