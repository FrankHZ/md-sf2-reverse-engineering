# Creed magic door and Desktop transfer admission

Evidence date: 2026-10-14. **Confirmed:** Map33 zone `(12,14)` checks F783,
not F774. When admitted with F783 clear, its complete `cs_5AF36` stages the
warning, concurrent shrinking actions, narrated pickup and collection, then
requests Map22 `(0,0)`, Down. The source scene writes no story flag. Its final
warp is pending work, not destination arrival. MainLoop can admit Battle22
when F422 is set, even though `(0,0)` lies outside that battle's area rectangle.
**Inferred:** the staging depicts the party becoming Goliath's collection.
**Unknown:** natural reach, actual supplied state, completed animation/services,
visible pickup, usable destination control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #896](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/896)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The novel work joins a complete source scene, its immediate action consumers
and actual destination admission. This document is the sole owned path; no
index, registry, contract, fixture, schema, tool, design or remake change.
Accepted starting base: `887c6f6772c0c5f785a3a3684a14a7b0cfd19d7e`, with no stack.

Consume accepted [Creed Mansion entry and first Goliath scene](./creed-mansion-entry-and-first-goliath-scene.md),
[map inventory](./map-data-inventory.md),
[setup selection](./map-event-flag-route-selection.md) and
[request consumption](./map-event-request-consumption.md).
Stop at fresh Map22 exploration entry **before population/init**, or at
admitted Battle22 **before its opening**. Destination population and Goliath's
Desktop arrival scene, later restoration/basement, Battle22 opening/roster and
whole-renderer/terrain research are excluded. No runtime acquisition or new
verification machinery is selected.

