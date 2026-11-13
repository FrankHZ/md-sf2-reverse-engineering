# UI Along the Player's Journey

This is the scene application of the [shared visual language](ui-design-language.md) for
[#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
The user accepted the F1–F10 layouts in the [Godot preview](../../game/probes/previews/README.md).
The ordinary pre-voyage game uses the corresponding production presenters. The entries below link to the actual
controls, their observed returns and the limits that matter during play.

## Scenes in the playable demo

| Scene | What the player can do | Production evidence and limit |
| --- | --- | --- |
| F1 Witch and slots / F2 dialogue | Start or Continue in either slot, read a portrait question, answer and regain control. A new name requires a physical keyboard, even when navigating with a pad. | [Entry and dialogue](presentation.md#production-entry-and-field-dialogue-1606) covers retained drafts, safe overwrite Back, backup recovery and an actual Map3 Yes/No in English and Chinese. |
| F3 Church | Scan labeled services with Up/Down, decline a quote, or Record and return from the shared slots to the field. Promotion displays its refusal. | [Church list and caller](#10-production-church-and-battle-command-lists) distinguishes the actual private Record route from the authored paid Raise case. |
| F5 party / F6 shop / F7 field | Open Member, Magic, Item or contextual Search. Inspect four numbered slots, compare a recipient’s pack or a Galam quote, then back out or confirm. | [Field, party and shop](field-and-shop.md#verification-boundary-and-remaining-gaps) covers selection return, full-pack exchange, Search dismissal and real buy/sell followed by walking. The [field footer and shop hint audit](#production-controls-audit) checks the mounted controls across input and shop phases. Repair and Deals remain unavailable. |
| F4 battle / F8 inspection | Move with directions, confirm a stop, then choose from layout A. Cycle or hover eligible targets. From Movement, Cancel undoes the preview and opens free inspection. | [Battle decisions and inspection](battle-interaction-baseline.md#production-f4f8f9-contract) covers actual range, targeting, board gestures, Map and return. See the [input and artwork differences](#9-remaining-differences-in-play). |
| F9 results | Read damage, death, rewards and growth, then wait for the next actor or walk after a field return. | [Playback and results](battle-scenes.md#production-playback-and-result-layout) distinguishes Battle01 combat, connected Battle02 EGRESS and authored victory/defeat. [Campaign handoffs](../application/pre-voyage-campaign.md#session-and-handoffs) cover Galam, the tower and the later return. |
| F10 Menu, Settings, Save and recovery | Open Help from the Witch menu or settled field. Cancel a slot overwrite or settings draft, restore a covered page after resize, and Continue a saved turn. | [Host access and restore](host-ui.md#access-through-the-finite-demo) lists actual Save predicates. General Settings is unavailable in battle. The tower return on Map58 supports Save; first control on Map14 has no field-save recipe. |

### Player-facing copy

Keep the prompt at the point where it changes a decision. A focusable row can say it is unavailable;
an attempted action can explain why. Keep the price, item or recipient, turn cost, overwrite and
recovery path before the player commits. A readable English story row needs the same reading
controls as a translated row; text diagnostics can be inspected separately.

| Scene | What appears in play |
| --- | --- |
| Title / Witch | Start and greeting retain their mapped actions and story. “Choose your journey • Normal difficulty” no longer accompanies every line: only the focused New Game choice shows “Normal difficulty” / “普通难度”. A missing entry translation still displays the English line. |
| Slots / naming | Choosing a slot does not write it. A blank name shows the actual default once; invalid input shows the limit beside the editor. Final Start names the slot, opening return, backup and loss of unsaved progress after success. Back retains the draft. An uncertain write directs both languages to Continue. |
| Field / Search | Each Adventure row says what it does. “Field effects and MP costs follow the existing preparation rules” becomes “Choose who will cast the spell.” / “选择施法的队员。” The reading hint appears for overflowing notes. Talk, choice, item/full-pack results and navigation bindings remain. A missing story translation preserves the body and diagnostic reason; only Debug diagnostics adds that reason to the reading cue. |
| Party / inventory | The selected field row leads to the existing member or preparation screen. Keep the four slot identities, full-pack exchange, equipment changes and resource costs at their own choices. The [member footer](field-and-shop.md#current-composition) owns its actions and layout. |
| Shop / Church | Repair, Deals and Promotion are marked unavailable in the list; their explanation appears on attempted selection. “Opens your save slots. Closing them returns you to the field.” / “打开存档列表。关闭后回到地图。” replaces the two-slot tutorial on Record. Keep merchant/priest dialogue, quotes, payment and refusal results. |
| System / recovery | Short System and slot pages omit the scroll instruction; overflowing pages keep it. Host actions that skip disabled rows still show their reasons. Save destinations, uncertain writes, backup recovery and Quit consequences retain their own messages. “Restore window size” names the action; the recovery body still gives the host's required and current dimensions. [Settings](host-ui.md) and the desktop size policy have separate owners. |
| Battle / results | Give and exchange name the items, recipient and end of the action. Drop asks once and says that movement and another action remain available. Keep targeted-click execution, MP, damage, item, reward and return messages. English battle-story lines in Chinese mode keep the reading cue; Debug diagnostics can add the untranslated-body reason. |

**Confirmed:** [ordinary production input observer](../../game/probes/player_copy_observation.gd)
at 1920×1080, using English and zh-CN instant text and default keyboard bindings. It reads mounted
controls and session state. No screenshots, player slots or generated game exports are needed.
Use the [existing adapter/Debug launch](#direct-reproduction), a fresh external validation namespace
starting with `issue1789-`, and a fresh ignored path for `SF2_COPY_OUTPUT`. Set `SF2_COPY_LANG`
to match the read-only input settings. Run the script with these `SF2_COPY_CASE` selections:

- `entry` uses the registered Witch/world inputs. In Chinese, `entry-long` varies the prelaunch
  slot prompt to exercise scrolling; `entry-fallback` and `entry-debug` use a missing entry catalog
  with diagnostics Off/On.
- `field` and `window` use an ignored copy of [the Map3 start](../../reference/inputs/map3-followers-start.json)
  in `SF2_COPY_START`; `field-fallback` and `field-debug` exercise Search with a missing world
  catalog and diagnostics Off/On. `church` and `shop` use the [Map3 priest](#direct-reproduction)
  and [Galam merchant](field-and-shop.md#verification-boundary-and-remaining-gaps) controlled starts.
  Talk and subsequent choices are ordinary inputs; these starts do not prove travel to the counters.
- `give` and `discard` use the public `battle-give.json` and `battle-discard.json` packages.
  For `exchange`, add item 0 to the receiver's three starting items in an ignored authored copy
  of `battle-give.json`. For `story` and `story-debug`, use `jaro-conversion.json` with Chinese
  selected and diagnostics Off/On. Two real acknowledgements return control to Movement.

Human reading comfort, physical controller testing and a cross-resolution scene matrix are outside
this copy observation. It exercises ordinary cancellation, not a storage-fault injection.

### Desktop clients

**Confirmed.** The [window policy](presentation.md#window-support-and-recovery-1455) uses the
actual client in physical pixels. These measurements come from running Windows game windows.

| Client | Title, Witch and entry slots | Gameplay canvas | One world cell |
| --- | --- | --- | --- |
| 1600×900 | 1920×1080 scaled to 5/6 | 1920×1080 scaled to 5/6 | 80 physical pixels |
| 1920×1080 | 1920×1080 | 1920×1080 | 96 physical pixels |
| 1920×1200 | 1920×1080, centered with 60px above and below | 1920×1200 | 96 physical pixels |
| 2560×1440 | 1920×1080 scaled to 4/3 | 2560×1440 | 96 physical pixels |

| Production page | Bounded input and return at each size |
| --- | --- |
| Title → Witch → Continue | Click Start and Continue in window pixels. Select the occupied first slot with Confirm. The Map3 field and saved player battle turn each mount through the ordinary host. |
| Field and Party | Open Member from the field menu. Its [footer](field-and-shop.md) contains Prepare and Close to the right of the hint. Clicking Close restores field navigation. |
| Dialogue and shop | Start at the controlled Map3 merchant approach. Face the counter, speak, read its greeting, click Buy, back out of stock and leave. |
| Battle and inspection | Open the action menu and measure the cards. Back out to inspection, hover a second unit by its projected map cell, click for details and return to that cell. At 1600×900, logical (1008,936) landed at client (840,780) in the observed Battle01. |
| Menu, settings, slots and recovery | Click the top-right Menu, Settings, Assist and its flash setting. Cross the width and height thresholds independently. Cancel recovery Quit and restore the supported size. The paused session revision, edited draft and subpage focus survive. Click Back twice to discard the edit and return to Menu. Back from the Save list returns to Save. |

To reproduce, build the actual Debug adapter and use the selected private entry/world/battle
packages and a disposable populated first slot. Set `SF2_HOST_UI_CASE` to `field`, `battle` or
`shop` and `SF2_HOST_UI_OUTPUT` to a fresh ignored report. For shop, set `SF2_HOST_UI_START` to a
worktree-local controlled start at Map3 (7,20), entity-0, speed32, flags 0/1/2/32/33/34/609.
Launch the configured Godot with `--path remake/game --resolution 1600x900 --max-fps 120
--audio-driver Dummy --script res://probes/desktop_layout_observation.gd` and repeat at the other
three sizes. The [camera resize recipe](display-composition-contract.md) checks actual player draw, camera origin
and cell size through live resizing at the center, edges, in a small room and during story framing.
Some exits emitted the existing ObjectDB warnings. **Unknown:** physical monitor/DPI combinations
and human reading comfort remain with the [play review](../evidence/opening-loop-acceptance.md).

### Production controls audit

**Confirmed.** These observations used the production host, registered campaign packages, scoped test slots and native input events. The two player reports reproduced on base `886cdcdb`. At 1920×1080 the field frame covered x=240–1680. The separate 320px label ended at x=1888 and its Chinese glyphs at x=1777. Testing the label against the screen alone passed. The gameplay Menu button resolved 8px corners in five states. Its actual hover popup independently resolved 3px corners and 30px text.

| Scene and caller | Production observation and disposition |
| --- | --- |
| Title, Witch and slots | New Game and Continue showed the focused row in the Witch column, with 24px between actions and footer. The scaled entry layout is intentional. The name form showed its default, editing keys and immediate errors for eight characters or an unsupported character. A corrected draft survived Back. Settings Help gives a single keyboard instruction. [Entry presenter](presentation.md#production-entry-and-field-dialogue-1606) |
| Field and top-right Menu | In a Ribble slot, the frame now contains both the context label and the Adventure hit area. At 1080p the latter is x=1346–1666 and the Chinese keyboard caption is x=1457–1666. Talk and the mapped speed cue share the other column. Actual mouse press/release, resize during a press, mapped Back, keyboard/pad changes, language Apply and 1440p exercised the same live controls. The button and its hover popup now resolve square corners. [Field controls](field-and-shop.md#reading-input-and-return) · [Host action](host-ui.md#implemented-composition-and-return) |
| Dialogue and choices | Controlled Map3 approach, ordinary directions and Confirm reached a Chinese source question. Reading, attached choices and hint had disjoint bounds. Left/Right moved the selection and Cancel answered No. The supplied portrait and reading viewport keep their own reserved space. [Source question](presentation.md#production-entry-and-field-dialogue-1606) |
| Party and items | Member and Item opened from the field list and Back restored the Item row. The status sheet's operation hint now uses 24px. Inventory keeps four positions. Original 16×24 and mounted HD 64×96 identity textures occupy the same 48×72 field slot. The system action is disabled while Party owns input. [Member and item sheets](field-and-shop.md#current-composition) |
| Shop and Church | Ribble Continue reached two actual counters with Talk, speed and Adventure enclosed. The controlled Galam merchant exposed Buy/Sell, Repair refusal, member, slot, quote, result and Back. The same mapped hint measured 24px throughout. The Church list retained focus on refusal, Record completed the caller, and the shared Save host returned to field. [Shop phases](field-and-shop.md#reading-input-and-return) · [Church caller](#10-production-church-and-battle-command-lists) |
| Battle | At native 1080p the acting and target headers begin at (1328,112). Free inspection places the unit/ground card at (1328,238) in the observed Battle01 position. The overview and right-side close-up card clear the host entry. [Menu clearance and input](#reproduce-gameplay-menu-clearance) |
| Host layers and save recovery | Menu opens on Resume and Back restores the caller. Field cues retire during host cover. Church Record and title Continue exercise the shared slots. The 24px primary failure and translation notice scroll above fixed Cancel/Recover Backup buttons. [1080p slot recovery and overwrite](host-ui.md#slot-recovery-and-overwrite-at-1080p) |

Physical controller legends, desktop DPI and reading comfort remain **Unknown** under [human play review](../verification/host.md#later-physical-focus-human-checklist-1456).

#### Reproduce gameplay Menu clearance

**Confirmed.** At 1920×1080 the square 160×56 Menu hit area spans x=1728–1888 and y=32–88.
The acting and target headers, the F8 overview and the right-side ally close-up start at y=112,
leaving 24px below Menu. The left-side close-up keeps its own position. The overview ends at y=948,
12px above its mapped Back hint. Clicking a marker on the resized map returns to that unit.
A click 3px inside the Menu corner opens Resume on release. Mapped Back restores the field,
inspection cell or battle command/target, and the caller accepts its next input.

On the first Battle01 actor, focusing unavailable Magic wraps the complete reason. At 1080p its
right-aligned footer widens below the fixed 560px command group to 1117px in English with keyboard
hints and 722px in Chinese with pad hints. Back occupies y=965–1009 in both cases. At 2560×1440
the reason fits the ordinary 560px column. The Attack row and the chosen target accept full clicks.
On base `bccb670c`, the Menu intersected both the overview and the ally close-up card, and the
same Magic reason placed Back below the viewport in both languages.

Build Debug and load the registered world, scenes, catalogs and disposable namespace as in the
[battle reproduction](battle-interaction-baseline.md#direct-production-reproduction).
Point `SF2_COMPACT_START` to an ignored copy of
[`map3-followers-start.json`](../../reference/inputs/map3-followers-start.json) for the field,
or the linked reproduction’s `battle` JSON for Battle01. Use English keyboard and zh-CN pad
settings for the battle runs, with `SF2_INSPECTION_ROUTE` set to `keyboard` or `pad`.
Each run uses its own output and settings. The [observer](../../game/probes/gameplay_menu_clearance.gd)
reads live hit rectangles, shaped text, selection, pause and return. Some processes reported two
or six ObjectDB instances at exit; their owner remains **Unknown**.

```powershell
$env:SF2_VIEWPORT_OUTPUT = Join-Path $freshRun 'menu-clearance.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
  --path remake/game --script res://probes/gameplay_menu_clearance.gd `
  --log-file (Join-Path $freshRun 'godot.log') -- --input-settings $inputSettings
```

#### Reproduce the field controls

Load the [registered environment](../../../docs/operations/local-private-inputs.md) and select the compatible world, scene, entry and catalogs. Use a fresh external `issue1764-*` validation namespace with a copy of a settled field save in slot 1 (observed on Ribble F708). Use its own writable input preferences with instant text, an ignored output directory and the current Debug host. The probe uses Settings Apply on this copy. Default controls start in zh-CN. The reverse run starts in English with Cancel on B and Accelerate on Q / RightY−.

```powershell
$env:SF2_HOST_UI_OUTPUT = Join-Path $freshRun 'field-chrome.json'
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --resolution 1920x1080 `
  --path remake/game --script res://probes/field_chrome_observation.gd --log-file (Join-Path $freshRun 'godot.log')
```

The [observer](../../game/probes/field_chrome_observation.gd) checks the rendered character bounds, child and panel rectangles, mounted tooltip, actual bindings, speed hold/release, popup return and adapter state. The [shop reproduction](field-and-shop.md#reproduce-the-bounded-observations) exercises both prices and sizes. Some bounded processes reported two to six ObjectDB instances at exit. Their owner remains **Unknown**.

### Short play route

Use the ordinary demo with a fresh play slot and a gameplay client of at least 1600×900.
[Launch and Continue instructions](../evidence/opening-loop-acceptance.md#current-human-play-preparation-1456)

1. At the Witch, open Settings → Controls help to see your bindings. Choose New Game in an empty
   slot, enter a name with the keyboard and confirm Start.
2. In Granseal, hold a direction to walk, face someone and use Confirm to talk. After the dialogue,
   open the field menu with Cancel, inspect Member → Prepare, then back out to walking. At the
   Granseal priest, choose Record at a supported savepoint, save to your play slot and quit.
   Relaunch and Continue that slot. Take a step to check that you are back in control.
3. Follow Astral to the tower. On your first turn, Cancel to inspect a unit, then Back to Movement.
   Move with directions and confirm your stop. Choose Attack when an enemy is in range. Hover to
   inspect the target, then click it or press Confirm to attack. Read the result until control returns.
4. If you already have a compatible post-Battle07 tower save, Continue it for a short late-game segment. Walk down
   through the castle and east through the damaged town to the docks. Tell the captain No to stay,
   then Yes to board. After the crossing, take one step on the damaged ship. That is the demo endpoint.
   [Tower save, dock choice and ship arrival](../application/pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control)

## 1. Title and the Witch

**Components:** entry greeting → vertical text choices. Compare the selected row beside the greeting
frame in English and Chinese. Both keep the same straight edge, and the active row alone has a focus
marker.

Keep the title's original admitted art and the Witch as the visual subject. At the existing 1920×1080
entry canvas, preserve the complete Witch rectangle `(160,92,1024,896)` and the separate right region
`(1216,64,640,952)`. The 32px horizontal gap is real empty space, not a panel extending behind her.
No full-screen shade covers the Witch. Entry's smaller-window scaling remains distinct from gameplay.

```text
+-------------------------- 1920 × 1080 entry ---------------------------+
|                                                      [Settings/help]  |
|   +----------------------------+   +------------------------------+    |
|   |                            |   | Witch's greeting             |    |
|   |       WITCH ART             |   | full readable text           |    |
|   |       reserved in full      |   +------------------------------+    |
|   |                            |      › New Game                      |
|   |                            |        Continue                      |
|   |                            |        [existing entry choices]      |
|   +----------------------------+   +------------------------------+    |
|                                    | current mapped hints         |    |
|                                    +------------------------------+    |
+-----------------------------------------------------------------------+
```

The right side is one layout column, not necessarily one enclosing blue rectangle. Greeting and
action rows use the same square edge family; the active choice alone gets gold emphasis. A greeting
that requires reading owns the column before choices become available. No new simultaneous menu
is implied by the diagram. Keep existing title entrance, static idle/return and reduced-flash behavior.

| State | What the player should see / do |
| --- | --- |
| Normal and selected | Art stays unobscured; the current eligible choice has a marker and gold text/surface. Preserve the current default choice and Continue eligibility rather than selecting New Game because it is first in this sketch. |
| Unavailable / entry failure | An unavailable Continue or invalid entry resource has an explicit current reason. Safe existing retry/back actions stay reachable; no fake successful load or invisible button. |
| Long greeting / English / zh-CN | Reading grows/pages within the right region. Choices and hints use separate measured space after the reading phase; do not overlay rows on the last text line or shrink the greeting. |
| Covered / Back | Settings uses its existing allowed entry path and restores the same entry phase. A held Confirm or mouse release cannot select a new game on return. Cancel leaves the title on Start. |

The [production entry owner](presentation.md#production-entry-and-field-dialogue-1606) records
the observed layout, inputs, name validation and return from slots.

## 2. New Game, name and slot choice

Reuse the same right-column footprint while the Witch is present. Show a short heading, the current
step, choices/editor, then a fixed action/help area. Never compress the Witch to make more room.
Slot cards identify the actual slot, occupant and supported continuation information; unavailable
or corrupt data does not masquerade as an empty slot. The current two-slot design remains two slots.

| State | Layout and return rule |
| --- | --- |
| Choose slot / focus | Two clearly separated measured rows; marker beside the current row. Long occupant and location information wraps before the confirmation/footer. Opening a slot does not overwrite it. |
| Name editing | The focused editor shows the draft and fixed editing keys. Invalid input brings up the length or character explanation beside it. Enter/Escape retain their physical-keyboard editing behavior. The admitted alphabet and seven-character limit also apply in zh-CN. |
| Invalid name / failed storage | Error sits immediately below the editor or affected slot/action, before the fixed buttons. Keep the draft and safe Retry/Back where currently supported. Do not erase the name or consume the final Start confirmation. |
| Occupied slot / final Start | Separate confirmation names the destination and consequence; Cancel is initially selected where the existing lifecycle requires it. No pointer-down or returning held key can confirm. |
| Long labels / narrow entry window | Wrap and scroll/read within the existing scaled column. Keep full destination and overwrite wording; existing 1280×720 entry support does not imply native 720p gameplay. |
| Back / overlay | Return through the existing slot/name/setup phases with retained draft. Native focus recovery restores editor focus without editing text. System/window recovery covers the editor and consumes its own inputs first. |

This is presentation of the [slot/name lifecycle](../application/opening-session-lifecycle.md#slot-name-and-initial-save-design-1443),
not a change to initial-save publication, backup recovery or format compatibility.

## 3. Exploration: world, speech, choices and Search

**Components:** subject-aware dialogue frame → attached choice only when the script asks. The same reader handles a short portraitless line and a long question with a supplied portrait.

Ordinary field control has an unobstructed world, the existing right-top Menu and the currently
legal context hint. No new quest bar, minimap or resource dashboard is added. The Adventure entry
and Cancel retain their gameplay-menu route; Talk/Search still depend on the actual faced context.

```text
  WORLD / current speaker                                        [Menu]
                    person remains visible

  +---------------------------------------------------------------+
  | [portrait, when supplied] | Dialogue, 30px                       |
  | [reserved 192 × 256]      | wraps in its own text area           |
  |                          |                                     |
  |                          | › Yes          No   [choice phase]   |
  |                          | existing mapped reading/choice hint  |
  +---------------------------------------------------------------+
```

The normal reading frame begins at the existing 1440×280 size, 304px with a portrait, centered near
the bottom safe edge. If that position covers the speaker and the top is clear, select the top
placement once for the live window. Body, portrait, choices and hints are disjoint regions; the
portrait is not an icon floating outside the text window. Do not invent a speaker name from a
sprite or a party-name substitution. Long text uses existing measured expansion/reading pages.

| State | Visible response and boundary |
| --- | --- |
| Ordinary speech / reading | Text and portrait belong to one visible reading composition. Preserve the current ordinary Confirm/reveal policy; Cancel cannot skip story. A new segment resets reading as its owner specifies, even if words repeat. |
| Yes/No or other admitted choice | Show the actual options only in the choice phase, with marker/gold focus. Respect source defaults and source Cancel=No where applicable. Unread text is read before a fresh Confirm can answer. |
| Search result / unavailable action | Show the actual search description and acquisition/full-pack/no-result feedback in the same family. Search is not Talk. Cancel/right click may dismiss the Search result under its existing contract; an acquired item stays acquired. |
| Long source lines / translated text | Preserve meaningful Search line breaks, the current portrait column and complete fallback text. Grow/page the body, then keep the choices and footer reserved. A full-pack refusal is not clipped to make room for an item name. |
| Closing / next speaker | Close portrait and frame as one visible result when the owning close is due. The [coordinated exit](presentation.md#coordinated-dialogue-exit-1613) keeps the portrait mounted through the existing wait and retires it with the window. |
| Host cover / return | An allowed Menu covers the reading state without advancing it. Return restores that same phase and page; release barriers prevent an extra advance. A stopped scene displays its real failure over world fades, not an invented Continue. |

Field commands use the compact right-hand list beside the visible world. Member, Magic, Item and
Search retain their original art and admission. Show Search's actual reason when it is unavailable.
Back from preparation restores the initiating choice, then Back returns to field control. The
[production field menu](field-and-shop.md#reading-input-and-return) owns Up/Down selection,
Left/Right reading, pause and the member-before-item-operation flow.

## 4. Party, inventory and preparation

Preserve source-grouped information rather than producing a dense spreadsheet of every stat.
The member list is the navigation column; the selected person's name/class/level, HP/MP, combat
statistics, items and spells remain grouped in the detail region. Prepare opens the existing
action flow; it does not turn Party Status into a battle command sheet.

```text
  +----------------+  +---------------------------------------------+
  | Members        |  | Selected member · class / level              |
  | › Bowie        |  | HP / MP            combat statistics          |
  |   Sarah        |  | Items              learned spells            |
  |   Chester      |  | [selected item/spell detail or current pane]  |
  +----------------+  +---------------------------------------------+
                      [Prepare]                         [Close/Back]
```

| State | Layout and interaction |
| --- | --- |
| Member / pane selection | One marker per active navigation area; the selected member persists while a detail pane scrolls. Gauges and values remain current. Prepare/Close are ordinary actions, not both permanently gold. |
| Item / spell / equipment | Current inventory slot and real item/spell name, cost and prospective equipment change are explicit. Selection previews; a separate existing confirmation commits. A long list scrolls with the selected row visible. |
| Unavailable / no effect | Use the actual query reason; retain muted focus for inspectable unavailable rows. Field HEAL/Herb is not promised as free HP recovery. Unsupported effects remain explicit rather than looking like successful healing. |
| Give/exchange / Drop | Show the owner and item. Focusing a recipient reveals their four fixed slots. If their pack is full, select the numbered return slot before confirming the exchange. Back restores the parent selection. Successful transfers or Drop remain completed when the result is dismissed. |
| Long names / wide values | Grow rows/detail within the reading region, preserve full counts and costs, and use existing detail scrolling. Keep fixed Prepare/Back areas outside scroll content; do not reduce all text to fit the longest metadata. |
| Cover / Back | Back unwinds action → member/pane → originating field menu or direct field entry as currently owned. Party's current host-Menu exclusion stays; no new overlay access or remote-service return is introduced. |

The [Party agreement](presentation.md#party-details-agreement) and
[field preparation owner](../application/opening-session-lifecycle.md#implemented-field-preparation)
retain the detailed capabilities. Apply the same labeled-list treatment to Use/Give/Equip/Drop
and the admitted item/recipient/return slots. Distinct copies of an item keep their slot identity
when focus moves or Back restores a choice.

## 5. Church services

Use the compact right-hand service list with the priest visible. Each row shows the original
command icon and its localized name. Up/Down changes the selected service. Leave has a
separate row. A refusal expands the panel below Leave, keeping all service hit areas fixed.

```text
     priest and room                       Church          Gold [amount]
                                           [icon] Raise
                                           [icon] Cure
                                         › [icon] Promotion   Unavailable
                                           [icon] Record adventure
                                           [Leave]
                                           [actual reason for this choice]
                                                         [mapped hints]
```

In Chinese, scan 复活 / 治疗 / 转职 / 记录冒险…（存档） before moving focus. In English, compare
full service names against the existing short icon captions. Confirming an eligible service replaces
the list in the same side region with the member,
condition, exact price and Confirm/Decline prompt. Check that the priest remains identifiable while
reading the longest reason. The reviewed Church veil dims the room while the list stays on the right.

| State | Layout, transaction and return |
| --- | --- |
| Root / selected service | Show each name with its original icon and focus marker on the current row. Keep current gold visible. |
| Quote / confirm | Replace the service list and use a reading panel with the actual next source-ordered member, condition, exact price and funds, followed by distinct Confirm/Decline rows. No new member picker or remote service. |
| Unavailable / refusal / no eligible member | Actual curse, promotion, funds and no-member reasons remain readable. A refusal is located where the quote/result belongs, not as a fleeting toast behind another panel. No automatic payment. |
| Long reason / name / localized hint | Grow the reading region or page the reason. Keep the focused row visible and measure the full mapped footer. Do not shrink fonts or delete hints to preserve the starting height. |
| Result / Back | Result acknowledgement proceeds to the next real offer or services. Cancel declines the quote or leaves services as the existing wait dictates; prior paid treatment is retained. Restore the prior service focus where supported. |
| Record / host overlap | Record completes the Church caller before the shared Save host opens at a legal settled point. Do not stack slots above an active ChurchWait or restore a consumed quote after Save. Other host/gameplay modals retain current exclusion. |

The reviewed layout uses the transactions defined by the
[Church/save agreement](presentation.md#church-and-shared-save-agreement-1519).
Promotion, curse removal and general free healing are not delivered by drawing their labels.

## 6. Shop: stock, recipient and price before commitment

Buy, Sell, Repair and Deals use the right-hand service list. Stock, recipient or owner,
slot and quote replace it in place. The person's four-slot pack sits immediately to its
left during recipient, slot and quote decisions. The quote names the item, member and
price and shows the balance before and after payment. Keep the merchant visible.

| State | What must remain legible and actionable |
| --- | --- |
| Buy / Sell selection | Marker identifies the current mode and subsequently the stock/owned-item row. Show the actual eligible stock and owner; no implied infinite depot or cross-town catalogue. |
| Recipient / equipment preview | Name and occupied slots remain visible before purchase. Show only the existing equip/stat information; do not invent a purchase-and-auto-equip action. |
| Insufficient funds / pack full / unsupported | Explain the actual refusal beside the affected selection/quote. Deals/Repair and unsupported rare/broken-item sales do not become enabled because the original menu had them. |
| Long stock names / wide gold | Allow taller rows and an aligned amount column; never cut a digit or replace the full price with an ellipsis. Scroll stock independently of fixed Back/confirm controls. |
| Confirmation / result / Back | Display item, member and exact payment before the existing explicit commit. Back unwinds the current provisional phase; a successful sale/purchase stays committed. Leave completes the real service caller. |
| Overlay / access | The real service owns input throughout; no Party/Menu panel competes for clicks. No near-town shortcut is added. Actual Galam caller/setup admission stays with the [capability owner](../capability-status.md#finite-galam-shop-and-purchased-supply-closure). |

The [Galam shop observation](field-and-shop.md#verification-boundary-and-remaining-gaps)
covers actual payment, slot changes, refusal and the merchant’s return to walking.

## 7. Host Menu, Settings, Save and recovery

The [implemented F10 host layout and finite observations](host-ui.md) apply this design to the
ordinary Menu, preferences/help, shared slots and window recovery. Its access table retains the
existing per-phase settings and save predicates; headless evidence does not imply desktop or human
visual acceptance.

The right-top Menu remains an unmistakable host entry with its own safe region. Its centered 800px
menu retains Resume, the current Journey actions and Session actions. Use the shared square
surface treatment and ≥64px rows. Existing unavailable rows keep their explanatory text even
when directional navigation skips them. Do not add a second persistent toolbar.

| Page / state | Normal and focused composition | Unavailable, long, covered and return states |
| --- | --- | --- |
| Menu | Resume remains initial focus; Journey and Session are quiet group headings. Selected action uses marker/outline plus gold. | Save/Settings/New Game retain their real access predicates. Longer reasons wrap; measured rows scroll if necessary. Menu toggles Resume only at its own root. Back resumes the retained gameplay phase after release. |
| Settings / help | Current preference label, value and description are aligned; Apply/Back remain outside the reading/scroll area. Focus identifies the edited control. | Preserve draft/Apply/Back and current safe language refresh. Existing binding help/configuration remains; no binding-capture or device-detection UI is added. Long bilingual help scrolls. Return to the originating host page without applying on Cancel. |
| Save slots / overwrite | Two clear slot rows; full actual continuation destination and selected slot. Overwrite is a separate consequence panel, Cancel initially selected. | Unavailable location is a reason, not a disabled-looking successful save. Wrap occupant/date/location/error text; keep Retry/Back where legal. Back from confirmation → slots → Menu for that origin; Church entry follows its completed-caller route. |
| Continue / backup recovery | Name the actual primary failure and the offered backup; recover only through the existing explicit choice. | Never label corrupt primary as Empty or silently load backup. Preserve the failure detail and safe Back. Loaded state, not UI recollection, determines the resulting world. |
| Quit / session replacement | Show current clean/dirty consequence and existing confirm/cancel actions, with Cancel initially selected as required. | Long warnings fit before action rows; no destructive click-through. Cancel returns to the origin, including recovery if it owned entry. |
| Undersized-window recovery | Recovery is the sole visible host action owner; offer the current legal restore/fullscreen/quit actions. | It hides covered host pages and restores the same page/selection on exit before gameplay resumes. No sub-1080p gameplay, forced restart or automatic save is introduced. |

Keep the current [system Menu](presentation.md#system-menu-agreement-1518),
[preferences](presentation.md#basic-preferences-agreement),
[Save/Quit](presentation.md#gameplay-save-and-quit-windows-1450) and
[window recovery](presentation.md#window-support-and-recovery-1455) authorities.
The [implemented battle Save/Continue contract](../application/battle-save-and-continue.md#implemented-ordinary-host-boundary-c)
now supplies admitted settled-Movement eligibility, two-slot Save and return / Save and Quit,
battle destination labels and fresh-process continuation. The [host observation](host-ui.md#reproduction-and-finite-evidence) covers a real save and fresh-process Continue at the saved actor.
The current user slot and backup must never be a design-validation fixture.

## 8. Battle: one complete decision, then its consequences

The [battle feature trace](battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
owns gameplay and source evidence. Use **layout A** on the command board. Place the acting
unit and HP/MP at the head of the right-hand panel. The four commands form a vertical list.
The B comparison places vitals at the bottom in the F4 preview only.

```text
   Round                                              Sarah · Battle
                                                      HP 18/24   MP 8/12
               board and acting unit                  › [icon] Attack
                                                        [icon] Magic
                                                        [icon] Item
                                                        [icon] Stay

   Direction / confirm hints                          Reposition
```

Up/Down moves through commands. Magic or Item replaces the rows at the same origin.
During targeting the board shows reach and the active recipient, the right card shows
that recipient's values, and the footer owns cycle, Confirm and Back. Back restores the
chosen level or command. Close-up windows follow the separate playback layout below.

### From entry to committed action

| Decision phase | Recommended visible composition and focus | Invalid/long state and return |
| --- | --- | --- |
| Battle entry / before player control | Present the existing scene/intro dialogue and real entry status. No commands while the script owns play. | Long intro uses the story reader; existing Skip appears only when eligible. No new deployment or pre-battle equipment screen is inferred. Host cover preserves delivery. |
| Movement | Acting card plus quiet round/gold; legal range and current preview/cursor on the board. The player's attention stays on the proposed cell. | Preserve actual range/refusal feedback; no invented path legality. Confirm opens actions. Current Cancel and movement-preview behavior remain session-owned. [Held directions](presentation.md#battle-directional-hold-and-mode-isolation-1615) repeat within Movement. |
| Main action | Four labeled command items in the right column beneath the acting unit's vitals. Show a detail only for a cost, consequence or refusal. At a registered faced chest, the fourth item becomes Search under the [existing admission](../content/profiles-and-trust.md#battle-search-admission). Its authored text fallback remains available. | Disabled focus is inspectable and cannot commit. Long reason grows/pages the detail group. Back returns to movement and undoes the preview through the current Cancel contract. Stay retains explicit confirmation. |
| Spell / item list | List replaces the command rows; selected offer shows actual spell level/cost or item/slot. Acting card remains; no stale target card. | Rows include actual unavailable reasons, scroll and retain focus. Confirm enters only an enabled offer. Back restores the same command focus with no resource/RNG change. |
| Target / affected area | Replace the command panel with the selected recipient's card. Keep action reach and the effect cursor on the board. Show target cycling, Confirm and Back in the footer. | Clicking an empty, defeated, wrong-side or out-of-range cell cannot confirm an earlier valid target. Long name uses the card policy; detail uses reading space. Back returns to the spell/item list or main action and clears provisional target through the existing return command. |
| Equipment / Give / Drop | Reuse the auxiliary presenter and advertised mapped shortcuts from action choice; show the actual candidate, owner/recipient or consequence in one reading panel. | No simultaneous unrelated equipment/item/spell stacks. Preserve read-before-confirm and cancellation. Product connected Equip/Give have separate [Content admission](../content/profiles-and-trust.md#connected-battle-give-admission); [Drop](../content/profiles-and-trust.md#connected-battle-discard-admission) binds its own finite free transaction. Controlled field-only equipment bindings remain immutable. This layout grants no new mouse coverage or transaction capability. |
| Commit | The explicit current command owns payment/action commitment; UI retires the provisional pages and switches to the real presentation phase. | A refusal/stale offer remains with its current responsible view and reason; do not invent success or silently submit a previous choice. Returning held directions/Confirm cannot act on the next phase. |

### Showing range and choosing a target

During movement, mark legal stopping cells using the actor's movement budget, terrain costs and
occupancy. On Attack or a highlighted spell level, draw its reach from the selected stop and
keep that reach visible while selecting a recipient. Draw the effect cursor on the recipient.
Use cyan for movement and gold for action reach. Each tile has a bright inner rim and dark
outer edge. The production fill pulses between 53% and 68% opacity over 1.2 seconds. Reduced Flash
holds it at 60.5%. The original varies the tile
palette during its pulse.

Up/Left selects the previous eligible combatant and Down/Right the next, wrapping within the action's living, in-range
target list. Hover previews a candidate from that list. A complete left-click on the same legal unit commits the action. A single candidate stays selected. With none,
show the reason and Back. Returning to the command menu allows the player to reconsider the move.
[Original grid construction](../../../docs/research/battle/battlefield-pathfinding.md#range-and-target-construction)
and [target input](../../../docs/research/battle/battle-functions.md#confirmed-player-control-state-machine).

### Viewing the battlefield

During free inspection, pointer hover and directional focus reveal the same short status.
Put the unit name and its land-effect percentage on the first line, with the percentage at
the trailing edge. The next row gives HP and MP equal space for values and gauges. The
location sits below them. On an empty cell, keep the percentage in the same position and
label it “Ground effect” for a regular ground unit. Show “Ground impassable” on cells it
cannot enter, or a dash when terrain data is unavailable. Keep the coordinates below.
Confirm on empty ground opens battlefield options. Confirm on a unit opens
its read-only detail sheet. Back restores the inspected cell.

For mouse camera control, use the exposed board edge during free inspection. On the 1080p
review canvas the band is 32px wide, with a 180ms dwell and a speed of two to six cells
per second according to pointer depth. The clear outer window rim also supports a pointer
pushed against the top or bottom edge. Diagonals share the same maximum speed. Center axes
that fit and stop scrolling at the encounter bounds. Keep hover and click aligned with the
moving map. Interior movement, a button press, keyboard or pad navigation, window exit,
focus loss and modal entry end the gesture. Re-entry requires fresh pointer movement.

The [battle interaction baseline](battle-interaction-baseline.md) records the original inspection
path and current cursor, details, Map, Member, Speed and Quit consumers. Empty-ground options
use the same labeled command items with each option's original art. Identify the inspected unit and terrain beside the freely moved board cursor.
The whole-board Map has room for its unit list and legend. Only disclosed unit/terrain facts
appear. Movement Cancel finishes the real undo before inspection opens at the origin. Action-menu
Cancel returns to Movement first. On another unit, closing details restores the cursor. Map returns
to Options, then cursor. `Member` restores its selected roster row. Battle Quit uses its existing save eligibility and return flow. The optional host Overview and a
predicted action-order strip remain outside this design.

### Presentation, outcome and usable return

**Close-up layout decision.** Use the original battlescene mini-status window placement during
action playback. The ordinary ally/enemy opening places the ally's window at the **upper right**
and the enemy's at the **lower left**. In F4, Sarah's rod attack uses these two windows. For HEAL on Bowie, Sarah casts at the
upper right, then Bowie appears at the upper left for recovery. Self-healing keeps Sarah in place.

**Confirmed (pinned US source).** At SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`InitializeBattlescene` opens each present combatant's window with a zero placement argument and
skips an absent index (-1). In tile coordinates, the ally opener chooses x = 31 − window width,
y = 1, and the enemy opener chooses x = 1, y = 20. Their nonzero branches change the horizontal
side while retaining the respective row. The switch/reaction routines supply these placement
flags when reopening windows. Use the actual scene participants and phase for cases such as
ally-to-ally healing. Exact on-screen transitions in that particular case remain **Unknown**
here. [Initialization](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/initializebattlescene.asm#L253-L269)
· [window destinations](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/ministatuswindow.asm#L110-L205)
· [ally reaction](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_0.asm#L1218-L1224)

| Phase | Composition | Stress state, acknowledgement and return |
| --- | --- | --- |
| Physical / item / spell close-up | Hide the board under the existing close-up rule. Show only actual acting/current reaction-target cards and the current message; retire command panels and round/gold strip. Character/weapon/effects stay in their own local composition below UI. | Costs and HP/MP update at the real committed phases. Multi-target reactions switch the target card at that boundary. Long messages read/page without premature acknowledgement. [Weapon-frame composition](battle-scenes.md#weapon-frame-composition-1620) owns front/back order. |
| Quiet support / suppressed / no-effect | Existing message/card presentation may have a quiet black backdrop. Keep the actual reason and recipient readable. | Do not fabricate an attack animation to fill silence. Preserve source/delivery semantics and supported effects; a quiet frame is not evidence of a stuck action. |
| Death / after-turn notice / automatic enemy turn | Restore the board for field-owned removal and notices; show only relevant status/text. Player command controls are absent while automatic progression owns play. | Low/zero HP is explicit; cards never retain a departed target into the next action. Message delivery, removal completion and focus suspension remain under current owners. |
| Reward / growth / victory / loss / retreat | Show the existing ordered results and current return prompt in the same reading family. No new duplicate reward summary or clickable future actor strip. | Wide amounts and multiple growth lines use reading pages. Closing cannot replay rewards. Retreat payment/return and special victory/loss programs keep their owners. |
| Back to player or field control | Restore live status/context and only the legal input owner after the real presentation/program finishes. Field returns use field chrome, not a lingering battle HUD. | Preserve failure feedback if return stops. An animation ending is not proof that exploration can move or save. No Return button bypasses the current continuation. |
| Host Menu / recovery during any permitted phase | Host owns pause/input and covers battle UI; recovery hides covered host pages first. | Close to the same actor/phase/page. Input release cannot move, confirm, acknowledge or start the next actor. Cards and map visibility must restore coherently without reconstructing gameplay. |

The [battle-scene owner](battle-scenes.md#message-and-outcome-presentation) and
[current status-card contract](presentation.md#battle-status-card-agreement) own committed values,
scene pairing and delivery. [Selection feedback](presentation.md#battle-selection-and-preview-feedback-1617)
keeps sprite colors intact and marks the moving actor with steady brackets.

## 9. Remaining differences in play

**Progression.** The recorded campaign carries the party through Battles01–07, restores the
post-Battle07 tower save in a fresh process and reaches a movable player on Map14. The UI records
cover individual scenes and identify no new progression or restore blocker. Mid-battle Save needs settled
Movement at the actor’s origin. For the final stretch, save on the Map58 tower return before leaving for the docks.
[Campaign route](../application/pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control) ·
[Save access by phase](host-ui.md#access-through-the-finite-demo)

- **Aligned — target preview versus execution (#1686).** The actual English and Chinese F10
  targeting help now distinguishes hover preview from a complete legal-target click, which executes
  immediately without another Confirm. An invalid click cannot submit an earlier target; Back remains
  free. The existing target/input consumer is unchanged. The
  [narrow production observer](../../game/probes/engine_battle_vitals_observation.gd) checks actual
  help and real mouse/pad/Cancel input. Cases `help`, `battle` and `wide` select the ordinary field,
  controlled private Battle01 or a prelaunch authored variant; use the
  [existing production launch/environment recipe](battle-interaction-baseline.md#direct-production-reproduction)
  with that script, `SF2_VITALS_CASE`, a fresh `SF2_VIEWPORT_OUTPUT`, matching input settings and
  `SF2_COMPACT_START` for private/field starts. [English](../../game/localization/en.po) ·
  [Chinese](../../game/localization/zh_CN.po) · [BoardTarget](../../game/src/Battles/BattleCommandMenu.cs)
- **Active input hints (#1544).** Movement, command/target and auxiliary cues now use the most
  recently effective keyboard/mouse or pad binding. Pure cue refresh preserves the live command,
  target and reader; full Help retains alternatives. Unknown layouts use localized positions;
  physical controller recognition and human discovery remain with #1456.
  [Battle footer](../../game/src/Battles/BattleCommandMenu.cs) ·
  [Mappings and acceptance](presentation.md#mappings-first-use-and-reading-limits)
- **Movement input.** The F4 example accepts a click on a cyan cell. In real Movement, use mapped
  directions, then Confirm. Mouse input is supported on command rows, eligible targets and the
  free-inspection board. Click-to-walk was outside the accepted movement scope. It would require
  a new product decision. [Preview controls](../../game/probes/previews/README.md#f4-action-playback) ·
  [Movement agreement](presentation.md#battle-movement-range-agreement) · [Production input](battle-interaction-baseline.md#production-f4f8f9-contract)
- **Aligned — F4 decision vitals (#1686).** Actual acting/target headers opt into 36px HP/MP
  readings above side-by-side 232×28 gauge images. Header and commands/child list share one outer
  decision frame in layout A; the footer shares its right edge and widens for a long reason when vertical space runs out.
  Full-width values grow the group horizontally; two-line names
  reserve their own height. Short lists use their actual rows, and longer lists retain scrolling with
  space reserved for the measured reason/footer. Existing values and the no-MP dash/empty track stay
  authoritative; F5/F8/full/close-up card consumers do not opt into this decision-only layout.
  The same narrow observer checks actual private Battle01 and an admitted authored copy of
  `attack-spells.json`: rename only actor identifiers and their start/placement references to valid
  64-character IDs, set caster/center max HP30000 and max MP255, with start HP30000/23456 and MP255.
  It asserts those actual names/values, geometry at 1080p/1440p/wide aspect, real Magic/target/host
  combinations and unchanged close-up layout. English/Chinese, keyboard/remapped/pad and mouse are
  bounded native checks, not a new whole-campaign or human visual acceptance claim. Retained local
  runs and completed failures use an independent `issue1686-vitals` namespace; earlier accepted
  #1693 results remain valid within their named boundary. No screenshot or player save is used.
  [Reviewed layout](../../game/probes/previews/DesignPreview.cs) ·
  [Decision cards](../../game/src/Battles/BattleMenuLayout.cs) ·
  [Decision vitals](../../game/src/UI/BattleDecisionVitals.cs) · [Gauge layout](../../game/src/UI/BattleStatusCard.cs)
- **Artwork — private F4/F8 single-cell cursor.** The complete private publication binds the
  reviewed 32×32 four-corner sprite to actual targeting and inspection, centered at nearest4×
  on a96px cell. Old private packages and public authored content retain their fallback. Native
  input/pixel/geometry checks cover the bounded consumer; foreground human appearance remains
  **Unknown**. Acting-unit brackets and area effects retain their separate meanings.
  [Publication owner](battle-scenes.md#original-target-and-inspection-cursor-publication) ·
  [#1694](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1694)
- **Human review.** Physical pad use, release after switching desktop focus, fullscreen/DPI fit
  and reading comfort in the ordinary game remain **Unknown**. The most useful checks are whether
  focus returns to the intended row and whether a player can read the next action after a long
  choice or battle result. Optional destination guidance, shorter hints and predicted turn order
  retain their separate design scope. [Human review](../verification/host.md#later-physical-focus-human-checklist-1456) ·
  [Optional guidance](modernization-design.md#optional-objective-and-destination-guidance-1578) ·
  [Context hints](presentation.md#minimal-contextual-hints-1544)

## 10. Production Church and battle command lists

**High rigor, bounded migration:** the shared design remains the agreement; this implementation
changes the existing Church and battle command presenters, their input focus and directly used
icon projection. It adds no gameplay/session authority or preview examples to the product.
The [compact list](../../game/src/UI/CompactCommandList.cs) is shared by the two production
presenters. It reserves the marker, 72×72 command icon and full 30px label within each 528px row;
rows start at 80px with 8px gaps and grow for wrapped labels. Both admitted icon frames alternate
at 250ms intervals while selected. Reduced Flash holds the selected frame. Unmounted public
commands and Search show their full text without an invented icon. The [field and shop lists](field-and-shop.md#current-composition) also use this component.

Church places the measured 560px service panel on the right, below the host safe area, with the
room under a 35% veil. The four services and separate Leave row use Up/Down; moving focus or
showing a refusal leaves the root hit areas fixed. Promotion remains unavailable. Quotes retain
the actual source-ordered member, condition, funds and exact price; Cancel declines and paid
results stay paid. Record completes the caller before the shared Save host opens. Back from that
host resumes field control; reopening services requires a fresh priest interaction.

Battle uses layout A: actual acting vitals head the right command column; round/gold stays left.
Up/Down scans Attack, Magic, Item and Stay (or the currently admitted Search replacement).
The mapped action shortcuts still open their existing commands. Spell/item lists replace the
root at its origin, use 48×72 identity icons, and retain their actual MP/slot labels, availability
and scroll behavior. Unavailable focus is inspectable and cannot commit. Target direction and
Confirm remain session-owned; Back restores the chosen spell/item, then its root command, then
Movement. The recipient card replaces the acting card while targeting. Equipment/Give/Drop
remain on their existing advertised bindings.

### Direct reproduction

The [bounded native observer](../../game/probes/engine_compact_commands_observation.gd) uses the
ordinary production presenters and installed keyboard/pad bindings. It reads state, drawn icon
identity and geometry only. It does not mutate a running session, take screenshots, copy a world,
or write player saves. Select the registered full-world/entry/battle inputs and an independently
named namespace beneath the external validation save root using the
[environment owner](../../../docs/operations/local-private-inputs.md).

After loading that environment in the launching PowerShell process:

```powershell
uv run sf2 verify adapter
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore
$env:SF2_COMPACT_CASE = 'church' # or battle
$env:SF2_COMPACT_START = $controlledStart
$env:SF2_INSPECTION_ROUTE = 'pad' # keyboard, pad, or remapped (installed keyboard bindings)
$env:SF2_VIEWPORT_OUTPUT = Join-Path $freshRun 'native.json'
& $env:GODOT_BIN --headless --path remake/game --audio-driver Dummy --max-fps 120 `
  --resolution 1920x1080 --script res://probes/engine_compact_commands_observation.gd `
  -- --input-settings $inputSettings
```

The start file is beneath this checkout's ignored `local/` and is consumed through the existing
`StartPreVoyageLocalizationObservation` before any session mounts. Church uses map3, entity0,
cell `(32,13)`, facing1, speed32 and flags `0,1,2,32,33,34,66,67,80,600,602,603`.
Battle uses `battle-1`, initial `ally-0` HP1 and `ally-2` HP0; ordinary Stay turns reach the
actual learned-spell actor. These are controlled consumer starts, not claims of natural reach.
Use English/zh-CN instant-text settings; the remapped observation used U/J/K/L, Q/B for
Up/Down/Left/Right, Confirm/Cancel. Both routes resize their own viewport to 2560×1440 and back.

For `authored-church`, derive an ignored copy of the public `localization-pilot.json`, changing
only `start.program` to `{"program":"practice-church","instruction":0}`, and pass it through
`--authored-package`. This observes the actual quoted Raise payment and held-confirm guard.
For `authored-search`, pass the existing public `battle-search.json`; its ordinary movement and
facing must produce the real Search offer, explicit confirmation and Item result. Neither authored
case invokes private admission. Outputs and supporting scripts stay worktree-local; a few serial
native cases are sufficient and each is bounded to under 60 seconds.

**Confirmed:** English/zh-CN production Church and Battle01 command consumers, keyboard, custom
bindings, pad, complete row clicks, canceled relayout gestures, two window sizes, selected source
frames and Reduced Flash. Church refusal/no-member, authored quote/decline/payment, Record host
return, actual learned HEAL target/directional return, item parent return, Stay cancellation and
authored Search substitution/result were observed. Provisional battle pages preserve resources
and both RNG seeds. Adapter Release and Debug builds compile the affected production code.

F3 uses the real ordered services, quotes and caller-to-Save return. In F4 the current layout A
shows compact acting vitals above real spell/item offers. Full statistics stay in unit details.
The [battle consumer](battle-interaction-baseline.md#production-f4f8f9-contract) owns the
subsequent composition and observations. Some native processes in the compact-list run reported
ObjectDB exit warnings (4 or 6 instances). Their cause remains **Unknown**.
