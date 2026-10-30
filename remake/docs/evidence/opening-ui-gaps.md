# Opening UI Gap Assessment

The connected private opening is playable from a controlled Map3 start through story/exploration,
tower entry, Battle01 and usable return. That acceptance does not establish a complete product
opening or a finished UI. The remaining product gaps include entry/settings/persistence UX, field inventory actions,
and whole-opening human/aesthetic approval. A–E now have accepted bounded deliveries: world scale,
walking/Talk, story presentation, menu mouse input, battle status/messages/return and read-only Party
Status. Their engineering acceptance does not establish full-game or all-device experience.

## Scope and reading rules

**Standard rigor:** this assessment supports prioritization under
[#1385](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1385) and
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). It owns the gap matrix and
bounded acceptance slices. Implementation truth stays with [architecture](../architecture.md),
[capability status](../capability-status.md), [presentation](../godot/presentation.md),
[assets](../content/presentation-assets.md) and [verification scope](../development-and-verification.md#scope).
Cross-scene dependencies and concurrent candidate acceptance require explicit evidence and ownership boundaries; this bounded prioritization needs no new runtime observation. Do not copy this backlog into those owners.

Current delivery status is calibrated against accepted main
[`30a688b920c69be41240588d9f55f90657f35efd`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/30a688b920c69be41240588d9f55f90657f35efd).
The original #1385 static assessment used
[`5138b532c4d4ff84dd13ca97d5a02892448f0e69`](https://github.com/FrankHZ/md-sf2-reverse-engineering/commit/5138b532c4d4ff84dd13ca97d5a02892448f0e69);
its findings and historical observations retain their own boundaries. Current source links route to
the owning implementation; pinned links identify historical evidence. B7 and the complete portrait
fix are merged in [PR1413](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1413)
and [PR1415](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1415). E7's details extension has
bounded executor evidence below; [PR1417](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1417) owns integration
and independent acceptance status. This document adds no original-game finding.

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
implementation. No new P0 is established. Delivered slices A–E are defined below; an Epic link means
there is no separately dispatched implementation Issue yet.

## Entry and exploration matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| E1 · Launch; title, Start/New Game, Continue | **Not implemented** as product screens. Ordinary startup immediately selects an authored practice yard or an explicit private start. | **Unknown:** intended first-launch/menu/exit journey. Continue has no persistence backend; a controlled JSON start cannot be presented as a player save. **Inferred:** users cannot begin a normal campaign from a familiar front door. | **Confirmed structure:** [GameRoot._Ready](../../game/src/GameRoot.cs), [profile summary](../content/profiles-and-trust.md#profile-summary). [Persistence is Unsupported](../capability-status.md#milestone-and-deferred-surfaces). No title-to-Map3 observation exists in this scope. | **P2, decision first.** Define supported entry and save expectations under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438) and [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). No entry implementation ticket proposed before that choice. |
| E2 · Change settings before or during play | Versioned startup input/text/reduced-flash settings are **accepted, bounded**; settings screen and automatic save are **not implemented**. | Existing configuration is not a discoverable in-game menu. **Unknown:** which settings must be exposed and persisted. Do not conflate supported remapping with a delivered settings UI. | **Confirmed structure and retained input boundary:** [InputSettings](../../game/src/Input/InputSettings.cs), [logical input/settings owner](../verification/host.md#logical-input-and-accessibility-adr-0010-9a). | **P2, decision first.** Same entry/settings product boundary as E1; [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), persistence dependency [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369). |
| E3 · Invalid launch option, missing content or invalid settings | Typed startup failure reaches the common battle view: **accepted, bounded** diagnostics. | **Integrated, experience pending:** technical kind/code/field is shown; no product recovery/retry screen is connected. **Inferred:** a player may know startup failed without knowing an actionable remedy. | **Confirmed structure:** [GameRoot.Fail](../../game/src/GameRoot.cs), [BattleSessionView.FailStartup](../../game/src/Battles/BattleSessionView.cs). [Startup observations](../verification/host.md#ordinary-host-startup) establish rejection, not a complete recovery experience. | **P2.** Entry choice E1 precedes recovery design; error wording also belongs in [C](#c--read-and-answer-a-story-sequence). [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| E4 · See Map3, move between scenes, follow camera | **Accepted, bounded:** shared 1920×1080 viewport, 96px cells/ordinary frames and 4× nearest sampling. Logical state remains authoritative; modern dead-zone/story panning maps it into the larger frame. | Small legal rooms center on exterior color; packed-area transfers reframe in their existing transition. Original hardware composition and broader aesthetic experience remain **Unknown**. | [Unified viewport agreement](../godot/presentation.md#unified-world-viewport-agreement) and [native recipe](../verification/host.md#unified-world-viewport-observation) cover private room/boundary, teacher-wall ink repair, map transition and battle handoffs. | **P1; A/E4/X1 implemented by [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391); [integration status](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).** C/D/E are also accepted within their separate scopes. |
| E5 · Hold direction, release, lose focus; then use a menu | Continuous mapped field walking is **accepted, bounded**: fresh step, 220 ms initial delay, 120 ms repeat without catch-up. Ordinary motion waits retain the hold; release/lifecycle/story handoffs stop future steps. | Bounded engineering observations do not establish full opening experience. **Unknown:** physical device release outside the focused window. Choices/menu retain edge input; no original timing claim. | **Confirmed structure and bounded executor observations:** [control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk), covering mapped keys/stick, real native focus transfer and handoffs. Independent attempts encountered external focus loss; completed failures remain with merged [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); independent of #689 and viewport choice. |
| E6 · Face an NPC and try to interact | Binding-derived Confirm + Talk appears only for a current legal interaction offer: **accepted, bounded**. Confirm requeries; Interact rechecks the same target/event predicate. | No fabricated names or new interactions. **Unknown:** broader prompt discoverability and opening-route experience; cue placement uses A's accepted baseline. | **Confirmed structure and bounded engineering observations:** [shared Application query and host observation](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk); tests cover visibility/range/facing/flag/busy changes; executor host observations cover facing/leaving/busy hints and dialogue. Accepted integration belongs to [PR #1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390). | **P1.** [B](#b--walk-to-a-character-talk-and-regain-control), [#1389](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1389); no Search/acquisition. |
| E7 · Inspect party/status or open a field menu | **Accepted, bounded initial entry:** E / Pad Start opens Party Status. **Implemented extension, bounded executor observation:** original member-screen grouping puts portrait/KD/gold left, aligned Level/ATT, HP/DEF, MP/AGI, EXP/MOV right and magic/items below in parallel columns, with a compact member selector and mapped/mouse browsing. | Use/Give/Drop/Equip, field spells and persistence remain **not implemented** for this menu. Human discoverability, all-device and whole-opening approval remain **Unknown**. | [E7 agreement](../godot/presentation.md#party-details-agreement), [native query/input/return evidence](../verification/host.md#party-details-observation); accepted initial [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400). Inspection writes no gameplay state. | **P2; [#1416](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1416).** This inspect subset does not close [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); field actions/save scope remains separate. |

## Dialogue and cutscene matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| D1 · Story dialogue opens with/without a portrait | **Accepted, bounded:** shared 1440px blue/pale-gold panels, 30px text, measured expansion/reading pages, complete 48×64 portrait raster at 192×256, side/mirror/eye/mouth and window lifecycle. | Names are intentionally absent: current admitted entity metadata has no reliable speaker-name mapping. Member names inside text remain separate. Full-game readability/final aesthetics remain **Unknown**. | [Current treatment](../godot/presentation.md#shared-story-dialogue-agreement), [private Zone8 and authored native bounds](../verification/host.md#shared-story-dialogue-observation). Real portrait is a child above the background; no inferred visibility from rect alone. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Merged C/PR1394 and portrait correction [PR1415](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1415) retain their bounded acceptance; no new source-fidelity claim. |
| D2 · Reveal text, advance a page, or press Cancel | **Accepted, bounded:** contextual Reveal, Read more and eligible Continue; full shaped layout; Confirm reveal-all, later fresh acknowledgement, instant mode and held/echo protection. Ordinary Cancel does not skip dialogue. | No text is shrunk or silently clipped. Screen-sized content uses display-only pages before acknowledgement/delivery; original typewriting timing and whole-opening experience remain **Unknown**. | [Field controller](../../game/src/Exploration/ExplorationSessionView.cs), [shared panel](../../game/src/UI/StoryDialoguePanel.cs), [70-line/repeated-window native cases](../verification/host.md#shared-story-dialogue-observation). Existing speech/confirmation authority is retained. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Broader text/settings/history/autoplay are excluded. |
| D3 · Yes/No opens, focus changes, confirm/cancel answers | **Accepted, bounded:** attached options share pointer/directional focus; Confirm answers the highlight, click answers that option, Cancel/right click means No. Source default Yes and entry/release/closing remain session-owned. | Reading and complete-gesture guards precede answers; no fabricated authored source phase or answer from held skip. Broader human/full-game experience remains **Unknown**. | [C story cases](../verification/host.md#shared-story-dialogue-observation), [D pointer/source cases](../verification/host.md#menu-pointer-observation), [retained choice evidence](../verification/field-programs.md#source-bound-choice-observation). | **P1 C / #1393; P2 D / [#1408](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1408).** D is merged in [PR1409](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1409); broader human acceptance remains separate. |
| D4 · Opening or tower camera/motion/fade waits | **Accepted, bounded:** quiet scene-in-progress feedback; Continue only at eligible input; existing entry Skip only when eligible; readable stopped wording preserves diagnostics/state. | No ordinary-wait timeout, general scene skip or fabricated retry. Whole-opening perceived pacing and stall diagnosis remain **Unknown**. | [Current presentation](../godot/presentation.md#shared-story-dialogue-agreement), [automatic/failure/battle-story/skip cases](../verification/host.md#shared-story-dialogue-observation). Existing camera/fade/wait consumers remain authoritative. | **P1; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** Original timing and broader UX remain outside this boundary. |
| D5 · Skip eligible tower before/load/start | **Accepted, bounded:** mapped Cancel (default Escape/X) requests Skip battle intro; Confirm/Enter remains reveal/advance. Skip preserves script work, mounts real resources and clears before ordinary battle advancement. | This is not a remaining missing feature or a request for another skip ticket. Choice/failure stops it; focus loss disarms it and fresh input is required. **Unknown:** physical focus-loss/short-tail experience beyond recorded observations. | **Confirmed accepted implementation/observation:** [skip owner](../godot/battle-entry-skip.md#current-acceptance-boundary), [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386#issuecomment-6462571007), merged [#1387](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1387). Six/eight ObjectDB exit warnings remain historical evidence; the [audio-lifetime finding](../verification/host.md#audio-resource-lifetime-observation) explains a bounded teardown-retention cause without individually proving these older instances. | **P2, preserve accepted behavior.** C may reuse wording/controls without broadening eligibility. Existing [#1386](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1386); no duplicate implementation slice. |

## Battle and return matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| B1 · First battle input; open the action menu | Compact cardinal diamond, original normal/selected icons, one shared hint and full-map UI overlays are **accepted, bounded** in #689. | User play acceptance at f60ed974 is **Confirmed** by the [acceptance record](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454); main-gate completed independent engineering acceptance at a386b66e. Git integration status is owned by [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692). This does not accept the remaining opening UI gaps. | **Confirmed structure at f60ed974:** [BattleCommandMenu](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs), [BattleMenuLayout](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleMenuLayout.cs); subsequent a386 changes only documentation. [#689 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6461883659) retains the earlier visible-entry 150s timeout; later acceptance does not rewrite it. | **Existing bounded slice:** [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) / [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692); no additional requirement from this assessment. |
| B2 · Magic/item list, disabled choice, target, explicit commit, layered Cancel | Semantic offers/targets/reasons, diamond lists and `ReturnToBattleActions` UI are **accepted, bounded** under #689. | Selection, movement preview, resources/RNG and supplementary Equipment/Give/Drop routes are preserved within the accepted scope. User interaction/readability approval and independent engineering acceptance are complete; [PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) owns Git integration status. Search, mouse and global layouts remain separate. | **Confirmed structure:** [semantic interface](../godot/presentation.md#semantic-interface), [command menu at f60ed974](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/f60ed9744549fee461102a97c480f1e55ee0fb0c/remake/game/src/Battles/BattleCommandMenu.cs). [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) owns actual input and unavailable-target/cancel acceptance, including the subsequent independent a386 observations. | **Existing #689 boundary.** D consumes this accepted menu interface; no scope expansion of [#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689). |
| B3 · Read current actor, HP/MP, round, target and control state | **Accepted, bounded:** shared readable-name HP/MP cards in command, target and action scenes; retained round/gold context and diagnostic observations. | Admitted member/enemy names replace raw IDs on these surfaces; public content has an intentional readable identity fallback. E2 supplies result feedback and read-only field status; broader information/actions and final aesthetics remain **Unknown**. | [E1 agreement](../godot/presentation.md#battle-status-card-agreement), [native card/phase observations](../verification/host.md#battle-status-card-observation). Independent acceptance distinguishes windowed heal/private/names from headless damage/overflow; retained window failures and input/shutdown Unknowns remain. | **P1; E1 / [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).** Independently accepted at `03f1e952`; [PR1399](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1399) is merged; E2 is also merged in [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400). |
| B4 · Action, damage/healing, sleep/death and control release | Close-up messages, real reveal/acknowledgement, finite scene/movement and ordered field-death removal are **accepted, bounded** consumers. | **Integrated, experience pending:** information ordering, message occlusion, removal timing and readiness cues need a bounded experience check. E1/E2 supply shared HP/MP cards and readable message framing; full status-effect coverage is not established; authored poison and other capability-specific notices do not prove private Battle01 status coverage. | **Confirmed structure:** [BattleSceneView.Message/Consume/Acknowledge](../../game/src/Battles/BattleSceneView.cs); [physical scene](../godot/battle-scenes.md#physical-battle-scenes), [death batch](../godot/battle-scenes.md#battlefield-death-batches) and [capability limits](../capability-status.md). Original timing/per-character font fallback remain **Unknown**. | **P1.** [E](#e--understand-a-battle-result-and-continue-in-the-field), existing scene/round/input ownership; no new statuses or scene rules. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| B5 · Receive EXP/gold/level/stat/spell messages | Action reward/growth message phases are **accepted, bounded**. A separate post-battle summary is deliberately not added. | **User decision:** retain existing reward/growth messages without a separate summary. Merged E2 supplies clear result/return control and read-only returned-party information; it must not count rewards again. Direct-reader growth Unsupported failures do not establish whole-engine absence. | **Confirmed structure:** [BattleSceneView.Message](../../game/src/Battles/BattleSceneView.cs). [Private milestone](../../../docs/design/synthesis/milestones/map3-battle01-readiness.md#accepted-current-milestone) and [font/operand observation limits](../verification/battles.md#battle-message-operand-and-font-observation) distinguish connected growth from a limited direct start. | **P2; E2 accepted and merged / [#1398](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1398).** No new summary; current progression remains authoritative. |
| B6 · Read active actor HP/MP | **Accepted, bounded:** one image frame/track/fill component plus current/max numbers serves command, target and scene cards. | No admitted private gauge was found; minimal project-authored CC0 PNG segments supply the treatment. Zero-max MP has a dash; low/dead HP has text cues. Original gauge art/timing and broader aesthetic approval remain **Unknown**. | [Asset selection](../content/presentation-assets.md#public-authored-battle-gauges), [native ratio/damage/heal/MP phases](../verification/host.md#battle-status-card-observation). Candidate SVG meters are not runtime integration. | **P2; E1 / [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).** Independent engineering acceptance is complete at the documented windowed/headless boundary; no easing, gameplay-state copy or progression-blocking bar animation. |
| B7 · Change focus among the four commands | **Accepted, bounded; merged [PR1413](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1413):** a short 180 ms selected-image emphasis draws at vertical offsets −4/−2/−1/0 pixels using existing normal/selected resources. Keyboard and pointer share the same focus. | Stable geometry, hit rectangles and immediate semantic input remain; idle/current-option input cannot retrigger. Reduced-flash is static. Lifecycle cancellation prevents stale motion. Whole-opening aesthetic approval remains **Unknown**. | [Modern presentation agreement](../godot/presentation.md#diamond-focus-emphasis), [compact native recipe/results](../verification/host.md#diamond-focus-emphasis-observation). The original [reported gap](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454) remains historical evidence. Original cadence is **Unknown**; this is a modern design choice with no emulator acquisition. | **P2; [#1412](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1412).** Independent engineering acceptance is complete; no reopening of #689 or new art pipeline. |
| B8 · Select an actor and preview movement | **Implemented, bounded headless/windowed observation:** authoritative legal destinations use a translucent cyan board layer at the accepted 96px scale. | Range stays at committed origin through preview/return delivery; menus, targeting, automatic turns, scenes, story/outcome and field return clear it. Living allies are traversable but excluded as stops. | **Confirmed:** engine behavior checks and actual mapped-input/draw observations cover authored occupancy/budget, actor refresh, automatic movement, outcome and private Battle02 region(16,19)/Egress. [Agreement](../godot/presentation.md#battle-movement-range-agreement); [reproduction/limits](../verification/host.md#battle-movement-range-observation). Independent windowed checks passed; broader fixture coverage remains **Unknown**. The initial nonreproduced Cancel-sequence failure and private headless shutdown warnings are retained. | **P1; [#1395](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1395)** under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). No new movement rules, world-click input or original-pixel claim. |
| R1 · Win Battle01, close battle/story windows, return to field | Genuine outcome/return programs, fresh logical view/closed windows and same-session usable movement are **accepted, bounded**. E2 adds readable battle messages, return feedback, stale-window retirement and read-only Party Status. | **Unknown:** whole-opening player understanding, information hierarchy and aesthetic approval. The delivered result-to-field treatment is no longer absent; no extra reward summary/accounting is implied. | [E2 agreement](../godot/presentation.md#battle-messages-return-and-party-status), [actual return/Party Status observations](../verification/host.md#battle-return-and-party-status-observation), merged [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400), and [original victory/return boundary](../verification/field-programs.md#bound-victory-and-return-observation). These do not establish human-play approval. | **P1, preserve accepted [E](#e--understand-a-battle-result-and-continue-in-the-field).** Broader experience remains under [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438). |
| R2 · Lose or request retreat | **Confirmed retained evidence:** ordinary connected defeat has bounded return evidence. Selected Battle02 EGRESS/Angel Wing has a separate consumer; arbitrary Battle01 retreat is **not established**. | **Unknown:** player-facing defeat/retry/save expectations and Battle01 retreat scope. Returning to a controlled savepoint is not a delivered Continue/reload UI. Do not make wider retreat or later chapters an opening-UI requirement. | [Defeat/route observations](../verification/field-programs.md#battle01-through-battle02) and [retained failures](../verification/field-programs.md#retained-candidate-results-and-open-acceptance) retain their exact boundaries; [BattleRetreatView](../../game/src/Battles/BattleRetreatView.cs) is **Confirmed structure** for admitted retreat, not general availability. | **P2.** E reviews supported defeat only; entry/retry and unsupported retreat require explicit scope first. [#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), [#1369](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1369), [#1370](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370). |

## Cross-scene consistency matrix

| ID / flow and trigger | Existing behavior and delivery state | Gap and user impact | Evidence and limits | Priority / dependencies / suggested slice and Issue |
| --- | --- | --- | --- | --- |
| X1 · Compare field, dialogue, battle and return at one size | **Accepted, bounded:** the user-selected 1920×1080 baseline shares one world rectangle and 96px actor/cell scale across exploration and battle. UI overlays do not change zoom or world rect. | Movement/targets pan at fixed zoom; corner panels avoid an edge-clamped focus. Private Battle01 entry and nonzero-region Battle02 Egress/field return are observed. Multi-resolution settings, free zoom and full UI theming remain out of scope. | [project.godot](../../game/project.godot), [layout agreement](../godot/presentation.md#unified-world-viewport-agreement), [native geometry/input limits](../verification/host.md#unified-world-viewport-observation). Original timing and global aesthetic approval are **Unknown**. | **P1; [A](#a--see-the-connected-world-at-the-chosen-baseline), [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391); [integration status](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).** #689 retains its earlier review-size evidence. |
| X2 · Reuse windows, fonts, borders, focus and hints | **Accepted, bounded for actual C consumers:** shared native Godot Theme, dialogue panel, choice row and contextual hint; existing battle menu frames consume the same style. | This is a bounded story/choice treatment, not a finished global UI kit. D and E consume the accepted shared treatment. Broader typography/art direction and all-map aesthetics remain **Unknown**. | [Shared components/current owner](../godot/presentation.md#shared-story-dialogue-agreement), [finite native recipe](../verification/host.md#shared-story-dialogue-observation). No new fonts/private SVG assets or framework. | **P2; C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).** E/D reuse only where justified. |
| X3 · Switch keyboard and basic menu mouse use | **Accepted, bounded:** hover/click on battle commands and spell/item rows; local wheel scrolling; ordered Previous/Next targets, Confirm and Back/right click; field choices share keyboard/pointer focus. | Complete current-control gestures prevent drag-out, focus-loss and cross-layer/scene release reuse. Native window focus is observed with an explicit axis-adjusted input configuration; physical-device release and all-device experience remain **Unknown**. World click movement and map targeting are excluded. | [Pointer agreement](../godot/presentation.md#menu-pointer-agreement), [finite native recipe/results](../verification/host.md#menu-pointer-observation). Existing [9A](../verification/host.md#native-9a-observation) retains its keyboard/pad scope. | **P2 D / [#1408](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1408).** Merged [PR1409](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1409) records independent integration; broader human acceptance remains separate. |
| X4 · Invalid/disabled command or adapter failure during any scene | Field/story stopped presentation now uses the shared readable frame above fades, retained message/code and pending state; no invented retry. Existing semantic Enabled/Reason and battle/startup diagnostics remain. | **Bounded C disposition:** a controlled field failure is observed; E2 also supplies readable battle messages and unobscured recoverable illegal-command feedback; generic recovery/retry remains **Unknown**. | [Failure/current owner](../godot/presentation.md#shared-story-dialogue-agreement), [controlled failure witness](../verification/host.md#shared-story-dialogue-observation), [semantic interface](../godot/presentation.md#semantic-interface). | **P1.** C / [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393) handles field/story; #689/E retain their own reasons and result boundaries. |

## User feedback disposition and existing acceptance

The [2026-10-28 user feedback](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438#issuecomment-6461577919)
is remake product intent, not evidence about original SF2:

| Request | Matrix / disposition |
| --- | --- |
| Continuous directions, release/focus stop; distinct menu cadence | E5/E6 → B; #1389 implements and observes the bounded control/hint behavior. Physical-device and whole-opening experience limits remain explicit. |
| Choose one resolution, then modern world viewport/camera | E4/X1 → A/#1391: user selected 1920×1080,96px cells/ordinary frames, common scale, fixed zoom/panning and UI overlays. Bounded implementation/observation is recorded below; multi-resolution support remains deferred. |
| Systematic UI used in exploration and cutscenes | E7/D1–D4/X2/X4 → C/#1393 implements the bounded shared story treatment; E1/E2 now also consume shared presentation. Existing assets/interfaces do not establish a finished global UI kit. |
| Basic mouse for existing menus | D3/X3 → D/#1408 implements the bounded hover/complete-click/explicit-confirm/back agreement; no world-click movement promise. |
| Skip battle-entry scene; Enter did not skip | D5 → existing #1386, independently accepted via #1387. The earlier missing-skip report is superseded only within its explicit before/load/start boundary. |

[#689](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689) remains only the already-made
battle command UI: cardinal placement, direct original icons, compact shared selection hint, keyboard
lists/targets/commit/cancel, resource preservation and actual operation at its review window. That historical
menu acceptance did not choose the later A world-viewport design. The user accepted
f60ed974 in [human play review](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/689#issuecomment-6482742454),
and main-gate completed independent engineering acceptance at a386b66e.
[PR692](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/692) is authoritative for Git integration
status: it is merged. B3/B6, B8 and X1 now have their own accepted deliveries; B7 is the separate
merged focus-emphasis slice below. None reopens that bounded approval. This assessment, exploration
redesign, fixed resolution, continuous walking, mouse and generic UI reuse are not hidden completion
conditions for #689. Accepted intro skip removes a scheduling dependency for its observations; it
does not erase the earlier 150-second visible-entry timeout. The later skip-assisted visible-window
observation passed in 3,751 ms at f60ed974; human approval is recorded separately above, without
claiming natural full-opening timing or whole-game UI acceptance.

## Delivered slices and remaining acceptance

A–E are merged bounded deliveries under #438, with the limits below. Keep their existing owners;
there is no pending A–E implementation batch or reason to open duplicate tickets. Product decisions
about entry/settings/persistence and broader human/aesthetic acceptance remain open; link #1369
when its engine scope is actually needed.

### A · See the connected world at the chosen baseline

**P1; E4/X1; accepted, bounded; merged [PR1392](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1392).**
The user approved 1920×1080,96px cells/ordinary sprite frames, common world scale, fixed zoom/panning
and independent UI overlays in [#1391](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1391).
The [presentation owner](../godot/presentation.md#unified-world-viewport-agreement) defines units,
logical/display mapping, legal bounds and handoff. The [native recipe](../verification/host.md#unified-world-viewport-observation)
checks actual private drawing and inputs at this one baseline. No gameplay/Content/research changes,
new asset copies, free zoom or multi-resolution matrix are included. Broader story components,
mouse, result/status UI and B3/B6–B8 remain with C/D/E/their separate slices.

### B · Walk to a character, talk and regain control

**P1; E5/E6; accepted, bounded; merged [PR1390](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1390).** Continuous mapped walking and contextual Talk use
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
timing or visual baseline was selected. Cue placement uses A's accepted baseline, and #689's separate
follow-ups and historical results remain preserved.

### C · Read and answer a story sequence

**P1 behavior, P2 component consistency; D1–D4/X2/X4; accepted, bounded; merged [PR1394](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1394), under [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).**
The approved [shared story treatment](../godot/presentation.md#shared-story-dialogue-agreement)
uses A's accepted geometry and existing text/portrait/choice/wait authorities. The
[finite recipe](../verification/host.md#shared-story-dialogue-observation) covers real Zone8 text,
portrait layering, source Yes/No/default/release, authored reveal/instant/automatic return,
long/repeated text reading, stable top placement, failure and existing battle-story/skip consumers.
The complete portrait correction in merged [PR1415](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1415)
restores the full lower row; its [pixel witness](../verification/host.md#complete-portrait-raster-observation)
retains earlier failed admissions/observer attempts. No reliable story speaker-name mapping is admitted, so names are omitted. D and E subsequently
delivered their separately scoped mouse and result/party consumers. Global fonts/art/settings,
original timing, broader aesthetic and all-map acceptance remain outside C's bounded acceptance.

### D · Choose existing menu options with a mouse

**P2; D3/X3; accepted, bounded; merged [PR1409](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1409), under
[#1408](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1408).** The
[current agreement](../godot/presentation.md#menu-pointer-agreement) consumes the accepted #689 menus
and C choices. D1 battle input was piloted before D2 story choices. Hover focuses, complete click
activates that option, and a separate Confirm commits a battle action. Directional input takes focus
back; stationary pointers do not steal it. Disabled offers/targets explain without committing a prior
valid selection. Back/right click unwind one layer; story Cancel/right click means No. Gesture and
reading guards prevent click-through into newly attached controls or scenes. The
[finite host witness](../verification/host.md#menu-pointer-observation) preserves failures and its
windowed/headless/physical-device limits. No title menu, world-click movement, new targeting system,
controller expansion or full-game mouse-only claim is included. Independent integration is complete;
human/aesthetic acceptance remains separate.

### E · Understand a battle result and continue in the field

**P1 control/result clarity; P2 optional information treatment; E7/B3–B6/R1/R2/X4.** E is
split into two user-approved outcomes. [E1 / #1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397)
implements shared battle names and image-based current/max HP/MP cards after accepted A/C/B8/#689.
Its [presentation agreement](../godot/presentation.md#battle-status-card-agreement) and
[native recipe](../verification/host.md#battle-status-card-observation) own the actual scope, assets,
phase timing and independent acceptance at `03f1e952`: windowed heal/private/names and headless
damage/overflow. Earlier window failures, native joypad interference and shutdown Unknowns remain;
no windowed damage/overflow pass is claimed. Engineering acceptance and merge are complete in [PR1399](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1399).

[E2 / #1398](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1398) consumes merged E1.
Its [presentation owner](../godot/presentation.md#battle-messages-return-and-party-status) implements
readable battle messages, actual return feedback/window retirement and the explicitly approved
read-only Party Status entry. Existing reward/growth messages remain; no extra summary or reward
accounting is introduced. The [host recipe](../verification/host.md#battle-return-and-party-status-observation)
records independently accepted headless authored win/ordinary defeat, return, private/physical
close-up visibility, startup/program failure and recoverable illegal-move feedback observations.
Main-gate engineering acceptance and merge are complete in
[PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400).
The [E7 details extension](../godot/presentation.md#party-details-agreement) adds source-grouped read-only member selection,
growth/stats/spells/inventory inspection with [executor native evidence](../verification/host.md#party-details-observation);
[PR1417](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1417) owns independent acceptance and integration. It closes only this inspect subset when accepted.
Wider retreat, saves/retry, item actions and later chapters
remain outside this slice.

A–E's dependency sequence is now delivered, as is B8 movement-range visibility in merged
[PR1396](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1396). B7 has bounded independent acceptance in merged [PR1413](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1413). New product scope needs its own exact ownership and acceptance;
these deliveries do not close #438 or authorize concurrent shared-document writers.

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
it adds no SDK, Godot, replay or original-game observation. Their later deliveries retain their own scope and evidence above.

In particular, preserve the UI visible-entry timeout; #1386's six/eight ObjectDB exit warnings and
unobserved physical focus/speech-tail boundaries; limited direct-reader growth failures; and existing
route/comparison failures. No claim that all opening paths fail, all status effects work, every
window is readable, or native exit is universally clean follows from this inventory. Later matching
evidence may narrow a specific Unknown; source existence and green CI alone cannot do so.

For the original assessment, verification covered its two owned paths and Markdown links, anchors,
tables, fences, source references and private boundary. The later #689 correction owns only this
file: verify the diff scope, links/table structure, preserved remaining-gap rows and `git diff --check`.
Record exact Git identity and actual CI in the Issue/PR handoff; PR-level CI may select implementation
jobs because PR692 also contains the battle UI. Experience confirmation of the remaining gaps,
implementation ticket creation and product-direction selection remain outside this assessment's
completion claim. Git integration is tracked by PR692 rather than a static pending-state assertion.

The current calibration uses the merged PRs above and the current presentation owners. Historical
warning records remain intact. The [audio-lifetime investigation](../verification/host.md#audio-resource-lifetime-observation)
confirms bounded shutdown retention and no accumulation in its named runtime sequence; it does not
individually explain every earlier warning or establish universally clean exit. B7 observations and
limits belong to its linked host recipe. Title/New Game/Continue, settings UI/persistence, field
Use/Give/Drop/Equip/spell actions, all-device coverage and whole-opening human/aesthetic approval
remain explicit gaps; #438 stays open.
