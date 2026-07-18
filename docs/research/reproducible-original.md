# Reproducible Original Baseline

- Status: **Confirmed**
- Evidence date: 2026-07-17
- Scope: USA retail ROM identity, pinned local toolchain, split/build reconstruction

## Result

Phase 1 can reproduce the original input ROM byte for byte with one non-interactive command:

```powershell
uv run sf2 verify
```

The verified input and output are both 2,097,152 bytes and both have SHA-256:

```text
9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9
```

`fc.exe /b` returned exit code 0 with no differences. The build output is deleted after a passing
run unless `Invoke-Sf2Rebuild.ps1 -KeepBuildArtifacts` is used; all such output remains under the
ignored local workspace.

## Evidence Set

| Evidence | Pinned value |
| --- | --- |
| ROM identity | [`manifests/roms/sf2-us.json`](../../manifests/roms/sf2-us.json) |
| SF2DISASM repository | `https://github.com/ShiningForceCentral/SF2DISASM.git` |
| Baseline branch role | `master`, original-game reconstruction |
| SF2DISASM commit | `c834c652b6862bc5679fd7f69a38a7093206efc6` |
| Build-tool hashes | [`manifests/toolchain.json`](../../manifests/toolchain.json) |
| Local Java | Eclipse Temurin `17.0.19+10` |
| Java archive SHA-256 | `B5B235C48ADF6A081874B812C630B9F4B5F637B7A5ED18B9174D08A41EC4C235` |
| H3 emulator | BizHawk `2.11.1`, Genesis Plus GX |

The ROM MD5 and SHA-1 also match the public USA-version records at
[TASVideos](https://tasvideos.org/615G) and
[RetroAchievements](https://retroachievements.org/game/75/hashes). The Java archive comes from
[Eclipse Adoptium's Temurin distribution](https://adoptium.net/temurin/releases/), whose archive
installation guidance recommends verifying the published SHA-256 checksum.

The pinned SF2DISASM commit contains no file named LICENSE, LICENCE, COPYING, or NOTICE. It is
therefore a local research dependency, not vendored or redistributed project source. Its five
executed build tools are individually size/hash checked before every H1 run. Microsoft Defender was
enabled and completed a custom scan of the upstream `tools/` directory before first execution; that
is useful hygiene, not a proof that old unsigned binaries are safe.

## Reproduction Pipeline

`sf2tool.harness` performs the following rails. During the accepted Python migration, maintained
rails run natively and the remaining proven H1-H3 PowerShell implementations are isolated behind
one frozen compatibility adapter:

1. Validate the ROM manifest against its JSON Schema.
2. Recompute file hashes, parse the Mega Drive header, and independently recompute the checksum from
   16-bit big-endian words starting at ROM offset `0x200`.
3. Verify the upstream remote, exact commit, clean tracked worktree, and five executable hashes.
4. Verify the project-local Java 17 runtime. Java is for the SF2 editor ecosystem; vanilla H1 does
   not execute the Java editor JARs.
5. Copy the canonical ROM into the ignored upstream `rom/sf2.bin` path.
6. Run `splitrom.exe` from the disassembly directory using `split/sf2splits.txt`.
7. Reassemble the sound driver and two music banks with AS Macro Assembler/P2BIN.
8. Assemble `disasm/sf2.asm` with `VANILLA_BUILD=1`, then run `fixheader.exe`.
9. Compare the rebuilt ROM against the canonical input with `fc.exe /b` and SHA-256.

The project wrapper intentionally does not call the upstream batch files: they use `pause`, derive
timestamped filenames through `wmic`, and rely on caller state. The wrapper preserves their actual
tool order and arguments while making exit-code handling and provenance explicit.

## Local Setup

The setup script never obtains a ROM. Given a user-supplied compatible ROM, it validates the input
before copying it into `local/`, fetches only the pinned SF2DISASM commit, downloads the pinned
Temurin and BizHawk archives, verifies their size/hash, extracts them locally, and scans the upstream
tools when Microsoft Defender is available:

```powershell
uv run sf2 init --rom-path <ROM path>
uv run sf2 verify
```

No system PATH or system Java installation is changed.

## What H1 Proves

**Confirmed:**

- The local ROM is the intended USA baseline and its header checksum is internally consistent.
- At the pinned commit, the selected split table, extracted data, sound/music assembly, main 68000
  assembly, and header fix can reconstruct that baseline exactly on this Windows environment.
- The project wrapper detects changes to the input identity, upstream commit, tracked upstream files,
  or executed build-tool binaries before accepting a rebuild.

## What H1 Does Not Prove

**Unknown or not yet covered:**

- Whether every upstream symbol name, comment, data boundary, and inferred mechanic is semantically
  correct. Byte-perfect reconstruction validates composition, not documentation quality.
- Whether the Java editors produce deterministic or semantically correct exports; no editor JAR was
  executed in Phase 1.
- Runtime parity across emulators or real hardware. H3 currently locks one BizHawk/Genesis Plus GX
  path with RNG, stat-gain, and complete level-up fixtures; it is not cross-emulator or hardware validation.
- Licensing permission to redistribute upstream disassembly code, executable tools, extracted game
  content, or rebuilt ROMs.
- Any modern-engine design choice. H1 only establishes the original evidence baseline.

## Next Research Contract

Phase 2 continues from the static core-data contracts and RNG/growth H3 fixtures into boundary,
turn-order, and damage behavior fixtures. Findings should cite pinned assembly symbols and
ROM ranges, while uncertain mechanics move into the explicit behavioral-test queue rather than the
data model.
