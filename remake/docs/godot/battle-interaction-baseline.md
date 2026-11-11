# Battle interaction baseline and inspection design

## Decision and scope

**High rigor for the production UI migration (#1686).** Decision, inspection, commit and return
share input and presentation boundaries; a wrong gesture can spend resources or acknowledge a
different phase. The agreed outcome is the accepted F4/F8/F9 behavior in the pre-voyage demo,
using existing gameplay, transaction and continuation owners. Acceptance combines actual input,
state, geometry and errors; a preview or passing build alone is insufficient. No new campaign,
rule, asset acquisition, persistence format or inspection platform is in scope.
This is the maintained battle feature entry: original capability or accepted modernization → remake
decision → player operation and return → Domain/Application/Content/Godot consumer → ordinary
connected admission → acceptance and remaining work. Cross-layer input, visibility, provisional
movement and recovery require explicit boundaries. Reuse the existing detailed owners; no new
tracking framework, duplicate capability ledger or runtime collection is needed.

Free cursor inspection, enemy/ally status, the whole-battle map and battle Quit/save are original
features. The remake connects movement, bounded actions, playback, outcomes and battle Save/Continue;
it now connects arbitrary disclosed-unit inspection through #1633. This restores original access
within the admitted encounters before tactical predictions. The inspection contract uses original-style Movement B/Cancel: finish the
existing movement undo, then enter read-only inspection, public unit details and on-demand Map.
Its bounded implementation is [#1633](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1633),
dependent on #1615 input isolation and #1617 selection/path feedback. A separate preview-preserving
Inspect shortcut is **Deferred**.

This document owns functional traceability and the accepted inspection boundary from
[#1622](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1622). Existing
[presentation](presentation.md), [battle scenes](battle-scenes.md),
[capability status](../capability-status.md) and [overview proposal](modernization-design.md#read-only-battle-overview)
retain their implementation and observation ownership. The overview proposal's modern menu placement,
disclosure rules and accessibility treatment remain useful; the map capability itself is original.
The tracked [shared visual language](ui-design-language.md) and [scene review](ui-scene-review.md#8-battle-one-complete-decision-then-its-consequences)
are the accepted [#1616](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616) design.
The [production coverage below](#production-f4f8f9-contract) records deployed behavior separately
from original fidelity and human visual approval. Main-gate serializes shared ownership.

The trace covers battle entry → observation → movement → action/item → target/confirmation →
presentation/settlement → next actor or victory/defeat/retreat → Save/Continue. Acceptance here is a
code-calibrated reader route with source, decision, actual admission, bounded evidence and a clear
disposition for every remaining gap. Feature implementation, refactoring, a new visual decision,
new original investigation, later campaign admission and runtime/test execution are outside #1642.

Resource acceptance is a separate axis from the functional rows below: **function/return** records
implemented gameplay and caller flow; **resource/presentation** records actual ordinary consumption
or a named gap; **human experience** remains Unknown until explicit user acceptance. The
[#1666 ordinary resource observation](presentation.md#ordinary-menu-resource-acceptance-1666) does not
close all list/options art or claim human approval; #1667/#1668 have their bounded resource acceptance below. Exploration/church/shop details stay
with the [presentation asset owner](../content/presentation-assets.md#ordinary-menu-publication-and-coverage-1666).

### Reading and maintenance

Start with the [player-flow matrix](#one-player-decision-from-entry-through-return), use the
[layer responsibility map](#responsibility-and-admission-route) to locate consumers, and follow
[sequencing and gaps](#sequencing-and-remaining-uncertainty) for the remaining scope. The provenance
below supports those routes; it need not be read first.

“Connected” below means the admitted private-local campaign, currently Battles01–07; it does not mean
every original encounter. “Standalone authored” means an independently admitted example, not ordinary
campaign integration. **Confirmed code structure** establishes a reachable consumer or guard;
engineering observations, controlled preconditions, natural reach and human experience retain their
own evidence boundaries. A test, menu label, class or closed Issue alone establishes none of them.

When a battle feature merges, update its row here and its detailed owner together: decision/return,
real consumer and Content guard, connected reach, acceptance limits and residual Issue. Keep source
provenance and existing anchors. Link evidence instead of copying reports or adding another complete
checklist. Linked follow-up Issues identify uncovered scope; a filed ticket is neither implementation
authorization nor evidence that the feature is complete.

## Production F4/F8/F9 contract

The production presenters consume the accepted #1683/#1684 layout and #1685 compact-list API.
They do not instantiate preview heroes, maps, prices or staged results. The following is **Confirmed
bounded remake behavior** through `engine_demo_battle_ui_observation.gd`; original source claims
retain the separate provenance below. The [scene owner](battle-scenes.md#production-playback-and-result-layout)
owns playback and result placement.

| Accepted scene / behavior | Production consumer and actual coverage |
| --- | --- |
| F4 Movement and layout A | Existing weighted movement/undo and steady actor brackets remain. Cyan legal-stop tiles and gold action reach use a dark outer edge, bright inner rim and 1.2s pulse (53–68% fill); Reduced Flash holds 60.5%. Compact acting HP/MP head the right column. Real command/child offers retain aligned origins, mapped hints and explicit unavailable reasons. English/Chinese, remapped/pad input and 1080p/1440p geometry are observed. Class/level/status remain in full details rather than the compact decision header. |
| F4 real reach and effect cursor | `BattleCommandMenu.PreviewAction` supplies the highlighted admitted offer to `BattleActionRangeView`. Minimum/maximum reach comes from the current physical/spell/item offer at the provisional stop. Effect radius comes from that spell's content; AURA4 marks actual living placed allies. Radius zero remains one cell. This is drawing geometry, never a second action/target authority. BLAZE2 and AURA4 exercise changed multi-target content through the actual production session. |
| F4 target selection and cancellation | Up/Left selects previous and Down/Right/Target next, wrapping only enabled live targets. Board hover and a complete stable press/release select through that same offer. Empty/wrong-side/out-of-range/defeated cells clear UI focus; both pointer and keyboard Confirm require that current enabled candidate. They cannot confirm the older session target. Relayout/camera movement cancels an incomplete gesture. Back returns to the exact spell/item row or command without payment or RNG. |
| F4 auxiliary pages | Existing Equip/Give/Drop use their dedicated reading/confirmation caller and advertised shortcuts. One auxiliary page replaces commands; real item identities and source transactions remain authoritative. Battle01 cancellation observes resource/seed equality. Existing [connected transaction admission](../content/profiles-and-trust.md#connected-battle-give-admission), Search and actual transaction evidence retain their own scopes; this migration grants no additional transaction or mouse capability. |
| F8 hover and details | Pointer or directional focus shows the same disclosed live unit, mover-qualified land effect, equal-width HP/MP gauges and location. Empty ground explicitly evaluates a regular ground unit and labels impassability. A/Confirm or a complete unit click opens shared read-only details. Back restores the inspected cell for every unit, including the actor. This accepted modernization supersedes the earlier current-actor direct-return/mini-status UI policy; the original S2 distinction below remains evidence. |
| F8 free camera | Exposed 32px edge band, 180ms dwell and depth-scaled 2–6 cells/s; diagonal speed is bounded. The clear top/bottom outer rim also works. Small axes center, bounds clamp, and hit testing follows the actual moving board. Interior motion, buttons, mapped navigation, modal entry, pointer exit and focus loss retire the gesture; return needs fresh motion. Inspection camera state is disposable and cannot alter battle state. |
| F8 options and Map | Original Member/Map/Speed/Quit art accompanies the shared labeled vertical rows. Whole-row pointer gestures and Up/Down navigate. Details restore the roster row; Map Back restores Options and a map-unit click locates that disclosed unit at the cursor without acting. Existing Speed draft and Quit/save callers retain their eligibility and release barriers. Connected Battle02 verifies member return, nonzero-region Map, Speed/Quit Back and real EGRESS return. Explicit eligible Quit may publish legitimate music metadata; gameplay/resources/RNG remain unchanged. |
| F9 actual results and return | Production damage, death, reward/growth and outcome readers consume real session continuations. The authored victory/defeat packages enter from their actual field interaction and return to usable field movement. Source Battle02 EGRESS enters through its real field/before/load/start path, cancels freely, pays once and returns usable Map3. These are actual gameplay results, distinct from the preview's staged outcomes. |

Movement continues to use its advertised directional input. Inspection and target selection add board
pointer gestures; they do not synthesize click-to-walk gameplay commands. Source Battles01–07 share
these presenters; their [campaign admission and prior route evidence](../application/pre-voyage-campaign.md)
remain authoritative. This slice does not replay the campaign or certify every natural species/action
combination. Human foreground composition, physical devices/DPI and original pixels/timing remain
**Unknown**. Some native exits retain ObjectDB leak warnings; consumer success is not clean-shutdown
acceptance. No player saves or screenshots are involved.

### Direct production reproduction

Reuse the selected ordinary private world/scenes and locked tool environment from the
[asset owner](../content/presentation-assets.md#final-publication-and-ordinary-selection). Load the
owning checkout's ignored configuration in the same launching process. After
`uv run sf2 verify adapter`, build Debug with the selected `DOTNET_BIN` and protected shared CLI home:
`& $env:DOTNET_BIN build remake/game/Sf2.Remake.Godot.csproj --configuration Debug --no-restore`.
Use a fresh ignored output directory, worktree-local APPDATA/LOCALAPPDATA/TEMP, and a unique
`SF2_SAVE_NAMESPACE` under the explicit external validation `SF2_SAVE_ROOT`.

| `SF2_DEMO_BATTLE_CASE` | Initial selection, before host startup |
| --- | --- |
| `battle` | `SF2_COMPACT_START` points to UTF-8 JSON: `{"start":{"battle":"battle-1","actors":[{"actor":"ally-0","hp":6,"position":{"x":8,"y":14}},{"actor":"ally-1","position":{"x":9,"y":14}},{"actor":"ally-2","hp":0},{"actor":"enemy-0","hp":1,"position":{"x":8,"y":13}}]}}`. This is an explicit consumer start, not natural reach or a connected outcome fixture. Subsequent movement, attack, HEAL and inspection use actual input only. |
| `retreat` | Copy the tracked `remake/reference/inputs/engine-battle02-egress-start.json` to fresh ignored input, add `start.partyInput = "remake/reference/inputs/battle02-outcome-party.json"`, and select that copy through `SF2_COMPACT_START`. No post-start mutation. This enters actual Battle02, preserves a real joined roster and permits its actual return. |
| `area` / `aura` | Pass `--authored-package` with `remake/content/authored/attack-spells.json` / `aura-healing.json`. The observer selects the real list's BLAZE2 / AURA4, cancels once, then consumes all actual reactions, payment and next control. Long-name stress uses a prelaunch authored copy with renamed identifiers and all their references; no private assets are changed. |
| `victory` / `defeat` | Pass `--authored-package` with `remake/content/authored/progression-outcome-victory.json` / `progression-outcome-defeat.json`. Real initial interaction enters the battle; ordinary attack/Stay produces its outcome, reading and field return. These public authored outcomes do not replace private source outcome evidence. |

Set `SF2_VIEWPORT_OUTPUT` to a fresh absolute JSON destination and `SF2_INSPECTION_ROUTE` to
`keyboard`, `pad`, or `remapped` with an actual matching input-settings file. Run the existing
`GODOT_BIN` in a hidden bounded child with arguments:

```text
--headless --path <checkout>/remake/game --audio-driver Dummy --max-fps 120
--resolution 1920x1080 --script res://probes/engine_demo_battle_ui_observation.gd
-- --input-settings <settings.json> [--authored-package <package.json>]
```

Each case has a 50s observer / 55s owned-process deadline and writes a compact final report, not a
frame stream. Read `passed`, `failures`, `unavailable`, geometry and native stdout/stderr together;
a script/startup error is a failure even when no assertion was reached. Preserve earlier outputs.
Current owned-run recipes/results and completed failures are retained under ignored `local/issue1686`;
the Issue/PR freezes the exact candidate and actual CI. No aggregate/research/emulator suite is needed
for this presentation-only change. Independent main-gate review remains required.

## Evidence and reproduction boundary

**Confirmed** below means either a named static source fact or an explicitly linked retained
observation, as qualified. **Inferred** marks a recommendation's expected benefit or reuse fit.
**Unknown** marks a boundary not established by those sources. Static reach through calls is not
natural runtime reach, exact display timing or proof that every data variant works.

The original baseline is the USA retail ROM, SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`, and
`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`.
The original investigation's remake baseline was `7fb1cecb432ff4032193df88f3d93d0e92f11364`.
The current consumer trace is calibrated to accepted main
`d574f0e8a8cfbd907dd9507bbd49b639052b216d`. Later changes must recheck the named entries and guards;
the original investigation and retained observations do not automatically certify newer code.

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
`uv run sf2 h2 common-menus`. The #1622 investigation read the pinned source directly without a
new H2/H3 run, ROM extraction or original runtime. Its local reproduction identity remains in
ignored `local/issue1622/HANDOFF.md`. The #1642 maintenance pass consumes that accepted evidence and
reads current code/document owners; it adds no original-game finding. Its direct checks belong in
ignored `local/issue1642/HANDOFF.md`. No private absolute path, binary, game artwork, capture or source
listing is published.

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

## Responsibility and admission route

Use this map with the player-flow rows below; it names responsibilities, not a second status ledger.
The [architecture](../architecture.md#production-assemblies) retains the dependency boundary.

| Layer | Authority and real consumer |
| --- | --- |
| Domain (D) | `EngineBattleState`, `BattleMovement`, `BattleTurnFlow` and selected SF2 action rules own legality, numerical effects, actor/queue/loadout state and RNG. UI availability cannot override them. [Rules](../domain/gameplay-rules.md). |
| Application (A) | `GameSession` publishes state; `BattleChoices` and `BattleMovementQuery` expose current offers; dispatchers and continuations own selection, actual delivery, outcomes and restore. `QueryBattleInspection` exposes settled public details and shared `BattleVisibility`; it never submits a command. [Session](../application/session-and-programs.md). |
| Content (C) | Scenario/private readers admit encounter definitions, actions, equipment, return programs and resources. `OpeningRestoreReader`/`BattleSaveWire` admit continuation without executing battle entry. A supported rule is unavailable when its content binding is absent. [Profiles](../content/profiles-and-trust.md). |
| Godot (G) | `GameRoot` routes input/pause/host modals; `BattleSessionView`, `BattleCommandMenu`, `BattleMapViewport` and scene/result views project live state and report actual completion. `OpeningPersistenceHost` and entry restore consume A/C save decisions. They do not own gameplay legality. [Presentation](presentation.md). |

The source links above and each row's detailed owner lead to implementation and acceptance. The
[current consumer audit](#evidence-and-reproduction-boundary) identifies the inspected facade/views;
the transaction-admission discussion below names the decisive private guards. No query or UI class
substitutes for Content admission and a mounted normal route.

## One player decision, from entry through return

Original entries are **Confirmed static** S1–S8 unless marked manual. Current consumers/guards are
**Confirmed code structure** at the current audit pin. Linked retained observations establish only
their named native/controlled boundaries. #1633 adds the focused inspection observations linked below. Modern layout,
source fidelity, functional access and human acceptance are deliberately distinct.

### Enter, understand and inspect

| Player need / original basis | Remake decision and operation / return | Responsible consumers and ordinary admission | Acceptance boundary / remaining work |
| --- | --- | --- | --- |
| Enter battle, then know whose turn it is. S1 and the [battle loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control) focus/settle the actor, display mini status/land effect and build movement; sleep/stun/AI restrict control. | Finish actual before/load/start and scripted work before player input. Show actor HP/MP, legal stops and round/gold; no added deployment screen. | C's connected encounter/entry/return programs → A `BattleAdvancer`/`GameSession` → G `GameRoot`/`BattleSessionView`/acting card. D owns player/automatic control. **Connected Battles01–07**, not the later campaign. | [Campaign agreement](../application/pre-voyage-campaign.md), [status-card agreement](presentation.md#battle-status-card-agreement) and [movement observation](../verification/host.md#battle-movement-range-observation) bound engineering acceptance. The [ordinary acting-card owner](presentation.md#ordinary-acting-card-information-1655) records #1655: live class/level and actor-qualified land effect now share disclosure with inspection and follow actual player/undo delivery. Human visual approval remains #1456. |
| Look independently of movement. S1–S3 enter free-cursor `BattlefieldMenu` after Movement B undo. | B completes real undo/delivery → cursor at origin; B from cursor returns to the same actor’s post-undo Movement. Action-menu Cancel still returns to Movement first. No preview-preserving shortcut. | A `QueryBattleInspection` admits the settled player boundary; G `BattleSessionView.Inspection` waits for actual return delivery, then `BattleInspectionView` owns disposable focus and `BattleMapViewport` temporarily frames its cursor. **Connected Battles01–07** use this common consumer. | [Inspection implementation/observation](presentation.md#battle-inspection-and-on-demand-map-1633) covers moved/no-move entry, pure browsing and same-origin return on connected Battle01 and an authored appearance case. Nonzero-region geometry is separately bounded to existing controlled Battle02. This is not full-campaign or human visual acceptance. |
| Inspect any living disclosed ally/enemy, including outside action range. S2–S5 provide short/full sheets with caller-specific returns. | Pointer/directional focus shows short status. A/Confirm or a complete unit click opens full details for any disclosed unit; Back restores that cursor, including the acting unit. Cursor Cancel returns to Movement. This is the accepted modern caller policy; original S2 returns remain documented below. | A `BattleInspectionQuery` reads current actors and admitted names; shared `BattleVisibility` filters ordinary board disclosure. G reuses `PartyDetailsPanel` scalar widgets and the dedicated `BattleInspectionCard`. `QueryPartyDetails` remains field-only. | [Disclosure boundary](#what-status-and-terrain-inspection-may-disclose) and [bounded native evidence](presentation.md#battle-inspection-and-on-demand-map-1633) cover ally/enemy cards, full sheets, caller returns and an off-camera enemy. Private stat values ≥100 remain unavailable; enemy LV/EXP/class are N/A. Unsupported status details remain explicit. |
| Understand land effect. S6 uses the inspected/moving actor’s mover and terrain; displayed setting ×15 gives 0/15/30. | Hover status names the inspected actor and location alongside the setting. Empty ground names the regular-ground evaluation and impassability. Compact decision cards retain HP/MP; full details retain class/level/status. Confirm on empty ground opens options. The setting is not a damage-reduction percentage. | D `BattleTerrainRules.LandEffect` shares mover/protection semantics with `Sf2PhysicalAction.LandMultiplier`; A `QueryBattleInspection`/`QueryBattleActingStatus` share `BattleUnitProjection`; G displays actor-qualified terrain beside short status and on the ordinary acting card. C semantic terrain supplies the actual tile. | Engine behavior checks cover Regular/Heavy, Flying/Heavy and Hovering/Light. Native checks bind the shown land effect to the inspected live unit. The [#1655 owner](presentation.md#ordinary-acting-card-information-1655) adds Regular/Flying/Hovering engine checks, native terrain crossing/undo, actor handoff and inspection return, including Chinese/remapped controls. Original sub-tile cadence and human visual approval remain Unknown. |
| Open empty-ground options. S2 dispatches Map, Member, Quit and Speed. | Inspection options return to cursor. Map, Member, Speed and Quit are offered here; host Menu stays separate. | G `BattleInspectionView` opens Options from A/C on empty ground, Map/Member from the selected A/C choice, and returns through the real parents. The existing host Menu can pause/return without publishing music metadata during inspection; its Save offer assesses the live audio observation without publication, while explicit Save retains strict actual capture. | #1633 covers Map/options/cursor. The [Member owner](presentation.md#battle-member-roster-and-read-only-details-1645) covers the roster extension; [Speed #1650](presentation.md#battle-speed-and-selective-messages-1650) is delivered through this exact safe caller; [Quit #1651](presentation.md#battle-options-quit-1651) now consumes the existing two-slot Save/SaveQuit flow with its own Options/cursor return and shared pause/release barriers. **Resource/presentation:** the original Member/Map/Speed/Quit normal/selected art identified by [#1669](presentation.md#shop-and-options-art-acceptance-1669) now accompanies the accepted vertical labeled list. Up/Down and full-row pointer gestures select; caller, eligibility and return remain unchanged. [Production observations](#production-f4f8f9-contract) cover the new composition. Shop details remain in the [resource owner](../content/presentation-assets.md#shop-and-battlefield-menu-resources-1669). **Human experience:** Unknown. |
| See the whole admitted battlefield. S7 draws the area and entity indicators, then returns to options. | On-demand Map → empty-ground menu → cursor; disclosed units/camera rectangle at unchanged ordinary board scale. No click-to-act or permanent minimap. | C `SourceMapRegion` bounds the area; A `QueryBattleInspection` supplies only known living placed units. G `BattleOverviewView` draws admitted semantic terrain classes/outline, ally circles/enemy diamonds, active-unit ring and clipped camera rectangle, plus a navigable known-unit list. | [Native evidence](presentation.md#battle-inspection-and-on-demand-map-1633) checks actual Map parents/list, camera purity, small-board centering and nonzero source-region offset. Original minimap pixel/category fidelity and human visual approval remain Unknown. |
| Browse battle Member roster. S2 copies `UpdateForce`'s joined-and-active list (including defeated/unplaced members), then S5 full sheet → same list → options. Reserve is separate. | Preserve list caller identity and selected row, even for the current actor; read retained live scalars without initializing missing members. | A `QueryBattleMembers` shares settled-origin eligibility and `BattleUnitProjection.Details` with inspection. G `BattleInspectionView`/`BattleMemberListView` reuse `PartyDetailsPanel`, mapped input and the same stale lifecycle. Field `QueryPartyDetails` remains field-only. **Connected Battles01–07 admission.** | [#1645 owner and bounded acceptance](presentation.md#battle-member-roster-and-read-only-details-1645): engine dead/unplaced/empty/unavailable and active/reserve checks; actual controlled-connected Battle01 current/other details, selection/multilevel Back, purity, replacement and Chinese/remapped projection. Natural original reach, later encounters and human visual acceptance remain Unknown. **Resource/presentation:** shared PartyDetailsPanel item/spell rows borrow semantic source textures, retain live slots/equipped text and independent scrolling; [#1668 acceptance](presentation.md#member-inventory-art-acceptance-1668) separates ordinary access, actual art and controlled variants. **Human experience:** Unknown. |

### Move, choose an action, target and confirm

| Player need / original basis | Remake decision and operation / return | Responsible consumers and ordinary admission | Acceptance boundary / remaining work |
| --- | --- | --- | --- |
| Move or stay at origin, then reconsider. S2/S3 accept a legal stop and undo provisional movement on action-menu Cancel; Movement B then enters inspection. | Weighted legal range includes the committed origin. Confirm at origin opens actions; Stay is an explicit action confirmation. Cancel restores origin/range through actual return delivery. | D `BattleMovement`/terrain own paths/stops; A dispatcher keeps actor position separate from `Selection.Preview`, and `BattleMovementContinuation` delivers preview/undo. G range/map/menu consume them. **Connected**, including no-move turns. | [Movement agreement](presentation.md#battle-movement-range-agreement) and [actual delivery](../verification/battles.md#battlefield-movement-consumer). Inspection transition remains #1633. Directional hold now uses the [battle navigation agreement](presentation.md#battle-directional-hold-and-mode-isolation-1615); the [selection/preview agreement](presentation.md#battle-selection-and-preview-feedback-1617) replaces gold tint/path underlines with a steady separate actor cue; original pixels and timing remain Unknown. |
| Choose Attack, Magic, Item or Search/Stay. S2 derives initial Attack/Stay focus from targets and substitutes Search for a faced chest. | Fresh Main focuses Attack only for an enabled physical target, otherwise Down. Down is Search for a registered chest at preview + delivered facing, otherwise Stay. Search confirmation cancels free; full/occupied result returns to the same menu; item/gold/empty result ends through Stay after actual feedback. | D `BattleSearch`/shared inventory → A `BattleSearchContinuation`/typed choices and receipt flags → C per-encounter `BattleSearchReader` → G contextual menu, result and open marker. **Connected basic actions; Search is explicitly authored/controlled.** | [Initial focus](presentation.md#initial-battle-command-focus-1644), [Search contract](../application/session-and-programs.md#battle-search-and-chest-return), [admission/source inventory](../content/profiles-and-trust.md#battle-search-admission), [native input](../development-and-verification.md#battle-search-observation). Battles01–07 provide no in-region chest candidate; original natural access remains Unknown. Traps/later maps and private source-noun translation are not claimed. **Resource/presentation:** #1666 confirms ordinary Continue consumes complete Attack/Magic/Item/Stay normal/selected rasters; absent private art previously fell back to text. Contextual authored Search remains text, not a restored original index4 image. **Human experience:** Unknown. |
| Choose spell/level or item, target and commit; back out safely. S2 checks MP/range and restores spell/item/action parent on cancellation. | Modern finite lists retain actual spell level/cost, item slot, target and unavailable reason. Target Back restores its list/main; Main Cancel undoes movement. Complete selection and explicit Confirm are required to commit. | D selected action rules own effects/payment/RNG; A `BattleChoices`/dispatcher validate the live offer, target and revision. C supports only admitted definitions. G `Handle/Select/PickTarget`/`ReturnToBattleActions` own pages. **Connected bounded actions.** | [Localized command/target boundary](presentation.md#localized-battle-command-and-equipment-display), [native commands](../verification/host.md#localized-battle-commands-targets-and-status). Off-range attempted candidates cannot commit and are not a free-inspection feature. Unsupported effects stay explicit; no universal spell/item claim. **Resource/presentation:** ordinary Continue consumes semantic spell/item normal/AND-selected icons with localized level/cost/slot text and unavailable reasons; [#1667 acceptance](presentation.md#identity-art-acceptance-1667) separates ordinary entry from controlled multi-level/duplicate coverage. Target Back retains the chosen list row. **Human experience:** Unknown. |
| Give/exchange held items. S2 selects a slot/adjacent recipient, exchanges a full pack and consumes the turn as Stay. | ActionChoice advertised G/installed mapping → actual slot → adjacent recipient → required full-pack return slot → confirmation/result → ordinary Stay/next-turn suffix. Cancel backs up before commit. | D `BattleGive`/A selection and continuation → existing localized G `BattleGiveView`. C `OpeningLoadoutBindings` explicitly binds field carry identities after live equipment for product New Game/Continue. `BattleGiveReader` keeps its standalone guard. **Connected Battles01–07 finite supported catalog.** | [Capability/evidence](../capability-status.md#connected-battle-give-and-exchange), [admission](../content/profiles-and-trust.md#connected-battle-give-admission), [native input](../development-and-verification.md#connected-battle-give-observation). #1647 covers ordinary/equipped full exchange, no-target/illegal/cancel/stale refusal, current ATT/range, exactly-once Stay, v2 record restore and field custody. Chinese/remapped connected Battle01 observes templates plus selected member/item names. Rings/curses/broken/unsupported states reject; Drop and later campaigns remain separate. **Resource/presentation:** the existing HUD draws semantic given/returned item icons beside localized slot/recipient text; [#1668 acceptance](presentation.md#member-inventory-art-acceptance-1668) covers ordinary Continue and controlled-connected full-pack exchange. No separate source recipient art is invented. **Human experience:** Unknown. |
| Drop an item. S2 confirms, refuses cursed items and handles rare Deals custody. | Advertised R/installed mapping → exact slot → confirmation → real result → same ActionChoice/preview without spending the turn. Committed removal survives outer movement Cancel. | D `BattleDiscard`/A selection/continuation → existing localized G `BattleDiscardView`. C `OpeningLoadoutBindings` explicitly binds admitted item metadata after live equipment/Give. `BattleDiscardReader` retains its standalone guard; controlled callers without product binding stay unavailable. **Connected Battles01–07 finite catalog.** | [Capability/evidence](../capability-status.md#connected-battle-discard-and-deals), [admission](../content/profiles-and-trust.md#connected-battle-discard-admission), [native input](../development-and-verification.md#connected-battle-discard-observation). #1648 covers ordinary/equipped/rare Deals, cancel/stale/repeat refusal, one receipt, free return, v2 record restore and field custody. Connected English/Chinese remapped Battle01 observes real equipment removal and movement undo. Unsupported cursed/ring/broken states explicitly reject; no full curse system, later campaign or physical disk claim. **Resource/presentation:** the existing slot/confirmation HUD draws the live item identity with source selected pixels; [#1668 acceptance](presentation.md#member-inventory-art-acceptance-1668) preserves localized equipped/rare/protected text and the existing free return. **Human experience:** Unknown. |
| Change equipment in battle. S2 returns to commands after equipping. | Eligible carried slot/Unarmed preview → confirm → same action menu and movement preview; no action cost. Cancel retains live gear. Current slots drive later ATT/range and survive battle save/return. | D `BattleEquipment` → A equipment selection → existing G localized auxiliary presenter/default E or installed mapping. C `OpeningLoadoutBindings` explicitly enables supported player allies through `PrivateFieldPreparation.Equipment`; field-only controlled callers and enemies stay immutable. **Connected Battles01–07 finite ordinary weapons.** | [Connected capability/evidence](../capability-status.md#connected-battle-equipment-agreement), [explicit admission](../content/profiles-and-trust.md#connected-battle-equipment-admission), [native input](../development-and-verification.md#connected-battle-equipment-observation). Rings/curses/broken or other effects and unsupported states remain rejected/unoffered. No battle Give/Drop admission is implied. **Resource/presentation:** ordinary Continue consumes the selected item/Unarmed source icon beside localized equipment preview text at16×24/4×/Nearest; [#1667 acceptance](presentation.md#identity-art-acceptance-1667) preserves functional/resource/human boundaries. **Human experience:** Unknown. |

### Deliver the consequences and regain control

| Player need / original basis | Remake decision and operation / return | Responsible consumers and ordinary admission | Acceptance boundary / remaining work |
| --- | --- | --- | --- |
| See committed action, damage/healing, messages and death. S1 separates Stay/retreat from ordinary battlescenes. | Publish only validated action state; deliver real movement/scene/resource/message/death phases before handing control back. Keep cards tied to the live actor/reaction target. | D selected action rules → A action/scene/death continuations → C admitted scene resources → G `BattleSceneView`/map/status cards and actual completion. **Connected finite scenes**, with authored support separately bounded. | [Scene/message owner](battle-scenes.md#message-and-outcome-presentation), [semantic feedback](presentation.md#localized-semantic-battle-feedback) and their observations retain failures. [#1620 weapon composition](battle-scenes.md#weapon-frame-composition-1620) corrects the reversed interpretation of admitted layers1/2; bounded knight hit and sword gesture observations preserve frame/flip/offset and UI delivery. Original natural reach, animation/pixel parity and human readability remain **Unknown** beyond named evidence. |
| Receive EXP/gold/growth, then next actor/round. The [loop](../../../docs/research/battle/battle-loop.md#top-level-battle-control) orders deaths, faction checks, after-turn and next entry. | Retain ordered reward/growth messages; no separate duplicate summary. Finish real acknowledgements, status and round/appearance programs before the next legal input. | D reward/turn rules and queue/RNG → A `BattleAdvancer` and continuations → C admitted programs → G message/death/result consumers. **Connected bounded progression**; not all source statuses or encounters. | [Return/status agreement](presentation.md#battle-messages-return-and-party-status), [campaign acceptance](../application/pre-voyage-campaign.md) and [capability limits](../capability-status.md). Inspection must remain unavailable during these transitions. An exposed queue is not an approved action-order display. |
| Resolve victory, defeat or Egress and return to a usable field. Original battle loop and outcome programs own distinct paths. | Complete actual outcome/caller cleanup; retire battle windows and resume real field input/save eligibility. Defeat/retry differs from victory; retreat is offered only by admitted actions. | D outcome/retreat policy → A outcome/retreat/program continuations and `LeaveBattle` → C connected return routes → G result/retreat/field presenters. **Connected finite outcomes**; private Egress is bound for Battle02, not a general Battle01 retreat capability. | [Campaign boundary](../application/pre-voyage-campaign.md), [field evidence](../verification/field-programs.md#battle01-through-battle02) and [save return observations](../verification/battle-save.md#confirmed-observations-and-limits). Focused Battle01 victory/defeat and Battle02 Egress do not prove natural seven-battle replay or later campaign support. |
| Adjust original Speed/message options. S8 edits speed 0–3 and selective suppression; B restores both, A/C keeps them. | Settled inspection Options → Speed edits two drafts. The explicit modern mapping preserves custom cps; B cancels, A/C persists. Distinct from Shift acceleration. | G SettingsHost/SettingsView reuse preference storage and pause/release/focus rules without opening general Settings in battle. A BattleSceneState marks only ordinary noncritical physical damage ResultMessage and GoldMessage optional; G hides them and acknowledges each exact token through the real continuation. All other messages remain delivered. | **Implemented bounded [#1650](presentation.md#battle-speed-and-selective-messages-1650).** Actual native default/suppressed attack sequences and final gameplay/RNG agree; transaction, physical preference restart and Chinese/remapped input pass. [Completion policy](battle-scenes.md#selective-hitcoin-message-delivery-1650). Original timing/full message parity and human visual approval remain unclaimed. |

### Save here, quit and Continue this turn

| Player need / original basis | Remake decision and operation / return | Responsible consumers and ordinary admission | Acceptance boundary / remaining work |
| --- | --- | --- | --- |
| Save an unfinished battle and later resume. S2 excludes Battle00; confirmation copies elapsed seconds, sets flag88, writes the slot and normally transfers to `WitchSuspend`; the loop has suspended entry. | **Implemented modern decision:** host Menu → Save at settled player Movement/origin → two slots/explicit overwrite → Save and return or Save and Quit. Fresh title Continue resumes this actor/queue entry. Saves are reusable. Busy/preview requests refuse without undo or queued save. Host Menu and empty-ground Options Quit both consume the same admitted save flow; Quit preserves its Options/cursor caller on cancellation, failure and Save/return. | D continuation values/queue/AI/RNG → A `QueryBattleSave`/`CaptureBattleSave`/`RestoreBattle`, stable/route guards → C strict battle-v2 codec and no-init restored definitions → shared `LocalSaveStore` → G `OpeningPersistenceHost`, ordinary entry, real battle resources/audio preparation. **Connected Battles01–07**, not standalone diagnostics or arbitrary animation. | [High A/B/C contract](../application/battle-save-and-continue.md), [native acceptance](../verification/battle-save.md). **Confirmed bounded engineering delivery**, not a missing persistence feature: #1639/#1640/#1641 implement #1636–#1638 under #1618. Actual exit/new PID, exact pre-input state/two seeds, matched suffix, seven controlled shapes, focused outcomes, explicit backup and publication failures are retained there. Physical focus/OS-close, human experience and exit warning limits remain **Unknown** as qualified. |

The original empty-ground **Quit entry is connected by [#1651](presentation.md#battle-options-quit-1651)**.
G owns its current Options eligibility and caller-specific return/pause/release flow, consuming A's
existing save availability and shared host dialog. Slot cancellation, overwrite refusal, Save/return,
error Back and uncertain Inspect/Back preserve the same legal cursor and Options focus; explicit
Save and Quit still requires verified publication. This adds no C admission, format or recovery policy.
The [native owner](../verification/battle-save.md#options-quit-caller-1651) retains bounded evidence and
exit-warning/physical-device Unknowns; A/B/C remain the persistence and restart evidence.

The battle direction route reuses `GameInput.Resolve` for fresh mapped edges and held direction;
`BattleSessionView.Navigation` now repeats eligible directional navigation at the existing 120 ms
adapter cadence, waiting for actual player movement delivery. **Confirmed bounded adapter behavior:**
release, turn, rejection, actor/page changes, focus handlers and host pause clear the hold.
Confirm/Cancel and shortcuts remain edge-only. The [current agreement and finite native observation](presentation.md#battle-directional-hold-and-mode-isolation-1615)
own [#1615](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1615)'s engineering acceptance;
physical OS focus, original timing and human experience remain unclaimed. The field clock is unchanged.
The [selection/preview agreement](presentation.md#battle-selection-and-preview-feedback-1617) owns
#1617: `BattleMapViewport` retains sprite colors and `BattleSelectionView` follows the actual actor
with steady white corner brackets; the gold path underlines are removed. Legal-stop and target
feedback remain separate. The [source evidence](battle-scenes.md#battlefield-selection-evidence)
confirms distinct movement/blink/grid/cursor mechanisms, not these modern pixels or timing.
Do not duplicate either foundation issue in an inspection implementation.

Transaction support must be read with content admission. The precise current boundaries above come
from [Give admission](../../src/Sf2.Remake.Content/Scenarios/BattleGiveReader.cs),
[Discard admission](../../src/Sf2.Remake.Content/Scenarios/BattleDiscardReader.cs),
[private battle construction](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs),
[private field equipment binding](../../src/Sf2.Remake.Content/Scenarios/PrivateFieldPreparation.cs),
[restored loadout binding](../../src/Sf2.Remake.Content/Persistence/OpeningLoadoutBindings.cs) and
[battle Equip guard](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs).
Private weapon contributions and field Prepare alone do not establish battle transaction admission.
Product battle Equip, Give and Drop now have the explicit bindings above. Standalone reader guards
and controlled callers without those bindings remain distinct; this trace does not itself admit a capability.

Message suppression is selective. **Confirmed static:** S8's `bsc10_displayMessage` checks
`NO_BATTLE_MESSAGES_TOGGLE` before `DisplayText`; `bsc11_displayMessageWithNoWait` does not.
The manual describes retaining level/EXP/special-item messages when ordinary hit/coin text is off.
This does not authorize suppressing all rewards, status or progression acknowledgements in the
remake. The accepted [finite mapping and completion contract](battle-scenes.md#selective-hitcoin-message-delivery-1650)
implements that distinction without changing logical timing or required messages.

## What status and terrain inspection may disclose

| Information | Original static boundary | Remake reuse and limit |
| --- | --- | --- |
| Short card | S4 reads current/max HP/MP; ally header includes name/class/level, enemy header name. | `BattleStatusCard` reads live HP/MP and readable names; inspection and the ordinary acting card share ally class/level disclosure through `BattleUnitProjection`. Missing class remains unavailable; do not infer identity from labels. |
| Full attributes | S5 reads current HP/MP, ATT/DEF/AGI/MOV and status effects; allies have class/LV/EXP. Enemy LV/EXP render N/A rather than an inferred value. | `BattleActorState` has runtime resources/progress/stats; content carries class/loadout information. A battle query must read the live actor, not reinitialize from a deployment or use field carried-party values. Preserve missing/unsupported values visibly. |
| Full status effects | S5 explicitly writes curse, poison, muddle, silence, stun, sleep and stat-modifier indicators. | Current small card's silence/low/dead cues are not a full status screen. Decode only admitted status semantics and retain duration uncertainty. |
| Spells/items | S5 reads spell slots and held items, with equipped annotation; enemy sheets use the same relevant readers. | Reuse admitted names, runtime loadout, spell and equipment mechanisms. Inspection displays them without opening Prepare, equipping, giving, dropping or spending MP. Unknown names are not permission to expose raw private diagnostics. |
| Portrait/records/gold | S5 omits enemy kills/defeats; portrait absence skips the portrait/gold branch. | Reuse admitted field detail widgets where suitable, but do not fabricate enemy records or require missing private portrait art to browse combat stats. |
| Land effect | S6 obtains the inspected/moving combatant's move type and terrain, displays setting ×15 percent (0/15/30). S2 opens land effect with another unit's short card; S3 updates its movement-time input. | Semantic terrain exists in `BattleDefinition.Terrain`; inspection short status and the ordinary acting card read actor-qualified land effect. The acting query uses the last completed movement/undo segment tile, then the settled preview destination. `BattleTerrainRules.LandEffect` and `Sf2PhysicalAction.LandMultiplier` share protection/mover semantics (Flying gets no protection); `BattleTerrainRules` owns movement cost separately. Both public queries reuse these semantics, never infer percentage from tile artwork or treat it as the damage-reduction percentage. |
| Empty-tile terrain | S2's empty-ground branch opens options; it does not open a land-effect window for every arbitrary cursor tile. | The accepted modern hover shows Ground effect for a regular ground unit, Ground impassable for Barrier/HighSky/Impassable, and coordinates. Occupied cells use the inspected unit's actual mover-qualified query. This is not an actor-independent damage percentage. |
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

## Inspection contract and original return distinctions

The heading is retained as a stable link from the original proposal. This is now the accepted
functional contract implemented by #1633. The [detailed current owner](presentation.md#battle-inspection-and-on-demand-map-1633)
records its consumers, bounded native observations and remaining limits. A public read-only projection
plus disposable inspection focus consumes the live state. No second battle, AI simulation, saved inspection state or generalized knowledge platform
is justified for the settled opening scope.

### Query and session boundary

Add one battle-inspection query alongside `GameSession.QueryBattleChoices` and
`QueryBattleMovementRange`. Its minimum result is availability/reason, session/revision identity,
active actor, battle dimensions/region, disclosed unit IDs/positions, requested unit public details
and actor-qualified terrain information. It reads `Current`; it never calls Start, Submit, Prepare,
AdvanceSimulation or any RNG service. `QueryBattleChoices` remains the authority for actionable
offers; it cannot supply arbitrary inspection targets. `QueryPartyDetails` stays field-owned until
a deliberately shared scalar detail projection is extracted; do not make it initialize a battle.

Initially enter only after Movement B/Cancel has completed its existing undo/return path, with a
live player selection in settled Movement and no movement delivery,
scene/retreat/Give/Discard/round/appearance/program/outcome or failure. Reject hidden/unplaced units
and stale IDs explicitly. Reuse ordinary-board visibility rules at that boundary, including staged
entity visibility; do not reveal a positioned actor before its appearance. Existing board projection
and appearance filtering are the first owners to reuse, rather than a second reveal-history store.

Keep only cursor tile, selected public unit, panel/focus and return context in the adapter. Bind
that context to the current session/revision/actor; retire it on replacement or a changed boundary.
During inspection no gameplay commands, automatic turns, movement acknowledgements, resource writes
or reward acknowledgements may pass through. Establish the purity baseline **after** the authorized
Cancel and its real return delivery finish. Browsing/closing preserves that post-undo selection,
origin preview/path, round/cursor/queue, all actor/party/story state and both RNG seeds. Revision and
observation sequence do not advance merely for inspection reads. The entry's real Cancel/delivery
may publish their ordinary revisions; the old tentative destination is intentionally canceled.
Animation of a cursor/camera is display state, not a logical battle opportunity.

### Entry, return and provisional movement

The selected first version follows the original entry/return contract. It does not add a default
Inspect shortcut or permanent entry that preserves a tentative move:

| Transition | Required behavior |
| --- | --- |
| Movement B/Cancel (S1–S3) | Reuse the existing undo and actual return-path delivery, then enter inspection. If the actor has not moved, enter directly at the settled boundary without fabricating a movement wait. `MoveBattleEntityByMoveString` writes the final chosen X/Y; the returned actor's origin is the initial cursor location. |
| Action menu Cancel | Keep its existing return to Movement/undo. Do not enter inspection from every menu Cancel; the subsequent Movement B/Cancel owns the inspection entry. |
| Inspect another unit or empty tile | Directions move only the inspection cursor over admitted battle cells. A/full-details and C/short-status meanings are local to this mode; empty ground opens battle options. These inputs never issue gameplay `Move`, `SelectTarget`, Attack or `Confirm`. |
| B while in the free cursor | Close inspection, retire its camera override/cursor and restore the same actor's post-undo Movement state. It does not end the turn. Require release before ordinary gameplay consumes another action. |
| A on the current actor | S2 opens `BuildMemberScreen`; after that screen closes, `@loc_5` compares the selected ID with the current actor and returns from `BattlefieldMenu`. Resume the same actor's Movement state, rather than reopen the free cursor. |
| C on the current actor | S2's `@IsCursorOnMovingEntity` returns directly before `@OpenMiniStatus`. Resume actor control; do not add a new current-actor short-card child here. |
| A/C on another living unit | A opens the full sheet; C opens mini status and land effect. On dismissal S2 loops to `BattlefieldMenu` for the same cursor context. S5's full screen waits for A/B/C; this caller's return destination differs from inspecting the current actor. |
| Empty-ground menu / Map / Member | Menu Cancel returns to the cursor. Map returns to the empty-ground menu. Member's selected full sheet returns to its member list, even when it displays the current actor; leaving that list returns to the empty-ground menu. Preserve caller identity rather than using the direct-cursor active-unit rule for every full sheet. |

**Current modern override:** #1683/#1686 use one detail child for A/Confirm/pointer, and Back restores the inspected cursor even for the acting unit. The table above retains the original S2 distinction as evidence, not the current UI return rule.

**Deferred intentional modernization:** a separate non-destructive Inspect entry could preserve a
Movement preview, or later an ActionChoice/TargetChoice parent, selected item/spell and target.
The preview-preserving entry remains outside this scope.
Default B/Cancel entry is fixed above; map the mode-local A/C functions and release barriers through
the existing input authority with #1615, and let #1616 place contextual hints rather than another
permanent Inspect control. The accepted explicit override above owns modern details return; original evidence is unchanged.

The first slice includes the cursor → empty-ground Map entry → options → cursor route. Member's
caller-specific return is implemented by [#1645](presentation.md#battle-member-roster-and-read-only-details-1645)
under its separate acceptance; Speed and Quit/save remain outside Map acceptance. Show only working actions, or
an explicit real availability reason where the chosen layout retains an unavailable original row.

### Camera and overview

`BattleMapViewport.Present/Frame` currently chooses focus from movement, target candidate, preview
or story; `_Process` reapplies it. Add a temporary inspection subject at that owner so ordinary
Frame cannot immediately replace the cursor. #1686 also retains disposable edge-pan framing until
fresh keyboard navigation resumes cursor reveal. Reuse `WorldViewportLayout.Focus/Clamp` and the
battle's admitted `SourceMapRegion` offset. Keep 96px cells, clipping and legal area boundaries;
do not pan into neighboring packed rooms, storage, hidden entities or an entire source-world map.
Returning removes only the override and resumes the same actor at the post-undo origin. The modern
dead zone, easing and framing remain modern policy, not original camera evidence.

The on-demand Map reuses the [overview proposal](modernization-design.md#read-only-battle-overview):
whole admitted battle area, disclosed allies/enemies, active unit at its post-undo origin and the
clipped camera rectangle, with shapes/labels and a navigable known-unit list. This first scope has
no retained tentative destination to display. It reads the camera without moving it; Back returns
to the empty-ground menu, whose own Back returns to the inspection cursor. No map
click can move a unit or commit a target. Free battlefield cursor and overview are complementary;
one is not acceptance for the other. A permanent minimap, threat overlays and undisclosed objectives remain outside this scope.
The accepted modern Map pointer locates a disclosed unit at the free cursor without acting.

### Smallest useful acceptance after implementation

Use one existing ordinary player-turn boundary and one existing visibility/appearance case. Start
with nearby ally/enemy, an off-camera disclosed enemy and empty ground; expand only if a failure
needs another input. No new full-route replay, screenshots, bulk capture or comparison platform.

| Check | Required result |
| --- | --- |
| Query purity and disclosure | Small engine behavior tests show repeated live reads preserve state/RNG, reject busy/stale/hidden units and do not reveal pending appearances. Use the real query, not tests of a probe/report. |
| Real entry and return | Actual adapter input/state/projection/errors show moved preview → B/Cancel → real undo delivery → cursor at origin; no-move B enters directly. Capture the post-undo baseline, then inspect another unit and Map, return through their real parents and B back to the same actor at origin. Compare selection/path, session/revision/sequence, seeds, resources and queue against that baseline, not the canceled preview. Menu Cancel still returns to Movement before a later B enters inspection. |
| Current actor and caller-specific returns | From the cursor, C on the current actor resumes Movement directly; A opens full details then resumes Movement on dismissal. A/C on another unit returns to the free cursor after dismissal. A member-list full sheet returns to its list even for the active actor. No close event leaks into a gameplay confirmation. |
| Information | Ally/enemy short/full details read live HP/MP, admitted stats/status/equipment; unsupported fields remain explicit. Test two mover/protection combinations so land effect cannot be a hardcoded tile percentage. |
| Camera | Read actual cursor/focus, region origin and camera bounds at opposing board edges and return. The board scale never changes when panels open. Include a nonzero source-region origin and small-board centering where existing inputs permit. |
| Input isolation | Keyboard/pad and mapped controls, pointer controls where offered, hold/release, child Back, focus loss and host Menu pause never move/commit on dismissal or carry a held direction into gameplay. Coordinate with #1615 rather than duplicating its repeat clock. |
| Availability and errors | Reject entry during movement delivery, AI, scene/result/death, appearance/program, retreat/outcome and fault. A session replacement closes inspection and cannot restore a stale actor or pending command. |

Use affected engine behavior tests if a public query/rule is added, adapter compilation for its
consumer, the clean committed planner and direct bounded native observations. Reuse the owned
Godot project/instance and registered inputs; a concrete startup failure must justify any restart.
The detailed current owner distinguishes the checks observed by #1633 from the Member flow,
physical OS focus behavior and human visual acceptance that it does not establish.

## Sequencing and remaining uncertainty

| Order / owner | Bounded outcome and dependency |
| --- | --- |
| Foundation: #1615 and #1617 | #1615 now supplies bounded directional hold/mode isolation under its [agreement and native observation](presentation.md#battle-directional-hold-and-mode-isolation-1615). #1617 supplies [source-calibrated selection feedback](presentation.md#battle-selection-and-preview-feedback-1617): unchanged sprite colors, a steady actor cue, retained legal-stop/target feedback and no gold trail. Its bounded actual consumer checks retain original timing/pixel and human-approval Unknowns. Both are foundations of #1633; neither supplies inspection. |
| Original access: #1633 | Implemented public live query + Movement B undo/delivery → cursor + caller-specific details + Map. [Detailed owner and observation limits](presentation.md#battle-inspection-and-on-demand-map-1633). Member, Speed and [Quit](presentation.md#battle-options-quit-1651) have their separate delivered owners; host save remains available through its existing owner. |
| Delivered finite capabilities | Member is delivered under [#1645](presentation.md#battle-member-roster-and-read-only-details-1645); connected Equip has the [finite #1646 boundary](../capability-status.md#connected-battle-equipment-agreement), Give the [finite #1647 boundary](../capability-status.md#connected-battle-give-and-exchange), and Drop the [finite #1648 boundary](../capability-status.md#connected-battle-discard-and-deals). #1649 Search now has the [finite authored/controlled boundary](../capability-status.md#battle-search-capability); #1650 Speed/message access is [delivered](presentation.md#battle-speed-and-selective-messages-1650); #1651 empty-ground Quit is [connected](presentation.md#battle-options-quit-1651) to the existing save flow. The player-flow rows retain A/C/G responsibility and existing authored evidence. Inspection/options consumers are supplied by #1633; Quit consumes the existing save boundary. #1369/#438 own dispatch, not this trace. |
| Delivered recovery: #1618 | A/B/C are implemented through #1639/#1640/#1641. Use the [current save contract](../application/battle-save-and-continue.md#implemented-ordinary-host-boundary-c), not the obsolete field-only assumption. #1633 does not reopen persistence; #1651's Options Quit entry consumes its eligibility, real parent returns and shared failure/publication rules. |
| Visual composition: #1616 | The [design system](ui-design-language.md) specifies persistent command names beside original icons. The [accepted scene review](ui-scene-review.md) and [current production contract](#production-f4f8f9-contract) own right-hand compact vitals, aligned command/child lists, real range/target feedback and inspection. Historical diamond observations retain only their named resource/input evidence. |
| Independent correction: #1620 | [Located and corrected](battle-scenes.md#weapon-frame-composition-1620): source layer1 is behind the body, layer2 in front. Actual knight attack/hit and adjacent sword item gesture preserve frame/flip/offset, action return and UI canvas order. The finite layer2 content variation verifies the front path only; it is not original natural-source evidence. Exact user-image frame, later weapons, original timing/pixels and human visual approval remain **Unknown**. |
| Modernization: #1619 and deferred candidates | [#1619](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1619) implements held 1.5× pacing across exploration/battle at the [bounded presentation/acceptance boundary](presentation.md#held-battle-acceleration-1619-bc); original rules and fresh confirmation remain unchanged. Separate preview-preserving Inspect, ActionChoice/TargetChoice inspection, action-order bar, permanent minimap and tactical threat/intent remain **Deferred**. No action-order implementation Issue is filed; disclosure/product selection precedes a bounded consumer. No predictive simulation is authorized here. |

An action-order bar must distinguish the already constructed current-round queue, an explicitly
qualified prediction and unknown future changes. `EngineBattleState.TurnOrder` exposes the retained
queue to code, not an approved player disclosure. Original order construction uses random alteration
and can include extra entries; simple AGI sorting is not the reliable order. Future activation,
status/death and round generation can change what happens next. No bar is part of this delivery.

**Unknown:** exact original free-cursor edge/camera cadence, natural key timing, visual composition,
all minimap entity-category anomalies, and original idle/VInt effects on RNG during browsing.
The source waits for VInts, so absence of direct action/RNG calls in `BattlefieldMenu` is not proof
that every original RNG value stays frozen. Read-only remake inspection's no-RNG rule is its own
explicit contract. Hardware timing is unnecessary for the admitted access/information boundary.

**Confirmed, bounded remake evidence:** actual native input/state/projection observations reach
inspection, caller-specific details and Map through the [#1633 owner](presentation.md#battle-inspection-and-on-demand-map-1633),
with separate Member, Speed and Quit observations linked above. Quit's [save observation](../verification/battle-save.md#options-quit-caller-1651)
covers its actual two-slot caller, cancellation/failure returns, verified process exit and fresh
Continue. These controlled/authored observations establish those implemented paths and projected
content, not natural original reach or final human readability.

**Unknown:** human acceptance of the current layout/readability, physical OS focus/device behavior
outside the named observations, and natural original reach/key timing at these seams. Exact original
B-return motion and broader pixel/category fidelity retain the source/hardware limits above. Do not
convert these Unknowns into an automatic acquisition queue. A newly blocking question needs a named
boundary and the smallest existing observation that can answer it. Documentation-only reconciliation
uses direct source/link/scope/private-boundary checks, a clean committed planner and actual CI
disposition; it does not invalidate completed engine/native results or require another SDK,
normal/full suite or Godot run.

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
