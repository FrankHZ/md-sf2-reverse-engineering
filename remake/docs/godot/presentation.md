# Godot Host and Presentation

Godot is the outer composition, semantic input and presentation adapter. Its nodes/textures/audio
are projections; Application owns gameplay state, program/scene continuation and completion tokens.
See [audio](audio.md), [battle scenes](battle-scenes.md), [Content assets](../content/presentation-assets.md)
and [host observations](../verification/host.md).

## Ordinary Godot Host

[`Main.tscn`](../../game/Main.tscn) instantiates [`GameRoot`](../../game/src/GameRoot.cs).
The project references only Application, Content and Domain. Startup accepts one of
`--authored-package <path>`, `--private-battle-start <path>` or `--private-exploration-start <path>`,
plus optional `--input-settings <path>`. No session-entry option selects the authored practice yard,
including when only input settings are supplied. Unknown/positional, duplicate, conflicting and
missing-path options fail explicitly before creating a session. Diagnostic observer settings belong
to external probes, not game argument routing.

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

The current battle view responds to actual viewport size, with a scrollable HUD beside the map in
wide windows and below it in narrow windows. Map framing follows current origin/path/target; HUD
coordinates do not restrict legal maps or actor positions. Field presentation uses a 320-by-192
source view and current logical camera/layout state. Texture/node caches are disposable projections.

The earlier 960-by-540 logical grid, 4x authored masters/2x and4x buckets, SVG chrome, product font
theme, UI-scale choices and safe-area model are retained presentation design/proposal context.
They are not an implemented ordinary-host layout or a new #438 UX decision. The legacy private
window/catalog consumers were retired. Keep simulation coordinates, presentation units, asset
pixels and physical output pixels distinct; none may change gameplay collision or rules.

Current configurable input, text speed/reveal, reduced flash and finite audio policies have their
specific owners. Broader art/theme/fullscreen/resizing/UI-scale work remains
[#438](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438), pending user discussion.
The [original proposal and legacy consumers](https://github.com/FrankHZ/md-sf2-reverse-engineering/blob/1c4c786a6e2c630e1cfadf4daa88dbd7687caa19/remake/docs/presentation-and-assets.md#decision-summary)
preserve the exact design and receipts without declaring them current functionality.

## Field Projection

Content validates the prepared world's raster shape/identity and required map/sprite/portrait
references before startup. `ExplorationPresentation` consumes those admitted visuals and current
layout/entity state. It does not admit a pinned asset repository, fixed comparison case or entity142
diagnostic trajectory. Logical source camera/text/motion services remain intentionally bounded.
Unknown source palette/layer/timing or final pixels remain unproved by modern node projection.

### Bound map-init foreground projection

The bound camera admits zero-autoscroll layer0/255, with unity/origin0 on the main plane and
secondary per-axis parallax128/256 and tile offsets0–63, within nonnegative signed-word positions.
[Source/behavior contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle)
owns initialization, transforms, independent scroll speeds and main-plane follow. A and B tile
draws use their respective origins; foreground presence compares both offsets and parallax.
Ordinary actors use the area's main plane (B for0, A for255), selected before the source entity
loop; entity Layer selects signed window priority rather than coordinates. Observation exposes
each actor's actual origin. Equal unity planes still suppress duplicate foreground. Tile alpha,
clipping, priority and sprite ordering retain their existing owners. Readback compares draw tick,
logical origins, actual resources and geometry throughout changing plane separation. Other layer,
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
