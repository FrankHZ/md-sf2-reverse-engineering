# South Parmecia, the Kraken Reveal and Battle16 Entry

The selected South Parmecia exit leads to the Kraken battlefield, where an unlocked fresh battle
can stage the raft stopping and a river monster emerging. Peter and the conditional second follower
react before the game rebuilds the party and enemies for Battle16. Arriving does not guarantee
admission, scene sprites do not establish the combat roster, and the start-wrapper return does not
establish a playable first turn.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/south-parmecia-through-kraken-battle16-entry.md) supports the route,
complete encounter and initialization below. **Inferred:** the staging depicts the Kraken stopping
the raft and emerging from the river. **Unknown:** natural traversal, actual supplied state,
completed services, original presentation, input readiness and persistence. Fnnn means flag nnn;
coordinates are map tiles unless otherwise stated, and row numbers are zero-based.

Read [raft boarding and South Parmecia entry](polca-raft-boarding-through-south-parmecia-entry.md)
for the preceding conditional Map69 control boundary. This guide selects Map69 row0 and stops after
Battle16's start wrapper returns, before enemy activation, region/spawn work, turn generation,
combat or outcomes. It imposes no Battle14/15 prerequisite or natural order of numbered battles,
and establishes no remake capability or continuous playability.

## Selecting the Battlefield

