# Bedoe's King Address and First Audience

The king's address stages a report about the devils, orders vigilance and disperses seven guards.
The first audience brings Peter and Bowie into a conversation with Luke, the king and a birdwoman:
introductions lead to warnings about Zeon and an invitation to see Volcanon. These are two separately
admitted scenes. Their dialogue supplies a candidate content connection from
[Bedoe's initial guard encounter](battle12-outcomes-through-bedoe-entry.md), without enforcing that
encounter or the address as an audience prerequisite.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/bedoe-king-address-through-first-audience.md) supports the requests,
actors and caller boundaries below. **Inferred:** the scenes form a candidate continuation of the
English content guides. **Unknown:** natural floor access and order, supplied state, completed
services, visible presentation and usable input. This guide adds no remake capability or product
decision. All conversation summaries are attributed speech, rather than independently established
history about Grans, the devils or Volcanon.

Stop after the first audience caller and conditional restoration of control. The separate
[Bedoe to the Volcanon audience guide](bedoe-to-volcanon-audience.md) continues through selected
shrine requests, the conversation, Peter's membership/AI/following changes and conditional Map11
control. That connection retains its own admission and natural-route limits. `cs_5766C` and later
king choices, Skreech, further recruitment, services/floor inventory, Battle13 and unmerged successor
findings remain outside this guide.
Fnnn means flag nnn; coordinates are map tiles unless another unit is named.

## Two Independent Opportunities to Hear the King

**Confirmed selection:** Map10's default setup supplies these ordered zone rows:

| Request predicate | Caller and admission |
| --- | --- |
| X29, any Y | `Map10_ZoneEvent0`: F720 clear runs address `cs_573EC`, then sets F720; set returns. |
| X33, any Y | `Map10_ZoneEvent1`: F721 clear runs audience `cs_574A6`, then sets F721; set returns. |
| X31, Y53 | The same audience caller and F721 gate. |
| No match | Default caller returns without either scene. |

The table uses byte coordinates, independent 255 wildcards and the first matching row. A caller
suppressed by its own flag does not fall through to another row. Neither caller tests F719, the
other caller's bit, membership, HP or actor presence. Both bits clear permit either selected caller;
one set suppresses only its own scene; both set suppress both. Two audience rows still share one
script call site. This establishes independent admission in supplied state, not a demonstrated
route that reaches the audience first.

F722 selects a different setup and replaces the zone table, including all three rows above.
Its later zone is not analyzed here. Zone dispatch resolves the current setup again; it does not
derive the cast or zone table from dialogue, a remembered scene order or F719 alone.

A coordinate predicate is only part of admission. Ordinary movement's zone producer requires the
selected layout attribute masked by `$3C00` to equal `$1400`. It writes pending event type 6 and
word X/Y parameters derived from destination plus attempted movement offsets, divided by 384.
`WaitForEvent` consumes pending requests; `ProcessMapEvent` clears the event type before dispatch.
The zone consumer installs `eas_Init` for physical 0 and supplies the coordinate words; the table
compares their low bytes. No selected floor/path evidence proves that those attributes are naturally
reached, that stairs connect these opportunities, or that walking enforces a scene order.

## Who the Scenes Use

**Confirmed actor identity:** initialization clears mappings, installs flag-selected followers,
then reads the selected fixed/walking declarations. Ally declarations retain their character
lookup and do not increment the regular NPC stream from 128. An existing ally mapping skips its
fixed declaration. Character lookup, declaration position and physical entity slot are distinct.

Default Bedoe declares Luke as ally 10 at `(38,51)`, Up; an F75 follower mapping can supersede that
placement. King 147 starts at `(36,49)`, Down, and birdwoman 148 at `(37,50)`, Down. The birdwoman has
no established personal name here. Guards 150–156 are seven birdmen; the preceding guard scene
explicitly returns 150/151 to `(34,51)` / `(35,51)`, Up. Peter 7 has no fixed row: F67 supplies his
follower declaration. Bowie/main is separately declared as physical 0.

F722's alternate population preserves Luke, king 147 and birdwoman 148, but changes other identities
and placements: lookup 152 is a birdwoman, 153 a chick and 154–156 have no fixed rows. Its normal
zone table does not select either scene. A supplied mismatched population/caller cannot safely be
described as the default seven-guard cast.

Peter's F67 declaration is independent of joined/active F7/F39, word HP and force AI. Luke's fixed
row or F75 mapping likewise proves neither F10/F42 membership nor recruitment. Neither scene joins
force/party, changes AI or repairs either follower flag. Exploration healing, including Peter's
special eligibility, does not prove a mapping and is not repeated just for a zone scene.

