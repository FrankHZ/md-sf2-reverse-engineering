# UI Along the Player's Journey

This is the scene application of the [shared visual language](ui-design-language.md) for
[#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
The shared icon-and-name rule is fixed. Scene placements below are **candidates for review**.
The input change moves focus along the displayed list or bar. The current caller still owns
eligibility, confirm, cancel, reading and return. The wireframes show candidate bounds.

Start visual review with sections 1, 3, 5 and 8: entry, exploration dialogue, Church and battle
exercise four different compositions. Then check the remaining paths against the same rules.
The tables describe visible states, not a new UI state machine. Application owns legality and
transactions; the current view/input owners retain provisional focus, reading and Back semantics.

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

For field commands, try a short labeled list beside the visible world. Member, Magic, Item and
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
| Give/exchange / Drop | Show owner, item and recipient/destination before the existing confirmation; cancellation returns to the parent selection. Successful transfers or Drop remain completed when the result is dismissed. |
| Long names / wide values | Grow rows/detail within the reading region, preserve full counts and costs, and use existing detail scrolling. Keep fixed Prepare/Back areas outside scroll content; do not reduce all text to fit the longest metadata. |
| Cover / Back | Back unwinds action → member/pane → originating field menu or direct field entry as currently owned. Party's current host-Menu exclusion stays; no new overlay access or remote-service return is introduced. |

The [Party agreement](presentation.md#party-details-agreement) and
[field preparation owner](../application/opening-session-lifecycle.md#implemented-field-preparation)
retain the detailed capabilities. Apply the same labeled-list treatment to Use/Give/Equip/Drop
and the admitted item/recipient/return slots. Distinct copies of an item keep their slot identity
when focus moves or Back restores a choice.

## 5. Church services

**Candidate:** a vertical service list on the clear side of the priest. Each row uses the admitted
normal/selected icon and a visible localized name. Move with Up/Down. Use the existing Leave
action separately. The current game still draws a service diamond inside a shaded panel.
Compare how easily the player can identify all four services before touching a direction.

```text
     priest and room                       Church          Gold [amount]
                                           [icon] Raise
                                           [icon] Cure
                                         › [icon] Promotion   Unavailable
                                           [icon] Record adventure
                                           [actual reason for this choice]
                                           [Leave]       [mapped hints]
```

In Chinese, scan 复活 / 治疗 / 转职 / 记录冒险…（存档） before moving focus. In English, compare
full service names against the existing short icon captions. Move to an eligible service and confirm. In the same side region, replace the list with the member,
condition, exact price and Confirm/Decline prompt. Check that the priest remains identifiable while
reading the longest reason. Veil opacity and which side to use depend on that review.

| State | Layout, transaction and return |
| --- | --- |
| Root / selected service | Show each name with its original icon and focus marker on the current row. Keep current gold visible. |
| Quote / confirm | Replace the service list and use a reading panel with the actual next source-ordered member, condition, exact price and funds, followed by distinct Confirm/Decline rows. No new member picker or remote service. |
| Unavailable / refusal / no eligible member | Actual curse, promotion, funds and no-member reasons remain readable. A refusal is located where the quote/result belongs, not as a fleeting toast behind another panel. No automatic payment. |
| Long reason / name / localized hint | Grow the reading region or page the reason. Keep the focused row visible and measure the full mapped footer. Do not shrink fonts or delete hints to preserve the starting height. |
| Result / Back | Result acknowledgement proceeds to the next real offer or services. Cancel declines the quote or leaves services as the existing wait dictates; prior paid treatment is retained. Restore the prior service focus where supported. |
| Record / host overlap | Record completes the Church caller before the shared Save host opens at a legal settled point. Do not stack slots above an active ChurchWait or restore a consumed quote after Save. Other host/gameplay modals retain current exclusion. |

This is a layout recommendation over the
[Church/save agreement](presentation.md#church-and-shared-save-agreement-1519).
Promotion, curse removal and general free healing are not delivered by drawing their labels.

## 6. Shop: stock, recipient and price before commitment

Try the Church vertical list for Buy, Sell, Repair and Deals, with their admitted original service
icons, localized names and existing reasons. Keep the merchant visible. The stock or owned-item list sits left; selected item, real price,
funds and destination/member detail sit right. The quote/confirmation replaces that working view
when the transaction requires undivided attention.

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

Use the [battle feature trace](battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
for current source, decision/return, real layer consumers, Content guards, connected acceptance and
uncovered implementation scopes. The following is #1616's tracked **Proposed** composition, not
deployed UI or user visual approval; visual coverage does not close those functional gaps.

The first visual question is “whose decision is this, and what am I considering?” The second is
“what will it cost or affect?” Round and gold are useful context, not the primary headline.
Keep the center available for the board and the currently considered subject. Use the measured
region budgets and host exclusion from the [language page](ui-design-language.md#layout-that-preserves-the-world).

```text
  Round 3 · Gold 1,250                                        [ Menu ]
  +--------------------+                      +--------------------+
  | Acting member      |                      | Current target     |
  | HP 24/30  MP 8/12   |                      | HP 18/18  MP —     |
  +--------------------+                      +--------------------+

                       BOARD / VISIBLE TARGET
                  range, cursor and actor stay readable

  +--------------------+  +----------------------------------------+
  | Spell list         |  | Selected spell · actual level / MP cost |
  | › selected offer   |  | Current target / affected area          |
  |   unavailable row  |  | Refusal reason when not legal           |
  +--------------------+  | existing target / Confirm / Back actions|
                          +----------------------------------------+
```

This schematic shows the information grouping across list and target steps. The list is visible
only while its page owns selection; on the target page it yields to the board/target controls.
Numbers are illustrative. Here is the candidate root action bar for the phase *before* that list.
Each [icon] is the command's own admitted normal or selected image.

```text
             acting unit / movement preview remains visible on the board

  [icon] Attack     › [icon] Magic      [icon] Item      [icon] Stay
  [actual refusal reason if needed]                     [mapped hints]
```

Read all four names at rest, including 攻击 / 魔法 / 道具 / 待机 in the Chinese layout. Left/Right moves between commands. Confirm on Magic replaces the
bar with the spell list; Back from that list restores Magic. On the target page the board owns
directional input. Compare the extra presses needed to reach Stay with the board space saved by
one row. Use a vertical alternative if the supported languages cannot fit or targeting is obscured.

### From entry to committed action

| Decision phase | Recommended visible composition and focus | Invalid/long state and return |
| --- | --- | --- |
| Battle entry / before player control | Present the existing scene/intro dialogue and real entry status. No commands while the script owns play. | Long intro uses the story reader; existing Skip appears only when eligible. No new deployment or pre-battle equipment screen is inferred. Host cover preserves delivery. |
| Movement | Acting card plus quiet round/gold; legal range and current preview/cursor on the board. The player's attention stays on the proposed cell. | Preserve actual range/refusal feedback; no invented path legality. Confirm opens actions. Current Cancel and movement-preview behavior remain session-owned. [#1615](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1615) owns held-direction repeat; this layout does not implement it. |
| Main action | Four labeled command items in a horizontal bar. Show a detail only for a cost, consequence or refusal. At a registered faced chest, the fourth item becomes Search under the [existing admission](../content/profiles-and-trust.md#battle-search-admission). Its authored text fallback remains available. | Disabled focus is inspectable and cannot commit. Long reason grows/pages the detail group. Back returns to movement and undoes the preview through the current Cancel contract. Stay retains explicit confirmation. |
| Spell / item list | List replaces bar; selected offer shows actual spell level/cost or item/slot. Acting card remains; no stale target card. | Rows include actual unavailable reasons, scroll and retain focus. Confirm enters only an enabled offer. Back restores the same command focus with no resource/RNG change. |
| Target / affected area | Acting and current target cards, board cursor/affected set, selected action and cost, actual target/Confirm/Back controls. List yields its space; detail names the considered target. | Disabled candidate stays identifiable and cannot confirm an earlier valid target. Long name uses the card policy; detail uses reading space. Back returns to the spell/item list or main action and clears provisional target through the existing return command. |
| Equipment / Give / Drop | Reuse the auxiliary presenter and advertised mapped shortcuts from action choice; show the actual candidate, owner/recipient or consequence in one reading panel. | No simultaneous unrelated equipment/item/spell stacks. Preserve read-before-confirm and cancellation. Product connected Equip/Give have separate [Content admission](../content/profiles-and-trust.md#connected-battle-give-admission); [Drop](../content/profiles-and-trust.md#connected-battle-discard-admission) binds its own finite free transaction. Controlled field-only equipment bindings remain immutable. This layout grants no new mouse coverage or transaction capability. |
| Commit | The explicit current command owns payment/action commitment; UI retires the provisional pages and switches to the real presentation phase. | A refusal/stale offer remains with its current responsible view and reason; do not invent success or silently submit a previous choice. Returning held directions/Confirm cannot act on the next phase. |

### Viewing the battlefield

During free inspection, pointer hover and directional focus reveal the same short status.
Put the unit name and its land-effect percentage on the first line, with the percentage at
the trailing edge. The next row gives HP and MP equal space for values and gauges. The
location sits below them. Empty ground shows the location alone. Confirm on a unit opens
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
and the enemy's at the **lower left**. The command layout belongs to the board decision phase.

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
| 1920×1080, English and zh-CN | The four pilot scenes retain body/hint sizes, correct source-icon scale, readable reasons and distinct input owners. Every command name is readable before focus moves. Compare Church's vertical services and battle's horizontal bar, including their respective Up/Down and Left/Right hints. |
| Long admitted text, fallback note, wide numbers | Choice/body/footer stay disjoint; prices and HP/MP digits remain complete. Reading/page/scrolling reaches all content before commitment. A short translated label does not conceal an English fallback overflow. |
| Two widest allowed battle cards + host Menu + long action reason | Use the explicit top reservations and separate bottom group. Actual measured bounds, not intended sizes, must be disjoint. Target/preview remains visible. |
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
| 1. Four-scene visual pilot, with entry styling owned by #1606 | `remake/game/src/UI/StoryTheme.cs`; `remake/game/src/Entry/SessionEntryView.cs`; `remake/game/src/Entry/EntryLayout.cs`; `remake/game/src/Exploration/ExplorationDialogueView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Battles/BattleMenuLayout.cs`; `remake/game/src/Battles/BattleCommandMenu.cs` | Compare both candidates with full labels and original-size icons before freezing geometry. Keep axis navigation in the existing input owners. #1606 owns entry styling. Observe dialogue as a reading reference. Stop at human review before sibling migration. |
| 2. Battle decision composition under #1616 | `remake/game/src/Battles/BattleMenuLayout.cs`; `remake/game/src/Battles/BattleCommandMenu.cs`; `remake/game/src/Battles/BattleSessionView.cs`; `remake/game/src/UI/BattleStatusCard.cs` | Consume the reviewed pilot and the [display contract](display-composition-contract.md), including close-up art/card resize at the retained phase under pause. Complete counter/host exclusion, card/list/target/auxiliary-page combinations and returns. No engine rules or inspection feature. |
| 3. Field and service siblings under #1616 | `remake/game/src/Exploration/FieldMenuView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Exploration/ShopServiceView.cs`; `remake/game/src/Exploration/PartyStatusView.cs`; `remake/game/src/Exploration/PartyDetailsPanel.cs`; `remake/game/src/Exploration/FieldActionView.cs` | Consume the accepted shared theme with one writer. Keep each transaction/lifecycle owner intact; split Party/preparation from services if they cannot be reviewed as one coherent layout change. Stop at existing admitted consumers. |
| 4. Remaining entry/host surfaces, preserving #1606 ownership | `remake/game/src/Entry/SaveSlotView.cs`; `remake/game/src/Entry/NameEntryView.cs`; `remake/game/src/Entry/SystemMenuView.cs`; `remake/game/src/UI/SettingsView.cs`; `remake/game/src/GameRoot.cs`; `remake/game/src/Entry/OpeningPersistenceHost.cs`; `remake/game/src/Entry/WindowSupportHost.cs` | Accepted pilot; preserve actual save/settings/recovery predicates and the existing owned instance. `GameRoot`/persistence changes require a specific demonstrated layout need, not a second navigation service. No save format or user-save experiment. |
| 5. Remaining labeled options | `remake/game/src/Battles/BattleInspectionView.cs` and `remake/game/src/Exploration/FieldActionView.cs` | Apply the reviewed component to the existing inspection/options and item-operation entry points. Preserve admitted artwork, child-page returns and save/transaction authority. |

Each future code slice updates the closest current agreement and uses its scoped adapter/behavior
checks and actual state/input/geometry/error observations. New engine behavior warrants small
behavior tests; a visual change does not justify tests of probes or a full legacy suite. Use existing
tools/instances and disposable evidence namespaces, never screenshots or current user saves.
Main-gate retains independent acceptance and integration; user visual acceptance remains separate.

The composition and inspection contracts constrain these layouts. **Unknown:** final bilingual fit,
controller comfort, room/board visibility and human acceptance. The next review compares four
scenes with those criteria. #1578 guidance, #1544 hint changes and an action-order strip remain
separate. Product implementation requires its own bounded dispatch.
