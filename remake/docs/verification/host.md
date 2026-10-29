# Host and Input Verification

Startup, logical input and accessibility checks observe the actual ordinary Godot host.

Load the owning worktree's private-input configuration and protected SDK/Godot environment from
[development](../development-and-verification.md#locked-net-workflow) in the same launching process.
Unless a recipe explicitly changes directory, commands run from the repository root. Reuse the
existing instance when suitable and choose fresh ignored outputs. These are focused recipes, not
an aggregate checklist. No screenshots; no new tests of probes/helpers. Read completed failure and
Unknown boundaries before reproducing a claim. Full historical receipts remain in the
[pre-reorganization guide](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/development-and-verification.md).

## Ordinary Host Startup

After loading the retained worktree SDK/Godot environment, ordinary play uses:

```powershell
& $godotBinary --path remake/game
& $godotBinary --path remake/game -- --authored-package (Resolve-Path -LiteralPath 'remake/content/authored/garden-watch.json').Path
```

The same project accepts `--private-battle-start` and `--private-exploration-start` with the selected private inputs described below. These and `--authored-package` select mutually exclusive session entries.
An optional `--input-settings <path>` selects the versioned product settings below, independently of
the entry and in either argument order. Each option takes one path and may appear only once.
Unknown/positional, duplicate, conflicting entry and missing-path arguments report
startup ContentError in the common view before creating a session. They cannot select legacy play.

The external observer owns `SF2_OBSERVATION_CASE`, `OUTPUT`, `FOLLOWUP_SHAPE`, `TARGET_SHAPE`,
`ENEMY_SHAPE`, `CONTINUATION_SHAPE`, `LONG_PATH` and `REVERSE_KILLS` (all with the same prefix).
Unset/empty CASE runs the default observation; flags use1. Former `--observation-*`/shape options are
not game arguments. Each example assigns its case/output; optional shapes belong to that named case.
Diagnostics do not affect ordinary launches without the script. Direct startup observation uses:

```powershell
$env:SF2_OBSERVATION_CASE = 'startup'
$env:SF2_OBSERVATION_EXPECT_FAILURE = ''
$env:SF2_OBSERVATION_EXPECT_ORIGIN = 'public-authored-controlled-start'
$env:SF2_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'startup.json'
& $godotBinary --headless --path remake/game --script res://probes/engine_battle_observation.gd
```

For a rejected real-argument launch, set EXPECT_FAILURE to `unknown-startup-option`,
`duplicate-startup-option`, `conflicting-startup-options` or `missing-startup-path` and append the
corresponding arguments after Godot’s `--`. Private success uses EXPECT_ORIGIN
`private-local-controlled-start`. The script observes actual Main/GameRoot/BattleSessionView and
published startup state; it does not set engine state. Preserve process output and require passed:true.

The legacy `remake/reference/game` host, its `--map3-smoke`/profile arguments and their observations
were retired at M5. Ordinary export configuration excludes `probes/*`; complete ordinary export and
package contents remain unverified.

## Unified world viewport observation

The [Standard presentation agreement](../godot/presentation.md#unified-world-viewport-agreement)
owns 1920×1080,96px cells/ordinary frames and 4× nearest sampling. Build the actual Release adapter
with `uv run sf2 verify adapter` and Debug adapter with the configured absolute SDK after loading
`local/private-inputs.ps1`; force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the registered
Godot installation and owning project. A changed adapter binary requires a new native load; no
SDK, project or asset copy is needed. Screenshots and full/H4 replay are excluded.

`engine_world_viewport_observation.gd` loads ordinary `Main.tscn`, submits mapped keyboard events,
waits on actual input/delivery state and reads existing camera/layout/terrain observations. It
requests native focus before input and permits one bounded recovery, then reports Unavailable.
Select `SF2_VIEWPORT_CASE`, a fresh `SF2_VIEWPORT_OUTPUT` and the corresponding ordinary startup
argument. With the selected private inputs or authored package loaded, run from the repository root:

```powershell
$env:SF2_VIEWPORT_CASE = 'room'
$env:SF2_VIEWPORT_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'viewport-room.json'
& $env:GODOT_BIN --path remake/game --max-fps 60 --resolution 1920x1080 `
    --script res://probes/engine_world_viewport_observation.gd -- `
    --private-exploration-start $selectedStart
```

Require exit 0, `passed:true`, empty failure/unavailable lists, and no Godot script,
C# or draw errors. An output file or zero process status alone is insufficient. Reports retain
only a few semantic checkpoints, never a full scene per frame. A 75-second observer deadline and
small-MB reports bound each short case; inspect the room/entry pilot before adding other cases.

| Case | Existing input / bounded observation |
| --- | --- |
| `room` | Retained private connected Map3 world, initial player `(56,3)`, flags already compatible with ordinary field input. Actual 11×10 legal area gives 1056×960 centered at `(432,60)`. A small requested layout strip crossing the room's left edge contains only legal source draws; ordinary step/turn/release keeps 96px frames. |
| `warp` | Same room input. Two ordinary Left steps use the existing packed-room warp to town. Require the player visible immediately, new area bounds and full world rectangle; do not pan through unopened packed storage. |
| `entry` | Existing connected Battle01 start `battle01-to-battle02-start.json`, selected retained world/party. Cancel the eligible intro, release, wait for ordinary battle control. Read real map textures/sprites; open/cancel menus, move the preview and cycle item targets. Older private packages retain their explicit Unsupported Egress instead of changing gameplay to satisfy the observer. |
| `region` | Accepted Battle02 outcome world/party with `engine-battle02-egress-start.json`, from the [Egress owner](field-programs.md#battle02-egress-and-walking-retry). Actual map-region origin `(16,19)` maps terrain/actors into the shared viewport. Short ordinary Egress reaches usable Map3 and subsequent movement; no full battle is replayed. |
| `scene` | Same retained Map3 world, controlled initial player `(42,9)` and flags `0,32,256,260,261,601,602`, with the current explicit palette/text start fields. Down then Right naturally enters Zone8 at `(43,10)`. Existing caller runs `cs-5149a`; ordinary reveal/acknowledge reaches text525. Actual teacher ink gets high-priority wall fragments after the 96px sprite draw, with independent plane origins. Stop at that boundary. No direct text/program-state injection or private world duplication. |
| `far` | Small pre-start variation of authored practice yard: 32×20 open terrain, injured `guard-a` at `(29,17)`, MEND range 48. Move preview, choose self then distant target, cancel. World zoom remains 1 (96px board cells), the target pans inside legal bounds, and a corner description avoids the edge-clamped subject. Repeat with medic at `(1,1)` and require mutually disjoint visible panels around that subject. |

Create the small controlled starts before startup, preserving the tracked input's explicit palette,
text and display fields; this writes no private world or asset copy:

```powershell
$startData = Get-Content remake/reference/inputs/battle01-to-battle02-start.json -Raw | ConvertFrom-Json -AsHashtable
$startData.start.map = 'map-3'
$startData.start.position = @{ x=56; y=3 }
$startData.start.facing = 3
$startData.start.flags = @(0,32,256,260,261,601,602)
$selectedStart = Join-Path $env:SF2_RUN_OUTPUT 'room-start.json'
$startData | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $selectedStart -Encoding utf8
# For scene: change only the initial position to @{ x=42; y=9 } and save another start.
```

For `far`, use `--authored-package $package`; all other cases use their selected private start.
Case recipes share the explicit native options `--max-fps 60 --resolution 1920x1080`.
Private payloads, start variants, reports and logs remain in the owning ignored output directory.
No new observation establishes original hardware timing, all-map coverage, physical-device release
while another OS window owns focus, or final aesthetic quality. Preserve #689's entry timeout,
#1386's exit warning and #1389's focus-release Unknown; source geometry updates do not erase them.
Static-room runs can retain the existing two-ObjectDB exit warning; successful active cases must
still be checked for errors. Original icon namespace selection remains with the
[private icon owner](../content/presentation-assets.md#private-command-menu-icons); these map/sprite
checks do not claim a new original-icon acquisition or asset redistribution right.

## Shared story dialogue observation

The [Standard agreement/current treatment](../godot/presentation.md#shared-story-dialogue-agreement)
owns this bounded modern UI. Reuse the registered Godot, configured absolute SDK/shared CLI home,
existing project and prepared private content. Load `local/private-inputs.ps1` in each launching
process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, run `uv sync --locked`, Release
`uv run sf2 verify adapter`, and Debug `dotnet build remake/game/Sf2.Remake.Godot.csproj --no-restore`
using `$env:DOTNET_BIN`. A changed adapter requires a new native load; no asset/toolchain export or
copy is involved. No engine unit, full, research, H4 or screenshot suite is selected.

`engine_story_dialogue_observation.gd` reuses the actual-host focus/input/report helpers from the
viewport observer. It loads ordinary Main and supplies mapped key events only. It reads native
panel/body/portrait/choice/hint rectangles, full shaped line counts, visible characters, reading
coverage, real session tokens/choice phases and errors. A portrait must be a live textured child
after the background, not merely a valid rectangle behind it. Reports are semantic checkpoints;
they do not retain scene frames. Budget: one process, 75-second outer deadline, private loop at
most 60 seconds, under 5 MiB per report. Stop and retain any failed/unavailable run; a passed flag
with runtime/script errors is not accepted. Private source cases retain the observed two-ObjectDB
shutdown warning; its cause is **Unknown**, not proven harmless.

Create these small **pre-start authored inputs** in ignored `local/issue1393`, using Python from
the owning checkout. They change no production content. The harbor copy omits its next battle
so the existing Yes/automatic-transfer consumer returns to field control. The original package
is still used by `skip`; the existing Jaro package supplies the battle-story consumer.

```python
import copy, json
from pathlib import Path
out = Path("local/issue1393"); out.mkdir(parents=True, exist_ok=True)
base = json.loads(Path("remake/content/authored/harbor-arrival.json").read_text())
base["world"]["maps"][1]["battle"] = None
def write(name, value):
    (out / (name + ".json")).write_text(json.dumps(value), encoding="utf-8")
write("control", base)
for mode in ("adjustable", "instant"):
    write(mode, dict(formatVersion=1, textMode=mode, charactersPerSecond=40))
long = copy.deepcopy(base)
long["world"]["texts"][0]["text"] = "\n".join(
    f"Passage {n}: The harbor keeper checks every ticket before the ferry leaves. "
    "Read each account and choose whether to travel." for n in range(1, 36))
write("overflow", long)
repeat = copy.deepcopy(long)
repeat["world"]["programs"][0]["instructions"][2:2] = [
    dict(op="text-cursor", text=100), dict(op="show-text", mode="single", speaker=None)]
write("repeat", repeat)
failed = copy.deepcopy(base)
failed["world"]["programs"][0]["instructions"][2:] = [dict(op="native-call",
    symbol="unavailable-dialogue-service", source="authored-failure-boundary")]
write("failure", failed)
top = copy.deepcopy(base)
top["start"]["position"]["y"] = 5
top["world"]["maps"][0]["entities"][0]["position"]["y"] = 5
write("top", top)
```

For `private`, `private-choice` or `private-yes`, use the retained selected Map3 world and controlled
party from the [viewport recipe](#unified-world-viewport-observation): source player `(42,9)`, same
explicit palette/text/display fields and flags `0,32,256,260,261,601,602`. Set
`SF2_PRIVATE_EXPLORATION_CONTENT` and `SF2_PRIVATE_CONTROLLED_START` through the existing prepared
selection (`local/issue1045/inputs.ps1` and its retained `world-route-13.json`/tracked
`battle01-player-ready.json`); do not duplicate the private world or assets. Down then Right
naturally enters Zone8 and runs the existing caller. This is controlled modern-host reach, not
original natural timing. Missing retained inputs are a prerequisite, not acquisition authority.

Choose a fresh run directory and selected absolute start/package path, then run from the owning
checkout. Authored cases select `--authored-package`; private cases select
`--private-exploration-start`. Settings are `instant.json` only for `instant`, otherwise
`adjustable.json`. Case names and inputs are in the table below.

```powershell
$repo = (Get-Location).Path
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$case = 'repeat'
$run = Join-Path $repo 'local/issue1393/reproduce-repeat'
if (Test-Path -LiteralPath $run) { throw 'Choose a fresh run directory' }
New-Item -ItemType Directory -Path $run | Out-Null
foreach ($name in @('appdata','localappdata','temp')) {
    New-Item -ItemType Directory -Path (Join-Path $run $name) | Out-Null
}
$env:APPDATA = Join-Path $run 'appdata'; $env:LOCALAPPDATA = Join-Path $run 'localappdata'
$env:TEMP = Join-Path $run 'temp'; $env:TMP = $env:TEMP
$env:SF2_VIEWPORT_CASE = $case
$env:SF2_VIEWPORT_OUTPUT = Join-Path $run 'native.json'
$package = Join-Path $repo 'local/issue1393/repeat.json'
$settings = Join-Path $repo 'local/issue1393/adjustable.json'
& $env:GODOT_BIN --headless --path (Join-Path $repo 'remake/game') --max-fps 60 `
    --resolution 1920x1080 --script res://probes/engine_story_dialogue_observation.gd -- `
    --authored-package $package --input-settings $settings
# Retain stdout/stderr alongside native.json and inspect both, plus the actual exit code.
```

| Case / selected input | Bounded observation |
| --- | --- |
| `authored`, `instant` / `control.json` | Hidden field dialogue; legal Talk/Wait; no invented name/portrait; incremental/reveal-all or instant; fresh acknowledgement; authored direct Yes/Cancel=No; same-session automatic transfer/close and fresh field movement. |
| `top` / `top.json` | Same consumer with a bottom-edge subject; top placement stays stable through reveal, parent hide/show and question. |
| `overflow`, `repeat` / matching JSON | 4,295 characters, 70 full shaped lines at 30px. Panel expands to 1440×1016; overlapping reading pages retain the token. Choice attachment preserves already-read coverage. `repeat` opens identical text in a new window and proves reading resets to the beginning. |
| `failure` / `failure.json` | Acknowledge real authored text into existing unsupported native-call boundary; readable stopped wording, retained diagnostic/state, no automatic retry or response to later Confirm/Cancel. |
| `private` / Zone8 start | Real portrait resources, side/mirror/eye/mouth selector, actual child order and safe native bounds; source lines through text525. |
| `private-choice` / same start | Continues through531, retains default Yes, changes focus, holds Cancel=No without entering skip, releases, closes/deletes choices, naturally returns through532/533 to fresh field control. |
| `private-yes` / same start | Holds Confirm=Yes through source choice, releases, observes deleted choice and next genuine text535 input; stops before later JOIN work. |
| `battle` / authored `jaro-conversion.json` | Actual movement/stay trigger; two shared battle-story windows, retained physical camera slot/focus, real acknowledgements, correct close and fresh movement. |
| `skip` / original authored `harbor-arrival.json` | Choice offers no skip; later eligible intro shows actual binding; held Cancel stops at handoff, release admits battle, fresh Confirm/Cancel operate its ordinary menu. |

All executor cases below ran with `--headless`. They exercise actual Godot controls, layout,
process/draw callbacks, mounted resources and injected mapped input, but do **not** establish
windowed compositor delivery, OS window focus or physical-device behavior. Headless root-focus
flags are only observer preconditions. Independent main-gate owns the bounded windowed checks;
those results remain separate from these executor records. No executor windowed pass is claimed.

**Confirmed at the candidate boundary:** the private portrait/Zone8 pilot and authored question/return
pilot passed before expansion. Private No and Yes took about50/46 seconds, with24/22 compact samples
and about122/115 KiB reports. Reached source question531 uses three shaped lines; its full binding
hint measures71px and expands the panel to327px high, all within the safe area. The final repeated
long-text case took about1.9 seconds with eight samples/54 KiB; top, instant, failure, battle-story
and skip cases took about1–3.3 seconds each. Body/portrait/choice/hint bounds are disjoint and the
world rectangle/96px scale remain unchanged. Exact head/build/check identities belong to the Issue
and Draft PR freeze handoff; these are executor observations pending independent acceptance.

**Independent main-gate windowed validation:** on the unchanged `7066720c` adapter
(`fd037ee1` changed documentation only), main-gate reports authored, private-choice, repeat, battle,
top, skip, failure and instant all **PASS**: eight processes exited0,64 semantic checkpoints,
65.734 seconds observer time and289,323 bytes total. Its stdout/stderr checks found no runtime/script
errors, focus Unavailable or shutdown warnings; its independent Release adapter build also passed
with zero warnings/errors. Reproduce the same cases/installation/project/prepared inputs above by
omitting `--headless`; keep the windowed logs/receipts separate. Results are retained in ignored
`local/issue1393/main-gate-*` and `main-gate-window-results.json`. These supplied independent results
do not erase executor headless shutdown warnings or establish physical-device/original fidelity.

Retained failed attempts are in ignored `local/issue1393`: observer parse/type, numeric flag check,
insufficient reveal/movement sequencing and the original harbor case's automatic battle entry
were observer/input-plan failures. Product discoveries were the already-read authored question
restarting reveal, lost reading coverage after choice-row resize, and the source choice hint
exceeding its fixed footer. Their corrections passed focused reruns. The first repeated-window
fixture accidentally advanced text100 to101; corrected explicit text-cursor selection proves the
identical-window boundary. Completed failures/logs remain, including a four-ObjectDB warning on
the failed first source-choice run. No completed failure is relabeled interrupted or erased.

**Unknown/limits:** physical device release outside OS focus, original timing/audio/hardware fidelity,
all-map or multi-resolution coverage and final aesthetic quality. Temporary source `HideWindows`
retains identity by direct ownership review; parent hide/show is natively observed. Do not infer a
new natural full-opening claim. Earlier A unsuitable-start/probe failures and missed-skip timing
Unknown, B focus-loss Unavailable/physical-release Unknown, #689's entry timeout and #1386 exit
warnings remain preserved. No broader original comparison or cleanup authority follows.

## Battle status-card observation

[E1's Standard agreement](../godot/presentation.md#battle-status-card-agreement) owns the outcome.
`engine_battle_status_observation.gd` reuses the existing viewport observer's ordinary Main.tscn,
mapped input, focus recovery, 75-second deadline and output helpers. It reads actual session actors,
native name bounds/line counts, card/resource values, cropped-fill draw operands, menu/range geometry
and scene phases. It never sets gameplay state, completes a presentation synthetically or captures
images. Six serial finite cases use a few dozen semantic checkpoints each; the pilot reports were
below 2MB per case. No whole-battle replay, bulk capture, external acquisition or new comparison
platform is required. Retain failures; do not repeat an unrelated full suite.

Load `local/private-inputs.ps1` in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Run `uv run sf2 verify adapter`; use the configured absolute
SDK from `remake/` to build `game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore` for
native load. The first load of the new PNGs needs one existing-editor import:
`& $env:GODOT_BIN --headless --path remake/game --editor --import --quit`. Reuse the same installation,
project, imports and caches; a code build needs a new native load, not another installation/project.
No engine/data change in E1 requires unit tests. On the clean committed head, inspect
`uv run sf2 verify plan --base origin/main --head HEAD`; adapter-build is the affected product gate.

Prepare only small pre-start authored variants in a fresh ignored output directory:

```python
import json
from pathlib import Path
out = Path("local/issue1397")
out.mkdir(parents=True, exist_ok=True)
d = json.loads(Path("remake/content/authored/practice-yard.json").read_text(encoding="utf-8"))
name = "wandering-medic-of-the-northern-watch"
d["actors"][0]["id"] = name
d["start"]["actors"][0].update(actor=name, hp=25)
d["encounters"][0]["placements"][0]["actor"] = name
d["encounters"][0]["placements"][1].update(x=3, y=4)
d["start"]["actors"][1]["hp"] = 2
(out / "heal.json").write_text(json.dumps(d), encoding="utf-8")
long_name = "w" * 64
d["actors"][0]["id"] = long_name
d["start"]["actors"][0]["actor"] = long_name
d["encounters"][0]["placements"][0]["actor"] = long_name
(out / "long.json").write_text(json.dumps(d), encoding="utf-8")
```

Set `SF2_VIEWPORT_CASE`, `SF2_VIEWPORT_OUTPUT` to a new `native.json`, and select the corresponding
ordinary startup input below. Give each run its own ignored APPDATA/LOCALAPPDATA/TEMP/TMP directories.
For example, from the repository root with `$selectedInput` resolved to the selected authored file:

```powershell
$env:SF2_VIEWPORT_CASE = 'heal'
$env:SF2_VIEWPORT_OUTPUT = Join-Path $runRoot 'native.json'
& $env:GODOT_BIN --path remake/game --max-fps 60 --resolution 1920x1080 `
  --script res://probes/engine_battle_status_observation.gd -- --authored-package $selectedInput
$nativeExit = $LASTEXITCODE
```

Adding `--headless` selects a headless actual-host check; label it accordingly. Require actual exit0,
`passed:true`, empty failure/Unavailable arrays, and inspection of both logs for script/C#/draw errors.
JSON success does not erase shutdown warnings or prove original pixels/hardware timing. Scene waits
continuously check focus; one bounded recovery is permitted by the existing helper. Explicit fresh
menu-page waits prevent an observer from treating an unprocessed input as a selected action.

| Case | Entry | Finite acceptance boundary |
| --- | --- | --- |
| `heal` | Prepared `heal.json`, `--authored-package` | Two-line readable name, quarter-HP warning, full MP, image ratios, Magic/self/other target, layered Cancel with unchanged resources, fresh selection and actual heal/cost phases. HP remains old until Reaction; MP remains old until SpellCost. Next actor has its own card and no stale target. |
| `long` | Prepared `long.json`, `--authored-package` | The valid 64-character identity stays complete in two native lines at >=22px, with a wider safe-edge card, visible numeric/image rows and the same command/heal path. |
| `damage` | Tracked `content/authored/stone-court.json`, unchanged | Readable Swordsman/Raider, enemy target, full/zero-max-MP tracks, lethal ordinary physical action, HP15→0 at Reaction and the explicit Defeated cue; subsequent usable control. |
| `laser` | Tracked `content/authored/prism-laser.json`, unchanged | Existing two off-ray Stays and one Up+Stay per ally trigger one actual automatic shot. Every reached TargetEnter/Reaction/ResultMessage/DeathMessage card matches `laser.target`; Reaction cards traverse companion → leader → sentry, including both factions/deaths, then return control. |
| `aura` | Tracked `content/authored/aura-healing.json`, unchanged | Ordinary Magic list selects AURA4. Shared cards follow a-edge → b-center → n-neutral → m-full → z-priest → f-far, including full-HP recovery and one MP cost. |
| `private` | Same retained issue1047 private selections/start/party as [B8 private-region](#battle-movement-range-observation), `--private-exploration-start remake/reference/inputs/engine-battle02-egress-start.json` | Real Battle02 entry/skip, Chester name, ordinary Stay to Sarah, actual HEAL list/self/unavailable-other target and Cancel, then real private HEAL scene. Sarah HP11 stays11 while MP10→7 at SpellCost; no fictitious HP event is required for full-HP recovery. Existing scene rasters remain local. |

Private selection reuses `local/issue1047/inputs.ps1`, explicitly selects its `world-01.json` and the
tracked `reference/inputs/battle02-outcome-party.json`, resolved within the owning checkout. It copies
no canonical ROM, world or asset pack. All private payloads/starts/logs/reports remain ignored.
No original enemy/name/art acquisition is implied. Selected scene-name metadata is projected as
admitted; the host adds no party rename operation.

The targeted `names` / `overflow` cases use that same retained world and
`--private-exploration-start remake/reference/inputs/battle01-to-battle02-start.json`. Select a small
ignored copy of the already-admitted Battle01 scene JSON through `SF2_PRIVATE_BATTLE_SCENE_CONTENT`;
change only its first three `memberNames` before startup. For `names`, use
`"Northern\n\r\nWatch\t Keeper\nThree"`; require the complete displayed name
“Northern Watch Keeper Three”. For `overflow`, use three newline-separated 400-character `W`
strings; require the normalized 1202-character observation, 800px frame and two visible native lines.
Only these small metadata fixtures are copied (the selected scene is about 122KB); the existing
127.6MB world/assets remain read-only and are not copied. These cases stop at the first battle menu.
They directly check the finite width/ellipsis correction; they do not invalidate unrelated completed
damage/heal/MP observations or require another full case run.

**Confirmed executor boundary:** six windowed source-final cases passed with actual exits0, empty
failure/Unavailable arrays and clean stdout/stderr (about 51 seconds and 4.93MB total reports).
The later finite-name correction passed the two targeted windowed cases above, about 6.4 seconds
and 132KB, also with exits0 and clean logs. Debug/Release adapter compilation passed. Prior private
runs retained two ObjectDB exit warnings; clean final runs do not establish that historical issue
resolved. No Godot process is retained. Independent acceptance remains pending.

Completed failures remain in ignored `local/issue1397`: an initial JSON float/string expectation;
first-show wrapped-card height1402 (fixed using native minimum-size notification); a helper-name
collision caught by Debug compilation; private-case assumptions about name casing, terrain reach,
target order and an HP event on full-HP healing; and a long-name case initially routed to an
unsupported physical action by the observer. Earlier window runs also missed a page/failed to
complete ActionMessage; explicit page waits and continuous focus checks precede the final cases.
The extra name-fixture attempts selected Battle02's scene instead of the changed Battle01 names,
then exposed the direct private-battle start's unavailable battlefield-map resource, then missed the
eligible Battle01 intro Skip. Corrected cases reuse the admitted world and its ordinary eligible Skip.
Those window-input anomalies are not declared resolved merely by a later passing run. These are preserved results,
not interrupted suites or waivers. Prior #689/#1386/#1389/#1393/#1395 focus/release/timing failures
and ObjectDB shutdown Unknowns remain. Independent main-gate review owns integration and final
acceptance; broader physical-device, all-map and aesthetic approval are not established here.

## Logical Input and Accessibility (ADR 0010 9A)

`GameRoot` loads one `InputSettings`/`GameInput` owner before starting a session, installs its
`sf2_*` Godot InputMap actions, and dispatches each physical event once to the current view.
Exploration, dialogue/choice, battle and return use the same semantic actions. Defaults are:

| Action identity | Keyboard keys | Standard gamepad buttons / axes |
| --- | --- | --- |
| `up` | Up, W | DpadUp, LeftY- |
| `right` | Right, D | DpadRight, LeftX+ |
| `down` | Down, S | DpadDown, LeftY+ |
| `left` | Left, A | DpadLeft, LeftX- |
| `confirm` | Enter, KpEnter, Z | South |
| `cancel` | Escape, X | East |
| `attack` | F | West |
| `spell` | H | North |
| `item` | I | Back |
| `target` | Tab | RightShoulder |
| `stay` | Space | LeftShoulder |

Confirm talks, acknowledges dialogue, answers Yes, or selects/commits in battle. Cancel answers No
or backs out one menu level in battle; it does not acknowledge dialogue. Spell and Item open their
lists from the diamond; Confirm selects a row, initially targeting self when the offer permits it.
Target cycles the selected action's offered candidates. The [diamond recipe](#diamond-menu-observation)
owns the current battle interaction; older immediate-selection shortcuts in probes are retained
historical recipes, not acceptance of this revised UI.
All accepted commands still use Application's existing legality and state. F replaces the former X
attack binding; former C/Y/N exploration shortcuts are replaced by Confirm/Cancel. View help and
spell/item hints read effective bindings, including after a convention swap.

### Diamond menu observation

The first #438 production iteration uses default keyboard A and live semantic offers. After the
locked environment and Debug adapter build, prepare a small variation of the existing authored
practice yard in a fresh ignored output directory. This is controlled content, not a session setter:

```powershell
$package = Join-Path $env:SF2_RUN_OUTPUT 'menu-package.json'
$data = Get-Content remake/content/authored/practice-yard.json -Raw | ConvertFrom-Json
$data | Add-Member -NotePropertyName items -NotePropertyValue @(
    [pscustomobject]@{id=0;name='Recovery leaf';effect='consumable-healing';power=10;minimumRange=0;maximumRange=1},
    [pscustomobject]@{id=6;name='Small remedy';effect='consumable-healing';power=4;minimumRange=0;maximumRange=2}) -Force
$data.actors[0].items = @(256,6,255,383)
$data.actors[1].items = @(0,127,127,127)
$data.start.actors[0].hp = 60
$spell = $data.spells[0] | ConvertTo-Json -Depth 10 | ConvertFrom-Json
$spell.level = 2; $spell.mpCost = 25
$data.spells += $spell
$data.actors[0].spells += [pscustomobject]@{id='mend';level=2}
$data | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $package -Encoding utf8
$env:SF2_DIAMOND_CASE = 'menu'
$env:SF2_DIAMOND_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'diamond-menu.json'
& $godotBinary --headless --path remake/game --max-fps 120 `
    --script res://probes/engine_diamond_ui_observation.gd -- --authored-package $package
```

The probe checks cardinal focus, disabled spell/item/target feedback, nested cancel without resource
use, real healing and another actor's item consumption. Current acceptance uses the existing
1280×720 review window, including disabled-list and target descriptions; older multi-size receipts
are not a requirement to expand multi-resolution support. Check the 216×168 command cluster and
72×72 icons, full-window map rectangle, and unchanged map rectangle/zoom/preview framing when opening
the main menu or a list. Gameplay target/movement changes can still move the camera. No claim of
final visual quality follows from geometry alone.

For physical attack, set `SF2_DIAMOND_CASE=physical`, a fresh output, and use the existing
`remake/content/authored/stone-court.json` package. For a bounded private menu observation, load
registered complete private selections, set CASE=`private`, a fresh output, and use
`--private-exploration-start remake/reference/inputs/battle01-to-battle02-start.json` with its
accepted connected world and party. The probe waits for the ordinary tower entry's skip hint, sends
default-keyboard Cancel (Esc), releases it and waits for ordinary battle control through the accepted
[battle-entry skip](../godot/battle-entry-skip.md). Its entry deadline is ten seconds; a missing or
failed skip does not silently fall back to waiting through the entire intro. A legacy standalone
battle input lacking map-region/field resources is
not a substitute. That case checks menu navigation and cancel without committing an action; it is
not another continuous opening/outcome comparison.
For original-art acceptance, prepare/select the [private command rasters](../content/presentation-assets.md#private-command-menu-icons)
and set `SF2_DIAMOND_ORIGINAL_ICONS=1`. Require square command rectangles at the review size, mounted
24×24 textures with nearest sampling, and actual draw readback selecting each normal/selected raster
as focus moves. The observation has a 150-second deadline. These checks establish resource use and
geometry, not human visual approval.
Original-icon observations also require no extra drawn frame or visible per-icon label. The compact
main hint must name the focused command at every cardinal position, hide keyboard prose and retain
the edge placement; unavailable feedback remains scoped to that selected command.
Cases `equipment`, `give` and `discard` use the corresponding existing
`remake/content/authored/battle-<case>.json` packages. They check that each supplementary shortcut
opens its existing presenter and Cancel returns to the diamond without spending resources. The
private pilot's older selected content does not supply every supplementary capability.
For a partial-resource negative check, copy the prepared private scene into another ignored input,
remove one command-menu raster and select that copy. Set `SF2_DIAMOND_EXPECT_FAILURE` to
`command-menu-resource-required`; require the field-owned admission failure before battle control.
Require exit 0, `passed:true`, empty failures and clean Godot errors. Preserve failed reports when
correcting a layout. The public Release adapter build and engine behavior tests are separate gates;
no tests of this probe or screenshot audit are added.

Keyboard echo is ignored. A stick deflection crossing magnitude0.5 submits one action; it must return
below that threshold or change sign to submit another. Unrelated axes and releases cannot repeat a
held direction. All standard Godot-mapped gamepads are accepted. This is product input behavior,
not a claim about original hardware scancodes or repeat cadence; physical controller-driver mapping
and hot-plug behavior are outside the injected native-event observations.

For example, save this UTF-8 JSON in a local settings file and pass its absolute path:

```json
{
  "formatVersion": 1,
  "confirmCancel": "swapped",
  "reducedFlash": true,
  "textMode": "adjustable",
  "charactersPerSecond": 40,
  "bindings": {
    "attack": { "keys": ["R"], "buttons": ["West"], "axes": [] }
  }
}
```

```powershell
& $godotBinary --path remake/game -- --input-settings $settingsPath
```

`formatVersion` is required and currently1. Other fields default to `standard`, false, `instant`,
40 and an empty binding override map. `confirmCancel` accepts `standard` or `swapped`; swapping
exchanges the **whole effective Confirm and Cancel binding sets after overrides**, across both
keyboard and gamepad, without changing their semantic roles. `textMode` accepts `instant` or
`adjustable`; `charactersPerSecond` must be finite in1–240 even in instant mode.

Each binding override replaces one complete action and requires all three arrays (`keys`, `buttons`,
`axes`), at least one key and at least one button or axis. Key identities are case-sensitive Godot
`Key` enum names (for example `I`, `F1`, `Space`, `Escape`); `None` and standalone modifier keys
`Shift`/`Ctrl`/`Alt`/`Meta` are rejected, and key chords are not part of version1. Button identities
are the names in the table plus `Start`, `LeftStick` and `RightStick`. Signed axis identities
are `LeftX-`, `LeftX+`, `LeftY-`, `LeftY+`, `RightX-`, `RightX+`, `RightY-`, `RightY+`.
Unknown fields/actions/identities, conflicting bindings, missing device access and invalid values
produce `ContentError: invalid-input-settings` before a session is published. A missing or unreadable
explicit file also fails; it never silently restores defaults. No file selects documented defaults.
Settings are loaded once at startup; there is no in-game settings UI or automatic settings save.

Adjustable text uses the actual Label's visible-character count. Time reveals characters but never
submits a gameplay command. Confirm while text is incomplete reveals the rest and preserves the real
wait token; a subsequent Confirm acknowledges. A choice with incomplete text likewise reveals first
and still requires a semantic Yes/No input. Instant text displays everything and still awaits its
normal acknowledgement and choice. A completed reveal cannot complete a presentation cue.

Reduced-flash keeps white FadeOut/FadeIn overlay opacity at0 for the same service duration. It still
submits `CompletePresentation` with the reached token and cue kind. Black fades and other services
retain their existing behavior. This intentional visual deviation, logical remapping and modern text
progression belong to ADR0010 9A/10A; they do not establish original visual/timing parity or 8C/H4.

### Native 9A observation

After the locked environment and Debug adapter build, select a fresh ignored output directory. The
bounded external observer writes an authored variation/settings into the specified destinations,
then instantiates ordinary `Main.tscn` with those actual startup arguments. It never sets session
state. It adds white presentation cues, a second spell, ordinary physical definitions/rewards and
an adjacent durable opponent to the selected existing authored world.

```powershell
$env:SF2_INPUT_CASE = 'keyboard' # gamepad, remapped-keyboard, remapped-gamepad
$env:SF2_INPUT_VARIANT = 'harbor' # hill exercises another world, actor and spellbook
$env:SF2_INPUT_RATE = '20'       # also observe80 with the hill variation
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'input.json'
$package = Join-Path $env:SF2_RUN_OUTPUT 'input-package.json'
$settings = Join-Path $env:SF2_RUN_OUTPUT 'input-settings.json'
$arguments = @('--headless', '--path', 'remake/game', '--fixed-fps', '60',
    '--script', 'res://probes/engine_input_accessibility_observation.gd', '--',
    '--authored-package', $package)
if ($env:SF2_INPUT_CASE.StartsWith('remapped-')) { $arguments += @('--input-settings', $settings) }
& $godotBinary @arguments
```

The remapped cases replace every action binding, move stick input to the right stick, swap the
Confirm/Cancel convention and select reduced-flash/adjustable text. Require `passed:true`, empty
failures and clean process errors. Observe actual field movement, decline/re-prompt/acceptance,
partial/full text reveal with retained waits, both real completed white cue tokens, the same session
at first battle control, movement/cancel, two spells, another allied target, actual HEAL, another
actor's Stay, next-round attack and binding-derived help. Gamepad cases also check repeated axis
values, unrelated trigger events and release edges against the actual session revision/preview.
Compare default keyboard/gamepad and remapped harbor outputs at the matching semantic checkpoints:
flags, actor, round, resources, seeds and white token/kind completion must agree; only the intended
presentation/settings differ. The hill case checks independent valid content rather than a fixed
receipt sequence. Default white opacity reaches1; reduced-flash stays0.

For the current private continuous outcome/return observation, reuse the existing private selection,
R1 start and opening navigation setup from [exploration reproduction](field-programs.md#reproduction).
Select `SF2_INPUT_CASE=private-remapped-gamepad`, `SF2_BATTLE_OUTCOME_CASE=victory`, and the new observer
script, with `--private-exploration-start <selected-R1-start> --input-settings <ignored-settings-path>`.
It reuses the existing actual-input outcome driver, including its single-frame input cadence, with
all commands mapped to the remapped/swapped gamepad. This case uses instant text and ordinary flashes;
the authored cases own the paired accessibility comparison. Require the existing main/admission/
outcome reports to pass through usable return movement, retained session identity and valid node
projection. The settings startup matrix uses the existing `engine_battle_observation.gd` startup
case with EXPECT_FAILURE=`invalid-input-settings`; require no session/actors/RNG on rejected input.
Each launch tests an actual startup configuration in the retained project/installation; no editor,
project copy, screenshot, original emulator, reference replay or test of the observer is needed.

## Battle Movement Range Observation

[B8 agreement](../godot/presentation.md#battle-movement-range-agreement) owns semantics and layering.
`engine_movement_range_observation.gd` reuses the existing world-viewport observer's input, focus,
deadline and output helpers. It performs no post-start state injection. Its four cases are finite
checkpoints, not a whole-battle or opening replay. Reports retain actual state, range draw destinations,
board transform/order, actor nodes and lifecycle boundaries. No images or frame captures are produced.

Load `local/private-inputs.ps1`, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and build the actual
adapter Debug project from `remake/` with the existing absolute SDK. Reuse its Godot installation.
For each run choose a new ignored output directory, with APPDATA/LOCALAPPDATA/TEMP/TMP beneath it.
Set `SF2_VIEWPORT_CASE` to a case below and `SF2_VIEWPORT_OUTPUT` to its `native.json`, then launch
from the repository root (replace the selected entry option/input with the table's entry):

```powershell
& $env:GODOT_BIN --headless --path remake/game --max-fps 60 --resolution 1920x1080 `
  --script res://probes/engine_movement_range_observation.gd -- --authored-package $selectedInput
$nativeExit = $LASTEXITCODE
```

Read both logs and the actual exit status. `passed:true` alone does not excuse script errors or
shutdown warnings. Omitting `--headless` selects a bounded windowed integration check; do not claim
windowed or pixel acceptance from a headless report.

| Case | Pre-start input | Named boundary |
| --- | --- | --- |
| `corridor` | Ignored variant of `practice-yard.json`: all terrain rows barriers except row3 all `g`; guard at(4,3), dummy at(1,3); all other fields unchanged. | Medic origin(3,3) has exactly(2,3),(3,3),(5,3),(6,3),(7,3),(8,3). Enemy blocks left travel, ally can be crossed but not stopped on, open budget ends at8. Mapped Right twice retains the same range during animation and after crossing ally; actions/Magic/Item/commit hide; Cancel restores through return animation; Stay refreshes guard's origin and occupied tiles. |
| `automatic` | Ignored variant of `stone-court.json`: actor2 move3; placement2 strategy `attack-then-approach`, x7; placement3 x6,y5; all other fields unchanged. | Mapped Stay reaches actual Automatic movement with range absent. |
| `outcome` | Tracked `progression-outcome-victory.json`, unchanged. | Mapped interaction enters battle, one normal physical command defeats the one-HP enemy; range clears at target/commit/scene, remains absent during field-owned outcome dialogue on the retained battle backdrop and after usable return in the same session. |
| `private-region` | `--private-exploration-start remake/reference/inputs/engine-battle02-egress-start.json`, with retained `local/issue1047/inputs.ps1`, `SF2_PRIVATE_EXPLORATION_CONTENT=local/issue1047/world-01.json` and `SF2_PRIVATE_CONTROLLED_START=remake/reference/inputs/battle02-outcome-party.json` resolved to absolute checkout-local paths. | Existing controlled Battle02 entry/skip reaches actual terrain, foreground and sprite projection at region(16,19); actions/Magic/self-target/retreat clear the overlay; same-session usable field return detaches the battle range. This is not natural original reach. |

Generate the authored variants before startup using Python and fresh ignored paths; no production
fixture change is required. For example, from repository root:

```python
import json
from pathlib import Path
out = Path("local/issue1395")
out.mkdir(parents=True, exist_ok=True)
d = json.loads(Path("remake/content/authored/practice-yard.json").read_text(encoding="utf-8"))
d["terrains"][0]["rows"] = ["gggggggggggg" if y == 3 else "############" for y in range(9)]
p = d["encounters"][0]["placements"]
p[1].update(x=4, y=3)
p[2].update(x=1, y=3)
(out / "corridor.json").write_text(json.dumps(d), encoding="utf-8")
d = json.loads(Path("remake/content/authored/stone-court.json").read_text(encoding="utf-8"))
d["actors"][2]["move"] = 3
p = d["encounters"][0]["placements"]
p[2].update(aiStrategy="attack-then-approach", x=7)
p[3].update(x=6, y=5)
(out / "automatic.json").write_text(json.dumps(d), encoding="utf-8")
```

**Confirmed:** executor headless cases corridor-final, automatic-01, outcome-04 and private-region-final
passed with actual exits 0 and no script errors. They total about 14 seconds and 0.75MB of checkpoint JSON.
Private region retains two ObjectDB shutdown leaks; authored cases have clean logs. No process remains.
Engine behavior tests cover weighted/impassable terrain, exact budget, map edges, occupancy,
read-only immutable results, preview/return anchoring, stage/story/round/field clearing and actor refresh.
The selected engine run passed 1642, skipped 31 private-input cases, failed 0; adapter Release and Debug
builds passed with zero warnings/errors. Native private success is separate from those skips.

Preserved failures under ignored `local/issue1395`: movement-tests-01/02 expected a pause after Stay,
although the real turn flow advances immediately; corrected query tests pass. corridor-01 compared
JSON float arrays to integer literals; private-region-01 guessed terrain node names; corrected
checks use numeric operands and actual nodes. story-01 assumed an initial wait at an input start and
was retired from the selected coverage. outcome-01 missed DialogueWait input; outcome-02 missed
field-owned after-program presentation; outcome-03 incorrectly required its retained battlefield
backdrop to be hidden. Corrected observation checks range visibility on that backdrop and after
return. Earlier reports/logs remain intact. These observer failures are not product or evidence waivers.

Independent Release movement/query checks passed 15/15 with no skips, using the existing build.
**Confirmed:** independent windowed corridor, private-region, automatic and outcome observations
passed on the same production code with the observer phase waits/receipts below. All four actual
exits were 0, with no failures, Unavailable results, runtime/script errors or warnings. The retained
`main-gate-window-results.json` records 30 checkpoints, 29.407 seconds and 1,088,375 bytes. This
includes legal tile sets, fixed geometry/layers, preview/cancel, next actor, automatic clearing,
private nonzero-region projection, targeting, retreat, outcome story and usable field return.

The first independent windowed corridor run (`main-gate-corridor-01`) failed its Cancel-restoration
check: earlier legality/preview/menu checks passed, but the final receipt remained ActionChoice at
revision12 with preview(5,3), no movement and no range. Exit1 and clean stderr are preserved.
The cause is **Unknown**. A windowed diagnostic recorded the expected first Cancel to ActionChoice
and second Cancel to Return movement without a production change. The observer now explicitly
waits for those two phase boundaries and retains menu/command receipts; a bounded windowed check
passed. There is no extra Cancel, automatic retry or weakened restoration expectation. A passing
diagnosis does not explain or erase the initial failure.

**Unknown:** complete fixture/full-game coverage, original range pixels/cadence, the initial
nonreproduced windowed Cancel sequence, earlier physical focus-release boundaries and the private headless shutdown
warning cause. The earlier A/B/C failures and Unknowns remain in their owners. This slice adds no
research/H3/H4 comparison or screenshot requirement.