**Confirmed route:** Map69 row0 matches `(28,51)` and resolves the source symbol
`MAP_KRAKEN_BATTLEFIELD` to **Map49**. It requests a no-scroll warp to `(15,16)`, Up.
The other seven rows do not compete at this point; row0 also has first-match priority.
The [selected request owner](../../research/south-parmecia-through-kraken-battle16-entry.md#selected-request-and-conditional-battle-admission)
retains the symbol-to-number join and immediate consumers.

Committed raft mode2 installs source `eas_Caravan` / opcode8 despite the symbolic name.
After input, bounds and actor-collision checks, the proposed point's working-layout `$1000`
marker calls the warp matcher; the movement tail still follows. Ordinary walking can also call
that producer. A matching row writes numeric type1, its tuple and warp sound request without
checking raft ownership, membership or battle flags. Neither table nor controller proves
a natural water path to `(28,51)`.

The ordinary no-scroll consumer clears pending, requests/clears warp sound, idles physical0 and
unwinds exploration to `MainLoop/@Start(49,15,16,Up)`. It preserves `PLAYER_TYPE` and `RAFT_MAP/X/Y`;
the request neither places Bowie nor updates the saved raft tuple. Pending and parameters are
mutable slots, so surviving writes and service completion remain Unknown. A supplied nonzero warp
parameter1 takes scrolling exploration instead of this admission path.

The predecessor's scrolling Map72 exit reaches Map69 without a new battle scan; its no-scroll
exit can admit Map69 battles9/17 first. This guide starts from conditional Map69 control and does
not assume either earlier battle was avoided. Map49 has no flag-selected replacement.
Its sole battle-map row selects **Battle16**, `BATTLE_VERSUS_KRAKEN`, using F416 and wildcard triggers.
The `(0,0,30,30)` rectangle is an area output, not an entry-coordinate containment test.

| Supplied admission state | Consequence on this call |
| --- | --- |
| F416 clear, either F516 value | No Battle16 admission; completion alone cannot admit it. |
| F416 set, F516 clear | Admit Battle16. |
| F416 and F516 set | Clear F416, but still admit Battle16 on this call. A later call can differ. |

Luke's accepted recruitment scene supplies the F416 unlock. Mode, F64, following flags and
membership are not additional admission predicates. **F516 is the completion flag; F466 is the
later introduction/start flag.** Battle return's alternate MainLoop entry skips the scan, so a
return or saved entry cannot be substituted for this original no-scroll call.

## If the Battle Is Not Admitted

**Confirmed fallback:** ordinary exploration of Map49 selects **`ms_Void`**, the `$FFFF` sentinel,
because Map49 has no map-setup entry or dedicated setup directory. The init wrapper skips its
callback. There is no named empty Map49 init to execute.

The generic exploration lifecycle still clears pending, heals living/immortal allies, loads tiles
and map, rebuilds main and flag-selected followers, clears temporary flags256–383 and sets F80.
The sentinel supplies no fixed setup rows. Map49 uses the normal follower table rather than the
overworld table; its palette/music/fade and event-wait path can conditionally install control.

Preserved mode2 gives main its raft sprite, relocates listed followers to `(64,64)` with standing
actions and declares physical raft31 there. Other modes declare the separate raft only when F64
and the supplied saved raft map match49. These branches explain supplied states without proving
usable exploration or exits on this battle-only map. The
[fallback owner](../../research/south-parmecia-through-kraken-battle16-entry.md#no-admission-fallback-and-transport-boundary)
retains the population and transport details; further fallback travel is outside this guide.

## Fresh Entry and the Encounter Cast

**Confirmed caller order:** BattleLoop clears `PLAYER_TYPE=0` and sets F399 **before** testing F88.
This clears transport mode without a disembark action or saved-raft-tuple update. With F88 clear,
it resets elapsed time, stores Map49/Battle16 and invokes the before wrapper. F466 set skips the
introduction while continuing fresh initialization. F466 clear selects `bbcs_16`; the before
wrapper does not set F466.

The introduction loads Map49 with camera `(9,10)`, then calls exactly `csub_4B1CA`, which clears
PLAYER_TYPE and returns. Its location in the after-battle source does not execute that outcome
script. Scene population then clears physical state and lookups, rebuilds admitted followers and
loads the fixed rows. Its header places Bowie at `(15,14)`, Right. This differs from the warp
`(15,16)`, Up and camera `(9,10)`; the later combat placement is another boundary.

The eleven fixed rows begin at `(63,63)`, Down, with init actions: one head, two arms and eight
legs. They supply logical scene actors128–138. The head uses special physical47; ordinary limbs
use slots after whichever followers were actually declared. These are mapsprites and actor rows,
not eleven initialized enemies. Rebuilding discards the boarding population's hidden positions
and mappings. A separate raft needs F64 and RAFT_MAP=49; the earlier Map72 grant tuple does not
satisfy that check.

Peter is logical ally7. On this map, F67 admits his follower and F68 supplies FOLLOWER_B31 with
Oddler's sprite, subject to later enabled declarations overwriting the same lookup. Thus Oddler
is the **conditional** second speaker, not an unconditional script identity. FOLLOWER_B31 differs
from physical raft31 and logical FOLLOWER_D159/lookup63. Missing fresh lookups can resolve to
physical main0; commands do not validate a live actor.

Position/facing commands skip zero-HP allies0–29, including Peter. FOLLOWER_B31 and scene actors
bypass that ally guard; actions, waits and clones have no equivalent HP check. A dead or missing
Peter can skip placement yet receive an action through its resolved mapping. The later fresh-battle
heal cannot justify his initial scene HP or presence. No introduction command joins, removes or
revives a party member or changes following flags. The
[cast owner](../../research/south-parmecia-through-kraken-battle16-entry.md#fresh-caller-scene-population-and-identities)
retains physical-slot and mapping details.

## The Complete Reveal

**Confirmed encoded order:** `bbcs_16` has 78 top-level records and four separately inspected inline
bodies. The encounter has seven ordinary text sites, IDs2513–2519, with no choice or story branch.
The table paraphrases their content and identifies their speaker operands; Bowie has no text site.
The [complete program and dialogue owner](../../research/south-parmecia-through-kraken-battle16-entry.md#complete-pre-battle-program-and-dialogue)
retains exact record ranges, original text keys and source provenance.

Before the first line, Bowie receives a waited init action. The script saves the 3×3 working-layout
region `(13,4)` at `(0,0)`, replaces it from `(31,4)` and makes the two arms share one sprite slot
and the eight legs another. Waited Peter and FOLLOWER_B init actions follow. Peter is positioned
at `(15,15)`, Left; FOLLOWER_B at `(13,14)`, Down, before fade-in. After waits of 30 and 5, it
installs Bowie's Right/Left/Right/Left action, waits 5, installs Peter's, waits 5, installs
FOLLOWER_B's and waits 120. Those three installs are not individually waited. Peter then has a
waited Right1 move, followed by wait20 and all three facing Down.

| Text ID | Speaker | Content and surrounding progression |
| --- | --- | --- |
| 2513 | Peter | Notices the stopped raft. Bowie/Peter then face Left and FOLLOWER_B Right. |
| 2514 | FOLLOWER_B, conditionally Oddler | Questions the water's motion. Wait30, Peter faces Up, wait30, then two separately waited Peter jumps follow. |
| 2515 | Peter | Reacts to something ahead. Bowie and FOLLOWER_B face Up; wait20 and Peter's waited Up2 follow. |
| 2516 | Peter | Directs the leader's attention before the camera moves to the reveal. |
| 2517 | Peter | Identifies a river monster after the head and limbs have been staged. |
| 2518 | FOLLOWER_B, conditionally Oddler | Connects it to the report in Polca; this ordinary `nextText` leaves window closure to the next single-text site. |
| 2519 | FOLLOWER_B, conditionally Oddler | Warns the party. Wait60 and script termination follow. |

Between texts2516 and2517, the camera requests `(9,0)` and waits for scrolling. Rumble sound, the first
quake ramp, another rumble and wait30 precede the head's waited auto-facing-off and speed16/idle
bodies. Logical128 is positioned at `(14,5)`, Down, then requests a waited Up2. Only completed
movement would put it at `(14,3)`. The saved 3×3 block region is restored; arms129/130 are positioned
at `(12,4)` and `(16,4)`, Down. A third rumble and FX on129 follow. Legs131–138 are positioned,
all Down, at `(13,2),(15,2),(17,3),(11,4),(12,5),(13,5),(16,5),(14,6)`; FX on131 and the second
quake ramp precede the final three lines.

**Confirmed service limits:** `cloneEntity` copies only **ENTNUM**, the sprite-resource slot
selector. It does not copy positions, action pointers, mapsprite IDs, lookups, HP or membership.
The arm pair shares129's slot and the eight legs share131's; two FX commands address shared
resources. Synchronized visible limbs are **Inferred**, not observed actors being cloned.

Block operations copy nine working-layout words and request view-plane updates; they do not alter
the ROM or prove the depicted picture. The `$8002` quake requests amplitudes1,2 with Sleep40 after
each, and `$4002` requests1,0 the same way. Each rumble submits sound93 without an audio-completion
wait. `SLIDE_IN=5` requests 23 mapsprite/crop/DMA iterations with two VInt waits per iteration;
it changes sprite material rather than actor position. Fades, camera, text, sound and FX remain
required services whose original rendered/audio completion is Unknown.

Action waits poll the named resolved actor's pointer for idle. Fixed sleeps do not join all three
concurrent rocking actions. Position commands set position and destination without clearing
travel/velocity; init does not heal or declare a missing actor. Text resolves portrait/speech
properties from the mapped actor, waits scrolling and advances the cursor; single-text also closes
windows and sleeps10. Debug text skip can suppress displays, cursor increments and encoded waits
without skipping every action/effect. The seven IDs describe the ordinary non-skipped path.
The terminator's conditional scroll wait and speed clear are not an all-actor/service completion
receipt. The [immediate consumer owner](../../research/south-parmecia-through-kraken-battle16-entry.md#immediate-action-and-presentation-consumers)
provides detailed action and presentation boundaries.

## Rebuilding the Actual Battle

**Confirmed fresh initialization:** after the introduction returns or is skipped, BattleLoop
restores Map49/Battle16, clears region flags90–105, heals living/immortal allies, initializes ally
and enemy positions, clears AI memory and calls `LoadBattle(0)`. Healing restores living allies
and immortal Peter/Lemon; it does not revive every dead ally. Scene HP and mapping limits apply
before that heal.

BattleSpriteset16 supplies **12 ally placement rows and 11 enemy definitions**, with zero AI
regions and zero AI points. The enemies are one head, two arms and eight legs, all STARTING.
Their AI/item/behavior definitions still exist: zero region/point tables do not mean no AI.
No AI or combat execution is covered here.

All ally X positions first become -1 and `UpdateForce` rebuilds membership lists. Placement uses
identities from `BATTLE_PARTY_MEMBERS`, not scene followers or placement-row ordinal. Zero-HP
members consume a list/loop slot without consuming a placement row; living members take the next
row. Capacity overflow can remove remaining active members. Twelve rows therefore prove neither
twelve living allies nor Peter/FOLLOWER_B participation. Cached roster state, current HP and
active membership cannot be inferred from sprites.

The enemy initializer uses the selected eleven definitions to initialize stats and positions;
remaining slots follow hidden/cleared branches. It does not copy scene positions or clone links.
`LoadBattle` clears/rebuilds actors and lookups from placed combatants, selects the requested
combatant's view target, reloads map/terrain/mapsprites and requests music/fade-in. The first ally
placement row is `(15,15)`, conditional on the active/living list, independently of Bowie's scene
`(15,14)` and warp `(15,16)` inputs.

The start wrapper skips when F466 is already set; otherwise it **sets F466 before** executing
Battle16's shared `ms_Empty` terminator. There is no dedicated Battle16 start scene. This guide
stops at that wrapper's return into `@Start`, before `ActivateEnemies`, regions, spawning, turns,
combat and outcomes. An empty script is not proof of first-turn readiness. The
[initialization owner](../../research/south-parmecia-through-kraken-battle16-entry.md#battle-initialization-and-stopping-boundary)
retains exact pointer/header, placement and caller order.

## Repeat, Interruption and Saved Entry

**Confirmed distinctions:** F466 set suppresses the introduction on a fresh entry, while fresh
healing and roster/enemy rebuilding still occur. Interruption before the start wrapper can leave
F466 clear after some scene effects, allowing the introduction to be selected again. Once set,
F466 is not a receipt for every preceding action, text or presentation service. F516's completion
handling and F416's unlock handling remain separate from that introduction decision.

F88 set selects saved entry instead. BattleLoop has already cleared transport mode, but this
branch restores saved seconds, clears F88/AI memory, calls LoadBattle and branches to the
individual-turn loop. It bypasses before/start wrappers, fresh healing/placement and new turn-order
generation. It does not replace saved current map/battle with the incoming tuple or supply the
fresh branch's explicit combatant0 argument. Saved map/battle/combatant/turn state and caller
operands must be consistent; the warp tuple repairs none of them. Resumed combat, save consistency
and persistent raft state remain Unknown and outside the stopping boundary.

## Evidence Trace and Remaining Questions

**Confirmed provenance:** this guide consumes accepted #820/PR822 research from main, which binds
SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM.
Its [reproduction and coverage owner](../../research/south-parmecia-through-kraken-battle16-entry.md#reproduction-coverage-and-preserved-limits)
names exact source bodies, retained comparison recipe, fixtures and direct joins. This is no new
source investigation or runtime acquisition.

| Boundary | Contract and existing fixture trace | Coverage limit |
| --- | --- | --- |
| MainLoop/request/admission/population | [exploration flow](../contracts/exploration-control-flow.md), [entry routing](../contracts/map-entry-routing-state.md); `map-event-request-consumption-static-v1.json`, `battle-routing-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json` under `tests/fixtures/h2/` | Shared structural facts; the Map69→49→16 join and Map49 sentinel absence remain the research owner's direct review, not a composed route fixture. |
| Introduction/start and complete scene | [cutscene routing](../contracts/battle-cutscene-routing.md), [dialogue](../contracts/dialogue-system.md); [map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), [cutscene fixture](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json) | The fixture retains command facts, including eight bbcs_16 clone sites, rather than a full programCorpus. Research checked the retained payload's 78 records and separately inspected four inline bodies and actor rows. Aggregate comparisons are not natural-entry/service receipts. |
| Combatant rebuild and membership | [battle lifecycle](../contracts/battle-control-lifecycle.md), [roster state](../contracts/party-roster-state.md); [spriteset fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json), `battle-control-static-v1.json`, `battle-loop-static-v1.json`, `common-stats-static-v1.json` | The spriteset fixture explicitly has rowContentTracked=false; selected12/11/0/0 inputs are research's direct review. Scene/roster equality and active/living deployment are not inferred from matching counts. |

Existing owning H2 commands include `uv run sf2 h2 map-script-engine`, `battle-cutscenes`,
`battle-spriteset-data`, `battle-loop` and `common-stats`; the linked research recipe reuses retained
map-init/map-script payloads and cutscene facts without rewriting inputs. This prose slice checks
bounded tracked selections and traceability rather than rerunning payload generation or blanket
suites. Load the owning worktree's ignored private-input configuration in the same process before
any controlled reproduction.

The [PR822 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072)
preserves completed comparisons/CI, resolved bounded-inspection/path errors, corrected commentary
flag F466 and a policy-rejected combined handoff command. The
[PR818 corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108)
preserves its failed initial destination assumption and corrected join. Neither is ritual-replayed
or promoted to broader coverage here.

**Unknown:** natural travel from Map69 control; actual flags, mode, HP, membership, mappings,
lists and pending parameters; all-actor/action/service completion; original rendering/audio timing;
usable input; interruption/repeat history, saved-entry consistency and persistence. These remain
deferred under [ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). They do not authorize
activation/turn/combat/outcome investigation, acquisition, campaign completion or a new product decision.
