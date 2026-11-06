# UI Along the Player's Journey

This is the scene application of the [shared visual language](ui-design-language.md) for
[#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616).
All layouts here are **proposed**. They preserve existing behavior unless a paragraph explicitly
identifies a future capability pending another owner. They are authored wireframes, not screenshots
or claims that the current product already has these bounds.

Start visual review with sections 1, 3, 5 and 8: entry, exploration dialogue, Church and battle
exercise four different compositions. Then check the remaining paths against the same rules.
The tables describe visible states, not a new UI state machine. Application owns legality and
transactions; the current view/input owners retain provisional focus, reading and Back semantics.

## 1. Title and the Witch

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

Ordinary field control has an unobstructed world, the existing right-top Menu and the currently
legal context hint. No new quest bar, minimap or resource dashboard is added. The Adventure entry
and Cancel retain their gameplay-diamond route; Talk/Search still depend on the actual faced context.

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
| Closing / next speaker | Close portrait and frame as one visible result when the owning close is due. [#1613](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1613) must resolve real lifecycle ordering without early acknowledgement or hiding required script waits. This design does not declare that defect fixed. |
| Host cover / return | An allowed Menu covers the reading state without advancing it. Return restores that same phase and page; release barriers prevent an extra advance. A stopped scene displays its real failure over world fades, not an invented Continue. |

For the field diamond, use the freestanding lower-side group from the language page. Up Member,
Left Magic, Right Item, Down Search remain fixed. Selected name and actual unavailable reason sit
beside it in a separate frame; no enclosing blue tray or new remote-service command. Back from
member/preparation restores the same diamond selection, then Back closes to field control.
Church, Shop, story and battle cannot be opened concurrently with it. The current
[field menu agreement](presentation.md#exploration-gameplay-diamond-agreement-1588) owns input,
pause and its deliberate member-before-item-operation flow. [#1608](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1608)
owns Search reflow independently.

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
| Cover / Back | Back unwinds action → member/pane → originating diamond or direct field entry as currently owned. Party's current host-Menu exclusion stays; no new overlay access or remote-service return is introduced. |

The [Party agreement](presentation.md#party-details-agreement) and
[field preparation owner](../application/opening-session-lifecycle.md#implemented-field-preparation)
retain the detailed capabilities. Authored-only examples do not establish private/connected support.

## 5. Church: a freestanding service diamond

The priest and room should still read as the context. Recommend the restrained service veil from
the language page. The diamond and description are sibling scene controls, with **no common blue
background**. At 1080p, start with the diamond at `(320,432,288,224)`, the separate detail frame at
`(640,360,960,400)`, and Leave at `(320,680,288,64)`. A 32px horizontal gap remains between diamond
and frame. Measure actual hints before final sizing; align the whole group within the safe area.

```text
                    PRIEST / ROOM, still visible

           [Raise]         +-----------------------------------------+
     [Cure]    [Promotion] | Church · current gold                    |
           [Record]        | Promotion — Unavailable                  |
                           | Actual supported-service reason         |
          [Leave]          | Existing mapped service hints            |
                           +-----------------------------------------+
   four original icons          one separate selected-service frame
```

This sketch deliberately focuses an unavailable service. Focus must remain obvious without gold
available text or a new transaction. The real root maps Up Raise, Left Cure, Right Promotion,
Down Record adventure. Leave is separate; it is not a fifth point on the diamond.

| State | Layout, transaction and return |
| --- | --- |
| Root / selected service | Keep original normal/selected art and cardinal relationships. Name, availability and current gold are beside the diamond, not repeated above each icon. |
| Quote / confirm | Hide the root diamond and use a reading panel with the actual next source-ordered member, condition, exact price and funds, followed by distinct Confirm/Decline rows. No new member picker or remote service. |
| Unavailable / refusal / no eligible member | Actual curse, promotion, funds and no-member reasons remain readable. A refusal is located where the quote/result belongs, not as a fleeting toast behind another panel. No automatic payment. |
| Long reason / name / localized hint | Grow the separate description frame or use its existing reading region. The diamond stays outside; the existing potentially multi-line footer gets measured space. Do not shrink fonts or delete hints to preserve the starting height. |
| Result / Back | Result acknowledgement proceeds to the next real offer or services. Cancel declines the quote or leaves services as the existing wait dictates; prior paid treatment is retained. Restore the prior service focus where supported. |
| Record / host overlap | Record completes the Church caller before the shared Save host opens at a legal settled point. Do not stack slots above an active ChurchWait or restore a consumed quote after Save. Other host/gameplay modals retain current exclusion. |

This is a layout recommendation over the
[Church/save agreement](presentation.md#church-and-shared-save-agreement-1519).
Promotion, curse removal and general free healing are not delivered by drawing their labels.

## 6. Shop: stock, recipient and price before commitment

Use the same reading/action surfaces as Church; retain the existing shop action selector rather
than inventing another diamond without a scope decision. Keep the merchant/world context visible
under the same service veil. The stock or owned-item list sits left; selected item, real price,
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
The current user slot and backup must never be a design-validation fixture.

## 8. Battle: one complete decision, then its consequences

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
Numbers are illustrative, not a fixture or a requirement for legal play. Root commands instead use
the independent diamond shown on the language page; never place it inside the message frame.

### From entry to committed action

| Decision phase | Recommended visible composition and focus | Invalid/long state and return |
| --- | --- | --- |
| Battle entry / before player control | Present the existing scene/intro dialogue and real entry status. No commands while the script owns play. | Long intro uses the story reader; existing Skip appears only when eligible. No new deployment or pre-battle equipment screen is inferred. Host cover preserves delivery. |
| Movement | Acting card plus quiet round/gold; legal range and current preview/cursor on the board. The player's attention stays on the proposed cell. | Preserve actual range/refusal feedback; no invented path legality. Confirm opens actions. Current Cancel and movement-preview behavior remain session-owned. [#1615](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1615) owns held-direction repeat; this layout does not implement it. |
| Main action | Freestanding Up Attack / Left Magic / Right Item / Down Stay diamond, with selected name/reason in the adjacent frame. Original Search substitution remains a source-specific admission question. | Disabled focus is inspectable and cannot commit. Long reason grows/pages the detail group. Back returns to movement and undoes the preview through the current Cancel contract. Stay retains explicit confirmation. |
| Spell / item list | List replaces diamond; selected offer shows actual spell level/cost or item/slot. Acting card remains; no stale target card. | Rows include actual unavailable reasons, scroll and retain focus. Confirm enters only an enabled offer. Back restores the same cardinal focus with no resource/RNG change. |
| Target / affected area | Acting and current target cards, board cursor/affected set, selected action and cost, actual target/Confirm/Back controls. List yields its space; detail names the considered target. | Disabled candidate stays identifiable and cannot confirm an earlier valid target. Long name uses the card policy; detail uses reading space. Back returns to the spell/item list or main action and clears provisional target through the existing return command. |
| Equipment / Give / Drop | Retain the current auxiliary selection presenter and shortcuts from action choice; show the actual candidate, owner/recipient or consequence in one reading panel. | No simultaneous unrelated equipment/item/spell stacks. Preserve existing read-before-confirm and cancellation; Drop confirmation and supported transaction limits remain. Do not promise new mouse coverage or private Give/Drop admission. |
| Commit | The explicit current command owns payment/action commitment; UI retires the provisional pages and switches to the real presentation phase. | A refusal/stale offer remains with its current responsible view and reason; do not invent success or silently submit a previous choice. Returning held directions/Confirm cannot act on the next phase. |

### Viewing the battlefield is an original-feature question

The [SEGA USA manual, “Special Game Options”](https://www.sega.jp/genesismini2/assets/manual/pdf/US_Shining-Force-II.pdf)
describes B free-cursor viewing during battle, A for full ally/enemy information, C for status and B
to return; an empty-ground menu offers Map, Member, Quit and Speed, with Map showing the battlefield
and Quit saving. **Confirmed at manual-description level**, not exact source-call, engine-support or
modern-input equivalence. [#1622](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1622)
owns that source/capability closure. Do not reclassify this whole feature as a new modern convenience.

Reserve this proposed layout and interaction seam, without adding a working control in this slice:

```text
  [Viewing battlefield]                 [viewed unit / terrain if admitted]
                 freely inspected board cursor
       empty-ground options: Map · Member · Quit · Speed
  [existing mapped inspection actions after #1622]              [Back]

  Map page: whole-board schematic | legend / selected known unit | Back
```

The mode banner distinguishes inspection from movement. A viewed unit card must say which unit is
being inspected rather than masquerading as an attack target. Full details reuse the Party/card
visual grouping; a whole-board page gets a bounded map region and legend, never a tiny world zoom
behind live command controls. Long unit/terrain text scrolls in its own detail area. Unknown or
undisclosed information is absent or explicitly unavailable, not invented from hidden future AI.

**Unknown:** precise entry phases, modern binding, empty-ground cardinal order, disclosed terrain
fields and camera restoration. Existing mapped Cancel already unwinds provisional battle actions;
do not steal it because the manual says B. #1622 must settle the entry/return contract and preservation
of actor, movement preview, camera/selection, action count and RNG before implementation. Reserve a
visible inspection affordance in the movement context area, with its eventual mapped hint; leave it
out of the current working UI until those semantics are accepted. Map/Member/Quit/Speed are distinct
source functions, not automatically four enabled remake commands. Battle saving belongs to
[#1618](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1618). The original Speed entry
concerns text speed / battle-message settings; #1622 must compare its exact options with existing
Settings. [#1619](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1619) separately owns
the proposed Shift-held 1.5× simulation/presentation acceleration, not implementation of the original
Speed entry. Final labels, bindings and eligibility remain with their respective behavior owners.

The prior [host Overview recommendation](modernization-design.md#read-only-battle-overview) remains
a separate unimplemented proposal. Prefer first closing the original inspection path; do not add a
second competing entry now. An action-order strip is **Deferred**. No predicted sequence, AGI sort
or future-RNG disclosure is required for this design or its first implementation.

### Presentation, outcome and usable return

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
| 1920×1080, English and zh-CN | The four pilot scenes retain body/hint sizes, correct source-icon scale, readable reasons and distinct input owners. Church/field/battle diamonds are outside enclosing reading panels. |
| Long admitted text, fallback note, wide numbers | Choice/body/footer stay disjoint; prices and HP/MP digits remain complete. Reading/page/scrolling reaches all content before commitment. A short translated label does not conceal an English fallback overflow. |
| Two widest allowed battle cards + host Menu + long action reason | Use the explicit top reservations and separate bottom group. Actual measured bounds, not intended sizes, must be disjoint. Target/preview remains visible. |
| 2560×1440, native 4K and an already supported wide aspect | Edge anchors follow the viewport, centered reading panels keep bounded width, and the world remains 96px. Current resize defects stay explicit under #1614; no new subminimum policy. |
| Keyboard / supported pad / existing mouse path | Same current action and reason; actual remap/swap hints; disabled focus never activates; pointer press/release is cancelled by relayout/cover. Name entry remains the stated keyboard exception, and auxiliary battle controls keep their present device coverage. |
| Menu → subpage → recovery → return | Exactly one active input owner; covered controls cannot click through. Focus, reading and provisional selection return intact after release, with no gameplay commit or unintended advance. |
| Portrait close / scene switch / field return | Visible groups retire coherently at their actual lifecycle boundaries. No stale target card, Church quote, gameplay diamond or battle frame survives into its successor. |

The following are **candidate bounded implementation slices**, not authorization, new Issues or
directory-wide ownership. Paths are relative to the repository. At dispatch, freeze the minimal
subset against accepted main; existing behavior tickets keep their own scope and evidence. The
coordinator serializes shared paths, especially `StoryTheme`, `GameRoot` and `presentation.md`.

| Order / outcome | Exact candidate product paths | Dependency and stopping condition |
| --- | --- | --- |
| 1. Four-scene visual pilot, with entry styling owned by #1606 | `remake/game/src/UI/StoryTheme.cs`; `remake/game/src/Entry/SessionEntryView.cs`; `remake/game/src/Entry/EntryLayout.cs`; `remake/game/src/Exploration/ExplorationDialogueView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Battles/BattleMenuLayout.cs` | First select the shared visual choices. Reuse the existing widgets; read-only regression check on unchanged dialogue is preferable to editing it. Pilot entry, dialogue, Church root and battle root together; stop for visual review before sibling migration. #1613 lifecycle changes are not folded into the style pilot. |
| 2. Battle decision composition under #1616 | `remake/game/src/Battles/BattleMenuLayout.cs`; `remake/game/src/Battles/BattleCommandMenu.cs`; `remake/game/src/Battles/BattleSessionView.cs`; `remake/game/src/UI/BattleStatusCard.cs` | Accepted pilot plus #1621's relevant responsibility findings before changing composition assumptions. Complete counter/host exclusion, card/list/target/auxiliary-page combinations and returns. No engine rules or inspection feature. |
| 3. Field and service siblings under #1616 | `remake/game/src/Exploration/FieldMenuView.cs`; `remake/game/src/Exploration/ChurchServiceView.cs`; `remake/game/src/Exploration/ShopServiceView.cs`; `remake/game/src/Exploration/PartyStatusView.cs`; `remake/game/src/Exploration/PartyDetailsPanel.cs`; `remake/game/src/Exploration/FieldActionView.cs` | Consume the accepted shared theme with one writer. Keep each transaction/lifecycle owner intact; split Party/preparation from services if they cannot be reviewed as one coherent layout change. Stop at existing admitted consumers. |
| 4. Remaining entry/host surfaces, preserving #1606 ownership | `remake/game/src/Entry/SaveSlotView.cs`; `remake/game/src/Entry/NameEntryView.cs`; `remake/game/src/Entry/SystemMenuView.cs`; `remake/game/src/UI/SettingsView.cs`; `remake/game/src/GameRoot.cs`; `remake/game/src/Entry/OpeningPersistenceHost.cs`; `remake/game/src/Entry/WindowSupportHost.cs` | Accepted pilot; preserve actual save/settings/recovery predicates and the existing owned instance. `GameRoot`/persistence changes require a specific demonstrated layout need, not a second navigation service. No save format or user-save experiment. |
| 5. Original battle inspection / map, separately bounded after #1622 | No product paths authorized by this design. #1622 must name the actual query/input/presentation owners once source and capability comparison closes. | Original-feature closure takes priority over optional modern additions; it need not wait for unrelated sibling polish. Define binding, public information and state-preserving return before implementation; do not hide missing functionality behind a disabled cosmetic mock. |

Each future code slice updates the closest current agreement and uses its scoped adapter/behavior
checks and actual state/input/geometry/error observations. New engine behavior warrants small
behavior tests; a visual change does not justify tests of probes or a full legacy suite. Use existing
tools/instances and disposable evidence namespaces, never screenshots or current user saves.
Main-gate retains independent acceptance and integration; user visual acceptance remains separate.

No new blocker is created for finishing this **design** by #1621/#1622's unresolved details. They
limit the affected implementation seam, not the ability to review the recommended look. Remaining
Unknowns are human visual acceptance, measured final long-content combinations, original inspection
bindings/disclosure, and the separately owned close/tint/path/weapon/resize defects. Preserve their
own Issues; do not open duplicates or call them resolved by this document. #1544 hint simplification
and the action-order strip remain deferred. Freeze this design PR after direct checks and actual CI;
do not begin any implementation or next ticket from its delivery alone.
