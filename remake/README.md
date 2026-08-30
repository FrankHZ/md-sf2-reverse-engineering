# SF2 Remake

This directory contains the independently maintained remake implementation. The first bounded Phase 4
slice was authorized by the user on 2026-08-28 (America/Chicago) under ADR 0016. It does not declare
the continuous Map 3 through Battle 01 milestone ready.

## Current boundary

The bounded `public-synthetic-map3-smoke-v1` vertical now composes the production Domain, Application,
and Content assemblies through a thin Godot adapter. It admits only the exact tracked project-authored
synthetic package, starts an Application `GameSession` in Map 3 exploration, applies one logical move,
selects one bounded setup, area-description result, and opaque zone-event target through the accepted
Domain selectors, admits a typed Application-owned request/cue for that selected synthetic target,
requires an exact acknowledgement, and projects the resulting immutable snapshot in Godot with a persistent
`PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY` label. That acknowledgement now atomically applies one
package-declared synthetic flag effect; re-selecting context then exposes the already-declared synthetic
setup variant. A second exact synthetic zone now admits a typed same-map local-transition request;
acknowledging it atomically relocates the session to a package-declared passable cell and projects the
result in Godot. The accepted field-search discovery can now admit one exact project-authored placeholder
item acquisition through an immutable Domain inventory. One distinct synthetic zone now admits an outbound
transition into a tiny second project-authored public-synthetic map shell. The official Godot 4.7.2 .NET CLI
gate performs a hash-locked import, headless source run, export, and headless exported-build run using only
tracked redistribution-safe inputs.

The selector vertical is driven by `SelectExplorationContextCommand` through the same Application
`GameSession` mutation facade as movement. Its typed public-synthetic package owns a setup catalog,
bounded synthetic flag set, area-description source, and zone-event table. The resulting snapshot
contains only the selected logical setup, logical text indexes or opaque function identity, and opaque
zone target. Godot displays those values but does not execute a function or event target; moving clears
the position-specific selection so stale adapter state cannot become gameplay state.

Application now admits those map-specific values through an exact `MapId` exploration-runtime
catalog. Each immutable runtime definition binds one Domain working layout and synthetic walkability
grid to that map's area-description source and zone-event table; lookup has no default or fallback.
The current tracked package contributes exactly one synthetic Map 3 runtime and one intentionally tiny
`public-synthetic-outbound-shell` runtime. Scenario start is constructed only from the admitted Map 3
runtime plus the logical start position. Setup and runtime map-ID sets must match exactly, context snapshots
carry the live map ID, and the session projects only entities belonging to the live map while retaining the
full typed catalog for authoritative lookup. The existing local transition remains same-map and rebuilds
its destination state from that map's admitted runtime.

The request vertical adds `RequestSelectedZoneEventCommand` and
`AcknowledgeMapEventRequestCommand`. Application admits only package-declared request-to-specific-zone
cross-references, owns the pending/acknowledged snapshot and monotonic cue sequence, and rejects other
session commands while an acknowledgement is pending. Godot sends semantic commands and projects the
result; it neither interprets the opaque target nor decides whether an acknowledgement is valid. This
synthetic presentation request is not the original game's guarded `PROGRAM_REQUEST`; the admitted-start
fact `NoProgramRequest` remains unchanged and no original program effect is claimed.

The state-effect vertical adds one exact request-to-effect-to-flag mapping. Application owns the mutable
session flag snapshot, checks request/cue/effect identity on acknowledgement, applies the flag exactly once,
emits a typed non-blocking effect cue and receipt, and clears the prior context selection before the new setup
can be observed. Duplicate acknowledgements and repeat effect requests cannot mutate the snapshot, while a
new session reconstructs the package's initially clear flag state. Godot only sends the semantic
acknowledgement and renders the resulting flag, effect, and selected-setup facts. The synthetic flag and cue
do not claim an original flag number, target effect, program lifecycle, persistence rule, or natural route.

The local-transition vertical adds `RequestSelectedLocalTransitionCommand` and
`AcknowledgeMapLocalTransitionCommand`. Content binds one exact non-default synthetic zone to a unique
request and transition ID, source map/position/setup context, same-map destination position, opaque synthetic arrival
orientation, and presentation cue. Admission rejects duplicate or reused IDs and cues, default or
dangling targets, source-zone mismatches, unadmitted or cross-map references, identical endpoints, and
blocked destinations. Application owns the one pending transition and exact request/cue-sequence/transition
acknowledgement, then replaces the exploration position atomically while retaining the immutable layout,
walkability, and session-owned synthetic flags. It clears the position-specific context plus earlier event
request/effect snapshots; wrong or duplicate acknowledgements cannot mutate state, and restart begins at
the admitted start without transition state. Godot sends only those semantic commands and projects the
snapshot, cue, orientation label, and relocation result. It does not interpret the zone target, execute
original warp logic, or claim original coordinates, facing semantics, predicates, effects, or reachability.

