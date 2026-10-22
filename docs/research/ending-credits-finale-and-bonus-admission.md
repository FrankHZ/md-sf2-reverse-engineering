# Ending credits, finale and bonus-battle admission

Evidence date: 2026-10-22. **Confirmed (static):** the returned kiss route
calls credits, returns through the complete castle finale, then tail-enters
WitchEnd and EndGame. EndGame writes a completion bit in SRAM-mapped storage,
requests the witch/jewel presentation, waits, and checks the real Battle0
entry. A normal BattleLoop return leads to a start-vector reset. This is a
source account, not evidence of natural completion or playability.

## Agreement and provenance

Standard rigor for [Issue #1192](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1192)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one coherent credits → finale → witch → hidden-battle admission account,
including the caller's reset suffix. Accepted base
`6256c0965b556d033bf1241da77f5d7b7868fd66`, tree
`a0bdf5a9d3d0190a592c420825f8033024071ff2`; no stacked dependency.
Own this document and navigation in the
[chamber predecessor](./granseal-chamber-interactions-and-ending-kiss.md) and
[special screens](./special-screens.md#ending-screens). Battle0's actual
battle, victory/defeat/escape rules, post-reset menus, rendering parity,
runtime collection and engine implementation are outside this slice.

All original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`map/` means `code/common/scripting/map/`; `entity/` means the sibling
`entity/`. Addresses and packed words are hexadecimal, intervals half-open;
IDs, positions and waits are decimal. Registered USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new ROM/source equivalence or hardware observation is claimed.
Reuse [common scripting](./common-scripting.md), [common maps](./common-maps.md),
[party services](./common-stats.md#confirmed-services),
[SRAM services](./technical-services.md#sram-save-system-contract),
[interrupts](./technical-interrupts.md) and [gameflow](./gameflow-core.md).
The precise completed-flag branch of CheckBattle is stated below; the older
common-maps summary is not a substitute for that use-site predicate.

The five main source files total32,711 UTF-8 bytes: endcredits.asm,
cs_introendroutine.asm, cs_end.asm, witchendinit.asm and witchend.asm. Read
actual adjacent callees, constants, staff format, Battle0's coordinate row and
actual consumers. Reuse the18,285,979-byte init/script pair with provenance
`local/issue1005/pair.txt`; do not regenerate it. Work is linear in selected
source plus that existing pair, with small local review receipts. Direct-check
reassessment signals are <60s/<512MiB, <2MiB per output and <4MiB total new
ignored output; peak memory is **unmeasured**. No acquisition, rendering,
large intermediate corpus or model/API batch is needed. One selected lowering
answers the concrete mapLoad/entity-table and native-service seams; its
exception and partial programs are retained without retry or expansion.

## Credits entry, portraits and completion

**Confirmed:** `data/maps/entries/map20/mapsetups/s3_zoneevents_543.asm`,
Map20_21F_ZoneEvent0, tests the raw YesNoPrompt D0 word. Zero runs cs_EndingKiss;
on normal return: JSR PlayEndingCredits → D0=−1 → JSR PlayIntroOrEndCutscene →
tail JMP WitchEnd. Nonzero restores sound driver/town music and RTS. The
predecessor owns the prompt, HP-sensitive approach and kiss-picture service;
this account begins at their returned credits call.

`code/common/scripting/endcredits.asm:$1AC068..$1AC38E` contains complete
PlayEndingCredits, ClearPalette2, VInt_EndCredits, EndCreditSubroutine and
GenerateEndingCreditsFont. Their ordered interface is:

| Phase | Requests and state |
| --- | --- |
| Display entry | Initial JMP *+4; clear contextual VInts; WaitForVInt; disable display/interrupts; clear VSRAM/sprites; enable; InitializeDisplay; clear **base** palette2; disable; generate font. |
| Portrait layout | Write8 rows of6 sequential tile words from$27C0, advancing tile index by an extra2 and destination by$34 after each row. Transfer$400 words from PLANE_B_LAYOUT to VRAM$E000, type2. Clear VInts again; enable display/interrupts. |
| Scroll/staff entry | Zero D6 and update foreground/background H/V scroll buffers; enable queue processing; FadeInFromBlack; set long staff pointer CONFIGURATION_MODE_OR_GAME_STAFF_POINTER=$FFB1AA from p_table_GameStaff; add VInt_EndCredits; Sleep120. |
| Portrait list | j_UpdateForce; TARGETS_LIST_LENGTH−1 in D7 and TARGETS_LIST in A0. Enter loop directly, save D7/A0 with MOVEM.L, read ally byte, GetAllyPortrait then LoadPortrait. |
| Each portrait | Clear8 longwords=32 bytes of **current** palette2; request ApplyVIntCramDma. RNG range$40 plus$80 sets background H scroll; RNG range$60 plus$20, negated, sets background V scroll. Fading bitfield2, IN_FROM_BLACK, clear timer word/pointer byte, reload counter from maximum; Sleep180. OUT_TO_BLACK, reload timer/pointer/counter; Sleep44. Restore D7/A0, advance A0 by1, DBF. |
| Completion | Spin until byte at live staff pointer is FF; clear base palette2; Sleep180; FadeOutToBlack; Sleep120; RTS. No local removal of VInt_EndCredits or restoration of the prior contextual list. |

`j_UpdateForce` resolves through s02_jumpinterface.asm to
`code/common/stats/battleparty.asm:UpdateForce:$9900`. It scans allies0–29,
checks each joined flag, appends joined allies to TARGETS_LIST in ID order,
and independently partitions active/reserve lists using the active flag.
It does **not** filter current HP or use only the current battle party.
The portrait loop saves its count/list pointer before helpers reuse D7/A0,
so RNG does not replace its outer count. There is no zero-length check before
entering the body; do not invent a safe empty-force exit. Actual joined/active/HP
state and portrait availability are Unknown.

`code/common/tech/randomnumbergenerator.asm:GenerateRandomNumber` updates the
word seed as (seed×13+7)&FFFF, scales by twice D6, swaps and shifts right1.
The supplied ranges yield0..63 or0..95, requesting initial scroll H128..191
and V−32..−127. VInt subsequently changes H; these do not fix visible positions.

VInt_EndCredits tests FRAME_COUNTER bit0: even decrements background H by1
and updates H. If staff byte is not FF and the frame is even, foreground V
low nibble0 calls EndCreditSubroutine, then foreground V increments by1.
The tail always updates H/V and enables DMA processing, including odd frames
and after FF. The earlier even-frame H call and tail H call are both present.
FF stops staff/foreground advancement, not all scrolling.

`p_table_GameStaff` in s06_gamestaffpointer.asm binds table_GameStaff in
`data/scripting/gamestaff.asm:$4201E..$425E7`. Records contain a leading column
byte, zero-terminated character bytes for nonzero columns, a **single zero
byte for a blank line**, and final FF. DCB.B zero runs are repeated blank
records, not embedded text. Actual nonblank columns are2,12,10; the staff-name
corpus and graphic payloads are not reproduced here.

EndCreditSubroutine consumes the leading byte, chooses layout offset
`(((VScroll >> 3)+$1C)&$1F)<<6`, clears32 longwords=128 bytes, two tile rows.
Zero immediately publishes the advanced pointer. Nonzero shifts D1.W left1
and adds it to the row pointer; the leading MOVE.B does not itself clear D1's
upper byte. Each character iteration explicitly clears D1.W, reads a byte,
stops on zero, or builds `(character−$20)<<1 | $C100`, writing that word and
its successor at+$40, then advancing horizontally by2 bytes. It publishes
A0 **after** the consumed zero, queues$400 words of plane A to$C000, type2,
and RTS. FF is checked before this routine consumes it. Unproved ambient
register state is not silently zero-extended into a stronger placement claim.

GenerateEndingCreditsFont loads p_tiles_Base via LoadStackCompressedData to
$FF6802, reads$FF6C02 and writes strided longwords at$FF8804 using64×8
iterations, masks$22222222, XOR and shift. A second64-glyph pass uses$FF8808
(RAM alias TARGETS_REACHABLE_BY_ITEM_NUMBER), neighbor bytes±4, masks$10/1
and a zero row. Two subsequent passes use BATTLE_ENTITY_MOVE_STRING=$FF9804
as shadow scratch: initial zero plus1023 longwords, then1023 combines against
the generated buffer. Final transfer: $800 **words**=4096 bytes from$FF8804
to VRAM$2000, type2. These are temporary RAM usages, not battle target/move
operations. Font resource/pixel ownership remains with special screens.

There is no local controller skip in these credits bodies. Sleep's positive
word count calls WaitForVInt that many times; zero returns. The terminal spin
has no timeout/input branch. This is not proof external interruption is
impossible: `code/common/tech/interrupts/vint.asm` contains the separately guarded
AFTER_INTRO_JUMP_POINTER/Start escape; its live value is Unknown here.
Staff progress requires enabled contextual servicing. Sleep requests are not
measured wall-clock durations.

## Finale setup and entity identities

**Confirmed:** `data/scripting/map/cs_introendroutine.asm:$47EF8..$47F7E`
saves D0.W, initializes window properties, writes MOUTH_CONTROL_TOGGLE and
DEACTIVATE_WINDOW_HIDING bytesFF, clears VInts, then adds in order:
VInt_UpdateMapPlanes, VInt_UpdateEntities, VInt_UpdateViewData,
VInt_UpdateScrollingData, VInt_UpdateSprites, VInt_UpdateWindows,
VInt_UpdateMapAnimations. CURRENT_BATTLE becomes NOT_CURRENTLY_IN_BATTLE;
exploration sprite timing becomes start2/counter0/blink threshold1.
It restores/tests D0.W: zero runs four intro scripts; **every nonzero value**
takes piano music and EndCutscene. This caller supplies−1. Normal csc_end
returns through wrapper RTS to the zone's WitchEnd tail jump. Credits' VInts
are replaced, not saved/restored.

`data/scripting/map/cs_end.asm:EndCutscene:$49058..$492CC` has119 map commands,
488 encoded command bytes and140 inline action bytes in628 physical bytes.
`ce_492CC:$492CC..$4931C` is80 bytes: six main-entity bytes, nine eight-byte
rows, two-byte terminator. These are separate structures/source-text sizes.

First `mapLoad MAP_GRANSEAL_CASTLE_1F,19,33` requests map20 and a camera origin,
not player placement or setup population. Opcode$48 reaches csc48_loadMap:
neutralize camera target; LoadMapTilesets; wait for fading zero; deactivate
contextual functions; pack view coordinates and LoadMap; enable display/
interrupts; reactivate; WaitForVInt. Unlike opcode$37 it does not start an
out-to-black fade. It does not implicitly execute ordinary setup/init.

Next `loadMapEntities ce_492CC`, opcode$42, separately deactivates contextual
functions/disables display and interrupts, reads main X/Y/facing words,
InitializeMapEntities, LoadEntityMapsprites, enables display/interrupts and
reactivates functions. Main Bowie is63,63/Down. Complete rows:

| Logical identity | Declared position/facing | Sprite/initial stream | No-follower physical slot |
| --- | --- | --- | --- |
|Rohde11|63,63/Down|ally-derived /eas_Init|1|
|Chester2|63,63/Down|ally-derived /eas_Init|2|
|Luke10|63,63/Down|ally-derived /eas_Init|3|
|Jaha3|63,63/Down|ally-derived /eas_Init|4|
|Peter7|63,63/Down|ally-derived /eas_Init|5|
|Astral128|21,40/Right|209 /eas_Init|6|
|King Granseal129|21,39/Right|203 /eas_Init|7|
|Minister130|22,38/Right|205 /eas_Init|8|
|Elis131|63,63/Down|204 /eas_Init|9|

Bowie is slot0. `map/mapfunctions.asm:InitializeMapEntities` clears entities/
aliases, allocates enabled followers first, reuses an existing ally alias
rather than redeclaring that row, allocates remaining rows and finally Bowie.
Ordinary row coordinates are masked3F then multiplied by MAP_TILE_SIZE384.
Aliases128–131 are not slots. The slot column assumes **no followers**;
no joined/HP test makes these five ally rows optional. Follower reuse can
change their physical placement/state. Later setSprite changes Rohde11 to
NPC_ROHDE170; it does not rename him to nonally128.

All rows use eas_Init. Bowie explicitly waits for eas_Init, then custom
speed20/20→jump idle; Astral128, King129 and Minister130 each get the same
custom speed20/20→idle. Other rows retain init speed32/32 unless later changed.
eas_Init's orientUp writes FLAGS_B orientation0, **not** independent FACING.
Refresh reads and rewrites existing FACING. The #1190 correction remains
binding: init does not replace declared Down/Right with facingUp. Autofacing,
later movement and explicit setFacing remain separate mechanisms.

## Complete castle finale choreography

**Confirmed:** there is no text command. This staged ledger retains all ordered
map requests, waits, parallel movements and repeated gestures. It describes a
procession and reactions without invented dialogue or marriage/awakening flag.
W installs a stream and waits for its pointer to reach eas_Idle; A installs
without that wait. R/L/U/D mean Right/Left/Up/Down; counts are source operands.
`waitN` means csWaitN. Setup/speed requests above precede these rows.

| Phase | Ordered requests after setup |
| --- | --- |
|Bowie greets court|setSprite Rohde170; fadeInB; wait60; setPos Bowie25,35/R; W Bowie DR1→R1; wait30; W Bowie D4; face Bowie L; W Astral R1; W King R1; wait60; nod Astral; nod King; nod Bowie; wait40; W Bowie U4; face Bowie L; wait80.|
|Elis joins|setPos Elis25,35/R; W Elis DR1; wait40; nod Bowie; wait60; **A Bowie D4**; W Elis R1→D3; face Bowie L; face Elis L; A eas_Jump Astral; A eas_Jump King; W eas_Jump Minister; wait40. Elis's wait does not join Bowie's earlier stream.|
|Court reactions|face Minister D; wait30; face King U; nod Minister; wait60; face Astral U; wait30; face King D; nod Astral; wait40; face King R; wait20; face Astral R; wait40; face Bowie U; wait20; face Elis D; wait60; face Bowie L; wait10; face Elis L; wait30.|
|Pair moves left|**A Bowie L4**; W Elis L4; nod King; nod Elis; wait60; face Minister U; wait30; W eas_Jump Minister; wait30; W Minister U1; face Minister D.|
|Companions arrive|setPos Rohde23,37/D; **A Rohde D1→L2→D2**; wait20; setPos Chester23,37/D; **A Chester D1→L2→D1**; wait20; setPos Luke23,37/D; **A Luke D1→L2**; wait20; setPos Jaha23,37/D; **A Jaha D1→L1**; wait20; setPos Peter23,37/D; **A Peter D1**. No map-script join for these five arrivals.|
|Faces and reactions|face Rohde R; wait10; face Chester R; wait10; face Luke D; wait10; face Jaha D; face Bowie U; face Elis U; wait30; face Bowie L; face Elis L; wait30; face Bowie U; face Elis U; wait40.|
|Farewell/return|A eas_Jump Rohde, Chester, Luke, Jaha, Peter **in that order**; face Elis L; nod Astral; face Bowie L; wait50; nod Bowie; A eas_Jump the same five in the same order again; fadeOutB; csc_end. No final all-actors join.|

entityActions includes endActions and a generated jump idle. Directional actions
request a destination and wait for that actor's destination before the next
directional action; enclosing A still returns after installation.
csc14/csc15/csc2D resolve the physical slot and write its **byte** to
ACTSCRIPTWAITTIMER; only csc2D clears FLAGS_A bits5/6 with AND9F. Custom/
external installation does not add that clear. Replacement is not a join.

eas_Jump is autoFacing OFF → ac_moveRel0,−1 → ac_wait5 → ac_moveRel0,+1 →
ac_wait5 → ac_moveRel0,0 → autoFacing ON → jump idle. Relative moves use
then-current X/Y plus offset×384, not the prior destination. Destination
conflicts/scheduling still apply; these waits do not establish a physical arc,
complete tile travel or exact final position. Async replacements can precede
prior completion.

csc19 setPos reads a **byte** actor and skips four operand bytes for a dead
ally; csc23 setFacing reads a **byte** actor and skips two. Their helper checks
live ally HP and bypasses nonallies. HP0 can suppress Bowie/companion placement/
facing while later streams still install; no supplied constant HP or revival.
setSprite uses word actor/sprite operands with no such HP guard: sprite<30
requests ally lookup, NPC_ROHDE170 is literal. Installation and nod have no
ally-HP guard either.

csc26 nod reads a word actor, sets animation counterFF, Sleep10, loads/shifts/
DMAs mapsprite, Sleep20, refreshes, Sleep10, then counter0. D7=0 DBF gives
one inner pass. These are sprite/animation requests, not HP/status changes.
csc_end restores interpreter registers and ordinary view-scroll/speed cleanup,
not prior population, speed/facing, camera target, music or VInt list. Wrapper
RTS then the zone's tail jump close the route. Motion/timing/pixels are Unknown.

## WitchEnd, text and completion storage

**Confirmed:** `code/specialscreens/witchend/witchendinit.asm:WitchEnd`
`$7094..$70D2`: Sleep60 → InitializeWitchSuspendVIntFunctions → blink counter
word30, mouth-phase word_FFB07C=6, blink-toggle byteFF → txt238 → clsTxt →
clear blink-toggle → ReinitializeWitchLayout → FadeOutToWhite → remove
VInt_WitchBlink → tail JMP j_EndGame. `s05_jumpinterface.asm:j_EndGame` is
JMP EndGame(pc). Neither tail jump pushes a return address; EndGame's normal
path ultimately jumps to reset rather than returning to the zone.

`witch/witchfunctions.asm:InitializeWitchSuspendVIntFunctions` writes hidingFF,
clears VInts, zeros four scroll buffers, waits DMA, clears VSRAM/sprites and
initializes display. It clears MOUTH_CONTROL_TOGGLE, selects speech bleep4,
BuildWitchScreen, patches head from layout+$700 with packed dimensions$707,
queues/waits DMA, fades in, adds VInt_UpdateWindows, initializes windows,
waits one VInt, then adds VInt_WitchBlink. BuildWitchScreen loads tiles at
$FF6802 and transfers8192 **words** to VRAM$2000; copies2048 layout bytes
and transfers1024 words to$E000; copies32 palette bytes to base1 and the
next source palette to base4; initializes blink30/mouth6. ReinitializeWitchLayout
copies$800 bytes and tail-queues$220 words to$E000 with a queue wait. The
castle VInts are not left active.

Text comes from `data/scripting/text/gamescript.txt`, decimal ID → hex key.
These are author paraphrases and complete ordered control-token arrays, not
literal translated dialogue or newly assigned map speakers.

| ID /key /actual call | Author meaning | Ordered controls |
| --- | --- | --- |
|238 /00EE /WitchEnd|The forest witch says the player's success frees her and reacts to the player's surprise.|N,N,D2,D2,N,D2,N,N,D2,D2,D2|
|239 /00EF /EndGame|Her restored form thanks the named leader, anticipates meeting again and promises remembrance.|NAME;0,N,N,D2,D2,N,N,D2,D2,D2|
|465 /01D1 /EndGame|An announcement introduces additional content.|W1|

EndGame's first instruction at$279D8 is **BSET #7,(SAVE_FLAGS).L**: byte
read/modify/write at$202035 (`.L` is absolute-long addressing, not a four-byte
store), setting mask80 while retaining other bits. It is SRAM-mapped storage,
not COMBATANT_DATA story flags. `code/romheader.asm` declares SRAM$200001..$203FFF;
sf2const.asm names SAVE_FLAGS=$202035 with occupied-slot bits0/1. No SaveGame,
slot-data copy or checksum write accompanies this set. There is no additional
SRAM bank/write-enable setup in this local body. Header/mapping and instruction
are source evidence; hardware write success/durable persistence are Unknown.

Visible consumers/writers are separate from a story flag: `witchstart.asm`
tests bit7 after naming Bowie, then Start before the additional ally-naming
loop. `gameflow/special/configurationmode.asm:CheatModeConfiguration` requires
Start then Up and bit7 for its SoundTest branch; its configuration choice also
sets/clears bit7. `sramfunctions.asm:CheckSram`'s signature-failure path clears
SRAM and SAVE_FLAGS; normal slot occupancy/checksum handling changes bits0/1.
Existing bit7 does not prove this ending executed, and this write does not
save the whole party or certify lasting completion.

## Restored witch, falling jewels and final jewel screen

**Confirmed:** `witchend.asm:$279D8..$27D8E` contains EndGame and three helpers.
After the completion-bit write the complete sequence is:

| Stage | Ordered resources, state and requests |
| --- | --- |
|Restored witch|Disable display/interrupts; decompress p_tiles_WitchEnd to$FF6802; transfer$2000 words to$2000. Copy$800 layout bytes to plane B; transfer$400 words to$E000. Write longwords$21382139/$21422143 at plane A+$21E/+$25E; transfer$380 words to$C000. Copy the same32-byte WitchEnd palette to base1 and base2. Enable; Sleep60; FadeInFromWhite.|
|Speech|Blink counter30/toggleFF; add VInt_PerformEndingWitchBlink; speech bleep2; txt239; clsTxt; Sleep90. OUT_TO_BLACK, clear fading timer/pointer, reload counter, palette bitfield1; Sleep60.|
|Falling overlay|Clear blink toggle; remove ending-witch blink; add VInt_FallingJewels; Sleep30. OUT_TO_BLACK with reloaded timer/pointer/counter, bitfield2; Sleep60; remove falling-jewels VInt.|
|Jewel replacement|Disable; clear the two plane-A longwords; transfer$380 words to$C000. Decompress p_tiles_JewelEndScreen to$FF6802; transfer$2000 words to$2000. Copy$800 jewel layout bytes to plane B; transfer$400 words to$E000. Copy64 palette bytes to base1/base2; enable.|
|Jewel reveal|IN_FROM_BLACK, timer/pointer clear, counter reload, bitfield1; Sleep120. D0.W=0; sub_27C64 seven times, each followed by NOP; Sleep120. IN_FROM_BLACK/reloaded state, bitfield2; Sleep300. Sound fade-out; Sleep END_GAME_TIMER.|
|Bonus notice|txt465; clsTxt; FADING_COUNTER_MAX byte3; FadeOutToBlack; clear32 longwords=128 bytes from base palette1. Clear entities; flag400; CheckBattle map63; D7.W→D1.W; BSR BattleLoop. Return/reset below.|

s14_pointers.asm binds WitchEnd to palette/layout/tiles_EndingWitch;
s17_pointers.asm binds jewels to palette/layout/tiles_EndingJewels.
CopyBytes preserves D7/A0/A1: the second WitchEnd palette copy reuses the
**same**32 source bytes after advancing only destination by NEXT_PALETTE32.
It does not consume a second source palette. Jewel copy is explicitly64 bytes.
DMA lengths are words; CopyBytes lengths are bytes. Resource/material owners
remain with special-screens; no payload is published.

VInt_FallingJewels decrements foreground V by1, updates V and enables DMA.
There is no termination test; EndGame removes it after30- and60-count sleeps.
No implicit zero-scroll restore follows. VInt_UpdateWindows from witch setup
remains installed across these local add/remove pairs until later battle/base
setup or reset clearing.

VInt_PerformEndingWitchBlink uses blink-toggle byte, BLINK_COUNTER word and
word_FFB07C. Enabled: clear local update word, decrement blink counter; at3
patch4×3 words from layout+$700 into$FFE0DC; at0 patch from+$720 and reload
RNG(120)+30, range30..149. CURRENTLY_TYPEWRITING nonzero decrements mouth
phase; at5 patch4×1 from+$718 into$FFE19C; at0 patch from+$738 and reload
RNG(5)+10, range10..14. When not typewriting, phase>5 skips mouth patch;
phase<=5 uses close/reload. Nonzero local update queues$200 words of plane B
to$E000 and enables DMA. Toggle-zero skips local-word initialization before
the tail tests it: do not infer a guaranteed no-DMA return from that branch.
EndGame writes blink30/toggleFF but does not freshly initialize mouth6 or
CURRENTLY_TYPEWRITING here. Incoming phase/callback schedule remain Unknown.

sub_27C64 saves D0.W, selects jewel layout+$700+(D0.W×30), patches5×3 words at
$FFE31C, QueueDmaForWitchLayout ($220 words and wait), Sleep4, restores D0.W,
increments and RTS. Seven explicit calls select offsets$700,$71E,$73C,$75A,
$778,$796,$7B4, leave D0.W=7 and request28 Sleep handshakes plus DMA waits.
They are not seven independent animations or observed frames. UpdateWitchLayoutZone
preserves D0–A1 and copies packed width/height rows at destination stride$40.

END_GAME_TIMER=$2A30=10800. Sleep requests10800 positive WaitForVInt handshakes;
the source's three-minute comment corresponds to10800/60, not measured time,
skip affordance or hardware-region guarantee. Neither this call nor Sleep
polls input. Text465's W1 is a separate subsequent text-control wait. Global
VInt escape state and actual input are not established here.

Cleanup is local: paired blink/falling callbacks are removed, selected layout/
palette/entity state is overwritten, later reset clears contextual functions.
No general restore of prior scroll, palette4, hiding/mouth bytes, speech SFX,
fading maximum3, scratch, windows, music or saved state is promised. Actual
DMA/CRAM publication, pixels, audio and interrupted prefixes remain Unknown.

## Battle0 admission and normal-return reset

**Confirmed:** EndGame's j_ClearEntities resolves to entityfunctions_1.asm:
ClearEntities: clear49 entity records and64 alias bytes, off-map positions/
destinations and reset walking scratch pointer. It does not heal allies,
clear completion flags or establish battle combatants. Then setFlg400 and
D0.W=MAP_MAGIC_TUNNEL_HUB=63 → CheckBattle; no preceding CURRENT_MAP/player
coordinate write is added here.

`code/common/maps/getbattle.asm:CheckBattle:$799C..$7A36` saves D1–D6/A0 and
input map word, treats byteFF as current-map selection, scans45 seven-byte
rows in order, and requires map match, unlocked400+index, and each coordinate
eitherFF or equal to supplied coordinate byte. First row of
`data/battles/global/battlemapcoords.asm:$7A36`:

| Index /unlocked /map | Area X,Y,width,height | Trigger X,Y | Completed |
| --- | --- | --- | --- |
|0 /400 /63|0,12,32,36|FF,FF|500|

Both axes are wildcards, so inherited D1/D2 do not restrict this **particular**
match. Explicit map63 and freshly set400 also matter; flag400 alone does not
guarantee admission in any map/context. CheckBattle writes four area bytes
before testing completed500. If500 is **clear**, branch straight to return
without clearing400; if500 is **held**, clear400 then return the match.
Completion does not reject the match. This caller does not clear500.

No match: D7=−1 and pop input map to D0; restore D1–D6/A0. Match: pop map to
D1, then saved D1 is restored; supplied map63 remains D0. EndGame does **not**
test D7's sentinel: MOVE.W D7,D1 then BSR BattleLoop. Its unconditional call
is not a safe generic no-match path. The direct guard checks changed map/
unlocked/coordinate/completed inputs, without simulating original gameplay.

`code/gameflow/battle/battleloop_1.asm:BattleLoop:$23A84` takes D0.B map and
D1.B battle. Entry clears player type, sets399 and tests suspend88. Ordinary
initialize writes CURRENT_MAP/CURRENT_BATTLE, sets base VInts and invokes
before-battle script. This is only the receiving interface; flag88's alternate
path is a real precondition, not an assumed fresh start. Battle0 initialization,
enemies, turns/outcomes and escape belong to the next coherent slice.

After **normal return**, EndGame requests FadeOutToWhite, clears VInts, disables
display/interrupts, writes SR=$2700, restores SP from InitialStack, loads A0
from p_Start and JMP(A0). It does not RTS to zone/menu. This is the actual
software reset boundary; Start's later hardware/menu behavior and persistence
are not re-investigated. An interrupted/nonreturning BattleLoop does not prove
this suffix occurred.

## Actual producer and consumer boundary

**Confirmed:** one selected OriginalPrograms observation reused the pair, with
no final preparation/execution. source_operations stops at the next global
label; its returned native fragments are not whole-service emulations.
Preserved output has six program objects and one exception:

| Selection /actual owner | Expression or refusal |
| --- | --- |
|EndCutscene /[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|119 instructions; first mapLoad is native-call. Typed suffix includes scene-entities for main63,63/all nine rows/population metadata, init/speed/actions, position/facing/sprite, nod/fades/waits/end-map-script. Entry refusal prevents ordinary reach into that suffix.|
|PlayEndingCredits /same|12 instructions, all native, first JMP. First global loc_ boundary ends import before portrait/staff loops. No credits service completion.|
|PlayIntroOrEndCutscene /same|Partial32 instructions, all native; ValueError: source program target not imported: IntroCutscene1. Compiler traversed zero/intro branch, did not evaluate live nonzero predicate/bind end path. Partial remains; no expanded intro import/retry.|
|WitchEnd /same|15 instructions,12 native; first BSR Sleep is native. Typed text238 is availability after refusal, not reached speech or supported tail call.|
|j_EndGame /same|One native JMP, not an executable resolved EndGame service.|
|EndGame /same|119 instructions,113 native, first BSET. Text239/465 and some forms are typed, but RAM/SRAM/VInt/resources/subroutines/reset remain native. Import stops before loc_27C2C; BattleLoop/reset suffix is absent from this output.|
|[Map producer](../../src/sf2tool/remake_exploration_maps.py), [preparation](../../src/sf2tool/remake_exploration_content.py)|Source selection/population are separate from admitted map/material/program packages. No finale package, art/font/staff or battle package was published.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [story types](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Read scene-entities, motion, position/facing/sprite, gestures/fades/wait/end. native-call becomes UnsupportedInstruction, not automatic 68000 execution.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs)|Unsupported throws program-opcode **before** Next/commit; earlier commits survive. Scene reload allocates followers/aliases, waits sprite readiness. Typed position/facing omit original live-HP suppression. Typed state/list operations do not implement global RAM/VInts/SRAM or stack/register reset.|
|[EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [Domain motion](../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs)|Consume installed streams, physical-slot timer, optional AND9F, current-relative movement/speed/script-idle waits. This is not a reproduced finale schedule.|
|[Adapter presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [session view](../../remake/game/src/Exploration/ExplorationSessionView.cs), [capabilities](../../remake/docs/capability-status.md)|Project admitted text/gestures/motion and errors. Generic nod/fade/audio is not bound credits font/staff scroll, witch/jewel VInts, SRAM or reset. No SDK/Godot observation ran.|

**Inferred:** state/content primitives could express much castle choreography
after explicit mapLoad/native/HP/content admission. Credits, witch/jewel
resources/lifecycle, completion storage and battle/reset need separately owned
binding/support decisions. These findings are not an engine-ready claim or
implementation authorization. No engine/importer/parser/allowlist/schema/
fixture/index changes.

## Reproduction, retained results and open boundary

Ignored `local/issue1192/observe.py` is the sole selected lowering; do not rerun
it for ordinary review. observe-01.json retains native instructions, partial
object, exception traceback and unchanged pair metadata. It completed in0.1425s,
235,017-byte receipt; wrapper exception is a limitation, not complete import
or a green retry. No world package was published.

`capture-sections.py` retains author-inspected comment-free sections; `review.py`
and its local helper check complete sections, pin/source relationships,
corpus/table/text/token bindings, actual consumers, links/UTF-8/LF, owned diff
and private boundary. They read source, pair and observation, writing only a
**fresh, nonexistent** receipt. Complete recipe/helpers and handoff remain local
for independent review. Choose a fresh receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1192/review.py <configured-source-disasm-root> --receipt local/issue1192/reproduction-main-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Every uv call loads current worktree configuration in the same process.
Handoff records exact-head plan/actual CI/resources, peak memory unmeasured.
Normal/full/.NET/Godot/emulator/H3/H4/screenshots, old recipes/comparisons/
lowering/render, pair regeneration and full world are **SKIPPED**, not passed,
under explicit Issue acceptance. No ROM availability/runtime completion claim.

Preserve #1190 orientation/FACING P2, original author/main/CI PASS omissions
and accepted focused correction; #1188 search-kind text meanings/interaction
wrapper P2, original PASS omissions/corrections; #1184 physical-slot timer/CURSE
P2 and completed failures. Owning documents/local handoffs retain exact
identities/results. Native refusals, partial/failed imports, HP/raw-choice/
RotateRight gaps, twelve completed private comparison failures and carried
Unknowns are not rerun/relabelled. Any new direct-check failure remains in a
separate receipt and handoff; no prior receipt is replaced. Direct01 caught a
missing N in this document's238 token array. Correcting that array and the
local expected list leaves source and selected output unchanged; retain the
failed receipt rather than replacing it.

**Unknown:** natural reach, live flags/party/followers/HP/aliases/input/debug/
escape-pointer state, empty-force behavior, service completion/interruption,
action/VInt/register scheduling, positions/frames, font/staff/window/witch/
jewel/palette/DMA pixels, audio, hardware persistence, source-ROM equivalence,
original/remake playability. Battle0 combat/post-reset behavior are outside
scope. Special-screens' historical queue remains without reopening it. This
is not epic completion. Freeze a clean pushed Draft PR and stop the writer
for independent main-gate review/merge; same task owns corrections. No merge,
cleanup, archive, ref deletion or next dispatch belongs to this slice.
