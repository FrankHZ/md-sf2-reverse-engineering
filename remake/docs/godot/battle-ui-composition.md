# Battle UI composition

[Issue #2064](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2064) defines a **High**
rigor design slice. Several presenters can paint different phases of the same decision, and a
misrouted input can commit an action. This page specifies the proposed battle composition and the
implementation boundaries. The [feature trace](battle-interaction-baseline.md#one-player-decision-from-entry-through-return)
records delivered behavior. Design acceptance belongs to main-gate. The player reviews the composed
experience in [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456).

The scope is the mounted battle UI and the roster and spell reading surfaces it shares with field
preparation. Use the existing admitted encounters and same scene, action, movement, transaction,
continuation and save contracts. Keep the original source findings separate from the remake design.
In particular, the new inspection range and the queue strip are remake features.

## Why the presenters overlap

**Confirmed, remake source.** The session projects its selected actor independently of the menu
page. The map presents that actor through `BattleSelectionView`, places the target through a
separate sprite or fallback rectangle, and owns a second cursor for inspection. It calls both range
views. Action reach can begin while browsing Attack, Magic or Item. Inspection hides ranges and the
target cursor in `Inspect`, leaves the actor cue on its own lifetime, and calls `Inspect` even on
its subordinate pages. The command layout, auxiliary HUD, inspection and close-up each have their
own card, prompt and visibility logic. These paths explain how marks with different meanings can
coexist. The origin of each line in the player's report remains **Unknown**. The removed gold path
underline or sprite tint has no verified regression here.
[Map projection](../../game/src/Battles/BattleMapViewport.cs) ·
[Session presentation](../../game/src/Battles/BattleSessionView.cs) ·
[Inspection](../../game/src/Battles/BattleInspectionView.cs) ·
[#1616 feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616#issuecomment-7085667332)

Keeping a distinct actor bracket and tuning its color or opacity would preserve the separate
lifetimes and fallback styles. Rebuilding the turn UI as a single modal or introducing a common
scene graph would also displace the existing input, transaction and playback owners. Use a smaller
change. Let `BattleSessionView` derive the visible phase from the live session and its current
command or inspection page, and let the existing presenters render only the surfaces for that phase.
Keep layout authority local and use the admitted source cursor style for spatial focus. The
[display contract](display-composition-contract.md#battle-board-and-close-up-composition) already
supplies the canvas and world ordering.

## Phase, focus and return

Host cover has precedence for input. Next come authoritative continuation, outcome and failure
states, then the active inspection page, then the live player selection and command page. Player
preview/undo delivery belongs to Movement until it completes. `TargetChoice` and `CommitReady`
share the same target page. Only a legal control or presentation state can supply its corresponding focus. Derive this
on transitions and refreshes, including refusal, settings, resize, restoration and battle exit.
Visibility and hit testing change together.

| Phase | Sole primary focus | Allowed support | Input and exit |
| --- | --- | --- | --- |
| Battle entry, round program or appearance | Current scene, or the current message when it needs acknowledgement | Board and authored motion when delivery owns them | Existing continuation. Decision cards, command hints, queue and board selection marks are hidden. |
| Player movement or its delivered preview/return step | One cell cursor following the unit's displayed position through the delivered step | Acting card, round/gold, queue reservation, one movement field and movement hint | Mapped movement holds continue on arrival. Confirm uses the real legal stop. Cancel from an unsettled move finishes undo before opening inspection. While movement is delivering, keep the display, with input governed by its existing step boundary. |
| Main action | One focused command in the freestanding diamond | Acting card, round/gold, queue reservation, footer with consequence/refusal and Back | Initial Attack when enabled, otherwise the lower Stay/Search slot. Direction changes focus, Confirm chooses, and mapped shortcuts still choose their existing command. Main Back returns through the real movement undo. Ordinary Stay selects and commits here. |
| Magic or Item | One row, keyed by spell and level or actual inventory slot | Acting card, round/gold, queue reservation, one selected-offer reader in the footer | Child rows replace the diamond. Scrolling keeps focus visible. A disabled row is readable. Choose an enabled row to enter target selection. Back restores the parent command. |
| Target, including self, single- or multi-target | One board cursor at the current legal candidate | Recipient card replaces acting card. The one action field shows reach and the current effect. The footer shows a concise action name, Confirm/Back and refusal when needed. Round/gold and queue reservation stay quiet. | Fresh directions cycle once. Hover previews a candidate. A complete stable legal click uses the existing commit path. An invalid cell clears the cursor/card/effect and disables commitment even if the session still holds an older target. An empty candidate set gives Back and the reason. Back clears target state and restores the same command or item/spell row. |
| Search confirmation, Equip, Give/exchange or Drop | Current transaction choice, slot, recipient or confirmation | Acting identity where relevant, the one existing auxiliary reader and its own instructions | Existing eligibility, selection, read-before-confirm, receipt and return contracts. The transaction replaces the command/target reader, board cursor and range. |
| Free inspection, Cursor page | One board cursor on the inspected cell | One hover card, one inspection hint and, for a disclosed living unit, its movement field from its actual position | Directions and board pointer select a cell for reading. Confirm opens details on a unit or Options on empty ground. Empty ground clears the range and still shows terrain. Cancel restores live Movement. |
| Inspection Options, Map, Members or details | The open panel's option, map selection, member row or detail pane | One applicable hint and data from the existing read-only query | Retain cursor coordinates and caller selection for Back, hide board cursor and range. Full → Cursor, MemberFull → Members, Map/Members → Options, Options → Cursor. An on-demand Map selection can locate its disclosed unit and return to Cursor. |
| Enemy action, close-up, field notice/death, result or battle exit | Actual delivered action or current message/return prompt | Playback cards for the actual participants, message reader, board only when owned by the continuation | Existing completion token and acknowledgement. All decision surfaces and board marks retire. Rebuild from live control after delivery. Field return uses field chrome. A failure shows once at its owning view. |
| Host Menu, Save, Speed or recovery cover | Host/modal's current choice or reader | Only the permitted covered backdrop | Suspend battle gestures, holds and pacing using existing ownership. Restore the same eligible caller, with its current layout and fresh input after release. A resize while covered may recompute geometry. |

## World marks and panel placement

Replace the independent modern moving-actor bracket retained by [#1694](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1694)
with the same board-owned single-cell cursor used for targeting and inspection. Retain the
[published 32×32 asset](battle-scenes.md#original-target-and-inspection-cursor-publication)
and its nearest 4× placement centered on 96px cells (16px overhang per side). Reuse the cursor
consumer and one board-aligned authored pixel fallback for public or older private packages. Retire
the separate cyan target rectangle and the actor-bracket draw node once all three roles use that
consumer. Keep `FollowMovement`'s behavior for the player cursor, camera targeting and clipping in
the map. On inspection page changes, clear the mark without discarding the retained cell or moving
the camera. Command and panel pages express focus in their own controls. Keep actor colors and
source foreground priority.

Reuse the board-local movement and action drawing owners. Only one field may be visible, below
units and source foreground, using a pixel-aligned tile motif that leaves terrain readable.
Movement uses cyan, with distinct interior fill for legal stops and a simpler edge mark for
traversable-only cells such as occupied friendly cells. In targeting use warm reach tiles and one
lighter inset for affected cells. A cell in both has one outline and a distinct effect interior.
A radius-zero effect shares the candidate cell with the cursor. Rules, costs, target side,
AURA-all behavior and multi-target incidence come from the live offer and effect content.
Range geometry explains reach. Enabled recipients remain the session's admission decision.
When the candidate is invalid, only the valid offer's reach can remain. Keep a restrained
version of the existing pulse, with a steady presentation under Reduced Flash. Exact original
pulse and graphics parity remain **Unknown**. [Original range construction](../../../docs/research/battle/battlefield-pathfinding.md#range-and-target-construction)

Use the [shared visual language](ui-design-language.md) for all panels. At 1920×1080, anchor the
quiet round/gold panel at the existing upper-left gameplay inset below host chrome and reserve a
left stack under it for the queue. Use the existing roughly 560px right-side decision column,
32px gameplay inset and below-Menu band. The acting, recipient or hover card takes its top role
as the phase changes. Let wide values grow within the layout budget. Place the main diamond or
child list below the measured card. Use one independent reading/footer region below the controls
and keep Back and Confirm fixed within their phase. The main four source slots are Up Attack,
Left Magic, Right Item, Down Stay/Search. Reuse `DiamondCommandButton` and
`DiamondMenuGeometry` from the existing diamond surfaces, keep labels visible next to their
slots, and keep the controls outside the blue reading frame. This replaces battle Main's
accepted vertical rows. Keep the modern list for Magic and Item and leave other scene layouts
with their existing owners. A blocked command can show its refusal in the same reader.

Reserve the footer's reading bounds on page entry from the available viewport and keep them
stable across offers. Long rows and descriptions wrap and scroll within their own bounds without moving
the controls. On 1600×900 use the existing fitted canvas. At larger native sizes use the
existing edge anchors and readable panel widths. Reflow by phase, viewport and protected-subject
geometry only. Use `BattleMenuLayout.AvoidSubject` and the existing hover-card clearance to
protect the focused board cell and honor the right-top host exclusion. When the right column
would cover that cell, keep the footer and card aligned in the alternate clear stack and suppress
the secondary queue for that placement if needed. Fit a bounded reader and scroll before
allowing card, command, hint or host rectangles to intersect. Hit rects and pointer occlusion
follow the actual visible panels.

`BattleSessionView` owns visibility, transition fences and the resize call. `BattleMenuLayout`
owns decision geometry and its card and footer. `BattleMapViewport` owns map-local markers and
ground effects. Keep the existing transaction viewers and restrict the legacy HUD to the
auxiliary or failure information it actually owns. Retire the dormant general equipment/spell/item
text stack from ordinary decisions and show each error once at its current owner. Keep the
`PresentationCanvas` boundaries and the local actor/weapon/effect order. Wire the existing
resize owner into a geometry-only `BattleSceneView` refresh for close-up art, cards and message
bounds, including a paused cover. Keep the same participants, phase, token, reveal state and
elapsed delivery. [Close-up resize finding](display-composition-contract.md#findings-and-minimum-remedies)

## Read-only inspection and the member view

Resolve hover from `BattleInspectionQuery`'s disclosed unit at the cursor. The session query for
that actor's range uses the same settled-origin/session/revision guard and admission as the rest of
inspection, then calls the existing `BattleMovement.Range` with the inspected actor and its
actual cell. Live Move, mover type, terrain and opposing occupancy are the same inputs used for
movement. Read both `ReachableCells` and `Destinations`. A same-faction unit's cell can be
traversable without being a stop, including when inspecting the enemy side. Query on a changed
inspected actor or valid snapshot, never by advancing simulation. Only the Cursor page requests the
range. Clear it for empty ground, stale/undisclosed/absent/defeated subjects, any subordinate page,
host suspension and exit. Back derives it again from the retained cell and the live snapshot.
This describes geometric movement now. AI strategy, future turn eligibility and hidden-unit
details do not enter the display.
[Inspection query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleInspectionQuery.cs) ·
[Movement rules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleMovement.cs)

Replace `BattleMemberListView`'s one formatted Label with the roster presentation drawn from
`PartyStatusView`. Share the scrollable labeled rows, focused/unavailable treatment, naming and
portrait slots where supported, and reuse the existing `PartyDetailsPanel`. Give the views a small
row projection keyed by actor identity, with display name, optional portrait and localized
condition/availability. Preserve the focused member and scroll position across details/Back.
If membership changes, retain the identity if present. For a removed member, select the nearest
surviving row and reveal it. Keep long names wrapped in the list and full details readable. Battle adapts
`BattleMemberQuery` with its active-force ordering and initialized, defeated, absent and deployed
distinctions. An initialized unavailable-for-deployment member can have read-only details.
A missing/uninitialized member cannot be opened. Field adapts `QueryPartyDetails` and keeps
Prepare and its own command eligibility. Both use the same presentation semantics, with pointer
and mapped navigation. Neither query derives membership from the other. Free inspection of
a disclosed enemy stays a separate entry into the shared details panel.
[Battle roster](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMemberQuery.cs) ·
[Field roster and details](../../game/src/Exploration/PartyStatusView.cs)

## Fresh target input and direct Stay

`BattleSessionView.Navigation` currently classifies `CommitReady` as `TargetChoice` and
arms the same 120ms repeat used by non-movement pages. Use the actual Target page, independent of engine staging, to route target direction events through
the existing fresh physical edge resolution in `GameInput`. Arm repeat only for modes that retain
it. On entry, cancel or exit, clear the prior repeat, pending pointer gesture and inherited held
directions. A carried keyboard/d-pad or stick direction requires its release or neutral crossing
before it acts in the new phase. A separately pressed direction is a new edge. A held direction
changes at most one candidate per press. Rapid separate presses each work. The mapped
Target shortcut is also one cycle per fresh press. Keep previous/next order, wrapping, default
candidate, and the separate explicit Confirm untouched. A repeated refresh or a candidate change
is still the same target navigation boundary.
[Navigation](../../game/src/Battles/BattleSessionView.Navigation.cs) ·
[Input resolution](../../game/src/Input/GameInput.cs)

**Confirmed, original static source.** At pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, the menu calls `ExecuteDiamondMenu`.
With no chest (`CHEST_CONTENTS == -1`), `@CheckChoice_SearchStay` writes `BATTLEACTION_STAY`
and exits control. `@CheckBattleaction_Stay` then takes the no-action suffix. The faced chest
takes a separate branch.
[Menu and Stay branch](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm#L952-L964) ·
[Turn continuation](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm#L166-L172)

**Confirmed, remake source.** `BattleCommandMenu.Select` enters a second Commit page after
selecting Stay. Remove that extra player step. A fresh Confirm on Stay, its mapped shortcut, or a
complete Stay click commits once at the legal preview stop. Keep the same session validation and
commit/continuation path, including after-turn effects. Prefer a single ordinary-Stay intent at
the session boundary that reuses the existing admission and commit logic. It avoids exposing an
intermediate Stay page to the synchronous `Submit`/presentation callbacks. Existing explicit
staged action callers can retain their contract. On rejection retain the current decision and
reason. Consume the triggering input through the next actor or presentation acknowledgement
boundary. Main Back still cancels before choice. The Search substitution, its confirmation, and
transaction confirmation and acknowledgement keep their own paths.
[Command presenter](../../game/src/Battles/BattleCommandMenu.cs) ·
[Dispatcher](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs)

## Selected spell descriptions

Reuse the fixed-height reader from `ItemDescriptionView` in battle Magic and field Prepare Magic.
Use one typed spell-description projection and localized formatter for both, keyed by exact
`SpellRef` including level. Derive it from the same admitted effect/offer data as the action.
Battle uses the typed choices, healing/attack/support/retreat definitions, and live availability
reason. Field contributes the actual field effect, member/target requirements, cost and map/party
availability from its recovery rules. Share meanings and names where they match. Explain a
no-effect field use as free and distinguish a nominal field MP cost from the amount deducted when
consumption clamps at zero. Reserve the insufficient-MP refusal for a context that enforces it. The battle cost, target team, min/max reach and area radii apply on the battlefield.
Distinguish self, single target, selected-area and all applicable allies. Explain current refusal
such as insufficient MP, Silence, no eligible target or missing effect support as returned by the
context. Use actual level effects and omit invented damage or availability.
[Battle offers](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) ·
[Field recovery](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs)

The row keeps label, level, icon and cost. The reader below it provides the details and resets
reading position on selection, while row bounds and focus stay stable. Enter Target with the
short action identity and applicable hint in the same footer, yielding the description area.
Back restores the same level/slot and its description. Keep the old item behavior and reuse the
PO catalogs for English and Chinese, including localized refusal reasons. Field retains its
own eligible choice, target and Confirm flow.
[Battle layout](../../game/src/Battles/BattleMenuLayout.cs) ·
[Field preparation](../../game/src/Exploration/FieldActionView.cs) ·
[Reader](../../game/src/UI/ItemDescriptionView.cs)

## Current-round order (#2065)

Reserve a quiet readout immediately below round/gold during Movement, Main, Magic/Item and
Target. Default to current actor plus up to three subsequent eligible entries, with the count of
additional entries when truncated. Use distinct current/next roles, allied/enemy identity and a
disambiguator for same-name units. Repeated slots for one unit stay distinct in sequence. Keep
the strip read-only and outside the input focus order. Use a compact two-line block at 1080p
with the current entry above the ordered upcoming chips. A slot's side, current cell or stable encounter ordinal
can disambiguate duplicate names in the strip. Wrap names within the width budget. If a layout needs room,
reduce the number of later chips before shrinking the text or hiding the first eligible entry.
The overflow count keeps the first version usable without another input mode. At the real end of the generated slots show a localized
next-round-undetermined message. Once the next round is generated, rebuild from that queue.

Read `EngineBattleState.Queue` from `Cursor` to the first null-actor sentinel or end. Apply the
same current-state dispatch classification as `BattleAdvancer` for dead/unplaced and skipped
sleep/stun entries. The displayed eligible order can change after a committed status update,
including skip processing or waking before a repeated slot. Refresh from the new snapshot and
make the strip's current-round, current-state meaning clear. Reading the queue leaves skip side effects to actual dispatch. Apply existing disclosure to
identities. An otherwise undisclosed actor can occupy an unnamed slot. Read actor identity/control from the
live state so disabling, departure, revival, appearance and control changes are reflected.
Continue uses the restored queue and cursor. Projection preserves engine order, RNG, AI decisions,
round generation and persistence.
[Turn advancement](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
[Queue owner](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs) ·
[Existing implementation ticket](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2065)

## Implementation handoff

Main-gate owns acceptance and dispatch. Slice 1 settles composition. Slices 2 and 3 share battle
session input/presentation paths with it and with each other, so schedule their writes serially.
Slice 4 consumes the reader and phase layout from slice 1. #2065 consumes that same layout.
Serialize any overlap with other writers of its battle layout or trace. Declare exact paths
before each implementation. The following are principal owners and distinct acceptance outcomes.

| Slice | Player behavior and principal owners | Acceptance and risk |
| --- | --- | --- |
| 1. Board focus and decision layout | One cursor/field/card owner per phase, freestanding command diamond, stable child and footer regions, auxiliary and close-up retirement. `BattleSessionView`, `BattleMapViewport`, range/cursor views, `BattleCommandMenu`/`BattleMenuLayout`, `BattleInspectionView`, `BattleSceneView`. | Short real Move → command → Attack/Magic/item target → Back → Movement and inspection/options/return. Include a rejected mouse target, ally crossing, multi-target and field delivery. Inspect visible/hit rectangles, focus, map clipping and errors in both languages, at supported minimum/1080p/larger wide viewport, with wide values/long text and Reduced Flash. Verify private source cursor and public fallback. Resize a retained close-up token while paused and check art/card positions and unchanged delivery on return. |
| 2. Target edges and ordinary Stay | One selection per fresh direction, one gesture to Stay, continuous Movement. `BattleSessionView.Navigation`, `BattleCommandMenu`, current `GameInput` release tracking and `BattleCommandDispatcher`. | Keyboard and pad/d-pad or stick single target and multiple targets for physical, Magic and Item. Compare hold, neutral/repress, fast taps, menu/move → target with inherited direction, target Back/reentry, host cover, and continued held movement. Origin and moved Stay commit once on key/pad/click. Exercise next actor or after-turn delivery while holding Confirm. Search and Give/Drop confirmations, refusal and backwards routes still work. |
| 3. Inspect movement and share member presentation | One true movement field under the inspected unit and the same recognizable roster/detail experience in battle and exploration. `BattleInspectionQuery`, `BattleMovementQuery`, `GameSession`, `BattleInspectionView`, `PartyStatusView`, `PartyDetailsPanel` and shared roster surface. | Ally → enemy with another mover/terrain → empty, ally-through versus legal stop, detail/Map/Members/host transitions and cursor exit. Verify displayed facts and read-only state/seed equality. Compare long names, defeated/absent/not initialized, roster size and selection/scroll after Back with active Prepare in field only. |
| 4. Describe spells by level and context | Actual MP/effect/targets/range and current reason in the shared reader. `BattleChoices`, field recovery/choice owners, `BattleCommandMenu`/`BattleMenuLayout`, `FieldActionView`, reader and PO catalogs. | Compare two levels, a multi-target effect, self or EGRESS, lack of MP/Silence/unavailable content, legal field recovery and field-limited effect. Focus or scroll changes do not spend MP or move the rows/footer. Confirm/cancel still use their existing action path and restore the chosen row. |
| 5. Show the actual queue (#2065) | Current and next scheduled actors in the reserved left stack. Read projection beside `BattleAdvancer`/`BattleTurnFlow`. Render through the battle HUD/layout and document in the feature trace. | Compare the next actual eligible actor, repeated slots, same names, status skips and changes, dead/unplaced entries, sentinel and empty remainder, new round and cold Continue. Verify hide/show by phase and equality of seeds, resources and queue after repeated projection and cover/return. |

[#2068](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2068) owns formal BLAZE
playback assets and shares the playback writer only as needed. [#2071](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2071)
owns the field Prepare command icons and its six actions. [#2070](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2070)
owns special dialogue portraits, [#2073](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2073)
owns story wait/Confirm, and [#2062](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2062)
owns pickup glints. Schedule shared-path work around those owners.

For implementation use short controlled states through the ordinary presenters and actual
input, state, geometry and errors, including the needed source/private or public asset selection.
Use engine unit coverage only for changed real behavior. Engineering acceptance uses the
state, input, geometry and error observations above. Human readability, device feel and visual
approval stay with #1456.
This design uses direct document, reference and scope checks. Original pulse parity, natural
source reach beyond the linked evidence and native observations of the proposed changes remain
**Unknown**.
