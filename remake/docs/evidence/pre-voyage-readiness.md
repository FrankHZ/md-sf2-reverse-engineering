# Readiness audit for the first post-voyage slice

The audited baseline was `7c225fda347d74b3ee1e0fc62e699e616f860de7`; delivery at
`28193f7fab5af0620636bf1acdc1922a6f5b9c8f` added the three-line #1726 test correction.
Start at ordinary damaged-ship movement and stop after stepping onto the deck, reading and
advancing its first dialogue. A No or cancel on the plank choice should return control and allow
retry. Battle08 first input required additional map and battle content at this baseline. The
audit used Git, pinned source, bounded compiled behavior and hosted records. It did not launch
the game or emulator or use the player's saves. See [#1728](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1728)
and [the original audit revision](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1e9dc63868e9d4378e81856003e9691807b93500/remake/docs/evidence/pre-voyage-readiness.zh-CN.md).

## Acceptance and dependencies at the audited revision

- **Confirmed starting point.** Independent [PR #1563](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1563)
  reached Map14 (40,8), Left, F700 set, F703/F408/F508 clear and no pending script wait.
  Down moved to (40,9). The supported later field slot was on Map58. Use a disposable compatible
  slot, continue via the ordinary departure and cover refusal, retry, deck actors, text and input.
- **Confirmed Battle08 dependency.** The ordinary package had 18 maps and seven battles under
  gameplay revision 4. Map72 and Battle08 were absent. Broader acceptance needed actual identity,
  formation, terrain, scene, carried equipment and route inputs, and both optional Kiwi states.
  This did not block the deck slice. [Plank and Battle08 conditions](../../../docs/design/synthesis/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md)
- **Capacity and experience.** The reported HD world was about 121 MB against a 134,217,728-byte
  limit. The next delta was **Unknown**. The reviewed 806 Chinese world lines ended before
  the later deck interaction. The reader allowed 1,024 rows, 256 KiB and 8,192 UTF-16 units per
  message. Measure the new package and read the admitted dialogue in each supported language.
  Readability, controls/DPI and art acceptance remained human review. [Asset inventory](../content/asset-inventory.md)
  · [Chinese game content admission](../application/pre-voyage-chinese-admission.md)
- **Shutdown observation.** #1716 reported one access violation after the Chinese Continue
  terminal report without a separately retained native exit code. A repeat on the same input
  passed 13 checks and exited 0. Cause and ordinary-player impact were **Unknown**. [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)

## Source and engine boundary for the deck

**Confirmed static.** Map14 (45,5) invokes `cs_58E5C`; F257 is written after return and does not prevent
retry. Yes requests Map14 (13,11), Down. With F458/F703 clear the initialization runs `cs_58FA4`,
fades in, installs `customActscript 129` and starts the merchant's text. This differs from the
working-layout-preserving stairs and the earlier internal Map73 load. `customActscript` reduces
to `csc14 actor,0`, without waiting for actor idle. Installation initializes the timer from the
resolved actor selector's low byte. `ac_wait` supplies a word, but `esc00_wait` compares its low
byte with the current timer using signed byte ordering. A reached threshold clears the timer and
continues in the same service; otherwise the timer increments and yields. [Pinned Map14 source](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map14/mapsetups/scripts_2.asm)

**Confirmed remake.** `ac_wait` uses the operand word in `wait-timer`; `eaWait` and turn macros
sign-extend their duration byte. `ac_wait 255` is `0x00FF` and `eaWait 255` is `0xFFFF`, both
comparing as −1. Legacy and authored `wait`/ticks preserve unsigned semantics. The earlier
compiler left long 256, 600 and 1200 waits as operandless native calls. Recompilation supplies
the operand. The fixed Map14 script compiled with SlotTimer and `wait=false`. In a controlled
scene, slot 1's first service passed 1200 (signed low byte −80) and moved right. After the next
movement, 600 (low byte 88) incremented from 0 for 88 services before clearing and moving up seven
tiles. With initial timer 5 it took 83 increments. The older and recompiled `eas_Walking` supported
the controlled cursors 0/9; existing world and recompiled Map3 setup walks both passed start,
capture and restore. Natural original timing, ordinary plank traversal and text handoff needed
their own observations. [Program compiler](../../../src/sf2tool/remake_exploration_actions.py)

## Other implementation and delivery findings

- Stable field save queries select and attempt to rebuild the candidate before revision-bound
  capture. A changed revision yields `save-stale-revision`. Map58 used F507/74/640, excluded
  F407/999 and expected the map-local facing-3 anchor. Map3's setup-609 route with F603 goes to
  (32,13); F3 controls Jaha's reconstruction. The slot owner used per-slot locking and temporary
  readback, reporting `save-publication-uncertain` on uncertain publication. Recovery from a
  valid backup required an admitted primary failure. Ordinary settled restore did not replay
  OnLoad. [Independent saved route](../verification/map3-commission-save.md)
- The ordinary reader accepted one root and six additional battles, the controlled reader one
  root and one additional battle. Dependencies were relative to the world file. The package
  read battle identity and shared ally definitions; battle entry overrode default ally resources
  with actual carried state and returned actual results. The discrepancy explains why the
  normal seven-battle Continue and #1726 controlled reader had different limits.
- HD blocks had the expected count and 96×96 dimensions. Graphics preparation consumed the
  selected mode and material availability, while logic retained 24-pixel map cells.
  [PR #1724](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1724) observed reversible
  24→96→24 on the same paused progress with unchanged panel sizing. The menu-only publication
  instructions did not create the HD world; that needed a separate material and entry selection.
  [Asset production](../content/asset-inventory.md)
- F3 encounter rebuild used `WithOutcome`; F4 resource recovery used `WithResources`; F5
  target checks preceded silence. F6 checked actual defeat instructions, F7 transferred
  `CurrentEntities` and F8 resolved the music region with `ExplorationViewContext`.
  The Map20/F605 actor-130, sprite-209 adjustment remained a private binding. No script machine
  redesign was needed for the deck. [Prior quality review](structure-and-quality-review.md)
- The old research retained-failures link missed the `methods/` directory. The audit located
  [the actual evidence owner](../../../docs/research/methods/original-reference-replay-capability.md#current-lineage-hard-stop).
  The campaign tracking text had stale #1629/#1674 acceptance and the remake Chinese work
  entry understated Battle03 support. Update such routes against their exact accepted owner.
- The inherited local seven-battle configuration finished with **2,409 passed, 27 failed**.
  Targeted rereads under the intended selections passed 24 without changes. The remaining three
  methods passed after #1729 updated exact Mithril identity 123 and two `AllowChanges`
  expectations. The retained failure describes the historical run. The #1717 HD work's seven
  public checks were distinct from the private UI route. [#1726](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1726)
  · [PR #1729](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1729)
- The pre-voyage handoff reused the Map58 ordinary Save, Quit and separate-process Continue
  leading through the port branches. The later #1711/#1714 feedback distinguished the selected
  setup after the commission from follower motion. #1592 advanced 1/60-second ticks with a
  four-step callback bound and a 120-step menu wait limit. Records should identify commit,
  world/scene/entry, language, save selection, and the actual route and outcomes. [Feedback #1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456)
  · [Fix PR #1714](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1714)

## Reproduction

The first commands locate the historical change, reader configurations and source/hosting
evidence. The second block assumes the audited revision and pinned inputs, and uses that
revision's test paths. The selected actor installation is inserted in memory only. The two
timer operands' low bytes were 176 and 88. No broad gate run is needed to reproduce the
distinguishing signed comparison and asynchronous parent handoff.

```powershell
git diff 7c225fda 28193f7f -- remake/tests
git show 7c225fda:remake/content/private/pre-voyage-opening.json
git log -60 --first-parent --format= --name-only 7c225fda |
    Where-Object { $_ -match '\.(cs|py|gd)$' } | Group-Object | Sort-Object Count -Descending | Select-Object -First 10 Count,Name
git ls-tree -d --name-only 7c225fda
git ls-files '*original-reference-replay-capability.md'
git grep -n 'original-reference-replay-capability.md' 7c225fda -- docs/research/story-content-coverage.md
git check-ignore -v local/example.json remake/game/.godot/example remake/src/Sf2.Remake.Domain/bin/x
gh issue view 1717 --json state,projectItems,milestone
gh issue view 1456 --json body,comments,projectItems
gh pr view 1563 --json headRefOid,mergeCommit,comments,statusCheckRollup
gh pr view 1729 --json files,mergeCommit,body
```

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj --configuration Release --no-restore --filter 'FullyQualifiedName~SourceEntityWaitUses|FullyQualifiedName~AsynchronousEntityWait|FullyQualifiedName~AmbientDefaultWalking'
$motion = @'
import json, sys
from pathlib import Path
from sf2tool.remake_exploration_programs import OriginalPrograms
p = OriginalPrograms({"resources": {"standaloneScriptPrograms": [], "initSourcePrograms": []}}, Path(sys.argv[1]))
p.register_file("disasm/data/maps/entries/map14/mapsetups/scripts_2.asm")
p.compile("cs_58FA4")
print(json.dumps(next(row for row in p.programs["cs_58FA4"]["instructions"]
                      if row["op"] == "motion" and row["entity"] == "entity-129")))
'@ | & ./.venv/Scripts/python.exe -B -X utf8 - $upstream | ConvertFrom-Json -AsHashtable
$doc = Get-Content -Raw remake/content/authored/harbor-arrival.json | ConvertFrom-Json -AsHashtable
$motion.entity = 'ferryman'
$doc.world.programs[0].instructions = @($motion, @{op='end'})
$doc.start.program = @{program='invitation'; instruction=0}
foreach ($name in 'Domain','Application','Content') {
    Add-Type -Path (Resolve-Path "remake/src/Sf2.Remake.$name/bin/Release/net8.0/Sf2.Remake.$name.dll")
}
$bytes = [Text.Encoding]::UTF8.GetBytes(($doc | ConvertTo-Json -Depth 64 -Compress))
$reader = [Sf2.Remake.Content.Scenarios.AuthoredScenarioPackageReader]::FromDocumentBytes($bytes)
$session = [Sf2.Remake.Application.Runtime.GameSession]::Start($reader).Session
$actor = $session.Current.Exploration.AllEntities | Where-Object { $_.Entity.Value -eq 'ferryman' }
$actor.Actions.Actions | Where-Object { $_.GetType().Name -eq 'WaitEntityTimer' } | Select-Object Ticks
```
