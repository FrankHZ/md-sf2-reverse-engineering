# Map Design Principles: Exploration, Interaction and World Continuity

<a id="map-design-principles-evidence-bounded-structural-patterns"></a>

After the first victory in Battle13, Luke suggests asking Polca's mayor for a raft. The south
exit is open even if Bowie leaves without speaking to the mayor. Staying to talk can change what awaits the party outside. Leaving
halfway through those conversations and coming back can mean starting them again. Even with the
mayor's promise, Bowie has to find and board the raft before he can steer it. Polca shows how
a piece of information becomes a choice of route. [Battle13 and the mayor](../chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md)

## Connected Example: Polca Return to Southern Travel

**Confirmed** original rules give the following alternatives. The connections between them are
an **Inferred** reading of the journey. The sources identify the exits and their consequences
under specific conditions. An uninterrupted original trip along each route remains **Unknown**.

1. **Listen to Luke or head south.** With Bowie alive and the enemy leader defeated, the first
   victory brings the party back to Polca and arranges for Luke to follow. He has yet to join
   the force. A repeat win skips that scene. Defeat or an eligible retreat returns the party
   to Bedoe. At Polca the south exit works without the raft, Luke's recruitment or Oddler's
   company. [Battle outcomes](../chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md#battle-outcome-and-where-it-returns) · [South exit and Luke](../chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md)
2. **Stay for the bargain.** In the post-victory town, before the raft grant or the later Oddler
   conversations have been recorded, speak to the mayor three times. Each completed exchange
   advances the conversation for this visit. On the fourth, Luke offers to keep a secret in
   exchange for the raft. A fresh entry resets the unfinished exchanges. Once the bargain is
   complete the raft has a berth outside town. Asking again gives a reply and leaves the raft
   where it is. Ripster, who left during the scene, can appear on a fresh visit.
   [Mayor contacts and grant](../chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md#optional-mayor-contacts)
3. **Find the raft.** The south exit leads to the overworld. An unlocked encounter there can
   intervene before exploration begins. On an ordinary fresh arrival on foot, the party's raft
   appears if its recorded berth is on this map. To board, approach an unobstructed boarding
   point outside battle with the raft directly ahead. If the raft is out of alignment, Bowie
   stays on foot and can try from another position.
   [Raft boarding](../chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md)
4. **Cross into South Parmecia.** Two southern exits lead to the same region. The fixed exit
   checks for an unlocked encounter before placing the party on the new map. The scrolling
   border, away from corners claimed by an earlier exit, carries the current group and raft
   across without checking for a battle at that crossing. An encounter can therefore interrupt
   one arrival while the other continues sailing. Whether both approaches are naturally reachable
   in a given playthrough, and what happens after a full raft landing, remain **Unknown** here.
   [The two southern exits](../chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#two-southern-exits-one-correct-destination)

## Action and Interaction

What you try at a place matters as much as where you stand. Face a resident to talk, search the
square in front of you for an object, choose a carried item to use it, or walk into an event
area. A counter can allow conversation across it. Another actor can block a step before Bowie
reaches a doorway or boarding point. A door that changes the terrain requires a fresh check
before the step can go ahead. [Movement and interactions](../../contracts/world/map-exploration.md#areas-events-and-items)

In the original, A opens the field menu. C can open a co-located Caravan's services, talk to
someone or inspect a spot, falling back to the menu when appropriate. A takes precedence when
both actions are pressed. A map event already pending at the same check takes priority over
either button. Exactly when the original makes an event available relative to a button press
remains **Unknown**. [Exploration controls](../../contracts/world/exploration-control-flow.md#player-action-and-entity-interaction-boundary)

At Ribble, searching the tree and *using* the Wooden Panel do different things. Select the
Panel while facing the tree from either accepted position to open the passage. Opening it still
leaves the Achilles Sword to be collected from a separate chest. A full pack on Bowie passes
the offer to the other eligible members. If nobody has room, the chest closes and the party can
come back for the sword. Inspecting a place can also give a description rather than an item.
[Ribble tree and sword](../chapters/southern-travel-creed/ribble-tree-opening-and-achilles-sword.md) · [Search rules](../../../research/world/field-search-control.md#confirmed-static-spine)

## Dialogue and Continuation

A refusal can change who gets to ask next. After the raft bargain, the mayor can ask the party
to take Oddler along. Decline, and the party leaves that conversation as it came. Oddler can still
ask for himself if Bowie reaches the place where he speaks up. Decline *him* and he explains
the danger of staying. Luke pleads for him and Peter agrees to take him along, without a second
prompt. The mayor's request is optional even when Oddler later speaks. Accompanying the party
here gives Oddler a place in the travelling group, without a battle place. [Polca's two requests](../chapters/eastern-parmecia/polca-oddler-choice-and-following.md)

The player still needs time to read a reply, answer a question and wait for people to move.
The original has distinct waits for different kinds of text. In the remake, revealing the rest
of a line lets the player read it before acknowledging it. Ordinary walking resumes after the
scene releases control and any pending event has been handled. [Dialogue and input](../../contracts/interface/dialogue-system.md#poll-order-and-distinct-consumers) · [Remake text policy](../../../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy)

## Transport: Acquisition, Boarding, Travel and Landing

Polca's raft has a berth the party must reach. The Caravan, earned after the first victory over
Taros, has its own boarding point. Walking onto that point can put the party aboard. Opening
its service menu to manage items is a separate choice. On an eligible landing the party goes
back to walking. A walking encounter such as the approach to Battle19 still needs its own
trigger after landing. The raft and Caravan have different boarding and landing rules. The raft
evidence follows the request to land, leaving the full landing **Unknown**. [Caravan acquisition](../chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md) · [Caravan travel and landing](../chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)

## Return and Persistence

A place can remember a bargain and still bring back a resident who walked away during it. On a
fresh entry, the original rebuilds the town and its people for the current story. When several
versions of a map qualify, the last eligible version in the ordered list wins. Within that
version, the *first* matching interaction wins. In Polca, an earlier event area can take priority
even where it overlaps the spot for one of Oddler's requests. Simply standing on the right square
does not guarantee he'll speak. [Map selection and load order](../../contracts/world/map-exploration.md#load-and-selection-order) · [Oddler's conditions](../chapters/eastern-parmecia/polca-oddler-choice-and-following.md)

How Bowie returns matters too. A scrolling crossing can rebuild the terrain and keep the people
already present. Reloading the current map can preserve changes to its working terrain. After an
ordinary battle the game switches maps and returns to exploration. Entering by a warp can check
for a battle first. A return point identifies where to go during play. A church save or a battle
suspend has its own rules for what will be restored. The original evidence covers particular
saved fields and in-process returns. Complete persistence across a restart and the natural route
to every interaction remain **Unknown**. [Map entry and battle selection](../../contracts/world/map-entry-routing-state.md#battle-candidate-selection) · [Original save rules](../../contracts/system/save-system.md) · [Remake save and Continue](../../design-map.md#exploration-services-and-savecontinue)

The private remake has playable routes through the mayor's bargain, boarding and a first water
step, and through Taros's first defeat and Caravan travel. Those are observations of the remake.
The original route limits above still apply. [Polca and the first raft step](../../../../remake/docs/application/polca-first-raft.md) · [Taros Shrine](../../../../remake/docs/application/taros-caravan.md) · [Current campaign support](../../../../remake/docs/capability-status.md#campaign-feature-trace)

Return to the [gameplay overview](../gameplay-overview.md#exploration-moving-facing-and-discovering-a-result)
for the wider loop. [Chapter guides](story-progression.md#read-story-content-in-segments) follow the routes region by region.

## Pre-Synthesis Evidence Audit

### Retained 2026-08-02 audit

The following is the original structural audit and its resolved #22 failure record. Its limits apply
to those fixtures at that date; later accepted chapter and consumer evidence qualifies the older
map-wide Unknown wording at named scope, rather than retroactively expanding the old observations.

The audit compared owning research prose, the evidence-bound
[map and exploration contract](../../contracts/world/map-exploration.md), fixture payloads and IDs, and the focused
commands named by each owner. It deliberately checked aggregate units, alias counts, selection
quantifiers, mutation lifecycle, and whether runtime harnesses execute later story or presentation
effects.

| Surface reviewed | Cross-check and disposition | Boundary retained |
| --- | --- | --- |
| map content and geometry | [Map-content research](../../../research/world/map-content.md), `sf2-map-content-static-v1`, `sf2-map-layout-decode-v1`, and `sf2-canonical-map-import-v1` agree on 79 map definitions, 77 block/layout payload pairs, two map aliases, 64x64 layouts, 3x3-tile blocks, and 1,027 map-content logical records. Fresh content, layout, and canonical-import reproduction passes after the owner correction recorded below. | The tracked fixtures retain aggregate structure, not redistributable layouts or a rendered-map result. |
| setup variants | [Map-data research](../../../research/world/map-data-inventory.md), the static setup fixture, and the ten-case runtime selector agree on 64 routed map IDs, 126 setup definitions, 15 missing routes, ordered flag scanning, and last-set-flag-wins. | The runtime matrix uses the debug Map Test path; normal-save flag combinations and story reachability are **Unknown**. |
| entity, zone, and item selection | The static event owner records 1,134 physical records and first-match dispatch. The nine-case runtime matrix confirms selected offsets/targets while replacing each selected script entry with `rts`. | Script effects, transition persistence, facing/presentation consequences, and normal-story admission are outside that fixture. |
| area descriptions | `sf2-map-descriptions-static-v1` owns 75 targets and 227 physical entries plus first-match consumer shape. It also distinguishes normal exploration's `d6=1` path from conditioned functions. | No runtime fixture closes text content, player-visible presentation, or nonstandard callers. |
| working-layout mutation | [Map-content research](../../../research/world/map-content.md) owns rebuild/preservation phases. The seven-case block-mutation fixture confirms ordered word copies and update-bit seams on bounded layout windows. | Collision, pathfinding, save/reload persistence, normal-story reachability, and visible VDP effects remain **Unknown**. |
| triggers, camera, and transitions | The interaction, camera, and transition fixtures confirm only bounded handler entry/return, state transfer, and service seams. | Source labels and successful returns do not prove player-visible movement, camera composition, fade duration, event consumption, or route meaning. |

No semantic contradiction was found among the accepted fixture payloads or their owning prose. Fresh
reproduction on the initial base `e9b60f18832e9e74ed8749239d7726e871710d7b` did find an owner
integration mismatch: `uv run sf2 h2 map-import` read stale `callTargets` from map-init source rows
that exposed `directCallTargets`, and exited with `KeyError`. The design lane did not alter that
tooling, fixture, or schema surface. The research owner corrected the mismatch through
[Issue #22](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/22); after this branch was
updated to main `5f1dbcca961a678cac774a4eadf1e21e3bd1c03b`, a fresh map-import run passed with
79 maps, 1,859 resources, and 15,805 logical records. This history records the adversarial review
without treating the earlier failure as a current evidence boundary.

The audit also found an important quantifier boundary: complete static corpora coexist with
deliberately narrow runtime cases. This document therefore uses corpus counts to describe available
channels and identities, but never uses the runtime matrices to claim complete campaign behavior.

### Current thematic evidence review

This revision reviewed the map-content/map-data and gameflow owners, relevant map/routing, dialogue,
input and save contracts, current thematic return/resource explanations and selected Polca/Caravan
research and guides. It inspected the public fixture facts/IDs and owning schema/verifier boundaries
for gameflow, Search, setup, event selection and map-init, including the selected Map69 init body.
A bounded reread of pinned exploration dispatch, vehicle boarding records/consumers and Map69's init
confirmed their selected source joins; this did not collect new original runtime evidence.
No original observation, extraction or historical suite was rerun for prose.

| Selected claim | Cross-check and disposition |
| --- | --- |
| Attempt, Search and order | `sf2-gameflow-core-static-v1`, `sf2-field-search-control-static-v1`, `gameflow.py` and `field_search_control.py` retain caller polarity, marker/fallback and inventory-call order; their schemas/semantic comparisons do not claim a natural player route. |
| Setup versus script effect | `sf2-map-setup-static-v1`, `sf2-map-event-dispatch-runtime-v1`, its schema and `map_event_dispatch.py` retain last-set selection, nine cases and target stubs. Later caller/script evidence adds concrete effects without converting these stubs into execution. |
| Polca grant and return | Selected outcome, caller temporary flags, F513 setup, F716/F64 tuple order, current Ripster hide versus fresh declaration, and Luke's independent init/membership gates agree across the accepted chapter and research owners. No speech implies HP, following implies recruitment, or stored tuple implies contact. |
| Boarding and destination | Request-state/consumption fixtures retain topology; the raft owner's adjacent source review owns physical31 alignment, mode2/opcode8 and exact Map72→69 rows. `sf2-map-init-static-v1` `primarySourceBodies` sourceOrder76 retains Map69's sole `rts`. The corrected PR818 destination join controls over an assumed Map70; aggregate fixtures do not cover a whole journey. |
| Dialogue, input, transport and saving | Current source-bound W1/W2/choice and host/audio owners distinguish original observations from modern input/clock/reveal. Caravan's event4 landing is separate from raft's event5 request. Later Church/serialization owners qualify old helper-only save prose without claiming durable storage. |

The historical map-data summary's broad side-effect/transition labels and old runtime question
queues are read at their owning fixture boundary. The current map contract, chapter source reviews
and accepted remake consumers establish selected additional rules; full campaign reach/collision,
original rendered parity and complete persistence stay **Unknown**. No blocking owner contradiction
was selected or repaired outside this file. Reproduction commands remain with each research owner;
no old queue becomes an automatic new collection task.

## Evidence Matrix

The retained structural matrix below describes each fixture's own proof boundary. “Unknown” in a
row means unproved by that fixture; it does not erase the later selected chapter/text/control joins
above. The contracts and research owners retain exact commands and original provenance.

| Design surface | Bounded evidence | Owner and exact executable trace | Remaining boundary |
| --- | --- | --- | --- |
| content envelope | **Confirmed static** 79 definitions, 662 source sections, 154 private payloads, optional-pointer boundary | [map-content research](../../../research/world/map-content.md); `sf2-map-content-static-v1` ([`map-content-static-v1.json`](../../../../tests/fixtures/h2/map-content-static-v1.json)) | Rendered content and private payloads are not tracked |
| geometry and aliases | **Confirmed static** 77 payload pairs serve 79 references, with two aliases; every decoded layout is 64x64 and every block reference is in range | [common-map research](../../../research/world/common-maps.md); `sf2-map-layout-decode-v1` ([`map-layout-decode-v1.json`](../../../../tests/fixtures/h2/map-layout-decode-v1.json)) | Passability, collision graph, and rendered parity |
| engine-neutral resource graph | **Confirmed static** 79 definitions and 1,859 identity-preserving resources with resolved non-null references and raw flags | [map-content research](../../../research/world/map-content.md); `sf2-canonical-map-import-v1` ([`canonical-map-import-v1.json`](../../../../tests/fixtures/h2/canonical-map-import-v1.json)) | Full generated import is private; no engine/editor decision |
| state-selected setup | **Confirmed static/runtime** 64 routes, 126 setup definitions, 15 missing routes, all-row scan, last-set-wins, and bounded default/alias cases | [map-data research](../../../research/world/map-data-inventory.md); `sf2-map-setup-static-v1` ([`map-setup-static-v1.json`](../../../../tests/fixtures/h2/map-setup-static-v1.json)) and `sf2-map-setup-selection-runtime-v1` ([`map-setup-selection-v1.json`](../../../../tests/fixtures/h3/map-setup-selection-v1.json)) | Normal-save combinations, selected-setup meaning, and story reachability |
| entity/zone/item selectors | **Confirmed static/runtime** 1,134 physical records, first-match rules, and nine bounded selected target cases | [map-data research](../../../research/world/map-data-inventory.md); `sf2-map-events-static-v1` ([`map-events-static-v1.json`](../../../../tests/fixtures/h2/map-events-static-v1.json)) and `sf2-map-event-dispatch-runtime-v1` ([`map-event-dispatch-v1.json`](../../../../tests/fixtures/h3/map-event-dispatch-v1.json)) | Selected scripts were stubbed; later effects and presentation are **Unknown** |
| area descriptions | **Confirmed static** 75 targets, 227 physical entries, first-match shape, and normal-exploration `d6` boundary | [map-data research](../../../research/world/map-data-inventory.md); `sf2-map-descriptions-static-v1` ([`map-descriptions-static-v1.json`](../../../../tests/fixtures/h2/map-descriptions-static-v1.json)) | Text, nonstandard callers, runtime reachability, and presentation |
| scripted block mutation | **Confirmed static/runtime seam** source-shaped block-copy commands plus seven bounded forward-copy/update observations | [map/exploration contract](../../contracts/world/map-exploration.md); `sf2-map-script-engine-static-v1` ([`map-script-engine-static-v1.json`](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)) and `sf2-map-block-mutation-runtime-v1` ([`map-block-mutation-v1.json`](../../../../tests/fixtures/h3/map-block-mutation-v1.json)) | Collision/pathfinding consumers, persistence, normal reachability, and visible timing |
| roof and step triggers | **Confirmed bounded runtime** six hit/miss/gate cases over original handlers and table scans | [map/exploration contract](../../contracts/world/map-exploration.md); `sf2-map-interaction-trigger-runtime-v1` ([`map-interaction-trigger-v1.json`](../../../../tests/fixtures/h3/map-interaction-trigger-v1.json)) | Full layout effects, collision/pathfinding, audio/presentation, persistence, and story reachability |
| camera seam | **Confirmed bounded runtime** seven target/destination/speed handler cases and state/service chronology | [common-scripting research](../../../research/world/common-scripting.md); `sf2-map-camera-control-runtime-v1` ([`map-camera-control-v1.json`](../../../../tests/fixtures/h3/map-camera-control-v1.json)) | Player-visible camera behavior and normal-story reachability |
| transition seam | **Confirmed bounded runtime** five interpreter streams including `warp`, bounded event bytes, state fields, and direct service entry/return | [common-scripting research](../../../research/world/common-scripting.md); `sf2-map-script-transition-runtime-v1` ([`map-script-transition-v1.json`](../../../../tests/fixtures/h3/map-script-transition-v1.json)) | Event consumption, fade/display timing, persistence, collision/pathfinding, and normal-story reachability |
