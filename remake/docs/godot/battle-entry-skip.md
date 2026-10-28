# Battle-entry skip

## Accepted delivery contract

The accepted design was committed before implementation under [Issue 1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386).
The [session agreement](../application/session-and-programs.md#battle-entry-skip-agreement) owns
eligibility, logical equivalence and failure boundaries. Contextual Cancel means **Skip battle
intro** only at eligible entry waits. Show `GameInput.Hint(Cancel)`, respecting the existing mapping
(default Escape/X). Confirm keeps ordinary reveal/advance. Every ChoiceWait excludes skip.

The host yields between bounded batches, observes focus/errors and requires input release before
choice or combat input. Focus loss disarms acceleration. Repeated/held/stale input cannot select a
choice or affect a later entry. Session completion clears skip before round/AI dispatch, not at
an eventual player-input state.

Omitted delivery has the explicit reason `battle-entry-skip`. Existing completion reducers and
wait/generation validation own cleanup. Mount sprite resources and the actual BattleLoad board;
resource errors remain errors. Set the final camera/fade/text/portrait projection and invalidate
old callbacks through their existing tokens. Preserve already-playing short speech tails; suppress
future omitted speech/transient effects. Do not use StopSharedEffects. Reconcile the session audio
owner's cue/context/history after suppressed playback; never invoke ordinary PlayPrevious against
an adapter history which missed intermediate requests. Logical finite music services still run.

## Staged acceptance and budget

1. Small actual-engine tests compare identical request states mid-motion and mid-text, varied IDs,
   nested calls/branches, mandatory service/RNG, party/resources/flags and first battle order. Exercise
   choices, errors, stale/repeated/late completions, cancellation and an enemy-first round.
2. Measure a small authored pilot before native connected work. Initial implementation budget:
   at most 32 accelerated wait-reducer operations per host frame, each retaining the runner's 256-instruction
   limit; at most 32,768 advancing commands per request, including retained sprite and BattleLoad
   completions. Stale/rejected receipts do not consume budget. No unbounded host loop. Tune only with measured
   evidence. Native waits are at most 60 seconds per tool; retain and stop an owned hung process.
3. One ordinary-host connected Battle01 input/state/projection/music witness uses the actual mapped
   key, same session, prepared content and installed Godot. No screenshots, post-start injection,
   frame capture or native-wide loops. Initially budget 60 seconds startup and 60 seconds entry,
   fewer than 100 compact boundary records and under 1 MiB combined logs/report. Reassess on the
   pilot's measured step count; a budget expiry is a retained failure, never success.

From the owning checkout load `local/private-inputs.ps1` in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse configured absolute SDK/shared CLI home, worktree
caches and prepared private content. New output belongs in ignored `local/issue1386`.

Selected gates: `uv run sf2 verify engine`, `uv run sf2 verify adapter`, and on the clean committed
head `uv run sf2 verify plan --base origin/main --head HEAD`; direct document, source-size,
ownership and private-boundary checks. Focused native/test commands and measured results are
recorded below. No full, research or emulator suites or tests of probes/helpers.

### Pilot and connected-run sizing

The authored pilot measured 200 logical operations over 11 host frames, one actual BattleLoad and
usable combat after release (0.514 seconds wall time, about 10 KiB report/logs). Its first attempt
retains a probe duplicate-signal error on reparenting; it is not a pass. A targeted probe correction
passed. The connected run reuses the prepared world (at most 128 MiB admitted JSON) and selected
battle inputs; no acquisition or asset copy. Expect hundreds to a few thousand mandatory services,
tens to a few hundred skip frames, O(service steps × live entities) time, O(one session + bounded
batch observations) working state, and approximately 1 GiB peak admission/presentation memory.
The single process has a 58-second stop deadline and compact output target below 1 MiB. Reassess
rather than extending blindly if the measured connected run exceeds these bounds.

## Reproduction

Run from the owning checkout. The following Python recipe creates only authored/controlled inputs
in a fresh ignored directory. The transparent raster and sine wave are project-generated test
content. Their digests satisfy the existing content schema. The private start copies accepted
tracked bindings; it makes no natural-reach claim. Save this block to ignored scratch and run with
`python -X utf8 <script>` (or pipe a PowerShell single-quoted here-string to `python -X utf8 -`).

```python
import base64, copy, hashlib, json, math, struct
from pathlib import Path
root = Path.cwd()
out = root / "local/issue1386/reproduction-inputs"
out.mkdir(parents=True, exist_ok=False)
def read(path): return json.loads((root / path).read_text(encoding="utf-8"))
def write(name, value): (out / name).write_text(json.dumps(value), encoding="utf-8")
def cue(kind, resource=None, **extra):
    return dict(op="present", kind=kind, resource=resource, entity=None, position=None, **extra)
pilot = read("remake/content/authored/harbor-arrival.json")
pilot["start"].update(map="yard-map", flags=[13])
programs = pilot["world"]["programs"]
before = next(p for p in programs if p["id"] == "before-battle")
before["instructions"].insert(0, dict(op="wait-ticks", ticks=200))
pilot["world"]["maps"][1]["battle"]["load"] = dict(program="pilot-load", instruction=0)
programs.append(dict(id="pilot-load", instructions=[cue("FadeOut", "black"),
    cue("BattleLoad"), cue("FadeIn", "black"), dict(op="end")]))
write("pilot.json", pilot)
audio = copy.deepcopy(pilot)
before = next(p for p in audio["world"]["programs"] if p["id"] == "before-battle")
before["instructions"] = [dict(op="sprite", entity="traveler", sprite=30),
    dict(op="wait-ticks", ticks=2), cue("Sound", "town"), cue("Sound", "join"),
    cue("SoundWait"), cue("FadeOut", "white"),
    cue("FlashWhite", "white-four-pulses", durationTicks=30), dict(op="yes-no", flag=905),
    cue("SoundWait"), dict(op="wait-ticks", ticks=200), cue("FadeIn", "white"),
    cue("PreviousMusic"), dict(op="end")]
def raster(w, h):
    data = bytes(w*h*4)
    return dict(width=w, height=h, format="rgba8", data=base64.b64encode(data).decode(),
                sha256=hashlib.sha256(data).hexdigest())
pcm = b"".join(struct.pack("<h", int(1000*math.sin(2*math.pi*440*i/8000))) for i in range(1600))
def sound(name, command, loop):
    return dict(cue=name, command=command, timerB=192, sampleRate=8000, channels=1,
        sampleFrames=1600, pcm16=base64.b64encode(pcm).decode(), sha256=hashlib.sha256(pcm).hexdigest(),
        loopBegin=0 if loop else None, loopEnd=1600 if loop else None)
audio["world"]["presentation"] = dict(
    maps=[dict(map=m["id"], atlas=raster(128,320), scale=1, blocks=[[0]*9 for _ in range(1024)],
               music=[dict(field=8,battle=9)]) for m in audio["world"]["maps"]],
    sprites=[dict(sprite=s, directions=[raster(48,24) for _ in range(3)], portrait=None, speech=0)
             for s in (0,30)], portraits=[],
    audio=[sound("town",8,True),sound("battle",9,True),sound("join",19,False)] +
          [sound(f"effect-{n}",n,False) for n in (65,66,67)])
write("audio.json", audio)
write("input.json", dict(formatVersion=1,textMode="adjustable",charactersPerSecond=120,
    bindings=dict(cancel=dict(keys=["Q"],buttons=["East"],axes=[]))))
private = read("remake/reference/inputs/map40-intro-start.json")
bindings = read("remake/reference/inputs/battle01-to-battle02-start.json")["start"]
private["controlledBoundary"] = "Controlled bridge approach, initial accepted palette/text bindings; not natural original reach."
private["start"] = dict(map="map-21",player="entity-0",position=dict(x=5,y=15),facing=3,
    speed=32,flags=[0,1,2,32,33,34,401],mapPalettes=bindings["mapPalettes"],textSettings=bindings["textSettings"])
palette = next(row["base"] for row in bindings["mapPalettes"] if row["map"] == "map-21")
private["start"]["display"] = dict(period=3,base=palette,current=palette,visibility="base-restored")
write("private-start.json",private)
```

Use the existing private input/tool selection owner; the retained connected observation used the
prepared `local/issue1045/inputs.ps1` selection with `local/issue1045/world-route-13.json` and tracked
`remake/reference/inputs/battle01-player-ready.json`. Missing prepared content is a prerequisite,
not authorization for new acquisition. Public authored cases do not load those private selections.

```powershell
$ErrorActionPreference = 'Stop'
$repo = (Get-Location).Path
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv sync --locked
uv run sf2 verify engine
uv run sf2 verify adapter
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --no-restore
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
    --no-restore --filter FullyQualifiedName~BattleEntrySkipTests
# Optional connected same-request comparison after loading the prepared selection:
. ./local/issue1045/inputs.ps1
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $repo 'local/issue1045/world-route-13.json'
$env:SF2_PRIVATE_CONTROLLED_START = Join-Path $repo 'remake/reference/inputs/battle01-player-ready.json'
$env:SF2_ENTRY_SKIP_START = Join-Path $repo 'local/issue1386/reproduction-inputs/private-start.json'
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
    --no-restore --filter FullyQualifiedName~ConnectedBattle01NormalAndSkipKeepTheSameInitializedPartySeedsAndControl
```

For each native case, select `pilot`, `audio` or `private`, a fresh output directory and the same
prepared selections above for `private`. The tracked probe opens ordinary `Main.tscn`, follows the
existing route via mapped keyboard input, holds the initiating Cancel through entry, verifies the
release barrier, then exercises ordinary combat Confirm/Cancel. It uses no snapshot injection.

```powershell
$case = 'audio'
$inputs = Join-Path $repo 'local/issue1386/reproduction-inputs'
$run = Join-Path $repo "local/issue1386/reproduce-$case"
if (Test-Path -LiteralPath $run) { throw 'Select a fresh run directory' }
New-Item -ItemType Directory -Path $run | Out-Null
foreach ($name in @('appdata','localappdata','temp')) {
    New-Item -ItemType Directory -Path (Join-Path $run $name) | Out-Null
}
$env:APPDATA = Join-Path $run 'appdata'; $env:LOCALAPPDATA = Join-Path $run 'localappdata'
$env:TEMP = Join-Path $run 'temp'; $env:TMP = $env:TEMP
$env:SF2_ENTRY_CASE = $case
$env:SF2_ENTRY_ROUTE = if ($case -eq 'private') {
    Join-Path $repo 'tests/fixtures/h3/map3-battle01-player-ready-v1.json'
} else { '' }
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $run 'native.json'
$nativeArgs = @('--headless','--fixed-fps','60','--path',(Join-Path $repo 'remake/game'),
          '--script','res://probes/engine_battle_entry_skip_observation.gd','--')
$nativeArgs += if ($case -eq 'private') { @('--private-exploration-start',(Join-Path $inputs 'private-start.json')) }
         else { @('--authored-package',(Join-Path $inputs "$case.json")) }
$nativeArgs += @('--input-settings',(Join-Path $inputs 'input.json'))
# Quote path arguments for Start-Process's flattened Windows command line.
$quoted = $nativeArgs | ForEach-Object { '"' + $_ + '"' }
$process = Start-Process -FilePath $env:GODOT_BIN -ArgumentList $quoted -WorkingDirectory $repo `
    -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $run 'stdout.log') `
    -RedirectStandardError (Join-Path $run 'stderr.log')
$process.Id | Set-Content -LiteralPath (Join-Path $run 'pid.txt')
if (-not $process.WaitForExit(58000)) { Stop-Process -Id $process.Id; throw 'Native budget exceeded' }
$process.Refresh()
$result = Get-Content -LiteralPath $env:SF2_EXPLORATION_OBSERVATION_OUTPUT -Raw | ConvertFrom-Json
$errors = Get-Content -LiteralPath (Join-Path $run 'stderr.log') -Raw
if ($process.ExitCode -ne 0 -or -not $result.passed -or $errors -match 'ERROR|Exception') {
    throw 'Native witness failed; retain the report and logs'
}
```

