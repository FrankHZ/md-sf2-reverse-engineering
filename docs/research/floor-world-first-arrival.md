# Floor World first arrival and conditional control

Evidence date: 2026-10-14. **Confirmed:** explicit Map26 exploration rebuilds
nine population rows. With F779 clear, its init runs all 41 commands of
`cs_59E04`, then sets F779 and EGRESS_MAP=26. The independent F780 tail moves
resident 129 out of the map. F779 already set skips both the scene and that
EGRESS assignment. **Inferred:** the six independent actors portray the
Cotton Balloon landing and popping. **Unknown:** natural arrival, supplied
state, completed actions/services, actual presentation, usable control and
persistence.

## Agreement and provenance

Standard rigor under [Issue #920](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/920)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is in population, action and return-consumer joins for one
arrival outcome. Starting accepted base:
`63fab597ee78136cd88f7cdefa2bc6c4512cf6b5`; no stacked dependency.
This document is the sole owned path. No shared index, registry, fixture,
schema, contract, design, tooling or remake change is required.

Consume accepted [Cotton Balloon departure/admission](./cotton-balloon-through-floor-world-entry.md),
[Desktop first arrival](./desktop-kingdom-first-arrival.md),
[Battle22 outcomes](./battle22-outcomes-and-desktop-return.md),
[setup selection](./map-event-flag-route-selection.md) and
[request consumption](./map-event-request-consumption.md). Begin at the
surviving explicit Map26 exploration admission; stop at conditional local
control or pending-event handling after init. Resident interactions, the
F780 giant-ring scene `cs_59F20`, chest/search, exits/tunnel, Battle23 and
later recruitment/Creed content are excluded. No runtime acquisition,
general physics/renderer work or new verification machinery is admitted.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Short setup paths mean `data/maps/entries/map26/mapsetups/`; map and entity
consumers mean `code/common/scripting/map/` and `code/common/scripting/entity/`.
Fnnn denotes flag nnn; row/command indices are zero-based. Coordinates,
speeds and waits are source operands, not measured motion or duration.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected sources/fixtures. Linear comparisons are budgeted below 512 MiB and
one minute, plus small prose/receipts. Retained equality and source-cutscene
comparison passed in 0.079 seconds. No payload regeneration, environment,
installation or capture is needed; reassess before material expansion.

## Admission, fresh load and current-state entry

**Confirmed:** the predecessor's surviving request is Map26 `(26,8)`, Down.
MainLoop's normal SwitchMap/CheckBattle route neither substitutes Map26 nor
finds a battle row for it. Battle23 uses Map27. This is exploration admission,
not evidence that the party naturally arrived or already has usable control.
The departed item handler/scene wrote no EGRESS_MAP; the destination init
write described below is a separate, later effect.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` first
clears MAP_EVENT_TYPE, adjusts STEP_COUNTER, heals eligible living/immortal
allies, starts fade-out and disables camera following. The explicit map-index
path writes CURRENT_MAP=26 and CURRENT_BATTLE=255, loads tilesets, waits
fading, clears VInts, selects and initializes entities, then loads mapsprites.
It clears temporary flags 256–383 and sets F80. This does not clear F779/F780
or establish that every ally is alive. It initializes sprite-frame state,
targets physical 0, calls LoadMap with the explicit map index, installs base
VInts, then calls RunMapSetupInitFunction.

This fresh load replaces Desktop actors, lookups and their motion. It does
not carry Desktop137–139 into Floor World130–132. `data/maps/mapsetups.asm`
lines 90–91 contain only `msMap 26,ms_map26; msMapEnd`: no flag override.
The six pointers in `pointertable.asm` at `$59D08` select this map's Entities,
EntityEvents, ZoneEvents, AreaDescriptions, Section5 and InitFunction.
GetCurrentMapSetup resolves the current map on each accessor call; neither
F522 nor F779/F780 selects another Map26 population.

A supplied map index 255 takes a different ExplorationLoop branch: it waits
fading, calls UpdateMainEntityProperties and DeclareRaftEntity, then joins
sprite-frame initialization, LoadMap and init dispatch. It does not perform
fresh entity allocation, tileset loading, temporary-flag clearing or F80 set.
The current map and existing lookups/layout therefore matter. Nonnegative
supplied X/Y bytes update main current/destination; facing and main sprite
are updated, and the raft/follower helper follows its mode/flag branches.
This path is not the accepted explicit Map26 arrival. Neither the scene's
reloadMap nor its return re-enters this map index255 branch.

In that alternate branch, DeclareRaftEntity returns immediately for mode2.
Mode1 skips follower repositioning; other modes first place the listed
followers' current/destination at the supplied coordinates. It then requires
F64 to handle the separate raft: matching RAFT_MAP declares physical31 and
lookup `$3F`; a different saved map clears that lookup and parks physical31
at raw `$7000,$7000`. This differs from fresh-load sub_44404 below.

## Complete population and logical-to-physical mapping

**Confirmed:** `mapfunctions.asm:InitializeMapEntities` clears records/lookups,
allocates selected followers from physical 1, consumes setup rows in order,
and finally declares Bowie at physical 0 at the requested `(26,8)`, Down,
with eas_Idle. It writes shared SPRITE_SIZE=16, terminates the exploration
follower list and calls `followersfunctions_2.asm:sub_44404` for raft handling.
The complete `s1_entities.asm` at `$59D20..$59D6A` has these nine rows.
P assumes only F67/Peter and F68/Oddler, no skipped ally row and no overwrite;
Init/Fixed mean eas_Init/eas_InitFixedSprite.

| Row | Logical identity | P | Sprite / action | Position, facing |
| --- | --- | --- | --- | --- |
| 0 | Randolf16 | 3 | Ally mapsprite lookup; Init | `(27,22)`, Right |
| 1 | Tyrin17 | 4 | Ally mapsprite lookup; Init | `(29,22)`, Left |
| 2 | NPC128 | 5 | OLD_MAN198; walking origin `(50,12)`, range 3 | `(50,12)`, Down |
| 3 | NPC129 | 6 | YOUNG_MAN196; Init | `(5,24)`, Down |
| 4 | NPC130 | 7 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 5 | NPC131 | 8 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 6 | NPC132 | 9 | OBJECT1=173; Fixed | `(63,63)`, Up |
| 7 | NPC133 | 10 | PRIEST192; Init | `(4,35)`, Down |
| 8 | NPC134 | 11 | CARAVAN62; Init | `(11,35)`, Down |

`sf2mapsetupmacros.asm` encodes each row's X/Y/facing/sprite followed by
an action pointer, or `$FF,originX,originY,range` for the walking row.
The allocator masks coordinates by `$3F` and multiplies by MAP_TILE_SIZE=384.
Sprite operands below COMBATANT_ALLIES_NUMBER=30 use ally lookup slots;
a nonzero existing lookup skips that entire row without consuming a physical
slot. The declared follower table contains neither Randolf nor Tyrin, so this
skip is not created for them by the ordinary fresh follower allocation.
No joined-force or active-party test admits these two rows. Their presence
is not recruitment; HP/class select their actual mapsprite, including
BLUE_FLAME for a dead ally outside battle (`getallymapsprite.asm`).

The seven ordinary sprite rows append lookup indices from offset 32; ally
rows do not consume that cursor. GetEntityAddressFromCharacter masks the
logical ID and subtracts 96 for NPC128–134. Thus logical129 is the young man,
not Tyrin or a balloon; its ordinary physical slot is 6. `DeclareNewEntity`
in entityfunctions_1 initializes current/destination, velocity/travel, sprite,
facing, physical ENTNUM, action/timer and flags. The walking descriptor calls
entityfunctions_2:SetWalkingActscript, which copies 50 bytes of eas_Walking
and patches origin/range. Its wait30, random walk, destination wait and wait20
loop does not establish the old man's later observed position.

Map26 is absent from the complete `data/maps/global/overworldmaps.asm` and
is neither Tactical Base nor Nazca Ship. F65 therefore adds no overworld
Caravan/A prefix. `followersfunctions_1.asm` consumes
`data/scripting/entity/followers.asm` in the following selected-row order:
F66/Sarah, F67/Peter, F72/Kazin, F73/Slade, F74/B(Astral), F66/Chester,
F68/B(Oddler), F75/Luke, F69/C(Fairy), F70/B(Astral), F74/A(King Granseal),
F71/Lemon, F74/D(Minister), F76/Zynk, F77/A(Old Man), F82/Frayja,
F83/A(King Granseal), F83/D(Minister), F84/Rohde. Action pointers advance
only for selected rows. Only F67/F68 gives Peter7/physical 1/eas_Follower1
and B31/physical 2/eas_Follower2, initially at the requested position/facing.

Later B/A/D declarations overwrite their logical lookup while earlier physical
records remain. Other follower flags shift population physical slots.
Absent zero lookups resolve to physical 0, not a missing-actor no-op. Joined
party membership alone does not allocate Peter or B; the scene neither creates
these missing followers nor recruits/revives anyone. Its six logical subjects
are only six distinct physical actors under coherent, distinct mappings.

No selected command changes PLAYER_TYPE or saved raft state. In sub_44404,
mode 2 replaces main's sprite with RAFT, parks followers at raw `$6000,$6000`
with eas_Standing, and declares physical 31 as RAFT at tile `(64,64)` without
the saved-map test. Otherwise F64 plus RAFT_MAP=26 declares physical 31 at
saved X/Y masked by `$7F`. It writes lookup offset `$3F` and can overwrite
that slot's previous record. In the ordinary two-follower example slots 0–11
are separate from 31; do not import Desktop's slot31/ORB overlap. Supplied
mappings and mode still need their actual consumers, not an assumed walking
arrival or identical visual identities.

## Init dispatcher and all 41 landing commands

**Confirmed:** the complete `ms_map26_InitFunction` at `$59DDC`,
`s6_initfunction.asm:7–24`, checks 779 first. Clear executes cs_59E04; only
its return permits setFlg 779, then the byte write EGRESS_MAP=MAP_FLOOR_WORLD (26).
Set jumps directly to the 780 check, skipping both operations. F780 clear
returns; set calls MoveEntityOutOfMap with logical129, then returns.
No F780 write or giant-ring action occurs in this init tail.

`cs_59E04` at `$59E04`, lines 28–98 in that same file, is the whole selected
scene, including six custom bodies and six inline sequences. B means 31;
ordered six means Bowie, Peter, B,130,131,132. Each custom body ends ac_end,
and each inline body ends endActions.

| Index | Complete command / nested body |
| --- | --- |
| 0 | textCursor 1739 |
| 1 | setCameraEntity CAMERA_NEUTRAL=`$FFFF` |
| 2 | reloadMap 21,8 |
| 3–5 | setActscriptWait Bowie/Peter/B, eas_Init, in that order |
| 6–8 | stopEntity Bowie/Peter/B, in that order |
| 9–14 | Six customActscriptWait bodies in ordered-six order; each `ac_setSpeed 10,10; ac_jump eas_Idle; ac_end` |
| 15 | setPos Bowie,26,7,Down |
| 16 | setPos Peter,27,6,Down |
| 17 | setPos B,25,6,Down |
| 18 | setPos 130,26,6,Up |
| 19 | setPos 131,25,5,Up |
| 20 | setPos 132,27,5,Up |
| 21 | fadeInB |
| 22–26 | Five entityActions bodies for Bowie/Peter/B/130/131; each `moveDown 7; endActions` |
| 27 | entityActionsWait 132: `moveDown 7; endActions` |
| 28 | csWait 20 |
| 29–31 | startEntity Bowie/Peter/B, in that order |
| 32 | csWait 40 |
| 33–35 | hide 130; hide 131; hide 132 |
| 36 | csWait 30 |
| 37 | nextSingleText `$FF,255` (1739) |
| 38 | followEntity Peter,Bowie,2 |
| 39 | followEntity B,Peter,2 |
| 40 | csc_end |

### Camera, initialization, animation and absolute placement

The `script` macro's trap6 activates VInt_UpdateEntities before
ExecuteMapScript. `mapscriptengine_1.asm:csc24` writes negative camera target
`$FF` directly, without an entity lookup. csc46 reloadMap again disables
following, deactivates VInts, packs 21/8 and multiplies by 3 for LoadMap with
d1=-1, then enables display/interrupts, reactivates VInts and waits one VInt.
`code/common/maps/mapload.asm:LoadMap` uses the current map's area properties
and the existing working layout on that negative-map-index path. It bypasses
LoadMapBlocksAndLayout; it does not rebuild actors, clear temporary flags,
restore initial positions or invoke init recursively. `(21,8)` is the scene's
view input, not a new player spawn, destination warp or clean layout reset.

csc15 and csc14 install the named/custom action on the resolved physical
actor, seed its wait timer with that physical index and, for these wait forms,
poll its action pointer until eas_Idle. They have no alive/missing-actor guard,
all-actor barrier or timeout. A subsequent installation can replace an
unfinished action. Actual VInt/action progress remains a premise.

`data/scripting/entity/eas_actions.asm:eas_Init` writes acceleration factors 0/0,
speed 32/32, acceleration/deceleration off, obstructable on, entity/map
uncollidable off, autofacing on, orientation Up, immersion off and shared
SPRITE_SIZE=24, then updates the sprite and jumps idle. It does not reset
position, HP, travel/velocity or turn animation on. InitFixedSprite additionally
turns autofacing/animation off. The three object rows start with Fixed;
this scene installs waited Init only for the trio, not for 130–132. Completion
of each original object Init is not separately joined before its custom
speed pointer replaces it. Shared sprite size is not persistent world scale.

The six custom bodies use esc10 to write adjacent X/Y speed bytes 10/10 and
esc34 to jump idle. They do not themselves restore the other Init properties.
csc1C stopEntity writes animation counter -1; csc1B startEntity later writes 0.
Neither stops velocity, idles movement or waits for an actor's destination.
Both skip a true ally 0–29 at HP0; B31 bypasses that guard.

csc19 setPos writes current and destination X/Y to the absolute tile×384,
writes facing and updates the sprite. It leaves action, velocity and speed
unchanged. Its true-ally alive guard can skip Bowie/Peter at HP0; B and the
three NPCs bypass it. Consequently even with a shared visual sequence,
action installation, placement and animation commands have different dead-ally
behavior. GetCurrentHp reaches the low-byte-masked combatant lookup in
`code/common/stats/combatantstats_1.asm` and `_3.asm`; it does not test NPC HP.

fadeInB/csc39 calls `FadeInFromBlack` in
`code/common/tech/interrupts/fadingcommands.asm`. ExecuteFading initializes
fading state, waits VInts until FADING_SETTING clears, then waits another VInt.
This is a service-return condition, not observed successful display or a
measured duration. Motion installation follows its return.

### Independent downward actions and distinct hide helpers

`csc2D` clears target FLAGS_A bits 5/6 with `$9F`, compiles each moveDown 7 into
esc04 relative `(0,+7)`, esc01 destination wait and appended esc34 idle jump.
The six are installed sequentially, permitting concurrent progress; they do
not start simultaneously. `entityscriptengine_2.asm:esc04` multiplies signed
deltas by 384 **from current position**, then sets destinations, travel and
signed velocities from the speed bytes through loc_55C8. Its
HasSameDestinationAsOtherEntity bit5-clear arm returns Z set; esc04's `bne`
conflict branch is bypassed for these unmodified actions. This is not a
general collision/physics proof or a reason to add a destination-occupancy gate.

If the six placements occurred and remain their current positions when the
relative command executes, the requested endpoints are Bowie `(26,14)`,
Peter `(27,13)`, B `(25,13)`,130 `(26,13)`,131 `(25,12)`,132 `(27,12)`.
These are conditional requested destinations, not guaranteed final positions.
There is no object/party attachment, carrying constraint or single balloon
physics object in these commands. Each actor owns its own action/destination.

esc01 advances only when that actor's current X/Y equal its current destination;
it does not retain an immutable original target. The map script's sole waited
movement is 132's idle-pointer wait. Even 132 completing its unchanged sequence
joins only 132, not the other five. Later waits 20/40/30 are sleeps, not
completion checks. Animation restart, hide and following can therefore occur
without an established all-six completion. Replaced actions/destinations,
aliases, stalled updates or deferred sprite work can change/prevent progress.
VInt_UpdateEntities skips raw X at least `$7000`; `(63,63)` is below that
threshold, while an already hidden supplied actor need not execute a new action.
esc0A's sprite-load capacity condition can defer its script's progress.

csc2E hide 130/131/132 resolves each logical ID and calls
`entityfunctions_2.asm:HideEntity`. It sets current and destination to raw
`$7000,$7000`, rewrites **all matching** ENTITY_INDEX_LIST entries to `$FF`,
and removes a matching entry from EXPLORATION_ENTITIES by shifting the list.
It does not recruit, free inventory, change mapsprite or play a pop sound.
It has no HP guard and no wait for the other actors. Missing zero lookups can
address main; aliases can hide more than one logical identity. After lookup
invalidation, these resolvers do not supply a safe `$FF` missing-actor no-op;
subsequent supplied-state accesses cannot be described as normal actors.

The independent F780 tail uses a different helper:
`mapsetupsfunctions_1.asm:MoveEntityOutOfMap` at `$477EA` calls
j_GetEntityIndexForCombatant, then j_SetEntityPosition with raw `$7000` for
both axes. The s05 jump aliases resolve to the actual bodies in
`code/gameflow/battle/battlefunctions/battlefunctions_0.asm`.
The first uses the same NPC129-minus96 lookup; the second writes only current
and destination X/Y. It preserves lookup, follower list, action, sprite and
animation state. It therefore parks the young man/ordinary physical 6 without
HideEntity's lookup removal. An altered alias changes which physical record
is parked; the source comment alone cannot establish that mapping.

### Literal text and final follower copies

**Confirmed:** `data/scripting/text/gamescript.txt` key06CB/decimal1739 is
`The Cotton Balloon popped.{W1}`. No name, Yes/No branch or second text occurs
in this scene. `sf2cutscenemacros.asm:nextSingleText` encodes modifier `$FF`
and entity 255 as word `$FFFF`. In `mapscriptengine_2.asm:csc00`, that exact
sentinel bypasses portrait and speaker-sprite lookup and sets CURRENT_SPEECH_SFX=0.
It is not NPC255 speaking, nor a request for Peter/Oddler's portrait or bleep.
An already open window is not proof of a newly selected portrait.

The handler waits view scrolling, displays text 1739, increments the cursor,
closes portrait/text and sleeps 10. `textfunctions_1.asm:symbol_wait1` waits
on the directional/A/B/C mask. Debug mode plus P2 Start can latch
SKIP_CUTSCENE_TEXT: csc00 then skips its presentation and cursor increment,
and csWait sleeps are bypassed; action/flag commands still execute. The
literal says the balloon popped, but the scene has no explicit playSound,
explosion or physics operation. Audio/visible performance is unobserved.

`csc2C_followEntity` first calls AdjustScriptPointerByCharacterAliveStatus
using `move.b (a6),d0` **before** reading the word follower operand. Both
`$0007` and `$001F` have high byte 0: these two calls test **Bowie's** HP,
not Peter's or B's. If admitted, the full words resolve follower and target
to physical IDs. Operand 2 selects signed `(-24,0)` in table_FollowerPositions.
`entityfunctions_2.asm:AddFollower` copies 42 bytes of eas_Follower1 into
walking-parameter storage, patches target/offset words and installs the pointer;
it appends the physical follower to EXPLORATION_ENTITIES only if absent.
Peter follows Bowie, then B follows Peter, under the actual mappings.

The copies configure speed 32/32, acceleration/deceleration off and the
follower collision settings when executed. Their esc03 follow loop considers
target current position, destination/facing and layout before writing travel,
then waits for destination and repeats. This is neither an absolute formation
placement nor a completed following observation. The copies replace earlier
motion pointers even if that motion is unfinished. AddFollower does not
teleport, heal, recruit or set follower/story flags; altered mappings can
produce a different or self-referential chain. No final waited Init resets
the trio here. Bowie's later controller is a separate installation.

## Return ordering, repeats and conditional local control

**Confirmed:** csc_end reaches ExecuteMapScript's tail: conditionally wait
view scrolling if a dialogue window exists, clear VIEW_SCROLLING_SPEED and
restore caller registers. It adds no all-actor wait. Trap6 returns to init,
which sets F779, writes EGRESS_MAP=26, then independently checks 780/parks129.
RunMapSetupInitFunction restores its saved registers after calling the selected
callback. Unlike the item/zone wrappers, it adds no physical 0 destination wait
or portrait/text closure.

| State at the init checks | Ordered conditional result |
| --- | --- |
| F779 clear, F780 clear | Complete scene return → set F779 → EGRESS_MAP=26 → return; resident 129 remains. |
| F779 clear, F780 set | Same scene/779/EGRESS sequence → park129 → return. F780 does not suppress landing. |
| F779 set, F780 clear | Skip landing, the F779 write and EGRESS write; return with existing EGRESS_MAP. |
| F779 set, F780 set | Skip landing/EGRESS, park129, then return. |

These are checks when reached, not a snapshot reserved at entry. Fresh repeat
Map26 loads still rebuild the same nine rows, including objects at `(63,63)`;
F779 alone bypasses scene placement/hiding/follow-copy work. Existing follower
declarations still apply. F780 controls the separate resident 129 tail. A
map index255 repeat instead retains its supplied population/layout subject to its
own main/raft updates. Supplied F779 proves neither prior completed landing
nor EGRESS_MAP=26. No skipped-scene branch repairs an earlier EGRESS value.

Failure before scene return can leave placements, hidden objects or following
installed while F779 remains clear. Failure after set F779 but before the byte
EGRESS write can leave F779 set with the old EGRESS; a later skipped scene will
not repair it. Failure after EGRESS but before the 780 tail can leave129 present.
Completed prefix writes have no general rollback, and this static ordering
is not evidence of an observed interruption, save or persistence outcome.

After callback return, ExplorationLoop compares PALETTE_1_BASE_02 with
PALETTE_1_CURRENT_02 as longwords. Only inequality calls PlayMapMusic then
FadeInFromBlack. An earlier fade command does not prove that comparison's
actual result, music playback or screen state. It next calls SetMoveSfx and
WaitForEvent. If MAP_EVENT_TYPE is zero at entry, WaitForEvent targets
physical 0 and installs SetControlledEntityActScript's current-mode action:
mode 0 eas_ControlledCharacter; mode 1 the source-named eas_Raft (Caravan
control); other nonzero modes eas_Caravan (raft control). It does not validate
an arbitrary supplied mode. The walking action configures speed 32 and its
control loop when executed; scene return alone has not restored every field.

A nonzero pending event skips controller installation and wins over A/C input.
ProcessMapEvent clears MAP_EVENT_TYPE before dispatching the sampled type;
the current request/parameters determine subsequent work. No landing command
itself writes a warp, battle request, PLAYER_TYPE or saved-raft state. No
unconditional usable-control claim follows from script return, particularly
with a supplied event, changed mode/aliases or incomplete service. Stop here.
`s2_entityevents.asm` identifies later Randolf/Tyrin/resident/service callbacks;
`s3_zoneevents.asm` identifies the F780-gated cs_59F20 giant-ring zone as a
separate next boundary. Their dialogues/actions, later travel and Battle23
are not part of this arrival result.

## Reproduction and retained limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
verify the read-only source HEAD/clean status at the pin above. Reuse the
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init equality, _verify_map_script_engine_fixture and _cutscene_facts.
Do not regenerate the pair. `tests/fixtures/h2/map-init-static-v1.json`
retains routeJoins sourceOrder63 (Map26/default), pointerTableJoins
sourceOrder66 (`$59D08` → `$59DDC`), scriptCallSites siteOrder44
(init operation2 → cs_59E04) and directCallSites siteOrder40
(operation8 → MoveEntityOutOfMap `$477EA`). These are separate source-order
identities, not gameplay order or proof of natural arrival.

Select entryLabel cs_59E04 from `local/derived/map-script-engine-static.json`'s
programCorpus.programs: source lines 28–98, 41 commands, csc-end termination.
Compare every source-line/macro/operand against the retained record, then
read all six custom bodies and six inline bodies. The map-script fixture's
lifecycle, action, placement, presentation and relationship handlers retain
the named command seams; direct source review joins their actual effects.
The request-consumption fixture at
`tests/fixtures/h2/map-event-request-consumption-static-v1.json` retains the top-level
eventRequestConsumption.consumerContexts explorationLoop/waitForEvent seams.
The flag-route-selection fixture has no ms_map26_InitFunction program-route
context; use the explicit map-init joins above for this init, not an inferred
route from that aggregate. Verify the entire nine-row table, single setup/init,
constants, literal 1739 and each selected immediate consumer above.

Direct checks cover the whole owned prose/diff, source provenance, relative
links/anchors, English scope and private/generated boundary. Run
`git diff --check` and configured `uv run sf2 verify plan --base origin/main --head HEAD`
on the clean committed candidate. Record that plan and actual final-head CI
on Issue920. Documentation scope requires no local blanket normal/full/engine/
Godot/H3/H4 suite, new fixture or runtime acquisition. Aggregate comparisons
do not independently guard every prose claim or establish natural continuity.

Preserve [#916 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/916#issuecomment-6175681254)
and [PR918 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/918#issuecomment-6175709354):
original head `9aaa845048a3be6d3c9011849e1f832c220ecde4`, rebased head
`48d5175f0027a2a01088d171a2aaca02260da8a8`, original/rebased CI38616821310/
38617104204, unchanged 18,285,979-byte pair/0.081s, all inspection, guessed-path,
fixture-key and private-regex failures and linked predecessor corrections.
Preserve [#915 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/915#issuecomment-6175613847)
and [PR917 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/917#issuecomment-6175632640)
with separate scope-only CI38616608492. This slice replaces none of those
completed results or failures.

**Unknown:** natural arrival; actual supplied flags, force/HP/class, follower/
lookup/raft mappings, layout and positions; completed action, input, text,
scroll, fade, sprite, music and control services; exact motion/duration,
audiovisual presentation, usable exploration, saving and persistence.
The changed-state distinctions above are static counterexamples, not played
scenarios. [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md) retain these
limits without creating an automatic runtime queue.
