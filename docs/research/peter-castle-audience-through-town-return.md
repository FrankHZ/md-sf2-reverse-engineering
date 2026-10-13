# Peter's castle audience through the town return

Evidence date: 2026-10-13. **Confirmed:** Map7's early audience requires F802 clear,
F701 set and F702 clear. Its `joinForceAI ALLY_PETER,$FFFF` sets Peter's AI-control bit
and calls `JoinForce`; temporary scene followers and caller F67 are separate writes.
The gift dialogue names 100 gold, but this bounded source chain supplies no effective
increase to `CURRENT_GOLD`. The script produces the Map6 request before returning to
caller F702 then F67. **Inferred:** this sends the party with Peter while Astral stays
behind. **Unknown:** natural travel, completed actors/services, observed gold balance
and actual usable destination control. This is conditional static research, not a
continuous playthrough or a remake capability.

## Agreement and provenance

This is the Standard-rigor agreement for
[Issue #748](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/748), under
[the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Membership, transient followers, caller flags and pending-request order require explicit
state boundaries. Sole tracked owner: this document. Accepted starting base:
`81f2812a55900a804e07cfb68535044b7259daeb`, including
[the town/Peter introduction](./new-granseal-town-through-peter-introduction.md) and
[the castle year-later control boundary](./new-granseal-castle-year-later-control.md).
No unmerged dependency or shared-path write; concurrent design consumes accepted evidence.

Start at the accepted town post-Peter event, trace the conditional castle return,
`cs_55A36` and outgoing Map6 entry, and stop at the first destination post-init
pending/control read. Other castle zone branches are selector controls only. The anomalous
F707-clear history reuses its owner rather than reopening that scene. Overworld departure,
Battle09, Bedoe/Volcanon travel, town services and later Astral audiences are excluded.
No parser/schema/fixture, comparison platform, engine/UI change, acquisition or translation.
Acceptance is direct pinned source/text/list/handler/fixture review, retained comparisons,
document/link/private/diff checks, a clean committed planner and actual final-head CI.
The clean pushed Draft PR freezes for independent main-gate acceptance and merge.

Original paths below are relative to `disasm/` in
[`ShiningForceCentral/SF2DISASM`, pinned `master` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The reused read-only checkout is clean at this pin. Retained `map-init` and
`map-script-engine` payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Their fixture comparisons pass; no new extraction/runtime observation is claimed.
Fnnn denotes flag nnn. Command indices are zero-based retained
`programCorpus.programs` records, not frames or inline action indices. Bare map-handler
filenames are in `code/common/scripting/map/`; Map7 setup filenames are in
`data/maps/entries/map07/mapsetups/`. Addresses identify static source anchors.

## Conditional return and audience admission

**Confirmed:** the predecessor ends after `cs_54F26` returns and its caller sets F701.
It does not join Peter, set F702/F67, reload the town population or request this journey.
F706 belongs to the independent sailor/guard event; it need not be set merely because
F701 is set. Carry F707/F411/egress and `PLAYER_TYPE=0` from the stated completed
predecessor lineage, while other membership, follower and later-history flags remain
supplied inputs.

`data/maps/entries/map06/6-warp-events.asm` (`0x9A3F4`) begins with `mWarp 255,0`,
`warpNoScroll`, Map7 `(11,22)`, Up. `WarpIfSetAtPoint` in
`code/gameflow/exploration/exploration.asm` treats negative X as wildcard, compares Y=0,
and writes event type 1, the request tuple and `SFX_WARP`. The ordinary movement consumer
`code/common/scripting/entity/entityscriptengine_2.asm:loc_528A` invokes it for layout
attribute `$1000` after earlier movement/collision work. This row does not prove an
accessible tile or a natural walk from the Peter event. No layout is decoded here.

If the request survives, `WaitForEvent` returns it; `ProcessMapEvent` clears pending
before dispatch. Its no-scroll warp handler idles main, removes the exploration return
frame and returns the explicit tuple to `MainLoop`. `SwitchMap` then `CheckBattle` have
no Map7 substitution/battle-coordinate row, so the result is exploration. Ordinary entry
clears pending, applies the accepted living/immortal heal, sets current map/not-in-battle,
loads the selected population and sprites, clears temporary flags 256-383, sets F80,
loads the map and runs shared init before control. It is not a full-force reset; the
[accepted heal limits](./battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)
and [request consumer](./map-event-request-consumption.md) remain applicable.

Map7's `data/maps/mapsetups.asm` order is default, F701, F702, F805; last applicable wins.
All three pointer tables share the zone and init functions:

| Supplied setup history | Population selected before init |
| --- | --- |
| F805 set | `ms_map7_flag805_Entities`, 11 NPCs; no fixed Astral. |
| F805 clear, F702 set | `ms_map7_Entities`, 12 NPCs; default restored even with F701 set. |
| F805/F702 clear, F701 set | `ms_map7_flag701_Entities`, five allies plus 12 NPCs. |
| All clear | `ms_map7_Entities`. |

`ms_map7_InitFunction` (`0x55816`; retained map-init body 11) tests only F707.
When set, it returns, preserving egress/F411 and this population. When clear, it writes
`EGRESS_MAP=6`, runs `cs_55832`, then sets F707 and F411; that existing scene ends by
explicitly reloading the default population. Thus F701-set/F707-clear is not the ordinary
Peter audience staging: setup alone cannot prove the five allies remain present. Its
full scene and other limits stay with the castle year-later owner.

The shared `s3_zoneevents.asm` (`0x5579E`) maps all five points `(9..13,8)` to
`Map7_ZoneEvent0`. The movement zone producer requires layout attribute `$1400` and
writes event type 6 plus destination-derived word coordinates. The type-6 consumer
clears pending through `ProcessMapEvent`, applies main's init actscript, then passes the
coordinates to `RunMapSetupZoneEvent`, which resolves the setup anew. Neither a row nor
an entry tuple proves this zone request naturally occurs.

| Zone caller input | Result |
| --- | --- |
| F802 clear, F701 set, F702 clear | Execute `cs_55A36`; only after normal return set F702 then F67; return through the default handler. |
| F802 clear, F701 clear, any F702 | Skip the audience and its writes. |
| F802 clear, F701/F702 set | Skip; does not repair an absent F67 or Peter membership. |
| F802 set | Takes the later F805 selector before checking F701/F702; F805 set returns, clear selects `cs_55BEE` and its F805/F70 writes. That later body is excluded. |

F707, F706, F67, membership, AI state and actor presence are not audience guards. F805
is not a guard on the F802-clear early branch: supplied F805-set/F802-clear/F701-set/
F702-clear can select this audience with a different population. These are discriminator
states, not assertions that all are naturally reachable.

## Cast and finite program

**Confirmed:** `scripts.asm:cs_55A36` (`0x55A36`), lines 4-127, has 88 commands (0-87),
39 sequential dialogue IDs 1198-1236 (`04AE`-`04D4`), and no script-level branch.
The source/comment text matches `data/scripting/text/gamescript.txt`. Narrative summaries
below interpret that text; they do not establish the truth of characters' world claims.
The normal F701 population supplies these script references:

| Reference | Declared tile, facing and role |
| --- | --- |
| Peter (ally 7) | `(11,6)`, Left, `eas_Init`. |
| Sarah / Chester | `(12,8)` / `(10,8)`, Up, `eas_Init`. |
| Jaha / Kazin | `(13,8)` / `(9,8)`, Up, `eas_Init`. |
| 128 minister / 129 king | `(12,5)` / `(11,4)`, Down, `eas_Init`. |
| 139 Astral | `(10,6)`, Right, `eas_Init`. |
| 130-138 | Guards/soldiers/maids in the same list; no commands target them in this program. |
| Main / Bowie | Admitted `(11,22)`, Up before controlled movement; the zone's actual main position is supplied, not a fixed declaration in this list. |

`InitializeMapEntities` allocates flag-selected followers first; an already mapped ally
skips its fixed declaration. Thus these are declaration operands, not guaranteed physical
slots or observed starting positions. The script neither loads a map nor reloads entities
before using the supplied cast. F707-clear init or a supplied follower can invalidate the
normal staging assumptions without suppressing the caller.

| Commands | Ordered content and actions |
| --- | --- |
| 0-19 | Set text cursor and camera destination `(6,2)`. Astral and the king discuss Peter's phoenix identity and Volcanon. Peter describes his injured wing and need for help. Astral moves up then down, faces Peter and nods. Camera coordinates are not player coordinates. |
| 20-40 | Astral moves down/right, faces the party and proposes accompanying Peter east. Minister moves down two, challenges him; Astral's waited `eas_JumpLeft`, 40 wait units and right movement precede his reply. Minister and king say he must stay. |
| 41-59 | Minister's up-one action is unjoined; Astral's left/up action is waited. King sends the party, Astral agrees, Peter moves up one then down two and introduces himself. Text 1230 (`04CE`) narrates accompaniment. It precedes the actual join. |
| 60-67 | `joinForceAI ALLY_PETER,$FFFF`; Peter moves down two under a named wait; five `followEntity` commands install the temporary chains below; wait 60. |
| 68-78 | King requests the gift; minister nods, moves right two/up one, faces Up and waits 60, then left three/down two. Text 1233 offers it and 1234 names 100 gold. Minister then moves up one/right one and faces Down. |
| 79-87 | Astral moves right/down, gives farewell/advice; Bowie nods; wait 60; camera follows Bowie; waited Bowie movement down 18; script warp Map6 `(11,1)`, Down; end. |

The motion/wait handlers retain the predecessor's
[actor and service limits](./new-granseal-town-through-peter-introduction.md#handler-effects-and-post-event-boundary).
`entityActionsWait` joins only its named actor's completion, and `csWait` is a sleep
operand, not measured rendered time. HP guards are command-specific: the five
`followEntity` calls below probe Bowie, not their named followers; `joinForceAI` has
no equivalent alive guard. Minister's command 41 does not itself join
before subsequent commands. A script return is not proof that every actor completed,
that a particular route was walkable, or that text/audio/render services were observed.
The final down-18 operation does not replace the explicit destination warp tuple.

## Membership, AI and the two follower mechanisms

**Confirmed:** the existing [party/roster contract](../design/contracts/party-roster-state.md),
[party-membership contract](../design/contracts/party-membership-state.md) and
[common-scripting evidence](./common-scripting.md#confirmed-map-script-active-partyaifollower-command-family)
keep these mechanisms distinct. Direct source reads bind this invocation:

1. `mapscriptengine_1.asm:csc54_joinForceAi` (`0x46FDC`) reads ally word 7, calls
   `j_GetActivationBitfield`, then reads selector word `$FFFF` into `d2`.
2. Any nonzero word takes the set path: OR `AIBITFIELD_AI_CONTROLLED` (4) into the old
   activation word, then call `j_JoinForce`. `$FFFF` is a nonzero selector, not the value
   written over the entire activation field. Zero instead clears only the AI bit
   (mask 4) and does not call `JoinForce`.
3. `code/common/stats/battleparty.asm:JoinForce` sets the joined flag, calls `UpdateForce`,
   and if the rebuilt active count is below `FORCE_MAX_SIZE`, calls `JoinBattleParty`.
   It preserves `d1`, so the handler subsequently stores the ORed word through
   `j_SetActivationBitfield`. The jump aliases resolve to the named stats helpers;
   activation accessors in `combatantstats_1.asm` / `_2.asm` use the combatant word field.

For this pinned source, `sf2enums.asm` gives joined base 0, active base 32, Peter 7 and
capacity 12: Peter's joined flag is F7; his active flag is F39. The ordinary absent,
under-capacity case joins and activates him; at capacity it joins without setting a
previously clear active flag. An already active Peter stays active. An already joined
reserve Peter can become active if room exists. Repeating the nonzero command repeats
the rebuild/capacity decision and reasserts AI control, rather than adding another member.
Unrelated activation bits are preserved; there is no HP reset, naming/join-music command,
F67 write or follower installation in this handler. These numeric source facts do not
expand the implementation-neutral contract's deliberately symbolic capacity boundary.

`UpdateForce` precedes the possible active-flag write; no second rebuild occurs within
`JoinForce`. Its return lists can therefore lag a newly set active flag. Later normal
text processing in this very scene is relevant: text 1234's `{LEADER}` calls
`UpdateForceAndGetFirstBattlePartyMemberIndex`, hence another `j_UpdateForce`, before
looking up the name. It must not be omitted from a composed list-state account. Debug
text skipping can bypass that text-side rebuild. Neither helper-local fixtures nor this
static chronology establish the exact observed lists at town control.

Commands 62-66 call `csc2C_followEntity` with these target/leader/position-index operands:

| Follower | Leader | Table index / signed offset pair |
| --- | --- | --- |
| Peter | Bowie | 2 / `(-24,0)` |
| Sarah | Peter | 2 / `(-24,0)` |
| Chester | Sarah | 2 / `(-24,0)` |
| Jaha | Bowie | 5 / `(-24,-24)` |
| Kazin | Bowie | 6 / `(-24,24)` |

`sf2cutscenemacros.asm:followEntity` encodes all three operands as words. In table
order the exact triples are `$0007,$0000,$0002`; `$0001,$0007,$0002`;
`$0002,$0001,$0002`; `$0003,$0000,$0005`; and `$0004,$0000,$0006`.
`csc2C_followEntity` first reads `move.b (a6),d0` without advancing A6, so each
first word's high byte is zero. `AdjustScriptPointerByCharacterAliveStatus` checks
character 0/Bowie's HP for all five calls. Bowie HP=0 skips each call's six operand
bytes and returns without `AddFollower`; Peter or another named follower's HP=0
with Bowie alive does not itself skip these calls. This probe does not validate entity
presence or separately check the named follower/leader HP. The
[existing high-byte seam observation](./common-scripting.md#confirmed-map-script-entity-gesturerelationshipmotion-command-family)
retains a controlled handler case, not this audience composition.

When the guard permits execution, the handler resolves both character references,
reads the indexed pair from `table_FollowerPositions`, then calls
`code/common/scripting/entity/entityfunctions_2.asm:AddFollower`. That helper installs a
42-byte `eas_Follower1` walking block with leader/offset words and adds the physical
entity to `EXPLORATION_ENTITIES` unless already present. Duplicate list membership still
allocates/replaces the walking script. These commands do not set story follower flags
or join/activate allies. Missing mappings, supplied followers and dead allies alter this
composition; the table is not a promise of visible final positions.

F67 is written only by `Map7_ZoneEvent0` after the script returns. Destination entity
initialization clears/rebuilds the temporary actors and uses
`data/scripting/entity/followers.asm:table_Followers` row
`follower 67, ALLY_PETER, $FF, 1`. `InitializeFollowerEntities` tests this flag and
allocates Peter's map entity; it does not test F7/F39 or the AI bit as this row's admission
predicate. Map6 is neither a no-follower map nor an overworld map, so the ordinary table
applies even if caravan flag F65 is supplied. Other follower flags remain independent
inputs; this scene does not set Sarah/Chester/Jaha/Kazin persistent followers. Astral's
narrative staying behind is not a dismissal operation or a clear of supplied F70/F74.

## The narrated gold gift

**Confirmed at the reviewed static boundary:** command 76 is `nextSingleText $FF,ENTITY_NONE`,
text 1234 (`04D2`). The string consists of the leader substitution, ordinary text with
literal digits 100, a newline and W1 wait. It has no numeric reward operand or hidden
gold instruction. The complete 88-command program has no `custom` call or gold-mutating
command; the zone caller after it writes only F702 and F67.

`mapscriptengine_2.asm:csc00_displaySingleTextbox` calls `DisplayText`,
increments the dialogue cursor, closes portrait/text and sleeps. The relevant chain in
`code/common/scripting/text/textfunctions_1.asm` is:

- `DisplayText` selects/decodes the string; ordinary symbols enter graphical/typewriting work.
- `ParseSpecialTextSymbol` dispatches leader `$F3`, newline `$EF` and wait `$FA`.
- `symbol_leader` rebuilds force lists, obtains the first active member's name and copies
  its characters; it does not alter gold. This is a real non-presentation side effect,
  distinct from crediting money.
- The newline branch changes dialogue layout/cursor; `symbol_wait1` waits for player
  input with VInt/RNG work. Neither calls a gold service or writes `CURRENT_GOLD`.

The actual mutation services in `code/common/stats/gold.asm` are `SetGold`, `IncreaseGold`
and `DecreaseGold`; none is reached as a gift award in this script/text/caller chain,
and no direct `CURRENT_GOLD` write or 100-valued award parameter is present there.
Thus this source does not implement the advertised +100 award. This is a bounded
narrative/state mismatch, not an assumed bug repaired in research. **Unknown:** observed
before/after gold in a naturally reached retail audience, authorial intent and any remake
decision about the discrepancy. No balance rule, translation or runtime task follows.

## Warp, caller writes and the destination control read

**Confirmed:** command 86's `csc07_warp` writes word event type 1 followed by byte
parameter 1=0, map=6, X=11, Y=1, facing=Down. It does not change `CURRENT_MAP`, enter the
destination, set F702/F67 or set `WARP_SFX`. Command 87 ends the script; the interpreter
conditionally waits for view scrolling when a dialogue window is present, clears view
scrolling speed, restores registers and returns. The caller then sets F702 followed by
F67. Therefore
membership/AI and the temporary chains precede request production, while the persistent
follower/setup flags follow it and precede ordinary request consumption.

`RunMapSetupZoneEvent` then closes portrait/text, waits one VInt and waits for physical
entity 0 to stop before restoring registers. It does not wait for the whole follower
chain and does not clear the new request. After type-6 return, the exploration loop
re-enters `WaitForEvent`: if pending is nonzero, it bypasses installing ordinary control
and returns the request to `ProcessMapEvent`, which clears it before dispatch. The
no-scroll type-1 handler plays/clears the supplied warp SFX, idles main, removes the
exploration return frame and returns Map6 `(11,1)`, Down to `MainLoop`. It is a mutable
pending slot, not a queue: interruption or an intervening overwrite breaks this chain.

Map6 has no `SwitchMap` substitution or `CheckBattle` coordinate row; ordinary exploration
entry follows. It clears pending, applies the accepted living/immortal heal, selects Map6,
rebuilds entities/sprites, clears temporary flags 256-383, sets F80, loads map and runs init.
F7/F39/F67/F701/F702/F706/F707/F411 lie outside the temporary-clear range. The destination
rebuild replaces the audience's temporary follower scripts; F67 independently readmits
Peter. Main starts at the explicit `(11,1)`, Down tuple under the ordinary entry assumptions.

F702, now set, wins the Map6 setup order over F701. `ms_map6_flag702_Entities`
(`s1_entities_702.asm`, `0x54786`) has 14 NPC declarations (references 128-141), no fixed
Peter or minister. Peter is present through follower selection, not through this list.
The shared init (`0x54E54`, retained body 10) preserves the existing alternatives:

| Init input | Program/result with the F702 population |
| --- | --- |
| F802 set | `cs_54E70` sets reference 132 (priest) to `(11,18)`, Down; F706 is not tested. |
| F802 clear, F706 set | `cs_54E78` sets 138 (worker) to `(63,63)`, Down, then 139 (guard) to `(15,23)`, Down. |
| F802/F706 clear | Return without a script or repositioning. |

Ordinary completion of the early audience carries F802 clear, so the last two rows
are its alternatives; F802 set is a supplied-state control. These init scripts write
no flag/request and perform no join/gold award. They do not restart the Peter event.
Following the conditional palette/music work, `WaitForEvent` reads pending:

| First destination read | Source branch and stopping boundary |
| --- | --- |
| Pending nonzero | Bypass control installation and handle that pending request; do not claim usable control. |
| Pending zero | Set view target 0, call `SetControlledEntityActScript(0)`; with carried player type 0 install `eas_ControlledCharacter`. The next loop reads pending again and tests A/C. |

No unconditional pending clear occurs between init return and this read. The scene does
not assign player type or egress; nonzero supplied player type retains the vehicle branch.
An interrupted scene can leave joined/AI or temporary followers changed without F702/F67;
F702-set/F67-clear reentry skips the audience and does not repair the follower. Stop here:
actual control, natural travel, actor/service completion and save persistence remain Unknown.

## Coverage, discriminators and reproduction

Fixtures are under `tests/fixtures/h2/`; command names have prefix `uv run sf2 h2`.
These are existing evidence boundaries, not claims that their complete rails ran here.

| Relation | Existing owner and concrete remaining composition |
| --- | --- |
| Conditional warp, setup, population and init | `map-data-static-v1.json`, `map-content-static-v1.json`, `gameflow-static-v1.json`, `common-maps-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`; [map inventory](./map-data-inventory.md). Commands use those basenames. Direct reads bind Map6 north row, no Map6/7 substitution/battle row, setup priorities/shared pointers/lists and retained init bodies 10/11. Missing relation: natural movement/layout/occupancy to the warp and zone. |
| Audience predicate and caller order | `map-events-static-v1.json`, predicate/flag-route owners; commands `map-events`, `map-event-predicate-results`, `map-event-flag-route-selection`. Direct source binds the five zone rows, F802 priority and script → F702 → F67. Controls: F701 clear, F702 set/F67 clear, F802 priority, F707-clear population replacement, F805 anomalous staging. |
| Program, text, membership/AI and followers | `map-script-engine-static-v1.json`, `forceStateCommandFacts.activePartyCommandFacts`, `common-stats-static-v1.json`; commands `map-script-engine`, `common-stats`; party contracts linked above. Retained comparison and complete 88-command/39-text review. Existing `tests/fixtures/h3/force-state-active-party-v1.json` is handler-local evidence, not this audience; no H3 rerun. Controls: zero/nonzero selector, full party, joined reserve/already active, Bowie HP zero (all five follow calls skipped) versus Bowie alive/named follower HP zero (not skipped by these guards), duplicate or missing mapping, F67 independent of membership/AI, text-side list rebuild versus debug skip. |
| Gift text and missing award | Direct pinned `04D2`, command 76, display/token handlers, gold services and zone caller. The script fixture covers command shape, not an audience gold-delta assertion. Missing relation: naturally reached before/after `CURRENT_GOLD`; retain the static mismatch and runtime Unknown without adding a generic text comparator. |
| Request → caller flags → destination/control | `map-event-request-consumption-static-v1.json`, `common-scripting-static-v1.json`; [request consumption](./map-event-request-consumption.md). Shared producer/consumer evidence does not execute the full audience composition. Controls: interrupted script after join, overwritten request, independent F706/F802 init, other supplied followers and nonzero player type. |

A future necessary composition should reuse/extend these owners and bind source/caller IDs,
setup/history inputs, actor mappings, membership/activation words and counted-list moments,
request tuple, ordered caller writes, destination selection and the first pending/control read.
The current decision needs bounded source evidence, not a new fixture/platform or capture.
Unknowns above do not automatically authorize runtime work.

The retained payloads total 18,285,979 bytes (about 18.3 MB). One process with linear
selected reads/comparisons, estimated under 512 MiB and a one-minute comparison budget,
is sufficient; outputs are this prose and small ignored review/planner receipts. No
transcript, layout, capture, new environment or installation. Replan before leaving the
finite audience, its selectors, relevant handlers and destination init boundary.

Load `. ./local/private-inputs.ps1` in the owning checkout. Verify the locally selected
read-only `$upstream` with `git -C $upstream rev-parse HEAD` and `status --short`.
Run the existing [retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary)
for map-init/map-script-engine and its ancillary cutscene-source check; the latter is not
independent audience evidence. Select `cs_55A36` from `programCorpus.programs`, compare
all 88 `sourceLine`/`macro`/`arguments` records to pinned `scripts.asm`, and read its inline
actions. Resolve the 39 sequential text keys in `gamescript.txt`, especially `04CE`/`04D2`;
review the named join/follow/display/token/caller handlers, setup lists, follower row and
init bodies 10/11. No source, fixture, schema, counter or private payload is changed.

Record document/source/reference/link/private checks, `git diff --check`, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and any command
failures in the Issue/PR handoff. No normal/full/engine/H3/H4/Godot suite is selected by
this documentation-only agreement. Preserve prior results/inputs, freeze the clean pushed
Draft PR and leave independent acceptance/merge to main-gate; no cleanup or next Issue.
