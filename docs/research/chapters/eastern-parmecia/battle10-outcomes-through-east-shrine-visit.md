# Battle10 outcomes through the optional East Shrine visit

Evidence date: 2026-10-13. **Confirmed:** Battle10 victory returns to Map70 at a
battle-derived position. A separate no-scroll warp connects `(43,40)` to East Shrine,
Map62, `(6,18)`, Up. The shrine's two zone scenes and two old-man conversations have
different gates; neither victory nor May recruitment is a tested visit prerequisite.
The last conversation requests following through character `FOLLOWER_B=31`, sets F77,
then returns to its caller, which sets F729. Later ordinary entry rebuilds the old man
under `FOLLOWER_A=30` instead. **Inferred:** this optional content branch develops the
Sky Orb/Grans dialogue. It is not a proved mandatory route to Battle11. **Unknown:**
natural reach, actual actors/story state, completed services and continuous playability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #764](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/764), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Outcome, caller flags and current versus rebuilt follower identities need explicit
cross-owner boundaries. This document owns the new investigation and scope agreement.
The same Issue also owns only the `followEntity` HP-guard correction and its controls in
[Ribble research](./battle09-outcomes-through-ribble-welcome.md),
[Ribble synthesis](../../../design/synthesis/chapters/eastern-parmecia/battle09-outcomes-through-ribble-welcome.md),
[castle-audience research](./peter-castle-audience-through-town-return.md) and
[castle-audience synthesis](../../../design/synthesis/chapters/eastern-parmecia/peter-castle-audience-through-town-return.md).
Those four paths have exclusive transferred ownership; no other shared-path write or
stacked dependency. This bounded dependency correction remains Standard: existing
macro/handler and observer evidence suffice, with no new runtime or product behavior.
Starting accepted base: `72b254c8d24669102242fc96bbffca8811918739`.
Continue [Battle10 admission](./ribble-may-through-battle10-admission.md) through
outcomes and the selected Map70→62 visit. Stop after the final old-man caller and first
conditional restored exploration control. The south exit is only a continuation
boundary. Combat/roster simulation, other world routes, cave/Battle11, return-shrine
or Grans travel, wooden panel, Sky Orb/Nazca acquisition, the later F732 scene,
unrelated NPC/shop/treasure content, broader design, translation, engine/UI and runtime
acquisition are excluded. This is original-game research, not remake acceptance.

Source paths are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons and the existing cutscene-source comparison pass; no fresh
extraction or runtime observation is claimed. Fnnn means flag nnn; battle rows are
zero-based. Map62 setup filenames below are under `data/maps/entries/map62/mapsetups/`.
Script record numbers are one-based indices in retained
`programCorpus.programs[].commands`; nested entity actions are not extra map-script
records. Map-event `programOrder` and init `sourceOrder` are zero-based.

Acceptance is direct source/row/handler/list/fixture review, retained comparisons,
document/link/private/diff checks, clean committed planner and actual final-head CI.
The reused payloads total 18,285,979 bytes: one process, linear selected reads,
estimated peak below 512 MiB and one-minute comparisons, with only prose and small
ignored receipts. No new comparator, parser, fixture or capture platform is needed.
Reassess before expanding routes/data/semantics. Unknowns are not an acquisition queue.

## Battle10 outcomes and immediate return

**Confirmed:** shared [battle lifecycle](../../battle/battle-loop.md) and
[cutscene dispatch](../../battle/battle-cutscenes.md) apply to row 10, `BATTLE_TO_THE_EAST`.
`battleloop_1.asm` checks counted allies before counted enemies after actions and
after-turn effects. `battleloop/countremainingcombatants.asm` counts nonnegative-X,
nonzero-HP combatants; Bowie's zero HP forces ally count zero. Victory requires allies
remaining and no counted enemies; simultaneous exhaustion takes defeat.

