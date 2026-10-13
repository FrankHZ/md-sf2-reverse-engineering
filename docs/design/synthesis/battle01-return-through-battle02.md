# Battle 01 Return through Battle 02 Admission

The first encounter leaves the party at the tower entrance. The next conditional source-connected segment
returns to the castle, develops the King's situation through an exorcism scene and a commission,
adds Jaha to the force, and opens an encounter outside Granseal on the way toward Yeel.
**Inferred story connection:** the authored events below form the next segment after
[Opening through Battle 01](opening-through-battle01.md). **Confirmed static:** the accepted
[route investigation](../../research/battle01-to-battle02-content-route.md) identifies the callers,
programs, flags and table connections described below. **Unknown:** a naturally executed, continuously
legal journey through this extension. This guide explains the conditional content without presenting
its table connections as a walkthrough.

- Audience: readers following locations, people, dialogue and encounters, with links for research
  review and fidelity implementation.
- Scope: Battle 01 usable return through source Battle 02 admission. Battle 02 play, resolution and
  later scenes are outside this guide.
- Original baseline: `ShiningForceCentral/SF2DISASM` `master` at
  `c834c652b6862bc5679fd7f69a38a7093206efc6`, the pinned US ROM and accepted
  [original evidence owner](../../research/battle01-to-battle02-content-route.md#scope-and-evidence-boundary).
- Remake boundary: the accepted private milestone ends at Battle 01 usable return. The static
  extension here grants no additional playable support or acceptance.

Here **Confirmed** states a source fact or a specifically named bounded original observation;
**Inferred** states the narrative connection; **Unknown** keeps an unobserved relationship open.
Map numbers, source floor names and battle slot numbers are separate identities. In particular,
“Battle 02” means zero-based source slot 2, `BATTLE_TO_YEEL`, rather than verified original UI numbering.

## 1. Leaving the Tower and Returning to the Castle

**Confirmed bounded original endpoint:** the selected Battle 01 winning lineage settles on Map 57
at `(5,12)`, facing Down, with F401 clear, F501 set and roster `[0,1,2]`. A separate accepted ordinary
Down input reaches `(5,13)`. Both observations are terminal and nonresumable; neither is a capture
of the journey below. The [current original frontier](map3-battle01-readiness.md#accepted-original-frontier)
and [endpoint contract](../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint)
own the complete state and provenance.

The preceding after-program, `abcs_battle01`, supplies the narrative handoff. **Confirmed authored
content:** its source comments describe the spirit leaving and the instruction to return toward the
King. It loads the scene's mist-demon entity 128 and Astral entity 129; cursor 2305 and dialogue
commands selecting 129 belong to that loaded scene. These identities come from the source actor
binding, not a guess from a numeric speaker operand. The
[return investigation](../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)
relates that authored content to the existing victory evidence. The whole static program and the
operations reached by the winning acquisition remain different evidence sets.

**Confirmed static connections:** leaving Map 57 at the wildcard `y=19` warp targets Map 40 `(15,13)`,
Down. Map 40's wildcard `y=31` warp targets Map 21 `(9,2)`, Down. Map 21 `(3,16)` targets Map 20
`(3,36)`, Right. These return connections revisit the passage and castle maps from the opening,
but their selected content has changed: Map 21's F501 setup initializes entity 128 at `(6,16)`.
The return setup must not be treated as the earlier tower-guard state merely because the map is the same.

One table-connected castle approach continues from Map 20 `(6,37)` to Map 19 `(6,2)`, Left; from
Map 19 `(23,3)` to Map 20 `(23,37)`, Down; and through Map 20's local warp `(25,35) → (25,50)`,
Right. This identifies where the royal-room event can be investigated. **Unknown:** movement between
these seams with the selected layout, event markers, collision and occupied entity positions. A
wildcard warp record does not make every matching walkable cell a warp terminal. The
[conditional connection owner](../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table)
retains the exact records; [map exploration](../contracts/map-exploration.md) owns their controller boundary.

### Which castle content is selected?

**Confirmed selection rules:** setups use the last set flag in source order; local event tables use
the first matching record. The castle return initially uses F501 setups on Maps 19/20/21 and the
default Map 40 setup. Map 57 selects the void setup. The selected early-state reading assumes F609
initially clear, F3 clear and F402/F502/F452/F88 clear, in addition to the observed F501 set. Those
extra assumptions are not a reconstructed full flag bitset from the terminal observation.

Later flags must not override the intended setup. After 501, Map 19 has ordered alternatives
609/506/507/543/982; Maps 20/21 have 609/506/543. Map 40 has 506/507 after its default; Map 3 has
506/543 after 609. F609 intentionally changes the selected content later in this segment; the other
later overrides must remain absent for this reading. F640 must also be clear when returning to
Map 3, because it switches that map to Map 4 before setup selection. Map 66 has only its default
setup. The [setup owner](../../research/map-data-inventory.md#setup-selection-and-rom-parity)
and [route assumptions](../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table)
keep these local rules distinct from natural campaign state.

The initialization entries matter as well. Map 19's F501 setup enters an internal direct-return
function rather than executing the default init prefix. Map 20's F501 internal init still tests F507.
A setup pointer does not authorize replaying all preceding code in its file.

## 2. The Castle Exorcism Has Two Different Entrances

**Inferred content reading:** the castle scene develops the King's condition and Astral's intervention
before the party receives its next commission. The source binds Map 20's selected F501 entity 128
to the King Granseal sprite and entity 130 to Astral. These are **Confirmed authored identities**;
naturally displayed speakers, portraits, sound and timing for this extension remain **Unknown**.

The full entry is a zone event at `(27,52)`, `Map20_1F5_ZoneEvent0`. **Confirmed caller order:** it
gets the maximum HP and MP for allies 1 and 2 and writes those values to their current HP and MP,
then calls `cs_53B66`. That preparation belongs to the zone caller, before the scene. The script
begins at cursor 2197 and proceeds through the internal suffix `cs_53C42`, where the cursor is reset
to 2200. A zone table record identifies the selected target when the controller dispatches that
event; it does not establish that normal movement reaches and dispatches it.

There is also an entity entrance. Under the same F501 setup, entity 130 with the Down-facing
condition selects `Map20_1F5_EntityEvent2`, which calls `cs_53C42` directly. **Confirmed static
alternative:** this caller enters the shared suffix, omitting both the zone's HP/MP preparation
and the full script's prefix. It is additional local content access, not an observed optional shortcut
or an equivalent initial actor state. Treating the zone as the only source entry would lose this
relationship; treating the suffix as full execution would invent effects.

The shared continuation sets F609 and issues a script warp to Map 19 `(29,8)`, Up. This scripted
transition is distinct from the physical warp records used to approach the scene. **Inferred narrative
connection:** it hands the castle story into the commission setup. **Unknown:** the extension's actual
caller state, transition consumption and visible handoff. The
[accepted caller/script owner](../../research/battle01-to-battle02-content-route.md#conditional-transition-and-content-table),
[dialogue contract](../contracts/dialogue-system.md) and
[script-transition contract](../contracts/map-exploration.md) supply the relevant boundaries.

Additional F501 castle conversations remain local reactions rather than required unlock steps.
Map 19's entity-event references include text IDs 583–586; Map 20's include 588–596/2198, with facing
conditions and temporary flags 256/259–262. The entity-130 suffix call above is the relevant
alternative progression entry. Static availability alone does not establish player-facing optionality,
missability or that every reaction belongs to one playthrough.

## 3. The Commission: A Request to Fetch Hawel

F609 selects Map 19's commission content, `ms_map19_flag609`. **Confirmed init behavior:** its
initialization tests F610. When clear it calls `cs_53176`, then sets F610 only after the script returns;
when set it skips the script. F609 selects the setup, while F610 controls this init call. Neither flag
should be renamed as a single “quest accepted” state.

**Confirmed authored content:** the script stages a departing soldier group and a request from the
King/Minister to fetch Hawel near Yeel. Cursor 721 opens the program; entity operands 138/140 join
the selected Minister/King sprite entries, and `newEntity 144` supplies the speaking soldier. Sarah
and Chester use explicit ally operands. The
[dialogue and actor owner](../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)
retains those bindings and the short source-grounded summary without publishing a transcript.

The choice is a loop in the source rather than evidence for a separate permanent route. At
`cs_53482`, clear F89 repeats the prompt branch, whose refusal cursor is 757. Set F89 enters
`cs_534AC`, whose acceptance cursor is 758. The shared story prompt handler sets F89 for result 0
and clears it otherwise. **Confirmed static:** the accepting continuation ends with Sarah following
Bowie and Chester following Sarah. **Unknown:** a naturally presented answer, the timing and visible
completion of these actions, or a persistent player-choice consequence.

**Inferred story connection:** the commission gives the westward departure its narrative context.
It is not an additional predicate in the later Jaha event or Battle02 selector. F610 already set can
skip the commission; the subsequent Jaha caller does not read F610. The guide therefore explains a
selected conditional sequence without making every scene a shared admission requirement.

## 4. Back in Granseal: Jaha Joins and the Caller Unlocks the Encounter

**Confirmed static return:** Map 19's wildcard `y=31` warp targets Granseal, Map 3 `(29,2)`, Down.
With F609 selected and the overriding flags absent, `ms_map3_flag609` supplies Jaha at `(1,29)` and
two west-side zone records. Its init hides `ALLY_JAHA` when F3 is already set. Entity presence and
joined membership are related source state, not proof of his visible position during an observed return.

The zone order is material to this content:

| Dispatched coordinate / entry state | Confirmed selected behavior |
| --- | --- |
| `(2,23)` | The first exact record selects `Map3_261_ZoneEvent0`, a direct return. It excludes the later wildcard event. |
| `x=2`, another matching Y, F3 set | The wildcard record selects `Map3_261_ZoneEvent1`, whose F3 branch reaches the same direct return. |
| `x=2`, another matching Y, F3 clear | The wildcard caller runs `cs_516A8`; after script return it sets F402 and clears F66. |

**Confirmed authored join sequence:** `cs_516A8` uses cursor 662 and three `nextSingleText` commands
addressing `ALLY_JAHA`. The `join` lies between the second and third, and the script installs Jaha
following behind Chester. The join service's own message path does not add another step to that
script cursor. `csc08_joinForce` calls `JoinForce`, which sets Jaha's joined flag F3 and updates force
membership; battle-party entry remains conditional on capacity. Four Battle02 placement rows do
not establish four live admitted allies under every caller state. The
[party/roster contract](../contracts/party-roster-state.md) and
[accepted join analysis](../../research/battle01-to-battle02-content-route.md#dialogue-and-actor-evidence)
retain those distinctions.

Joining and unlocking are separate effects. **Confirmed caller ownership:** F402/F66 are written
by `Map3_261_ZoneEvent1` after `cs_516A8` returns, not by the `join` command. If F3 is already set
while F402 is clear, this inspected event skips the unlock writer; it does not repair that state.
The exact `(2,23)` exception likewise supplies no unlock. **Unknown:** naturally reached event
coordinates, caller flags, visible following and persistent save state for this segment.

## 5. West of Granseal: What Selects Battle 02?

**Confirmed static exit:** Map 3's wildcard `x=0` warp targets Map 66 `(29,32)`, Down, the source-named
Granseal Kingdom overworld. **Inferred story connection:** this is the departure toward the commission's
Yeel destination. **Unknown:** a legal walk from the Jaha event to an actual west-exit marker; the
warp tuple alone supplies no such path.

Admission uses current map and unlock state. `MainLoop` applies `SwitchMap`, calls `CheckBattle`,
and passes a returned index other than `-1` to `BattleLoop`. **Confirmed source selection:** on Map 66
with F402 set, row 2 is the first eligible row; rows 0/1 have different maps, and row 2's X/Y triggers
are both wildcards. It writes battle area `(16,19,22,18)` and returns index 2. The rectangle is output
origin/size, not a bounds check on the player. Neither reaching that rectangle, finishing Battle01,
F610, nor proximity to an enemy replaces the F402 gate.

Completion does not reject the same matched call. **Confirmed static:** if F502 is already set,
`CheckBattle` clears F402 and still returns index 2 on that call. A later call sees the changed unlock
state. The selected fresh-entry assumption F502 clear is a scenario precondition, not another
shared eligibility rule. With F402 clear, row 2 is ineligible; later Map66 rows may still be selected
if their own unlocks are set. The
[selector analysis](../../research/battle01-to-battle02-content-route.md#battle-02-data-and-admission-semantics)
and [common-map owner](../../research/common-maps.md#confirmed-map-routing) retain scan and completion semantics.

### The encounter data that this admission names

**Confirmed source declarations:** slot 2 selects `BattleSpriteset02` and `BattleTerrain02`; the terrain
slot is not an alias. The spriteset declares four ally placement rows, six enemies, no AI regions and
one AI point. Enemy types comprise two `HUGE_RAT` and four `OOZE`; all ten behavior records use
`STARTING`. These facts describe authored encounter content, not its complete live roster, activation,
spawn state, targetability or first actor. The
[encounter definition](../contracts/battle-encounter-definition.md),
[spriteset inventory](../../research/battle-spriteset-data.md) and
[terrain/routing owner](../../research/battle-routing-data.md) locate the data without reproducing its
placement table or terrain grid. Map 66's empty ordinary entity list and RTS init do not describe this
battle roster; admission precedes the ordinary exploration branch.

Both source slot-2 before and start programs target `ms_Empty`. **Confirmed static wrapper effects:**
they share F452; before-battle tests the flag without setting it, and battle-start sets it before
dispatch when clear. Empty targets therefore do not mean absent wrapper effects or missing extraction
because there is no Battle02 script directory. The
[cutscene-routing contract](../contracts/battle-cutscene-routing.md#shared-intro-gate) owns that shared order.

On the new-battle clear-F88 branch, the controller installs the current map/battle, runs before-battle,
clears F90–F105, heals/initializes combatants, clears AI memory, loads battle content, then runs start.
The saved-battle F88 branch is separate. **Confirmed shared source order:** the existing
[Battle01 admission owner](../../research/map3-battle01-admission.md) closes that controller sequence
for its selected data; it does not become a Battle02 initialization or player-ready observation.
This guide stops at admission. Continue with
[Battle 02 outcome through Battle 03 admission](battle02-outcome-through-battle03.md) for the
conditional return, Yeel/Kazin content and overworld request/consumer connection. That continuation
retains natural-route Unknowns and grants no later remake support.

## Evidence Review and Limits

The review inspected the following accepted payloads and verifier semantics. The
[route investigation](../../research/battle01-to-battle02-content-route.md) owns the individual pinned-source
findings and its reproduced static inventories. This guide adds no fixture or observation, and does
not rerun those evidence commands merely to summarize them.

| Material connection | Accepted payload and verifier / reproduction owner | Review disposition |
| --- | --- | --- |
| Usable return and authored handoff | [victory fixture](../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json), `sf2-map3-battle01-victory-return-static-v1`; [verifier](../../../src/sf2tool/h2/map3_battle01_victory_return.py), `sf2 h2 map3-battle01-victory-return`; [original frontier](map3-battle01-readiness.md#accepted-original-frontier) | Static return spine and later terminal/input observations remain distinct; no resumed extension is inferred. |
| Setups, warps and entity presence | [setup](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`; [content](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [setup](../../../src/sf2tool/h2/map_setup.py), [entity](../../../src/sf2tool/h2/map_entities.py) and [content verifiers](../../../src/sf2tool/h2/map_content.py), `sf2 h2 map-setup / map-entities / map-content` | Ordered selection and data do not close marker/collision/occupancy-aware walking. Individual route records/bindings trace to the accepted source investigation, not invented aggregate fixture rows. |
| Exorcism callers and Jaha unlock producer | [zone programs](../../../tests/fixtures/h2/map-events/zone-programs.json) and [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), sections of `sf2-map-events-static-v1`; [routing setup](../../../tests/fixtures/h2/map-events/routing-setup.json) and [script invocation](../../../tests/fixtures/h2/map-events/script-invocation.json); [verifier](../../../src/sf2tool/h2/map_events.py), `sf2 h2 map-events` | Reviewed `Map20_1F5_ZoneEvent0:341870`, `Map20_1F5_EntityEvent2:342108`, `Map3_261_ZoneEvent1:331510` and ordered tables. HP/MP preparation, suffix entry and post-return F402/F66 remain separate. |
| Commission init | [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`: `routeJoins`, `sourceFiles`, `scriptCallSites`; [verifier](../../../src/sf2tool/h2/map_init.py), `sf2 h2 map-init` | F609 route joins the F610-test/script/post-return-write sequence; internal F501 init entries do not inherit default prefixes. |
| Scene flags, text, join and transitions | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`: `storyStateCommandFacts`, `dialogueCommandFacts`, `forceStateCommandFacts`, `transitionCommandFacts`; [verifier](../../../src/sf2tool/h2/map_script_engine.py), `sf2 h2 map-script-engine` | Source-site/handler joins retain F609, F89, `ALLY_JAHA` and script-warp operands. Individual cursor/actor bindings use the accepted source owner. Handler-local matrices do not prove natural display, choice or chronology. |
| Battle02 row and completion selection | [common maps](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [verifier](../../../src/sf2tool/h2/maps.py), `sf2 h2 common-maps`; [battle global](../../../tests/fixtures/h2/battle-global-data-static-v1.json), `sf2-battle-global-data-static-v1`; [verifier](../../../src/sf2tool/h2/battle_global_data.py), `sf2 h2 battle-global-data` | Shared ordered fragments and aggregate dimensions are not a selected row-2 executable join. The accepted source analysis and its inspected generated row own that conclusion, including same-call completed return. Its proposed fixture extension remains unimplemented. |
| Encounter declarations, terrain and empty routes | [spritesets](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `sf2-battle-spriteset-data-static-v1`; [routing](../../../tests/fixtures/h2/battle-routing-data-static-v1.json), `sf2-battle-routing-data-static-v1`; [spriteset](../../../src/sf2tool/h2/battle_spriteset_data.py), [routing](../../../src/sf2tool/h2/battle_routing_data.py) and [cutscene verifiers](../../../src/sf2tool/h2/battle_cutscenes.py), `sf2 h2 battle-spriteset-data / battle-routing-data / battle-cutscenes` | `rowContentTracked=false` and `routeTargetsParsed=false` prevent treating inventory success as full selected behavior. Specific row-2 operands/targets are accepted source findings; F452 wrapper effects are shared source semantics. |

The map-event verifier preserves opcode/operand/order against pinned H1 before comparing the complete
semantic fixture; init and script verifiers compare their source-derived fields and provenance.
Those guards justify structural claims at their declared boundary. They do not execute these callers
as a continuous story. The selected Battle01 admission fixture also cannot silently supply missing
Battle02 operands or a live force merely because the controller is shared.

**Unknown** remains: legal movement between the listed seams, naturally selected setups and caller
flags, scene/prompt delivery and input readiness, initialized Battle02 membership/activation/first
turn, and cross-process persistence. **Inferred** remains: the connected events' intended narrative
order. No new content platform, comparator, route model, runtime capture or remake capability is
needed to read the accepted conditional segment. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns later scope;
[Story Progression](story-progression.md) explains the wider state relationships.