The outbound-transition vertical is a separate `RequestSelectedOutboundTransitionCommand` and
`AcknowledgeMapOutboundTransitionCommand` lifecycle. Content binds one exact non-default Map 3 synthetic
zone to unique request/transition/cue identities, exact source map/position/setup state, and an exact
destination runtime/map/position/setup/semantic-facing projection. Admission rejects same-map, dangling,
default, duplicate, blocked, mismatched-setup, or reused-cue definitions. A matching acknowledgement
atomically replaces the live exploration runtime, facing, context, and current-map entity projection while
retaining the session's synthetic flags, discoveries, and immutable inventory. Old event, local-transition,
interaction, dialogue, search, and acquisition lifecycle views are cleared; wrong, stale, and duplicate
acknowledgements are zero-mutation, and restart returns to the admitted Map 3 state. Godot sends only the
semantic request/acknowledgement and projects the typed transition plus live exploration map/context. The
second map is project-authored synthetic content, not original Map 21; this lifecycle claims no original
map identity, layout, coordinates, warp predicate, facing, asset, story, natural reach, or fidelity.

The entity-interaction vertical adds one project-authored placeholder entity on an admitted synthetic
solid cell plus one exact entity-target/request/cue mapping. A separately declared semantic facing is
Application state; it is initialized by public-synthetic content, updated by movement direction or a
turn-in-place command, and is never inferred from the opaque original-facing admission fact. Application
selects only an entity exactly one tile ahead, owns the single pending interaction, and requires an exact
request/cue-sequence/entity/target acknowledgement. Movement, turning, context re-selection, and local
relocation clear stale acknowledged interaction state, while a pending acknowledgement blocks every
other session command. Wrong and duplicate acknowledgements are zero-mutation, and a restart reconstructs
the initial facing with no interaction state. Godot sends semantic turn/interact/acknowledge commands and
projects only placeholder entity position, cue, target label, and lifecycle status. It does not interpret
the target or claim original NPC identity, coordinates, facing, story behavior, or reachability.

The placeholder-dialogue vertical maps that one admitted interaction target to one unique synthetic
dialogue with two stable line IDs, project-authored non-original line strings, distinct line cues, and
one terminal close cue. The exact entity-interaction acknowledgement opens its first line atomically;
Application then owns the current line/index and requires the exact dialogue/cue-sequence/current-line
identity for each advance. The final advance closes the dialogue, wrong, stale, duplicate, and
after-close commands are zero-mutation, and an open line excludes unrelated session commands. Movement,
turning, context selection, and local relocation clear stale dialogue state, while restart begins with
none. Godot sends only semantic acknowledge/advance commands and projects the typed snapshot plus these
project-owned placeholder strings. It does not own sequencing or claim original dialogue IDs, speaker,
text, timing, window, portrait, audio, story meaning, or natural reachability.

The field-search vertical declares one exact project-authored searchable Map 3 position and selected
synthetic setup/zone context. Content binds that context to stable semantic request/result IDs, one
opaque placeholder discovery identity, and distinct pending/discovery cues. Application owns search
admission, the single pending acknowledgement, exact request/cue-sequence/result matching, the
immutable discovery receipt, and a session-local once-only discovery set. Wrong, stale, duplicate,
and repeated confirmations are zero-mutation; movement or context re-selection clears stale lifecycle
state without removing the admitted discovery, while restart reconstructs an empty discovery set.
Godot sends only semantic search/acknowledge commands and projects typed pending/discovered/result/cue
state. The discovery token is not itself an item, equipment, a consumable, original text or item identity;
it only admits the separately typed placeholder acquisition below. Neither lifecycle is evidence of
original search rules, coordinates, event-target meaning, item behavior, or natural reachability.

The placeholder item-acquisition vertical maps that one discovered identity to one unique synthetic
request/result pair, one project-authored opaque item ID, and distinct pending/acquired cues. Domain owns
the immutable item inventory and deterministic unique-acquisition reducer; Application remains the sole
mutation facade, requires the exact discovery/request/cue-sequence/result/item acknowledgement, and applies
the Domain result atomically. Wrong, stale, duplicate, and repeated commands are zero-mutation. Movement or
context re-selection clears the acquisition lifecycle view while acquired inventory remains session state;
restart creates an empty inventory. Godot sends only semantic acquire/acknowledge commands and projects the
typed lifecycle, receipt, cue, and inventory snapshot. This slice adds no item use, equipment, giving,
dropping, Caravan, capacity, consumption, persistence, save/load, icon, text, stat, effect, or original item
identity/semantics.