| Row / consumer | Exact Battle10 consequence |
| --- | --- |
| `data/battles/global/enemyleaderpresence.asm`, row 10 | Zero. Killing enemy slot 128 is not sufficient to win or to remove the other enemies. |
| `data/battles/cutscenes/battleendcutscenes.asm`, row 10 | `ms_Empty`. `battleendcutscenesstart.asm` dispatches only with Bowie alive, enemy 128 dead and F510 clear. Its shared tail reads the zero leader row and does not append all remaining enemies to the death list. F510 set skips the script, not that tail. |
| `data/battles/cutscenes/afterbattlecutscenes.asm`, row 10 | `ms_Empty`, dispatched by `afterbattlecutscenesstart.asm` only with F510 clear. The script does not replace the seeded return tuple. |
| `data/battles/cutscenes/afterbattlejoins.asm`, row 10 | Zero, Bowie. The included table in `layout/sf2-07-0x044000-0x064000.asm` agrees with the duplicate global-path table. `EndAfterBattleCutscene` executes `j_JoinForce` even when the after script was skipped. The unused-feature comment does not suppress this call. |

The shared join tail sets F0, updates force, then conditionally sets F32 if active
capacity permits. It does not join May or the shrine's old man. The first-member
return position has already been selected before this tail. An empty map script still
performs its terminator's conditional view-scroll wait and clears scrolling speed.

| Outcome | Ordered behavior and destination |
| --- | --- |
| Victory | `code/gameflow/battle/battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies, seeds map from `CURRENT_MAP`, gets the first battle-party member after `UpdateForce`, adds battle-area X/Y to that member's combatant coordinates and reads its mapped entity facing. It sets parameter 1 to zero, calls the after wrapper, clears F410, sets F510 and returns parameters 2–5 with D4=1. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, presents exhaustion text 363, restores Bowie's HP, halves gold by unsigned right shift and returns the egress tuple with D4=-1. Battle4's exception does not apply; F510 is not newly set. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` consumes spell MP or item, runs its text/flash services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame and returns the egress tuple with D4=0. No defeat gold/HP penalty or victory completion write is added. |

`data/battles/global/battlemapcoords.asm` row 10 is Map70, area `(0,0,32,32)`,
trigger `(255,255)`. Hence ordinary victory returns Map70 with the chosen member's
coordinates/facing, not fixed shrine coordinates and not necessarily Bowie's position.
Dead non-immortal allies are not resurrected by victory healing. `UpdateBattleUnlockedFlag`
clears F410 only if F510 was already set; first-battle defeat/egress retains the supplied
unlock. `battleloop/getegresspositionforbattle.asm` has no Battle10 override: carried
`EGRESS_MAP` goes to `GetSavepointForMap`, including its unmatched-row `(1,1)`, Up
fallback. The earlier F399 battle-entry write excludes the pre-first-battle bedroom
fallback on this lineage. No last savepoint is established here; shared F64 raft effects
do not designate the shrine as the egress destination.

`code/gameflow/mainloop.asm:alt_MainLoopEntry` calls `SwitchMap`, then exploration
without a second `CheckBattle` on that immediate return. Map70 has no switched-map
replacement. Supplied-map `ExplorationLoop` clears pending, adjusts the step counter,
heals living/immortal allies, sets current map/not-in-battle, rebuilds entities/sprites,
clears temporary flags 256–383, sets F80, loads the map and calls init. Map70 has the
default empty fixed list and `ms_map70_InitFunction` (`0x4FE42`, body 77), just `rts`.
Flag followers may still exist. Pending zero after services permits `WaitForEvent`
to reinstall control; BattleLoop supplied `PLAYER_TYPE=0` unless later overwritten.
Carried F414 cannot intercept that immediate post-battle return.

## Selected Map70→62 visit and first control

