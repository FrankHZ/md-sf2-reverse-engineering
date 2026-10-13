# New Granseal Departure through Battle09 Admission

The selected continuation after Peter's castle audience begins at New Granseal's south exit. Its
overworld connections lead east into the Bedoe region and south into South Parmecia. **Inferred:** this is a
candidate continuation toward the encounter named `BATTLE_TO_RIBBLE`. **Confirmed static:** crossing
those scrolling borders keeps exploration running; Battle09 admission instead depends on an
ordinary entry or a surviving reload request and the battle-unlock state. Its two opening scripts
are empty, but their shared wrapper still changes the intro flag. **Unknown:** a natural continuous
walk and completed battle preparation at this boundary.

Read this after [Peter's castle audience and the return to town](peter-castle-audience-through-town-return.md).
The accepted [research owner](../../research/new-granseal-departure-through-battle09.md) supplies the
selected connections, consumers, zone predicates and opening order. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#755](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/755).
This guide supports content/design planning. It stops at the Battle09 start-dispatcher return,
immediately before `ActivateEnemies`; combat, outcomes, Ribble content and remake readiness are outside
the segment. Fnnn denotes flag nnn, and battle IDs use the source's zero-based table rows.

## 1. Leaving Town Carries History, Party State and Followers

**Confirmed conditional connection:** Map6's south exit matches Y31 with wildcard X and requests
Map73 `(29,53)`, Down, without scrolling. The movement caller still requires the warp layout
attribute `$1000`; an encoded row does not prove that the exit is accessible. A separate `(31,14)`
row has the same destination and is commented `unusable warp`; it supplies no alternate usable-path
claim. If the request survives the poll and pre-dispatch clear, the no-scroll consumer returns its
tuple to `MainLoop`. Map73 has no flag-driven substitution or battle-selection row, so ordinary
exploration entry follows, subject to its services and pending state.

The completed voyage/construction lineage carries F700/F508/F705 set. Peter's audience carries F7
membership, his AI-control bit, F702 and persistent map-follower flag F67. F39 active-party membership
remains conditional on capacity and prior state. Being joined, being active in battle, being under
AI control and being visible as a follower are four distinct facts. Other supplied story flags,
followers and requests remain inputs; the preceding town control read does not establish exit reach.

Ordinary Map73 entry clears pending, sets current map/not-in-battle, rebuilds entities and sprites,
clears temporary flags 256–383, sets F80 and loads the map before init. It also heals eligible
living/immortal allies and adjusts the step counter;
this is not a full-force reset. Map73 uses its default setup with an empty fixed entity list, while
flag-selected followers can still be allocated. Its init chooses from supplied history:

| State at ordinary Map73 init | Selected content |
| --- | --- |
| F700 clear | Run the earlier voyage scene, then set F700 and return without testing construction. |
| F700 set, F508 clear | Return without a scene. |
| F700/F508 set, F705 clear | Run construction, then set F705 and return. |
| F700/F508/F705 set | Return without a scene, as in the completed lineage used here. |

The [voyage](voyage-through-damaged-ship-control.md) and
[construction](battle08-outcomes-through-new-granseal-handoff.md) guides own those earlier scenes.
The selector does not test F701/F702 or arrival coordinates. Map73's default zone handler simply
returns and does not unlock Battle09. Installing control after init still requires pending zero and
carried player type 0; init's return does not guarantee those conditions or usable input.

## 2. East and South Borders Scroll within Exploration

**Confirmed static connections:** the selected Map73 east border leads to Map72, and Map72's south
border leads to Map69. Map73 also has a south exit to Southwest Parmecia, and Map72 has other exits.
This guide selects one candidate branch without making it exclusive or naturally traversable.

| Selected connection | Trigger | Destination and consumer behavior |
| --- | --- | --- |
| Map73 east → Map72 | X63, wildcard Y | Scroll Right; set X to 1, retain Y. |
| Map72 south edge → Map69 | Wildcard X, Y63 | Scroll Down; retain X, set Y to 1. |
| Separate Map72 south connection → Map69 | `(40,62)` | Ordinary no-scroll entry at `(41,16)`, Down. |

The scrolling consumer changes current map, loads map tiles/blocks/layout and performs the map
transition. A negative destination coordinate byte retains that axis. It reads main's actual facing
rather than applying the row's encoded facing byte; the latter remains request-parameter state.
It then returns to the existing exploration loop. These are consumer rules, not a claim that the
movement, scroll, actor or raft services have completed.

Scrolling does not return through `MainLoop` or `CheckBattle`, run ordinary map init, rebuild the
entity cohort, heal the party or clear temporary flags. Consequently, scrolling Map73 → Map72 →
Map69 retains the prior cohort despite both destinations having empty fixed entity lists. Map72's
ordinary-init Luke branch, selected only with F716 set and F10 clear, is not invoked by this border
scroll. Map69's ordinary init just returns. Those facts do not remove zone interruptions during
continued exploration.

The separate fixed south connection is materially different. Its surviving no-scroll request
returns Map69 `(41,16)`, Down, to `MainLoop`, where battle selection precedes ordinary exploration
entry and init. A carried F409 can therefore admit Battle09 immediately, without a Map69 zone visit.
If no battle row qualifies, ordinary Map69 entry runs. No Map69/72 map substitution changes these
selected destinations. Reverse fixed/scroll rows are connection counterparts, not proof of a usable
walk in either direction.

## 3. Overworld Zones Can Interrupt the Candidate Walk

**Confirmed static:** movement needs zone layout attribute `$1400` to produce a zone event. A zone
table's default does not mean every ordinary tile dispatches it. On Map72, selection first checks
X32/33/34 with wildcard Y for the random Battle26 helper, then X25–30,Y37 for the later cannon
handler. The cannon handler immediately returns when F750 is set or F805 is clear; its later body
is outside this segment. The default calls the Battle08 helper with F523 clear, or Battle24's helper
with F523 set.

Carried F508 set puts the Battle08 helper in its completed/random branch, so the selected Map72
walk is not guaranteed silent. The [random-battle owner](../../research/map-event-random-battle-state.md)
owns step-counter, RNG and request production. If a request reaches ordinary battle selection,
Map72's rows are ordered 8,24,26 with wildcard triggers: the first unlocked row wins, rather than
the nearest battle area. These possible interruptions prevent an uninterrupted journey claim;
their encounters and later scene bodies are not audited here.

On Map69, specific and default zones have different consequences:

| Selected Map69 zone | Handler effect |
| --- | --- |
| X23,24,25 with wildcard Y | Call the Battle17 random helper; its own F517/step/RNG rules can set F417 and request reload. It does not set F409. |
| Default, F509 clear | Set F409 and write a partial same-map reload request. |
| Default, F509 set | Return without writes; an existing F409 is not cleared here. |

Specific rows precede the default. A specific handler that makes no request still does not fall
through to the default. The default tests F509 alone: Peter's membership/AI, F702, F700/F705,
facing, step count and RNG are not extra Battle09 unlock gates. The preceding story supplies context
without adding predicates to this handler. The south scroll retains X, so it does not establish
whether a subsequent dispatched zone selects a specific row or the default.

## 4. The Reload Request Retains Part of Its Producer's State

**Confirmed static request boundary:** the ordinary type-6 producer writes the zone X and Y as
words into parameters 1/2 and 3/4. Its consumer applies main's init actscript and dispatches the
selected zone. After that handler, the dispatcher closes portrait/text, waits one VInt and waits
for physical entity 0 to stop. It neither waits for every follower nor clears a newly written
request. Only a later exploration poll can consume the reload; service completion and request
survival remain conditions.

The default's `MAP_EVENT_RELOADMAP=$100FF` long write produces type 1, parameter 1=`00`
(no scroll) and parameter 2=`FF` (`MAP_CURRENT`). It leaves parameters 3/4/5 untouched. This is a
partial ordinary warp request, not a special reload event or a newly supplied position/facing tuple.

The no-scroll consumer samples entity 0's map position, overrides each coordinate with a nonnegative
parameter 3/4 byte, and always takes parameter 5 as facing. With intact ordinary type-6 lineage and
zone Y in 0–63, those coordinate bytes are `00,Y`: the returned tuple is therefore
`MAP_CURRENT,(0,Y),retained parameter 5`, rather than a guaranteed restoration of main's sampled
position. Other lineage retains the general signed-override rule. The selected preceding south warp
writes Down into parameter 5, but neither type 6 nor this default writes it, and intervening writers
can change it. The mutable pending slot is not a queue. Overwrite, interruption, changed current map
or incomplete services can break the entire proposed composition.

## 5. Ordered Battle Selection Determines Admission

**Confirmed static:** `MainLoop` applies `SwitchMap` then `CheckBattle`. `MAP_CURRENT` resolves from
current-map state. Map69's rows are Battle09 (`BATTLE_TO_RIBBLE`) before Battle17
(`BATTLE_TO_TAROS_SHRINE`), both with wildcard trigger X/Y. Their respective unlock/completion pairs
are F409/F509 and F417/F517. The helper writes battle-area outputs before its completion check;
area bounds do not restrict admission.

| Supplied Map69 state at selection | Result of that call |
| --- | --- |
| F409 set, F509 clear | Select 9, even with F417 set or coordinates outside Battle09's area. |
| F409 and F509 set | Clear F409 but still return 9 in the same call; do not continue to 17. |
| F409 clear, F417 set | Select 17, with its own completion handling. |
| F409 and F417 clear | Return the no-battle sentinel `-1`. |

A surviving F509-clear default request sets F409 before selection and therefore takes priority over
a carried F417. Conversely, even a specific zone's Battle17 helper request can select 9 if F409 was
already set: the helper's encounter ID is not a direct `BattleLoop` argument. Completion suppressing
default production is separate from the completed-but-unlocked same-call rule. These conditions
support a candidate Battle09 handoff, not an inevitable story-order encounter.

## 6. Empty Opening Content Still Has a Wrapper Effect

**Confirmed static new-battle order:** selected index 9 enters `BattleLoop`, clears `PLAYER_TYPE`,
sets F399 and tests F88. With F88 clear, it clears elapsed seconds, establishes current map/battle
and base VInts, then calls the before-battle dispatcher. It restores/reasserts map/battle, clears
region flags 90–105, heals eligible allies, initializes ally positions then enemy positions, clears
AI memory, calls `LoadBattle` for combatant 0 and calls the battle-start dispatcher. This guide stops
at that dispatcher's return, before `ActivateEnemies`.

Both source row-9 opening targets are `ms_Empty`, the map-script terminator. They contain no
story-command dialogue, recruitment, warp or story-flag sequence. There is no missing Battle09
before-script dependency to invent. The wrappers nevertheless compute intro flag 450+9, F459:

| Intro state | Before/start behavior |
| --- | --- |
| F459 clear at each wrapper | Before dispatches without setting it; start sets F459 before dispatching its empty target. |
| F459 already set | Skip the corresponding script dispatch; new-battle initialization still runs. |

F509 completion and F459 intro state are separate gates. The empty interpreter return can still
wait for view scrolling when a dialogue window exists, clears scrolling speed and restores registers.
Empty story content therefore does not prove zero service effects or instantaneous completion.
`LoadBattle` also invokes map/sprite/terrain, music and fade services before the start return; their
visible completion has not been observed for this composition.

Battle preparation consumes the active-party list. It places living active allies and can remove
overflow members; it does not derive that list from visible followers. Ordinary Map73 follower
initialization tests F67 for Peter, with supplied F65 selecting the overworld follower prefix on
Maps69/72/73. Other follower flags remain inputs, and scrolling does not rebuild the cohort. Empty
Battle09 scripts do not join Peter or install followers. The 48-entry last-target/memory reset is
separate from Peter's combatant AI-control bit. No concrete roster, Peter action or ready turn is
claimed here.

With F88 set, the separate resume branch restores saved seconds, clears F88 and AI memory, loads
battle state and jumps to the individual-turn loop. It bypasses the new-battle before/start sequence
and its F459 write. Supplying F88 alone does not establish a valid saved-state composition; resume
is a discriminator, not a naturally proved alternate journey in this guide.

## Evidence Review and Remaining Limits

The accepted research supplies the scene-specific connections and their composed static chronology,
anchored to USA retail and pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`.
Tracked fixture selections and the stricter contracts agree with this explanation. In particular,
the helper-local entry contract does not prove natural entry, and intro routing does not promise an
immediate empty return. No research/contract correction is required. The following reviewed seams
are not an executed continuous departure-to-Battle09 comparison:

| Relation | Reviewed executable owner | Limit retained here |
| --- | --- | --- |
| Ordinary entry and battle selection | [sf2-common-maps-static-v1](../../../tests/fixtures/h2/common-maps-static-v1.json), `expected.mapFacts.battleTrigger`; [entry-routing contract](../contracts/map-entry-routing-state.md) and [exploration contract](../contracts/exploration-control-flow.md). | Wildcards, area-before-completion and helper-local mutation; selected connection tuples and scrolling composition come from accepted research, not a natural walk fixture. |
| Destination init | [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[76]`, `[78]`, `[79]`. | Map69 return, Map72 F716/F10 and Map73 F700/F508/F705 selectors; no guarantee that scrolling runs them or that services complete. |
| Zone handler selection/effects | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json) and [zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json), `expected.zoneTargetPrograms` for Map69/72/73. | Named predicates and request writes; no natural first zone or accessible layout. |
| Pending request and interruptions | [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `eventRequestConsumption.consumerContexts.waitForEvent` / `processMapEvent`; [request owner](../../research/map-event-request-consumption.md) and [random-battle owner](../../research/map-event-random-battle-state.md). | Poll/clear/access and shared helper seams; actor state, remaining parameter bytes and request survival need the caller lineage retained above. |
| Opening wrappers and preparation | [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `expected.cutsceneFacts.intro`; [sf2-battle-control-static-v1](../../../tests/fixtures/h2/battle-control-static-v1.json), `expected.controlFacts.mainLoop`; [sf2-battle-loop-static-v1](../../../tests/fixtures/h2/battle-loop-static-v1.json), `expected.lifecycleFacts`; [intro contract](../contracts/battle-cutscene-routing.md). | Shared flag and resume/initialization boundaries; row 9's empty targets and pre-activation composition are accepted research, without ready-turn or service completion proof. |
| Peter and script return | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), force-state/control command facts; [party/roster contract](../contracts/party-roster-state.md) and the predecessor guide. | Joined/active/AI/follower distinctions and terminator semantics; no loaded actor cohort, Peter AI action or natural control claim. |

The [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../schemas/h2-map-init-static-fixture.schema.json),
[request-consumption verifier](../../../src/sf2tool/h2/map_event_request_consumption.py) and
[schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json), and
[cutscene verifier](../../../src/sf2tool/h2/battle_cutscenes.py) and
[schema](../../../schemas/h2-battle-cutscenes-static-fixture.schema.json) compare bounded structures,
provenance and fixture facts rather than proving this natural route. Their CLI owners use
`uv run sf2 h2` followed by `map-init`, `map-events`, `map-event-request-consumption`,
`map-event-random-battle-state`, `common-maps`, `battle-control`, `battle-loop`, `battle-cutscenes`
or `map-script-engine`. **These extraction/runtime rails were not rerun for this design slice.**
The [research reproduction boundary](../../research/new-granseal-departure-through-battle09.md#coverage-controls-and-reproduction)
owns source anchors and retained comparisons; optional derived payloads are not required or generated.

**Unknown:** natural exit/border/zone reach, collision and first zone coordinates; reachable supplied
flags, followers and active party; intact actor/request state through all services; completed scroll,
load, music/fade and interpreter effects; actual battle/input readiness; and save persistence.
These gaps are not an automatic acquisition queue. Battle09 outcomes and Ribble require separately
accepted successor evidence. This guide completes neither campaign chronology nor epic #514,
selects no engine/product change and leaves Chinese synchronization deferred under the roadmap.
