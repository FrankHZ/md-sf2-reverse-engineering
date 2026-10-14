# Evil Spirit Shrine Entry and Consultation Admission

The preceding guide follows Creed and the Fairy's requested departure from the
mansion's main room to consult the Evil Spirit.
**Inferred:** the shrine's prepared cast belongs to that anticipated consultation.
**Confirmed:** the mansion has a separate exit into Evil Spirit Shrine, two local
transfers connect its declared regions, and a later zone request can admit the
scene. Entering the room does not itself start that consultation.

This guide continues [Creed's restoration and main-room departure](creed-restoration-and-basement-departure.md)
and consumes only the accepted [entry/admission research](../../research/evil-spirit-shrine-entry-and-scene-admission.md)
from #945/PR948. It stops at the call to cs_5B6C0. Consultation dialogue and effects,
recruitment, Oddler's departure and postscene control remain later boundaries.
**Unknown:** natural travel, actual supplied state, completed services, presentation
and persistence. This is an implementation-neutral Layer B explanation, not a
remake requirement or a completed campaign route.

## From the Mansion into the Shrine

**Confirmed:** Map33's exit at `(9,5)` requests Evil Spirit Shrine34 at `(26,28)`,
facing Left. The mansion's other exit, wildcard X with Y31, requests Devils Tail67
at `(26,23)`, Up. Neither record nor its immediate matcher checks F783, F784, F523
or completed restoration. The predecessor's restoration flag and invitation do
not become an extra shrine-entry prerequisite.

These are conditional destinations rather than a demonstrated walk. Ordinary
walking first derives a candidate position from current position and movement
input. Bounds, slopes and entity obstruction affect that path; the actual working
layout must classify the candidate as a warp block (`$1000` under mask `$3C00`)
before the warp matcher is called. The matcher scans in order, treats negative
trigger bytes as wildcards and otherwise compares tile-coordinate low bytes.
A coordinate in a table alone proves neither passable geometry nor a produced event.

A match writes a pending warp request. WaitForEvent prioritizes pending events;
ProcessMapEvent clears the type before dispatch. The no-scroll consumer returns
the tuple to MainLoop, where SwitchMap has no Map34 substitution and CheckBattle
has no Map34 battle row, so this selected route admits exploration. An intervening
writer or an unconsumed request remains a separate concern.

Explicit destination bytes replace only the low bytes of coordinates derived
from physical0's prior position. Reading the result as exactly `(26,28)` assumes
zero inherited high bytes; the destination does not repair arbitrary supplied
high bytes. Full-map exploration loading also means these coordinates are not
relative to Battle23's cropped battlefield. The accepted research owns the
[complete route and immediate consumers](../../research/evil-spirit-shrine-entry-and-scene-admission.md#mansion-exit-and-fresh-map34-admission).

## Moving Between the Shrine's Regions

**Confirmed:** the complete Map34 table contains three no-scroll warps:

| Candidate trigger | Requested destination | Population consequence |
| --- | --- | --- |
| `(26,28)` | Creed Mansion33, `(9,5)`, Right | Explicit Map33 admission rebuilds its flag-selected population and runs its selected init. |
| `(22,29)` | CURRENT255, `(24,2)`, Left | Retains Map34 and its existing residents. |
| `(24,2)` | CURRENT255, `(22,29)`, Right | Retains Map34 and its existing residents. |

The two declared main-layer areas are `(0,0)..(26,25)` and `(19,25)..(29,34)`.
**Inferred:** the reciprocal transfers connect the shrine's two declared regions.
They are not evidence of a walked stairway, automatic reverse warp or demonstrated
basement arrival. All three records retain the actual candidate/layout/request
and coordinate-high-byte premises above; none checks F783, F784 or F802.

CURRENT255 follows the retained-map path. It updates main's current/destination
position, facing and ally sprite, and can relocate existing followers in walking
mode. It does not redeclare residents, clear the fresh-map temporary flags or set
F80 through the fresh-entry branch. Common loading/init still runs, using Map34
and the currently selected setup. Caravan/raft modes have different follower and
raft handling; a local transfer does not normalize them to walking.

Likewise, requesting the mansion does not prove a completed return, a particular
postscene population or restored player control. The destination's setup and init
remain owned by the [Creed guide](creed-restoration-and-basement-departure.md).
The source tables and retained-path consumers are in the research's
[three-warp review](../../research/evil-spirit-shrine-entry-and-scene-admission.md#return-only-local-surfaces-and-the-three-warps).

## Who Is Prepared in a Fresh Room

**Confirmed:** Map34 has one default setup and one F784 override. F784 clear selects
21 resident rows; F784 set selects an empty resident list. Both select the same
entity events, zone events, descriptions, item events and return-only init.
F783, F523 and F802 do not select the shrine setup. Fresh explicit Map34 entry
rebuilds actors and clears temporary flags256–383; F783/F784/F802 survive that reset.

The baseline prepares Creed128 at `(7,10)` and Fairy129 at `(6,10)`, both Up.
It also allocates Evil Spirit130, Beastman131, Boy132 and sixteen distinct
EFFECT1 residents133–148 at `(63,63)`, Up. These are 21 separate resident records,
not a single effect with sixteen frames. The baseline file has no terminator of
its own: the immediately included F784 alternate's sole `$FFFF` word ends the
baseline scan and also makes the alternate empty from its first byte.

**Confirmed allocation detail:** the Evil Spirit's sprite252 takes a special path:
logical130 maps to physical47 with stored ENTNUM32. It consumes a lookup entry
without advancing the ordinary physical counter. Creed/Fairy, Beastman/Boy and
all sixteen EFFECT1 sprite63 rows use ordinary allocation. With N selected
ordinary followers, Creed/Fairy occupy N+1/N+2, Beastman/Boy N+3/N+4 and effects
N+5 through N+20. Under the research's F67/F68-only example, Peter and B/Oddler
occupy physical1/2, Creed/Fairy3/4, Beastman/Boy5/6 and effects7–22; Evil Spirit
remains47. Logical ID, lookup offset, physical slot and ENTNUM are different
identities here.

The five character rows install eas_Init; effect rows install eas_InitFixedSprite.
Both end in eas_Idle after setting movement/collision/sprite properties; the
fixed-sprite version additionally disables autofacing and motion. Their sprite-size
write targets shared state, not an independently proven persistent body scale.
**Unknown:** whether these asynchronous actions execute, settle or render in a
naturally reached room. `(63,63)` is still an allocation, not HideEntity, and the
EFFECT1 name proves no summoning, explosion, animation or visible staging.
The research retains [all row identities and complete action bodies](../../research/evil-spirit-shrine-entry-and-scene-admission.md#setup-pointers-and-all-resident-identities).

Main/Bowie is declared separately at physical0 with the supplied arrival tuple
and ally sprite. Followers depend on their own flags, class/HP and mapping state;
party membership does not substitute for following. Map34 has no overworld Caravan
prefix. A Fairy follower under F69 is logical158, separate from resident129.
Repeated B follower rows can allocate separately and overwrite the same lookup;
missing Peter/B or residents leave zero lookups that can alias physical0. No
local population check repairs those aliases before the consultation call.

An empty resident override therefore does not mean an empty map: main, selected
followers and possibly a raft remain independent. PLAYER_TYPE is not reset;
walking0 is the coherent predecessor premise, not a local enforcement. F64 with
saved RAFT_MAP34 can declare physical31, while raft mode2 has its own main-sprite,
follower and parked-raft behavior. Excessive supplied ordinary populations can
collide with physical31; the allocation formulas certify no arbitrary flag mix.
Ally sprites use actual class/HP, and fresh exploration healing is not general
revival. Fixed resident sprite declarations remain independent of ally HP.

## The Room Waits for a Separate Request

**Confirmed:** both setups' init is only rts. It assigns no scene, position, flag,
pending request or EGRESS_MAP. The local entity-event and item-event tables contain
only returning defaults; the description callback returns without a handled
result. Flag, step, roof, chest and other-item tables contain only their endings.
These are local absence claims: shared interaction/search wrappers can still run,
close windows and wait. They do not imply that all generic interaction is absent.

After a fresh load, exploration can reach its camera/controller tail if pending
state and services permit. That is conditional source flow, not observed usable
arrival. Neither `(26,28)` nor the two local warp destinations generates a scene
request merely by appearing in the warp table, and none has Y13.

**Confirmed:** ordinary movement can write a zone request when the actual candidate
layout block has type `$1400`. This candidate uses current position plus movement
offsets. The request's X/Y words instead come from destination position plus those
offsets, using signed word arithmetic and division by384. They describe the same
tile only under a settled-position premise. A later collision check can suppress
movement after the request was written. Producing type6 proves neither completed
movement nor that any setup row will match.

The Map34 zone table matches exactly wildcard X255 and request Y **low byte13**;
other Y values select its returning default. The X wildcard is exact `$FF`, unlike
the warp matcher's any-negative-byte rule. A nonzero Y high byte does not prevent
this low-byte match. The table does not establish a region-wide natural trigger
at every X: a qualifying request still needs its actual producer, unless supplied
directly at a controlled seam.

## Admission Is Independent of the Restoration Flag

**Confirmed:** consuming type6 clears the pending type and installs eas_Init on
physical0 before selecting the current zone table. This is an action-pointer
replacement, with no HP guard, coordinate snap or wait for execution; it does not
prove main is already stopped when the callback begins.

For the Y13 match, Map34_ZoneEvent0 checks F784. If set, it returns without the
scene call or either caller flag write. If clear, it calls cs_5B6C0 through the
script macro and MAPSCRIPT trap's ExecuteMapScript interface. F783, F523, F802,
restoration completion, residents, following, HP and EGRESS_MAP are not additional
callback predicates. **Inferred:** this admits the anticipated consultation;
**Unknown:** its contents and outcome at this boundary.

Only if that unanalyzed scene returns does the caller set F784 and then F802.
These are conditional ordered effects, not completed facts or an atomic pair.
Interruption before return can leave neither caller write; interruption between
writes can leave F784 set with F802 unchanged. Scene internals might have other
effects; this guide does not claim these are its only writes or prove persistence.
An externally supplied F784 set can suppress the scene without proving it ran.

If the callback returns, the zone wrapper closes portrait/text, waits one VInt
and waits for physical0's current coordinates to equal its destination. That wait
does not join every actor/action/service. Register restoration likewise depends
on the relevant calls returning. These boundaries establish no postscene player
control, recruitment or departure. The exact callback and trap/wrapper join belong
to the [accepted admission owner](../../research/evil-spirit-shrine-entry-and-scene-admission.md#actual-zone-request-and-scene-call-boundary).

## Changed State Changes What Can Be Claimed

These are **Confirmed static discriminators**, not executed save-state scenarios:

- A fresh explicit Map34 entry with F784 clear rebuilds all 21 residents. A repeat
  entry still rebuilds them if F784 remains clear, including after an interrupted
  attempt; neither entry automatically calls the scene.
- Fresh F784-set entry creates no residents and suppresses the Y13 scene call.
  Setting F784 after baseline creation changes selection/admission but does not
  retroactively remove residents during a CURRENT255 transfer.
- Clearing F784 after an empty load can admit the call while resident lookups
  remain zero. The callback does not create its missing cast.
- Changing F783/F523/F802 alone leaves the shrine's setup and callback polarity
  unchanged; Map33's earlier init/history can differ independently.
- Different follower flags, HP/class, mode or raft state can change allocations,
  aliases, sprites and control without changing the local F784 gate.
- A supplied type6/Y13 request can match without demonstrating natural layout
  reach. A different Y low byte returns through the default. Current/destination
  disagreement and intervening request writers retain their separate limits.
- An unreturned scene or unfinished wrapper service proves no completed caller
  flags, wrapper wait or usable control.

## Evidence Review and Remaining Limits

**Confirmed provenance:** accepted main `c6bbc1c11ea52d9a0828abf2280e9ff125bfaf91`
contains #945/PR948 and the accepted predecessor. The research pins
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Its [reproduction owner](../../research/evil-spirit-shrine-entry-and-scene-admission.md#changed-premise-checks-and-reproduction)
retains source paths, addresses, operands, immediate consumers and commands.
No concurrent consultation research is evidence for this guide.

Adversarial review covered the complete accepted prose, both complete warp tables,
all resident/allocation/action distinctions, return-only local surfaces and the
candidate/layout/destination/request join. Selected executable owners were:

| Public fixture / schema and consumer | Checked boundary |
| --- | --- |
| sf2-map-events-static-v1:routing-setup, tests/fixtures/h2/map-events/routing-setup.json | ms_map34_ZoneEvents: both exact rows, selectors, offsets, targets and source lines; map-events-routing-setup.schema.json and map_events_fixture.py retain shard identity/field composition. |
| sf2-map-events-static-v1:zone-programs, tests/fixtures/h2/map-events/zone-programs.json | Map34_ZoneEvent0: all six operations at `$5B69E..$5B6B4`, exact source lines/operands, branch polarity, cs_5B6C0 target, F784 then F802 and shared default rts; map-events-zone-programs.schema.json and target-program schema preserve structure. |
| sf2-map-init-static-v1, tests/fixtures/h2/map-init-static-v1.json | expected.routeJoins79/80, pointerTableJoins80/81 and primarySourceBodies50 join both Map34 setups to the same rts at `$5B6BE`; h2-map-init-static-fixture.schema.json and map_init.py compare the complete semantic contract and provenance. |
| sf2-map-script-engine-static-v1, tests/fixtures/h2/map-script-engine-static-v1.json | entityActionBridgeCommandFacts, entityPlacementCommandFacts and entityLifecyclePresentationCommandFacts distinguish action installation, positioning and explicit hiding; composed map-script schemas and map_script_engine.py preserve handler/operand boundaries. They do not prove the unreviewed scene body or its execution. |

Schemas are under schemas/ (map-event/map-script shards under schemas/h2/), consumers
under src/sf2tool/h2/. Both selected event shards and their expected sections, plus
the init/script fixtures, were directly schema-validated; exact zone rows, callback
operations and return-only init were checked against the accepted prose. The
map_events.py verifier compares complete semantic output/provenance, not just shape.
No contradiction was found in these selected joins. Lower-level allocation/layout
claims retain the accepted research's source review; they are not newly executed
or independently re-extracted here.

This checkout lacks the pinned upstream and retained private pair. The narrow
reproduction owners remain `uv run sf2 h2 map-init`, `uv run sf2 h2 map-events` and
`uv run sf2 h2 map-script-engine`, with the accepted retained-comparison recipe.
No regeneration, acquisition, runtime or new environment is required for this
prose slice. The [original #945 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849),
[rebased receipt](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178108485)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/948#issuecomment-6178118055)
preserve the 0.084s unchanged 18,285,979-byte pair comparison, source joins,
CI38634122032 and CI38634788342, and inspection/newline/index diagnostic failures.
Their linked #941/#937 corrected facts and failures remain retained, including
1892's text control and encoded followEntity high-byte/Bowie-HP reasoning.
These are accepted results, not fresh reruns; no completed failure was restarted.

Direct whole-prose/diff, evidence/link/anchor, English/private-boundary checks and
git diff --check apply, followed by the configured clean committed planner and
actual exact-head CI. No blanket local suite or translation check follows from
this documentation-only change. Chinese synchronization remains deferred.

**Unknown:** natural travel and active layout/collision, supplied flags/population,
class/HP/mappings/mode/raft, request timing, service/action completion, rendered
actors/effects/camera/text/audio, usable control and persistence. The consultation
body and all subsequent recruitment/departure consequences remain outside this
entry/admission guide. [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md) create no
automatic runtime queue or extension of remake acceptance.