Neither program reloads the map or rebuilds entities. It consumes current mappings, positions and
actions. Lookup resolution masks the character to a byte, translates NPC 128+ through the `$60`
index difference, reads the physical index and scales it to an entity address. An uninitialized
zero mapping resolves physical 0; it does not safely skip absent Peter. A mapping invalidated to
`$FF` by hiding instead scales physical index 255 without a validity check. Action, nod and hide
requests add no general presence guard. Supplied missing or aliased mappings can therefore change
which entity is affected.

## The Report, the King's Orders and Dispersing Guards

**Confirmed ordered content:** address `cs_573EC` requests ten texts, 1641–1650 (`0669..0672`),
with six from king 147 and four from birdwoman 148. The following paraphrases cover all requests;
the full command/text ledger stays with research.

1. **The report (1641–1644).** The camera is sent to `(31,47)` with a scrolling wait. The birdwoman
   describes a powerful greater devil (1641) and credits Volcanon with sweeping away the devils
   (1642). The king asks about the greater devil (1643). After the birdwoman moves Left1 and faces
   Up, she reports his escape (1644).
2. **Pursuit and danger (1645–1649).** The king asks about pursuit (1645) and capturing the greater
   devil before he recovers (1646). The birdwoman asks whether he came from Grans (1647). The king
   nods, offers his belief about that suggestion (1648), and warns of another attack (1649).
   These remain speculation, reports and intentions within the conversation.
3. **Vigilance and dispersal (1650).** After a requested 10-frame sleep, the birdwoman faces Down
   and the king orders vigilance. A requested 40-frame sleep precedes the guards' relative paths.
   Actions start for 150 and 152–156 without waiting; 151's path is started and waited. The script
   then hides 150–156 in order, waits for 148 to move Right1/faceDown1, and ends.

There is no choice, warp, join, following command or internal story-flag write. The two sleeps total
50 requested frames; text, camera and action services prevent treating that total as scene duration.
The dispersal requests do not prove a completed patrol or its destination.

**Confirmed hiding boundary:** `HideEntity` writes current positions and destinations to internal
`$7000,$7000`, replaces every matching physical-index mapping entry in the 64-byte
`ENTITY_INDEX_LIST` with `$FF`, and removes that physical index from `EXPLORATION_ENTITIES` by
shifting later entries. Action pointers and travel fields remain. The ordinary update loop skips
entity/action dispatch when X is at least `$7000`; retained nonwaited actions are not thereby
completed, and do not keep advancing normally there. No party flag is cleared. With normal distinct
guard mappings, king, birdwoman, Luke and Peter remain mapped; bad aliases can broaden the effect.

This differs from the predecessor's F720 re-entry handling. Fresh initialization reconstructs
population and mappings; default init then calls `MoveEntityOutOfMap(150..156)` when F720 is set.
That helper changes position/destination to `$7000,$7000` while retaining mappings and exploration
list entries. It neither reuses the old `$FF` mappings nor performs the address's hide operation.
F719 still independently gates the initial guard scene, before this F720 handling.

## Luke's Greeting and the Visitors' Introductions

**Confirmed ordered content:** audience `cs_574A6` requests 29 texts, 1651–1679 (`0673..068F`).
Luke speaks five times, Peter twelve, king 147 eight and birdwoman 148 four. Bowie has no text
request; his requested nods are staging, rather than spoken responses.

1. **Greeting Peter and the king (1651–1654).** The camera moves to `(34,47)` and waits. Luke
   moves Down1/faceLeft1, performs two waited jumps and calls Peter (1651). The script requests
   Peter at `(33,52)`, Right, and Bowie at `(33,53)`, Right, then waits for the camera at `(31,47)`.
   Peter moves Right3, greets Luke (1652), faces Up and greets the king (1653). The king receives
   a waited inline speed 48/48 action and moves Down1 before returning Peter's greeting (1654).
2. **Introducing Bowie (1655–1657).** Peter moves Up1; requested sleeps and a nod surround turns
   Left then Up, before his introduction (1655). He moves Down2/Left2, followed by a sleep and
   Bowie's nod. Peter starts Right3/faceUp1 without waiting while Bowie performs the same path
   with a wait and nods. Peter introduces Bowie as coming from Grans (1656). Luke moves
   Down1/faceLeft1; Peter/Bowie face Right, and Luke identifies Bowie as Astral's pupil (1657).
3. **Luke's acquaintance and messenger role (1658–1661).** Peter performs two waited jumps and
   questions Luke's knowledge (1658). Luke explains his familiarity with Bowie (1659), Peter
   questions that account (1660), and Luke nods and explains that he was Volcanon's messenger (1661).
   These requests convey the speakers' explanation of their connection, without independently proving the
   history of Astral, Bowie or Luke's travels.

