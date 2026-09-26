# Common exploration and resumable programs

## Current capability

The ordinary `GameSession` executes exploration, resumable story programs and map transfer, then
hands the same session to the existing battle flow. `harbor-arrival.json` and `hill-passage.json`
exercise different maps, actor identities, dialogue, choices and motion through this path. Accepting
runs entity actions, a call/return and a timer, transfers maps, executes map init and both intro
hooks, initializes the encounter and reaches first player control. Declining returns field input.
Neither package identity nor an expected route/receipt admits a command.

Exploration and return consume the same [logical input/settings owner](./development-and-verification.md#logical-input-and-accessibility-adr-0010-9a) as battle. Confirm talks/acknowledges/accepts and Cancel declines. Text reveal stays in the adapter; W1 delivery reports its completion separately from player acknowledgement, including automatic completion of a trailing span. Reduced-flash suppresses the reached white overlay while completing its real token/kind through the existing presentation service, an intentional 9A deviation.

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
only closes text. `closeTxt` closes portrait then text. JOIN's existing SoundWait → PreviousMusic →
`wait-text-input` → text close → Sleep(10) retains the incoming portrait gate. These rules are
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
is rejected at this admitted consumer. The [host contract and reproduction](./development-and-verification.md#plain-text-input-gameplay-wait)
cover reveal, delivery and input rearm.

**Unknown:** the selected natural post-JOIN helper entry, logical audio end, complete enabled VInt
table and portrait counter chronology remain unobserved. The current program's entity setting is
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
`StoryState.RandomSeedCopy` is nullable until an admitted write and survives ordinary copies;
it does not replace the separate `ThinkingSeed` image. Entity actions/motion/followers and portrait
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

**Confirmed (source):** the [reached W1 contract](../../docs/design/contracts/dialogue-system.md#reached-w1-consumer-binding)
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
1/2/4 polls. [Verification and limits](./development-and-verification.md#w1-entity-event-input)
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

The [idle/caller source owner](../../docs/research/map3-controlled-start-egress-transition.md#source-idle-completion-and-caller-installation)
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
scene camera changes, cursor targets, scrolling overrides, autoscroll and non-unity parallax
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

The [binding evidence](../../docs/research/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding)
separates source facts, later saved bytes and inferred opening ancestry. The
[verification owner](./development-and-verification.md#bound-opening-field-text-observation)
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
introduced. The [research binding](../../docs/research/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)
and [native observation](development-and-verification.md#bound-portrait-entity-event-observation)
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
seed selects this runtime mechanism. The [source binding](../../docs/research/map3-messenger-acceptance.md#first-introduction-source-zone-caller)
and [actual observation](development-and-verification.md#source-zone-caller-observation)
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
[lookup contract](../../docs/design/contracts/map-exploration.md) owns the source boundary.

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
[castle H2 graph](../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json) only as test navigation
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

## Presentation and private Content

Content validates private provenance, embedded raster identities/shapes, required map/sprite links
and portrait references before session creation. Preparation verifies the registered USA ROM and
pinned source/asset manifest, reuses the existing map atlas, and decodes required sprite directions
and portraits through maintained decoders. The generated world and all game text/raster bytes remain
ignored private output. No ROM, extracted asset, dialogue prose or private absolute path is tracked.

Godot draws the working block layout and source sprite frames, mounts requested entity sprites,
shows source portraits and resolves `{LEADER}`/`{NAME;n}` from configured source names. It scrolls
the camera to the requested destination and waits for arrival. Nod presentation freezes the entity
animation counter and uses a 40/60-second timeline, with the altered head band from 10/60 to 30/60
seconds and the restored sprite afterward. Completion follows elapsed time independently of viewport
culling or missed render phases. Actual visible nod and restored-sprite draws are counted separately;
an off-camera actor or a frame that skips a phase does not fabricate a draw or hold the story forever.
`CompletePresentation` validates both wait token and kind; `EntitySpriteReady` validates slot and
request generation. Enter acknowledges dialogue only and cannot complete an unperformed service.

The audio contract permits an explicit project-authored presentation mapping: MUSIC_JOIN and
MUSIC_SAD_JOIN play a generated C-major/C-minor chord loop through `AudioStreamPlayer`, with an actual
fade and stop. This is a modern cue, **not original audio parity**. Sound and fade services continue
while the common story owns control, including after
the active state has become a battle with no exploration world. Camera and gesture cues require a
current exploration world; without it they report `AdapterError` and retain their wait instead of
using stale map state. Camera smoothing, portrait crop,
text layout/full-line display, nod pixel transformation and cue duration are bounded presentation
choices. Outside the admitted field-text/entity-event profile, portrait blink/mouth/typewriter
RNG remains unbound. Original VRAM/DMA timing, speech SFX, door/warp music
and fade fidelity, waveform/tempo and hardware frame equivalence remain **Unknown or Unsupported**.
They are not silently reported as performed original services. An unbound explicit presentation cue
reports `AdapterError` and retains its wait.

## Battle01 admission and first input

**Confirmed, bounded:** the same R1 session continues from Map21 `(5,15)` through the accepted
46-input Map21/40 extension, the marked Map40 exit, complete `bbcs_01`, source new-battle
initialization, `LoadBattle` presentation, empty start hook and the first actual battle input.
The [admission evidence owner](../../docs/research/map3-battle01-admission.md) and its H2/H3 fixtures
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
`byte_513A8:1`, the [accepted source contract](../../docs/design/contracts/map-exploration.md) permits
the four writes into inactive window scratch to have no further map/entity/camera effect: windows
are empty, old presentation work has drained, and a new window is rebuilt before publication.
The common renderer has no emulated DMA queue. The engine requires this normal reload continuation,
closed window, source cursor/flags and real hidden entity record. A still-live alias receives the
real move-out operation. Missing records, other call sites and active windows are rejected.

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

## Reproduction

Use the current worktree's retained environment, SDK/Godot and registered private input selectors.
Choose fresh ignored output paths. The canonical import (`uv run sf2 h2 map-import`) reads the H1
listing `build/sf2build-h1.lst`, so run the H1 rebuild with `-KeepBuildArtifacts` and install its
listing, symbols and binary under the conventional `sf2build-h1.*` names first; the frozen H1/H2
PowerShell rails require PowerShell 7. The ordinary host needs a world prepared with `--rom-path` and
`--presentation-root`: without presentation data its first sprite mount stops with the adapter error
`entity-sprite-binding`. Preparation and actual opening launch are:

```powershell
uv run --locked python -m sf2tool.remake_exploration_content `
  --canonical $env:SF2_PRIVATE_CANONICAL_MAP_IMPORT `
  --upstream $selectedUpstreamRoot `
  --selection remake/reference/inputs/map3-programs.json `
  --rom-path $registeredRomPath --presentation-root $selectedPresentationRoot `
  --output $env:SF2_PRIVATE_EXPLORATION_CONTENT

$env:SF2_PRIVATE_CONTROLLED_START = (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-party.json').Path
& $godotBinary --path remake/game -- --private-exploration-start (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-start.json').Path
```

The existing private encounter/static/enemy/gold selections are also required. Program data never
reads reference fixtures. For the native comparison, build the ordinary Debug project first and run:

```powershell
$env:SF2_PRIVATE_EXPLORATION_CASE = 'map3-opening' # or map3-decline
$env:SF2_PRIVATE_EXPLORATION_PLAN = (Resolve-Path -LiteralPath 'tests/fixtures/h3/map3-battle01-natural-route-v1.json').Path
$env:SF2_EXPLORATION_OBSERVATION_OUTPUT = Join-Path $env:SF2_RUN_OUTPUT 'opening.json'
& $godotBinary --headless --path remake/game --fixed-fps 60 `
  --script res://probes/engine_map3_opening_observation.gd -- `
  --private-exploration-start (Resolve-Path -LiteralPath 'remake/reference/inputs/map3-opening-start.json').Path
```

Require exit0, `passed: true`, both real sound counters, actual gesture draws, stable input and no
script/process errors. The observer waits briefly for the independent audio thread's stop/free queue
before headless exit; that wait does not advance game state. No screenshots or state setters are used.

```powershell
uv run sf2 verify engine
uv run sf2 verify adapter
$env:SF2_REQUIRE_PRIVATE_TESTS = '1'
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --configuration Release --no-restore --filter 'FullyQualifiedName~PrivateExplorationTests'
```

The owning engine project copies R1/R2/R2a comparison fixtures and explicit starts. Private input
checks run only with their required input configuration; public runs report their skips. Use the
committed `verify plan --scope engine` selection and proportionate direct checks. Completed failing
runs remain recorded; rerun their corrected nodes rather than replaying the aggregate. The
remaining narrow observer is `engine_private_exploration_observation.gd`, with `map21-guard`
or the authored missing-presentation case.

For the connected castle group, use the same R1 start and R2 input-plan environment with
`--script res://probes/engine_castle_tower_observation.gd`. The observer continues the same live
instance, reads the H2 castle graph for navigation, revisits the palace, declines/re-prompts Astral,
and writes an adjacent `.castle.json` with checkpoints, actual palette brightness, priority and the
guard's pending/completed sprite service state. `map3-decline` first refuses and later joins Sarah
through actual input. Run the ordinary host at fixed60 and30 FPS, require `passed:true` and exit0,
and retain any completed failures in the local report. The final input sequence repeats the guard
interaction and walks the released passage, then checks stable PlayerInput.
No observer supplies an expected endpoint, completion flag, entity alias or session replacement.

For complete Battle01 admission, keep those same retained environment and R1 input selections and
replace the script with `res://probes/engine_battle01_admission_observation.gd`. Run fixed60 and30
FPS. The observer explicitly sets the root window to 960×640 before starting the route; headless
`--resolution` alone can leave the actual viewport at 64×64. Success requires the observed 960×640
viewport at battle load, first control and after cancel, a positive map viewport inside it with zoom
at least 1, and a positive preview inside the map when control is available. The adjacent
`.admission.json` retains the expected size and actual loaded/ready geometry; the main output retains
the final geometry. A completed 64×64 run is insufficient for this projection acceptance.
The observer reuses the opening/castle route and reads the existing PlayerReady static input
plan only for navigation. Its adjacent `.admission.json` records actual effects, load-before-input,
first battle projection and real confirmation/cancel. Require exit0, `passed: true`, nonzero mosaic
and shiver draws, white/load observations and no Godot script/process errors. Direct comparisons use:

```powershell
& $env:DOTNET_BIN test remake/tests/Sf2.Remake.Engine.Tests/Sf2.Remake.Engine.Tests.csproj `
  --no-restore --filter 'FullyQualifiedName~BattleEntryProgramTests'
```

`BattleEntryProgramTests` covers scene replacement and sprite waits, camera/shiver ownership,
first/seen/completed selection, input gating and initialization failure. With the registered private
bindings, `PrivateBattleEntryProgramTests` covers continuous R1 admission, the independently seeded
H3 comparison, live membership/formation/resources and the F88 boundary. These are engine behavior
checks; they do not test the observers or replay the legacy aggregate.

For the complete outcome group, use `res://probes/engine_battle01_outcome_observation.gd` with the
same retained project, R1 start and opening navigation selection. Set `SF2_BATTLE_OUTCOME_CASE` to
`victory` or `defeat`. The external observer chooses normal movement/action/target keys from the
live battle, then checks source programs, flags, actual presentation, 960×640 geometry and return
movement. It does not restore an expected seed, HP, roster, endpoint or program cursor. Fixed60/30
FPS may reach different battle seeds because real presentation allows background entity ticks.
The adjacent `.outcome.json` and normal output retain actual observations. Require `passed:true`,
exit0 and no Godot errors. Direct engine checks are `BattleOutcomeProgramTests`,
`PrivateBattleOutcomeProgramTests` and `BattleGrowthTests`; the private continuous victory also
crosses a real level threshold. It reads the accepted R4a static spine
(`sf2-map3-battle01-victory-return-static-v1`) for the unlocked/completed flags, the Battle01 join
row and the after-program, clear, set, SwitchMap and exploration order; that static spine proves
source order, not natural original execution. Preserve completed failures and rerun their affected nodes only.

## Ordinary Medical Herb battle action

The common battle session accepts `SelectItem(slot)`, `SelectTarget(actor)` and `Confirm`
from ordinary action choice. `Cancel` returns to provisional movement. Item reselection
clears the previous target. Invalid slots, empty inventory, unsupported item effects,
dead/opposing/out-of-range targets and failed reward/growth admission publish no partial
movement, HP, inventory or RNG change. Reusing an old command envelope cannot consume twice.
The shared target validator measures from the provisional destination, including self-targets.

`BattleActorState.SourceLoadout` is the live ordered four-slot loadout. Definition loadouts
supply initial content only; explicit `BattleActorStartInput.SourceLoadout` carries later
state. Growth retains that live inventory while changing learned spells, and `BattleOutcome`
passes it into the exploration party on both victory and defeat. Resource resets heal HP/MP
without replenishing items. `item-consumed` records the original item word in `Before` and
the chosen zero-based slot in `After`; the resulting snapshot exposes the arranged inventory.
HP recovery now executes through the [Medical Herb scene](./presentation-and-assets.md#medical-herb-scenes),
after construction has consumed inventory. EXP/growth and turn release wait for their scene commands.
Application owns single-side target/actor switching; SFX113 and the source NONE/Nothing selection do
not imply fairy or physical recoil draws. There is no separate inventory service or field inventory UI.

### Original source and effect boundary

**Confirmed static:** the pinned SF2DISASM revision
`c834c652b6862bc5679fd7f69a38a7093206efc6` supplies these dependencies beneath `disasm/`:

| Source / symbol | Consumed rule |
| --- | --- |
| `data/stats/items/itemdefs.asm`, item 0 at `table_ItemDefinitions` (`0x16EA6`) | Medical Herb is CONSUMABLE, with no equip effects; use spell HEALIN-1 and item range 0–1. |
| `data/stats/spells/spelldefs.asm`, HEALIN-1 | Base ID 16, MP cost 0, teammate healing, range 0–1, radius 0, power 10. |
| `code/gameflow/battle/battleactions/useitem.asm`, `battlesceneScript_UseItem` (`0xBBB8`) | Select the held item definition, unpack use spell and delegate to its spell effect. |
| `castspell.asm`, `spellEffect_Heal`; `calculatespelldamage.asm`, `AdjustSpellPower` (same battleactions directory) | Recovery is `min(power, missing HP)`. Item actions skip the spell-only promotion multiplier. HEALIN recovery has no variance draw. |
| `earnexp.asm`, `battlesceneScript_CalculateHealingExp`; `giveexpandgold.asm`, `battlesceneScript_GiveExpAndGold` | Ally healer classes contribute `min(25, max(10, floor(25 * recovered / maxHp)))`; other classes contribute zero. Same-side actions skip battle halving. Two range-16 draws add/subtract one on zero, with final minimum one. |
| `breakuseditem.asm`, `battlesceneScript_BreakUsedItem` (`0xBBE6`); `code/common/stats/itemstats.asm`, `RemoveItemBySlot` / `RemoveAndArrangeItems` | Non-equipment consumes without a break roll, shifts subsequent full item words down one slot and appends NOTHING (127). Identity lookup masks the low seven bits; retained words preserve flags. |

The [item-definition](../../docs/design/contracts/item-definition-data.md),
[spell-definition](../../docs/design/contracts/spell-definition-data.md),
[spell-resolution](../../docs/design/contracts/spell-resolution.md) and
[action-construction](../../docs/design/contracts/battle-action-construction.md) contracts
own the accepted boundaries. **Confirmed native:** the
[original herb observations](../../docs/research/map3-messenger-acceptance.md#native-herb-observation-and-completed-branch-recovery)
and winning continuation record the reached item/slot/HEALIN/consumption family. Their bounded
HP facts include 6→11, 9→12, 3→12 and 4→11; these are missing-HP clamps, not four different
item powers. `MedicalHerbTests` checks those minimal facts separately from controlled RNG cases.
It does not relabel authored tests as an original trace replay.

Private Content admits only the selected Medical Herb definition into this item family.
Other carried item IDs remain visible but reject as `item-effect`; equipped weapons are
not herbs. The current class model admits PRST healing EXP and the existing non-healer
classes. VICR/MMNK class admission, broader item families, Equip/Give/Drop, field inventory,
AI healing-item choice and save/load remain unsupported. Exact original presentation timing remains Unknown. The existing
physical-only AI policy rejects a nonempty item loadout at start. This does not close the
continuous H4 milestone or establish successful native-original cancellation.

### Ordinary controls and reproduction

I / gamepad Back selects and cycles held slots; Tab cycles living allied targets, Enter commits
and Escape cancels. The inventory HUD shows slot, item name, selection and rejected attempts.
`bindings.item` in the existing version-1 input settings remaps both keyboard and gamepad.
The adapter initially targets self after selection; legality and all resource changes stay
in the common session.

Authored format-7 packages may supply an optional root `items` list of
`{id, name, effect: "consumable-healing", power, minimumRange, maximumRange}` definitions.
IDs are 0–126; 127 is the empty slot. `actors[].items` contains up to four ordered item words,
padded with 127. This supports independently authored powers/ranges without changing private
source admission. There is no new public schema or original asset in that format.

For affected item/scene changes, use the locked SDK's `dotnet test --filter`
`'FullyQualifiedName~MedicalHerbTests|FullyQualifiedName~BattleSceneTests'`, followed by
`uv run sf2 verify adapter` and the committed engine-scope planner. Existing completed failures remain
in their named handoff; these commands do not request a full slow-suite rerun.

The [scene owner](./presentation-and-assets.md#medical-herb-scenes) gives the current private candidate
and ordinary-input native reproduction. Its existing observer reads actual session inventory/HP,
scene nodes, wait tokens and audio receipts. Source-world startup covers full-HP use after the real
entry program heals the party; a separate controlled battle start covers wounded recovery. Authored
item legality/changed-content assertions remain in `MedicalHerbTests`. The older scalar item observer
is not the scene acceptance command. Preserve failed and successful outputs separately. No original
emulator, screenshot or continuous H4 comparison is implied.
