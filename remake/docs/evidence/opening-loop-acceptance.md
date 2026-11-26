# Composed opening-loop acceptance

## Outcome and evidence boundary

**High rigor:** this is the final cross-system state/recovery acceptance for
[#1425](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1425) under the existing
[opening lifecycle agreement](../application/opening-session-lifecycle.md). Its endpoint is usable
Battle01 return and resumed play after Save, actual Quit and a distinct-process Continue.
The same ordinary New Game must include preparation, a reachable pickup/description and the real
Church caller. The accepted [branch inventory](../content/opening-branch-inventory.md) defines
required opening consumers and future boundaries; success on this route does not admit every
optional source branch or later campaign. [UI gaps](opening-ui-gaps.md) retains human/aesthetic scope.

The historical composed observation used accepted `cf7a511f2d909c40c74cb72f2c828319d81f9d29`, tree
`97f7693249ef33c08e18a97b21c05ed7552a26b2`. Its documentation-only acceptance retained that built host.
The observations below are executor evidence; merged
[PR1432](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1432) owns independent review and
integration evidence. These receipts have not been rerun at the current UI preparation base.

## Current human play preparation (#1456)

**Standard rigor:** prepare a usable review route against accepted
[`b78f630fe754ae6e7b7e610acc1a8761cf0e1718`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/b78f630fe754ae6e7b7e610acc1a8761cf0e1718)
after integrated #1451–#1455. Reuse its already compiled, independently native-observed Debug host,
existing imports and current private entry/world/battle selections. This documentation/navigation
slice does not change the High save/recovery agreement or require another all-opening run.
Engineering readiness is **Confirmed** by direct configuration/build-path checks and the accepted
finite observations in the [host owner](../verification/host.md). Current whole-route readability,
feel and aesthetic approval remain **Unknown** until the user plays. The
[gap matrix](opening-ui-gaps.md#scope-and-reading-rules) preserves #689's actual bounded human approval.

Load the owning checkout's ignored private-input and prepared entry selections in the launching
PowerShell process and set `$repoRoot` to that checkout's absolute root. Keep
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Set an absolute `SF2_SAVE_ROOT`
outside every checkout and a new `SF2_SAVE_NAMESPACE` used only for this disposable review; check
that it is unused. Reuse that pair for Continue. Concrete selections and copyable commands live in
the ignored `local/issue1456/PLAY.md`; private content remains in place. Do not select an input-settings
override for ordinary review, so the delivered Settings/Apply/persistence path stays available.

```powershell
# After loading the owning checkout's selections and fresh external save namespace:
Remove-Item Env:SF2_PRIVATE_CONTROLLED_START -ErrorAction SilentlyContinue
& $env:GODOT_BIN --path (Join-Path $repoRoot 'remake/game')
# After actual Quit and process exit, repeat the same launch, then choose Continue.
```

Use ordinary title/Witch/New Game rather than a probe or controlled-start argument. Gameplay needs
a [supported desktop client](../godot/ui-scene-review.md#desktop-clients). Use Restore or fullscreen from recovery when needed.
A 1080p desktop's decorations/work area and mixed-DPI behavior still need the
[physical/display review](../verification/host.md#later-physical-focus-human-checklist-1456).
No launch, SDK/build, export, private-world copy, save conversion or new runtime observation is
needed for this preparation; availability checks do not establish human acceptance.

Play in short segments; a complete uninterrupted replay is unnecessary:

1. At the Witch action menu, inspect Settings and grouped Controls help, Cancel a draft, then Apply a desired
   preference. Choose New Game in a disposable slot; use the physical keyboard for the admitted
   name (up to seven characters). Check focus, selected/unavailable distinctions and confirmation wording.
2. In Map3, walk/talk, read/reveal/advance dialogue and regain control. Open Party → Prepare; inspect
   Equip/Give/Use/Magic/Drop and back out or make a deliberate legal change. Try a reachable Search
   pickup/description. At the real Church caller inspect Raise/Cure, decline and return; Promotion
   remains unavailable. Review the next legal action rather than requiring a positive status ailment.
3. Continue the castle/palace/Astral/tower route into Battle01. Check eligible intro Skip separately
   from dialogue Confirm; read commands, unavailable reasons, targets, HP/MP, action/results and
   EXP/gold/growth. Finish victory and usable field return when reviewing R1. Ordinary defeat/retry
   is a separate supported R2 segment; no loss needs to be manufactured during preparation.
4. At a supported settled field, use Save (button or mapped Stay), read the destination and explicit
   confirmation, then actual Quit. Wait for process exit, relaunch with the same selections/root/
   namespace and choose Continue. Move or interact to check resumed control and retained name/party.

The existing [restart recipe](../verification/host.md#opening-save-and-continue) supplies optional
joined-Map3, victory-return and defeat/retry segments. Reuse an existing checkpoint only when its
content/rules identity matches the selected world and its original namespace is explicitly disposable;
do not convert/copy saves or reinterpret a controlled start as ordinary New Game. No checkpoint is
required for the fresh route above. Storage corruption/backup and broader failure matrices retain
their accepted engineering evidence; this review needs only safe visible refusal/Cancel checks.

For each short segment, record the matrix IDs, actual device/display conditions, visible result,
feedback and explicit accept/fix/defer/Unknown decision. Include dialogue/control return, reading
comfort, selected/unavailable states, battle results, Save/Continue and the linked physical-focus/
display checklist. Absence of a user result remains pending, not an environment blocker.
[#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456) stays open after engineering
integration until the user explicitly accepts the chosen scope. #1447 title animation/music and
#1463 Appearance remain deferred; no new art direction, full campaign or gameplay expansion is implied.

This is remake product evidence. Original facts retain their named source/H2/H3 owners and pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` plus the registered USA input identity.
No original reset, hardware restart, menu raster, exhausted-sprite or timing claim is added.

## Pre-voyage showcase (#2245)

**Standard rigor.** [#2245](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2245)
uses the accepted opening to plan a short feature reel and an ordinary-input functional smoke.
The planned continuous journey runs from New Game through a Battle01 action, Save and Quit,
and playable Continue in a new process. Magic and boarding have independent starts.
[#2257](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2257) targets a compact
30–45 second pilot from ordinary opening Skip through Continue and one Battle01 decision.
Full editing, release review and distribution belong to
[#1825](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1825).

### Launch and checkpoints

Use the owned Debug adapter and installed Godot with the
[locked SDK workflow](../development-and-verification.md#locked-net-workflow). For visible play
and recording, select the directory containing the configured `DOTNET_BIN` as `DOTNET_ROOT` in
the launch process. Set `DOTNET_ROOT_X64` to the same value because Windows x64 Godot checks that
selection first. The registered installation was checked for the locked SDK, `hostfxr.dll` and the
.NET 8 runtime family used by Godot. [Godot host resolver at `ed1daf0bf`](https://github.com/godotengine/godot/blob/ed1daf0bf/modules/mono/editor/hostfxr_resolver.cpp#L269-L308)

Select a named predecessor in the [campaign showcase table](../application/pre-voyage-campaign.md#showcase-starts-and-continuity).
Use its exact registered record for an independent branch. The continuous route starts in a
fresh namespace and keeps its newly published records across the cold launch. A slot number
only identifies a record within that namespace. On Continue, read the displayed summary and
choose the occupied record. Slot 2 can follow an empty Slot 1.

For development Main use the current complete Kiwi gameplay, HD opening, Witch and Chinese
catalog publications from the [candidate recipe](../windows-local-candidate.md#build-and-run).
For the existing #2336 Release, packaged startup selects those inputs beside the executable.
Use the same disposable copy and settings preparation for either host.

```powershell
. ./local/private-inputs.ps1
$repoRoot = (Get-Location).Path # Run at the owning checkout's root.
$shared = $env:SF2_SHARED_INPUT_ROOT
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:DOTNET_ROOT = Split-Path -Parent $env:DOTNET_BIN
$env:DOTNET_ROOT_X64 = $env:DOTNET_ROOT
$package = Join-Path $shared 'derived/issue2316/kiwi-v1'
$entry = Join-Path $shared 'derived/issue2255/witch-entry-v1'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $entry 'entry.json'
$env:SF2_DISPLAY_CATALOG = Join-Path $shared 'derived/issue2266/catalogs-v1/world.zh-CN.json'
$env:SF2_ENTRY_DISPLAY_CATALOG = Join-Path $entry 'entry.zh-CN.json'
$env:SF2_PRIVATE_OPENING_CONTENT = Join-Path $shared 'derived/issue2324/opening-hd-v1/opening/opening.json'
foreach ($file in Get-ChildItem -LiteralPath (Join-Path $package 'opening-inputs') -Filter 'SF2_PRIVATE_*.json' -File) {
    [Environment]::SetEnvironmentVariable($file.BaseName, $file.FullName, 'Process')
}
Remove-Item Env:SF2_PRIVATE_CONTROLLED_START -ErrorAction SilentlyContinue
# Supply the absolute disposable $saveRoot, $namespace and a fresh $runName for each process.
$env:SF2_SAVE_ROOT = $saveRoot
$env:SF2_SAVE_NAMESPACE = $namespace
# Set $checkpoint to a shared selection from the table on the first launch of that segment.
# Use $null for New Game and subsequent launches in the same namespace.
if ($checkpoint) {
    $namespaceRoot = Join-Path $saveRoot $namespace
    if (Test-Path -LiteralPath $namespaceRoot) { throw 'Select a fresh disposable namespace' }
    $source = Get-Item -LiteralPath (Join-Path $shared $checkpoint) -ErrorAction Stop
    $slotDirectory = Join-Path $namespaceRoot 'sf2-opening'
    [void][IO.Directory]::CreateDirectory($slotDirectory)
    $destination = Join-Path $slotDirectory $source.Name
    [IO.File]::Copy($source.FullName, $destination)
    (Get-Item -LiteralPath $destination).IsReadOnly = $false
}
$runRoot = Join-Path $repoRoot "local/issue2343/$runName"
$env:APPDATA = Join-Path $runRoot 'appdata'
$env:LOCALAPPDATA = Join-Path $runRoot 'localappdata'
$env:TEMP = Join-Path $runRoot 'temp'
$env:TMP = $env:TEMP
foreach ($directory in $env:APPDATA, $env:LOCALAPPDATA, $env:TEMP) {
    [void][IO.Directory]::CreateDirectory($directory)
}
$settings = Join-Path $runRoot 'settings.json'
Set-Content -LiteralPath $settings -Encoding utf8 -Value '{"formatVersion":1,"language":"en","textMode":"instant","runInBackground":true}'
$env:SF2_DEBUG_PORT = '42332'
$argsForGame = @('--resolution', '1920x1080', '--max-fps', '120', '--log-file', (Join-Path $runRoot 'game.log'),
    '--', '--input-settings', $settings)
& $env:GODOT_BIN --path (Join-Path $repoRoot 'remake/game') @argsForGame
# For the registered Release, launch its Sf2Remake.exe with @argsForGame on the owned desktop instead.
```

Use a supported visible gameplay client, preferably 1920×1080, with default mapped controls. Choose
settings through production Main. Use the same inputs and acceptance boundaries with recording on
or off. Record the actual slot, phase, selected action, before/after result, input return and any
engine, projection or audio error at step boundaries. Compare the saved and resumed progress with
the existing persistence observation. For a manual pass, record what the real UI proves and leave
unread state fields unverified. The [host owner](../verification/host.md#opening-save-and-continue)
and [battle save owner](../verification/battle-save.md#method-and-reproduction) name the existing
read-only state and projection calls.

### Shot order and functional checks

| Shot | Ordinary start and input | Pass evidence and retry point |
| --- | --- | --- |
| Title, Witch and naming | Fresh namespace. At the Witch select Settings → Display → Language, Apply Simplified Chinese, then New Game in an empty slot. Enter a valid name of up to seven characters, confirm Start and reach Granseal. | Title and choices render in the selected language, the confirmed name survives, and the field accepts a direction. Retry from the Witch before choosing a slot, or use a fresh namespace for a new take. [Entry and naming](../godot/ui-scene-review.md#2-new-game-name-and-slot-choice) |
| Walking, dialogue and artwork | Continue the same New Game. Walk out through the starting house threshold, talk to the opening speakers, follow Sarah, school and messenger input, and Search the reachable school pickup. Use Settings → Display → Graphics to switch Original/HD in the same settled scene, Apply, then move. | Show the house foreground across the threshold, acknowledgement returning control, the concise current guidance and the item/description. Repeat Search once for no duplicate grant. Compare position, party and pickup receipt on both sides of Apply. Retry the graphics comparison at the same settled field. Repeat the pickup take from an earlier legitimate save or a fresh take before collection. [Opening route](#reproduce-the-composed-route) · [Field UI](../godot/ui-scene-review.md#3-exploration-world-speech-choices-and-search) · [Guidance](../capability-status.md#modern-guidance) |
| Party, equipment and Church | Still on that journey, open Member → Prepare. Show all four item positions and one item's description. Select Unarmed then legally re-equip Bowie's Wooden Sword. At the actual Granseal priest, inspect a service, return from it, and use Record to Save and Return at the explicitly selected field destination. | Check the equipped marker/attack change and return to the party and walking controls. Read the ordinary service result/refusal and require successful publication at the named field destination. Retry equipment or service after Back, or continue from the successful field record in a fresh take. [Field and Church](../godot/ui-scene-review.md#4-party-inventory-and-preparation) · [Field save](../verification/host.md#opening-save-and-continue) |
| Battle01, one resolved physical attack | Follow the actual castle/palace/Astral/tower route from the same field. At player Movement, Cancel for inspection, Back, move to a legal stop, confirm, then Attack and choose a living in-range enemy. Advance ordinary turns if no target is currently in range. | See Movement → Main → Target → action/result → usable player control. Check the actual target and outcome including HP and any awarded EXP/gold, once. If the hit misses, retain its result. Retry a failed take from the legal field save or a battle save produced on this same journey. [Battle decision](../godot/ui-scene-review.md#8-battle-one-complete-decision-then-its-consequences) · [Tower route](../verification/host.md#opening-save-and-continue) |
| Battle Save and Quit, cold Continue | At the next settled player Movement use the production Menu's Save, select a different slot from the field record, confirm Save and Quit, and wait for the native process to exit. Launch the same command and Continue that battle slot. | Observe the slot label/confirmation, actual process exit and distinct new PID. Require the same encounter, turn/actor, board/resources and usable Movement, with no replay of the completed attack. Take a legal preview step after restore. Retry Continue from the successful battle publication. If Save refuses, keep the refusal and resume at eligible Movement. [Battle persistence](../verification/battle-save.md#confirmed-observations-and-limits) |
| Independent learned BLAZE and HEAL | Use the independent `map16` record. Continue slot 2, then take the Map16 `(26,31)` and Map18 `(9,29)` gates to Battle06. On Kazin's turn choose a learned BLAZE rank with a legal enemy or area. On Sarah's turn use learned HEAL 1 on a living wounded ally after ordinary damage. | Verify the displayed rank, explicit target, one MP debit, damage/healing, art and return to Movement. Retry from a fresh copy or a newly written settled turn on that branch. The retained Battle04 Kazin and Battle01 HEAL observations establish the mechanics. [Battle04 cast](../application/pre-voyage-campaign.md#battle04-and-the-galam-prison-handoff) · [Galam gate](../application/pre-voyage-campaign.md#reproduce-the-south-gate-entry) · [HEAL](../verification/battle-save.md#confirmed-observations-and-limits) |
| Independent boarding tail | Continue slot 1 from the [boarding predecessor](../application/pre-voyage-campaign.md#showcase-starts-and-continuity). From the post-Battle07 tower return, descend to the castle, walk across the damaged town and take the east docks exit. Talk to the captain, decline once, then affirm on a fresh Talk. | No retains docks control. Yes resolves the crossing. Show regained Map14 control at `(40,8)` and one legal step. End before the plank or Battle08. Retry from a fresh tower copy. Lead the edit with the docks decision if the approach runs long. [Tower and ship handoff](../application/pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) |

### Pilot and recording boundary

Start production Main with English instant text and background playback enabled as above.
Use the [external semantic client](../../tools/pre_voyage_showcase.py) from another shell,
once per freshly launched named record. Each command expects an active opening. The terminal
records only selected decisions and positions. Inspect the corresponding native log for errors.

```powershell
python -B remake/tools/pre_voyage_showcase.py 42332 battle01
# On a fresh instance prepared from the Galam record:
python -B remake/tools/pre_voyage_showcase.py 42332 map16
```

The functional path waits on current projections with a deadline and no reading hold. For a
future pilot take, the optional `--reading-hold 1.5` adds wall-clock pauses at settled shots.
Movie time for this external driver has not been measured. The accepted #2257 movie below
retains its own pacing and receipts.

**Confirmed:** on the #2336 Release from product `3077da05`, handshake v2, module
`fc513454-ac4d-4644-b5eb-cd72b98783f3`, two fresh disposable namespaces on inactive Windows
desktops admitted their respective unedited records. Both checked the title music, Witch,
default Continue, question 225, selected slot, recognition 226 and farewell 224.

| Start | Observed native boundary |
| --- | --- |
| `battle01`, PID 24540 | Chester at (7,18), round 1 queue 0. Inspection/Back retained the turn. Up previewed (7,17), Main opened, Stay committed Chester there, and Bowie at queue 3 previewed (8,17). Battle theme 1, no reported projection/audio failure. |
| `map16`, PID 12764 | Continue slot 2 returned six active members and 1,072 gold at (26,30). Down crossed to Map18 (9,10). The south gate led to texts 2342–2356 in order. The same session and gold, six living deployed allies, F407/F456 set and F506 clear reached Battle06 Movement. Confirm opened ActionChoice and Back returned control. |

Both clients exited 0 and both disposable primaries matched their registered sources afterward.
The first Release attempt using headless defaults opened 64×64 window recovery before the
opening. A supported 1920×1080 client on an inactive desktop supplied the production window
boundary. Existing package reused in place, one game at a time, about 4 MB total local scratch
including attempts. No new capture. Full continuous Save/Quit/cold Continue, casting and boarding
on the current package remain **Unknown** for this showcase.

### Compact pilot evidence (#2257)

**Confirmed.** The visible Godot 4.7.2 take used the MMNK campaign, Witch entry and four-part
opening from #2252. The adapter included PR #2263 at
`8a776f2bdb849680d3b7c667792157f4746639ad`, the independently reviewed command-list frame
dependency. The Movie Maker writer finalized 1,977 frames in 32.95 seconds of movie time,
with 39.6 seconds of wall time and 15,681,171 bytes of OGV at 1920×1080 and 60 fps.

The visible and background runs each exited 0 and matched across 14 key presses and 15 state
samples. Ordinary Skip released to the title. The Witch defaulted to Continue and showed 225,
then Slot 1, recognition 226 and farewell 224. Chester's inspection and Back preserved queue 0.
Up previewed `(7,17)`, Confirm opened Main and Stay committed Chester's tile. Bowie took queue 3
and previewed `(8,17)`. The command list opened at 27.22 seconds and Stay was submitted at
29.45 seconds. Each slot copy still matched its shared source byte for byte.

The background check sets the headless root extent before Main attaches so the same production
window validation can run. The first attempt remained at Godot's 64×64 headless default and
stopped before Skip. With the supported extent the route completed, with an intermittent
four-instance ObjectDB warning at headless teardown. The visible take completed without the
warning or script, gameplay, presentation or audio errors.

FFmpeg 9.1 decoded both streams to completion with
`ffmpeg -v error -xerror -nostdin -i $movie -map 0:v:0 -map 0:a:0 -f null -`.
Ten in-memory picture samples taken 15 frames after their receipt marks showed distinct, nonblank
shots, including the command list. Timestamp-based 60 fps sampling retains repeated display
frames when the decoder omits duplicate pictures. The headless Godot player's texture readback was
uniform, so its receipt establishes audio only. It played through 32.93 seconds and measured
1,450,628 non-silent stereo pairs, with a non-silent mix at the command-list mark. The matching
route receipt places the menu-switch cue at that mark. Human review of the compact take's
readability and listening experience remains **Unknown**. [FFmpeg 9.1 build](https://github.com/GyanD/codexffmpeg/releases/tag/9.1)

The read-only copy in `handoffs/issue2257/pilot-compact-1080p60.ogv` is byte-identical to the
verified take. The BRGN comparison file remains in the earlier handoff. Receipts remain in the
owning checkout.

### Retained BRGN pilot evidence (#2245)

**Confirmed with the BRGN inputs.** A five-second title sample produced 300 frames at 60 fps and
530,543 bytes. Its 1920×1080 picture and non-silent audio decoded in Godot. The pilot then produced
4,699 frames in 78.32 seconds of movie time, with a 94-second writer run and 28,884,168 bytes of OGV
(27.55 MiB). The title alone had a lower data rate than the full battle sequence.

Both the recorded run and the unrecorded functional run sent the same keys and reached the same
decision boundaries. Slot 1 mounted round 1 Chester at `(7,18)`, queue 0. Inspection and Back
preserved that turn. Up previewed `(7,17)`, Confirm opened Main and Stay committed the new tile.
Bowie received the next player input at queue 3 and Up previewed `(8,17)`. The receipt includes
the title, Witch and battlefield music and the menu-switch cue on Main. Both route processes
exited 0 with no script, gameplay, presentation or audio errors. The first unrecorded attempt
reported a script error when reading session audio on the title. The corrected observer uses
the entry view's audio reading until the battle mounts.

Godot's Theora player decoded the complete file through 78.31 seconds, with nonblank 1920×1080
samples at all ten checked shots. The picture changed across the Witch, slot confirmation,
battle mount, inspection, command list and next turn. The decoded mix had 3,443,671 stereo sample
pairs above 0.001 peak amplitude, with non-silent audio during the command-list shot. The audio
receipt ties the menu-switch cue to that shot. These results establish captured rendering and
engine audio delivery at the checked boundaries. Human playback review remains **Unknown**.

Both disposable slot copies still match the published primary byte for byte. The private finished
take is `handoffs/issue2245/pilot-1080p60.ogv` beneath `SF2_SHARED_INPUT_ROOT`. This read-only copy
was compared byte for byte with the decoded take. Run receipts remain local to the owned checkout.

**Confirmed checkpoint checks.** The Debug build at `f92a02bc` succeeded with the selected SDK.
With the BRGN inputs, separate headless/Dummy invocations of the existing
[battle save observer](../../game/probes/engine_battle_save_observation.gd) opened the title and
Continued both copied slots. Use the [observer launch](../verification/battle-save.md#method-and-reproduction)
with format 1 English instant text (`{"formatVersion":1,"language":"en","textMode":"instant"}`),
`--max-fps 120` in place of `--fixed-fps 60`, a separate report/log per run,
and no controlled field input. `SF2_SAVE_CASE=field-continue` ended at playable Map58 control and
exited 0. `SF2_SAVE_CASE=shape` reached Battle01 round 1 Chester Movement, then the observer's
indexed save sequence opened `confirm-quit` and timed out waiting for `save-slots` (exit 1).
The latter did not exercise publication or the intended Stay, and reported two ObjectDB instances
at exit. Both processes exited. These receipts check checkpoint admission. The combined current
opening smoke and current battle Save/Continue after an action remain **Unknown** for this showcase.
The named prior acceptance observations retain their scope.

## Retained dependencies

| Boundary reused without another full run | Accepted owner and supporting evidence |
| --- | --- |
| Genuine initialization and title replacement | [New Game](../verification/host.md#opening-new-game-observation); accepted #1420/PR1427. Only ordinary input joins Sarah/Chester; diagnostic checkpoints are not product starts. |
| Save compatibility, atomic write/recovery, ordinary defeat and retry | [Save/Continue](../verification/host.md#opening-save-and-continue); accepted #1421/PR1428. Separate native/UI and storage controls cover cancellation, overwrite, corrupt-primary recovery and fresh-process retry. |
| Preparation legality and changed valid states | [Field preparation](../verification/host.md#opening-field-preparation); accepted #1422/PR1429. Actual behavior assertions and retained native inputs cover Unarmed, Give/full-pack exchange, Drop/Cancel, field NoEffect and caller-specific poison cure. |
| Optional branches, receipts and safe future stops | [Search observation](../verification/field-programs.md#opening-search-and-receipts) and [inventory](../content/opening-branch-inventory.md); accepted #1423/PR1430. Direct-castle refusal/return, reachable chests, full-pack retry and reconstruction retain their own scope. |
| Required services | [Church](../verification/host.md#opening-church-services); accepted #1424/PR1431. Real battle-earned gold, defeat, dead Sarah, decline/retry, paid Raise, settled service Save and distinct-process Continue preserve recovery/payment. Engine cases vary levels, caps, status, funds and stale/duplicate answers. |

The live opening has no Shop caller: only excluded F609 tables contain the scoped Shop calls.
The fixed merchant obstructs the empty shop chest's opening approach. Opening docks are blocked;
the returned Map57 stair row has no opening warp marker. Future-destination preflight rejects before
committed transfer. These source/inventory and engine boundaries are reused, not replaced by
teleporting a player or changing flags to manufacture native reach. F506 and later campaign stay out.

## Reproduce the composed route

This is the historical #1425 engineering recipe, separate from the current human play guide above.

Load the owning checkout's ignored `local/issue1045/inputs.ps1` in the launching process; keep absolute
configured SDK/Godot selections and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Select the existing
Search-and-Church-complete `world-services-issue1424.json` through `SF2_PRIVATE_EXPLORATION_CONTENT`.
Reuse the installed Godot 4.7.2 .NET host, project and imports. No rebuild or asset export is required
for this documentation-only acceptance. Unset `SF2_PRIVATE_CONTROLLED_START`, provide no game-entry
argument, and use a fresh disposable namespace under the configured external save root.

The retained `local/issue1425/opening-loop.gd` extends the existing #1424/#1423/#1422 native drivers.
It reads actual state/projection and sends mapped keyboard/controller/pointer events to ordinary
controls. Existing fixtures/navigation data supply directions only; no snapshots, seeds, expected
results or later gameplay states are installed into the session.

1. Choose New Game. In Party Status → Prepare, select Unarmed and then re-equip the displayed Wooden
   Sword. Use Medical Herb and verify the source field NoEffect without payment or RNG changes.
2. Complete the actual house/Sarah/school/messenger sequence. Give Chester's herb to Bowie. Search
   the reachable school pickup, repeat it without a second grant, then read the school description
   and dismiss it to usable field control.
3. Reach the priest through ordinary counter interaction. Inspect Raise and Cure with no applicable
   target, dismiss them and leave through the actual caller. Service wait forbids Save; returning
   leaves the carried party, gold and both RNG values unchanged and field input usable.
4. Walk the actual castle/palace/Astral/tower route and finish before/load/start into Battle01.
   Read live joined actors and compare their complete item/spell words, resources and attained
   progression against the prepared field party. Execute legal movement, attack and HEAL commands.
   If defeated, preserve the real outcome; ordinary Raise/retry may continue that same game. A
   defeat is not victory acceptance. No reward, death, recovery or outcome state is injected.
5. Finish the real victory program and require settled Map57 field control. Compare the final
   battle resources/progress/custody with the returned party and verify the retained pickup receipt.
   Save slot 1 through the ordinary field menu, confirm its result, then use the actual Quit button,
   inspect its confirmation panel and submit Quit. Wait for the process to end.
6. Launch a distinct OS process with the same content/root/namespace. Continue slot 1. Compare every
   detached progress field, including all retained allies, full item/spell words, status, EXP,
   kills/defeats, levels/maxima, gold, flags/receipts, custody/Deals, RNG, save recipe/anchor and music.
   Require a new session identity, then perform a legal move and Search/dismissal. These actions must
   not replay pickup, reward, payment, joins, initialization or victory. Quit normally again.

```powershell
./local/issue1425/run-native.ps1 -Run <fresh-composed-output> -Namespace <fresh-namespace>
./local/issue1425/run-native.ps1 -Run <fresh-continue-output> -EntryMode continue -Namespace <same-namespace>
```

The existing 125.8 MB world is read in place. Serial processes, selected semantic boundary records
and compact logs suffice; reports are budgeted below 30 MB with a 1 GB free-space replan floor.
A genuine defeat/retry can extend the finite route; at most four battle attempts are allowed, with
the existing 80-mark cap and no concurrent runtime. There is no screenshot, per-frame capture, new comparator/framework or test
of a helper. Direct state comparisons inspect the recorded boundary values themselves.

## Observed result

**Confirmed, bounded remake execution:** `composed-05` completed one ordinary New Game through
preparation, pickup/repeat/description, actual Church return, Battle01 victory, Map57 Save and actual
Quit (659 mapped keys; 103,864 ms). It won its first battle attempt; the optional retry branch was not
needed in the passing run. `continue-01` launched a distinct process, restored slot 1, moved from
Map57 `(7,8)` to `(7,9)`, opened/dismissed Search and quit normally (6 mapped keys; 965 ms).
Both native stderr logs are empty. The composed process ended before Continue launched; its wrapper
had yielded after 45 seconds, so a final OS exit code was not retained. Continue reported exit 0.
Actual Quit confirmation was observed in both; no Godot/testhost remained afterward.

Direct comparison of selected real boundary values in `local/issue1425/native-summary.json` establishes:

- Unarmed changes the equipped word; re-equipping restores the starting party. Field Herb NoEffect
  preserves the party and both seeds. Give moves Chester's herb into Bowie's pack; the school pickup
  adds one item and source receipt 128. Repeat gives nothing; description and Church return preserve it.
- The same session enters Battle01 with all three joined members' carried HP/MP, EXP, kills/defeats,
  status, level/maxima, defense and complete item/spell words. Gold and custody match the field.
- Actual `battle-outcome: Victory` precedes usable Map57 with F501 and the pickup receipt. Gold 360,
  earned EXP/kills, Sarah's attained level 2, items/spells and custody survive the outcome. The existing
  [victory policy](../application/session-and-programs.md#battle01-outcome-after-program-and-return)
  restores eligible living HP/MP; this is not an equality assertion across intentional healing.
  The unjoined reserve record stays unchanged. Story/outcome execution may advance RNG; those seeds
  are compared exactly at the subsequent save/restore boundary, not frozen across gameplay.
- All 24 detached progress fields match between saved and fresh-continued state, including every
  retained ally, full words, resources/progression/status, gold, story/item receipts, battle once-flags,
  Deals, both seeds, compatibility, recipe/anchor, text settings and music. Session identities differ.
  The subsequent move and Search return preserve the party, 360 gold and pickup/victory receipts;
  no item, reward, payment, join, initialization or outcome is repeated.

Preserved attempts: `composed-01/02` stopped in preparation because the driver selected the wrong
weapon label; the corrected input uses the displayed Wooden Sword. `composed-03/04` completed genuine
ordinary defeats and therefore failed the required victory condition. Their raw reports remain;
03's boundary label says “victory-return” but its actual Map3/defeat values establish a loss, not a
pass. Earlier-healing input planning in 04 was discarded. The passing 05 run uses the retained ordinary
battle planner and real inputs, with no seed override, state injection or production correction.
These failures do not invalidate the accepted child controls or become successes through this report.

## Remaining limits and verification selection

**Unknown:** original natural optional-route and service reach/timing; positive poison/stun naturally
arising in this opening; original exhausted sprites; original SRAM/power-loss parity; physical-device
and whole-opening human/aesthetic acceptance. Promotion, curse removal, Repair, buy/sell/Deals UI,
arbitrary saves, battle suspend, F506 and later campaign are outside this outcome. #438 remains open.

Preserve the completed inherited private aggregate (1692 pass/20 fail/1 skip) and its nine remaining
controlled-start `full-fade-state` diagnostics in `local/issue1423/inherited-failures.txt`, with the
#1421–#1424 handoffs. Those starts lack required display initialization. This ordinary product
route neither changes their fixtures nor turns those failures into passes. Accepted child runtime,
behavior and CI evidence remains valid by dependency; no blanket legacy aggregate is repeated.

The #1425 acceptance used direct diff/ownership, links/anchors/tables/fences and private-boundary
checks plus its then-new composed native observation. Current #1456 preparation uses only direct
document/scope/private checks and build/configuration availability inspection; no SDK, engine,
adapter, full suite or all-route rerun. On the clean committed candidate,
run `uv run sf2 verify plan --base origin/main --head HEAD`; report its actual selection and exact-head
CI in the handoff. A documentation-only CI skip is not a runtime pass. Main-gate independently reviews
the evidence before integration; current user acceptance remains pending separately.