## Zeon, Grans and the Invitation to Volcanon

**Confirmed continuation of the same audience:**

4. **The king's warning (1662–1666).** The king moves Down1; Luke, Peter and Bowie face Up.
   The king reports Zeon's release (1662) and blames Grans (1663). After a waited speed 16/16
   action, he moves Up2/faceDown1 and warns the visitors (1664). Peter moves Up1/Left1/faceUp1
   and asks about Zeon (1665); the king identifies him as king of the devils (1666).
5. **Going to see Volcanon (1667–1672).** Birdwoman 148 moves Down1, urges a visit to Volcanon
   (1667) and suggests Peter may calm him (1668). Peter hesitates (1669). Luke moves Up1/Left1;
   Peter faces Right, Luke urges an immediate visit (1670), and Peter agrees (1671). Peter turns
   Down and tells Bowie to follow him (1672). This is a spoken request; the final following
   command has Peter following Bowie.
6. **Permission for Bowie (1673–1679).** The script waits for king 147's `eas_Init` and then his
   Down1 movement. He asks whether Bowie is coming (1673). Peter/Luke face Up, Peter answers
   (1674), and the king begins an objection (1675). The birdwoman moves Left1/faceUp1 and
   advocates Bowie's audience (1676–1677). The king nods and consents (1678). Birdwoman, Luke
   and Peter face Down; Peter closes the exchange (1679), the script attempts Peter→Bowie
   following with selector 2, and ends.

The invitation does not request Map11, display Volcanon, set F722 or recruit Luke/Peter. There is
no choice/menu, warp, entity/map reload, join or internal story-flag write. The four sleeps total
120 requested frames, not elapsed audience time. Statements about Zeon's release, responsibility,
Volcanon's temper and a prospective visit remain attributed speech.

## Relative Positions Depend on Earlier Scenes and Supplied State

**Confirmed shared actors:** the audience never commands hidden guards 150–156, so it has no
hidden-guard dependency requiring the address first. Both programs do command 147/148 using
relative movement. Successful motion from normal fixed placements would leave the address's
birdwoman at `(37,50)`, Down, with the king still `(36,49)`. Under the same assumptions, the audience
leaves the king at `(36,50)` and birdwoman at `(36,51)`, Down.

If the audience runs first, the address's later Left1/Right1 uses that changed birdwoman position;
it does not initialize her back to `(37,50)`. Neither caller repairs prior positions or requires the
other scene to normalize them. An existing Luke follower placement, HP-skipped commands, missing
mappings, unfinished actions or supplied populations likewise prevents an unconditional account
of absolute staging. Independent flags permit such caller order without proving natural access.

## Movement, HP, Following and Service Boundaries

**Confirmed units and admission:** `setPos` and `setFacing` read the actual byte actor and test
that ally's word HP, skipping only at zero; HP256 is nonzero. Thus Peter/Bowie staging and Luke's
facing have their own HP predicates. NPC 147/148 bypass the ally-HP check. Skipping a position or
facing command suppresses neither text nor action/jump/nod requests. Admitted Peter positioning
with F67 absent can affect physical 0 through a zero mapping.

Position writes set current/destination coordinates and facing. One map tile is 384 internal
1/16-pixel units, or 24 pixels. Relative movement multiplies signed tile deltas by 384 and adds
current position, subject to destination/obstruction services. Face-action counts are update/wait
operands, not distances. Packed speed 48/48, 16/16 and `eas_Init` 32/32 are distinct; `eas_Init` also
restores collision, obstruction, auto-facing/orientation and sprite properties.

Action installation has no general HP or mapping guard. Waited actions poll the chosen actor's
pointer for `eas_Idle`; nonwaited actions return after installation. Waiting guard 151 is not
waiting 150/152–156; waiting Bowie is not waiting Peter. Later installation can replace an unfinished
action. Jumps temporarily disable auto-facing and restore it after their motion/wait; nods run
animation/sprite/DMA/sleep services without an HP check. Hiding is update exclusion, not an
action-completion barrier, and scene end adds no global actor wait.

Camera destination commands clear tracking and wait for scrolling. Text requests use portrait/
speech services, wait for view scrolling, display and advance the cursor. Single-text requests
also close windows and sleep 10 frames. Debug skips prevent equating requests with visible output
or duration. The terminator conditionally waits for scrolling if a dialogue window remains and
clears scrolling speed; it does not itself restore control. Existing controlled handler observations
do not certify these composed scenes or their natural presentation.