## Current acceptance boundary

Implemented: session-owned bounded acceleration, explicit skipped-delivery receipts, mandatory
logical services, real sprite/BattleLoad readiness, final projection and authoritative music
reconciliation. The finite-music consumed disposition remains available to a subsequent ordinary
SoundWait, is distinct from natural `Finished`, and resets on a new music selection. Choice/focus
boundaries disarm acceleration; held input must release before choice/combat handling resumes.

Confirmed at the candidate boundary:

- `uv sync --locked`, engine and adapter gates passed. The completed engine gate reported 1,624
  passes and 30 private skips. After adding the optional connected comparison, the focused class
  passed all 10 public cases with that private case skipped; the separately configured private
  comparison passed. It compares initialized party, seeds, resources, turn order and first control
  from identical request state against earliest normal acknowledgements without optional polls.
- `audio-final`: passed in 561 ms, 14 skip frames, maximum observed request count 202, eight compact
  samples and about 21 KiB report. It covers inherited white overlay, finite omission at a choice,
  repeated ordinary SoundWait, natural-vs-skipped readiness and PreviousMusic recovery.
- `private-final`: passed in 3,069 ms, 116 skip frames, maximum observed request count 2,888, six
  samples and about 27 KiB report. Same session; one actual BattleLoad; sprite preparation;
  skipped text; final camera/fades; visible remapped hint; held-input handoff; usable combat; final
  `MUSIC_BATTLE_THEME_1` playing at restored volume. This is controlled modern-host evidence.

