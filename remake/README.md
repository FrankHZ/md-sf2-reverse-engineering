# SF2 Remake

This directory contains the independently maintained remake implementation. The first bounded Phase 4
slice was authorized by the user on 2026-08-28 (America/Chicago) under ADR 0016. It does not declare
the continuous Map 3 through Battle 01 milestone ready.

## Current boundary

The bounded `public-synthetic-map3-smoke-v1` vertical now composes the production Domain, Application,
and Content assemblies through a thin Godot adapter. It admits only the exact tracked project-authored
synthetic package, starts an Application `GameSession` in Map 3 exploration, applies one logical move,
and projects the resulting immutable snapshot in Godot with a persistent
`PUBLIC SYNTHETIC — NOT ORIGINAL FIDELITY` label. The official Godot 4.7.2 .NET CLI gate performs a
hash-locked import, headless source run, export, and headless exported-build run using only tracked
redistribution-safe inputs.

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
function targets, and entries are process-local Domain values, not a public content or save format. How
content loaders translate accepted source tables into these typed values is deferred to a later
Application/Content boundary.

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

Natural route, original-map fidelity, event and area-description reachability; actual flag values and
lifetime; target effects; decoded text; inventory or story mutation; persistence; original/natural Map 3
admission; Battle 01 continuity;
complete Application/Content/Godot game layers; original presentation; H4; private-content admission;
and milestone acceptance remain Unknown or deferred at their existing owners. The bounded synthetic
Map 3 admission above is not evidence of the original natural route or a full-content implementation.
Flag, step, roof, collision, reload, VDP/DMA, script-cursor, and update-toggle effects
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

The repository Public workflow runs the same locked restore, build, and test sequence and the official
Godot 4.7.2 public-synthetic import/run/export gate. No H3, H4, private-input, original-fidelity, or
emulator gate is implied by that public profile.
