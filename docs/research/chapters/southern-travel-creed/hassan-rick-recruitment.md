# Hassan Rick recruitment and following handoff

Evidence date: 2026-10-14. **Confirmed:** F518 selects Hassan's population containing
Rick; F12 moves his mapped actor off-map during init. Ordinary Rick interaction
runs the complete recruitment scene only while temporary F256 is clear. The scene
joins Rick to the force, then requests following in the current population; its
caller sets F256 after script return. **Inferred:** the exchange presents Rick's
curiosity about the Caravan as his reason to accompany the party. **Unknown:**
natural reach, actual flags/roster/HP/mappings, completed services/presentation,
usable input and save/resume persistence.

## Agreement and provenance

Standard rigor under [Issue #848](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/848)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Separate population, temporary suppression, membership and follower lifetimes
require explicit caller/consumer boundaries. Accepted starting base is
`72fd81f1a07d20fdcfc34afae7ba6c2aa992af0e`. This sole research document owns the
conditional Rick population, ordinary interaction, complete `cs_56B02`, immediate
join/following consumers and conditional control/reentry. There are no shared-path
writes or stacked dependencies.

Accepted owners are [Battle18 outcomes and Caravan acquisition](./battle18-outcomes-and-caravan-acquisition.md),
[Taros entry](./southwest-parmecia-through-taros-battle18-entry.md),
[Hassan population/init](./battle16-outcomes-through-hassan-entry.md),
[Hassan/Rohde interaction](./hassan-ancestry-and-rohde-caravan-choice.md),
[common scripting](../../world/common-scripting.md), [membership](../../party/common-stats.md),
[following](../eastern-parmecia/polca-oddler-choice-and-following.md) and
[map requests](../../world/map-event-request-consumption.md). This does not establish a
mandatory shrine-to-Hassan route, require a natural battle order or investigate
other NPC conversations, Caravan operation, later travel/Battle19, Rick's combat
stats/growth/equipment or inventory. No new runtime acquisition, extraction,
fixture, parser or verification platform is introduced.

Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind the pin and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
The unchanged pair totals 18,285,979 bytes; existing linear comparisons were
budgeted below 512 MiB and one minute and completed in 0.107 seconds. Remaining
work is bounded source selection, one prose owner and small ignored receipts.
Reassess material scope/resource growth before acquisition or new machinery.
Fnnn denotes flag nnn; records are zero-based. Text is paraphrased; coordinates,
wait operands and action pointers are source requests, not runtime receipts.

## Population and init are separate from recruitment admission

**Confirmed:** `sf2enums.asm` defines `MAP_HASSAN=9`, `ALLY_RICK=12`,
`ALLY_BOWIE=0`, `ALLY_PETER=7`, `ALLY_ROHDE=11`, `RIGHT=0`, `DOWN=3`.
`data/maps/mapsetups.asm` selects `ms_map9` with the sole F518 alternate
`ms_map9_flag518`. `mapsetupsfunctions_1.asm:GetCurrentMapSetup` selects the map's
default then replaces its pointer for a set matching flag. The two Map9 pointer
tables differ only in entity list; they share entity events and init.

`data/maps/entries/map09/mapsetups/s1_entities.asm` contains 11 setup rows without
Rick. `s1_entities_518.asm` contains 12, adding fixed Rick at `(12,3)`, Right,
`eas_Init`, immediately after Rohde. Both have ten non-ally rows mapped to logical
128–137; ally rows do not consume that sequence. The alternate also changes three
other actors' placement/action operands, without changing Rick's event table.
Those actors' conversations are outside scope. Rick's `eas_Init` sets speed32,
turns acceleration/deceleration off, enables entity obstruction and map/entity
collision, enables auto-facing, sets orientation Up, disables immersion, sets
size24, refreshes the sprite and idles. This actor initialization does not heal.

`mapfunctions.asm:InitializeMapEntities` clears physical entity state and all
64 lookup bytes, declares flag-selected followers starting at physical 1, then
setup actors, then main physical 0. A setup ally reuses an existing nonzero ally
lookup rather than declaring a duplicate. It does not test joined membership to
admit Rick's row. For Rick, `GetAllyMapsprite` selects a blue flame at zero HP outside
battle; otherwise it selects the current class-based sprite. HP0 does not remove
the actor or suppress its event row. No Rick class/gear values are assumed here.

Hassan is neither an overworld nor a follower-exclusion map, so
`InitializeFollowerEntities` uses `data/scripting/entity/followers.asm:table_Followers`
even with F65 set. That table contains no Rick declaration. Peter is conditional
on F67; there is no fixed Peter row in either Hassan list. F84 can declare Rohde
before his fixed row and suppress the duplicate. Population alone establishes
neither a living Peter nor a particular current follower chain.

The complete shared `ms_map9_InitFunction` (`0x568C4`) has 13 operations:

| Ordered group | Predicate and result |
| --- | --- |
| 0–3 | F518 clear moves logical137, the final merchant, off-map; set skips that call. |
| 4–7 | F65 set moves logical Rohde11 off-map; clear skips. This is independent of Rohde's F11 membership. |
| 8–11 | F12 set moves logical Rick12 off-map; clear skips. It does not join or remove Rick from the force. |
| 12 | Return. No recruitment, transport-mode or new battle-flag write. |

`MoveEntityOutOfMap → j_GetEntityIndexForCombatant → j_SetEntityPosition` resolves
the current lookup then writes `$7000` to X/Y and both destinations. It does not
call `HideEntity`, delete mapping/list entries, clear actions/travel/velocity or
change HP. Thus the F12-set alternate still declares Rick before moving him away.
With fresh F518-clear/F12-set state, lookup12 remains zero and init moves physical0
instead. This supplied inconsistency is not proof of a natural occurrence.
Missing/aliased mappings are not checked or repaired by this helper.

`ExplorationLoop` first calls `HealLivingAndImmortalAllies` independently of
recruitment: living allies and the hardcoded immortals Peter/Lemon have HP/MP
restored and the shared status mask applied; ordinary dead Rick is skipped.
With an explicit map, it rebuilds entities and calls `ClearMapSetupTempFlags`,
which clears F256–383 from start256 and DBF counter127, then sets F80. With map
argument -1, the existing-population branch instead updates main/raft state and
skips rebuilding and temporary-flag clearing. Both paths later run current map
init. Reentering an exploration function therefore does not invariably clear F256
or discard a current Rick follower. Transport remains a supplied independent state.

## Ordinary activation and temporary guard

**Confirmed:** `explorationvints.asm:ProcessPlayerAction` idles physical0, waits
for its movement and view scrolling, and prioritizes A over C. On the ordinary C
path after debug/Caravan alternatives, `GetActivatedEntity` uses the current view
target/facing and working layout. It extends the addressed point across `$2800`
counter blocks, excludes the view target and physical actors in
`EXPLORATION_ENTITIES`, and returns the first qualifying nearby actor with
Manhattan distance below one `MAP_TILE_SIZE=384`. It is not a lookup of the fixed
Rick coordinate or a test of F518/F12. Actual layout, supplied positions and
prior competing actors matter.

`battlefunctions_0.asm:GetEntityEventIndex` maps back by scanning the lookup in
its source order for that physical actor. The selected actor must resolve to
logical12 to select Rick; aliases may produce an earlier identity. No matching
ordinary actor/row is proved by directly calling the handler with an artificial
state or by observing a default-zero Rick lookup.

`map09/mapsetups/s2_entityevents.asm` row1 is
`ALLY_RICK, DOWN, Map9_EntityEvent1`. DOWN=3 is an **action-flags byte**, not a
required player-facing direction. `RunMapSetupEntityEvent` saves the supplied
player facing in `EVENT_RELATIVE_POSITION`, obtains portrait/speech properties,
and uses bit0 to turn the mapped actor to `(playerFacing+2)&3` before the call.
Bit1 restores the actor's saved pre-interaction facing after the call. Thus the
scene's final Rick facing need not survive the wrapper. Restore uses the saved
physical index, not a new recruitment/population decision.

The wrapper disables entity updates around the handler, but `script` uses
Trap6, whose `Trap6_TriggerAndExecuteMapScript` activates entity updates before
`j_ExecuteMapScript`. Waited Rick actions therefore have an updating path; actual
completion is not guaranteed by that source order. After the handler the wrapper
restores facing, closes portrait/text and reactivates entity updates.

`Map9_EntityEvent1` at `0x56842` is exactly five retained operations:
`chkFlg 256`; branch-if-set to return; `script cs_56B02`; `setFlg 256`; return.
It checks neither F12 nor F518, F65, F516, sword possession, active capacity or HP.
F256 set suppresses the script, though wrapper portrait/facing/closure work still
occurs on an admitted entity call. F256 clear executes the scene regardless of
those other supplied flags; F256 is written only after it returns. It is not a
persistent joined flag or an atomic transaction with the scene.

## Complete Rick scene and dialogue

**Confirmed:** `map09/mapsetups/scripts.asm:134–162`, entry `cs_56B02` (`0x56B02`),
contains all 29 selected commands and no inline action bodies. The retained
`programCorpus.programs[id=cs_56AE6]` physically groups source through `csc_end`:
Rick occupies records6–34 of that 35-record program. Records0–5 belong to Rohde's
repeat prompt and branch away; they are not a Rick preface. The Rick handler enters
the label directly. The complete Rick sequence has no choice or internal branch.

| Physical records | Complete ordered work and attributed speech |
| --- | --- |
| 6–11 | Cursor1411; Rick priority `$FFFF`; Bowie priority0. Rick asks whether the leader killed the Kraken1411, credits him for reaching Hassan1412, then calls the town dull1413. |
| 12–18 | Sleep20; Rick Up; Sleep30; wait Rick `eas_Jump`; Sleep20; face Rick toward Bowie; Rick asks what he is seeing1414. |
| 19–23 | Face Peter toward Rick, then Rick toward Peter; Peter identifies the Caravan as an ancient vehicle found in the shrine1415; Sleep20; face Rick toward Bowie. |
| 24–28 | Rick expresses interest1416, asks about its discovery1417, requests to accompany the party1418; `join ALLY_RICK`; then Rick celebrates departure1419. |
| 29–34 | Wait Rick `eas_Jump`; wait Rick `eas_Jump` again; Sleep20; Bowie priority `$FFFF`; `addNewFollower ALLY_RICK`; end. |

All nine scene text sites match `gamescript.txt` keys `0583..058B` (1411–1419).
Rick uses `$0,ALLY_RICK`; Peter uses `$C0,ALLY_PETER`. IDs1411,1412,1416,1417 use
`nextText` and source `{W2}`; 1413,1414,1415,1418,1419 use `nextSingleText` and
`{W1}`. The join service additionally requests generic text446 (`01BE`) with Rick's
name/current class, independently of the scene cursor. Peter's description and
Rick's Kraken/arrival account are attributed speech, not evidence that this caller
checked a vehicle, F65, a completed travel route or the narrated history.

`csc02_displayTextbox` obtains portrait/speech settings, waits for view scrolling,
requests `DisplayText` and advances the cursor. `csc00_displaySingleTextbox` does
the same, then closes portrait/text and sleeps10. The W1/W2 markers remain text
service controls, not extra script commands or proof that the user acknowledged
anything. `ExecuteMapScript` clears its skip-text byte at entry; P2 Start plus
debug mode can set it, suppressing these ordinary text calls/cursor increments
and `csWait` sleeps. This is not the ordinary narrative path and does not bypass
all action/join consumers. No full text-renderer/input trajectory is claimed.

## Immediate action, facing and membership consumers

**Confirmed:** `mapscriptengine_1.asm:csc53_setPriority` maps the logical actor
and writes its `byte_FFAFB0` entry to0 for a zero operand or1 otherwise. The
sprite-link consumer `entityscriptengine_1.asm:sub_4E24` processes nonzero entries
before zero entries. Rick is set1, Bowie0, then Bowie1; these are not saved-value
restores, battle turn priorities or direct VDP tile-priority writes.

`csc23_setEntityFacing` checks the actual packed ally byte through
`AdjustScriptPointerByCharacterAliveStatus`; Rick HP0 skips the explicit Up
command. The four `csc52_faceEntity` calls have no HP guard. They resolve both
actors and use their **destinations**, not current visible positions: compare
absolute X/Y differences, choose the horizontal axis only if |X|>|Y|, otherwise
the vertical axis. A target left/up selects Left/Up; the opposite selects
Right/Down, so coincident destinations select Down. It writes facing, refreshes
the sprite and waits one VInt. Missing Peter lookup7 can therefore make a
Peter-directed operation address physical0; it does not omit Peter's text.

All three `setActscriptWait` commands dispatch through `csc15`, resolve Rick,
replace his current action pointer and wait for that actor's pointer to become
`eas_Idle`. There is no HP guard or whole-party barrier. `eas_Jump` disables
auto-facing, requests relative `(0,-1)`, waits5, requests `(0,1)`, waits5,
requests `(0,0)`, enables auto-facing and jumps to idle. `ac_moveRel` uses
`esc04_moveToRelativeDest`, scales by384 relative to current position, checks
other-entity destination conflicts and enters the movement consumer. The body
contains no destination-equality wait between those requests. A finished action
pointer does not establish an exact visible arc or settled position. Stalled
updates/services can prevent join or later F256; earlier writes remain.

`join` encodes csc08 with word12, without the sad-join bit or the special128
Sarah/Chester selector. `mapscriptengine_2.asm:csc08_joinForce` clears current
speech SFX, waits view scrolling, selects `MUSIC_JOIN`, calls
`j_JoinForce → battleparty.asm:JoinForce`, then `j_GetClass`, writes the dialogue
name/class indices and requests text446. Its acknowledgement tail calls
`j_FadeOut_WaitForP1Input`: this schedules previous music through
`PlayMusicAfterCurrentOne` and calls `WaitForPlayerInput`. The latter waits VInts
until masked `CURRENT_PLAYER_INPUT` has a nonzero standard direction/button bit;
it does not itself require a new edge or release. Then csc08 closes text and
sleeps10. This helper's name is not evidence of a screen fade or completed sound.

`FORCEMEMBER_JOINED_FLAGS_START=0`, `FORCEMEMBER_ACTIVE_FLAGS_START=32` and
`FORCE_MAX_SIZE=12` resolve the membership consumer:

1. Set F12, even if already set; there is no already-joined rejection.
2. `UpdateForce` rebuilds joined/active/reserve lists from the separate flags,
   without filtering HP. Active membership is F44 for Rick.
3. If the rebuilt active count is below12, call `JoinBattleParty` to set F44.
   At12 or above, skip the active write; an existing F44 is not cleared.

The active/reserve counts and lists are rebuilt **before** that possible F44 write,
with no second `UpdateForce` in this join tail. Newly activated Rick can therefore
remain in the prior reserve snapshot until a later rebuild. Neither this service
nor the scene heals/revives, resets combat stats, grants AI control, replaces a
party member or reselects Rick's mapsprite identity. A full party does not block F12,
join text or the later following request. HP0 likewise does not block membership.
The generic class text is a current-data read, not a new fixed class/stat claim.

## Current following, repetition and reentry

**Confirmed:** `addNewFollower` is csc56, distinct from the earlier owners'
`followEntity` csc2C. `csc56_addFollower` has **no HP guard**, including no Bowie-HP
check. It resolves Rick12 to a physical actor, scans `EXPLORATION_ENTITIES` to
`$FF`, and selects the last listed physical actor as target, or physical0 for an
empty list. It passes offsets `($FFE8,0)` = `(-24,0)` pixels to `AddFollower`.
It does not select Peter or FOLLOWER_B by identity or create a missing Rick.

`entityfunctions_2.asm:AddFollower` installs a copied 42-byte `eas_Follower1`
block on that physical follower, patches the target and offsets, and advances
`ENTITY_WALKING_PARAMETERS`. It appends the physical follower before the list's
sentinel only if absent; existing entries are neither moved nor removed. Even
a duplicate allocates/installs a new block. If supplied Rick is already the last
listed actor, csc56 selects Rick himself as target; it does not reject that case.
A missing Rick mapping can instead install the action on physical0. These are
supplied-state consequences, not naturally observed recruitment outcomes.

`eas_Follower1` sets speed32, disables acceleration/deceleration and entity
obstruction, enables map/entity uncollidability, then loops follow/destination
wait. `entityscriptengine_2.asm:esc03_follow` reads the target's current position
and moves only when the larger coordinate separation exceeds `$1E0` internal
units. It derives a destination from target destination/facing and the patched
offsets, tests layout with fallback offsets/target destination, and writes
travel/velocity. Installation is not immediate placement or completed following.
There is no final follow-distance or all-actor wait before scene return.

No scene operation sets a persistent Rick follower flag. F12/F44 are force/active
membership; F256 is temporary interaction suppression; the physical list/action
allocation belongs to the current population. `InitializeFollowerEntities`
rebuilds from the normal flag table on fresh Hassan entry and has no Rick row.
`InitializeFollowerActscripts` is different: it writes consecutive physical
slots from current follower flags, without rebuilding the list or resolving
logical identities. Neither routine is called by this Rick scene. Do not infer
permanent following or a stable target identity from a successful join.

| Supplied case | Source consequence, conditional on called services returning |
| --- | --- |
| Fresh F518 set, F12 clear | Rick's fixed row is declared and init leaves it in place; fresh F256 is clear. Ordinary proximity/mapping still must admit his interaction. F65 need not be set. |
| F518 clear, F12 clear | Fresh population contains no Rick. The shared event row still exists, but it does not itself supply an interactable actor. |
| F256 set with F12 clear | Direct/admitted handler skips recruitment despite no membership. Wrapper work still occurs; fresh map rebuild clears this temporary suppression. |
| F256 clear with F12 set | Handler would replay the entire scene if actually reached; init/population and follower exclusion usually provide separate obstacles. No handler-level already-joined shortcut or rollback exists. |
| Successful same-instance completion | F12 is set, F44 conditionally set, Rick action/list updated and F256 set afterward. Ordinary activation excludes his listed physical actor; a separately supplied direct handler call is also suppressed by F256. |
| Interrupted after join, before following or caller return | F12 and possibly F44 already changed while F256 can remain clear. Join acknowledgement or a later jump can be unfinished. A retry is not an atomic recovery operation. |
| Fresh F518/F12 set reentry | Old action/list allocation is rebuilt; fixed Rick is declared, then moved off-map by F12. F256 clears, but that does not restore the ordinary visible NPC or permanent following. |
| Existing-population exploration entry (-1) | No fresh list/temp reset. Current F256/list/mappings persist, but init still sees F12 and moves mapped Rick off-map without stopping his follow action. Subsequent action/service effects are not proved by that move. |
| F518 clear, F12 set fresh entry | Missing lookup12 aliases physical0 and init moves main off-map; this is not evidence that Rick vanished safely or that control is usable. |
| HP0 Rick, absent Peter, empty/changed follower list or full party | The distinct HP, lookup, active-capacity and current-target rules above apply; no common eligibility check normalizes them. |

Priority/facing/jump mutations precede membership; membership precedes the last
text/two jumps, Bowie priority restoration, follow installation and caller F256.
Partial execution may retain any completed prefix. F518 and F65 are neither
written nor repaired here. There is no warp, travel request, sword transaction,
Caravan creation/operation or PLAYER_TYPE normalization in the selected program.

After `csc_end`, `ExecuteMapScript` conditionally waits for view scrolling when
its dialogue-window index is nonzero, clears view-scrolling speed and restores
registers. Trap6 returns to the caller, which sets F256. The entity wrapper then
restores Rick's pre-talk facing, closes windows and activates updates before
returning through `ProcessPlayerAction`. The exploration loop reaches
`WaitForEvent`; only pending zero retargets view to physical0 and installs the
controlled action. PLAYER_TYPE0 selects ordinary walking control; other supplied
modes keep their shared selection. Nothing here establishes a responsive player,
settled followers or a natural next journey. This is the stopping boundary.

## Reproduction and coverage limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands,
verify the read-only source pin/clean state, and run the existing
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Map-init and map-script-engine payload/fixture comparisons and `_cutscene_facts`
against `battle-cutscenes-static-v1.json` passed unchanged. These are retained
comparisons, not newly extracted ROM evidence or a natural recruitment capture.

| Existing fixture under `tests/fixtures/h2/` | Selected use and limit |
| --- | --- |
| `map-script-engine-static-v1.json` | `programCorpus.programs[id=cs_56AE6]` records6–34, source134–162: all29 command/macro/argument joins, nine scene text sites and shared command contracts. No inline action body is hidden inside that count. |
| `map-events/entity-programs.json` | Program672 `Map9_EntityEvent1`, address0x56842: all five operations, branch polarity and post-script F256; one physical row shared by two setups/routes. |
| `map-init-static-v1.json` | `primarySourceBodies[sourceOwnerSymbol=ms_map9_InitFunction]`: all13 operations and both pointer joins; fresh versus existing-population callers inspected separately. |
| `map-setup-static-v1.json`, `map-entities-static-v1.json` | Existing setup/population owners; both actual Map9 lists, enums, lookup and follower declarations directly reviewed. |
| `common-stats-static-v1.json`, `map-event-request-consumption-static-v1.json` | Shared membership/request contracts; no per-Rick input, actor, save/resume or composed control observation. |

For direct reproduction, select the exact program IDs rather than a prefix
matching Event10/11. Compare each selected sourceLine, macro/mnemonic and ordered
operands to its pinned source line with comments removed. Resolve enums from
`sf2enums.asm`; compare scene comments to `data/scripting/text/gamescript.txt`
1411–1419 and generic446. Read the complete named immediate consumers separately:
map setup selection/init, ordinary C activation/reverse lookup/entity wrapper,
Trap6/script dispatcher, csc00/02/08/15/23/52/53/56, `JoinForce`/`UpdateForce`/
`JoinBattleParty`, `eas_Jump`, `AddFollower`/`eas_Follower1`/`esc03_follow`,
fresh/existing ExplorationLoop and `WaitForEvent`/controlled-action selection.
Shared source joins establish bounded static semantics; fixture equality alone
does not establish the composed scene or its natural reach.

The supplied-state cases above are a source review matrix, not executed runtime
cases. Shared scripting's retained direct-handler observations keep their own
bounded claims; they are not a Hassan recruitment or persistence receipt.
**Unknown:** natural visit/talk admission, actual F518/F12/F256/F65 and other
flags, current HP/active capacity/class, logical/physical mappings and follower
order, text acknowledgement and services, action/position/priority/sprite/audio
presentation, usable control and save/resume persistence. No new runtime task is
implied under [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md).

Preserve [PR846's completed comparisons/CI/inspection failures](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161451537)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/846#issuecomment-6161473505),
including the linked [PR842 original results](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161224116)
and [rebased/final results](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/842#issuecomment-6161232331).
Failed source-path guesses or truncated inspections are not absence evidence;
required bodies must be located and reread. No prior failure is relabeled or
accepted fixture weakened by this document.

Acceptance is complete owned-diff/source/provenance/link/private-boundary review,
`git diff --check`, clean committed configured
`uv run sf2 verify plan --base origin/main --head HEAD`, and actual final-head CI.
No blanket local normal/full/engine/Godot/H3/H4 suite is run solely for prose.
Exact candidate/check/failure/process state belongs in the PR handoff; independent
acceptance and integration remain main-gate's.
