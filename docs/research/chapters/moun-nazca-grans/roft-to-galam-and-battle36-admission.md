# Roft to Galam, Dry Stone and Battle36 admission

Continue with [Battle36 initialization and the Prism Flowers opening](./battle36-initialization-and-opening.md).

Evidence date: 2026-10-20. **Confirmed (static):** Roft38 exits to76;
76's west scroll selects77 without a fresh exploration entry or CheckBattle.
Map77 has two separate no-scroll entrances to53. Battle36 admission uses
F436 and map53, whereas77's default zone independently sets437 and requests
a CURRENT reload while537 is clear. Neither that callback nor Battle37's
coordinate row checks536. Battle numbering does not establish a mandatory
36→37 sequence. **Stop at admission**, before Battle36 initialization,
roster, opening, turns or outcomes; Battle37 is only an admission interface.
Natural reach, actual carried state and completed services remain **Unknown**.

## Agreement and evidence route

Standard rigor for [Issue #1126](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1126)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
provide the next research/content boundary from Roft to Galam and Battle36,
including only the local Dry Stone branch. Accepted base
`7207daaca210481c007238b2bcc4efecdfc97846` includes the
[Roft town owner](./roft-town-interactions-and-zynk-recruitment.md),
[regional entry/lifetime owner](./nazca-to-roft-and-battle35-admission.md)
and [Nazca shootdown owner](./nazca-ship-shootdown-and-crash-handoff.md).
Own this document and navigation only in the town owner. No shared index,
registry, schema, fixture, design, engine or tooling mutation; no stacked
dependency. Village interiors, forging, Granseal's body and later921
production are excluded. Conversion success does not establish playable content.

Source paths are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`sf2enums.asm`, `sf2const.asm`, `sf2mapmacros.asm`,
`sf2mapsetupmacros.asm` and `sf2cutscenemacros.asm` own constants/encoding.
`map/` means `code/common/scripting/map/`; local files below are
`data/maps/entries/mapNN/`. Fnnn means flag nnn; ordinals are zero-based;
address intervals are half-open. USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This slice adds no ROM/source equality, emulator or hardware observation.

The minimum sufficient evidence is selected complete tables, the one complete
Dry Stone scene, changed-state predicates and actual compiler/consumer paths.
Reuse the existing18,285,979-byte init/script pair, `local/issue1005/pair.txt`
and registered canonical map export verified against
`manifests/extractions/canonical-map-import.json`. Linear selected reads and
compilation load those inputs once per observation; small unique ignored
receipts live in `local/issue1126`. Per-direct budget is one minute/512MiB;
peak memory is unmeasured. No new export, parser, fixture, platform, network
acquisition or verification-tool tests. Reassess material growth before scaling.

## Roft departure and retained regional scrolling

**Confirmed:** reuse the town owner's eight CURRENT paths and ninth exit,
not a second town analysis. Its row8 `$AFD16..$AFD1E` publishes
source255,31 → no-scroll0 /76,8,48 /Down3. The return is
76 row0 `$C4F98..$C4FA0`:8,47 → no-scroll0 /38,14,30 /Up1.
Neither row requires Zynk26,76/911/921,435/535 or a successful town scene.
The separate 76 row1 `$C4FA0..$C4FA8` is0,255 → Left$12
/77,62,255 /Left2. Map77 row4 returns63,255 → Right$10
/76,1,255 /Right0. These declarations do not prove connected walking paths.

`entity/entityscriptengine_2.asm:esc02_controlCharacter` admits warp checking
through the candidate movement/layout$1000 marker. In
`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint`, signed pixel
coordinates divide by MAP_TILE_SIZE384; terminator precedes wildcard/match
tests and the first row wins. Negative source bytes match any value.
Each selected row is eight bytes: two trigger bytes, scroll/map/X/Y,
facing and zero padding. The producer writes TYPE.w=1, PARAM1..4 as one
longword, PARAM5 plus padding as one word, and WARP_SFX.w. Standing at a
declared coordinate alone does not post a warp.

`explorationfunctions_2.asm:ProcessMapEvent` clears TYPE.w before dispatch;
WaitForEvent prioritizes this mutable request over A/C. With intact nonzero
PARAM1 the scroll handler idles physical0, writes CURRENT_MAP.b and calls
`code/common/maps/mapload.asm:ProcessMapTransition`. Its map71/F530 exception
does not apply to76/77. Destination77 uses palette0/tilesets101..105;
the transition reloads tilesets1/3/4 (101/103/104), blocks and working layout,
including held flag copies/chest blocks and an existing battle crop when
CURRENT_BATTLE is not255. It scales the first destination area and runs the
then-live PARAM1&3 direction through plane/VInt/DMA scrolling helpers.

This path retains entities, aliases, follower list/scripts, temporary256..383,
party/resources and current battle state. It does not run fresh population,
mapsprite loading, F80/temp initialization, map init, common exploration
healing/step subtraction, SwitchMap or CheckBattle. LoadMapArea's fresh
music/palette/full layer/parallax/autoscroll/animation/roof setup does not run.
CURRENT_MAP77 can therefore select77 callbacks with actors carried from76.
An empty77 fixed-NPC table does not empty that live population.

After the transition, X is read into d0.b without clearing d0.w. For returned
word R and intact X byte K (62 westward,1 eastward), native X/XDEST become
`(((R & $FF00) | K) * 384) mod 65536`; nominal K tiles requires a zero high
byte. Y explicitly clears its word first; destination255 is signed negative
and preserves post-transition Y and YDEST independently. PARAM5 does not set
facing in this branch. Idle replaces the action pointer, not velocities or
speed. A later control-script installation and its execution are separate.
The handler samples signed actual player position/facing and calls
DeclareRaftEntity: mode2 returns, mode1 skips follower relocation, other modes
relocate listed physical followers; F64 and stored raft map/position separately
govern physical31. No party joining or alias repair is implied. The
[shared scroll owner](./moun-to-nazca-and-battle34-admission.md)
and regional entry owner retain the full common helper/raft distinctions.

For no-scroll, the handler unwinds to MainLoop after SFX/idle, samples signed
main coordinates/384, and replaces only low coordinate bytes for nonnegative
destinations. Supplied high bytes can survive even with explicit destinations.
It reads facing from PARAM5; SwitchMap then precedes CheckBattle. Neither
38,53,76 nor77 is a source in `data/maps/global/flagswitchedmaps.asm`
(only3/F640→4,71/F530→78,58/F999→59). Fresh explicit-map exploration,
if no battle is selected, rebuilds population, clears temps, sets80 and loads
resources/init through the accepted entry owner. A CURRENT negative-map
exploration entry retains population/temps/working layout, but still runs
area/plane/roof/init and common heal/step work. Neither is the scroll lifetime.
If a battle is selected, this document stops before its separate load/init.

## Complete Map77 setup and connection tables

**Confirmed:** `data/maps/mapsetups.asm` has one unflagged77 route, no
variant. `map77/mapsetups/pointertable.asm` `$50A50..$50A68` contains all six
entity/entity-event/zone/description/item/init pointers. Their local content:

| Owner / interval | Complete declaration |
| --- | --- |
|s1 `$50A68..$50A6A`|msEntitiesEnd; zero fixed NPCs. Fresh common player/flag-followers remain separate.|
|s2 `$50A84..$50A8A`|Default entity callback only, RTS.|
|s3 `$50A6A..$50A84`|Default zone callback below; trailing nullsub_50A82 is RTS.|
|s4 `$50A8A..$50A8C`|Area description RTS; no local description rows.|
|s5 `$50A8C..$50AE6`|One Dry Stone item row, default RTS, callback and11-operation scene below.|
|s6 `$50AE6..$50AE8`|Init RTS; no local story producer.|
|00-tilesets `$C5D1C..$C5D22`|Palette0; tilesets101,102,103,104,105 in slot order.|
|2-areas `$C5D4A..$C5D6A`|One area0,0..63,63; secondary foreground/background0,0; both parallax256,256; both autoscroll0,0; type254; MUSIC_MAIN_THEME; terminator.|
|3-flags `$C5D6A..$C5D74`|F900: copy source1,1 /size1×1 /destination42,42; terminator.|
|4-step /5-roof /7-chest /8-other|Only endWord, at$C5D74,$C5D76,$C5DA2,$C5DA4 respectively.|
|9-animations `$C697A..$C6990`|mapAnimation111,96; ordered entries0,48,$350,20 and48,48,$350,20; endWord. These encoded operands do not prove displayed timing.|

All five warp rows occupy `$C5D78..$C5DA0`, then endWord through$C5DA2:

| Ordinal / source | Scroll / destination / X,Y / declared facing |
| --- | --- |
|0 /40,42|No-scroll0 /29 Dwarven Village exterior /15,20 /Up1|
|1 /255,33|No-scroll0 /53 Prism Flowers battlefield /19,1 /Down3|
|2 /255,34|No-scroll0 /53 /10,30 /Up1|
|3 /255,63|Down$13 /66 Granseal overworld /255,1 /Down3|
|4 /63,255|Right$10 /76 /1,255 /Right0|

There is no F900/536/537/party gate in these warp records. The layout marker
and movement/collision admission remain required; the table alone does not
prove the village entrance becomes passable. Village29's two external
no-scroll returns (source255,0 and255,21) both publish77,41,42,Down.
Map66's source255,0 returns by Up$11 scroll to77,255,62,Up. These are
interface-only observations; no village interior or kingdom event is expanded.
Vertical scrolling uses the same retained branch and signed destination
rules; no fresh entry or CheckBattle is implied by those connection labels.

## Default zone, request widths and independent Battle37 admission

**Confirmed:** the entire Map77_DefaultZoneEvent body is chkFlg537;
bne return_50A80; setFlg437; move.l #MAP_EVENT_RELOADMAP to MAP_EVENT_TYPE;
rts. It tests no location,436/536,435/535,900,921,party,HP or joined flag.
`RunMapSetupZoneEvent` selects the default only when invoked; ordinary
controlled movement uses the$1400 marker/event6 path. It initializes main's
actions, samples event X/Y words, invokes the live map callback, closes
windows, waits for VInt/main stop. Scroll entry alone does not invoke it.

MAP_EVENT_RELOADMAP=$000100FF. The big-endian **longword** at$FFA84A writes
TYPE.w=1, PARAM1.b=0 and PARAM2.b=255/CURRENT. It leaves PARAM3/4 at$FFA84E/F,
PARAM5 at$FFA850, padding and WARP_SFX untouched. A previous warp's coordinate,
facing and sound can survive. ProcessMapEvent's TYPE.w clear leaves those
parameters intact. The no-scroll consumer later samples current position,
then uses live retained PARAM3/4 if nonnegative and live PARAM5 as facing.
The request does not install new coordinates, copy a snapshot or clear sound.
Replacement/interruption between producer and consumer can change the result.

| Changed state at the named seam | Result |
| --- | --- |
|Scroll76→77, any437/537|No battle scan or automatic37; no437 clear.|
|Invoked77 zone,537 clear,437 either|Sets437, posts four-byte reload prefix; repeat does the same.|
|Invoked77 zone,537 held|Returns without write/reload; preheld437 survives.|
|Explicit77/CURRENT scan,437 clear|No37, including when537 is held.|
|Same,437 held,537 clear|Admits37;437 remains held.|
|Same,437 and537 held|Clears437 but admits37 **this time**; a later scan with437 still clear does not.|
|537 changes after zone|Scan rereads it. A newly held537 can clear437 while admitting37 now.|

This is callback/scan behavior, not evidence that a particular edge, movement
or natural play sequence reaches it. In particular536 is absent from both
the zone predicate and37's coordinate admission; no36-victory prerequisite
can be manufactured from the battle labels.

## Dry Stone caller, scene and changed layout lifetime

**Confirmed:** `msItemEvent 43,42,LEFT,ITEM_DRY_STONE,...` has six bytes
(X,Y,facing,item,relative word). ITEM_DRY_STONE=$73=115, Left2. These are
the first entity's coordinates/facing, not the investigated block42,42 or
the item-holder's position. The default callback is RTS.

`code/common/menus/main/mainactions.asm:FieldMenu`'s ITEM→USE branch obtains
the selected member, slot and item. Dry Stone is absent from the nine-item
`data/stats/items/usableoutsidebattleitems.asm` list. The effective menu
`IsItemUsableOnField` returns nonzero for it, selecting map events instead
of the target/use/remove branch. `GetPlayerEntityPosition` divides physical0's
sign-extended coordinate words by384 using **DIVU**, and masks facing with3.
`RunMapSetupItemEvent` masks d4 with ITEMENTRY_MASK_INDEX, initializes d6=0,
then matches first X/Y/facing/item row (FF coordinate/facing wildcard
only where declared). It does not test selected member HP or F900. A selected
row calls the callback, closes windows, waits VInt/main stop and preserves
d6 for the caller. Wrong coordinate/facing/item reaches default, d6 remains0.

Map77_ItemEvent0 first writes Dry Stone to DIALOGUE_NAME_INDEX_1.w and emits
txt421, **before checking900**. Clear900 sets900 before script cs_50ABE;
held900 emits422 instead. Both paths set d6=-1 and return. FieldMenu tests
nonzero and exits directly, with no RemoveItemBySlot. Thus the event reports
handled and retains the item on both first and repeat use; d6 does not attest
scene completion, changed passability or consumption. The unmatched d6=0
path emits73 then422 and exits the item-use flow without removal. The other
targeted field-item branch's removal is not part of this callback path.

Public text is limited to IDs, modes and short authored meanings. Ordered
tokens are complete; these are text tokens, not promised frame durations:

| ID / mode / speaker | Meaning | Ordered tokens |
| --- | --- | --- |
|421 /txt /no explicit speaker|Leader uses selected Dry Stone|LEADER,ITEM,D3|
|422 /txt /no explicit speaker|No effect|D1|
|73 /txt /no explicit speaker|Unmatched caller's selected member uses item|NAME,N,ITEM,W2|

LEADER expansion uses the dialogue service, not proof that the item-holder
is Bowie. Actual name/material/speech context remains caller-dependent.

The complete cs_50ABE stream, in source order:

| Index | Operation |
| --- | --- |
|0|playSound SFX_BIG_DOOR_RUMBLE|
|1|setQuake1|
|2|csWait60|
|3|playSound SFX_BOOST|
|4|flashScreenWhite20|
|5|setBlocks1,1,1,1,42,42|
|6|flashScreenWhite20|
|7|csWait60|
|8|setQuake0|
|9|csWait60|
|10|csc_end|

`mapscriptengine_1.asm:csc33_setQuakeAmount` with these unflagged operands
writes amplitude1 or0 directly. csc41 reads20, shifts right3, and DBF invokes
LaunchFading three times, then DuplicatePalettes:20 is not20 flashes or
a20-frame completion proof. csc34 calls CopyMapBlocks with the packed
source/size/destination words and sets both plane-update bits. The copy is
one working-layout block; no NPC, HP, party or additional story flag is written.
Three60 waits are ordered requests, not a measured total scene duration.

F900 is already held before rumble/quake/waits/flash/copy. Interruption can
therefore leave900 held with the old working block, quake1 or incomplete
presentation. Held900 repeat gives421+422 and does not replay/repair the
scene. `mapload.asm:LoadMapBlocksAndLayout` reloads the base then applies
held flag rows: a later fresh77 entry or scroll loading77 reapplies the same
1×1 copy. CURRENT negative LoadMap retains the existing working layout and
does not reapply that row; clearing900 alone does not undo an already copied
block until a later base load. Persistent flag, layout copy, plane update,
actual geometry/collision and traversable village route are different facts.

## Map53 default setup and Battle36 scan

**Confirmed:** the complete `data/maps/mapsetups.asm` route table has no53
entry. There is no map53/mapsetups directory to cite. GetCurrentMapSetup scans
map words toFFFF; matching entries start with their default pointer, then
each held flag overwrites it in table order untilFFFD. Absent53 reaches
`ms_Void:dc.w $FFFF`. GetMapSetupEntityList returns that sentinel as the
empty list; init/entity/item/description/zone callers test it before loading
any section pointer. Init/entity callbacks are skipped, item d6 and
description d7 remain0; zone retains its common VInt/main-stop epilogue.
This is absence of a setup, not a fabricated local RTS initializer. Common
fresh player/follower creation is independent of fixed setup entries.

Map53's single area `$B6C5C..$B6C7C` is0,0..27,31; secondary origins0,0;
both parallax256,256; both autoscroll0,0; type255; MUSIC_BATTLE_THEME_3.
Palette3, tilesets0,89,255,255,57 are separate resource declarations.
Flag/step/roof/other tables are endWord only. Chest table `$B6C94..$B6C9A`
declares24,27 /F192 /VALKYRIE then endWord; this is a local record, not an
award or authorization to expand search/loot or Battle36 contents.
Both warp rows at `$B6C82..$B6C92` use no-scroll0 and Down3:
source255,0→77,57,32; source255,31→77,57,35; terminator through$B6C94.
Neither exit requires completion536/537. Entering53 via77 row1 or2 selects
different nominal positions/facings but the same setup/scan predicates.

`code/common/maps/getbattle.asm:CheckBattle` scans ordered seven-byte records
with BATTLE_UNLOCKED_FLAGS_START400 and completed offset100:

| Battle / row address | Map / crop X,Y,width,height | Trigger X,Y | Unlock / complete |
| --- | --- | --- | --- |
|36 /$7B32|53 /0,0,28,32|255,255|436 /536|
|37 /$7B39|77 /16,32,48,32|255,255|437 /537|

The crop is output, not an inclusion test on player position. Trigger255
skips that coordinate comparison; actual source map and unlock are required.
Thus either53 entry can select36 regardless of its coordinate,536,437/537,
900 or town recruitment. Supplied436-clear never admits36 even with536 held.
Held436 with536 clear admits36 and retains436. Held436+536 writes the crop,
clears436 and **still returns36 now**; the next scan requires another unlock.
Changing536 between producer and scan affects that clear, not a new gate.

The selective accepted producer is `map59/mapsetups/s6_initfunction.asm`:
with999 clear it runs five shootdown scripts, then sets999, writes EGRESS_MAP
to Dojo, sets436,440,442,443 and returns. With999 held, that first arm and436
write are skipped. This is the accepted
[shootdown interface](./nazca-ship-shootdown-and-crash-handoff.md), not a claim
that999 alone repairs missing436 or that36 completion is needed for37.
Script loadMap calls during the shootdown are not this ordinary warp/MainLoop
scan. No initialization or outcome is inferred from a selected d7 battle ID.

## Actual lowering and current consumer boundary

**Confirmed (direct offline execution):** fresh OriginalPrograms instances
register all six Map77 defining files and local continuations before each
selected compilation. Real `compile_maps` consumes the registered canonical
unchanged. No synthetic success, consumer repair, SDK or host run is used.

| Actual output | Result / limit |
| --- | --- |
|cs_50ABE|11 instructions: typed sound/quake/wait/end; native flashScreenWhite at4/6 and setBlocks at5.|
|Map77_ItemEvent0|Root7 instructions, first native move.w; continuations byte_50AB6=3 and loc_50ABA=1 plus scene11. Typed flag/text/call branches exist after that first native; d6=-1 is not emitted as a usable item-result contract.|
|Default zone|Root3: branch537, set437, native move.l; return continuation1. This does not implement the mutable reload write.|
|Default item /description /init|One end each.|
|Event-enabled77 conversion|One map, zero fixed entities,7 programs; three ordinary Rebuild warps to29/53/53, two scroll frontier events; default interaction/source-zone; population and temp clears+80; one F900 copy, empty doors/roofs; init bound. No item-use event or Dry Stone scene binding, battle=null.|
|53 without eventMaps|One map, zero fixed entities, two Rebuild exits to77, one native setup-frontier program, no population/layoutEvents/setup binding; battle=null.|
|53 with eventMaps=[53]|Refuses with UnboundLocalError: local variable setup is unassigned. Partial compiler has zero programs; no map result. Absent source setup is not repaired by enabling the option.|

The unchanged [compiler](../../../../src/sf2tool/remake_exploration_programs.py)
and [map importer](../../../../src/sf2tool/remake_exploration_maps.py) own these
results. Destination selection admits a structural warp; it does not supply
destination content, geometry, palette or a successful scene. Conversion
does not bind item-event tables or chest services. The three-word native
layout operation remains unsupported despite a separate flag-copy structure.

[ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)
reads native-call as UnsupportedInstruction;
[ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)
stops Unsupported at program-opcode. Full item callback stops at its initial
name-index move before text/flag; a direct scene start can reach quake1 then
stops at first flash before copy/quake0, subject to preceding acknowledgements.
Clear537's zone can set437 before its unsupported request; held537 returns.
These are actual path limits, not guaranteed atomic rollback or host results.

[WorkingMapLayout](../../../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs)
and [traversal](../../../../remake/src/Sf2.Remake.Domain/Maps/OriginalMapTraversal.cs)
provide layout copies, active-area and collision rules.
[MapTransfer.Build/Apply](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)
applies held layout flags on Build, supports same-map Preserve and Rebuild,
and checks destination/setup/entry position. Cross-map Preserve is refused;
it does not express the original retained regional scroll. ReloadScene
separately replaces the base working layout without native CURRENT init,
and is not the longword reload packet.
[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)
matches supplied markers, enters typed zones and performs door/roof work;
none supplies this missing item-use caller or native mutable packet/widths.

The [battle-route compiler](../../../../src/sf2tool/remake_exploration_battles.py)
builds routes only from explicitly selected battles, compiles before/start
hooks and attaches them to maps; this slice supplies no battle routes and
does not compile those out-of-scope hooks. Its encounter preparation accepts
only selected2/3. [BattleEntry](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs)
uses bound ordered routes/unlock/completed policy; completed admission depends
on SourceStory policy and can clear unlock before selection. A generic route
type is not an actual36/37 encounter, roster, load, intro or resource binding.

Fresh common [SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)
initializes64 normal aliases0..31/128..159 to physical0 before allocated
overwrites. [ExplorationState](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationState.cs)
can therefore resolve an unallocated normal alias to player0; removed/absent
aliases and out-of-contract references are separately unresolved. Empty fixed
entities do not establish absent followers or guaranteed safe entity failure.
Reuse the accepted town correction for the full distinction.

Godot [ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)
projects typed quake (alternating by simulation tick, suppressible by reduced
flash) and acknowledges admitted cues; Sound calls
[SessionAudio](../../../../remake/game/src/Audio/SessionAudio.cs).Play with resource
bindings required. Existing FlashWhite presentation's four-pulse binding does
not implement native csc41(20)'s three fade requests. It cannot supply missing
scene opcodes, packet semantics, item services or36/37 content. No running
instance was accessed. Research completion here is the bounded static/consumer
boundary, not a playable Galam, successful Dry Stone or engine-ready battle.

## Reproduction, preservation and Unknowns

Load current ignored selections in the launching process. Source root ends
in disasm; canonical is the existing registered read-only export. Use unique
receipt paths:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1126/observe.py <configured-source-disasm-root> --canonical <registered-canonical-export> --receipt local/issue1126/observe-reproduction-01.json
uv run --locked python -X utf8 local/issue1126/review.py <configured-source-disasm-root> --receipt local/issue1126/review-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The ignored direct recipes check source/table/scene order and polarity, exact
changed-state results, tokens, actual consumer limits, pin/immutable inputs,
two-path/navigation-only scope, links, UTF-8/LF and private boundary. Source
and tracked prose own durable findings; recipes/receipts are coordination
state. Apply ADR0004's relevant width/selector/order/polarity/provenance
checklist; new parser/schema/fixture/H3 obligations are N/A. Clean committed
planner and exact-head scoped CI are separate receipts; suggested local
gates are not executed passes. Explicit documentation agreement excludes
normal/full/old comparisons, SDK/.NET/Godot/emulator/H3/H4/screenshots,
full exports and new verification machinery.

Preserve1124's original b571a496 audit/CI39537957812 that missed alias0,
accepted d5bffb02 focused correction/CI39538519434,1116's collision-polarity
correction,1114's follow offset2=(−24,0)/local-action refusals,1112's complete
import @Return partial and1104's defeat versus FIRST_ALLY kill-accounting
correction. The linked predecessors retain their details and twelve completed
old comparison failures; none is interrupted, superseded by a green new
check or an automatic rerun queue. The actual53 event conversion refusal
and Dry Stone/zone native limits above remain visible without consumer repair.

**Unknown:** natural walking/zone/item/door/exit reach and order; actual
436/536/437/537/900/999, caller/member/item slot/inventory, party/HP/MP/status/
class/resources/aliases/followers/raft/player/step state; geometry/collision/
materials; interruption/interleaving and finished text/audio/quake/flash/copy/
scroll/load/roof/input services; original VInt/DMA/render timing, usable
control and save/load persistence. Unknowns do not authorize new acquisition.
Battle36 initialization, roster, opening, turns and results; Battle37 content;
village/kingdom bodies and921 production remain outside this slice.

Freeze a clean committed/pushed Draft PR with exact base/head/tree, direct
checks/planner/actual CI, preserved failures/Unknowns and owned process state.
Project Review / Verify and one successful main-gate notification precede
writer stop. Independent acceptance and merge belong to main-gate; no next
Issue, merge, cleanup or archive.
