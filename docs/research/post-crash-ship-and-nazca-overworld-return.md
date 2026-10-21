# Post-crash ship interactions and Nazca overworld return

Evidence date: 2026-10-20. **Confirmed (static):** fresh Map37 with F999
dispatches cs_5FABC before setting temp256. Astral's interaction runs
cs_5FB30 and hides his current physical entity before setting temp257.
The shared exit can request Map55; with434 clear, its999-held initializer
loads Map75 as a scene, rebuilds followers, moves Bowie and returns to
the existing exploration loop. **Stop at that initializer return and its
Map75 event/control context.** Natural travel, completed services, usable
control and saved persistence remain **Unknown**. No playable ship or
supported later-story package is established.

## Agreement and evidence route

Standard rigor for [Issue #1112](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1112)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect the post-crash scene, interactions and departure for later content
and engine decisions. Accepted base is
`c8c097a36c3b62221fab3cf5791867d731a69f47`. Own this document and navigation
only in [shootdown and fresh crash handoff](./nazca-ship-shootdown-and-crash-handoff.md).
No index, registry, schema, fixture, tool or engine change; no stacked
dependency. Reuse that predecessor's complete alternate31-record crew,
[boarding/shared ship services](./nazca-ship-boarding-and-departure-admission.md),
[voyage/follower allocation](./nazca-voyage-and-map59-arrival.md),
[Battle34 return](./battle34-outcomes-and-sky-orb-handoff.md) and
[Nazca route admission](./moun-to-nazca-and-battle34-admission.md).

Original paths below are relative to `disasm/` in clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The USA ROM identity remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/runtime equality claim is made. `sf2enums.asm`/`sf2const.asm`
own constants and RAM widths. `map/` and `entity/` abbreviate
`code/common/scripting/`; `exploration/` means `code/gameflow/exploration/`.
Fnnn means flag nnn; command ordinals are zero-based, intervals half-open.

Selected originals: Map37 alternate init `$5FAA4..$5FB30`, alternate
events `$5F9D2..$5FA22`, cs_5FB30 `$5FB30..$5FB6A`; Map55 init
`$5E268..$5E27C`, cs_5E320/table/cs_5E346 `$5E320..$5E37C`.
Use their complete defining files under `data/maps/entries/mapNN/mapsetups/`,
plus the named opcode, map-loader, follower, RNG and event consumers below.
No later route, Dojo, town, Battle35 or ending body is included.

Reuse the immutable18,285,979-byte static pair and existing provenance
`local/issue1005/pair.txt`; do not regenerate it. Selected linear reads,
one pair/provenance check and separate in-memory compiler invocations are
budgeted at one minute/512MiB per direct audit, with small uniquely named
ignored receipts under `local/issue1112/`. Peak memory is not measured.
No acquisition, runtime, SDK, Godot, screenshots or comparison platform;
reassess material scope/resource growth before expansion.

## First post-crash scene and return to event waiting

**Confirmed:** `s6_initfunction_999.asm:ms_map37_flag999_InitFunction`
always calls InitializeNazcaShipForceMembers, reads256, skips to @Return
if held, otherwise invokes cs_5FABC, sets256 only after it returns, then
rts. The predecessor owns fresh37 clearing256..383, resource healing,
fixed allocation, joined-based hiding and F80. Retained init does not
itself clear256/257 or heal. The complete30 command records of cs_5FABC
are below; nested motion records are counted explicitly.

| Ordinals | Ordered source work |
| --- | --- |
|0–1|textCursor3359; reloadMap6,6.|
|2–6|setPos Bowie0,8,12,Up; Peter7,13,12,Up; Zynk26,10,10,Down;128,10,11,Up; stopEntity128.|
|7–10|playSound MUSIC_STOP; fadeInB; nextText3359 then nextSingleText3360, both modifier80/speaker7.|
|11–18|Peter Right/wait20, Left/wait20, Right/wait20, Left/wait20. Four separate waits.|
|19–21|shiver Peter; nextSingleText3361/80/7; setActscriptWait Peter,eas_Init.|
|22–25|entityActionsWait Peter: moveLeft3; faceUp1; endActions.|
|26–29|nextSingleText3362/80/7; face Peter Left; nextSingleText3363/80/7; csc_end.|

`map/mapscriptengine_1.asm:csc46_reloadMap` detaches the view target,
deactivates installed VInt functions, passes d1=−1 and camera6,6 to
LoadMap, enables display/interrupts, reactivates installed functions and
waits one VInt. LoadMap's negative-index path retains CURRENT_MAP37 and
the current working layout, selects its area and refreshes display state;
this is not fresh exploration or another entity construction/init. No
temp clear or heal occurs in reloadMap. Reload does not prove a completed
fade, camera or frame service.

setPos/csc19 and setFacing/csc23 use
AdjustScriptPointerByCharacterAliveStatus: ally selectors0..29 with zero
HP skip their operands and operation; nonallies128 and follower31 bypass
that ally test. Living selected allies resolve the current alias, write
coordinates/destinations/facing and request sprite update. Joined status
is not tested. Thus a living unjoined26 can be positioned after the crew
helper hid his slot. Zero-HP unjoined26 remains hidden/skips; joined dead26
retains its fixed position when skipped. Fresh entry
has already revived immortal Peter7. No scene damage or revival is implied.

stopEntity is csc1C_stopEntityAnim: its same HP guard precedes alias
resolution and ANIMCOUNTER byte=−1. For128 it stops animation, not the
action pointer, motion, membership or event table. setActscriptWait and
entityActionsWait have no HP/joined guard; they install actions on the
resolved physical slot and await script-idle, not simply arrival. The
Peter path nominally ends10,12/Up before the final Left request. shiver,
text and waits are not HP-skipped merely because earlier positioning was.
The four waits are source requests, not measured elapsed hardware time.

On csc_end return, set256 executes; RunMapSetupInitFunction restores its
saved registers and returns to `explorationfunctions_2.asm:ExplorationLoop`.
That caller compares base/current palette data, conditionally requests
PlayMapMusic/FadeInFromBlack, then enters loc_2586A. WaitForEvent first
reads MAP_EVENT_TYPE.w: if zero, follows physical0 and calls
j_SetControlledEntityActScript with d0=0; otherwise it leaves control
installation alone and returns the pending event. The controlled-action
helper chooses by live PLAYER_TYPE (ordinary0 uses eas_ControlledCharacter).
Then the loop dispatches events or A/C player action. Pending/stale requests,
unfinished action/text/fade waits or unserviced VInts prevent a claim of
restored usable control. No new warp is published by cs_5FABC itself.

## Alternate interactions, RNG and Astral's departure

**Confirmed:** `s2_entityevents_999.asm` declares exactly four four-byte
rows plus the default record in this order. Labels describe logical
selectors; direction operands are handler flags, not required player facing.
Ordinary allocation is in the predecessor, not repeated here.

| Selector / ordinary physical slot | Flags / callback | Complete selected behavior |
| --- | --- | --- |
|Zynk26 /26|Up1 /Map37_3E7_EntityEvent0|txt3364; rts.|
|128 Astral /30|Up1 /Map37_3E7_EntityEvent1|Read257; held→return_5FA0E; clear→script cs_5FB30, set257, return.|
|129 Caravan /31|Right0 /Map37_3E7_EntityEvent2|rts; no local text/menu call.|
|Peter7 /7|Up1 /Map37_3E7_EntityEvent3|Read257; clear→txt3363 then return_5FA20; held→byte_5FA1C txt3370 then same return.|
|Default|Map37_DefaultEntityEvent|moveq4,d6; GenerateRandomNumber; move.w EAC,d0; add.w d7,d0; DisplayText; fall through event2's rts.|

These replace the first-visit tactical quote handlers; ordinary other
crew, including reserve/dead members when actually selected, use this
default instead of new HP/active quote selection. Admission still needs
the actual C-button interaction producer, reachable entity, reverse alias
lookup and live setup selection. The shared RunMapSetupEntityEvent owns
portrait/speech, flags-bit0 facing waits, entity-update suppression and
caller cleanup. Up1 has no flags-bit1 restoration; Right0 does not request
the facing wait. The physical-slot1 Caravan shortcut remains independent
of129/31. Astral128 is not FOLLOWER_B31. Raft overlap, missing aliases and
nonordinary state do not gain guards from these rows.

The actual RNG is `code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber`,
not GenerateRandomOrDebugNumber. d6.w=4 supplies the range, d7.w returns
0..3 and d6 is saved/restored. It updates RANDOM_SEED.w to
`s=(13*oldSeed+7)&65535`, doubles range to8 for unsigned multiplication,
swaps and shifts the product's high word right1. For this range the
result is `floor(s/16384)`. The selected text is3756+d7 (EAC=3756),
so3756..3759 inclusive. Old seeds0/1260/2521/3781 give updated
7/16387/32780/49160 and offsets0/1/2/3; these are arithmetic examples,
not observed RNG history or equal-probability/runtime claims. The callback
does not preserve a private copy of the seed or call the debug wrapper.
DisplayText's normal return reaches event2's rts; the outer event consumer
owns window closure/reactivation, not a missing local close command.

cs_5FB30 has the complete16 command records below. F257 is a temporary
conversation latch, not a persistent departure/join flag.

| Ordinals | Ordered source work |
| --- | --- |
|0–3|textCursor3365; setActscriptWait128,eas_Init; face128 Down; headshake128.|
|4–8|nextText3365,3366,3367,3368, then nextSingleText3369; all modifier00/speaker128.|
|9–13|entityActionsWait128: moveRight4; moveUpRight1; moveRight3; endActions.|
|14–15|hide128; csc_end. Only after return does the callback set257.|

With preceding position10,11 and ordinary completed actions the nominal
endpoint is18,10 before hide. The source does not warp Astral to Map75.
csc2E resolves the current alias without an HP guard, then HideEntity
sets physical X/Y and destinations to7000h, clears every alias pointing
to that physical slot to FF and removes its follower-list entry if present.
It does not erase the declared setup record, set HP, join/leave anyone,
clear70/76 or transfer inventory. This is stronger than BLANK-sprite use
and different from the crew helper's63,63 hiding with aliases retained.
An interruption after hide but before script return leaves257 unproven;
an injected repeat/missing alias must not be described as safely handled.

Fresh37 later clears both256/257, rebuilds fixed Astral128 and can replay
both scenes. Retained init with256 held skips the crash scene; retained
257 held skips Astral's scene and changes Peter's line. Fresh55 also clears
temps once; its subsequent scene load75 does not clear them again.

## Shared services and the actual exit request

**Confirmed:** pointertable_999 replaces entities/events/init but shares
ms_map37_ZoneEvents, AreaDescriptions and Section5. The shared zone/default
and description routines still return directly; no new local zone/story
gate is introduced. Chest25,8/collected185/NAZCA_CANNON51 and the empty
other-items table are unchanged. The [shared service owner](./nazca-ship-boarding-and-departure-admission.md#search-tables-and-nazca-cannon-chest185)
retains marker/search, inventory-room and rollback details: a still-clear185
can attempt that award; held185 gives the empty path. This slice does not
infer natural access or rerun the earlier complete chest audit.

The shared exact Item Use row8,11/Left/SKY_ORB113 is still selected by live
setup. With999 held, Map37_ItemEvent0 immediately returns with d6=0:
no836 read, item-name write, dialogue3373..3375 or cs_5FB6A call. The outer
item callback closes/waits as before; mainactions' zero result requests
generic73/422 and returns to the item submenu. No Orb removal or inventory
mutation occurs. Neither a cockpit position nor possession is a zone
trigger; injected item operands still do not prove possession. Refer to
the [existing complete item protocol](./nazca-ship-boarding-and-departure-admission.md#sky-orb-producer-callback-and-launch-boundary)
for unchanged producer and return semantics/text tokens.

`map37/6-warp-events.asm` remains one no-scroll row29,8→Map55,7,7,Up1
plus terminator. The actual controlled-movement opcode esc02 first
evaluates candidate movement/obstruction and the working-layout marker.
Only its1000h marker branch calls WarpIfSetAtPoint with candidate pixel
coordinates. That consumer divides by384, looks up CURRENT_MAP37's warp
table, matches29,8, writes MAP_EVENT_TYPE.w=1, copies payload bytes
0,55,7,7 and the facing/padding word, and requests WARP_SFX. No999,257,
256, joined, HP or Orb check is in this row/consumer. Thus talking to
Astral is not a declared exit prerequisite. Layout/collision still gate
the producer; standing or spawning at29,8 alone does not publish a warp.

WaitForEvent observes the request; ProcessMapEvent clears its type word,
and ProcessMapEventType1_Warp's parameter0 path requests/clears the SFX,
idles player and unwinds exploration to MainLoop with55,7,7,Up and d4=0.
These are asynchronous publication/consumption/return seams. The existing
[request producer](./map-event-request-state.md) and
[consumer](./map-event-request-consumption.md) own them. Overwritten,
interrupted or malformed requests do not gain queueing or completion proof.

MainLoop @Start calls SwitchMap then CheckBattle. The complete switch table
has no55 substitution. The sole55 battle row34 has wildcard triggerX/Y
and unlock434; ordinary accepted Battle34 victory has cleared434, so this
return enters exploration. Held434 admits Battle34 even with534 or999
held; if534 is already held, CheckBattle clears434 but still admits34 now.
**Stop on that changed-state battle admission; no battle body is included.**
No new battle check is automatically made by the later scene-map load75.

## Map55 initializer and the resulting Map75 context

**Confirmed:** fresh positive55 performs ordinary heal, CURRENT_MAP55,
CURRENT_BATTLE=FF, tilesets, empty-fixed setup plus live followers, mapsprites,
temp256..383 clear/F80, LoadMap and SetBaseVIntFunctions before its init.
`ms_map55_InitFunction` reads live999: clear→return_5E27A/rts; held→script
cs_5E320, script cs_5E346, same return. No new latch is set. Spawn7,7 does
not automatically run Map55's534-gated zone; that requires a later actual
zone marker/request. The two complete scripts and table are:

| Program / ordinals | Ordered source work |
| --- | --- |
|cs_5E320 /0–5|mapLoad75,8,14; loadMapEntities ce_5E33E; setActscriptWait Bowie,eas_Init; playSound MUSIC_MAIN_THEME; fadeInB; csc_end.|
|ce_5E33E|mainEntity12,19,Down; cscEntitiesEnd. No fixed nonplayer records.|
|cs_5E346 /0–6|textCursor3371; setActscriptWait Bowie,eas_Init; entityActionsWait Bowie: Left1, Left1, endActions; csWait30.|
|cs_5E346 /7–14|face Peter Right; Zynk Right; FOLLOWER_B Right; Bowie Right; nextText3371/00/FOLLOWER_B; face FOLLOWER_B Left; nextSingleText3372/00/FOLLOWER_B; csc_end.|

mapLoad is csc48: detach view, load target tilesets, wait for fading to
finish, deactivate installed VInts, LoadMap with explicit75/camera8,14,
enable display/interrupts, reactivate installed VInts and wait one VInt.
LoadMap's nonnegative path writes CURRENT_MAP75, loads palette/blocks/layout
and applies live flag block copies. F999 selects75's56,59/4×5→12,17 and
60,59/4×5→44,15 copies. The sole area is0,0..63,63, both foreground and
background origins0,0, both parallax256,256, autoscroll0,0, type254 and
default MUSIC_MAIN_THEME. CURRENT_BATTLE staysFF from fresh55, so LoadMapArea
uses exploration area bounds. Binary block/collision/marker values and
rendered crash placement are not decoded/observed here.

loadMapEntities/csc42 suspends VInts/display, calls InitializeMapEntities
with the table's12,19/Down seed, loads mapsprites and resumes services.
InitializeMapEntities clears entities/aliases and rebuilds live followers
before the empty fixed table/player0, resets SPRITE_SIZE16, terminates
the follower list and runs the raft/player-type helper. CURRENT_MAP is
already75, so Map37's follower exclusion no longer applies. Held65 on an
overworld starts at table_OverworldFollowers but continues through the
entire ordinary follower table to its real terminator, as the accepted
[follower owner](./nazca-voyage-and-map59-arrival.md) establishes.

For ordinary flags65/67/70/76, Caravan30 occupies1, Peter7 occupies2,
Astral/FOLLOWER_B31 occupies3, Zynk26 occupies4; player is0. This new Astral
is declared from the follower table despite the earlier hide128; it is
not a persistent deletion or transfer of128. Duplicate aliases and later
flagged rows still apply. Clear70 with no other31 producer leaves alias31
at its cleared0; actions/text/facing can then resolve player0 rather than
silently skip. Nonstandard raft/player-type state remains separate.

Bowie's nominal completed Left1/Left1 path ends10,19. Peter/Zynk facing
uses their HP guards; FOLLOWER_B31 bypasses that ally check and uses the
current alias. Text's speaker31 resolves current mapsprite/portrait/speech,
not a hardcoded Astral identity. The script never positions followers at
fixed endpoints, validates70/76 or changes joins/inventory. Missing live
actors/blocked motion are not repaired by dialogue intent.

This scene path does not rerun ExplorationLoop, heal again, clear temps,
set a new request, run SwitchMap/CheckBattle or invoke Map75's init. The
already installed base VInt set (planes/entities/view/scrolling/sprites/
windows/map animations) is suspended/resumed; SetBaseVIntFunctions is not
called anew by these scripts. Map75 init happens to be rts, but it is not
executed here. Resources were healed at fresh55, not by scene load75.
After cs_5E346 returns, Map55 initializer returns through its caller with
saved registers restored while CURRENT_MAP stays75. ExplorationLoop's
palette/music check and WaitForEvent use that context. A pending event
takes priority; otherwise control is installed on physical0 as described
above. Subsequent event/setup lookups and warp/search table selection read
live CURRENT_MAP75; Map55's7,7 zone is not automatically retriggered.
No next route/encounter or claim of actual playable return follows.

Across these selected scene/interaction/init bodies the only story-flag
writes are temp256/257 after their returning scripts. There is no new999,
70/76, battle unlock/completion, EGRESS_MAP, join/leave or item/HP/MP
mutation. The RNG seed write, entity hiding/rebuilding, fresh-entry heal
and temp clear are separate effects. A later actual chest transaction is
the unchanged shared service, not an automatic scene reward.

## Selected text references

Short authored paraphrases only; raw text stays ignored. N=nextText,
S=nextSingleText, E=txt with inherited event portrait/speech and no explicit
modifier, R=DisplayText after RNG with inherited event context. Every row
retains the complete ordered control tokens. A repeated ID is one message
used at different callers, not an extra sentence or inferred continuation.

| ID | Mode / modifier / speaker | Short summary | Ordered tokens |
| --- | --- | --- | --- |
|3359|N/80/7|rough-driving complaint|`["{NAME;26}","{N}","{W2}"]`|
|3360|S/80/7|asks for Astral|`["{W1}"]`|
|3361|S/80/7|spots Astral|`["{W1}"]`|
|3362|S/80/7|checks Astral's condition|`["{W1}"]`|
|3363|S/80/7; E/Peter event|calls leader over|`["{LEADER}","{W1}"]`|
|3364|E/Zynk event|questions roughness|`["{W1}"]`|
|3365|N/00/128|arrival relief|`["{N}","{W1}"]`|
|3366|N/00/128|pursuit warning|`["{LEADER}","{N}","{W2}"]`|
|3367|N/00/128|evacuation order|`["{W1}"]`|
|3368|N/00/128|urges departure|`["{N}","{W2}"]`|
|3369|S/00/128|leaves first|`["{W1}"]`|
|3370|E/Peter event|prepare before pursuit|`["{N}","{N}","{W1}"]`|
|3371|N/00/31|regrets ancient loss|`["{N}","{N}","{W2}"]`|
|3372|S/00/31|next destination proposal|`["{N}","{N}","{LEADER}","{W1}"]`|
|3756|R/default|groan|`["{W1}"]`|
|3757|R/default|disorientation|`["{W1}"]`|
|3758|R/default|rough journey|`["{W1}"]`|
|3759|R/default|pain|`["{W1}"]`|

Shared73/422 and chest text references/tokens remain in the shared owner;
the999-held Orb path adds no new local text. Spoken damage, evacuation,
destination or membership intent is not a gameplay-state mutation.

## Actual current consumers and acceptance limits

**Confirmed (current code):** separate actual OriginalPrograms invocations
in [program lowering](../../src/sf2tool/remake_exploration_programs.py)
register the complete selected defining file first, then all other selected
Map37/55 files. Each root's raw source path is checked. No failed instance
is reused as a successful cache; no repaired action/source is injected.
The pinned source has one definition of Map37_DefaultEntityEvent, physically
in s2_entityevents_999.asm and referenced by both setups. A reference is not
another definition; first registration must not let a foreign same-name
record substitute for this body. Local @Return is distinct from globally
registered return_5FA0E/return_5FA20/return_5E27A.

| Root | Actual result / boundary |
| --- | --- |
|ms_map37_flag999_InitFunction|ValueError target not imported:@Return; one native InitializeNazcaShipForceMembers partial only. Complete defining file was registered; local-label support gap, not omitted source.|
|cs_5FABC|Complete50 instructions, end-map-script, no native-call. Representation only.|
|Map37_DefaultEntityEvent|Three native calls: jsr GenerateRandomNumber, add.w, jsr DisplayText. Immediate registers are tracked but no typed RNG/text or event2 fallthrough/end emitted.|
|Map37_3E7_EntityEvent0 /2|3 instructions(cursor/show/end) /1(end).|
|Map37_3E7_EntityEvent1|4(branch-flag/call/set-flag/jump), return_5FA0E1(end), complete cs_5FB30 dependency25. F257 follows returning call.|
|Map37_3E7_EntityEvent3|4(branch/cursor/show/jump), byte_5FA1C3(cursor/show/jump), return_5FA20 one end.|
|cs_5FB30|Complete25 instructions ending end-map-script; typed motion/hide, no native-call.|
|ms_map55_InitFunction|4(branch-flag/call/call/jump), return_5E27A one end, dependencies cs_5E3206 and cs_5E34619.|
|cs_5E320|Complete6 emitted records, first native mapLoad, then scene-entities/motion/present/present/end-map-script. Successful emission does not implement the first service.|
|cs_5E346|Complete19 instructions ending end-map-script, no native-call.|

The exact50/25/19 counts are compiler records including text/window waits,
not original command counts or runtime frames. mapLoad remains native even
when75 is admitted in scene_maps: only loadMapFadeIn has typed scene-map
lowering in this branch. No selected special entity or missing local action
failure occurs in these scene bodies; the earlier special/local-action
failures remain preserved, not silently fixed or rerun.

[ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
maps native-call to UnsupportedInstruction; [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)
stops with program-opcode. It has typed position/facing/animation/motion,
reload and hide operations. Its direct position/facing/animation writes
do not acquire original HP guards by lowering alone. Hide uses actual
HideMapEntity/alias removal, distinct from visibility-only; it is not a
join/leave operation. ReloadScene requires bound scene/window/palette
state, and successful compiler output does not prove assembled package
reader admission, original service completion or presentation parity.

[MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)
owns interaction facing/portrait/text return cleanup when the matching
entity-event context exists; standalone lowering does not create it.
[SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)
uses the imported flag-selected population, which still omits original
Map37 follower exclusion. Its generic data does not bind this post-crash
sequence or guarantee current aliases/HP. The existing field event model
does not add the shared item-use/chest protocol. PrivateBattleEncounterReader
still binds encounters1/2/3 only. No SDK, reader, engine or Godot execution
is claimed. Minimum follow-up gaps are concrete content binding, local
init label/native crew/RNG/mapLoad services, original guard parity and
actual adapter services; this research dispatches no implementation.

One concrete changed-layout limit remains in
[MapTransfer.ReloadScene](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs):
it replaces working layout with world.Definition.Layout and resets its
copy lifecycle. Original csc46's negative LoadMap path retains the current
working layout. Successful lowering therefore does not establish parity
after a prior layout mutation; no implementation repair is included here.

## Verification, preservation and Unknowns

Reproduce after loading the current ignored selection in the same process:

`<registered-source-root>` is the registered disassembly directory
containing sf2enums.asm, code/ and data/.

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1112/review.py <registered-source-root> --committed --receipt local/issue1112/direct-reproduction-01.json
uv run sf2 verify plan --base origin/main --head HEAD
```

Choose an unused ignored receipt path each run; --receipt is required and
existing outputs are rejected. The direct recipe joins all selected source
records, helper/control/load boundaries, changed-state examples, text modes/
speakers/tokens and actual compiler/consumer results; checks exact two-path
scope/navigation, links, UTF-8/LF and private boundary; preserves pair
size/mtime. Clean committed checks, planner and actual exact-head scoped
CI are separate receipts. Planner suggestions are not executed passes.
No normal/full, old comparison, SDK/Godot/emulator/H3/H4, screenshots or
new maintained parser/schema/fixture is run or introduced.

Preserve #1110 original a83432f5/direct0.381s/CI39522799626 missed3754 order;
corrected577bdddc/direct0.380s/main0.377s/CI39522916959, then required-receipt
correction82cd1082/author0.164s/main0.163s/CI39523487961. Earlier special
entity/local action/native failures, #1108 continuous-follower correction,
#1106 operation/caller-cleanup correction and other historical failures
remain in the linked accepted owners and ignored handoffs. Twelve completed
old comparison failures are preserved; none is an interrupted/new queue.

**Unknown:** natural reach of crash/interactions/exit, actual membership,
HP, flags, inventory, follower/raft/alias/player/RNG/input/request state;
binary layouts/collision/markers and actual standing/trigger conditions;
completed movement/action/VInt/DMA/sprite/palette/fade/camera/portrait/text/
audio services, rendering/timing, usable control, saving and persistence.
No automatic zone/reboarding loop, repair of original behavior or next
route/battle completion is inferred. Freeze clean committed/pushed Draft
PR with exact head/tree/base, checks/actual CI, failures/Unknowns and stopped
process state; Project Review / Verify and successful main-gate notification
precede task stop. Independent acceptance/merge stays with main-gate; no
next Issue, merge, cleanup or archive.
