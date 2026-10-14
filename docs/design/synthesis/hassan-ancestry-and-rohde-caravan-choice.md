# Hassan Ancestry Conversation and Rohde's Caravan Choice

Hassan offers two independent conversations. A priest asks about the group's origins and tells
Peter a story about ancestors who followed Max. Separately, Rohde recognizes the Achilles Sword
and proposes accompanying the group to obtain the Ancient Caravan. Refusing that proposal allows
another talk-based choice; accepting requests following and sets future story state. It does not
recruit a force member, consume the sword or make the Caravan operational.

**Confirmed:** the accepted [research owner](../../research/hassan-ancestry-and-rohde-caravan-choice.md)
provides the static admission, complete dialogue/action graph and immediate caller/consumer joins
explained here. **Inferred:** the exchanges present an ancestry story and a prospective vehicle guide.
**Unknown:** natural admission, actual supplied state/input, completed services and presentation,
usable control, historical truth of the speakers' claims and durable persistence. Fnnn means flag nnn;
coordinates are map tiles, record indices are zero-based, and dialogue below is paraphrased.

Read [Battle16 outcomes through Hassan entry](battle16-outcomes-through-hassan-entry.md) for town
population and transport limits. This guide does not require that outcome or either conversation
first. It stops at the conversations' conditional caller/control return, before sword acquisition,
Rick, town departure, Taros combat or Caravan use. It adds no remake capability or natural playthrough.

