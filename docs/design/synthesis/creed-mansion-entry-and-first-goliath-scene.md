# Creed Mansion Entry and the First Goliath Obstruction

**Confirmed:** the Devil's Tail doorway requests a fresh Creed Mansion arrival. The first
Goliath scene belongs to a separate admitted zone event, not that arrival's initialization.
The complete scene stages an audience request, refusal, attempted entry, interception and an
apparent retreat; it changes two live layout blocks and installs temporary following.
**Inferred:** the intended second companion is Oddler. **Unknown:** natural reach, the actual
supplied cast/state, completed services, rendered performance, usable control and persistence.

This English guide consumes [accepted entry/scene research](../../research/creed-mansion-entry-and-first-goliath-scene.md)
and follows [Battle21 outcomes and Devil's Tail return](battle21-outcomes-and-devils-tail-return.md).
Its Standard agreement is [Issue #895](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/895)
and the [documentation roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
It explains the first local return/repeat, stopping before the separate shrinking event.
It establishes no complete mansion itinerary, later recruitment, Desktop Kingdom/Battle22,
renderer/terrain audit or remake capability.

## The Doorway and Arrival Are Separate From the Scene

**Confirmed:** the preceding victory return is full-map 67 `(26,23)`, Down. The independent
Map67 doorway at `(26,22)` submits warpNoScroll to Map33 `(15,30)`, Up. Adjacency does not
prove walking, passability or activation. The warp handler consumes the shared request,
idles main and unwinds to MainLoop; no-scroll parameter1 is zero. Ordinary admission calls
SwitchMap **then CheckBattle**, unlike the alternate post-battle return. Neither the complete
map-switch table nor battle-coordinate table has a Map33 row. The battle scan returns d7=-1
and preserves this tuple: supplied unlock flags alone do not turn this arrival into Battle22.

Fresh exploration clears the pending map-event type, reduces STEP_COUNTER by 20000 with a
negative clamp, heals eligible allies and begins fade/resource work. It writes CURRENT_MAP=33
and CURRENT_BATTLE=255, selects the setup, initializes actors at the requested arrival and
loads sprites/map. It clears temporary flags 256–383, sets F80, installs base VInts and calls
selected init. Palette comparison governs subsequent music/fade calls. These are ordered
source consumers, conditional on returning services; they do not prove an executed arrival.

CURRENT_BATTLE=255 selects the **full-map** layout domain. Stale Battle21 crop offsets do not
shift the mansion arrival or scene operands. This explicit Map33 request also takes fresh
population, rather than the map 255 preserve-population branch. At WaitForEvent, pending work
wins; only pending-zero installs physical main 0 as camera target and the current-mode controller.
The [map contract](../contracts/map-exploration.md#load-and-selection-order) owns generic
load/layout rules; [request consumption](../../research/map-event-request-consumption.md) owns
the shared-slot lifecycle. Neither owner guarantees this particular natural route.

## Which Setup and Actors the Scene Actually Receives

**Confirmed:** the selected first visit has F523/F783/F784/F786/F22 clear and initially F774
clear. GetCurrentMapSetup starts with the default and replaces it for **every** set row until
the terminator: the last set row wins.

| Source order | Selected pointer |
| --- | --- |
| Default | ms_map33 |
| F523 | ms_map33_flag523 |
| F783 | ms_map33 |
| F784 | ms_map33_flag784 |
| F786 | ms_map33_flag786 |
| F22, last | ms_map33 |

All five clear selects ms_map33, but pointer identity does not prove first-visit state:
F783 or F22 can restore that same pointer after an earlier alternate. F774/F785 do not
select pointers. Each accessor resolves setup again; no promise freezes the flags between calls.
The pointer's six fields are Entities, EntityEvents, ZoneEvents, AreaDescriptions, Section5
and InitFunction. [Setup selection](../../research/map-event-flag-route-selection.md) owns the
shared selector; accepted entry research supplies the complete Map33 joins.

The baseline entity list declares six fixed rows, all parked at `(63,63)` with eas_Init:

| Row / logical identity | Mapsprite source | Initial facing |
| --- | --- | --- |
| 0 / NPC 128 | WORKER=202; the scene identifies Goliath | Up |
| 1 / NPC 129 | CREED=212 | Left |
| 2 / Eric 14 | Ally sprite lookup | Down |
| 3 / Randolf 16 | Ally sprite lookup | Up |
| 4 / Tyrin 17 | Ally sprite lookup | Down |
| 5 / Karna 15 | Ally sprite lookup | Up |

Parked declarations do not prove visible room occupants. Initialization clears entities/lookups,
allocates selected followers from physical 1, then fixed rows. Table coordinates are masked
with `$3F` and scaled by MAP_TILE_SIZE=384. Ordinary NPCs append physical indices in the lookup
region beginning at offset 32; logical 128/129 resolve through that table, not literal physical
slots. A fixed ally row with an existing nonzero ally lookup is skipped completely. These
rows have no joined/active membership gate. There is no fixed Peter row or newEntity fallback.
Bowie is declared at physical 0, `(15,30)`, Up, eas_Idle.

Map33 is **not overworld**. F65 contributes no Caravan/A prefix here. Ordinary follower flags
still select their rows. With only F67/F68 selected among those rows, Peter 7 is physical 1 with
eas_Follower1 and B 31, using the Oddler sprite, is physical 2 with eas_Follower2. NPC 128/129 then
map to physical 3/4 and Eric/Randolf/Tyrin/Karna to 5/6/7/8. Followers initially receive the supplied
arrival coordinates/facing. This is a conditional allocation example, not a guaranteed cast.

Other selected follower rows change physical allocation. F74/F68/F70 can declare B repeatedly
with Astral/Oddler/Astral sprites; initialization does not deduplicate those rows. The last
writes the logical B lookup while earlier physical actors remain. Missing Peter/B lookups are
zero and can alias Bowie 0. Membership does not supply missing follower flags. Class and HP
also affect ally sprites: zero HP outside battle selects the blue flame, and entry healing
does not revive every dead ally. Fixed declarations, force membership, selected followers
and current action relations remain distinct [roster boundaries](../contracts/party-roster-state.md).

PLAYER_TYPE is unchanged by this entry/init/scene; the coherent predecessor supplies walking 0.
F64 plus RAFT_MAP=33 independently permits a saved raft at physical 31, masking saved coordinates
with `$7F`; this does not board or relocate it. Supplied mode 2 makes main the raft sprite and
followers stand before raft declaration. Controller selection is mode 0 ControlledCharacter,
mode 1 Caravan control via source-named eas_Raft, otherwise raft control via eas_Caravan; arbitrary
mode is not validated here. Dialogue does not select transport state.

The complete baseline init has separate ordered guards:

| Guard | Immediate effect |
| --- | --- |
| F523 set, F783 clear | Later restoration cs_5B016, then F783 and EGRESS_MAP=Hassan; body excluded. |
| F22 set | cs_5ABA8 places 128 `(14,14)`, Up, bypassing following placement alternatives. |
| Otherwise F784 set | Skips intervening placements. |
| Otherwise F783 set | cs_5ABCC places 128 `(17,10)`, Down. |
| Otherwise F774 set | cs_5ABE6 places 128 `(17,12)`, Down. |
| Tail, F784 set | Positions each not-joined Eric/Randolf/Tyrin/Karna; later recruitment context excluded. |

With the five selected later-story flags and F774 clear, init returns without these scripts or
writes. It neither runs cs_5AC58 nor creates its zone request. A baseline F774-set reentry only
places 128 `(17,12)`, Down after rebuilding population. It replays neither conversation nor door copies.

## An Admitted Zone Starts the Audience Request

**Confirmed:** the zone producer recognizes layout type `$1400`, writes event type 6 and derives
word coordinates from main destination plus movement offsets divided by 384. The row alone
proves neither natural input/collision access nor admission. ProcessMapEvent clears pending type
before dispatch; type 6 installs main eas_Init and supplies the coordinate words to the current
setup's zone wrapper. [The shared zone caller](../contracts/dialogue-system.md#source-zone-caller-through-return)
owns its service and return predicates.

The first matching `(15,21)` row selects Map33_ZoneEvent0. Arrival was `(15,30)`, not this zone.
With F774 clear, Zone0 calls **all** of cs_5AC58. Only after it returns does Zone0 set F774 and
branch directly to return; it does not check the later F785 branch on that first invocation.

The scene has 129 commands, indexed 0–128, with 17 complete inline action bodies and six custom
bodies. The accepted source owner identifies scripts.asm lines 4–191, address 0x5AC58, ending at
csc_end before cs_5AF36/0x5AF36. Below, P is logical Peter 7, B is FOLLOWER_B 31, and 128/129 are
logical NPCs. Every position, relative move, speed and wait is a source operand, not measured
motion/time. The content sections cover the complete program, including its concurrent work.

### Dismissal, Door Work and Approach (Commands 0–39)

NPC 129 dismisses his servant so he can speak with the fairy; 128 acknowledges him. The camera
moves to `(12,11)`, waits 30, and stepEvent `(17,11)` changes a live block. NPC 128 is placed
`(17,11)`, Down, moves Down 3 with a wait, then after wait 5 starts eas_2xRightLeft without an
idle wait. Wait 80 precedes his humming/collection-check line; camera follows 128, his Left 5
move is waited, then he faces Up. That wait 80 does not finish the whole named action before
its replacement by movement.

Peter and B receive waited eas_Init actions. P/Bowie/B are set at `(15,21)/(15,22)/(15,23)`, Up.
A second stepEvent at `(15,20)`, camera destination `(8,12)` and wait 30 precede speakerless
narration that Goliath unlocks the door. Three approaches are installed: P Up 4/Left 3/Up 1,
Bowie Up 5/Left 3, B Up 6/Left 2. **Only B is waited**. Bowie/B face Up; Peter requests attention.
NPC 128 shivers, responds, stops animation, waits 30, turns Right, waits 60, turns Down, waits 30,
restarts animation and hesitantly responds again. Animation stop is not a motion lock.

### Appointment, Refusal and the Companions' Plan (Commands 40–74)

Peter's waited Up 1 precedes his request to see Creed. Goliath asks whether they mean his master,
then asks about an appointment; Peter does not understand. Peter's Down 1 is waited, Bowie
headshakes, then Peter's Up 1 is waited. Peter admits they have no appointment; Goliath says
Creed is busy and tells them to return later. Peter's Down 1 is waited before he reports refusal.
Bowie faces Right and B Left. B reacts and urges forceful entry, with runtime
leader/Peter-name substitutions. Peter's waited Right 1 and turn Down precede approval of
what he calls Oddler's bold idea; Bowie/B face Up.

NPC 128 faces Up, waits 10 and shivers. Peter's Left 1 is waited; Bowie/P/B face Up. NPC 128 shivers
again, wait 10 precedes his disapproval, and wait 20 precedes Peter's signal. **Inferred:** explicit
Oddler address plus the F68 sprite support intended B attribution. They do not prove F68 was
supplied or exclude another B lookup. Dialogue establishes the characters' claims and proposed
action, not reliable lore, successful forceful entry or a later itinerary.

### Attempted Entry and Interception (Commands 75–112)

The camera follows Bowie. Bowie/P eas_Jump actions are installed, then B eas_Jump is waited.
Three custom bodies are installed with waits, on Bowie/P/B in order: ac_setSpeed 40,40; ac_jump eas_Idle;
ac_end. Movement Right 5/Up 3 is installed on P, B and Bowie without individual waits. After wait 10,
NPC 128 receives WIPE_OUT, is explicitly placed `(17,12)`, Down, then receives WIPE_IN; waitIdle
waits only for Bowie. The effect pair creates no actor and performs no warp; the source
choreography does not measure supernatural speed.

P/B eas_BumpUp are installed before Bowie's waited eas_BumpUp. Peter's question, NPC 128's shiver
and his rebuke/barred-entry/order-away lines follow. Waited eas_DeactivateAutoFacing runs on
Bowie/P/B. Three more complete waited custom bodies, same order, set speed 10,10 and jump idle.
Bowie/P Down 1 are installed before B's waited Down 1. Waited eas_Init on Bowie/P/B resets actions,
then wait 25 and wait 5 precede Peter's unawaited eas_2xRightLeft and wait 120.

All 17 inline bodies end with endActions: the two early NPC moves; three approaches; four P
Up/Down moves in the appointment exchange; P Right 1/Left 1; three Right 5/Up 3 attempts; and three
Down 1 withdrawals. The six custom bodies above end with ac_end (`$8080`). Their 40/40 and 10/10
are packed speed operands, not pixels per frame. Shared [movement/action timing](../contracts/map-exploration.md#entity-movement-and-action-timing)
is not proof that every concurrent move reached its destination.

### The Apparent Retreat and Temporary Following (Commands 113–128)

Peter suggests the door on their left may lead to Creed, turns Down, then proposes avoiding
the servant. Bowie turns Right and B Left for B's encouragement. Bowie nods.
Peter turns Up and pretends to concede, saying they will return later and leave. B/Bowie face
Up and B agrees. This is a proposed alternative and apparent retreat, not a completed route.

The final two commands before csc_end install P→Bowie then B→P, place 2, with offset `(-24,0)`.
There is no scene warp, flag write, recruitment, choice or call into the next program. Crucially,
csc2C tests the **high byte of each follower word** for its ally-alive guard: 7 and 31 both supply 0,
so both test Bowie's HP. If admitted, actual lookups determine physical follower/target identities.
AddFollower copies the 42-byte eas_Follower1 template with target/offset into the walking buffer and
appends the physical follower to EXPLORATION_ENTITIES only if absent. It changes no joined/active
or persistent follower flag. Executed templates reset speed 32/32 and movement flags, then follow.
Neither call waits for settling or defines an absolute tile. Aliases can self-follow or overwrite
another relation; reentry discards these actions and rebuilds from follower flags.

## All Thirty Texts and Their Consumers

**Confirmed:** textCursor 1804 selects the consecutive gamescript IDs 1804–1833 (`070C`–`0729`).
The accepted owner checked every literal/comment including spacing/controls. This table paraphrases
content and preserves ordered tokens; S=nextSingleText, T=nextText. Speakers use modifier 0 except 1807.

| ID | Form / speaker | Content | Ordered controls |
| --- | --- | --- | --- |
| 1804 | S / 129 | Dismisses servant to speak with fairy | `{N}{W1}` |
| 1805 | S / 128 | Acknowledges master | `{W1}` |
| 1806 | S / 128 | Hums, remembers checking collections | `{N}{N}{W1}` |
| 1807 | S / ENTITY_NONE, `$FF` | Narration: Goliath unlocks door | `{N}{W1}` |
| 1808 | S / P | Requests attention | `{W1}` |
| 1809 | S / 128 | Startled exclamation | `{W1}` |
| 1810 | S / 128 | Hesitant response | `{W1}` |
| 1811 | S / P | Requests audience with Creed | `{W1}` |
| 1812 | T / 128 | Asks whether they mean his master | `{W2}` |
| 1813 | S / 128 | Asks about appointment | `{W1}` |
| 1814 | S / P | Does not understand | `{W1}` |
| 1815 | S / P | Admits no appointment | `{W1}` |
| 1816 | T / 128 | Says Creed is busy | `{W2}` |
| 1817 | S / 128 | Tells them to return later | `{W1}` |
| 1818 | S / P | Reports refusal | `{W1}` |
| 1819 | T / B | Reacts to refusal | `{W2}` |
| 1820 | S / B | Urges leader and Peter to force entry | `{LEADER}{NAME;7}{N}{W1}` |
| 1821 | T / P | Calls Oddler bold | `{W2}` |
| 1822 | S / P | Approves idea | `{W1}` |
| 1823 | S / 128 | Disapproves | `{W1}` |
| 1824 | S / P | Signals attempt | `{W1}` |
| 1825 | S / P | Questions sudden interception | `{W1}` |
| 1826 | T / 128 | Rebukes group and bars entry | `{N}{W2}` |
| 1827 | S / 128 | Repeats refusal, orders them away | `{N}{W1}` |
| 1828 | T / P | Suggests left door may reach Creed | `{LEADER}{N}{N}{W2}` |
| 1829 | S / P | Proposes avoiding servant via that door | `{N}{W1}` |
| 1830 | S / B | Urges leader onward | `{LEADER}{N}{W1}` |
| 1831 | T / P | Pretends to concede | `{W2}` |
| 1832 | S / P | Says they will return later and leave | `{N}{W1}` |
| 1833 | S / B | Agrees with apparent retreat | `{W1}` |

The [dialogue contract](../contracts/dialogue-system.md#confirmed-static-contract) owns shared
cursor, display, close and wait rules. Each selected display waits for camera scrolling, consumes
DisplayText and increments the cursor. S closes portrait/text and sleeps 10; T retains continuation.
`{N}` is a line break; W1/W2 distinguish text waits/continuations, not a choice. Name controls resolve
runtime names. `$FF,ENTITY_NONE` encodes `$FFFF`, bypassing speaker/portrait lookup and selecting
speech SFX 0. Other speakers resolve the actual mapped sprite through dialogue properties; modifier 0
sets neither right-side nor mirrored portrait bits. Existing/missing portrait state still matters.
These facts guarantee no rendered portrait or sound. Debug/P2-Start can latch SKIP_CUTSCENE_TEXT;
nominal thirty-text/csWait order assumes that skip mode is not latched. B's identity remains supplied.

## Door Changes and Action Completion Have Their Own Boundaries

**Confirmed:** csc47 scales step operands by 384 and calls OpenDoor, which returns immediately
unless CURRENT_BATTLE=255. Here the selected records are:

| Command | Trigger / destination | Source | Size |
| --- | --- | --- | --- |
| 5 | `(17,11)` | `(26,0)` | 1x1 |
| 20 | `(15,20)` | `(25,0)` | 1x1 |

Nonnegative source Y selects one-word copies in FF0000's 64-word-stride live layout. OpenDoor plays
SFX_DOOR_OPEN and sets the plane-update bit for MAP_AREA_LAYER_TYPE. It writes no F774, save,
warp or persistent event flag. Map33's complete flag-event table has only F22's large replacement,
not F774 replay of these copies. Baseline fresh reentry does not restore them from F774 alone.
Narrated unlocking and block mutation prove neither rendered opening nor collision/passability.

The accepted [scene/action consumer review](../../research/creed-mansion-entry-and-first-goliath-scene.md#block-copies-actions-and-presentation-boundaries)
owns exact macros/handlers. Camera destinations disable following and wait for scroll; camera-actor
commands resolve physical targets. setPos changes both position and destination; facing/placement
skip zero-HP allies and do not repair missing lookups. Action installers lack that placement HP skip.
Their wait variants and waitIdle poll only the resolved actor for eas_Idle, without timeout/recovery.
Relative moves compile into buffers, scale signed deltas by 384 and check other-entity destination
conflict; installing them is not proof of arrival.

Selected eas_Jump/eas_BumpUp disable autofacing, move `(0,-1)`, wait 5, move `(0,1)`, wait 5, move
`(0,0)`, restore autofacing and jump idle. eas_Init resets speed 32/32, acceleration factors 0/0,
acceleration/deceleration off, obstructability on,
entity/map uncollidable off, autofacing on, orientation Up, immersion off, size 24 and sprite,
then idle; it is no extra position set.
eas_2xRightLeft changes facing/sprite Right/Left/Right/Left with wait 30 each. Shiver uses three
wait 5/wait 5 iterations and restores size/animation/flag state; nod/headshake use synchronous
sprite/DMA/wait sequences and restore animation. WIPE_OUT 2/WIPE_IN 3 select rows `(1,0,1,0)` and
`(22,0,-1,0)`, each 23 crop/DMA iterations with two VInt waits each. None proves visible completion,
measured timing, actor creation, mosaic behavior or a general renderer contract.

## Return, Immediate Repeat and Reentry

**Confirmed:** csc_end conditionally waits for view scrolling when a dialogue window exists,
clears VIEW_SCROLLING_SPEED and restores caller registers. It does not set F774. Zone0 writes
F774 only afterward. The zone wrapper closes portrait/text, waits one VInt and waits for
**physical 0's current X/Y to equal destination X/Y**; it does not wait for all follower scripts
to be idle. WaitForEvent handles new pending work before normal camera/controller installation.
Earlier mutations are not rolled back if later action, text/input, scroll, DMA/VInt or movement
services stall. F774 can already be set when a later wrapper wait stalls.

| Supplied state | Local consequence |
| --- | --- |
| F774 clear, selected later flags clear | Admitted Zone0 runs whole first scene, then writes F774; no first-branch F785 check. |
| F774 set, F785 clear | Zone0 returns without scene/move/text/flag change; wrapper closure/waits still apply. |
| F774 set, F785 set, F786 clear | Later cs_5B466, then F786/F69 set and F68 clear; identified, body excluded. |
| F774 set, F786 set | Later departure bypassed; setup precedence may already select another population/init. |
| Fresh baseline reentry, F774 set | Population rebuilt, 128 placed `(17,12)` Down; no automatic dialogue/door-copy replay. |
| Missing/overridden follower lookup, changed HP/class | Aliasing, changed sprite or skipped placement/facing/follows; no guaranteed actors or completion. |
| Supplied transport state, pending event or stalled service | Actual mode/raft/pending/service consumers control continuation; prose supplies no usable-control result. |

Zone `(12,14)` is separate: F783 clear calls cs_5AF36, set returns. Its shrinking/collection body
is excluded. The first scene's end does not fall through into it; the companions' plan and following
relations guarantee no later activation. F774/later-story/follower flags survive temporary 256–383
reset, but surviving RAM does not establish save/load durability. Stop at this conditional local
return/repeat, before shrinking, later departure/restoration/recruitment or Battle22.

## Evidence Review and Preserved Limits

**Confirmed provenance:** the consumed research is accepted in main
`887c6f6772c0c5f785a3a3684a14a7b0cfd19d7e`, pinned to
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The [research owner](../../research/creed-mansion-entry-and-first-goliath-scene.md#agreement-and-provenance)
owns exact selected paths, addresses, commands/bodies/literal texts and consumer joins.
This synthesis reviews accepted evidence, not raw-source reinvestigation or a new playthrough.

| Reviewed surface | Support and limit |
| --- | --- |
| Complete accepted entry research and predecessor guide | Doorway/admission, full-map domain, last-set setup, all baseline actor/init/zone joins, 129 commands/17 inline/six custom bodies/thirty texts, both live copies, actions and return/repeat. Selected direct-source joins own local completeness. |
| `sf2-map-init-static-v1`, [fixture](../../../tests/fixtures/h2/map-init-static-v1.json), expected dispatcher/entity/init facts | Shared setup/population/init context. Aggregate facts alone do not prove all Map33 content or natural arrival. |
| `sf2-map-setup-static-v1`, [fixture](../../../tests/fixtures/h2/map-setup-static-v1.json) | Setup selector/accessor facts; pointer identity is not first-visit state. |
| `sf2-map-script-engine-static-v1`, [fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), expected programSummary and selected command-family facts | Public ordered family references for cs_5AC58 and interpreter consumers. Complete command bodies/literals remain accepted retained/direct-source joins, not a public aggregate payload guarantee. |
| `sf2-map-events-static-v1`, [zone-programs partition](../../../tests/fixtures/h2/map-events/zone-programs.json) | Selected Zone0 branch/script/flag operations; callback differs from wrapper/service completion. |
| Dialogue, map and roster contracts linked above | Shared services/state boundaries. Earlier H3 seams and accepted remake bindings prove no original mansion runtime or composed scene success. |

Existing schema/provenance validation uses [map-init schema](../../../schemas/h2-map-init-static-fixture.schema.json),
[setup schema](../../../schemas/h2-map-setup-static-fixture.schema.json) and
[script schema](../../../schemas/h2/map-script-engine-fixture.schema.json) through
[jsonio.validate_json](../../../src/sf2tool/jsonio.py). The zone partition uses the existing
[shard schema](../../../schemas/h2/map-events-fixture-shard.schema.json) and
[zone section schema](../../../schemas/h2/map-events-zone-programs.schema.json); all 14 Zone0 operations
retain the selected flag/script/branch order. Reviewed verifiers are
[map_init.py](../../../src/sf2tool/h2/map_init.py), [map_setup.py](../../../src/sf2tool/h2/map_setup.py),
[map_script_engine.py](../../../src/sf2tool/h2/map_script_engine.py) and
[map_events.py](../../../src/sf2tool/h2/map_events.py). Their shape/provenance/semantic gates do not
reproduce every prose claim. The [research reproduction route](../../research/creed-mansion-entry-and-first-goliath-scene.md#reproduction-and-review-boundary)
owns retained equality and selected source checks. No payload, comparison or observation is regenerated here.

Preserve [#892's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/892#issuecomment-6173418824)
and [PR894 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/894#issuecomment-6173456022),
including original/rebased head/tree/base and both CI receipts, unchanged 18,285,979-byte pair/0.081-second
comparison, complete selected joins and inspection failures. Preserve
[#891's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/891#issuecomment-6173346871)
and [PR893 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/893#issuecomment-6173367158),
including predecessor failures and the resolved lifecycle-summary P2 with distinct CI receipts.
Handoffs locate checks; tracked owners and accepted Git objects establish facts. This slice's direct
checks, configured committed planner and actual final CI belong to Issue #895.

**Unknown:** natural route/access/itinerary; supplied flags/party/HP/class/lookups, geometry/mode/raft/
pending state; completed services, actual scene/portrait/sound, usable control and storage persistence.
Changed-state rows are static discriminators, not executed scenarios or an automatic runtime queue.
This guide completes neither the campaign nor [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
