# Battle26 outcomes and Higins recruitment

Battle26's accepted continuation has three distinct results: the battle can end,
an action can transfer the Black Ring, and a first-victory scene can recruit
Higins. **Confirmed:** each has its own predicates and writes. Leader cleanup
does not award the ring, and joining does not restore Higins's HP. The rescue
conversation asks for help in Pacalon, but the battle returns a position chosen
before that conversation; it does not transport the party there.

This continues [activation and the first turn handoff](battle26-activation-and-first-turn-dispatch.md)
without supplying a combat transcript between that first call and an outcome.
It consumes accepted [Battle26 outcome research](../../../../research/chapters/northern-travel-mitula/battle26-outcomes-and-higins-recruitment.md)
under [#1018](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1018).
**Confirmed** below means the named static source/consumer boundary unless an
existing controlled observation is explicitly identified. **Inferred:** the
cloud's departure represents the threatened possession being averted.
**Unknown:** natural Battle26 reach, actual combat/reward/state, completed
services, rendered rescue, usable exploration and persistence. This guide
establishes no remake capability or later town admission.

## Ending the battle

**Confirmed:** after an individual turn returns, BattleLoop optionally runs its
debug kill-all branch, then invokes the defeated hook, processes the current
death list and counts remaining combatants. It tests zero allies first and zero
enemies second. If both are exhausted, the result is defeat. If neither result
applies, it reads the actual current actor, runs after-turn effects, processes
deaths and repeats those count tests. Only a nonterminal result advances the
current-turn byte offset by2.

The status pass is a separate opportunity to end the battle. Its effect helper
clears the death-list length on entry, skips HP0 actors and, for lethal poison,
writes that current actor as the new list's first member. It does not replay
the action's earlier deaths. There is no second defeated-hook invocation in
this pass: a leader killed there is processed and counted before another leader
append could occur.

Battle26's leader cleanup concerns **Lizardman128**, the first initial enemy,
rather than Geshp or the battle-neutral Higins159. With Bowie HP nonzero and128
HP zero, the defeated wrapper checks completion F526. Clear526 runs an empty
script; held526 skips that script. Both reach the shared leader-presence tail.
Battle26's nonzero leader byte then scans128–159 and appends every nonzero-HP
slot to the supplied death list, adding a trailing255 and incrementing length.
It does not test position, configured row, activation or neutrality, clear HP,
clear the old list, or deduplicate it. Fresh neutral159 has HP0; altered HP
can make it participate. The similarly guarded position listener has no
Battle26 override row, so it performs no relocation for this battle.

Processing this list spins/sounds the listed actors, updates ally defeats or
enemy kills credited to BATTLESCENE_FIRST_ALLY, clears their X/Y to -1 and
status to 0, recomputes stats and parks mapped entities at `$7000,$7000`.
It neither zeros their HP nor clears the list length. An interrupted service
or supplied stale/duplicate list is not repaired by the description of cleanup.

The later count has different predicates: across 30 ally and 32 enemy slots,
nonnegative X.b and nonzero HP.w count. Y, joined/active membership, AI, regions
and neutral bits do not. Bowie HP0 forces the ally count to0. Thus ordinary
zero-enemy victory can happen without leader death, while the leader tail can
remove still-living enemies from the count by clearing X. Neither a zero count
nor an HP value proves all presentation/death services completed. An F88
continuation uses the supplied slots, lists and flags, not a reconstructed
fresh fifteen-enemy formation.

The shared [battle lifecycle](../../../../research/battle/battle-loop.md) and
[cutscene routing](../../../contracts/battle/battle-cutscene-routing.md) own the general
machinery; the selected research owns these exact Battle26 branches.

## The Black Ring belongs to a separate action transaction

**Confirmed:** selected drop-table row10 is `(battle26, target130, item24,
dropFlag10)`, at `$BE7A..$BE7E`. Source enemy ordinal2 becomes combatant130:
the initial Dark Bishop whose initialized inventory includes Black Ring.
Its role is independent of leader128. Successful initialization strips item
entry flags; the later drop rule matches the masked item index, so those
flags do not determine whether it qualifies.

The action engine calls `battlesceneScript_DropEnemyItem` after each admitted
target effect and after admitted second/counter attacks, using their actual
actor/target. Each target starts with targetDies clear; the damage helper sets
it when its HP decrease/read returns zero. This joins the conditional caller
to the drop helper, without proving any particular attack took place.

The selected Black Ring branch orders these operations:

1. Require an allied actor, enemy target and nonzero targetDies. Match current
   battle/target, then find item index24 in the target's four inventory slots.
   Equipment, broken, AI-use and drop entry bits are masked for that search.
   A missing item does not become a reward because the battle ended.
2. Take the unconditional drop arm: Black Ring is outside the three weapon
   cases that use RNG(32). Neither drop RNG nor an upgrade/completion test is
   used here; the upgrade instructions after unconditional branches are
   unreachable.
3. Test-and-set bit10 of ENEMY_ITEM_DROPPED_FLAGS: byte1/bit2, mask4, at
   `$FFF7A5`. An already-set bit returns. This is neither joined F10 nor F526.
   The write precedes removal and receipt.
4. Remove the target slot, compact later items, fill the last with NOTHING
   and recompute target stats. Then test the acting ally's HP and capacity.
5. A living actor with an empty slot receives plain index24 in its first
   empty slot. Only successful AddItem appends the target-drop and actor-receipt
   message commands. Their later rendering is a separate service. The helper
   does not equip it, award gold, choose Bowie or search other party inventories.
6. A dead or full actor takes rare-item fallback. Black Ring is
   RING/RARE/CURSED/BREAKABLE, so its high-nibble deals count at DEALS_ITEMS+12
   (`$FFF610`) increments up to15. An existing15 stays15, with item and drop
   flag already consumed. This arm emits neither successful-receipt message.

Leader-list cleanup calls no drop transaction:130 can disappear from the count
with no ring awarded. After-turn death and victory are not substitute callers
either. Holding drop bit10 prevents another transfer even with respawn, a new
copy of the ring or a different actor. Interruption after setting the bit can
suppress a retry before removal; interruption after removal can leave no
delivered ring. There is no rollback or demonstrated save durability here.
The ring's BLAZE2 use and equip-class rules do not change this lookup.

The [combat contract](../../../contracts/battle/combat-resolution.md) retains general drop
policy. Its existing controlled observations replace routine inputs at a
Battle01 action seam; they are not observations of Battle26 or Black Ring.

## Victory prepares the return before it stages the rescue

**Confirmed:** victory first heals all eligible ally slots0–29, independently
of joined/active flags. Nonzero HP qualifies; Peter7 and Lemon28 also qualify
at HP0. Eligible slots receive maximum HP/MP, status masked to
STUN/POISON/CURSE and stat recomputation. Other dead allies stay dead. A living,
unjoined ally19 can therefore be healed before recruitment, while dead Higins19
is not revived. The separate battle-neutral159 supplies no stats to ally19.

Next, PARAM2 receives CURRENT_MAP. A force rebuild selects the first byte of
the ascending active-member list without checking length or HP. That member's
X/Y plus battle-area origin are added as bytes into PARAM3/4; its mapped
entity supplies facing for PARAM5. PARAM1 becomes0, while MAP_EVENT_TYPE is
not written. With ordinary Map72/Battle26 origin `(28,0)`, the seed is
`Map72, ((x+28) mod256, y mod256), actual facing`. It need not select Bowie
or a living/onscreen member. An empty active list exposes its retained first
byte; changed map/crop/positions/mapping change the result.

Only then does the after wrapper test F526. Clear526 dispatches
`abcs_battle26`; held526 skips the rescue script. Both reach a shared
JoinForce0 tail. Joined F19 is not the scene gate: a supplied already-joined
Higins can still receive the scene and join presentation when526 is clear.
Held526 does not replay rescue, join19 or recreate his temporary follower.

Keep the three battle flags separate. The earlier request helper sets
unlock F426; ordered CheckBattle can clear it when completion526 is already
held. Before-scene dispatch checks intro F476 without setting it; the fresh
start wrapper later sets476 before its empty start script. F88 continuation
bypasses fresh before/start setup. None of those facts proves a completed
introduction or recruitment. Completion526 is written only after the after
wrapper returns.

## Who is present in the rescue scene

**Confirmed:** the scene fades toward black, loads Map72 with camera operands
`13,11` scaled by3, and installs list `ce_4BE4A`. Loading the map does not
clear CURRENT_BATTLE26, rewrite PARAM2–5, or run the ExplorationLoop's complete
reset/population/healing/setup-init lifecycle. Camera coordinates are not
the player's return coordinates.

Actor initialization clears entity lookup/state and walking scratch, creates
eligible followers from physical1, processes the fixed list, then declares
Bowie at physical0. The battle's neutral159/B31 mapping is replaced; it is
not the rescue's logical ally19.

| Fixed list record | Tile position / facing | Actual allocation |
| --- | --- | --- |
| Bowie0 | 19,16 / Up | physical0, initially Idle |
| Peter7 | 17,14 / Down | allocate after followers only if his lookup is absent; otherwise retain his follower actor |
| Higins19 | 18,14 / Down | next physical slot; no persistent Higins follower row |
| EVIL_CLOUD106 | 63,63 / Up | NPC logical128, lookup offset32; next physical slot |

InitializeMapEntities scales positions by384. Declaration installs sprite,
action, facing, position/destination and entity number/layer, clearing travel
and velocity; fixed rows use Init. Actor presence does not set membership.
With N admitted followers and P=1 only when Peter needs the fixed row,
Higins is physical N+P+1 and Cloud N+P+2. For coherent F65/F67/F70 only,
Caravan A is physical1, Peter2, follower B3, Higins4 and Cloud5.

Other supplied flags change that cast. On Map72, F65 enables Caravan A30;
the remaining follower rows, in source order, are66/Sarah,67/Peter,72/Kazin,
73/Slade,74/B/Astral,66/Chester,68/B/Oddler,75/Luke,69/C/Fairy,70/B/Astral,
74/A/King,71/Lemon,74/D/Minister,76/Zynk,77/A/Old Man,82/Frayja,83/A/King,
83/D/Minister,84/Rohde. Aliases A/B/C/D mean logical30/31/158/159, not those
physical slots. Duplicate alias rows allocate and overwrite lookup; only
fixed allies deduplicate. Five follower action pointers and finite unguarded
storage do not promise safety with arbitrary mass-enabled flags. F64 with
RAFT_MAP72 can separately declare raft physical31; it is not logical B31.

Consequently the conversation's **B** speaker is FOLLOWER_B31, normally
Astral under the named coherent premise, but potentially Oddler or another
supplied alias. Missing A/B lookup0 aliases physical0; hidden255/corrupt
lookups are not safe no-ops. Their setPos can move the main actor. BattleLoop
cleared PLAYER_TYPE; an altered mode2 uses its distinct population tail.
Class determines ally mapsprites; retained CURRENT_BATTLE26 bypasses the
dead-blue-flame branch.

SetPos, setFacing and startEntity skip actual dead allies0–29. Follower
aliases/NPCs bypass that HP guard; action installation, headshake, nod,
priority, text and addNewFollower do not inherit it. A dead/missing actor
therefore does not produce one uniform scene skip. Higins priority0 clears
his physical priority byte; Cloud's nonzero operand stores1. The sprite-link
consumer emits nonzero entries before zero entries. This is a draw-order
input, not a signed layer, combat priority or proof of rendered overlap.

## The rescue and the request to accompany the force

**Confirmed:** the uninterrupted script has 72 top-level commands, three inline
actions and five packed moves. The following covers its entire sequence;
dialogue descriptions paraphrase actual literals2637–2657 rather than treating
source comments as the spoken text. No choice or destination warp is encoded.

| Commands | Connected scene and dialogue |
| --- | --- |
| 0–12 | Set cursor2637; load Map72/list; wait for Bowie Init, Higins motion-off/left-orientation and Peter Init; place Peter17,14/Right, wait B Init, place B18,16/Up and A19,17/Up; set Higins/Cloud priority; fade in. |
| 13–19 | Peter moves Down2 and faces Right. Peter asks about Higins; B warns of imminent evil possession (2637–2638). Flash B, flash the screen and move the camera to13,9. |
| 20–29 | Slide Cloud out, place it18,13/Down, slide in; Peter faces Up. Cloud moves Up3, installs speed64/64, starts circling asynchronously; after csWait200, replace that action with waited Die and return camera13,11. |
| 30–40 | Higins motion-off/up-orientation, face Down/headshake; he asks what happened (2639), moves Up1, jumps and describes almost being possessed (2640), then moves Down2 and offers thanks (2641). B asks about recovery and his wounded journey (2642–2643). |
| 41–51 | Higins explains Pacalon's war, North Parmecia's insufficient strength and his southward search for allies (2644–2646). Peter moves Up1/Right and says he need go no farther (2647). Higins faces Left and asks whether they come from the south; B identifies Granseal (2648–2649). Higins faces Down and requests help in Pacalon (2650). |
| 52–61 | Higins nods; Peter names their obligation to Tristan (2651). Facing Left, Higins pleads, accepts a later opportunity, then volunteers to accompany them (2652–2654). Execute join19. B notes his injury (2655); Higins faces Down and reports feeling better (2656). |
| 62–71 | Start Higins RotateRight asynchronously, csWait40, replace it with waited Jump, face Down, Jump again, wait Init and startEntity19. Higins again asks to go to Pacalon (2657), then addNewFollower19 and terminate. |

The dialogue's requests express intention, not actual travel. **Inferred:**
the staged cloud departure and Higins's account depict rescue from possession.
The source establishes instructions and words, not that the cloud completed
a visible circuit or that any effect cured an illness/status.

Literal boundaries matter even when summarizing the exchange:

| Actual ID | Speaker | Command form | Final literal control |
| --- | --- | --- | --- |
| 2637 | Peter | Single | W1 |
| 2638 | B | Single | W1 |
| 2639 | Higins | Single | W2 |
| 2640 | Higins | Single | W1 |
| 2641 | Higins | Single | W1 |
| 2642 | B | Text | W2 |
| 2643 | B | Single | W1 |
| 2644 | Higins | Text | W2 |
| 2645 | Higins | Text | W2 |
| 2646 | Higins | Single | W1 |
| 2647 | Peter | Single | W1 |
| 2648 | Higins | Single | W1 |
| 2649 | B | Single | W1 |
| 2650 | Higins | Single | W1 |
| 2651 | Peter | Single | W1 |
| 2652 | Higins | Text | W2 |
| 2653 | Higins | Text | W2 |
| 2654 | Higins | Single | W1 |
| 2655 | B | Single | W1 |
| 2656 | Higins | Single | W1 |
| 2657 | Higins | Single | W1 |

Single/Text mean nextSingleText/nextText; a Single command does not imply W1.
All 21 actual source keys are `$0A4D..$0A61`. Higins's portrait control is $C0;
Peter/B use $00. The name operand in 2655 resolves actual ally19's name, not
physical actor19 or a fixed English spelling. Portrait/bleep uses mapped
sprite/class: Peter base/promoted and B's actual alias can differ; normal
HIGINS_PROMO selects Higins/bleep6. An existing portrait window can affect
portrait opening.

Normal text paths wait for scrolling, display and increment the cursor.
Single additionally closes windows and sleeps10; Text keeps windows. The
uninterrupted cursor ends2658, independently of fixed join message446.
Interpreter entry clears its skip latch; debug plus P2 Start can set it.
Skipped Single/Text consumes operands without display, increment, closure or
sleep, while scene actions/effects/joins still run. Negative csWait words
skip Sleep. A midscene skip leaves a prefix cursor and does not skip the
join's music/fixed-text/input-wait services.

## Actions can be replaced before their motion completes

**Confirmed:** waited action forms poll for the resolved physical actor's
action pointer to equal Idle. Asynchronous forms return after installation.
The three inline bodies end at `$8080`, distinct from the map script's
`$FFFF`. Packed moves generate relative-move/destination-wait pairs followed
by an Idle tail. Their five one-move programs occupy70 generated bytes from20
packed bytes. Movement uses actual position and signed tile deltas*384.
Peter's nominal path is17,14 →17,16, later17,15; Cloud's18,13 →18,10.
Higins's Up1 nominally reaches18,13; Down2 begins after a jump, so18,15
is not a guaranteed endpoint.

The circle action installs its own speeds/acceleration, overriding speed64.
It moves through three relative legs and branches before its entry label to
a fourth, then repeats without an Idle terminator. csWait200 does not join
that action; Die replaces whatever progress it reached. Die rotates through
four facings three times, requests death sound, changes to EFFECT1 and parks
at63,63 before Idle. It kills no combatant and deletes no actor:63*384 is
`$5E80`, below inactive-X `$7000`. Slide-out/in likewise changes crop/DMA
requests without deleting the actor; hidden storage can remain active.

Higins's motion-off changes an animation counter, and orientation is distinct
from facing. B's flash operand60 expands to16 iterations/64 explicit VInt
waits; screen flash30 makes four fading calls and palette duplication, not a
measured thirty-frame effect. Camera requests wait for scrolling. Headshake
and nod bracket sprite transformations with waits/counter writes; they do
not modify HP or status.

RotateRight loops through facing/update/wait instructions forever. csWait40
does not prove two rotations; the next Jump replaces it. Jump uses timed
relative movement requests and reaches Idle without a destination barrier.
Waited Jump therefore does not prove exact displacement or settled motion.
Final Init restores movement factors/speeds32, obstruction, auto-facing and
sprite settings, but neither HP/status nor animation counter. startEntity
writes counter0 only if Higins is alive. The later follower action replaces
his pointer again. Requested waits do not establish elapsed presentation,
completed DMA/audio/input services or a final physical pose.

## Joining, active membership and temporary following

**Confirmed:** command58's join19 clears speech SFX, waits for scrolling,
selects ordinary join music and calls JoinForce19. It reads actual class,
sets dialogue-name/class indices and displays fixed446's recruitment
template, then waits for fade/input, closes text and sleeps10. This is not
the operand128 Sarah/Chester special path. Membership writes precede these
presentation services; later dialogue/following requires them to return.

JoinForce sets joined F19, rebuilds ascending joined/active/reserve lists,
then sets active F51 only if the rebuilt active count is below12. A full
party evicts nobody; already-active stays active, and a reserve member can
become active if space exists. A supplied F51 can make a newly joined member
active during the rebuild. The later JoinBattleParty only sets its flag:
no second immediate rebuild synchronizes newly activated lists/counts.

No join here initializes HP/MP/class/level/items, removes status, clears AI
or repairs position. A dead Higins can join dead. His statement of recovery
does not supply a missing stat operation. Repeating a clear526 scene with
F19 already set still repeats its calls/presentation. The
[membership contract](../../../contracts/party/party-membership-state.md) preserves the
flag/rebuild chronology; the [roster contract](../../../contracts/party/party-roster-state.md)
keeps presentation and follower commands separate.

At command70, addNewFollower19 resolves the actual physical actor and scans
EXPLORATION_ENTITIES to255. It passes the last follower's physical ID
(0 if empty), offset -24 and 0 to AddFollower. That helper copies a 42-byte
Follower1 action into walking scratch, advances its pointer and patches
target/offsets before testing whether the actor is already listed. Duplicate
membership still consumes scratch and replaces the action; the last follower
can target itself. There is no HP, membership, active-count, capacity or safe
mapping guard. No persistent Higins follower flag/table row is added.
The installed relation supplies subsequent destinations/velocity; it is not
a teleport or a proved final following position.

After csc_end's possible scroll wait/speed clear, the shared after tail uses
table byte0 to call JoinForce0, on both scene and held526 paths. This sets
Bowie's F0, rebuilds lists including any reached Higins writes and conditionally
sets F32 below capacity. It is not a second Higins join. No scene/join/follower
command rewrites PARAM2–5 or repairs the earlier return seed.

## Completion, retreat and the returned handoff

**Confirmed:** only after the after wrapper returns does victory clear426 and
set526. It leaves476 as supplied, writes no destination-town flag, clears
d0–d3 words and copies PARAM2–5 bytes into them, returning d4=1. Upper longword
halves are not established by clr.w. Nominal victory therefore returns Map72
and the pre-scene member-derived coordinates/facing, not camera13,11,
scene Bowie19,16 or Pacalon. Any interrupted script/join/service/flag write
retains only its reached prefix, with no rollback or completed return claim.

| Alternative terminal route | Cost and return boundary |
| --- | --- |
| Defeat | Update unlock; clear dialogue name1; request sad music/text363/close; restore Bowie HP to maximum; halve unsigned gold; select egress position; d4=-1. Battle04's special tail does not apply. |
| Admitted Egress | Deduct selected spell MP cost; close windows, text274 and common sound/flash/close; update unlock, unwind action frame and select egress position; d4.w=0. |
| Admitted Angel Wing | Remove the selected actor item slot and recompute stats before text275; use the same escape tail/d4.w=0. Neither escape adds defeat's gold penalty/Bowie restoration. |

UpdateBattleUnlockedFlag preserves426 when526 is clear and clears426 when526
is held. Defeat/escape do not newly set526, change476 or run the after/join
scene. They do not revert earlier item/drop/party writes. Later exploration
healing lies outside this endpoint, so deducted MP/restored HP is not a
promise about eventual exploration values.

There is no Battle26-specific egress override. With F399 set by fresh or
resumed BattleLoop entry, GetSavepointForMap uses **EGRESS_MAP**, rather than
the victory seed or stored egress X/Y/facing. Ketto25 selects `(12,11)` Up;
New Granseal6 selects `(11,22)` Up; unmatched72 retains72 and defaults to
`(1,1)` Up. Other supplied selectors use their own rows/fallback. This does
not prove a church visit. Helper table writes are bytes; victory's cleared
word contract must not be imposed on every escape register.

With399/F64 held, raft reset selects72 `(43,48)` for Polca/Bedoe,
69 `(28,55)` for Hassan, otherwise69 `(42,15)`, including selectors6/25/72.
Clear64 preserves raft fields. A direct399-clear helper entry returns
initial Map3 `(56,3)` Down before raft reset; it is outside the ordinary
reached Battle26 premise. These writes establish neither boarding nor
exploration admission.

All returns reach MainLoop's alt_MainLoopEntry. Its next calls are SwitchMap
then ExplorationLoop, without a d4 sign test or intervening CheckBattle.
**This guide stops at that returned handoff.** Map setup/population, usable
control, subsequent movement, Pacalon/Tristan arrival, another battle and
save/load persistence remain outside it. Continue with
[Battle26 return through conditional Pacalon entry](battle26-return-through-pacalon-entry.md)
for the fresh rebuild, retained scrolling state and separate town/battle admission.

## Evidence review and retained results

The pre-synthesis review inspected complete accepted #1013, the narrower
Battle26 admission/initialization/activation owners, shared lifecycle/control,
action/drop, cutscene and party/follower contracts, and the following public
fixture payloads, schemas and actual consumers. The reproduction pin is
SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`; the USA ROM identity
and exact source paths/addresses remain in the research owner.

| Reviewed fixture / consumer | Accepted proof and stricter limit |
| --- | --- |
| `sf2-battle-loop-static-v1`; [battle_loop.py](../../../../../src/sf2tool/h2/battle_loop.py) | `_build_lifecycle_facts` and its fixture/schema cover roster/healing/death structure. Generic positive-HP wording does not replace the selected nonzero-HP instruction test; debug kill-all is separate from leader append. |
| `sf2-battle-control-static-v1`; [battle_control.py](../../../../../src/sf2tool/h2/battle_control.py) | `_build_control_facts` retains entry/round/victory/defeat order. Exact terminal precedence, status pass and byte/word seed/return joins come from accepted research. |
| `sf2-battle-cutscenes-static-v1`; [battle_cutscenes.py](../../../../../src/sf2tool/h2/battle_cutscenes.py) | `_cutscene_facts` covers intro/after/defeated wrappers. Completion gates the defeated script, not the shared append tail; generic relocation prose does not create a Battle26 table row. |
| `sf2-enemy-item-drops-v1`; [H2 enemy_drops.py](../../../../../src/sf2tool/h2/enemy_drops.py) | Source/ROM row comparison and consumer verification, with30-row aggregate public fixture. That aggregate alone does not identify row10 or its actual caller; selected26/130/24/10 joins remain with research. |
| `sf2-enemy-item-drop-behavior-v1`; [H3 enemy_drops.py](../../../../../src/sf2tool/h3/enemy_drops.py) | Ten controlled Battle01 entry cases and actual observer comparison, including prior flag, dead/full actor and deals14/15. No natural Battle26/Black Ring reward is observed. |
| `sf2-common-stats-static-v1`; [stats.py](../../../../../src/sf2tool/h2/stats.py) | Party/list and inventory/deals facts; numeric capacity, exact chronology and selected scene caller require their accepted source joins. Auto-activation summary does not imply an immediate second rebuild. |
| `sf2-map-script-engine-static-v1`; [map_script_engine.py](../../../../../src/sf2tool/h2/map_script_engine.py) | forceStateCommandFacts/activePartyCommandFacts, exact form/handler guards and source-identity joins. Public structure does not supply private abcs_battle26's whole program or actual literals. |
| `sf2-force-state-roster-death-runtime-v1` and `sf2-force-state-active-party-runtime-v1` | Existing14/nine-case handler matrices preserve scoped membership/list/follower behavior. Narrow saved-domain evidence is not natural Battle26 recruitment, capacity safety or durable scene following. |

Owning commands include `uv run sf2 h2 battle-loop`, `battle-control`,
`battle-cutscenes`, `enemy-drops`, `common-stats` and `map-script-engine`;
`uv run sf2 h3 enemy-drops`, `force-state-roster-death` and
`force-state-active-party` own their controlled observations.
They are reproduction routes, not newly executed source/runtime comparisons
in this design checkout. Broader owners' static-question queues and rendered/
capacity Unknowns do not override the accepted selected source branches.
No blocking evidence contradiction was found; the narrower claims above are
used instead of inherited count summaries. A contradiction requires research
correction, not a Layer B repair or new acquisition.

[#1013's final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1013#issuecomment-6182436816)
and [PR1016 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1016#issuecomment-6182452296)
retain its full source/program/literal/actor/transaction review, final
head12a6ab08/tree125e38de/baseee85e039, author0.143s and independent0.132s
checks, original0.129s/0.128s receipts, original CI38666515728 and rebased
CI38666866738 (scope16s/research33s). The unchanged18,285,979-byte pair's
accepted0.083s receipt is reused by provenance, without regeneration or a new
comparison. Whitespace/include/masked-table and publication-guard diagnostics,
predecessor failures and Unknowns remain in those handoffs.

[#1014's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1014#issuecomment-6182346394)
retains its schema/table/link checks, CI38666201758 (scope21s; other jobs
skipped), source/pair absence and completed inspection/tool diagnostics,
including its linked #1009/#1010 results and corrections. Source and retained
pair remain unavailable locally here. No private data is acquired/copied,
no fresh private comparison is claimed, and no completed suite is reset.

This slice's Issue handoff records direct full-prose/changed-state/diff,
five-path/navigation/link/anchor/UTF-8/private checks, schema checks,
git diff --check, configured clean-head planner and actual exact-head CI.
Documentation-only scope skips blanket normal/full/Python/.NET/Godot/H3/H4
and translation suites. **Unknown:** natural terminal/state/reward, all
supplied actor/flag/HP/item/map/parameter/mapping/capacity premises, completed
services, action progress, rendering, usable exploration and persistence.
These do not create a runtime queue or expand the accepted remake frontier.
