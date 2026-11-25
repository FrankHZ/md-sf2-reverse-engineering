# Presentation Assets and Private Preparation

Content owns external asset identity and prepared resource admission. The ordinary runtime consumes
prepared private worlds/scene documents; it does not reopen a ROM or reconstruct a legacy Godot
asset catalog. [Profiles and trust](profiles-and-trust.md) owns runtime admission and redistribution.
[Godot](../godot/presentation.md) owns disposable rendering, not source provenance.

The [asset inventory](asset-inventory.md) routes each resource family from its source and prepared
version to the registered selection, actual consumer and bounded evidence. Use the
[registered selections](asset-inventory.md#registered-selections) for current package identity.

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

Private source art/audio and accepted asset Git objects own material identity. A generated cache,
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

The tracked layout is:

| Path | Authority |
| --- | --- |
| `source/` | original art/audio and other input material in its admitted source form |
| `masters/` | editable HUD SVG and other authored masters |
| `runtime/` | reviewed runtime-ready 2x/4x textures, UI, fonts, music, and sound assets |
| `manifests/` | local identity, semantic mapping, derivation, import policy, and code-compatibility records |

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
The following selects the registered handoff inputs for the first publication. A second command
checks the complete file set and bytes. To copy only from the stable publication, change the four
source selections to the same files/directories under `$stable` and select a new destination.

```powershell
. ./local/private-inputs.ps1
uv sync --locked
$handoffs = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs'
$base = Join-Path $handoffs 'issue1966/content/package'
$opening = Join-Path $handoffs 'issue1857'
$stable = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2001/issue1966-original'
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
`-Upstream`, `-Listing`, `-Library` and `-Package` paths for installations with different layouts.
The existing `issue1966-original` package and its retail source CSV are the baseline selections.
The local-only asset Git records the reviewed version in `manifests/retail-v1.json`.
`runtime/retail-v1` is directly selectable as `-Library` for Mount. Its matching
`source/retail-v1` subtree holds the command captures and driver boundaries at the same relative
family/command paths. For production recovery, copy both subtrees into a new registered derived
library directory, retaining the file layout. The separately versioned base package still provides
the existing campaign consumers and already admitted audio variants.

```powershell
$repo = (Get-Location).ProviderPath    # start here in the selected code checkout
$run = Join-Path $repo 'tools/Prepare-PrivateAssets.ps1'
& $run -Repo $repo -Operation Prepare -Family All
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

To regenerate coverage and assemble the supported first-consumer packages, select `$assetRoot`
as the absolute path to the private asset Git checkout and `$pinnedSource` as the absolute
source checkout at the revisions named in the
[asset inventory](asset-inventory.md#retail-original-preparation).

```powershell
. (Join-Path $repo 'local/private-inputs.ps1')
$root = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2001'
$runtime = Join-Path $assetRoot 'runtime/retail-v1'
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
    -Package "$progression/pldn-v1" -Upstream $pinnedSource -Destination "$progression/wiz-$token"

# Add the three GLDT/BDBT progressions and appearances to the registered WIZ package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family melee `
    -Package "$progression/wiz-v1" -Upstream $pinnedSource -Destination "$progression/melee-$token"

# Add the three SNIP/BWNT progressions and appearances to the registered melee package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family ranged `
    -Package "$progression/melee-v1" -Upstream $pinnedSource -Destination "$progression/ranged-$token"

# Add the two VICR progressions and appearances to the registered ranged package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family vicar `
    -Package "$progression/ranged-v1" -Upstream $pinnedSource -Destination "$progression/vicar-$token"

# Add the WFBR and PHNX progressions and appearances to the registered Vicar package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family innate `
    -Package "$progression/vicar-v1" -Upstream $pinnedSource -Destination "$progression/innate-$token"

# Add Slade's NINJ progression and appearances to the registered innate package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family ninja `
    -Package "$progression/innate-v1" -Upstream $pinnedSource -Destination "$progression/ninja-$token"

# Add Kiwi's MNST progression and appearances to the registered Ninja package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family monster `
    -Package "$progression/ninja-v1" -Upstream $pinnedSource -Destination "$progression/monster-$token"

# Add the prepared sound-wave overlay to the registered Monster package.
& $run -Repo $repo -Operation Mount -Library $runtime -Family phoenix-wave `
    -Package "$progression/monster-v1" -Destination "$progression/phoenix-wave-$token"

# Bind Indra Staff and the after-break unarmed appearances for WIZ and VICR.
$equipment = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue1952'
& $run -Repo $repo -Operation Mount -Library $runtime -Family indra `
    -Package "$progression/phoenix-wave-v1" -Destination "$equipment/indra-$token"
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

For Bedoe rescue, select the registered `derived/issue2001/priority-v1` package under the
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
record the accepted caller boundary and pending independent acceptance.

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
