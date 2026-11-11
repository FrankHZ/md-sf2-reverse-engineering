# Godot design-system preview

Open `DesignPreview.tscn` in the remake Godot project and run the current scene (F6). The preview
starts in Chinese. Click a scene name or press its function key. F5 opens character details over the current
selection. F10 opens the system menu. Click the language button or press L for
English. Arrow keys move focus along the visible row or column, Enter confirms, and Esc returns.
On a gamepad use the D-pad to select, A to confirm and B to return. In dialogue, click the text
to continue and click a response to answer. Close the window to finish.

| Scene | Try |
| --- | --- |
| F1 · Witch | Choose New Game, select either slot, type a name and review the destination. Continue can offer explicit backup recovery. |
| F2 · Dialogue | Advance from a portrait line to a question, choose either answer and read the reply. |
| F3 · Church | Scan the four services, inspect Promotion or decline a Cure quote. Record opens the same slots as the system menu and returns to the service row. |
| F4 · Battle | Move down the four command rows to Magic. Pick a level and a target, then back out to the same level and command. Item, Attack and Stay have sample paths too. |
| F5 · Members | Browse Bowie, Sarah and Chester with Up/Down or click a name. Inspect the portrait, class, level/EXP, HP/MP, attributes, spell costs and four inventory slots. Enter/A or Prepare opens the supply handoff. Esc/Back unwinds each step; F5 or gamepad X closes details. |
| F6 · Shop | Buy supplies for a chosen member or sell a specific numbered slot. Review the party balance and price, then confirm. The receipt and F5 show the resulting inventory. |
| F7 · Field | Inspect a member, a caster's field spell availability or an owner's four item operations. Search the sign, dismiss the reading, turn away and inspect the unavailable Search row. |
| F8 · Inspect | Hover or move onto a unit for its current HP/MP. Confirm opens its read-only detail sheet. Confirm on empty ground for options, including Map, Speed and Quit. |
| F9 · Results | Read through victory, retreat or defeat. The final acknowledgement returns to town or the church. |
| F10 · System | Cover the current scene, draft settings, save at an eligible location, start a sample journey or quit. Resume restores the covered selection or reading page. |

### Entry, field and return paths

Slot 1 starts with a damaged primary record and a recoverable Bowie sample. Slot 2 starts empty.
New Game uses the existing physical-keyboard name editor and its seven-character validation.
Confirm names the slot and any replacement, with Cancel focused first. Continue or recovery
opens the recorded field or battlefield destination. The two slots retain the entered name and
destination in memory for this preview process. They do not serialize inventories, campaign
state or real player data. The prepared F4/F5 examples retain their named cast.

F7 and F8 reuse the right-hand command column with the original field, item-operation and
battlefield-option icons when available. F7 Give and confirmed Drop act on the same numbered
slots as F5 and F6. Use and Equip show the limitations of these sample loadouts. F8 Map
shows the full Battle 01 area and the three staged units. Arrow keys or the D-pad navigate the cursor.
Moving the mouse over a board cell also selects it. Left-click a unit for details or empty
ground for options. Hover and click a unit marker on Map to locate it on the board.
From an empty cell choose Map, select Sarah and confirm to locate her, then confirm to inspect.
F8 focuses one combatant on its detail sheet. Its attributes, spells and four inventory slots
use the F5 layout. Closing returns to the same cell and refreshes the short status. Gizmo
uses his own attributes and empty loadout with the preview encounter's staged HP.
Pass `-- --inspect` to launch directly on the inspection board.

