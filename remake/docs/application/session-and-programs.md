# Session and Resumable Programs

Application owns command admission, continuation and snapshot publication across exploration,
battle, scene work and return. [Domain rules](../domain/gameplay-rules.md) supply selected algorithms;
[Content](../content/profiles-and-trust.md) supplies admitted immutable definitions and explicit starts.

| Work | Read |
| --- | --- |
| State publication and failures | [Session authority](#session-authority) |
| Typed content and conditional authoring | [Definitions](#definitions-and-execution), [authoring](#authoring-conditional-programs), [source policy](#source-story-policy) |
| Dialogue and field input | [Portrait/plain wait](#portrait-lifecycle-and-plain-current-input-wait), [W1](#w1-in-a-suppressed-entity-event), [bound text](#bound-field-text-work), [yes/no](#source-bound-yesno-lifecycle) |
| Field services | [Camera](#source-bound-camera), [nod](#source-bound-nod), [music](#modern-finite-music), [raw text](#raw-field-display) |
| Connected source programs | [Opening](#original-map-3-opening), [castle/tower](#castle-palace-astral-and-tower), [battle entry](#battle01-admission-and-first-input), [outcome/return](#battle01-outcome-after-program-and-return) |
| Original limits | [Remaining source boundaries](#remaining-source-boundaries) |

## Session Authority

`Runtime.GameSession.Start` accepts either an `IScenarioSource` or admitted definition/start pair.
It reads external content once and binds `SessionRules` before creating live actors. One `Submit`
checks the command envelope and dispatches the explicit `ActiveBattle` / `ActiveExploration` mode.
`GameSession` alone assigns Current. `ExplorationDispatcher`, `ProgramRunner`, `EntityActionRunner`,
`MapTransfer`, `BattleAdvancer` and scene/movement/outcome continuations compute the next result.

`SessionSnapshot` carries the active state and shared `StoryState`; it does not store a reference
trajectory or scenario-specific receipt chain. Mode-specific access is explicit. Keep immutable
observations and preserve unrelated state when transitioning. A failed preparation retains the
previous state; later failure retains already published effects, current cursor/callers and waits.
`SessionFailure` distinguishes IllegalCommand, UnsupportedCapability, ContentError, InvariantFailure
and AdapterError. Selected-policy failures include identity/operation and source-story PC context.

The [field/program verification owner](../verification/field-programs.md) holds preparation and
actual-input/state recipes. Native projection belongs to [Godot](../godot/presentation.md), not to
these execution rules.

## Current capability

The ordinary `GameSession` executes exploration, resumable story programs and map transfer, then
hands the same session to the existing battle flow. `harbor-arrival.json` and `hill-passage.json`
exercise different maps, actor identities, dialogue, choices and motion through this path. Accepting
runs entity actions, a call/return and a timer, transfers maps, executes map init and both intro
hooks, initializes the encounter and reaches first player control. Declining returns field input.
Neither package identity nor an expected route/receipt admits a command.

Exploration and return consume the same [logical input/settings owner](../verification/host.md#logical-input-and-accessibility-adr-0010-9a) as battle. Confirm talks/acknowledges/accepts and Cancel declines. Text reveal stays in the adapter; W1 delivery reports its completion separately from player acknowledgement, including automatic completion of a trailing span. Reduced-flash suppresses the reached white overlay while completing its real token/kind through the existing presentation service, an intentional 9A deviation.

`GameSession` remains the sole snapshot publisher. `ExplorationDispatcher`, `ProgramRunner`,
`EntityActionRunner`, `SceneEntities`, `MapTransfer`, `BattleEntry` and `BattleOutcome` compute immutable results. The active payload is either
`ActiveExploration` or `ActiveBattle`; story flags, program PC/call stack, typed wait, text window,
simulation tick and observations survive the switch. The battle view attaches to the existing
session. It cannot restart it at the transfer endpoint. Startup and transfer use the same derived
`HasBattleControl` boundary: an active battle with a pending program or wait keeps the program view
until its dialogue, choice, timer or presentation service finishes, then exposes ordinary battle
control.

Before battle entry, `ExplorationState.Party` owns the explicit candidate resources, seeds and
accounting. Joined/active membership is separately owned by story flags and source-order counted
lists; follower links refer to physical map slots. `JoinForce` refreshes counted lists before its
active-flag write, so the immediately exposed list can lag that final write. This is intentional
source chronology. The opening input supplies the first three R1 allies' class, resources,
equipment and spells. Other initial ally appearances and names come from pinned source data;
this group does not claim a full travelling roster, mutable names, promotion/death appearance,
general field inventory operations or later battles. At Battle01 admission the sequencer rebuilds active membership
from live flags, so the earlier `JoinForce` list timing never forces a missing ally into battle.

## Definitions and execution

Format-v7 battle packages remain current. Format-v8 exploration packages contain `package`, an
embedded format-v7 `battle` definition/start, `world` and `start`. World data contains maps, ordered
events, programs and text; start data contains the selected map/player, position/facing/speed and
flags. An optional start `program` invokes instruction zero of a whole program in a fresh session.
It does not import a PC inside that program, a call stack, an active wait or any executed history.

The Content reader links all call/jump/branch targets, including untaken branches, and checks map
references, duplicate IDs, scalar domains and terminal fallthrough. Entity references that depend
on runtime population are checked when reached. Typed operations support flags, calls/returns,
conditional branches, text cursors, continued/single text, explicit close, yes/no, fixed-point
entity motion, facing/sprite/position/visibility, tick waits, presentation requests and map transfers.
Known unimplemented operations retain a source-attributed stop instruction.

A blocking instruction keeps its PC until its own wait completes. A stale token/revision cannot
release it. Continued text remains visible after acknowledgement until close or replacement;
legacy single text closes on acknowledgement. Source-produced text uses explicit window operations
described below. Speaker flag bytes remain available for speech/display, separately from the
persistent portrait gate. Real ticks advance background actions at legacy open-text consumers,
and increment `SimulationTick` only when executed. The admitted plain input-first consumer instead
requires explicit player Wait; delivery/reveal supplies none. A tick batch stops at a newly reached
wait boundary. The host batches elapsed 60 Hz ticks and subtracts only ticks the engine executed.
Unused time remains available for automatic continuation on the next frame; reaching paused input
clears the accumulator. Lower frame rates therefore retain elapsed ticks without draining a newly
reached dialogue or choosing an answer automatically. Existing field input does not truncate a
background action batch; completing a foreground wait still yields at the newly reached input or
program boundary before further ticks are submitted.

### Authoring conditional programs

Use the existing public v8 envelope and typed operations. Edit `world.programs[].instructions`,
link targets with `{program,instruction}`, and select the entry through an ordinary map event or
whole-program start. The reader admits all targets, including untaken branches, before creating a
session. No C# implementation name or package-name branch selects the executed instructions.
Content is read once: restart after a JSON edit; compiled rule edits also require the
[rule author workflow](../domain/gameplay-rules.md#rule-author-workflow-and-diagnostics).

[Story A](../../content/authored/story-rule-demo-a.json) and
[story B](../../content/authored/story-rule-demo-b.json) use the same quay, player, ferryman interaction,
marker and ordinary battle definition. A's `invitation` branches when live flag40 is set to
`repeat`; its initial fallthrough calls `first-a` and sets40. B reverses the condition: when40
is clear it branches to `first-visit-b`, calls `first-b` and sets40; its later fallthrough calls
`again-b` and clears40. A's repeat calls `again-a` and also clears40. The branch condition, targets,
called programs and effects are external data; both outcomes run through the same runner.
Each called program calls `pause-a` or `pause-b`, leaving two real return addresses on the stack.
The pause displays its package's text, waits for real acknowledgement, waits30 logical services,
then returns to the called program's later effects and finally to field input.

| Content/branch | Before dialogue/timer | After nested pause returns |
| --- | --- | --- |
| A, flag40 clear | Set41, hide marker at(4,3) | Set42, position(4,2), show marker, return and set40 |
| A, flag40 set | Set43, show marker at(4,2) | Set44, position(3,2), hide marker, return and clear40 |
| B, flag40 clear | Set51, hide marker at(4,3) | Set52, position(3,4), show marker, return and set40 |
| B, flag40 set | Set53, show marker at(3,4) | Set54, position(4,4), hide marker, return and clear40 |

For the compiled demonstration choose `RuleCompositions.AuthoredStory()` before startup. The two
files run unchanged through the same runner and composition, with real Confirm inputs followed by
West movement. Source-only compatibility is unsupported under that composition; ordinary programs
do not invoke it. The host's committed default remains SF2, which also executes ordinary authored
programs. [Verification](../verification/field-programs.md#replaceable-story-observation) gives the
short observer and exact recipe. This is project-authored replacement evidence, not original-game
reach, frame timing, private presentation completeness or a no-compile algorithm platform.

While `ShowText` or `WaitProgramTicks` is pending, its PC remains at that blocking instruction;
no later flag/entity effect executes. The engine releases the exact token, advances that PC and
returns each nested caller in order. Unsupported reached instructions or selected source-policy
errors retain prior effects, cursor and callers; malformed references reject before start.
Wrong, stale and duplicate completions cannot skip the wait or replay a later effect.

### Portrait lifecycle and plain current-input wait

`StoryState.PortraitWindow` is persistent Unknown, Closed or Open (portrait identity and packed
flags). `open-portrait` takes an entity and flags: an existing Open or Unknown gate is retained;
a null entity represents a skipped lookup and also preserves the gate. From Closed, the resolved
entity's current sprite metadata supplies the portrait, or a known absent portrait keeps Closed.
Missing metadata becomes Unknown. Only executed `close-portrait` establishes Closed. Text-only
close, raw text and JOIN preserve it; calls, returns and branches carry it. The Godot portrait
projection consumes this state independently of the text window and retains an opened identity
even if later text has another speaker. Close is an atomic engine lifecycle operation; it does
not claim original window-movement timing or its VInt work.

The real producer emits `explicitWindows: true` on `show-text`; its acknowledgement never closes
either window implicitly. `nextSingleText` lowers to portrait lookup, DisplayText acknowledgement,
portrait close, text close and mandatory Sleep(10). `nextText` has no close tail. Packed FFFF skips
the portrait lookup without closing an existing portrait. Raw `txt` only displays text; raw `clsTxt`
only closes text. `closeTxt` closes portrait then text. JOIN's emitted SoundWait → PreviousMusic →
`wait-text-input` → text close → Sleep(10) retains the incoming portrait gate. Bound execution stops
at SoundWait without a finite profile; see [modern finite music](#modern-finite-music). These rules are
implemented by the existing compiler/typed reader, with no selected-script eligibility exception.
Fresh output is required to gain these distinctions; old generated content cannot reconstruct them.

**Confirmed source:** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`,
`mapscriptengine_2.asm:csc00_displaySingleTextbox`, `csc02_displayTextbox`,
`csc08_joinForce`, `csc09_hideDialogueAndPortraitWindows`; `mapscriptengine_1.asm:csc1D_showPortrait`;
`trap5_textbox.asm`, `portraitwindow.asm:ClosePortraitWindow` and
`mapsetupsfunctions_1.asm:DisplayCurrentPortrait`. A source-wide cutscene text-skip mode is not
implemented; unsupported native operations remain stops. The FFFF lookup skip is distinct from it.

Initial/restored portrait provenance is Unknown. An initial source open cannot resolve a possibly
existing window, so it stays Unknown until a real close executes; no asset absence proves Closed.
Legacy `show-text` without explicit window operations retains its old display hint inside Unknown
state and its old single-text auto-close policy. That hint never admits the plain consumer. Thus
legacy content remains readable but cannot claim the new source lifecycle or natural initial portrait.

`WaitForTextInput` records the input-first consumer and its current program's established
`EntitiesRunning` setting. `WaitForText(token)` is admitted only in exploration presentation wait,
with an open text window, proven Closed portrait, field continuation, no pending battle entry,
no busy player and no pending entity sprite readiness. Calls may remain on the stack. One Wait
executes the existing entity service once when enabled, retaining physical-slot order and shared
RNG rules; a disabled service produces no entity draw. `Acknowledge` returns immediately with no
accepting-poll tick, then executes subsequent explicit operations. Generic `AdvanceSimulation`
is rejected at this admitted consumer. The [host contract and reproduction](../verification/field-programs.md#plain-text-input-gameplay-wait)
cover reveal, delivery and input rearm.

The [natural JOIN observation](../../../docs/research/map3-messenger-acceptance.md#natural-join-audio-and-input-boundary)
confirms its named helper entry/return. **Unknown:** joint logical audio/service progress, the complete
enabled VInt table and portrait counter chronology. The current program's entity setting is
the admitted engine service state, not a sampled original service table. W2, active/unknown portrait
consumers and preceding audio delivery retain their separate legacy boundaries. The bounded W1
consumer below does not establish whole-host Option A, 9A or H4 conformance.

### W1 in a suppressed entity event

The reader retains raw text for compatibility and an ordered token stream for execution: literal
spans, `{N}`, `{LEADER}`, `{NAME;n}` and each `{W1}` occurrence. Names are substituted as literal
data; their contents are never parsed again as controls. Any unsupported control, malformed brace
or unavailable name excludes the entire text from this capability. W2 cannot become W1.

`ShowText` admits this stream only with explicit source windows, a live `EntityEventContext`,
field continuation without battle entry, an established Closed portrait, resolved event-actor
sprite metadata explicitly declaring no portrait, and `EntitiesRunning=false` on the executing
program. No map, text, actor, seed or expected endpoint identity participates in admission. Without explicit bound field-text settings, other
consumers retain legacy display/acknowledgement and cannot claim this W1 contract.

`W1TextWait` keeps the displayed text ID, exclusive token endpoint, delivery state and whether
the endpoint is a W1 or the final trailing span. Every optional `WaitForText` and the accepting
`Acknowledge` executes the same ordered preamble: main-seed draw256, source copy-byte write, one
logical suppressed-service wait, then input decision. Observations expose those four stages.
The copy commit writes the result into bits31..24 of the party's `ThinkingSeed` image, retaining
its other24 bits. That image is the live authority carried into battle, updated by AI and carried
back through battle outcome. `StoryState.RandomSeedCopy` is only the last text-write diagnostic:
it is nullable until an admitted text write and survives ordinary copies, including later AI
changes. `SessionSnapshot.CurrentRandomSeedCopy` always derives the current byte from the active
party/battle image; a declared modern start supplies a value even before any text write. This does
not confirm the original game's initial byte. The host reports this live byte as `randomSeedCopy`
and the historical text write separately as `lastTextSeedCopy`. Entity actions/motion/followers and portrait
RNG receive no service in this admitted wait. The source wrapper's existing pre-facing service
still happens before admission; ordinary return/control and other programs keep their own rules.

`CompleteTextReveal` is an actual host-delivery receipt, consumes no tick/RNG, and cannot accept
a W1. Wait and Ack reject incomplete delivery. Each accepted W1 creates a fresh token for the
next span; the host retains already shown characters and reveals only up to that next endpoint.
The full projection remains mounted for at least one host frame before the delivery receipt.
A trailing span then completes automatically, with no invented manual Ack or logical tick.
Generic elapsed `AdvanceSimulation` is rejected for all W1 delivery/input states. Confirm can
reveal text without polling; accepting W1 does not play generic SFX67. Plain JOIN and choice
semantics remain with their separate owners.

**Confirmed (source):** the [reached W1 contract](../../../docs/design/contracts/dialogue-system.md#reached-w1-consumer-binding)
owns `textfunctions_1.asm:symbol_wait1/loc_659C/loc_65B4` and the distinct W2 tail at pinned
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. The bounded ordinary caller is
`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` → `map03/mapsetups/s2_entityevents.asm:Map3_EntityEvent2`,
F602 clear → raw text483. Its actor128 uses sprite195/WOMAN with `PORTRAIT_NONE` in
`spritedialogproperties.asm`; the wrapper faces before suppressing entities, then closes/reactivates
after return. `WaitForVInt` includes its enable/handshake. Closed portrait excludes blink/mouth
service. Quake consumes RNG only for nonzero `QUAKE_AMPLITUDE`: the intro clears it and csc33 is
the source setter; this reached Map3 path contains no setter. The later retained prepared-68
`FFA80C=0` supports that ancestry, not a direct text483 service sample. No quake capability is added.

**Confirmed (remake):** ordinary physical facing/Interact after the existing three-step opening
setup reaches this caller and the required gates, then 0/1/3 optional Waits plus actual Ack give
1/2/4 polls. [Verification and limits](../verification/field-programs.md#w1-entity-event-input)
record the actual settings comparison and preserved failures. **Unknown:** original opening
W2/typewrite/entity timing, general portrait/service tables and whole-route 9A/H4 remain open.

Movement uses the existing `OriginalMapTraversal` area, collision and stair rules. The extracted
entity core uses 384 fixed units per tile, source signed-word arithmetic, acceleration/deceleration,
destination obstruction, facing/animation and arrival layer/immersed changes. Each entity's movement
runs before its action dispatch. Nonwaiting configuration actions execute in the same tick. Source
`ac_moveRel` installs a relative destination and redispatches immediately; shorthand `moveRight` and
its siblings additionally wait for arrival. Timed reversal can therefore replace an unfinished
destination, as in the palace's entity action streams. Replacing an action stream retains current physical motion. Source `setPos`
changes position, destination and facing without resetting unrelated speed/flags/action state.
`SPRITE_SIZE` is global. A later failing action preserves completed configuration and movement in
that tick and retains the failing action cursor. No destination assignment replaces a motion path.

Source motion installation has an optional `installation` field: `Preserve` (the authored
default and native MakeEntityWalk), `SlotTimer` (custom/named cutscene scripts), or
`SlotTimerClearCollision` (entityActions sequences). The latter two use the resolved physical
slot as the wait timer; only the last also clears flagsA bits5/6. Existing source aliases
determine the slot at runtime. No timer value is baked into compiled content.

A true source jump-to-idle compiles to `jump` plus terminal `idle`; entityActions gets its
implicit source idle tail. Inline bytes after an unconditional idle jump are consumed through
the delimiter without becoming executable. Plain authored Stop/exhaustion or Unsupported
does not imply idle. The terminal stays in the existing Actions/cursor, services wait1/branch
once per existing slot service, and contributes no active-script Busy. Direct installation
preserves its timer; the preceding source jump clears it before idle runs in that same service.
Source cutscene motion waits require script idle, so a following caller may run while physical
travel remains; ordinary field movement and authored default waits retain their physical/busy
completion rule. No active infinite idle loop or second state authority is involved.

The [idle/caller source owner](../../../docs/research/map3-controlled-start-egress-transition.md#source-idle-completion-and-caller-installation)
records the Zone6 leading-wait consumer and the boundaries. Ordinary player control still
supersedes its old action/follower continuation and establishes controlled motion settings after
movement at the next existing service. Follower installation clears Actions; source phase
startup still binds real walking wait cursors. General high-bit source wait fidelity and the
unimplemented waitIdle/native producers remain outside this bounded correction.

Source `MainLoop` selects an unlocked, uncompleted battle before `ExplorationLoop` initializes its map.
The before-battle program retains the previous field scene until its own load/entity calls.
Authored map-init routing remains explicit in its definitions. Entry runs
**before program → region/party/enemy initialization → battle load → start program → first round**.
Both hooks check the same intro flag; the start wrapper sets it. The internal `BeforeBattleRouted`
policy is produced only after Application completes that routing. External standalone starts still
require the existing explicit controlled skip. Battle region state and its activation rules retain
their existing Domain owner; general story access to battle-region flag aliases is outside the
selected private programs described below.

## Bound field text work

The explicit `start.textSettings` profile (`messageSpeed`, `mouthControl`, `viewSpeed`) enables
source text work for field programs. It requires a known Closed portrait with an explicitly
portrait-less speaker, or the registered entity/zone caller portrait described below, regular font
metadata, and a supported logical view. Names,
text/actor IDs, seeds, routes and receipt counts never select this capability. A start without
this profile retains its existing plain/W1/legacy consumer. An unsupported bound context stops;
it cannot silently use display latency as gameplay work. Battle continuation, unadmitted caller portraits,
scene camera entity changes, cursor targets, scrolling overrides, autoscroll and non-unity parallax
are outside this profile. Quake and pulsating fade variants have no admitted implementation.

`world.textFont` contains the existing source reader's 256 ASCII-to-symbol entries and 80 glyph
advances. The private producer reuses `build_variable_width_font_contract`: it checks the registered
USA ROM, existing H1 symbol listing, split-font bytes, font pointer and ASCII table parity. The
split font is a private binary, not a pinned Git blob. Only widths/mapping enter Content; no bitmap,
comparison fixture or runtime reference dependency is added. Area `view` metadata carries bounds,
foreground/background offsets, per-plane parallax/autoscroll and layer. The selected profile is
layer0, zero background offset, unity parallax and zero autoscroll. Authored width/name/area changes
execute through the same rules. Source IDs7C/7D bypass typewriting, but cannot occur in the admitted
normal symbol range1–80; other control/font families remain unsupported.

`FieldTextWait` separates the ordered glyph/control cursor, mandatory phase and actual delivery.
It preserves the current span and the first-regular-glyph bit across W1/W2 occurrences. DisplayText
resets that bit (source DIALOGUE_REGULAR_TILE_TOGGLE) and selects regular font1, while a reused
window retains X/Y/row. Names are substituted as literal glyphs, never reparsed as controls. LEADER uses the first active
member after rebuilding the source party flags; authored content without those flags names member0.
A fresh window performs two clear/DMA opportunities, creates the source29×8 window at(2,29),
and services its eight-step move to(2,19). Every glyph applies the first-glyph/automatic newline
rule, advances X by its symbol width, performs one cursor/DMA opportunity and then the source
speed0/1/2/3 delay of4/2/1/0 opportunities. X>204 wraps before the next glyph; a newline adds16
and Y>=48 performs two row-scroll waits plus a final wait before subtracting16. Row offset wraps
modulo6 for this bound event/black-bar style. Close services eight movement steps and the final
moving-bit clear observation. Reopening starts a fresh layout.

Neutral mandatory work supplies logical input0. Source nonzero input shortens the extra glyph
delay only when mouth control is0; the engine tests that rule, but exposes no new shortening
player action. Reveal-only Confirm and actual delivery neither supply this input nor consume
work. They cannot skip outstanding work; logical completion without delivery also cannot poll.
Each optional Wait and accepting Ack draws main RNG256, copies its byte, updates the W2 indicator
if applicable, services once, then decides input. Enabled NPC draws may change the main image
in that service; they cannot overwrite the copy. W2's20-step indicator is visible at counter>=7,
is hidden by view scrolling, and acceptance hides it and requests validation67. W1 adds no67.
Plain JOIN remains input-first with no accepting-poll preamble.

The shared field service executes the existing entity reducer before logical view/scroll/window
work and increments one simulation opportunity. Suppressed entity events still service view and
window work. Their wrapper restores facing first, retains context and the live entity-service flag
through portrait and `TextCloseWait`, then closes logical/projected windows together before clearing context and
returning control. The no-window and unbound legacy paths keep their own lifecycle.

`wait-view` represents the source helper before nextText/nextSingleText. It checks active axes,
services until settled, services and rechecks (that service can start scrolling), then performs
the final service. It is not a fixed two-tick delay. `LogicalView` carries both plane positions,
active-axis destinations/speeds, target slot and follow counter. Inactive destinations remain
absent/don't-care. LoadMap clamps/quantizes origins in source order. View data reads the target
**after** entity service, uses strict1536/2304 deadbands and area bounds, retargets by384, then
scrolls each active axis and clears it on completion. Speed is24 or32 when the signed follow
counter exceeds6; active scrolling preserves its speeds. Godot interpolation supplies no readiness.

**Confirmed (source):** pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, beneath
`disasm/`: `code/common/scripting/text/textfunctions_1.asm` (DisplayText, ApplyAutomaticNewline,
@line, symbol_wait1, @wait2/sub_64A8); `textfunctions_2.asm` (CreateDialogueWindow,
HandleDialogueTypewriting, HandleBlinkingDialogueCursor, sub_6AD2/sub_6AE0, CloseDialogueWindow);
`code/common/windows/windowengine.asm` (VInt_UpdateWindows, WaitForWindowMovementEnd);
`code/common/maps/camerafunctions.asm` (VInt_UpdateViewData, WaitForViewScrollEnd),
`animations.asm` (VInt_UpdateScrollingData); and `code/common/scripting/map/mapsetupsfunctions_1.asm`
(RunMapSetupEntityEvent/loc_476A8–loc_476D6). The base VInt order is map planes, entities, view,
scrolling, sprites, windows, map animations. Only the admitted gameplay effects are modeled;
no additional interrupt RNG or CPU-time padding is invented.

The [binding evidence](../../../docs/research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding)
separates source facts, later saved bytes and inferred opening ancestry. The
[verification owner](../verification/field-programs.md#bound-opening-field-text-observation)
records actual settings comparisons and unresolved boundaries. Pending #517 speech policy remains;
this consumer retains existing actual speech projection and grants no policy waiver.

### Bound entity-event portrait

The bound wrapper opens the live actor's portrait before its enabled facing service, then
suppresses entity service before the handler. Known absent portraits retain the closed path;
missing metadata, Unknown state and an unregistered open portrait stop admission. A supported
existing open portrait retains its identity, flags and counters. No actor, text, flag or route
identity selects this capability.

`OpenPortraitWindow.Work` owns movement, registration, blink and mouth state. A fresh open starts
blink20/mouth6, moves from source Y=-10 to1 in four steps and observes the final moving-bit clear
before registering. Close removes the service first, performs the reverse move and deletes the
window. Both movement requests deliver existing SFX65. Packed right/mirror flags and ROM eye/mouth
tile mappings select the existing64×64 raster; the host crops/mirrors the composed face and
publishes actual tile/layout metadata. Raster/audio extraction is independent of this metadata.

After the admitted entity/view/window services, the registered portrait decrements blink;
at3 it selects alternate eyes, at0 normal eyes and main RNG120+30. Typewriting independently
gates mouth decrement; at5 it selects alternate mouth, at0 normal mouth and main RNG5+10.
DisplayText preserves incoming typewriting throughout fresh dialogue clear/open work and sets it
only after CreateDialogueWindow returns, before token processing. A reused window returns without
creation work. Empty/W-only text sets then clears it without exposing a typing service opportunity.
When typewriting is clear, mouth<=5 resets/draws immediately, otherwise it holds. Blink draws
precede mouth draws. W tokens clear typewriting and acceptance restores it for subsequent glyphs;
mouth-control shortening remains separate. NPC and portrait draws after a poll never replace
that poll's copied byte. Delivery frames supply no portrait opportunities or RNG.

`call.activateEntities` expresses Trap6's live activation, which survives its return.
`end-map-script` distinguishes the source script end from native RTS: an open dialogue window
requires the existing view helper before returning; then view override clears. A closed window
returns without that wait. The wrapper restores facing, closes portrait then dialogue and
finally returns control with entities enabled. A no-script suppressed event remains suppressed
during its close; a Trap6 caller retains activation through its close.

**Confirmed (static source):** the pinned revision above, `code/common/menus/portraitwindow.asm`,
`portraitfunctions.asm:VInt_PerformPortraitBlinking/UpdatePortrait/LoadPortrait`,
`code/common/tech/interrupts/trap6_mapscript.asm`, `trap9_contextualfunctions.asm`, `vint.asm`,
and `code/common/scripting/map/mapscriptengine_2.asm:loc_47234` support this bounded service order.
The wrapper binds `mapsetupsfunctions_1.asm:loc_4765E/loc_476A8/loc_476C4`.
Trap9 appends the portrait after the installed base services; no global interrupt scheduler is
introduced. The [research binding](../../../docs/research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
and [native observation](../verification/field-programs.md#bound-portrait-entity-event-observation)
retain provenance and the actual caller-return boundary. The source-zone section below admits the first introduction; later camera/JOIN/battle consumers
and original hardware/DMA timing remain outside this admission.

### Source zone caller

Source-produced `source-zone` events carry validated init actions compiled from `eas_Init`.
Authored `step` events retain arrival-before-program behavior. For a source zone, the marker
request follows entity obstruction and precedes map passability. The complete producing entity
pass runs before installation of the init stream and immediate handler entry. Physical travel,
its timer and destination survive; subsequent enabled services execute init and the existing
sprite handshake.

`StoryState.EventCaller` is the sole live caller authority: `EntityEventContext` and
`ZoneEventContext` select their own entry/return rules. `EntityEvent` is an entity-specific
projection. A zone uses the existing registered portrait/text/view reducers and the carried
live entity-service flag, regardless of native program defaults. It does not impersonate an
entity interaction. Its return removes/closes portrait, closes dialogue, then owns one mandatory
`ZoneArrivalWait` opportunity followed by physical-coordinate equality checks. Control stays
with the caller until arrival; script-idle/Busy are not the return condition.

The current actual admission is the first Astral introduction at(58,13), including affected
opening Zone6 and Sarah from the original bound start. No route, text, speaker, flag value or
seed selects this runtime mechanism. The [source binding](../../../docs/research/map3-messenger-acceptance.md#first-introduction-source-zone-caller)
and [actual observation](../verification/field-programs.md#source-zone-caller-observation)
record its limits. Later text500/501/F602, the second zone branch, camera, choice, JOIN and battle
consumers remain separate. This extends no original runtime or distribution claim.

## Original Map 3 opening

The named `map3-opening-start.json` begins at R1's controlled Map3 initialization seam, at (56,3)
facing down with only Bowie joined/active. It is not natural title/menu reach. The ordinary host
consumes canonical layouts, ordered setup/entity/event tables, pinned native wrappers and complete
map-script bodies. No expected endpoint, route ID or reference receipt participates in execution.
The separately read R2 controller trace exercises house exit, both doors, Sarah's classroom
interaction, stairs, entity142, Astral's zone and the messenger trigger. The complete messenger
acceptance runs through its motion, text, choice, two nods, camera waits, join music/fade, membership,
follower installation and guard positions; the zone wrapper then sets F603 and returns field input.

**Confirmed:** common-session comparisons match the accepted R1 position/membership/start RNG,
R2 program order and route positions, and R2a text IDs/packed speaker operands, flags, guards,
follower links and (43,10)/down endpoint. Ordinary Godot input observations reach the same endpoint
and move down then back up through a follower-occupied tile. A second input run declines, verifies
that join/follower flags stay clear, then talks to Sarah and accepts. That alternate behavior is
supported by pinned source and reproduced in the remake; it is not an additional original H3 claim.
An independent live-flag start enables another follower and moves logical entity142 from physical
slot17 to18. Logical identity and source record identity do not depend on that slot number.

Population allocation preserves physical processing order, enabled source followers, source allies
that reuse those slots, and non-ally logical aliases. Source `eas_Init` configurations execute and
wait for a matching sprite mount. Source walking templates are lowered from their actual operations;
random movement uses the existing main RNG. Followers measure current separation, rotate source
offsets around the leader's destination, and apply terrain fallbacks. Player input checks the source
obstructable bit and both current/reserved positions; scripted motion keeps its distinct destination
check. Source populations start with all 64 identity entries mapped to physical slot0, then overlay
allocated ally/non-ally references. Numeric selectors use the source low-byte/signed encoding;
32–63 share entries with128–159. These are references to `AllEntities`, never duplicate entity state.
Hiding removes every reference to the hidden slot and its follower behavior while retaining the
physical record. Missing source-table keys represent FF tombstones and survive state copies and
same-map preservation; only rebuilding population initializes fresh zero entries. Out-of-table
selectors and references outside the 49 normal records stop as Unsupported. Authored maps without
source population keep strict named-ID resolution. The accepted
[lookup contract](../../../docs/design/contracts/map-exploration.md) owns the source boundary.

Ordered door copies run before traversal; flag copies run at rebuild; roof activation/restoration
uses the shared layout reducers and signed area overlays. Source map255 same-map reload preserves
entities and the working layout, updates player position/facing, performs roof-on-load and reruns
selected initialization. The original setup selector still refuses an unimplemented alternative
instead of publishing default entities. This group uses Map3's default setup; broader initialization,
healing/temp reset, dynamically promoted/dead allies and other setups are outside its start boundary.

## Castle, palace, Astral and tower

The same live opening session continues through Map3's castle gate, Map19, the first Map20 palace
visit, royal return, Astral's invitation and the west/middle tower. Preparation imports the complete
source bodies `cs_51652`, `cs_53104`, `cs_53996`, `cs_52F0C`, `cs_52F40` and `cs_53EF4`, including
their native event/init callers and entity action streams. All six complete in the connected group.

**Confirmed:** the common-session group comparison starts at R1 and uses the accepted
[castle H2 graph](../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json) only as test navigation
and expected facts. The engine reads actual source programs. Gate guards move out and back before
the caller sets F604. Palace execution returns Bowie to (23,39), minister131 to (20,39), removes
Astral130's aliases and then lets the caller set F605. A repeat visit runs the source repeat init
without replaying the palace. Astral refusal sets caller F607; accepting a later prompt moves him
to (63,63), sets F608 and releases the passage. With F608 set and F256 clear, Map21's actual guard
interaction moves128 to (6,16), resolves unassigned135 to the existing player slot0, faces it down
and waits for the mounted sprite service. Only then does the script set F401 and return to the caller
which sets F256. Repeating the interaction displays579 without another move. Ordinary input reaches
(5,15) and remains available. Original natural reach and cadence remain **Unknown**;
the H2 graph is static evidence, not an H3 timeline.

Actual map rebuilds select/populate using incoming flags, clear source temporary flags256–383,
set F80, apply layout flags and run the selected initializer. Same-map preservation keeps temporary
flags. Map20's native entry branch compares exact signed fixed-point X/Y against $2280/$3780;
other entrances do not run the royal scene. Physical slot allocation uses the live follower flags,
while membership remains a separate source flag range. The group tests cover first/repeat visits,
initial/revealed/departed Astral, zero/two/three followers, initial acceptance and refusal followed
by an actual Sarah re-prompt. All four F608/F256 guard branches are compared with each follower count,
including dialogue-only branches that never set F401 or move the guard. A direct castle approach with F600 clear still reaches the explicitly
unimplemented `cs_51454`/`moveNextToPlayer` branch; the tests do not manufacture F600.

Map19/20 reuse their registered shared atlas; Map21 uses its own registered atlas. `setPriority`
persists on the physical entity. The modern renderer draws priority entities after ordinary
entities, retaining its existing Y/slot ordering within a group. Palace `fadeInB` performs a black
to full-brightness transition over 0.5 seconds and completes only after the actual view updates.
These are bounded presentation mappings, not original VDP ordering, palette cadence or pixel parity.
Source `setFacing` requests a new sprite generation on its resolved physical slot and retains its
program PC until `EntitySpriteReady` returns for that slot/generation. Authored `face` can omit this
service. Actual ordinary-host observations at60 and30 FPS cover the connected group, repeat visits,
the player-facing wait, guard repeat interaction and stable field input without state injection.

### Bound map-initialization lifecycle

Bound text/view settings now admit ordinary map initialization with its real `MapLoaded`
continuation, no event caller, and a live cursor. Explicit portraits use the same movement,
registration, blink/mouth service and close lifecycle as field callers. Bound text retains its
window/view service and enables only eligible text Wait/Ack after logical work and delivery;
`CanWaitAtInput` still requires complete field return. A nested `end-map-script` waits for the
view when its text remains open, then resumes its actual caller. Only the outer init completion
restores field following. No map/program/text ID selects production admission.

D1 is a typed neutral22-service pause with saved/cleared/restored typewriting; nonzero mouth
control skips its waits. It preserves first-glyph/token order and uses the same live entity/window/
portrait services, with no poll/copy or fake acknowledgement. Held logical-input shortening remains
unadmitted; reveal/Ack cannot supply it. Other unbound controls remain Unsupported. Explicit
sourceFF/null/no-event-speaker narration is admitted without replacing a retained portrait or
inferring its absence; actual speech uses the null speaker and remains silent. Explicit closes
still own removal. [Dialogue provenance/limits](../../../docs/design/contracts/dialogue-system.md#bound-neutral-text-delay-and-narration).

The map-init camera profile includes source layer255 with zero foreground/background offsets,
unity parallax and zero autoscroll. It follows A rather than B, retaining identical initialized
A/B axes through destination/scroll/window service. The existing renderer uses that same origin
without a duplicate foreground. The field profile also admits main-plane unity/origin0 and secondary
per-axis128/256 parallax, offsets0–63 and zero autoscroll within nonnegative signed-word positions.
Initialization quantizes before scaling/offset; destination and speed transforms preserve independent
axes. Ordinary actors subtract the area main plane; entity Layer still controls window priority.
Foreground presence includes parallax differences. Other view profiles, outcome windows and camera-entity retain their explicit limits.
Before-battle scene/windows use the separate bounded lifecycle below.
[Source contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle).

Actual engine cases cover nested camera and open/closed text returns, portrait registration/close,
readiness and unsupported contexts, first/repeat flag branches and a distinct authored foreground
follow state. **Confirmed, bounded modern host:** instant and adjustable/reveal delivery execute the same
opening/JOIN, guard out/back/F604, Map19 init, full first palace/F605 and Map19 field return.
Source255 A/B origins and actual tile/actor projections agree through init/scroll; sourceFF narration
requests no speech. Matching semantic readbacks preserve the accepted JOIN prefix and gameplay
across delivery. The bounded first palace route is described in
[verification](../verification/field-programs.md#bound-map-initialization-observation). Original
natural timing and full modern Battle01/H4 acceptance remain **Unknown**.
**Confirmed, bounded modern host:** the retained opening/JOIN and first-palace checkpoint continues
through Astral refusal/re-prompt/acceptance, tower guard sprite completion before F401/F256,
repeat interaction and Map21 readiness to Map40(14,13). The existing navigation omits the final
Up into the marked battle exit. Actual independent A/B tile origins and main-plane actor geometry,
resources, clipping and signed window priority are read back throughout changing plane separation.
Instant and adjustable/reveal settings retain identical post-JOIN logical transitions and carried RNG.
This field endpoint does not admit scene-map, camera-entity or before-battle windows, and does
not close executable H4 or original natural-cadence acceptance.

## Presentation and private Content

Content validates private provenance, embedded raster identities/shapes, required map/sprite links
and portrait references before session creation. Preparation verifies the registered USA ROM and
pinned source/asset manifest, reuses the existing map atlas, and decodes required sprite directions
and portraits through maintained decoders. The generated world and all game text/raster bytes remain
ignored private output. No ROM, extracted asset, dialogue prose or private absolute path is tracked.

Godot draws the working block layout and source sprite frames, mounts requested entity sprites,
shows source portraits and resolves `{LEADER}`/`{NAME;n}` from configured source names.
The bound camera consumes logical view state as described below; unbound cues retain their
presentation-driven arrival wait.

### Source-bound camera

With explicit field settings, `SetCameraTarget` validates the admitted field context and installs
four destinations in `LogicalView`; nullable `TargetSlot` represents source FF/no-follow.
Positions and follow counter survive target changes. An equal active axis remains active until
scroll service; an equal inactive axis stays inactive. `CameraWait` uses the existing `ViewWait`
clear/recheck/final-service lifecycle. It needs no asset acknowledgement, wall-clock duration or
second camera state. Field Wait/Ack/movement and `CompletePresentation` cannot release it.

`ExplorationViewRunner` prepares24/32 speed from the inherited signed counter every no-follow
service, scrolls each axis independently and evaluates window hiding before scrolling. Existing
entity→view/window→portrait service carries the live entity flag, shared RNG and poll-copy byte.
Dialogue and nested returns retain the held camera. Actual ordinary field-control return restores
the player's physical slot without clearing pending scrolling, positions or counter. The
[source contract](../../../docs/design/contracts/map-exploration.md#bound-field-camera-lifecycle)
and [static provenance](../../../docs/research/map3-messenger-acceptance.md#source-bound-camera-lifecycle)
own these rules and their source limits.

All bound draws use B logical positions/16 for base and layer0 actors, and A positions/16 for
foreground. A already includes its layout offset, so bound layer sampling adds no second offset.
Each8px tile is drawn using its retained priority bit. Base composition is low B, low A,
high B, high A, retaining alpha. Sprites keep their existing mutual order; entity layer versus
live window count determines display priority independently of `SetEntityPriority`. After a low
sprite, high B/A regions are restored only under that sprite's nontransparent ink. A high sprite
requires no repair. This preserves mixed-priority sprite order and transparent holes; no character,
text or room selects the rule. The existing sprite resource cache retains its row ink spans for
this mask (mosaic draws use their sampled block coverage), and all map regions reuse the existing block texture. Actual draw passes and masked
8px overlaps are included in `cameraProjection.occlusionDraws`.
The adapter applies no player-centering, viewport clamp or MoveToward to this profile. Modern
320×192 viewport/layout and existing textures remain. `cameraProjection` records actual draw
tick/token, plane origins/source offsets, first clipped tile/count and actor rectangles/resources/
culling; observation compares only matching source-state draws. Focus/visibility suspension reuses
the common clock with no accumulated debt. Unbound centering, clamping, smoothing and completion
remain unchanged.

[Native acceptance](../verification/field-programs.md#source-bound-camera-observation) covers the
whole accepted prefix and both Messenger destinations, stopping at text531 W1 before Wait/Ack.
The camera stays held; zone/script return and F603 remain pending. Physical tracking uses the
bound target rule below. Explicit speed, cursor/pulsating overrides, profiles outside the admitted
secondary parallax domain, autoscroll and word-wrap destinations remain unsupported; no hardware timing claim follows.

### Source-bound nod

With explicit field text/view settings, a nod validates the admitted field context and creates
`NodWait` for the resolved physical entity. AnimationFF precedes ten common services; elapsed10
selects the transformed sprite, elapsed30 restores normal selection, and elapsed40 waits for actual
completion before animation0 and caller continuation. Services retain the current entity flag and
entity→view/window→portrait order, shared RNG and prior poll copy. Adjacent nods retain their own
40 opportunities. This implements the [bounded source contract](../../../docs/design/contracts/map-exploration.md#bound-field-nod-lifecycle),
not a natural hardware-frame count.

The adapter mounts the existing normal/transformed sprite resources and draws the semantic phase,
without a wall-clock gesture timer or per-frame acknowledgement. The existing token/kind completion
joins logical completion; an early receipt cannot skip services and a late one cannot add them.
Focus loss or hidden view suspends automatic work and clears debt. `nodProjection` records the
actual actor, slot, sprite, facing, phase, texture selection, dimensions and viewport visibility.
The [native observation](../verification/field-programs.md#source-bound-nod-observation) reaches both
Messenger nods from the unchanged start and stops at text521 W1 before Ack with the zone still active.

Unbound nods retain their 40/60-second presentation timeline, altered band from10/60 to30/60,
and elapsed-time completion independent of culling. A culled actor or missed render phase does not
fabricate a draw; aggregate gesture counts alone are not visible-phase evidence.
`CompletePresentation` validates both wait token and kind; `EntitySpriteReady` validates slot and
request generation. Enter acknowledges dialogue only and cannot complete an unperformed service.

The audio contract permits an explicit project-authored presentation mapping: MUSIC_JOIN and
MUSIC_SAD_JOIN play a generated C-major/C-minor chord loop through `AudioStreamPlayer`, with an actual
fade and stop. This is a modern cue, **not original audio parity**. Sound and fade services continue
while the common story owns control, including after
the active state has become a battle with no exploration world. Camera and gesture cues require a
current exploration world; without it they report `AdapterError` and retain their wait instead of
using stale map state. Unbound camera smoothing, portrait crop,
text layout/full-line display and unbound cue duration remain bounded presentation choices.
The nod transform reuses the inspected source band mapping; original raster/DMA timing remains unproven. Outside the admitted field-text/entity-event profile, portrait blink/mouth/typewriter
RNG remains unbound. Original VRAM/DMA timing, speech SFX, door/warp music
and fade fidelity, waveform/tempo and hardware frame equivalence remain **Unknown or Unsupported**.
They are not silently reported as performed original services. An unbound explicit presentation cue
reports `AdapterError` and retains its wait.

### Bound before-battle scene and windows

A real `BeforeBattleFinished`/`EnteringBattle` program with bound text/view state, no event caller
and a live cursor can use ordinary explicit portrait/text/view services. This retains actual
nested callers, route and source text cursor; text Wait/Ack requires logical input and completed
delivery, while field movement remains closed. Nested `end-map-script` retains the real
continuation rather than manufacturing field input. Legacy and outcome contexts gain no admission.

Explicit black FadeOut/FadeIn cues without a period override use validated Display.Period;
supplied nonzero overrides restore the prior period after completion. csc37 and ordinary black
helpers share the existing finite fade service, without a new clock.
The source loadMapFadeIn lowering now explicitly emits camera-entity:null before that helper.
Bound null detach validates the same caller context, clears only logical tracking and preserves
axes/destinations/counter/services/RNG with no time advance. This represents the source FF store
before its first enabled fade wait, without claiming CPU-cycle ordering of fade-setting writes.
Standalone black fades retain following; production never infers composition from the next opcode.
After scene loading, source lowering emits one explicit `wait-ticks` service before returning to
subsequent instructions. This services retained entities under the existing enablement policy;
it is separate from pre-load fade services and precedes any `scene-entities` replacement.

Bound `scene-map` requires settled closed windows, no ordinary warp, a valid target base palette,
a settled black current display and the accepted view profile. Its explicit camera origin selects
the target area and independently transforms A/B; following and scroll speeds/mask clear, but the
source follow counter and entity service policy remain. Window state clears without resetting
party/RNG/music/callers or invoking OnLoad. Base changes while black Current remains unchanged;
only the later real fade restores new colors. Visible/transitioning/open-window/missing-palette
loads reject before any replacement. This restriction is a remake representation boundary,
not a statement about original visible csc48 behavior. Separate `scene-entities` replaces physical
aliases and mounts sprites without re-centering or reattaching the camera.
Bound rendering uses the selected logical area and both plane origins even when no target-map
area covers the retained player. Player-area and legacy overlay lookup apply only to unbound drawing.

**Confirmed (bounded remake observation):** the representative ordinary native run continues the existing same-session route from Map40 approach
through its last marked exit Up, Map57 scene/entity replacement, explicit destination(2,8) and
Astral135 text2292/W2 confirmation to2293/W1 input. These operands identify observation only.
No tick/seed, route endpoint or instruction index controls production legality. Bound white palette
services follow the rule below; the bound before-body/first-input acceptance is described below.
The older unbound full Battle01 admission is separate evidence; modern carried-party/RNG admission
and original natural cadence remain open. [Source/semantic contract](../../../docs/design/contracts/map-exploration.md#bound-black-scene-replacement-and-before-battle-windows).

### Bound physical camera tracking

`camera-entity` validates the existing bound field/map-init/real-before caller, resolves a logical
selector once through the normal physical entity resolver and installs only LogicalView.TargetSlot.
Valid slot0/default aliases and invisible allocated sprites remain valid. Missing/removed/out-of-table
references, reserved physical cursor slot63 and incomplete/outcome contexts reject before publication.
Installation/retarget/detach advances no tick or RNG and preserves independent axes, destinations,
speeds, source follow counter, area, windows, callers, service policy and party. Compatibility camera
entity/destination hints clear; actual bound projection continues to use LogicalView alone.

The existing common service moves enabled entities before updating the view/windows and registered
portrait. Following reads that live physical record, selecting B for layer0 or A for255. While any
axis is active it preserves prepared speed/counter and completes the existing scroll; later settled
following uses the existing deadband/clamp/counter rule and independent128/256 parallax. Null detach
retains axes/counter; explicit destinations clear tracking. Sprite visibility is independent of
identity removal. No new reducer, render smoothing, resource, clock or content lowering is needed.

Ordinary W1 Ack, portrait unregister/close, text close and source Sleep10 precede subsequent target
installation/motion. Later portrait/text opening and input retain the real before continuation and
route. Idle input presentation performs no camera service. Bound white palette uses the finite
service below; complete before-body,
later effects and battle admission are separate. [Observation](../verification/field-programs.md#bound-physical-camera-tracking-observation)
and [contract](../../../docs/design/contracts/map-exploration.md#bound-physical-camera-target) own this boundary.
Independent ordinary native readback reaches tracked motion and three later W1s before the white
Unsupported. Its raw observer FAIL from two expected-stop assertions is preserved separately from
semantic readback PASS; the corrected observer has not been executed.

## Battle01 admission and first input

**Confirmed, bounded:** the same R1 session continues from Map21 `(5,15)` through the accepted
46-input Map21/40 extension, the marked Map40 exit, complete `bbcs_01`, source new-battle
initialization, `LoadBattle` presentation, empty start hook and the first actual battle input.
The [admission evidence owner](../../../docs/research/map3-battle01-admission.md) and its H2/H3 fixtures
own the original facts. The pinned `mainloop.asm`, `battleloop_1.asm`, `loadBattle.asm`,
`cs_beforebattle.asm`, map script engine and map entity allocator supply executable content.
The ordinary program never reads those comparison fixtures.

`loadMapFadeIn` starts out-to-black and loads the scene layout/camera; the later `fadeInB` remains
separate. Scene loading does not run exploration init or replace entities. `loadMapEntities` then
rebuilds the physical records and identity table with live followers and custom main-entity coordinates.
Every new sprite must mount before the next instruction. Astral135 is now a real allocated record,
distinct from Map21's earlier unassigned135 alias to player slot0. Camera tracking resolves a physical
slot; explicit camera destinations clear tracking. Shiver saves/restores the animation counter and
global sprite size around its typed presentation wait.

Selection writes source F399 before the before-battle program. Initialization validates before
publishing battle state or clearing F90–F105. Living active allies consume source formation slots
in live membership order; dead ordinary allies remain unpositioned. An absent Sarah leaves no deployed Sarah and
Chester takes the next available slot. Living eligible allies heal, dead ordinary allies remain dead,
and Peter/Lemon follow the source immortal exception. Accounting, source equipment/spells and
main/thinking RNG survive; enemy initialization and region/AI reset use existing Domain rules.
This bounded baseline already has refreshed/equipped ally stats. Status requiring broader stat
refresh, F88 resume, other difficulty/control modes and a larger unadmitted roster remain Unsupported.

The load program holds control while Godot fades out, mounts the existing battle board and roster
in the same view/session, then fades in. Input and automatic turns stay closed during that service.
Only completion permits the start wrapper to set F451 and generate the first round. Seen-intro entry
skips both hooks but still initializes/loads. Completed selection clears its unlock and returns field control.

The independent H3 comparison deliberately supplies its external `0x1234` seed and three-member
roster, runs the whole before body and checks first actor1, RNG and turn order. These values are
never injected into the continuous R1 run. The native observer continues the same opening/castle
instance, observes white fades, mosaic and shiver draws, then presses Enter/Escape at battle control.
Original timing/pixels/hardware effects and natural title/menu reach remain Unknown.

White fades use an actual white overlay; black fades modulate the current scene. Both are blocking
modern half-second services, not original asynchronous palette cadence. Mosaic-in draws coarse
source-sprite samples through progressively finer blocks over half a second. Shiver draws three
alternating five-tick offsets, then restores the engine fields; original sprite DMA/bitfield waveform
is unclaimed. The renderer reports an adapter failure if an admitted service cannot be bound.

## Battle01 outcome, after-program and return

Bound outcomes initialize the newly built field world through `ExplorationTextRunner.Initialize`:
its view starts from that world, windows are closed and old entity-service/window work is cleared.
Display, party/resources/progress/loadout/RNG, route and captured return anchor are retained.
`BattleReturn` is shared by text admission, portrait admission and text readiness. It requires the
actual outcome continuation, route, anchor, cursor and initialized settings/view; it does not
rename the program as ordinary field input. Nested return-map initialization retains its callers
until completion. Existing reveal/input phases and portrait registration still control confirmation.

**Confirmed — bounded modern native route:** the unchanged opening party/world with the accepted
cumulative scene selection reaches live battle control, adaptive legal commits, victory and the
full bound after/return through actual keyboard inputs. Registered explicit windows retain the
victory continuation, source join/unlock/completed order precedes returned field input, and a real
Move settles afterward. The existing target-browse contract permits range rejection without changing
accepted target, party/resources or RNG; rejected attempts and subsequent legal confirmations remain
in the observation. Original natural cadence/pixels and executable H4 applicability are separate.

The connected private world binds the original outcome hooks and growth tables through Content.
Actual HEAL/item/physical awards consume one EXP threshold, grow the five base stats with carried main
RNG, refresh admitted ATT-only equipment and retain current HP/MP separately from their new maxima.
Learned spell upgrades update both the live spell choices and packed source spellbook. Class caps
consume the threshold without a growth draw. Missing growth or unsupported status/equipment/spell
effects remain explicit boundaries; growth metadata never supplies an action history or kill order.

Action publication orders the reached empty enemy-defeated hook, death accounting/cleanup and
outcome check before an ordinary after-turn/queue advance. Leader loss takes precedence over enemy
exhaustion. A terminal action does not consume another turn. The same session then owns the complete
outcome program; `battle-returned` is emitted only after its callers and return-map init finish.

Victory heals eligible living/immortal allies, executes all of `abcs_battle01`, applies the source
join-table tail (including member zero), clears F401, sets F501 and executes the return load. The
script's `resetForceBattleStats` separately restores all supplied allies, including ordinary dead
allies. Its displayed map is **Map57**, not Map40. The controller captures the first active ally's
battlefield position before the script; the script's mainEntity position does not replace that
return tuple. Reached action tails recreate the ally facing DOWN. Map57's void setup still creates
the player/followers; its F506 layout-copy branch remains a guarded frontier.

Ordinary defeat plays its own sound/text, restores the leader's HP, halves unsigned current gold,
then heals eligible living/immortal allies on exploration entry. It retains F401 and does not set
F501 or execute the after-program/join tail. The selection explicitly supplies egress Map3; F399
must be set and F64/F640 clear, selecting (32,13)/UP. This is a controlled egress choice, not evidence
for the original campaign's natural producer.

Normal Map3 reload retains the real `cs_513BA` hide and removed entity142 alias. At exactly
`byte_513A8:1`, the [accepted source contract](../../../docs/design/contracts/map-exploration.md) permits
the four writes into inactive window scratch to have no further map/entity/camera effect: windows
are empty, old presentation work has drained, and a new window is rebuilt before publication.
The common renderer has no emulated DMA queue. The selected
[`Sf2StoryPolicy`](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs) requires exactly
`byte-513a8:1`, Map3, MapLoaded/OutcomeMapLoaded, a closed window and F603. A still-live142 alias
receives the real `Hide` move-out, retaining aliases. A removed alias requires F1 and a real hidden
entity142 tombstone at X/Y0x7000. Missing records, other call sites and active windows are rejected.
The [typed source family and finite candidate](#source-story-policy)
leave PC/stack/wait/revision publication with the generic runner. Explicitly authored compositions
reject this operation at its PC; the private produced opcode remains unchanged.

Godot keeps the battle projection until the actual fade/load hands over to the scene, renders
mosaic-out and sprite replacement, and plays a distinct project-authored defeat cue. Sprite changes
wait for the exact slot/request completion. After source init and fade-in, ordinary movement uses
the existing exploration view and session identity. These are modern presentation services;
original waveforms, DMA/VInt behavior and natural original continuity remain Unknown.

## Remaining source boundaries

The accepted Map21 default-population contract resolves135 to slot0 under its stated initialization
conditions. Natural original call-time RAM, full sprite/hardware effects and timing remain **Unknown**;
remake continuation is not a new original H3 observation. The continuous remake route reaches
Battle01 victory/ordinary-defeat return; original natural continuity across the H3 bridge and
original presentation remain outside this claim. Other Map3 native branches such as ChurchMenu and
moveNextToPlayer retain executable source frontiers.

The post-F603 abstraction above is specific to inactive window scratch on normal reload. Active
windows, deferred DMA, other missing-entity helpers and later storage reuse remain Unknown; no
generic missing-entity no-op, player fallback or entity255 is introduced.

## Reference migration boundary

The ordinary source path has no Reference dependency. The legacy Sarah, zone601, entity142,
Astral-zone, messenger, castle gate, palace, Astral invitation, tower guard, castle/tower cross-map,
Map40 pending-admission and defeat-recovery/return/arrival executors were removed with their callers
during M3/M4. M5 then retired the remaining independent startup/action comparisons, frozen context
DTOs, legacy geometry/visual bindings, the reference host and the `map3-post-opening-reference-start.json`
input together with `SF2_REFERENCE_POST_OPENING_START`. Whole-flow comparisons use common commands and
actual native input. The separate private probe retains the Map21 guard and missing-presentation
comparisons. A1–A8 closure and 8C/H4 are not reported.

## Source-bound yes/no lifecycle

With the explicit field text/view profile, `ChooseYesNo` retains a `ChoiceWait` through entry,
opening, raw held release, conditioned input, closing and the caller's return delay. Legacy
unbound `ChooseDialogue(bool)` remains separate and cannot release a bound choice. The flag operand
is generic; no text, route, instruction index or seed selects legality.

`ChoiceHeldInput` captures mapped held state at entry. If held, opening must finish and genuine
release must occur before polling. Release adds zero semantic service under the admitted modern
input boundary; this does not assert zero original hardware interrupts. `PollChoice` accepts a
fresh conditioned semantic mask. Left, Right, Cancel, ConfirmC, ConfirmA have source precedence.
Left/Right select Yes/No even when reselecting; Cancel returns No; either Confirm accepts the
current selection. Up/Down alone are neutral polls. Initial selection is Yes. A neutral explicit
Wait or directional poll draws/decrements the logical animation counter and runs one common service;
confirmation/cancel starts closing without that loop tail. Host idle/reveal/focus latency does not
create opportunities or debt. Raw held bytes are never silently treated as conditioned input.

The own14×3 window opens from(32,17) to(12,17), closes to(-16,17), and advances at the common window
stage. Source length4 has four movement passes followed by busy clearing; completion reads every
represented moving window, including dialogue and portrait. Opening/closing refresh an existing
dialogue window for4; selection refreshes it for1 only when settled. Delete the own window before
writing the result flag, then run exactly ten common services before advancing the instruction.
The choice does not close the dialogue/portrait or increment dialogue/portrait presence.

`StoryState.WindowFixPending` carries the source post-scroll flag through waits, empty-window passes
and map initialization. At the common window stage, dialogue, portrait and choice advance their own
geometry/counters and record incoming busy status. A hidden pass sets the pending flag. An unhidden
pass consumes it only when a represented window exists, refreshing every geometrically settled window
to length1/counter0 while preserving the pass's busy result. Stationary animation can still be busy;
its counters cannot stand in for geometric equality. A four-step own move arriving on the first
unhidden pass after three hidden passes therefore completes on service6, not the usual5. No fixed pad
is applied. Dialogue tracks window coordinates separately from its glyph cursor; every close entry
uses its actual current position. Portrait geometry runs here once, with registered blink/mouth RNG
remaining later. Dialogue and portrait close helpers also wait for every represented moving window.

The fresh remake session admits a clear pending flag. **Confirmed (static source):** full system
initialization clears its RAM byte. **Inferred:** the retained controlled opening start has that value;
it does not capture the byte or establish natural cold-boot ancestry. Ordinary initialization does
not reset it. See the named initialization and postpass bodies in the source owner.

Common entity→view/scroll/window→registered portrait service uses live enablement, counters,
Typewriting, held/follow camera and RNG. There is no direct choice RNG draw or poll-copy write.
The animation begins at15, wraps1→20, uses the alternate variant at values>=10, and navigation
resets the old variant then draws the new choice at19. Actual Godot labels/selected color and panel
position consume this logical state; original menu tile rasters are not extracted. Remapped keys,
pad buttons/axes and swapped Confirm/Cancel use the installed input actions; entry release includes
all mapped gameplay actions except the separate modern Wait. Duplicate held pad packets and axis
values do not become fresh edges. Focus/hide requires a fresh release and resets clock debt.

Source callsite audio emits65 per move-window request (own and existing dialogue),66 per selection,
including reselection. Bound confirmation emits no generic67. Existing whole-PCM playback and
preemption remain authoritative for actual sound delivery; no sound duration controls entity ticks.

Admission is normal movement, gold absent, field continuation with logical text/view and Closed or
admitted EventCaller portrait. **Unknown:** incoming original global24/6 repeat/direction ancestry,
release-spin CPU interrupts, debug turbo, gold-window lifecycle, exact menu raster and hardware
DMA timing. These exclusions do not prevent both answers in the declared fresh semantic stream.
JOIN's audio/input/live-service coupling remains separate. See the
[source contract](../../../docs/design/contracts/map-exploration.md#source-bound-choice-consumer),
[original provenance](../../../docs/research/map3-messenger-acceptance.md#source-bound-choice-lifecycle)
and [observation owner](../verification/field-programs.md#source-bound-choice-observation).

## Raw field display

Bound explicit-window `ShowText` uses `FieldTextWait` even when `WaitForAcknowledgement=false`.
The existing token stream, substituted names/font advances, window/glyph/scroll work and
`FinishDelivery` own completion. Actual W1/W2 retain their input consumers and ordered poll/RNG
semantics. A final no-W span returns only after logical End and actual reveal; it neither invents
an Ack nor closes the window. Early reveal cannot skip mandatory work; late reveal adds no service
or clock debt. Unbound legacy execution is unchanged.

Only raw text with no explicit speaker and no event-speaker lookup admits intentional absence.
Missing event context and invalid explicit speakers still report `field-text-speaker`. Speakerless
host delivery uses the existing silent path, while an admitted registered portrait keeps its own
blink/mouth service and RNG. This does not decide ordinary speech delivery policy under#517.

Bound `PresentCue(SoundWait)` without a finite profile reports `field-music-progress-unbound` at `program.presentation`,
kind `UnsupportedCapability`, stop `Unsupported`. Completed text/window/entity/party/flag state
remains at that instruction. No PresentationWait is created and actual finite-audio completion
cannot release gameplay through previous-music, plain input or script return. Existing PCM playback
and unbound presentation observations retain their previous applicability.

The [source contract](../../../docs/design/contracts/map-exploration.md#raw-field-text-and-the-music-boundary)
and [provenance](../../../docs/research/map3-messenger-acceptance.md#raw-display-and-unbound-music-progress)
separate this supported raw-text rule from the unknown sound-to-service clock. The known505 driver
updates are not a duration in entity services. See [verification](../verification/field-programs.md#raw-field-text-observation)
for behavior and native expected-boundary acceptance.

## Modern Finite Music

The [accepted modern policy](../../../docs/design/contracts/music-wait-service.md#accepted-modern-finite-music-policy)
uses optional finite audio metadata `modernEndStep`; the private lowerer emits505 for command19.
The reader permits a positive endpoint only on non-looping music. Ordinary playback needs no profile.
`MusicProgress` retains cue, semantic generation, previous stack, map/area, progress, actual completion
and logical previous eligibility. Duplicate current cues preserve it; earlier replacements start at0.
Ordinary unprofiled/looping `PreviousMusic` also pops this history and changes the semantic generation;
it does not require a finite helper. Restoring a profiled cue starts its progress at0, matching the
player restart. Unknown/empty history remains Unsupported.
`ExplorationMusicRunner` advances this state before each existing bound common service, regardless
of entity enablement. It never adds a second entity service or uses PCM duration.

`MusicWait` first arms, subsequently samples logical end, and returns only after a complete group
of3 services plus matching `CompleteMusic(generation,cue)`. Early receipts latch; late receipts
cause no tick/RNG/debt. The host observes actual finite completion and then the existing actual
previous-track restart. Wrong/stale/duplicate receipts reject; active-helper interference or a
changed bound map/area/battle-selection reports Unsupported, and changed host playback reports an explicit adapter error.
Missing profiles retain the raw-text boundary above. Looping/other unbound playback remains supported.

The common commit boundary invalidates history and finite phase on context changes outside a helper,
including an intermediate map/area change followed by a return. `HistoryBound` prevents that old
stack from becoming valid again merely because the final map matches. The first subsequent explicit
request establishes only its current cue with empty history/no finite profile; a later different
request can use that known cue as previous and admit finite progress. This avoids assuming that
SessionAudio consumed every intermediate map selection within one application result. No host
history or timing is copied back into gameplay; context changes add no extra service.

Plain input after the helper keeps the raw window without revealing it anew. A genuine Wait uses
the existing one-service input-first rule without a W1/W2 copy. Bound plain Ack is silent; only
accepted W2 requests validation67. Ack, close/Sleep10, follower/position
instructions and script/Zone return reuse their owners. No production rule names text447, cs-51614
or a selected seed. [Verification](../verification/field-programs.md#modern-finite-music-observation)
owns behavior cases and the bounded native JOIN return. The [current composed milestone](../../../docs/design/synthesis/map3-battle01-readiness.md#accepted-current-milestone)
has separate acceptance; this JOIN observation alone does not prove full-route or hardware parity.

### Bound white palette

With validated Display, typed `present` FadeOut/FadeIn resource `white` selects the source map-white
profile; existing lowering already supplies this cue. `FullFadeWait.Color` distinguishes black and
white, retaining token, countdown, entry, extra service and logical/actual completion. White uses
period1, seven base-derived positive offsets, terminator and one extra service, then restores the
stored period. Base is unchanged; White/BaseRestored/Black visibility is explicit even for endpoint
color equality. Black temporary overrides remain black-only; FlashWhite/RestorePalette and conflicting
FullBlack+white remain Unsupported. Starting from settled black/restored/white is valid; invalid or
transitioning state rejects. Startup JSON still admits only black/base-restored states.

Fade ticks reuse common service with current script EntityServices override, then program default;
ordinary warp deliberately retains enabled field entities. Existing music runs exactly once, followed
by entity, view/window and registered portrait. Logical completion suspends service while actual
delivery finishes; early delivery still waits for all logical work. Late/stale/duplicate receipts add
no opportunities. Renderer clears inherited black modulation before white and stale white before
black, preserving the half-second modern delivery and reduced-flash alpha0 policy. Its viewport
coverage is not source map-only palette pixel equivalence.

The white-helper observation ends at Chester2297 W1 after all six white helpers and their
intervening source waits. It does not establish the following bound mosaic/shiver, complete before
body, battle load/input or winning trace. The bound continuation is described below; the next genuine
Unsupported blocker is **Unknown** until observed. See [verification](../verification/field-programs.md#bound-white-palette-observation)
and [source/semantic rule](../../../docs/design/contracts/map-exploration.md#bound-map-white-finite-service).

### Bound before-body and battle entry

Field palette routing checks `ActiveExploration`, not merely retained Display. After the genuine
before-context completes, BattleEntry initializes `ActiveBattle`; its unprofiled black loader cues
use the existing `PresentationWait` modern delivery lifetime. Matching receipts retain ActiveBattle and its state
and frozen field Display/view/settings; they add no field opportunity. Explicit FullBlack in a
non-field context rejects as `full-fade-context` before helper publication. The old scene facade
remains available until the existing mount and handoff. Load/start/intro ordering and first queue
remain owned by BattleEntry and BattleTurnFlow; the final delivery can resume those owners and their
ordinary first-queue RNG draws without adding a field opportunity.

Bound generic `PresentationWait`, including mosaic/shiver and retained-profile loader delivery,
now uses the existing visibility/focus suspension and zero-debt resume. Effect completion/restoration
and unbound/music policy are unchanged. The [contract](../../../docs/design/contracts/map-exploration.md#bound-before-body-and-battle-loader-ownership)
distinguishes this modern loader policy from original LoadBattle opportunities. **Confirmed (bounded remake):** the ordinary full bound
before-body and first-input observation passes through the [verification route](../verification/field-programs.md#bound-before-body-and-battle-entry-observation).

## Source Story Policy

The existing public v8 reader admits [story A](../../content/authored/story-rule-demo-a.json) and
[story B](../../content/authored/story-rule-demo-b.json), including all untaken branch/call targets.
An ordinary interaction branches on live flag40, calls an external first/again program, then calls
a nested pause program. That program presents its own text and waits30 logical services before
returning. The called program then changes a flag and entity position/visibility; its caller toggles
flag40 and returns field control. Both outcomes execute in each package in one session. Package
names never choose runner algorithms. [The program owner](#authoring-conditional-programs)
describes the actual effects and content editing steps.

[`SourceStoryInstruction`](../../src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs) is a typed
source-only family. Only this family delegates to the selected
[`ISourceStoryPolicy`](../../src/Sf2.Remake.Application/Runtime/Exploration/ISourceStoryPolicy.cs).
The SF2 [instruction](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryInstructions.cs) and
[policy](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2StoryPolicy.cs) own the existing retired Map3
operation. The reader maps the unchanged opcode; private produced bytes and schema are unchanged.
`ProgramRunner` knows neither this exact source operation nor its map/program/flag guards.

The policy returns only a `SourceStoryCandidate`: no active-state change, or one retired existing
physical entity record. `SourceStoryRules` checks that record against the existing `Hide` result
at its actual slot; it preserves every other record, aliases, layout, party and world metadata.
It does not resolve a record's other logical alias again. The candidate cannot contain story state,
PC, stack, wait, seed, revision or observations. The runner alone advances/commits the instruction.
All original exact context, live-alias hide and removed-alias tombstone guards remain in SF2.
This is the accepted conditional inactive-scratch abstraction, not a new visible scratch effect.

Every explicitly authored factory selects
[`AuthoredStoryPolicy`](../../src/Sf2.Remake.Application/Gameplay/Authored/AuthoredStoryPolicy.cs), which
rejects source-only instructions at the reached PC. Direct source-default starts and `ForGame`
remain SF2. Content profile does not automatically select executable policy; see
[trust/selection](../content/profiles-and-trust.md#story-content-and-source-policy-selection).
Ordinary branch/call/return/text/tick operations are independent of that policy and unchanged.
Existing callers already pass the same `SessionRules` through field and battle-outcome continuations.

Expected source errors retain code/field/category and add policy/operation/PC to the message.
Malformed candidates or unexpected exceptions use the existing InvariantFailure policy diagnostic
with the same context and no private exception detail. Earlier committed instructions, the reached
PC and stack remain. A rejected or duplicate completion cannot apply a later effect or return a caller.
[Behavior tests](../../tests/Sf2.Remake.Engine.Tests/StoryPolicyReplacementTests.cs) and
[short actual observations](../verification/field-programs.md#replaceable-story-observation)
cover content replacement, failure retention and usable control; source-context tests do not claim
private natural reach, original timing or measured performance.
