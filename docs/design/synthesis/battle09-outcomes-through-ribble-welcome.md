# Battle09 Outcomes through Ribble's Welcome

Battle09's victory returns the party to the overworld at a position derived from the battle. A
separate connection leads into Ribble, where a suspicious young man and the mayor challenge the
newcomers. Peter explains their arrival after the disaster, identifies himself as a phoenix, and
the village's response changes. **Confirmed static:** the selected scripts replace the residents,
change four door blocks and restore Peter's current following relation. **Inferred:** this is the
narrative continuation of the encounter named `BATTLE_TO_RIBBLE`. **Unknown:** a natural continuous
journey, successful actor staging and the visible completion of these changes.

Read this after [New Granseal departure through Battle09 admission](new-granseal-departure-through-battle09.md).
The accepted [research owner](../../research/battle09-outcomes-through-ribble-welcome.md) supplies
the outcome, entry, dialogue and caller order. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#759](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/759).
This is a content/design guide over accepted evidence. It stops after the mayor caller and the
first conditional restored-control boundary; May's recruitment and onward travel require successor
evidence. Fnnn denotes flag nnn; battle IDs use zero-based source table rows.

## 1. Resolving the Battle Returns to Exploration

**Confirmed static:** the battle loop counts placed, living combatants after actions and after-turn
effects. Bowie's death forces the counted ally total to zero, and defeat is tested before victory.
Battle09 requires remaining counted allies and no counted enemies. Killing enemy slot 128 alone
does not establish victory. Its leader-presence row is zero, so the end wrapper does not append all
remaining enemies to the death list.

| Outcome | State change and return |
| --- | --- |
| Victory | Heal living/immortal allies, then derive map/X/Y/facing from current map, the first battle-party member after `UpdateForce`, the battle-area origin and that member's mapped entity. Call the after wrapper, clear F409 and set F509; return with D4=1. |
| Defeat | Present the defeat message, restore Bowie's HP to maximum, halve gold by unsigned right shift and return the egress tuple with D4=-1. F509 is not newly set. Battle4's special branch does not apply. |
| Egress / Angel Wing | Consume spell MP or the item, invoke the escape services and return the egress tuple with D4=0. This path does not apply defeat's HP/gold changes or victory's completion write. |

Battle09's area origin is `(0,0)` on Map69. Victory therefore uses the chosen member's battle
coordinates and facing on that overworld map; the member is not hardcoded as Bowie and the return
is not a fixed town entrance. Dead non-immortal allies are not revived by the victory heal.
Defeat/escape instead use the carried `EGRESS_MAP` and its savepoint row, or `(1,1)`, Up if no row
matches. This segment establishes no particular last savepoint. On first-battle defeat/escape,
F409 remains as supplied; the shared unlock helper clears it only if F509 was already set.

The defeated wrapper dispatches the empty row-9 script only with Bowie alive, slot 128 dead and
F509 clear. The after wrapper also has an empty row-9 target, gated by F509. Their empty content
does not write a Ribble destination. An interpreter terminator can still wait on view scrolling
when a dialogue window exists and clear scroll speed, so an empty script is not an instantaneous
service-completion claim. The shared after tail runs even when its script is skipped: row 9 calls
`JoinForce` for Bowie, setting F0, updating force and conditionally F32 if capacity permits.
It neither recruits May nor changes Peter's membership.

After any returned battle, `alt_MainLoopEntry` calls `SwitchMap` and enters exploration without
another immediate `CheckBattle`. A carried F417 therefore does not intercept that immediate
Map69 victory return. Ordinary supplied-map exploration clears pending, rebuilds entities/sprites,
clears temporary flags 256–383, heals living/immortal allies and loads the map before init. Map69
has no replacement, its default fixed list is empty and its init returns; supplied followers can
still be present. Pending zero and ordinary player type 0 permit the next control handoff after
services return. The [battle lifecycle](../../research/battle-loop.md) and
[exploration contract](../contracts/exploration-control-flow.md) retain the wider boundaries.

## 2. Ribble Is a Separate Destination

**Confirmed static connection:** Map69's exact warp row `(44,7)` requests Map8, Ribble, at
`(15,30)`, Up, without scrolling. Walking must encounter layout attribute `$1000`, match the row
and preserve the complete type-1 request until its no-scroll consumer returns to `MainLoop`.
A connection row does not establish an accessible path from the variable victory position.

