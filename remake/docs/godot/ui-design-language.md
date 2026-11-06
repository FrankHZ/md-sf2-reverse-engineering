# A Shared Visual Language

Design proposal for [#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616),
under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438).
Read the [scene review](ui-scene-review.md) alongside this page: it shows what these rules mean
from the Witch through a complete battle decision and return.

**High rigor — design scope control.** Repeated local fixes have left adjacent controls with
different visual rules. The outcome is a coherent, player-reviewable recommendation before any
product implementation. This page owns the proposed language and its choices; the scene review
owns its application, gaps and bounded implementation order. Existing
[presentation agreements](presentation.md), [battle scenes](battle-scenes.md) and
[capability status](../capability-status.md) remain the authorities for implemented behavior.
Approval of this document is not approval of finished visuals or authorization to change code.

Recommend **original icon art over a visible world, with quiet blue reading surfaces and one gold
focus language**. The diamond is a freestanding group of four icons. Its selected name and reason
sit in a separate adjacent panel; no blue panel encloses the diamond. Dialogue remains a reading
surface, not a tray for gameplay buttons. Entry, services and battle should look related without
using identical coordinates or making every choice a diamond.

## The picture to review

```text
                                      [ Menu ]  host access, separate from play
  [current person / current resources]

                   WORLD / CHARACTER ART
            keep the person or target being considered visible

           [up]
    [left]      [right]    +------------------------------------+
          [down]          | Selected command                    |
                          | Current cost / consequence / reason |
                          +------------------------------------+
       no enclosing blue background      separate reading frame
```

The sketch is a composition, not source art or a screenshot. Icons retain their own original
square raster backgrounds, if present; “outside the box” means outside the **common enclosing
panel**, not removing original pixels. A transparent grouping Control is appropriate. A large blue
rectangle behind both groups, even with a separator or different Z, fails this design.

Four visual decisions deserve the user's review of the paired scene sketches:

| Choice | Recommendation and visible consequence | Alternative and cost |
| --- | --- | --- |
| Window shape | Straight 2px gold reading-panel edges and the same square geometry for ordinary buttons; idle button edges remain muted, with gold reserved for focus. Focus also uses an outline/marker. This joins entry choices to their adjacent dialogue. | Retain the existing 8px entry-button corners while keeping square dialogue. Less change, but retains the reported mismatch. Do not mix the alternatives per scene. |
| Diamond association | Place the selected-name/detail panel beside the independent diamond, separated by 24–32px of visible world. It is obvious which explanation belongs to the current icon. | Keep the battle detail at the far right edge. More open center, but a longer eye movement and weaker association, especially on wide displays. |
| Service backdrop | Keep the priest/shop context visible under a quiet neutral veil; propose black at 35% for service selection, with opaque reading panels. No veil on ordinary exploration/battle decisions. | Retain the current strong Church shade. It isolates a transaction more strongly but makes the service resemble a disconnected system dialog. |
| Battle information density | Round/gold form one small top-left strip; cards appear only for the active decision or scene pair. No permanent large ledger or action-order strip. | Retain the large round/gold context frame. Easier isolated scanning, but consumes board space and competes with Menu/status. |

These are recommendations awaiting visual acceptance, not unresolved routine engineering choices.
The freestanding diamond, original art direction, existing input semantics, 1080p baseline and
96px world scale are already constraints, not options. Review these four decisions together on
entry, exploration dialogue, Church and battle before adapting sibling screens.

## Scope, evidence and boundaries

