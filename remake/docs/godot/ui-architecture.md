# Godot UI responsibilities and refactoring design

The current decision in [#2191](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2191)
is how to migrate two production surfaces: the complete field/battle member page and Settings
with its child pages. **High rigor** applies because moving input from the root to Godot controls
can change which player action fires when a page opens, closes or remounts. Both production surfaces
now use native focus and GUI picking. The
[campaign capability trace](../capability-status.md#campaign-feature-trace) records shipped behavior.

Both callers already use the
[complete member page](battle-ui-composition.md#shared-member-page-2154). Battle Main already
uses the [vertical command list](battle-ui-composition.md#vertical-battle-main-2090), with
the accepted blue rows and mapped Up/Down. The [visual language](ui-design-language.md),
[battle phases](battle-ui-composition.md#phase-focus-and-return), [field services](field-and-shop.md)
and [host return](host-ui.md) supply the player contracts.

## Current ownership and transition paths

### Gameplay phase and action admission

`GameSession` publishes the current snapshot. Battle selection, preview stop, action, target,
transaction and pending delivery belong to the session. `BattleCommandDispatcher` rechecks admission
for selection and confirmation. Field waits, source-text servicing and service commands belong to
the reached program. Delivery owners report actual completion with the matching token.
`SessionPresentationOwner.IsBattle` routes the battle snapshot to the field presenter through
before-battle/load/start delivery. A mounted or visible battlefield alone cannot decide who advances
the session. [Session contract](../application/session-and-programs.md#session-authority) ·
[Presentation routing](../../game/src/SessionPresentationOwner.cs) · [Battle
dispatch](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs)

### Battle page and focus

A confirmed legal stop enters ActionChoice. `BattleSessionView` derives `BattleDecision` from the
live selection, offers, active presentation and disposable navigation. The selected action projects
Target or Commit. Main versus Magic/Items comes from the local parent while ActionChoice is active.
Action entry prefers self or the first enabled target from the resulting action state. The menu,
board, mapped actions and pointer gestures consume the same decision. An invalid cell clears the card,
cursor and effect marking. Confirm requires the attempted candidate to agree with the enabled
session selection. Target Back clears the chosen action and target through the session and returns
to the retained parent and row. Search keeps its distinct confirmation, ordinary Stay commits
directly, and Main Back resumes Movement at the current preview when it is still in range. [Decision](../../game/src/Battles/BattleDecision.cs) ·
[Transition owner](../../game/src/Battles/BattleSessionView.Decision.cs) ·
[Battle composition](battle-ui-composition.md#battle-decision-projection-2107)

### Inspection and field pages

`BattleInspectionView` owns Cursor, direct Full, Options, Map and the read-only Members caller. A provisional move first returns through the real movement delivery before inspection can
open. Direct Full returns to Cursor, Map and Members return to Options, and Cursor Cancel restores Movement. Inspection rejects stale session/revision/actor
identity, with a specific refresh after a successful paused Save can publish music. In exploration,
`ExplorationSessionView` owns field-menu admission, the active service/wait and step settling.
Both Member callers mount `MemberPageView`, which owns actor selection, pane focus and scrolling.
`PartyStatusView` supplies the field roster, records, gold and Prepare action. `FieldActionView` owns the preparation page, row
and parent return. Field preparation sends a revision-bound command. Read-only inspection/roster
calls and local Back do not publish a gameplay command. [Inspection entry and
return](../../game/src/Battles/BattleSessionView.Inspection.cs) · [Inspection
pages](../../game/src/Battles/BattleInspectionView.cs) · [Field
entry](../../game/src/Exploration/ExplorationSessionView.Menu.cs) ·
[Prepare](../../game/src/Exploration/FieldActionView.cs)

### Device, repeat and gesture

`GameInput.Resolve` resolves installed bindings, button/axis edges and neutral state.
`GameRoot._Input` asks the battle presenter for its fresh direction policy.
Battle owns repeat on preview-step arrival or a deadline for ordinary
navigation, with fresh edges in Target. Exploration owns walking and neutral input service on its
own clock. `MenuPointer` matches button, action ID, rectangle and caller-supplied boundary across
press/release. Field dialogue may keep the same wait token/phase across revisions, so its gesture
boundary intentionally uses that identity. Field reading services reveal, scroll and acknowledgement
against the live text span. Description scrolling is a local view operation. [Input
mapping](../../game/src/Input/GameInput.cs) · [Battle
repeat](../../game/src/Battles/BattleSessionView.Navigation.cs) · [Field
clock](../../game/src/Exploration/ExplorationSessionView.Clock.cs) · [Pointer
contract](../../game/src/Input/MenuPointer.cs) · [Field choice
pointer](../../game/src/Exploration/ExplorationSessionView.Mouse.cs) · [Text
reading](../../game/src/Exploration/ExplorationSessionView.Reading.cs)

### Host cover and return

`GameRoot` owns entry, name, Menu, Settings, slots and recovery priority. Its native GUI handoff for
name/entry/slots is intentional. `BeginMenuEntry` can settle a field step before host mount.
`ApplyHostPause` disables presenter processing and pauses audio. Battle responds to process
notifications by hiding decision HUD, inspection and decision marks. On resume it retains inspection
focus or reprojects without restarting playback. The root disarms movement, wait and acceleration
and blocks held mapped input until neutral. Individual pointer owners also suspend press capture.
Ordinary Continue validates the candidate and its required presentation before releasing the
previous session. It reconstructs the presentation of the restored gameplay boundary. Same-session
modal Back retains a page. A replacement session starts with fresh UI focus. [Host input and
mount](../../game/src/GameRoot.cs) · [Pause and recovery](../../game/src/Entry/WindowSupportHost.cs)
· [Save/Continue contract](../application/battle-save-and-continue.md)

### Geometry and resources

`PresentationCanvas` and the world viewport provide coordinate/layer boundaries. `BattleMenuLayout`
measures and clips the decision UI and reports visible hit areas. `BattleSceneView.RefreshGeometry`
can reflow a paused close-up from live delivery. Roster rows own local textures, actor focus and
scroll. Field and battle callers own query and capability. The shared page borrows the selected row texture for its portrait and clears the borrow on tree exit. `IdentityIcons` prepares decoded replacements before
disposal of the old set. `GameRoot.ReleaseSession` detaches and frees old view nodes before ending
replaced-session audio. A battle view may also leave/reenter the tree during field handoff without
being retired. [Layout](../../game/src/Battles/BattleMenuLayout.cs) ·
[Playback](../../game/src/Battles/BattleSceneView.cs) ·
[Roster](../../game/src/UI/MemberRosterView.cs) · [Shared
summary](../../game/src/UI/MemberPageView.cs) · [Identity
textures](../../game/src/UI/IdentityIcons.cs)

Preserve the useful boundaries. `BattleChoices` tests offers through the same selected rule and
confirmed-stop state as the dispatcher. `QueryBattleMovementRange` and
`QueryBattleInspectionRange` already supply rule-derived cells, with traversable and stoppable
cells distinguished. `MemberRosterView` successfully shares row geometry, clipped hits,
actor-based selection and scroll without deciding who is on the force, who can be inspected or
who can Prepare. Battle retains defeated/absent details and active-force order, while field
retains its own membership and preparation rules. Host Menu's left/right reading and clipped
row hits are a working example of separate read and action intents. The source-text delivery and
step arrival policies are important counterexamples to a common repeat, press or reading clock.
[Choices](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs) ·
[Movement query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMovementQuery.cs) ·
[Battle roster query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleMemberQuery.cs) ·
[Host Menu](../../game/src/Entry/SystemMenuView.cs) ·
[Accepted roster observation](battle-ui-composition.md#reproduce-the-shared-roster)

## Native controls for members and Settings

**Recommendation.** Give containers responsibility for child geometry and give each open page one
Godot focus owner. Use native GUI dispatch to pick the visible control under the pointer. Extend
a control or its owning presenter where the game distinguishes *reading* a row from *acting* on it.
Keep the existing `GameInput` mapping and host precedence. This removes two concrete maintenance
costs: adjusting coordinates in multiple controls when text wraps, and reconciling a painted index
with the button that actually received input.

The [adapter](../../game/Sf2.Remake.Godot.csproj) pins Godot .NET 4.7.2. The API at the installed
engine revision supplies focus neighbours, deferred focus placement, clipped GUI input, focus
following and release-mode button signals. A plain button is sufficient for an ordinary choice.
An `ItemList` can represent a simple single selection, but the member rows compose a portrait,
wrapped condition, status treatment and an independently readable sheet. Reuse those children
in focusable rows. A UI plugin would still need the same admission, return and input contracts.
There is no uncovered control requirement in these two pages that justifies adopting one.
[Containers](https://docs.godotengine.org/en/stable/tutorials/ui/gui_containers.html) ·
[Theme inheritance](https://docs.godotengine.org/en/stable/tutorials/ui/gui_skinning.html) ·
[Focus navigation](https://docs.godotengine.org/en/stable/tutorials/ui/gui_navigation.html) ·
[4.7.2 Control](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/doc/classes/Control.xml) ·
[BaseButton](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/doc/classes/BaseButton.xml) ·
[ScrollContainer](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/doc/classes/ScrollContainer.xml)

### Member page from either caller

**Confirmed.** Both callers mount the same capped 1712px column on the fitted game canvas.
Native box containers stack its header, expanding body, summary row and measured footer. The framed
roster gives spare height to its scroll viewport and reserves a separate arrow row. The member
sheet puts the borrowed portrait above the attribute grid. The adjacent name and class wrap beside
the vitals. The spell column reserves 196px for the reader while the four 88px-minimum pack rows
use their own scrolling column. Field accounting uses equal-width cells. Battle hides the frames
and retains the same reading height. The direct board Full sheet still uses `ArrangeInspection`
and 4× icons. [Page](../../game/src/UI/MemberPageView.cs) ·
[Member sheet composition](../../game/src/Exploration/PartyDetailsPanel.Member.cs) ·
[Native and visual checks](battle-ui-composition.md#reproduce-the-native-member-layout)

**Confirmed.** The focused roster row, spell reader or slot receives mapped navigation. Up/Down
follows explicit wrapping neighbours in the roster and pack, and scrolls text in Spells. Target
cycles those three panes. Left/Right changes the sheet's actor from any pane. Focus-entered signals
retain the pane and numbered slot for restoration. The roster reconciles the selected `ActorRef`
against each new projection. Its nearest-survivor rule checks neighbours in the previous order,
preferring the following member on a tie. Empty rosters use the header as a focus target.

Godot picks a release-mode button over the visible row, reader, arrow or footer action. These
buttons leave keyboard focus on its previous owner during a press. On a valid release they focus
the chosen row or reader, switch actor in place, or invoke the caller's action. A gesture carries
its button, clipped bounds, actor, pane and scroll context. Roster buttons also carry their row's
actor identity. Motion outside the original bounds, a wheel, resize or window exit cancels capture.
Scroll containers own wheel reading. The page pairs window subscriptions across tree entry and exit.

Field Prepare retains the member, slot and roster scroll. Battle exposes Back and applies
its inspection release barrier on the return to Options. The host consumes mapped and unmapped
keyboard/pad edges on the open page and yields pointer events to its controls after higher-priority
host handling. [Page](../../game/src/UI/MemberPageView.cs) ·
[Pointer button](../../game/src/UI/MemberPageButton.cs) ·
[Observed return and remount](battle-ui-composition.md#native-member-input)

### Settings and its children

**Confirmed.** The root, editor pages and Battle Speed use a centered, capped panel with a
scrollable choice column, fixed details region and separate footer. Changing page mounts a
new set of choices. Editing a value updates the mounted captions and details in place. Up/Down
moves the viewport's actual focus through explicit neighbours, including the footer and wrap.
The focused button supplies the stable choice ID, marker and description. Disabled actions are
skipped on mixed pages. On a wholly read-only page the disabled values take reading focus.
Back restores the launching ID and list scroll. Left/Right adjusts the focused value or scrolls
the list. Target pages overflowing details. [Settings](../../game/src/UI/SettingsView.cs)

The GUI picks the clipped row or scroll region. Deliberate motion focuses a readable row.
A release-mode button activates through its native signal after the page's gesture check.
The root handles matched right-click Back over any child. Help uses the same buttons for
Previous and Next. Mapped Left/Right/Confirm turn its pages and Up/Down scroll the text.
Its Back restores the Controls → Help row. [Settings button](../../game/src/UI/SettingsChoice.cs) ·
[Help](../../game/src/UI/ControlsHelpView.cs)

Raw capture, release draining, duplicate and required-family refusal, the draft and the
preference store retain their existing owners. The host alone prepares and publishes a
binding change. Direct Battle Speed returns to its inspection option. [Editor](../../game/src/UI/SettingsView.Bindings.cs) ·
[Host](../../game/src/Entry/SettingsHost.cs) · [Observed returns and visual review](../verification/host.md#native-settings-focus-and-gui-picking)

### One event from device to page and back

1. Keep window recovery, release barriers and raw binding capture ahead of ordinary page input
   in `GameRoot._Input`. For a migrated page, update device hints and resolve physical key/pad
   events once using the installed `GameInput` actions. Consume these events there, including
   unmapped keys, echoes and neutral releases, so default `ui_*` bindings cannot activate a row
   after a remap. Translate an admitted navigation step to the focused control's explicit neighbour
   (`FindValidFocusNeighbor` / `GrabFocus`). Dispatch Confirm for the focused ID or page-specific
   operation once. Both mapped activation and native pointer signals call the same operation.
   For a mapped Apply or capture opener, pass its actual key/button/axis event synchronously.
   A parameterless `Pressed` signal cannot identify which old binding must release. A completed
   mouse click has already released: keep it out of the host's non-mouse release drain. Preserve
   the member page's entry/Prepare-return neutral barrier.
2. For pointer events, the root yields to GUI picking only on the active migrated surface, before
   forwarding to any covered field, board or system menu. Use a full-page input shield, ignored
   decorative children, clipped scroll containers and a release-mode button. Stop wheel propagation
   at the modal boundary. Intercept right-button edges for the active page before GUI delivery so Back works over a child that stops mouse input.
   Raw Capture still sees keyboard/pad first. Its visible pointer Cancel must have one route as well.
   Cover and restore the same caller through the existing host pause, audio and disarm operations.
   Keep battle cell navigation, Target fresh-edge rules, movement repeat and wait delivery on their
   existing owners. [Root](../../game/src/GameRoot.cs) · [Device edges](../../game/src/Input/GameInput.cs) ·
   [Input order](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html)
3. Use the GUI-picked control as the target. Where a native click can rearm after leaving and
   re-entering, use the existing gesture cancellation policy as a small veto on the single
   activation callback. Keep only press identity, button, visible bounds and caller boundary
   needed to reject drag-out/back-in, chord/double click, scroll, resize, changed actor/page,
   cover or window exit. Right-click uses a matched press/release on the same page. A stale
   release after any transition is consumed. Retire the migrated callers' manual rectangle
   searches and direct click dispatch together. Other `MenuPointer` consumers keep their own
   boundaries. [Gesture contract](../../game/src/Input/MenuPointer.cs)

### Hide, remount and disposal

Build a node's owned children and connect their local signals once. When hidden or covered, clear
armed presses, remember the reading identity/scroll and withdraw focus from the covered subtree.
On same-session return, restore focus only after it is visible and still current. For subscriptions
to a longer-lived window, connect on each `_EnterTree` and disconnect on its matching `_ExitTree`.
Godot ordinarily calls `_Ready` once per instance. The #2152 command rows subscribed to
`Window.MouseExited` in `_Ready` and unsubscribed on tree exit. The retained menu then missed
window exit after remount. #2154 restored the pair on tree entry/exit. Settings subscribes to
window resize and mouse exit for its mounted lifetime. Help shares its parent’s gesture. Keep row-signal lifetimes tied to their row and
reject callbacks from a retired page. Clear the sheet's borrowed portrait before disposing the row
texture and rebind from the live actor on remount. A replacement session gets fresh UI state.
[Observed failure and correction](battle-ui-composition.md#reproduce-registered-portraits) ·
[Command menu](../../game/src/Battles/BattleCommandMenu.cs) ·
[Node lifecycle](https://github.com/godotengine/godot/blob/ed1daf0bf001b61586d9930840f2f1394092c079/doc/classes/Node.xml)

### Finite implementation slices

Estimates are engineer-days for one implementer including debugging, visual review and independent
review time. They assume the owned installation, production preview and existing disposable inputs.
Each PR replaces one production path with its consumers and can be reverted with those consumers.

| Player outcome, in delivery order | Main risk and acceptance before proceeding | Estimate |
| --- | --- | --- |
| 1. Read a complete member sheet in field and battle. | Implemented with containers and the existing input route. [Layout, visual review and ordinary returns](battle-ui-composition.md#reproduce-the-native-member-layout) | Implemented |
| 2. Edit a preference or binding and return to the same caller. | Implemented in Settings, its editor and Help. [Mapped input, release drain, refusals and caller return](../verification/host.md#native-settings-focus-and-gui-picking) | Implemented |
| 3. Browse members and slots by mapped device or pointer, then resume play. | [Native focus, clipped picking, retained Prepare return, remount and neutral battle return](battle-ui-composition.md#native-member-input) | Implemented |

Use the [production demo routes](../../game/probes/previews/README.md#scene-ownership) for actual
appearance and reading comfort. F5's public sample can display the unsupported Prepare outcome.
Use the [connected route](battle-ui-composition.md#reproduce-registered-portraits) for actual Prepare.
Use the [host observation](../verification/host.md#battle-system-settings-observation) and
[remap observation](input-remapping.md#reproduce-the-installed-controls) for input and state.
[#2147](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2147) owns broader scene/subpage
coverage. Necessary pictures belong only to the dedicated UI demo and stay ignored locally.
Gameplay acceptance reads live input, state, projection and errors. Physical-device comfort remains **Unknown**.

## Findings and limits

The pre-migration findings from [#2089](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2089)
refer to `3bff875d`. The implemented decisions and reading behavior have their own linked witnesses.
A static defect is actionable within that boundary. **Inferred** findings specify the maintenance or
runtime consequence that has yet to be witnessed. Prior native acceptance applies to its named paths.

### Pre-migration 1 Target Back ordering

P2. **Confirmed at the inspected base.** Whether the old intermediate projection reached a drawn
frame or changed an accepted gesture remains **Unknown**. Migration 1 derives the parent after the
session result and projects once.

Target/Commit Back in the inspected `BattleCommandMenu.Handle` called `Submit(ReturnToBattleActions)` before setting
Magic/Items/Main. The dispatcher publishes ActionChoice, and `BattleSessionView.Send` synchronously
calls `Present`. At that intermediate projection the menu still holds Target/Commit. The new snapshot
has no selected action, so the old footprint drops while the stale page can still drive Target hints and
input policy. The caller then restores the local page and the outer battle input path presents again.
The first target when entering an action has a similar nested presentation before the candidate is
installed. The accepted Back corridor reached the correct final page. [Inspected
menu](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/3bff875d994758c30b44086bebf57c64346a0dfd/remake/game/src/Battles/BattleCommandMenu.cs) ·
[Current submission and transition](../../game/src/Battles/BattleSessionView.Decision.cs)

### Pre-migration board pointer classification

P2. **Confirmed at the inspected base.** Migration 1 classifies these events before selecting.

With Target active, over a board cell outside menu chrome, a pressed wheel or middle button reached
`BoardTarget` before `MenuPointer.Read` filters it. On an admitted actor the path can submit
`SelectTarget`. On an empty or ineligible cell it clears the attempted candidate. Those paths change target focus or selection. Inspection's `BoardPointer` also sets its Cursor on a pressed non-right button before filtering.
The scoped reproduction compares candidate, inspection focus, revision and submissions.
[Battle HandlePointer](../../game/src/Battles/BattleSessionView.Decision.cs) · [Inspection
BoardPointer](../../game/src/Battles/BattleInspectionPointer.cs) ·
[MenuPointer.Read](../../game/src/Input/MenuPointer.cs)

### Decision reading and inspection return

**Confirmed** against the current adapter and the
[bounded native observations](battle-ui-composition.md#shared-description-reading-and-inspection-return).
The earlier battle footer only took a wheel gesture. Shared description reading now uses the
active Left/Right mappings in vertical Main, Magic, Item and field Prepare descriptions. Field
Prepare directs the operation to the selected description alone while it is present. Target
retains its existing commands with a short action and availability footer.

Inspection puts return instructions in its framed footer. Cursor and detail sheets also show a
Back button whose complete left-click must match the actual button rectangle. Options, Map and
Members use mapped Cancel or a complete right-click. Movement and inspection share the unit card and an independent terrain panel for cell coordinates and mover-qualified effect. The shared compact status card reads the live ally's current
level from the acting projection or actor state in Movement, Target and playback. Enemies keep
their established disclosure.
[Description reader](../../game/src/UI/ItemDescriptionView.cs) ·
[Inspection view](../../game/src/Battles/BattleInspectionView.cs) ·
[Compact card](../../game/src/UI/BattleStatusCard.cs)

### DesignPreview production entry

All F1–F10 tabs instantiate or reuse the production `Main.tscn`. The preview owns the
toolbar, controlled startup choice, reset, fixed review viewport and pointer translation.
F1 uses ordinary entry and one explicit isolated save. F2/F3 use the admitted authored
dialogue and Church programs. F5–F7 share one live field session, so Party and Shop read
the same inventory. F4/F8 use the same Practice Yard production battle, and F9 uses
ordinary authored outcome continuations. F10 opens the actual System Menu over its
eligible caller. Preferences live in memory. Game entry, admission, presentation and
return keep their production owners.

`StoryTheme` supplies one gold texture for the frame of all panel, popup and tooltip styles.
Presenters retain their own content insets. The [sample routes](../../game/probes/previews/README.md#scene-ownership)
describe which actual authored inputs exercise each view. The fixed review window fits the
production viewport. Desktop window recovery and device/appearance acceptance retain their
ordinary separate review boundaries.

### Risk from parallel route eligibility

P2. **Confirmed** independent predicates in the #2089 inspection. **Inferred** drift outside the observed routes.

At that base, battle action admission, pointer, navigation repeat and projection checked related
wait/cover/stage conditions. Today the root asks the battle presenter for its direction policy. Field independently routes mapped input, pointer, draw calls and clock. For example pointer
forwards open Party before the general field guard and leaves part of that guard to Party/Prepare, while
mapped input reaches Party after rejecting presenter failure and handoff. Some of the differences are
needed by services and the field-to-battle handoff. Matching predicates across layers is fragile when
adding a page, shortcut, cover or rejection. The duplicate legacy spell/item/target cycling and
candidate fields inside `BattleSessionView` are a separate fallback from the visible menu. Trace
supported callers before retiring that fallback. [Root input](../../game/src/GameRoot.cs) · [Battle
input](../../game/src/Battles/BattleSessionView.cs) · [Repeat
boundary](../../game/src/Battles/BattleSessionView.Navigation.cs) · [Field mapped and pointer
routes](../../game/src/Exploration/ExplorationSessionView.cs) · [Field
pointer](../../game/src/Exploration/ExplorationSessionView.Mouse.cs)

### The action overlay omits admitted recipients

P2. **Confirmed** discrepancy between projected and resolved effect.

`BattleActionRangeView.Present` scans cells using its own Manhattan calculation, terrain mask and
spell-radius lookup. It knows all-ally healing only from `Action.Spell`. The admitted Healing Rain
binding is an item with `FullPartyHp`, self center and a placed-side recipient scan. Its Item action
reference has no spell, so the overlay shows only the center even when the resolver applies the effect
to several allies. Attack/support items also carry their effect through item admission. The spell AoE ring denotes geometry. Recipient rules also apply eligibility inside it. Keep that
distinction when replacing the duplicate calculations. This is a preview defect with no evidence of
mis-settlement. Application and Domain own the admitted footprint. Godot owns the marks. [Current
overlay](../../game/src/Battles/BattleActionRangeView.cs) · [Recovery item
admission](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ItemAction.cs) · [Recipient
rules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/HealingTargetRules.cs) · [Healing Rain
binding](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleActionBindings.cs)

### Texture borrowers depend on projection order

P3. **Confirmed** lifetime assumptions. **Inferred** stale-borrow and partial-publication risk.

Roster texture replacement clears the sheet's borrow before disposing the old row handle.
`PresentRoster` rebinds the selected live texture. Prepare hides the sheet and retains its reading
selection. `MemberRosterView._ExitTree` drops row textures. The registered field remount observation
checks the new borrow against its row after re-entry.
`BattleMenuLayout` retains its icon set across reparent, `BattleMapViewport` releases on
rebuild/predelete, and `BattleSceneView._ExitTree` clears its texture cache while retaining the mounted
overlay. Current handoff retains only the permissible board during return. Tree exit is therefore an
unsafe generic destroy signal. The accepted registered portraits observation exercised both callers and
reported live textures and zero disposed-texture errors. Portrait leaks and failed reparent remain Unknown. Keep each texture owner and name its valid borrow, remount and error boundary when changing
that caller. [Roster](../../game/src/UI/MemberRosterView.cs) ·
[Summary](../../game/src/UI/MemberPageView.cs) · [Layout icon
lifetime](../../game/src/Battles/BattleMenuLayout.cs) · [Board
lifetime](../../game/src/Battles/BattleMapViewport.cs) · [Playback
lifetime](../../game/src/Battles/BattleSceneView.cs)

### Prepare command rows

[#2071](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2071) gives the five Member →
Prepare commands the common `FieldServiceMenu` projection. `FieldActionView` retains focus, Back,
the revision-bound transaction and the `FieldChoicePanel` subpages. The Item command family supplies
Equip, Give, Use and Drop. The field command family supplies Magic. Mapped Cancel and right-click
return through the existing caller to the selected member. [Production observation](field-and-shop.md#prepare-command-icons)

Some defenses express real boundaries. Stale session/revision and actual wait tokens protect
commands and acknowledgements. Distinct press/hold invalidation is needed at different physical
and logical transitions. Keep these checks, content preflight, and local render guards for
asynchronous node lifecycle. Remove equivalent guards only once the higher owner guarantees
admission. Examples to inspect during migration are Party's already-returned `Open` branch
repeated in `ExplorationSessionView.HandlePointer` and the repeated details-page test inside
`BattleInspectionView.Handle`. Preserve the fallback behaviors reached by supported callers. The
battle session's long `Present` currently combines eligibility, strings, widget projection, scene
advancement and observation payload. The actionable coupling is visibility and command authority
crossing that method, irrespective of file length.

**Unknown.** The exact transient frame at nested Target Back, physical device release/reading
comfort, and texture behavior on a newly introduced detach or bad raster replacement were outside
the retained observations. The two retained WAV objects in #2082 are the shutdown evidence at this boundary. Their relationship to intermittent exit failure remains Unknown. `SessionAudio` owns those resources.
[#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) retains the paused
shutdown investigation. Human experience and physical devices remain with
[#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456). Original pixel and
timing parity remain Unknown.

## Proposed responsibility changes

The default is to derive the active decision surface once in the existing scene presenter and
reuse that decision for input and projection. Move rule-derived queries to the existing rule
owners. Share only controls and gestures with established matching semantics. Keeping the
current implementation and patching labels or pointer filters alone closes individual defects
quickly, but the next change would still need to synchronize independent page/availability
checks. A global navigator or general declarative menu framework could unify machinery at the
cost of mirroring session state and conflating gameplay waits, native host controls, inspection
and field service transactions. The selected design limits changes to concrete transition and
query boundaries.

### Active surface, input and Back

- **Root** selects the current host receiver or world presenter using the existing finite modal
  precedence. Use that same choice for mapped input, pointer and world cover, including explicit
  native GUI delegation. The active gameplay presenter reports the directional edge policy used
  by `GameInput`. The root owns focus loss/device release/host cover and replacement notification. The gameplay presenter owns when its movement, preview or wait may service. Reuse the existing
  process-mode pause and release barrier. Migrate the battle consumer first and leave unrelated native
  entry/capture and service paths on their existing contracts until their own changes need this
  boundary.
- **Battle session presenter** derives one disposable decision value from the live snapshot,
  active inspection/host cover, and local navigation. It carries the current surface, allowed
  operations and their input policy. Keep it local to presentation and rebuild as authority
  changes. Main versus Magic/Items belongs to local navigation only while the session allows
  ActionChoice. Target and Search/transaction commit derive from engine phase. Keep focused
  spell level/item slot and parent identity, with a nearest valid fallback when the parent choice
  disappeared. The attempted candidate remains local, while Confirm requires an enabled
  candidate that agrees with the live selection. Rejection can expose an unavailable reason
  without inventing a new selection or moving focus to another page.
- **Command/result order** is explicit. The presenter evaluates a local intent against the current
  surface, submits the necessary gameplay command, resolves local focus and Back against the
  returned snapshot, then projects the coherent result. Use ordinary return values or a direct
  presenter call. On Target Back, the caller knows the parent before submitting. Install it only
  on accepted ActionChoice. Preserve the current Target on rejection. Action entry can still
  submit its normal initial `SelectTarget` after `SelectBattleAction`. Keep the session, audio and
  observation sequence for both submissions. Publish the final decision projection with the
  resolved focus and candidate, including a rejected initial target. Terminal/failure/hand-off
  results invalidate the old interaction. This retires the menu's synchronous submit-and-reread
  contract without moving scene clocks or suppressing actual gameplay events.
- **Battle decision controls** render allowed actions, captions, reasons and focus from that value. They return
  clipped rectangles or stable action IDs from actual geometry. A layout rectangle may block
  board input without being an activation target. A disabled row may still be readable if the
  caller allows it, with the same explicit refusal for mapped and pointer activation. Classify
  move, left/right gesture, wheel and other buttons before dispatch. Reuse `MenuPointer` for
  matching gestures and invalidate captures and repeats on changes of page/owner, eligibility,
  client rectangle, focus or cover. A release cannot substitute for a fresh activation in the
  new owner. Retain the existing field token/phase matching and different movement clocks.
- **Reader and return** use the existing fixed-height description body and measured footer where
  the control semantics match. Use an explicit mapped reading intent with a visible overflow
  hint. Left/Right is a workable separate pair for list pages that use Up/Down to select.
  Use that pair on the restored vertical Main and the child lists where the page has it free.
  Keep Target's accepted directions and use a concise action and availability footer there.
  If supported layouts still overflow, accept a dedicated read operation only when the page
  has an existing free mapped action and displays how to enter and leave the reader. Apply wheel input to the reader under the pointer.
  Read changes scroll only and stays clear of selection, command publication and cost. Inspection
  uses one visible Back action with the displayed hit rectangle and the same local return for
  pointer and mapped input. Dialogue reveal/acknowledge and the auxiliary transaction Confirm
  reading step keep their current semantics.
- **Lifetime** distinguishes cover (pause and retain), handoff/reparent (release captures and
  subscriptions as appropriate, retain or rebuild according to owner), and destruction (unbind
  borrowers before releasing owned resources). Restore a failed candidate through the existing
  error path and keep the current session intact. Only make a portrait replacement all-or-nothing
  or change borrowing if a changed caller crosses the synchronous publication boundary. Prefer
  storing roster focus by `ActorRef` and keeping a live row/texture projection. Keep shared member
  rows independent of Party Prepare, battle disclosure, and service eligibility. Route audio through the existing session audio owner.

### Read queries and renderer responsibilities

- **Action geometry.** Extend the battle choices/read boundary with a typed description of the
  selected action's admitted reach and optional candidate footprint. Read it for the live
  session/revision, acting unit, action reference and attempted candidate. Resolve against the same
  `AtConfirmedStop` state, selected rule and action admission as `BattleChoices.RequireTarget`.
  Reuse `BattleRange`, `HealingTargetRules` and existing attack/support recipient enumeration.
  Distinguish selectable centers, painted reach, area cells and applicable recipient positions.
  Preserve the view's terrain mask where it is an intentional display convention, with a legal
  center remaining actionable even if display geometry is clipped. A rejected/missing candidate
  has no candidate-specific footprint. Rules supply all-side/source-roster selection. The scene
  consumes the resulting positions and reasons. Read the nominal area separately from currently
  applicable recipients, including suppressed effects. Use the rule's query path for geometry
  and reserve `Prepare` for gameplay execution. Keep queries free of payments, random draws
  and published revisions. Hand the result to the existing board painter for clipping, color,
  opacity and Reduced Flash. [Rule interface](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs) ·
  [Attack recipient rules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2AttackSpellAction.cs) ·
  [Support recipient rules](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2SupportSpellAction.cs)
- **Spell description.** `BattleChoices` and `FieldRecovery` project `SpellDescriptionFacts`
  for the exact `SpellRef`. The Godot formatter consumes typed effect operands and current
  restrictions in the shared reader. Field recovery owns free NoEffect and clamped MP.
  `SilenceRules` owns whether an otherwise eligible battle cast pays and loses its effect.
  Future target geometry can consume the same admitted reach and radius operands.
  [Shared facts](../../src/Sf2.Remake.Application/Runtime/SpellDescriptionFacts.cs) ·
  [Formatter](../../game/src/Localization/SpellDescriptions.cs) ·
  [Behavior and reproduction](battle-ui-composition.md#selected-spell-descriptions)
- **Current-round queue.** `BattleRoundOrderQuery` reads the stored queue/cursor through the
  sentinel or end. It shares current-state eligibility with `BattleAdvancer` and board disclosure
  with inspection. The dispatcher owns skip effects, AI and round generation. Repeated slots
  remain distinct. The production decision HUD leaves the action strip unmounted, including
  in the shared DesignPreview entry.
  [Queue contract](battle-ui-composition.md#current-round-order-2065) ·
  [Query](../../src/Sf2.Remake.Application/Runtime/Battles/BattleRoundOrderQuery.cs) ·
  [Advance and skip](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
  [Queue and cursor](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)

## Migration and independent acceptance

The first two battle and reading boundaries below are implemented. The target-footprint query
remains a separate proposal. The [native UI slices](#finite-implementation-slices) consume the
shipped player behavior.

1. **Coherent battle action entry, rejection and Back (#2107).** `BattleSessionView` owns local
   focus and the action/target submission sequence. `BattleDecision` supplies the menu, board,
   pointer and input policy from the same live revision. Session receipts, audio and pacing still
   receive both action entry submissions before the final decision is drawn. Back derives its page
   from the returned selection stage and the retained parent. Movement, direct Stay, Search,
   transaction readers, read-only inspection and actual completion owners keep their boundaries.
   The [bounded reproduction](battle-ui-composition.md#reproduce-the-battle-decision) covers the
   shared input surface, retained parent and host/resize behavior. Reverting this slice restores
   routing as a unit and needs no state conversion.
2. **Shared reading behavior and visible inspection return (#2113 and #2085).**
   `ItemDescriptionView` owns one clipped description and an overflow-dependent mapped hint.
   The decision and Prepare callers route reading to that control with selection and transaction
   state owned by the existing phase. A page, actor or focused offer change resets the body.
   Same-page host cover retains scroll and focus. Inspection presents one visible pointer Back
   control with the same parent as mapped Back. The reused compact status card adds current levels
   for allies. The [bounded reproduction](battle-ui-composition.md#shared-description-reading-and-inspection-return)
   exercises the reader, neighboring phases, retained roster, and card at real input/playback
   boundaries. Revert the reader and Back with their direct consumers if the input boundary regresses.
   Field Party follows this reader contract in [#2094](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2094).
   Its independent spell, inventory and reading regions keep the four carried positions and their
   item text visible together. Field summaries align live party gold and the selected member's
   record with shared metrics. Battle Member mounts the complete shared page with its own read query and Back. [Current field behavior and verification](field-and-shop.md#reproduce-four-slot-capacity-and-accounting)
3. **Rule-backed battle target footprint.** After slice 1 for the final adapter integration
   (the pure rule/query work can be scoped independently), replace `BattleActionRangeView`'s
   spell lookup and distance/party selection with selected-action geometry from the authoritative
   read boundary. Align the existing movement overlay without changing its query. Keep range
   meanings and layering. Accept a physical or single target, radius effect at a legal stop,
   rejected center, party-wide item and a suppression case where supported. Include an ally
   outside the area or an ineligible actor inside. Compare nominal geometry and applicable
   recipients with the actual resolution from that controlled state, preserving a separate check
   for no state/RNG/payment changes from reading. Cover new rules with small engine behavior
   assertions where needed. Revert the query and consumer together before downstream uses.

[Independent review](ui-scene-review.md#review-changed-ui-responsibilities) complements the
existing scene feature checks. The accepted [#2079 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2084#issuecomment-7087838808)
exercised final Target Back, legal/rejected gestures, wheel overflow, multiple recipients and
paused resize. Those checks did not inspect the intermediate projection sequence, unrelated
mouse buttons, mapped overflow or a full-party item's preview. The
[#2082 review evidence](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2088) covered
retained focus and live textures on a real bounded path. It did not generalize that lifetime
to arbitrary detach or partial decode failure. Check one adjacent path that can contradict the
changed ownership claim. Reuse the affected native observations and the existing adapter build
for implementation. Documentation-only design changes use direct source and document checks, scope and link verification, and
`uv run sf2 verify plan --base origin/main --head HEAD` on the committed candidate.
