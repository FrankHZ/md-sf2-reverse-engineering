# SF2 Remake

This directory contains the independently maintained remake implementation. The first bounded Phase 4
slice was authorized by the user on 2026-08-28 (America/Chicago) under ADR 0016. It does not declare
the continuous Map 3 through Battle 01 milestone ready.

## Current boundary

The only production project is `Sf2.Remake.Domain`. Its first implemented behavior is a pure
map-setup selector with an engine-native catalog admission boundary. The catalog maps opaque map IDs
to already parsed routes, rejects duplicate map IDs, and delegates every known route to the ordered
route selector. It consumes the accepted behavior categories from:

- `sf2-map-setup-static-v1`: default-before-flags, complete ordered scanning, overwrite-on-set,
  last-set-wins, missing-map result, and the bounded selection-case categories;
- `sf2-map-setup-selection-runtime-v1`: the accepted selector observation boundary and bounded
  case outcomes.

Production code and ordinary unit tests do not load those fixture files. Tests use project-authored
opaque IDs and synthetic routes. The selector does not contain original addresses, source symbols,
pointer tables, the original route corpus, map content, ROM data, or private assets. Catalog IDs and
entries are process-local Domain values, not a public content or save format.

Natural route reachability, actual flag values and lifetime, persistence, Map 3 admission,
Battle 01 continuity, presentation, Godot integration, H4, and milestone acceptance remain Unknown or
deferred at their existing owners.

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

The repository Public workflow runs the same locked restore, build, and test sequence. This slice has
no Godot, H3, H4, private-input, or emulator gate.