The Domain's broader implemented behavior includes a pure map-setup selector with engine-native
catalog and event-table admission boundaries. The catalog maps opaque map
IDs to already parsed routes, rejects duplicate map IDs, and delegates every known route to the ordered
route selector. Typed entity, zone, and item tables then select the first matching event record or their
single required default without executing the opaque target. Typed area-description sources similarly
select the first admitted X/Y entry, compute logical text indexes, or return an opaque function target
without invoking presentation or target behavior. These boundaries consume the accepted behavior
categories from:

- `sf2-map-setup-static-v1`: default-before-flags, complete ordered scanning, overwrite-on-set,
  last-set-wins, missing-map result, and the bounded selection-case categories;
- `sf2-map-setup-selection-runtime-v1`: the accepted selector observation boundary and bounded
  case outcomes;
- `sf2-map-events-static-v1`: first-match/default entity, zone, and item selection shapes, wildcard
  fields, event-flags transport, and item-index normalization;
- `sf2-map-event-dispatch-runtime-v1`: the accepted nine-case event-selection observation boundary;
- `sf2-map-descriptions-static-v1`: direct-return handling, ordered X/Y matching, conditioned-function
  admission, and logical text-index construction.

Production code and ordinary unit tests do not load those fixture files. Tests use project-authored
opaque IDs and synthetic routes, event tables, and area descriptions. The selectors do not contain
original sentinels, addresses, relative offsets, source symbols, pointer tables, the original route or
event corpus, map content, decoded text, ROM data, or private assets. Catalog IDs, event targets,
function targets, and entries are process-local Domain values, not a public content or save format.
The current Content adapter translates only the exact digest-locked project-authored synthetic package
into those typed values and validates its event-request IDs, cue IDs, and specific-zone cross-references.
It also validates the bounded local-transition IDs, exact source-zone mapping, admitted same-map endpoints,
passability, opaque orientation, and cue ownership.
The same raw-byte-locked package validates the synthetic initial semantic facing, unique placeholder entity
ID and occupied cell, explicit non-default interaction target, request and cue IDs, and closed
entity-to-target cross-reference. Entity cells must be in bounds, non-passable, and disjoint from the
admitted player and local-transition endpoint cells.
It also validates a closed one-to-one interaction-target/dialogue mapping, one to three trimmed single-line
placeholder strings of at most 120 characters, globally unique dialogue and line IDs, and globally unique
line/close presentation cues that cannot collide with other admitted cues.
The same package admits exactly one field-search record whose context/request/result/discovery IDs,
selected map/position/setup/zone, and two cues are unique and closed. The selected setup and zone must
match the existing typed context selectors, the cell must be in-bounds and passable, and its cues cannot
collide with any other presentation lifecycle.
It also admits exactly one item-acquisition record whose discovery must resolve to that field search and
whose request, result, opaque item, and two cue IDs are globally unique at their typed boundaries. The raw
package remains byte-digest locked and rejects unknown shape, defaults, dangling references, duplicate
identities, and cue reuse before a session starts.
The same closed package admits exactly one outbound transition and one second synthetic runtime. Their map,
setup, zone, endpoint, facing, request, transition, and cue references are validated against the exact runtime
and setup catalogs before a session starts.

A separate `PrivateCanonicalMap3ImportReader` now admits the caller's ignored canonical map-import
bytes only under `PrivateLocal`, after both their raw SHA-256 and the caller-supplied expected digest
match the fixed accepted canonical import SHA-256. The caller digest is an additional pin against that
accepted trust root, not the authority for choosing it. It fail-closes the canonical provenance and
shape, projects the exact 64-by-64 Map 3 layout,
checks every opaque layout word against its referenced blockset, retains the controlled
Map 3/(56,3)/facing-3/`ms_map3`/`ms_map3_InitFunction`/no-program-request boundary, and returns a
path-free immutable definition and receipt. The original layout, block, tileset, palette, and other
private payloads remain ignored or in memory; none is copied into Git, a PCK, or the public package.

`OriginalMapTraversal` is the corresponding bounded Domain policy over the current working layout.
It reads the current source and destination words for every request, applies the accepted `0xC000`
collision class and directional `0x8000`/`0x4000` stair mapping within the imported active-area
bounds, and therefore observes a later immutable block-copy result without caching a synthetic
walkability grid. This import/traversal slice is not connected to `MapExplorationRuntimeCatalog`,
`GameSession`, or Godot and is not a runnable original profile. Natural flags and setup variants,
entity occupancy, warps, init/event effects, persistence, assets, presentation, H3/H4, and 8C remain
explicitly unsupported or Unknown.