Source paths below are relative to `disasm/` in
[SF2DISASM master at `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This is static source and retained-evidence review, not a new ROM extraction.
Fnnn means flag nnn; command indices are zero-based. Coordinates, speeds and
waits are literal source operands, not measured motion or elapsed time.
Short map/entity consumer paths mean `code/common/scripting/map/` and
`code/common/scripting/entity/`, respectively; short Map33 setup paths mean
`data/maps/entries/map33/mapsetups/`.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair. Comparisons
are linear in those payloads, budgeted below 512 MiB and one minute, plus
bounded source reads and small prose/receipts. The retained comparisons took
0.079 seconds. No new payload, environment or capture is needed. Reassess before
material source/resource expansion.

## Zone admission and supplied actors

**Confirmed:** `s3_zoneevents.asm:5–7` declares `(15,21)` for Zone0,
`(12,14)` for Zone1, then the default. The complete Zone1 callback at lines
50–58 checks F783, branches to return when set, otherwise executes `cs_5AF36`
and returns. It does not check or set F774, set F783, or fall through into the
next scene. F774 clear and F774 set both admit this same callback when F783 is
clear; their earlier init/itinerary and actor positions need not be equal.

The accepted setup selector is evaluated at invocation. Baseline `ms_map33`
supplies this zone table; Map33's ordered overrides remain F523, F783, F784,
F786, F22, with last-set precedence. Pointer identity alone does not mean a
first visit. An alternate setup must actually supply the selected table before
the local gate applies. No new premise that the first Goliath scene completed
is added to this callback.

`code/gameflow/exploration/explorationfunctions_2.asm` clears the old event type
before dispatching type 6. Its zone handler applies the init action to physical
0, reads zone X/Y words from the request parameters and calls
`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent`. That wrapper selects the current
setup/table and matching row before invoking Zone1. A source coordinate and
callback do not prove the natural player route or actual event production.

The source assumes existing logical actors Bowie 0, Peter 7, B 31 and NPC 128.
It creates none. The predecessor owns the baseline six fixed rows and follower
allocation: Map33 is not overworld, so F65 adds no Caravan prefix; F67 selects
Peter, and B's independent F74/F68/F70 rows can allocate duplicate logical B
entries with later lookup overwrite. Under only F67/F68, Peter/B/128 resolve to
physical 1/2/3. Missing follower lookups can alias physical 0; another selected
B row changes attribution and action ownership. A WORKER mapsprite occupies
baseline logical 128, while text identifies Goliath. Party membership alone
does not supply these follower mappings.

`mapscriptengine_1.asm:GetEntityAddressFromCharacter` uses the logical lookup
table without a missing/alias guard. The action installers do not use the
ally-alive skip employed by some placement/facing commands. HP/class can alter
the mapped ally sprite without preventing a shrinking-action installation;
special sprites can bypass `ChangeEntityMapsprite` work. F774 does not repair
these inputs. Relative moves start from the actual actor position, not an
assumed endpoint of the prior scene.

## Complete scene order

**Confirmed:** `scripts.asm:192–246` is the entire `cs_5AF36`, ending at its own
`csc_end`, before `cs_5B016`. The retained corpus has 43 commands including the
terminator. All three inline action sequences and the one custom body are
included below; `endActions` and `ac_end` delimit their complete bodies.

| Commands | Ordered source actions and content |
| --- | --- |
| 0–4 | Set text cursor 1834; wait for 128's `eas_Init`, then `eas_Jump`; custom waited body sets speed `48,48`, jumps to `eas_Idle`, then `ac_end`; waited inline body moves Down 2, Left 2, then `endActions`. |
| 5–12 | Set camera target to 128; single text 1834 warns against entering. White flash 4; wait 40; white flash 4; wait 30; white flash 4; wait 5. |
| 13–21 | Install Bowie's `eas_2xRightLeft`; wait 5; install Peter's `eas_2xUpDown`; wait 5; install B's `eas_2xRightLeft`; wait 20; white flash 4; wait 10; white flash 30. These action installations do not wait for idle. |
| 22–26 | Install `eas_TwirlShrinkDisappear` on Bowie, Peter, then B, without action-completion waits; wait 60; single text 1835 rebukes the children. |
| 27–31 | Wait for 128's `eas_Init` again; waited inline Left 3, `endActions`; face 128 Up; continuing text 1836 explains the door, then single text 1837 gives the claimed exception. |
| 32–36 | Stop 128's animation; wait 20; single text 1838 narrates pickup with no portrait; restart 128's animation; single text 1839 announces collection. |
| 37–42 | `stepEvent 12,13`; waited inline Up 2, Left 5, `faceUp 20`, `endActions`; wait for 128's `eas_Jump`; wait 20; `warp MAP_DESKTOP_KINGDOM,0,0,DOWN`; `csc_end`. |

`sf2cutscenemacros.asm` encodes waited action variants with `$FF` and unawaited
variants with 0. csc14/csc15 install a script on the resolved physical actor;
their waited forms poll that actor's script address for `eas_Idle`. csc2D
compiles the inline sequence, appends a jump to idle, and likewise waits only
for its selected actor. Its `faceUp 20` compiles facing Up, sprite update,
then action wait 20; it is not a move of twenty tiles. These waits have no
timeout or missing-actor recovery and do not join the three shrinking actors.
The installers seed the action wait timer with the resolved physical index;
`esc00_wait` consumes that timer, so even the first action wait is not a
measurement derivable from its name alone.

The selected named helpers are complete in `data/scripting/entity/eas_actions.asm`:
`eas_Init` resets acceleration factors 0/0, speed 32/32, acceleration and
deceleration off, obstructability on, entity/map uncollidable off, autofacing
on, orientation Up, immersion off, size 24, sprite update, then idle. It does
not reset position. The second Init therefore replaces the earlier speed 48.
`eas_Jump` disables autofacing, moves relative `(0,-1)`, waits 5, moves `(0,1)`,
waits 5, moves `(0,0)`, restores autofacing and jumps idle. RightLeft and UpDown
perform their named four facing changes, each followed by sprite update and
wait 30, then idle. Later installations replace the same actor's action pointer;
the script does not first prove those look sequences finished.

The inline relative-move compiler and `entityscriptengine_2.asm:esc04_moveToRelativeDest`
use signed tile deltas scaled by MAP_TILE_SIZE=384, with destination/conflict
handling. Waiting for a selected action to become idle is not a general proof
that every actor followed an unobstructed intended itinerary. The speed operand
is consumed by `esc10_setSpeed`, not a measured pixel rate.

## Text, flashes and narrated collection

**Confirmed:** `data/scripting/text/gamescript.txt` keys 072A–072F are decimal
1834–1839. Each literal matches the corresponding selected script comment,
including controls. Five lines use speaker 128/modifier 0; the pickup line
uses `$FF,ENTITY_NONE`, encoded `$FFFF`.

| ID | Macro / speaker | Exact literal |
| --- | --- | --- |
| 1834 | nextSingleText / 128 | `No, don't go in there!{N}That door is...!{W1}` |
| 1835 | nextSingleText / 128 | `What stupid kids!{W1}` |
| 1836 | nextText / 128 | `Whoever passes through this{N}magic door becomes small.{W2}` |
| 1837 | nextSingleText / 128 | `That is, anybody who's not{N}protected by Mr. Creed.{W1}` |
| 1838 | nextSingleText / none | `Goliath picks up{N}{LEADER}.{W1}` |
| 1839 | nextSingleText / 128 | `Now, you're part of the{N}collection!{W1}` |

`mapscriptengine_2.asm:csc00/csc02` resolve the actual mapped sprite through
`getentityportaitandspeechsfx.asm` in the entity consumer directory for portrait
and speech SFX. Modifier 0 sets neither mirrored nor right-side bits; it does not guarantee a displayed
portrait. `$FFFF` bypasses that lookup and sets speech SFX to zero. The single
form closes portrait/text and sleeps 10 after DisplayText; the continuing form
leaves that closure to the following command. Thus 1836–1837 is a continuation,
not a choice branch.

`code/common/scripting/text/textfunctions_1.asm` maps `{N}` to line advancement;
W1 and W2 poll the current directional/A/B/C input mask. W2 additionally runs
the arrow helper and validation sound path. `{LEADER}` calls UpdateForce,
selects the first BATTLE_PARTY_MEMBERS entry and resolves its combatant name;
it is not an immutable literal "Bowie". Debug mode plus P2 Start can latch
SKIP_CUTSCENE_TEXT, bypassing the selected text commands and csWait sleeps.
The nominal six-text presentation assumes that skip is not latched.

`mapscriptengine_1.asm:csc41_flashScreenWhite` shifts the word operand right
three bits and runs LaunchFading with a DBF loop. Operand 4 therefore requests
one launch, and operand 30 requests four. Each launch selects all palettes
(`$F`), counter maximum 1 and FLASH_QUICKLY_2, waits through FADING_SETTING
clearing and another VInt, then restores the saved counter maximum. csc41 then
calls DuplicatePalettes. These are four single-launch calls plus one four-launch
call in the selected order, not proof of flashes lasting four or thirty frames.

csc24 maps the camera target to the physical actor for 128; it does not set a
fixed camera coordinate. csc1C/csc1B write animation counter -1/0. They stop and
restart sprite animation, not motion, action execution or input globally.
The pickup line is placed between those writes. No selected command attaches
Bowie to 128, records collection membership, recruits/removes a party member,
or performs a choice. **Inferred:** the ordered dialogue and staging express
shrinking and collection; the universal rule and Creed's protection are the
speaker's explanation, not a separately implemented universal door mechanic.

## Shrinking action and temporary state

**Confirmed:** `eas_actions.asm:369–411` contains the complete selected
`eas_TwirlShrinkDisappear`. It first waits 5, enables resize, then executes each
row below as facing change, size write, sprite update, wait 5:

| Order | Facing | Size operand |
| --- | --- | --- |
| 1 | Down | 22 |
| 2 | Right | 20 |
| 3 | Up | 18 |
| 4 | Left | 16 |
| 5 | Down | 14 |
| 6 | Right | 12 |
| 7 | Up | 10 |
| 8 | Left | 8 |
| 9 | Down | 6 |

The tail is exactly `ac_setPos 63,63`, `ac_resizable OFF`, `ac_setSize 18`,
`ac_jump eas_Idle`. There is no sprite-update command after that final size
write. Immediate consumers in `entityscriptengine_2.asm` establish the boundary:

- esc0B writes the **shared word SPRITE_SIZE**, not a per-combatant scale field.
  esc1F sets/clears resize bit 3 in this physical entity's flags B.
- esc0A requests ChangeEntityMapsprite, with sprite-load capacity affecting
  progress. ChangeEntityMapsprite skips special mapsprites or entity number 32;
  otherwise its resize-bit branch passes the current SPRITE_SIZE to
  `entityfunctions_4.asm:ResizeSprite`. That routine operates on decompressed
  mapsprite pixel data in RAM, including both walking frames. This is a sprite
  transformation, not a gameplay-stat, terrain-size or persistent actor-scale
  assignment. Concurrent actions share this size scratch word.
- esc0C scales 63 by 384, writes both current X/Y and destination X/Y and clears
  the travel pair. It does not deallocate the slot, clear its logical lookup,
  change force membership or issue a warp. The resulting X is below the entity
  scheduler's `$7000` empty-slot threshold.
- `entityscriptengine_1.asm:VInt_UpdateSprites` projects current position against
  the view and sends out-of-range sprites offscreen. Parking at `(63,63)` is
  therefore distinct from a guaranteed visible disappearance under every
  supplied camera state. No explicit `removeShadow` or shadow-restoration
  command occurs in this selected scene or named action; no independent shadow
  observation follows from the action's name.

After the three installations the map script sleeps 60 and continues; it never
polls all three action pointers for completion. Later waits concern 128 or,
in the wrapper, physical 0's coordinates. A stalled or replaced action, an alias
between logical actors, skipped sprite work, debug text-skip or changed supplied
state defeats an unconditional claim that all three visibly shrank. Even a
completed tail establishes only parked temporary actors, resize disabled and
the shared size write 18. It does not establish three persistently tiny gameplay
entities or the state of destination actors rebuilt later.

## Door copy, return and pending warp

**Confirmed:** csc47 scales `(12,13)` by 384 and calls
`code/gameflow/exploration/exploration.asm:OpenDoor`. With CURRENT_BATTLE=255,
the complete Map33 step table selects `4-step-events.asm:12–15`: source `(25,0)`,
size 1x1, destination `(12,13)`. Nonnegative source Y takes the copy branch:
one live layout word at FF0000, with 64-word row stride, is copied; SFX_DOOR_OPEN
plays and the plane-update bit selected by MAP_AREA_LAYER_TYPE is set. An
in-battle call returns without this work.

This consumer writes no persistence flag. Map33's complete `3-flag-events.asm`
contains only F22's large block replacement, not a replay of this event keyed
by F774 or F783. A live copy is not proof of rendered opening, passability,
successful traversal or save/load persistence. The final waited movement and
Jump follow the copy; their completion is required to reach the warp command.

`warp` encodes opcode 7 and bytes map 22, X 0, Y 0, facing Down=3.
`mapscriptengine_2.asm:csc07_warp` writes MAP_EVENT_TYPE=1 first, then parameter
1=0 and parameters 2–5=`22,0,0,3`. It neither loads Map22 nor waits for it.
This shared request is not a queued transaction or protected completion token:
earlier failures can leave partial scene mutations, and a later request writer
can replace the pending values. The route below assumes this complete tuple
survives until consumption; interrupted or interleaved writes are not an
observed atomic handoff.

csc_end conditionally waits for view scrolling if a dialogue window exists,
clears VIEW_SCROLLING_SPEED and restores caller registers. Zone1 then returns
without a flag write. RunMapSetupZoneEvent closes portrait/text, waits one VInt
and waits for **physical 0's current X/Y to equal its destination**. This does
not join Peter/B actions or guarantee their visible completion. A completed
shrinking tail has already set both current and destination to `(63,63)`, so
coordinate equality alone also does not prove normal exploration placement.

WaitForEvent sees pending MAP_EVENT_TYPE before reinstalling camera target 0
and the current-mode controller. With the surviving warp it returns immediately;
ProcessMapEvent clears the type before dispatching type 1. Its no-scroll branch
consumes and clears WARP_SFX, makes physical 0 idle, unwinds the exploration
return and reads the destination parameters. UpdatePlayerPosFromMapEvent first
reads physical 0's tile position/facing, but the nonnegative explicit X/Y bytes
override both positions and parameter 5 supplies Down. Thus parked source
coordinates do not change the requested `(22,0,0,Down)` tuple. No source-map
control handoff is promised between scene return and this transfer.

## Actual Desktop admission and stop

**Confirmed:** `code/gameflow/mainloop.asm:20–35` returns from exploration to
SwitchMap **then CheckBattle**, not the alternate post-battle entry that skips
CheckBattle. `code/common/maps/mapinit_0.asm` and the complete
`data/maps/global/flagswitchedmaps.asm` contain no Map22 switch, so it remains 22.

The complete `data/battles/global/battlemapcoords.asm` has one Map22 row:
zero-based battle index 22 at line 28, tuple `22,9,0,30,30,255,255`.
`code/common/maps/getbattle.asm:CheckBattle` first matches map, then checks
400+22=F422, then the trigger X/Y fields. Both 255 fields are wildcards. It
does **not** compare the request to the area rectangle. `(0,0)` is outside the
area starting at `(9,0)`, yet still passes both trigger checks when F422 is set.

On selection it writes area X/Y/width/height=`9,0,30,30`, then checks F522
(unlock flag plus 100). If F522 is set it clears F422, but still returns selected
battle index 22 for this invocation. This clear occurs after area assignment
and is not an exclusion from the current battle. The selected source zone,
scene and action contain no F422/F522 write; these are independent supplied
destination-admission flags.

| Supplied F422 | Supplied F522 | Immediate CheckBattle/MainLoop result |
| --- | --- | --- |
| Clear | Either | Map22 row skipped before area assignment/completed-flag check; scan returns d7=-1. Enter fresh ExplorationLoop with map 22, `(0,0)`, Down. |
| Set | Clear | Write Battle22 area; retain F422; return d7=22 and call BattleLoop with battle index 22. |
| Set | Set | Write the same area; clear F422; still return d7=22 and call BattleLoop with battle index 22. A later check needs its own supplied state. |

The exploration branch supplies an explicit map rather than MAP_CURRENT=255.
Its entry clears the pending type and begins the fresh-map path, assigning
CURRENT_MAP=22/CURRENT_BATTLE=255 before map population. **The accepted endpoint
is that entry, before GetMapSetupEntities/InitializeMapEntities and init**;
the battle endpoint is the selected BattleLoop handoff before opening. Neither
endpoint establishes arrival dialogue, destination placement, loaded roster,
successful loading or usable control.

Successor boundaries are identified only: Map22's baseline `s1_entities.asm`
declares 30 rows; `data/maps/mapsetups.asm` selects an alternate entity setup
under F522. Both pointer tables reference `ms_map22_InitFunction`. Its dispatcher
checks F776, calls `cs_59656` when clear and sets F776 only after return; later
F522-set/F778-clear selects `cs_5996E` then sets F778. Those scene bodies and
population are separate arrival/post-battle content, excluded here. The source
warp's `(0,0)` is not a claim about their eventual placement or completion.

## Counterexamples and reproduction

| Changed state | Discriminating boundary |
| --- | --- |
| F783 set, selected Zone1 | No scene, text, copy or new warp from this callback; wrapper closure/waits still apply. No flag write marks a new completion. |
| F783 clear, F774 independently clear/set | Same local scene selection; prior positioning/setup and natural reach may differ. No F774 prerequisite or completion stamp is introduced. |
| Different F67/B selectors, duplicate/missing mappings, HP/class | Different or aliased physical actors and sprite work; three logical names do not guarantee three visible participants. |
| Incomplete/replaced action after wait 60 | Map script may continue without all three idle; 128's waits and physical 0's coordinate wait are different predicates. |
| Stalled copy/action/text/flash/wrapper service | Earlier effects are not rolled back; final warp write or its consumption may never occur. |
| Replaced or incomplete pending request | The later consumer follows actual pending type/parameters, not the earlier source literal. |
| F422/F522 changes | Admission follows the truth table above; neither the collection narration nor `(0,0)` suppresses the selected battle. |

Load `. ./local/private-inputs.ps1` in the same process as controlled commands,
then verify the assigned source HEAD/clean status. Reuse the
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture`, and source
`_cutscene_facts` equality, without regenerating the retained pair.

| Evidence surface | Direct check for this slice |
| --- | --- |
| Retained `local/derived/map-script-engine-static.json` and `tests/fixtures/h2/map-script-engine-static-v1.json` | Exact cs_5AF36 source-line/macro/argument joins for all 43 commands; three inline bodies and one custom body through their terminators; selected actual macros/handlers. |
| `gamescript.txt` keys 072A–072F and selected comments | Six exact literal/control joins, sequential cursor and actual speaker/modifier/continuation/name consumers. |
| `tests/fixtures/h2/map-events/zone-programs.json` and complete selected source | F783 gate/return, no F774 prerequisite or selected flag write, actual setup/wrapper/caller and pending-request order. |
| Named action and immediate consumers above | Complete shrinking action; shared size versus per-entity resize/position/action state, wait and presentation bounds. |
| Complete Map33 step/flag tables; complete switch/battle-coordinate tables and selected consumers | Exact copy record; no scene persistence stamp; Map22 row, wildcard checks, area-before-F522/clear order and both entry endpoints. |

Retained fixtures do not automatically guard every prose claim. Complete selected
source/consumer review supplies the additional joins. Acceptance checks the
entire owned diff, provenance/links/private boundary, whitespace, clean committed
configured `uv run sf2 verify plan --base origin/main --head HEAD` and actual
final-head CI. No local blanket normal/full/engine/Godot/H3/H4 suite is selected
solely for this documentation change. Exact Git/check/CI identities and inspection
failures belong in Issue896's handoff. Preserve
[Issue892's original/rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/892#issuecomment-6173418824)
and [PR894 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/894#issuecomment-6173456022),
including earlier linked failures and retained results.

**Unknown:** natural trigger reach, actual setup/flags/party/HP/class/mapping,
source position/mode/pending state, service and concurrent-action completion,
rendered shrinking/shadows/pickup, actual destination admission, usable control
and persistence. Counterexamples are static discriminators, not executed
scenarios or an automatic runtime queue under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
