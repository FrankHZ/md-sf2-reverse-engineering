# A Shared Visual Language

[Issue #1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616) uses this page as the
remake's design system. The [scene review](ui-scene-review.md) applies it to the player journey.
The [presentation agreement](presentation.md) records what runs today.

**High rigor.** Shared controls and scene layouts affect the whole player journey. The user
accepted the Godot review at its 1920×1080 design size. The ordinary pre-voyage game uses the reviewed scenes. Each gameplay or host caller still owns input, transactions and return.

Use compact right-hand vertical lists for four-command menus in the field, Church, shop and
battle. Keep each admitted original icon beside its localized name, visible before focus.
Child lists replace the parent in the same column. In battle, layout A joins the acting unit's
status to its command list. Layout B remains a comparison in the preview. The
[scene review](ui-scene-review.md#scenes-in-the-playable-demo) links each layout to its production
consumer and the limits of its evidence.

## Components

| Component | Contents and states | When to use it |
| --- | --- | --- |
| Command item | Admitted original icon, persistent localized name, focus marker. Align an amount, level, MP cost or slot when it distinguishes offers. Unavailable items keep their name and status. | A choice in a list or bar. Entry and host actions can use text alone. |
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
journey theme. Field, Church, shop and battle use labeled right-hand lists with nearest 3×
command art where mounted. Up/Down scans rows. The [Church agreement](ui-scene-review.md#10-production-church-and-battle-command-lists)
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
Remote services, a host Overview and predicted turn order remain outside the accepted design.
[Modernization proposal](modernization-design.md)

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
| Decide on a battle action | Acting unit, candidate cell or target | Layout A puts the acting card above the right-hand list. The list yields to the target card and board during targeting. |

Use the following battle geometry at the 1920×1080 design size. Keep the host Menu and
the inspected subject clear at each supported viewport. Function-key tabs belong to the preview.

| Phase | Components and placement |
| --- | --- |
| Movement | Keep round information quiet at the upper left. Draw legal stops below the units. Confirming a stop opens commands. |
| Command and spell/item list | Layout A uses a 560px-wide right panel with the first command row at (1312,312). The acting unit's identity and HP/MP head the panel. Use 36px values above two 232×28px gauges. Align child rows to the same origin and size the panel to its contents. |
| Choose a target | Keep reach on the board. The right card shows the selected recipient and their vitals. Put cycle, Confirm and Back in the footer. Return to the previous command or spell level. |
| Free inspection | Keep the status card clear of the hovered cell. Unit name and land effect share its first row, HP and MP share the next row, and coordinates follow. Empty ground uses the shorter terrain card. |
| Action playback / outcome | The board command panel retires. Close-ups use the original ally/enemy window positions. Ordered reward and return messages use the reading frame. |

In production, row placement follows the measured height of the acting card. Its HP and MP
values currently use a different size and arrangement from design A. See the
[decision-card comparison](ui-scene-review.md#9-remaining-differences-in-play). Measure the whole
group against the host exclusion, the projected subject and the footer for long reasons and wide
values. Move or grow the panel as a group when needed and preserve focus across a resize. Scroll or page overflow inside its owner.

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
