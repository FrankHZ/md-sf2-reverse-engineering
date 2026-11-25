# Godot design preview

Click a tab or press F1–F10. Each sample opens in the production `Main.tscn` host.
The toolbar selects the entry, reset and review preferences. The game owns all menus,
dialogue, choices, transactions and their return paths. Enter / A confirms. Esc, B or a
right-click returns when the current game page admits Cancel. L switches Chinese and English.

## Scene ownership

Use the [scene review](../../../docs/godot/ui-scene-review.md#scenes-in-the-playable-demo) for
the subpage paths, visual checks and connected-game fallbacks.

| Tab | Production entry and sample |
| --- | --- |
| F1 Witch | `SessionEntryView` at the actual title. Start opens the Witch greeting and menu. New Game, name, final Start and Continue use the actual entry host and one isolated preview file. After creating a journey, reopen Witch to see Continue selected by default. With the current private entry bank, acknowledge the question, select the preview slot, then acknowledge recognition and farewell. Settings opens from the Witch's own action. |
| F2 Dialogue | **Reading** runs the original practice invitation. Click **Show Yes / No** to keep its two reading waits and open the production choice before closing the question. Confirm through the waits, then use Left/Right and Confirm or press Esc for No. The choice closes and walking resumes. The authored speaker is portraitless. |
| F3 Church | `ChurchServiceView` enters the existing practice Church program from the same authored package. Choose Raise to read or decline the treatment quote, or see Promotion refusal on the practice party. Connected Record and admitted Promotion have separate routes in the scene review. |
| F4 Battle | `BattleSessionView` opens Practice Yard. Move Down, let the step settle and confirm. Each deep-blue Main row contains its icon and label. Main Back reselects a legal preview immediately with the original movement budget. H opens Magic from Main. Select MEND and GUARD A for the real friendly target and production recovery scene. |
| F5 Members | **Supplies** opens Member in the shared shop session and shows the unavailable Prepare outcome. Click **Show Preparation** for the `localization-pilot` party. On Medic A, Confirm **Prepare…** → **Give / exchange** → **Practice Leaf** to see the item and recipient pages. Esc returns through the item, command list and retained Member. Magic opens its empty choice page in this package. |
| F6 Shop | `ShopServiceView` opens through ordinary interaction with the `shop-supplies` merchant. Buy or sell through the actual stock, slot, quote and receipt. |
| F7 Field | The production field command list opens in the same shop/supplies area, with an authored description on the cell facing the merchant. Choose Search to read it through the existing field reader. **Show Caravan** opens the [registered service approach](#caravan-sample). |
| F8 Inspect | `BattleInspectionView` opens at the acting unit in Practice Yard. Inspect the enemy at (8, 5) for its red movement field. Confirm an empty cell → Member. Switch rows to read live details, focus Items and select a slot. Back returns to Options, then Cursor. The field and battle pages share layout and browsing. Battle offers Back only. |
| F9 Results | Select Victory, Defeat or Retreat in the toolbar. The current sample enters its actual before-battle message. Advance to Movement, confirm a stop, then use Attack for Victory, Stay for Defeat, or Magic → EGRESS for Retreat. The normal presenters own messages, rewards, departure and field return. |
| F10 System | The production System Menu covers the current eligible session. Without a session (including the title), F10 opens the authored field sample first. Settings categories, Help and Change controls are reachable. Save is subject to ordinary admission; the authored sample exposes its refusal. Cancel returns to the covered game phase. |

For a larger F8 roster, append `-- --inspect --authored-package <absolute member-large.json>`
using the [14-member recipe](../../../docs/godot/battle-ui-composition.md#reproduce-the-shared-roster).
Review the production Member pages at 1600×900 and 1920×1080.
Use the [dedicated preview visual review rules](../../../docs/development-and-verification.md#dedicated-ui-preview-visual-review) for necessary local stills.

Switch directly among F5 Supplies, F6 and F7 to keep the same live party and inventories.
Selecting Preparation starts a fresh sample. F6 or F7 from Preparation starts a fresh
Supplies session, and F5 then opens Member in that session. F6 opens the shop automatically
while the party faces the merchant. If the party has walked elsewhere, face the merchant again
or press F6 again for a fresh sample. Re-select the current tab to restart its sample. F10 toggles
the eligible live host menu.
Close an input-owning field page before opening it. The [preview gaps](../../../docs/godot/ui-scene-review.md#preview-gaps-and-targeted-follow-ups)
cover remaining connected campaign variants.

The production viewport keeps the 1920×1080 canvas and fits below the toolbar. The preview
translates pointer input into that viewport. Main panels, menus, dialogue, tooltips and status
cards use the shared gold art from [StoryTheme](../../src/UI/StoryTheme.cs). Each presenter
keeps its own layout and content padding.

## Run from the repository root

Load the checkout's [registered local environment](../../../../docs/operations/local-private-inputs.md)
and build the adapter in Debug using the [locked SDK workflow](../../../docs/development-and-verification.md#locked-net-workflow).
Import the project once. Use isolated app-data and temp directories for the child process.
For the F1 full lifecycle, select the registered entry, world, scene index and opening inputs
using the [ordinary presentation resource route](../../../docs/content/presentation-assets.md#final-publication-and-ordinary-selection).
The authored tabs use their own content and available resources.

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
& $env:GODOT_BIN --path $project res://probes/previews/DesignPreview.tscn
```

Pass `-- --dialogue`, `-- --church`, `-- --battle`, `-- --members`, `-- --shop`,
`-- --field`, `-- --inspect` or `-- --system` for direct entry. `-- --dialogue-choice` and
`-- --prepare` select the alternate samples directly. For localized F2 script text, set
`SF2_DISPLAY_CATALOG` to the absolute path of
[`localization-pilot.zh-CN.json`](../../../content/authored/localization-pilot.zh-CN.json) in the
launching process. L switches the UI language. F9 starts on Victory with `-- --results`. Select
Defeat or Retreat directly with `-- --defeat` or `-- --retreat`. F1 uses one explicit save file in a
fresh `user://design-preview` directory. The preview keeps preferences in memory. The public export excludes `probes/`.

For direct checks, inspect the actual mounted `GameRoot`, field and battle views and their
`ReadObservationJson` projections. The toolbar's and the game's panel style boxes use the
same texture. Check that a purchase changes the party seen in F5, inspection returns through
the right parent, F10 restores the covered session, and outcome waits reach actual field
control. The scene review records bounded visual findings by page. Ordinary-game reading comfort and physical-device review remain **Unknown**.

## Numbered saves and native window recovery

Use the [dedicated Main launch and reset recipe](../../../docs/godot/host-ui.md#dedicated-numbered-save-and-native-recovery-demo)
for Witch Continue and field Save on page 2, slot protection and backup recovery, and window
recovery while Save is open. The launch opens a native root window with a fresh disposable copy
of numbered history. The F1–F10 sample canvas has a fixed internal client.

## Caravan sample

From the repository root, load the registered inputs and use the imported Debug adapter as above.
Select the matched world, scene and opening inputs from the
[registered #2177 package](../../../docs/content/asset-inventory.md#battle-raster-variant-binding-2177).
The Map26 approach and reviewed catalog come from the registered #1870 bundle.

```powershell
. ./local/private-inputs.ps1
$package = Join-Path $env:SF2_SHARED_INPUT_ROOT 'derived/issue2177/pre-voyage-battle-v1'
$caravan = Join-Path $env:SF2_SHARED_INPUT_ROOT 'handoffs/issue1870'
$env:SF2_PRIVATE_EXPLORATION_CONTENT = Join-Path $package 'world.json'
$env:SF2_PRIVATE_BATTLE_SCENE_CONTENT = Join-Path $package 'main-scenes.json'
$env:SF2_PRIVATE_ENTRY_CONTENT = Join-Path $package 'entry/entry.json'
foreach ($file in Get-ChildItem -LiteralPath "$package/opening-inputs" -Filter 'SF2_PRIVATE_*.json' -File) {
    [Environment]::SetEnvironmentVariable($file.BaseName, $file.FullName, 'Process')
}
$env:SF2_DISPLAY_CATALOG = Join-Path $caravan 'catalog02/world.zh-CN.json'
$env:SF2_CARAVAN_PREVIEW_SEED = Join-Path $caravan 'slots/service-en.json'

$run = New-Item -ItemType Directory -Path ("local/preview-caravan-" + [Guid]::NewGuid().ToString('N'))
$env:APPDATA = (New-Item -ItemType Directory -Path "$($run.FullName)/appdata").FullName
$env:LOCALAPPDATA = (New-Item -ItemType Directory -Path "$($run.FullName)/localappdata").FullName
$env:TEMP = (New-Item -ItemType Directory -Path "$($run.FullName)/temp").FullName
$env:TMP = $env:TEMP
$project = (Resolve-Path -LiteralPath 'remake/game').Path
& $env:GODOT_BIN --path $project --resolution 1600x900 res://probes/previews/DesignPreview.tscn -- --caravan
```

Use `service-zh-CN.json` for the corresponding Chinese name seed. Omit `-- --caravan` to enter via
**F7 Field → Show Caravan**. Use Confirm to Talk to the NPC Bowie is facing.
For representative subpage paths, see [Caravan pages](../../../docs/godot/ui-scene-review.md#caravan-service-pages).
Press F7 again to reload the read-only approach. Each Caravan load uses a fresh preview save
file, separate from F1. **Show Field** returns to the authored F7 sample. Switching from Caravan
to F5 or F6 starts the authored supplies session.