The Domain also owns an immutable 64-by-64 working-layout state and its ordered rectangular block-copy
reducer. The reducer clones the input, then performs forward word-by-word reads and writes on that clone,
preserving the accepted cascade behavior for overlapping copies. Logical words remain opaque `ushort`
values; original memory addresses, byte offsets, script cursors, and display-update behavior are not
part of the API. This reducer consumes the bounded copy chronology and seven-case observation boundary
from `sf2-map-block-mutation-runtime-v1`, with command-shape provenance retained by
`sf2-map-script-engine-static-v1`.

The command-level block-mutation reducer composes that copy with an immutable logical two-channel view
update state. `SetBlocks` requests channels 0 then 1 after a successful copy without clearing prior
requests; `SetBlocksVar` performs the same copy without requesting either channel. Ordered update marks
are compatibility output only: they do not claim render-queue acceptance, VDP/DMA work, or visible
refresh completion. Invalid copies fail before any result exists, leaving all immutable inputs intact.

The block-copy lifecycle reducer snapshots an admitted destination rectangle before either a forward
copy or an opaque-zero clear, retains the exact one-based matched-record ordinal while active, and can
later restore only that saved rectangle. Active activation and inactive restoration are no-ops. Each
successful activation or restoration requests logical update channel 0 without clearing channel 1.
The state uses typed copy/clear variants and an optional active snapshot instead of exposing original
sentinel values, buffers, addresses, or dispatcher mechanics.

The block-copy action reducer composes normalized 64-by-64 map cells, masked working-layout flags,
ordered typed action records, and the lifecycle reducer. Fading skips the action; show cells select the
first exact X/Y record and activate its copy or clear using the record's one-based position; hide cells
restore an active snapshot; other cells are neutral. Typed outcomes describe only this Domain decision.
Entity pixel-to-cell conversion, terminated source tables, and original dispatcher state stay outside
the API.

Natural route, original-map fidelity, event and area-description reachability; original flag values and
lifetime; target effects; decoded text; original inventory or story mutation; persistence; original/natural Map 3
admission; Battle 01 continuity;
original entity identity, placement, facing, dialogue, interaction effects, or reachability;
original field-search locations, predicates, targets, discoveries, items, or reachability;
original outbound maps, Map 21 identity, layouts, coordinates, warp predicates, or natural reachability;
complete Application/Content/Godot game layers; original presentation; H4; private-content admission;
and milestone acceptance remain Unknown or deferred at their existing owners. The bounded synthetic
Map 3 admission above is not evidence of the original natural route or a full-content implementation.
Private-profile runtime/product admission remains deferred even though the path-free canonical Map 3
import and bounded current-layout traversal prerequisite now exist. Flag, step, roof, reload, VDP/DMA,
script-cursor, and update-toggle effects
around working-layout mutation likewise remain outside these reducers. Roof-record matching, fade
dispatch, entity-coordinate conversion, lifecycle persistence, and content-driven record construction
remain deferred composition boundaries.

## Toolchain and dependencies

- .NET SDK `10.0.204` is selected by `global.json`; C# 12 targets `net8.0`.
- Clean CI also installs .NET SDK `8.0.424` to provide the supported .NET 8 runtime.
- NuGet is restricted to `https://api.nuget.org/v3/index.json`.
- `Microsoft.NET.Test.Sdk` `18.9.0` is MIT licensed.
- `xunit` `2.9.3` and `xunit.runner.visualstudio` `4.0.0` are Apache-2.0 licensed.
- Direct and transitive package versions are frozen by checked-in lock files.

The locked transitive test graph contains only `Microsoft.CodeCoverage`,
`Microsoft.TestPlatform.ObjectModel`, and `Microsoft.TestPlatform.TestHost` `18.9.0` under MIT, plus
the xUnit `abstractions`, `analyzers`, `assert`, `core`, and `extensibility` packages under the xUnit
Apache-2.0 license. The accepted slice audited the complete lock graph against the package metadata
from the sole configured NuGet source and found no reported vulnerabilities.

No SDK, package, Godot binary, ROM, extracted asset, capture, or generated build output is committed.

## Build and test

Run commands from this directory so the pinned `global.json` is authoritative:

```powershell
dotnet restore Sf2.Remake.sln --locked-mode
dotnet build Sf2.Remake.sln --configuration Release --no-restore
dotnet test Sf2.Remake.sln --configuration Release --no-build --no-restore
```

Relevant remake slices require the maintained local official Godot 4.7.2 gate: exact artifact and
version preflight, public-synthetic import, headless source run, export, exported-build run, and owned
process cleanup. GitHub Public intentionally stays lightweight: its sole `tracked-inputs` job runs the
locked whole-solution restore, build, and test sequence plus public tooling, architecture, planner, and
design checks, and it does not download or run Godot. No H3, H4, private-input, original-fidelity, or
emulator gate is implied by that public profile.