**Confirmed — accepted implementation/source structure:**
[StoryTheme](../../game/src/UI/StoryTheme.cs) supplies blue/gold reading panels, 30px body,
24px hints, selected `ffe19a` and unavailable `8e9bab`; its entry buttons currently have 8px
corners while reading panels are square. The [display-layer agreement](presentation.md#display-layer-boundaries-1517)
separates world, battle UI, field UI and host UI. The
[diamond experiment](presentation.md#modern-ui-experiment) preserves cardinal input and original
24px command rasters at nearest 4×. Church currently puts its diamond inside its service panel;
the [Church agreement](presentation.md#church-and-shared-save-agreement-1519) owns its transaction.
These are code/contract claims, not a fresh native or aesthetic pass.

**Confirmed — original structural evidence:** the linked
[common-menu input contract](../../../docs/research/presentation/common-menus.md#confirmed-input-contracts)
and [battle action contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md)
own cardinal selection and source command meaning. The field mapping is documented with its pinned
source in [the exploration agreement](presentation.md#exploration-gameplay-diamond-agreement-1588).
This proposal does not infer original timing, opacity, cursor animation or natural reach from them.

**Inferred:** a consistent frame language, nearby command explanation and less persistent HUD
competition should make these screens easier to scan. Human readability and final visual balance
remain **Unknown** until review. All numerical layout recommendations below are modern design
values, not original-game measurements or already observed new geometry.

In scope: existing entry, field, party/preparation, service, host and battle presentation, including
their supported languages, windows, reading and return states. The plan reserves explicit original
battle-viewing entry points pending [#1622](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1622).
It does not admit later campaign content or missing services merely to fill a design diagram.

Out of scope: product code in this slice; new UI framework, global layer registry, new art/font
pipeline, gameplay rules, navigation or pause authority, arbitrary UI zoom, below-minimum gameplay,
remote services, save-anywhere, new input devices, hint redesign, and action-order prediction.
[The modernization proposal](modernization-design.md) remains a separate product proposal; its
remote-service and host Overview suggestions are not silently adopted here.

## Surfaces and hierarchy

Use the existing licensed bundled font and current English/zh-CN display admission. Preserve
canonical/custom names and fallback notices under the
[localization owner](../application/localization-design.md). No font substitution, Chinese naming
or IME admission follows from this style proposal.

| Role | Proposed treatment at native 1080p | Use and limit |
| --- | --- | --- |
| Scene/section heading | 32px, ordinary light text; 40px only for a sparse entry heading | One heading per panel. Gold is primarily selection, not every heading. Preserve original title artwork rather than typesetting a replacement logo. |
| Dialogue, menu choice, transaction facts | 30px `f4f5fc` | Readable full text and explicit amounts. Keep choice rows at least 64px tall; wrap and increase height. |
| Status identity | Existing card 26px baseline and bounded 22px exceptional-name fallback | Retain its existing measured two-line/widening policy. Do not propagate the exceptional 22px fallback to body or menu text. |
| Hints, quiet counters, metadata | 24px; preserve a consumer's larger accepted role where needed | Secondary information only. Price, a refusal reason or an irreversible choice is not demoted to tiny metadata. |
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

No new per-icon caption or frame is placed around original diamond art. Public authored content
without admitted art keeps the existing measured text-only fallback in the same cardinal positions.
Fallback labels may wrap inside their existing control, but cannot invent source-like assets.
Private original pixels stay private; these documents contain only authored wireframes.

## Focus, action and return

```text
Normal                 Focused available       Focused unavailable
  Medical Herb  ×1     › Medical Herb  ×1      › HEAL 2   Unavailable
  light body text        gold + marker           muted text + marker
                                                 Needs 5 MP; Sarah has 3 MP.
```

The example illustrates hierarchy, not an assertion about current inventory, learned spells or
source costs. Real values and availability always come from the existing offer/query. Inspecting
a disabled choice is useful; confirming it must not commit an older valid choice. Host menus that
currently skip unavailable rows keep doing so and show their reasons in the row. This proposal
unifies the appearance, not the differing navigation contracts.

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
Keep current consumer-specific reading/reveal/acknowledgement ownership. The design does not add
click-to-skip or extra confirmation, and does not settle
[#1613](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1613)'s closing-lifecycle defect.

1. Wrap at the allocated width; grow the panel within its declared region. A long option becomes
   a taller row with its marker aligned to the first line. Cost/count columns remain readable.
2. When height is exhausted, use the existing menu scroll or story reading pages. Keep the
   focused row and fixed Back/action footer visible. Focus, scrolling and reading are separate
   from committing an action; unread text cannot cause accidental confirmation.
3. Preserve intentional source Search line boundaries and admitted translation/fallback notes.
   Do not collapse all newlines as a layout shortcut. [#1608](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1608)
   owns that behavior correction.
4. Never solve overflow by shrinking 30px body or 24px hints, clipping an unavailable reason,
   truncating a price/HP number, or hiding the Back control. Existing bounded status-name ellipsis
   is the documented exception, not a new general truncation policy.

Keep existing hint content in a measured footer, including multiple lines where necessary.
[#1544](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1544) remains deferred: this
proposal neither removes existing hints nor adds first-use detection, device switching or a new
help system. A compact diamond currently showing only its selected name need not acquire a
paragraph of new generic instructions.

## Layout that preserves the world

Gameplay remains native coordinates at width ≥1920 and height ≥1080, with 96px world cells and
ordinary sprites at nearest 4×. Larger supported native windows, including 2560×1440, 3840×2160
and wider aspect ratios, reveal more world; fonts and icons do not automatically double at 4K.
Use actual viewport bounds, fixed readable panel widths and edge/center anchors. Do not stretch a
paragraph across the full ultrawide screen. Entry/Settings/Help retain their existing scaled
smaller-window policy; this does not admit 720p gameplay.

The following is the **recommended battle decision composition**, in `(x,y,width,height)` native
pixels at 1920×1080. Dimensions are initial budgets, then measured against the actual text.

| Region | Starting budget | Growth / collision rule |
| --- | --- | --- |
| Host Menu | Existing `(1740,12,160,56)` | Reserve `(1724,0,196,92)` including a guard. No counter/card may use this area. If the measured Menu grows, its actual bounds plus 16px define the exclusion. |
| Round and gold | `(32,32,600,48)` at 24px | One quiet strip, no separate large boxes. Grow width to 800px, then wrap to two lines; never abbreviate real values. Its measured bottom +16 sets the card top, at least 96px. |
| Acting / selected-target cards | `(32,96,500,192)` / `(1388,96,500,192)` | Retain existing bounded growth to 800px inward and measured height. At maximum widths their inner edges are 832/1088: 256px remains between them. Both stay below the host exclusion. No target card in movement/root-command mode. |
| Command diamond | `(32,824,288,224)` | Existing 96px icons/cardinal steps; freestanding. Bottom edge is 32px above the viewport. No icon resizing to fit more text. |
| Selected command detail | `(352,824,640,224)` | Separate sibling, 32px after the diamond. Prefer measured short name/cost/reason; grow upward with the paired group. A long existing footer uses more height, not smaller type. |
| Spell/item list and detail | Left list width 560px, 24px gap, adjacent detail width 720px; bottom 32px | List replaces the diamond. Initial list height up to 448px; rows scroll, while description/actions have their own measured region. Cards and host reservation remain. |
| Story/action message | Existing 1440×280 or portrait-bearing 304px baseline | Centered reading surface near bottom, subject-aware top alternative; command/list panels are absent in this phase. Long text grows/pages under its current reading owner. |

These regions do not all appear at once. Scene phase controls visibility. The list/detail group's
1304px width and diamond/detail group's 960px width leave useful exposed board at 1080p; they do
not reduce the map viewport. Round/gold yield during close-ups and long reading states where the
scene pair and message matter more. The same information returns from live state after reading.

For movement/target decisions, use the existing projected subject and layout owner to test the
**whole decision group** against the current actor/preview/target plus a 16px guard. Prefer lower
left; mirror the group to lower right when needed, without changing icon order. If both collide,
try the upper free band below the cards. Lock the chosen placement for that page/selection; do not
make individual panels chase one another each frame. Recompute on a real target/page/viewport
change, cancel pointer gestures before moving hit regions, and preserve focus/reading position.
No group may collide with host access, cards, another visible reading panel or the selected subject.
If measured content cannot fit these candidates, that is a layout failure to resolve in the owning
consumer before acceptance; do not silently cover the target, shrink text or pan/zoom the world to
make room. Long detail should first use its existing reading/scroll mechanism.

Entry uses its established Witch art and right-column reservation instead of this battle grid.
Field dialogue keeps its stable top/bottom speaker avoidance. Church uses a centered pair;
Party/Settings are reading/work panels. The [scene review](ui-scene-review.md) shows these cases.
Live-resize anchor repair remains [#1614](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1614),
an optional P3; this proposal does not claim that every current consumer already relocates correctly.

## Motion, composition and modal ownership

Reuse the [existing diamond emphasis](presentation.md#diamond-focus-emphasis): selected source
frame plus the bounded 4→2→1→0px rise/settle over 180ms. Reduced-flash uses a stable selected image.
No idle pulse, bouncing unavailable text, new screen shake or animated HUD counting is proposed.
HP/MP follow committed state immediately; a UI flourish cannot postpone a scene acknowledgement.
Existing title/story/effect clocks retain their owners. Use direct panel transitions by default;
do not introduce a decorative fade that masks a lifecycle or input bug.

World terrain/actors, local weapon composition and UI overlays are different responsibilities.
Keep the existing world / `BattleUi` / `FieldUi` / `HostUi` roles and scene-owned visibility.
[#1621](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1621) audits their detailed
contract; no proposed Z constant here overrides it or creates a general layer registry.
[#1620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1620) owns weapon-frame
front/back, offset and flip investigation. Moving a dialogue above the world cannot correct a
weapon passing through a body.

At most one gameplay page accepts input. An allowed host Menu covers it, owns its own pause and
input, and returns to the same selection/reading state. Subpages hide the parent host controls.
Window recovery hides those covered host views, owns recovery actions and restores the retained
page before gameplay can resume. A veil is only visual; it never supplies pause/input protection.
Stopped-scene feedback remains readable above world fades and does not invent a retry.

## Design completion boundary

The design is ready for independent review when all scene rows have a normal view, a selectable
state, an unavailable/error treatment, long-content behavior and an explicit return/overlap rule;
the four representative sketches obey the same language; and future implementation has bounded
owners and preserved Unknowns. Direct document, relative-link/anchor, geometry-budget, scope and
private-boundary checks are sufficient for this design slice, followed by a clean committed
verification plan and actual CI readback. No SDK, Godot, ROM, screenshot or full-suite run is needed.

Implementation is a later explicit dispatch after main-gate review and resolution of material
visual choices. Its acceptance must read actual input, node geometry, state projection and errors
at the named combinations, then obtain human visual acceptance under
[#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456). A design PR or passing
geometry check alone does not establish that the user likes the finished result.
