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
implementing or experiencing every gap. This assessment owns the inventory and routing; separately dispatched slices own their production
changes and linked presentation/asset/verification acceptance.

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
| E4 · See Map3, move between scenes, follow camera | **Implemented, bounded observation:** shared 1920×1080 viewport, 96px cells/ordinary frames and 4× nearest sampling. Logical state remains authoritative; modern dead-zone/story panning maps it into the larger frame. | Small legal rooms center on exterior color; packed-area transfers reframe in their existing transition. Original hardware composition and broader aesthetic experience remain **Unknown**. | [Unified viewport agreement](../godot/presentation.md#unified-world-viewport-agreement) and [native recipe](../verification/host.md#unified-world-viewport-observation) cover private room/boundary, teacher-wall ink repair, map transition and battle handoffs. | **P1; A/E4/X1 implemented by [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391); [integration status](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).** C/D/E and the remaining gaps keep their own scopes. |
| E5 · Hold direction, release, lose focus; then use a menu | Continuous mapped field walking is **integrated, experience pending**: fresh step, 220 ms initial delay, 120 ms repeat without catch-up. Ordinary motion waits retain the hold; release/lifecycle/story handoffs stop future steps. | Bounded engineering observations do not establish full opening experience. **Unknown:** physical device release outside the focused window. Choices/menu retain edge input; no original timing claim. | **Confirmed structure and bounded executor observations:** [control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk), covering mapped keys/stick, real native focus transfer and handoffs. Independent attempts encountered external focus loss; completed failures and integration status belong to [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); independent of #689 and viewport choice. |
| E6 · Face an NPC and try to interact | Binding-derived Confirm + Talk appears only for a current legal interaction offer: **integrated, experience pending**. Confirm requeries; Interact rechecks the same target/event predicate. | No fabricated names or new interactions. **Unknown:** broader prompt discoverability and opening-route experience; cue placement follows A once chosen. | **Confirmed structure and bounded engineering observations:** [shared Application query and host observation](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk); tests cover visibility/range/facing/flag/busy changes; executor host observations cover facing/leaving/busy hints and dialogue. Review/integration status belongs to [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); no Search/acquisition. |
| E7 · Inspect party/status or open a field menu | An exploration party/status/inventory menu is **not implemented**. Existing party/session data is **available, not integrated** into that UI. The field view exposes title, dialogue, choice and help. | **Inferred:** users cannot inspect carried health/resources/party through a field menu. **Unknown:** desired information/actions; battle-only equipment/Give/Drop cannot be advertised as field services. | **Confirmed structure:** [field view](../../game/src/Exploration/ExplorationSessionView.cs), [session authority](../application/session-and-programs.md#session-authority); [capability exclusions](../capability-status.md#milestone-and-deferred-surfaces). A menu is not proof that missing services exist. | **P2.** Define a read-only information subset in [E](#e--understand-a-battle-result-and-continue-in-the-field); any new field action needs separate scope. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), wider engine work [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). |

## Dialogue and cutscene matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| D1 · Story dialogue opens with/without a portrait | **Implemented, bounded executor observation:** shared 1440px blue/pale-gold panels, 30px text, measured expansion/reading pages, real portrait side/mirror/eye/mouth and window lifecycle. | Names are intentionally absent: current admitted entity metadata has no reliable speaker-name mapping. Member names inside text remain separate. Full-game readability/final aesthetics remain **Unknown**. | [Current treatment](../godot/presentation.md#shared-story-dialogue-agreement), [private Zone8 and authored native bounds](../verification/host.md#shared-story-dialogue-observation). Real portrait is a child above the background; no inferred visibility from rect alone. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Independent integration belongs to its PR; no new source-fidelity claim. |
| D2 · Reveal text, advance a page, or press Cancel | **Implemented, bounded observation:** contextual Reveal, Read more and eligible Continue; full shaped layout; Confirm reveal-all, later fresh acknowledgement, instant mode and held/echo protection. Ordinary Cancel does not skip dialogue. | No text is shrunk or silently clipped. Screen-sized content uses display-only pages before acknowledgement/delivery; original typewriting timing and whole-opening experience remain **Unknown**. | [Field controller](../../game/src/Exploration/ExplorationSessionView.cs), [shared panel](../../game/src/UI/StoryDialoguePanel.cs), [70-line/repeated-window native cases](../verification/host.md#shared-story-dialogue-observation). Existing speech/confirmation authority is retained. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Broader text/settings/history/autoplay are excluded. |
| D3 · Yes/No opens, focus changes, confirm/cancel answers | **Implemented, bounded observation:** question with attached focused options, measured actual binding hints, source default Yes/Cancel=No and entry/release phases. Authored choices retain direct Yes/No semantics. | Basic mouse selection remains absent; no fabricated source-choice phase or answer from held skip. Broader human/full-game experience remains **Unknown**. | [Source No/Yes and authored cases](../verification/host.md#shared-story-dialogue-observation), [retained choice evidence](../verification/field-programs.md#source-bound-choice-observation), [skip exclusion](../godot/battle-entry-skip.md#accepted-delivery-contract). | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Mouse remains [D](#d--choose-existing-menu-options-with-a-mouse). |
| D4 · Opening or tower camera/motion/fade waits | **Implemented, bounded observation:** quiet scene-in-progress feedback; Continue only at eligible input; existing entry Skip only when eligible; readable stopped wording preserves diagnostics/state. | No ordinary-wait timeout, general scene skip or fabricated retry. Whole-opening perceived pacing and stall diagnosis remain **Unknown**. | [Current presentation](../godot/presentation.md#shared-story-dialogue-agreement), [automatic/failure/battle-story/skip cases](../verification/host.md#shared-story-dialogue-observation). Existing camera/fade/wait consumers remain authoritative. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Original timing and broader UX remain outside this boundary. |
| D5 · Skip eligible tower before/load/start | **Accepted, bounded:** mapped Cancel (default Escape/X) requests Skip battle intro; Confirm/Enter remains reveal/advance. Skip preserves script work, mounts real resources and clears before ordinary battle advancement. | This is not a remaining missing feature or a request for another skip ticket. Choice/failure stops it; focus loss disarms it and fresh input is required. **Unknown:** physical focus-loss/short-tail experience beyond recorded observations. | **Confirmed accepted implementation/observation:** [skip owner](../godot/battle-entry-skip.md#current-acceptance-boundary), [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386#issuecomment-6462571007), merged [#1387](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1387). Six/eight ObjectDB exit warnings remain **Unknown**, not proven harmless. | **P2, preserve accepted behavior.** C may reuse wording/controls without broadening eligibility. Existing [#1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386); no duplicate implementation slice. |

## Battle and return matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| B1 · First battle input; open the action menu | Compact cardinal diamond, original normal/selected icons, one shared hint and full-map UI overlays are **accepted, bounded** in #689. | User play acceptance at f60ed974 is **Confirmed** by the [acceptance record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454); main-gate completed independent engineering acceptance at a386b66e. Git integration status is owned by [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692). This does not accept the remaining opening UI gaps. | **Confirmed structure at f60ed974:** [BattleCommandMenu](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs), [BattleMenuLayout](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleMenuLayout.cs); subsequent a386 changes only documentation. [#689 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6461883659) retains the earlier visible-entry 150s timeout; later acceptance does not rewrite it. | **Existing bounded slice:** [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) / [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692); no additional requirement from this assessment. |
| B2 · Magic/item list, disabled choice, target, explicit commit, layered Cancel | Semantic offers/targets/reasons, diamond lists and `ReturnToBattleActions` UI are **accepted, bounded** under #689. | Selection, movement preview, resources/RNG and supplementary Equipment/Give/Drop routes are preserved within the accepted scope. User interaction/readability approval and independent engineering acceptance are complete; [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) owns Git integration status. Search, mouse and global layouts remain separate. | **Confirmed structure:** [semantic interface](../godot/presentation.md#semantic-interface), [command menu at f60ed974](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs). [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) owns actual input and unavailable-target/cancel acceptance, including the subsequent independent a386 observations. | **Existing #689 boundary.** D consumes this accepted menu interface; no scope expansion of [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689). |
| B3 · Read current actor, HP/MP, round, target and control state | **Implemented, bounded executor observation:** shared readable-name HP/MP cards in command, target and action scenes; retained round/gold context and diagnostic observations. | Admitted member/enemy names replace raw IDs on these surfaces; public content has an intentional readable identity fallback. Broader result/field information and final aesthetics remain **Unknown**. | [E1 agreement](../godot/presentation.md#battle-status-card-agreement), [native card/phase observations](../verification/host.md#battle-status-card-observation). Scope includes complete long names, target Cancel, actor refresh and unchanged world/menu/range seams. | **P1; E1 / [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).** Independent acceptance/integration remains main-gate owned; E2 follows its merge. |
| B4 · Action, damage/healing, sleep/death and control release | Close-up messages, real reveal/acknowledgement, finite scene/movement and ordered field-death removal are **accepted, bounded** consumers. | **Integrated, experience pending:** information ordering, message occlusion, removal timing and readiness cues need a bounded experience check. A generic status UI is not established; authored poison and other capability-specific notices do not prove private Battle01 status coverage. | **Confirmed structure:** [BattleSceneView.Message/Consume/Acknowledge](../../game/src/Battles/BattleSceneView.cs); [physical scene](../godot/battle-scenes.md#physical-battle-scenes), [death batch](../godot/battle-scenes.md#battlefield-death-batches) and [capability limits](../capability-status.md). Original timing/per-character font fallback remain **Unknown**. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), existing scene/round/input ownership; no new statuses or scene rules. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B5 · Receive EXP/gold/level/stat/spell messages | Action reward/growth message phases are **accepted, bounded**; a unified post-battle summary is **not implemented**. | **Unknown:** whether a separate summary is wanted. **Inferred:** message-by-message rewards may be hard to relate to retained party state. Do not count all battle rewards again on return or treat direct-reader growth Unsupported failures as a whole-engine absence. | **Confirmed structure:** [BattleSceneView.Message](../../game/src/Battles/BattleSceneView.cs). [Private milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone) and [font/operand observation limits](../verification/battles.md#battle-message-operand-and-font-observation) distinguish connected growth from a limited direct start. | **P2.** [E](#e--understand-a-battle-result-and-continue-in-the-field), decide information treatment before a summary; current progression remains authoritative. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B6 · Read active actor HP/MP | **Implemented, bounded executor observation:** one image frame/track/fill component plus current/max numbers serves command, target and scene cards. | No admitted private gauge was found; minimal project-authored CC0 PNG segments supply the treatment. Zero-max MP has a dash; low/dead HP has text cues. Original gauge art/timing and broader aesthetic approval remain **Unknown**. | [Asset selection](../content/presentation-assets.md#public-authored-battle-gauges), [native ratio/damage/heal/MP phases](../verification/host.md#battle-status-card-observation). Candidate SVG meters are not runtime integration. | **P2; E1 / [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).** No easing, gameplay-state copy or progression-blocking bar animation. |
| B7 · Change diamond command selection | Candidate normal/selected images provide a static selection state; animated focus feedback remains a follow-up. | User reports no selection animation. Add original-style selection feedback without moving the compact cardinal layout or delaying/duplicating semantic input. | **Confirmed:** [user-observed gap](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) at f60ed974; exact original animation frames/cadence are **Unknown** pending the existing resource/research owners. Static selected images do not establish animated behavior. | **P2.** Separate bounded polish after accepted #689, under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); respect reduced-flash settings and resource provenance. Not a new #689 acceptance condition. |
| B8 · Select an actor and preview movement | **Implemented, bounded headless/windowed observation:** authoritative legal destinations use a translucent cyan board layer at the accepted 96px scale. | Range stays at committed origin through preview/return delivery; menus, targeting, automatic turns, scenes, story/outcome and field return clear it. Living allies are traversable but excluded as stops. | **Confirmed:** engine behavior checks and actual mapped-input/draw observations cover authored occupancy/budget, actor refresh, automatic movement, outcome and private Battle02 region(16,19)/Egress. [Agreement](../godot/presentation.md#battle-movement-range-agreement); [reproduction/limits](../verification/host.md#battle-movement-range-observation). Independent windowed checks passed; broader fixture coverage remains **Unknown**. The initial nonreproduced Cancel-sequence failure and private headless shutdown warnings are retained. | **P1; [#1395](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1395)** under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). No new movement rules, world-click input or original-pixel claim. |
| R1 · Win Battle01, close battle/story windows, return to field | Genuine outcome/return programs, fresh logical view/closed windows and same-session usable movement are **accepted, bounded**. | **Integrated, experience pending:** no finished result-to-exploration information treatment is established. **Unknown:** whether the player can clearly identify success, changed party/resources and regained control without diagnostic help. | **Confirmed retained boundary:** [outcome projection](../godot/presentation.md#bound-outcome-projection-lifecycle), [victory/return observation](../verification/field-programs.md#bound-victory-and-return-observation), [ordinary host wiring](../development-and-verification.md#ordinary-host-lifecycle-observation). These do not establish human-play approval. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), with C's dialogue components; use the existing return seam. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| R2 · Lose or request retreat | **Confirmed retained evidence:** ordinary connected defeat has bounded return evidence. Selected Battle02 EGRESS/Angel Wing has a separate consumer; arbitrary Battle01 retreat is **not established**. | **Unknown:** player-facing defeat/retry/save expectations and Battle01 retreat scope. Returning to a controlled savepoint is not a delivered Continue/reload UI. Do not make wider retreat or later chapters an opening-UI requirement. | [Defeat/route observations](../verification/field-programs.md#battle01-through-battle02) and [retained failures](../verification/field-programs.md#retained-candidate-results-and-open-acceptance) retain their exact boundaries; [BattleRetreatView](../../game/src/Battles/BattleRetreatView.cs) is **Confirmed structure** for admitted retreat, not general availability. | **P2.** E reviews supported defeat only; entry/retry and unsupported retreat require explicit scope first. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369), [#1370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370). |

## Cross-scene consistency matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| X1 · Compare field, dialogue, battle and return at one size | **Implemented, bounded observation:** the user-selected 1920×1080 baseline shares one world rectangle and 96px actor/cell scale across exploration and battle. UI overlays do not change zoom or world rect. | Movement/targets pan at fixed zoom; corner panels avoid an edge-clamped focus. Private Battle01 entry and nonzero-region Battle02 Egress/field return are observed. Multi-resolution settings, free zoom and full UI theming remain out of scope. | [project.godot](../../game/project.godot), [layout agreement](../godot/presentation.md#unified-world-viewport-agreement), [native geometry/input limits](../verification/host.md#unified-world-viewport-observation). Original timing and global aesthetic approval are **Unknown**. | **P1; [A](#a--see-the-connected-world-at-the-chosen-baseline), [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391); [integration status](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).** #689 retains its earlier review-size evidence. |
| X2 · Reuse windows, fonts, borders, focus and hints | **Implemented for actual C consumers:** shared native Godot Theme, dialogue panel, choice row and contextual hint; existing battle menu frames consume the same style. | This is a bounded story/choice treatment, not a finished global UI kit. Broader typography/art direction, D/E migration and all-map aesthetics remain **Unknown**. | [Shared components/current owner](../godot/presentation.md#shared-story-dialogue-agreement), [finite native recipe](../verification/host.md#shared-story-dialogue-observation). No new fonts/private SVG assets or framework. | **P2; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** E/D reuse only where justified. |
| X3 · Switch keyboard and basic menu mouse use | Binding-derived keyboard hints/input are **accepted, bounded**. Semantic mouse hover/click/confirm/back is **not implemented**; a diagnostic scroll container is not menu mouse support. | **Inferred:** visible menu items invite clicks that cannot select actions. **Unknown:** desired hover versus commit convention and back affordance. World click movement and complex tactical targeting are not implicit. | **Confirmed structure:** [GameInput.Resolve](../../game/src/Input/GameInput.cs) accepts key/joy events; field choice and candidate command controls use ignored mouse filtering. [Native 9A](../verification/host.md#native-9a-observation) covers keyboard/gamepad only; mouse absence here is a source-structure finding. | **P2.** [D](#d--choose-existing-menu-options-with-a-mouse), after accepted #689 and selected C surfaces. [#438 feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919). |
| X4 · Invalid/disabled command or adapter failure during any scene | Field/story stopped presentation now uses the shared readable frame above fades, retained message/code and pending state; no invented retry. Existing semantic Enabled/Reason and battle/startup diagnostics remain. | **Bounded C disposition:** a controlled field failure is observed; global battle/result recovery UX and generic recoverability remain **Unknown**. | [Failure/current owner](../godot/presentation.md#shared-story-dialogue-agreement), [controlled failure witness](../verification/host.md#shared-story-dialogue-observation), [semantic interface](../godot/presentation.md#semantic-interface). | **P1.** C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393) handles field/story; #689/E retain their own reasons and result boundaries. |

## User feedback disposition and existing acceptance

The [2026-10-28 user feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919)
is remake product intent, not evidence about original SF2:

| Request | Matrix / disposition |
| --- | --- |
| Continuous directions, release/focus stop; distinct menu cadence | E5/E6 → B; #1389 implements and observes the bounded control/hint behavior. Physical-device and whole-opening experience limits remain explicit. |
| Choose one resolution, then modern world viewport/camera | E4/X1 → A/#1391: user selected 1920×1080,96px cells/ordinary frames, common scale, fixed zoom/panning and UI overlays. Bounded implementation/observation is recorded below; multi-resolution support remains deferred. |
| Systematic UI used in exploration and cutscenes | E7/D1–D4/X2/X4 → C/#1393 implements the bounded shared story treatment; E remains separate. Existing assets/interfaces do not establish a global UI kit. |
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

**P1; E4/X1; implemented with bounded observation; [integration status](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).**
The user approved 1920×1080,96px cells/ordinary sprite frames, common world scale, fixed zoom/panning
and independent UI overlays in [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391).
The [presentation owner](../godot/presentation.md#unified-world-viewport-agreement) defines units,
logical/display mapping, legal bounds and handoff. The [native recipe](../verification/host.md#unified-world-viewport-observation)
checks actual private drawing and inputs at this one baseline. No gameplay/Content/research changes,
new asset copies, free zoom or multi-resolution matrix are included. Broader story components,
mouse, result/status UI and B3/B6–B8 remain with C/D/E/their separate slices.

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

**P1 behavior, P2 component consistency; D1–D4/X2/X4; implemented with bounded executor
observations under [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).**
The approved [shared story treatment](../godot/presentation.md#shared-story-dialogue-agreement)
uses A's accepted geometry and existing text/portrait/choice/wait authorities. The
[finite recipe](../verification/host.md#shared-story-dialogue-observation) covers real Zone8 text,
portrait layering, source Yes/No/default/release, authored reveal/instant/automatic return,
long/repeated text reading, stable top placement, failure and existing battle-story/skip consumers.
No reliable speaker-name mapping is admitted, so names are omitted. Mouse, result/party UI,
global fonts/art/settings and original timing remain excluded. Independent review/integration
status belongs to the Issue's PR; broader aesthetic and all-map acceptance remain Unknown.

### D · Choose existing menu options with a mouse

**P2; D3/X3.** Depends on an accepted #689 menu interface and the explicitly selected C choice surfaces.
Settle hover/focus, click selection versus confirmation, back and visible feedback before coding.
Acceptance: mouse and keyboard interleave on main commands, an existing list, disabled choices and
back/confirm; each physical event submits at most one semantic command, never a click-through action
in a newly attached scene. Preserve existing legality/resources and keyboard access. No title menu,
world-click movement, new targeting system or controller expansion is included.

### E · Understand a battle result and continue in the field

**P1 control/result clarity; P2 optional information treatment; E7/B3–B6/R1/R2/X4.** E is
split into two user-approved outcomes. [E1 / #1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397)
implements shared battle names and image-based current/max HP/MP cards after accepted A/C/B8/#689.
Its [presentation agreement](../godot/presentation.md#battle-status-card-agreement) and
[native recipe](../verification/host.md#battle-status-card-observation) own the actual scope, assets,
phase timing and bounded executor evidence; independent acceptance/merge remains main-gate owned.

[E2 / #1398](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1398) waits for E1's independent
merge. It owns the separately dispatched result/return and read-only returned-party information
surface, reusing the accepted shared card. Existing rewards must not be counted twice. A supported
win and ordinary defeat must return clear usable field control without invented values. A new
summary or field action needs its product scope settled there. E1 does not claim this result/return
outcome complete. Wider retreat, saves/retry, new statuses and later chapters remain excluded.

Recommended dependency order is **consume accepted #689; integrate A/#1391; B can proceed in parallel with
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
