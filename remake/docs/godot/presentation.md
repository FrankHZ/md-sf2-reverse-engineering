# Godot Host and Presentation

## Menu pointer agreement

**Standard rigor; [#1408](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1408).**
This bounded input change covers the existing battle command diamond, spell/item lists, ordered
targets and Stay confirmation, followed by the existing field Yes/No choices. It retains the
accepted 1920×1080 geometry, original admitted command rasters and shared story panel. World movement,
map/actor picking, title/settings menus, supplementary Equipment/Give/Drop and Party Status mouse
redesign, new targeting rules and full-game mouse-only play are outside this agreement.

Actual pointer movement over an option takes focus and updates its description. Keyboard/pad
navigation takes focus back; redraw and a stationary pointer never select another option. Disabled
entries remain inspectable and explain their refusal. A complete left press/release on the same
current control activates that option. Blank clicks do nothing. Dragging out, double-click tails,
focus loss, hiding, control replacement, keyboard handoff and changed session/menu boundaries cancel
an old press. A release without its own press cannot activate a newly attached menu or question.
Native wrapped-label geometry settles independently of input before hit regions are used.

Action/list clicks enter the existing target or Stay flow. Previous/Next cycle the existing ordered
candidate set, including unavailable candidates. Confirm commits only the current enabled candidate;
inspecting an unavailable candidate cannot commit an earlier valid target. Back and right click
unwind one layer, preserving the existing movement-preview, resource and RNG rules. Wheel scrolling
is confined to the open list. Existing keyboard/pad commands retain their ordinary bindings.

Yes/No starts with Yes focused. Pointer hover, then directional selection, share the displayed focus;
Confirm answers that focus. A deliberate click answers the clicked option. Cancel and right click
mean No, explicitly labeled in the hint. Authored focus is disposable UI state; the answer still uses
the existing `ChooseDialogue` command. Source focus remains `ChoiceWork.Yes`; the same command now
accepts an explicit answer only in its admitted Input phase, using the existing choice runner's
selection and closing path. Entry, held-button Release, Opening, Closing, result publication and
program continuation remain Application-owned. Reading/reveal and matching wait-token guards
precede pointer admission. A completed answer never writes a story flag from a widget.

The [finite host recipe](../verification/host.md#menu-pointer-observation) owns controlled inputs,
windowed focus/drag/layer observations, headless battle and source/authored choice observations,
completed failures and remaining physical-device limits. Small actual Application behavior tests,
adapter compilation and committed scope selection are the gates. No research suite, H3/H4 comparison,
new input framework, recorder or asset pipeline is introduced. Independent main-gate integration and
broader human/aesthetic acceptance remain separate.

## Diamond focus emphasis

**Standard rigor; [#1412](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1412).**
The existing diamond has one short focus-change emphasis shared by keyboard/pad and pointer focus.
The selected drawing rises four display pixels, then settles through two and one pixels to zero
over 180 ms. It uses the existing normal/selected 24×24 resources, four-times nearest sampling and
integer draw coordinates. Text-only public squares use the same drawing offset. This is a modern
presentation choice; original animation cadence remains **Unknown** and creates no acquisition task.

Only a new selection starts the emphasis. Idle, refresh, repeated current-direction input and a
stationary pointer never restart it. Another focus cancels the previous item's motion. Closing,
list/target handoff, hiding, pause, native focus loss and scene retirement cancel pending emphasis;
showing or regaining focus cannot resume it. Reopening starts from the current menu selection.
Reduced-flash always draws the stable selected image without motion.

Only drawing changes: cardinal controls, hit rectangles, description, disabled reasons, resources,
RNG, input admission and explicit command confirmation keep their existing authorities. Frame
processing advances a button's short timer; it submits no command and plays no sound. No general
animation framework, art pipeline or gameplay state was added. The
[native recipe](../verification/host.md#diamond-focus-emphasis-observation) owns actual drawing,
lifecycle observations and mounted-original-icon acceptance evidence. Merged
[PR1413](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1413) owns bounded engineering acceptance;
whole-opening human/aesthetic approval remains Unknown.

## Battle messages, return and Party Status

**Standard rigor; [#1398](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1398).**
This bounded presentation slice consumes accepted E1 and C. Existing action, damage/heal, status,
death, EXP/gold/growth message order, tokens, payment and outcome programs remain session-owned.
`BattleSceneView` uses the shared blue/gold `StoryDialoguePanel` at native 30px, with mapped Reveal,
Continue and eligible Wait hints. New tokens reset reading position; same-token refresh retains it.
Display-only reading pages precede acknowledgement. E1 cards retain their live actor resources.

Battle chrome distinguishes ready selection, “Battle in progress…”, rejected commands with their
reason, and “The battle has stopped” with the retained failure. A visible command menu owns its
recoverable IllegalCommand reason; the global feedback panel stays hidden to avoid covering the
menu description and controls. Errors without a menu and fatal/startup failures retain global feedback. Startup/adapter errors use the same
visible feedback panel; `ContextHint.Arrange` explicitly shows nonempty text. Fatal stopped sessions
cannot continue automatic or ordinary input. Diagnostic IDs/stages remain in observations.

At the actual battle-to-field handoff, battle windows retire while the battlefield may remain beneath
field-owned after/return effects. Existing reward messages and outcome programs finish normally.
A transient Victory/Defeat/Returned notice becomes “Back in the field” only at `CanWaitAtInput`;
it introduces no acknowledgement or reward summary. Ready return also releases any retained battle
mount. The first fresh field command dismisses the notice and executes normally.

At settled field input, the existing Equipment binding (default E / Pad Start) opens **Party Status**.
The E7 extension's agreement below owns the roster/details layout and read-only data projection.
Opening, browsing and closing submit no session commands. Busy or failed field state cannot open it;
a changed field boundary closes it. Existing return feedback and reward messages retain their own
ordering and accounting.

Acceptance is adapter compilation and the clean-head scope plan, plus the
[bounded host witness](../verification/host.md#battle-return-and-party-status-observation).
No Domain/Application/Content behavior changes require new unit tests. Native observations use
headless Godot because external joypad interference remains unresolved in windowed runs; this is
not a new windowed acceptance claim. Main-gate independently accepted the bounded victory/defeat,
return, private/physical close-up visibility, startup/program failure and recoverable illegal-move
feedback observations. The host owner records the exact evidence and retained limits; integration
is tracked by [PR1400](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1400).

## Party details agreement

**Standard rigor; [#1416](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1416).**
This bounded E7 extension lets the player inspect preparation and returned progression. At the
accepted 1920×1080 baseline a scrollable left roster selects a stable right HP/MP card and scrollable
details: name, gold, level/EXP, effective attack, defense/agility, learned magic and four numbered
inventory slots with equipped/broken markers. Duplicate items retain their slots. Full names wrap;
long lists scroll. Blue/gold shared styling and the existing HP/MP gauges remain. Unavailable maxima
hide the card; missing accounting, loadout or item/spell names say unavailable, distinct from known
empty slots or no learned magic. Defeated and absent joined members remain inspectable.

`GameSession.QueryPartyDetails` reads the current carried party and admitted definitions. The existing
`MapPartyMembership.Rebuild` with current flags supplies joined membership where source member
bindings exist; reserve membership is included. Seeded future allies and enemy records are excluded.
Authored packages without source member bindings retain their carried-ally fallback. Current progress
and carried loadout precede admitted baselines. The existing Domain equipment owner computes attack
for both battle construction and the query; no temporary battle or mutable actor copy is created.
Private item/spell display names come from admitted `PrivateBattleDefinitions`; authored definitions
supply weapon/item labels and readable learned spell identities. There is no UI catalog keyed by
source IDs, class-name invention or second roster authority.

Up/Down selects the roster; Left/Right also changes member. Target (default Tab / Pad RB) switches
Up/Down to details scrolling and back. Cancel/Equipment/Confirm closes. A completed left click selects
a roster row or the visible Close affordance; wheel scrolling stays inside its pane. The existing
`MenuPointer` rejects orphan releases, drag-out, stale selection, double clicks and suspended presses.
Opening/visibility/focus suspension requires released gameplay input; the field input owner clears
movement holds, and modal input cannot reach the field. A fresh command after closing resumes field
control. Selection/focus/scroll are disposable UI state; no gold, resources, growth, inventory, flags,
RNG or session revision is written. Use/Give/Drop/Equip/field magic, saves, shops, title/settings,
new content admission and full inventory services are outside this agreement.

Acceptance uses small actual engine behavior checks, adapter compilation, the clean-head scope plan
and [native party-details witness](../verification/host.md#party-details-observation). Executor
headless observations establish the named input/projection boundary; [PR1417](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1417) owns independent
main-gate windowed acceptance and integration status. Physical-device release, whole-opening human/aesthetic approval and
pinned Godot audio shutdown warnings retain their existing Unknowns.

## Battle status-card agreement

**Standard rigor; [#1397](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1397).**
E1 is a bounded presentation change spanning command, target and action-scene consumers of the
same authoritative session. `BattleStatusCard` supplies one readable name, labeled HP/MP image
frame/track/fill and current/max numbers. No new rule, resource authority, rename feature, animation
clock, generic UI framework or asset pipeline is introduced. E2's party/result/field information,
rewards, mouse, command animation and additional statuses remain outside this agreement.

`BattleCharacterNames` centralizes presentation names. Admitted scene member names (including
selected custom names) use the source party-member identity where available; admitted enemy names
use the content's actor binding. Public-authored packages deliberately title-case their hyphenated
content identities, for example `guard-a` → “Guard A”. They need no private content or invented
original character mapping. No runtime rename operation exists in this slice. Diagnostic actor IDs,
scene target/displayed-side fields and observations remain stable. Existing authored healing wording
keeps its engine clock; the view replaces only its known leading subject with the readable name.

At the accepted 1920×1080 viewport, cards begin at 500×192 and the 32px safe edges. Native smart
wrapping keeps complete ordinary names and valid 64-character IDs in two lines at 26–22px, widening
the card where needed. Name lookup normalizes whitespace only for display. Width expansion has at
most 19 steps and stops at 800px; exceptional metadata beyond that capacity retains its normalized
text in observation but uses a two-line native ellipsis fallback. Original name/story data is unchanged.
The menu retains its existing subject-avoidance algorithm, diamond/list geometry, controls,
fixed 96px world scale and movement-range visibility. A target card appears only on its target page;
Cancel and actor changes clear it. Context moves below a visible target card. Action scenes use
the same component at the safe top edges without changing the scene canvas or world framing.
The existing native minimum-size notification resets hidden/wrapped controls after actual sorting,
preventing a temporary width-1 measurement from retaining an oversized frame.

Cards project current `BattleActorState` HP/MP and progress maxima on every session presentation,
including updates within a single wait token. They never use prepared final reaction operands as
vitals. MP changes at the existing SpellCost commit; damage/recovery changes at Reaction. A full-HP
heal can spend MP without an HP event. Bars crop the fill image immediately; they cannot delay a
wait or progression. Zero maximum MP displays “—” and an empty track. HP at/below one quarter has
an explicit “! Low HP” cue; zero HP has “Defeated”. Growth uses the then-current maxima.

The asset inspection found no admitted gauge in the local asset repository at
`293c9460818f4768905be809e83488d77f698fba`. Four minimal public-authored CC0 PNG segments now live
under the ordinary game's [status art directory](../../game/assets/ui/battle-status/README.md),
with lossless/no-mipmap imports and nearest sampling. They contain no original game pixels and
make no original-art or hardware-fidelity claim. Candidate SVG meters are not mounted resources.
The [asset owner](../content/presentation-assets.md#public-authored-battle-gauges) records that boundary.

Acceptance uses adapter compilation, the committed-head scope plan and the
[bounded native recipe](../verification/host.md#battle-status-card-observation): public long/low/full/
dead/zero-MP, target/cancel/actor changes, damage/heal/cost phases, multi-target AURA/Prism and a
necessary private-local menu/HEAL seam. No engine/data behavior changes require new unit tests.
Retain completed observer failures and the existing focus/release/shutdown Unknowns. Independent
main-gate integration and broader aesthetic/all-map approval remain separate.

## Shared story dialogue agreement

**Standard rigor; [#1393](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1393).**
This bounded multi-consumer presentation change uses the accepted 1920×1080/96px world and
existing session text, portrait, choice, wait, input and audio authorities. Ordinary field talk,
source story text and existing battle-story dialogue share a blue/dark-blue panel, thin pale-gold
border, 30px body and binding-derived hints. Initial dialogue geometry is 1440×280 with a 32px
safe edge. UI overlays never resize or zoom the world. Extract field UI rather than growing its
controller; native Godot Theme values serve these actual consumers, with no general UI framework.

Acceptance covers private portrait/real Zone8 text, no-portrait authored text, reveal/all/instant,
actual input pauses, both choice answers and release gates, automatic/failure feedback, stable
subject-aware placement, close and fresh movement, and existing battle-story camera/waits. Measure
native text bounds; expand the panel for reached content and use display-only reading pages for
screen-sized overflow before acknowledging any script input. A window chooses top/bottom once
for its lifetime. Portrait side/mirror/eyes/mouth/resources stay with the existing presenter.
Names require admitted speaker-name metadata; raw entity/sprite identities are not names.

No gameplay, source text/clock/input/audio policy, general skip, mouse, settings/history/autoplay,
new font/art/export, whole-game migration or original hardware fidelity is included. Release adapter,
necessary Debug/native load, direct source/doc/private checks and committed-head scoped CI are the
gates; no new engine or observer tests and no research/full/H4 suite. Reuse the owned installation,
project and prepared private content. Pilot one private portrait seam and one authored choice/return;
then several finite semantic cases (minutes, small-MB output) with logs checked for runtime errors.
Repeated observer repairs without useful evidence or a new cross-system authority require replanning.
Retain prior focus-loss Unavailable, physical-release Unknown, missed-skip timing Unknown and exit
warnings. Independent main-gate review owns integration; this agreement is the approved modern
presentation direction and does not assert final aesthetic or all-map acceptance.

### Current story presentation

`ExplorationDialogueView` owns field text-to-label projection. The existing field controller still
owns incremental speech, reveal delivery and all commands; `BattleStoryView` retains its camera,
tick and acknowledgement boundary. `StoryDialoguePanel`, `StoryChoiceRow` and `ContextHint` own
only native controls/layout/reading position. `StoryTheme` supplies the native Theme also used by
the existing battle menu frames. There is no gameplay/wait/clock service in these widgets.

At 1080p the panel begins at 1440×280 (304px high with a portrait), uses 30px body/24px hints, and measures full shaped text
before incremental reveal. It expands within the 32px safe area; screen-sized overflow uses
Confirm-driven reading pages with a line of overlap. Reveal-all omits incremental speech, retains
the pending input and shows the full current segment. A later fresh Confirm reads remaining pages
before it can acknowledge. Field `CompleteTextReveal` also waits for that display coverage; it
remains distinct from script `Acknowledge`. Ordinary Cancel remains unable to skip dialogue.
Reading resets on an explicitly new window/segment, including identical text; source append spans
and temporary window hiding retain it. Attached choice rows cannot submit Confirm past unread
content. Source Yes/default/Cancel=No and release phases remain session-owned; authored choices
retain direct `ChooseDialogue` answers without a fabricated source selection phase. The current
[menu pointer agreement](#menu-pointer-agreement) adds shared focus and Confirm-on-highlight behavior.

Placement is selected once when a live window first becomes visible. It uses the projected speaker,
retained camera subject or player and moves to the top only if the bottom covers that subject and
the top is clear. Temporary `HideWindows` or parent hiding does not end the window. Opening/closing,
text changes and reveal never move/resize the world. An expanding long panel may occupy more of
the screen; it never chases the actor on every tick. Body, portrait, choices and measured hint rows
have separate reserved space. The portrait TextureRect is a child **after** the background, using
the existing presenter's cached texture, mirror/eye/mouth selector and source Work.Y movement
(mapped into its reserved column). The presenter retains resource lifetime and source lifecycle;
a second portrait service is not introduced.

The portrait's visible source area is 48×64, mounted at 192×256 with nearest sampling. The accepted
normal/mirrored 8×10 source window layouts contain a border around six columns and eight rows;
the two remaining atlas columns supply alternate animation tiles. Composition applies eye/mouth
replacements before cropping and mirroring. The last eight source rows contain intended visible
pixels for Sarah (1), Chester (2) and Astral (40); their earlier 48×56 crop discarded those pixels.
Their admitted atlases and animation entries match direct ROM decoding, so preparation stays
unchanged. The portrait-bearing panel minimum preserves 24px top/bottom margins at rest; source
movement remains inside that column. The [focused pixel recipe](../verification/host.md#complete-portrait-raster-observation)
checks the actual mounted textures and native bounds, including both sides and facial states.

There is no reliable admitted entity-to-speaker-name mapping in the current field definition.
Member-name substitutions in dialogue are not speaker attribution. Names are omitted deliberately;
no raw identity or sprite heuristic is displayed. Ordinary field chrome hides package/debug names
and shows only legal Talk and Hold Wait bindings. An automatic scene says “Scene in progress…”;
Continue appears only at a real eligible input. Only the existing eligible entry service offers
Skip battle intro. A failure replaces that chrome with “The scene has stopped” and the retained
message/code above world fades, with no invented retry or timeout. Diagnostic observations retain
package, tokens, raw identities and failure detail.

The [finite host recipe](../verification/host.md#shared-story-dialogue-observation) owns reproduction,
measured native results and limits. This is bounded modern presentation, not original timing,
all-map coverage or final aesthetic approval.

## Authored battle discard presentation

R / right stick down starts Drop from the ordinary battle ActionChoice; directions cycle exact
slot labels, including duplicates. Protected items show a readable refusal reason. Enter selects
and confirms; Escape keeps the item and returns to actions. The confirmation explicitly states
that the action and movement preview remain available. Bind `discard` through ordinary input
settings to change its keyboard/gamepad control.

`BattleDiscardView` displays authored result text and reports CompleteTextReveal only after actual
delivery; Confirm then acknowledges it. Parent focus/hide suspension blocks input and delivery,
preserving the accepted loadout/Deals prefix. It resumes without timing debt. The view chooses no
inventory/turn policy and acknowledgement returns to the same actor/action; later outer Cancel
returns movement. No new media or original dialogue/hardware timing is claimed. The
[native recipe](../development-and-verification.md#battle-discard-authored-observation) exercises
ordinary keys and reads live state, labels and errors without screenshots or gameplay injection.

## Authored battle Give presentation

In the ordinary battle action menu, G / left-stick press starts Give. Directions cycle the held
slots; Confirm selects the item. Directions or Target cycle eligible adjacent recipients; Confirm
selects the recipient. A full pack displays its return slots. A final confirmation states that the
transfer ends the action; Cancel backs up one stage. Slot numbers distinguish duplicate identities.
The query/selection state remains authoritative; the adapter does not calculate transfer legality.

`BattleGiveView` displays project-authored result text and completes the existing reveal token only
after delivery. Confirm then acknowledges it. The parent battle view suspends Give input and
reveal while unfocused/hidden; resumption advances only the pending stage. The existing instant or
adjustable text setting applies. This result text is a modern authored addition, not an original
Give scene, sound or timing claim. No new media assets are introduced.

The [compact native witness](../development-and-verification.md#battle-give-authored-observation)
uses ordinary host keys, visible labels and read-only session facts. It covers both custody routes,
selection cancellation, suspension and later usable control; it emits no screenshots.

## Authored Caravan projection

The ordinary exploration host draws the Caravan package with public geometric markers. Vehicle
sprite 62 uses a brown rectangle; player/followers use circles and facing marks. Blue tiles admit
vehicle travel; the green marker is a later ordinary walking zone. The presenter reads live raw
coordinates, appearance and per-refresh RenderSize, so off-map actors are clipped rather than removed
from gameplay. Ordinary Confirm finds adjacent targets using raw motion coordinates, so parked
physical actors never require an invalid layout position. `authoredDraws` readback carries the actual draw frame, position, size, visibility,
sprite and sprite-request identity. Sprite-ready completion follows a matching real draw, including
an intentionally clipped off-map marker. No arbitrary acknowledgement skips the typed program.

The same session supplies travel mode, current physical list, motion and waits. The host owns no
vehicle position/controller authority. Hidden/unfocused views suspend automatic ticks and sprite
completions; resume discards timing debt. Existing input envelopes and request IDs reject duplicate
or stale completion. The authored package has no audio assets: source warp-sound observations are
retained, but no original audible SFX parity is claimed. The finite witness reads actual state/input/
draw/errors without screenshots, session injection or synthetic presentation completion.

## Authored battle-region scene

`BattleStoryView` consumes shared StoryState dialogue/camera/tick waits while the mode remains Battle.
The authored scene shows project-written paraphrased text, focuses the existing map transform on the
retained visual slot, and accepts ordinary Confirm. A mounted presentation must survive a process
opportunity before completion or acknowledgement; hidden/suspended views do not deliver. CameraWait
completes after applying the actual map transform. This is modern semantic delivery, not original
camera speed, portrait, audio or frame timing. No menu/layout/input redesign is part of this slice.

`BattleMapViewport` maps logical actors to retained physical appearances. Conversion reuses the
existing marker/sprite node rather than allocating an actor or visual at the conversion boundary.
Initial placement resolves live coordinates once; acknowledged movement alone updates that physical
slot's position/facing. Logical conversion writes do not move the old ally slot before alias copy,
and departed subjects retain their last physical position. Normal movement animates that binding; dead-ally camera/text can retain the physical scene subject
without reviving gameplay HP. Effective control/activation, aliases, story cursor/callers/wait,
actual text visibility, camera slot/focus, marker direction indicator and geometry and failures are exposed by the existing
semantic observation API. Unsupported source physical services/private Battle32 media remain explicit.
See the [ordinary-key witness](../development-and-verification.md#jaro-conversion-authored-observation).

## Authored enemy appearance

`BattleAppearanceView` consumes the pending round's actor/list/token through the ordinary battle
view. The map focuses the current actor, hides later pending list members, and delivers an authored
0.6-second spin/fade,0.2-second settle and0.7-second paraphrased appearance notice. Reduced-flash mode
omits rotation. Focus loss suspends delivery. Only the actual view's completion submits the matching
`EntityEffect` token; the view never resets actors or generates a queue. Ordinary commands are blocked
during this finite wait and resume after appearances and automatic work.

The semantic observation includes current stage/token and actual marker rotation/alpha/visibility,
plus future-member hiding and final control. The [native recipe](../development-and-verification.md#hidden-spawn-authored-observation)
uses ordinary keys and verifies intermediate/final node values. This is a project-authored visual
with no original sprites, SFX, frame timing or natural-source reach claim. It adds no menu/layout or
input redesign to the separately owned UI work.

Godot is the outer composition, semantic input and presentation adapter. Its nodes/textures/audio
are projections; Application owns gameplay state, program/scene continuation and completion tokens.
See [audio](audio.md), [battle scenes](battle-scenes.md), [Content assets](../content/presentation-assets.md)
and [host observations](../verification/host.md).

## Ordinary Godot Host

[`Main.tscn`](../../game/Main.tscn) instantiates [`GameRoot`](../../game/src/GameRoot.cs).
The project references only Application, Content and Domain. Startup accepts one of
`--authored-package <path>`, `--private-battle-start <path>` or `--private-exploration-start <path>`,
plus optional `--input-settings <path>`. When no session-entry option is supplied, startup selects the authored practice yard,
including when only input settings are supplied. Unknown/positional, duplicate, conflicting and
missing-path options fail explicitly before creating a session. Diagnostic observer settings belong
to external probes, not game argument routing.

`SessionPresentationOwner` chooses the presenter from the current mode and retained entry
continuation. Field owns exploration and the unfinished before/load/start programs, including load
and start waits that already carry `ActiveBattle`. Once entry finishes, battle owns every battle
snapshot: initial round programs, appearances, ordinary/automatic control, terminal outcomes and
failures. This decision is shared by initial attachment, field handoff and field input dispatch;
`GameSession` supplies all lifecycle state.

The root installs entry and return callbacks before choosing the initial presenter. An admitted
connected package may start directly in battle, win through ordinary input and return to one usable
field view in the same session. Pending round delivery still uses the existing consumers and matching
tokens; routing does not complete waits or generate queues. The
[bounded native recipe](../development-and-verification.md#ordinary-host-lifecycle-observation) owns
startup, pre-battle and return controls. The accepted cardinal battle menu uses this same lifecycle
boundary for attachment, ordinary input and field return.

Both source-start paths use `RuleCompositions.ForGame()` and the same `GameSession`. The root routes
each input event once to the active view. Battle view pumps `AdvanceSimulation` only at the
Application's simulation wait; it does not choose rounds, enemy policy or control. Field/scene views
submit actual matching delivery/completion, retaining pending state when an adapter fails.

External scripts under `game/probes` drive ordinary input and read state. `ObservationCapture` is
diagnostic support: callbacks copy primitive/immutable facts to the ordered writer; its worker does
not read live nodes or session state. Writer failure retains an explicitly incomplete prefix.
The [retained capture contract](../evidence/retained-comparisons.md#bounded-h4-capture) is historical
protocol evidence, not authorization to resume bulk capture.

The export preset excludes `probes/*`. No current gate establishes complete ordinary export/package
contents; project configuration or a successful C# build cannot make that claim.

## Semantic Interface

This is the current Application/adapter contract for future art/UI work, with no UX or layout choice:

| Surface | Consumer obligation |
| --- | --- |
| `QueryBattleChoices()` | Consume the immutable `BattleChoicesSnapshot` session/revision/actor/stage/origin and ordered `Actions`. Each action supplies its typed reference, label, presenter kind, range, Empty, Enabled, Reason and ordered target references with their own Enabled/Reason. `Spells` and `Items` derive from those same offers. |
| Inventory | Present the returned slot/label/Empty/reason. Raw source words remain engine/content data; the view never masks item bits or infers usable items. Slot identity participates in the typed action and is rechecked at commit. |
| Selection and confirmation | Submit semantic `SelectBattleAction`/`SelectSpell`/`SelectItem`, `SelectTarget`, Move/Confirm/Cancel through the current `CommandEnvelope`. The engine owns live target legality and preparation. Requery after state changes; a disabled candidate or missing consumer retains its diagnostic. |
| State and effect projection | Render the current single-session snapshot and ordered scene/program requests. Selecting an existing presenter by `BattlePresentationKind` or cue kind is allowed; selecting rules by package/module/strategy name, calculating target factions/ranges or replaying observation strings as effects is not. |
| Completion/input | Submit actual matching token/kind delivery, acknowledgement, text reveal and eligible Wait separately. The engine owns PC/stack/timers, resource publication, queue and control release. Duplicate/stale completion rejects; displaying text does not acknowledge it or finish a timer. |
| Diagnostics | Display the typed failure and relevant context; preserve the reached pending scene/program state. Rule, Content and adapter failures have the separate ownership described above. |

The actual [`BattleSessionView`](../../game/src/Battles/BattleSessionView.cs) already consumes semantic
offers/targets/inventory through this interface. Domain has no outward Application/Content/Godot
dependency; Content admits data and its typed source opcode, while factories alone select concrete
implementations. The generic scheduler/runner and Godot have no new policy-name switches.
There is one current snapshot authority and one selected calculator per migrated seam.
S1 is resolved for implemented formulas; S2 for semantic actions/targets/inventory; S3 retains explicit
legality-versus-consumer support; S4 has the guarded source-story policy; S5 separates rule binding
from state/control/faction/order for the finite admitted set. Fixed refresh/activation/control,
movement-cost, field text/music/motion and battle-scene source-service algorithms remain the
explicit exclusions in the independently accepted bounded scope. #438 remains open for UI/UX
discussion with the user; Git integration and Epic closure remain main-gate responsibilities.

## Presentation Policy and Current Layout

The common design baseline is 1920×1080 (16:9). One map cell and ordinary 24×24 sprite
frame occupy 96×96 output pixels: both consumers use nearest sampling at four output pixels
per source pixel. The world fills the viewport (480×270 source pixels); UI overlays never resize
or zoom it. Other resolutions, free zoom and broader menu redesign remain outside the bounded world-layout
work. The shared story treatment above supplies the current narrow Theme implementation.

### Unified world viewport agreement

Standard rigor: this changes Godot presentation only. Application retains entity coordinates,
logical camera axes, collision, scripts, clocks and completion tokens. `WorldViewportLayout` owns
only disposable source-pixel camera geometry, shared by field and battle through `GameRoot`.
It does not store gameplay state. Both authored and private consumers use the same cell scale.

Field coordinates are sixteenth-source-pixel motion values divided by16. A battle cell is mapped
to the source map by adding `SourceMapRegion.Origin`, then multiplying by24. Sprite top-left and
foot alignment stay on that same transform. Battle terrain uses the inverse region translation.
A same-area, same-map handoff retains the presentation origin and pans toward the new subject; a different
map initializes its own frame under the existing transition. Disjoint packed rooms on the same
map also initialize within that transition; the camera never pans across unopened storage. Bounds may clamp a handoff when its
legal region changes; continuity never authorizes displaying packed neighboring rooms.

Ordinary exploration follows the player through a small dead zone with bounded linear panning.
An explicit followed story entity has priority. A detached logical camera maps its original
240×216 logical frame center (origin +120,+108) into the modern frame; logical scroll progress
remains authoritative. Plane origins are reconstructed from the presentation origin using each
plane's admitted parallax and offset, preserving the two-plane relationship. Display displacement
and quake do not write back to logical axes. CameraWait still requires actual settled delivery.

The current logical view area bounds the displayed world (traversal area for unbound authored
content), independently of packed 64×64 storage. Explicit scene origins select their own area,
independently of the retained player; non-player story follow targets bypass the player dead zone.
Its inclusive cell bounds become a half-open pixel rectangle. Rendering clips every plane and
actor to that rectangle and the viewport. Small areas center on exterior base color, never zoom
or reveal another area. High tile priority, actor ink masks and draw order remain in their owners.
Battle regions bound their own legal draw rectangle. Movement/target/story subjects pan into the
central safe area above the bottom UI; distant objects never cause a zoom change. When a legal
edge clamp prevents that placement, the overlapping corner UI panel moves vertically to avoid
the subject and the other currently visible panels. Menu visibility itself never changes the camera. A subject change
may move the camera, while menu/description visibility alone cannot do so.

At 1080p, body text is 30px, safe edges 32px and field dialogue reserves a 260px bottom overlay.
The diamond keeps its cardinal arrangement with crisp 96px icons. This is a first readable layout,
not a new component kit or an aesthetic acceptance claim. Acceptance is an adapter build and a
bounded native witness of private room/edge/occlusion, map/region handoff, battle movement/target
and UI geometry plus the existing input/reveal/entry-skip boundaries. No full/H4 replay or new
Domain tests are needed for this presentation-only change. The
[host recipe](../verification/host.md#unified-world-viewport-observation) records reproduction and limits.

The earlier 960-by-540 logical grid, 4x authored masters/2x and4x buckets, SVG chrome, product font
theme, UI-scale choices and safe-area model are retained presentation design/proposal context.
They are not an implemented ordinary-host layout or a new #438 UX decision. The legacy private
window/catalog consumers were retired. Keep simulation coordinates, presentation units, asset
pixels and physical output pixels distinct; none may change gameplay collision or rules.

Current configurable input, text speed/reveal, reduced flash and finite audio policies have their
specific owners. Broader art/theme/fullscreen/resizing/UI-scale work remains
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438); its selected initial
experiment is described below, with remaining product choices still open.
The [original proposal and legacy consumers](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#decision-summary)
preserve the exact design and receipts without declaring them current functionality.

### Battle movement range agreement

Standard rigor applies to B8 / [#1395](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1395):
a bounded read-only engine query and battlefield projection. The outcome is visible legal stopping
tiles during player movement selection, retaining the accepted 1920×1080 world and 96px cells.
Movement rules, world-click input, danger ranges, command animation and broader UI work are outside
this agreement. [Host observation](../verification/host.md#battle-movement-range-observation) owns
reproduction and its headless/windowed acceptance boundary.

`GameSession.QueryBattleMovementRange` returns immutable actor/origin/destination information only
for a living player actor in Movement, including that actor's preview/return animation. Domain
`BattleMovement.Destinations` builds the existing weighted mover/terrain/budget/enemy grid once,
then reuses Preview's path validation with that grid. Living allies remain traversable but cannot
be stops; enemies block traversal; the actor's own committed origin is included. Map bounds,
impassable terrain and budget remain authoritative. Unexpected path-validation failures propagate;
they are never silently relabeled as unreachable tiles. The query changes no session state, RNG,
resources, revision or observation sequence.

The origin is the committed actor position, independent of the provisional cursor and moving sprite.
Action, equipment, list, target and commit stages have no range. Automatic movement, rounds, story,
scenes, outcomes, retreat and field mode have no range. Cancel restores it from current state; next
actor and rebuilt map/session presentation compute fresh information. No new cache or invalidation
protocol is used. One grid plus bounded path checks runs when session state is presented, never per
render frame; camera animation only transforms the retained drawing. Work is bounded by the existing
48×48 movement grid and reachable paths, with no per-destination grid propagation.

`BattleMovementRangeView` draws quiet cyan fill (alpha 0.20) with an edge (alpha 0.55) inside each legal
96px cell, using the existing clipped board and camera transform. Ground is Z0; the range follows
ground at Z0; actor sprites/labels remain Z1; original foreground walls remain Z2; preview/path/target
marks remain Z3. Preserving foreground occlusion is part of acceptance. No new art, zoom, camera or
menu geometry is introduced. Source-original range pixels, animation and hardware cadence remain
**Unknown**; this is an accepted modern affordance, not an original presentation-parity claim.

### Modern UI experiment

The user-selected direction for #438 retains the original diamond command arrangement and spatial
input relationships. The [common-menu input contract](../../../docs/research/presentation/common-menus.md#confirmed-input-contracts)
maps up/left/right/down to choices 0/1/2/3; the
[battle action contract](../../../docs/design/contracts/battle/battle-functions-control-flow.md)
orders attack, magic, item and search-or-stay. Search admission remains a gameplay/content decision;
an interface experiment showing Stay does not remove the source-specific Search route.

Modernization separates world framing, UI dimensions and drawing resolution. The current experiment
uses the original square command rasters directly, arranged up/left/right/down. No additional button
frame or per-icon caption surrounds those rasters. One compact native-text window identifies the
focused command, with an unavailable reason when needed; main-menu keyboard instructions and generic
description paragraphs are hidden. Do not uniformly enlarge the entire original menu or preserve its
tiny physical size. Icons keep their original selected state; selected-item costs and unavailable reasons
sit against the right safe edge, leaving the middle open. Magic and inventory use list submenus, with explicit confirmation and a
cancel path back to the preceding selection. A vertical list replacing the main diamond is not the
selected direction.

The current 1920×1080 layout uses 96×96 icons (4× source pixels) in a compact 288×224 cardinal
cluster, with 64-pixel vertical steps between the top, side and bottom positions. This is a modern
spacing choice inspired by the source arrangement, not a pixel-exact original layout claim.
The battle map occupies the full game viewport; command menus, lists, hints and supplementary HUD
draw above it. Opening or closing UI must not resize the map, alter its zoom or reframe its current
focus. Gameplay movement/target changes use the shared fixed-scale pan policy above.
The earlier1280×720 menu observation remains historical acceptance of its own UI slice.

The existing graphics-remake experiment at commit `172662709b31cb296b55f51894754ce810c30f6c`,
owned by its `docs/ui-treatment-01.md` and `src/remaster/vector_ui.py`, supplies the window-chrome
reference. Its Yes/No sample is not a complete UI kit: placeholder text and the embedded private item
image must be separated from reusable frame geometry. Private art is not licensed by this selection.

The ordinary host now implements a first playable iteration through `BattleCommandMenu` and
`BattleMenuLayout`, using a shared selection-name window and the original 24×24 normal/selected
command rasters from a local private scene input. Icons use nearest sampling at integral scale;
icon positions and the shared text window scale independently. No authored replacement glyph is used. Public scenes
without these rasters show text-only squares. The [asset owner](../content/presentation-assets.md#private-command-menu-icons)
defines preparation and admission through the existing scene raster collection.
Standard rigor applies: one bounded interaction slice, with the existing semantic interface as its
dependency. Acceptance covers cardinal focus, spell/item lists, costs and unavailable reasons,
target selection, explicit commit, layered cancellation and actual resized node geometry. Engine
behavior tests cover back navigation; the [host recipe](../verification/host.md#diamond-menu-observation)
checks real keyboard input and projection. Final visual/user acceptance and wider Epic completion
remain separate from these engineering checks. Original pixels are private local inputs; neither the
tracked UI implementation nor public builds include them. The graphics sample remains a chrome reference.

Confirm at movement opens the diamond. Up/left/right/down focus Attack/Magic/Item/Stay; Confirm
enters the focused choice. F/H/I/Space are shortcuts from the main diamond; H/I now open lists,
not immediate selections. Up/down inspect list rows, including disabled rows. Confirm selects an
enabled offer and its first enabled target (self when offered). Arrows or Tab cycle all offered
targets; a disabled candidate shows its reason and cannot commit the previously accepted target.
Target confirmation commits through the session. Stay has its own confirmation step.
Equipment/Give/Discard retain their existing shortcuts and presenters from the main menu. Spell and
item lists group by the semantic offer's spell/item metadata, including admitted retreat offers.

Cancel returns target selection to its list or main diamond through `ReturnToBattleActions`, which
clears provisional action/target while preserving movement preview, resources and RNG. List cancel
returns to the same cardinal focus; main cancel uses the existing `Cancel` to return to movement and
undo the preview. Menu focus is disposable adapter state; actions and targets remain session-owned.
The UI never calculates range, faction or effect legality. Unsupported Search is not added by the
Stay slot. Actor labels retain current content identities; a complete naming/portrait treatment is
outside this iteration. Pointer activation and new controller acceptance are also outside scope.

Continuous exploration movement retains its [accepted control owner](../verification/field-programs.md#continuous-exploration-control-and-contextual-talk).
The shared 1080p world viewport is defined above; the earlier 640-by-360 proposal is not a runtime constant.

## Field Projection

Content validates the prepared world's raster shape/identity and required map/sprite/portrait
references before startup. `ExplorationPresentation` consumes those admitted visuals and current
layout/entity state. It does not admit a pinned asset repository, fixed comparison case or entity142
diagnostic trajectory. Logical source camera/text/motion services remain intentionally bounded.
Unknown source palette/layer/timing or final pixels remain unproved by modern node projection.

### Bound map-init foreground projection

The bound camera admits zero-autoscroll layer0/255, with unity/origin0 on the main plane and
secondary per-axis parallax128/256 and tile offsets0–63, within nonnegative signed-word positions.
[Source/behavior contract](../../../docs/design/contracts/world/map-exploration.md#bound-field-camera-lifecycle)
owns initialization, transforms, independent scroll speeds and main-plane follow. A and B logical
origins remain source state. Their modern draw origins derive from the shared presentation origin
with each plane's admitted parallax and offsets; foreground presence compares both offsets and parallax.
Ordinary actors use the area's main plane (B for0, A for255), selected before the source entity
loop; entity Layer selects signed window priority rather than coordinates. Observation exposes
each actor's actual origin. Equal unity planes still suppress duplicate foreground. Tile alpha,
clipping, priority and sprite ordering retain their existing owners. Readback compares draw tick,
logical state, modern draw origins, actual resources and geometry throughout changing plane separation.
Historical original-viewport camera readbacks retain their original scale expectations; the unified
viewport witness checks the modern mapping without rewriting those evidence records. Other layer,
autoscroll, word-wrap and raster/scroll-table profiles remain Unsupported or Unknown; this does
not establish original interrupt timing or final hardware pixels.

## Bound before-battle scene presentation

Closed-window bound scene loading admits the settled black fade/load/fade domain. It replaces the
map's logical base pair and retains black Current until actual fade service restores colors.
Map57 uses the existing map atlas/blocks and source135 sprite209/portrait31 bindings; the narrow
palette producer supplies its missing base pair in a fresh private start. The narrowly recompiled bbcs_01 row adds explicit null detach before the finite fade and one enabled common service after loading; other
start/world/party/asset/music inputs remain unchanged. Explicit scene origin selects the area's main-plane camera;
entity replacement does not re-center. Bound A/B drawing retains that origin even when the old
player is outside every new area; only unbound drawing selects legacy overlays by player location.
Actual text/portrait projection and input delivery use the
ordinary adapter. Bound physical target installation reuses this projection and the existing common
service; no player recentering or render smoothing is added. Valid invisible allocated targets remain
physical camera targets. Actual target movement precedes view/window/portrait service; idle input
presentation performs no such service. Bound white uses the finite helper projection below;
bound effects/first-input acceptance is described below. Later battle/return families remain open. [Tracking rule](../application/session-and-programs.md#bound-physical-camera-tracking).
[Behavior/limits](../application/session-and-programs.md#bound-before-battle-scene-and-windows).

### Bound white palette projection

`FullFadeWait.Color` drives the existing half-second fade delivery; logical service and the matching
actual token must both complete before script release. White resets parent Modulate to white so a
previous black endpoint cannot suppress the child ColorRect or leave fade-in black. Black clears
white alpha before delivery so an earlier white endpoint cannot cover it. Endpoints persist across
ordinary waits; reduced-flash suppresses only white opacity while retaining completion and gameplay.
The ordinary six-white/W1 boundary and authored mixed normal/reduced observations read actual nodes,
state, projection, input and errors. A newly published logical wait can precede its first projection
by one host frame; that delivery latency is not an extra logical opportunity.

The overlay covers the viewport as an accepted modern approximation. Logical PalettePair concerns
the source map palette; source-selected sprite/window colors, CRAM/VDP pixels and natural cadence
remain Unknown. Bound fade opportunities now use the existing live entity→view/window→registered
portrait service with explicit script enablement and one modern music admission; renderer duration
never supplies another service. [Behavior](../application/session-and-programs.md#bound-white-palette),
[verification](../verification/field-programs.md#bound-white-palette-observation).

### Bound generic effects and battle-loader facade

The existing mosaic/shiver renderer consumes the same effect tokens and restores engine fields on
actual completion. Bound generic presentation waits suspend logical services and actual delivery
while hidden/unfocused; the host clears debt and resumes with zero delta. The retained profile also
suspends the battle loader's modern delivery-only fades. No extra presentation clock is introduced.

Field palette helpers require an active exploration world. After initialization, retained field
Display and frozen LogicalView serve the old-scene facade without driving field services. Existing
black out → BattleLoad mount → black in remains a modern delivery-only sequence; matching completion
precedes start/first input. It does not claim original LoadBattle callback opportunities or hardware
palette pixels. Asset bindings, source null detach/post-load wait, viewport approximation and inputs
remain unchanged. [Execution](../application/session-and-programs.md#bound-before-body-and-battle-entry),
[observation](../verification/field-programs.md#bound-before-body-and-battle-entry-observation).

### Bound outcome projection lifecycle

At outcome entry the new exploration world owns a freshly initialized logical view and closed
text/portrait windows; the loader's frozen old-field projection is not reused as that authority.
Carried Display and the live party/RNG retain their existing owners. Explicit after/return windows
keep the actual outcome continuation and anchor through map initialization; confirmation requires
the existing reveal/input phase and admitted registered portrait. The existing renderer delivers
scene effects and fades. Original pixels, timing and audio fidelity remain Unknown.

The ordinary bound winning observation consumes actual movement, scene messages, automatic turns,
growth/death and after-program effects before releasing the battle facade. The result signal at the
battle→field boundary precedes installation of the returning field view: observers retain that
payload and read the new field projection after attach, rather than query the old battle view in
exploration mode. Target-browse range rejections retain accepted gameplay state while the attempted
UI candidate changes, as specified by the existing architecture owner.

### Continuous mounted-resource and audio readback

`BattleSceneView.Bind` records a resource ID on its existing Sprite2D only after successful raster
decode/size validation and texture assignment. `Observe` exposes background, backgroundWrap and
ground node-bound IDs, texture presence and actual tree visibility, alongside existing actor/weapon
IDs and completion state. A configured content ID or retained metadata on a hidden node is not a new
consumption observation. The H4 input probe records changed scene facts, distinguishing the result
signal before Present from later host polls. Completion must be actually observed; elapsed time or
the next phase does not synthesize a receipt.

The same probe uses `GameRoot.ReadAudioObservationJson` throughout an H4 session to retain every
unseen receipt from the existing 64-entry window. Sequence checks include initial overflow and the
terminal read, with receipt revision/time/wait token distinct from poll session/input/frame context.
Started, stopped, finished and fade-command remain distinct facts. Cue totals can reconcile with
terminal voices; a wait token is not a playback identity, same-cue overlap is not assigned invented
generations, and ongoing field music is not required to end at observation termination. The older
D speech subset stays separately available. Missing receipt ranges and unobserved scene edges remain
explicit. This readback changes neither the audio buffer nor scene/audio playback behavior.

See [same-session observation](../evidence/retained-comparisons.md#same-session-actual-observation) for
input freezing and session boundaries. New A observations do not fill omitted fields in old A-D02
records or prove complete original resource provenance and dependent consumer bindings.

### Bounded working-layout and draw operands

`ExplorationSessionView.ObserveLayoutRegion(x,y,width,height)` explicitly enables a local observation
of at most256 cells inside the64×64 working layout. `ReadLayoutRegion` reads authoritative row-major
words, current flags and intersecting saved roof words; it does not advance the session. The latest
successful draw carries request, session, map, revision, observation-sequence and fresh draw identity.
Readers must match those identities and rectangle, and reject missing draws or overflow.
`StopLayoutObservation` releases the observation. Disabled rendering allocates no added observation
collections.

`ExplorationPresentation.DrawLayer` records coordinate-specific use immediately after the actual
`DrawTextureRectRegion` call. Records retain selected block/tile words, texture instance and selector,
background/foreground/occlusion, pass, priority, actor-mask identity, offsets and clipped screen/source
rectangles. The cap is4096 records per draw; overflow is explicit. These are actual draw operands, not
GPU pixels or a coverage guarantee for every cell in the requested rectangle. Existing deduplicated
resource requirements are not the expected-value authority for this observation.

**Confirmed:** the private `map-layout` case in
[`engine_private_exploration_observation.gd`](../../game/probes/engine_private_exploration_observation.gd)
uses real GameRoot controlled starts and keyboard input. From Map3(4,7), DOWN opens the(4,8) door;
DOWN restores the roof, UP clears it, and DOWN restores it again. Door post-state is the pre-operation
source word at(62,0). Roof restoration uses the56 original words in(2,32),7×8, independently retained
from the selected content before roof-on-load clears them. Repeated reads/redraws preserve revision,
observation sequence and working words while requiring new draw identities. The retained local case
joins the door cell and21 visible restored roof coordinates to actual block/tile use; all56 roof
words are checked, but offscreen cells have no draw claim. Cleared roof cells remain in the viewport
and issue no texture draw. The separate flag-off controlled load at(28,24) observes both destination
cells at(28,22),1×2 and stable repeated reads.

Source operands are the first records in pinned SF2DISASM commit
`c834c652b6862bc5679fd7f69a38a7093206efc6`, under
`disasm/data/maps/entries/map03/{4-step-events,5-roof-events,3-flag-events}.asm`:
copy(62,0)→(4,8),1×1; clear/save/restore(2,32),7×8 (source255,255 denotes clear);
and flag506 copy(23,23)→(28,22),1×2. Inspect those named records with `git show` in the pinned checkout.
The independently selected content supplies the pre-load layout words and block tile table; applying
these source operations gives expected working words and `word & 1023` gives the expected block.
This local check does not prove original natural reach, original display cadence or raster fidelity.

**Confirmed:** the flag506-on attempt fails at startup with `UnsupportedCapability: map-setup
(ms-map3-flag506)`. `MapTransfer.ValidateSetup` rejects the non-default setup before layout loading.
No variant substitution or setup bypass is admitted. **Unknown:** successful flag-on copy/draw use,
including the second flag506 copy into(57,23), and complete H4 map/layer comparison. The unsupported
alternate setup is not automatically a required gap for the selected H4 route. Applicability needs
the actual caller/load flags at every reached Map3 load joined to the source506 gate; final flags,
one controlled start and static metadata cannot establish that route-wide result. The retained small
W2 selections do not cover every Map3 load, so that binding remains **Unknown**. Providing these
operands does not close H4 item9 or the other outstanding obligations.
