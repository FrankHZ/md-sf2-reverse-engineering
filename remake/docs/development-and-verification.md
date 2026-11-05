# Development and Verification

## Galam shop and supply observation

Use the finite [scope/source agreement](domain/gameplay-rules.md#finite-purchased-supplies-and-detox1)
and [private admission](content/profiles-and-trust.md#finite-galam-shop-admission). This is engine and
Content/adapter work consuming existing static evidence; it adds no research extractor or original
runtime claim. Normal/full research suites and new H3 capture are not required solely for these paths.
Keep selected existing input/tool configuration in the same process as every command.

`ShopServiceTests`, `PoisonCureTests`, `OpeningSaveTests.CompletedShopResourcesUseTheExistingSavePayloadAndRestoreAuthority`
and optional `GalamShopContentTests` exercise actual transaction, effect, connected-entry and save
behavior. The private test selects `SF2_GALAM_WORLD` (prepared metadata world), with separately declared
`party.json`, `start-item.json`, `start-weapon.json` and `start-default.json` beside it. Party input uses
the existing private controlled-start schema and finite accounting/loadout choices. Start documents
use the existing format1 controlled exploration schema: Map16/entity-0/speed32/facingUp; F663 counter
starts are `(35,26)` for item entity144 and `(39,26)` for weapon entity135, flags0/1/2/32/33/34/663.
Default uses no663 and a valid field position such as `(17,22)`; it must not acquire the two shop rows.

Prepare selected source definitions using the existing producer and registered read-only inputs:

```powershell
uv run python -m sf2tool.remake_exploration_content --canonical $canonicalMapExport `
  --upstream $pinnedSource --selection remake/reference/inputs/galam-shop-programs.json `
  --output local/galam/world.json
```

For actual private rendering/audio, the same producer additionally needs `--rom-path $registeredRom`
and `--presentation-root $existingProductAssets`, with the selected source Castle music and the
existing required resources available. The retained bounded observation reused prepared
Map16 graphics, existing `MUSIC_CASTLE` for the field-only profile. Leave `SF2_PRIVATE_BATTLE_SCENE_CONTENT` unselected for this
shop-only world: it has no battle route and requires no unrelated Battle01 scene/field-death
resources. These are private input preparation, not new original capture.
A metadata-only world correctly fails native startup; never remove its required audio/resource guard.
The music choice is source Map16 command38, rather than Town command8. Reuse the existing installation,
project, runtime directory and process ownership. Initialize only necessary changed `.po`/source inputs.

Launch the ordinary host with the selected `SF2_PRIVATE_EXPLORATION_CONTENT`, existing private static/
party selections and `--private-exploration-start <start-item.json or start-weapon.json>`, adding
`--script remake/game/probes/engine_shop_observation.gd` before `--`. `SF2_SHOP_CASE=item|weapon`
selects the input witness. English item and Chinese weapon consumers check actual stock, Buy, recipient,
payment/grant, sale of the actual new slot, gold/removal, caller return and further movement. They read
current controls and labels/glyph/layout/errors; they do not mutate gameplay, synthesize delivery or
capture screenshots. Names retain the established original-name fallback note.

The public authored `shop-supplies.json` and `engine_supplies_observation.gd` form a separate ordinary
input witness: Buy Seed for Medic and Antidote for Guard, leave, walk to the existing battle transition,
consume the purchased Seed for20 HP and Antidote for poison removal, then learned DETOX1 pays3 MP to
cure Scout. Same-session identity, actual slot consumption, visible English/Chinese messages and later
movement are observed. It uses the existing friendly scene and poison lifecycle, not a scripted effect
setter or original-game trace. Example, after protected SDK build and runtime setup:

```powershell
$env:SF2_OBSERVATION_OUTPUT = (Join-Path $PWD 'local/galam/supplies-en.json')
& $env:GODOT_BIN --headless --path remake/game --script `
  remake/game/probes/engine_supplies_observation.gd -- `
  --authored-package remake/content/authored/shop-supplies.json --input-settings $englishInputSettings
```

Run the affected behavior selection and `uv run sf2 verify adapter`; on the clean committed candidate
run `uv run sf2 verify plan --base origin/main --head HEAD`, interpret its actual dependency selection,
and inspect actual CI. Preserve completed failures and rerun only their owning nodes/corrections;
comparison fixtures with Unknown accounting need their own comparison start, whereas action tests need
accounting-bearing starts. Neither input is a universal substitute for the other. Reports stay<=15MiB,
native instances serial/bounded and disk>=1GiB free. Actual private metadata/native inputs are separate
ignored files; private content and local machine paths never enter the PR. Original natural reach,
shop/audio timing, full Map16 and full Battle03–07 connection remain Unknown/Unsupported.

## Silence observation

Use the existing selected SDK/editor, worktree project and native runtime. New launches here follow
a stopped prior native instance and a required new semantic consumer; do not copy installations,
capture screenshots or produce per-frame reports. Load the ignored environment in the same process
as every command, with absolute `DOTNET_BIN`/`DOTNET_CLI_HOME` and forced PATH opt-out.

Affected behavior lives in `SilenceSpellTests`, `SilenceSessionTests` and `SupportDecisionTests`.
Run their actual engine tests and the affected existing physical/healing/AI/poison/equipment/growth
owners, plus `uv run sf2 verify adapter`. Private admission needs selected registered inputs;
`PrivateSourceAiTests` specifically uses `remake/reference/inputs/battle01-player-ready.json`, while
the private learned-player-spell admission test needs its normal configured accounting-bearing start.
Do not apply that comparison override to unrelated private tests. On a clean committed head run
`uv run sf2 verify plan --base origin/main --head HEAD` and inspect actual public CI. Preserve
completed failures and rerun only their affected nodes/files; an older red aggregate is not a new
full-suite requirement.

For native acceptance, build the actual Debug adapter, then launch the selected editor against
`remake/game`, `--script remake/game/probes/engine_silence_observation.gd` and
`--authored-package remake/content/authored/silence-watch.json`, with one English and one Chinese
input-settings selection. Reuse the existing child-only APPDATA/LOCALAPPDATA/TEMP isolation.
`SF2_OBSERVATION_OUTPUT` must name a fresh ignored report destination. Single native instance,
45s/run, <=15MiB total issue reports and >=1GiB free disk remain the budget.

The observer sends ordinary Confirm/Stay/Magic/Cancel/target/Confirm keys; the real host supplies
all simulation/presentation completions. It reads SUPPORT's two area results, one5MP payment,
packed statuses/cards, a still-selectable silenced BLAZE with2MP paid and no damage/EXP, actual
expiry, an effective later BLAZE and usable subsequent movement in the same session. English and
Chinese visible glyph/layout/messages are inspected from the running UI state. No state setters,
synthetic completion or reference-round predicates define legality. Physical infliction, mixed
sleep/silence/poison and private Dark Smoke binding have separate actual engine tests; this authored
witness does not establish original media/natural timing or full Battle07 story integration.

## Enemy spell observation

The [finite rule agreement](domain/gameplay-rules.md#finite-early-enemy-spell-decisions),
[source binding](content/profiles-and-trust.md#early-enemy-spell-admission) and
[capability boundary](capability-status.md#early-enemy-blaze1-and-heal1) separate implementation,
private admission and chapter connection. Run the actual engine's
`EnemySpellDecisionTests|EnemySpellSessionTests|EnemySpellAdmissionTests`, plus affected source
orders/physical, attack-spell, healing/scene/AURA, equipment, decision replacement, progression and
outcome behavior tests. Use locked SDK/input setup below. The pinned binding test needs the
registered seven private inputs. For the retained `PrivateSourceAiTests` comparison specifically,
select `SF2_PRIVATE_CONTROLLED_START` to the absolute resolved
`remake/reference/inputs/battle01-player-ready.json`; the ordinary current private play start has
different seeds/accounting and is not that test's fixture. Run `uv run sf2 rom verify` after loading
selections before diagnosing a private input blocker. Run `uv run sf2 verify adapter`, clean-head
`uv run sf2 verify plan --base origin/main --head HEAD` and inspect actual CI.

Build the existing Godot Debug host after changes. Use the same serial native/runtime/input setup
as [the player-spell recipe](#player-attack-spell-observation), then select
[enemy-spells.json](../content/authored/enemy-spells.json) and
`res://probes/engine_enemy_spell_observation.gd` in place of its package/observer. Launch with
English and Chinese input settings in separate native children. No retained native instance existed
at this slice's start. Fresh `SF2_OBSERVATION_OUTPUT`, isolated child APPDATA/LOCALAPPDATA/TEMP,
45-second deadline, total reports<=15MiB, free disk>=1GiB, one instance and no screenshots apply.
The existing local reproduction is
`pwsh -NoProfile -File local/issue1525/run-native.ps1 -Run <fresh> -Language en|zh-CN`.

Ordinary Confirm/Stay/Confirm dispatches both enemies. BLAZE1 moves Mage(6,2)->(4,2), pays5->3
and damages Scout60->53. HEAL1 moves Cleric(6,5)->(5,5), pays7->4 and heals Wounded8->23.
The observer reads visible result messages, real faction-side projection, mounted glyph/layout
checks, exactly two MP events, no enemy EXP/gold and same-session Scout control; a new Right input
moves its preview2->3. It reads existing state/scene projections and events; it never sets state or
fabricates completions. The authored gesture is not original media/timing or private scene-asset
acceptance. Preserve initial failure logs and existing older failed evidence in the local handoff;
narrow reruns cannot erase a completed red aggregate.

## Player attack-spell observation

The [finite rule owner](domain/gameplay-rules.md#finite-player-attack-spells) and
[Content owner](content/profiles-and-trust.md#attack-spell-admission) distinguish implemented behavior,
private admission and story connection. Use `AttackSpellTests|AttackSpellSessionTests|AttackSpellAdmissionTests`
in the actual Engine.Tests project, plus affected healing/scene/progression/growth/drop/selection
and SourceEnemyAi behavior tests. The pinned source-admission test requires the current registered
static/enemy/gold/controlled-start/encounter inputs; missing inputs mean skipped, not a private PASS.
Run `uv run sf2 verify adapter`, then the clean committed-head
`uv run sf2 verify plan --base origin/main --head HEAD` and inspect actual CI.
Do not replay the old full engine/research suites solely for this engine capability.

Reuse the existing installation, project, input selections and worktree-local native runtime.
Build the Debug host once after product changes with the locked SDK selection:
`& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.
Run `engine_attack_spell_observation.gd` in one serial native child, with a fresh explicit ignored
`SF2_OBSERVATION_OUTPUT`, a45-second process deadline, aggregate local reports<=15MiB and free disk>=1GiB.
The observer uses ordinary menu/target/confirm keys, existing read-only state/scene projections and
`SessionResultObserved`; no setters, fabricated completions, screenshots, frame dumps or original
magic-asset acquisition. Each compact final native report is below1MiB. Retain initial failures and
previous reports; a new run never overwrites them.

For BLAZE2 use [attack-spells.json](../content/authored/attack-spells.json),
`SF2_ATTACK_SPELL_KIND=blaze` and English input settings. For the changed BLAST1 configuration,
create a separate ignored copy **before startup**: only first start actor positionOverride becomes
`{"x":3,"y":2}`; first actor classRule becomes `unpromoted-priest`, physical.movementType `regular`,
level10. Set `SF2_ATTACK_SPELL_KIND=blast` and use Chinese input settings. No state is injected after
session start. The current retained child launcher is
`pwsh -NoProfile -File local/issue1524/run-native.ps1 -Run <fresh> -Kind blaze|blast -Language en|zh-CN`.
Its core launch, from the repository root with absolute resolved paths, is:

```powershell
# Execute inside the bounded native child shell, never the shell used for GitHub credentials.
. ./local/issue1441/inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$nativeRuntime = Join-Path (Get-Location).Path 'local/issue1471/field-01'
$env:APPDATA = Join-Path $nativeRuntime 'appdata'
$env:LOCALAPPDATA = Join-Path $nativeRuntime 'localappdata'
$env:TEMP = Join-Path $nativeRuntime 'temp'
$env:TMP = $env:TEMP
# Set SF2_ATTACK_SPELL_KIND and fresh SF2_OBSERVATION_OUTPUT, and select package/input settings above.
& $env:GODOT_BIN --headless --rendering-method gl_compatibility --fixed-fps 60 `
    --resolution 1920x1080 --path ./remake/game `
    --script res://probes/engine_attack_spell_observation.gd -- `
    --authored-package $selectedPackage --input-settings $selectedInputSettings
```

The observer cancels the magic list without payment/RNG, opens it again, selects BLAZE2 or BLAST1,
checks the real target and confirms. It reads the visible spell announcement, MP payment, fire/wind
drawing, each ordered damage result, death/gold/EXP, next guard control and new Right movement.
Visible English/Chinese labels are checked for glyph coverage, minimum height and viewport bounds.
BLAZE2 hits wing then center, pays20->14, changes wing30->20, kills center4->0, awards49 EXP and
one kill; BLAST1 pays20->18, kills only center, leaves wing30 and awards24 EXP at the changed level.
Both change gold100->107 and return usable guard control. Functional geometry/text assertions are
not physical-device, final-pixel, human-aesthetic, original-effect or natural-route acceptance.

Completed #1491's1801PASS/6FAIL/1SKIP, school03exit2/ObjectDBUnknown, #1523's437PASS/2existing
PrivateBattleOutcomeProgramTests failures and #1519's actual-save observer-lock-file failure remain
in their handoffs. The focused passing checks do not erase or rerun those completed results.

## Early ranged physical observation

Use the [early-ranged authored package](../content/authored/early-ranged.json), with the existing
Godot project/installation and Debug host build. Load the current ignored inputs in the launching
process and follow the [locked SDK workflow](#locked-net-workflow). No emulator, screenshots,
asset preparation, frame capture or full route is required. Changed immutable start content requires
a fresh session; keep native instances serial and retain the installed environment.

The small behavior selection is `EarlyRangedPhysicalTests|EarlyClassAdmissionTests`, plus affected
`BattleEquipmentTests`, `BattleEquipmentSessionTests`, `PhysicalBattleTests`,
`PhysicalRuleConfigurationTests`, `SourceEnemyAiTests`, `TargetSelectionTests`,
`CommandsetContinuationTests` and `PrivateBattleInitializationTests`. Run the production Engine.Tests
project with the ordinary `dotnet test --filter` syntax; no verification-tool tests are added.
The pinned private class test uses the current controlled-start/static/enemy/gold selections;
without them it skips. Run `uv run sf2 verify adapter` and the clean committed-head
`uv run sf2 verify plan --base origin/main --head HEAD`; reconcile planner scope with the current
[engine policy](#scope). The selected local discovery also reproduces the already retained
`PrivateBattleOutcomeProgramTests.ImportedGrowthMatchesTheAcceptedConnectedBattleExperienceBoundary`
and `CompletedFlagSkipsOnlyTheAfterProgramAndRetainsItsJoinTail` multi-element `Single()` failures.
They remain failures; the bounded passing action checks do not relabel them or erase #1491's
completed1801PASS/6FAIL/1SKIP, school03exit2, or ObjectDB teardown Unknown.

Native reproduction starts the authored package with ordinary input settings and observes
`BattleSessionView.ReadObservationJson`, `ReadSceneObservationJson` and `SessionResultObserved`:

1. On lancer control, Confirm -> Equipment -> Unarmed -> Confirm. Check ATT20/range1 and preserved
   gold/RNG; equip Short Spear again and check ATT26/range1-2.
2. Use Attack/Target/Confirm at distance2, acknowledge actual scene messages and verify target HP,
   physical-first event and next archer control. Move the archer twice right, use its bow at distance2,
   and verify the next mage. Stay, observe the enemy physical action and usable lancer control; one
   fresh movement proves subsequent input. Check visible text glyph/minimum-size/viewport geometry.
3. For the second valid configuration, change only the first actor to `unpromoted-archer`, mover
   `archer`, ATT27, items `[173,127,127,127]`; retain baseATT20 and the existing Wooden Arrow45.
   Repeat with Chinese settings. This is changed authored content, not an authoritative-state setter.

The ignored `local/issue1523/run-native.ps1 -Run <fresh> -Kind spear|bow -Language en|zh-CN`
records the two bounded witnesses using the existing equipment observer, current inputs and native
runtime directories in a child PowerShell. Final spear-en01 and bow-zh01 both exit0 and show the
above actual state/input/action/control boundaries; stderr is empty. No Godot production change was
needed. Each run took under3seconds with peak working set below250MiB; each native JSON is below1MiB.
The task keeps combined reports <=15MiB and free disk >=1GiB. Physical device delivery, raster/pixels,
human aesthetics and original natural timing remain Unknown. The earlier #1519 real-save then
observer lock-file-assumption failure remains in its handoff; this witness does not repair it.

## Typed JSON object-shape observation

The bounded [F2 admission correction](evidence/structure-and-quality-review.md#f2--p2-validate-json-object-shape-before-optional-field-discovery)
uses public authored bytes and the ordinary `GameSession.Start(IScenarioSource)` boundary. Run from
the owning checkout root in a fresh PowerShell process after the affected Release build (assemblies
already loaded into a process cannot be replaced). No native Godot or private payload is needed.
The ignored `local/issue1377` records retain the original escapes, corrected portrait witness and
post-fix results; the public inputs and commands below reproduce the claims independently.

```powershell
$ErrorActionPreference = 'Stop'
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
uv run sf2 verify engine
uv run sf2 verify adapter
foreach ($assembly in @('Domain','Application','Content')) {
    Add-Type -Path (Join-Path (Get-Location) "remake/src/Sf2.Remake.$assembly/bin/Release/net8.0/Sf2.Remake.$assembly.dll")
}
function Show-Admission([string]$name, [string]$json) {
    foreach ($api in @('Read','Start')) {
        try {
            $reader = [Sf2.Remake.Content.Scenarios.AuthoredScenarioPackageReader]::FromDocumentBytes([Text.Encoding]::UTF8.GetBytes($json))
            $result = if ($api -eq 'Read') { $reader.Read() } else { [Sf2.Remake.Application.Runtime.GameSession]::Start($reader) }
            [pscustomobject]@{ Case=$name; API=$api; Result=$result.GetType().Name; Failure=$result.Failure }
        } catch { [pscustomobject]@{ Case=$name; API=$api; Escaped=$_.Exception.GetBaseException().Message } }
    }
}
foreach ($json in @('[]','null','1','true','"scalar"','{}','{')) { Show-Admission "root:$json" $json }
$battle = Get-Content -LiteralPath remake/content/authored/hidden-spawns.json -Raw
$world = Get-Content -LiteralPath remake/content/authored/battle-drops.json -Raw
Show-Admission 'valid:battle' $battle
Show-Admission 'valid:connected' $world
$battleEdits = @(
    { param($d) $d.start = @() },
    { param($d) $d.actors[0] = @() },
    { param($d) $d.encounters[0] = @() },
    { param($d) $d.encounters[0].placements[0] = @() },
    { param($d) $d.start.actors[0] = @() },
    { param($d) $d['unexpected'] = 1 },
    { param($d) $d.start['unexpected'] = 1 },
    { param($d) $d.Remove('profile') | Out-Null },
    { param($d) $d.ruleProfile = 'unimplemented' }
)
$worldEdits = @(
    { param($d) $d.battle = @() },
    { param($d) $d.world = @() },
    { param($d) $d.start = @() },
    { param($d) $d.world.programs[0] = @() },
    { param($d) $d.world.maps[0] = @() },
    { param($d) $d.world.maps[0].areas[0] = @() },
    { param($d) $d.world.maps[0].entities[0] = @() },
    { param($d) $d.world.maps[1].battle = @() },
    { param($d) $d.world.presentation = @{ maps=@(); sprites=@(); portraits=@($null) } }
)
foreach ($inputCase in @(@{ Json=$battle; Edits=$battleEdits }, @{ Json=$world; Edits=$worldEdits })) {
    foreach ($edit in $inputCase.Edits) {
        $d = $inputCase.Json | ConvertFrom-Json -AsHashtable
        & $edit $d
        Show-Admission $edit.ToString().Trim() ($d | ConvertTo-Json -Depth 80 -Compress)
    }
}
Show-Admission 'duplicate:root' ($battle.Replace('"formatVersion": 7', '"formatVersion": 7, "formatVersion": 7'))
Show-Admission 'duplicate:start' ($battle.Replace('"initialVitals":', '"encounter":"extra", "initialVitals":'))
```

The malformed object boundaries return `ContentError/object-required` with the fields recorded in F2;
`{}`, syntax, unknown/duplicate and unsupported-rule cases retain their distinct existing results.
Both valid packages admit and start. The selected engine check passed with 1,594 passes and 30
optional-private skips; adapter Release compiled with zero warnings/errors. No new parser/helper
unit tests were added. Private provenance order is confirmed by unchanged reader/call-order review,
not a new private runtime observation. Other malformed fields/opcode discriminators remain outside
this bounded claim. Exact candidate plan, actual CI and independent acceptance belong in the PR.

## Authored field-player return

The [Standard scope and binding lifetime](application/session-and-programs.md#field-player-binding-across-connected-battles)
and [B1 disposition](evidence/engine-script-boundary-review.md#b1--implemented-retain-field-player-binding-across-battle)
own this bounded correction. `FieldPlayerReturnTests` exercises actual entry, outcome/program and
retreat behavior with varied field IDs/speeds. Terminal projection tests construct valid terminal
battle states to isolate return from damage calculation; paid retreat uses ordinary session commands.
The native witness below covers an actual attack/victory through the ordinary host.

Load `local/private-inputs.ps1` and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` before SDK launches.
The focused command, after locked restore, is:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter FullyQualifiedName~FieldPlayerReturnTests
```

Before the correction, the two named-player outcome cases failed with `program-entity` at the reached
after-program. Preserve that red result separately from fixture authoring errors. Corrected tests
cover named player motion/text/presentation waits, source population's speed32 reset, authored
`entity-0` retaining its live speed, direct startup, later transfer, legitimate pre-battle speed
changes, failed initialization and retained later-failure cursor/prefix. Select `uv run sf2 verify engine`
and `uv run sf2 verify adapter`, and inspect the committed planner and actual CI.

For the one native witness, use the existing installation/project and Debug host build. From the
repository root, choose a fresh ignored output folder in `$freshIgnoredOutput`, load the existing
private/tool selections, and create the variant **before** startup:

```powershell
$repoRoot = (Get-Location).Path
New-Item -ItemType Directory -Path $freshIgnoredOutput -ErrorAction Stop | Out-Null
$d = Get-Content -LiteralPath 'remake/content/authored/battle-drops.json' -Raw | ConvertFrom-Json -AsHashtable
$d.start.map = 'arena'
$d.start.flags += 401
$d.start.player = 'traveler'
$d.start.speed = 96
($d.world.programs | Where-Object id -eq 'before').instructions = @(@{op='end'})
$package = Join-Path $freshIgnoredOutput 'package.json'
$d | ConvertTo-Json -Depth 80 | Set-Content -LiteralPath $package -Encoding utf8
$inputSettings = Join-Path $freshIgnoredOutput 'input.json'
'{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}' |
  Set-Content -LiteralPath $inputSettings -Encoding utf8
$env:SF2_HOST_LIFECYCLE_CASE = 'direct-return'
$env:SF2_EXPECT_FIELD_PLAYER = 'traveler'
$env:SF2_EXPECT_FIELD_SPEED = '96'
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
```

Select worktree-local APPDATA/LOCALAPPDATA/TEMP/TMP beneath that fresh output as in the
[ordinary host recipe](#ordinary-host-lifecycle-observation), then launch the existing `$env:GODOT_BIN`
hidden with separate arguments: `--headless --fixed-fps 60 --path <repo>/remake/game
--script res://probes/engine_host_lifecycle_observation.gd -- --authored-package <package>
--input-settings <inputSettings>`. Retain stdout/stderr and stop the owned child on its bounded
deadline. The probe sends ordinary keys and reads existing state; it adds no completion injection,
screenshots, asset acquisition or new capture interface. Bound the report/logs to512KiB and one
short run; other lifecycle combinations belong to the unit tests.

The witness passed with one real victory/return and the same session's `traveler` moving x768→864
at y768 with both speeds96. It exited0 without runtime errors in about1.5s; report/logs total38KiB.
This confirms usable authored return/movement, not original-route or hardware parity. Optional
private tests remain explicitly skipped by the public engine entry. Exact head/tree, retained red
results, selected gates, CI and independent acceptance belong in the PR handoff.

## Ordinary host lifecycle observation

[Issue #1375](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1375) repairs
[audit F1](evidence/structure-and-quality-review.md#f1--p1-make-host-ownership-and-return-wiring-cover-every-admitted-entry).
The [capability agreement](capability-status.md#ordinary-host-lifecycle-agreement) owns its Standard
rigor and bounded acceptance. `engine_host_lifecycle_observation.gd` drives ordinary keys and reads
actual presenter, session, wait, queue, text, outcome and field movement facts. No post-start gameplay
injection, fabricated completions, screenshots, media acquisition or engine/content change is used.
The field keeps before/load/start delivery; battle consumes initial round programs/appearances and
terminal/failure snapshots. Direct startup must have complete return wiring.

Author these small variants **before** startup from the repository root. Save the following as an
ignored `local/issue1375/prepare.py`, after creating that directory, and run
`python -X utf8 local/issue1375/prepare.py`. The connected initialization follows the existing
`HiddenSpawnSessionTests.OrdinaryLeaderTerminationAndWalkingRetryResetDormancyButKeepStoryAndItemCustody`
recipe; its added BattleLoad cue and two text programs witness field-owned mounting/load/start waits.

```python
import copy,json
from pathlib import Path
out=Path('local/issue1375')
def read(name): return json.loads((Path('remake/content/authored')/(name+'.json')).read_text(encoding='utf-8-sig'))
def save(name,d): (out/(name+'.json')).write_text(json.dumps(d,indent=2)+'\n',encoding='utf-8')
h=read('hidden-spawns'); h['start']['actors'][0]['positionOverride']={'x':2,'y':3}; save('initial-appearance',h)
j=read('jaro-conversion'); j['start']['actors'][0]['positionOverride']={'x':2,'y':3}; save('initial-program',j)
f=copy.deepcopy(j); f['battleStory']['programs'][0]['instructions']=[{'op':'set-flag','flag':402,'value':True},{'op':'reset-party-battle-stats'},{'op':'end'}]; save('battle-failure',f)
d=read('battle-drops'); d['start']['map']='arena'; d['start']['flags'].append(401)
next(p for p in d['world']['programs'] if p['id']=='before')['instructions']=[{'op':'end'}]; save('direct-return',d)
c=read('battle-drops'); b=c['battle']; e=b['encounters'][0]; t=read('hidden-spawns')['encounters'][0]
i=copy.deepcopy(t['initialization']); i['enemyCount']=2; i['regions'][0]['vertices']=[{'x':1,'y':1},{'x':3,'y':1},{'x':3,'y':2},{'x':1,'y':2}]
for n,slot in enumerate([0,1,128,129]):
 e['placements'][n]['initialization']=copy.deepcopy(t['placements'][2 if n==3 else 3 if n==2 else 0]['initialization'])
 i['sourceCombatants'][n]['actor']=e['placements'][n]['actor']; i['sourceCombatants'][n]['slot']=slot
 b['start']['actors'][n]['initialization']={'initialized':n!=3,'activationWord':512 if n==3 else 0}
e['initialization']=i; b['start']['actors'][3]['hp']=0; b['actors'][3]['items']=[]; b['start']['itemCustody']['onceFlags']=[24]
r=next(m for m in c['world']['maps'] if m['id']=='arena')['battle']
for name in ['load','start']:
 r[name]={'program':'lifecycle-'+name,'instruction':0}
 c['world']['programs'].append({'id':'lifecycle-'+name,'instructions':[{'op':'show-text','mode':'single','speaker':None,'text':100},{'op':'end'}]})
next(p for p in c['world']['programs'] if p['id']=='lifecycle-load')['instructions'].insert(0,{'op':'present','kind':'BattleLoad','resource':None,'entity':None,'position':None})
save('connected-appearance',c)
p=copy.deepcopy(c); p['start']['map']='arena'; p['start']['flags'].append(401)
next(row for row in p['world']['programs'] if row['id']=='before')['instructions']=[{'op':'end'}]; save('initial-prebattle',p)
z=read('poison-turn'); z['start']['actors'][0]['hp']=0; save('terminal',z)
save('ordinary-control',read('hidden-spawns'))
(out/'input.json').write_text('{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}\n',encoding='utf-8')
```

Load `local/private-inputs.ps1` in each launching process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and reuse the existing absolute SDK/shared CLI-home/Godot
selections and worktree caches. Run `uv run sf2 verify adapter`; from `remake/` build
`& $env:DOTNET_BIN build game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.
No retained instance existed at entry; each serial startup below is necessary for a distinct entry
condition. On a clean committed head inspect `uv run sf2 verify plan --base origin/main --head HEAD`
plus direct ownership/docs/source-size/private checks and actual exact-head CI. Adapter changes add
no engine-behavior tests and select no normal/full research/H3/H4/comparator run.

For each case below select a fresh ignored run directory, then launch the same project. Keep native
stdout/stderr and JSON, await actual process completion, and require both `passed:true` and no runtime
errors. Native exit0 with an exception/error is **FAIL**. Example from the repository root:

```powershell
. ./local/private-inputs.ps1
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$repoRoot = (Get-Location).Path
$case = 'initial-appearance' # Repeat serially for the named cases, in fresh directories.
$runRoot = Join-Path $repoRoot "local/issue1375/$case-run"
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
foreach ($dir in @('appdata', 'localappdata', 'temp')) {
    New-Item -ItemType Directory -Path (Join-Path $runRoot $dir) -Force | Out-Null
}
$env:APPDATA = Join-Path $runRoot 'appdata'
$env:LOCALAPPDATA = Join-Path $runRoot 'localappdata'
$env:TEMP = Join-Path $runRoot 'temp'; $env:TMP = $env:TEMP
$env:SF2_HOST_LIFECYCLE_CASE = $case
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $runRoot 'native.json'
$nativeArgs = @('--headless', '--fixed-fps', '60', '--path', "`"$(Join-Path $repoRoot 'remake/game')`"",
    '--script', 'res://probes/engine_host_lifecycle_observation.gd', '--',
    '--authored-package', "`"$(Join-Path $repoRoot "local/issue1375/$case.json")`"",
    '--input-settings', "`"$(Join-Path $repoRoot 'local/issue1375/input.json')`"")
$p = Start-Process -FilePath $env:GODOT_BIN -ArgumentList $nativeArgs -WorkingDirectory $repoRoot `
    -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runRoot 'stdout.log') `
    -RedirectStandardError (Join-Path $runRoot 'stderr.log')
if (-not $p.WaitForExit(130000)) { Stop-Process -Id $p.Id -Force; throw 'Native deadline' }
$p.Refresh()
$p.ExitCode # Inspect JSON and both logs as well; the exit code alone is insufficient.
```

Resource estimate: eight small serial cases, minutes and <2MiB total semantic reports/logs; no
acquisition or API/model processing. Existing bounded roster/region work is unchanged; the routing
decision is O(1). Reassess a concrete mismatch instead of adding a general observation platform.
Actual successful native elapsed times were 0.35–1.28s at fixed-fps60, with 245,982 bytes total reports/
logs including the failed attempt and two strengthened load-control starts. These are modern authored observations, not original timing.

| Case | Actual result / boundary |
| --- | --- |
| `initial-appearance` | PASS: round0, ordered `[returner, hidden]` tokens, blocked input, real spin/notice, exactly-once completion before one queue and usable movement/cancel. |
| `initial-program` | PASS: round0 region program, camera completion, two mounted text acknowledgements, one region run, then usable battle control. |
| `connected-appearance` | PASS: field interaction/before text, one real BattleLoad mount beneath field-owned load/start with `ActiveBattle`, one `survivor` appearance, one queue, detached field, usable battle control. |
| `direct-return` | PASS on corrected witness: direct initial battle control, ordinary physical victory, one return view, one completed return, same-session field movement. |
| `initial-prebattle` | PASS: root initially selects field for `ActiveBattle` load; real BattleLoad mounts once and load/start text finishes before first-round appearance and battle control. |
| `ordinary-control` | PASS: unmodified hidden-spawns start, action/cancel, STAY, automatic work and round2 player control. |
| `terminal` | PASS: initial terminal Defeat label, no field attachment, input cannot restart the ended battle. |
| `battle-failure` | PASS: initial stopped region program displays `battle-program-instruction`, retains round0 and rejects further input. This expected engine diagnostic is distinct from a native runtime error. |

Retained ignored evidence is under `local/issue1375/<case>-run1`; final direct return, connected
appearance and initial-prebattle reports are in the corresponding `-run2` directories. The latter two
strengthen the earlier text-only controls with the actual BattleLoad mount callback; the original
controls and variants remain retained. All successful processes completed exit0 without native errors. The first
`direct-return-run1` is **FAIL**: the probe reconnected its signal when a view reparented, then read
nonexistent top-level position keys. Its logs remain; the errored process was explicitly stopped
(exit-1), with no completed JSON report. The corrected probe guards connections and reads live entity
positions; only the affected direct-return witness reran. No product change was needed for those
probe errors. Adapter Release and Debug builds passed with 0 warnings/errors.

The audit's `local/issue1373` red startup/control and public API results remain untouched. F1's old
connected/return symptom limits remain labeled Inferred at the audit base; the witnesses above provide
separate authored evidence after repair. F2/F3, #1047 Unknowns, private media/natural original reach,
#638 pause and ordinary export/package completeness remain unresolved. Frozen
[PR #692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) must semantically rebase its
overlapping presentation/view work onto this lifecycle boundary.

## Jaro victory authored observation

Use [jaro-victory.json](../content/authored/jaro-victory.json) in the ordinary host. Acknowledge the
before text, Enter/F/Tab/Enter attacks the adjacent enemy, then Enter acknowledges the battle and
recruitment text. The real host advances typed waits and returns to the saved field anchor; Left
moves afterward. No post-start state injection or synthetic presentation completions.

`JaroVictoryTests`/`JaroVictoryProgramTests` own actual engine acceptance alongside affected Jaro,
outcome, party-list and source-story tests. Load `local/private-inputs.ps1` in the launching process,
force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, then run `uv run sf2 verify engine`,
`uv run sf2 verify adapter` and the Debug adapter build from `remake/`. Preserve completed failures;
rerun corrected/affected cases. On a clean committed head inspect `uv run sf2 verify plan --base
origin/main --head HEAD`, direct document/link/source-size/private checks and actual exact-head CI.
No normal/full research, emulator, H3/H4, comparator or asset acquisition is selected.

Reuse the owned SDK/Godot/project. No retained instance exists at slice start; starting the built
adapter is necessary for the one native delivery witness. Resolve paths from the owning repository:

```powershell
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_jaro_victory_observation.gd -- `
  --authored-package (Join-Path $repoRoot 'remake/content/authored/jaro-victory.json') `
  --input-settings (Join-Path $freshIgnoredOutput 'input.json')
```

Use `{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}` for input.json. Keep
APPDATA/LOCALAPPDATA/TEMP/TMP, stdout/stderr and report in a fresh ignored `local/issue1371` run
folder; await actual process exit. One serial witness targets <180s and <2MiB semantic output.
Membership work is O(30), party publication O(N), and the small package/witness needs no acquisition
or model/API processing. The probe reads actual text, flags, resources, caches and control, tests
hidden-view input suspension and counts exactly-once writes; screenshots are prohibited. A failed witness
retains its output and only the corrected witness reruns. Private skips establish no private claim.

## Poison authored observation

Use [poison-turn.json](../content/authored/poison-turn.json) in the ordinary host. Enter, Space, Enter
commits STAY: champion3→1 receives nonlethal feedback; receiver2→0 then shows exhaustion, ordered
field removal and one defeat before next-round champion movement/cancel remains usable. No post-start
state injection. `engine_poison_observation.gd` drives ordinary keys and reads actual state/text,
checks focus/hide suspension and exactly-once damage/action/death counts. The terminal outcome and
automatic standby variants are small engine tests, not extra native trajectories.

Load `local/private-inputs.ps1` in the launch process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`,
and reuse the selected SDK/Godot and existing project. Run `uv run sf2 verify engine`,
`uv run sf2 verify adapter` and the adapter Debug build. No retained instance exists at slice start,
so a startup of the newly built adapter is necessary. Resolve paths from the owning repository root:

```powershell
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_poison_observation.gd -- `
  --authored-package (Join-Path $repoRoot 'remake/content/authored/poison-turn.json') `
  --input-settings (Join-Path $freshIgnoredOutput 'input.json')
```

Use `{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}` for input.json. Keep
APPDATA, LOCALAPPDATA, TEMP/TMP, stdout/stderr and JSON in fresh ignored `local/issue1367` run folders;
await actual native process exit. One serial instance, <180s and <2MiB semantic output are the bounded
target; no assets, screenshots, emulator/H3/H4 or normal/full research run. No general observer platform.

`PoisonTurnTests`/`PoisonSessionTests` cover HP1/2/3/high, dead/unplaced/status0, player and automatic
standby, action-ending outcomes, retained credit/sentinel, growth and healing/item/physical ordering,
free inventory operations, typed/stale completion, missing context and outcome-failure retry after
HP/cleanup. Existing sleep/field-death/action tests are affected regressions. Preserve completed
failures and rerun failed/affected cases. On a clean committed head inspect `uv run sf2 verify plan
--base origin/main --head HEAD`, direct docs/ownership/private-boundary/source-size checks and exact-head
CI. Optional private skips do not make a private poison claim.

## Battle discard authored observation

Use [battle-discard.json](../content/authored/battle-discard.json) in the ordinary host. Enter then R
opens Drop; directions select exact slots, Enter selects/confirms and Escape cancels back to actions.
Sky Orb refuses visibly. Herb and equipped Achilles discard show authored result text; actual reveal
and acknowledgement return the same actor's action and preview. A later outer Escape returns movement
while retaining inventory/equipment/Deals. E and I remain ordinary independent equip/use commands.

`BattleDiscardTests` and `BattleDiscardSessionTests` cover actual behavior with duplicate/empty/last
slots, ordinary/rare/protected metadata, Deals0/14/15 and absent custody/entry, current base/growth,
live range/use, stale state/envelopes, cancellation and repeated delivery. Run configured
`uv run sf2 verify engine`, `uv run sf2 verify adapter` and Debug adapter build. Preserve completed
failures and rerun affected cases after corrections. On a clean committed head inspect
`uv run sf2 verify plan --base origin/main --head HEAD`, exact-head CI, source sizes and direct
ownership/document/link/private-boundary checks. Optional private skips make no private claim.

Load `local/private-inputs.ps1` in the launch process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`; reuse the selected SDK/Godot/project. A changed binary or no
retained instance requires startup. Build Debug from `remake/` using the configured SDK, then resolve
absolute project/package/output paths before launch:

```powershell
$repoRoot = (Get-Location).Path
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_discard_observation.gd -- `
  --authored-package (Join-Path $repoRoot 'remake/content/authored/battle-discard.json') `
  --input-settings (Join-Path $freshIgnoredOutput 'input.json')
```

Use `{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}` for `input.json`.
Keep APPDATA, LOCALAPPDATA, TEMP/TMP, stdout/stderr and report in a fresh `local/issue1365` run
subdirectory. Await actual process completion if the GUI launcher detaches. The compact probe uses
ordinary keys at1280×720 and reads actual state/text/errors: protected refusal, both cancellations,
normal and equipped rare removal, focus/hide suspension, unchanged actor/preview/turn/RNG, later
STAY, Herb use and usable movement. No post-start gameplay injection, synthetic completion,
screenshots, frame capture or new assets. JSON numeric arrays use float expected entries.

Resource plan: one serial native instance, one small authored package and fixed semantic samples;
target <180 seconds and <2MiB. Drop is O(4) plus existing O(N) state publication/offer traversal.
No research/full/H3/H4 or original acquisition is selected. Modern authored delivery makes no original
natural-route/timing claim; retain the twelve #1047 Unknowns, #638 pause and
[predecessor failure records](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1363#issuecomment-6458523190).

## Battle Give authored observation

[battle-give.json](../content/authored/battle-give.json) uses the ordinary host. Enter then G opens
Give; directions select slots, Enter selects, Tab/directions choose the recipient, and Enter advances
to final confirmation or a full-recipient exchange slot. Escape backs up one selection stage. The
result text must be delivered and acknowledged before the ordinary next turn. E equips later
received weapons and I uses eligible items independently of transfer eligibility.

`BattleGiveTests` and `BattleGiveSessionTests` vary exact duplicate slots, compaction/first-empty
order, full capacity, geometry/status, current base/equipment/growth, stale words/envelopes, unknown
targets, cancellation, typed delivery, suffix failure and later use/equip. Run the configured
`uv run sf2 verify engine` and `uv run sf2 verify adapter`; after corrections rerun only failed or
affected cases. A completed public suite with private tests skipped makes no private assertion.
Inspect the clean-head planner and actual scoped CI. Preserve all prior failure logs/results.

Build the owning adapter Debug configuration after loading `local/private-inputs.ps1` in the same
process and forcing `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the existing selected Godot
installation and project; startup is necessary when no owned instance remains or the adapter binary
changed. Resolve absolute project/package/output paths before launch:

```powershell
$repoRoot = (Get-Location).Path
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_give_observation.gd -- `
  --authored-package (Join-Path $repoRoot 'remake/content/authored/battle-give.json') `
  --input-settings (Join-Path $freshIgnoredOutput 'input.json')
```

Use `{"formatVersion":1,"textMode":"adjustable","charactersPerSecond":120}` for `input.json`.
Keep APPDATA, LOCALAPPDATA, TEMP/TMP and stdout/stderr in the fresh ignored output. Await the
actual process completion if the Windows GUI launcher detaches. The finite observer checks
normal Give and full exchange, both equipped removals, readable feedback, cancellation at each
stage, focus/hide suspension, subsequent received-weapon equip, Herb use and movement/cancel.
It sends only ordinary keys and reads live state/labels/errors: no post-start gameplay injection,
synthetic completion, screenshots or per-frame capture. JSON numeric slot arrays compare as floats.

Resource plan: one small authored package, one serial native instance, fixed semantic samples and
command events (target <2 MiB and <180 seconds); no asset acquisition/network/native reference run.
Transfer work is O(4); target queries visit at most the admitted roster and existing movement grid.
Retain both failed and corrected observations. Modern authored delivery is not original natural
reach or hardware timing; #1047 Unknowns and #638 pause remain unchanged.

## Battle equipment authored observation

Use [battle-equipment.json](../content/authored/battle-equipment.json) in the ordinary host. Enter,
F, Tab, Enter attacks the adjacent guardian while carrying Achilles; acknowledge the scene with
Enter. At the next player turn, Enter then E opens equipment. Directions cycle slots (including
Unarmed when fewer than four eligible weapons); Enter confirms and Escape cancels. Equip Achilles,
return through movement/action cancellation and repeat the attack to see effective damage.

`BattleEquipmentTests` and `BattleEquipmentSessionTests` cover actual rules, admission and session
behavior: same/duplicate slots, zero/four eligible slots, masks/nonweapon flags, base/live stats,
nonzero ATT growth, counter range, stale/invalid input and one publication. Run affected cases with
the configured absolute SDK; `uv run sf2 verify engine`, `uv run sf2 verify adapter` and the committed
`uv run sf2 verify plan --base origin/main --head HEAD` own normal scope. Preserve completed failures
and rerun their cases/files rather than repeating the aggregate to replace its result.

Load `local/private-inputs.ps1` in the launching process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and build the actual adapter Debug configuration with the
configured SDK. Reuse the owned project/Godot installation. A missing retained instance or a changed
adapter binary requires startup; no install, extraction or private art is needed. Resolve absolute
paths before Godot changes its working directory:

```powershell
$repoRoot = (Get-Location).Path
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path (Join-Path $repoRoot 'remake/game') `
  --script res://probes/engine_equipment_observation.gd -- `
  --authored-package (Join-Path $repoRoot 'remake/content/authored/battle-equipment.json')
```

Keep APPDATA, LOCALAPPDATA, TEMP/TMP, report and logs under the fresh ignored output directory.
Wait for the actual process/report completion when the Windows GUI launcher returns early. The
finite probe uses ordinary keys and actual state/labels/errors, including focus/hide input suspension,
preview/cancel, Unarmed, alternate range, Achilles confirmation, carried/equipped Taros attacks and
continued movement/cancel. Target <180 seconds and <2 MiB; retain compact semantic boundaries, no
per-frame dumps or screenshots. This is modern authored semantic acceptance, not original natural
reach, hardware UI/timing, connected/private equipment or the twelve #1047 Unknowns. Existing
comparison failures retain their historical owners; the Issue handoff records this slice's failures.

## AURA authored observation

Launch `remake/content/authored/aura-healing.json` with `--authored-package`. Enter opens actions,
H cycles AURA1–4, Tab cycles centers and Enter confirms. AURA4 selects self and recovers living
placed allies in source-slot order, including neutral/distant actors. Escape cancels. Confirm advances
recovery/reward messages before next-turn control.

Load `local/private-inputs.ps1` in every launching process; preserve the configured absolute SDK and
shared CLI home and force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Run affected `AuraHealingTests`,
`AuraHealingSessionTests` and `HealingRulesTests`, configured `uv run sf2 verify engine`,
`uv run sf2 verify adapter`, and the existing Godot Debug build. Preserve completed failures and
rerun affected cases after correction. On a clean committed head inspect
`uv run sf2 verify plan --base origin/main --head HEAD`, then exact-head CI and direct
ownership/source-size/private/document review. No research/full/H4/emulator gate is selected.
[Issue #1359](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1359) retains the current
CI/failure handoff. The twelve #1047 comparison Unknowns and
[predecessor failures](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1357#issuecomment-6451208918)
remain unchanged.

Reuse the owned installation/project. Launch when no instance is retained or changed adapter
assemblies need loading. Put APPDATA/LOCALAPPDATA/TEMP, logs and report in a fresh ignored
`local/issue1359/` directory, then run:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/aura-healing.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_aura_observation.gd -- --authored-package $package
```

The observer uses ordinary keys and window hide/focus lifecycle events, reads real session/draw/error
state and ordered cost/recovery/contribution/award facts, then moves the next actor in the same
session. It injects no gameplay state or presentation completion and takes no screenshots. Each
witness stays below 180 seconds/5 MiB with compact transition samples. This proves authored delivery, not
natural source reach, fairy RNG or original media/timing.

## Caravan authored observation

Launch `remake/content/authored/caravan-travel.json` with `--authored-package`. Right boards the
nearby marker after actual motion/shrinking. Five more Right inputs drive across the admitted river;
another Right lands. Down then Up demonstrates ordinary walking and its green zone flag. Arrow keys
are cardinal commands; stages own control until their existing motion/sprite/view waits complete.

Load `local/private-inputs.ps1` in the launching process and force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Build the existing Godot project in Debug with the configured
SDK. Reuse the owned installation/project and running instance. Start an instance when none is
available; rebuilding changed adapter code requires loading that build. Put APPDATA/LOCALAPPDATA/TEMP
and logs/reports in a fresh ignored
`local/issue1357/` subdirectory. Run the existing observer:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/caravan-travel.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_caravan_observation.gd -- --authored-package $package
```

The probe uses ordinary keys, temporarily hides/shows the view to exercise suspension, and reads
actual session/draw/error state. It does not inject gameplay state or complete engine waits itself.
Each witness is bounded to 180s/5MiB and records compact transition samples. Check same-session
boarding/travel/landing, real position/scale/appearance delivery, preserved list/step counter during
travel/landing, ordinary Confirm while the physical Caravan is parked off-map, exactly two mode
commits, and later walking/zone behavior.

Run affected `CaravanTests`, configured `uv run sf2 verify engine`, `uv run sf2 verify adapter` and
Debug build. On the clean committed head inspect `uv run sf2 verify plan --base origin/main --head HEAD`,
then exact-head CI and direct docs/diff/private/source-size review. Preserve completed failures and
rerun affected cases only after correction; no research/full/H4/emulator suite is selected here.
Prior failures remain in the [predecessor handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1355#issuecomment-6445201005),
and the twelve #1047 comparison Unknowns remain unchanged. Natural source reach, original timing,
full raft behavior and durable transport persistence remain Unknown or outside this admission.

## Jaro conversion authored observation

Launch `remake/content/authored/jaro-conversion.json` with `--authored-package`. Up twice, then
Enter/Space/Enter, moves and stays inside the trigger. The next eligible round commits the played
flag and conversion; Enter acknowledges each of two paraphrased dialogue windows. The new queue
returns leader control; another Enter/Space/Enter permits the allied automatic physical action.
Enter acknowledges ordinary battle messages. Down proves usable later player movement.

Load the worktree's `local/private-inputs.ps1` in the launching process; force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`. Reuse the configured SDK/Godot and build the adapter Debug
configuration. No instance was retained at transfer, so startup for this witness is required. Use a
fresh ignored `local/issue1355/` subdirectory for APPDATA/LOCALAPPDATA/TEMP, log and report:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/jaro-conversion.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_jaro_conversion_observation.gd -- --authored-package $package
```

The witness supplies ordinary keys only and reads the running session/nodes/input/errors. It observes
same-session scene delivery, changed retained coordinates, physical alias/camera geometry and visible
facing, automatic ally combat, played-flag
suppression and usable movement. Each run is bounded to180s/5MiB; no screenshots, frame collection,
state injection or fabricated completions. Changed/dead/retained actors, alternate IDs/slots,
list/control/RNG rules and pending failures are covered by the small engine behavior tests. Run
`uv run sf2 verify engine`, affected `uv run sf2 verify adapter` and Debug build, plus the clean
committed planner interpreted as engine-only, direct docs/private/diff/source-size checks and CI.

Preserve completed failures and narrow correction runs under the Issue handoff. Predecessor failures
remain at [PR1354](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1354#issuecomment-6434646121);
the twelve #1047 Unknown comparisons are unchanged. No full/research/H4/retired/emulator suite or
private Battle32 acquisition is implied. Original natural reach, alias timing and presentation remain Unknown.

## Hidden-spawn authored observation

Start `remake/content/authored/hidden-spawns.json` through `--authored-package`. Up moves the ally
into the trigger; Enter/Space/Enter commits Stay. The source enemy's ordinary turn clears the checked
word, and the next round resets/appears the returner then hidden enemy. Enter/F/Tab/Enter attacks the
returner; Enter acknowledges combat messages. Its ordinary death is followed by a later-round actual
appearance. Down then demonstrates usable movement. No actor IDs or this route define legality.

Load the retained `local/private-inputs.ps1` in the launch process, force
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and build the actual adapter Debug configuration with the
selected absolute SDK. Reuse the project/installation; a new instance is justified when none is
retained. Use a fresh worktree-local ignored directory for APPDATA/LOCALAPPDATA/TEMP/logs/report:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/hidden-spawns.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_hidden_spawns_observation.gd -- --authored-package $package
```

The bounded probe supplies ordinary keys only after startup. It reads real appearance completion,
intermediate marker rotation/alpha, settled values, pending-member hiding, ordered queue/appearance
facts, ordinary death and subsequent movement, with errors surfaced. Each run is limited to180s and
5MiB semantic output. No screenshots, state mutation or fabricated completion. Engine behavior tests
cover mode2 death, mode3 return, source geometry/checked bits, reset refusal, input validation, failure
prefixes and ordinary leader return/retry with retained custody. Use `uv run sf2 verify engine`,
affected adapter/Debug build, clean committed planner and exact-head CI. Preserve completed failures;
correct only affected cases instead of rerunning an aggregate to erase a red result.

Prior #1351's seven failures/corrections remain in its
[handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1352#issuecomment-6434280116), along
with the earlier #1341/#1343/#1345/#1347/#1349 failures and twelve unresolved #1047 comparisons.
No full/retired/H4/emulator run or private Battle44 acquisition follows from this slice.

## Prism laser authored observation

Use `remake/content/authored/prism-laser.json` with ordinary `--authored-package` startup. Both allies
start off the ray. Enter/Space/Enter commits Stay for each, producing no laser. On their next turns,
Up moves each onto the ray; wait for movement delivery, then Enter/Space/Enter commits Stay. Enter
acknowledges laser and damage/death messages. The Flower hits companion, leader and sentry in source
slot order, removes the two HP1 targets without ally kill credit, and returns leader control.
Down demonstrates usable movement. No particular IDs/order are engine legality conditions.

Load `local/private-inputs.ps1` in the launch process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`,
and build the actual adapter Debug configuration using the configured absolute SDK. Reuse the owned
Godot/project; only startup needs a new instance when none is retained. With a fresh ignored
worktree-local run directory for APPDATA/LOCALAPPDATA/TEMP/logs/report, run from repository root:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/prism-laser.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_prism_laser_observation.gd -- --authored-package $package
```

The probe uses ordinary keys only, with compact semantic view/input/error observations. Bound each
run to180s and5MiB; no screenshots, media capture, post-start mutation or fabricated completion.
Actual `PrismLaserTests`/`PrismLaserSessionTests` also cover empty reconstruction/scene completion,
changed directions/positions/slots, normal damage, rejection, leader sweep and retained failure prefixes.
Run `uv run sf2 verify engine`, affected adapter, clean committed dependency plan and exact-head CI.
Preserve completed failures and correct their affected tests rather than repeat a completed aggregate.
Historical #1341/#1343/#1345/#1347/#1349 failures and twelve unresolved #1047 comparisons remain with
their existing owners. No retired/full/H4/emulator run follows from this authored slice.


Use this entry for environment, scope and check selection. Choose a focused recipe only for an
affected behavior: [host/input](verification/host.md), [battle/rules/scenes](verification/battles.md)
or [field/programs](verification/field-programs.md). [Retained evidence](evidence/retained-comparisons.md)
separates accepted composition from historical failures and retired run instructions.

The selected [Battle01-through-Battle02 recipe](verification/field-programs.md#battle01-through-battle02)
owns private preparation, ordinary human-play launch and semantic observation for the second encounter.
It starts at an explicit tower boundary, keeps the subsequent route continuous, and stops before
Battle02 resolution. Its candidate results do not extend the independently accepted milestone.

Documentation upkeep uses Standard rigor: current behavior/navigation and preserved evidence are
the acceptance boundary. It changes no gameplay, original evidence, UX policy or verification
framework. Historical dispatch plans and audit dispositions belong to Git/Issues, not this guide.

## Battle item-drop authored observations

Use `battle-drops.json` and `battle-drops-full.json` under `remake/content/authored/` with the ordinary
`--authored-package` host. Enter interacts and acknowledges the before text; Enter/F/Tab/Enter selects
and commits the enemy attack. Enter acknowledges scene text. Direct receipt displays dropped and
received messages; full-pack Deals fallback is silent. After usable field return, Left walks onto
the authored retry event; repeat the attack. Both reached drop draws are zero, so the second kill
exercises the held once bit rather than merely a failed chance roll.

Load `local/private-inputs.ps1` in the launching process, force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`,
and build `game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore` from `remake/` using the
configured absolute `DOTNET_BIN`. Reuse the existing Godot/project; each variant needs its own startup
package, with no hot state injection. Set worktree-local APPDATA/LOCALAPPDATA/TEMP, logs and a fresh
ignored report destination. From the repository root:

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$package = (Resolve-Path -LiteralPath 'remake/content/authored/battle-drops.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_battle_drops_observation.gd -- --authored-package $package
```

Repeat with `battle-drops-full.json` and a separate destination. The probe submits ordinary keys,
observes real session/presenter errors, message/token/field-death waits, inventory/accounting,
once flags and Deals through return and walking reentry, then repeats the kill. It never supplies
successful completion or post-start state. Compact samples and selected semantic events fit the
180s/5MiB per-run budget. No screenshots, exports, new media or bulk capture are involved.

Run `EnemyItemDropTests`/`EnemyItemDropSessionTests`, `uv run sf2 verify engine`, affected adapter
build, and the clean committed planner. Preserve completed failures and rerun only corrected nodes
or newly affected checks. Native results establish authored engine/host behavior; static source
provenance and original runtime limits remain with the rule/research owners. Independent review
and exact-head CI remain required; no H4/emulator or retired aggregate is implied.

## Leader cleanup authored observation

Use `--authored-package <absolute path to remake/content/authored/leader-cleanup.json>` with the
ordinary host. Enter interacts with the ferryman; Enter acknowledges the before text. At battle
control, Enter confirms the position, F chooses ATTACK, Tab selects `enemy`, and Enter commits.
Acknowledge ordinary scene/result text with Enter. The gate and living survivor use the existing
field death phases, followed by victory text and usable exploration; Left moves the returned player.

After loading `local/private-inputs.ps1`, forcing `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`, and building
the actual adapter Debug configuration from `remake/`, run from the repository root:

```powershell
$package = (Resolve-Path -LiteralPath 'remake/content/authored/leader-cleanup.json').Path
$project = (Resolve-Path -LiteralPath 'remake/game').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --resolution 1280x720 --path $project `
  --script res://probes/engine_leader_cleanup_observation.gd -- --authored-package $package
```

Keep APPDATA/LOCALAPPDATA/TEMP, report and logs inside the fresh owning-worktree run directory;
absolute package paths avoid Godot's project-cwd resolution. Reuse the selected installation and
project. Reuse an owned running instance when available; otherwise start it for actual presenter
observation. The probe submits ordinary keys and reads real state, phase tokens, marker visibility,
HP, rewards, ordered events and errors. No screenshots, injected state, synthetic successful
completion, exports or bulk capture. Budget <180s and <5MiB/run; retain launch and completed failures.

`LeaderCleanupTests` covers real attack/return, source-slot/caller variations, simultaneous leader
loss, held completion, unplaced survivors, accounting and retry boundaries. Run affected scene,
physical, sleep, Taros and outcome behavior plus `uv run sf2 verify engine`, `uv run sf2 verify adapter`
and the clean committed planner. Corrections rerun their affected tests; exact-head CI establishes
the final public selection. Prior results and twelve unresolved #1047 comparisons stay preserved.
This is authored engine/presenter acceptance, not full Battle18, private outcome or hardware parity.

## Taros authored observation

Use the ordinary host with `--authored-package remake/content/authored/taros-effectiveness.json`.
The controlled package starts two allies beside a mapped Taros-type guardian. With the default
keyboard, move North, Enter, F (ATTACK), Tab (guardian), Enter. The apprentice's equipped61 is
ineffective because it is not bound Bowie. Repeat those keys with champion for normal damage.
Ordinary scene acknowledgements are Enter; both paths return to usable battle control.

After loading the worktree's `local/private-inputs.ps1`, forcing
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` and building the actual adapter Debug configuration from
`remake/`, the bounded native recipe is:

```powershell
$env:SF2_OBSERVATION_OUTPUT = Join-Path $freshIgnoredOutput 'native.json'
& $env:GODOT_BIN --headless --fixed-fps 60 --path remake/game `
  --script res://probes/engine_taros_effectiveness_observation.gd -- `
  --authored-package remake/content/authored/taros-effectiveness.json
```

Resolve arguments from the repository root (or supply absolute package/project paths). Keep
APPDATA/LOCALAPPDATA/TEMP and the report/logs inside the owning worktree's ignored run directory;
use the existing selected editor/project, with no copied installation or new assets. The probe
submits ordinary keys and reads actual state, result label, delivery events and errors; it has no
state setters, synthetic completions or images. It checks ineffective HP/gold/status and no recoil,
effective damage/recoil, and movement/cancel after scene return. Store semantic boundary samples,
not per-frame world dumps. Budget <180s and <5MiB/run; retain completed failures with their cause.

Run `TarosEffectivenessTests` plus affected `SleepAndFlyingTests`/physical/scene behavior directly,
then the current `uv run sf2 verify engine`, `uv run sf2 verify adapter` and clean-head planner.
The observation proves modern authored physical feedback only. No H4, emulator, full/retired suite,
private Battle18 import, natural original route, all-action immunity, equipment UI or hardware
presentation claim follows. Earlier #1047/#1341/#1343 failures/results remain preserved.

## Scope

Checks that do not require a real foreground window run in the background by default, using the
existing [headless native launch](#authored-field-player-return) for suitable native state/input/layout observations. A Hidden wrapper
does not ensure its game child cannot take focus. Use foreground only for evidence that requires
actual focus, a physical window or rendering; explain the concrete need before launch and group the
minimum necessary observations. Background results do not establish foreground coverage, and must
not bypass production focus/suspension protections.

### Retired bulk frame workflow

The user's 2026-10-04 decision retires the previous whole-route per-frame H4 capture,
processing and audit workflow. Its [historical recipes](evidence/retained-comparisons.md) do not authorize another bulk run,
processing pass or frame-by-frame audit. The old C comparison was terminated by user retirement;
D was already cancelled. Large raw frame captures and reader-derived databases from the named
#534 C/D and #605 allocations are authorized for disposal under the exact inventory in #615.
Cleanup status belongs to that Issue; authorization is not proof that deletion completed.
Small failure/acceptance records, necessary reproduction slices and published compact reports
remain retained. Original ROMs, save states, source and asset libraries are outside that cleanup.

Future work starts from the missing behavior or evidence relation, reuses accepted static rules
and bounded results, and observes necessary logical actions, state changes and actual consumer
boundaries. Use the [existing scale plan](#observation-and-comparison-planning) before acquisition.
Per-frame measurements remain appropriate only for a named timing claim over a bounded interval;
they are not the default format for whole-route state or resource evidence. Retiring this workflow
does not turn unresolved assertions into PASS or erase the completed historical failures.

On this host, future mutable execution uses the selected H-drive checkout and its local outputs,
TEMP, Python/NuGet environments and caches. Exact paths stay in ignored machine configuration.
Existing shared read-only inputs and tool installations are reused. The former C-drive execution
checkout is retained history, not a destination for new work; do not migrate its retired bulk data
into the new checkout merely to continue the old workflow.

Current product acceptance uses the [explicit default-keyboard scope](evidence/retained-comparisons.md#current-keyboard-comparison-scope).
Older four-variant recipes in the retained record are retained history, not authorization to run B/D or require C.

This document owns current remake commands and verification selection. The user's test policy,
recorded in [ADR 0019](../../docs/decisions/0019-state-and-content-driven-remake-engine.md), controls
over older blanket gate and test-preservation recipes:

- Add small unit tests of actual new-engine behavior, with independent expected values and meaningful
  state/configuration variation. A small session using real reducers is a useful unit boundary.
- Reference comparisons, validation, probes, fixture drivers, planners, gates, reports and test helpers
  are themselves verification. Use them directly when needed; do not add tests of those programs.
- Migrate useful behavior assertions or retire obsolete tests with their owning capability. Test
  counts, a replacement for every deletion, and a green old aggregate are not acceptance targets.
- Documentation-only changes use direct link/anchor/fence/table/example, scope and private-boundary
  checks. They do not require a new normal/full, .NET, Godot or H3 run.

The engine unit project covers actual Domain/Application/Content behavior for authored battle
packages, two connected exploration packages and the bounded private entries, with scoped engine/adapter entries and public jobs. The native observation directly drives
the same session with Godot input and reads real state/HUD/nodes; it emits no images. Main-gate owns required-check configuration during integration; a candidate must report its
actual CI outcome and that configuration boundary explicitly.
[ADR 0012](../../docs/decisions/0012-dependency-aware-partitioned-verification.md) continues to own
affected research evidence selection. Shared research changes keep their separate owning requirements.

Godot acceptance observes the actual running instance: Application snapshots, commands, presenter
projections, node geometry/visibility/focus where relevant, and process errors. Screenshots, image
comparison and frame-inspection recipes are prohibited. Existing completed images remain historical
artifacts. A probe that still emits images needs that side effect disabled in an owned change before
a future required run. A remote debug server is not an established dependency; first use an existing
native probe or suitable debug/state interface that answers the concrete question.

Reuse the same verified Godot installation, owning project and running debug instance. A concrete
failure, necessary code/import restart, observed contamination, or test of startup/export/cleanup can
justify a bounded restart or isolated instance; record the reason. A new slice or comparison does not
justify copying the project, extracting another editor, or building a debug framework.

## Observation and Comparison Planning

Before a large or expanded capture/comparison, apply the
[scale-planning owner](../../docs/operations/bounded-inspection-and-review.md#plan-before-scaling).
Declare the claim and minimum sufficient granularity, input/intermediate/output cardinalities,
algorithmic cost, resource budgets, stages, concurrency and publication headroom. Reuse relevant
measurements or a representative small pilot before the full run. Evaluate native collection and
offline comparison separately: acceptable frame time, an indexed database or a bounded writer queue
does not establish acceptable whole-run storage or join cost. Include repeated resource uses,
requirement/candidate products, copies, journals and final report companions in the estimate.

For future probes, start ordinary CPU/memory/progress and coarse state sampling around1Hz, while
retaining necessary semantic and actual-consumption events at their originating boundaries with
occurrence/order identity. Choose finer sampling for a named claim; do not miss a short event by
sampling once per second. A draw callback may itself occur every frame for many resources, so
event-driven collection still needs an explicit total-volume estimate.

Reuse stable descriptors and compact unchanged uses into counts or delimited spans only when the
accepted predicate permits it and completeness, changes and contradictions remain auditable.
Per-frame motion/camera/draw evidence needs a bounded interval and byte budget. Frame-time
percentiles may require per-frame timing statistics, not per-frame full-state serialization.
Sampling, serialization and flush rates are separate: batched writes preserve all required events,
overflow/error behavior and complete terminal records.

Select offline scope before expensive import and derivation, preserving required context and
explicitly marking unselected or missing obligations. Estimate peak logical bytes separately from
physical volume headroom; compression does not remove redundant work. Preserve diagnostics and
replan when observed costs materially exceed the estimate. These design rules do not alter existing
captures, acceptance predicates, running-process control or cleanup authority. A guidance-only change
uses direct document checks and does not trigger native, SDK, normal/full or H3 verification.

## Locked .NET Workflow

Load the existing ignored host/worktree environment before every SDK command, including informational
commands. Select one existing absolute `DOTNET_BIN` and one absolute `DOTNET_CLI_HOME` shared by this
project's worktrees. Force `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false` at the actual child launch.
The ignored machine configuration places complete .NET and Godot installations beneath its selected
shared tool root; `GODOT_BIN` selects the editor and `$godotBinary = $env:GODOT_BIN` supplies the
examples below. Reuse those installations without repeating setup or gates merely to change paths.
The maintained .NET helper preserves valid local NuGet selections, rejects external cache paths and
selects worktree-local TEMP/TMP. See the [shared tool owner](../../docs/operations/local-private-inputs.md)
for the BizHawk runtime-copy exception and preparation/runtime distinction.
The maintained entrypoints reject missing/relative selections. No launch may change registry PATH.

The SDK otherwise adds its tools directory by default; a new CLI home can repeat first-run setup.
`DOTNET_SKIP_FIRST_TIME_EXPERIENCE` does not supply the PATH protection. The
[official variable reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables#dotnet_add_global_tools_to_path)
owns that SDK behavior. Use the SDK pinned by [global.json](../global.json); do not install another one
to configure sharing or make a check pass. Keep NuGet packages/HTTP cache worktree-local, outside the
shared CLI home, and preserve the tracked package versions and locks.

For an affected current product project, run from `remake/` after that setup. For example, when
Domain itself is affected, select its existing project explicitly:

```powershell
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$project = 'src/Sf2.Remake.Domain/Sf2.Remake.Domain.csproj'
& $env:DOTNET_BIN restore $project --locked-mode
& $env:DOTNET_BIN build $project --configuration Release --no-restore
```

From the repository root after loading that environment, the maintained entries are:

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
```

`verify engine` restores in locked mode, builds and tests
`remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj`; it references production
Domain/Application/Content and copies the actual `remake/content/authored/*.json` inputs.
`verify adapter` restores in locked mode and builds `remake/game/Sf2.Remake.Godot.csproj` without tests
or native Godot. These commands launch the selected executable from `remake/`, honoring `global.json`
and the protected environment. Neither needs private inputs. `Sf2.Remake.sln` contains the four
production projects and Engine.Tests; the legacy test projects and reference host were retired at M5. Use formatting only for affected product projects when needed.

## Private world mutation probes

The #1569 agreement uses **Standard** rigor for a bounded verification repair; production admission,
gameplay and the existing High-rigor state/save boundaries are unchanged. Scope is the two obsolete
`PrivateExplorationTests` input-validation nodes and their verification/campaign/audit dispositions.
They are retired under the current test policy; the direct recipe below retains all three intended
rejections, valid admission and strict size rejection. No new world, production API, reader limit,
verification framework or aggregate rerun is needed.

**Confirmed:** on .NET8.0.31 / System.Text.Json8.0.0.0, the registered services world is125,828,417 bytes.
Default `JsonNode.ToJsonString()` produces146,157,627 bytes, including4,064,367 `\u002B` escapes.
`UnsafeRelaxedJsonEscaping` produces125,828,417 bytes with zero escaped plus signs. Both are compact
JSON. This encoder is suitable for JSON-file scratch inputs, not HTML embedding. The unchanged
`PrivateExplorationReader` has pre-read and post-read134,217,728-byte guards. It rejects the default
file with `document-size`; the JSON-only control starts through the actual `GameSession.Start`.

The pre-run estimate used O(n) time/space per serialization, one worker, <=3GiB peak memory,
<=1GiB local scratch and ten minutes of measurement/check execution, with22.8GiB free disk.
Escaping the plus signs alone was estimated to add20,321,835 bytes. Reassess before expanding these
budgets. The initial serial measurement took1.63seconds and peaked at2,002,157,568 bytes; input
size/mtime stayed unchanged. The final five-case direct check took3.07seconds and peaked at
1,731,530,752 bytes. Retained scratch stays below1GiB. No network acquisition or API processing is used.

### Explicit staged-package contract

This recipe supports the registered `world-services-issue1424.json`, its accepted Battle01 placement,
scene, terrain, static, enemies, gold and controlled-start selections, and the eight relative encounter
inputs below. It is not a universal world-package copier or a test requiring only the world file.
`PrivateExplorationReader` reads encounters **before** provenance; their relative paths resolve from
the world file's directory. The old nodes lost that base by writing to arbitrary TEMP. Changing only
their encoder is insufficient. The first measurement without staged dependencies returned
`private-encounter`; preserve `local/issue1569/measurement.log` as that failed attempt.

Load the owning worktree's ignored selections in the same process as each command. Keep shared
inputs read-only and caches/TEMP local under the locked SDK workflow. Prepare a fresh explicit
directory beneath this checkout's `local/`, copying the selected world as `world.json` and these
eight selected inputs with their relative paths unchanged (669,487 dependency bytes):

- `battle02/placement.json`, `battle02/scene.json`, `battle02/terrain.bin`, `battle02-party.json`
- `accepted-inputs/SF2_PRIVATE_STATIC_DATA.json`, `accepted-inputs/SF2_PRIVATE_ENEMY_DATA.json`,
  `accepted-inputs/SF2_PRIVATE_ENEMY_GOLD.json`, `scene02-field-01/battle-scenes.json`

Byte-compare each copy to its selected source. No source directory is writable scratch. The program
takes this staged directory explicitly; it never relies on TEMP containing hidden dependencies.
Its valid JSON-only control runs first and stops the command on any missing/incompatible dependency,
before the negative cases can be counted. It reuses only its own newly named mutation file, retaining
the staged source and compact results. Other worlds/dependency layouts need a separately declared
selection and budget; they are not silently supported by this bounded recipe.

### Direct reproduction

Save the following as `local/issue1569/direct/direct.csproj` and `Program.cs` respectively (ignored
local reproduction files, not another maintained verification project). The code is the direct
production admission check, with no tests of the check itself:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../../remake/src/Sf2.Remake.Content/Sf2.Remake.Content.csproj" />
  </ItemGroup>
</Project>
```

```csharp
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;

// Run from remake/ with an explicitly prepared package under this checkout's local/.
string Input(string key) => Environment.GetEnvironmentVariable("SF2_PRIVATE_" + key)!;
var directory = Path.GetFullPath(args[0]);
var local = Path.GetFullPath("../local") + Path.DirectorySeparatorChar;
if (!directory.StartsWith(local, StringComparison.OrdinalIgnoreCase))
    throw new Exception("Select a worktree-local staged package, never the canonical input directory.");
var world = Path.Combine(directory, "world.json");
var scratch = Path.Combine(directory, "probe-" + Guid.NewGuid().ToString("N") + ".json");
var party = new PrivateBattleScenarioReader(Input("BATTLE01_DATA"), Input("BATTLE01_SCENE"),
    Input("BATTLE01_TERRAIN"), Input("STATIC_DATA"), Input("ENEMY_DATA"), Input("ENEMY_GOLD"), Input("CONTROLLED_START"));
var watch = Stopwatch.StartNew();
foreach (string expected in new[] { "accepted", "world-rom-identity", "raster-identity", "missing-sprite-visual", "document-size" })
{
    Probe(expected);
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
}
Console.WriteLine($"seconds={watch.Elapsed.TotalSeconds:F2}; peakWorkingSetBytes={Process.GetCurrentProcess().PeakWorkingSet64}");

void Probe(string expected)
{
    var node = JsonNode.Parse(File.ReadAllText(world))!;
    if (expected == "world-rom-identity") node["provenance"]!["romSha256"] = new string('0', 64);
    if (expected == "raster-identity") node["world"]!["presentation"]!["maps"]![0]!["atlas"]!["data"] = "AAAA";
    if (expected == "missing-sprite-visual")
    {
        var sprites = node["world"]!["presentation"]!["sprites"]!.AsArray();
        sprites.Remove(sprites.Single(sprite => sprite!["sprite"]!.GetValue<int>() == 209));
    }
    // Only JSON files, never HTML. Default encoding is retained solely as the oversized control.
    var options = expected == "document-size" ? null : new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    File.WriteAllText(scratch, node.ToJsonString(options));
    var start = expected == "world-rom-identity" ? "map3-sarah-start.json" : "map3-opening-start.json";
    var result = GameSession.Start(new PrivateExplorationReader(scratch, Path.GetFullPath("reference/inputs/" + start), party));
    var code = result is SessionStartFailed failed ? failed.Failure.Code : result is SessionStarted ? "accepted" : "unexpected-result";
    Console.WriteLine($"expected={expected}; actual={code}; bytes={new FileInfo(scratch).Length}");
    if (code != expected || result is SessionStartFailed failure && failure.Failure.Kind != SessionFailureKind.ContentError)
        throw new Exception("Admission control failed: " + code);
}
```

From `remake/`, after same-process input/SDK setup, use the explicit staged directory:

```powershell
& $env:DOTNET_BIN restore ../local/issue1569/direct/direct.csproj --locked-mode
& $env:DOTNET_BIN run --project ../local/issue1569/direct/direct.csproj `
  --configuration Release --no-restore -- ../local/issue1569/scratch
```

**Confirmed:** the direct check exits0 with `accepted` (125,828,417 bytes), `world-rom-identity`
(125,828,417), `raster-identity` (125,799,637), `missing-sprite-visual` (125,809,545), and
`document-size` (146,157,627). Each rejection is `ContentError`. No guard, golden or expected code
changed. The intermediate two-node run with staged TEMP passed2/0/0, but is not the final delivery:
the final recipe makes its package dependency explicit and retires those validation unit nodes.

Preserve the completed2069 PASS/25 FAIL/2 SKIP aggregate and later20 PASS/2 FAIL disposition; these
focused controls do not turn historical runs green. Clean-head planner and actual CI belong to the
PR handoff. Original reach/timing, full campaign and Godot/ObjectDB exit behavior remain outside
this admission-only repair.

## Optional Private .NET Checks

Private engine checks use xUnit's explicit private-input selection through `PrivateInputFact`.
`PrivateBattleEncounterTests` and `PrivateBattleInitializationTests` are ordinary facts over in-memory
authored records and always run.

| Engine.Tests family | Required selections |
| --- | --- |
| `PrivateBattleScenarioTests`, `PrivateSourceAiTests`, `PrivateActionBindingTests` | `SF2_PRIVATE_BATTLE01_DATA`, `SF2_PRIVATE_BATTLE01_SCENE`, `SF2_PRIVATE_BATTLE01_TERRAIN`, `SF2_PRIVATE_STATIC_DATA`, `SF2_PRIVATE_ENEMY_DATA`, `SF2_PRIVATE_ENEMY_GOLD`, plus `SF2_PRIVATE_CONTROLLED_START` for the scenario and source-AI checks |
| `PrivateExplorationTests`, `PrivateBattleEntryProgramTests`, `PrivateBattleOutcomeProgramTests` | the six battle inputs above plus `SF2_PRIVATE_EXPLORATION_CONTENT` |

`PrivatePresentationBytesAndRequiredSpriteLinksAreAdmittedBeforeStartup` additionally requires a world
prepared with `--rom-path` and `--presentation-root`; a world prepared without the local asset pack
cannot exercise it. The Battle01 exports and enemy gold come from the pinned H2 rails below; the
canonical import from `uv run sf2 h2 map-import`; the world from the
[exploration preparation](verification/field-programs.md#reproduction).

With no selected inputs, an optional check reports **skipped / no private assertions ran**. Partial
selection fails. For a required check set `SF2_REQUIRE_PRIVATE_TESTS=1` before discovery; missing inputs
then fail. `--filter` does not opt into private execution and other switch values do not require it.
Scope variables to that run and keep logs/output local. Do not interpret public success with private
skips as private acceptance.

[Local Private Inputs](../../docs/operations/local-private-inputs.md) owns registered read-only input
selection. Load it in the same process as any command that needs those inputs. Check the configured
shared ROM and its narrow identity command before reporting it missing; keep configuration, missing
input, identity mismatch, and later upstream/toolchain failure distinct. No copied input is required
solely to repair the appearance of an aggregate result.

## Selected Battle01 Startup Inputs

The production `PrivateBattleEncounterReader` consumes the selected Battle01 data and scene JSON
exports plus compressed terrain. Their exact identities remain in
[battle01-data](../../manifests/extractions/battle01-data.json),
[battle01-scene](../../manifests/extractions/battle01-scene.json), and that Content reader. These
transport identities describe fixed private admission; they are not the general authored-content
schema or a gameplay legality condition. The compressed terrain input is the H1 split output
`disasm/data/battles/entries/battle01/terrain.bin`, so it exists only after a passing H1 rebuild.

Reuse accepted exports read-only after checking identity. If the owning task actually needs to
reproduce them, the existing `scripts/Export-Battle01Data.ps1` and `Export-Battle01Scene.ps1` consume
the pinned upstream and explicit ignored destinations. New maintained tooling still belongs in
`src/sf2tool/`; this is a frozen compatibility route, not permission to add more PowerShell rails.

The [Map 3 implementation/reference record](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/map03-playability-plan.md) retains selected inputs,
exact control histories, source-derived expectations, test method names and completed failures.
Use the named seam needed by a comparison. Its historical instructions to replay prefixes, preserve
every refusal, run full managed suites or inspect frame totals are not current engine acceptance rules.
Fixed traces remain external comparison data; obsolete trace guards and their tests can retire through
the separately owned engine migration. Preserve original H2/H3 goldens and their provenance.

## Repository Planner

On a clean committed head, inspect current selection without executing gates:

```powershell
uv run sf2 verify plan --base origin/main --head HEAD
```

The [planner](../../src/sf2tool/verification_plan.py) automatically selects `engine-unit` and/or
`adapter-build` for ordinary product/test paths, without the old solution or always-run `public-core`.
Non-research documentation and retired engine-test paths add no execution. Engine test changes select
unit tests; ordinary game changes select ordinary compilation. Controlled inputs under
`remake/reference/inputs/` select `engine-unit`, because Engine.Tests copies them and actual private
engine facts consume them. Shared product/build inputs select both actual downstream builds.

Shared CLI/harness/planner changes retain conservative research selection by default. For a declared
engine-only wiring change in those shared modules,
inspect the actual diff and use:

```powershell
uv run sf2 verify plan --scope engine --base origin/main --head HEAD
```

That explicit scope adds `research-public` direct lint/traceability/index checks for the wiring and
rejects research artifacts or other shared inputs. Path admission cannot detect a semantic research
change inside an allowed module: such a change must retain its research plan/dependency. Unknown
research paths keep conservative fanout. Retired verification tests do not fall back to the old suite.

The existing normal `uv run sf2 verify` command still performs its public stages followed by selected
private identity/provenance stages. It remains the research lane's normal route under its owner.
It is not a new-engine or documentation-only default. Preserve completed stage results and failures;
do not rerun it just to replace a known upstream-availability failure with green.

`uv run sf2 verify --full` remains exceptional for an applicable research milestone, materially shared
evidence-harness change, release boundary, or explicit full-parity request. An ordinary engine slice
does not inherit it. Test/gate wall time and earlier SHA changes do not by themselves invalidate results.

## Retired Official Godot Gate

The former `sf2tool.remake_godot` import/export gate verified only the explicit public-synthetic
reference host, not ordinary game packaging, and is retired together with that profile. Its bounded
native process runner now lives in `sf2tool.bounded_process` for the asset candidate builder. No
maintained gate verifies ordinary package/export contents; claim none without an actually executed,
separately owned export check. Repeated behavior observations use the existing
installation/project/instance.

## GitHub Public

Current [public-checks.yml](../../.github/workflows/public-checks.yml) remains triggered on every pull
request and main push. Its Windows jobs are:

| Job | Current execution |
| --- | --- |
| `scope` | Compare the actual event Git range; fail on a missing/invalid range or failed comparison. Emit engine, adapter, research and H4 affected-path booleans. |
| `engine-unit` | Locked restore/build/test of Engine.Tests and its actual production dependencies, using the pinned .NET SDK/runtime. No Python, native Godot, private input or legacy test project. |
| `adapter-build` | Locked restore/build of the ordinary game C# project. No Godot tests or native editor launch. |
| `research-public` | Locked Python/uv dependencies, Ruff, direct design-contract traceability and research-index checks. No engine or verification-tool pytest families. |
| `h4-comparison-build` | Build the standalone C# comparison executable through the controlled Python launcher with pinned SDK/runtime. No native game or original input. |

Shared product/build inputs select engine and adapter; engine tests select engine; `remake/game/`
selects the ordinary adapter. `remake/reference/inputs/` selects engine for its Engine.Tests consumers. Research/contracts/source/fixtures/manifests/schema inputs select
research. Workflow and shared CLI/harness/planner changes select
engine/adapter/research; the workflow also selects H4 for its explicit comparison/build inputs.
Changes under `tools/h4-comparison/` or to the named H4 bridge/comparator/environment/planner/workflow
paths select `h4-comparison-build`. Non-research documentation and legacy remake test edits select no product jobs. Other non-remake inputs conservatively select research. The full
predicate lives in the workflow; add a genuinely consumed external unit input when that dependency
is introduced. M1's consumed authored JSON lives under `remake/content/`, which conservatively selects all product/host builds.

Main-gate owns required-check configuration. The reference host and its build job are retired;
do not request that obsolete context as a current gate.
Verify the actual candidate CI/check state at integration; changing configuration remains main-gate
authority. Path-irrelevant jobs report skipped; applicable jobs need a
real successful result. Review the diff and first applicable CI outcome directly, without tests of
job selection, workflow text or the migration. Research/private protections remain independently owned.

## Process, Path, and Artifact Safety

### Worktree Environment and Run Evidence

Reuse the dedicated isolated worktree after its previous topic is merged, tracked state is clean and
owned processes are settled. Start the next topic at accepted main in that worktree. Separate writers
and a specifically required reproduction environment justify another worktree; a slice number does not.

| State | Lifetime |
| --- | --- |
| Python environment and uv cache | Reuse worktree-local `UV_PROJECT_ENVIRONMENT` and `UV_CACHE_DIR`; sync the lock only when needed. |
| NuGet packages and HTTP cache | Reuse worktree-local selections; only SDK-owned CLI state is shared. |
| .NET CLI | Existing absolute `DOTNET_BIN`, shared absolute `DOTNET_CLI_HOME`, forced PATH opt-out at every controlled launch. |
| Godot | Existing verified installation/project/debug instance unless a concrete exception applies. |
| Outputs | Separate logs, receipts and failure evidence for actual new runs; reuse build/import state unless that state is the reason for isolation. |

Keep configuration in the existing ignored worktree environment script. Historical per-slice
launchers are evidence, not current environment setup. No user/machine PATH edits, populated-cache
copies, or environment installation merely because a topic changes. Registered immutable inputs keep
their read-only selection rules. Writable emulator/import/export state stays with its owner.

Use argument lists, bounded timeouts and owned process-tree cleanup. Inspect actual errors as well as
exit status. A zero-exit process or source-only reading does not prove a running adapter observation.
Preserve the first failure and its node/command IDs, logs and process-completion state; after a
correction run only affected engine behavior or directly invalidated verification. No full rerun
solely to replace a completed red aggregate, and no timeout increase as the first fix.

Cleanup is separately authorized work: inspect live references/processes and exact paths, record the
delete/retain list, and preserve private inputs, completed evidence and required reproduction state.
Do not replay earlier cleanup scripts. Keep private/generated inputs, tools, `.godot/`, `bin/`, `obj/`,
exports and receipts out of Git; inspect staged paths and any actually produced export boundary.

## Proportional Gate Guide

| Change | Required kind of evidence |
| --- | --- |
| New-engine rule/state/content behavior | Small meaningful engine unit tests; affected product build; direct reference comparison only for its affected claim |
| Godot input/presentation | Affected adapter build and needed actual-instance observation; no new probe/helper tests or screenshots |
| Verification/probe/planner/report code | Execute and inspect the needed verification directly; add no tests of verification infrastructure |
| Documentation/instructions | Direct document and scope checks, committed planner inspection, honest existing CI outcome |
| Original research or genuinely shared evidence input | Owning research requirements and affected dependencies; keep original evidence/provenance intact |

These scope rules do not waive accepted 8C/H4 evidence. A migration handoff names any pending remote
required-check change and unsupported product capability. Use
[Bounded Inspection and Review](../../docs/operations/bounded-inspection-and-review.md) for exact
identity, semantic review and completed-result handoff.
