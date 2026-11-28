# Presentation Assets and Private Preparation

Content owns external asset identity and prepared resource admission. The ordinary runtime consumes
prepared private worlds/scene documents; it does not reopen a ROM or reconstruct a legacy Godot
asset catalog. [Profiles and trust](profiles-and-trust.md) owns runtime admission and redistribution.
[Godot](../godot/presentation.md) owns disposable rendering, not source provenance.

The [asset inventory](asset-inventory.md) routes each resource family from its source and prepared
version to the registered selection, actual consumer and bounded evidence. Use the
[registered selections](asset-inventory.md#registered-selections) for current package identity.

## Controlled opening scene package (#2250)

The successor ordinary-start selection is documented in [four-scene startup](#four-scene-startup-package-2252).
The commands and retained results below continue to describe the earlier controlled package.

The finite private selection is `derived/issue2250/opening-scenes-v1` beneath the configured
`SF2_SHARED_INPUT_ROOT`. It is separate from the active campaign package. It contains the two scene
starts, controlled party input, six opening definition inputs, scene-only world, matching Chinese
catalog, command9 WAV and preparation receipt. No ROM, upstream checkout, full campaign world,
producer scratch or entry/title assets are needed by its runtime route. It does not select ordinary
startup. [Behavior and ownership](../application/opening-session-lifecycle.md#bounded-scene-implementation-2250)

The producer reads the registered USA ROM, pinned c834c652 source, registered canonical map export,
accepted #2242 package and the sole full-text authority. The latter remains read-only. Configure
`$pinnedSource` and `$fullTextRoot` from their existing private-input/localization owners, then run
from the explicit checkout root after loading `local/private-inputs.ps1`:

```powershell
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2242/pre-voyage-brgn-v1'
$destination = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2250/opening-scenes-v1'
uv run python -m sf2tool.remake_opening_scene `
    --base $base `
    --canonical (Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/canonical-map-import.json') `
    --upstream $pinnedSource `
    --rom (Join-Path $env:SF2_SHARED_INPUT_ROOT 'roms/sf2-us.bin') `
    --corpus $fullTextRoot --party remake/reference/inputs/map3-opening-party.json `
    --output $destination
```

The output must be fresh; use another ignored candidate path to reproduce without overwriting the
published package. The final content is about23.17MiB, including a4.43MiB world and18.34MiB existing
intro WAV. Measured production took0.451s with284.58MiB peak working set. The native pilot reached
about583MiB peak working set. These fit the selected20–32MiB output budget and a2GiB working-memory
allowance; all failed candidates remain review scratch. Only two40KiB terrain index planes are
added. Forty-two prepared sprite identities include the nine scene-used sprites and the existing
population's required fallback/ally references. There is no frame-by-frame asset production.

Run the actual controlled host with the published package. `$repo` is the explicit isolated checkout;
`$run` is a fresh child of its ignored `local/issue2250/`; `$disposableSaveRoot` is the separately
permitted external #2250 disposable namespace. Set `$scene` to1 or2 and `$locale` to`en` or`zh-CN`.

```powershell
. (Join-Path $repo 'local/private-inputs.ps1')
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$pack = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2250/opening-scenes-v1'
foreach ($name in @('SF2_PRIVATE_BATTLE01_DATA','SF2_PRIVATE_BATTLE01_SCENE',
    'SF2_PRIVATE_BATTLE01_TERRAIN','SF2_PRIVATE_STATIC_DATA','SF2_PRIVATE_ENEMY_DATA',
    'SF2_PRIVATE_ENEMY_GOLD')) {
    [Environment]::SetEnvironmentVariable($name, (Join-Path $pack "opening-inputs/$name.json"), 'Process')
}
$env:SF2_PRIVATE_CONTROLLED_START = Join-Path $pack 'party.json'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $pack 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = $null
$env:SF2_DISPLAY_CATALOG = Join-Path $pack 'world.zh-CN.json'
$env:SF2_SAVE_ROOT = $disposableSaveRoot
$env:SF2_SAVE_NAMESPACE = 'opening-observation'
$env:SF2_OPENING_SCENE = [string]$scene
$env:SF2_OPENING_LOCALE = $locale
New-Item -ItemType Directory -Path $run | Out-Null
$env:SF2_OPENING_OUTPUT = Join-Path $run 'observation.json'
$settings = Join-Path $run 'settings.json'
@{formatVersion=1;language=$locale;reducedFlash=$reducedFlash;textMode='instant'} |
    ConvertTo-Json | Set-Content -LiteralPath $settings -Encoding utf8
& $env:DOTNET_BIN build (Join-Path $repo 'remake/game/Sf2.Remake.Godot.csproj') --no-restore
& $env:GODOT_BIN --path (Join-Path $repo 'remake/game') --max-fps 60 `
    --resolution 1920x1080 --script res://probes/engine_opening_scene_observation.gd -- `
    --private-exploration-start (Join-Path $pack "scene-$scene.json") --input-settings $settings
```

The observer uses normal clock progression and no Confirm, completion injection or screenshots.
Require `passed:true`, exit0, no script/C#/draw errors, all16 shrine or13 castle text IDs, selected
locale without fallback, an actually drawn183/187 effect resource, actual indexed texture color
proofs and unchanged party/gold. Castle requires six ten-service replacement/restore pairs; Reduced
Flash must retain them. The terminal has no temporary bank and remains ineligible for field input
or saves. Inspect the external disposable namespace for zero save writes. Remove producer candidates
from their original paths before repeating both published starts; retained failed logs are not a
runtime dependency. Native observations do not establish original natural timing or human quality.

### Scene validation and retained failures

**Confirmed:** the complete shrine finishes at logical tick5070 on the final controlled start,
with16 source IDs in each locale. The castle prefix finishes at3974 with13 IDs and six restored
pulses. The native English castle and Chinese Reduced Flash runs take about70.5s; the Chinese
shrine about89.5s. These are remake observations, not original-game timing. The final shared-package
runs use the same executable route after all producer candidates are moved off their original
paths. Both report actual183/187 draws, palette texture proofs, unchanged party/gold and no native
errors. No file is created in the external disposable save namespace. The actual reader admits
81 messages and two display names; only the reached29 messages claim native consumer evidence.

`AutomaticTextDelayTests` covers119 normal services, mouth-control independence and matching
localization topology. `ScenePaletteTests` covers independent same-RGB slots, ten-service restore,
source fade periods/masks, stale/early completion, transient-state exclusion from ordinary control
and Battle Entry Skip retaining logical work plus its consequential flag. The selected existing
entry/scene regression set passed94 cases with five private-input skips; the final palette file
passed six cases; the final scene-load/scene-lifecycle selection passed24 cases. Release adapter and Debug native builds pass with zero warnings/errors. A skipped
private-input case is not a scene observation.

Retain the completed discovery results with the corresponding corrections:

| Failure boundary | Correction and retained evidence |
| --- | --- |
| Scene load after a visible-base half-fade | A focused valid-state case exposed stale full-fade visibility after the scene bank reached black. Scene binding now uses that bank's actual black state, clears it and establishes the next black display; ordinary warp admission remains unchanged. The failed case and24-case corrected selection are retained. |
| First three palette behavior cases rejected program admission | Add the finite terminal to Content's valid endings; the same three cases then pass. |
| Producer indexed tile decoding | Consume the existing decoder's flat64 slots; the failed first pilot and subsequent packages remain separate. |
| Native start admission and first draw | Preserve numeric gameplay revision, include the population's fallback sprite references, bind the initial actor and scene music. Failed `native-1` through`native-6` remain in ignored review output. |
| Native first portrait | Use the existing map-initialization continuation for the scene caller, and bind required window/speech SFX. The `native-7` context failure and `native-8` audio failure remain retained. |
| First Chinese projection | Supply the required private source identity in the catalog envelope. `shrine-zh-1` remains failed; the corrected complete run resolves all16 messages in Chinese. |
| Added Skip behavior case setup | Correct the authored opcode and declared battle unlock flags; retain both failed focused runs. The corrected case passes without production weakening. |

The committed dependency plan uses `uv run sf2 verify plan --scope engine --base origin/main --head HEAD`:
the sole shared registry change admits this producer and does not change research semantics. Its six
partitions are `private-admission-build`, `live-inspection-build`, `h4-comparison-build`, `engine-unit`,
`adapter-build` and `research-public`, with no unclassified paths. The implementation commit's local
engine gate passes 3,290 cases with 50 private-input skips. All four builds pass with zero warnings or
errors; Ruff, design-contract traceability and research-index checks pass. This selection does not
require the normal research aggregate or a full private profile. Public checks on implementation
commit `f2a6778b0934b0057edaf414660b754daf33a58e` completed successfully in
[run 45547177318](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/45547177318):
scope plus all six selected jobs passed. Draft PR #2251 records the final exact-head result after this
documentation-only receipt. Private observations stay local; the public owner records commands and
bounded conclusions. Human appearance,
readability preference, original natural reach/timing and the four-scene ordinary startup remain
**Unknown** or explicitly not implemented.

The exterior-color correction keeps the scene's outer mask opaque black independently of its
palette backdrop; ordinary exploration and dialogue/menu styling are unchanged. The native observer
checks `cameraProjection.exteriorColor == "000000ff"` on every observed draw and requires at least one
such proof. The focused corrected shrine run recorded 704 matching draws, the first automatic text
and effect 183 before losing OS focus at 14.917s; it is a background-projection observation, not a
completed scene rerun. Preserve that interrupted result and the independent review's earlier
37.340s focus-loss result as environment interruptions, not product logic failures. Release adapter
and Debug builds pass without warnings/errors. Independent review retains the two complete published
native routes after this correction; the earlier completed scene results above remain separately
identified.

## Four-scene startup package (#2252)

The four-role [runtime selection](metadata/selection.json) selects the HD opening at
`media/opening/hd-v1/opening/opening.json`; select it with the accepted full-game
world, Witch and catalogs without replacing those packages.
[HD production, palette banks and camera observations](hd-rollout.md#opening-terrain-and-palette-banks-2324)
extend the lifecycle selection below. Human visual acceptance remains **Unknown**.

For the current four-scene indexed production input, select `media/opening/indexed-v1`.
To regenerate the opening by itself, load the private configuration and choose a fresh output:

```powershell
. ./local/private-inputs.ps1
$shared = $env:SF2_SHARED_INPUT_ROOT
$pinnedSource = Join-Path (Split-Path -Parent $shared) 'upstream/SF2DISASM'
$fresh = Join-Path $PWD ('local/indexed-opening-' + [guid]::NewGuid().ToString('N'))
uv run --locked python -m sf2tool.remake_opening_scene --complete `
    --base (Join-Path $shared 'media/gameplay/atlas-v1') `
    --canonical (Join-Path $shared 'derived/canonical-map-import.json') `
    --upstream $pinnedSource --rom (Join-Path $shared 'roms/sf2-us.bin') `
    --corpus (Join-Path $shared 'sources/localization/v1/full-text') `
    --party remake/reference/inputs/map3-opening-party.json --output $fresh
```

All fifteen names matched the registered input. Thirteen files were byte-identical. Only the
world source provenance and the size/timing report differed. Use the registered indexed root
for exact-version HD production.

The original lifecycle observation used a complete campaign successor. The following configuration
and commands record that historical selection. It reuses the accepted #2242 campaign, #2240 world catalog and registered Witch
catalog; its `opening/` subdirectory independently contains both scene maps, all four scripts,
all81 English/Chinese opening rows, default names, selected actors/portraits, finite audio and inputs.
Startup reads only `opening/opening.json` and its bounded dependencies. The Witch's subsequent
compatibility check admits campaign content only if it has a candidate primary to validate.
No ROM/upstream/scratch is a runtime dependency. Private use does not authorize redistribution.

From the owning checkout with private configuration loaded, prepare a **fresh** destination using
the registered pinned source and full-text corpus roots. `$witchCatalog` is the existing registered
Witch overlay; it is copied without translation changes:

```powershell
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2242/pre-voyage-brgn-v1'
$pack = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2252/pre-voyage-opening-v1'
uv run python -m sf2tool.remake_opening_scene --complete `
    --base $base --canonical (Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/canonical-map-import.json') `
    --upstream $pinnedSource --rom (Join-Path $env:SF2_SHARED_INPUT_ROOT 'roms/sf2-us.bin') `
    --corpus $fullTextRoot --party remake/reference/inputs/map3-opening-party.json `
    --campaign-catalogs (Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2240/catalogs-v1') `
    --entry-catalog $witchCatalog --output $pack
```

The opening payload is24,903,861 bytes (23.75MiB); the closed campaign plus opening/catalogs is
386,091,826 bytes (368.21MiB, before the small final preparation receipt). The measured selected
opening preparation took0.433s. This fits the planned24–32MiB opening and about370MiB successor.
The full campaign copy is the requested reusable successor, not the startup working set.

Configure the complete selection together. `$disposableSaveRoot` is an explicitly permitted
external test root; choose a fresh namespace for a New Game control:

```powershell
foreach ($name in @('SF2_PRIVATE_BATTLE01_DATA','SF2_PRIVATE_BATTLE01_SCENE',
    'SF2_PRIVATE_BATTLE01_TERRAIN','SF2_PRIVATE_STATIC_DATA','SF2_PRIVATE_ENEMY_DATA',
    'SF2_PRIVATE_ENEMY_GOLD')) {
    [Environment]::SetEnvironmentVariable($name, (Join-Path $pack "opening-inputs/$name.json"), 'Process')
}
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $pack 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $pack 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $pack 'entry/entry.json'
$env:SF2_PRIVATE_OPENING_CONTENT = Join-Path $pack 'opening/opening.json'
$env:SF2_DISPLAY_CATALOG = Join-Path $pack 'catalogs/world.zh-CN.json'
$env:SF2_ENTRY_DISPLAY_CATALOG = Join-Path $pack 'catalogs/entry.zh-CN.json'
$env:SF2_SAVE_ROOT = $disposableSaveRoot
$env:SF2_SAVE_NAMESPACE = 'opening-review'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:GODOT_BIN --path (Join-Path $repo 'remake/game')
```

Unselecting `SF2_PRIVATE_OPENING_CONTENT` preserves direct title startup. A selected malformed or
unavailable opening produces a visible bounded diagnostic and the same title when entry assets
are valid. Diagnostic starts, preview and explicit seeded Continue bypass the sequence.

HD uses the existing per-asset preference. Map42 retains its accepted external HD terrain bank;
uniform scene fades transform those actual textures. The #2324 independent successor adds Map65's
HD terrain and finite storm/Reduced Flash palette banks. Absent optional color banks retain indexed
Original fallback. Compatible HD actor directions and portraits remain selected through fades and
do not inherit terrain-only fallback;183/187 now have complete HD direction strips. The six-layer title/Witch HD bank
retains its coherent entry selection. Original source palette accuracy, HD visual approval and
original hardware timing are separate from actual resource consumption.

The four scripts actually require sprites14/59/159/173/183/187/202/203/205/206. All have admitted
HD directions in the successor, including Slade's special pose59. Their non-null
portrait bindings are5/32/45 and have HD rasters. The bounded selection also retains the population
reader's required fallback/ally sprite bindings; these do not imply that every retained actor or
portrait appears in the opening. #2324 derives only the missing terrain/effects from published
private inputs with the existing deterministic filter; it adds no generated-art direction.

Native observation uses `engine_opening_scene_observation.gd` with `SF2_OPENING_SCENE=all`,
`SF2_OPENING_LOCALE=en` or`zh-CN`, and a fresh `SF2_OPENING_OUTPUT`. It uses ordinary startup,
automatic text and state/resource/audio readback; no screenshots or per-frame output. Set
`SF2_OPENING_NEW_GAME=1` only with disposable slots to follow natural title completion through
ordinary slot/name/confirmation using controlled comparison seeds. The short
`engine_opening_startup_observation.gd` uses `SF2_OPENING_CONTROL` values `load`, `delay`, `motion`,
`pointer`, `focus`, `back`, `absent`, `bad`, `empty`, `newgame`, `continue` or`seeded`. The absent control
unselects the opening; bad/empty select a fresh malformed/zero-byte manifest. Seeded uses an explicit
`--load-save` copy outside every player namespace, as required by the existing seed-path guard.
Late completion and controlled seeds use the bounded
`OpeningStartupControl` probe; production has no seed override. Focus recovery makes at most three
requests and waits at most30s per pause. An unrecovered loss is an environment interruption.

**Confirmed — worker observation:** the ordinary published selection completed all four scenes and
all81 messages, reached a usable title with command28 and then completed the ordinary New Game
flow in471,819ms. The run had zero focus pauses, no engine/script errors or shutdown warnings, and
no producer scratch at the original preparation paths. The opening owner was absent at title,
with no journey allocated. It retained party/gold throughout the intro. Native opening working-set
peak observed during playback was632.45MiB, within the2GiB budget; this is not a campaign peak claim.
The retained receipt is `local/issue2252/native-8/observation.json` in the owning review checkout.

Actual projected HD actor sprites0/14/59/159/173/202/203/205/206 used96-pixel rasters; Map42 supplied
11,055 HD terrain draw proofs. The two rendered portraits5/32 used192×256 textures; portrait45 is
available in the package but was not observed mounted. All six title/Witch textures were1024×896.
The observation retained459 indexed palette proofs and26,611 opaque-black exterior proofs.
The Chinese Original control consumed24-pixel actor/terrain resources under the active palette.
The bounded complete fourth-scene Chinese/Original/Reduced Flash control passed all34 messages
4233–4266 and terminal tick10,986. This control is distinct from the full native ordinary launch.

Short controls passed loading Skip, actual D2, entity motion, palette/pointer Skip, held-key/release,
late detached load and duplicate completion, title music ownership and Witch Back without replay.
The real native focus control paused scene time/music and resumed the same owner in9,748ms.
Absent, malformed and zero-byte selected opening controls reached the usable title as specified.
Explicit seeded Continue bypassed opening. Headless controls retain the existing2/6 ObjectDB
shutdown warnings; their object ownership remains **Unknown**. The native focus and complete runs
had none; do not generalize that to every shutdown or device.

Natural-finish and Skip New Game used identical controlled seeds. Their complete decoded save
`data` objects were equal, both envelopes had sequence1, and each fresh namespace contained only
`slot1.json` plus its lock, with no backup. Map, wait, complete party, gold, flags, seeds, actors,
custody, travel and text/music state at first control also matched. Ordinary Continue restored those
same facts while the8,142-byte existing slot remained byte-for-byte unchanged. All saves were in
the explicitly disposable issue namespace; real player namespaces and backups were untouched.

Retained failures include the initial missing pose59 binding, observer cursor/type mistakes,
bounded foreground interruptions and the fourth-scene `full-fade-state` failure. That last failure
was the existing FlashWhite admission guard rejecting a scene bank immediately before full fade;
native-7 had zero focus loss and is retained as a completed product failure. The producer now includes explicit script sprite
changes. Draw derives its palette from the consumed snapshot across same-frame map transfers.
Script full fades service existing scene terrain/actor banks; ordinary warp validation remains
unchanged. The two added behavior cases initially assumed early FullFade acknowledgement must
reject; the existing contract allows independent actual/logical completion. The corrected cases
check stale tokens, final banks, restoration and save exclusion. All8 focused palette cases pass.
The earlier engine run passed3,290 with50 private skips; that result predates the full-fade extension.
The final local engine execution completed with3,301 passes,6 skips and35 failures after the ordinary
play configuration enabled private tests. Those selected tests require their separate controlled
starts/sleep fixture and older bounded worlds (including7 encounters/4 roster entries); the closed
campaign is not their fixture. The exact nodes and completed log remain in
`local/issue2252/failed-private-nodes.txt` and `engine-final.log`. No expectations were changed.
A narrow rerun of those35 nodes with private selections absent skipped all35, as in public CI;
it is not a private acceptance pass. Affected scene palette/full-fade/Battle Entry Skip behavior
passed20 with1 private skip. Release adapter and admission/inspection builds, Ruff, design contracts
and research-index checks passed. The first focus probe used an embedded subwindow and therefore
failed to produce OS focus loss; the native-window correction passed. The first seeded control
selected a file inside a player namespace and the existing guard rejected it; a byte-identical
ignored copy outside that namespace restored successfully. Both failed controls remain retained.

On clean implementation commit `a6418e838bc954830bc1303c37b002fa7c613b27`,
`uv run sf2 verify plan --scope engine --base origin/main --head HEAD` selected
`private-admission-build`, `live-inspection-build`, `engine-unit`, `adapter-build` and
`research-public`, with no unclassified paths. This producer diff is engine-only; it changes no
research extraction or evidence semantics. [Public checks run45555538206](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/45555538206)
passed scope, engine-unit (**3,292 passed /50 private skipped /0 failed**), adapter-build,
private-admission-build and research-public. Remote live-inspection-build and h4-comparison-build
were skipped by the actual workflow. The planner's extra live-inspection build passed locally;
H4 was not selected by the plan. This receipt update changes documentation only.
Independent main-gate acceptance, physical-device coverage, hardware timing and human visual/audio
approval remain separate.

**Independent review and Back synchronization:** at frozen `f3d30f71`, main-gate's loading and
Chinese pointer controls passed. Loading reported4 ObjectDB shutdown warnings; pointer reported
none. Main-gate independently decoded both natural/Skip save payloads: complete `data` objects
matched, both sequences were1, and each directory contained only `slot1.json` and `slot1.lock`.
It also checked `native-8`'s four scene labels,81 automatic messages, title/New Game tail and HD
resource proofs without replaying the full sequence. Its rebuilt Debug
`ScenePaletteTests|FullFadeTests|BattleEntrySkipTests` selection passed18
with1 private skip; the earlier Release `--no-build` result is not its acceptance evidence.
The native `main-gate-focus` run completed in9,829ms with only “Witch Back returns without replay”
failing. Real focus recovery, Skip/title and fresh Witch menu had passed, with no engine errors.
Its retained `witch` receipt shows `stage=menu`, `latched=true`: the observer sent Back without
establishing that the prior Start release had reached the entry's normal neutral-input boundary.
**Inferred:** the existing latch guard suppressed that Back; the old probe did not retain a
post-Back input receipt. The release-edge control below directly confirms the pending-release state.

The probe now records physical/mapped held state, host release barrier, entry latch/stage and focus
before Back, then waits at most2s for their actual neutral state. It sends one ordinary Back and
requires the returned title and inactive opening owner; it never retries Back or relaxes that
assertion. The focused `back` control releases held Start immediately before this check. Native
`witch-back-release-edge` passed in1,814ms: Start/held/latch were initially true, cleared after35ms,
and the single fresh Back returned to title. The corrected native focus run passed in9,989ms.
Both logs had no errors or shutdown warnings. This is a probe input-synchronization correction;
production code, private package and the completed full-play observation are unchanged. No
eight-minute replay was performed. The original failed receipt remains available beside these
short controls under `local/issue2252/`.

For the ordinary opening skip, title music, Witch greeting and menu and Back sequence, use the
[network title round-trip client](../verification/debug-port.md#launch-and-operate) with a disposable
namespace. It works with the exported game as well as the development entry. To reproduce the
specialized retired-owner callback and native focus controls after the ordinary selection above:
set `SF2_OPENING_CONTROL=back` (or`focus`) and launch
`& $env:GODOT_BIN --path remake/game --script res://probes/engine_opening_startup_observation.gd`.
The owning checkout's prepared equivalent is
`./local/issue2252/run.ps1 -Run <fresh-name> -Control back -Namespace main-gate-controls`.

## Public localization pilot assets

The game bundles one unmodified OFL 1.1 Noto Sans CJK SC Regular font and its
[exact upstream pin/notices](../../game/fonts/localization/README.md). The adjacent `.otf.import`
records `res://` paths and disables system fallback; generated `.godot` font data stays local.
The original authored localization pilot contains only simple solid diagnostic rasters, a generated
20ms tone and original prose/translation. Its minimal glyph metrics express authored logical work,
not original font evidence. No source-derived script, translated private text or extracted asset is
publicized. Package display overlays are optional separate inputs; `SF2_DISPLAY_CATALOG` is explicit
and absolute, with existing private repository/commit/ROM identity checked by Content when applicable.
See [admission/fallback](../application/localization-design.md#implemented-m1-boundary).

## Master and Runtime Buckets

Original material is the canonical private source. Newly authored raster masters and reviewed
derivatives remain distinct from ignored candidates. Existing tooling supports the versioned 2x/4x
asset buckets; this is not proof that the ordinary host implements the old bucket-selection UI.

### Source authority

Private source art/audio and accepted asset Git objects own material identity. The
[asset pipeline workflow](asset-pipeline-workflow.md#tracked-metadata-and-private-records)
assigns current build selection and queryable semantic bindings to main Git. Private manifests
retain input and payload identity. The migration pilot will connect these records. A generated cache,
preview, screenshot or mutable checkout name does not replace that authority. Source color changes,
smoothing and derivative policy require explicit reviewed provenance.

### Bucket selection

The earlier high-DPI product proposal uses 4x new-raster masters, deterministic 2x/4x buckets and one
resident selection. Its retired catalog/window consumers are historical. Keep bucket policies and
derivation identity explicit when preparing material; ordinary runtime resource compatibility must
be checked at the actual prepared-world/scene consumer. [Presentation policy](../godot/presentation.md#presentation-policy-and-current-layout)
records the current layout and deferred product UI boundary.

The [HD art pilot](hd-art-pilot.md) compares one authored reference at inventory size and proposes a first switchable field item.

## Local Asset Repository

`md-sf2-remake-assets` is the actual local-only product-art and audio repository. It is a sibling of
the main code repository and of the separate graphics R&D repository; neither sibling is a nested
dependency or a durable substitute for another.

Its bootstrap identity is recorded as an origin point, not as a floating current version:

| Property | Bootstrap value |
| --- | --- |
| branch | `main` |
| root commit | `c82aa7e353e808e4ef12117f247c5c7065839801` |
| root tree | `96d499cabc0d3310b12168cc9c8d392b1862c1b6` |
| remote policy | no remote; tracked pre-push hook rejects pushes |
| binary storage | ordinary local Git history, not Git LFS |

The current tracked layout in that independent repository is:

| Path | Authority |
| --- | --- |
| `source/` | original art/audio and other input material in its admitted source form |
| `masters/` | editable HUD SVG and other authored masters |
| `runtime/` | reviewed runtime-ready 2x/4x textures, UI, fonts, music, and sound assets |
| `manifests/` | local identity, semantic mapping, derivation, import policy, and code-compatibility records |

The [directory responsibilities](asset-pipeline-workflow.md#directory-responsibilities)
cover relocation of the real independent Git root, the canonical media entity and the shared
reader entry. Existing private manifest mappings describe the revisions that produced them.

The repository ignores `cache/`, `scratch/`, `previews/`, temporary output, and Godot import state.
Reviewed binary art and audio are intentionally trackable. A local build or verification receipt
records the exact asset-repository commit it consumed; it never records the machine's absolute path.

## Local Product Asset Pack

The maintained `sf2tool.remake_assets` checkout/export preflight consumes an explicitly selected
local asset repository or pack and verifies the exact manifest/payload relationships. Selected asset
commit, semantic IDs, source capture/derivation and payload identities remain bound. Presence of a
directory is not profile selection; private inputs cannot fall back to synthetic product material.

Offline `sf2tool.remake_exploration_content` and `sf2tool.remake_battle_scene_content` prepare the
world/scene resource inputs consumed by current Content readers. Runtime reads those selected inputs
once, validates profiles, references, raster/audio shape and digest and admits immutable resources.
Paths stay at the preparation/Content boundary; session observations and ordinary diagnostics remain
path-free. Godot renders admitted bytes and resource selections without authenticating repositories.

The former direct `private-local` Godot catalog/mount route is retired. Its prospective error names
and mount API are not the current `SessionFailure` API. Missing or inconsistent current input returns
typed ContentError/UnsupportedCapability; actual delivery failure is AdapterError and cannot release
a pending engine completion as success. See [private admission](profiles-and-trust.md#private-admission-layers).

### Reuse a closed private package

The retail preparation's private Git record is `manifests/retail-original-preparation.json` in the
local-only asset repository. It identifies the stable package relative to the registered shared
data root, the exact asset manifest version used by the inventory, and the retained observation
selections. The payload remains under shared `derived/`, outside all replaceable worktrees.
The retail preparation baseline `issue1966-original` contains the matching `world.json`, `main-scenes.json`,
entry media and opening selectors. Encounter inputs and external WAV files keep their relative
names. Selecting or copying this package requires no ROM or H1 tool. Keep the world and scene
selection together. Save slots and locale catalogs use their existing independent selections.

Run PowerShell 7 in the owning checkout. Bootstrap its private configuration as needed using
[Local Private Inputs](../../../docs/operations/local-private-inputs.md#fresh-worktree-setup).
The original publication used the historical handoff selections below. To reproduce its copy,
select a fresh ignored checkout destination. Current mounting reads the published base under
`media/retail-preparation/v1/issue1966-original`.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
$handoffs = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs'
$base = Join-Path $handoffs 'issue1966/content/package'
$opening = Join-Path $handoffs 'issue1857'
$stable = Join-Path (Get-Location).Path ('local/retail-base-' + [guid]::NewGuid().ToString('N'))
$packageArgs = @('--world', "$base/world.json", '--scene', "$base/main-scenes.json",
    '--entry', "$opening/entry/entry.json", '--opening-inputs', "$opening/inputs",
    '--destination', $stable)
if (!(Test-Path -LiteralPath $stable)) {
    uv run --locked python -m sf2tool.remake_private_package @packageArgs
    if ($LASTEXITCODE) { throw 'Package copy failed' }
}
uv run --locked python -m sf2tool.remake_private_package @packageArgs --check
if ($LASTEXITCODE) { throw 'Package byte check failed' }
```

The packager reads the existing relative world dependencies and explicit entry/first-encounter
inputs. It publishes a fresh directory after a direct comparison. `--check` compares an existing
directory without mutating either copy. This verifies closure and preservation. The Content readers
remain the authority for semantic admission. For launch, select the three documents from `$stable`
and set the first-encounter environment variables from its six named opening files:

```powershell
$env:SF2_PRIVATE_EXPLORATION_CONTENT = "$stable/world.json"
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = "$stable/main-scenes.json"
$env:SF2_PRIVATE_ENTRY_CONTENT = "$stable/entry/entry.json"
foreach ($file in Get-ChildItem -LiteralPath "$stable/opening-inputs" -Filter 'SF2_PRIVATE_*.json' -File) {
    [Environment]::SetEnvironmentVariable($file.BaseName, $file.FullName, 'Process')
}
```

Repeat the [entry New Game witness](../godot/presentation.md#entry-confirmation-observation) with
`SF2_ENTRY_CONFIRMATION_CASE=new` and a fresh external disposable save namespace. The private
receipt directory retains the two short variants for the actual title/witch playback and the
Battle26 Continue witness. The latter uses a disposable copy of `deployed-en.json` from the
registered Battle26 review. Keep the real save untouched and inspect semantic state, raster
selection, audio receipts, exit and stderr. No gameplay settings or save schema change is needed.

Run the inventory when either base selection changes. `$upstream` selects the pinned read-only
SF2DISASM checkout, and `$assetRoot` selects the existing private asset Git repository. Use the
asset commit pinned in its preparation record. Keep the output CSV private.

```powershell
$catalog = Join-Path $handoffs 'issue1941/content/catalog-final/world.zh-CN.json'
$inventory = Join-Path (Split-Path -Parent $stable) 'retail-inventory-next.csv'
uv run --locked python -m sf2tool.remake_private_inventory --upstream $upstream --package $stable `
    --asset-root $assetRoot --asset-commit e0259ccef943d47a565d2f86a0e2118e368d428f `
    --catalog $catalog --output $inventory
```

### Prepare and mount retail resources

Use the tracked PowerShell 7 launcher from any directory. Select a checkout containing this
revision. Its checkout-local configuration selects the registered ROM, toolchains and shared data
root. The source checkout and matching H1 listing are read-only. The launcher accepts explicit
`-PreparationRoot`, `-Upstream`, `-Listing`, `-Library` and `-Package` paths for installations with different layouts.
The default preparation root is `media/retail-preparation/v1` beneath the shared input root.
Its `issue1966-original` and `retail-source-and-coverage.csv` supply the base and inventory.
Coverage and Mount read its published `retail-v1`. Prepare writes to the owning checkout's
`local/retail-prepare/retail-v1` by default. To inspect newly generated results, pass that same
absolute `-Library` to Coverage or Mount. An explicit preparation root selects a matching
inventory and base for a different version.
The local-only asset Git records the reviewed version in `manifests/retail-v1.json`.
`runtime/retail-v1` is directly selectable as `-Library` for Mount. Its matching
`source/retail-v1` subtree holds the command captures and driver boundaries at the same relative
family/command paths. For production recovery, copy both subtrees into a fresh private staging
library directory, retaining the file layout. The separately versioned base package still provides
the existing campaign consumers and already admitted audio variants.

```powershell
$repo = (Get-Location).ProviderPath    # start here in the selected code checkout
$run = Join-Path $repo 'tools/Prepare-PrivateAssets.ps1'
& $run -Repo $repo -Operation Prepare -Family icons
```

`All` verifies existing results and prepares missing families, including the three-member PLDN
selection. The reviewed private library includes the user-supplied `pldn-eric` family.
Its index binds member 14 to palette 3 and its scene additions carry five body frames and six item
bindings for Battle26. The two-member pilot took about five seconds. Allow 1 GiB of available
scratch for a PLDN rebuild on the observed host.

Choose `maps`, `walking`, `portraits`, `bodies`, `weapons`, `terrain`, `effects`, `icons`,
`ui`, `entry`, `music`, `sfx` or `pldn` to prepare a single family. PLDN also exposes
`pldn-party` (Chester and Rick) and `pldn-eric`. Audio supports a bounded selection such as
`-Family sfx -Command 87,99`. A fresh run uses the pinned source inventory, the registered
BizHawk/Genplus-gx installation and existing PCM producer. Completed commands publish
individually. Retain a failure's named native scratch before retrying its command. Existing
published versions are immutable. Restore damaged reviewed files from their private Git version
or choose a fresh library destination.

To regenerate coverage or assemble a package from the reviewed library, load the same checkout
configuration and choose a fresh output. For mounts that prepare character progressions, set
`$pinnedSource` to the matching pinned upstream checkout.

```powershell
. (Join-Path $repo 'local/private-inputs.ps1')
$root = Join-Path $repo 'local/retail-mounts'
[void][IO.Directory]::CreateDirectory($root)
$runtime = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$token = [guid]::NewGuid().ToString('N')
& $run -Repo $repo -Operation Coverage -Library $runtime -Destination "$root/coverage-$token.csv"
# Current accepted readers can use these field, UI and audio additions now.
& $run -Repo $repo -Operation Mount -Library $runtime -Family walking,icons,music,sfx `
    -Command 4,25,87,99 -Destination "$root/priority-$token"
# Assemble the PLDN package with growth and class-conditioned field sprites.
& $run -Repo $repo -Operation Mount -Library $runtime -Family All `
    -Command 4,25,87,99 -Upstream $pinnedSource -Destination "$root/pldn-$token"

# Add the two WIZ progressions and appearances to the registered PLDN package.
$progression = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue1956'
& $run -Repo $repo -Operation Mount -Library $runtime -Family wiz `
    -Package "$progression/pldn-v1" -Upstream $pinnedSource -Destination "$root/wiz-$token"

# Add the three GLDT/BDBT progressions and appearances to the registered WIZ package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family melee `
    -Package "$progression/wiz-v1" -Upstream $pinnedSource -Destination "$root/melee-$token"

# Add the three SNIP/BWNT progressions and appearances to the registered melee package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family ranged `
    -Package "$progression/melee-v1" -Upstream $pinnedSource -Destination "$root/ranged-$token"

# Add the two VICR progressions and appearances to the registered ranged package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family vicar `
    -Package "$progression/ranged-v1" -Upstream $pinnedSource -Destination "$root/vicar-$token"

# Add the WFBR and PHNX progressions and appearances to the registered Vicar package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family innate `
    -Package "$progression/vicar-v1" -Upstream $pinnedSource -Destination "$root/innate-$token"

# Add Slade's NINJ progression and appearances to the registered innate package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family ninja `
    -Package "$progression/innate-v1" -Upstream $pinnedSource -Destination "$root/ninja-$token"

# Add Kiwi's MNST progression and appearances to the registered Ninja package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family monster `
    -Package "$progression/ninja-v1" -Upstream $pinnedSource -Destination "$root/monster-$token"

# Add the prepared sound-wave overlay to the registered Monster package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family phoenix-wave `
    -Package "$progression/monster-v1" -Destination "$root/phoenix-wave-$token"

# Bind Indra Staff and the after-break unarmed appearances for WIZ and VICR.
& $run -Repo $repo -Operation Mount -Library $runtime -Family indra `
    -Package "$progression/phoenix-wave-v1" -Destination "$root/indra-$token"
```

The Indra mount consumes the already prepared weapon bank 16 and palette 31. It adds four
weapon frames and the armed and unarmed entries for Kazin, Tyrin, Sarah and Karna to
Battle26's scene, which supplies actor appearances to earlier encounters. The other package
files are carried unchanged. [Indra Staff reproduction](../domain/equipment-use-repair-design.md#indra-staff-reproduction)

Asset-only Mount without `-Upstream` uses the prepared family files and closed base package. The stable
`promotion-v1` package records that assembly with the three PLDN appearances. When PLDN is
selected, a supplied `-Upstream` adds growth and class-conditioned map sprite bindings from the
pinned source tables. The resulting progression package admits Chester, Rick and Eric. It uses
prepared media and needs no ROM, audio capture or production scratch. [PLDN reproduction](../application/ordinary-promotion-classes.md#pldn-ordinary-reproduction)

<a id="bedoe-audio-package"></a>

For Bedoe rescue, select the registered `media/retail-preparation/v1/priority-v1` package under the
shared input root. Use its `world.json`, `main-scenes.json` and `entry/entry.json` for
`SF2_PRIVATE_EXPLORATION_CONTENT`, `SF2_PRIVATE_BATTLE_SCENE_CONTENT` and
`SF2_PRIVATE_ENTRY_CONTENT`. Each JSON filename in `opening-inputs/` names its matching
input variable without the extension. The package already has the D2 recording for command 87
from `runtime/retail-v1/sfx/87` in the accepted private asset version. The existing Debug adapter
resolves `SFX_JUMP` against this mounted cue. [Bedoe input and Save/Continue](../application/dark-caves-bedoe.md#reproduce-the-optional-routes)

<a id="galam-sandals-audio-package"></a>

The registered `derived/issue2035/galam-town-v1` package includes the CB recording
for command 99 from the accepted retail asset publication. Select its world, scene,
entry and opening inputs with the normal Galam launcher configuration. Session audio
uses PSG tone 3 for command 99. In the pinned driver, `sfx_23` has one active
pointer in that slot. [Galam selection variables](../application/opening-session-lifecycle.md#reproduce-the-map16-observation) ·
[Actual equipment and audio observation](special-equipment-reach.md#remake-controls-and-reproduction)

The WIZ mount selects already prepared Kazin and Tyrin walking and battle bodies, reuses
the existing rod/stick layers and binds the two members' progression from `-Upstream`.
Use the resulting `wiz-v1` for both locales. Ordinary Kazin was played through Battle26. Tyrin's rite has controlled coverage. [WIZ reproduction](../application/ordinary-promotion-classes.md#wiz-ordinary-reproduction)

The melee mount adds the prepared Jaha, Randolf and Luke bodies and walking sprites to the
WIZ base and binds their member growth. It reuses the compatible axe and sword layers, including
Broad Sword. Ordinary Jaha and Luke were played through Battle26. Randolf's rite has
controlled coverage. [Melee reproduction](../application/ordinary-promotion-classes.md#gladiator-and-bird-battler-ordinary-reproduction)

The ranged mount adds the prepared Elric, Janet and May bodies, walking sprites and member
growth. Items 45–50 select the promoted body animation with no independent weapon layer.
This includes the retained arrows and promoted-only Robin Arrow, Assault Shell and Great Shot.
[Ranged reproduction](../application/ordinary-promotion-classes.md#sniper-and-bow-knight-ordinary-reproduction)

The Vicar mount adds the prepared Sarah and Karna walking sprites, bodies and growth.
The existing rod/stick layers supply their attack, item and spell scenes. Ordinary
healing and staff attack reused the mounted Battle26 resources. [Vicar reproduction](../application/ordinary-promotion-classes.md#vicar-ordinary-reproduction)

The innate mount adds Gerhalt and Peter's prepared walking strips, portraits and unarmed
battle bodies, with their own growth and current-class field bindings in `innate-v1`.
[Wolf Baron and Phoenix reproduction](../application/ordinary-promotion-classes.md#wolf-baron-and-phoenix-ordinary-reproduction)

The Ninja mount adds Slade's walking strips, portrait, body and spell icons to `ninja-v1`.
It reuses the mounted sword layers and binds a separate unarmed row. The learned attack
presentation selects the source flame or lightning variation in the current modern playback.
[Ninja reproduction](../application/ordinary-promotion-classes.md#ninja-controlled-reproduction-and-ordinary-pilot)

The Monster mount adds Kiwi's walking strips, remapped portrait, unarmed body and
special cast sequence in `monster-v1`. It binds two particles from the prepared
Flame Breath sheet using the source palette operands. [Monster reproduction](../application/ordinary-promotion-classes.md#monster-controlled-reproduction-and-ordinary-pilot)

The asset-only Phoenix wave mount adds four 32×32 scene rasters from the prepared 2,048-byte
tileset 20, using its palette operands. It adds 1,424 bytes to the closed 145-file package.
The package stages a single complete output before publication. The campaign scene reader shares
the new rasters with earlier mounted encounters. Selector 61 plays the overlay when all four
rasters are present. [Phoenix wave reproduction](../application/ordinary-promotion-classes.md#phoenix-sound-wave-playback)

Mount preserves the relative world, scene and entry dependencies, mounts any selected WAVs
and publishes a fresh directory after checking closure. Select the result with the ordinary
environment variables above. `Mount -Family All` includes the first consumers' 25, 87 and 99 cues.
Select other prepared cues with `-Command`. For new maps, bodies and effects, use the original ID
in the family index to select its atlas, tile bank, frames, palette and operands.

The closed-package template/deduplication experiment and its receipts remain reproducible at
code commit `2097434ec68c2e709312c25adab7e638e9566ccd`. Direct ID-indexed production and
the existing ordinary package files cover the demonstrated consumers without that intermediate
representation. [Measured coverage and admission boundaries](asset-inventory.md#retail-original-preparation)

### Sleep display name

Select the prepared catalog alongside the assembled world for the zh-CN name of spell 9:

```powershell
$env:SF2_DISPLAY_CATALOG = Join-Path $root 'catalogs-v1/world.zh-CN.json'
```

The private publication adds `{"namespace":"name.spell","index":9,"text":"催眠"}` to the
registered #1941 world display catalog. To reproduce it, read
`handoffs/issue1941/content/catalog-final/world.zh-CN.json` beneath the shared input root, add
that object to `names`, and serialize to a fresh UTF-8 file. Keep the existing source identity,
texts and name rows. The production display reader admitted 189 names and the same 2,221 messages
against the assembled world. Resolving spell 9 returned 催眠 in zh-CN with no fallback. This
observation covers label selection. The retail publication supplies the icon pairs. Ordinary spell use remains with the consumer.

## Battle weapon operands

`sf2tool.remake_battle_scene_content` preserves the selected animation's raw weapon frame/layer
and signed X/Y, and composes each 64×64 weapon from four column-major 32×32 hardware chunks.
The existing scene reader admits layers1–2. Neither preparation nor admission reverses their
meaning: layer1 is behind the body, layer2 in front; frame bits4/5 select horizontal/vertical flip.
The [bounded source/ROM and native audit](../godot/battle-scenes.md#weapon-frame-composition-1620)
locates #1620 in the Godot projection. Existing selected scene files and asset packs require no
regeneration or promotion. The authored front-layer experiment is disposable diagnostic content,
not an original asset correction or an update to the selected private world's authority.

## Deterministic Derivation and Cache

Main Git owns schemas, maintained generators/readers and minimum authored fixtures. The separate
private asset Git owns source/master/runtime/manifest history. Decoded intermediates, candidates,
font/audio imports, previews, `.godot`, exports, logs and scratch remain ignored worktree-local state.

Derivation binds source identity, policy/generator version, dimensions/format, semantic selection and
the resulting runtime payload. Reuse accepted assets read-only; a successful candidate/preflight
does not promote it or mutate the asset library. An owned change reviews and publishes the exact
master/runtime/manifest transaction separately. Do not rebuild material merely because a task changes.
The original [cache contract and completed candidate receipts](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#deterministic-derivation-and-cache)
retain tool versions, commands and detailed identities.

### Map57 candidate policy

Map57 is an explicitly prepared private resource selection, not an inferred ordinary startup map.
Retain its palette/slot/map-header provenance and unloaded-slot boundary; candidate generation does
not establish natural route, palette fidelity or general map loading. The complete
[candidate policy and reproduction](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#map57-candidate-policy)
preserve the source joins and retained results. Current before-battle projection has its
[Godot owner](../godot/presentation.md#bound-before-battle-scene-presentation).

## Ordinary menu publication and coverage (#1666)

**Standard rigor:** restore the existing exploration, battle and church diamond resource paths in
ordinary private startup, with one bounded publication and no gameplay or new asset-platform change.
Acceptance separates functional navigation, actual resource drawing and human experience. The
[presentation agreement](../godot/presentation.md#ordinary-menu-resource-acceptance-1666) owns native
observations; the [battle baseline](../godot/battle-interaction-baseline.md) keeps battle residuals.

**Confirmed:** the inspected ordinary input selections contained neither `presentation.fieldMenu`
nor `presentation.churchMenu`, and the selected initial battle scene contained no `command-menu.*`.
Title New Game and fresh title Continue reproduced text squares. Prior dedicated scene observations
prove their own prepared inputs, not these selections. The first break was publication/selection,
not raster decoding or the existing diamond consumers. User/machine environment scopes supplied no
replacement world/scene selection on the inspected host; the ignored launcher supplied them.

Use one publication for the **actual selected world and opening battle scene**, after loading the
[private-input environment](../../../docs/operations/local-private-inputs.md). All path variables below
are explicit absolute input/output selections; output must be a fresh ignored directory in this
checkout. The pinned source and retained H1 listing may be read-only.

```powershell
uv run python -m sf2tool.remake_command_menu --rom $rom --upstream $upstream --listing $listing --rom-assets --world $world --scene $scene --menu all --identity-icons --item-menu --shop-battlefield --target-cursor --out-dir $output
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
& $env:GODOT_BIN --path remake/game
```

The current complete publication additionally includes the [Shop/Options families](#shop-and-battlefield-menu-resources-1669).
This mounts the world menus, the external opening scene, and every non-null `encounters[].battleScenes`
selection. It preserves all encounter paths and gameplay data; existing raster namespaces must be
absent or exactly equal to the source-derived complete set. Partial/conflicting sets reject before
publication. Ordinary readers and `FieldMenuView`, `ChurchServiceView`, `BattleMenuLayout` remain the
runtime consumers. Selecting only the new opening scene leaves later battle scenes unchanged;
selecting only the world leaves the external opening scene unchanged. Both selections above are
therefore required. Local launchers must retain these selections for later ordinary sessions.

The publisher copies declared dependencies once, including external PCM WAV files at the existing
`PrivateAudioWave` convention `audio-cc-tt/command-cc-timer-tt.wav` (hex command/timer). `runtimeWav`
is a boolean declaration, not a path. The earlier single-menu helper's path assumption failed on
the current world and is corrected here. It does not copy an upstream checkout or change canonical
inputs. `provenance.json` records each menu's source, palette, mapping and raster identities, plus
prepared scene paths and copied dependencies. No extracted pixels enter Git or public packages.

The inspected input world is118,241,084 bytes. Its full publication including seven scene files,
external audio and provenance is about148MB, below the existing128MiB individual-world admission
limit and1GiB scratch budget. Added raw menu pixels are18,432 bytes per eight-image set: two world
sets and one set per selected battle scene. Work is O(world + selected dependencies), with one world
parse/write and bounded source decoding; retain the existing<3GiB process budget. Reassess if the
selected world/dependencies exceed these bounds. Reuse this publication read-only for native cases;
no per-language world copies, full campaign replay or screenshot acquisition is required.

### Bounded source-to-consumer inventory

**Confirmed** static source identities below use SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered US ROM. Paths are relative to
`disasm/`. Read them with `git show <pin>:disasm/<path>`; menu addresses/mappings are retained in
`tests/fixtures/h2/ui-graphics-decode-v1.json`. These facts do not prove original natural reach,
original animation cadence or human visual acceptance.

| Surface / source identity | Existing reuse and current consumer | Current resource boundary / next owner |
| --- | --- | --- |
| Field Member/Magic/Item/Search: `tiles_MainMenu` ROM0x29A02, `menutilespointertable.asm` packed0x85010204, indices5/1/2/4 | `remake_command_menu --menu all`; `world.presentation.fieldMenu` → `ExplorationAssetReader` → `FieldMenuView` → `DiamondCommandButton` | **Confirmed** ordinary New Game drawing, both states, English/Chinese and parent returns. |
| Battle Attack/Magic/Item/Stay: same source, packed0x80010203, indices0/1/2/3 | `scene.rasters.command-menu.*` → existing scene admission → `BattleMenuLayout` → shared button | **Confirmed** ordinary title Continue drawing on an admitted battle, both states and English/Chinese. All seven prepared scene collections contain the same complete set; this is not seven native battle observations. The authored/controlled chest Search caption still deliberately suppresses Stay art; its original index4 variant is not claimed by this repair. Battles01–07 have no admitted in-region chest candidate. |
| Church Raise/Cure/Promo./Save: `tiles_ChurchMenu` ROM0x2B072, four sequential icons | `world.presentation.churchMenu` → existing admission → `ChurchServiceView` → shared button | **Confirmed** ordinary New Game, real Map3 stairs/walk/priest, normal/selected/unavailable/return. |
| Spell and item identity icons: `data/graphics/icons/entries.asm`, `ItemIconNNN`, `OtherIconNNN`, `SpellIconNNN`, `p_Icons`;192 bytes per16×24 icon | Existing `texture_extract.extract_icons`, verified source reader, base palette and tile renderer. `loadhighlightableicon.asm` masks item index with0x7F; spell index with0x3F then adds130; empty spell uses icon127. Selected art is an AND overlay from `tiles_IconHighlight`, not a second stored image. | [#1667](#spell-and-item-identity-art-1667) publishes bounded validated bindings consumed by `BattleMenuLayout` spell/item rows, `BattleSessionView` equipment HUD and `FieldActionView` magic/use/equipment rows. The earlier diamond-only pack remains a distinct optional text mode for these lists. |
| Spell level: `code/common/menus/magicmenu.asm:SelectSpellLevel`, `tiles_TextHighlight`; equipment: `equipmenu.asm:BuildBattleEquipWindow`, `itemmenu.asm` | Level text/selector and item icon are distinct source operands. Current localized text retains actual level/cost/slot and rules. | #1667 binds semantic spell/item IDs and retains localized level/cost/description text. No separate per-level picture is established by this source; exact original border/window presentation remains outside this ticket. |
| Item operations Use/Give/Equip/Drop: `tiles_ItemMenu` ROM0x2A9C2; `pt_ItemMenuNames` gives source order. Member sheet: `memberscreen.asm:BuildMemberStatusWindow` loads spell/item icons and broken-item overlays. | Shared diamond decoder/palette for operation icons, `extract_icons` for individual slots. Shared `PartyDetailsPanel`, field Give/Drop/exchange rows and the battle transaction HUD consume semantic identities. | [#1668](#member-inventory-and-item-operations-1668) adds the separate24×24 Item operation set and existing Member/inventory/Give/Drop consumers. Broken overlays/current unsupported item states stay explicit. |
| Shop Buy/Sell/Repair/Deals: `tiles_ShopMenu` ROM0x2B418, `pt_ShopMenuNames`. Empty-ground Member/Map/Speed/Quit: `tiles_BattleFieldMenu` ROM0x2ACF6, `pt_BattlefieldMenuNames`. | Existing stack decoder, verified source, palette and 3×3 half renderer; current `ShopServiceView` and `BattleInspectionView` consume source cardinal art. Goods use the same individual item icon family. | [#1669 resource owner](#shop-and-battlefield-menu-resources-1669) records current service/options bindings and unchanged caller authority. Modern Map drawing, HP/MP gauges, panel chrome and Chinese font are accepted separate choices, not missing original menu assets. |

The inventory is finite: sources, admission seams and actual consumers are named. #1667 implements
the spell/Use/Equip subset; #1668 extends Member/Give/Drop and Item operations. Shop/empty-ground options art has the [#1669 owner](#shop-and-battlefield-menu-resources-1669). Native entry/return coverage and static source facts are
separate from the **Unknown** original natural/cadence/pixel parity and human/aesthetic approval.

## Private Command Menu Icons

The current diamond-menu experiment uses the original square attack/magic/item/stay rasters directly
at the four cardinal positions, without another drawn frame or per-icon caption. One shared window
shows the focused command. This is a private presentation choice, separate from the
future SVG pipeline below. No authored icon substitutes for a missing original raster.

Prepare a copy of an admitted battle scene with `uv run python -m sf2tool.remake_command_menu
--rom <registered-rom> --upstream <pinned-upstream-with-H1-listing> --scene <admitted-scene.json>
--out-dir <fresh-ignored-directory>`. The helper reuses `texture_extract` source/ROM verification,
base-palette decoding and tile rendering. `tiles_MainMenu` in
`data/graphics/tech/menus/mainmenutiles.bin` contains seven 576-byte icons; the first four supply the
four commands. Each 288-byte half is nine tiles in the row-major 3×3 arrangement of
`diamondmenulayout.asm`: normal first, selected second. These are source-backed pixel/layout facts,
not a claim about original animation timing or modern visual parity.

The output scene adds eight 24×24 RGBA8 rasters named
`command-menu.{attack|magic|item|stay}.{normal|selected}`, with the existing raster digest fields.
`provenance.json` records pinned upstream/ROM identity, source offsets and raster digests. Select the
new scene through the existing `SF2_PRIVATE_BATTLE_SCENE_CONTENT` input; no new startup switch or
resource loader is involved. Preparation reads source inputs only and refuses to replace an output
directory or a partial/conflicting command-menu namespace; identical complete art may be reused. The eight raw rasters total 18KiB, in
addition to the copied scene and JSON encoding.

The ordinary Content reader admits the rasters. The adapter requires all eight when any is present,
requires 24×24 dimensions, and owns/replaces textures with the active scene. A malformed or partial
set fails through the existing adapter-error boundary. Nearest sampling, no mipmaps and integral
icon scale preserve pixel edges. Scenes with no command-menu namespace display text-only squares.
Extracted pixels and prepared scenes stay ignored and must not enter public packages or PR attachments.

## Private church menu icons

**Confirmed:** the optional `world.presentation.churchMenu` contains exactly
`{raise|cure|promote|save}.{normal|selected}` using the existing authenticated raster fields.
All eight must be24×24; absence preserves compatible public/older inputs and renders short authored
text squares. Private original pixels remain ignored, never bundled in public content or PR attachments.
Content validates the set/shape/bytes; native decoding failure stops the host and releases temporary
textures. The [current UI agreement](../godot/presentation.md#church-and-shared-save-agreement-1519)
owns focus, choice and service behavior.

Use the existing preparation entry after same-process input/tool configuration:

```powershell
uv run python -m sf2tool.remake_command_menu --rom $rom --upstream $upstream `
    --world $world --out-dir $freshIgnoredOutput
```

`--world` accepts either a private-world wrapper or a standalone world object; it adds only church
rasters and emits `world.json`/`provenance.json` into a new destination. Preserve the original inputs.
Relative selected encounter files and external audio are copied beneath that destination with their
existing relative paths/fixed command-timer convention; escaping/missing dependencies fail before publication. No world rule,
program, event or source-asset substitution is performed. Input/output CLI paths resolve from the
caller's cwd; dependencies resolve from the explicit input world's directory. Use absolute selections
when invoking from another checkout or working directory.

A read-only pinned checkout may omit split binaries and generated H1 output. Select an existing
pinned H1 listing with `--listing $listing` and deliberately add `--rom-assets` to decode the verified
canonical ROM directly. The source identity/parity owner is `sf2tool.graphics_source`; existing imports through
`texture_extract` retain their entry points. The unchanged split-payload parity reader remains mandatory for every split
payload that is present; mismatched bytes/lengths reject. This route verifies the same canonical ROM
identity, pinned upstream, existing accepted H1 menu addresses/pointer and palette address. It does
not build H1, modify upstream or infer permission to redistribute pixels.

Named provenance: pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`menustringspointertable.asm:pt_ChurchMenuNames` gives Raise/Cure/Promo./Save order;
`diamondmenu.asm:dmaDiamondMenuIcon_Up/Left/Right/Down` reads offsets0/0x240/0x480/0x6C0
and adds0x120 when frame counter>=15. Each288-byte half is nine row-major tiles. The modern
normal/selected assignment uses those two halves; it does not reproduce the original15-frame cadence
or inserted side border tiles. `tiles_ChurchMenu` is ROM0x2B072,934 compressed bytes through
`tiles_ShopMenu`0x2B418; the decoder consumes7438 bits into2304 bytes. Existing
`tests/fixtures/h2/ui-graphics-decode-v1.json` binds these menu addresses.
`displayinit.asm:InitializeDisplay` copies `palette_Base`/`CRAM_PALETTE_SIZE` into `PALETTE_3_BASE`;
`diamondmenulayout.asm` selects `PALETTE3` for its menu tiles. The accepted player-frame fixture's
`static.indexBindings.paletteBase`/`static.palettePolicy` binds ROM0x309E,32 bytes/16 big-endian
0x0EEE words. Index0 stays transparent. Preparation records those addresses, consumed bits,
source mapping and each raster's existing content digest.

Eight raw rasters total18,432 bytes (about25KiB base64). The retained single-menu measurement below describes its older input; use the
[current ordinary publication](#ordinary-menu-publication-and-coverage-1666) for current sizing. One world publication is O(n)
time/memory plus its finite selected dependencies. The observed private input is125,828,417 bytes;
publication is125,978,971 bytes plus669,487 dependency bytes and2,490 provenance bytes. Use one world copy, one process,
<3GiB peak and<1GiB total scratch; reassess before larger worlds/dependency sets. Native acceptance
uses semantic samples under5MiB/run, no screenshots or whole-route acquisition.

## Public-authored battle gauges

The shared ordinary-game status card loads four small PNGs directly from
[`game/assets/ui/battle-status`](../../game/assets/ui/battle-status/README.md), using Godot's existing
resource import/packaging. The accompanying source statement dedicates the project-authored
geometric frame, dark track and two shaded fills to CC0 1.0. Frame size is 244×28 RGBA8; track/fills
are 232×16. Lossless imports disable mipmaps; the card uses nearest sampling and crops fill width
from the live current/max ratio. Labels and numbers remain native text. These images are public,
contain no extracted/private pixels, and are not a reconstruction claimed as original art.

Read-only inspection of local asset Git `293c9460818f4768905be809e83488d77f698fba` found admitted
yes/no-frame and tactical-cursor masters but no gauge resource in its manifest/runtime. Unreviewed
SVG candidates therefore supply no runtime mapping. This small public set does not modify that
independent repository, private preparation/admission, source provenance or its license boundary.
Its PNGs are the masters and runtime resources; no new derivation platform or private input is needed.

## SVG HUD Source

HUD SVG tracked in the local asset repository's `masters/` is restricted to a deterministic,
reviewable subset:

- explicit `viewBox`, logical dimensions, and stable element IDs;
- paths and basic geometric shapes with declared fills/strokes;
- no scripts, external URLs, machine paths, embedded raster payloads, or local original assets;
- no semantic text embedded as `<text>`; Godot's font system owns localizable text;
- no effect whose rendering is not closed by the selected pinned rasterizer.

The SVG sources reconstruct HUD frames, panels, focus/focus-loss states, cursors, meters, separators,
and semantic input glyphs in the project's visual language. They are not screenshots or extracted UI
payloads. Candidate derivatives are generated into ignored `cache/`; reviewed 2x/4x derivatives are
promoted into tracked `runtime/` with their manifest.

Godot can rasterize SVG at import and can use `DPITexture` for oversampling, but its SVG support is a
bounded ThorVG subset. The local product pipeline therefore requires a pinned, verified derivation rather
than depending on arbitrary runtime SVG parsing. See Godot's
[image import documentation](https://docs.godotengine.org/en/latest/tutorials/assets_pipeline/importing_images.html).

## Texture and Color Policies

Texture policy is declared per asset family instead of globally:

| Family | Default policy | Mipmap boundary |
| --- | --- | --- |
| SVG-derived HUD and glyphs | lossless sRGB RGBA, clamp, linear filter at selected bucket | off at bucket-native scale |
| source-crisp world/character material | color-preserving deterministic integer derivation; nearest sampling where crisp cells are intentional | off unless a verified camera/downscale path needs it |
| reviewed smooth world/character derivative | named, pinned edge treatment; linear sampling only after visual approval | per family, not global |
| large backgrounds | declared lossless or reviewed compression and filtering | on only when material is materially downscaled |

The default original-material derivation preserves source color identity. Dither smoothing, expanded
ramps, interpolation, post-processing, or other visual treatments are explicit presentation deviation
profiles under 9A. They are never silently inferred from the source and never change gameplay or
evidence state.

HUD color tokens are project-owned theme values. They may preserve the original-style visual language
through compact ramps, strong silhouettes, layered frames, and restrained highlights, but they remain
separate from original palette/CRAM evidence and from the private world-material palette.

## Fonts, Theme, and Input Glyphs

The local asset repository tracks product font files, theme data, and their manifests. The main code
repository tracks only the consuming schema/API and fixture configurations. Product configuration owns
logical sizes, weights, line height, outline/shadow tokens, fallback roles, required glyph ranges, and
whether a role uses dynamic or MSDF rendering. Font and theme resources are explicit asset-pack entries
with stable semantic IDs.

MSDF is appropriate for headings or roles that span a large scale range after optical testing. Small
body text and dense CJK text may use hinted dynamic rendering when it is clearer. Required glyphs are
prewarmed to avoid first-use stalls, and the fallback order is deterministic. Semantic text is never
converted into HUD SVG outlines.

Input hints bind semantic actions to project-authored SVG glyph IDs for the current device and binding.
Every glyph has a localized text fallback. Keyboard, controller, and accessibility remapping select a
glyph; glyph filenames and presenter nodes never own command rules.

## Public Synthetic and Test Fixtures

The `public-synthetic` smoke/export profile was retired at M5. Project-authored fixtures may still
exercise dimensions, animation counts, missing assets, bucket selection, cache drift, safe areas, and
cue-to-resource mapping. Fixtures and synthetic presentation are never selected as product content,
and an explicit local product request cannot fall back to them while reporting success.

## Product Design and Distribution Limit

The SVG/font/theme specifications above constrain proposed product assets and existing candidate
tooling; they do not claim that the ordinary host has a complete product Theme, font/glyph UI or
general HUD catalog. #438 owns later art/UI/UX choices. No public export or distribution right is
established by private preparation, local asset Git history, successful checks or native play.

## Private exploration menu icons

`presentation.fieldMenu` optionally supplies exactly member/magic/item/search normal/selected
24×24 rasters. The ordinary Content raster identity/shape checks remain authoritative; partial sets
reject. The adapter displays unchanged pixels at nearest4× through the shared diamond control.
Missing art keeps short text captions; native decode failure stops once with
`field-menu-resource-invalid`. No original bitmap is tracked or granted public distribution rights.

The existing `sf2tool.remake_command_menu` world publisher adds `--menu field`; its default remains
Church and its battle scene route is unchanged. Example after loading the local input configuration:

```powershell
uv run python -m sf2tool.remake_command_menu --rom <verified-rom> --upstream <pinned-upstream> `
    --world <existing-world-or-authored-package> --out-dir <new-ignored-directory> `
    --listing <existing-pinned-H1-listing> --rom-assets --menu field
```

Only use explicit `--rom-assets` when the pinned read-only checkout lacks split binaries. Identity,
H1/accepted table addresses, pointer and palette checks precede decoding; present split payloads
still require strict ROM parity and exact size. `tiles_MainMenu` is4032 uncompressed bytes at
0x29A02, `p_tiles_MainMenu` at0x28010 and `pt_tiles_Menu` at0x1115C. The first packed entry
`0x85010204` selects icon records5/1/2/4 (Member/Magic/Item/Search); the accepted
`tests/fixtures/h2/ui-graphics-decode-v1.json` binds those addresses and indices. Palette_Base at
0x309E and its32 bytes follow the existing church palette binding. The pinned revision and source
symbols are in the [gameplay agreement](../godot/presentation.md#exploration-gameplay-diamond-agreement-1588).
A provenance receipt records source offsets, canonical identity, palette and resource bytes.

Minimum work is8×24×24×4=18,432 raw icon bytes, approximately25KiB base64; publication additionally
reads/writes its whole input JSON and selected finite relative encounter/audio dependencies. The
bounded observation reused one126,677,125-byte private publication and tiny authored variants,
with O(input bytes) time/space, <3GiB assumed peak memory and <1GiB scratch. Existing private data
and listing remained read-only. No full asset export, network acquisition or comparison pipeline
was added. Replan for new dependencies, escaping paths or measured volume/memory above that budget.

## Spell and item identity art (#1667)

**Standard rigor:** add original identity art to the existing battle Magic/Item/Equip and exploration
Magic/Use/Equip choices. Preserve gameplay, localization, confirmation, return and 96px world scale.
Member/Give/Drop, operation diamonds, shops/options, broken overlays, full UI reproduction and human
visual approval are outside this slice. Acceptance separates ordinary functional access/return,
actual admitted texture drawing, and human experience.

`remake_command_menu --menu all --identity-icons` extends the existing one-publication command above.
`remake_identity_icons` reuses the verified source reader, `extract_icons`' base-palette/2×3 tile
renderer and the existing raster format. It resolves source `ITEM_…`/`SPELL_…` enums, validates assembled
entry order, accepted H1 table/pointer/palette addresses and present split-payload parity. The selected
frame is the original 192-byte icon AND `tiles_IconHighlight`. `magicmenulayout.asm` places tiles1/2,
3/4,5/6 in rows; there is no invented corner cleanup, separate level art or unavailable-state artwork.

The finite player catalog is items0/1/3/4/33/56/57/71/72/73/85/86/103/123/127 (initial seven-member
gear, admitted supplies/stock/Mithril and empty); spells0/2/6/10/11/14 (HEAL/DETOX/DISPEL/EGRESS/BLAZE/BLAST);
and source `ICON_UNARMED`128. Enemy-only Wooden Arrow45 is not required by these player consumers.
Only already admitted capabilities are displayed; resource membership never grants an action.
Each base spell ID has one pair for all offered levels. The source's physical spell16–18 storage
collisions are excluded. Empty spell slots have no selectable row; empty item127 and Unarmed128 keep
their distinct source identities. Item equipped flags are masked with127, and live slot order remains
independent of image identity, duplicate items, translated names and action availability.

The optional `presentation.hdIdentityIcons` bank binds the existing identity state keys to 64×96
variants. `hdFieldMenu`, `hdItemMenu`, `hdShopMenu` and `hdChurchMenu` bind their source menu
keys to 96×96 variants. Battle declarations use the existing `hdRasters` bank under the same
Original keys. The [complete UI raster package](asset-inventory.md#pre-voyage-ui-rasters-2178)
preserves the previously mounted herb pair. The [consumer contract](hd-rollout.md#ui-raster-consumers-2178)
defines group fallback and the saved Original/HD setting.

World `presentation.identityIcons` and every selected battle scene's `rasters` carry
`identity-icon.item.<id>.normal/selected`, `identity-icon.spell.<base-id>.normal/selected`, and
`identity-icon.other.128.normal/selected`. `IdentityIconReader` checks canonical keys, dimensions,
pairs and bindings for the admitted player classes/items and supported spells; existing raster
admission checks bytes/digests. A declared partial pair, missing required complete pair or wrong index
is a ContentError. Absence of the entire optional namespace preserves legacy/private or public
`unmounted-text` presentation; it does not report bound art. New ordinary publication selects both
world and scene, as before. No asset payload or additional repository is vendored.

`IdentityIcons` alone creates/disposes identity textures; rows and the equipment HUD borrow them.
The battle menu shares its owner with equipment, inspection and transaction HUDs, retaining it across
the same view's field/battle reparenting until content replacement or disposal. Field Member and
preparation share the parent `PartyStatusView` owner for its view/content lifetime.
`IdentityIcon` draws16×24 at integral4×/Nearest without tinting source pixels. Text
independently marks focus/availability and retains localized names, slot, level, MP and ATT/range.
An unexpected runtime binding miss produces an explicit error marker/observation with usable Back,
rather than throwing from a row or silently counting a placeholder as bound art. The current Content
checks normally reject that condition before mounting.

Reuse the #1666 publication read-only as input. One complete output is about149MB;44 raw icon rasters
add67,584 bytes per collection (one world and seven scene collections), about721KB base64 before JSON
metadata. Work stays O(input plus declared dependencies), with the existing<3GiB assumed process and
1GiB scratch budgets. Do not publish separate worlds per language/case. Native/resource acceptance and
its independent presenter limitation are in the [presentation owner](../godot/presentation.md#identity-art-acceptance-1667).

## Member, inventory and Item operations (#1668)

**Standard rigor:** extend the accepted identity bindings to the existing field/battle Member sheet,
field Give/Drop/full-pack exchange, and battle Give/Drop selection and confirmation HUD. Preserve live
slots, equipment, localized names, unavailable reasons, scrolling, caller-specific Back and actual
transactions. This adds presentation only; Domain rules, inventory authority and gameplay admission
do not depend on the art catalog. Public authored and old private namespaces remain explicit text.
Shop and empty-ground options art has the [separate owner below](#shop-and-battlefield-menu-resources-1669); broken overlays, unadmitted item states and human
visual approval remain outside this slice.

`remake_command_menu --menu all --item-menu` consumes the accepted publication read-only and adds
world `presentation.itemMenu`. The eight exact keys are `use/give/equip/drop.normal/selected`.
`ExplorationAssetReader` requires the complete24×24 set and existing digest/byte admission; partial
sets and conflicting existing publication resources reject. Whole-namespace absence is optional text.
The producer verifies `tiles_ItemMenu`0x2A9C2, `tiles_BattleFieldMenu`0x2ACF6,
`p_tiles_ItemMenu`0x28014 and `pt_tiles_Menu+12` against the pinned listing/accepted fixture and ROM.
It stack-decodes820 source bytes into2304 bytes, then uses the existing base-palette3×3-tile renderer.
`pt_ItemMenuNames` and diamond DMA slots select Up Use, Left Give, Right Equip, Down Drop.
These are separate24×24 normal/selected source records; `command-menu.item` is not substituted.

One publication reuses all seven unchanged scene sets and26 unchanged dependencies. The eight
operation rasters add18,432 raw bytes, approximately25KiB base64; the resulting product pack remains
about149MB. Time/space are O(input plus dependencies), with the existing<3GiB assumed process/1GiB
scratch bounds; peak memory is not independently measured. Two additional world JSON variants for
bounded corrupt-PNG and partial-set rejection observations reuse the same dependencies; no per-language packages,
new extraction pipeline or public asset payload is created.

Member rows borrow normal identity textures from their parent view owner, including distinct source
empty127; unknown source slots remain text. The field operation menu owns its four pairs once per
active content and reuses the existing diamond geometry/button lifecycle. Field Give/Drop and return
slots use live item words with equipped flags masked. Battle HUD uses the same menu texture owner,
with separate given/returned images outside its existing readable text column. No original recipient
portrait, independent unavailable image, new fifth operation or per-level spell image is invented.
The [presentation owner](../godot/presentation.md#member-inventory-art-acceptance-1668) records finite
ordinary/controlled acceptance and remaining Unknowns.

## Shop and battlefield menu resources (#1669)

**Standard rigor:** show the original service/options and item identities at the existing admitted
shop and empty-ground callers. Keep Buy/Sell, unavailable Repair/Deals reasons, independent Leave,
Map/Member browsing, Speed drafts, Quit save safety and actual parent returns. No new shop service,
battle/save/RNG rule, global #1616 layout, original pixel distribution or human approval is included.
The [presentation acceptance](../godot/presentation.md#shop-and-options-art-acceptance-1669) separates
actual ordinary/controlled consumers from source facts and remaining Unknowns.

**Confirmed static:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, registered
US ROM and retained H1 listing resolve `tiles_ShopMenu`0x2B418/936 bytes and the exact spelling
`tiles_BattleFieldMenu`0x2ACF6/892 bytes. The existing stack decoder produces2304 bytes from7445
and7103 consumed bits respectively. `menutilespointertable.asm` selects `p_tiles_ShopMenu`0x28020
at `pt_tiles_Menu+24`, and `p_tiles_BattlefieldMenu`0x28018 at `+16`. The producer verifies these
against the accepted UI graphics fixture/listing/ROM and reads the ROM name pointers, rather than
using modern text-list indices. `menustringspointertable.asm` gives these source slots:

| Family | Up / source0 | Left / source1 | Right / source2 | Down / source3 |
| --- | --- | --- | --- | --- |
| `pt_ShopMenuNames` | Buy | Sell | Repair | Deals |
| `pt_BattlefieldMenuNames` | Member | Map | Speed | Quit |

`diamondmenu.asm`'s cardinal DMA consumers select each record's288-byte normal/selected halves.
The existing base palette/3×3 renderer supplies24×24 images. `shop/shopactions.asm:ShopMenu`
initializes Up, dispatches Buy/Sell/Repair/Deals and exits on `-1`; there is no fifth Leave icon.
`battlefunctions_2.asm:BattlefieldMenu` dispatches0 Member,1 minimap,2 settings,3 suspend and
returns to its cursor on `-1`. The remake retains its separately accepted browsing, modern Speed
and shared two-slot Quit policies; original suspend semantics do not redefine that save contract.

### Final publication and ordinary selection

The command in [ordinary menu publication](#ordinary-menu-publication-and-coverage-1666) is the
complete preparation command. `--identity-icons --item-menu --shop-battlefield` adds the admitted
identity catalog, Item operations, shop services and battlefield options to `--menu all`.
`--target-cursor` additionally mounts the accepted original32×32 single-cell resource in all seven
scenes. The [cursor owner](../godot/battle-scenes.md#original-target-and-inspection-cursor-publication)
records its source/pixel identity, optional-old-package admission, exact F4/F8 placement and native
acceptance. The normal battle-scene producer includes the same resource in its existing candidate
manifest and provenance; neither producer changes the separate private asset library's modern SVG.
When reusing an already complete identity/Item publication, omit `--identity-icons` to preserve
those existing bytes. Inputs are read-only; choose a fresh ignored `$output`. No output is canonical
until explicitly selected. The base campaign `$freshWorld` supplies gameplay data; it is not the
complete product-art selection. Old private packages keep their existing identities or explicit
text mode, and public authored content remains text where the private namespace is absent.

The final slice publication consumes the prior complete publication once, adds world
`presentation.shopMenu` (`buy/sell/repair/deals.normal/selected`) and adds
`battlefield-menu.member/map/speed/quit.normal/selected` to all seven selected scene sets, including
the external opening `scene.json`. Whole-family absence is optional; declared partial, extra,
wrong-shaped or conflicting sets reject. A native decode failure stays explicit in the owning menu,
blocks its service/options dispatch and retains Leave/Back. It must not redirect to another caller
or report original-art success. Identity icons use the existing16×24 semantic item catalog.

Select **both** outputs in the same process before ordinary title/New Game/Continue, retaining the
caller's existing save root/namespace, settings and both display catalogs:

```powershell
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $output 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $output 'scene.json'
```

The owning checkout's ignored current-content selector selects the already generated complete pack
using paths relative to its own script. Dot-source it from that checkout;
it sets only these two environment selections and does not launch a game or configure player saves.
Its availability and exact local reproduction belong in the slice handoff. The CLI recipe above
remains the durable way to regenerate the pack.

For an already complete menu/identity/Item/Shop pack, only `--menu all --target-cursor` is needed;
the existing menus are compared/preserved and the remaining families are carried through unchanged.
Select both outputs before ordinary title/New Game/Continue. This publication keeps the prior
world/gameplay/save identities and every declared dependency. Retain the previous output and use
a fresh explicit ignored destination; never edit its only copy or a player's save namespace.

The [cursor publication budget](../godot/battle-scenes.md#placement-and-bounded-acceptance) records the
current complete pack size and unchanged-world/dependency checks. Work is O(input plus declared
dependencies), one world copy; retain the assumed<3GiB peak-process/1GiB scratch budget, not an
independently measured peak. Small negative scene variants reuse the world/dependencies; no language
copies, full-route collection or new asset platform. Replan if growth breaks these bounds.

### Switchable entry layers

The [entry producer](asset-inventory.md#pre-voyage-entry-layers-2179) adds optional
`hdResources` beside the six required Original `resources`. Each alternate row contains
`name`, `file`, `width`, and `height`; names match the Original six, filenames are
`<name>.hd.png`, and dimensions are 1024×896. Unknown or duplicate names, extra/missing
row fields, inconsistent dimensions, missing files and decode failures reject entry art
through its existing content-error presentation. Original dimensions remain 256×224.

EntryPresentationAssets decodes every declared texture at mount and owns both banks until
unmount. EntryBackdrop reads the existing InputSettings.GraphicsMode reference after the
SettingsHost prepare/write/publish transaction. Only a complete six-layer group selects HD.
Missing or partial groups select Original together. No new preference or animation clock is
introduced. Numeral offset, source-layer order, Witch arrival/base/blink/mouth selection and
Reduced Flash are shared by both modes. The complete published package selection is
`derived/issue2179/pre-voyage-entry-v1`. [Production observation](hd-rollout.md#production-entry-reproduction).

### Animated map tiles

The complete animated-terrain base is `derived/issue2180/pre-voyage-tiles-v1`; the
[enemy healing successor](#enemy-healing-successor) retains its tile banks.
A map visual may include `tileAnimation` with an absolute VDP tile target, a contiguous
tile count, a cache capacity and ordered phases. Each phase records its source tile,
enabled-tick duration, changed block pairs and HD neighborhood overrides. The renderer
reads all geometry, flip and priority from the existing terrain words. Field pickup
alpha uses the same selected block as the field draw. Legacy packages omit the table
and continue to resolve their static banks. The mechanism, production and bounded
native readbacks are recorded in the [rollout decision](hd-rollout.md#tile-substitutions-2180)
and [resource trace](asset-inventory.md#pre-voyage-animated-terrain-2180).

### Enemy healing successor

The enemy healing successor is `derived/issue2227/pre-voyage-healing-v1`.
Publish from the complete #2180 base using the registered retail effects and pinned graphics
checkout, with explicit absolute paths selected locally:

```powershell
uv run --with pillow python -m sf2tool.remake_enemy_healing `
  --base $base --effects $effects --output $output --gfx-root $gfxRoot
```

The output must be fresh under worktree `local/` or registered shared `derived/`. The producer
reuses the closed-package dependency discovery and Original/HD raster recipe, adds optional
`healing.enemy` bodies/wings/dust and retains every existing bank. Select its `world.json`,
`main-scenes.json`, `entry/entry.json` and all six `opening-inputs` together. New input parsing
accepts legacy healing groups without `enemy`; declared enemy groups require two bodies, two
wings and five dust resources. The [resource owner](asset-inventory.md#enemy-healing-family-2227)
records the bounded volume, provenance and native acceptance. Private enemy AURA and unbound
higher-rank gameplay have no new delivery claim.

### DETOX successor

Select `derived/issue2232/pre-voyage-detox-v1` for prepared private Battle Magic 1–3.
Publish from the complete #2227 healing successor with the registered retail effects
and pinned graphics checkout. Select explicit absolute paths for the four arguments.

```powershell
uv run --with pillow python -m sf2tool.remake_detox_art `
  --base $base --effects $effects --output $output --gfx-root $gfxRoot
```

The output must be fresh under worktree `local/` or the registered shared `derived/`
directory. The named package mounts all eleven Original and HD rasters in each declared
battle scene. Select its `world.json`, `main-scenes.json`, `entry/entry.json` and six
`opening-inputs` selections as a unit. Earlier packages keep the pose and result path.
The [resource trace](asset-inventory.md#detox-family-2232) records provenance and closure,
and the [spell owner](../godot/spell-presentation.md#prepared-detox-presentation) records
the controlled native runs.


### DISPEL successor

`derived/issue2238/pre-voyage-dispel-v1` is the complete successor to the #2232 package.
It adds three Original/4× HD DEBUFF1 rasters in each declared battle scene. Select world,
main scene, entry and six opening inputs together. Registered retail effects and the accepted
filter checkout are read-only inputs. Publish to a fresh absolute local or registered derived path.

```powershell
uv run --with pillow python -m sf2tool.remake_dispel_art `
  --base $base --effects $effects --output $output --gfx-root $gfxRoot
```

The [resource inventory](asset-inventory.md#debuff1-family-2238) records source mapping and
volume. [Playback and reproduction](../godot/spell-presentation.md#dispel-playback-implementation-2238)
record the original caller boundary. The [family extension](../godot/spell-presentation.md#debuff-family-playback-2280)
reuses these exact rasters from the current `derived/issue2274/equipment-v2` publication,
with independent #2252 opening, #2255 Witch and #2266 catalog selections. It publishes no
new asset successor; `derived/issue2280/review-v1` holds only independent review inputs/receipts.

### Baron promotion successor

`derived/issue2240/pre-voyage-brn-v1` extends the complete DISPEL package with Jaha’s and
Randolf’s BRN variants. It also mounts Eric’s accepted PGNT variant, which was absent
from the DISPEL predecessor. Use the same world, main scene, entry and six opening
inputs selection as a unit. Select the registered prepared retail library and the
pinned source checkout read-only. [Resource and closure evidence](asset-inventory.md#baron-promotion-2240)

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2238/pre-voyage-dispel-v1'
uv run python -m sf2tool.remake_retail_assets --root $registeredRetailLibrary `
    --base $base --upstream $pinnedSource --mount $freshAbsolutePackage --family pgnt --family brn
```

Select `derived/issue2240/catalogs-v1/world.zh-CN.json` for the Chinese names. It retains
the accepted PGNT catalog’s 2,221 messages and 190 names and adds `name.class/16` from
the registered `names.zh-CN.jsonl`. The private review’s `runner/prepare.py catalog`
reproduces the addition from the PGNT catalog and the registered name source.

### Brass Gunner promotion successor

`derived/issue2242/pre-voyage-brgn-v1` extends the complete #2240 package with Elric and
Janet BRGN growth, walking sprites and six current legal weapon bindings each. Select its
world, main scene, entry and six opening inputs together with the registered prepared retail
library and `derived/issue2240/catalogs-v1/world.zh-CN.json`. The catalog already contains
BRGN's registered Chinese name. [Closure and budget](asset-inventory.md#brass-gunner-promotion-2242) ·
[Native reproduction](../application/church-promotion-design.md#brass-gunner-promotion)

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2240/pre-voyage-brn-v1'
uv run python -m sf2tool.remake_retail_assets --root $registeredRetailLibrary `
    --base $base --upstream $pinnedSource --mount $freshAbsolutePackage --family brgn
```

### Master Monk promotion successor

`derived/issue2244/pre-voyage-mmnk-v1` extends the complete BRGN successor with Sarah and
Karna MMNK growth, walking sprites, unarmed and current item26–28 battle bindings, cast and
special-critical animation83. Select its world, main scene, entry and six opening inputs
together; retain the registered prepared retail library. It preserves all predecessor
PGNT/BRN/BRGN and spell content. [Closure and budget](asset-inventory.md#master-monk-promotion-2244) ·
[Consumer acceptance and reproduction](../application/church-promotion-design.md#master-monk-promotion)

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2242/pre-voyage-brgn-v1'
uv run python -m sf2tool.remake_retail_assets --root $registeredRetailLibrary `
    --base $base --upstream $pinnedSource --mount $freshAbsolutePackage --family mmnk
```

Select `derived/issue2244/catalogs-v1/world.zh-CN.json`. The successor preserves the BRN
catalog and adds the registered `name.class/20` translation 武僧. Existing item26–28 names
remain unchanged. `derived/issue2244/review-v1/runner/prepare.py catalog <base-catalog>
<fresh-output> <registered-names-source>` reproduces this from the sole `names.zh-CN.jsonl`.
The production catalog reader and independent native Continue accept it. Generating scratch
was moved aside before the published-package field and battle Continue checks.


#### MMNK campaign with the accepted four-scene opening

The MMNK campaign and [four-scene startup](#four-scene-startup-package-2252) remain separate
runtime selections. Keep the MMNK world, main scene, entry and six opening-battle inputs
selected as above, then add the accepted opening selection and its independent entry catalog:

```powershell
$opening = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2252/pre-voyage-opening-v1'
$env:SF2_PRIVATE_OPENING_CONTENT = Join-Path $opening 'opening/opening.json'
$env:SF2_ENTRY_DISPLAY_CATALOG = Join-Path $opening 'catalogs/entry.zh-CN.json'
$env:SF2_DISPLAY_CATALOG = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2244/catalogs-v1/world.zh-CN.json'
```

`OpeningSequenceReader` reads its own bounded world, scripts, catalog and six inputs beneath
`opening/`; the journey reader consumes the MMNK campaign. Do not replace the MMNK world with
the opening publication's BRGN-based campaign, or replace the MMNK name catalog with its older
campaign catalog. Both registered directories are required for this combined selection. No asset
copy or regenerated campaign is needed: all 174 MMNK campaign paths exist in the accepted
opening publication, and all except the intended world and Battle26 scene differences are
byte-identical, including entry and opening-battle inputs. The original closed MMNK package
remains unchanged; it does not itself contain the independent four-scene payload.

#### Witch Continue entry bank (#2255)

Ordinary Debug selects the Witch bank and its catalog through the [runtime selection](metadata/selection.json).
The accepted Witch bank and its locale overlay are selected together by the
[tracked runtime roles](asset-inventory.md#pre-voyage-package-locations). Its producer still
requires the six-row greeting bank and overlay as inputs. The Atlas entry bank and the
versioned localization source supply that predecessor.

The producer selects 224–226 from the registered `source.jsonl` using the pinned Witch script
and checks both source agreement and accepted translation control topology. It retains the
member-0 token for the existing name resolver and projects the line breaks and waits onto the
shared entry reading control. The existing six greeting rows and all Original/HD images and two
PCM tracks were directly compared with their predecessors. Preparation reads and writes one
10.42 MiB bank of 14 files and reads the existing text corpus once. Use an 11 MiB output allowance
and one serial preparation. The campaign and four-scene packs remain the existing selected directories.
Six-row legacy entry banks retain their direct slot route and the current default selection rule.

To prepare a fresh ignored `$freshAbsoluteBank` with the registered pinned `$pinnedSource`:

```powershell
. ./local/private-inputs.ps1
$locale = Join-Path $env:SF2_SHARED_INPUT_ROOT 'sources/localization/v1'
$atlas = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/gameplay/atlas-v1'
uv run --locked python -m sf2tool.remake_witch_continue --upstream $pinnedSource `
    --corpus (Join-Path $locale 'full-text') --entry (Join-Path $atlas 'entry/entry.json') `
    --catalog (Join-Path $locale 'witch-entry/entry.zh-CN.json') --output $freshAbsoluteBank
```

For the native check, select the complete combination and a fresh namespace under an authorized
external disposable root. Use the [showcase's Map16 v6 witness](../evidence/opening-loop-acceptance.md#launch-and-checkpoints)
as the source for slot 2. In a controlled copy, add `ally-0 = WWWWWWW` to `data.registers.names`
inside the Base64 payload, serialize UTF-8 and update the envelope's uppercase SHA-256. The direct
seed control takes the same bytes from a path outside the selected namespace. The abnormal fixture
uses malformed slot 1 with an admitted backup (the same payload with slot header 1) and slot 2
with a different `data.compatibility.gameplayRevision`. Use an empty separate namespace for the
empty case. Retain a byte copy of the inputs for before/after comparison.

Run the actual Debug adapter with the [locked SDK/settings selection](../development-and-verification.md#locked-net-workflow),
the configured Godot binary, worktree-local APPDATA/LOCALAPPDATA/TEMP and a fresh receipt path.
Set `$case` to `empty`, `valid`, `abnormal` or `seeded` and use language `en` or `zh-CN`
with instant text in the read-only settings file. These checks use the full 1920×1080 headless
Godot canvas and drive Main with mapped keys.

```powershell
$env:SF2_WITCH_CONTINUE_CASE = $case
$env:SF2_WITCH_CONTINUE_LOCALE = $locale
$env:SF2_WITCH_CONTINUE_HERO = 'WWWWWWW'
$env:SF2_WITCH_CONTINUE_OUTPUT = Join-Path $runRoot 'witness.json'
$gameArgs = @('--headless', '--path', (Join-Path $repo 'remake/game'), '--max-fps', '120',
    '--quit-after', '5400', '--log-file', (Join-Path $runRoot 'godot.log'),
    '--script', 'res://probes/engine_witch_continue_observation.gd', '--', '--input-settings', $settings)
if ($case -eq 'seeded') { $gameArgs += @('--load-save', $disposableSeed) }
& $env:GODOT_BIN @gameArgs
```

**Confirmed, bounded in Godot 4.7.2.** Short ordinary startup Skip reached the shared greeting
and expected default in all three occupancy cases. Cancel from the slots returned to Continue
focus. The corrupt slot offered backup recovery; Cancel kept the corrupt primary and backup.
The incompatible slot offered diagnostics. Both ordinary successful runs showed 225 → slot →
226 with the seven-character custom name → 224, with direct English and Chinese bindings.
Recognition occupied one line at 30px in both locales and fit the existing body viewport.
Held Confirm and echo stayed at recognition and the gameplay release barrier. Release returned
usable control. At revision 0 the decoded `data` of each first captured settled field record
equaled the controlled slot's complete `data`, including the visit, party and registers.
Direct seeded Continue bypassed the Witch and captured the same state. The primary and backup
bytes were unchanged. Human rendering and original hardware cadence remain **Unknown**.

The first adapter build failed on the name token's property name. The first ordinary probes
incorrectly used an unchanged revision as the held-input assertion even though settled field
simulation advances revisions. Those receipts are retained. The corrected probe checks the
mapped release barrier, legal landing and continued field control across echo and release.
The first seeded attempt picked a path inside its active namespace and the existing guard
reported `invalid-save-paths`. The corrected seed is outside that namespace.

[Witch lifecycle and slot policy](../application/opening-session-lifecycle.md#witch-continue-dialogue-and-default-selection)

#### Combined resource admission check

A short read-only production-consumer check builds in the explicit checkout's ignored scratch
and admits both selections, both Chinese catalogs, each member's destination growth/book and
current unarmed/fist appearance in every campaign scene. Run from any working directory:

```powershell
. (Join-Path $repo 'local/private-inputs.ps1')
$consumer = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2244/consumer-v1/Verify.ps1'
& $consumer -Repo $repo
```

Require exit0, two member rows with `scenes:26`, and `Independent opening selection and Chinese
catalog: PASS`. This checks production admission, not complete opening playback, title controls
or saved-game continuity. It launches no Godot instance and reads or writes no save slots.
Accepted opening observations and the earlier MMNK native/cold-save results retain their own
scope and failures; updating the checkout does not turn the retained exit crash into a pass.

### Sorcerer promotion successor

`derived/issue2266/pre-voyage-sorc-v3` extends the complete MMNK selection with Kazin
and Tyrin SORC growth, walking and battle bodies, cast poses and the four summon icons.
The shared rod mount also refreshes the existing WIZ/VICR bindings for Iron Rod, Flail
and Indra Staff, with explicit unarmed bodies. `consumer-v2/Verify.ps1` under the same
issue checks all six member/class appearance sets across 26 loaded scenes.
Use its world, main scene and six opening inputs with
`derived/issue2266/catalogs-v1/world.zh-CN.json`. Keep #2252 opening and #2255 Witch entry
selected separately as above. The catalog adds only accepted class18 and spell29–32 names.
This older promotion package used modern summon fallback. The
[DAO successor](#dao-successor-2310) adds both DAO ranks; the [APOLLO successor](#apollo-successor-2312)
adds both APOLLO ranks. The [NEPTUN successor](#neptun-successor-2313) adds its body/water
composition; the [ATLAS successor](#atlas-successor-2314) adds its two-pose invocation.

```powershell
. ./local/private-inputs.ps1
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2244/pre-voyage-mmnk-v1'
uv run python -m sf2tool.remake_retail_assets --root $registeredRetailLibrary `
    --base $base --upstream $pinnedSource --mount $freshAbsolutePackage --family sorc
```

Use `sf2tool.remake_private_package` to copy its complete dependency closure to a fresh
registered `derived/` selection. [Budget and validation](asset-inventory.md#sorcerer-promotion-2266)
· [Ordinary reproduction and limits](../application/church-promotion-design.md#reproduce-the-sorc-consumers).

## BUFF successor

`derived/issue2282/buff-v1` is the complete equipment-v2 successor for BUFF playback.
It contains175 files/368,493,196 bytes. Only `world.json` gains the97 audio binding and
`audio-61-cb/command-61-timer-cb.wav` is added; all other files byte-match equipment-v2.
The world apart from that audio row is semantically identical. Existing maps, class/equipment
HD, old effects and music remain intact. Opening #2252, Witch #2255 and world catalog #2266
remain independent selections. No new raster group exists for BUFF.

Reproduce using already registered prepared inputs and a fresh absolute output:

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --locked python -m sf2tool.remake_retail_assets `
  --root (Join-Path $derived 'issue2001/retail-v1') `
  --base (Join-Path $derived 'issue2274/equipment-v2') `
  --mount $freshAbsolutePackage --family sfx --command 97
```

The existing mount verifies prepared WAV identity, copies the complete dependency closure,
adds the one binding and publishes atomically. The pilot reads5865 stereo samples (23,504-byte
WAV), before the full serial publication. One output/staging each below500MiB and2GiB total
scratch/RAM are the initial budget. No source capture or new producer code is required.
[Mechanism, native reproduction, retained failures and Unknowns](../godot/spell-presentation.md#buff-family-playback-2282)
retain the acceptance boundary. Private media is not redistributable.

The BUFF group is optional. Existing equipment-v2 and authored content without selectable77/97
retain their prior support messages/Item presentation; buff-v1 enables the original path.
Declared corrupt media still fails admission. See the [complete-group boundary](../godot/spell-presentation.md#buff-optional-group-compatibility).


### Flame successor #2293

`derived/issue2293/flame-v1` extends the complete #2282 buff-v1 without dropping any earlier
resource. Reuse #2001 `retail-v1` and the fixed clean graphics-filter checkout. Select world,
main scene and six opening inputs from the same successor; #2252 opening, #2255 Witch/entry
catalog and #2266 world catalog remain independent selections.

```powershell
uv run --with pillow python -m sf2tool.remake_retail_assets --root $library --base $base `
  --mount $freshOutput --family flame-family --gfx-root $gfxRoot
```

This existing package publisher mounts prepared tileset2 in every admitted scene, retaining
all existing HD entries and deriving missing variants through the accepted filter. No ROM
capture or new asset acquisition is needed. Omitting `--gfx-root` publishes Original only
while retaining previous HD; it does not claim complete new HD groups. The reader's actual
8MiB scene limit governs this full-roster package. The representative Battle26 pilot was
6,439,791 bytes and0.31s. Full publication has175 files and369,705,232 bytes;26 scenes change,
all other files compare byte-for-byte, and every old scene field/resource is preserved.
The size increase is1,212,036 bytes over buff-v1. Work is linear, serial, below the500MiB
successor limit. Same-volume unchanged-file hardlinks are read-only; rewritten scenes receive
independent files. Publication closes paths before moving producer scratch aside.

The independent private `review-v2` production reader checks all26 scene mounts, source selectors,
neutral/Fire rules and equipment bindings. The [spell owner](../godot/spell-presentation.md#flame-family-playback-2293)
owns ordinary playback and retained failed runs. Native startup measured about1.30GB peak working
set and direct reader about0.96GB, below the2GiB plan. These are observed host peaks, not a
portable performance promise. Public files contain no generated graphics or private saves.

### FREEZE successor #2298

`derived/issue2298/freeze-v1` extends complete #2293 flame-v1 using prepared #2001 tileset11
and sound120. It contains176 files/370,341,646 bytes (636,414 bytes above the predecessor).
All26 scenes gain14 Original/4x HD rasters; world gains one prepared120 cue and one94,388-byte
PCM file is added. Every old scene field/raster, audio row and all other files are preserved.
Opening #2252, Witch #2255 and catalog #2266 remain independent. Select world, main scene
and all six opening inputs from this successor together.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --with pillow python -m sf2tool.remake_retail_assets `
  --root (Join-Path $derived 'issue2001/retail-v1') `
  --base (Join-Path $derived 'issue2293/flame-v1') `
  --mount $freshAbsolutePackage --family freeze-family --gfx-root $pinnedGraphicsCheckout
```

The fixed clean graphics checkout remains172662709b31cb296b55f51894754ce810c30f6c.
Publish with the existing `sf2tool.remake_private_package` world/scene/entry/opening-inputs
closure, then move producer scratch aside before using the published selection. Missing
`--gfx-root` retains old HD but creates no new FREEZE HD claims. A missing HD member falls
back to all14 Original frames; missing Original groups retain the existing placeholder.

The Battle26 pilot is6,460,629 bytes/0.21s; complete publication is linear/serial, below500MiB.
One successor plus staging and retained small pilot fits the2GiB scratch plan. Production
reader peak was about0.96GB. The representative HD native process plus console launcher peaked at1,300,553,728 bytes;
final receipts are retained in review-v1. These are host measurements, not portable bounds.
The reader admits26 scenes and all seven restored records; ordinary playback is bounded
separately by the [spell owner](../godot/spell-presentation.md#freeze-reproduction-and-limits).
No original media, private saves or machine paths enter Git.

### Lightning successor #2301

`derived/issue2301/bolt-v1` is the complete #2298 freeze-v1 successor:177 files,
373,560,638 bytes. All26 scenes add30 Original and30 4x HD rasters; world adds prepared
sound119 and its PCM file. Every predecessor scene field/raster/audio row and every other
file compares unchanged. Independent #2252 opening, #2255 Witch and #2266 catalog remain
selected. The [mechanism owner](../godot/spell-presentation.md#lightning-family-playback-2301)
records source operands, native acceptance, failures and reproduction.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --with pillow python -m sf2tool.remake_retail_assets `
  --root (Join-Path $derived 'issue2001/retail-v1') `
  --base (Join-Path $derived 'issue2298/freeze-v1') `
  --mount $freshAbsolutePackage --family bolt-family --gfx-root $pinnedGraphicsCheckout
```

Use the existing package closure publisher for world/main scene/entry/opening inputs.
The graphics checkout remains fixed at172662709b31cb296b55f51894754ce810c30f6c. Omitting
`--gfx-root` creates no new HD group; incomplete HD falls back atomically to Original.
The serial pilot took0.37s, peaked at76,832,768 bytes and produced a6,580,921-byte scene,
retaining all older fields/resources. Complete publication remains below500MiB; final
published reader peak is about0.96GB and representative native peak about1.33GB, within
the2GiB allowance. No new capture, ROM acquisition or asset platform is used.

### Wind successor #2306

`derived/issue2306/blast-v1` extends complete #2301 bolt-v1:177 files /374,345,380 bytes.
All26 scenes contain23 Original/HD BLAST resources. Direct decoded comparison preserves
every previous field/resource; all other files, including audio114, are byte-identical.
Independent #2252 opening, #2255 Witch and #2266 catalog remain selected.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --with pillow python -m sf2tool.remake_retail_assets `
  --root (Join-Path $derived 'issue2001/retail-v1') `
  --base (Join-Path $derived 'issue2301/bolt-v1') `
  --mount $freshAbsolutePackage --family blast-family --gfx-root $pinnedGraphicsCheckout
```

The fixed graphics checkout remains172662709b31cb296b55f51894754ce810c30f6c.
The existing publisher preserves the dependency closure and copies scenes before mutation.
One-scene pilot is6,613,630 bytes,0.29s and76,963,840-byte peak. Publication is linear,
serial and below500MiB; reader/native peaks stay within2GiB. Omitting graphics source adds
no new HD claims. [Wind reproduction](../godot/spell-presentation.md#wind-reproduction-and-acceptance)
uses published inputs without producer scratch and records failures and acceptance limits.

### ABSORB resource reuse #2309

Select the unchanged complete `derived/issue2307/desoul-v1`, retaining independent
#2252 opening, #2255 Witch and #2266 catalogs. It already contains all four admitted
SPOIT equipment appearances and eleven warp89 audio variants, with explicit Timer203
default. The pilot validates its24,093 stereo samples /96,372 decoded bytes; actual
allied scene playback requests Timer198 and selects that default. Exact hardware
sound cadence is not claimed. No new media, capture, mount or raster group is needed.

The package remains178 files /374,714,972 bytes, below the500MiB successor budget.
`derived/issue2309/review-v1` contains controlled inputs, production-reader and native
launchers; `verification-v1` retains checks and completed failures. Review publication
is separate from the immutable #2307 content. The reader checks104 SPOIT definitions
and702 equipment appearances over26 scenes. Native Original/HD, Reduced Flash/held
speed and body-group fallback retain their [playback boundary](../godot/spell-presentation.md#absorb-reproduction-and-verification).

### Kiwi successor #2316

Complete `derived/issue2316/kiwi-v1` inherits accepted AQUA and contains181 files /
380,806,286 bytes, adding269,724 bytes across26 scenes. Every inherited JSON value and
unaffected file compares equal. Registered tileset1 already has all35 tiles; the producer
adds six Original and six HD growth resources without replacing the retained legacy pair.
World command92/CB already exists. No body, audio or source acquisition is needed.
Independent #2324 opening, #2255 Witch and #2266 catalogs are selected separately.

Before processing, the estimate was under1MiB added, one scene/raster bank in memory,
O(package bytes plus frame pixels), one serial run, complete output below500MiB and
scratch/peak below2GiB. The one-scene pilot takes0.14s and adds10,374 bytes; reader peak
is882MB and native peak below1.33GB. Replan on budget failure. Published-input acceptance
runs with the producer package path absent; one fallback only rewrites Battle26's HD group.

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
# Select the fresh Aqua output from the preparation command below as $aquaPackage.
uv run --locked --with pillow==13.0.0 python -m sf2tool.remake_retail_assets `
  --root $library --base $aquaPackage --mount $freshAbsolutePackage `
  --family kiwi-family --gfx-root $env:SF2_GRAPHICS_ROOT
```

Use a fresh worktree-local ignored destination and the registered graphics selection.
The Aqua recipe below prepares the same complete input as the earlier publication.
`derived/issue2316/review-v1` owns reader/Boot recipes and retained receipts.
[Source correction, results and limits](../godot/spell-presentation.md#kiwi-converted-attack-playback-2316).

### AQUA successor #2315

Complete `derived/issue2315/aqua-v1` consumes accepted #2314 and prepared
`derived/issue2001/retail-v1`, with graphics commit
`172662709b31cb296b55f51894754ce810c30f6c`. It contains181 files /380,536,562 bytes:
26 scenes gain14 Original and14 HD resources, and world audio gains prepared104/CB.
Every inherited JSON value and unaffected file compares equal. Independent #2252 opening,
#2255 Witch and #2266 catalogs remain selected. No new audio capture is required.

The one-scene pilot adds17,648 bytes in0.15s with55.5MB peak working memory. Serial
publication takes1.52s and adds572,399 bytes overall. Processing is O(package bytes plus
frame pixels), with one scene and raster bank live. Before scaling the estimate was under
30MiB added,1GiB working memory/scratch and minutes; publication remains below500MiB.
The hard scratch/peak limit is2GiB. Reader peak is below900MB and native peak below1.31GB;
replan if these limits fail. One fallback rewrites only Battle23's rank2 HD group and
links unchanged immutable inputs. The producer package path is absent during acceptance.

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/gameplay/atlas-v1'
$aquaPackage = Join-Path (Get-Location).Path ('local/aqua-' + [guid]::NewGuid().ToString('N'))
uv run --locked --with pillow==11.3.0 python -m sf2tool.remake_retail_assets `
  --root $library --base $base --mount $aquaPackage `
  --family aqua-family --gfx-root $env:SF2_GRAPHICS_ROOT
if ($LASTEXITCODE) { throw 'Aqua preparation failed' }
```

A fresh checkout-local build reproduced all 181 published files byte for byte with the
pinned graphics checkout. Select `$aquaPackage` when preparing Kiwi or reproducing the Aqua
selection. The [asset inventory](asset-inventory.md#pre-voyage-package-locations) records both producer inputs. Published `derived/issue2315/review-v1` holds reader and ordinary-input
recipes and retained receipts. Boot stages writable files beneath
the selected worktree and requires an explicit disposable save root.
[Source, caller distinction, checks and failures](../godot/spell-presentation.md#aqua-family-playback-2315).

### ATLAS successor #2314

Complete `derived/issue2314/atlas-v1` contains180 files /379,964,163 bytes, consuming
accepted `derived/issue2313/neptun-v1`, prepared `derived/issue2001/retail-v1` and graphics
commit`172662709b31cb296b55f51894754ce810c30f6c`. Only26 scene documents change, adding
two128×128 Original poses and two4× HD companions. Every inherited value and other file
compares equal, including world/audio, actor/equipment/terrain and earlier effects.
Cues77/83/84 already exist in both the package and SessionAudio's slot table. Retain
independent #2252 opening, #2255 Witch and #2266 catalogs.

The representative pilot takes0.25s and adds21,472 bytes. Generation adds558,272 bytes
over26 scenes, under the500MiB successor budget. Work is linear in package bytes plus
frame pixels, with one scene and raster bank in memory:131,072 raw RGBA bytes and
2,097,152 at4× before PNG encoding. Serial scratch/peak budget is2GiB; generation budget
is2 minutes, reader and each native witness1 minute. Replan if these limits fail. Observed
reader/native peaks are below1.4GB; a single fallback changes only Battle26's HD group.
The producer package was moved to immutable publication before reproduction; no consumer
uses pilot/scratch files. [Source, results and retained failures](../godot/spell-presentation.md#atlas-family-playback-2314).

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2313/neptun-v1'
uv run --with pillow python -m sf2tool.remake_retail_assets --root $library --base $base `
  --mount $freshAbsolutePackage --family atlas-family --gfx-root $env:SF2_GRAPHICS_ROOT
```

Generate to fresh checkout-local ignored output. Published `derived/issue2314/review-v1`
contains the production reader and ordinary-input launch recipes; its Boot stages mutable
state beneath the selected checkout and requires an explicit disposable save root.

### NEPTUN successor #2313

Complete `derived/issue2313/neptun-v1` contains180 files /379,405,891 bytes. It consumes
complete `derived/issue2312/apollo-v1` (178 files,
378,106,698 bytes), prepared `derived/issue2001/retail-v1` effects and registered graphics
commit `172662709b31cb296b55f51894754ce810c30f6c`. Independent #2252 opening, #2255 Witch
and #2266 catalogs remain selected. The26 scenes add ten
Original and ten HD rasters: two128×128 bodies, three16×24 rising frames, one16×32 and
one16×16 rain part, and three16×16 sizzles. Two already prepared audio cues108/115 are
mounted through the existing publisher;
77/110 remain inherited. The world audio list gains only those two rows.

Serial pilot:0.30s and39,863 added bytes for one scene, with direct equality of all
inherited values. The raster increase is1,036,438 bytes; reused audio plus binding rows add262,755 bytes.
Output remains below500MiB, scratch and peak RAM below2GiB, with no more than one successor/fallback copy per purpose.
Processing is linear in frame pixels plus package bytes, holding one scene and the small
raster bank; raw rasters occupy141,824 RGBA bytes and4× HD2,269,184 bytes before PNG encoding.
Budget full generation under2 minutes, reader under1 minute and each native witness under1
minute; replan if measured limits differ. Producer scratch is unavailable for published
reproduction. Direct comparison preserves every inherited scene/world value and all old files;
only26 scene documents, the world audio list and two reused WAV files differ. The final
production reader and native witnesses stay below1.4GB peak. Recorded setup failures are
retained with the [source and acceptance](../godot/spell-presentation.md#neptun-family-playback-2313).

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2312/apollo-v1'
uv run --with pillow python -m sf2tool.remake_retail_assets --root $library --base $base `
  --mount $freshAbsolutePackage --family neptun-family --gfx-root $env:SF2_GRAPHICS_ROOT
```

Use the registered graphics commit above; generate into a fresh checkout-local ignored path.
The published review's `Verify.ps1` and `Boot.ps1` consume the complete package without the
producer scratch or generation helpers.

### APOLLO successor #2312

Complete `derived/issue2312/apollo-v1` extends accepted `derived/issue2310/dao-v2`.
It contains178 files /378,106,698 bytes; only26 scene documents change. All inherited
world, audio, actor, terrain, equipment and effect values remain equal. Preserve independent
#2252 opening, #2255 Witch and #2266 catalogs. Each scene adds13 Original rasters and
13 optional HD companions: three128×128 body poses, two16×16 mouth frames and four
frames each of16×16 and32×32 flame particles. Existing77/116/118 audio is sufficient.

The serial estimate was less than5MB additional data, complete output below500MiB and
scratch/peak below2GiB. The one-scene pilot took0.35 seconds and added48,624 bytes;
whole-package increase is1,264,224 bytes. Complexity is linear in frame pixels plus
package bytes, holding one scene and the small raster bank. The published reader peaks
below1GiB and native witnesses below1.3GiB. Producer scratch plus fallback/review outputs
stay below2GiB. Replan if these bounds are exceeded; no full frame collection is used.
The extracted shared invocation decoder reproduces existing DAO Original rasters unchanged.

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2310/dao-v2'
uv run --with pillow python -m sf2tool.remake_retail_assets --root $library --base $base `
  --mount $freshAbsolutePackage --family apollo-family --gfx-root $env:SF2_GRAPHICS_ROOT
```

Use the registered graphics checkout `172662709b31cb296b55f51894754ce810c30f6c`.
[Source, delivery and reproduction](../godot/spell-presentation.md#apollo-family-playback-2312)
own palette/bank mapping, deterministic choices, published-input witnesses and Unknowns.

### DAO successor #2310

The complete `derived/issue2310/dao-v2` extends accepted `derived/issue2307/desoul-v1`
without changing world, audio, actors, terrain, equipment or prior effect rasters.
Retain independent #2252 opening, #2255 Witch and #2266 catalogs. The existing producer
mounts eight128×128 invocation0 frames and three DAO rock rasters, with atomic optional
HD companions, into all26 scenes. No new schema or media capture is needed: the prepared
retail library supplies the tiles, and the predecessor already has selectable93/118 audio.

The serial pilot budget was11 Original plus11 HD rasters, approximately2.2MB added
across26 scenes, a complete successor below500MiB and scratch/peak below2GiB. One-scene
derivation took0.62 seconds and added83,959 bytes before the palette-offset correction.
Work is linear in frame pixels plus the existing scene/package bytes; one scene and the
small frame bank are held at a time. The corrected successor has178 files /376,842,474
bytes, with only26 scene files changed. Native peak was1,351,806,976 bytes and
producer/review scratch405,863,110 bytes. Replan if those limits are exceeded.

From the checkout, use registered roots and a fresh absolute output:

```powershell
. ./local/private-inputs.ps1
$library = Join-Path $env:SF2_SHARED_INPUT_ROOT 'media/retail-preparation/v1/retail-v1'
$base = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2307/desoul-v1'
uv run --with pillow python -m sf2tool.remake_retail_assets --root $library --base $base `
  --mount $freshAbsolutePackage --family dao-family --gfx-root $env:SF2_GRAPHICS_ROOT
```

The graphics checkout is pinned to `172662709b31cb296b55f51894754ce810c30f6c`.
The earlier `dao-v1` output failed with an invocation palette offset error. Use `dao-v2` for
reproduction and keep the rejection receipt with the owning review.
[Source, delivery and ordinary-input reproduction](../godot/spell-presentation.md#dao-family-playback-2310)
separates original operands, deterministic cosmetic motion and remaining Unknowns.

### DESOUL successor #2307

The same complete package also supplies [ABSORB](#absorb-resource-reuse-2309); no
duplicate successor is required for that no-tile mechanism.

The registered `derived/issue2307/desoul-v1` is a complete successor to
`derived/issue2306/blast-v1` with 178 files and 374,714,972 bytes. The publisher
adds four Original and four HD rasters to the 26 scene projections and mounts
strike cue 106 in world audio. The closure comparison preserves the prior payloads
and links. Select world, main scenes and the six opening inputs together, retaining
independent #2252 opening, #2255 Witch and #2266 catalog selections.

```powershell
. ./local/private-inputs.ps1
$derived = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived'
uv run --with pillow python -m sf2tool.remake_retail_assets `
  --root (Join-Path $derived 'issue2001/retail-v1') `
  --base (Join-Path $derived 'issue2306/blast-v1') `
  --mount $freshAbsolutePackage --family desoul-family --gfx-root $pinnedGraphicsCheckout
```

Use the graphics checkout at `172662709b31cb296b55f51894754ce810c30f6c` for the 4×
group. The single-scene Battle26 pilot added 12,158 bytes in about 0.18 s. The
serial publication fit the 500 MiB package budget. Published-reader and independent
native peaks stayed below 2 GiB. The production reader and headless observation
launchers consume the registered package with producer scratch unavailable.
[Playback reproduction and limits](../godot/spell-presentation.md#desoul-reproduction-and-verification)
