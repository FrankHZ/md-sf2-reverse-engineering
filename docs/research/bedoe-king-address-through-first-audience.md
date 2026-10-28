# Bedoe king address through first audience

Evidence date: 2026-10-13. **Confirmed:** default Map10 selects the king's address
through an F720-only caller and the first audience through an F721-only caller.
Neither requires F719 or the other caller's bit. The address requests ten texts,
disperses and hides guards 150–156, and returns before its caller sets F720. The
29-text audience uses Luke, Peter, Bowie, king 147 and birdwoman 148, ends by attempting
Peter→Bowie following, and returns before its caller sets F721. **Inferred:** these
are candidate content continuations from the accepted Bedoe entry. **Unknown:**
natural floor access/order, supplied actors/state, completed services, presentation
and continuous playability. A scene's story framing does not supply its admission gate.

## Agreement and provenance

Standard rigor under [Issue #784](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/784)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
independent callers, shared actors and asynchronous services need explicit state and
stopping boundaries. Sole tracked owner is this document, with no shared-path needs.
Start from accepted `adaa70cf30d780be81fad9b0fba6cc0c8ac9c8c2`, consuming
[Bedoe entry](./battle12-outcomes-through-bedoe-entry.md),
[map-event consumption](./map-event-request-consumption.md) and
[common scripting](./common-scripting.md), without stacking on design work.

Scope is Map10 setup/zone selection, `cs_573EC` and `cs_574A6`, necessary population
and command consumers, then the first audience caller/conditional restored-control
boundary. Stop before `cs_5766C`; exclude Map11/Volcanon entry and scene, the
F722-producing scene, later king choice, Skreech, recruitment/services/town inventory,
further travel, Battle13, floor/path extraction and runtime acquisition. F722 is a
setup discriminator only. No inseparable ownership or scope expansion is needed.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons and the existing cutscene-source comparison pass. This
reuses provenance; it claims no fresh ROM extraction or composed scene observation.
Fnnn means flag nnn. Rows are zero-based; script record numbers below are one-based
positions in retained `programCorpus.programs[].commands`.

The retained pair is 18,285,979 bytes. Use one process, linear selected reads and
comparisons, estimated below 512 MiB peak and one minute, plus small prose/receipts;
no new payload, parser, comparator, per-scene fixture or capture. The existing
comparisons completed in 0.096 seconds. Map10 `mapsetups/scripts.asm` is 26,440 bytes /
637 lines: address lines 225–286 are **62 lines**, audience 287–414 **128 lines**, ending
before `cs_5766C` at 415. Inline action payloads are reviewed separately from script
records. Reassess before expanding these bounds. Unknowns are not an acquisition queue.
Docs-only acceptance is direct source/record/text/fixture/prose/link/private-boundary
review, clean committed dependency planning and actual final-head CI.

## Default zone admission and request consumption

**Confirmed:** `data/maps/mapsetups.asm` selects `ms_map10` with an F722 override
`ms_map10_flag722`. The six-pointer tables in `data/maps/entries/map10/mapsetups/`
select different entities, entity events, zone events and init functions. In
`code/common/scripting/map/mapsetupsfunctions_1.asm:GetCurrentMapSetup`, a set flag
replaces the selected pointer. Zone dispatch resolves the current setup again; it
does not infer population from the narrative or from an earlier selection.

`sf2mapsetupmacros.asm` encodes a zone row as X byte, Y byte and word displacement
relative to the zone-table base. `RunMapSetupZoneEvent` walks four-byte rows, treats
`255` independently as either coordinate's wildcard, and calls the **first** match.
The `$FD,0` default marker selects its relative target. The default table is:

| Row | Request coordinate predicate | Target and local behavior |
| --- | --- | --- |
| 0 | X=29, any Y | `Map10_ZoneEvent0` at `0x56F9C`: F720 set returns; clear runs `cs_573EC`, then sets F720. |
| 1 | X=33, any Y | `Map10_ZoneEvent1` at `0x56FAE`: F721 set returns; clear runs `cs_574A6`, then sets F721. |
| 2 | X=31, Y=53 | The same audience caller, not another program or a second flag. |
| Default | No earlier row matches | `Map10_DefaultZoneEvent` at `0x56F9A`, just `rts`. |

The address has one table reference and one script call site; the audience has two
table references but one script call site. Each caller has exactly five operations:
`chkFlg`, `bne.s` to return, `script`, `setFlg`, `rts`. Neither tests the other bit,
F719, party membership, HP or actor presence. With both bits clear, either selected
caller can run; with only one set, only that caller is suppressed. Both set suppress
both scenes. A suppressed first match does not fall through to a later row.

The F722 alternate zone table replaces all three rows with `(6,23)` selecting the
later Skreech caller and the same default return. It does not retain address/audience
rows. This establishes selection only; no later scene is analyzed here.

A matching X/Y in prose is not an automatic walking trigger.
`code/common/scripting/entity/entityscriptengine_2.asm:loc_5294` checks the selected
layout attribute masked by `$3C00` against `$1400`, writes event type
`MAP_EVENT_ZONE_EVENT=6`, and writes word X/Y parameters from destination plus the
attempted movement offsets, divided by `MAP_TILE_SIZE=384`.
`explorationfunctions_2.asm` under `code/gameflow/exploration/` polls that pending
request in `WaitForEvent`; `ProcessMapEvent` clears its type before dispatch.
`ProcessMapEventType6_ZoneEvent` installs `eas_Init` for physical 0, loads word X/Y
from parameters 1/3 and calls `j_RunMapSetupZoneEvent`. The table compares their low
bytes. These are actual producer/consumer and row-layout rules, not proof that a
particular floor tile has the required attribute or that the player can reach it.
Natural collision, stairs, request generation and scene order remain **Unknown**.

## Population and shared actor identities

**Confirmed:** the accepted entry owner retains F722/default population and
F719/F720 init behavior. Default init runs `cs_570B0` only with F719 clear, then sets
F719; independently, F720 set calls `MoveEntityOutOfMap` for 150–156. That init path
is not a prerequisite encoded in either zone caller. Neither new program reloads
a map or rebuilds entities, so it consumes current mappings, positions and actions.

`code/common/scripting/map/mapfunctions.asm:InitializeMapEntities` clears mappings,
installs flag followers, then consumes the selected fixed/walking declarations.
Regular NPC lookup numbering begins at 128. An ally declaration uses its ally lookup
and does not advance the regular stream; an already mapped ally skips the fixed
declaration. Consequently list position and physical slot are not character lookup:

| Lookup | Default declaration and relevant distinction |
| --- | --- |
| 10, Luke | First declaration `(38,51)`, Up, `eas_Init`; an existing F75 Luke follower mapping supersedes this placement. |
| 147, king | `MAPSPRITE_KING_BEDOE`, `(36,49)`, Down, `eas_Init`. |
| 148, birdwoman | `MAPSPRITE_BIRDWOMAN`, `(37,50)`, Down, `eas_Init`; no personal name is established here. |
| 150 /151 | Birdmen `(34,51)` /`(35,51)`, Up, `eas_Init`; the initial guard scene explicitly returns them to these coordinates. |
| 152–156 | Birdmen `(37,51)`, `(34,53)`, `(35,53)`, `(37,53)`, `(38,53)`, Up, `eas_Init`. |
| 7, Peter | No fixed row. `data/scripting/entity/followers.asm` supplies him through F67. |
| 0, Bowie/main | Physical 0 is declared separately after the lists. |

The F722 alternate preserves king 147/birdwoman 148 and Luke 10, with Luke initially
Down instead of Up; its 150/151 start at `(35,53)` /`(37,53)`, 152 is a birdwoman at
`(6,10)`, 153 a chick, and 154–156 have no fixed rows. Normal alternate zone selection
does not call either new program. A supplied mismatched population/caller must not
be described using the default guard identities.

Map10 is neither a no-follower map nor overworld; F65 does not choose the caravan
prefix. F67, not F7/F39 or Peter's HP/force-AI state, supplies Peter's declaration.
Likewise Luke's fixed declaration or F75 mapping does not prove F10/F42 membership.
Neither program joins force/party/AI, repairs F67 or F75, or changes those memberships.
Exploration's living/immortal healing, including Peter, is not an actor-presence
proof and does not run again merely because a zone scene is selected.

`mapscriptengine_1.asm:GetEntityAddressFromCharacter` masks a character lookup to a
byte, maps NPC128+ through the enemy-index difference `$60`, reads its physical
index and scales by `ENTITYDEF_SIZE_BITS=5`. A zero uninitialized mapping resolves
physical 0. It does not safely skip an absent Peter. A `$FF` mapping left by `hide`
instead scales index 255 without a validity check; it is not the zero-mapping case.
The action, nod and hide handlers do not add a presence guard.

## The king's address

**Confirmed:** `cs_573EC` at `0x573EC` has **33 records: 30 opcode commands, two
sleeps and one terminator**. Cursor 1641 produces ten text requests 1641–1650, keys
`0669..0672` in `data/scripting/text/gamescript.txt`: four `nextText`, six
`nextSingleText`; king 147 speaks six times and birdwoman 148 four. Every command's
source line, macro and operands agrees with the retained corpus; every text key
agrees with the pinned script comment. Inline movement bodies were read separately.

| Records | Ordered content and effects requested |
| --- | --- |
| 1–10 | Cursor 1641; wait for camera destination `(31,47)`. Birdwoman describes a strong greater devil and credits Volcanon with sweeping away the devils 1641–1642. King asks about the greater devil 1643. Wait 148 Left1/faceUp1; she reports his escape 1644. King asks about pursuit and capturing him before recovery 1645–1646; she asks about Grans origin 1647. |
| 11–17 | King nods, offers belief/speculation and warns of another attack 1648–1649; sleep 10; 148 Down; king orders vigilance 1650; sleep 40. |
| 18–24 | Start 150 Down1/Left3/Down3; 152 Down1/Right5; 153 Right2/Down3; 154 Right1/Down3; 155 and 156 each Right2/Down3, all **without waiting**. Then start and wait 151 Down1/Left4/Down3. |
| 25–33 | Hide 150,151,152,153,154,155,156 in order. Wait 148 Right1/faceDown1; end. The final movement is relative repositioning, not an absolute `setPos`. |

The address has no scene choice, warp, join, internal story-flag write or following
command. Its two sleeps total 50 requested frames, not scene duration. These are
attributed claims and intentions: neither the invasion account, Volcanon's success,
the greater devil's fate nor a completed patrol is independently proven by dialogue.

`csc2E_hideEntity` resolves each lookup then calls
`code/common/scripting/entity/entityfunctions_2.asm:HideEntity`. The latter writes
current X/Y and destinations to internal `$7000,$7000`, replaces every matching
physical-index entry in the 64-byte `ENTITY_INDEX_LIST` with `$FF`, and removes the
physical index from `EXPLORATION_ENTITIES` by shifting the following entries. It
leaves the entity's action pointer and travel fields intact. In
`entityscriptengine_2.asm:VInt_UpdateEntities`, X>=`$7000` skips entity update and
action dispatch, so retained nonwaited actions are not thereby proved complete or
allowed to keep advancing normally at that position. No force/party flag is cleared.
For the normal distinct mapped guards, king 147, birdwoman 148, Luke and Peter remain
mapped. With bad/aliased supplied mappings the effect is not confined by actor names.

This differs from F720 re-entry handling: after fresh population construction,
`MoveEntityOutOfMap` uses `j_GetEntityIndexForCombatant` then `j_SetEntityPosition`
(`battlefunctions_0.asm` under `code/gameflow/battle/battlefunctions/`). It writes
only position/destination to `$7000,$7000`; mappings and follower-list entries are
not invalidated. Both paths are out-of-map coordinate writes with distinct mapping
lifecycles, not interchangeable deletion operations. Re-entry does not require or
reuse the previous `$FF` mappings.

## First audience

**Confirmed:** `cs_574A6` at `0x574A6` has **82 records: 77 opcode commands, four
sleeps and one terminator**. Cursor 1651 produces 29 text requests 1651–1679, keys
`0673..068F`: six `nextText`, 23 `nextSingleText`. Speakers are Luke 10 five times,
Peter 7 twelve, king 147 eight and birdwoman 148 four; Bowie has no text request.
All 82 records and 29 text keys agree with the same retained/source checks.

| Records | Ordered staging and conversation |
| --- | --- |
| 1–13 | Cursor 1651; camera `(34,47)` waits. Luke Down1/faceLeft1, two waited jumps, calls Peter 1651. Set Peter `(33,52)` Right and Bowie `(33,53)` Right; camera `(31,47)` waits; Peter Right3, greets Luke 1652, faces Up and greets king 1653. |
| 14–27 | Waited inline king speed 48/48 then idle; king Down1, greeting 1654. Peter Up1; sleep 30, nod, sleep 20, Left, sleep 30, Up; introduction 1655. Peter Down2/Left2; sleep 40; Bowie nods. |
| 28–42 | Start Peter Right3/faceUp1 without waiting; wait Bowie Right3/faceUp1; Bowie nods. Peter introduces Bowie as coming from Grans 1656. Luke Down1/faceLeft1; Peter/Bowie Right. Luke says he knows Bowie as Astral's pupil 1657; two waited Peter jumps and questions 1658/1660 bracket Luke's account 1659. Luke nods and says he was Volcanon's messenger 1661. |
| 43–54 | King Down1; Luke/Peter/Bowie Up. King reports Zeon's release and blames Grans 1662–1663; waited speed 16/16 then idle; king Up2/faceDown1; warning 1664. Peter Up1/Left1/faceUp1 and question 1665; king names Zeon king of devils 1666. |
| 55–64 | Birdwoman Down1, urges a visit to Volcanon and suggests Peter may calm him 1667–1668. Peter hesitates 1669. Luke Up1/Left1; Peter Right; Luke urges going now 1670. Peter agrees 1671, turns Down and tells Bowie to follow 1672. |
| 65–76 | Wait king `eas_Init`, then Down1; asks whether Bowie is coming 1673. Peter/Luke Up; Peter answers 1674; king starts an objection 1675. Birdwoman Left1/faceUp1 and advocates Bowie's audience 1676–1677. King nods and consents 1678. |
| 77–82 | Birdwoman, Luke and Peter face Down; Peter's closing 1679; attempt Peter→Bowie following with selector 2; end. |

The four sleeps total 120 requested frames. There is no choice/menu, warp, map or
entity reload, join, or story-flag write inside this program. Reports about Astral,
Zeon, Grans and Volcanon's temper remain speech, not separately verified history.
The invitation does not enter Map11, make Volcanon appear, set F722 or establish
Luke's recruitment. Peter's instruction that Bowie follow him does not reverse the
actual final `followEntity ALLY_PETER,ALLY_BOWIE,2` operands.

## Services, state and ordering limits

**Confirmed:** `mapscriptengine_1.asm:csc19_setEntityPosAndFacing` reads the actual
byte actor for `setPos`, tests ally **word** HP via
`AdjustScriptPointerByCharacterAliveStatus`, and skips only at zero; HP256 is
nonzero. With admission it writes current/destination coordinates multiplied by 384
and facing. `csc23_setEntityFacing` uses the same byte-actor/word-HP rule. NPC147/148
bypass that ally-HP test. Zero Peter/Bowie HP can therefore skip their staging or
facing; Luke's `setFacing` likewise tests his own word HP. These skips do not
suppress text, movement, jump or nod requests. Nonzero HP with
F67 absent can reposition physical 0 through Peter's zero mapping.

Tile coordinates/deltas become 384 internal units per tile; sprite projection in
`entityscriptengine_1.asm:VInt_UpdateSprites` shifts by 4, giving 24 pixels per tile.
`csc2D_entityActionSequence` builds relative action sequences; its movement branches
emit `esc04_moveToRelativeDest`, which multiplies the signed deltas by 384 and adds
the current position, subject to its destination/obstruction services. Face-action
counts are update/wait operands, not tile distances. `csc14_setEntityActscriptManual`
and `csc15_setEntityActscript` install inline/named action pointers. Wait forms poll
that actor's pointer for `eas_Idle`; the no-wait forms return after installation.
None of these three installations has a general HP or mapping guard.

`esc10_setSpeed` stores packed X/Y speed bytes from `ac_setSpeed`; the king's 48/48,
16/16 and later `eas_Init` 32/32 are distinct. `eas_Init` also restores collision,
obstruction, auto-facing/orientation and sprite properties. `eas_Jump` temporarily
disables auto-facing, requests relative motion and waits, then restores auto-facing
and jumps idle. `csc26_entityNodHead` runs animation/sprite/DMA/sleep services without
an HP check. These services must actually finish for a waited scene to return.
Waiting 151 is not waiting 150/152–156, and waiting Bowie is not waiting Peter. Later
action installation can replace an unfinished action. No scene-end global action
barrier is implied; hide's update exclusion is likewise not action completion.

`csc32_setCameraDestInTiles` clears tracking, sends tile coordinates through
`j_SetCameraDestination` and waits for view scrolling. Text handlers in
`mapscriptengine_2.asm` select portrait/speech services, wait for view scrolling,
display and advance the cursor; single-text also closes windows and sleeps ten
frames. Debug skip behavior prevents equating source requests to visible output or
duration. The terminator conditionally waits for scrolling if a dialogue window
remains and clears scrolling speed; it does not restore player control itself.
[Common scripting](./common-scripting.md) retains controlled handler observations,
which do not prove this composed audience or its natural timing/presentation.

Final audience record 81 encodes follower/target/place words `$0007,$0000,$0002`.
`csc2C_followEntity` probes the **high byte** of the first word before advancing A6:
the alive helper tests Bowie 0's word HP, not Peter 7's. Zero skips all six operand
bytes. Nonzero proceeds independently of Peter's HP; valid mappings install Peter
following Bowie using selector 2's `(-24,0)` pixel-offset operands. The follower
consumer `esc03_follow` shifts these by 4 into internal units and applies its
facing/distance/layout rules; this is not an immediate fixed world-coordinate
placement. `AddFollower` copies and patches a
42-byte action and appends the physical index if absent; replacement occurs before
duplicate detection. A missing Peter can resolve main for both operands. It does
not wait for the following path, write F67, join force/party or change force AI.
No rebuild follows within this scene: this is a current relation. A later map-entry
rebuild uses then-selected follower flags/list and is not guaranteed to preserve it.

The callers permit audience-before-address in supplied state. That fact does not
prove a natural route or justify auditing campaign permutations. With the normal
population, address hides 150–156 whereas audience never addresses those lookups;
there is no hidden-guard dependency requiring F720 before F721. Both use 147/148.
With successful motion from default fixed placement, address moves 148 Left1 then
Right1, returning her to `(37,50)` Down; it does not relocate 147. Audience instead
leaves 147 at `(36,50)` and 148 at `(36,51)` Down. If audience ran first, the later
address's relative 148 movement uses that changed starting point, not a freshly
initialized `(37,50)`. Existing Luke follower placement, HP-skipped staging, missing
mappings, unfinished actions and supplied populations invalidate unconditional
fixed-position narratives. Neither caller repairs those states or requires the
other scene to have normalized them.

## Caller return, reproduction and stopping boundary

**Confirmed:** only after the respective script returns does its caller set F720
or F721. A stalled action/text/camera service does not establish that write. After
the selected caller returns (including the already-set-bit path),
`RunMapSetupZoneEvent` closes portrait/text windows, waits one VInt and calls
`WaitForEntityToStopMoving(0)`. That helper waits for physical 0's current X/Y to
equal its destinations; **it does not test `eas_Idle` or every actor**. The shared
exploration loop returns to `WaitForEvent`, which retargets camera 0 and calls
`j_SetControlledEntityActScript(0)` only when pending is zero. `PLAYER_TYPE=0`
selects `eas_ControlledCharacter`; other types select their own action. Completed
caller and wrapper services plus zero pending establish this conditional control
handoff, not actual input responsiveness. Stop here after the first audience;
there is no implied immediate next scene.

| Evidence surface | Reproduction selection and adversarial boundary |
| --- | --- |
| Setup/zone rows and callers | `map-setup-static-v1.json`, `map-events/zone-programs.json`; direct `mapsetups.asm`, Map10 pointer/zone files and `GetCurrentMapSetup`/`RunMapSetupZoneEvent`. Preserve F722 override, wildcard/first-match, one versus two row references, independent five-operation callers and post-script writes. |
| Init/population | `map-init-static-v1.json` bodies 14/15, `map-entities-static-v1.json`; direct Map10 lists, `InitializeMapEntities`, followers and lookup consumers. Preserve F719/F720 independence, ally versus regular numbering, F67/F75 and alternate identities. |
| Address/audience | Retained `programCorpus.programs` entries `cs_573EC` /`cs_574A6`, source lines 225–414, text keys 0669–068F. Review all 115 records, 39 texts and inline action bodies; no later program. |
| Handler/lifecycle | `map-script-engine-static-v1.json` plus the named pinned map/entity handlers, `HideEntity`, `VInt_UpdateEntities`, `MoveEntityOutOfMap`, position/follow/wait/control and exploration consumers. Distinguish zero/FF mappings, actual HP operand widths, hidden actions, tile units, current following and conditional control. |

Fixture paths above are under `tests/fixtures/h2/`. Load
`. ./local/private-inputs.ps1` in the owning checkout's tool process, select and
verify the clean pinned read-only `$upstream`, then use the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select only the two `entryLabel` values above, compare each record's `sourceLine`,
macro and arguments to its source, and read inline payloads separately. Starting
at each cursor, increment for each text request and resolve its four-digit hex key
in `gamescript.txt`. Select the three default target programs and named init bodies,
then inspect direct caller/list/handler sections. Summary fixture agreement alone
does not parse every population/table row or prove its narrative interpretation.
No new verifier or scene fixture is introduced.

**Unknown:** natural path/collision/floor access and ordering; actual supplied
flags, HP, party/follower/force-AI state and mapping identity; uninterrupted service
completion, hidden/nonwaited action lifetimes, pending state and responsive control;
visible camera, motion, portrait, speech and timing; later follower reconstruction
and continuity into Volcanon. No new runtime evidence closes these questions, and
none authorizes acquisition. Record direct checks, any completed failures, clean
committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head
CI in the Issue/PR handoff. Do not launch local normal/full/engine/H3/H4/Godot solely
for prose. Leave a clean pushed Draft PR, Project Review / Verify and frozen writer;
successfully notify main-gate for independent review/merge. No cleanup or next slice.