F509-set Map69 default zones stop producing the Battle09 reload, but specific X23/24/25 zones
still call the Battle17 random helper. The default does not clear a carried F409. At a later
Map69 admission, an unlocked row 9 still precedes row 17; even F409/F509 set clears the unlock
while returning 9 in that same call. The
[predecessor](new-granseal-departure-through-battle09.md#5-ordered-battle-selection-determines-admission)
and [random-battle owner](../../research/map-event-random-battle-state.md) preserve these possible
interruptions. Completion is not proof of an uninterrupted walk to Ribble.

Map8 has neither a flag-switched replacement nor a battle-map row. The surviving selected request
therefore reaches ordinary entry at the supplied town position: rebuild actors, clear temporary
flags including F258, load layout/flag copies and run init before reading pending/control state.
Init requests neither welcome scene nor a new warp. Neither the warp nor either welcome handler
checks F509, so Battle09 victory is not an encoded gate for an arbitrary supplied Ribble visit.
Actual access and surviving request lineage remain **Unknown**.

## 3. Entering the Village Selects Residents and Followers

**Confirmed static:** F708 selects the changed Map8 setup on each setup lookup. Default and F708
setups share zone, description, item and init targets, but have different entity lists and
entity-event targets. The `_518` entity-event filename is not an F518 setup condition.

| Setup / supplied state | Population or init effect |
| --- | --- |
| Default, F708 clear | Two fixed NPC rows: old man 128 at `(14,7)`, Down, and young man 129 at `(13,26)`, Right. Dialogue identifies them as mayor and accuser. |
| F708 set | Fourteen source rows: thirteen ordinary NPC rows plus May. Relevant positions include the old man at `(37,8)`, wandering young man near `(14,23)` and wandering May near `(18,27)`. These are list rows, not a guaranteed visible-actor or physical-slot count. |
| Flag-selected followers | Entity initialization adds supplied followers before the fixed list; F67 selects Peter. Other follower flags remain inputs. Map8 does not use the overworld caravan prefix selected by F65. |
| F8 set at ordinary init | Call `MoveEntityOutOfMap` for May, writing `(0x7000,0x7000)` through her current mapping. This does not join her, set F8 or establish a present actor; default setup has no authored May row. |

Character 128/129 are lookup identities. Followers consume physical slots first, ordinary NPCs
receive event identities from 128 upward, and existing ally mappings can cause ally rows to be
skipped. Bowie occupies physical slot 0. Actor names and source lists do not prove that the
script receives a valid cast.

Peter's preceding audience supplies F7 membership, an AI-control bit and persistent follower flag
F67; active-party F39 remains conditional. Recruitment, active battle status, AI control,
persistent map re-instantiation and a currently installed follow action must stay distinct.
The [audience guide](peter-castle-audience-through-town-return.md) and
[party/roster contract](../contracts/party-roster-state.md) own those distinctions.

## 4. Suspicion and Reconciliation Have Independent Gates

**Confirmed static:** ordinary movement needs zone attribute `$1400` before the selected row can
produce a type-6 event. The supplied row coordinates are not evidence of natural activation.

| Welcome zone | Gate and caller state after script return |
| --- | --- |
| `(15,28)` or `(16,28)` | Return if F708 or F258 is set. Otherwise run `cs_563B2`, then set F703 and F258. |
| Mayor zone `(14,8)` | Return if F708 is set. Otherwise run `cs_56422`, then set F708. No F703, F258 or F709 prerequisite. |
| Default | Return. |

The first scene has the young man approach, suspect the newcomers and call the mayor. Its normal
text stream is 1291–1293. The script waits for the named right/left/up movements, changes facing
and speed, and finally places him at `(15,7)`, Down. That placement does not prove earlier paths
were traversable. F703 is written only after interpreter return and was already used by Map14's
plank/deck init; this handler never tests it. An existing F703 neither suppresses the accusation
nor establishes an earlier Ribble visit.

F258 suppresses a repeat accusation within the current entry. Ordinary reentry clears it, allowing
the scene again while F708 remains clear. The mayor scene is independently available to its local
gate even when the accusation never ran. The dialogue's plausible order is **Inferred narrative
flow**, not an enforced prerequisite chain or proof of valid actors on the alternate route.

## 5. Peter Explains the Arrival and Changes the Response

**Confirmed static dialogue content:** the mayor scene begins by positioning Peter at `(14,10)`,
Up, disabling Bowie/Peter auto-facing and adjusting the young man's speed. Five pauses of 30
precede five passes of Bowie retreating Down and the mayor bumping/moving Down. Peter receives
non-waiting Down actions in passes 2–5. The mayor's movement is waited; those Bowie/Peter launches
are not individually joined. Peter's initial ordinary `setDest` does wait for movement.

The mayor challenges the party and the young man reports soldiers, supporting the accusation in
text 1294–1303. The young man's approach sequence is waited. Peter then objects and challenges
their assumptions (1304–1307), moves around the mayor's reply (1308), explains the disaster-driven
arrival (1309–1312) and identifies himself as a phoenix (1314). The mayor and accuser change their
response across 1313 and 1315–1320. The authored exchange offers no explicit choice or refusal
branch. **Inferred player meaning:** Peter's explanation reconciles the misunderstanding and
introduces the newcomers to the village; it is not a demonstrated branching persuasion mechanic.

The script initializes actors between phases and explicitly changes their facing. The mayor's
Up movement is non-waiting while the young man's is waited. Their paired surprise jumps likewise
wait on the young man, not the mayor. Named waits do not prove that every actor action has finished.
Action/sequence waits depend on the mapped actor reaching `eas_Idle`; a lookup is not a universal
presence or dead-actor check. Missing Peter/NPC mappings, supplied actor state and dead allies
remain material staging limits.

Dialogue uses authored `{W1}`/`{W2}` controls, line breaks and named substitutions. Those controls
are not measured delays. `nextText` leaves the shared lifecycle open; `nextSingleText` closes it
and sleeps 10 after display. Normal display advances the cursor; debug text-skip bypasses display,
cursor increments and sleep records. This guide describes the normal authored stream, without
claiming its natural timing or reproducing the dialogue transcript.

## 6. The Changed Village Has Several Separate State Effects

**Confirmed static order:** after the reconciliation, the script fades out, loads `ce_55FBE`,
initializes Bowie, copies four blocks, sets F709, calls `j_csub_40F2`, installs Peter→Bowie follow,
sleeps 50, fades in and terminates. Only then does the mayor caller set F708.

The entity reload uses an explicit pointer, not the F708 setup selector. `ce_55FBE` contains
main `(14,13)`, Up, followed in storage by `s1_entities_708.asm`. `csc42_loadMapEntities` reads the
three main words and rebuilds from the following address, including supplied flag-selected
followers and the fourteen-row population, while normal caller F708 is still clear. It brackets
entity/mapsprite rebuilding with display/interrupt and VInt controls. It does not perform a new
warp, `LoadMap`, temporary-flag reset or map init. In particular, it does not rerun F8-dependent
May hiding. An F8-set supplied state therefore does not guarantee hidden May immediately after
this reload. Later ordinary entry uses the F708-selected setup and runs that init again.

The four immediate `setBlocks` copies use source `(32,2)` for three 1×1 edits at `(25,12)`,
`(22,24)` and `(7,25)`, then source `(32,3)` for a 2×1 edit at `(6,15)`. They change working map
blocks and set both view-plane update bits. F709's four flag-event rows reconstruct the same edits
on a later map load. F708 selects residents/events; F709 reconstructs door changes. Either can be
supplied without the other. The mayor-door `(14,9)` step-table copy is separate from these four
edits. Working-layout writes and queued refresh do not prove visible open doors or passable collision.

`j_csub_40F2` restores a saved roof rectangle only if its busy word is nonzero. It clears that
state, reads the saved destination/dimensions and copies the saved words back into working layout,
then sends `MUSIC_NOTHING` and sets plane-update bit 0. An inactive cache returns without that work.
The mayor-door roof row at `(14,9)` uses a 7×8 rectangle at `(13,33)`, but this route does not prove
that it was naturally activated or that the saved cache belongs to it. A different overlapping
saved rectangle could affect preceding edits. This is conditional roof restoration, not the
door-unlock mechanism, an entity reload or a guaranteed full redraw/music change. The
[map-block lifecycle owner](../../research/common-scripting.md#confirmed-map-block-copy-showhide-lifecycle)
retains the bounded helper evidence.

The final follow command resolves the current Peter/Bowie mappings and installs a follow action
with selector 2, pixel offset `(-24,0)`. Its ally-HP guard can skip dead Peter. It does not set F67,
join Peter, set F39 or change AI control; F67 governs his later map re-instantiation. Those guards
must not be generalized to every earlier actor command.

After the interpreter returns, caller F708 is set. The zone dispatcher closes portrait/text,
waits one VInt and physical entity 0's movement, then returns to exploration. It neither joins all
other actions nor unconditionally clears newly pending work. Neither selected scene explicitly
requests a new event. At the next `WaitForEvent`, pending zero follows main and installs ordinary
character control for carried `PLAYER_TYPE=0`; nonzero pending dispatches first. This is the first
conditional restored-control boundary, dependent on invoked services returning, not observed
responsive input or complete fades/follower motion. Later ordinary F708/F709 reentry reconstructs
residents and door copies, clears F258 and runs F8 init, while both welcome gates remain suppressed.

## Evidence Review and Remaining Limits

The accepted research binds USA retail and pinned SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`. Its scene-specific rows, text and storage adjacency
supply the composition; tracked fixtures supply narrower executable seams. Direct review preserved
their schema/consumer limits and found no required research or contract correction. In particular,
source-site command indices are not the research's one-based full script-record indices, and the
public engine fixture is not a complete dialogue/choreography transcript.

| Reviewed relation | Executable/contract owner | Boundary retained |
| --- | --- | --- |
| Battle resolution and empty wrappers | [sf2-battle-loop-static-v1](../../../tests/fixtures/h2/battle-loop-static-v1.json), `expected.lifecycleFacts`; [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), `expected.cutsceneFacts`; [cutscene contract](../contracts/battle-cutscene-routing.md). | Shared count/gate/join seams; row-9 composition and variable return come from accepted research, without a played encounter. |
| Population selection and May init | [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-entities-static-v1](../../../tests/fixtures/h2/map-entities-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json), `expected.primarySourceBodies[12]`. | F708 selection, allocation/mapping and F8→hide call; no present actor or recruitment guarantee. |
| Independent welcome handlers | [sf2-map-events-static-v1](../../../tests/fixtures/h2/map-events-static-v1.json) and [zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json), `expected.zoneTargetPrograms[145..147]`. | Exact F708/F258 checks and script-return flag order; no layout reach or mandatory accusation-first order. |
| Explicit reload, blocks and follow | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `expected.entityPopulationCommandFacts`, `mapBlockMutationCommandFacts`, `entityGestureRelationshipMotionCommandFacts`; [map/exploration](../contracts/map-exploration.md) and [party/roster](../contracts/party-roster-state.md). | `cs_56422` source sites and distinct handler/operand facts; accepted research owns reload-before-F708, storage adjacency and no-init composition. |
| Saved roof restoration | [sf2-map-block-copy-lifecycle-runtime-v1](../../../tests/fixtures/h3/map-block-copy-lifecycle-v1.json) and the common-scripting owner. | Controlled active/inactive helper seams do not prove the mayor's natural roof cache or visible doors. |
| Request and first control read | [sf2-map-event-request-consumption-static-v1](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `eventRequestConsumption.consumerContexts.waitForEvent` / `processMapEvent`; [request owner](../../research/map-event-request-consumption.md) and exploration contract. | Pending poll/pre-dispatch clear and consumer lifetime, without composed actor/service completion. |

The [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[schema](../../../schemas/h2-map-init-static-fixture.schema.json),
[map-script engine verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[fixture schema](../../../schemas/h2/map-script-engine-fixture.schema.json), and
[request-consumption verifier](../../../src/sf2tool/h2/map_event_request_consumption.py) and
[schema](../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json) own the
bounded structures. Their owning commands are `uv run sf2 h2` followed by `map-init`, `map-setup`,
`map-entities`, `map-events`, `map-script-engine`, `battle-loop`, `battle-cutscenes` or
`map-event-request-consumption`. **These extraction/runtime rails were not rerun for this design
slice.** The [research reproduction boundary](../../research/battle09-outcomes-through-ribble-welcome.md#coverage-discriminators-and-reproduction)
retains exact source/text anchors and prior comparisons; optional derived payloads are neither
required nor generated/copied here.

**Unknown:** natural victory-position→warp→welcome reach, collision and zone attributes; reachable
flag/follower/party history; supplied, missing or dead actors and unjoined actions; roof-cache identity
and overlap; intact pending state and all movement/load/dialogue/fade/audio service completions;
visible village changes, responsive control and save persistence. These limits do not create a new
acquisition queue. May recruitment, other NPC/services/treasure, shrine/raft and onward travel remain
outside this guide. It completes neither campaign chronology nor epic #514, changes no remake
capability or engine/product choice and leaves Chinese synchronization deferred under the roadmap.
