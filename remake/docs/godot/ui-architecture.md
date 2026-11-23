# Godot UI responsibilities and refactoring design

[Issue #2089](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2089) governs this
documentation slice. The inspected base is `3bff875d994758c30b44086bebf57c64346a0dfd`,
which includes the phase composition, fresh Target input, inspection range and shared roster.
**High rigor** applies because several queued changes touch the same decision, reading and layout
paths and past scene-by-scene review missed adjacent behavior. The current milestone is shared
description reading, visible inspection return and allied compact-card levels under
[#2113](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2113). It consumes the accepted
[#2107](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2107) decision boundary.
The design preserves the accepted
[visual language](ui-design-language.md), [battle phase layout](battle-ui-composition.md#phase-focus-and-return),
[field and service behavior](field-and-shop.md) and [host/save return](host-ui.md).

[#2090](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2090) sets the current
product constraint for battle Main. At `277cb0dad2ce7dc7a6f34a7443426d422d48e25f`,
`BattleMenuLayout._Ready` mounts `CompactCommandList(Commands)` and
`BattleCommandMenu.Handle` steps through its vertical rows with Up/Down. The
`DiamondCommandButton` type supplies icon controls in both layouts. At `434c2ae9` (#2084)
the mount changes to a freestanding diamond, with different directional selection. That is
the layout in the inspected base and the one the player reported. Main-gate confirmed that
this change exceeded the requested scope. Restore the vertical list and its navigation under
#2090 while retaining #2079's focus, reader and resize work and #2080–#2082 behavior. The
remaining design and [decision presenter](#migration-and-independent-acceptance) consume the
restored vertical list.

The investigation covers representative battle decisions, read-only inspection, field Party and
preparation, service and dialogue routing, system Menu and replacement. It follows the action and
turn rules only where the UI needs their data. Gameplay adjudication, costs, AI, save format,
completion protocols, authored content and visual direction remain with their existing owners.
Static inspection and retained observations were sufficient to choose a design. No new runtime run
was needed. The remaining costs are implementation and review of the affected seams, with short
actual-instance checks on disposable inputs. No asset acquisition, large data run or campaign
replay is required.

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
directly, and Main Back sends the movement return. [Decision](../../game/src/Battles/BattleDecision.cs) ·
[Transition owner](../../game/src/Battles/BattleSessionView.Decision.cs) ·
[Battle composition](battle-ui-composition.md#battle-decision-projection-2107)

### Inspection and field pages

`BattleInspectionView` owns Cursor, direct Full, Options, Map, Members and MemberFull and their
focus. A provisional move first returns through the real movement delivery before inspection can
open. Direct Full returns to Cursor, MemberFull returns to the retained roster, Map/Members return
to Options, and Cursor Cancel restores Movement. Inspection rejects stale session/revision/actor
identity, with a specific refresh after a successful paused Save can publish music. In exploration,
`ExplorationSessionView` owns field-menu admission, the active service/wait and step settling.
`PartyStatusView` owns the roster and detail pane. `FieldActionView` owns the preparation page, row
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
scroll. Field and battle callers own query and capability. The field summary borrows the selected
row texture and clears the borrow on tree exit. `IdentityIcons` prepares decoded replacements before
disposal of the old set. `GameRoot.ReleaseSession` detaches and frees old view nodes before ending
replaced-session audio. A battle view may also leave/reenter the tree during field handoff without
being retired. [Layout](../../game/src/Battles/BattleMenuLayout.cs) ·
[Playback](../../game/src/Battles/BattleSceneView.cs) ·
[Roster](../../game/src/UI/MemberRosterView.cs) · [Field
summary](../../game/src/Exploration/PartyStatusView.cs) · [Identity
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

## Findings and limits

**Confirmed** findings below describe a source-demonstrable path or discrepancy at the named base.
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

### Non-activation buttons reach the board

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
their established disclosure. Selected spell effect copy remains with #2083.
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

### Parallel route eligibility can drift

P2. **Confirmed** independent predicates. **Inferred** drift outside the observed routes.

Battle action admission, pointer, navigation repeat and projection each check related wait/cover/stage
conditions. `BattleCommandMenu.Visible` and `Targeting` also determine how `GameRoot` decodes a
direction. Field independently routes mapped input, pointer, draw calls and clock. For example pointer
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

Roster texture replacement detaches the current row handle then disposes it. Field summary borrows that
handle and is rebound later in the same `Present`. Preparation hides it and retains row focus. A
projection failure partway through a multi-row update or use of such a borrow across a detach would
challenge this synchronous assumption. `MemberRosterView._ExitTree` drops row textures.
`BattleMenuLayout` retains its icon set across reparent, `BattleMapViewport` releases on
rebuild/predelete, and `BattleSceneView._ExitTree` clears its texture cache while retaining the mounted
overlay. Current handoff retains only the permissible board during return. Tree exit is therefore an
unsafe generic destroy signal. The accepted registered portraits observation exercised both callers and
reported live textures and zero disposed-texture errors. Portrait leaks and failed reparent remain Unknown. Keep each texture owner and name its valid borrow, remount and error boundary when changing
that caller. [Roster](../../game/src/UI/MemberRosterView.cs) ·
[Summary](../../game/src/Exploration/PartyStatusView.cs) · [Layout icon
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
- **Controls** render allowed actions, captions, reasons and focus from that value. They return
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
- **Spell description.** #2083 uses exact `SpellRef` and admitted effect facts from battle
  admission and the field recovery projection. Reuse the existing description query pattern
  and one localized formatter. Preserve field free no-effect and clamped deductions. A battle
  effect suppressed by Silence may still be selectable and paid under the current rule.
  Distinguish that result from a refusal such as insufficient MP. Field applies its own cost
  and availability rules. Where geometry and description need the same meaning, expose that
  operand once at admission. [Item query pattern](../../src/Sf2.Remake.Application/Runtime/ItemDescriptionQuery.cs) ·
  [Field recovery](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldRecovery.cs) ·
  [Silence](../../src/Sf2.Remake.Domain/Gameplay/Sf2/SilenceRules.cs)
- **Current-round queue.** #2065 can use a small read query beside `BattleAdvancer` and
  `BattleTurnFlow`. Factor the existing current-state dispatch classification for dead/unplaced
  and sleep/stun so a queue read cannot drift, and keep actual skip side effects in dispatch.
  Use the stored queue/cursor and stop at the real sentinel/end. Preserve repeated slots and
  disclosure. Label the projection as conditional on current state, since a prior action can
  alter the next eligible actor. No advance, next-round roll, AI prediction or saved projection.
  Host cover and Continue use the existing scope, and the readout remains outside focus.
  [Queue contract](battle-ui-composition.md#current-round-order-2065) ·
  [Advance and skip](../../src/Sf2.Remake.Application/Runtime/Battles/BattleAdvancer.cs) ·
  [Queue and cursor](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTurnFlow.cs)

## Migration and independent acceptance

These are integration boundaries for future dispatch. The principal owners below identify
overlap. Declare implementation paths against the then-current main. Each slice ships one
input, reading or query responsibility and removes its superseded path in the same PR. Before
acceptance, roll back a slice as a unit with its direct consumers. After dependent work lands,
revert consumers first or together. Use a single production path with disposable UI state.

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
   record with shared metrics. Battle Member continues to use the shared detail controls in its
   own inspection layout. [Current field behavior and verification](field-and-shop.md#reproduce-four-slot-capacity-and-accounting)
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

**Existing issue order.** Slices 2 and 3 consume the decision boundary from #2107. #2090 owns
the restored vertical Main and its acceptance. Slice 2 can use free reading directions on the
vertical page where the existing controls permit them. #2083 waits for coherent Back and the description reader from slices
1–2. It may follow before the full board painter migration if it already consumes the shared
admitted effect operands. Serialize any overlap in `BattleChoices` or `BattleMenuLayout`.
#2065 needs the stable phase/visibility boundary from slice 1 before mounting its strip. Its pure
queue query may be reviewed independently of the description reader and target footprint, with
cold Continue and read-vs-actual-next acceptance retained in #2065. The [Prepare command rows](#prepare-command-rows)
use the current reader and catalog. #2085 retains ownership of card level and
inspection wording/hits. Handle the Target input/feedback findings attached to it in slice 1 and
the reading/inspection findings in slice 2. This is one disposition for each reported behavior.

[#2068](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2068) artwork,
[#2070](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2070) dialogue portraits,
[#2073](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2073) story wait/Confirm,
and [#2062](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2062) field pickup
guidance keep their separate capability scopes. Their accepted entry points support independent delivery.
Serialize overlapping playback/field writers and call out any real shared input/lifetime need
when it arises. #1456 play can continue independently. Broad #1616 aesthetic choices and the
paused #1716 exit issue stay in their respective owners.

[Independent review](ui-scene-review.md#review-changed-ui-responsibilities) complements the
existing scene feature checks. The accepted [#2079 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2084#issuecomment-7087838808)
exercised final Target Back, legal/rejected gestures, wheel overflow, multiple recipients and
paused resize. Those checks did not inspect the intermediate projection sequence, unrelated
mouse buttons, mapped overflow or a full-party item's preview. The
[#2082 review evidence](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/2088) covered
retained focus and live textures on a real bounded path. It did not generalize that lifetime
to arbitrary detach or partial decode failure. Check one adjacent path that can contradict the
changed ownership claim. Reuse the affected native observations and the existing adapter build
for implementation. The present audit uses direct source and document checks, scope and link verification, and
`uv run sf2 verify plan --base origin/main --head HEAD` on the committed candidate.
