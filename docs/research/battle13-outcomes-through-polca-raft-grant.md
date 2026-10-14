# Battle13 outcomes through Polca raft grant

Evidence date: 2026-10-13. **Confirmed:** Battle13 uses a leader-present outcome
and an F513-gated aftermath. That aftermath sets Luke's follower flag F75 and
requests Polca `(16,12)`, Down; it does not join or revive Luke. Fresh Polca entry
selects the completed-battle population. Its optional mayor interaction runs
`cs_58856` only after its higher-priority story and temporary-flag tests permit it.
Only script return reaches F716, F64 and raft tuple Map72 `(43,48)`.
**Inferred:** the attributed exchange depicts the mayor apologizing for yielding
to the devils and supplying a raft at Luke's insistence. **Unknown:** natural
interaction reach, actual actors/services, rendered events and river availability.

## Agreement, provenance and resource boundary

Standard rigor under [Issue #804](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/804)
and [the content epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
battle return, temporary conversation progress, actor lifetime and persistent raft
state need distinct acceptance boundaries. This is the sole tracked owner; no
shared paths or stacked dependency. The initial consumed accepted main is
`ae2a8f86929a3cd1774fbf558c74a21cd9204bf3`. Dependencies are accepted
[Battle13 admission](./bedoe-return-through-battle13-admission.md),
[Battle12 shared outcomes](./battle12-outcomes-through-bedoe-entry.md),
[Polca arrival/setup](./battle11-outcomes-through-polca-arrival.md),
[membership and following](./bedoe-to-volcanon-audience.md),
[battle cutscenes](./battle-cutscenes.md), [common scripting](./common-scripting.md)
and [request/raft consumers](./map-event-request-consumption.md).

Scope is row13 outcomes, complete `abcs_battle13` and `ce_4AD96`, fresh Polca
return, the selected mayor137 caller, complete `cs_58856`, immediate raft-state
consumers and conditional caller/control return. Stop before Y11/Y30 programs
`cs_58AE2`/`cs_58BEA`, Oddler departure, river travel/boarding and Battle14. No combat
simulation, floor-route proof, runtime acquisition, parser/fixture or implementation
change. Unknowns do not authorize acquisition; no inseparable expansion is needed.

Source paths below are relative to `disasm/` in
[pinned SF2DISASM `master` revision `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected read-only checkout is clean at that pin. Retained map-init and
map-script-engine payloads bind the pin and USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Existing payload/fixture and cutscene-source comparisons pass; this is static
source evidence, not a new composed observation. Fnnn denotes flag nnn. Table and
fixture-program indices are zero-based; scene records below are one-based corpus
command positions, including sleeps and terminators. Dialogue is paraphrased.

The afterbattle file has 42 lines, including its header and entity declarations;
its program spans 4–39. Map13 `scripts.asm` spans 352–513 inclusive for `cs_58856`,
exactly 162 lines, before `cs_58AE2` at 514. These bounded reads confirm the planned
approximately 210-line scope plus caller/consumers. Reuse the unchanged
18,285,979-byte retained pair in one process, with linear selected reads/comparisons,
estimated below 512 MiB peak and one minute of comparisons, and small prose/receipts.
Existing comparisons took 0.082 seconds; comparisons plus selected checks took
0.097 seconds. All 148 scene records and 34 mayor-caller operations match their
source lines/macros/operands; all 38 scene text comments match their text keys.
Twenty inline bodies and the entity declarations are separately reviewed. No new
comparator, per-scene fixture, export or capture. Reassess before material expansion.

## Battle13 outcome selection and return

**Confirmed:** `sf2enums.asm` defines `BATTLE_POLCA_VILLAGE=13`, `MAP_POLCA=13`
and `MAP_BEDOE=10`. Read the actual row13 selections, not Battle12's scripts:

| Source under `data/battles/` | Row13 result |
| --- | --- |
| `global/enemyleaderpresence.asm` | `-1`, nonzero leader-present byte. |
| `cutscenes/battleendcutscenes.asm` | `ms_Empty`. |
| `cutscenes/afterbattlecutscenes.asm` | `abcs_battle13`, `0x4ACF8`. |
| `cutscenes/afterbattlejoins.asm` | `0`, Bowie; the layout-included table, despite its unused-feature comment. |

After an individual action, `battleloop_1.asm` calls the defeated wrapper, death
processing and remaining-combatant count in that order. The wrapper in
`code/gameflow/battle/cutscenes/battleendcutscenesstart.asm` first requires Bowie's
word HP nonzero and enemy128's word HP zero. Either failed HP test skips both
script and leader tail. With F513 clear it runs `ms_Empty`; F513 set skips the
script but still reaches `battleendcutscenesend.asm:loc_47C48`.

The nonzero leader row appends every enemy128–159 with nonzero word HP to the
existing death list, retaining a `-1` sentinel. It does not require placed X and
does not directly zero all enemy HP. `ProcessKilledCombatants` performs death
services, records deaths, sets combatant coordinates to `-1`, clears status,
updates stats and moves mapped entities off-map. `CountRemainingCombatants`
requires nonnegative X and nonzero word HP; Bowie dead forces zero allies. The
loop tests allies before enemies, so simultaneous exhaustion loses. With admitted
HP state, completed services and counted allies remaining, the leader tail removes
remaining enemies from the count and permits victory. An empty script is not itself
victory. The later after-turn-effects death/count path has no additional defeated
wrapper at that point. These are the accepted shared consumers, not simulated combat.

| Outcome | Ordered effects and boundary |
| --- | --- |
| Victory | `battleloop_2.asm:BattleLoop_Victory` heals living/immortal allies, seeds the return map from `CURRENT_MAP`, updates force and selects its first battle-party member. It seeds X/Y from that combatant plus battle-area origin and facing from its mapped entity, zeroes parameter1, calls the after wrapper, clears F413, sets F513, reads parameters2–5 and returns D4=1. The seed need not be Bowie's coordinates. |
| Defeat | `BattleLoop_Defeat` calls `UpdateBattleUnlockedFlag`, requests exhaustion text363, restores Bowie's HP, halves gold by unsigned right shift, resolves Egress and returns D4=-1. The Battle4 exception does not apply; no new F513 completion or aftermath. |
| Egress / Angel Wing | `battlefunctions/battlefunctions_1.asm` consumes MP/item, performs text/flash services, calls `UpdateBattleUnlockedFlag`, unwinds the action frame and resolves Egress with D4=0. No defeat penalty or victory completion/aftermath. |

Unlike Battle12, `battleloop/getegresspositionforbattle.asm` explicitly selects
**Map10 Bedoe** for `BATTLE_POLCA_VILLAGE`, bypassing carried `EGRESS_MAP`.
`code/common/maps/egressinit.asm:GetSavepointForMap` and
`data/maps/global/savepointmapcoords.asm` row7 produce `(27,35)`, Up. F399 set on
battle entry excludes the game-start bedroom fallback. With supplied F64 set,
`raftresetmapcoords.asm`'s Bedoe row also resets raft state to Map72 `(43,48)`;
this is an existing unlocked-raft reset, not a raft grant from losing. The unlock
helper clears F413 only when F513 was already set; first-completion loss/Egress
leaves F413 available. No natural return route to Polca is claimed.

`ExecuteAfterBattleCutscene` tests F513 before dispatch. Clear runs the full
program below; set skips it, retaining the variable victory seed and leaving F75
as supplied. Both branches reach `EndAfterBattleCutscene` and `j_JoinForce(0)`.
`battleparty.asm:JoinForce` sets Bowie's F0, updates force and sets F32 only if the
active count is below 12. This tail does not join Luke, and happens after the return
member was selected. The after script does not test F715 or repair a missing
confrontation/history. F513-set/F75-clear is not a reconstruction request.

## Complete aftermath and Luke's follower state

**Confirmed:** `data/battles/entries/battle13/cs_afterbattle.asm:abcs_battle13`
has **31 records: 26 commands, four sleeps and one terminator**. Cursor2495 requests
IDs2495–2502 (`09BF..09C6`): six single-text commands and two continued texts.
Peter speaks twice, Luke five times, and one line uses narrator operands
`$FF,ENTITY_NONE`; Luke uses `$C0`, Peter `$0`.

| Records | Ordered requests, inline body and attributed content |
| --- | --- |
| 1–7 | Cursor2495; sound fade-out; `loadMapFadeIn Map13,11,8`; load `ce_4AD96`; wait Bowie `eas_Init`; set Peter `(15,13)`, Right; waited Luke inline animation-off/Left-orientation/sprite-update/idle. |
| 8–16 | Town music; `fadeInB`; sleep50; Peter asks if Luke is all right (2495); sleep50; waited Luke `eas_Init`, then `eas_Jump`; headshake Luke; Luke says he was knocked senseless (2496). |
| 17–24 | Sleep5; start Luke `eas_2xRightLeft` without waiting; sleep60; Luke asks whether Bowie defeated the devils (2497); face Luke Up; Luke thanks him and praises his fighting (2498); face Luke Left; Peter praises Bowie and criticizes Volcanon's judgment (2499). |
| 25–31 | Luke expects Volcanon to learn (2500), proposes seeing the mayor for a raft (2501); face Luke Up; narrator says Bowie takes Luke along (2502); set F75; request Map13 `(16,12)`, Down; end. |

`ce_4AD96` at `0x4AD96` declares main `(16,12)`, Down, then Luke `(16,13)`, Down,
`eas_Init`, then ends. There is **no fixed Peter row**. `loadMapFadeIn` initiates
fade-out and falls into the map-loading handler: tile loading, fade/VInt waits,
map loading with camera operands `(11,8)`, display/interrupt restoration. Those
operands are not the player's destination. `loadMapEntities` disables display/
VInts, reads the main tuple, calls `InitializeMapEntities`, loads mapsprites and
restores services. It replaces the battle population; it is not the ordinary
F513 town setup and does not run that map's init or clear temporary flags.

`InitializeMapEntities` clears mappings, builds flag-based followers, then fixed
rows and main. With only F67 among existing follower flags, Peter is physical1,
Luke physical2 and main physical0. With no follower flags, the fixed Luke is
physical1 and absent Peter's untouched mapping resolves physical0. Supplied F75
instead builds Luke as a follower first and skips the fixed Luke row because his
mapping is already nonzero. Other follower flags alter allocation. Do not infer
a fixed physical index or valid Peter from text or `setPos`.

The accepted lookup masks the character byte, adjusts NPC128+ by `$60` and reads
its physical mapping; zero is not a missing-actor safe skip and `$FF` is not validated
as a live actor. `setPos`/`setFacing` skip allies below30 only on zero **word** HP;
HP256 passes. Actions, gestures, destination requests and speech do not inherit
that guard. Luke's initial custom body changes animation counter/orientation and
sprite, not HP or membership; `ac_motion OFF` is not a position freeze. Later
`eas_Init` resets movement/obstruction/auto-facing and sprite parameters, and
`eas_Jump` requests its up/down displacement and returns to idle. Neither revives.
The nonwaited `eas_2xRightLeft` requests alternating facing/sprite updates with four
30-frame waits, not a horizontal walking path. A script sleep60 is not its join;
subsequent facing/text can overlap its progress. The four scene sleeps total165
requested frames, excluding services and debug skipping, not elapsed time.

F75 changes future flag-based follower construction. It neither writes Luke's
membership F10 or active-party F42, calls `join Luke`, changes force-AI activation,
nor establishes a playable battle turn. The shared victory/exploration heal restores
living/immortal allies' HP/MP and preserves STUN/POISON/CURSE; Peter7 and Lemon28
are processed even when dead, but dead Luke is not revived. A dead Luke can still
be declared by these actor paths with his death mapsprite. Narrated recovery is
therefore an attributed presentation, not an HP or recruitment consumer.

`csc07_warp` writes pending type1, parameter1 zero and the four map/X/Y/facing bytes.
It overwrites the victory seed; it does not immediately enter exploration. Order is
**F75 → pending tuple → script return → Bowie join tail → clear F413 → set F513 →
read return tuple**. A stalled service can leave earlier mutations without later
ones; no atomic transaction or repair promise exists. The terminator conditionally
waits for scrolling when a dialogue window remains, then resets scrolling speed.

## Fresh Polca population and conditional control

**Confirmed:** `mainloop.asm:alt_MainLoopEntry` calls `SwitchMap` then exploration
without immediate `CheckBattle`. Map13 has no flag-switched replacement. The intact
first-completion tuple therefore enters fresh Polca `(16,12)`, Down; the F513-set
repeat uses its seed instead. The prior pending slot is cleared at exploration
entry, not replayed as a second warp. Future ordinary warp admission retains the
accepted independent F413/F513 behavior; no immediate battle retry is implied.

For supplied Map13, `ExplorationLoop` adjusts step count, heals living/immortal
allies, stores map/not-in-battle, rebuilds entities/mapsprites, clears temporary
F256–383, sets F80, loads map and runs init. `GetCurrentMapSetup` scans overrides
with priority **F513 > F799 > F710 > default**. Victory set F513 before this lookup.
The after-scene Luke pose/action and fixed `(16,13)` placement do not survive as
instructions to this fresh population.

`map13/mapsetups/s1_entities_513.asm` supplies thirteen regular NPC rows128–140,
no fixed Luke or Peter. Both depend on F75/F67 follower declarations, independently
of membership, active party, HP or AI. Polca is not an overworld/follower-excluded
map, so F65 does not add the caravan prefix. Selected population facts are:

| Lookup | Declaration and identity | Physical slot with only F67/F75 followers |
| --- | --- | --- |
| Peter7 / Luke10 | Follower prefix, in that order | 1 / 2 |
| 128–132 | Centaur man/woman, kappa, two merchants | 3–7 |
| 133 | Beastman at `(23,6)`, Right, `eas_Init`; addressed as Ripster | 8 |
| 134–136 | Priest and two walking beastmen | 9–11 |
| 137 | Man at `(15,3)`, Up, `eas_Init`; addressed as mayor | 12 |
| 138 / 139 | Dwarf `(38,6)` Up / orc `(63,63)` Down | 13 / 14 |
| 140 | Oddler `(14,4)`, Right, `eas_Init` | 15 |

These are lookup identities, not universally stable physical slots. Other follower
flags change allocation; F68 can additionally declare an Oddler-sprite follower
under a different lookup, not replace140. Current alternate/mutated mappings must
not be relabeled by dialogue. `GetCurrentMapSetup` selects an event table afresh;
a supplied F513 write without rebuilding actors can select these handlers against
a different population. Fresh victory return supplies the rebuild; arbitrary state
does not.

Shared init `s6_initfunction.asm:ms_map13_InitFunction` at `0x582E0` checks F711,
F716, F718 in order. F711-clear calls `cs_58310` to stage130 with facing, animation/
orientation/sprite service then `startEntity`. F716-set faces137 Down. F718-set
moves140 to internal `$7000,$7000`; it does not delete the mapping. None of these
branches hides133, grants/repairs the raft or invokes the mayor conversation.
After init/music/fade services, `WaitForEvent` resets camera target0 and calls
`j_SetControlledEntityActScript(0)` only with pending zero. That is conditional
control restoration, not observed responsive input or an automatic mayor contact.

## Optional mayor caller and temporary contacts

**Confirmed:** `s2_entityevents_513.asm` selects lookup137 at row9 with **RIGHT=0**
and `Map13_201_EntityEvent9` (`0x581A2`). The direction-valued byte is wrapper bits,
not a requirement to approach from the right. `ProcessPlayerAction` must first
select an actual actor via `GetActivatedEntity` and recover its event identity via
`GetEntityEventIndex`; adjacency/layout, view target and current mappings matter.
The row itself is not proof of that natural selection.

`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` obtains the setup and matching
row, prepares portrait/speech, resolves the actor mapping, and tests bits0/1 of
the row byte. Both are clear here: no pre-handler auto-turn toward Bowie and no
post-handler property/facing restoration. It clears the entity-update VInt flag,
calls the handler, closes portrait/text and reactivates entity updates on return.
The unconditional window cleanup still occurs when bit1 is clear.

All 34 operations in retained entity program 57 and source lines83–139 establish:

| Priority and supplied state | Selected request and writes |
| --- | --- |
| F718 set | Text1444: mayor reflects on avoiding the fight; return. |
| F718 clear, F717 set | Text1451: request to take the boy again; return. |
| F718/F717 clear, F716 set | Text1512: suggestion to inspect the raft; return. |
| All three clear, F260 clear | Text1448 asks to take the boy; after text returns set F260 and return. |
| Above clear, F260 set, F261 clear | Text1449 pleads for his life; after text returns set F261 and return. |
| Above clear, F260/F261 set, F262 clear | Text1450 repeats the plea; after text returns set F262 and return. |
| F718/F717/F716 clear and F260/F261/F262 all set | Execute `cs_58856`; only after return set F716, set F64, write `RAFT_MAP=72`, `RAFT_X=43`, `RAFT_Y=48`, then return. |

With freshly cleared temporary flags and unchanged higher flags, three completed
contacts prepare the fourth to run the scene. Arbitrary supplied temporary flags
can shorten that progression; missing an earlier flag selects its branch even if
later ones are set. Fresh supplied-map entry resets all three; same-map/no-index
entry bypasses that reset. Speech alone does not set the corresponding flag until
its service returns. The handler neither falls through multiple contacts nor tests
F75, membership, HP, actors, F64 or the existing raft tuple. An F716-set/missing-raft
mismatch selects repeat text and performs no repair. Supplied F717/F718 responses
are accounted for without asserting or investigating their later scenes/history.

## Complete raft conversation

**Confirmed:** `cs_58856` at `0x58856` has **117 records: 104 commands, 12 sleeps and
one terminator**. Cursor1487 yields 30 texts, IDs1487–1516 (`05CF..05EC`), 23 single
and seven continued texts. Speakers: mayor137 sixteen, Luke eight, Peter two,
Ripster133 three and narrator one. Peter uses `$C0`, narrator `$FF,ENTITY_NONE`,
all others `$0`. Inline actions below preserve every body; “wait” refers to the
named action's idle-pointer service unless `setDest` is identified separately.

| Records | Ordered requests, inline bodies and attributed text |
| --- | --- |
| 1–14 | Cursor1487; wait Peter then Luke `eas_Init`; mayor Down/sleep30/Up/sleep30/Down; wait one `eas_Jump`, start another; mayor cries out (1487), shivers, begs not to be possessed (1488), shivers again. |
| 15–24 | `setDest` Luke `(17,4)`, then Peter `(17,5)`; Peter Up, asks what mayor means (1489); Luke Down, dismisses him as confused (1490); wait Luke Left1, face Up, insist they are alive (1491); mayor reacts (1492). |
| 25–37 | Start Bowie Down1; wait mayor Down1; Bowie Up, mayor Right, Luke Left; sleep30; narrator describes mayor examining Luke (1493); sleep40; mayor acknowledges Luke is warm/alive (1494); two waited mayor jumps; sleep30; mayor Down. |
| 38–45 | Wait mayor custom speed `(10,10)`/idle, disable auto-facing, wait Up1, nod; mayor expresses regret (1495), says devils appeared (1496), claims no power to resist (1497), and says they offered safety for Oddler (1498). |
| 46–57 | Wait Luke Left1, face Up; Luke accuses mayor of sending him instead (1499); headshake mayor, apology and request not to tell King Bedoe (1500); sleep20; Luke Down/sleep50; Luke agrees (1501), faces Up/sleep20, conditioned on a raft for Bowie (1502). |
| 58–69 | Wait mayor `eas_Init`, wait jump, start another; thanks and agreement (1503); wait Luke then Peter auto-facing disable; Luke Left; start Luke Right2/Up1; wait mayor Down1/Right2; Luke and mayor Down; sleep20. |
| 70–81 | Start Peter Left1; wait mayor Down3; mayor Right; camera `(11,2)`; wait mayor jump, start another; call Ripster (1504); set133 `(23,7)` Left; wait133 custom speed `(48,48)`/idle then Left5;133 responds (1505); mayor orders raft for Prince Luke (1506). |
| 82–95 | Wait133 `eas_Init`, `eas_JumpRight`; sleep30; wait Left1;133 calls it the last raft (1507); wait mayor jump, start another; mayor dismisses objection (1508) and demands another be made if needed (1509); nod133;133 agrees (1510); wait custom speed `(48,48)`/idle then Right5; hide133. |
| 96–105 | Camera `(11,0)`; wait mayor Up3; sleep20; mayor promises a raft on the river (1511), suggests looking now (1512); wait mayor Left2/Up1; face Down/sleep30; wait Peter then Luke `eas_Init`. |
| 106–117 | Wait Luke Down1/Left1; face Down; Peter says mayor wants them gone (1513); Peter Up; Luke agrees (1514), says they now have a raft (1515); wait Luke Left1, face Down, urge departure (1516); follow Peter→Bowie, then Luke→Peter, both selector2; end. |

The nineteen inline bodies are sixteen movement sequences and three speed/idle
bodies. The twelve sleeps total350 requested frames, excluding services and debug
skipping. Text1487–1516 contains the promise and asserted receipt; there is no raft
flag/tuple write inside the script. Oddler140 is discussed but receives no command
here; his population identity is not a script action, transfer or recruitment.

## Service, following and actor-lifetime limits

**Confirmed:** `csc29_setEntityDest` consumes word entity/X/Y operands, scales tiles
by384 (1/16-pixel units), writes destinations, travel and signed32 velocity, and
waits for position to equal destination unless operand bit15 is set. Both selected
operands have bit15 clear: Luke then Peter are sequential destination waits, not
teleports or parallel fire-and-forget moves. The handler has no HP/presence guard.
`WaitForEntityToStopMoving` compares X/Y against XDEST/YDEST while waiting VInts;
collision, dynamic actor state and service progress can prevent completion.

The action installers replace pointers/timers, not queue every prior action to
completion. Waited forms poll the chosen pointer against `eas_Idle`; waiting for
the mayor does not wait for Bowie Down1, Luke Right2/Up1 or Peter Left1. Later
Luke/Peter `eas_Init` may replace their nonwaited actions; exact achieved positions
are Unknown. The mayor's second jump in each pair is also nonwaited and can overlap
speech, shiver or later commands. `eas_Jump`/`eas_JumpRight` request relative motion,
short waits, final `(0,0)`, auto-facing restoration and idle; this is not evidence
of actual rendered jumping. Speed10/48 values are internal parameters, not tiles
per frame. Gestures and camera destinations have their own sprite/scroll waits.
Single text closes windows and sleeps10; ongoing text/camera/audio completion is
separate from requested content. No wall-clock duration follows from these sums.

`csc2E_hideEntity` resolves133 then `entityfunctions_2.asm:HideEntity` writes its
position/destination to `$7000,$7000`, invalidates every mapping alias to that
physical actor with `$FF`, and removes it from the follower list if present.
It neither changes the population table nor writes a persistent Ripster flag.
A future fresh Map13 initialization redeclares133 at `(23,6)` from the F513 table;
shared init does not hide it. A map transition that retains mappings is different.
The scene has no subsequent133 lookup; its absence is not a durable departure fact.

Final `followEntity` words are `$0007,$0000,$0002` and `$000A,$0007,$0002`.
`csc2C_followEntity` reads the **high byte of the first word** before its HP helper,
so both calls test **Bowie0's word HP**, not Peter's or Luke's. Zero skips six
operand bytes; nonzero allows each call irrespective of those followers' HP.
With valid mappings, `AddFollower` copies/patches a 42-byte action, replaces the
follower's pointer and adds its physical index only if absent from the follower
list. Selector2 supplies `(-24,0)` pixel offsets to the follow consumer; it is not
an immediate position assignment or a wait for the chain to settle. The target
mapping also has no existence/HP guard. An absent Peter can therefore misresolve
as physical0 rather than produce the intended chain.

These calls configure current Peter→Bowie and Luke→Peter following; they do not
set F67/F75, F7/F39/F10/F42, revive anyone or modify force AI. The caller's raft
writes still follow script return even when Bowie's zero HP skipped both follow
commands. Persistent follower declarations remain controlled by supplied flags
and any earlier completed aftermath F75, not by the dialogue or current chain.

## Committed raft state, caller return and stopping point

**Confirmed:** only successful script return reaches caller writes **F716 → F64 →
RAFT_MAP72 → RAFT_X43 → RAFT_Y48**. These are separate writes, not an atomic grant.
A stalled earlier text/movement/scroll service establishes none of the later
writes; interruption between writes can leave partial state. The caller does not
queue a warp, change `PLAYER_TYPE`, board a raft or immediately declare one.

The accepted immediate consumers are `mapfunctions.asm:DeclareRaftEntity` at
`0x441AA` and `followersfunctions_2.asm:sub_44404` at `0x44404`. For ordinary player
state, both require F64 and `CURRENT_MAP==RAFT_MAP` before declaring the separate
raft from the stored bytes, each coordinate masked by `$7F` and scaled by384.
The declaration uses physical `ENTITY_RAFT=31`, mapping offset63, Left,
`MAPSPRITE_RAFT`, `eas_Standing`. `sub_44404` is called at the end of full entity
initialization; its supplied raft-player branch instead changes main's sprite,
stands followers and uses `(64,64)` for the separate entity. `DeclareRaftEntity`
returns immediately when already the raft player; otherwise it also relocates
ordinary followers, and on unlocked map mismatch clears the raft mapping and
moves its coordinates/destinations off-map. These distinct branches must not be
collapsed into unconditional spawn.

Thus Map72 `(43,48)` is a configured future spawn/unlock, **not a raft present in
current Map13**, an observed river resource, or a boarding result. Neither consumer
tests F716 as a substitute for F64/tuple. Conversely F64/tuple do not prove the
mayor scene ran: supplied state or an existing Egress reset can yield the same
values. The repeat F716 branch does not validate or repair mismatches. Actual
travel, load, collision, sprite presence and access are outside this endpoint.

After the mayor handler returns, the RIGHT=0 wrapper performs its unconditional
window cleanup and reactivates entity updates, without restoring the mayor's old
facing. `ProcessPlayerAction` returns to exploration's event loop. Only zero pending
lets `WaitForEvent` retarget camera0 and invoke the controlled-entity script service;
ordinary `PLAYER_TYPE=0` selects character control, other supplied types differ.
The scene/caller itself adds no pending event, but this is still a conditional
handoff, not an observed input receipt or proof all nonwaited actors finished.
**Stop at that return.** No automatic Y11/Y30 invocation, Oddler departure, raft
travel/boarding or Battle14 acceptance follows.

## Reproduction and evidence limits

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
select the retained read-only upstream and verify pin/clean status. Run the existing
[retained payload and cutscene comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary).
Select `abcs_battle13` and `cs_58856` from `programCorpus.programs`, compare all 148
`sourceLine`/macro/argument records to their stated spans, and separately read the
20 inline bodies and `ce_4AD96`. Select keys `09BF..09C6`, `05CF..05EC` and mayor
keys `05A8..05AB`, `05A4`, `05E8` from `data/scripting/text/gamescript.txt`; do not
export a transcript. Read all 34 operations of entity program 57 from the existing
`map-events/entity-programs.json` fixture's `expected.entityTargetPrograms`.

| Relation | Existing fixtures under `tests/fixtures/h2/` and direct source checks |
| --- | --- |
| Outcome/return | `battle-global-data-static-v1.json`, `battle-routing-data-static-v1.json`, `battle-cutscenes-static-v1.json`; exact row13, wrappers, shared death/count/heal/loop and Battle13 Bedoe Egress selection/savepoint/raft-reset rows. |
| Fresh setup/actors | `map-setup-static-v1.json`, `map-entities-static-v1.json`, `map-init-static-v1.json`; F513 population/event pointers, follower table, mapping/rebuild and init body 18 plus called `cs_58310`. |
| Scene/caller/services | Retained map-script-engine corpus and `map-script-engine-static-v1.json`; all records, inline bodies, text keys, entity program 57, RIGHT bits, destination/action/hide/follow handlers and request/return consumers. |
| Raft meaning | `map-event-request-state-static-v1.json`, `map-event-request-consumption-static-v1.json`; ordered caller writes and named declaration consumers, preserving supplied player/map/flag gates. |

Aggregate fixture shape alone does not prove every selected source row or composed
runtime relation. The direct scene/caller check is source-to-existing-record
comparison; no new extractor or fixture coverage is claimed. Completed predecessor
comparison and corrected navigation/fixture-read failures remain in
[Issue800's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/800#issuecomment-6158870823),
without ritual replay of the old scope.

**Unknown:** natural battle/contact reach, actual flags/HP/membership/AI and actor
mappings, exact concurrent positions/actions, all service returns, rendered/audible
presentation, input responsiveness, persistent save/load behavior, river availability
and boarding. No new runtime observation establishes those relations; these gaps do
not block this declared static outcome and are not an acquisition queue.

Record direct document/link/provenance/private/full-diff checks, clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`, actual final-head CI and
execution failures in the Issue/PR handoff. Documentation-only acceptance runs no
local blanket normal/full/engine/H3/H4/Godot suite. Leave a clean pushed Draft PR,
Project Review / Verify, frozen writer and successful authorized main-gate callback.
Independent review/merge remains main-gate's; no cleanup or next-Issue selection.
