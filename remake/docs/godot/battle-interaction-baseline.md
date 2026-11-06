# Battle interaction baseline and inspection design

## Decision and scope

**Standard rigor; [#1622](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1622).**
This investigation supports the battle layout decisions in
[#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616) and a bounded future
inspection implementation. Cross-layer input, visibility and provisional movement need an explicit
agreement, but no new runtime, collection platform or engine implementation is needed here.

Restore access to battlefield inspection before adding tactical predictions. Free cursor inspection,
enemy/ally status, the whole-battle map and battle Quit/save are original features. The current
remake has movement, command/target cards and action playback, but those consumers do not supply
arbitrary battlefield inspection. Recommend a read-only inspection entry, public unit details and
on-demand overview, followed by the remaining original battlefield-menu functions.

This document owns the functional comparison and proposed inspection boundary. Existing
[presentation](presentation.md), [battle scenes](battle-scenes.md),
[capability status](../capability-status.md) and [overview proposal](modernization-design.md#read-only-battle-overview)
retain their implementation and observation ownership. The overview proposal's modern menu placement,
disclosure rules and accessibility treatment remain useful; the map capability itself is original.
No conclusion from an unmerged layout proposal is consumed here. Main-gate serializes navigation
and shared-owner reconciliation after review.

Acceptance of this investigation means a sourced decision-flow matrix, reachable-consumer audit,
minimum implementation contract, issue boundaries and explicit Unknowns. It does not mean these
proposals have shipped or that the original UI has been observed running. Full battle reverse
engineering, later campaign admission, pixel/timing parity, new art, save implementation and
future-turn simulation are outside this slice.

## Evidence and reproduction boundary

**Confirmed** below means either a named static source fact or an explicitly linked retained
observation, as qualified. **Inferred** marks a recommendation's expected benefit or reuse fit.
**Unknown** marks a boundary not established by those sources. Static reach through calls is not
natural runtime reach, exact display timing or proof that every data variant works.

The original baseline is the USA retail ROM, SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, and
`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.
The remake audit reads accepted repository commit `7fb1cecb432ff4032193df88f3d93d0e92f11364`.
Later code should be checked against these consumers rather than inheriting a completion claim.

The [official SEGA USA manual](https://www.sega.jp/genesismini2/assets/manual/pdf/US_Shining-Force-II.pdf),
printed pages 14–17 (PDF pages 9–10), independently describes combat and special options:
B enters a free cursor during a movable ally's turn; A opens full status, C a short status window;
B returns to the actor; A/C on empty ground opens Map, Member, Quit and Speed. Map locates both
forces across the battlefield; Quit confirms a save and exit. These are **Confirmed manual facts**,
separate from US-code entry/commit restrictions and from remake support.

Existing project evidence owners are [battle functions](../../../docs/research/battle/battle-functions.md#confirmed-player-control-state-machine),
[battle loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control),
[common menus](../../../docs/research/presentation/common-menus.md),
[turn/control](../../../docs/research/chapters/grans-opening/map3-battle01-turn-control.md),
the [control contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md),
and [navigation contract](../../../docs/design/contracts/battle/battlefield-navigation.md).
The focused pinned-source references used below are:

| Reference | Source file and inspected symbols |
| --- | --- |
| [S1][s1] | `battlefunctions/executeindividualturn.asm`: `ExecuteIndividualTurn`, `@ProcessPlayerInput`, `@CheckBattleaction_CastEgress` |
| [S2][s2] | `battlefunctions/battlefunctions_2.asm`: `ProcessBattleEntityControlPlayerInput`, its item chunks, `BattlefieldMenu` |
| [S3][s3] | `battlefunctions/battlefunctions_0.asm`: `ControlBattleEntity`, `ControlCursorEntity`, `ControlCursorEntity_ChooseTarget` |
| [S4][s4] | `common/menus/ministatuswindow.asm`: `OpenBattlefieldMiniStatusWindow`, `BuildMiniStatusWindow` |
| [S5][s5] | `common/menus/memberscreen.asm`: `BuildMemberScreen`, `LoadMemberScreenWindowLayouts`, `BuildMemberStatusWindow` |
| [S6][s6] | `common/menus/landeffectwindow.asm`: `BuildLandEffectWindow`; `battlefield/battlefieldengine.asm:GetLandEffectSetting` ([lookup][s6lookup]) |
| [S7][s7] | `common/menus/minimapscreen.asm`: `BuildMinimapScreen`, `sub_129E8`, `sub_12CB0` |
| [S8][s8] | `common/menus/battlefieldsettingswindow.asm`: `BuildBattlefieldSettingsScreen`, `SetBattlefieldSettings`; [message consumers][s8messages]: `bsc10_displayMessage`, `bsc11_displayMessageWithNoWait` |

Paths in this table are relative to the pinned checkout's `disasm/code/`, with battle function,
battlefield and battlescene files beneath `gameflow/battle/`. Reproduce the static reading in a
registered, read-only upstream checkout; for example, from that checkout:

```powershell
git rev-parse HEAD
git status --short
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm
git show c834c652b6862bc5679fd7f69a38a7093206efc6:disasm/code/common/menus/memberscreen.asm
```

Apply the same `git show <pin>:disasm/code/<path>` form to the other named files. The existing
`tests/fixtures/h2/battle-functions-static-v1.json` and `common-menus-static-v1.json` retain
the inventories; their owning commands are `uv run sf2 h2 battle-functions` and
`uv run sf2 h2 common-menus`. This documentation audit reads the pinned source directly; it does
not claim a new H2/H3 run. No ROM extraction, source modification or original runtime was needed.
Local source identity, commands and check results belong in ignored `local/issue1622/HANDOFF.md`;
no private absolute path, binary, game artwork, capture or source listing is published.

The current-consumer audit follows [host input](../../game/src/GameRoot.cs),
[battle input/presentation](../../game/src/Battles/BattleSessionView.cs),
[command pages](../../game/src/Battles/BattleCommandMenu.cs),
[command dispatch](../../src/Sf2.Remake.Application/Runtime/Battles/BattleCommandDispatcher.cs),
[choice queries](../../src/Sf2.Remake.Application/Runtime/Battles/BattleChoices.cs),
[map projection](../../game/src/Battles/BattlePresentation.cs),
[map/camera consumer](../../game/src/Battles/BattleMapViewport.cs),
[session queries](../../src/Sf2.Remake.Application/Runtime/GameSession.cs) and the
[field-only party query](../../src/Sf2.Remake.Application/Runtime/Exploration/PartyDetailsQuery.cs).
These are executable routes inspected at the accepted code baseline, not claims based on class names.

## One player decision, from entry through return

All original entries below are **Confirmed static** unless identified as manual evidence. Current
consumer statements are **Confirmed code structure**, with retained native observations linked
through their owners; this audit did not run a new native session. “Missing” means the inspected
normal route has no such functional entry, even if a diagnostic read exposes similar data.

| Player step / original capability | Current actual route and available information | Gap and disposition |
| --- | --- | --- |
| Turn begins: S1 focuses the actor, waits for cursor/view settlement, opens mini status and land effect, builds movement range; sleep/stun and AI-control branches restrict player action. | `GameRoot` routes to the mounted `BattleSessionView`; `BattleCommandMenu.Refresh` presents the acting card in Movement. `QueryBattleMovementRange` supplies legal stops. [B8/E1 acceptance](presentation.md#battle-movement-range-agreement) owns the retained witnesses. | Actor and movement feedback exist. Land-effect information is missing. Exact original selection animation is **Unknown**; [#1617](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1617) owns its correction. |
| Observe the battlefield independently of movement: S1–S3 enter `BattlefieldMenu` after movement B cancellation; its cursor owns view focus. | `BattleSessionView.HandleAction` routes directions to `Move` in Movement. Cancel submits `Cancel`, whose dispatcher builds a return path and remains in Movement. There is no inspection branch/action in this route. | **Functional missing entry**, not merely an undiscovered key. Do not use movement to pan for inspection. Recommended first scope below. |
| Inspect any living ally/enemy on a tile, including one outside attack range: S2 checks current HP and coordinates; A invokes full status, C invokes mini status/land effect for another actor. | `BattleCommandMenu.Refresh` supplies the target card only on its Target page; the selected action controls candidate enumeration. `BattleStatusCard` displays name, role, HP/MP, low/dead warning and silence. | **Functional gap** in arbitrary browsing and full enemy/ally details. Existing cards are reusable rendering, not inspection evidence. |
| Empty-ground options: S2 calls `MENU_BATTLEFIELD`; return values dispatch Member, Map, Speed and Quit. | `BattleCommandMenu` has Main/Magic/Items/Target/Commit pages; Main is Attack/Magic/Item/Stay. Host Menu is a separate modal. Neither supplies the original empty-ground menu. | **Functional gap**. Keep system Menu distinct from battle options; layout belongs to #1616. |
| Map: S7 builds an overview of the battlefield area and entity indicators, then returns to the options caller. | `BattleMapViewport` renders the ordinary board at 96px cells; `Frame` follows movement/selection/target/story. No overview page exists. The [overview design](modernization-design.md#read-only-battle-overview) is proposed. | **Functional gap**, with an existing design to reuse. Modern placement/legend/accessibility can change; map availability is original. |
| Member: S2 rebuilds the battle-force list, opens its summary list and then full member screens. | `QueryPartyDetails` returns null outside Exploration; `ExplorationSessionView.HandleAction` opens `PartyStatusView` only at field input without a battle mount. Battle Equipment instead begins equipment selection. | **Functional gap** in battle roster inspection. The field details UI exists but its current query and entry cannot serve the live battle. |
| Move, reconsider, return to actor: S2 accepts A/C at a valid stop; diamond cancel restores logical origin/rebuilds movement. Movement B constructs and delivers a cancel-move string before free inspection. | `BattleCommandDispatcher` keeps the authoritative actor position separate from `Selection.Preview`; `BattleMovementContinuation` delivers preview/return movement. Cancel at the main diamond rolls back the preview. | Core undo exists; transition to inspection does not. Preserve the distinction between undoing a move and closing a read-only panel. No source promise to retain a tentative destination through B. |
| Choose Attack/Magic/Item/Search-or-Stay: S2 chooses Stay or Search from faced chest availability; it chooses initial Attack/Stay focus from attack targets. | `BattleCommandMenu` defaults to its Main Attack index and groups typed `QueryBattleChoices` actions into four commands. No battle Search command/page appears. | Basic actions exist; initial focus is a **behavior difference**, and contextual battle Search is a **functional gap** outside the first inspection scope. Field Search does not establish battle Search. |
| Choose spell/level or item, then target; cancel to the appropriate parent: S2 checks spell availability/MP/range and returns canceled targets to spell/item/action selection. | `BattleChoices.Query`, `BattleCommandMenu.Handle/Select/PickTarget` and `ReturnToBattleActions` drive lists, availability/reasons and target selection. Cancel restores the relevant list/main page; main Cancel returns to movement. | Existing core route. Menus flatten source spell/item structures into modern lists. Off-range attempted targets may be displayed but cannot commit; do not confuse these with free inspection. [Localized consumer boundary](presentation.md#localized-battle-command-and-equipment-display). |
| Give/Equip/Drop: S2's item submenu includes these operations, with separate curse/confirmation and turn-consumption branches. | Battle Equipment/Give/Discard shortcuts dispatch to the existing typed selections; Give/Discard have their own result consumers and equipment returns to the action page. Main help exposes the shortcuts rather than an original item diamond. | **Discoverability / navigation difference**, not absent transaction engines. Preserve each owning action's commit semantics; inspection must never invoke them. |
| Commit → scene → result: S1 handles Stay and retreat separately from ordinary battlescenes. | `Confirm` reaches action preparation only from a complete selection; scene continuations deliver resource effects/messages/rewards/deaths and then control/outcome. `BattleSceneView` uses live cards. | Existing bounded [battle-scene consumers](battle-scenes.md#message-and-outcome-presentation). Whole-game animation/pixel parity remains **Unknown**. Weapon layering belongs to [#1620](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1620). |
| Next actor/round or exploration return: the [battle-loop owner](../../../docs/research/battle/battle-loop.md#top-level-battle-control) orders deaths, faction checks, after-turn and next entry. | `BattleAdvancer` and continuations progress automatically; `LeaveBattle` and the field presenter retire windows and resume field control. [Retained return boundary](presentation.md#battle-messages-return-and-party-status). | Existing finite behavior; it is not proof of all encounters. Inspection must be unavailable across these transitions. |
| Speed/text options: S8 stores two old values; directions edit message speed 0–3 and message suppression; B restores both, A/C keeps changes. | [Settings](presentation.md#basic-preferences-agreement) offers instant/adjustable text, characters/sec, swapped controls and reduced flash at title/safe field only. `SettingsHost.CanOpenSettings` excludes battle. There is no battle-message suppression preference in `InputSettings`. | **Functional gap** in in-battle access and selective suppression; modern text reveal exists with a different scale. This is separate from [#1619](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1619)'s temporary 1.5× time policy. |
| Quit/save and later resume: S2 rejects suspend in Battle00; otherwise confirmation copies elapsed seconds, sets flag88, saves the selected slot and normally transfers to `WitchSuspend`. The [loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control) has a distinct suspended entry. | Host Menu can pause battle and offer unsaved Quit; current save availability/capture is the bounded field `OpeningSave` path. This is not a resumable battle save. | **Functional persistence gap**, already owned by [#1618](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1618). Slot UX/new safe boundaries are modern design, not evidence that the original lacked battle saving. |

The battle direction route consumes fresh input edges: `GameInput.Resolve` rejects keyboard Echo
and repeated pad deflections, and `BattleSessionView` has no held-direction loop. The field's
`ExplorationSessionView.Clock` is a different consumer. **Confirmed code gap**; [#1615](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1615)
owns hold/release/mode isolation. Gold sprite modulation and preview underlines in
`BattleMapViewport.Present` are **Confirmed current code**, not proof of original presentation;
#1617 owns that fidelity boundary. Do not duplicate either issue in an inspection implementation.

Message suppression is selective. **Confirmed static:** S8's `bsc10_displayMessage` checks
`NO_BATTLE_MESSAGES_TOGGLE` before `DisplayText`; `bsc11_displayMessageWithNoWait` does not.
The manual describes retaining level/EXP/special-item messages when ordinary hit/coin text is off.
This does not authorize suppressing all rewards, status or progression acknowledgements in the
remake. Which current message tokens may be hidden needs its own finite mapping and completion
contract; changing reveal speed alone cannot implement that behavior.

## What status and terrain inspection may disclose

| Information | Original static boundary | Remake reuse and limit |
| --- | --- | --- |
| Short card | S4 reads current/max HP/MP; ally header includes name/class/level, enemy header name. | `BattleStatusCard` already reads live HP/MP and readable names, but not ally class/level. Extend through an explicit public-details projection; do not infer identity from labels. |
| Full attributes | S5 reads current HP/MP, ATT/DEF/AGI/MOV and status effects; allies have class/LV/EXP. Enemy LV/EXP render N/A rather than an inferred value. | `BattleActorState` has runtime resources/progress/stats; content carries class/loadout information. A battle query must read the live actor, not reinitialize from a deployment or use field carried-party values. Preserve missing/unsupported values visibly. |
| Full status effects | S5 explicitly writes curse, poison, muddle, silence, stun, sleep and stat-modifier indicators. | Current small card's silence/low/dead cues are not a full status screen. Decode only admitted status semantics and retain duration uncertainty. |
| Spells/items | S5 reads spell slots and held items, with equipped annotation; enemy sheets use the same relevant readers. | Reuse admitted names, runtime loadout, spell and equipment mechanisms. Inspection displays them without opening Prepare, equipping, giving, dropping or spending MP. Unknown names are not permission to expose raw private diagnostics. |
| Portrait/records/gold | S5 omits enemy kills/defeats; portrait absence skips the portrait/gold branch. | Reuse admitted field detail widgets where suitable, but do not fabricate enemy records or require missing private portrait art to browse combat stats. |
| Land effect | S6 obtains the inspected/moving combatant's move type and terrain, displays setting ×15 percent (0/15/30). S2 opens land effect with another unit's short card; S3 updates its movement-time input. | Semantic terrain exists in `BattleDefinition.Terrain`, but no battle UI reads it as land-effect text. `Sf2PhysicalAction.LandMultiplier` uses protection and mover (Flying gets no protection); `BattleTerrainRules` owns movement cost separately. A small public query must reuse these semantics, not infer percentage from tile artwork or treat it as the damage-reduction percentage. |
| Empty-tile terrain | S2's empty-ground branch opens options; it does not open a land-effect window for every arbitrary cursor tile. | Showing a tile's terrain name is a proposed modern addition. A prospective land-effect value must name the evaluated actor/mover and location; it cannot be advertised as an actor-independent property. |
| Hidden/future information | S2's unit cursor tests living placed combatants; S7's indicator code consumes entity positions and mapsprite categories. Neither is evidence for showing future AI plans or reserve spawns. | Raw `Actors`, `Deployments`, `TurnOrder`, diagnostic roster, activation and AI memory are not a public inspection API. Apply the [existing disclosure boundary](modernization-design.md#read-only-battle-overview) before producing any list or marker. |

S5 also checks `UNKNOWN_STAT_VALUE_THRESHOLD` before writing HP/MP and ATT/DEF/AGI/MOV numbers.
The presence of an internal value does not itself grant player disclosure. Preserve source-like
unavailable values in a fidelity profile, and name any intentional change to that policy rather
than printing raw diagnostic fields. [Status-card code](../../game/src/UI/BattleStatusCard.cs),
[terrain rules](../../src/Sf2.Remake.Domain/Battles/Rules/BattleTerrainRules.cs) and
[physical land multiplier](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs)
are the specific existing consumers behind the reuse conclusions above.

**Confirmed static camera ownership:** S3 assigns `VIEW_TARGET_ENTITY = ENTITY_CURSOR` during
free control, saves chosen tile coordinates, then idles/hides the cursor and clears that target.
S1 re-enters actor control on return. This proves a separate inspection subject, not the original
scroll easing, precise edge clamps or cadence. Those remain **Unknown**.

## Minimum implementation contract proposed for review

This is a recommendation, not a new capability claim. **Inferred:** a public read-only projection
plus disposable inspection focus is sufficient; the inspected code supplies the underlying live
state. No second battle, AI simulation, saved inspection state or generalized knowledge platform
is justified for the settled opening scope.

### Query and session boundary

Add one battle-inspection query alongside `GameSession.QueryBattleChoices` and
`QueryBattleMovementRange`. Its minimum result is availability/reason, session/revision identity,
active actor, battle dimensions/region, disclosed unit IDs/positions, requested unit public details
and actor-qualified terrain information. It reads `Current`; it never calls Start, Submit, Prepare,
AdvanceSimulation or any RNG service. `QueryBattleChoices` remains the authority for actionable
offers; it cannot supply arbitrary inspection targets. `QueryPartyDetails` stays field-owned until
a deliberately shared scalar detail projection is extracted; do not make it initialize a battle.

Initially admit only a live player selection in settled Movement, without movement delivery,
scene/retreat/Give/Discard/round/appearance/program/outcome or failure. Reject hidden/unplaced units
and stale IDs explicitly. Reuse ordinary-board visibility rules at that boundary, including staged
entity visibility; do not reveal a positioned actor before its appearance. Existing board projection
and appearance filtering are the first owners to reuse, rather than a second reveal-history store.

Keep only cursor tile, selected public unit, panel/focus and return context in the adapter. Bind
that context to the current session/revision/actor; retire it on replacement or a changed boundary.
During inspection no gameplay commands, automatic turns, movement acknowledgements, resource writes
or reward acknowledgements may pass through. Opening/browsing/closing preserves the exact selection,
preview destination/path, round/cursor/queue, all actor/party/story state and both RNG seeds. Revision
and observation sequence do not advance merely for reading. Animation of a cursor/camera is display
state, not a logical battle opportunity.

### Entry, return and provisional movement

The source and proposed modern entry have deliberately different cancellation contracts:

| Transition | Required behavior |
| --- | --- |
| Original B during movement (S1–S3) | Retrace/cancel the move before `BattlefieldMenu`; on B return, restart the same actor's control. Original B does not promise to preserve the tentative destination. |
| Proposed explicit Inspect from settled Movement | Use a separate, discoverable mapped entry, with keyboard/pad and a labeled control. Preserve the existing Cancel undo operation. This non-destructive entry is an intentional modernization of access, not a claim that the source B preserves preview. Initial cursor/focus is the displayed actor at its preview destination. |
| Inspect another unit or empty tile | Directions move only the inspection cursor over admitted battle cells. Detail/short-card actions and empty-ground options remain inspection-owned. They never issue `Move`, `SelectTarget` or `Confirm` to the battle session. |
| Close details / Map / Member | Return to the same inspection cursor and panel focus. Back must first close the child, then close inspection on a fresh event. |
| Return to active unit / close inspection | Return camera subject to the same actor and preserved preview; retire the inspection cursor. The actor's authoritative position and provisional destination remain distinct. Require release before the next gameplay Confirm/Cancel/direction can be consumed. |
| Future inspection from ActionChoice/TargetChoice | Defer until parent page, spell/item candidate, target and preview can be restored as one context. The initial scope must not silently discard these by converting entry into Cancel. |

The initial proposed entry need not consume an existing Attack/Equipment/Target shortcut. Its exact
mapped control and placement are serialized with #1615 input and #1616 layout work. If main-gate
instead selects exact original B entry, reuse the existing return-path delivery first, explicitly
label that it cancels movement, and enter inspection only after that delivery completes. Do not
claim the non-destructive contract for that alternative. This is a bounded input choice, not a
second simulation or an authorization to change cancellation semantics silently.

### Camera and overview

`BattleMapViewport.Present/Frame` currently chooses focus from movement, target candidate, preview
or story; `_Process` reapplies it. Add a temporary inspection subject at that owner so ordinary
Frame cannot immediately replace the cursor. Reuse `WorldViewportLayout.Focus/Clamp` and the
battle's admitted `SourceMapRegion` offset. Keep 96px cells, clipping and legal area boundaries;
do not pan into neighboring packed rooms, storage, hidden entities or an entire source-world map.
Returning removes only the override and resumes the saved actor/preview subject. The modern
dead zone, easing and framing remain modern policy, not original camera evidence.

The on-demand Map reuses the [overview proposal](modernization-design.md#read-only-battle-overview):
whole admitted battle area, disclosed allies/enemies, active unit, clearly distinguished committed
position/preview and the clipped camera rectangle, with shapes/labels and a navigable known-unit
list. It reads the camera without moving it; Back returns to the same inspection cursor. No map
click can move a unit or commit a target. Free battlefield cursor and overview are complementary;
one is not acceptance for the other. A permanent minimap, click-to-center, threat overlays and
undisclosed objectives remain outside this first scope.

### Smallest useful acceptance after implementation

Use one existing ordinary player-turn boundary and one existing visibility/appearance case. Start
with nearby ally/enemy, an off-camera disclosed enemy and empty ground; expand only if a failure
needs another input. No new full-route replay, screenshots, bulk capture or comparison platform.

| Check | Required result |
| --- | --- |
| Query purity and disclosure | Small engine behavior tests show repeated live reads preserve state/RNG, reject busy/stale/hidden units and do not reveal pending appearances. Use the real query, not tests of a probe/report. |
| Real entry and return | Actual adapter input/state/projection/errors show movement preview → Inspect → distant unit → child details/Map → Back → same preview/actor. Compare selection, path, session/revision/sequence, seeds, resources and queue before/after. Undo remains a separate working action. |
| Information | Ally/enemy short/full details read live HP/MP, admitted stats/status/equipment; unsupported fields remain explicit. Test two mover/protection combinations so land effect cannot be a hardcoded tile percentage. |
| Camera | Read actual cursor/focus, region origin and camera bounds at opposing board edges and return. The board scale never changes when panels open. Include a nonzero source-region origin and small-board centering where existing inputs permit. |
| Input isolation | Keyboard/pad and mapped controls, pointer controls where offered, hold/release, child Back, focus loss and host Menu pause never move/commit on dismissal or carry a held direction into gameplay. Coordinate with #1615 rather than duplicating its repeat clock. |
| Availability and errors | Reject entry during movement delivery, AI, scene/result/death, appearance/program, retreat/outcome and fault. A session replacement closes inspection and cannot restore a stale actor or pending command. |

Use affected engine behavior tests if a public query/rule is added, adapter compilation for its
consumer, the clean committed planner and direct bounded native observations. Reuse the owned
Godot project/instance and registered inputs; a concrete startup failure must justify any restart.
This table is future acceptance, not a list of checks passed by this documentation change.

## Sequencing and remaining uncertainty

| Order / owner | Bounded outcome and dependency |
| --- | --- |
| First: this baseline → #1616 | Review original capabilities and the read-only contract before allocating layout space. Layout acceptance cannot turn a missing consumer into a completed feature. Shared documentation navigation is main-gate-owned. |
| Foundation: #1615 and #1617 | Direction hold/mode isolation and evidence-based selection/path feedback retain their issues. Inspection consumes the accepted input contract and respects visual ownership; neither issue must grow into full inspection. |
| Recommended new implementation slice, not created here | Live public battle query + settled-Movement Inspect + unit short/full details + on-demand Map, with original/modern entry choice fixed in dispatch. One coherent inspection outcome; serialize `GameSession`, battle views, input and UI ownership. Reuse #1543's overview rather than implement a second map. |
| Following bounded original-options slice | Battle Member roster reuses the query/details; source-like empty-ground menu connects available functions. Original Speed access/selective message suppression needs a separate explicit consumer/persistence contract. Do not expose dead rows that pretend to work. |
| #1618 | Battle Quit/save/Continue, stable commit and recovery. Inspection/options provide an entry only after its availability contract exists; no private save mutation or schema change here. |
| #1619 / #1620 | Temporary Shift 1.5× presentation/time policy and weapon frame/layer correction retain their own scopes and evidence. Neither is required to establish that original inspection exists. |
| Deferred modernization | Action-order bar, permanent minimap, tactical predictions and enemy threat/intent. No predictive run or simulation is authorized by this design. |

An action-order bar must distinguish the already constructed current-round queue, an explicitly
qualified prediction and unknown future changes. `EngineBattleState.TurnOrder` exposes the retained
queue to code, not an approved player disclosure. Original order construction uses random alteration
and can include extra entries; simple AGI sorting is not the reliable order. Future activation,
status/death and round generation can change what happens next. No bar is part of this delivery.

**Unknown:** exact original free-cursor edge/camera cadence, natural key timing, visual composition,
all minimap entity-category anomalies, and original idle/VInt effects on RNG during browsing.
The source waits for VInts, so absence of direct action/RNG calls in `BattlefieldMenu` is not proof
that every original RNG value stays frozen. Read-only remake inspection's no-RNG rule is its own
explicit contract. Hardware timing is unnecessary for the proposed access/information boundary.

**Unknown:** native reach and readability of a future inspection UI, exact final control placement,
and human acceptance of its layout. Current code absence is established statically; no new runtime
failure or success was manufactured. If exact original B-return physical motion or a specific
boundary becomes blocking, first state that question and reuse the existing player-ready seam for
one B → cursor → return observation. Do not convert this Unknown list into an automatic acquisition
queue. Documentation review here uses direct source/link/scope/private-boundary checks, a clean
committed planner and actual CI disposition only; no SDK, normal/full suite or Godot run is required.

[s1]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/executeindividualturn.asm
[s2]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_2.asm
[s3]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefunctions/battlefunctions_0.asm
[s4]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/ministatuswindow.asm
[s5]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/memberscreen.asm
[s6]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/landeffectwindow.asm
[s6lookup]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlefield/battlefieldengine.asm
[s7]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/minimapscreen.asm
[s8]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/menus/battlefieldsettingswindow.asm
[s8messages]: https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/gameflow/battle/battlescenes/battlesceneengine_0.asm