**Hover card.** Land effect is right-aligned beside the name. HP and MP share the next row
and the location stays below the gauges. The private encounter supplies the mover and the
terrain under the indicated battle-local cell. The shared rule gives Bowie (7,9) and Sarah
(9,9) 15%, and Gizmo (8,7) 0% in this staging. Empty cells show their coordinates. With no
terrain or mover data the unit card shows a dash. [Original land effect display and lookup](../../../docs/godot/battle-interaction-baseline.md#what-status-and-terrain-inspection-may-disclose)

**Mouse camera.** On the F8 free board, dwell for 180ms inside a 32px edge band. Speed rises
from two to six cells per second toward the edge. Corners share that speed across both axes.
The clear outer 8px above the tabs and below the footer also accept edge scrolling. All sizes
use the 1920×1080 review canvas. The camera clamps to the encounter area and centers any axis
that already fits. Battle 01 is 16×20 cells, so only vertical scrolling is needed here.
The cursor and hit testing follow the cell under the pointer as the board moves. Leaving
the band or window, clicking, changing input, losing focus or opening an overlay stops the
pan. Fresh pointer movement re-arms it. Keyboard navigation and overview selection bring the
chosen cell into view. These camera timings are preview design choices.

The US `BattlefieldMenu` tests A on a living combatant to open the member screen. C on a
different combatant opens mini status and land effect. C on the acting combatant returns to
movement. The F8 preview makes short status available on pointer or directional focus and
uses a single confirm action for details. [Original battlefield menu](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L1059-L1109)

F9 reads the illustrative reward, growth and return messages in order. Its example numbers
do not modify the roster. Use Enter/A or click the reading frame.

F10 retains the covered decision. Settings applies its language and animation drafts together.
The window-recovery sample button covers its current host page and restores its focus. It
simulates that visual state without changing the desktop window. Quit defaults to Cancel.
Choose Save and quit at an eligible field, inspection or church location to try the slot and
receipt path. Launch with `-- --field` to start directly at the exploration commands.

### F4 action playback

On the board, hover a legal target to move its cursor and update the status card. Left-click
the unit to confirm or right-click to return to the offer. A click on an empty tile or an
ineligible unit cannot commit the last selected target. Leaving the pressed tile cancels the
gesture. The footer controls and directional selection use the same target state.

- Choose **Attack → Gizmo → Confirm**. Sarah's window opens at upper right and Gizmo's at
  lower left. Her rod follows the attack frames; the impact changes Gizmo's HP from 12 to 8
  in the initial sample.
- Choose **Magic → HEAL Lv. 1 or 2 → Bowie → Confirm**. Sarah casts in the upper-right
  window, paying 3 or 5 MP. She gives way to Bowie on the left. His upper-left ally window
  shows the recovery from 16 to 24 HP; the fairy effect retires and Sarah returns. Self-targeting
  keeps the caster in place. The enemy window is absent for these ally-only scenes.
- Enter/A or the return hint goes back to the board after the result. The board retains the
  displayed HP and MP. F4 starts the sample again. Items and Stay keep their short result pages.

The private pack supplies sprite poses, weapon transforms/layers, backdrop, ground and fairy
rasters. The public fallback draws schematic actors. The two visual samples use a centered
256×224 canvas, nearest 4× display, localized text and review-paced motion. Damage, recovery,
fairy trajectory and pauses are staged; the production battle session owns gameplay resolution
and audio. This visual preview does not write a game save. The status placement follows pinned
SF2DISASM [window destinations](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/ministatuswindow.asm#L110-L205),
[same-side target selection](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battleactions/animateaction.asm)
and [ally switching](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_0.asm#L458-L627).

### Shop counter

Press **F6** or launch with `-- --shop`. The staged counter uses the admitted Galam item-shop
stock (Medical Herb 10 G, Healing Seed 200 G, Antidote 20 G, Angel Wing 40 G), the original
service and item art when available, and a starting purse of 150 G. Repair and Deals retain
their unavailable reasons. Buy a herb for Sarah: the quote shows slot 4 and 150 → 140 G.
Then Sell → Sarah → slot 4 offers 7 G (three quarters of base price, rounded down).
Only Confirm publishes the new gold and pack. The confirmation defaults to Cancel.
Member focus previews the actual pack. Full packs and unaffordable stock have explicit reasons.
Sale of a starting equipped weapon names the unequip consequence. Back from a quote restores
the member selector. Original service behavior is documented in the
[shop agreement](../../../docs/content/profiles-and-trust.md#finite-galam-shop-admission).

Shop and F5 share their in-memory inventories. If a Give changes a quoted pack while F5 is open,
return to the selector for fresh terms. Reopening the shop retains transactions for this process;
restart for the initial examples. The Church and battle examples remain independently staged.

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
examples. Portrait/icon selections come from the mounted pack.

### Give a supply

In F5 select Sarah and press **Prepare**. Pick her slot 2 Medical Herb. Move between Bowie
(three occupied slots) and Chester (four) to preview each candidate's pack before confirming
the recipient. Pick Chester to go directly to the exchange, or choose Bowie, then
select Confirm (Cancel has initial focus). His next free slot is 4. The result shows both packs
and **View Bowie's pack** opens his updated sheet. Switch back to Sarah and offer her remaining
Healing Seed: Bowie is now full. Select a numbered supply from his pack to give back and check
both slot movements before confirming. Duplicate herbs remain distinguishable by slot.
Back before commitment preserves both inventories. Already committed handoffs persist across
member/scene switches for this preview process. All three members can give to either companion.
Active, disabled and read-only slots share the same icon/name/status columns, including empty
slots and the text-only mode. This F5 exercise uses consumables, with weapons
kept equipped. F4 uses its own fixed combat example. Restarting the preview restores the samples.

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

### Single-cell cursor

To prepare the original F4/F8 cursor, load the worktree's private input configuration and run
`uv run python remake/game/probes/previews/prepare_cursor.py` from its repository root before
launch. The script verifies the registered US ROM and writes one 32×32 transparent PNG to
`local/design-preview/cell-cursor.png`. The private scene uses nearest-neighbor scaling. The
schematic and older unprepared checkouts draw four plain corner marks.

**Confirmed:** in the pinned US disassembly, `DmaCursorTiles` loads `cursortiles.bin` into
VRAM at $D000. The radius-zero entry in `sprite_Cursor` selects tile 1664, one 4×4-tile
sprite on palette 3. The ROM stream at 0x23658–0x23757 expands to 2048 bytes. The first
sprite with the display's base palette at 0x309E has a transparent center and four open
corners, with white outer pixels and pale peach inner pixels. HEAL levels 1 and 2 both have
radius zero. Larger radii use different sprite arrangements. The preview centers the single
sprite on its own tile grid.
[Cursor loader and sprite table](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_0.asm#L955-L1129)
· [base palette](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/tech/graphics/displayinit.asm#L36-L47)
· [HEAL definitions](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/stats/spells/spelldefs.asm#L26-L41).

Dialogue, prices, parties and positions are staged examples for judging layout. Only local preview
state changes when you answer, switch language, pay or select a target. The preview opens no game
session or player storage. The ordinary entry remains `Main.tscn`; the public export preset
excludes the scene under `probes/`.
