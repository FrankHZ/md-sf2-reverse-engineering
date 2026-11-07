# Godot design-system preview

Open `DesignPreview.tscn` in the remake Godot project and run the current scene (F6). The preview
starts in Chinese. Click the scene names or use F1–F4. Click the language button or press L for
English. Arrow keys move focus along the visible row or column, Enter confirms, and Esc returns.
On a gamepad use the D-pad to select, A to confirm and B to return. In dialogue, click the text
to continue and click a response to answer. Close the window to finish.

| Scene | Try |
| --- | --- |
| F1 · Witch | Read the greeting and compare the two action rows. Inspect unavailable Continue, then choose New Game. |
| F2 · Dialogue | Advance from a portrait line to a question, choose either answer and read the reply. |
| F3 · Church | Scan the four labeled services, focus Promotion for its reason, then open Cure and decline the price. Record shows the save handoff in text. |
| F4 · Battle | Switch from the horizontal action bar to Magic. Pick a level and a target, then back out to the same level and command. Item, Attack and Stay have sample paths too. |

The layout follows the user-selected direction for [#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
The [design review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1683) owns the shared
component proposal. Church and battle orientations are candidates for human comparison.

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