**Confirmed:** the [accepted Map70 admission owner](./ribble-may-through-battle10-admission.md#map70-zone-requests-and-ordered-battle-admission)
distinguishes a later request from the immediate return. A zone needs layout attribute
`$1400`. Specific X16–19, wildcard Y, reaches the Battle14 random helper, without
falling through to default if it declines. F514 clear requests; otherwise a nonzero
step counter declines, and at zero the range-8 RNG result decides request versus
range-4-plus-2 cooldown. The default requests Battle10 only with F510 clear; F510 set
does not clear a supplied F410. A request writes partial reload long `$000100FF`;
surviving parameters still matter. On a later Map70 admission, row 10/F410 precedes
row 14/F414; completion clears the selected unlock but does not skip its battle on
that call. These controls prevent a claim of uninterrupted walking. Battle14 content
and other world routes are not audited here.

The selected first row of `data/maps/entries/map70/6-warp-events.asm` is exactly
`(43,40)`, no scroll, `MAP_EAST_SHRINE=62`, destination `(6,18)`, Up.
`exploration.asm:WarpIfSetAtPoint` requires the warp layout attribute `$1000` and
matching row, then writes type 1, the complete tuple and warp SFX. The no-scroll
consumer in `explorationfunctions_2.asm` idles main and returns the tuple to main after
its services. Both coordinates are explicitly positive. The
[request owner](../../world/map-event-request-consumption.md) retains mutable-slot and request
survival limits; a warp row does not prove a navigable path to it.

There is no Map62 row in `battlemapcoords.asm` and no Map62/70 substitution in
`data/maps/global/flagswitchedmaps.asm`. `SwitchMap`/`CheckBattle` therefore permit
ordinary supplied Map62 entry for this intact tuple, independently of carried F410/F414.
No F510, F8, May, Peter or shrine-story check gates the selected warp or Map62 admission.
Its normal entry rebuilds the population below, resets F256/F257/F258 with the other
temporary flags, and calls `ms_map62_InitFunction` (`0x5C874`, retained body 71), just
`rts`. It neither runs a visit scene nor writes a pending request. After entry services,
pending zero permits the first controlled exploration at `(6,18)`, Up. This is a
conditional code boundary, not observed initial input responsiveness.

## Setup, population and event identity

**Confirmed:** `data/maps/mapsetups.asm` selects `ms_map62_flag77` when F77 is set,
otherwise `ms_map62`. `pointertable.asm` (`0x5C76A`) and `pointertable_77.asm`
(`0x5C782`) differ only in the entity-list pointer; both share entity events, zones,
descriptions, item events and init. `GetCurrentMapSetup` reevaluates flags at lookup.
A flag write is not itself a current-population reload.

| Population | Identity after ordinary initialization |
| --- | --- |
| F77 clear, `s1_entities.asm` | Two `eas_Init` fixed rows: young man `(6,11)`, Up → character/event 128; old man `(5,6)`, Down → 129. Dialogue identifies the latter as the grandfather and the former as Petro's brother. Authored origins are not observed final locations. |
| F77 set, `s1_entities_77.asm` | Only young man `(7,5)`, Down → 128. No fixed 129. Independently, the global F77 follower row declares an old-man sprite as character 30 (`FOLLOWER_A`). |
| Followers before fixed rows | `InitializeMapEntities` calls `ClearEntities` and `InitializeFollowerEntities` first. Map62 is neither a no-follower map nor overworld; F65 does not select the caravan prefix. F67 selects Peter (`ALLY_PETER=7`); other follower flags are independent inputs. |

`mapfunctions.asm` maps ordinary NPCs from event 128 upward while followers consume
physical slots first; lookup identities 128/129/30/31 are not physical entity indices.
Main occupies physical slot 0. `followersfunctions_1.asm` declares enabled rows in
source order and assigns their lookup bytes; it does not require joined/active status
or discard dead allies. `GetAllyMapsprite` supplies the blue-flame sprite for a dead
ally outside battle. F67 alone therefore creates a Peter mapping but does not prove
a living Peter, nor any mapping for 31. Dialogue using Peter's ID proves a text/speech
request, not that Peter is physically present.

`s2_entityevents.asm` has explicit 128/129 rows, both control byte `DOWN=3`, plus a
default return. In `RunMapSetupEntityEvent`, bit 0 faces the actor toward the player
before the handler and bit 1 restores its previous facing afterward. This byte is
not an approach-direction requirement. Ordinary activation resolves the forward
physical actor through `GetActivatedEntity`/`GetEntityEventIndex`; it excludes current
followers and the controlled entity. See [interaction state](../../world/map-event-interaction-state.md).
The dispatcher handles portrait/speech, disables entity updates around the direct
handler and reenables them at its tail; the `script` trap reenables entity updates
while the interpreter runs. Caller, mapping and script effects are separate boundaries.

## Independent zone and conversation gates

**Confirmed:** the complete bounded gates in `s3_zoneevents.asm` (`0x5C820`) and
`s2_entityevents.asm` (`0x5C7B6`) are:

| Caller | Predicate and ordered result |
| --- | --- |
| `(5,15)` / `(6,15)` → `Map62_ZoneEvent0` | F729 set or F256 set returns; otherwise run `cs_5C876`, then set F256. |
| `(5,16)` / `(6,16)` → `Map62_ZoneEvent2` | F729 set, F256 clear or F257 set returns; otherwise run `cs_5C8FE`, then set F257. |
| 129 → `Map62_EntityEvent1`, F729 clear and F728 clear | Run `cs_5C8D4`, then set F728 and return. |
| Same entity, F729 clear and F728 set | Run `cs_5C914`, then set F729 and return. The script has already attempted following and set F77. |
| Same entity, F729 set | Text 2512 (`09D0`), saying this route cannot reach Grans, then return. This branch tests neither F728 nor F732. Its presence is not proof of natural post-follow interaction. |
| Zone / entity default | Return. |

No old-man conversation checks F256/F257, F510, F67 or F77. The second zone does
not check F728. The gates allow talking twice without the zone scripts if physical
activation is possible; conversely the second zone may run before the first talk once
F256 is set. Entering the Y16 row before Y15 with F256 clear does nothing. A later
crossing after F256 may request the invitation, without requiring a prior attempt to
leave. Neither dialogue meaning nor coordinate order establishes the natural sequence.

F728/F729/F77 are independent persistent game flags. F256/F257/F258 are temporary,
cleared on ordinary supplied-map entry. Reentering with F729 clear permits the two
zone scripts again even if F728 survives; the next old-man talk uses F728. F729 set
suppresses both zones regardless of temporary flags. F77 affects population selection,
not these gates. A supplied F77-set/F729-clear state may run a zone with no 129 mapping;
it must not be treated as safely skipped or as a normal campaign state. No flag
implication is invented for arbitrary supplied states.

Entity 128 is bounded context: `Map62_EntityEvent0` checks F732 first. Set selects
3641 (`0E39`), lamenting the unavailable trip, with no temporary write. Otherwise F258
set selects 3639–3640 (`0E37..0E38`), a farewell and message for the brother. F258 clear
selects 3624–3626 (`0E28..0E2A`), describing the grandfather's Sky Orb study, ancestral
flight and the Nazca Ship's last known location. Only after those texts does F729 set
cause F258 to be set. F729 clear repeats that exposition on every eligible interaction;
F729 set still gives it once per ordinary entry before the farewell. A supplied F258
set can give the farewell even with F729 clear. No later F732 producer or travel claim
is admitted by these text alternatives.

## Four short programs and service boundaries

**Confirmed:** the inventoried pinned `scripts.asm` has 61 physical lines and four
programs, rather than the planning estimate of about 90 lines. The retained corpus
and direct source agree on **45 records: 41 opcode commands and four terminators**,
with 23 text requests and no sleep records. Inline movement/action payloads are
checked in the source, not counted again as map-script commands.

| Program / source lines | Records / texts | Ordered content |
| --- | --- | --- |
| `cs_5C876`, 4–28 (`0x5C876`) | 16: 15 commands + end; 6 texts, 3613–3618 (`0E1D..0E22`) | Cursor; wait for 128's jump; 128 asks about Petro, faces Down, asks who arrived; 129 asks whether Petro returned. Camera `(1,5)`; 129 demands the Sky Orb, moves Down 3, speaks, moves Down 2, gets inline speed `(16,16)` then idle, moves Down 1. Camera `(1,10)`; 129 recognizes that these visitors are not Petro; end. Caller then sets F256. |
| `cs_5C8D4`, 29–40 (`0x5C8D4`) | 10: 9 commands + end; 5 texts, 3619–3623 (`0E23..0E27`) | Old man explains his family's ancient stone, turns Right, describes its theft by his grandson, turns Down, says the grandson went to Grans to operate an ancient vehicle and expresses despair. Wait for 129 to move Up 6; end. Caller sets F728. |
| `cs_5C8FE`, 41–47 (`0x5C8FE`) | 5: 4 commands + end; 2 texts, 3627–3628 (`0E2B..0E2C`) | Old man offers help and invites conversation; wait for `ALLY_BOWIE` to move Up 1; end. Caller sets F257. |
| `cs_5C914`, 48–61 (`0x5C914`) | 14: 13 commands + end; 10 texts, 3629–3638 (`0E2D..0E36`) | Old man explains finding the Sky Orb and usable Grans tunnel; Peter asks about reaching Grans; old man invites the party, narration says the leader decides to accompany him, and old man asks for help recovering the Orb. Record 12 `followEntity 129,FOLLOWER_B,2`; record 13 `setF 77`; record 14 end. Caller then sets F729. |

In the final program records 5–6 are Peter's two text calls; record 10 is
`nextSingleText $FF,ENTITY_NONE`, the narrator; the other seven texts use 129.
There is no yes/no prompt, conditional jump, choice consumer, warp, join, force-AI
write, population reload or map-block edit in these four programs. The invitation
wording is not a decline branch. Text IDs assume ordinary non-skipped cursor behavior.
The text debug-skip path can omit display/cursor advance; it does not create a choice.

`mapscriptengine_1.asm:csc32_setCameraDestInTiles` sets view target to -1, requests
the destination and waits for scrolling. Text commands use the shared portrait,
speech, scrolling and text-window services; `nextSingleText` additionally closes
portrait/text and sleeps ten frames, even though there is no explicit sleep record.
`setActscriptWait`, `customActscriptWait` and `entityActionsWait` install actions and
poll for `eas_Idle`. Their presence proves waits in source, not natural collision-free
completion. `cs_5C876` requests total Down 6 and `cs_5C8D4` later requests Up 6 for
129, but neither is an absolute position assignment. If the first talk occurs before
the greeting, its relative movement starts from the actual current location.

## Current following versus the F77 rebuild

**Confirmed:** `sf2enums.asm` defines `FOLLOWER_B=31`, `FOLLOWER_A=30` and
`ALLY_PETER=7`. These are distinct lookup identities. At record 12 of `cs_5C914`,
`sf2cutscenemacros.asm:followEntity` encodes three word operands: `$0081,$001F,$0002`.
`mapscriptengine_1.asm:csc2C_followEntity` (`0x46E58`) first probes the **high byte**
of the first word through `move.b (a6),d0`, without advancing A6. Thus
`AdjustScriptPointerByCharacterAliveStatus` checks character 0/Bowie's HP here,
not 129's or Peter's. On zero HP it skips the six operand bytes and returns to the
interpreter without `AddFollower`; the subsequent F77 and caller F729 writes still
occur if execution otherwise completes. The
[existing relationship-handler observation](../../world/common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)
already covers this high-byte probe in a controlled seam; it is not an East Shrine
story observation.

With that guard permitting execution, the handler resolves 129 and 31 separately
through `GetEntityAddressFromCharacter`, then doubles selector 2 to read offset pair
`(-24,0)` from `table_FollowerPositions`. `AddFollower` in
`entityfunctions_2.asm` installs a copied 42-byte `eas_Follower1` block on the resolved
129 entity, patches target and offsets at `0x1E/0x20/0x22`, advances walking allocation,
and appends its physical index to `EXPLORATION_ENTITIES` unless already listed.
The duplicate test happens after replacing the action block. It does not choose the
list tail; that is the different `addNewFollower` consumer used by May.

`data/scripting/entity/followers.asm` supplies these relevant mappings on ordinary
Map62 population:

| Supplied flag state | Character 31 target at the follow command, absent intervening mapping writes |
| --- | --- |
| F67 only among follower flags | Peter maps as 7, normally physical slot 1. No row writes character 31, so its cleared lookup byte remains zero. The resolver returns physical slot 0, main/Bowie. This is a zero-mapping consequence, not a scripted Bowie target or an explicit fallback branch. |
| F74 enabled | Its Astral row writes character 31. Other F74 rows also declare king/minister under different identities. |
| F68 enabled | Its Oddler row writes character 31 after the F74 Astral row. |
| F70 enabled | Its Astral row writes character 31 after both earlier rows. If several are enabled, all their physical entities may be declared, but the last write wins this lookup. |
| None of F74/F68/F70, with other followers or a transient May | The other rows do not establish character 31; neither Peter's presence nor the current list tail substitutes for it. Actual mapping remains the required input. |

The resolver masks the character index, adjusts high-bit NPC identities by 96, reads
the lookup byte and computes the physical address without a missing-entity guard.
An absent 129 likewise resolves zero; a forced call can therefore install following
on main, including a self-target if 31 is also absent. This is a supplied-state
consequence, not proof of natural reach or author intent. The high-byte HP test does
not validate the target, old man, Peter, mapping freshness or follower capacity.
There is no source basis for replacing the target with Peter or declaring a remake fix.

`eas_Follower1` consumes the patched physical target through `esc03_follow`, uses its
current position/facing/destination and offset transform, then waits/loops. This is
an asynchronous relation with distance/layout branches, not instant placement or a
guaranteed visible spacing. `followEntity` itself does not wait for following to
finish. On a successful normal-population append, 129 remains the old man's lookup
identity in the current map but is now excluded by ordinary follower interaction
selection. The caller's F729-repeat text is therefore not evidence that talking to
him immediately again is naturally possible. F77 changes setup selection without
rebuilding or remapping this current 129.

On later ordinary Map62 entry, `ClearEntities` resets mappings and walking allocation;
the follower list is rebuilt and terminated. The F77 row declares old-man sprite
under **30**, while F77-selected fixed population has only young man 128. There is
no fixed 129 to preserve and no F728/F729-based substitution. F77 alone among follower
flags gives old man physical 1 with `eas_Follower1` targeting physical 0; with only
F67 and F77, Peter is physical 1 and old man physical 2 gets `eas_Follower2`, targeting
physical 1. These follow-action pointers are assigned by selected-row ordinal from
`followersfunctions_2.asm:pt_eas_Followers`, not by character 31. Additional flags
change slots/action selection; do not generalize these two bounded cases into an
arbitrary-count capacity or target guarantee. No full follower-table capacity audit
is added here. If F83 is also enabled, its later king row overwrites character 30's
lookup after the F77 old-man row; row identity does not guarantee the final alias.
Character 30 has no explicit Map62 entity-event row and is also a
current follower; it must not be treated as event 129.

F77/F728/F729 lie in the game-flag save domain; the transient follower list
(`0xFFAF22`), walking pointer (`0xFFB1A0`) and action storage (`0xFF5600`) lie outside
the logical SaveGame/LoadGame span `0xFFE800..0xFFF7AF` owned by
[common scripting](../../world/common-scripting.md). This distinguishes flag persistence from
the current relation. Actual cross-save presentation and natural reentry remain Unknown.

## Final caller, continuation and verification boundary

**Confirmed:** after final script return, `Map62_EntityEvent1` writes F729; the event
dispatcher restores prior actor facing, closes windows and reenables updates. Pending
zero at `explorationfunctions_2.asm:WaitForEvent` restores camera target physical 0
and calls `j_SetControlledEntityActScript`. This is the first conditional exploration
control boundary, not a wait for the old man's ongoing follow loop. The two zone
callers instead run through `RunMapSetupZoneEvent`, whose tail closes windows, waits
a VInt and waits for physical 0 to stop. Those tails do not prove every actor/service
finished or erase a newly written pending request.

`data/maps/entries/map62/6-warp-events.asm` has one south exit: wildcard X, Y19,
no scroll, Map70, `(43,41)`, Down. It has no F77/F728/F729 condition. Actual matching
still needs the warp attribute/request path. Stop here: no return trip, further
admission, Grans, tunnel, Battle11 or later shrine scene is claimed.

| Relation | Existing evidence and direct review | Discriminating limits |
| --- | --- | --- |
| Battle10 outcomes/return | `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`; selected row 10 and shared loop/wrapper/main/egress source. | F510 clear/set, leader-only death versus zero enemies, defeat precedence, actual first member/egress map, immediate versus later admission. Routing summary is not a substitute for direct row targets. |
| Warp/setup/init | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json` body 71; request and interaction owners. | Layout/reach, mutable pending, F77 setup with independent F729, actual population and first input. |
| Caller/short programs | `map-events/entity-programs.json` orders 610–612 and `zone-programs.json` 112–114; retained map-script programs named above; text hex keys and nested source actions. | Reentry temporary reset, conversations without zones, zones without F728, F729 repeat branch versus activation, F732 precedence, skipped text and service waits. |
| Following/lifetime | `map-script-engine-static-v1.json` `entityGestureRelationshipMotionCommandFacts`, `h3/map-entity-gesture-relationship-motion-v1.json`; direct macro/handler/lookup/follower/save-domain review. | High-byte Bowie-HP guard, absent 129/31, F67 with dead/missing Peter, other/duplicate mappings, current follow versus F77 rebuild. No new H3 execution or story observation. |

Static fixture names above are under `tests/fixtures/h2/`; the explicit `h3/` path
is under `tests/fixtures/`. Load `. ./local/private-inputs.ps1` in the owning checkout;
select the registered read-only `$upstream` and verify its pin/clean status. Run the
existing [retained payload/cutscene comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select the four `entryLabel` values above and compare each command's `sourceLine`,
macro and operands with `scripts.asm`; read the nested actions directly. Inspect the
six named event-program records, init body 71, selected warp/setup/battle rows and
named handler/list consumers. Resolve dialogue from four-digit hexadecimal keys
in `data/scripting/text/gamescript.txt`. This needs no new extraction or transcript.

**Unknown:** natural traversal/collision, actual caller/actor/mapping/HP/party/follower
state, service and pending-request completion, visible following, arbitrary follower
capacity, save/reentry presentation and continuous playability. Existing fixtures and
controlled handler observations do not prove this scene composition. No inseparable
new producer was needed within the declared endpoint; broader storyline work remains
outside scope.

Record direct checks, completed failures, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI in the
Issue/PR handoff. No normal/full/engine/H3/H4/Godot suite is selected solely for prose.
Leave a clean pushed Draft PR, Project Review / Verify and a successful authorized
completion callback; independent main-gate owns acceptance and merge. No cleanup or
next-slice selection is authorized here.
