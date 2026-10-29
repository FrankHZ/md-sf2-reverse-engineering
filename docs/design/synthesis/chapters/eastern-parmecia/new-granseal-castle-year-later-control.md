# New Granseal Castle: the Year-Later Address and First Control Handoff

The construction segment requests New Granseal's castle. Its first-visit scene presents a gathering
one year later: the minister recognizes the rebuilding work, the king declines a speech, Astral
proposes trading alliances, and the minister dismisses everyone. **Inferred narrative meaning:**
the new settlement is established, and attention turns from construction toward its future.
**Confirmed static behavior:** castle initialization selects the scene through F707, replaces its
assembly with the ordinary population, then returns to the first pending/control read.
**Unknown:** natural arrival, completed movement and presentation services, and actual usable control.

Read this after [Battle08 outcomes through the New Granseal castle handoff](battle08-outcomes-through-new-granseal-handoff.md).
The accepted [research owner](../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md) supplies the
pinned-source, dialogue, actor-list and caller review. This guide stops at the first post-init
pending/control read. Later Peter content, town exploration and battles remain separate work; it
adds no remake capability. The [roadmap](../../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)
owns the Standard-rigor agreement for [#743](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/743).

`Fnnn` denotes flag nnn. Source identities use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../../../README.md#pinned-original-baseline).
Command indices are zero-based script records, not frames or measured durations.

## 1. Arrival Selects a Population before It Selects the Address

**Confirmed conditional entry:** the predecessor supplies castle Map7 `(11,10)`, Up. Map7 has no
flag-substitution or battle-coordinate row, so that accepted admission selects exploration. The
intended completed lineage carries F408 clear, F508/F700/F705 set and player type 0; F458 comes from
the ordinary Battle08 start. These facts do not reset unrelated story or follower history, establish
natural arrival, or supply F707's value.

Exploration clears the old pending word, heals living/immortal allies and performs its entry loads.
It sets current map 7 and not-in-battle state, selects/rebuilds entities and loads sprites, clears
temporary flags 256–383, sets F80, loads the map/base VInts, then calls init before installing
control. The temporary clear excludes F411/F707 and the setup flags below. This entry heal retains
its [shared HP/status limits](../../../../research/chapters/grans-opening/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order);
it does not repeat construction's full-force reset.

The [accepted setup review](../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#entry-setup-priority-and-init-selection)
separates **population selection** from **scene selection**. Map7 scans default, F701, F702 and
F805 in that order; each set flag replaces the candidate, so the last applicable row wins.

| Supplied setup history | Initial population |
| --- | --- |
| F805 set | `ms_map7_flag805_Entities`: 11 NPC declarations, with no Astral declaration. |
| F805 clear, F702 set | `ms_map7_Entities`: 12 NPC declarations; F702 restores the default even with F701 set. |
| F805/F702 clear, F701 set | `ms_map7_flag701_Entities`: five ally and 12 NPC declarations. |
| All three clear | `ms_map7_Entities`, the intended early-history population. |

All three setup pointer tables select the **same** `ms_map7_InitFunction`. Thus unusual F701/F702/F805
history changes the initial population without suppressing the address. Fixed declarations also do
not exclude flag-selected followers or establish a live party roster.

## 2. First Visit Changes Egress; Repeat Entry Skips the Scene

**Confirmed init order:** `ms_map7_InitFunction` tests F707, independently of setup history, entry
coordinates, Battle08 flags or supplied F411. Its two paths have different retained state:

| F707 at entry | Init behavior |
| --- | --- |
| Clear | Set `EGRESS_MAP=6` (New Granseal town), run `cs_55832`, then set F707 followed by F411 **after normal script return**. Supplied F411 set does not skip the scene. |
| Set | Return immediately, preserving supplied egress and F411 and the initially selected population. No scene relocation or final explicit population reload occurs. |

Castle Map7 and town Map6 remain distinct: setting egress does not move the player into town. If the
scene stalls, egress has already changed while the caller's two flag writes remain unreached.
F707-set/F411-clear entry does **not** repair F411. The source comment calls F411 Battle11 unlocked,
but this write performs no battle scan or request. It supplies no immediate Battle11 encounter.

## 3. The Assembly Gives Rebuilding a Narrative Meaning

**Confirmed ordered staging:** `cs_55832` loads Map7 with camera `(6,2)`, installs assembly list
`ce_559AE`, awaits Bowie's init, resets/reloads the display and presents the year-later narration.
The list places Bowie at `(11,7)`, Up, facing the king and minister at the front. Guards and soldiers,
two maids and Astral form the NPC cast; Jaha, Sarah, Kazin and Chester have fixed ally declarations.
The [actor and phase owner](../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#address-actors-and-ordered-phases)
retains their exact references, declarations and all 78 commands.

Those are scene references through `ENTITY_INDEX_LIST`, not physical slots or force membership.
Entity initialization declares flag-selected followers first; an already mapped ally can keep that
mapping instead of receiving its fixed placement. NPC reference allocation is separate. The cast
therefore describes the assembly's intended staging without proving each actor's actual starting
position, recruitment or follower state. Loading it does not heal or recruit anyone.

The minister steps forward and recognizes the rebuilding effort, the need to avoid accidents and
the soldiers' contribution. He turns to the king and calls twice. The king's animation starts for
his reply that he has nothing to say, then stops; the minister responds and invites Astral. Astral
and the minister make sequential, individually awaited steps before addressing the gathering.

Astral discusses the past year, the completed construction and the lack of trading, then proposes
alliances. The minister acknowledges him and dismisses everyone. **Inferred:** the scene marks a
shift from establishing shelter to connecting the settlement with others. Its narration and speech
are not a measured game-clock advance, construction simulation, trading system or alliance choice.
The program contains no inventory, recruitment, alliance or time-advance operation. Starting or
stopping animation changes an animation counter, not health, death or membership.

### Display changes do not restart the visit

**Confirmed handler distinction:** the positive Map7 operands in `mapLoad` and `loadMapFadeIn`
take `LoadMap`'s new-map display branch even though the current map is already 7. They reload map
resources/layout, using `(6,2)` as camera input, not Bowie's destination. They do not call castle
init again, rebuild entities, clear story flags or install control. `loadMapFadeIn` seeds fade-out
state before falling through to the map-load handler; the later explicit fade-in remains distinct.

`resetMap` clears working layout storage and loads with map index -1; `reloadMap` also uses -1 with
its camera operands. These affect the current map's display/area state, not persistent progression
or exploration initialization. The [shared map-load contract](../../../contracts/world/map-exploration.md#load-and-selection-order)
and [scene handler review](../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#map-loading-and-action-semantics)
keep these paths separate. Their calls do not establish visible fade, camera, text or music timing.

## 4. Dismissal Starts Departures before Restoring the Ordinary Population

**Confirmed action order:** six guards/soldiers begin departure sequences without joins, then the
script starts and awaits soldier 136. Astral starts an unjoined step; Jaha's first Down step is
awaited, after which Jaha, Sarah and Kazin begin unjoined departures. Chester's departure is started
and awaited. Astral and references 132/135 then receive Down-facing commands before the reload.
The maids receive no departure commands.

Each named action wait tests that entity's script pointer for idle. Waiting for 136, Jaha or Chester
does not explicitly join earlier actors; there is no global completion barrier before population
replacement. The action and init handlers resolve current mappings directly and do not filter dead
allies. That differs from position/facing/animation commands using the alive-status guard: a
zero-HP ally can still receive these action scripts. Existing follower mappings can alter the entity
and starting position used by an action. Successful motion, obstruction and scheduling remain
**Unknown**; no final per-actor departure coordinates are established.

Command 76 explicitly reloads `ms_map7_Entities`, retaining the main entity's current tile
coordinates and facing. This ordinary list restores king, minister, guards, soldiers, maids and
Astral, with a mixture of init, walking and horizontal-loop declarations. It contains no fixed ally
records, while flag-selected followers still apply. Population replacement discards/rebuilds action
records; it does not prove that earlier unjoined departures finished. Unlike the scene-list load,
this reload has no direct `LoadEntityMapsprites` call, so it is not proof of completed sprite delivery.

The reload uses the **explicit default list**, not another setup selection. With unusual supplied
F701/F805 history it can therefore replace the population while leaving those flags and the selected
setup's entity-event pointers intact. Later population and interaction setup need not match under
arbitrary history; resolving those later interactions lies beyond this guide. F707-set entry instead
keeps its initially selected list.

## 5. The Caller Returns to a Conditional Control Boundary

**Confirmed source-derived position:** under the main mapping and normally completing init-service
assumption, the F707-clear scene leaves Bowie at `(11,7)`, Up. His awaited init returns to idle and
there is no later main movement/facing command; the explicit reload preserves that tuple. F707-set
entry skips the scene declaration and retains `(11,10)`, Up under the same no-intervening-change
assumption. Neither tuple is an observed leader endpoint.

The script's end returns through the interpreter's conditional scroll wait and normal tail. Only
then does the init caller write F707 and F411. Current map remains castle 7, executed-path egress
remains town 6, and supplied player type, force membership and unrelated persistent flags carry
through. There is no second heal, exploration initialization, battle-admission scan or pending clear
at that return.

After conditional music/fade handling and movement-sound setup, exploration reaches `WaitForEvent`.
The **first post-init pending-word read** is this guide's endpoint:

| State at that read | Immediate static consequence |
| --- | --- |
| `MAP_EVENT_TYPE=0`, carried `PLAYER_TYPE=0` | Set view target 0 and install `eas_ControlledCharacter` on entity 0. |
| `MAP_EVENT_TYPE=0`, supplied nonzero player type | The shared control setter selects vehicle control; the scene does not normalize player type. |
| Nonzero pending word | Skip control installation. If still nonzero at the subsequent read, that event returns to the exploration dispatcher. |

Entry cleared the old pending word, and neither this scene nor its init caller explicitly produces
an event. With no intervening write, zero selects the ordinary control path. The word is one shared
slot, not a queue; service writes, survival or overwrite remain unobserved. No specific event is
attributed to this address. Installing a control script does not establish actual usable input or
completed presentation, and both caller flags are already set before this read on the executed path.

## Evidence Review and Remaining Questions

**Confirmed review disposition:** accepted research, the predecessor and the tracked selections
below agree on the conditional chain. The init body preserves egress-before-scene and caller writes;
script witnesses preserve same-map display loads and the explicit default population reload. Action
handler guards distinguish named waits from global completion and omit the alive-status check used
by other command families. No evidence conflict requires a contract or research correction.
Scene-specific dialogue, inline movements, cast and shared init-pointer identities come from the
accepted research's pinned-source review; compact fixtures do not duplicate the entire scene.

The reviewed fixtures contain the exact IDs linked below. Narrow commands have prefix `uv run sf2 h2`;
they name the existing executable owners and **were not rerun for this documentation slice**.

| Relation | Tracked fixture selection / narrow command | Claim boundary |
| --- | --- | --- |
| Initial setup and shared init | [sf2-map-setup-static-v1](../../../../../tests/fixtures/h2/map-setup-static-v1.json), `expected.selectionCases` `map7-alias-restores-default` / `map7-final-variant-wins`; [sf2-map-init-static-v1](../../../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[11]`; [sf2-map-entities-static-v1](../../../../../tests/fixtures/h2/map-entities-static-v1.json); `map-setup`, `map-init`, `map-entities`. | Selector witnesses, declarations/consumers and the seven-operation init body; natural setup history is unproved. |
| Scene loads, actions and reload | [sf2-map-script-engine-static-v1](../../../../../tests/fixtures/h2/map-script-engine-static-v1.json), `expected.transitionCommandFacts`, `entityPopulationCommandFacts`, `entityActionBridgeCommandFacts`, `entityLifecyclePresentationCommandFacts`; `map-script-engine`. | `cs_55832` load indices 1/4/6/10, list indices 2/76 and handler guards/order. They do not execute the scene or close every actor's motion. |
| Init return and pending/control seam | [sf2-map-event-request-consumption-static-v1](../../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `eventRequestConsumption.consumerContexts.waitForEvent`; [sf2-common-maps-static-v1](../../../../../tests/fixtures/h2/common-maps-static-v1.json), [sf2-common-scripting-static-v1](../../../../../tests/fixtures/h2/common-scripting-static-v1.json); same basename commands. | Separate shared consumer facts; no executable composition proves arrival through usable control. |

The [map-init verifier](../../../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../../../schemas/h2-map-init-static-fixture.schema.json),
[script verifier](../../../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../../../src/sf2tool/h2/map_script_schema.py), and
[request-consumption verifier](../../../../../src/sf2tool/h2/map_event_request_consumption.py) retain the
comparison boundaries. The [setup contract](../../../contracts/world/map-setup-data.md#contract-boundary) covers
setup identity separately from the [map exploration contract](../../../contracts/world/map-exploration.md#load-and-selection-order).
The map contract's conceptual setup/init order describes exploration entry; the accepted handler
review distinguishes in-script display loads. Historical handler observations are bounded synthetic
seams, not natural observations of this visit.

**Unknown, preserved:** natural arrival and reachable setup/follower history; actual ally mappings
and HP; successful named actions and unjoined departures; completed display/sprite/camera/text/fade/
audio services and their timing; intervening pending writes, player-type history and actual usable
control; save/load persistence. Counterexamples include F707-set/F411-clear skip, F805 with F707
clear, an already mapped or dead ally, an unfinished unjoined departure, nonzero player type, a
pending overwrite or a service stall before caller return. They constrain interpretation rather
than creating an automatic acquisition queue.

Continue with [New Granseal town: Peter's first introduction](new-granseal-town-through-peter-introduction.md)
for the conditional castle exit, town setup, independent zone scenes and post-event control boundary.

This guide completes neither campaign chronology nor epic #514. Later town/Peter content and battles
need their own accepted owners. Chinese synchronization remains deferred under the roadmap.