The final follow command stores words `$0007,$0000,$0002`. Its alive helper probes the first word's
**high byte**, so admission tests **Bowie 0's word HP**, rather than Peter 7's. Bowie at zero skips
the six operand bytes. With Bowie alive and valid mappings, selector 2 installs Peter following
Bowie independently of Peter's HP. Its `(-24,0)` offsets are pixels: the follower consumer shifts
them by 4 into internal units and applies facing/distance/layout rules. This is not an immediate
fixed world position or an instruction for Bowie to follow Peter.

`AddFollower` copies/patches the action and replaces it before duplicate-list detection; absent
Peter can resolve main for both operands. There is no following-completion wait, F67 write, join,
active-party admission or force-AI change. No rebuild follows inside the audience. This installs
a current entity relation; later entry rebuilds from then-selected flags/lists and need not preserve
that relation or action.

## Returning from the Caller to Conditional Control

**Confirmed caller boundary:** F720 or F721 is written only after its respective script returns.
A stalled text, camera or action service does not establish the write. After the selected caller
returns, including an already-set-bit skip, the zone wrapper closes portrait/text windows, waits
one VInt and calls `WaitForEntityToStopMoving(0)`. This waits for physical 0's current X/Y to equal
its destinations; it tests neither `eas_Idle` nor all actors.

The shared exploration loop returns to `WaitForEvent`. Only pending zero retargets camera 0 and
calls `SetControlledEntityActScript(0)`; another pending request takes priority. Ordinary
`PLAYER_TYPE=0` selects `eas_ControlledCharacter`, while other types select their own action.
Completed caller/wrapper services and pending zero therefore establish conditional restored
control. They do not establish responsive input, settled Peter following or immediate shrine travel.
This is the guide's stopping boundary after the first audience.

## Evidence Review and Remaining Limits

This guide consumes research accepted in
[PR #786](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/786), alongside the predecessor
English guide from [PR #785](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/785).
Research retains the USA retail identity, pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`, 115 records, all 39 text keys and inline actions.
Its [reproduction boundary](../../research/bedoe-king-address-through-first-audience.md#caller-return-reproduction-and-stopping-boundary)
owns the direct source and retained comparisons. This prose slice reviewed accepted research and
bounded tracked-fixture selections; it did not repeat source investigation, generate/copy optional
payloads or acquire runtime evidence.

| Reviewed relation | Executable owner and contract | Adversarial boundary |
| --- | --- | --- |
| Independent callers | [sf2-map-events-static-v1, zone-programs](../../../tests/fixtures/h2/map-events/zone-programs.json), [map exploration](../contracts/map-exploration.md), [request consumption](../../research/map-event-request-consumption.md). | Selected programs retain `chkFlg`→branch→script→`setFlg`→return, independent 720/721 and one/two row references. Research owns exact zone rows and producer/consumer composition; matching coordinates alone do not prove walking. |
| Setup, init and cast | [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json) bodies 14/15, [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), [party/roster](../contracts/party-roster-state.md). | Summary inventories are not an independent decode of every row/list. Accepted research owns F722 replacement, actor joins and fresh mappings; body 14 separates F719 scene from F720 out-of-map handling. |
| Dialogue and staging | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [dialogue](../contracts/dialogue-system.md). | Public command-family facts do not contain this complete text transcript or establish scene reach. Research owns address 1641–1650/audience 1651–1679 and their 115 records; requests, inline actions and service completion remain distinct. |
| Hide, HP and follow | The same static fixture's placement/action/lifecycle/relationship families and [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json), [map exploration](../contracts/map-exploration.md). | Macro operand widths, lookup/HP helper calls and controlled follow seams remain narrower than a natural scene. Research supplies hide versus re-entry, update exclusion, high-byte admission, pixel offsets and coordinate-based main-stop composition. |

The [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[fixture schema](../../../schemas/h2-map-init-static-fixture.schema.json) retain complete delimited
bodies and provenance. The [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../src/sf2tool/h2/map_script_schema.py) retain command-family facts and
separate consumers. No Layer A correction was needed. Existing narrow reproduction routes include
`uv run sf2 h2 map-init`, `map-script-engine`, `map-setup`, `map-entities` and `map-events`; these are
routes, not fresh execution claims in this documentation slice.

**Unknown:** collision-free floor access and natural ordering; actual flags, word HP, mappings,
party/follower/AI state and inherited positions/actions; uninterrupted services, hidden/nonwaited
action lifetimes, pending completion and responsive input; visible camera/motion/portrait/speech
and timing; later follower reconstruction and continuity into Volcanon. These questions remain
with their evidence owners and do not authorize acquisition. The guide completes neither the
campaign nor the design epic.
