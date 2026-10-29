# Ancient Tower ascent, Creed and Battle41 admission

Continue with [Battle41 opening, activation and first control](./battle41-opening-activation-and-first-control.md).

Evidence date: 2026-10-22. **Confirmed (static):** Map56 has an authored
setup and one fixed Creed actor. Talking can set903 and enter Church;
the43,40 zone separately sets441 then904. Talking alone does not unlock41.
All14 warps are no-scroll requests; twelve encode CURRENT and retain the
exploration population/layout when no battle is selected. The selected
Battle41 row requires441 and Y24, with wildcard X. Held541 clears441
after the crop write but still returns41 on that call. **Stop before
Battle41's before hook.** Natural reach, presentation and playable remake
support remain **Unknown**.

## Agreement and provenance

Standard rigor for [Issue #1164](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1164)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
give future content integration the complete local exploration, Creed,
church-entry and encounter-admission contract. Accepted base
`44a09b794f81c9dcf4159aa75a4553d70b39303c` includes the corrected
[Battle40 outcome/Map54 exit owner](./battle40-outcomes-and-odd-eye-aftermath.md).
Own this document and successor navigation only in that predecessor.
No shared index/schema/fixture/extractor/engine change, stacked dependency,
Battle41 opening/roster/AI/outcome investigation or natural-play acceptance.

Original paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2enums.asm`, `sf2const.asm`, `sf2mapsetupmacros.asm`, `sf2mapmacros.asm`
and `sf2cutscenemacros.asm` own constants/encodings. Below, `map/` and
`entity/` mean children of `code/common/scripting/`; `exploration/` means
`code/gameflow/exploration/`. Local files are under
`data/maps/entries/map56/`; callbacks/scripts under its `mapsetups/`.
Fnnn denotes flag nnn; row ordinals are zero-based; address intervals are
half-open. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This adds no fresh source/ROM equality or runtime observation.

Minimum evidence is all16 local ASM files, shared selection/request/alias
uses and actual production consumers. Reuse the immutable18,285,979-byte
init/script pair with `local/issue1005/pair.txt` provenance once for the
selected script observation; do not rebuild it or export layouts/terrain.
Linear selected reads/lowering are expected to take seconds, evidence
below2MiB and total new coordination/output below4MiB, with a512MiB peak
memory target, unmeasured. New results use `local/issue1164/`; old results
are read-only. Reassess cross-system/runtime or material volume expansion
with main-gate. No new framework, maintained parser or verifier tests.

## Arrival, setup and local content

**Confirmed:** Map54's wildcard-X/Y0 warp requests56,16,48/Up with
scroll byte0. `SwitchMap` has no56 substitution.
`GetCurrentMapSetup` in `map/mapsetupsfunctions_1.asm` selects the exact
`msMap 56, ms_map56` row in `data/maps/mapsetups.asm`, terminated immediately
by msMapEnd with no flag variant. This is a real setup, unlike Map54's
ms_Void fallback. `pointertable.asm` `$61488..$614A0` has six long pointers:

| Section | Owner / address interval | Complete local content |
| --- | --- | --- |
|Entities|s1_entities `$614A0..$614AA`|One8-byte fixed record42,39/Up, MAPSPRITE_CREED212, eas_Init; word terminator|
|Entity events|s2_entityevents `$614AA..$614D6`|Logical128 with mask UP1, then default; callback and Church tail below|
|Zone events|s3_zoneevents `$614D6..$614FE`|43,40, then default; callback below|
|Area descriptions|s4_descriptions `$614FE..$61500`|RTS; no authored description routine body|
|Item events|s5_itemevents `$61500..$61508`|Default-only6-byte item-event row, then RTS|
|Init|s6_initfunction `$61508..$6150A`|RTS; no arrival scene or flag/savepoint write|

Entity/zone rows each encode two bytes plus a signed relative word;
defaults start FD,00. Callback offsets are relative to their table base.
The entity row's UP is an action mask, not a required approach direction:
`RunMapSetupEntityEvent` selects by logical ID only, uses bit0 to turn the
actor opposite the player's facing, and bit1 to restore its old facing
after return. Mask1 turns without that restoration. Default mask0 is RTS.
The wrapper obtains portrait/speech from the resolved actor and disables
entity VInt service before calling the callback. After return it closes
windows and activates service. The `script` trap enables entity service
during cs_6150A; this does not establish a whole-service freeze or
presentation timing.

Ordinary explicit56 exploration follows the accepted rebuild interface:
initialize flag-followers, then authored actors, then Bowie physical0;
clear temporary256..383, set80, load map, install base services, call RTS
init. This does not clear441/541/903/904 or copy CURRENT_MAP to EGRESS_MAP.
`InitializeMapEntities` in `map/mapfunctions.asm` assigns the nonally
Creed record to alias-table entry32, corresponding to logical128, after
the allocated ordinary followers. Physical slot is therefore variable,
not128 or always1. `map/followersfunctions_1.asm` and
`data/scripting/entity/followers.asm` own flag-selected population;
Map56 is not either hardcoded no-followers map. F67 can supply Peter7;
other supplied follower flags can change slots and duplicate aliases.
Joined/active membership or HP is not the condition for this fixed NPC.

`eas_Init` in `data/scripting/entity/eas_actions.asm` installs acceleration
factors0,0, speed32,32, acceleration/deceleration off, entity obstruction
on, entity/map uncollidable off, autoFacing on, orientUp, immersion off,
size24 and sprite update, then jumps to idle. Authored Up and an action
stream are not proof of a permanently facing or stationary rendered NPC.

`00-tilesets.asm` `$B7CA0..$B7CA6` declares palette9 and tilesets
100,95,93,255,255. `2-areas.asm` `$B7CCE..$B7D2A` contains three30-byte
records plus a word terminator:

| Row | Main start→end | Secondary foreground / background start | Main / secondary parallax | Music |
| --- | --- | --- | --- | --- |
|0|0,0→31,49|0,0 /32,0|256,256 /128,64|BATTLE_THEME_3=33|
|1|34,30→51,44|0,0 /0,0|256,256 /256,256|BATTLE_THEME_1=34|
|2|34,45→51,63|0,0 /0,0|256,256 /256,256|BATTLE_THEME_1=34|

All three have both autoscroll pairs0,0 and mainLayerType255, selecting
the foreground plane. `LoadMap` in `code/common/maps/mapload.asm` scans
authored order, compares inclusive main bounds in MAP_TILE_SIZE384 units
against the supplied selection point or tracked entity (physical0 if no
view target), and falls back to the first record after the terminator.
The alternative encoded point interface uses its own shifts; table ranges
alone do not prove which area is selected naturally. Parallax256 is the
stored unity scale, not a256-tile displacement. No decoded geometry,
collision, screen composition or music completion was inspected here.

The complete flag/step/roof/chest/other tables contain only endWord:
`3-flag-events` `$B7D2A..$B7D2C`, `4-step-events` `$B7D2C..$B7D2E`,
`5-roof-events` `$B7D2E..$B7D30`, `7-chest-items` `$B7DA2..$B7DA4`,
`8-other-items` `$B7DA4..$B7DA6`. Empty step data does not remove the
separate setup-zone callback or establish collision-free traversal.

## Creed's first conversation and repeat service entry

**Confirmed:** the normal action caller is
`exploration/explorationvints.asm:ProcessPlayerAction`: after waiting for
the entity/view to stop, C action uses GetActivatedEntity, then
GetEntityEventIndex and j_RunMapSetupEntityEvent. A action reaches the
field menu; configured128 is not itself proof of an activated actor.
`GetEntityEventIndex` scans aliases in order and returns the first matching
physical slot; altered/multiple mappings can change the logical result.

`Map56_EntityEvent0` checks903. Clear903 executes cs_6150A and sets903
**after** it returns. Held903 skips only that scene. Both paths then
display3611,3612, clsTxt and `jmp j_ChurchMenu`; the s05 jump interface
resolves to ChurchMenu. Church's RTS returns to the callback's wrapper.
Neither path sets441 or904. Canceling the Church menu does not roll back
903. The default entity callback is RTS and does not enter Church.

`scripts.asm:cs_6150A` `$6150A..$6153E` is12 commands/52 encoded bytes.
It sets textCursor3604, faces128 toward Bowie0, displays3604, faces0
toward128, faces Peter7 toward128, then displays3605..3610 and csc_end.
The complete text inventory is below; summaries are authored, not original
dialogue. IDs are decimal; tokens are in exact source order from
`data/scripting/text/gamescript.txt`.

| ID | Speaker / macro | Tokens | Authored summary |
| --- | --- | --- | --- |
|3604|128 / nextSingleText|`{N} {W1}`|Surprised welcome on arrival|
|3605|7 / nextSingleText|`{W1}`|Peter hesitates over how to address Creed|
|3606|128 / nextText|`{NAME;7} {N} {N} {W2}`|Accepts the address and recalls doubt about their arrival|
|3607|128 / nextSingleText|`{N} {W1}`|Acknowledges the enemies' strength|
|3608|0 / nextSingleText|`{W1}`|Bowie agrees|
|3609|128 / nextText|`{N} {LEADER} {W2}`|Anticipates the leader asking why he is here|
|3610|128 / nextText|`{W2}`|Offers help|
|3611|event speaker / txt|`{N} {N} {W1}`|Offers priest services where no church is nearby|
|3612|event speaker / txt|`{W1}`|Asks which service is wanted|

All seven scene text modifiers are0. `nextSingleText` emits opcode$00 and
modifier/entity bytes; `nextText` emits$02. Their selected00,80/00,07/00,00
words feed portrait/speech lookup without an HP guard. Ordinary mapsprite
212 resolves portrait CREED37 and DIALOG_BLEEP_8; the lookup uses the
actual resolved sprite, not a hardcoded narrative identity. csc00 closes
portrait/text and requests Sleep10 after display; csc02 continues without
that close/sleep. Debug text-skip can omit display and cursor increments.
csc_end waits for view scrolling only if the dialogue window is open,
clears view speed and returns; it does not establish hardware completion.

`faceEntity` emits opcode$52 plus two **words**. Its dispatch is
`mapscriptengine_2.asm:rjt_cutsceneScriptCommands`→
`mapscriptengine_1.asm:csc52_faceEntity`. It consumes acting word into d7,
target word into d0, resolves target XDEST/YDEST, then acting alias. It
compares absolute coordinate differences, chooses vertical on a tie,
Up/Down from Y or Left/Right from X, writes facing, updates sprite and
waits one VInt. Identical destinations choose Down. There is **no HP
precheck** here. HP0 Peter can be faced; a missing alias can resolve to
physical0, including a missing Peter facing Bowie instead. Alias helper
masks FF, maps logical128 to index32 (subtract96), and reads the physical
byte. None of these instructions join, revive, create or validate Peter.
This is distinct from csc2C's high-byte/Bowie HP guard in the predecessor.

## The43,40 zone and changed valid states

**Confirmed:** `msZoneEvent 43,40` routes to Map56_ZoneEvent0; unmatched
coordinates route to RTS. `entity/entityscriptengine_2.asm:loc_528A`
first tests the candidate working-layout marker `(word & 3C00)==1000`
for warp, then1400 for a zone request. A single masked value cannot match
both. Zone request6 stores candidate coordinates derived from
XDEST/YDEST plus movement offsets, divided by384, in PARAM1/PARAM3 words.
This happens before the later full-word passability/movement handling;
standing at a coordinate is not the complete trigger predicate.
WaitForEvent consumes the request; ProcessMapEvent clears event type
before dispatch, and type6 installs eas_Init on physical0 before calling
j_RunMapSetupZoneEvent with those words. The setup router checks row order,
FF coordinate wildcards and FD default; this row itself has no wildcard.
Its wrapper closes windows, waits one VInt and waits for physical0 to stop.

The zone checks904 **first** and returns immediately if set. Otherwise it
checks903, runs the same scene only if903 is clear, sets903 after return,
then sets441, sets904 and returns. It does not enter Church or display
3611/3612. The904 comment mentioning0387 is not an actual387 read/write;
only the instructions determine this branch.

| Supplied904 | Supplied903 | Supplied441 | Scene / resulting903,441,904 after ordinary return |
| --- | --- | --- | --- |
|clear|clear|clear or set|Scene once; set,set,set|
|clear|set|clear or set|No scene; set,set,set|
|set|clear|clear or set|No work; clear,unchanged,set|
|set|set|clear or set|No work; set,unchanged,set|

These four rows cover all eight flag combinations. Talking first sets903
but leaves441/904 unchanged; later a904-clear zone unlocks without replay.
Taking the zone first sets all three; later talking skips the scene and
offers Church. Held904 with missing441 is **not repaired**, even if903
is clear. Held441 can admit a coordinate match without903/904; Creed flags
are not direct CheckBattle predicates. No local branch checks540 or541.

Interrupted-prefix states differ from an ordinary return: before the
scene returns its caller has not set903; between903 and441 only903 is
guaranteed; between441 and904 the zone can be retriggered and reassert441.
With903 already set it skips the scene. Talking can leave903 set before
the menu is entered or canceled. These are static prefix consequences,
not observed resets, save transactions or naturally reachable interruptions.

## Church and the actual savepoint connection

**Confirmed:** reuse [common Church static contract](../../presentation/common-menus.md)
and its [accepted Save seam](../../presentation/common-menus.md#church-save-lifecycle),
also used by the [Roft caller owner](../moun-nazca-grans/roft-town-interactions-and-zynk-recruitment.md).
This caller adds no new menu algorithm or H3 observation. ChurchMenu at
`$20A02` starts MENU_CHURCH with choice0, compares−1 for Cancel,0 Raise,
1 Cure,2 Promotion; other noncancel selections fall through Save (normal
selector3). Ordinary Raise/Cure/Promotion and declined Save use the
another-desire prompt to repeat or exit. Cancel/exit closes windows and
RTS returns through Map56's tail transfer; there is no local post-service
mutation or automatic441/904 unlock.

`churchactions_1.asm:@StartSave` `$20FCC` first asks for confirmation.
Only accepted d0=0 enters @DoSaveGame: copy CURRENT_MAP byte to EGRESS_MAP
at `$20FE6`, read CURRENT_SAVE_SLOT word at `$20FEC`, set399 via the flag
trap at `$20FF0`, then call SaveGame at `$20FF4`→`$6F6A`. Declining does
none of these writes/calls. Talking, selecting a service or simply arriving
does not update EGRESS_MAP. Save then requests music/input and a continue
choice: yes exits normally, no fades and tail-transfers to WitchSuspend.
That latter branch does not return through the ordinary caller cleanup.

The accepted bounded seam covers five supplied cases, the original
Church/SaveGame call and return, touched SRAM bytes and continue/suspend
entry. It proves neither normal Map56 UI reach nor cross-process durability.
Slot selector0 chooses slot1; nonzero chooses slot2. The4,016 stored bytes span
8,032 interleaved SRAM addresses; those are different quantities.

`data/maps/global/savepointmapcoords.asm` row18 explicitly maps56 to
42,38/Down. `code/common/maps/egressinit.asm:GetSavepointForMap` first
checks399: clear399 forces the initial room; held399 searches map rows,
defaulting to supplied map with1,1/Up when no row matches, then applies
the conditional raft-reset interface. Thus with399 held and input56 it
selects42,38/Down. A confirmed Map56 Church save establishes EGRESS_MAP56
and399 before the SaveGame call; no claim of eventual persisted save follows.
`GetEgressPositionForBattle` has no41-specific destination override, so a
future ordinary41 retreat/defeat using that helper can select this point
if EGRESS_MAP is still56 and399 held. Other saved Egress state, special
battle callers or subsequent writes can select elsewhere; arrival16,48,
Creed42,39 and savepoint42,38 are three distinct coordinates.

## All14 warps and retain/rebuild semantics

**Confirmed:** `6-warp-events.asm` `$B7D30..$B7DA2` is fourteen8-byte rows
plus word terminator. Each row is trigger X/Y bytes, scroll byte0, target
map byte, destination X/Y bytes, facing byte and zero padding. All are
warpNoScroll;255 is MAP_CURRENT in target position and an X wildcard in
the first two trigger positions.

| Row | Trigger X,Y | Target / destination | Facing |
| --- | --- | --- | --- |
|0|255,49|54 /16,1|Down|
|1|255,5|60 (ANCIENT_TOWER_STAIRWAY) /57,24|Up|
|2|44,47|CURRENT /44,35|Left|
|3|44,35|CURRENT /44,47|Right|
|4|41,47|CURRENT /41,35|Right|
|5|41,35|CURRENT /41,47|Left|
|6|42,41|CURRENT /15,24|Down|
|7|43,41|CURRENT /16,24|Down|
|8|15,23|CURRENT /42,40|Up|
|9|16,23|CURRENT /43,40|Up|
|10|42,63|CURRENT /15,36|Down|
|11|43,63|CURRENT /16,36|Down|
|12|15,35|CURRENT /42,62|Up|
|13|16,35|CURRENT /43,62|Up|

Rows2/3 and4/5 are reciprocal authored coordinates. Rows6/7 and8/9,
and rows10/11 and12/13, connect adjacent but different trigger/arrival
coordinates. They are not proof of a traversable stair geometry or a
guaranteed inverse path. In particular,9 arrives43,40; a warp arrival
does not itself call the setup-zone callback.6/7 arrive Y24, supplying a
potential immediate Battle41 check if441 is held. Source order and
wildcards apply only after the working-layout1000 marker and movement
caller permit WarpIfSetAtPoint; rows alone do not prove marked tiles.

WarpIfSetAtPoint divides coordinates by384, scans authored order, treats
negative trigger bytes as wildcards and stops on FFFF. On match it writes
MAP_EVENT_WARP1, copies scroll/map/X/Y into PARAM1..4, facing/padding into
PARAM5..6, and SFX_WARP. ProcessMapEvent clears the type before type1
consumption. Scroll0 idles physical0, removes the ExplorationLoop return
frame and returns map/X/Y/facing/d4=0 to MainLoop. Negative destination
coordinate bytes would retain their coordinate; none of these rows uses
that option. Scroll0 does **not** mean retain entities by itself.

MainLoop runs SwitchMap, then CheckBattle before choosing exploration.
With no encounter, explicit54/60 rebuilds exploration population,
temporary flags and layout through its own setup interface. Neither54
nor60 has an authored msMap row; GetCurrentMapSetup falls back to ms_Void,
not Map56's Creed setup. CURRENT255
keeps the current map in CheckBattle; ExplorationLoop's−1 branch retains
actors/aliases/temp flags and uses LoadMap's current-layout path, updates
the main actor/followers and reruns base services/init. Map56 init is RTS.
The original therefore retains live mutable state for these twelve
internal warps; it does not reconstruct a fresh56 just because they fade.
If41 is selected, BattleLoop takes priority over this exploration path.
Stop at54/60 arrival/setup interfaces; their subsequent content is outside
this owner. Neither outgoing row adds a540/903/904 condition.

## Selected Battle41 boundary

**Confirmed:** `data/battles/global/battlemapcoords.asm` row41 is
`56,0,0,32,36,255,24`: map, crop X/Y/width/height, trigger X/Y, all bytes.
`code/common/maps/getbattle.asm:CheckBattle` `$799C..$7A36` scans ascending
indices0..BATTLES_MAX_INDEX, with first matching unlocked coordinate row
winning.41 is the only row for56; earlier maps cannot preempt it for a
supplied56. MAP_CURRENT255 is replaced with CURRENT_MAP for this lookup.
It checks400+41=441, accepts trigger XFF, requires player Y byte24,
then writes all four crop bytes0,0,32,36. These are bounds metadata, not
a decoded battle layout or proof that any X is physically legal.

After the crop write it checks441+100=541. Clear541 returns d7=41 with
441 retained. Held541 clears441 and still returns41 on that same call;
it does not clear541, return−1, or continue scanning. A later call with
441 clear skips the row and returns−1, leaving crop untouched by that row.
Locked441 or nonmatching Y likewise does not write this crop or inspect
541 here. X wildcard is independent of Y24. No540/903/904 direct predicate
exists in CheckBattle;903/904 affect441 only through this map's zone.

Entering56 at16,48 does not satisfy Y24. Completing a Creed conversation,
setting441, returning41 and entering BattleLoop are separate boundaries.
A type6 zone event returns to the exploration wait loop, not MainLoop;
unlock does not itself call CheckBattle. No-scroll warp exits do return
to MainLoop with their destination coordinates. On d7!=−1 MainLoop passes
it as d1 to j_BattleLoop. Its normal entry sets399, checks suspend88,
sets current map/battle and installs base services before the before-hook
call; suspend handling is a separate branch. This owner stops before
`j_ExecuteBeforeBattleCutscene` and does not claim that hook ran.

## Actual consumer support and selected observation

**Confirmed about current code:** one explicit-file OriginalPrograms
observation completed in0.1232s, no exceptions, retaining the full15,258-byte
receipt and unchanged pair metadata. It separately lowered all10 selected
entry/continuation/default/init/script labels. Counts are4/6/38/1/5/1/3/1/1/1
for EntityEvent0, byte_614C2, cs_6150A, DefaultEntityEvent, ZoneEvent0,
DefaultZoneEvent, byte_614F4, AreaDescriptions, DefaultItemEvent0, Init.
Every final instruction and entry was inspected; this is not a map import.
cs_6150A has native faceEntity at lowered indices1,8,9; byte_614C2 ends
with native jmp at5 for Church. Default/init routines lower to end. Typed
branches/flags/calls/text coexist with those four unsupported operations.
ProgramRunner refuses the first reached UnsupportedInstruction before
advancing it; clear903 paths stop at the first faceEntity before dialogue,
while held903 interaction can reach the Church frontier after3611/3612.
No callback is thereby linked to a runnable Map56 package.

| Actual owner | Existing expression and selected gap |
| --- | --- |
|[private content preparation](../../../../src/sf2tool/remake_exploration_content.py), [canonical importer](../../../../src/sf2tool/h2/map_import.py)|Preparation requires the registered canonical export identity and pinned source, registers selected eventMaps callback/init/script files, compiles selected programs/routes, then maps and setup variants. The canonical importer carries setup/entity/area/warp/program metadata; that transport is not runtime admission. This slice reuses the small static pair and explicit source registrations, without running this complete preparation, reading a full canonical layout export or producing a playable package.|
|[map compiler](../../../../src/sf2tool/remake_exploration_maps.py)|Real setup metadata, one NPC, three areas and14 warps can be represented. Outside eventMaps, map-56-setup remains a native frontier and input remains a native frontier because an NPC exists. The final pass replaces only route-absent setups with end; Map56 has a route and RTS init does not match its supported four-operation guard/script shape. Under explicit eventMaps selection, init can become compiled RTS/end and callbacks/actions/population link, but faceEntity/Church still stop. This was source inspection, not an executed map compile/import.|
|[program compiler](../../../../src/sf2tool/remake_exploration_programs.py), [Content reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|Flags, text, calls and defaults lower independently; faceEntity has no typed lowering and jmp Church is native. Reader maps native-call to UnsupportedInstruction; runner reports program-opcode. Existing constant SetEntityFacing is not coordinate-facing semantics; SetEgressMap is not a Church/save implementation.|
|[map allocation](../../../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [event dispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [map transfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Follower/NPC allocation, marker/flag matching and preserve/rebuild exist. Compiler retains all warp rows, making unselected destinations frontiers. It emits preserve for CURRENT and rebuild otherwise; runtime validates selected content/entry and applies onLoad. Authored rows and generic mechanisms do not supply actual56 layout, actor, service, music or rendering admission.|
|[battle preparation](../../../../src/sf2tool/remake_exploration_battles.py), [encounter reader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)|Preparation admits overworld2/3 and explicitly rejects non-FF coordinate triggers before hook binding; selected41's Y24 is not admitted. Content pins1/2/3 only. Generic unlock/completion/progression policies lack a bound41 route and coordinate predicate here. Do not equate an abstract completed-admission policy with this source's clear441-and-return41 sequence.|
|[adapter presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../../../remake/docs/capability-status.md)|Generic map/entity/text projections consume admitted content; no56/41 playable, Church UI/save, natural transition or audiovisual completion was observed. Runnable content through Battle03 entry remains a separate accepted boundary.|

**Inferred:** a later implementation can reuse these types and source
callbacks, but needs explicit Map56 content/admission, coordinate-facing,
Church/save lifecycle and coordinate-aware41 route support. Those are
implementation tasks with their own acceptance, not repairs hidden in
this documentation slice or evidence that later maps are playable.

## Reproduction, preserved failures and Unknowns

Ignored `local/issue1164/review.py` retains exact local tables, complete
selected/shared source and actual consumers, ordered width/branch/call
checks, text-token inventory, eight analytic flag cases, output/native
checks and relative-link/private/owned-diff checks. Its flag matrix is a
static projection, not runtime samples. Use a fresh receipt name:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1164/review.py <configured-source-disasm-root> --receipt local/issue1164/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The single selected lowering recipe is `local/issue1164/observe.py`; its
complete result is `observe-01.json`. Do not rerun it merely to replace an
unchanged receipt. Frozen identity, CI, timing/bytes, read/recipe failures,
resource totals and process completion belong to the Issue/PR handoff.
ADR0004 adversarial review covers provenance, exact local inventory,
operand widths/dispatch, state changes, alias validity, ordering, producer
final output, actual consumers and public/private boundary. New maintained
parser/schema/fixture/test and H3 callback items are N/A. Normal/full
verify, old comparisons, SDK/Godot/emulator/H3/H4/screenshots are **SKIPPED**
under the explicit documentation-only scope; actual scoped CI still runs.

Preserve1162's followEntity word-high-byte/Bowie HP correction and typed
consumer gap, original author/direct-main-gate/CI39725455473 missed-finding
passes, corrected narrow author PASS0.1548s/15,237B, main-gate PASS0.1553s/
15,237B and CI39726160754 SUCCESS. Preserve1160's R3 self-review/local
failures and direct02 semantic limit, Map54 final-overwrite correction,
Map53 UnboundLocalError with zero partial output and twelve completed old
comparison failures. Their owning handoffs retain detail. None is repaired,
rerun, erased or called interrupted here.

**Unknown:** natural marker/collision/warp/interaction reach, incoming
flags/HP/membership/aliases, interleaved or interrupted prefixes, decoded
layout/terrain and source/ROM equality beyond accepted owners, selected
area/portrait/text/camera/render/DMA/VInt/music/input completion, ordinary
savepoint use and cross-process persistence, usable56/60 arrival and
playable41. These are not an automatic acquisition or repair queue.
Freeze a clean pushed Draft PR and notify main-gate for independent
source/consumer review and integration; no self-merge, cleanup, archive
or next Issue.
