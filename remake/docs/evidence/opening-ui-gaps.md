# Opening UI Gap Assessment

The connected private opening is playable from a controlled Map3 start through story/exploration,
tower entry, Battle01 and usable return. That acceptance does not establish a complete product
opening or a finished UI. The largest remaining gaps are entry/persistence UX,
a chosen world viewport, consistent field/story information, and basic menu mouse input. Existing
battle messages and return delivery need experience review rather than being classified as absent.

## Scope and reading rules

**Standard rigor:** this assessment supports prioritization under
[#1385](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1385) and
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). It owns the gap matrix and
proposed acceptance slices. Implementation truth stays with [architecture](../architecture.md),
[capability status](../capability-status.md), [presentation](../godot/presentation.md),
[assets](../content/presentation-assets.md) and [verification scope](../development-and-verification.md#scope).
Cross-scene dependencies and concurrent candidate acceptance require explicit evidence and ownership boundaries; this bounded prioritization needs no new runtime observation. Do not copy this backlog into those owners.

The inspected accepted base is
[`5138b532c4d4ff84dd13ca97d5a02892448f0e69`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/5138b532c4d4ff84dd13ca97d5a02892448f0e69),
including the independently accepted battle-entry skip. Source links below describe that base;
candidate links name a separate immutable revision. This document adds no original-game finding.

The route is controlled Map3 opening → castle/palace/Astral and exploration → tower before/load/start
→ Battle01 → outcome and field return. Title/New Game/Continue/settings before that start are
explicitly assessed below, but have not been traversed as a product route. Later chapters, a new
save system, new gameplay, asset acquisition and complete art direction are outside this assessment.
Acceptance is a complete, traceable inventory with bounded follow-up proposals; it does not require
implementing or experiencing every gap. Only this file and one nearby README navigation row are
owned. No production code, shared presentation/asset/verification owner or test changes are needed.

Evidence labels have their repository meanings:

- **Confirmed:** a named source establishes structure, or a linked retained observation establishes
  its stated runtime boundary. Static confirmation is explicitly qualified as structure.
- **Inferred:** a likely user impact or interpretation that lacks independent experience confirmation.
- **Unknown:** an unobserved experience, unresolved product choice or unsupported route; no pass is implied.

Delivery states are separate from evidence labels:

| State | Meaning in this assessment |
| --- | --- |
| **Not implemented** | No player-facing consumer in the inspected ordinary route; this can coexist with useful underlying data. |
| **Available, not integrated** | A resource, interface or candidate exists but is not connected to accepted ordinary play for this purpose. |
| **Integrated, experience pending** | Accepted code connects the behavior; present evidence does not establish its readability/discoverability or user approval. |
| **Accepted, bounded** | Existing independent engineering acceptance covers the named behavior, with retained limits. It does not imply approval of the whole UI. |

Rows may distinguish multiple states for different parts of one surface. Priorities are assessment
recommendations, not a new implementation queue: **P1** affects ordinary control or understanding of
the connected route; **P2** is a later consistency/usability improvement or a product decision before
implementation. No new P0 is established. Proposed slices A–E are defined below; an Epic link means
there is no separately dispatched implementation Issue yet.

## Entry and exploration matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| E1 · Launch; title, Start/New Game, Continue | **Not implemented** as product screens. Ordinary startup immediately selects an authored practice yard or an explicit private start. | **Unknown:** intended first-launch/menu/exit journey. Continue has no persistence backend; a controlled JSON start cannot be presented as a player save. **Inferred:** users cannot begin a normal campaign from a familiar front door. | **Confirmed structure:** [GameRoot._Ready](../../game/src/GameRoot.cs), [profile summary](../content/profiles-and-trust.md#profile-summary). [Persistence is Unsupported](../capability-status.md#milestone-and-deferred-surfaces). No title-to-Map3 observation exists in this scope. | **P2, decision first.** Define supported entry and save expectations under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) and [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). No entry implementation ticket proposed before that choice. |
| E2 · Change settings before or during play | Versioned startup input/text/reduced-flash settings are **accepted, bounded**; settings screen and automatic save are **not implemented**. | Existing configuration is not a discoverable in-game menu. **Unknown:** which settings must be exposed and persisted. Do not conflate supported remapping with a delivered settings UI. | **Confirmed structure and retained input boundary:** [InputSettings](../../game/src/Input/InputSettings.cs), [logical input/settings owner](../verification/host.md#logical-input-and-accessibility-adr-0010-9a). | **P2, decision first.** Same entry/settings product boundary as E1; [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), persistence dependency [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). |
| E3 · Invalid launch option, missing content or invalid settings | Typed startup failure reaches the common battle view: **accepted, bounded** diagnostics. | **Integrated, experience pending:** technical kind/code/field is shown; no product recovery/retry screen is connected. **Inferred:** a player may know startup failed without knowing an actionable remedy. | **Confirmed structure:** [GameRoot.Fail](../../game/src/GameRoot.cs), [BattleSessionView.FailStartup](../../game/src/Battles/BattleSessionView.cs). [Startup observations](../verification/host.md#ordinary-host-startup) establish rejection, not a complete recovery experience. | **P2.** Entry choice E1 precedes recovery design; error wording also belongs in [C](#c--read-and-answer-a-story-sequence). [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| E4 · See Map3, move between scenes, follow camera | Field maps, actors, logical camera and scene transitions are **accepted, bounded**. Private field drawing still uses a 320×192 source view scaled into the window. | Modern visible world extent, actor size, UI occupancy and camera relationship are **not implemented as an agreed product treatment**. **Unknown:** target composition. More window pixels alone do not expand the visible world. | **Confirmed structure:** [ExplorationPresentation.Draw](../../game/src/Exploration/ExplorationPresentation.cs), constants `ViewWidth/ViewHeight`; [field projection](../godot/presentation.md#field-projection). Existing camera/plane observations prove their named transforms, not a modern viewport choice. | **P1.** [A](#a--see-the-connected-world-at-the-chosen-baseline), after user viewport/baseline decision; [#438 feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919). |
| E5 · Hold direction, release, lose focus; then use a menu | Continuous mapped field walking is **integrated, experience pending**: fresh step, 220 ms initial delay, 120 ms repeat without catch-up. Ordinary motion waits retain the hold; release/lifecycle/story handoffs stop future steps. | Bounded engineering observations do not establish full opening experience. **Unknown:** physical device release outside the focused window. Choices/menu retain edge input; no original timing claim. | **Confirmed structure and bounded executor observations:** [control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk), covering mapped keys/stick, real native focus transfer and handoffs. Independent attempts encountered external focus loss; completed failures and integration status belong to [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); independent of #689 and viewport choice. |
| E6 · Face an NPC and try to interact | Binding-derived Confirm + Talk appears only for a current legal interaction offer: **integrated, experience pending**. Confirm requeries; Interact rechecks the same target/event predicate. | No fabricated names or new interactions. **Unknown:** broader prompt discoverability and opening-route experience; cue placement follows A once chosen. | **Confirmed structure and bounded engineering observations:** [shared Application query and host observation](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk); tests cover visibility/range/facing/flag/busy changes; executor host observations cover facing/leaving/busy hints and dialogue. Review/integration status belongs to [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); no Search/acquisition. |
| E7 · Inspect party/status or open a field menu | An exploration party/status/inventory menu is **not implemented**. Existing party/session data is **available, not integrated** into that UI. The field view exposes title, dialogue, choice and help. | **Inferred:** users cannot inspect carried health/resources/party through a field menu. **Unknown:** desired information/actions; battle-only equipment/Give/Drop cannot be advertised as field services. | **Confirmed structure:** [field view](../../game/src/Exploration/ExplorationSessionView.cs), [session authority](../application/session-and-programs.md#session-authority); [capability exclusions](../capability-status.md#milestone-and-deferred-surfaces). A menu is not proof that missing services exist. | **P2.** Define a read-only information subset in [E](#e--understand-a-battle-result-and-continue-in-the-field); any new field action needs separate scope. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), wider engine work [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). |

## Dialogue and cutscene matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| D1 · Story dialogue opens with/without a portrait | Text, portrait side/mirror/eye/mouth projection and bound window lifecycle are **accepted, bounded**. A consistent modern window/name/typography treatment is **not integrated**. | No dedicated speaker-name label is created by the field view; resolved member names inside text are a different feature. **Inferred:** hierarchy and speaker attribution may be unclear. **Unknown:** actual clipping/readability across reached lines at the future baseline. | **Confirmed structure:** [ExplorationSessionView.Present](../../game/src/Exploration/ExplorationSessionView.cs), [portrait drawing](../../game/src/Exploration/ExplorationPresentation.cs). [Field text](../verification/field-programs.md#raw-field-text-observation) and [before-battle windows](../verification/field-programs.md#bound-before-battle-windows-observation) retain semantic projection limits. | **P1.** [C](#c--read-and-answer-a-story-sequence), after A's geometry decision. Reuse admitted text/portrait resources; speaker data availability must be checked before adding a nameplate. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| D2 · Reveal text, advance a page, or press Cancel | Instant/adjustable text and separate reveal/acknowledge are **accepted, bounded**. Ordinary dialogue Confirm reveals then advances; Cancel does not acknowledge it. | **Integrated, experience pending:** help says Reveal/Continue, but distinction between reveal, acknowledgement and non-input waits needs readable consistent feedback. **Unknown:** preferred text treatment; do not turn Cancel into an undeclared dialogue skip. | **Confirmed structure and retained runtime boundary:** [field HandleAction/_Process](../../game/src/Exploration/ExplorationSessionView.cs), [native 9A](../verification/host.md#native-9a-observation). Modern text policy is not original timing evidence. | **P1.** [C](#c--read-and-answer-a-story-sequence); existing wait/reveal protocol and startup settings. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| D3 · Yes/No opens, focus changes, confirm/cancel answers | Choice selection, selected marker/alternate color, mapped help and release phases are **accepted, bounded**. | **Integrated, experience pending:** focus, question hierarchy and answer/cancel meaning need user review. Basic mouse selection is absent; a held skip key must not answer a choice. | **Confirmed structure:** field choice panel and `PollChoice` dispatch in [field view](../../game/src/Exploration/ExplorationSessionView.cs). [Choice observations](../verification/field-programs.md#source-bound-choice-observation) retain their settings/branch boundaries; [skip contract](../godot/battle-entry-skip.md#accepted-delivery-contract) excludes every ChoiceWait phase. | **P1.** [C](#c--read-and-answer-a-story-sequence), then [D](#d--choose-existing-menu-options-with-a-mouse). [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); preserve accepted [#1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386). |
| D4 · Opening or tower camera/motion/fade waits | Actual camera/fade/effect consumers and window closing are **accepted, bounded**. Many non-text waits deliberately render empty help. | **Integrated, experience pending:** no consistent player-facing distinction among watching, waiting for input, skipped delivery and failure. **Inferred:** a silent wait can resemble a stall. Earlier opening scenes do not inherit a generic whole-scene skip. | **Confirmed structure:** [field Present wait switch](../../game/src/Exploration/ExplorationSessionView.cs); [before-body/entry observation](../verification/field-programs.md#bound-before-body-and-battle-entry-observation). [White fade](../godot/presentation.md#bound-white-palette-projection) is a modern viewport approximation. Perceived pacing/stall diagnosis is **Unknown** here. | **P1.** [C](#c--read-and-answer-a-story-sequence); actual wait kinds/tokens and logical view remain authoritative. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| D5 · Skip eligible tower before/load/start | **Accepted, bounded:** mapped Cancel (default Escape/X) requests Skip battle intro; Confirm/Enter remains reveal/advance. Skip preserves script work, mounts real resources and clears before ordinary battle advancement. | This is not a remaining missing feature or a request for another skip ticket. Choice/failure stops it; focus loss disarms it and fresh input is required. **Unknown:** physical focus-loss/short-tail experience beyond recorded observations. | **Confirmed accepted implementation/observation:** [skip owner](../godot/battle-entry-skip.md#current-acceptance-boundary), [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386#issuecomment-6462571007), merged [#1387](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1387). Six/eight ObjectDB exit warnings remain **Unknown**, not proven harmless. | **P2, preserve accepted behavior.** C may reuse wording/controls without broadening eligibility. Existing [#1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386); no duplicate implementation slice. |

## Battle and return matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| B1 · First battle input; open the action menu | Compact cardinal diamond, original normal/selected icons, one shared hint and full-map UI overlays are **accepted, bounded** in #689. | User play acceptance at f60ed974 is **Confirmed** by the [acceptance record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454); main-gate completed independent engineering acceptance at a386b66e. Git integration status is owned by [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692). This does not accept the remaining opening UI gaps. | **Confirmed structure at f60ed974:** [BattleCommandMenu](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs), [BattleMenuLayout](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleMenuLayout.cs); subsequent a386 changes only documentation. [#689 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6461883659) retains the earlier visible-entry 150s timeout; later acceptance does not rewrite it. | **Existing bounded slice:** [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) / [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692); no additional requirement from this assessment. |
| B2 · Magic/item list, disabled choice, target, explicit commit, layered Cancel | Semantic offers/targets/reasons, diamond lists and `ReturnToBattleActions` UI are **accepted, bounded** under #689. | Selection, movement preview, resources/RNG and supplementary Equipment/Give/Drop routes are preserved within the accepted scope. User interaction/readability approval and independent engineering acceptance are complete; [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) owns Git integration status. Search, mouse and global layouts remain separate. | **Confirmed structure:** [semantic interface](../godot/presentation.md#semantic-interface), [command menu at f60ed974](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs). [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) owns actual input and unavailable-target/cancel acceptance, including the subsequent independent a386 observations. | **Existing #689 boundary.** D consumes this accepted menu interface; no scope expansion of [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689). |
| B3 · Read current actor, HP/MP, round, target and control state | Roster HP/MP/EXP/accounting, stage text and board selection exist: **integrated, experience pending**. Scene actor HP/MP are also rendered. | Diagnostic actor IDs and internal stages remain visible; no unified player-facing status/hierarchy is established. The user reports the upper-left subject as “alley” rather than a character name (exact displayed spelling is not independently verified). Show the resolved current character name, including supported renamed party members, instead of a diagnostic identifier. **Inferred:** scrolling and competing panels make the active subject/control state harder to read. **Unknown:** actual overlap and disappearance defects, especially after candidate overlays. | **Confirmed structure:** [BattleSnapshotProjection.Project](../../game/src/Battles/BattlePresentation.cs), [BattleSessionView.Present](../../game/src/Battles/BattleSessionView.cs), [BattleSceneView](../../game/src/Battles/BattleSceneView.cs). [User feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) confirms this requested correction; candidate `BattleCommandMenu.ActorText` renders `actor.Value`. Existing geometry observations are not aesthetic approval. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), after A and accepted #689 interface; reuse current snapshot. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B4 · Action, damage/healing, sleep/death and control release | Close-up messages, real reveal/acknowledgement, finite scene/movement and ordered field-death removal are **accepted, bounded** consumers. | **Integrated, experience pending:** information ordering, message occlusion, removal timing and readiness cues need a bounded experience check. A generic status UI is not established; authored poison and other capability-specific notices do not prove private Battle01 status coverage. | **Confirmed structure:** [BattleSceneView.Message/Consume/Acknowledge](../../game/src/Battles/BattleSceneView.cs); [physical scene](../godot/battle-scenes.md#physical-battle-scenes), [death batch](../godot/battle-scenes.md#battlefield-death-batches) and [capability limits](../capability-status.md). Original timing/per-character font fallback remain **Unknown**. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), existing scene/round/input ownership; no new statuses or scene rules. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B5 · Receive EXP/gold/level/stat/spell messages | Action reward/growth message phases are **accepted, bounded**; a unified post-battle summary is **not implemented**. | **Unknown:** whether a separate summary is wanted. **Inferred:** message-by-message rewards may be hard to relate to retained party state. Do not count all battle rewards again on return or treat direct-reader growth Unsupported failures as a whole-engine absence. | **Confirmed structure:** [BattleSceneView.Message](../../game/src/Battles/BattleSceneView.cs). [Private milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone) and [font/operand observation limits](../verification/battles.md#battle-message-operand-and-font-observation) distinguish connected growth from a limited direct start. | **P2.** [E](#e--understand-a-battle-result-and-continue-in-the-field), decide information treatment before a summary; current progression remains authoritative. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B6 · Read active actor HP/MP | Numeric HP/MP are integrated; the candidate actor panel uses text. | User requests image-based HP/MP gauges using original-style resources. Resolve the admitted gauge assets and represent current/max values consistently; numeric text alone is not this treatment. | **Confirmed structure:** candidate [BattleCommandMenu.ActorText](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs). [User requirement](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454); exact asset mapping and original animation behavior are **Unknown**, not new original-game findings. | **P2.** B3 / slice E, under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). Reuse admitted resources and current/max snapshot values; retain the private asset boundary. |
| B7 · Change diamond command selection | Candidate normal/selected images provide a static selection state; animated focus feedback remains a follow-up. | User reports no selection animation. Add original-style selection feedback without moving the compact cardinal layout or delaying/duplicating semantic input. | **Confirmed:** [user-observed gap](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) at f60ed974; exact original animation frames/cadence are **Unknown** pending the existing resource/research owners. Static selected images do not establish animated behavior. | **P2.** Separate bounded polish after accepted #689, under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); respect reduced-flash settings and resource provenance. Not a new #689 acceptance condition. |
| B8 · Select an actor and preview movement | Existing movement/preview legality is available; the user reports no visible movement range in the reviewed play. | Show reachable tiles for the current actor and phase, update after relevant state changes, and remove the overlay when leaving movement selection. A cursor/preview position is not a reachable-area display. | **Confirmed:** [user report](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) of the missing affordance. **Unknown:** complete coverage of other fixtures and whether a reusable range projection already exists; inspect it before implementation. This is not a claim that movement legality is absent. | **P1.** Separate tactical movement-visibility slice under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), after A's geometry and accepted #689. Consume authoritative terrain/occupancy/movement rules; no new movement rules or world-click navigation. |
| R1 · Win Battle01, close battle/story windows, return to field | Genuine outcome/return programs, fresh logical view/closed windows and same-session usable movement are **accepted, bounded**. | **Integrated, experience pending:** no finished result-to-exploration information treatment is established. **Unknown:** whether the player can clearly identify success, changed party/resources and regained control without diagnostic help. | **Confirmed retained boundary:** [outcome projection](../godot/presentation.md#bound-outcome-projection-lifecycle), [victory/return observation](../verification/field-programs.md#bound-victory-and-return-observation), [ordinary host wiring](../development-and-verification.md#ordinary-host-lifecycle-observation). These do not establish human-play approval. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), with C's dialogue components; use the existing return seam. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| R2 · Lose or request retreat | **Confirmed retained evidence:** ordinary connected defeat has bounded return evidence. Selected Battle02 EGRESS/Angel Wing has a separate consumer; arbitrary Battle01 retreat is **not established**. | **Unknown:** player-facing defeat/retry/save expectations and Battle01 retreat scope. Returning to a controlled savepoint is not a delivered Continue/reload UI. Do not make wider retreat or later chapters an opening-UI requirement. | [Defeat/route observations](../verification/field-programs.md#battle01-through-battle02) and [retained failures](../verification/field-programs.md#retained-candidate-results-and-open-acceptance) retain their exact boundaries; [BattleRetreatView](../../game/src/Battles/BattleRetreatView.cs) is **Confirmed structure** for admitted retreat, not general availability. | **P2.** E reviews supported defeat only; entry/retry and unsupported retreat require explicit scope first. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369), [#1370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370). |

## Cross-scene consistency matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| X1 · Compare field, dialogue, battle and return at one size | Project config uses a resizable 960×540 viewport/window default; #689 uses a 1280×720 review window. These are **Confirmed structure/conditions**, not a final baseline choice. | A single product resolution and modern world composition remain **Unknown**. [User review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) identifies inconsistent battle/exploration viewports; acceptance must compare world extent, actor scale and framing across that transition, not only confirm that the battle menu overlays its map. Old 960×540 design proposals, 640×360 world candidate and five-size candidate checks do not select or require future multi-resolution support. | [project.godot](../../game/project.godot), [layout policy](../godot/presentation.md#presentation-policy-and-current-layout), [user feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919). | **P1, decision before layout work.** [A](#a--see-the-connected-world-at-the-chosen-baseline), [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). #689 remains limited to its own review window. |
| X2 · Reuse windows, fonts, borders, focus and hints | Separate field labels/choice panels and battle presenters are integrated. Asset specifications/tooling are **available, not integrated as a complete ordinary-host UI kit**. Candidate battle frames do not change this. | **Inferred:** inconsistent hierarchy and duplicated layout work. A reusable component/theme treatment is requested, but full visual direction and typography are **Unknown**. Do not infer a deployed kit from private SVG/font files. | **Confirmed structure:** [field view](../../game/src/Exploration/ExplorationSessionView.cs), [battle view](../../game/src/Battles/BattleSessionView.cs). [Asset distribution/design limit](../content/presentation-assets.md#product-design-and-distribution-limit) explicitly bounds proposed SVG/theme/glyph assets. | **P2.** C extracts only components its story/choice consumer needs, after A; E/D reuse them where justified. [#438 feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919). No new framework mandated. |
| X3 · Switch keyboard and basic menu mouse use | Binding-derived keyboard hints/input are **accepted, bounded**. Semantic mouse hover/click/confirm/back is **not implemented**; a diagnostic scroll container is not menu mouse support. | **Inferred:** visible menu items invite clicks that cannot select actions. **Unknown:** desired hover versus commit convention and back affordance. World click movement and complex tactical targeting are not implicit. | **Confirmed structure:** [GameInput.Resolve](../../game/src/Input/GameInput.cs) accepts key/joy events; field choice and candidate command controls use ignored mouse filtering. [Native 9A](../verification/host.md#native-9a-observation) covers keyboard/gamepad only; mouse absence here is a source-structure finding. | **P2.** [D](#d--choose-existing-menu-options-with-a-mouse), after accepted #689 and selected C surfaces. [#438 feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919). |
| X4 · Invalid/disabled command or adapter failure during any scene | Typed failures, semantic Enabled/Reason and pending-state retention exist: **accepted, bounded** interfaces/consumers. | **Integrated, experience pending:** distinguish unavailable choice, rejected input and stopped presentation with consistent readable wording and recovery affordance. **Unknown:** overlap/legibility; a generic technical error label is not a complete recovery UX. | **Confirmed structure:** [semantic interface](../godot/presentation.md#semantic-interface), field `TransitionFailure`, battle status/startup error. [Host failure witness](../development-and-verification.md#ordinary-host-lifecycle-observation) observes a visible stopped program, not generic recoverability. | **P1.** C handles field/story wording; #689 keeps its own unavailable reasons; E handles battle/result readiness. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689). |

## User feedback disposition and existing acceptance

The [2026-10-28 user feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919)
is remake product intent, not evidence about original SF2:

| Request | Matrix / disposition |
| --- | --- |
| Continuous directions, release/focus stop; distinct menu cadence | E5/E6 → B; #1389 implements and observes the bounded control/hint behavior. Physical-device and whole-opening experience limits remain explicit. |
| Choose one resolution, then modern world viewport/camera | E4/X1 → A; 1280×720 remains #689's review condition. No baseline selected here; multi-resolution support is deferred. |
| Systematic UI used in exploration and cutscenes | E7/D1–D4/X2/X4 → C, then E; existing assets/interfaces are inputs, not a finished kit. |
| Basic mouse for existing menus | D3/X3 → D; menu scope and commit/back convention first; no world-click movement promise. |
| Skip battle-entry scene; Enter did not skip | D5 → existing #1386, independently accepted via #1387. The earlier missing-skip report is superseded only within its explicit before/load/start boundary. |

[#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) remains only the already-made
battle command UI: cardinal placement, direct original icons, compact shared selection hint, keyboard
lists/targets/commit/cancel, resource preservation and actual operation at its review window. The
candidate's full-map overlay is not an accepted global world-viewport design. The user accepted
f60ed974 in [human play review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454),
and main-gate completed independent engineering acceptance at a386b66e.
[PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) is authoritative for Git integration
status; neither user nor engineering approval alone asserts that the merge has occurred. B3/B6–B8 and X1 retain the reported
follow-up gaps without reopening this bounded approval. This assessment, exploration
redesign, fixed resolution, continuous walking, mouse and generic UI reuse are not hidden completion
conditions for #689. Accepted intro skip removes a scheduling dependency for its observations; it
does not erase the earlier 150-second visible-entry timeout. The later skip-assisted visible-window
observation passed in 3,751 ms at f60ed974; human approval is recorded separately above, without
claiming natural full-opening timing or whole-game UI acceptance.

## Proposed independent follow-up slices

These are proposals for main-gate prioritization under #438, not newly created/dispatched Issues.
Keep #689 and #1386 as their existing owners. Do not open duplicate tickets. A product decision about
entry/settings/persistence remains outside these five proposals; link #1369 when it is actually needed.

### A · See the connected world at the chosen baseline

**P1; E4/X1.** First obtain the user's single resolution, visible-world/actor-size and camera/UI
occupancy choice; this assessment selects none. Implement only that accepted treatment through field,
dialogue, battle entry and return, consuming existing logical camera/layout state. Acceptance: actual
viewport and node/draw geometry match the chosen composition; moving/talking/transferring and opening
panels preserve intended framing and control; UI changes do not alter collision or gameplay positions.
Check one selected baseline, not a new multi-resolution matrix. #438 owns the decision; accepted #689
is consumed if battle layout is touched, without reopening its original acceptance scope.

### B · Walk to a character, talk and regain control

**P1; E5/E6; integrated, experience pending.** Continuous mapped walking and contextual Talk use
the existing Move/Interact/session authority. Release, focus/hide/pause, interaction, program and
map/battle handoffs cancel future repeat; a normal Move wait retains it. The
[control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk)
records cadence, direction priority, session tests and finite native reproduction. Bounded executor
observations exercised remapped keys/stick, obstruction, native focus, dialogue, map transfer and
fresh battle menu input. Independent attempts encountered external focus loss; the observer must
report an unavailable environment rather than continue input and produce cascading product failures.
Completed results and integration status belong to
[PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390).

Physical device release while unfocused, broader prompt discoverability and opening experience are
Unknown; injected events do not close those claims. No new interactions, input framework, original
timing or visual baseline was selected. Cue placement follows A when accepted, and #689's separate
follow-ups and historical results remain preserved.

### C · Read and answer a story sequence

**P1 behavior, P2 component consistency; D1–D4/X2/X4.** After A's geometry decision, apply the minimum
reusable window/text/focus/hint treatment to real opening dialogue, a Yes/No question, a non-input
transition and returned control. Reuse accepted text/portrait/wait services and actual bindings.
Acceptance: speaker/portrait attribution where supported, full line visibility, instant and reveal
behavior, focus/answer/Cancel meaning, window closing and non-input/failure feedback are readable;
no acknowledgement occurs from elapsed display alone. Preserve choice-stop and fresh-request skip
semantics. Use state/input/projection/error observations and user review without screenshots. Extract
only components used by these surfaces; do not require a complete global theme/catalog first.

### D · Choose existing menu options with a mouse

**P2; D3/X3.** Depends on an accepted #689 menu interface and the explicitly selected C choice surfaces.
Settle hover/focus, click selection versus confirmation, back and visible feedback before coding.
Acceptance: mouse and keyboard interleave on main commands, an existing list, disabled choices and
back/confirm; each physical event submits at most one semantic command, never a click-through action
in a newly attached scene. Preserve existing legality/resources and keyboard access. No title menu,
world-click movement, new targeting system or controller expansion is included.

### E · Understand a battle result and continue in the field

**P1 control/result clarity; P2 optional information treatment; E7/B3–B6/R1/R2/X4.** After A and accepted
#689, reuse C where useful to present the active actor/HP/MP/control state, existing action/death/reward
messages and outcome-to-field handoff. Show resolved character names and image-based HP/MP gauges
from admitted resources, with current/max values from the session. Define a small read-only returned-party information surface;
any new summary or field actions require an explicit product choice. Acceptance: ordinary Battle01
actions and existing messages remain visible in the correct order, unavailable versus busy/error
states are distinguishable, windows disappear at actual completion, a supported win and ordinary
defeat each return to clear usable field control, and carried resources are shown without duplicate
rewards or invented values. Use existing compact evidence where unchanged; observe only altered
surfaces. Wider retreat, save/retry, new statuses and later chapters are excluded.

Recommended dependency order is **finish #689 independently; decide A; B can proceed in parallel with
that decision; then C; D and E consume the accepted surfaces they need**. This is ordering advice,
not authorization for concurrent writers or a requirement to create all five tickets now. B7 animation
and B8 movement-range visibility are additional bounded follow-ups, not additions to #689 or one
expanded result-to-field implementation. Each
implementation needs its own exact paths, proportional evidence and independent acceptance.

## Evidence limits and assessment verification

The original #1385 static assessment inspected its declared accepted source baseline, linked
capability/presentation/assets/verification owners, the pinned PR692 candidate and Issue handoffs.
That assessment launched no SDK, Godot, emulator,
reference replay, acquisition or screenshot workflow. Static findings establish structure; inferred
impacts and unknown experience remain labeled. Existing accepted milestone/comparison evidence keeps
its own semantics, retained failed runs and Unknowns; this document does not replace those owners.

The later #689 update records user play approval and independently reported main-gate engineering
acceptance; these observations belong to #689/PR692, not to the original static assessment task.
Main-gate reviewed a386b66e without a production-code blocker: 34 BattleActionChoiceTests passed,
Release adapter and Debug builds had no warnings/errors, and ordinary-host menu/private original-icon
observations passed in 6,707/2,391 ms with clean logs. All applicable checks in
[CI40783218819](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/40783218819) passed.
This consistency correction changes only this owner and runs document/scope/private-boundary checks;
it adds no SDK, Godot, replay or original-game observation. B3/B6–B8/X1 retain their future scope.

In particular, preserve the UI visible-entry timeout; #1386's six/eight ObjectDB exit warnings and
unobserved physical focus/speech-tail boundaries; limited direct-reader growth failures; and existing
route/comparison failures. No claim that all opening paths fail, all status effects work, every
window is readable, or native exit is universally clean follows from this inventory. Later matching
evidence may narrow a specific Unknown; source existence and green CI alone cannot do so.

For the original assessment, verification covered its two owned paths and Markdown links, anchors,
tables, fences, source references and private boundary. The later #689 correction owns only this
file: verify the diff scope, links/table structure, unchanged future-gap rows and `git diff --check`.
Record exact Git identity and actual CI in the Issue/PR handoff; PR-level CI may select implementation
jobs because PR692 also contains the battle UI. Experience confirmation of the remaining gaps,
implementation ticket creation and product-direction selection remain outside this assessment's
completion claim. Git integration is tracked by PR692 rather than a static pending-state assertion.
