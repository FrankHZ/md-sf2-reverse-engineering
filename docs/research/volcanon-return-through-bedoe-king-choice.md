# Volcanon return through the Bedoe king choice

Evidence date: 2026-10-13. **Confirmed:** selected Map11 return requests lead to
Bedoe's F722-selected setup. Its king interaction offers a yes/no prompt while
F799 is clear. Yes runs `cs_5797A`, requests Luke's departure and hides his current
mapped entity, then sets F799 after script return. No or B cancellation requests
a refusal response; F799-set interaction requests only the closing text.
**Inferred:** this is an optional continuation of the shrine audience.
**Unknown:** natural access, actual supplied state, completed services and playable
continuity. Neither shrine completion nor the king's speech proves a Polca arrival
or an available raft.

## Agreement, provenance and resources

Standard rigor under [Issue #792](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/792)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
prompt results, actor mappings and post-script commitment require distinct boundaries.
This document is the sole tracked owner. It consumes accepted main
`09eabec0105ceda99c625a143c04951ec0f35b72`, especially
[the shrine audience](./bedoe-to-volcanon-audience.md),
[the first Bedoe audience](./bedoe-king-address-through-first-audience.md),
[Bedoe entry](./battle12-outcomes-through-bedoe-entry.md),
[interaction state](./map-event-interaction-state.md),
[request consumption](./map-event-request-consumption.md),
[common menus](./common-menus.md#confirmed-input-contracts) and
[common scripting](./common-scripting.md). No stacked dependency or shared edit is needed.

Scope ends after the first selected king response, caller and conditional control
handoff, including refusal and repeat branches. The two return rows establish
requests, not a floor/stair route. Exclude natural walking, a broader town inventory,
`cs_5766C`, `cs_5779C`, F724's producer, Bedoe exit, Polca arrival/raft service,
Luke recruitment, Battle13, runtime acquisition and new verification machinery.
No inseparable scope expansion was found.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. The retained map-init/map-script-engine
pair binds it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass. This is static
source/retained evidence, not a newly acquired composed observation. Fnnn denotes
flag nnn; table rows are zero-based and scene records below are one-based.

The unchanged pair is 18,285,979 bytes. Plan: one process, linear selected reads and
existing comparisons, estimated below 512 MiB peak and one minute, with small prose
and receipts only. Comparisons took 0.080 seconds; selected scene/caller/text checks
0.077 seconds. The scene is exactly Map10 `mapsetups/scripts.asm:615–637`, 23 lines.
No full export, capture, new parser, comparator or scene fixture is needed. Replan
before materially expanding these resources; Unknowns do not authorize acquisition.
Acceptance is direct source/record/text/fixture and document/link/private-boundary
review, clean committed dependency planning and actual final-head CI, under the
explicit documentation-only local profile.

## Selected return requests and setup

**Confirmed:** `data/maps/entries/map11/6-warp-events.asm` supplies these two
no-scroll discriminators:

| Row | Matching coordinates | Request |
| --- | --- | --- |
| 0 | Any X, Y=19 | `MAP_CURRENT=255`, destination `(8,13)`, Down. |
| 2 | X=8, Y=34 | `MAP_BEDOE=10`, destination `(45,50)`, Down. |

There is no F722, F799, membership or facing prerequisite in either row. Down is
the requested arrival facing. The self-map destination does not prove walking to
`(8,34)` or generate that next request by itself. The intervening entrance row is
already owned by the shrine document; no all-stairs inventory is implied.

Reuse `entityscriptengine_2.asm:loc_528A` and
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`: masked layout
attribute `$1000` admits the warp producer, which converts internal coordinates
by `MAP_TILE_SIZE=384`, scans eight-byte rows in order, accepts negative coordinate
bytes as wildcards and stores the first matching request and SFX. In
`explorationfunctions_2.asm`, `WaitForEvent` polls pending; `ProcessMapEvent` clears
the type before dispatch. Parameter1=0 selects `ProcessMapEventType1_Warp`'s
no-scroll path: request SFX, idle main, unwind to `MainLoop`, sample player position,
replace the nonnegative destination coordinates, and return map/facing.

`code/gameflow/mainloop.asm:MainLoop` calls `SwitchMap` then `CheckBattle`.
`data/maps/global/flagswitchedmaps.asm` has no Map10/11 replacement, and
`data/battles/global/battlemapcoords.asm` has no Map10/11 battle row. `CheckBattle`
uses current map for the sentinel comparison and preserves it on the no-battle
return. Thus these requests select exploration independently of battle unlock
flags, assuming the current map for row 0 remains Map11.

`ExplorationLoop` heals living/immortal allies and fades at entry. The current-map
path updates main properties/position and raft/follower placement, loads the map
and runs its init without rebuilding fixed population/mappings. Map11's init is
only `rts`. The explicit Map10 path instead sets current map/no-battle state,
loads tilesets, obtains setup entities, clears/rebuilds mappings and population,
loads mapsprites, clears temporary setup flags and sets F80 before map/VInt/init
services. Entry healing does not prove actor presence or later interaction HP.

`data/maps/mapsetups.asm` selects `ms_map10` with the single F722 override
`ms_map10_flag722`; `GetCurrentMapSetup` resolves the current flag again at each
setup lookup. `pointertable_722.asm` selects alternate entities, entity events,
zone events and init, with shared descriptions/item events. This continuation needs
F722 **at selection**, not merely a story assertion that the shrine was visited.
The accepted shrine caller writes F722 after its script. Supplied F722-clear state
selects the default; an unfinished caller does not establish the later write. No interaction is invoked
automatically by entry or by the F722 write.

## Population and F799 initialization

**Confirmed:** `map10/mapsetups/s1_entities_722.asm` declares Luke as ally lookup
10 at `(38,51)`, Down, `eas_Init`; regular NPC lookup 147 is KING_BEDOE at
`(36,49)`, Down, `eas_Init`. Peter lookup 7 has no fixed row. Physical slots are
allocated by `InitializeMapEntities` in `code/common/scripting/map/mapfunctions.asm`;
147 is a lookup identity, not physical slot 147.

The initializer clears mappings, declares followers first, then consumes fixed/
walking/sequenced rows. Map10 is neither a no-follower map nor an overworld map, so
F65 does not prepend the caravan. In `data/scripting/entity/followers.asm`, F67
supplies Peter and F75 supplies Luke. These flags do not test force membership,
active-party membership, word HP or force AI. For an ally row, an already nonzero
mapping skips the fixed declaration while consuming its action pointer operand.

| Supplied state at a fresh Map10 population build | Consequence |
| --- | --- |
| F75 clear | Luke's fixed row creates lookup 10 at `(38,51)` Down. |
| F75 set | Follower construction maps Luke first; the fixed Luke row is skipped. His current placement/action is the follower's, not the fixed starting coordinate. |
| F67 set | Peter is supplied by the follower declaration. |
| F67 clear | No Peter row supplies him; the cleared lookup maps to physical 0 unless some later operation changes it. |

`mapsetups/s6_initfunction_722.asm:ms_map10_flag722_InitFunction` at `0x5708E`
(retained map-init body 15) tests F799, conditionally calls `MoveEntityOutOfMap(10)`,
then independently tests F724 and conditionally calls `MoveEntityOutOfMap(152)`.
F724 changes neither the king choice predicate nor Luke's conditional removal;
its producer and scene are outside scope. Neither F799 nor F724 is written here.
The default init is a different owner; F799 does not select this alternate init
when F722 is clear.

`MoveEntityOutOfMap` in `mapsetupsfunctions_1.asm` resolves the combatant mapping
and calls `SetEntityPosition` in `code/gameflow/battle/battlefunctions/battlefunctions_0.asm`.
It writes current X/Y and destination X/Y to internal `$7000,$7000` only. It does
not invalidate mappings or remove a follower-list entry. This includes a
F75-supplied Luke: F799 can move that follower out of map without clearing F75 or
its list entry. A fresh population rebuild is not reuse of the prior scene's
invalidated mapping.

Supplied F799-set does not prove the acceptance scene ran. If F799 changes after
init, the repeat caller does not rerun this coordinate removal; a still-present
Luke is not repaired there. Conversely, a partial scene may already have hidden
Luke while F799 remains clear. Preserve these states rather than treating F799
as a complete actor/service receipt.

## King activation, speaker and event flags

**Confirmed:** alternate `s2_entityevents_722.asm` row 19 encodes lookup 147,
`DOWN=3`, and `Map10_2D2_EntityEvent19`. `sf2mapsetupmacros.asm:msEntityEvent`
emits lookup byte, flags byte and relative target word. Its facing-oriented
comment does not define the consumer: `RunMapSetupEntityEvent` in
`mapsetupsfunctions_1.asm:162–247` matches the lookup byte only, scans four-byte
rows to first match/default `$FD`, then treats the second byte as bits.

On the ordinary exploration route, `WaitForEvent` admits A/C input. In
`explorationvints.asm:ProcessPlayerAction`, main is idled, physical 0 is waited until
current X/Y equal destinations, and view scrolling is waited. A takes the menu
branch; ordinary C continues to activation after debug/caravan alternatives.
`explorationfunctions_0.asm:GetActivatedEntity` uses the current view target and
facing, offsets into the addressed tile, extends across `$2800` counter tiles,
excludes the view target and listed followers, and selects the first scanned
entity within Manhattan distance less than 384 internal units of that point.
A negative view target/no match does not enter the successful entity path. The
normal view target is physical 0; this is not a general guarantee for arbitrary
supplied camera/actor state or a natural route to the king.

`GetEntityEventIndex` in `battlefunctions_0.asm:333` scans the mapping list in order
to reverse-map the physical actor; the index changes from ally 31 to NPC128,
and no match falls back to lookup0. The successful ordinary king case therefore
requires the activated physical actor to reverse-map to 147. Mismatched or aliased
mappings need not select that row. `RunMapSetupEntityEvent` stores the supplied
player-facing byte in `EVENT_RELATIVE_POSITION`, resolves the current setup and
selects speaker properties from the lookup's current mapped mapsprite. Normal
KING_BEDOE selects portrait NONE and `DIALOG_BLEEP_7` from
`data/spritedialogproperties.asm`; supplied sprites can change that selection.
No independent portrait, visible speaker or audible result is observed here.

For row 19, `DOWN=3` means both bits 0 and 1 are set:

- Bit 0 waits one VInt and calls `UpdateEntityProperties` on the mapped actor with
  facing `(playerFacing+2)&3`, keeping sprite/other optional properties unchanged.
- The wrapper saves that physical actor and its **pre-interaction facing**. After
  caller return, bit 1 restores the saved facing with `UpdateEntityProperties`.

There is no test requiring king or player to face Down. The wrapper deactivates
`VInt_UpdateEntities` before invoking the caller, then closes portrait/text windows
and reactivates entity updates after return. The script macro's trap 6 separately
activates entity updates before `ExecuteMapScript`; thus the acceptance scene's
waited actions can run. Refusal/repeat do not invoke that trap. These source service
orders are not proof that input, windows or updates completed in a live interaction.

For contrast only, default `s2_entityevents.asm` row 19 encodes 147, `UP=1` and
`Map10_EntityEvent19`, which requests text1678 and returns. Bit 0 is set and bit 1 is
clear: it faces the actor toward the player but has no saved-facing restoration.
It does not test F799 or offer this prompt. F799 alone cannot override F722-clear
setup selection, and the choice caller does not independently require F720/F721.

## Caller, prompt result and response texts

**Confirmed:** entity fixture program 25 is `Map10_2D2_EntityEvent19` at `0x56F0C`,
source lines 37–64, with 15 operations and one table reference. It tests F799 before
requesting any prompt. All five caller text requests match their `gamescript.txt`
keys: 1603–1606 (`0643..0646`) and repeat 1614 (`064E`).

| Supplied state/result | Requests and first caller outcome |
| --- | --- |
| F799 clear | King attributes abandonment of earth-dwellers to Volcanon, text 1603, then asks whether the player will kill Zeon as demanded, 1604; call `j_YesNoPrompt`. |
| Returned D0 word=0 | Run `cs_5797A`; only after it returns set F799, then return. |
| Returned D0 word nonzero | Request 1605–1606, offering a stay in Bedoe and claiming safety; return with no F799 write and no scene/Luke action. |
| F799 set | Request 1614, wishing the leader luck; return without prompt, scene or F799 write. |

The jump interface `code/common/tech/jumpinterfaces/s03_jumpinterface_1.asm`
resolves `j_YesNoPrompt` at `0x10078` to `YesNoPrompt` at `0x15284` in
`code/common/menus/yesnoprompt.asm`. This is the actual input-aware entry, not
`alt_YesNoPrompt` and not the cutscene `yesNo` opcode/F89 service.

`YesNoPrompt` snapshots `PLAYER_1_INPUT`, creates and loads the prompt, clears
`CURRENT_DIAMOND_MENU_CHOICE` to Yes, moves existing dialogue/gold windows, and
waits for window movement. If its entry snapshot was nonzero it then waits until
`PLAYER_1_INPUT` is zero; a zero snapshot bypasses that release loop. The subsequent
loop tests `CURRENT_PLAYER_INPUT` in order Left, Right, B, C, A. Left stores 0,
Right stores `$FF`; these select rather than immediately return. B stores `$FF`
and exits; C/A confirm the selected byte. No input keeps animating/waiting for
VInt and looping: there is no timeout, automatic default acceptance or third
returned no-choice state. Direction tests take priority over buttons in the same
sample. The initial Yes selection still requires confirmation to return Yes.

After closing/deleting the prompt, the routine reloads the choice byte and
sign-extends it into D0 word. The supported results are 0 (Yes) and `$FFFF` (No
or B cancel). Caller `tst.w d0; bne.s byte_56F30` consequently sends both No and
cancel to the refusal texts. Although its nonzero branch also accepts an arbitrary
supplied nonzero word, this prompt does not normally produce other results.
**Unknown:** actual release/edge timing, sampled simultaneous inputs, complete
window services, visible selection and a naturally supplied outcome. The existing
menu owner's timing/presentation Unknowns remain; static input order does not
replace them with a runtime claim.

## Complete acceptance scene and actor effects

**Confirmed:** `cs_5797A` at `0x5797A`, lines 615–637, has **16 records: 15 opcode
commands and one terminator**, with no standalone sleep record. Cursor 1607 yields
eight requests 1607–1614, keys `0647..064E`: four `nextText`, four `nextSingleText`;
king 147 speaks seven times and Luke 10 once. The following table accounts for every
record and both inline bodies, without publishing the dialogue transcript.

| Records | Ordered request/content |
| --- | --- |
| 1–2 | Set cursor 1607; `faceEntity Peter,147`. |
| 3–6 | King praises the choice and suggests Creed (1607), describes him as an ex-greater devil who may give information (1608), directs travel along the river toward his mansion (1609), and suggests that Polca may have a raft (1610). |
| 7–8 | King faces Right; orders that the mayor be told to prepare a raft (1611). |
| 9 | Wait Luke Up2, then `faceLeft 1`. |
| 10 | Luke acknowledges the order (1612). |
| 11–12 | Wait Luke Down3, Right1, Down4; hide Luke. |
| 13–16 | `faceEntity 147,Peter`; king asks Peter to help the leader (1613) and wishes the leader luck (1614); end. |

The king's description, proposed route, possible raft and order are attributed
speech. Luke's acknowledgment and relative departure do not establish delivery to
the mayor, arrival in Polca, raft preparation/availability or a map transition.
There is no internal prompt, story-flag write, join, party/AI change, follow,
warp, entity reload or recruitment in this script. The caller alone writes F799.
No instruction here changes F67/F75, Peter's F7/F39 or Luke's F10/F42 membership.

`mapscriptengine_1.asm:csc52_faceEntity` reads two word lookups, resolves their
current mappings, compares absolute differences of **destination** X/Y, and writes
the first actor's facing toward the second. Vertical wins a tie; equal destination
coordinates select Down. It updates the sprite and waits one VInt, with **no HP
check on either actor**. This differs from `csc23_setEntityFacing`, whose actual
byte actor passes through the ally word-HP helper. Here the sole `setFacing` actor
is NPC147, which bypasses that ally check. Zero Peter/Luke HP therefore does not
skip these face/action/hide/text requests; this scene neither heals nor revives.

`GetEntityAddressFromCharacter` masks to a byte, subtracts `$60` for NPC128+,
reads the mapping and scales the physical index by 32. An unmapped zero entry
resolves physical 0; an existing `$FF` maps to index 255 without a validity guard.
Neither `faceEntity`, action installation nor hide supplies a general presence
check. F67-clear can face physical 0 as Peter; an already hidden/mismapped Luke
must not be treated as a safely skipped actor. All actor effects depend on the
actual mapping, not the dialogue's names or force/AI membership.

`entityActionsWait` encodes opcode `$2D` with wait byte `$FF`.
`csc2D_entityActionSequence` clears actor flags with mask `$9F`, installs its
constructed action pointer and waits for that actor's pointer to equal `eas_Idle`.
The movement entries emit `esc04_moveToRelativeDest`; signed tile deltas become
384-unit offsets from current position. `faceLeft 1` instead emits facing plus
sprite-update/wait work, not a leftward tile. Both action bodies are waited, but
this is not a global actor barrier or proof of unobstructed travel. No initial
Luke coordinate is imposed by the scene. With valid fixed placement, completed
movement and ordinary services, the arithmetic is `(38,51)` → `(38,49)` → `(39,56)`
before hide. **Inferred:** this conditional staging is not an observed endpoint;
F75-supplied following or changed coordinates invalidate the fixed-start narrative.

**Confirmed:** `csc2E_hideEntity` resolves Luke and calls
`entityfunctions_2.asm:HideEntity`. It writes current/destination X/Y to `$7000`,
replaces every matching physical-index entry in the 64-byte mapping list with
`$FF`, and removes the matching follower-list entry by shifting later entries.
It does not clear force/party/follower flags or erase the action/travel fields.
`entityscriptengine_2.asm:VInt_UpdateEntities` skips entity update/action dispatch
when current X is at least `$7000`. In the successful distinct-mapping case the
two Luke actions already reached their waited idle boundary; hide itself is not
a completion service. Bad/aliased mappings broaden which identities are invalidated.
This mapping/list invalidation is absent from F799's later coordinate-only init.

Text handlers select portrait/speech services and wait for view scrolling and
text; single-text additionally closes windows and sleeps 10. The debug-skip path
is separate. No standalone sleep record does not mean zero elapsed scene time.
`ExecuteMapScript`'s terminator conditionally waits for view scrolling when a
text window remains and clears scrolling speed. It neither sets F799 nor restores
player control. A stall before hide, after hide, or in either final text is a
different partial state, and none proves that the caller's F799 write occurred.

## Return, control and reproduction boundary

**Confirmed:** after successful script return the caller sets F799. After any
completed caller branch, the entity-event wrapper restores the saved physical
king's facing (bit 1), closes portrait/text windows and activates entity updates.
Thus record 13's king-facing-Peter request is not a guaranteed final interaction
facing; with normal unchanged king mapping, saved Down is restored. Supplied
pre-interaction facing is the restore value, and the wrapper uses its saved
physical actor even if later mapping state differs. Unlike the zone-event
wrapper, this entity wrapper does not add a post-caller wait for main to stop.

`ProcessPlayerAction` returns to `ExplorationLoop`, then `WaitForEvent` retargets
camera 0 and calls `SetControlledEntityActScript(0)` **only if pending map-event
type is zero**. `PLAYER_TYPE=0` selects `eas_ControlledCharacter`; other player
types select their own action. Finished caller/window/VInt services and zero
pending establish this conditional control handoff. F799-set alone establishes
none of them, and the handoff is not observed input responsiveness. Stop here.

| Evidence surface | Reproduction and discriminators |
| --- | --- |
| Return/setup/init | Read the two Map11 warp rows, `MainLoop`, no Map10/11 replacement/battle rows, F722 setup/pointer selection, alternate entities and init body 15. Preserve current-map versus explicit rebuild, F799/F724 independence and F75 fixed-row skip. |
| Event/caller | `tests/fixtures/h2/map-events/entity-programs.json` program 25; compare all 15 operations, labels/targets and source lines 37–64. Read row 19/default contrast, activation/reverse mapping, speaker and bit 0/bit 1 wrapper consumers. |
| Prompt | `s03_jumpinterface_1.asm:j_YesNoPrompt` and `yesnoprompt.asm:8–179`; inspect snapshot/release, ordered input tests, selection versus confirm, close and byte-to-word return. No-choice stays pending. |
| Scene/text | Retained `programCorpus.programs` entry `cs_5797A`; compare all 16 `sourceLine`/macro/argument records with lines 615–637 and read both inline bodies. Resolve cursor 1607–1614 and caller 1603–1606/1614 to their hex keys in `gamescript.txt`; 13 request sites share 12 distinct texts. |
| Services/control | Named face/HP/lookup/action/hide handlers, `Trap6_TriggerAndExecuteMapScript`, text/terminator consumers, `MoveEntityOutOfMap`/`SetEntityPosition`, `ProcessPlayerAction` and `WaitForEvent`. Preserve successful versus partial services, saved physical facing and pending-dependent control. |

Load `. ./local/private-inputs.ps1` in the owning checkout's tool process and
select the verified clean read-only pinned `$upstream`. Reuse the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
The pair comparison does not itself interpret every population row or prove
natural reach. Direct selected-record/text checks and consumer review provide the
bounded interpretation above; no new test program or fixture is introduced.

**Unknown:** natural route/collision/floors and interaction reach; actual supplied
flags, HP, mappings, membership/AI, follower action and player state; input release
and sampling timing, service completion, pending requests and responsive control;
visible speaker/facing/motion/hide/window/audio presentation; later persistence,
Luke's arrival and raft availability. No runtime work is automatically admitted.
Record direct checks, preserved failures, committed
`uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head CI
in the Issue/PR handoff. Do not run local blanket normal/full/engine/H3/H4/Godot
suites solely for this prose. Deliver a clean pushed Draft PR, Project Review /
Verify and frozen writer with successful main-gate notification; independent
review/merge belongs to main-gate. No cleanup or next slice is authorized here.
