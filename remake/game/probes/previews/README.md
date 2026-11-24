# Godot design preview

Click a tab or press F1–F10. Each sample opens in the production `Main.tscn` host.
The toolbar selects the entry, reset and review preferences. The game owns all menus,
dialogue, choices, transactions and their return paths. Enter / A confirms. Esc, B or a
right-click returns when the current game page admits Cancel. L switches Chinese and English.

## Scene ownership

| Tab | Production entry and sample |
| --- | --- |
| F1 Witch | `SessionEntryView` at the actual title. Start opens the Witch greeting and menu. New Game, name, slot and Continue use the real host and an isolated preview save. Select the registered entry and opening package to exercise this route. |
| F2 Dialogue | The hosted `ExplorationDialogueView` reads the invitation in `localization-pilot`. Advance and choose a response with the ordinary dialogue input. |
| F3 Church | `ChurchServiceView` enters the existing practice Church program from the same authored package. Choose a real service, read a refusal or quote, or return with Cancel. |
| F4 Battle | `BattleSessionView` opens Practice Yard. Move Down, let the step settle and confirm. Each deep-blue Main row contains its icon and label. Main Back reselects a legal preview immediately with the original movement budget. H opens Magic from Main. Select MEND and GUARD A for the real friendly target and production recovery scene. |
| F5 Members | Field Party opens the shared roster and detail page with records, gold and Prepare. The public shop sample reports unsupported field preparation. The [connected field observation](../../../docs/godot/battle-ui-composition.md#reproduce-registered-portraits) exercises actual Prepare and Back. |
| F6 Shop | `ShopServiceView` opens through ordinary interaction with the `shop-supplies` merchant. Buy or sell through the actual stock, slot, quote and receipt. |
| F7 Field | The production field command list opens in the same shop/supplies area, with an authored description on the cell facing the merchant. Choose Search to read it through the existing field reader. |
| F8 Inspect | `BattleInspectionView` opens at the acting unit in Practice Yard. Inspect the enemy at (8, 5) for its red movement field. Confirm an empty cell → Member. Switch rows to read live details, focus Items and select a slot. Back returns to Options, then Cursor. The field and battle pages share layout and browsing. Battle offers Back only. |
| F9 Results | Select Victory, Defeat or Retreat in the toolbar. The current sample enters its actual before-battle message. Advance to Movement, confirm a stop, then use Attack for Victory, Stay for Defeat, or Magic → EGRESS for Retreat. The normal presenters own messages, rewards, departure and field return. |
| F10 System | The production System Menu covers the current eligible session. From the title it opens the authored field sample first. Settings and Save honor ordinary host admission. Cancel returns to the covered game phase. |

For a larger F8 roster, append `-- --inspect --authored-package <absolute member-large.json>`
using the [14-member recipe](../../../docs/godot/battle-ui-composition.md#reproduce-the-shared-roster).
F5 keeps its ordinary field source. Review the two production pages at 1600×900 and 1920×1080.
Only dedicated preview images may be retained locally for visual review.

Switch directly among F5–F7 to keep the same live party and inventories. F6 opens the
shop automatically while the party faces the merchant. If the party has walked elsewhere,
use ordinary movement to face the merchant again or press F6 again for a fresh sample.
Re-selecting another sample's tab resets its session. F10 toggles the live host menu.

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
The other tabs use existing authored content and its own available resources.

```powershell
$project = (Resolve-Path -LiteralPath 'remake/game').Path
& $env:GODOT_BIN --path $project res://probes/previews/DesignPreview.tscn
```

Pass a user argument such as `-- --members`, `-- --battle`, `-- --inspect`, `-- --shop` or
`-- --system` for direct entry. F9 starts on Victory with `-- --results`. Select Defeat or
Retreat directly with `-- --defeat` or `-- --retreat`. The preview uses one explicit save
file in its own fresh `user://design-preview` directory. Its preferences live in memory.
The public export excludes `probes/`.

For direct checks, inspect the actual mounted `GameRoot`, field and battle views and their
`ReadObservationJson` projections. The toolbar's and the game's panel style boxes use the
same texture. Check that a purchase changes the party seen in F5, inspection returns through
the right parent, F10 restores the covered session, and outcome waits reach actual field
control. Human visual and physical-device review remains **Unknown**.
