# Hassan ancestry conversation and Rohde's Caravan choice

Evidence date: 2026-10-13. **Confirmed:** three Hassan zone rows share a priest
conversation suppressed by F726. Rohde's separate talk and selected-item callers
can enter sword recognition and a Yes/No proposal. Initial completion sets F727
after either answer; acceptance first sets F725, F84 and battle-unlock F418 and
requests Rohde to follow the current logical FOLLOWER_B. None of these operations
acquires or consumes the sword, joins Rohde to the force, or unlocks the Caravan.
**Inferred:** the attributed exchange presents an ancestry story and a prospective
vehicle guide. **Unknown:** natural reach, actual state/input/services, historical
truth of the speakers' claims, usable control and durable persistence.

## Agreement and provenance

Standard rigor under [Issue #828](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/828)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Distinct admission paths, internal branches and actor lifetimes require explicit
boundaries. Consumed accepted base is `28e03ecc3644bbff08850e096ea7d1dce47c544b`;
the [Hassan entry owner](./battle16-outcomes-through-hassan-entry.md) supplies
conditional Map9 population/init/control, not a mandatory route to these events.
This is the sole new research owner, without a stacked or shared-path dependency.

Scope covers the priest scene, Rohde's initial/refusal/acceptance/common/repeat
regions, complete inline actions and text sites, and their immediate callers and
consumers through conditional control return. It excludes other town interactions,
church/shop services, Rick `cs_56B02`, sword acquisition, departure, Taros combat,
Caravan operation and natural traversal. No runtime acquisition, new parser,
fixture, comparison platform or generalized inventory/dialogue framework is added.

Reused owners are [common scripting](./common-scripting.md),
[Polca choice/following](./polca-oddler-choice-and-following.md),
[prompt input/return](./volcanon-return-through-bedoe-king-choice.md#caller-prompt-result-and-response-texts),
[map requests](./map-event-request-consumption.md),
[item caller transactions](./map-event-item-transactions.md) and
[membership](./common-stats.md). Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.

The unchanged retained pair is 18,285,979 bytes. Existing linear comparisons were
budgeted below 512 MiB and one minute, and passed in 0.084 seconds. Additional
work is bounded source/record selection and small prose/receipts, without corpus
growth or new game-material acquisition. Reassess before materially expanding
cross-system work. Fnnn means flag nnn; record positions below are zero-based.
Coordinates and speech are source operands and paraphrases, not runtime receipts.

## Population and three independent admission paths

**Confirmed:** `sf2enums.asm` resolves `MAP_HASSAN=9`, `ALLY_BOWIE=0`,
`ALLY_PETER=7`, `ALLY_ROHDE=11`, `FOLLOWER_B=31`, `ITEM_ACHILLES_SWORD=$3D`
and `NO_DIRECTION=$FF`. Both Map9 setup variants share zone/entity/item tables;
F518 selects their different populations, not a different choice handler.
In both `s1_entities.asm` and `s1_entities_518.asm`, the third non-ally row is
logical 130, a priest at `(7,58)`. The default faces Up with `eas_InitSlow`;
the alternate faces Left with `eas_Init`. Ally rows do not consume the NPC sequence.
This priest is distinct from the church-service priest logical 133.

Both populations supply fixed Rohde 11 at `(16,59)`, Down, unless a current follower
mapping already supplied that ally. Peter comes from F67's follower declaration;
there is no fixed Peter row here. Fresh F67-clear population can therefore leave
lookup 7 at zero. F84 can supply Rohde as a follower; F65 causes the shared init
to move mapped Rohde off-map without invalidating his mapping. These states are
not repaired by the dialogue callers. The predecessor owns transport and other
init cases; usable walking actors are not presumed from town entry.

### Priest zone request

**Confirmed:** `entityscriptengine_2.asm`'s walking movement path tests the
working-layout `$1400` zone code after masking with `$3C00`. It writes event 6
and X/Y words from destination plus movement offsets divided by 384. Merely
occupying a listed coordinate does not prove that producer ran. In
`code/gameflow/exploration/explorationfunctions_2.asm`, `ProcessMapEvent` clears
pending before type 6 applies the init action to physical 0 and passes parameter 1
and parameter 3 to `j_RunMapSetupZoneEvent`.

`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` scans four-byte rows to the first
match/default; 255 is an axis wildcard. `map09/mapsetups/s3_zoneevents.asm` has
exact rows `(4,57)`, `(4,58)`, `(4,59)`, all targeting `Map9_ZoneEvent0` at
`0x56866`, then a return-only default. The five-operation caller checks F726:
set returns; clear runs `cs_568F6`, then sets F726 only after script return.
There is no F725/F727, inventory, membership or HP admission predicate.
All three rows suppress together after that write; an interrupted scene can
leave partial actor effects with F726 clear and remain eligible for another call.

### Rohde talk request and inventory predicate

**Confirmed:** ordinary C interaction in `explorationvints.asm:ProcessPlayerAction`
first idles/waits physical 0 and waits for view scrolling. After debug/Caravan
alternatives, `GetActivatedEntity` uses the view target, facing and current
layout; it extends across `$2800` counter blocks, excludes the view target and
physical actors in `EXPLORATION_ENTITIES`, and selects the first qualifying nearby
actor. `battlefunctions_0.asm:GetEntityEventIndex` reverse-maps by scanning the
current lookup table. The selected physical actor must resolve to logical 11 to select
the Rohde row; aliases and supplied population can change that result.

`s2_entityevents.asm` row 0 is `ALLY_ROHDE, RIGHT, Map9_EntityEvent0`. The second
byte is consumed as action flags, **not a required player direction**: RIGHT=0
sets neither bit 0 (turn toward player before call) nor bit 1 (restore saved facing
after call). The wrapper saves the player's supplied facing byte in
`EVENT_RELATIVE_POSITION`, selects portrait/speech properties from the mapped
actor, disables entity updates around the handler, then closes windows and
reactivates updates. The `script` macro's Trap6 explicitly activates entity
updates before `ExecuteMapScript`; the scene's action waits are not intentionally
run with updates disabled. Actual service completion remains conditional.

`Map9_EntityEvent0` at `0x567E6` has 25 retained operations:

| Supplied state | Ordered handler result, assuming services return |
| --- | --- |
| F727 clear; sword search returns -1 | Request Rohde's busy refusal1371; set F798; return. No recognition or prompt, even if F725 was supplied set. |
| F727 clear; sword found; F798 clear | Request the same 1371, close portrait/text, Sleep 40, then run `cs_569BC`; set F727 after return. This path does **not** set F798. |
| F727 clear; sword found; F798 set | Run `cs_569BC` immediately; set F727 after return. |
| F727 set; F725 clear | Do not search inventory. Call `GetRhodeFacing(11)`, run `cs_56AE6`, return. No new F727 write. |
| F727 set; F725 set | Return without text, prompt, facing helper or inventory search. |

The search alias `j_GetItemInventoryLocation` (`0x81D0`) resolves to
`code/common/stats/itemstats.asm:GetItemInventoryLocation` (`0x9146`). It calls
`UpdateForce`, then scans `TARGETS_LIST` of **joined members**, including reserves,
in member order. `battleparty.asm:UpdateForce` builds that list from joined flags,
separately from active flags; it applies no HP requirement. Each member's held
slots are searched from the reported last slot down to zero, masking item entries
with `$7F` before comparing to `$3D`. A match returns carrier/slot; exhaustion
returns d0=d1=-1. Rohde's caller only tests d0 against -1 and neither removes nor
equips anything. This is not a search of all possible storage or a requirement
that Bowie personally carry, equip or explicitly present the sword. It presumes
the ordinary nonempty force/list invariant; it adds no malformed-empty-list repair.

`GetRhodeFacing` is a misleading name for a write: resolve logical 11 through
`j_GetEntityIndexForCombatant`, take `(EVENT_RELATIVE_POSITION+2)&3` and call
`UpdateEntityProperties` with that facing and -1 for unchanged optional fields.
It returns restored registers, not a queried facing result. The zero row flags
do not undo this turn on return, nor the repeat refusal's explicit Down facing.

### Selected-item request and result

**Confirmed:** `mainactions.asm:FieldMenu` Item → Use obtains a member, selected
slot and selected item from the current-force item screen. Cancel does not reach
the map callback. `IsItemUsableOnField` uses
`data/stats/items/usableoutsidebattleitems.asm`; Achilles Sword is absent, so it
takes the map-event branch rather than the ordinary consumable target branch.
`GetPlayerEntityPosition` supplies **physical 0's current** X/Y divided by 384 and
facing, not a forward tile or the selected carrier's location. The selected item
and slot become d4/d5 for `j_RunMapSetupItemEvent` (`0x44088`→`0x47586`).

The item wrapper masks d4 with `$7F`, initializes d6=0 and scans six-byte rows.
`s5_itemevents.asm` row 0 requires player coordinates `(16,58)` and selected item
`$3D`; `NO_DIRECTION=$FF` bypasses the facing comparison. It does not identify
Rohde by proximity, check his presence, require F798 or independently search
inventory. Possession somewhere alone is insufficient to reach this callback.
The ordinary menu must supply a selected item and the current coordinates must
match. Being at the fixed Rohde row `(16,59)` is not the same predicate.

`Map9_ItemEvent0` at `0x568B0` has six operations. F727 set branches directly to
its shared return, preserving wrapper d6=0, regardless of F725. F727 clear runs
`cs_569BC`, then sets F727 and sets d6=-1 before returning. Both completed answers
therefore report a handled item event. There is no F798 preface and no removal,
receive, give or equip service; the sword remains in inventory under these calls.

The wrapper closes portrait/text, waits one VInt and physical 0's current position
to equal its destination, and preserves d6 for FieldMenu. Nonzero d6 exits the
menu. Zero produces generic used-item text 73 and no-effect text 422, then returns
to the item submenu; it does **not** automatically become a repeat Rohde prompt
or return directly to exploration. These fallback requests are shared caller
behavior, not extra Rohde dialogue or proof of consumption. Later menu exit is
still required for player control.

## Complete program and branch graph

**Confirmed:** the owned script span is
`data/maps/entries/map09/mapsetups/scripts.asm:4–133`. The retained corpus groups
physical text through a terminator, not through each label. It contains two
complete selected programs and the first six records of a third:

| Entry / address | Source lines | Physical corpus positions | Selected records / inline bodies / text sites |
| --- | --- | --- | --- |
| `cs_568F6` / `0x568F6` | 4–62 | `cs_568F6` 0–43 | 44 / 6 / 10 |
| `cs_569BC` / `0x569BC` | 63–106 | `cs_569BC` 0–28 | 29 / 6 / 9 |
| Refusal `cs_56A70` / `0x56A70` | 107–110 | same program 29–32 | 4 / 0 / 1 |
| Acceptance `cs_56A82` / `0x56A82` | 111–124 | same program 33–46 | 14 / 0 / 3 |
| Common `cs_56AD4` / `0x56AD4` | 125–127 | same program 47–49 | 3 / 0 / 0 |
| Repeat `cs_56AE6` / `0x56AE6` | 128–133 | `cs_56AE6` 0–5 | 6 / 0 / 2 |

The third physical program has 35 records and also contains Rick `cs_56B02`.
Only its six-record repeat prefix is admitted here: both repeat branches jump
back to Rohde's other regions, so normal repeat execution never falls into Rick.
Do not equate the corpus's physical extent with the executed branch graph.
All 100 selected records, 12 inline bodies and 25 script text sites are accounted
for. Cursor writes are 1372,1382,1391,1394,1392 in physical order; script text IDs
are 1372–1396 (`055C..0574`), each at one site. The talk caller has two separate
sites for 1371 (`055B`). Source text comments match `gamescript.txt` keys; only
paraphrases are published. A physical inventory is not one linear playthrough.

### Priest scene

**Confirmed:** all following positions are in physical program `cs_568F6`:

| Records | Complete ordered work and attributed text |
| --- | --- |
| 0–9 | Cursor 1372; Bowie Right; Sleep 20; wait 130 Left 1/Up 1; 130 Left; priest asks origin1372; Sleep 20; Peter destination `(4,58)` and Right; Peter says leader came from Grans1373. |
| 10–18 | Wait 130 custom speed 48/48 then idle; wait 130 Left 1; priest suggests relation1374; Sleep 20; Peter Up; Sleep 20; Peter asks meaning1375; Sleep 40; wait 130 `eas_InitSlow`. |
| 19–29 |130 Down; Sleep 30; 130 Left; Sleep 40; start 130 Right 1/Down 1 without waiting; Sleep 20; wait Bowie Right 1; Sleep 10; Bowie Down, Peter Right,130 Left. |
| 30–38 | Priest says ancestors followed a soldier Max to Parmecia1376, lost his trail at the shore and split1377, and one group returned to Grans via Devil's Tail1378; Sleep 20; Peter asks Max's fate1379; Sleep 20; priest speculates Max went to Grans and admits uncertainty1380, then announces a nap1381; Sleep 30. |
| 39–43 | Wait Peter Up 1; Peter Right; Bowie Left; request Peter→Bowie selector 2; end. Only afterward does the zone caller set F726. |

Priest text operands are `$C0,130`; Peter uses `$0,ALLY_PETER`. IDs 1376/1377/1380
use `nextText`; the other seven use `nextSingleText`. The lineage, Max's travels
and the Grans/Devil's Tail route are attributed speech, including the priest's
own uncertainty, not independently confirmed history or a newly verified travel
route. Nothing in this scene supplies an ancestry flag or changes party membership.

### Rohde recognition, first refusal and repeat

**Confirmed:** initial-region positions below use `cs_569BC`'s indices:

| Records | Complete ordered work and attributed text |
| --- | --- |
| 0–6 | Cursor 1382; Peter destination `(15,57)`; Peter and Bowie Down; Rohde remarks on age1382; Rohde Up; he identifies the Achilles Sword1383. |
| 7–14 | Wait Rohde speed 40/40→idle; wait Bowie speed 40/40→idle; wait Bowie `eas_DeactivateAutoFacing`; start Rohde Up 1; Sleep 3; wait Bowie Up 1; Rohde asks about adventure/vehicle1384 and claims the sword enables obtaining the Ancient Caravan1385. |
| 15–23 | Peter destination `(15,58)` and Right; Peter rebukes the earlier dismissal1386; Rohde Left, excuse1387, then Up; requests to accompany1388, claims exclusive driving knowledge1389, asks whether the group will get the Caravan1390. |
| 24–25 | Prompt; F89 set jumps to acceptance 33. |
| 26–28 | On No/B, wait Rohde speed 10/10→idle, wait `eas_DeactivateAutoFacing`, wait Down 1, then fall into refusal 29. |
| 29–32 | Cursor 1391; Rohde protests the missed opportunity1391; faces Down; jump common 47, bypassing acceptance. |

Rohde uses `$C0,ALLY_ROHDE`; Peter uses `$0,ALLY_PETER`. Initial 1382/1384 and
1387–1390 use `nextText`; 1383/1385/1386 and refusal1391 use `nextSingleText`.
Peter's rebuke is present even when the item route skipped 1371 and even when no
earlier dismissal was observed. Dialogue alone must not establish that history.

Repeat `cs_56AE6` resets cursor 1392, requests Rohde's recognition/question about
a changed mind1392–1393 via `nextText`, prompts, and jumps to acceptance on F89
set or refusal otherwise. Repeat No skips the initial speed 10/deactivate/Down 1
sequence; it requests 1391, faces Down, then takes common. Repeat Yes skips all
recognition/staging; acceptance must operate on the caller's current actor state.
F727-set repeat eligibility does not recheck possession of the sword.

### Prompt, acceptance and common return

**Confirmed:** `mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow` calls the
accepted input-aware `j_YesNoPrompt`→`YesNoPrompt`, preserving A6. The prompt
initially selects Yes; a nonzero entry input snapshot requires release to zero.
Its choice loop tests Left, Right, B, C, A in that order. Left selects 0, Right
selects 255, neither confirms; B selects No and exits; C/A confirm. No input waits,
with no timeout or automatic acceptance. After window closure, returned d0 is 0
for Yes or `$FFFF` for No/B. csc11 sets F89 for zero, clears it for nonzero, then
Sleep 10. `csc0C_jumpIfFlagSet` takes the target only for set F89. A supplied old
F89 is not a substitute for prompt completion; B is refusal, not a third branch.

Acceptance records 33–46 reset cursor 1394, request Rohde's approval1394 and
narrator1395 announcing the decision to take him, then wait Rohde `eas_Init`.
Set Rohde priority `$FFFF`, Bowie priority 0; wait Rohde `eas_Jump`; start another
Rohde `eas_Jump` without waiting; request Rohde's departure exhortation1396;
set Bowie priority `$FFFF`. Then, in order, set F725, set F84, `setStoryFlag 18`
(400+18=F418), and request Rohde 11→FOLLOWER_B31 selector 2. The three texts use
`nextSingleText`; narrator1395 is `$FF,ENTITY_NONE`. Speech about joining is
not a `JoinForce` call, a heal, a grant of active membership or an item transaction.

Acceptance falls into common 47–49: wait Bowie `eas_Init`, request Peter 7→Bowie 0
selector 2, end. Refusal jumps directly there. Thus both answers restore Bowie's
init action and request Peter following, but only acceptance writes 725/84/418
and requests Rohde following. First-script caller F727 is written **after** common
and script return; item d6=-1 follows F727. Repeat has no new F727 write.

| Path | Executed graph and final caller writes |
| --- | --- |
| Initial Yes | BC0–25 → A82 → AD4 → caller F727; item caller also d6=-1. |
| Initial No/B | BC0–28 → A70 → AD4 → caller F727; item caller also d6=-1. No acceptance writes. |
| Repeat Yes | AE6 prefix → A82 → AD4 → entity return. |
| Repeat No/B | AE6 prefix → A70 → AD4 → entity return. |

## Immediate motion, priority and following consumers

**Confirmed:** `mapscriptengine_1.asm:csc23_setEntityFacing` uses the actual packed
ally byte for its HP check; zero-HP Bowie/Peter/Rohde facing commands can be skipped.
NPC 130 bypasses that ally test. By contrast `csc14`, `csc15` and `csc2D` action
installation, `csc29_setEntityDest` and `csc53_setPriority` have no such HP guard.
Each resolves the current logical mapping without a presence check. A missing
default-zero mapping can target physical 0; an invalidated 255 mapping is a different
invalid state. Neither is repaired by the script.

`setDest` scales tile coordinates by 384, writes destination/travel and signed
per-axis velocity32, and with these nonnegative entity words waits for that actor's
position/destination equality. It is not a teleport or an action-pointer reset.
Action waits instead poll the selected actor's pointer for `eas_Idle`; nonwaited
priest Right 1/Down 1 and Rohde Up 1/second jump may still be running when another
actor or text finishes. Later pointer writes can replace unfinished work.
No group-wide barrier or unconditional completed position is implied.

`data/scripting/entity/eas_actions.asm` owns all shared actions. `eas_Init` sets
speed 32, acceleration/deceleration off, collision settings, auto-facing on,
orientation Up, immersion off and size 24, refreshes the sprite and idles.
`eas_InitSlow` uses speed 16, factors 1 and -2 acceleration/deceleration with its
other initialization; neither heals. `eas_DeactivateAutoFacing` turns auto-facing
off then jumps to idle: later adjacent instructions are not part of that action.
Initial refusal leaves Rohde's selected speed 10/auto-facing-off state; common
initializes Bowie only. Repeat refusal does not supply a new Rohde initialization.

`eas_Jump` turns auto-facing off, requests relative `(0,-1)`, waits 5, requests
`(0,1)`, waits 5, requests `(0,0)`, turns auto-facing on and jumps idle. Macro
`ac_moveRel` is opcode 4: `esc04_moveToRelativeDest` scales by 384 relative to the
actor's current position, checks destination conflicts and enters the movement
consumer. These are timed action requests, not proof of a completed two-tile
round trip; it contains no explicit destination-equality wait between those
requests. Subsequent following may replace the second jump.

`setPriority` does not change battle turn priority or write a VDP tile priority
bit. `csc53` writes the mapped physical actor's `byte_FFAFB0` entry to 0 for a
zero word, otherwise 1. `entityscriptengine_1.asm:sub_4E24` builds sprite links
in two passes, nonzero entries before zero entries. Acceptance sets Rohde 1,
Bowie 0, then Bowie 1; it does not restore a saved prior value. Visible overlap
and completed sprite services remain Unknown.

The follow macro encodes three words. `csc2C_followEntity` reads its HP operand
from the **high byte of the follower word**, zero
for 7 and11. Consequently all three selected follow sites test **Bowie's** HP,
not the followed actor's HP or Rohde/Peter's. Bowie HP0 skips the six operand
bytes; earlier flags/actions need not be skipped. Live Bowie admits mapping
resolution even for a dead/missing other actor, without a target validity guard.

`entityfunctions_2.asm:AddFollower` installs a patched 42-byte action for the
mapped physical follower, records the mapped physical target and selector 2's
`(-24,0)` pixel offsets, advances the allocation and appends the follower to
`EXPLORATION_ENTITIES` only if absent. Existing list entries are not reordered
or removed. `esc03_follow` derives later destinations from the target's position,
destination/facing and layout; it is not an immediate placement or completed
movement receipt. Normal talk activation excludes actors in that physical list.

FOLLOWER_B31 can denote Oddler under F68, Astral under F74/F70, or another supplied
mapping. It is not automatically Peter, Rohde, or physical 31. Fresh default-zero
absence aliases physical 0; multiple declarations can overwrite the same lookup.
These scenes do not create31 or set F68. The intended Rohde→Oddler connection is
conditional on the current lookup actually naming Oddler. Peter→Bowie is a
separate requested connection; no complete chain is inferred merely from F84.

Future population is a separate operation: `table_Followers` has F84→Rohde 11
as its last declaration; Hassan uses the normal table even with F65 set.
`InitializeFollowerEntities` declares current enabled rows into fresh physical
slots/list/mappings, and existing ally mappings suppress fixed duplicates.
`InitializeFollowerActscripts` instead assigns sequential physical slots from
current flags without rebuilding that list or looking up identities. Neither
routine is called by these scripts. A later fresh population can differ from
the current AddFollower chain and is subject again to Hassan's F65 init removal.

## Partial state, suppression and control boundary

**Confirmed:** F726 does not gate Rohde, and F727/F725 do not gate the priest.
Neither interaction requires the other first. The caller's F798 write follows
the no-sword refusal; a sword-present preface or item presentation need not
set it. Its F727 write follows an initial script return, regardless of answer;
it does not establish acceptance or that the sword still exists. Supplied flags
alone prove neither this execution history nor completed presentation. F725 suppresses only
the F727-set talk branch; supplied F725-set/F727-clear state still takes initial
admission. Item F727-set suppression offers no repeat choice.

Interrupted acceptance can leave priority/action changes, F725, F84 or F418
written before following, common return or caller F727. Initial refusal does
not clear pre-existing725/84/418; repeat No also does not undo supplied flags.
F727-set/F725-clear talk can ask again without inventory, while F727-set/F725-set
talk is silent even with inconsistent following. Dead Bowie can prevent follow
installation while all acceptance flags and later F727 still get written if
the intervening services return. Such states are source consequences, not
claims that natural play supplied them. No handler repairs them or guarantees
progress when a wait/service stalls.

There is no `joinForce`, active-party/AI grant, F65 write, transport-mode change,
Caravan entity creation, warp or departure request here. F418 is a future battle
unlock, not immediate Battle18 admission or completion F518. Existing F65,
membership and transport state remain independent inputs.

After `csc_end`'s shared conditional scroll wait/scroll-speed clear and register
return, the zone wrapper closes windows, waits a VInt and physical 0 destination
equality. The entity wrapper closes windows and reactivates entity updates;
its zero action flags do not restore Rohde facing. The item wrapper/FieldMenu
uses the d6 distinction above; handled results exit, suppressed results stay
in the menu until subsequent user action. `ProcessPlayerAction` reactivates
view/entity updates after FieldMenu exits.

The exploration loop returns to `WaitForEvent`. Only pending zero retargets
camera 0 and installs the controlled-entity action; ordinary player type 0 selects
walking control. No selected scene normalizes supplied mode or pending state.
Stop at this conditional caller/control return: it proves neither responsive
input nor natural exit, sword acquisition, Rick recruitment, Taros or Caravan use.

## Reproduction and coverage limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands.
Verify the read-only pin/clean state, then use the existing
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init payload versus fixture, map-script-engine payload versus fixture, and
`_cutscene_facts` versus `battle-cutscenes-static-v1.json`. All pass unchanged;
no extraction, ROM verification or emulator observation is newly claimed.

Select `programCorpus.programs` by the three IDs above and bound the repeat to
sourceLine≤133. Compare every selected sourceLine/macro/argument with source,
then account for the 12 inline bodies separately. All 100 records and 25 script
text comments match; the two caller 1371 sites also match `gamescript.txt`.
Compare all operations of the three target programs against source mnemonic,
operand order and source line; preserve branch target and flag-write order:

| Existing fixture under `tests/fixtures/h2/` | Direct selection and evidence limit |
| --- | --- |
| `map-script-engine-static-v1.json`, `map-init-static-v1.json` | Retained comparisons pass; two whole scene programs plus six repeat records. Prior Hassan init/population joins remain accepted, not a new composed observation. |
| `map-events/zone-programs.json` | Program 148 `Map9_ZoneEvent0`, five operations; three exact rows share its target across both setups. |
| `map-events/entity-programs.json` | Program 671 `Map9_EntityEvent0`, 25 operations; row 0 action flags, current activation/mapping, search and facing-helper consumers reviewed separately. |
| `map-events/item-programs.json` | Program 78 `Map9_ItemEvent0`, six operations; selected-item wrapper, coordinates, direction wildcard and d6 consumer reviewed separately. |
| `map-event-item-transactions-static-v1.json`, `common-menus-static-v1.json`, `common-stats-static-v1.json`, `map-event-request-consumption-static-v1.json` | Accepted aggregate caller/service contracts. They do not provide per-choice natural-input, inventory, all-actor or persistence coverage for these scenes. |

Resolve enums before selecting source owners; inspect both setup pointer/population
lists, selected zone/entity/item rows, script macro/Trap6, prompt and jump aliases,
HP/mapping/action/destination/priority/follower consumers and each actual return
path named above. These direct source joins establish bounded static semantics;
aggregate fixture equality alone does not establish the composition.

Discriminating states include F726 already set; sword found with either F798;
no sword after F727; wrong selected item or wrong player coordinates; F727-set
item use; F725-set/F727-clear; absent Peter/FOLLOWER_B; follower Rohde excluded
from talk; F65-moved Rohde; zero Bowie versus other-ally HP; unfinished second
jump; stale mappings/list and partial acceptance flags. This is source review,
not an executed runtime matrix or a new acquisition candidate.

**Unknown:** natural zone/talk/item admission, supplied inventories/flags/HP,
logical mappings and physical lists, actual input/release/choice, all-actor and
window/action/update completion, rendered text/gestures/overlap/audio, usable
control, historical lore and save/resume persistence. These remain deferred under
[ADR 0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR 0016](../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR826's corrected macro/inspection failure and original checks](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160316137)
and [its accepted rebase/final CI](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/826#issuecomment-6160352020),
[PR822's comparison/inspection/rejected-command history](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/822#issuecomment-6160096072),
and [PR818's failed destination join](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159788942)
with [corrected checks/final CI](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/818#issuecomment-6159852108).
Those results keep their completed statuses and do not require ritual replay.

Acceptance is complete owned-diff/source/provenance/link/private-boundary review,
`git diff --check`, clean committed `uv run sf2 verify plan --base origin/main --head HEAD`
and actual final-head CI. Documentation-only scope runs no local blanket
normal/full/engine/Godot/H3/H4 suite. Exact candidate/check/failure/process state
belongs in the Issue/PR handoff; independent acceptance and merge remain main-gate's.