For the separate item-custody question, read
[Ribble's tree opening and the Achilles Sword](ribble-tree-opening-and-achilles-sword.md).
It explains the selected Panel/Sword chest capacity,
Panel use and conditional shrine/Ribble return, without requiring those events before every Rohde
conversation or expanding this guide into acquisition. Chest/opening flags alone do not prove a
held Sword.

For the separate departure and encounter boundary, read
[Hassan departure, Battle17 entry and conditional returns](hassan-departure-and-battle17-outcomes.md).
It explains ordered Battle9/17 admission, walking/raft requests and conditional outcome returns.
Rohde's F418 is independent of Battle17's F417/F467/F517; this link adds no conversation prerequisite.

## Three Ways to Enter the Conversations

**Confirmed population:** both Map9 setup variants use the same zone, entity-event and item-event
tables. F518 selects different populations, rather than a different conversation handler. Their
third non-ally row is priest 130 at `(7,58)`, distinct from church-service priest 133. The default
priest faces Up with `eas_InitSlow`; the alternate faces Left with `eas_Init`.
Both supply Rohde 11 at `(16,59)`, Down, unless a current follower mapping already supplied him.
Peter has no fixed town row: F67's follower declaration supplies him. F67-clear fresh population
can leave Peter's lookup at zero, which aliases physical 0 rather than safely omitting his actions.

F84 can supply Rohde as a follower. Hassan's F65 init can move the mapped Rohde off-map without
invalidating that mapping. Ordinary activation excludes physical actors in `EXPLORATION_ENTITIES`.
Town entry, Rohde's presence in a table, or an acceptance flag therefore does not prove that the
current player can talk to him. The following predicates consume the supplied state, without
repairing population or enforcing a coherent earlier history.

### Crossing a Priest Zone

The three exact zone rows `(4,57)`, `(4,58)` and `(4,59)` share `Map9_ZoneEvent0` at `0x56866`.
With F726 set, it returns. With F726 clear, it runs the priest scene and sets F726 **after script
return**. That write suppresses all three rows together. There is no sword, HP, membership,
F725 or F727 test; Rohde's event is not a prerequisite.

The walking producer must actually encounter the working-layout zone marker and submit a type 6
request using destination-plus-movement-offset coordinates. The consumer clears pending, idles
physical 0 and dispatches the supplied coordinates. Merely standing on a listed tile is not a
receipt that the producer ran. Interrupted dialogue/actions can leave F726 clear and partial actor
effects, so another eligible call can request the scene again.

### Talking to Rohde

Ordinary C interaction first idles and waits physical 0 and view scrolling. Activation uses the
current view target, facing, layout and nearby physical actors, including the counter-extension
rule. The selected actor must reverse-map to logical 11 to select Rohde's row. Aliases, follower-list
exclusion, an absent actor or off-map placement can prevent that selection.

The row's `RIGHT=0` operand is **action flags, not a required player direction**. Neither the
turn-before-call bit nor saved-facing-restore bit is set. The wrapper stores the supplied player
facing, selects speaker properties from the mapped actor, disables entity updates around the
handler, then closes windows and reactivates updates. The script macro activates updates before
executing its scene; this service ordering does not demonstrate live completion.

| Supplied talk state | Consequence if the called services return |
| --- | --- |
| F727 clear, no sword found | Rohde requests busy-dismissal text 1371, sets F798 and returns without recognition or a choice. Supplied F725 does not bypass this path. |
| F727 clear, sword found, F798 clear | Request text 1371, close portrait/text, Sleep 40, then run recognition and the initial choice; set F727 after script return. This preface does not set F798. |
| F727 clear, sword found, F798 set | Enter recognition and the initial choice immediately; set F727 after script return. |
| F727 set, F725 clear | No inventory search. Turn mapped Rohde toward the supplied player-facing direction and run the repeat choice. No new F727 write. |
| F727 set, F725 set | Return silently, without inventory search, facing helper or prompt. |

The initial search calls `GetItemInventoryLocation`, which updates the force and scans **joined
members, including reserves**, without an HP requirement. It searches each member's held slots
from last to first, masking item entries with `$7F` before comparing Achilles Sword `$3D`.
The handler tests only the not-found result. Bowie need not personally carry or equip the sword;
it is not a search of every possible storage location. No item is removed or equipped. The search
presumes the ordinary nonempty force/list invariant, without a malformed-empty-list repair.

The repeat helper's name, `GetRhodeFacing`, conceals a write: it resolves Rohde and sets his facing
to `(suppliedPlayerFacing+2)&3`. The zero row flags do not undo that turn or the refusal's explicit
Down facing. F727-set repeat talk can proceed even after the sword is no longer held.

### Selecting the Sword in Item → Use

The other entry begins in FieldMenu's current-force item screen: choose a member, slot and item,
then Use. Cancel never reaches the callback. Achilles Sword is absent from the ordinary field-usable
item list, so this selection takes the map-item-event branch.

The first item row requires **selected Achilles Sword `$3D` and physical 0's current `(16,58)`**.
It uses current position divided by 384 internal units per tile, not a forward tile or the selected
carrier's location. `NO_DIRECTION=$FF` accepts any supplied facing. This route does not search
inventory again, test F798, activate Rohde by proximity or independently require his presence.
Possession somewhere is insufficient; the menu selection and coordinate predicate must match.
Rohde's fixed row `(16,59)` is a different coordinate.

| Selected-item handler state | Caller/menu consequence if services return |
| --- | --- |
| F727 clear | Run initial recognition/choice without text 1371; after either completed answer set F727, then handled result d6=-1. FieldMenu exits. |
| F727 set, either F725 value | Suppress the scene, leaving wrapper d6=0. FieldMenu requests generic used-item text 73 and no-effect text 422, then returns to the item submenu. |

Wrong coordinates/item do not select this row; the shared unmatched return has the same zero-result
fallback. Suppressed item use is not a repeat Rohde choice and does not directly restore exploration.
The player still needs a subsequent menu exit. No selected handler removes, receives, gives or
equips the sword. Generic used-item wording is not evidence of consumption.

## The Priest's Story

**Confirmed complete scene:** `cs_568F6` at `0x568F6` has 44 retained records and six inline bodies.
The sequence requests ten texts 1372–1381, with priest 130 and Peter as the speakers. Its movement
and pauses are part of the scene, rather than new travel or roster facts.

| Records | Ordered action and attributed meaning |
| --- | --- |
| 0–9 | Set cursor 1372; Bowie faces Right; Sleep 20; wait for priest Left 1/Up 1, then face him Left. The priest asks where the group comes from (1372). Sleep 20; set Peter's destination `(4,58)` and face Right; Peter says the leader came from Grans (1373). |
| 10–18 | Wait for priest speed 48/48 then idle, then Left 1. The priest suggests a relationship (1374). Sleep 20; Peter faces Up; Sleep 20; Peter asks what he means (1375). Sleep 40; wait for priest `eas_InitSlow`. |
| 19–29 | Priest Down; Sleep 30; Left; Sleep 40; start his Right 1/Down 1 action without waiting. Sleep 20; wait for Bowie Right 1; Sleep 10; Bowie Down, Peter Right, priest Left. |
| 30–38 | The priest says ancestors followed a soldier Max to Parmecia (1376), lost his trail at the shore and split (1377), and one group returned to Grans via Devil's Tail (1378). Sleep 20; Peter asks what happened to Max (1379). Sleep 20; the priest speculates Max went to Grans and admits uncertainty (1380), then announces a nap (1381). Sleep 30. |
| 39–43 | Wait for Peter Up 1; Peter Right; Bowie Left; request Peter→Bowie following with selector 2; end. Only afterward does the zone caller set F726. |

The story belongs to the priest, including his own uncertainty. Peter's origin statement is also
attributed speech. This exchange confirms neither Bowie's lineage, Max's history nor an independently
verified Devil's Tail route. It writes no ancestry or force-membership flag. The requested Peter
following remains subject to the mapping/HP boundary below.

Priest operands are `$C0,130`; Peter uses `$0,ALLY_PETER`. Text 1376/1377/1380 use continuing
`nextText`; the other seven use `nextSingleText`. These preserve command/service differences,
not a newly observed portrait layout, text cadence, gesture or audible result.

## Recognition and the Initial Choice

**Confirmed complete initial graph:** `cs_569BC` at `0x569BC` contains the recognition region,
refusal `cs_56A70`, acceptance `cs_56A82` and common return `cs_56AD4` in one 50-record physical
program. Six inline bodies accompany it. Its 13 text sites are 1382–1391 and 1394–1396; the
two sites 1392–1393 belong to the separate repeat prefix described below. Together the Rohde
regions have 15 text sites across alternate branches; no one answer plays every text in physical order.

| Records | Ordered staging and attributed meaning |
| --- | --- |
| 0–6 | Cursor 1382; Peter destination `(15,57)`; Peter and Bowie Down. Rohde comments on age (1382), faces Up and identifies the Achilles Sword (1383). |
| 7–14 | Wait for Rohde speed 40/40→idle, Bowie speed 40/40→idle and Bowie auto-facing deactivation. Start Rohde Up 1 without waiting; Sleep 3; wait for Bowie Up 1. Rohde asks about adventure/a vehicle (1384) and claims the sword enables obtaining the Ancient Caravan (1385). |
| 15–23 | Peter destination `(15,58)`, Right; he rebukes Rohde's earlier dismissal (1386). Rohde Left, excuse (1387), then Up; he asks to accompany (1388), claims exclusive driving knowledge (1389) and asks whether the group will get the Caravan (1390). |
| 24–25 | Request Yes/No; set F89 selects acceptance, clear F89 selects the initial refusal staging. |
| 26–28, No/B only | Wait for Rohde speed 10/10→idle, wait for auto-facing deactivation, then wait for Down 1; fall into refusal. |
| 29–32, refusal | Cursor 1391; Rohde protests the missed opportunity (1391), faces Down and jumps to common return, bypassing acceptance. |

Rohde uses `$C0,ALLY_ROHDE`; Peter uses `$0,ALLY_PETER`. Text 1382/1384 and 1387–1390 use
`nextText`; 1383/1385/1386 and refusal text 1391 use `nextSingleText`. Peter's rebuke occurs even when
selected-item Use skipped text 1371, or when no earlier dismissal was observed. It does not prove that
such a dismissal naturally happened. Rohde's statements about the sword and driving are claims
within this conversation, not proof of vehicle acquisition or operation.

### Choosing Yes or No

The accepted [prompt owner](../../research/volcanon-return-through-bedoe-king-choice.md#caller-prompt-result-and-response-texts)
provides the actual input rule. Yes is initially selected. A nonzero entry input snapshot must
first release to zero. The loop tests Left, Right, B, C, A in that order: Left selects Yes, Right
selects No; neither confirms. B chooses No and exits; C/A confirm the selection. No input waits,
without timeout or automatic acceptance. B is refusal, not a third story outcome.

After prompt-window closure, d0=0 means Yes and d0=`$FFFF` means No/B. The script sets F89 for
zero, clears it for nonzero, then sleeps 10 before the flag branch. An old supplied F89 is not a
substitute for completing this prompt. Actual sampled inputs, release timing and visible selection
remain Unknown.

### Acceptance and the Common Ending

Acceptance records 33–46 set cursor 1394. Rohde approves (1394); narrator (1395) announces the decision
to take him. Wait for Rohde `eas_Init`; set Rohde priority `$FFFF` and Bowie priority 0; wait for
Rohde `eas_Jump`, then start a second jump without waiting. Request his departure exhortation (1396)
and set Bowie priority `$FFFF`. These three texts use `nextSingleText`; the narrator has
`$FF,ENTITY_NONE`, rather than a new speaking actor.

Then, in order, **set F725, set F84, setStoryFlag 18→F418**, and request Rohde 11 to follow the
**current logical FOLLOWER_B31** with selector 2. Acceptance falls into common records 47–49:
wait for Bowie `eas_Init`, request Peter 7→Bowie 0 following with selector 2, then end.
Refusal jumps directly to that same common region. Both answers therefore initialize Bowie and
request Peter following; only acceptance writes 725/84/418 and requests Rohde following.

There is no `JoinForce`, active-party or AI grant, healing, F65 write, transport-mode change,
Caravan creation, warp or departure request. F418 is a future battle-unlock write, not immediate
Battle18 admission or F518 completion. F725 acceptance, F84 follower population and force
membership remain different state channels. The narrator's statement is not a membership service.

Only **after the initial script returns** does the talk/item caller set F727; the item caller then
sets d6=-1. F727 records initial completion after either answer, not agreement to the proposal.
A refusal does not clear supplied 725/84/418, and partial acceptance can leave those flags written
before following, common completion or F727. The handlers do not repair that inconsistency.

## Returning to the Proposal

**Confirmed repeat graph:** the first six records of `cs_56AE6` reset cursor 1392, request Rohde's
recognition/question about a changed mind (1392–1393) via `nextText`, prompt, then jump to acceptance
on set F89 or refusal otherwise. It runs only from F727-set/F725-clear **talk**, without another
sword search. The physical program also contains Rick's scene, but both selected repeat branches
jump away before it; Rick is outside this guide.

| Choice path | Executed regions and immediate caller tail |
| --- | --- |
| Initial Yes | Recognition 0–25 → acceptance 33–46 → common 47–49 → caller F727; selected-item caller also d6=-1. |
| Initial No/B | Recognition 0–28 → refusal 29–32 → common 47–49 → caller F727; selected-item caller also d6=-1. |
| Repeat Yes | Six-record repeat prefix → acceptance → common → entity return; no new F727 write. |
| Repeat No/B | Repeat prefix → refusal → common → entity return; no initial speed 10/deactivate/Down 1 staging or new F727 write. |

Repeat Yes skips all initial recognition/staging and uses current actors. Repeat No still requests
1391 and faces Rohde Down, but does not newly initialize his speed or auto-facing state. Initial
No leaves the earlier speed 10/auto-facing-off state; common initializes Bowie only.
F727-set/F725-set talk is silent even if following is inconsistent. Conversely, supplied
F725-set/F727-clear still takes initial admission. Priest F726 never participates in these decisions.

## What Following and Action Completion Actually Mean

**Confirmed consumer limits:** ordinary `setFacing` uses the packed ally byte for an ally-HP
check, so facing commands for zero-HP Bowie/Peter/Rohde can skip; NPC 130 bypasses it. Action
installation, destination writes and priority writes do not share that guard. Action waits resolve
the current mapping and poll the mapped actor's idle pointer, not every actor's destination.
Missing/default-zero and explicitly invalid mappings are not interchangeable or repaired.
`setDest` scales these coordinates by 384, writes destination/travel and signed per-axis velocity 32,
and waits for the selected actor's position/destination equality with these nonnegative entity words.
It neither teleports nor resets the action pointer. Peter's destination waits and another actor's
idle wait do not form a group barrier; the priest's nonwaited movement and Rohde's Up 1/second jump
may remain unfinished. Sleep values and speeds here are source parameters, not measured presentation timing.

`eas_Init` resets movement/action presentation fields to its source defaults; `eas_InitSlow` uses
slower initialization. Neither heals. `eas_DeactivateAutoFacing` turns auto-facing off and jumps
to idle; adjacent source instructions do not extend that action. `eas_Jump` requests relative
`(0,-1)`, waits 5, `(0,1)`, waits 5, then `(0,0)`, turns auto-facing on and idles. Each request is
relative to current position, with no explicit destination-equality wait between them. A requested
jump is not proof of a completed two-tile round trip; following can replace the unfinished second jump.

The priority command writes a mapped physical actor's sprite-link ordering byte: zero→0,
nonzero→1. The sprite-link builder places nonzero entries before zero entries. Acceptance sets
Rohde 1, Bowie 0, then Bowie 1; it restores no saved prior value. This is not battle turn priority
or a VDP tile-priority bit. Actual overlap/rendering remains Unknown.

All three follow sites encode follower words 7 or 11 whose **high byte is zero**. The handler reads
that high byte as its HP operand, so **Bowie's HP controls every selected following request**.
Zero Bowie HP skips the six operand bytes; earlier flags/actions can still run. Live Bowie admits
mapping resolution without checking Peter/Rohde/target health or presence.

`AddFollower` replaces the mapped physical follower's action with a patched 42-byte follow action,
records the mapped target and selector 2's `(-24,0)` pixel offsets, advances allocation and appends
the physical follower to `EXPLORATION_ENTITIES` only if absent. Existing entries are not reordered
or removed. The movement consumer derives later destinations from target state and layout;
installation is not immediate placement or a settled-following receipt.

FOLLOWER_B31 is a logical lookup, **not physical 31 or automatically Peter/Oddler**. It can name
Oddler under F68, Astral under F74/F70, another supplied mapping, or default-zero physical 0 when
absent; later declarations can overwrite it. These scenes do not create 31 or set F68. The intended
Rohde→Oddler connection is conditional on that current lookup naming Oddler. Peter→Bowie is a
separate request; F84 alone establishes no complete chain.

A later fresh population consumes F84→Rohde as the normal follower table's last declaration and
can suppress the fixed Rohde duplicate. It rebuilds slots, mappings and lists. Flag-based follower
action restoration instead assigns sequential slots without rebuilding the list or resolving
identities. Neither routine is invoked by these scripts. Future population can therefore differ
from the current installed chain, and Hassan's F65 init can again move mapped Rohde off-map.

## Conditional Return to Control

**Confirmed ordered return:** script end conditionally waits for view scrolling, clears scroll
speed and returns. The zone wrapper closes windows, waits a VInt and physical 0's current/destination
equality. The entity wrapper closes windows and reactivates updates, without saved-facing restoration
for this zero-flags row. The item wrapper closes windows and waits a VInt/physical 0 alignment before
FieldMenu interprets d6. Handled results exit the menu; zero results return to the item submenu.
`ProcessPlayerAction` reactivates view/entity updates after FieldMenu exits.

Exploration returns to `WaitForEvent`. Only pending zero retargets camera 0 and installs the
controlled-entity action; ordinary player type 0 selects walking. No scene normalizes supplied mode
or pending state. A stalled service/wait can prevent return, while dead Bowie can skip following
but leave completed acceptance flags/F727 if the remaining services return. These are conditional
source consequences, not evidence that natural play supplied such states or usable input resumed.

## Evidence Trace and Review Boundary

**Confirmed provenance:** accepted #828/PR830 research binds SF2DISASM master
`c834c652b6862bc5679fd7f69a38a7093206efc6` and the registered USA retail ROM. Its
[reproduction and coverage owner](../../research/hassan-ancestry-and-rohde-caravan-choice.md#reproduction-and-coverage-limits)
accounts for 100 selected records, twelve inline bodies, 25 script text sites 1372–1396 and two caller
sites 1371. Physical corpus extent is not an executed route. This guide consumes those accepted
joins, without original-source investigation, payload regeneration or runtime acquisition.

| Explained boundary | Existing owner and executable trace | What the trace does not establish |
| --- | --- | --- |
| Priest/talk/item callers | [map exploration](../contracts/map-exploration.md); `sf2-map-events-static-v1` shards [zone programs](../../../tests/fixtures/h2/map-events/zone-programs.json), [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), [item programs](../../../tests/fixtures/h2/map-events/item-programs.json): programs 148/671/78, all 5/25/6 operations | The selected rows, activation, inventory-service body and wrappers are accepted research joins; operation accounting is not natural admission. |
| Complete scenes, prompt and flags | [dialogue](../contracts/dialogue-system.md), [input](../contracts/input-system.md); `sf2-map-script-engine-static-v1` in [map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-common-menus-static-v1` in [menu fixture](../../../tests/fixtures/h2/common-menus-static-v1.json) | Tracked command-fact projections preserve sites/operands; the full programCorpus/inline bodies and text attribution are retained research selections. Menu/handler matrices are not these choices' supplied-input or enabled-service observations. |
| Inventory search and item result | [item-transaction research](../../research/map-event-item-transactions.md); `sf2-map-event-item-transactions-static-v1`, `sf2-common-stats-static-v1` in matching [item](../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)/[stats](../../../tests/fixtures/h2/common-stats-static-v1.json) fixtures | Aggregate caller facts retain Map9 talk's location call and FieldMenu/d6 seam; they do not independently execute Map9 item use, consumption or storage/persistence. |
| Following, population and return | [roster](../contracts/party-roster-state.md), [exploration flow](../contracts/exploration-control-flow.md), [accepted following](../../research/polca-oddler-choice-and-following.md#current-chain-versus-future-follower-population); `sf2-map-init-static-v1`, `sf2-map-event-request-consumption-static-v1` in [init](../../../tests/fixtures/h2/map-init-static-v1.json)/[request](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json) fixtures | Shared command/service, population and request facts do not prove the current chain, all-actor completion, responsive input or save/resume. |

Owning reproduction commands include `uv run sf2 h2 map-events`, `map-script-engine`, `common-menus`,
`common-stats`, `map-init`, `map-event-item-transactions` and `map-event-request-consumption`.
Controlled reproduction loads the owning worktree's ignored private-input configuration in the same
process. It is not required solely to review prose.

The [map-event fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) composes and validates
the [index](../../../schemas/h2/map-events-fixture-index.schema.json), section shards and
[target-operation schema](../../../schemas/h2/map-events-target-program.schema.json); the
[map-event verifier](../../../src/sf2tool/h2/map_events.py) owns their source/ROM comparison.
The [map-script verifier](../../../src/sf2tool/h2/map_script_engine.py) and
[schema composition](../../../src/sf2tool/h2/map_script_schema.py) compare command-fact projections,
not a public full-scene playthrough. The [item-transaction verifier](../../../src/sf2tool/h2/map_event_item_transactions.py)
and [schema](../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json) own static
caller choreography. This synthesis review selects the three complete caller programs, selected
dialogue/story/action sites and shared menu/item/init/request facts, and inspects their schemas and
consumers. No mismatch requiring a Layer A correction was found; aggregate equality does not close
natural-state or presentation gaps.

The [#828 final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160569868)
and [independent PR830 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/830#issuecomment-6160579144)
retain passed comparisons, actual final CI, resolved PowerShell/path/truncated-read inspection failures
and the [original full handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828#issuecomment-6160524404).
Linked PR826/822/818 comparison, corrected macro/destination/inspection and rejected-command histories
keep their completed statuses through that research owner; this slice does not replay or replace them.

**Unknown:** natural zone/talk/selected-item reach; actual inventory, flags, HP, mappings, lists,
layout, pending/mode and input/release/answer; action/window/update/service completion, text/gestures,
rendering/overlap/audio, usable control, lore and save/resume persistence. These remain deferred under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). A supplied flag is not a history receipt;
an Unknown is not automatic permission to acquire runtime evidence or expand into the next story beat.
