# Private local Windows candidate

## Agreement

Standard rigor: packaging crosses content selection, Godot/.NET export and persistence, but does
not change gameplay. [#2329](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2329)
under [#1825](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1825) owns this bounded
Windows x86_64 candidate. Acceptance requires a repeatable Release export, closed runtime inputs,
relocated clean-environment execution, ordinary New Game and cold Continue, one battle. The user accepted the local packaging outcome; natural voyage
validation is deferred and is not a packaging blocker. The current milestone is local candidate validation; public distribution,
CI artifact publication, signing, an installer and a hard story cutoff are outside this agreement.

The candidate carries the complete selected gameplay world, including later accepted content.
The pre-voyage evaluation ends after the captain's departure and usable Map14 arrival at (40,8),
Left. It does not enforce a demo wall. The existing [departure and arrival owner](application/map14-plank-deck.md)
and [save owner](application/battle-save-and-continue.md) retain endpoint/save legality; packaging
does not create a Map14 save recipe or disable the later route.

## Acceptance and follow-up boundaries

The user accepted the local runnable packaging result. **Confirmed:** the relocated exported
executable reached opening/title/Witch and New Game Map3, produced Witch/town music receipts,
applied English/Chinese and Original/HD settings, persisted Chinese/HD across restart, and completed
field Save/Quit/cold Continue. A controlled later-battle seed admitted an actual battle and an ordinary
attack advanced to the next actor. These observations used the exported Release executable, not an
editor substitute. Loaded native modules resolved only inside the candidate or Windows.

**Unknown:** natural pre-voyage departure/Map14 arrival and correct dock speaker portraits. The
reported portraits are tracked by [#2330](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2330)
and deferred until the separately requested debug capability is accepted. The #2053 controlled dock
seed explicitly preserves Slade as a follower with F73; Slade following the ship was traced to that
non-natural test setup, not a production follower defect. It cannot prove natural voyage acceptance.

[#2332](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2332) separately owns opt-in
C# network observation/control, replacing reliance on GDScript probes. No debug endpoint is included
here. The uncompiled named-pipe experiment and its CA1416 failure were retained in ignored local
handoff scratch, outside the compiled/exported source. It is not the requested network implementation.
[#2331](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2331) separately owns the persistent
background-continuation setting; default focus behavior is unchanged here. No new voyage test ticket
or follow-up implementation is part of this packaging handoff.

## Build and run

Load the checkout's ignored private-input configuration in the same process. Input paths and output
are explicit; no ROM extraction or asset generation occurs. Use the registered gameplay, independent
opening, Witch and catalog selections together. From this checkout:

```powershell
. ./local/private-inputs.ps1
uv sync --locked
uv run python -m sf2tool.remake_windows_package `
  --campaign "$env:SF2_SHARED_INPUT_ROOT/derived/issue2316/kiwi-v1" `
  --entry "$env:SF2_SHARED_INPUT_ROOT/derived/issue2255/witch-entry-v1/entry.json" `
  --opening "$env:SF2_SHARED_INPUT_ROOT/derived/issue2324/opening-hd-v1/opening/opening.json" `
  --catalog "$env:SF2_SHARED_INPUT_ROOT/derived/issue2266/catalogs-v1/world.zh-CN.json" `
  --templates "$env:SF2_SHARED_INPUT_ROOT/toolchains/godot-4.7.2/Godot_v4.7.2-stable_mono_export_templates.tpz" `
  --output "$PWD/local/windows-candidate-v1"
```

Use a fresh output name. Run from another cwd with `uv run --project <checkout> python -m
sf2tool.remake_windows_package` and absolute arguments. The command locates source from its package
root, validates required inputs/templates before export, copies only the declared content closure,
checks copied bytes, performs locked restore and uses the existing Common Game Windows preset.
Godot's RID-specific publish writes locks/import metadata in a disposable source copy, not in the
checkout. Its actual installed editor executable is used; the Windows console wrapper can remain
alive after its editor child exits. Logs and failed output remain available. No template download,
system PATH change, additional SDK or shared input mutation is performed.

Unzip and run `Sf2Remake.exe`. Keep the executable, separate PCK, `data_Sf2.Remake.Godot_windows_x86_64`
directory and `content` directory together. The PCK contains production resources and bundled fonts;
the data directory contains the self-contained .NET runtime and four production assemblies.
`candidate.json` records source commit/dirty state, template version, file sizes and elapsed build
time. Debug symbols and probe resources/classes are excluded from the candidate. Build from a clean
committed source for a reviewable final candidate; a dirty pilot is explicitly marked as such.

Packaged startup sets the existing readers' inputs relative to the executable, independently of cwd
and developer content variables. Settings and numbered saves default to
`%LOCALAPPDATA%/Sf2Remake/saves/local-candidate/`, with separate `sf2-preferences` and `sf2-opening`
subdirectories. The existing `SF2_SAVE_ROOT` and `SF2_SAVE_NAMESPACE` overrides support isolated
operator validation; their path/link/Git-ancestor checks still apply. Never select a real player
namespace for validation. Exported storage rejects the install directory; development storage retains
the checkout boundary. Copying an install does not copy saves or settings.

## Resource budget and verification

The selected closure measures 196 files / 408,278,969 bytes. Only Windows x86_64 Release template
members (109,702,656 bytes) are extracted from the existing 1.20 GB multi-platform archive. Closure
discovery is O(JSON size + dependency count); copy/byte comparison/ZIP are O(total selected bytes).
The largest world is parsed once for dependency discovery. Budget approximately 4.74 GB additional
disk for staging, a conservatively incompressible ZIP, one validation copy, template extraction and
2 GiB build/cache headroom; initially 25.5 GB was free. Keep exports and native launches serial.
Replan if a selected world substantially exceeds this input size or available space falls below the
reported budget. No API/model calls or acquisition are part of the build.

The corrected pilot measured 43.05 seconds for build, 61.44 seconds including ZIP, 621,569,018
candidate bytes and 381,695,265 ZIP bytes. These are measurements, not fixed size assertions.

Validation must launch the exported executable at a fresh path containing spaces with a different
cwd, developer content/tool variables removed and an OS-only PATH. Make the producer staging
unavailable without renaming shared immutable inputs. Use ordinary input and read-only process
state/projection; no screenshots. Stock templates disable external `--script` path overrides, so
an ignored GDScript supplied on the command line is not an executed observation. The existing
Windows PSS/ClrMD inspection approach can inspect the exported .NET process without adding a
probe to its package. Observe actual loaded module paths alongside content admission and saves.

Completed pilot failures remain evidence: the console-wrapper export wrote its output but required
termination of the stranded wrapper; the next locked restore rejected Godot's RID-mutated locks;
the attempted external GDScript observation was ignored by the stock template and timed out;
the first New Game attempt correctly rejected a save root inside a Git checkout. These do not
establish successful gameplay acceptance. Their local logs and PSS snapshots are retained.

**Unknown:** untouched second-machine execution, other OS/driver/device combinations, subjective
visual/audio quality and the broader [#1716 exit risk](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).
A clean environment on the development host is not a clean-machine claim. Original content, private
records, binaries, ZIPs and logs remain local; successful export grants no redistribution rights.