Retained failures live in ignored `local/issue1386`, not public artifacts. `skip-first-run`,
`skip-corrected-cases`, `skip-equivalence` and `skip-enemy-movement` retain exact failing xUnit nodes:
fixture admission/ordering, EntityRef JSON projection, missing enemy movement/reward setup. Targeted
corrections passed. Exact failed nodes in `Sf2.Remake.Engine.Tests.BattleEntrySkipTests` were
`SameRequestStatePreservesLiveWorkAndStopsBeforeCombat(speed: 0, midText: False, enemyFirst: False)`,
`SameRequestStatePreservesLiveWorkAndStopsBeforeCombat(speed: 2, midText: True, enemyFirst: False)`,
`SameRequestStatePreservesLiveWorkAndStopsBeforeCombat(speed: 3, midText: True, enemyFirst: True)`,
`RequestsAndLateCompletionsAreScopedAndChoicesRequireAFreshRequest`, and
`UnsupportedRetainsCommittedPrefixAndDisarms`. Compile-only logs retain early compiler failures separately. `pilot-01` retains
the duplicate signal error. `private-01` failed before skip with `full-fade-state` because its initial
controlled input omitted palette/text bindings. `audio-01`/`audio-02` retain missing sprite/UI-audio
fixture bindings; `audio-03` retains a probe input-sequencing failure and its approximately 1.2 MiB
log-budget overrun. The corrected probe fails fast at unexpected handoff. None is relabeled success
or erased. No completed slow suite was restarted to replace a failed result.

