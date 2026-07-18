# ADR 0001: BizHawk for H3 Runtime Observation

- Status: **Accepted**
- Decision date: 2026-07-17
- Scope: automated observations of original Mega Drive behavior

## Decision

Use pinned **BizHawk 2.11.1** with its **Genesis Plus GX** core for the first H3 fixtures. Keep the
release archive and extracted application under ignored `local/toolchains/`; verify their size and
SHA-256 through `manifests/toolchain.json`; do not redistribute the binary bundle.

The fixtures use tracked Lua bus-execution callbacks against natural game execution. They may write
a controlled RAM input when the original function is entered and read registers/RAM at a later
instruction boundary. It must not assume that `emu.setregister` works for this core.

BizHawk's [official repository](https://github.com/TASEmulators/BizHawk) documents command-line Lua,
Genesis support, stable-release usage, and the MIT license for EmuHawk source. The
[2.11.1 release](https://github.com/TASEmulators/BizHawk/releases/tag/2.11.1) is pinned rather than a
floating latest download. The [official Lua API reference](https://tasvideos.org/Bizhawk/LuaFunctions)
documents bus execution callbacks, memory access, register access, and process exit codes.

## Why

The project needs repeatable full-console evidence, not an interactive debugger transcript. BizHawk
can boot the locked ROM, run a script non-interactively, observe 68000 instruction addresses, emit a
small JSON fact set, and return an exit code. This is sufficient for state fixtures and later
input/movie-based scenarios while preserving the ROM's real system environment.

## Observed Capability Boundary

**Confirmed locally:** `emu.getregister` reports Genesis 68000 registers, `event.on_bus_exec` fires in
the `M68K BUS` scope, and big-endian bus RAM reads/writes work. In contrast, attempts to set D6, A7,
or PC with `emu.setregister` returned without error but did not change Genesis Plus GX state. The
base H3 RNG fixture therefore waits for the game's natural calls, verifies their natural D6 range,
writes only `RANDOM_SEED`, and observes the unmodified ROM routine's result. The companion
debug-aware fixture enters the original Battle Test with controller input and controls only the
documented debug toggle, player-input byte, and seed at natural wrapper calls.

The generic API's existence is not evidence that every core implements register mutation. Any future
fixture that needs arbitrary register/PC injection must first prove that capability or choose another
execution engine.

## Alternatives Considered

- **Unicorn 2.1.4 M68K:** useful in principle for isolated function execution, but the pinned build
  raised `UC_ERR_EXCEPTION` on the original `ADDI.W #7,D7` instruction at ROM `0x160A`. It is not part
  of the committed toolchain. Revisit only with an independently verified M68K compatibility path.
- **Manual emulator debugging:** valuable for exploration, but not acceptable as the only evidence
  because it cannot run as the repository's non-interactive harness.
- **Building a Mega Drive emulator:** outside project scope and unnecessary for the current evidence.

## Consequences

- `uv run sf2 init --rom-path <ROM path>` obtains only the official pinned BizHawk release through
  the migration adapter and verifies it before use.
- Root `uv run sf2 verify --full` runs both RNG fixtures and offers `--skip-runtime` for deliberately
  static-only milestone work. Default `uv run sf2 verify` stays on the commit gate. `uv run sf2 h3
  rng` is the narrow Python-owned rail; its tracked Lua observer emits only controlled state facts.
- Emulator-generated configs, Lua scripts, observations, states, traces, and movies remain ignored.
- The EmuHawk repository is MIT, but a release bundle contains separately licensed components. The
  manifest records that boundary and the project does not redistribute the bundle.
