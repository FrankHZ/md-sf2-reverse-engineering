# Godot design-system preview

Open `DesignPreview.tscn` in the remake Godot project and run the current scene (F6). The preview
starts in Chinese. Click a scene name or use F1–F4. F5 opens character details over the current
selection. Click the language button or press L for
English. Arrow keys move focus along the visible row or column, Enter confirms, and Esc returns.
On a gamepad use the D-pad to select, A to confirm and B to return. In dialogue, click the text
to continue and click a response to answer. Close the window to finish.

| Scene | Try |
| --- | --- |
| F1 · Witch | Read the greeting and compare the two action rows. Inspect unavailable Continue, then choose New Game. |
| F2 · Dialogue | Advance from a portrait line to a question, choose either answer and read the reply. |
| F3 · Church | Scan the four labeled services, focus Promotion for its reason, then open Cure and decline the price. Record shows the save handoff in text. |
| F4 · Battle | Move down the four command rows to Magic. Pick a level and a target, then back out to the same level and command. Item, Attack and Stay have sample paths too. |
| F5 · Members | Browse Bowie and Sarah with Up/Down or click a name. Compare the portrait, class, level/EXP, HP/MP, attributes, spell levels/costs and the four numbered inventory slots. Esc, Back or F5 restores the underlying selection. Gamepad X opens/closes details. |

The layout follows the user-selected direction for [#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
The [design review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1683) owns the shared
component proposal. Church and battle frame their root commands and child lists in the same
right-hand panel. Its height follows the visible rows, with 16px below the last row. A Church
refusal expands the frame below Leave while every hit target stays in place. The
first row starts at (1312, 312) on the preview canvas. Both use
528×80px rows with 8px gaps, 30px names and 24×24 command art at nearest 3× (72×72px).
The focused available command alternates its two admitted images every 250ms while the row's
outline and label stay steady. The motion button holds the selected image for comparison.
Unavailable commands and spell/item identity highlights stay still. Text-only choices retain
the same focus outline.

In F4, click **A / B**, press **V**, or press gamepad **Y** to compare layouts on the current
decision. **A · Combined** puts Sarah and her vitals at the head of the right-hand action or
spell list. During targeting the same area identifies the candidate and their vitals.
**B · Bottom** keeps the action list on the right and puts Sarah's vitals in a bottom strip.
The strip expands to show the target alongside her when comparing units. Try Magic → HEAL Lv. 2:
compare Sarah's 8 MP with the 5 MP cost, choose Bowie, and switch layout again on the board.
Both share the same state, original icons, gauges, command focus and footer actions.
The selected A design uses 36px HP/MP numbers above 232×28px gauges. The member sheet
and B comparison use the same vital readout. Member stats and inventory are staged review
examples; the sheet is read-only and portrait/icon selections come from the mounted pack.

Child menus replace the parent in the same column. Battle → Magic and Battle → Item reuse the
root row size and anchor, with cost or slot aligned at the right edge. Church quotes and battle
Stay use one confirmation layout: parent and action in the heading, consequence above the two
choices, safe return focused first.
The bottom hint strip shows the current action and cost with inline Switch target (when applicable),
Confirm and Cancel hints. Each hint is also clickable. Directions stay with the board, and Enter/A
and Esc/B act directly without entering another menu. Back from targeting
restores the exact offer. Back from the offer list restores its parent command.

The deployed [menu control](../../src/UI/DiamondCommandButton.cs) currently draws a 180ms
rise/settle on focus entry and then holds the selected image. The preview's loop uses the two
halves documented by the [asset owner](../../../docs/content/presentation-assets.md).
Its cadence is a preview setting. Pass `-- --battle` on launch to open directly on the battle list.

## Run from the repository root

Use the registered Godot .NET build and the existing [local SDK and import setup](../../../docs/development-and-verification.md#locked-net-workflow).
Build the adapter in **Debug** for this checkout and import the project resources once. After loading
the checkout's local environment, launch the scene directly:

```powershell
& $env:GODOT_BIN --path remake/game res://probes/previews/DesignPreview.tscn
```

For the original-art preview, inherit the same read-only entry and battle inputs as the ordinary
private New Game and select the [complete world and battle scene pair](../../../docs/content/presentation-assets.md#final-publication-and-ordinary-selection).
The preview consumes the existing content readers and runtime textures. With no private selections
it opens a schematic with text-only commands. Incomplete selections are marked in the toolbar.
The composition uses a 1920×1080 canvas that scales to the preview window.

Dialogue, prices, parties and positions are staged examples for judging layout. Only local preview
state changes when you answer, switch language, pay or select a target. The preview opens no game
session or player storage. The ordinary entry remains `Main.tscn`; the public export preset
excludes the scene under `probes/`.