Remaining limits: the audio and connected runs emit respectively eight and six ObjectDB leak
warnings during native shutdown: `WARNING: 8 ObjectDB instances were leaked at exit` and
`WARNING: 6 ObjectDB instances were leaked at exit`, both at `core/object/object.cpp:2536`.
They emitted no runtime/script errors and their owned processes exited. The probe calls `quit`
immediately after its final sample, as the existing exploration probe does. `GameRoot._ExitTree`
and `SessionAudio.Dispose/ReleasePlayer` are unchanged and use deferred `QueueFree`; this supports
an immediate-probe-exit explanation (Inferred), but does not identify the leaked objects. The
retained non-audio `pilot-02` and earlier Issue 1381 native run have empty stderr, so a matching
pre-change audio baseline is not established. Whether these warnings expose a product disposal
issue remains Unknown; they are not classified as a proven harmless baseline. Physical OS focus loss and existing short-effect tails were not independently observed in
these headless witnesses; cancellation behavior is covered at the session boundary and focus
handling uses the existing host notifications. Peak memory is a planning estimate, not a measured
result. Original hardware timing, natural route reach and broader-route fidelity remain Unknown.
The earlier UI 150-second visible-entry timeout remains separate historical failure evidence.

Independent main-gate acceptance owns integration. UI #689/PR692 and the UI gap assessment remain
separate; no contested battle UI files or unmerged UI behavior are dependencies. No full, research,
emulator or screenshot gate was run. The committed planner and actual remote CI are recorded in the
Issue/PR freeze handoff; this implementation does not claim independent acceptance.
