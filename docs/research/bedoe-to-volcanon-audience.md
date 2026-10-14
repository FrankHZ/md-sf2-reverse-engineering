# Bedoe to the Volcanon audience

Evidence date: 2026-10-13. **Confirmed:** the selected Bedoe warp requests Map11;
its `(39,11)` zone caller runs `cs_57AAA` only with F722 clear, then sets F722 and
F711 after script return. The scene requests 37 conversation texts, joins Peter
to the force, clears his AI-control bit and attempts current Peter→Bowie following.
Active-party admission is capacity-dependent; F67 is unchanged. **Inferred:** this
is a candidate continuation of the accepted Bedoe audience. **Unknown:** natural
floor traversal, actual supplied state/actors, service completion, presentation and
continuous playability. The invitation in Bedoe is not the shrine's admission predicate.

## Agreement and provenance

Standard rigor under [Issue #788](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/788)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
map requests, membership and asynchronous actors need separate acceptance boundaries.
Sole tracked owner is this document; no shared-path or stacked dependency is needed.
Consume accepted `ba88cf6fd5e5ecd3a8c713368f95b0d12e0135f9`, particularly
[the first Bedoe audience](./bedoe-king-address-through-first-audience.md),
[map-event consumption](./map-event-request-consumption.md),
[common scripting](./common-scripting.md) and [common stats](./common-stats.md).

Scope is the selected Map10→11 request, necessary Map11 self-map entrance and
return discriminators, selected setup/population, one zone scene, its membership/
AI/follow consumers and first conditional restored control. Stop on Map11 after
that caller. Exclude natural floor/path proof, all Bedoe stairs, return-to-Bedoe
`cs_5766C`/king choice, Skreech, town/services inventory, later travel/Battle13,
new harnesses/fixtures/runtime and remake implementation. No inseparable scope
expansion was found. A battle-turn consumer is read only to interpret the changed
AI bit; it is not a new battle-flow investigation or observation.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained map-init/map-script-engine
payloads bind it and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload and cutscene-source comparisons pass; no fresh extraction or
composed scene observation is claimed. Fnnn denotes flag nnn. Table rows are
zero-based; scene records below are one-based retained command positions.

The reused pair totals 18,285,979 bytes. Plan: one process, linear selected reads,
under 512 MiB estimated peak memory and one minute for existing comparisons, plus
small prose/receipts; no new payload, parser, comparator or capture. Actual existing
comparisons completed in 0.081 seconds. Map11 `mapsetups/scripts.asm` is 7,412 bytes,
150 lines; the sole scene occupies lines 4–150, exactly **147 lines** as planned.
Read all records/text keys and the 17 inline movement bodies. No bulk publication,
map decoding or source export is needed. Reassess before expanding this bounded
input/output work; Unknowns do not authorize acquisition. Acceptance is direct
source/fixture/content/link/private-boundary review, committed dependency planning
and actual final-head CI, with the documentation-only local profile.

## Selected requests and map admission

**Confirmed:** `data/maps/entries/map10/6-warp-events.asm` ends with the selected
`mWarp 45,50`: no scroll, `MAP_VOLCANON_SHRINE=11`, destination `(8,34)`, Up.
It does not check F720/F721, F722, Peter's membership or F67. Those facts do not prove
walking from the prior audience through Bedoe's floors to the source coordinate.
Only the following Map11 rows are needed to distinguish the selected continuation:

| Row in Map11 `6-warp-events.asm` | Predicate and request |
| --- | --- |
| 0 | Any X, Y=19 → `MAP_CURRENT`, `(8,13)`, Down; shrine exit discriminator. |
| 1 | Any X, Y=12 → `MAP_CURRENT`, `(39,18)`, Up; selected shrine entrance. |
| 2 | X=8, Y=34 → `MAP_BEDOE=10`, `(45,50)`, Down; return discriminator only. |

All three are no-scroll requests. Arriving at `(8,34)` does not automatically
execute its reverse row: the movement/event producer must generate another request.
The selected entrance's `(39,18)` destination likewise does not prove a natural
walk to zone `(39,11)`; rows specify requests, not traversable floors or collision.

`code/common/scripting/entity/entityscriptengine_2.asm:loc_528A` requires masked
layout attribute `$1000` before `WarpIfSetAtPoint` in
`code/gameflow/exploration/exploration.asm`. That routine divides internal coordinates
by `MAP_TILE_SIZE=384`, scans eight-byte rows in order, accepts negative coordinate
bytes as wildcards and writes the first match's type/parameters plus warp SFX.
`MAP_CURRENT` is the encoded current-map sentinel, not an explicit Map11 reload.

In `code/gameflow/exploration/explorationfunctions_2.asm`, `WaitForEvent` polls
pending; `ProcessMapEvent` clears the type before dispatch. Parameter1=0 selects
`ProcessMapEventType1_Warp`'s no-scroll path: request SFX, idle main, unwind to
`MainLoop`, sample current position/facing, replace nonnegative X/Y with the row's
coordinates and return map/facing. `code/gameflow/mainloop.asm` calls `SwitchMap`
then `CheckBattle`. `data/maps/global/flagswitchedmaps.asm` contains no Map10/11
replacement; `data/battles/global/battlemapcoords.asm` has no Map10/11 row. Thus the
selected explicit Map11 request and the self-map request while current map is 11
select exploration, regardless of battle unlock flags. `CheckBattle` resolves the
current-map sentinel for comparison but retains it on the no-battle return.

On the explicit entry, `ExplorationLoop` heals living/immortal allies, fades,
sets current map and no-battle state, loads tilesets, obtains setup entities,
initializes mappings/population, loads sprites, clears temporary setup flags and
sets F80; map load/VInt setup and the selected init follow. Peter is an immortal
healing case in the accepted [entry owner](./battle12-outcomes-through-bedoe-entry.md),
not an actor-presence guarantee. On the self-map entrance, the same loop uses its
map-not-provided path: update main's properties/position and raft/follower placement,
then map load and init, **without rebuilding the fixed population/mappings**.
Healing occurs at loop entry in either case; it is not repeated just because a
zone caller runs. Supplied HP at that later boundary remains a distinct input.

## Setup, population and caller

**Confirmed:** `data/maps/mapsetups.asm` selects `ms_map11` with no flag override.
Its `mapsetups/pointertable.asm` selects the ordinary six sections.
`s6_initfunction.asm:ms_map11_InitFunction` (`0x57AA8`, retained init body 16) is
only `rts`: it neither admits the conversation nor repairs its flags or actor state.
F722 set does not replace Map11's setup, init or population.

`InitializeMapEntities` in `code/common/scripting/map/mapfunctions.asm` clears
mappings and initializes flag followers before fixed/walking/sequenced declarations.
The regular NPC lookup stream starts at 128; physical slots depend on followers.
Map11 is neither a no-follower map nor an overworld map, so F65 does not select
the caravan follower prefix. `data/scripting/entity/followers.asm` declares Peter 7
from F67, independently of F7/F39, HP and AI bit. There is no fixed Peter row.
The four regular rows in `map11/mapsetups/s1_entities.asm` are:

| Lookup | Declaration |
| --- | --- |
| 128 | Birdman `(8,19)`, Down, sequenced `ems_57A04` with Left,Left,Right,Right,Right,Left movement-direction bytes. |
| 129 | Birdman `(6,28)`, Down, walking origin `(6,28)`, radius 2. |
| 130 | Birdwoman `(37,14)`, Down, `eas_Init`. |
| 131 | Birdwoman `(41,14)`, Down, `eas_Init`. |

Bowie/main is physical 0, separately declared. The scene creates/reloads no actor
and never replaces lookup 128's sprite or action. Its dialogue addresses Volcanon,
but lookup 128 is the first birdman declaration, not a separately declared Volcanon
entity or physical slot 128. `GetEntityPortaitAndSpeechSfx` in
`code/common/scripting/entity/getentityportaitandspeechsfx.asm` reads the mapped
entity's current mapsprite. `data/spritedialogproperties.asm` maps BIRDMAN to
portrait NONE and `DIALOG_BLEEP_6`. This is conditional source selection, not proof
of what a composed shrine scene displays or sounds like. Supplied changed mappings
or sprites change the consumer input. The speaker attribution does not rewrite
population evidence or establish how the map depicts Volcanon.

`map11/mapsetups/s3_zoneevents.asm` has one `(39,11)` row and a default return.
The row targets `Map11_ZoneEvent0` at `0x57A72`; default is `0x57A70`.
The retained zone program 6 agrees with all six operations in order:
`chkFlg 722; bne.s return_57A86; script cs_57AAA; setFlg 722; setFlg 711; rts`.
There is one table reference and one script call site. F720/F721, F719, F711,
F7/F39/F67, HP and AI are **not** admission checks. In particular:

| Supplied state | Selected caller result |
| --- | --- |
| F722 clear, F720/F721 clear or set | Run scene; on successful return set F722 then F711. Prior audiences are not proven. |
| F722 clear, F711 already set | Still run scene; the later F711 write is idempotent. |
| F722 set, F711 clear or set | Return without script or either write; F711-clear is not repaired. No join/AI/follow outcome may be inferred. |

`loc_5294` in `entityscriptengine_2.asm` needs masked layout attribute `$1400` to
write type 6 and word X/Y from destination plus attempted offsets divided by 384.
`ProcessMapEventType6_ZoneEvent` idles physical 0, loads those words and calls
`j_RunMapSetupZoneEvent`. The wrapper resolves the current setup again, compares
low coordinate bytes in four-byte rows, accepts each 255 wildcard independently
and dispatches the first match/default. Matching coordinates alone are not a
runtime request, a floor-route proof or an independent HP/population check.

## Complete scene content

**Confirmed:** `cs_57AAA` at `0x57AAA` has **113 records: 100 opcode commands,
12 sleeps and one terminator**. Cursor 3151 yields 37 `nextSingleText` requests
3151–3187, keys `0C4F..0C73` in `data/scripting/text/gamescript.txt`: Peter 7 speaks
17 times and lookup 128 20 times; Bowie has no conversation request. All records'
source lines/macros/arguments and all 37 text keys agree with source and retained
corpus. The table summarizes every record and inline movement without publishing
the dialogue transcript. Movement actions are relative; camera tuples are view
requests, not player warps. Each movement below uses `entityActionsWait`.

| Records | Ordered staging, requests and attributed speech |
| --- | --- |
| 1–14 | Cursor 3151; wait Peter `eas_Init`; Bowie destination `(39,11)`, Left1; track Peter; Peter destination `(39,11)`, Up facing, Up1; Bowie Right1, Up facing; Peter Up1; Peter greets Volcanon 3151; camera `(34,3)`; sleep 120. |
| 15–27 | Welcome 3152, Peter's trip 3153, approval/question 3154–3155; Peter nod, sleep 30; his ground-dweller/friendship account 3156–3157, approval 3158; nod; camera `(34,5)`, sleep 30; Peter introduces a friend 3159. |
| 28–37 | Peter Down facing; Bowie Right1/Up facing; Peter Down2/Right facing/Left1/Up facing; Bowie Left1/Up facing; Peter introduces Bowie as from Granseal 3160. |
| 38–50 | Camera `(34,2)`; rumble, quake 2, sleep 90, quake 0; Volcanon's reaction/blame 3161–3162; rumble, quake 3, sleep 90, quake 0; camera `(34,6)`, sleep 5. |
| 51–62 | Start Peter `eas_2xRightLeft` **without waiting**; sleep 120; Peter Up facing and appeal 3163; camera `(34,2)`; Volcanon blames Grans for Zeon, describes his protection and refusal to forgive 3164–3166; rumble, quake 4, sleep 90, quake 0. |
| 63–75 | Camera `(34,5)`; Bowie Right1; Peter Right1; Bowie Up facing; Peter Up3 and plea/exoneration 3167–3168; camera `(34,3)`; Volcanon assigns collective responsibility 3169–3170; Peter asks advice 3171; Volcanon announces withdrawal of protection and demands self-reliance 3172–3173. |
| 76–91 | Rumble, quake 4, sleep 90, quake 0; wait Peter `eas_DeactivateAutoFacing`; Peter Down1; wait Peter `eas_Init`; Peter objects and compares greater devils' strength 3174–3175; Volcanon invokes destiny 3176; rumble, quake 4, sleep 90, quake 0; challenge to show courage/reseal the devil 3177–3178. |
| 92–101 | Peter Up1 and asks advice 3179; Volcanon directs Bowie to trust the jewel 3180; Bowie Left1 then Up2; Volcanon describes light/darkness and the jewel, then dismisses them until Zeon is defeated 3181–3183; Peter Up1 and appeal 3184. |
| 102–113 | Sleep 30; two waited Peter `eas_Jump` actions; sleep 30; Peter Down1; promise, criticism and offer of help 3185–3187; `join Peter`; `joinForceAI Peter,0`; attempt `followEntity Peter,Bowie,2`; end. |

The 12 sleeps request 815 frames, not a measured duration. There are five
`SFX_BIG_DOOR_RUMBLE` requests and five quake intervals with amplitudes 2,3,4,4,4,
each followed by a requested sleep 90 and explicit quake 0. There is no choice,
map warp/reload, inventory/jewel mutation or internal F722/F711 write. Volcanon's
protection, Zeon's history, responsibility, comparative strength, the jewel's
properties and Peter's promise remain attributed speech; their narration alone
proves neither a world mechanic nor a fulfilled later outcome.

## Motion and service boundaries

**Confirmed:** `mapscriptengine_1.asm:csc29_setEntityDest` reads the actor as a
word and has **no HP guard**. It writes destination X/Y in 384-unit tiles, computes
absolute travel magnitudes and signed velocities of magnitude 32 from current
coordinates, and waits for current X/Y to equal destinations unless operand bit 15
is set. Both scene actors use clear bit 15. This is waited travel, not `setPos`
teleportation or an action-pointer-idle test. No facing is encoded in `setDest`.
`csc23_setEntityFacing` instead probes the actual byte actor through
`AdjustScriptPointerByCharacterAliveStatus`: ally word HP zero skips it; HP 256 is
nonzero. Thus zero HP can suppress Peter/Bowie facing while destination/action/text
requests still occur. The scene does not heal or revive them.

`GetEntityAddressFromCharacter` masks the lookup byte, subtracts `$60` for NPC128+
and resolves/scales the mapping by `ENTITYDEF_SIZE_BITS=5`. Zero unmapped Peter
resolves physical 0; an existing `$FF` mapping resolves index 255 without a validity
check. Missing Peter is not safely skipped by destination/action/camera handlers.
F67-clear on fresh Map11 population therefore cannot be treated as an ordinary
Peter performance even though his stats may exist, be joined and be alive.

`csc2D_entityActionSequence` emits relative `esc04_moveToRelativeDest` actions;
the latter scales signed tile deltas by 384 and adds current coordinates, subject
to destination/obstruction services. Sprite projection shifts internal units by 4,
so a tile is 24 pixels. Each of the 17 inline bodies here contains one movement.
Waited action installation polls that actor's pointer for `eas_Idle`; it does not
wait on every actor or imply completion of a previously replaced action.
`csc15_setEntityActscript` has no general HP/presence check.

`data/scripting/entity/eas_actions.asm:eas_Init` restores speed 32/32, acceleration,
collision/obstruction, auto-facing/orientation and sprite properties before idle.
`eas_DeactivateAutoFacing` turns auto-facing off then jumps idle, so the following
Down1 retreats without demanding automatic Down facing; the next `eas_Init`
restores ordinary behavior. `eas_Jump` temporarily disables auto-facing, performs
relative motion/waits, restores it and jumps idle. `eas_2xRightLeft` is a facing
oscillation, not left/right displacement: Right/Left/Right/Left with sprite updates
and four waits 30, then idle. A separate sleep 120 is not its completion barrier;
`setFacing Up` does not cancel it, and the later Peter Right1 action replaces its
pointer. Do not infer that the oscillation necessarily finished before either.

With valid distinct mappings, completed unobstructed relative moves and ordinary
updates, the coordinate arithmetic gives Bowie `(39,9)` and Peter `(39,8)` just
before final following. **Inferred:** this conditional staging result is not
an observed endpoint or a location guaranteed after the follow action starts.
The source-supported operations above remain valid without that favorable state.

**Confirmed:** `csc24_setCameraTargetEntity` resolves Peter's current mapping,
without HP admission; each later `csc32_setCameraDestInTiles` clears tracking,
requests the given camera destination and waits for scrolling. `csc33_setQuakeAmount`
masks the low 14 bits; these plain operands select direct `QUAKE_AMPLITUDE` writes,
not its high-bit gradual-ramp branches. `csc05_playSound` sends the word to the sound
command service, without waiting for audible completion. The source requests do
not establish rendered shake, audio, synchrony or actual time.

`csc00_displaySingleTextbox` handles portrait/speech selection, scroll wait, text,
cursor advance, window close and sleep 10; debug skip takes its separate operand-skip
path. Nod and join also have services/waits. The terminator conditionally waits
for scrolling if a dialogue window remains and clears scrolling speed; it does
not restore player control. Handler-local observations in the existing scripting
owner do not observe this composed scene or its natural timing.

## Peter: membership, capacity, AI and following

**Confirmed:** record 110's `join ALLY_PETER` encodes opcode `$08` and word 7. In
`mapscriptengine_2.asm:csc08_joinForce` (`0x47398`), it clears current speech SFX,
waits for scrolling, reads/clears operand bit 15, requests ordinary `MUSIC_JOIN`
(not the sad path), and takes the ordinary single-ally branch rather than special
selector 128. `j_JoinForce` resolves through `s02_jumpinterface.asm` to
`code/common/stats/battleparty.asm:JoinForce`. Class/name dialogue indices are
then supplied to text 446 (key 01BE), followed by fade/input, close and sleep 10.
This additional join-message request is separate from the 37 cursor texts; repeat
membership does not suppress these services. No naming prompt is encoded here.

`JoinForce` sets Peter's joined flag F7, calls `UpdateForce`, compares the resulting
active count against `FORCE_MAX_SIZE=12`, and calls `JoinBattleParty` only below 12.
That helper sets F39 (`32+7`) and does not refresh lists. `UpdateForce` enumerates
joined allies and separates them by active flag into force/active/reserve lists;
its call here precedes any new F39 mutation. Consequences depend on supplied state:

| At join entry | Result after membership service |
| --- | --- |
| F7 clear or set, F39 clear, active count below 12 after F7 is set | F7 and F39 set; cached lists/counts still reflect the earlier F39-clear snapshot until another `UpdateForce`. |
| F39 clear and count at least 12 | F7 set, F39 stays clear; Peter is in reserve in that snapshot. No replacement member is selected. |
| F39 already set | It remains set, even if the count is full/overfull; no normalization or eviction. A previously unjoined Peter becomes counted by the post-F7 rebuild. |

There is no HP test, healing, new entity declaration or F67 write in these helpers.
`gameflags.asm:SetFlag` only ORs its selected bit; F7/F39 cannot silently change F67.
An already joined/active Peter still runs the service and text. A dead Peter is not
revived by joining. This is not `joinBatParty`'s replacement algorithm and is more
than follower recruitment.

**Confirmed:** record 111 encodes opcode `$54`, Peter 7 and selector 0.
`mapscriptengine_1.asm:csc54_joinForceAi` (`0x46FDC`) gets the activation **word**, then
selector 0 takes `andi.w #($FFFF-AIBITFIELD_AI_CONTROLLED),d1` and the common setter.
`AIBITFIELD_AI_CONTROLLED=4`, so the write is old word AND 65531: clear bit 2, preserve
all other bits. `GetActivationBitfield`/`SetActivationBitfield` in
`combatantstats_1.asm`/`combatantstats_2.asm` use the word at
`COMBATANT_OFFSET_ACTIVATION_BITFIELD`. This selector does **not** call `JoinForce`;
the nonzero branch would set the bit and join. Neither branch alters F67. Here the
prior record already performed membership, with its independent capacity result.

The actual consumer `code/gameflow/battle/battlefunctions/executeindividualturn.asm`
checks word HP first, then MUDDLE and the AI bit for `StartAiControl`, and later
checks sleep/stun, MUDDLE, the same bit and auto-battle before player-input versus
`ExecuteAiControl`. Clearing the bit removes this forced-AI condition, not every
AI/turn-admission condition. Membership, active placement, life/status and auto-battle
remain relevant; this field scene neither starts a battle nor proves a playable turn.

**Confirmed:** record 112 has follower/target/place words `$0007,$0000,$0002`.
`csc2C_followEntity` probes the first word's **high byte**, so its alive helper
checks Bowie 0's word HP, not Peter 7's. Zero skips all six operand bytes; nonzero
continues independently of Peter's HP. Valid mappings install Peter following
Bowie with selector 2's `(-24,0)` pixel offsets. `AddFollower` copies/patches a 42-byte
action, then appends the physical index unless already listed; pointer replacement
happens even for a duplicate. `esc03_follow` shifts offsets by 4 and applies its
facing/distance/layout rules. There is no immediate fixed-position guarantee or
wait for following to settle. Bad mappings can instead target physical 0 for both.

This changes current following, **not F67**. F67 remains exactly as supplied through
the scene and caller. With F67 set, a later full population rebuild can declare
Peter through the flag table, independently of his F7/F39/AI state. With F67 clear,
`join` and current `followEntity` do not add that future declaration. A self-map
transition retaining current mappings is different from a full rebuild. Neither
successful following nor the promise in dialogue proves future persistence.

## Caller return and stopping boundary

**Confirmed:** only successful script return reaches caller writes F722 then F711.
A stalled earlier service proves neither write; a stall in join presentation may
occur after membership changed but before AI clear/follow and the caller writes.
F722-set entry skips the entire sequence without repairing membership, AI, F67,
F711 or current actors. Completed flag state is not a reconstruction of scene history.

F722 subsequently selects Bedoe's alternate setup in `data/maps/mapsetups.asm`, as
owned by the accepted Bedoe documents. That selection does not run a return scene
now. F711 also suppresses Polca's fairy opportunity: `map13/mapsetups/s2_entityevents.asm`
`Map13_EntityEvent3` branches around `cs_58512`; init skips `cs_58310`, and the
first F710-clear zone branch skips that staging and moves 130 out instead. Setting
F711 here does not prove the fairy was dislodged or acquired, and does not set its
follower flag F69. No Polca/Bedoe return or successor scene is executed by this caller.

After either caller path, `RunMapSetupZoneEvent` closes windows, waits one VInt and
calls `WaitForEntityToStopMoving(0)`. This tests physical 0's X/Y against destination,
not `eas_Idle`, other actors or completion of following. Exploration returns to
`WaitForEvent`; only zero pending retargets camera 0 and calls
`j_SetControlledEntityActScript(0)`. `PLAYER_TYPE=0` selects `eas_ControlledCharacter`;
other supplied types choose their own actions. Service completion plus zero pending
establishes a **conditional control handoff**, not observed input responsiveness.
Stop at this first Map11 boundary. No automatic later travel is part of the claim.

## Reproduction and evidence limits

Fixture paths below are under `tests/fixtures/h2/`.

| Boundary | Existing evidence and direct selection |
| --- | --- |
| Selected route/setup | `map-data-static-v1.json`, `map-setup-static-v1.json`, `map-entities-static-v1.json`; named warp rows, map setup/list files, `WarpIfSetAtPoint`, `MainLoop`, `SwitchMap`, `CheckBattle` and exploration consumers. No natural route proof. |
| Init/caller | `map-init-static-v1.json` body 16; `map-events/zone-programs.json` programs 6/7; direct six-operation F722 caller, table row and default. No F720/F721 or history prerequisite. |
| Scene/text/actions | Retained `programCorpus.programs` entry `cs_57AAA`, lines 4–150, all 113 records, 37 keys 0C4F–0C73, join key 01BE and 17 inline movements; named `eas_actions.asm` bodies. No rendered transcript or completed scene observation. |
| Membership/AI/follow | `map-script-engine-static-v1.json` roster and active-party command facts; `common-stats-static-v1.json` service owners; actual join/list/flag/activation/turn/follower consumers named above. Existing H3 handler matrices in common scripting establish only their controlled seams, not this route; no rerun or new acquisition. |

Load `. ./local/private-inputs.ps1` in the command's process, select and verify the
clean pinned read-only `$upstream`, and run the existing
[retained payload/cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select the one entry label above from the retained corpus, compare each record's
source line/macro/arguments, and read the inline bodies independently. Increment
cursor 3151 for each normal text request and compare its four-digit hex key with
`gamescript.txt`; read join's separate text 446 directly. The summary fixtures do
not parse every selected table/population row or establish this composition alone.
No new parser, verifier, per-scene fixture or harness is introduced.

**Unknown:** natural floor/collision traversal and request order; actual supplied
flags, HP, statuses, active capacity, AI bits, mappings and actor positions; completion
of movement, nonwaited facing, camera/text/join services and pending requests;
rendered shrine/Volcanon, quake, audio, timing and responsive control; later following,
battle placement and continuous playability. Keep these independent of the confirmed
source chain, without an automatic runtime queue.

Record direct evidence/link/private/full-diff checks, completed failures, clean
committed `uv run sf2 verify plan --base origin/main --head HEAD` and actual final-head
CI in the Issue/PR handoff. No local blanket normal/full/engine/Godot/H3/H4 suite is
selected solely for prose. Preserve earlier receipts/failures and private inputs;
leave the sole-path clean pushed Draft PR and frozen writer for independent main-gate
review/merge, with Project Review / Verify and a successfully submitted completion
callback. No cleanup or next-Issue selection follows this boundary.
