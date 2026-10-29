# Polca departure through Luke recruitment

Evidence date: 2026-10-13. **Confirmed:** Polca's selected south warp requests
Map72 `(43,47)`, Down. Ordered battle admission can interrupt before exploration.
If ordinary Map72 init runs, F716 set and F10 clear select `cs_4FF5A`, which creates
a Luke actor, stages the group, joins Luke, moves/hides his mapped actor, then sets
F416. Oddler following and raft presence are not init prerequisites. **Inferred:**
the attributed exchange presents Luke volunteering to travel and fight with Bowie.
**Unknown:** natural departure, actual supplied state and actor/service completion,
visible presentation, responsive control and save/load persistence.

## Agreement, provenance and method

Standard rigor under [Issue #812](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/812)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
map admission, actor allocation and recruitment commit at different boundaries.
This document is the sole tracked owner; no shared-path edits or stack. Consumed
accepted main is `3e396e854ad04e34caef57eda4934d1ee2c80cc0`. Dependencies are accepted
[raft grant](./battle13-outcomes-through-polca-raft-grant.md),
[Oddler choice/following](./polca-oddler-choice-and-following.md),
[Map72 battle interruption](./east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request),
[shared entry](./damaged-ship-plank-through-battle08.md#explicit-map14-entry-versus-stairs-and-repeat-entry),
[membership](./bedoe-to-volcanon-audience.md#peter-membership-capacity-ai-and-following),
and [request consumption](../../world/map-event-request-consumption.md).

Scope is the selected south exit, actual intervening battle predicates, Map72
population/init, complete Luke scene and its immediate consumers, through
conditional exploration control. Stop before boarding, further travel, random
zone calls, Battle14/16 admission, AI or combat. Neither numbered battles nor
narrative directions establish route order. No region inventory, floor trajectory,
runtime acquisition, new parser/fixture/comparator or engine change. The anticipated
new boundary is `newEntity`'s immediate allocation/sprite consumer; no inseparable
scope expansion is needed.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init and map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass. This is static
source evidence, not a new composed runtime observation. Fnnn denotes flag nnn;
warp/battle/body indices are zero-based, scene records below one-based. Dialogue
is paraphrased and retains speaker attribution.

The complete scene in `data/maps/entries/map72/mapsetups/s6_initfunction.asm`
is lines 31–61: **31 lines, 29 records** (24 commands, four sleeps, one terminator)
and one inline body. This confirms a smaller span than the approximate 37-line
planning estimate. Reuse the unchanged 18,285,979-byte retained pair in one process,
with linear selected reads/comparisons, estimated below 512 MiB peak and one minute
of comparisons, plus small prose/receipts. Existing comparisons took 0.080 seconds;
comparison plus all scene/text/caller checks took 0.082 seconds. No new exported corpus or
verification machinery. Reassess before materially expanding this boundary.

## Selected exit and interruption before entry

**Confirmed:** `data/maps/entries/map13/6-warp-events.asm` contains four ordered
no-scroll rows. The first two are same-map blacksmith doors, not departure:

| Row | Match `(X,Y)` | Requested map, position and facing |
| --- | --- | --- |
| 0 | `(9,19)` | `MAP_CURRENT`, `(37,8)`, Up |
| 1 | `(37,9)` | `MAP_CURRENT`, `(9,20)`, Down |
| 2 | `(255,31)` | `MAP_OVERWORLD_BEDOE_KINGDOM=72`, `(43,47)`, Down |
| 3 | `(31,255)` | `MAP_MOUNT_VOLCANO`, `(1,28)`, Right |

255 is a wildcard axis. At `(31,31)`, row2 wins over row3; the earlier doors do
not match that intersection. The selected row has no F716, F718, F68, F75, F64,
membership or prior-scene test. It does not prove a natural walk to Y31. In
particular, Polca's separate Y30 zone can select the accepted Oddler scene under
F716-set/F718-clear; zone selection requires its own producer and first-match
ordering. An admitted south-warp request is not proof that Y30 was crossed,
answered or bypassed. There is no inferred Oddler prerequisite.

`code/common/scripting/entity/entityscriptengine_2.asm`'s ordinary movement path
requires layout attribute `$1000` before calling
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`. The consumer scans
eight-byte rows, accepts negative coordinate bytes as wildcards, and writes pending
type1, the parameter tuple and warp SFX. `ProcessMapEvent` clears pending before
dispatch. `explorationfunctions_2.asm:ProcessMapEventType1_Warp`'s no-scroll branch
requests SFX, idles main, unwinds exploration, samples main's position and replaces
both coordinates with the nonnegative 43/47 bytes. It returns map/X/Y/facing to
`mainloop.asm:MainLoop/@Start`, which calls `SwitchMap` then `CheckBattle`.
Intervening writes and service completion remain the accepted mutable-slot boundary.

`data/maps/global/flagswitchedmaps.asm` has no Map72 replacement. The actual Map72
rows in `data/battles/global/battlemapcoords.asm`, in scan order, are:

| Battle row | Area output `(X,Y,width,height)` | Trigger | Unlock / completion |
| --- | --- | --- | --- |
| 8 | `(1,32,32,30)` | `(255,255)` | F408 / F508 |
| 24 | `(1,32,32,30)` | `(255,255)` | F424 / F524 |
| 26 | `(28,0,36,32)` | `(255,255)` | F426 / F526 |

`code/common/maps/getbattle.asm:CheckBattle` tests map, unlock and trigger in that
order. First unlocked matching row wins: F408 precedes F424, which precedes F426.
The rectangle is an output, not containment; all three can interrupt the selected
`(43,47)` request. A set completion flag clears that row's unlock but **still
selects that battle on this call**, without continuing to a later row. With all
three unlocks clear, ordinary Map72 exploration is selected. No Battle08 outcome
is reopened here, and no battle-return tuple is assumed. After any battle, the
separate `alt_MainLoopEntry` switches the returned map and enters exploration
without another immediate CheckBattle; it is not the original south-warp admission.

## Fresh population, raft declaration and init guard

**Confirmed:** Map72's `mapsetups.asm` entry has only its default setup.
`map72/mapsetups/pointertable.asm` points to `ms_map72_Entities`, whose complete
`s1_entities.asm` is `msEntitiesEnd`, and to `ms_map72_InitFunction` (`0x4FF2E`).
Empty fixed rows do not mean an empty scene population.

For an explicit map request, `ExplorationLoop` clears pending, adjusts the step
counter, heals living/immortal allies, fades out, sets Map72/not-in-battle, loads
tiles, initializes entities/mapsprites, clears temporary F256–383 and sets F80.
`mapfunctions.asm:InitializeMapEntities` clears entity data/mappings, constructs
flag followers from physical1 upward, consumes the empty fixed list, declares
main physical0 and calls `followersfunctions_2.asm:sub_44404`. It does not preserve
Polca's current NPC140 or its installed follower pointers. Map load and base VInts
then precede init; no player-control handoff intervenes.

Map72 is in `data/maps/global/overworldmaps.asm`, and is not follower-excluded.
`followersfunctions_1.asm:InitializeFollowerEntities` normally starts at
`table_Followers`; F65 set selects `table_OverworldFollowers`, adding the caravan
row before falling through the ordinary table. This prefix shifts physical slots
and selects the overworld action-pointer list. The flags, not membership/HP, admit
rows. Relevant declarations are F67→Peter7, F68→Oddler sprite under FOLLOWER_B31,
and F75→Luke10. Earlier F74 and later F70 also declare Astral under31; F68 overwrites
the earlier mapping when present, and F70 can overwrite it again. Unusual conflicting
flags are not normalized. No fixed Peter, Luke or NPC140 row fills an absent follower.

At the end of fresh initialization, ordinary non-raft player state plus **F64 and
CURRENT_MAP==RAFT_MAP** declares the separate raft. With the accepted grant tuple
Map72 `(43,48)`, it uses physical `ENTITY_RAFT=31`, mapping offset 63, Left,
`MAPSPRITE_RAFT`, `eas_Standing`, with coordinate bytes masked by `$7F` and scaled
by 384. This is distinct from **FOLLOWER_B lookup31**, which reads mapping offset 31.
The number 31 does not alias the boy and raft. Supplied raft-player state instead
changes main's sprite, stands followers and declares the separate raft at `(64,64)`.
F716 alone neither declares nor repairs a raft, and raft declaration precedes init.

A supplied `MAP_CURRENT` request follows the accepted alternative: CheckBattle
resolves the current map for its scan and preserves the sentinel when none matches.
Exploration updates main and calls `DeclareRaftEntity` instead of rebuilding actors
or clearing temporary flags; both entry branches still load/run init. That raft
consumer returns for raft player, otherwise relocates ordinary followers (except
caravan-player branch) and applies its F64/map gate; unlocked mismatch clears the
raft mapping/moves it off-map. Current mappings can therefore differ from the fresh
examples. The selected south row is explicit 72, not this no-rebuild alternative.

The entire ordinary init body (retained `primarySourceBodies` sourceOrder 78) is six
operations: check F716, branch-if-clear to return, check F10, branch-if-set to return,
script `cs_4FF5A`, return. Both branches target operation 5. It has **no F718, F68,
F75, F64, F416, HP, party-capacity or coordinate gate**. The unused suspend-scene
bytes between that return and `cs_4FF5A` are not executed by this ordinary path.

| Supplied flags | Init behavior |
| --- | --- |
| F716 clear, either F10 | Return; no recruitment or F416 repair. |
| F716 set, F10 set | Return; actor, follower, raft and F416 consistency is not checked. |
| F716 set, F10 clear | Run the complete scene, even without Oddler/raft/follower flags or with F416 already set. |

## New Luke actor and explicit staging

**Confirmed:** `sf2cutscenemacros.asm:newEntity` encodes opcode `$2B`, entity word 10,
X43/Y47/Down bytes and `NOTHING_BYTE=$FF` as the mapsprite operand.
`mapscriptengine_1.asm:csc2B_initializeNewEntity` reads those fields, supplies
`eas_Init`, and calls `entityfunctions_1.asm:InitializeNewEntity`.

For nonnegative entity 10, that helper calls `GetAllyMapsprite` **before allocation**,
replacing the supplied `$FF` sprite. It is not an invisible/no-sprite directive or
an instruction to preserve an existing Luke sprite. `getallymapsprite.asm` selects
blue flame for zero-word-HP allies outside battle, otherwise the class/table sprite;
Luke has no Rohde-specific exception. New entity creation does not heal him.

Allocation starts at zero, scans exactly 63 mapping bytes (offsets 0–62), compares
with signed-byte `bge`, and chooses maximum nonnegative mapped physical index+1.
Invalid `$FF` entries do not increase the maximum; raft mapping offset 63 is excluded.
This is not a first-free-slot search, an existing-Luke reuse check or an explicit
capacity guard. It overwrites lookup10 with the chosen index. `DeclareNewEntity`
writes position/destination scaled by 384, facing, sprite, layer0 and `eas_Init`,
clears travel/velocity/acceleration fields and initializes action/animation state.
The subsequent `UpdateEntityProperties` call preserves the selected sprite and
requests its facing/sprite refresh. No membership flag or exploration-list append
occurs. The scene's one-frame wait is not an explicit `eas_Init` completion join.

Examples below assume fresh entry, only the named follower flags, ordinary player
and normal mappings. The separate raft, if present, does not change these allocations:

| Follower flags | Before newEntity | New Luke and retained state |
| --- | --- | --- |
| F67/F68 only | Peter physical1; FOLLOWER_B physical2; no Luke | Luke physical3; exploration list remains1,2. |
| F67/F68/F75 | Peter1; FOLLOWER_B2; old Luke3 | New Luke4 replaces lookup10; old physical3 and its follower-list entry/action are not removed. |
| None | Main0; Peter/31 mappings remain zero | Luke physical1. Later absent-actor references can resolve main0. |

Adding only F65 to each example prefixes a caravan and shifts these declared follower/new
indices by one; more/conflicting flags require their actual mappings. Unlike fixed
ally-row initialization, `newEntity` does not skip an existing nonzero Luke mapping.
Later hide resolves the new Luke; it does not seek every physical actor with a
Luke sprite. The supplied F75 case can thus retain the older follower.

`setPos` and `setFacing` use their actual packed actor byte in
`AdjustScriptPointerByCharacterAliveStatus`: allies below 30 skip on zero **word**
HP, whereas FOLLOWER_B31 bypasses that test. `GetEntityAddressFromCharacter` masks
the lookup byte, adjusts NPC128+ by 96 and reads the current mapping; zero is not a
safe absent-actor skip and `$FF` is not validated as a live entity. If Peter is
absent but has nonzero HP, his position/facing commands can target main0. If31 is
undeclared, its position command can overwrite main to `(45,48)`, Left; if it maps
Astral, that actor is moved. No scene command creates/repairs Peter or31. Fresh31
can represent Oddler via F68, but it is not the departed Polca NPC140.

## Complete recruitment scene

**Confirmed:** `cs_4FF5A` (`0x4FF5A`) requests cursor texts 1556–1561
(keys `0614..0619`), five single-text and one continued-text site, plus join message 446
(key `01BE`). All 29 records, the inline body and all seven text keys are accounted for:

| Records | Ordered requests and attributed content |
| --- | --- |
| 1–7 | Cursor1556; new Luke `(43,47)`, Down; wait1; set Bowie `(45,47)`, Left; Peter `(44,47)`, Down; Luke `(43,47)`, Down; FOLLOWER_B `(45,48)`, Left. HP/mapping limits above apply to placements. |
| 8–14 | Main-theme music; `fadeInB`; wait40; Luke1556 proposes Devil's Tail to see Creed; two separately waited Luke `eas_Jump` actions; wait30. |
| 15–20 | Peter Left; Luke Right; Peter1557 notes Luke is coming; Luke1558 says he wants to fight with the leader (continued text), then1559 expresses excitement; `join ALLY_LUKE`. |
| 21–25 | Peter Right; Peter1560 remarks on Luke's adventurousness; Peter Left; Luke1561 urges departure; wait30. |
| 26–29 | Waited Luke inline `moveDown 1`/`endActions`; hide Luke; `setStoryFlag 16`; end. |

All Luke lines use `$0,ALLY_LUKE`; Peter uses `$C0,ALLY_PETER`. Names/directions in
speech do not locate an accessible route or establish combat state. There is no
choice or naming prompt. Single-text handlers close text/portrait and request
Sleep10; the continued text retains its separate dialogue lifecycle. The four
standalone sleeps total 101 requested frames, excluding services and debug skipping;
this is not elapsed wall-clock duration.

`csc15_setEntityActscript` installs each jump then waits for that mapped actor's
pointer to equal `eas_Idle`. `eas_Jump` disables auto-facing, requests relative
`(0,-1)`, wait5, `(0,1)`, wait5, `(0,0)`, restores auto-facing and idles. The final
`csc2D_entityActionSequence` builds the Down1 action and idle tail, installs it and
waits on that actor's pointer. These handlers do not inherit setPos's HP guard or
prove all followers have stopped. `eas_Init` configures speed32, movement/collision,
auto-facing/orientation and sprite state; it is not a heal. Actual positions,
scheduling and sprite/text/audio/fade completion remain Unknown.

## Membership, current hiding and the later unlock

**Confirmed:** `mapscriptengine_2.asm:csc08_joinForce` clears speech SFX, waits for
view scrolling, reads the Luke word with bit15 clear and requests ordinary join
music. It takes the single-ally branch, calls `j_JoinForce(10)`, reads class/name
indices for message 446, then fade/input, close-text and Sleep10 services. The alias
resolves to `code/common/stats/battleparty.asm:JoinForce`.

That helper sets **F10**, calls `UpdateForce`, then tests the rebuilt active count
against 12. Below 12 it calls `JoinBattleParty` to set **F42** (`32+10`); otherwise it leaves
that flag supplied. UpdateForce enumerates joined members and partitions active
versus reserve by their active flags. The helper does not refresh caches after
setting F42: a previously inactive Luke remains in that cached reserve/count snapshot
until another UpdateForce. Full capacity does not evict someone or force Luke
active; already-set F42 remains set even in an overfull supplied state. No HP test,
revival, F75 mutation, follower append or activation/AI write is present. This scene
has no `joinForceAI` or follow command. Membership is not a guaranteed player turn.

`csc2E_hideEntity` resolves lookup10 after the waited Down1 and calls
`entityfunctions_2.asm:HideEntity`. It moves that physical actor's position and
destination off-map to internal `$7000,$7000`, marks **every mapping to that physical
index** `$FF`, and removes its matching exploration-list entry by shifting the tail,
if present. The ordinary newly allocated actor was not appended to that list.
It neither clears F10/F42/F75 nor removes an older F75 follower at a different
physical index. It is not `MoveEntityOutOfMap`'s mapping-preserving operation.

A later fresh initialization clears mappings and consumes F75 independently of F10:
F75 set can construct Luke again even though joined; F75 clear omits him from this
empty fixed list. F10 set suppresses the recruitment scene in either case. Fresh
construction, current hide and a no-rebuild entry must not be collapsed into a
permanent visual removal or an automatic F75 clear.

`csc13_setStoryFlag` adds `BATTLE_UNLOCKED_FLAGS_START=400` to operand 16 and sets
**F416**. It unlocks Battle16; it does not set F414, start combat or run CheckBattle.
Battle numbering does not prove that Battle14 occurs before/after this scene on a
natural route. The decisive order is **new actor/staging → membership F10 (and
conditional F42) → join-message/services → further dialogue → waited Down1 → hide →
F416 → terminator/caller return**. These writes are not a transaction.

A stall before joining can leave a staged actor with F10 clear; a later eligible
init may create another actor rather than reuse it. Once F10 is set, interruption
inside the remaining join services or later dialogue/movement/hide can leave
F416 clear, yet future init skips the scene. F416-set/F10-clear instead still runs
it. Neither repeat path repairs missing flags, raft state, F75 or mappings. These
are source consequences for supplied/partial state, not observed save histories.

## Raft meaning and conditional control boundary

**Confirmed:** the scene neither writes PLAYER_TYPE/raft coordinates nor queues a
boarding/warp event. The separate raft may already be declared at `(43,48)` before
init under its own gates. With the intended Luke placement, Down1 points toward
that tile, but this is a Luke action followed by hide, **not player boarding** or a
receipt for raft contact. Movement/services, HP-guarded staging, alternate mappings,
collision and supplied tuple/player state prevent a guaranteed contact claim.

The script terminator conditionally waits for view scrolling when a dialogue
window remains, clears scrolling speed and returns to the init's `rts`.
Exploration then compares base/current palette state; only inequality calls
`PlayMapMusic` and `FadeInFromBlack`. Do not assume an unconditional second music
request after the scene's main-theme/fade/join services. At `WaitForEvent`, pending
zero retargets camera to physical0 and calls `SetControlledEntityActScript(0)`;
ordinary PLAYER_TYPE=0 selects character control. Nonzero pending defers that setup
to event handling, and other player types select other scripts. The selected
scene/caller adds no pending event but does not establish every supplied service
or input state. **Stop at this conditional control handoff.** No responsive-input
observation, boarding, further travel, random-zone call or Battle14/16 admission
is claimed.

## Reproduction and acceptance limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only upstream pin/clean state and run the existing
[retained payload/cutscene comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
without rewriting its inputs. Select `cs_4FF5A` from `programCorpus.programs` and
compare all 29 sourceLine/macro/argument records to lines 31–61; separately read
lines 57–58's inline body. Follow cursor1556 through the six text comments and
keys `0614..0619` in `data/scripting/text/gamescript.txt`; compare the join handler's
message 446 to key `01BE`. Do not export a dialogue transcript. Compare body 78's six
operations and both local branch targets against the init, excluding suspend bytes.

| Claim boundary | Existing fixtures under `tests/fixtures/h2/` and direct source owners |
| --- | --- |
| Exit/admission | `map-event-request-state-static-v1.json`, `map-event-request-consumption-static-v1.json`, `battle-routing-data-static-v1.json`; four Polca warp rows, movement producer, no-scroll consumer, SwitchMap and three actual Map72 battle rows/CheckBattle. |
| Setup/init/population | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`; Map72 default/empty list, init body 78, followers/overworld tables and both raft declaration consumers. |
| Scene and immediate actor lifecycle | `map-script-engine-static-v1.json` with the retained corpus; complete scene, text keys, macros, csc2B/InitializeNewEntity/GetAllyMapsprite/DeclareNewEntity, mapping/HP/action/hide helpers. |
| Recruitment/unlock/control | Existing accepted membership/entry owners plus join alias/JoinForce/UpdateForce/JoinBattleParty, csc13, script terminator and exploration init/palette/WaitForEvent consumers. |

Aggregate fixture agreement is not new per-scene executable coverage or proof of
composed natural behavior. Direct record/text/caller comparison establishes
consistency with existing records; bounded manual consumer review supplies the
semantic connections. [PR809's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/809#issuecomment-6159249354)
preserves its resolved navigation/label/patch/link failures and earlier linked
comparison/CI history; no ritual replay is required.

**Unknown:** natural Y30/exit selection and earlier choices, actual battle/follower/
raft/membership/HP/AI state, current mappings and concurrent positions, action and
service completion, rendered text/sprites/camera/music/fades, input responsiveness
and durable save/load behavior. These gaps do not block the declared static outcome
and do not authorize automatic acquisition.

Record the entire owned diff, direct comparison, links/provenance/private checks,
clean committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual
final-head CI in the Issue/PR handoff. Documentation-only acceptance runs no local
blanket normal/full/engine/Godot/H3/H4 suite. Leave a clean pushed Draft PR, Project
Review / Verify and frozen writer after the authorized main-gate callback succeeds.
Independent acceptance/merge remains main-gate's; no cleanup or next-Issue selection.
