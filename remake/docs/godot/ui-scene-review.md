# UI Along the Player's Journey

This is the scene application of the [shared visual language](ui-design-language.md) for
[#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
The user accepted the shared components and scene layouts shown in the Godot preview.
The tables below record how they apply to the production journey. The gameplay or host
caller owns eligibility, transaction state and return.

### Preview coverage

| Review entry | Interaction exercised | Related states outside the preview |
| --- | --- | --- |
| F1 Witch, slots and name | New Game, keyboard name validation, occupied slot and backup choice | Title entrance, real storage errors and fresh-process restore use their production owners. |
| F2 dialogue / F7 field | Speech with an attached question, contextual Search, member, spell and item lists | Scripted scene transitions use the same reader. Field chest acquisition and full-pack messages need their actual script. |
| F3 Church / F6 shop | Service selection, refusal and quote, Record to slots, purchase and sale with recipient, price and pack | Promotion, Repair and Deals are unavailable in these services. |
| F5 party and preparation | Member details, four fixed slots, recipient selection, Give and full-pack exchange | Field Equip and Use retain their content-specific eligibility. |
| F4 battle decision and playback | Movement, rod attack, HEAL levels, item, target cycle, physical and healing close-ups | Battle Search at a chest, battle Equip/Give/Drop, area spells and special attacks need their own live consumers. |
| F8 inspection / F9 results | Hover terrain or units, details, overview map, speed and quit; victory/growth, retreat and defeat | Result and return sequences in the preview are staged. |
| F10 host | Resume, settings draft, shared slots, quit and simulated window recovery | Full controls help, operating-system resize and real save failures need host verification. |

The existing [Caravan travel scene](../capability-status.md#authored-caravan-agreement) uses
field movement. Caravan depot and party recruitment services fall outside this review.
The [Godot preview PR](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1684)
has the runnable controls for all ten entries.

## 1. Title and the Witch

**Components:** entry greeting → vertical text choices. Compare the selected row beside the greeting
frame in English and Chinese. Both keep the same straight edge, and the active row alone has a focus
marker. #1606 owns implementation of the entry controls.

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
| Covered / Back | Settings uses its existing allowed entry path and restores the same entry phase. A held Confirm or mouse release cannot select a new game on return. Title Cancel retains its Settings meaning. |

[#1606](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1606) owns the entry styling
correction after design selection. The [entry agreement](presentation.md#entry-windows-and-hints-1445)
owns actual sizes, input and prior engineering observations; their existence is not visual approval.

## 2. New Game, name and slot choice

Reuse the same right-column footprint while the Witch is present. Show a short heading, the current
step, choices/editor, then a fixed action/help area. Never compress the Witch to make more room.
Slot cards identify the actual slot, occupant and supported continuation information; unavailable
or corrupt data does not masquerade as an empty slot. The current two-slot design remains two slots.

| State | Layout and return rule |
| --- | --- |
| Choose slot / focus | Two clearly separated measured rows; marker beside the current row. Long occupant and location information wraps before the confirmation/footer. Opening a slot does not overwrite it. |
| Name editing | Focused editor has the shared gold outline, visible draft, actual admitted alphabet/length help and explicit keyboard-only notice. Keep the current seven-character validation and physical-keyboard Enter/Escape editing semantics; this proposal does not enable Chinese names or controller typing. |
| Invalid name / failed storage | Error sits immediately below the editor or affected slot/action, before the fixed buttons. Keep the draft and safe Retry/Back where currently supported. Do not erase the name or consume the final Start confirmation. |
| Occupied slot / final Start | Separate confirmation names the destination and consequence; Cancel is initially selected where the existing lifecycle requires it. No pointer-down or returning held key can confirm. |
| Long labels / narrow entry window | Wrap and scroll/read within the existing scaled column. Keep full destination and overwrite wording; existing 1280×720 entry support does not imply native 720p gameplay. |
| Back / overlay | Return through the existing slot/name/setup phases with retained draft. Native focus recovery restores editor focus without editing text. System/window recovery covers the editor and consumes its own inputs first. |

This is presentation of the [slot/name lifecycle](../application/opening-session-lifecycle.md#slot-name-and-initial-save-design-1443),
not a change to initial-save publication, backup recovery or format compatibility.

## 3. Exploration: world, speech, choices and Search

**Components:** subject-aware dialogue frame → attached choice only when the script asks. Use this
scene as the reading reference for the pilot. Compare a short portraitless line with a long question
and portrait before adopting the same frame elsewhere.

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
| Long source lines / translated text | Preserve meaningful Search line breaks, the current portrait column and whole-row fallback notices. Grow/page the body, then keep the choices and footer reserved. A full-pack refusal is not clipped to make room for an item name. |
| Closing / next speaker | Close portrait and frame as one visible result when the owning close is due. The [coordinated exit](presentation.md#coordinated-dialogue-exit-1613) keeps the portrait mounted through the existing wait and retires it with the window. |
| Host cover / return | An allowed Menu covers the reading state without advancing it. Return restores that same phase and page; release barriers prevent an extra advance. A stopped scene displays its real failure over world fades, not an invented Continue. |

Field commands use the compact right-hand list beside the visible world. Member, Magic, Item and
Search retain their original art and admission. Show Search's actual reason when it is unavailable.
Back from preparation restores the initiating choice, then Back returns to field control. The
[field menu agreement](presentation.md#exploration-gameplay-diamond-agreement-1588) owns deployed
cardinal input, pause and the member-before-item-operation flow. Optional guidance under #1578
uses its own proposal and can follow this pilot.

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

The [modernization proposal's capability table](modernization-design.md#what-can-actually-be-reused)
distinguishes the finite shop consumer from remote-service recommendations. Reuse the transaction;
this design does not claim every full-game shop is connected.

## 7. Host Menu, Settings, Save and recovery

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
battle destination labels and fresh-process continuation. This visual proposal preserves that
behavior; it does not reopen save design or establish human acceptance of these layouts.
The current user slot and backup must never be a design-validation fixture.

## 8. Battle: one complete decision, then its consequences

The [battle feature trace](battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
owns gameplay and source evidence. Use **layout A** on the command board. Place the acting
unit and HP/MP at the head of the right-hand panel. The four commands form a vertical list.
The B comparison places vitals at the bottom and remains available in F4.

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
| Movement | Acting card plus quiet round/gold; legal range and current preview/cursor on the board. The player's attention stays on the proposed cell. | Preserve actual range/refusal feedback; no invented path legality. Confirm opens actions. Current Cancel and movement-preview behavior remain session-owned. [#1615](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1615) owns held-direction repeat; this layout does not implement it. |
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
outer edge. The reviewed fill pulses between 53% and 68% opacity over 1.2 seconds. Motion
off holds it at approximately 61%. These are preview values. The original varies the tile
palette during its pulse.

Up/Left selects the previous eligible combatant and Down/Right the next, wrapping within the action's living, in-range
target list. Hover and click use that same list. A single candidate stays selected. With none,
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
| Physical / item / spell close-up | Hide the board under the existing close-up rule. Show only actual acting/current reaction-target cards and the current message; retire command panels and round/gold strip. Character/weapon/effects stay in their own local composition below UI. | Costs and HP/MP update at the real committed phases. Multi-target reactions switch the target card at that boundary. Long messages read/page without premature acknowledgement. [#1620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1620) investigates weapon overlap; no global Z flip or new pose here. |
| Quiet support / suppressed / no-effect | Existing message/card presentation may have a quiet black backdrop. Keep the actual reason and recipient readable. | Do not fabricate an attack animation to fill silence. Preserve source/delivery semantics and supported effects; a quiet frame is not evidence of a stuck action. |
| Death / after-turn notice / automatic enemy turn | Restore the board for field-owned removal and notices; show only relevant status/text. Player command controls are absent while automatic progression owns play. | Low/zero HP is explicit; cards never retain a departed target into the next action. Message delivery, removal completion and focus suspension remain under current owners. |
| Reward / growth / victory / loss / retreat | Show the existing ordered results and current return prompt in the same reading family. No new duplicate reward summary or clickable future actor strip. | Wide amounts and multiple growth lines use reading pages. Closing cannot replay rewards. Retreat payment/return and special victory/loss programs keep their owners. |
| Back to player or field control | Restore live status/context and only the legal input owner after the real presentation/program finishes. Field returns use field chrome, not a lingering battle HUD. | Preserve failure feedback if return stops. An animation ending is not proof that exploration can move or save. No Return button bypasses the current continuation. |
| Host Menu / recovery during any permitted phase | Host owns pause/input and covers battle UI; recovery hides covered host pages first. | Close to the same actor/phase/page. Input release cannot move, confirm, acknowledge or start the next actor. Cards and map visibility must restore coherently without reconstructing gameplay. |

The [battle-scene owner](battle-scenes.md#message-and-outcome-presentation) and
[current status-card contract](presentation.md#battle-status-card-agreement) own committed values,
scene pairing and delivery. [#1617](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1617)
separately owns the selected-sprite gold tint and path underline versus original evidence. Retain
legible selection/range during that work; this proposal does not endorse the current tint as original
or declare a new cursor animation. [#1621](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1621)
owns layer/composition responsibility; layout cannot replace its contract.

## 9. Combination review and implementation boundaries

Review the visual choices as a set, then test these combinations on each affected real consumer
when implementation is separately authorized. This is a finite coverage guide, not a new fixture
platform, full campaign replay or demand to test every possible state cross-product.

| Combination | What the design must make possible |
| --- | --- |
| 1920×1080, English and zh-CN | The reviewed scene families retain body/hint sizes, compact source icons and readable reasons. Check Up/Down on field, Church, shop and battle lists and directional target cycling on the board. |
| Long admitted text, fallback note, wide numbers | Choice/body/footer stay disjoint; prices and HP/MP digits remain complete. Reading/page/scrolling reaches all content before commitment. A short translated label does not conceal an English fallback overflow. |
| Wide battle status + host Menu + long action reason | Measure the right-hand combined panel, host reservation and footer together. Actual measured bounds, not intended sizes, must be disjoint. Target/preview remains visible. |
| 2560×1440, native 4K and an already supported wide aspect | Edge anchors follow the viewport, centered reading panels keep bounded width, and the world remains 96px. Check the selected offer and subject remain visible after resize. |
| Keyboard / supported pad / existing mouse path | Same current action and reason; actual remap/swap hints; disabled focus never activates; pointer press/release is cancelled by relayout/cover. Name entry remains the stated keyboard exception, and auxiliary battle controls keep their present device coverage. |
| Menu → subpage → recovery → return | Exactly one active input owner; covered controls cannot click through. Focus, reading and provisional selection return intact after release, with no gameplay commit or unintended advance. |
| Portrait close / scene switch / field return | Visible groups retire coherently at their actual lifecycle boundaries. No stale target card, Church quote, command list/bar or battle frame survives into its successor. |

The following are **candidate bounded implementation slices**, not authorization, new Issues or
directory-wide ownership. Paths are relative to the repository. At dispatch, freeze the minimal
subset against accepted main; existing behavior tickets keep their own scope and evidence. The
coordinator serializes shared paths, especially `StoryTheme`, `GameRoot` and `presentation.md`.

| Order / outcome | Exact candidate product paths | Dependency and stopping condition |
| --- | --- | --- |
| 1. Four-scene visual pilot, with entry styling owned by #1606 | `remake/game/src/UI/StoryTheme.cs`; `remake/game/src/Entry/SessionEntryView.cs`; `remake/game/src/Entry/EntryLayout.cs`; `remake/game/src/Exploration/ExplorationDialogueView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Battles/BattleMenuLayout.cs`; `remake/game/src/Battles/BattleCommandMenu.cs` | Implement the reviewed right-column commands with full labels and nearest 3× command icons. Keep axis navigation in the existing input owners. #1606 owns entry styling. Observe dialogue as a reading reference. Stop at human review before sibling migration. |
| 2. Battle decision composition under #1616 | `remake/game/src/Battles/BattleMenuLayout.cs`; `remake/game/src/Battles/BattleCommandMenu.cs`; `remake/game/src/Battles/BattleSessionView.cs`; `remake/game/src/UI/BattleStatusCard.cs` | Consume the reviewed pilot and the [display contract](display-composition-contract.md), including close-up art/card resize at the retained phase under pause. Complete counter/host exclusion, card/list/target/auxiliary-page combinations and returns. No engine rules or inspection feature. |
| 3. Field and service siblings under #1616 | `remake/game/src/Exploration/FieldMenuView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Exploration/ShopServiceView.cs`; `remake/game/src/Exploration/PartyStatusView.cs`; `remake/game/src/Exploration/PartyDetailsPanel.cs`; `remake/game/src/Exploration/FieldActionView.cs` | Consume the accepted shared theme with one writer. Keep each transaction/lifecycle owner intact; split Party/preparation from services if they cannot be reviewed as one coherent layout change. Stop at existing admitted consumers. |
| 4. Remaining entry/host surfaces, preserving #1606 ownership | `remake/game/src/Entry/SaveSlotView.cs`; `remake/game/src/Entry/NameEntryView.cs`; `remake/game/src/Entry/SystemMenuView.cs`; `remake/game/src/UI/SettingsView.cs`; `remake/game/src/GameRoot.cs`; `remake/game/src/Entry/OpeningPersistenceHost.cs`; `remake/game/src/Entry/WindowSupportHost.cs` | Accepted pilot; preserve actual save/settings/recovery predicates and the existing owned instance. `GameRoot`/persistence changes require a specific demonstrated layout need, not a second navigation service. No save format or user-save experiment. |
| 5. Remaining labeled options | `remake/game/src/Battles/BattleInspectionView.cs` and `remake/game/src/Exploration/FieldActionView.cs` | Apply the reviewed component to the existing inspection/options and item-operation entry points. Preserve admitted artwork, child-page returns and save/transaction authority. |

Each future code slice updates the closest current agreement and uses its scoped adapter/behavior
checks and actual state/input/geometry/error observations. New engine behavior warrants small
behavior tests; a visual change does not justify tests of probes or a full legacy suite. Use existing
tools/instances and disposable evidence namespaces, never screenshots or current user saves.
Main-gate retains independent acceptance and integration; user visual acceptance remains separate.

**Unknown:** production fit with long localized text, other supported window sizes, and the
live gameplay and host-only states listed in [Preview coverage](#preview-coverage).
#1578 guidance, #1544 hint changes and an action-order strip retain separate scope.

Product implementation requires its own bounded dispatch.

## 10. Production Church and battle command lists

**High rigor, bounded migration:** the shared design remains the agreement; this implementation
changes the existing Church and battle command presenters, their input focus and directly used
icon projection. It adds no gameplay/session authority or preview examples to the product.
The [compact list](../../game/src/UI/CompactCommandList.cs) is shared by the two production
presenters. It reserves the marker, 72×72 command icon and full 30px label within each 528px row;
rows start at 80px with 8px gaps and grow for wrapped labels. Both admitted icon frames alternate
at 250ms intervals while selected. Reduced Flash holds the selected frame. Unmounted public
commands and Search show their full text without an invented icon. Existing diamond consumers
retain their previous rendering until their own migration.

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

Compared with the four-scene preview, F3 now uses real services, ordered quotes and caller-to-Save
return rather than staged sample results. F4 retains the production status card's class/level/terrain
body and separate command/detail frames; the preview uses one more compact combined heading.
Its actual learned spell/item rows replace staged offers. F1 and F2 remain under #1606 and are not
certified by this slice. The broader battle composition still needs its own production review.

Remaining demo UI scope: entry/reading migration belongs to #1606; target/range/inspection,
auxiliary battle pages, close-ups and outcome/retreat-return composition to #1686; field/party/shop
to #1687; host/settings/save composition to #1688. This command migration does not make the staged
F1–F10 preview a production campaign acceptance. Wider name/stat combinations, special/area
presentations and foreground human visual coordination remain **Unknown** outside the observations
above. Some native processes reported existing ObjectDB exit leaks (4 or 6 instances); clean
shutdown cause remains **Unknown**, even where the consumer checks passed.
