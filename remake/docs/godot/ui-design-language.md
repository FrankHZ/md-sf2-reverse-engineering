# A Shared Visual Language

[Issue #1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616) uses this page as the
remake's design system. The [scene review](ui-scene-review.md) applies it to the player journey.
The [presentation agreement](presentation.md) records what runs today. [#2079](battle-ui-composition.md)
delivers the first slice of #2064, with phase-specific battle focus, one shared board cursor,
a freestanding Main diamond and a fixed reader for the current decision.

**High rigor.** Shared controls and scene layouts affect the whole player journey. The user
accepted the Godot review at its 1920×1080 design size. Each gameplay or host caller owns its
input, transactions and return.

Field, Church and shop use compact labeled lists. Battle Main places four labeled directional
slots between the measured acting card and the separate footer. Magic and Item replace the diamond
with lists. Targeting moves focus to the board and replaces the acting card with the recipient
card. Admitted icons keep their localized names visible before focus. The
[scene review](ui-scene-review.md#scenes-in-the-playable-demo) links each layout to its mounted
consumer and evidence.

## Components

| Component | Contents and states | When to use it |
| --- | --- | --- |
| Command item | Admitted original icon, persistent localized name, focus marker. Align an amount, level, MP cost or slot when it distinguishes offers. Unavailable items keep their name and status. | A choice in a list, bar or diamond slot. Entry and host actions can use text alone. |
| Choice list or command bar | One ordered set of items. Up/Down moves focus in a vertical list, Left/Right in a horizontal bar. Confirm selects the focused offer. Retain the caller's initial choice. | Right-hand lists support commands, submenus and long labels. Horizontal choices suit an attached dialogue question. |
| Contextual detail | Show a cost, consequence or actual refusal reason when the item leaves a question. A self-explanatory command needs only its label. | Beside the current collection. Allow for a long reason without displacing focus. |
| Dialogue frame | Body, supplied portrait, choices only in a real choice phase, footer for mapped hints. | Greeting, story, Search and battle messages. Preserve each reader's reveal/page/acknowledge behavior. |
| Confirmation prompt | Identify the person, item or slot, exact price or consequence, and existing confirm/decline actions. Retain the caller's safe default. | Replace the previous group during a quote, overwrite or other existing confirmation step. |
| Status card | Stable identity, HP/MP gauges and numeric values. Show the actual acting, targeted or inspected unit. | Keep a card while its person matters to the decision or presentation. |

These roles use existing presenters. The owning session or host supplies offers, availability,
prices and transaction state.

## Scope, evidence and boundaries

**Confirmed — accepted implementation/source structure:**
[StoryTheme](../../game/src/UI/StoryTheme.cs) supplies blue/gold reading panels, 30px body,
24px hints, selected `ffe19a` and unavailable `8e9bab`. Entry and host actions use the square
journey theme. Field, Church and shop use labeled lists with nearest 3× command art where mounted.
Battle Main uses freestanding labeled 4× diamond slots. Magic and Item retain labeled rows. The [Church agreement](ui-scene-review.md#10-production-church-and-battle-command-lists)
and [field and shop owner](field-and-shop.md#current-composition) identify the actual consumers.
The [display contract](display-composition-contract.md) owns the world and UI layers.

**Confirmed — original structural evidence:** the linked
[common-menu input contract](../../../docs/research/presentation/common-menus.md#confirmed-input-contracts)
and [battle action contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md)
own cardinal selection and source command meaning. The field mapping is documented with its pinned
source in [the exploration agreement](presentation.md#exploration-gameplay-diamond-agreement-1588).
The vertical lists are a remake choice. Original timing and natural reach require their own evidence.

**Inferred:** a consistent frame language, nearby command explanation and less persistent HUD
competition should make these screens easier to scan. The scene owners record which English/zh-CN, long-text and
window-size combinations were observed. Physical display fit and human readability remain
**Unknown**. Layout values below describe the accepted design.

In scope: existing entry, field, party/preparation, service, host and battle presentation, including
their supported languages, windows, reading and return states. Keep the [implemented battle inspection and return paths](battle-interaction-baseline.md).

The existing font, scale, input mappings, pause ownership and save eligibility bound these layouts.
Remote services and an additional host Overview remain outside this design. The space below round/gold
is reserved for the pending [current-round readout #2065](battle-ui-composition.md#current-round-order-2065).
The battle Target phase uses one candidate change per fresh input, and choosing ordinary Stay
commits in one gesture. The [battle input owner](battle-ui-composition.md#fresh-target-input-and-direct-stay)
records the delivered behavior. The inspection Cursor page uses the shared board overlay to show
the disclosed unit's movement field. The overlay clears when a panel or host cover takes focus.
[Inspected movement #2081](battle-ui-composition.md#read-only-inspection-and-the-member-view)

Field Party and battle Member use shared scrollable rows with wrapped names, optional portraits,
and focused and unavailable treatment. Spell descriptions (#2083) remain pending.
[Shared roster #2082](battle-ui-composition.md#shared-member-roster-2082)
[Implementation handoff](battle-ui-composition.md#implementation-handoff)

## Surfaces and hierarchy

Use the existing licensed bundled font and current English/zh-CN display admission. Preserve
canonical/custom names and fallback notices under the
[localization owner](../application/localization-design.md). No font substitution, Chinese naming
or IME admission follows from these layouts.

| Role | Design baseline at native 1080p | Use and limit |
| --- | --- | --- |
| Scene/section heading | 32px, ordinary light text; 40px only for a sparse entry heading | One heading per panel. Gold is primarily selection, not every heading. Preserve original title artwork rather than typesetting a replacement logo. |
| Dialogue, menu choice, transaction facts | 30px `f4f5fc` | Readable full text and explicit amounts. Keep choice rows at least 64px tall; wrap and increase height. |
| Status identity | Existing card 26px baseline and bounded 22px exceptional-name fallback | Retain its existing measured two-line/widening policy. Do not propagate the exceptional 22px fallback to body or menu text. |
| Operation hints and binding tooltips | Fixed 24px in the native gameplay canvas | Wrap or allocate reading space as text, device or phase changes. Menu choices, transaction details and status values keep their own roles. |
| Quiet counters and metadata | 24px baseline | Keep price, a refusal reason and irreversible consequences at readable body size. |
| Reading panel | Existing `08275bf5` near-opaque blue, 2px `ffe19a` edge, restrained existing shadow | Square corners. The world may remain visible around it; terrain detail must not compete through the text. |
| Action surface | Existing dark blue family (`102c50` normal, `204c76` hover, `354c64` pressed) | Same square edge family; avoid extra gold boxes around every idle row. Native buttons retain focus/pressed distinctions. |
| Selected and available | `ffe19a`, plus `›`/`▸`, outline or original selected raster | Exactly one current choice per input owner. Pointer hover is not command submission. |
| Unavailable | `8e9bab`, explicit unavailable wording and actual reason | Focus may still mark an inspectable disabled option; never recolor it as available. Do not fade the whole panel and its explanation. |
| Health, danger and error | Keep current HP/MP gauges and numeric values; explicit Low HP, Defeated or failure wording | Do not use gold for damage/error. Color supplements the existing text/icon meaning; no new palette or status authority. |

Keep ordinary panel content margins at least the existing 14px horizontal/12px vertical; use 24px
between text groups or reading and action areas, 12px between related rows, and 32px at gameplay
viewport edges. A larger portrait or service panel can use 24–32px internal padding. These are a
small spacing vocabulary, not a demand to replace every layout constant with a token system.
Use alignment and space to group content before adding another border.

In compact command rows, render 24×24 source art at nearest 3× (72×72px) and 16×24 identity
icons at 48×72px. Keep each icon's aspect ratio and both admitted frames. In the right column,
use 528×80px rows, 8px gaps and 16px panel insets. Spell levels and item slots have visible
labels. The public schematic uses text when art is unavailable. Private art stays local.
The [asset inventory](../content/presentation-assets.md#ordinary-menu-publication-and-coverage-1666)
owns resource identity.

## Focus, action and return

**Keep item slots still.** Within the same inventory view, selectable, focused, unavailable
and read-only versions of a slot share the row bounds and the same positions for icon, slot
number/name and trailing status. Reserve these columns for empty slots and text-only fallback
too. Use color, an outline or the selected image for feedback. A marker needs its own reserved
space. Confirming a transfer may change which item occupies a slot. Moving focus or changing
which pack accepts input must leave the slot geometry intact.

```text
Normal                 Focused available       Focused unavailable
  Medical Herb  ×1     › Medical Herb  ×1      › HEAL 2   Unavailable
  light body text        gold + marker           muted text + marker
                                                 Needs 5 MP; Sarah has 3 MP.
```

The example illustrates hierarchy, not an assertion about current inventory, learned spells or
source costs. Real values and availability always come from the existing offer/query. Inspecting
a disabled choice is useful; confirming it must not commit an older valid choice. Host menus that
currently skip unavailable rows keep doing so and show their reasons in the row. Each menu keeps its own navigation rules.

Move focus along the visible collection's axis. Focus updates the marker, icon and detail together.
Battle movement uses mapped directions and Confirm. Directions move the inspection cursor freely
and cycle eligible combatants during action targeting. On pointer-enabled controls, the icon and
its label form one hit target. Back to a list restores its selected offer or slot.
Use the installed `GameInput` mappings for keyboard/pad hints and the existing complete
press/release pointer gesture. A stationary pointer does not steal directional focus. Closing,
relayout, hiding, loss of native focus and modal handoff cancel incomplete pointer gestures and
respect existing release barriers. The physical-keyboard name editor retains its separate fixed
editing keys. Do not imply controller text entry or pointer activation where a consumer lacks it.

Back removes one provisional page and restores its parent's selection. A paid transaction, used
item or accepted save does not become undoable because the panel closes. Ordinary story Cancel
cannot skip dialogue; Search result dismissal, Church decline, battle movement cancellation and
host Quit each retain their own semantics. The scene review names these differences explicitly.

## Reading without shrinking

Measure the full shaped string, portrait, choices and hints before placing the reading surface.
Keep current consumer-specific reading/reveal/acknowledgement ownership. The [dialogue exit agreement](presentation.md#coordinated-dialogue-exit-1613)
owns the coordinated end of the portrait and text window.

1. Wrap at the allocated width; grow the panel within its declared region. A long option becomes
   a taller row with its marker aligned to the first line. Cost/count columns remain readable.
2. When height is exhausted, use the existing menu scroll or story reading pages. Keep the
   focused row and fixed Back/action footer visible. Focus, scrolling and reading are separate
   from committing an action; unread text cannot cause accidental confirmation.
3. Preserve intentional source Search line boundaries and admitted translation/fallback notes.
   Do not collapse all newlines as a layout shortcut. [#1608](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1608)
   records the implemented Search correction.
4. Never solve overflow by shrinking 30px body or 24px hints, clipping an unavailable reason,
   truncating a price/HP number, or hiding the Back control. Existing bounded status-name ellipsis
   is the documented exception, not a new general truncation policy.

Use the footer for the current action, its cost or its consequence; allow multiple lines when
needed. Keep unavailable labels visible and explain a refusal when the player tries it. Rows
that skip disabled focus still need an inline reason. Keep tutorials that teach an unfamiliar
operation; omit descriptions of the implementation. See the [scene-by-scene examples](ui-scene-review.md#player-facing-copy)
and [mapped input hints](presentation.md#minimal-contextual-hints-1544).

## Selected item descriptions

[Issue #2023](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2023)
is a Standard slice within the shared design. It covers field Party inventory and preparation
item/equipment lists, merchant stock and held-item selection, and the battle Item command page.
The selected item gets a fixed reading area below its collection, with the shared 30px body face,
word wrapping and space reserved even for empty slots or items without admitted facts. The rows,
reading area and mapped actions have separate bounds. Selection can scroll a row into view.
Changing its prose cannot change row, list or footer geometry. The same layout fits the existing
1920×1080 design canvas at smaller supported window sizes.

Party inventory uses Up/Down to inspect slots when that pane has focus and Left/Right to change
members. A complete inventory row click also inspects a slot. In field preparation and the shop,
the description follows the existing choice, including unavailable rows. In battle the acting
card and choice rows retain their position and the description shares the existing footer frame.
Range and actual refusal reasons stay readable beside the item effect, and quotes, results and
target confirmation use their own existing pages.

Use the mounted weapon, ring, recovery and action definitions for effects and numbers, and the
current actor/slot for conditions such as class eligibility or damage. Distinguish field effects
from battle effects. Keep descriptions short and localize complete phrases in the UI catalog.
The pending [spell descriptions #2083](battle-ui-composition.md#selected-spell-descriptions)
will use the stable reading region and the admitted level and effect facts for their context.
Names keep the existing name catalog. Where gameplay has no bound effect, leave the item prose
blank. Depot, other battle menus, game rules, item data and save formats are outside this slice.

### Observe the selected item pages

Build Debug and load the registered local inputs as in the
[locked .NET workflow](../development-and-verification.md#locked-net-workflow). Use the existing
[Map16 package selectors](../application/opening-session-lifecycle.md#reproduce-the-map16-observation)
for the world, scene, entry and opening inputs. Run each case once in English at 1920×1080 and once
in Chinese at 1600×900. Use fresh checkout-local APPDATA, LOCALAPPDATA, TEMP/TMP and output, with
a fresh namespace under the registered disposable save root for each process.

| Case | Controlled start and ordinary input |
| --- | --- |
| `party` | Copy the `start` document from `handoffs/issue2035/galam-town-v1/return-writer.json` to checkout-local scratch. Set its position to (35,26) and facing to 1. Open Member from the field list, Tab twice to Items, click slot 2, choose slot 3, and change holder with Right. Open Prepare → Equip, examine the first two slots and Cancel all the way back. |
| `shop` | Use the same approach for the F663 Galam item counter. Talk, browse Herb and Seed, change recipient and decline the purchase quote. Return to the same stock row. Browse Sell and Repair slot selections for the first two holders and Leave. |
| `battle` | Prepare the disposable Battle02 record below. Continue it at the title, confirm movement in place and open Item. Browse the equipped weapon, damaged ring, Herb and empty slot. Cover Item with the host Menu, Resume, open the Herb target page, then Cancel to Item, actions and Movement. |

For the controlled battle pack, read the existing `issue2049-stable-battle2/sf2-opening/slot1.json`
under the disposable save root. Decode the envelope's base64 `payload` as UTF-8 JSON. In
`data.battle.actors`, select `actorId=ally-1`, the saved queue owner. In both
`loadout.value.items` and `progress.value.sourceLoadout.value.items`, replace
`[214,15,0,123]` with `[214,276,0,127]`. The second word is a bare damaged Protect Ring.
Retain the recorded class, equipped weapon and actor stats. Serialize the payload as UTF-8 JSON,
base64-encode those bytes and compute their SHA-256 for the envelope's existing `sha256` field.
Write the envelope to slot 1 in the fresh namespace only. This controlled starting pack permits
a long description and an empty slot in the same selection without performing a Use.

Set `SF2_ITEM_DESCRIPTION_CASE`, `SF2_ITEM_DESCRIPTION_LANGUAGE` (`en` or `zh-CN`),
`SF2_ITEM_DESCRIPTION_RESOLUTION` (`1920x1080` or `1600x900`) and
`SF2_ITEM_DESCRIPTION_OUTPUT` to a fresh ignored report. For the two field cases select the
checkout-local start with `SF2_ITEM_DESCRIPTION_START`. Use an input settings file with
`formatVersion=1`, the matching `language`, `textMode="instant"` and the default mappings.
Launch with the isolated runtime selections above.

```powershell
& $env:GODOT_BIN --headless --audio-driver Dummy --max-fps 120 --quit-after 3600 `
    --path remake/game --script res://probes/engine_item_description_observation.gd `
    --log-file $nativeLog -- --input-settings $settings
```

The [native observer](../../game/probes/engine_item_description_observation.gd) reads the running
host's current selection, contents, 30px font, wrapped minimum height, layout and scroll state.
It compares live packs, gold and flags and checks the disposable battle file after Continue.
Native window-manager behavior and human reading comfort remain **Unknown**.

## Layout that preserves the world

The [desktop window policy](presentation.md#window-support-and-recovery-1455) fits the 1920×1080
design canvas into supported smaller clients down to 1600×900. Gameplay uses native coordinates
when width ≥1920 and height ≥1080, with 96px world cells and
ordinary sprites at nearest 4×. Larger supported native windows, including 2560×1440, 3840×2160
and wider aspect ratios, reveal more world; fonts and icons do not automatically double at 4K.
Use actual viewport bounds, fixed readable panel widths and edge/center anchors. Do not stretch a
paragraph across the full ultrawide screen. Entry always fits its 16:9 canvas. Settings and Help inherit the active caller’s canvas.

Choose the overall composition by what the player needs to see during each step.

| Player task | Subject to protect | Active components |
| --- | --- | --- |
| Meet someone, then choose | Witch artwork keeps its full rectangle | Right-column greeting gives way to actions. |
| Read while watching the world | Speaker or searched object | Stable top/bottom reading frame with optional portrait and choices. |
| Compare services and goods | Priest or merchant | Side list with labeled actions. A quote replaces the list. |
| Decide on a battle action | Acting unit, candidate cell or target | The acting card heads the diamond or child list. The recipient card and board take over during targeting. |

The [battle composition](battle-ui-composition.md#world-marks-and-panel-placement) defines the
battle grid at 1920×1080. Keep the host Menu and the projected subject clear on the fitted smaller
canvas and larger native viewports.

| Phase | Components and placement |
| --- | --- |
| Movement | Quiet round/gold and a movement hint support the shared cursor and cyan field. Traversable ally cells have an edge and legal stops also have a fill. |
| Main and spell/item lists | A measured acting card heads the 560px decision column. Main uses four labeled diamond slots outside the blue reader. Magic and Item replace them with scrollable rows. The fixed footer contains descriptions, refusals and the mapped actions. |
| Choose a target | The selected recipient takes the card. The cursor denotes the legal candidate, with one reach and effect field. Back retains the previous command, spell level or item slot. |
| Free inspection | One hover card clears the cell. Unit name and land effect share its first row, HP and MP share the next row, and coordinates follow. Empty ground uses the shorter terrain card. |
| Action playback and outcome | Decision windows retire. Actual close-up participants and their messages own the scene. Resize refreshes the art, cards and reader around the retained delivery state. |

At supported sizes, move the card and footer together when the preferred column would cover the
subject. Wide values can expand the measured card and long text scrolls inside the reserved reader.

Entry uses its established Witch art and right-column reservation instead of this battle grid.
Field dialogue keeps its stable top/bottom speaker avoidance. Church and Shop position their services beside the visible provider.
Party and Settings use working panels. The [scene review](ui-scene-review.md) applies these rules.

## Motion, composition and modal ownership

For an available focused command, alternate its admitted frames every 250ms while focus remains. Keep the label, hit area and outline steady. Reduced Flash holds the selected frame. Unavailable commands and spell/item identity icons remain still. Any focus
movement stays inside the item, clear of the name, neighboring items and hit bounds.
HP/MP follow committed state immediately; a UI flourish cannot postpone a scene acknowledgement.
Existing title/story/effect clocks retain their owners. Use direct panel transitions by default;
do not introduce a decorative fade that masks a lifecycle or input bug.

World terrain/actors, local weapon composition and UI overlays are different responsibilities.
Keep the existing world / `BattleUi` / `FieldUi` / `HostUi` roles and scene-owned visibility.
The [display contract](display-composition-contract.md) owns layer ordering.
[Weapon-frame composition](battle-scenes.md#weapon-frame-composition-1620) owns front/back,
offset and flip within the combatant.

At most one gameplay page accepts input. An allowed host Menu covers it, owns its own pause and
input, and returns to the same selection/reading state. Subpages hide the parent host controls.
Window recovery hides those covered host views, owns recovery actions and restores the retained
page before gameplay can resume. A veil is only visual; it never supplies pause/input protection.
Stopped-scene feedback remains readable above world fades and does not invent a retry.

## Production and human review

The [playable scene review](ui-scene-review.md#scenes-in-the-playable-demo) connects F1–F10
to their deployed controls. The reviewed layouts establish the design reference. The production
owners record actual inputs, geometry and returns. For ordinary-game readability, physical
devices and desktop focus, use the [short play route](ui-scene-review.md#short-play-route)
and [human review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).
