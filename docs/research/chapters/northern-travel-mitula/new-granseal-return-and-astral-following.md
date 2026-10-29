# New Granseal return and Astral following

Evidence date: 2026-10-14. **Confirmed:** the selected mine-to-town route can
return to New Granseal Castle. Its audience zone gives F802 priority and runs
the return scene only while F805 is clear. The scene recounts earlier events,
announces Astral as adviser, installs current following and sets F70; after
return the caller sets F805 then F70. Neither the scene nor this admission
checks the Cannon or F761, recruits a battle ally, rescues Elis or travels north.
**Inferred:** the conversation presents Astral accompanying the party as adviser.
**Unknown:** natural arrival, supplied state, completed services/presentation,
usable control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #965](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/965)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one return/adviser outcome with conditional route, population, actor, service
and ordered flag boundaries. This document alone is owned. Reuse accepted
[dwarf recovery/Cannon](../southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md),
[town/Peter introduction](../eastern-parmecia/new-granseal-town-through-peter-introduction.md),
[year-later castle init](../eastern-parmecia/new-granseal-castle-year-later-control.md),
[departure/travel](../eastern-parmecia/new-granseal-departure-through-battle09.md),
[consultation palettes](../southern-travel-creed/evil-spirit-consultation-and-zone-return.md#special-graphics-palettes-and-presentation-services),
[request state](../../world/map-event-request-state.md) and
[request consumption](../../world/map-event-request-consumption.md).
No unmerged conclusion is a dependency. Stop at the conditional castle zone
return and relevant current/fresh-entry consequences. Janet, Dynamite, North
Cave scenes, later travel/battles, a full NPC dialogue catalog, runtime,
acquisition, engine/UI/translation and shared-file changes are excluded.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Existing fixtures bind USA retail
ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Unqualified scripts/setup files mean `data/maps/entries/map07/mapsetups/`;
map/entity engines mean `code/common/scripting/map/` and
`code/common/scripting/entity/`. Constants are from `sf2enums.asm` and RAM
symbols from `sf2const.asm`. Fnnn means flag nnn; address intervals are half-open.

Minimum evidence is selected pinned source/existing fixtures and the retained
18,285,979-byte map-init/map-script-engine pair. Linear comparison budget is
under one minute/512 MiB, with small inspection receipts and prose; no new
framework, regeneration, environment, installation or cleanup. Replan a
material contradiction or broader dependency. Counts locate evidence; exact
ordered source/fixture/consumer joins establish the static claims.

## Selected return route and admission

**Confirmed:** these rows of `data/maps/entries/mapNN/6-warp-events.asm` are
independent alternatives, not a forced or observed walk. Facing R/U/L/D is
0/1/2/3; any means byte255.

| Source table / zero-based row / trigger | Encoded request: scroll, map, X/Y, facing |
| --- | --- |
| Map30 `$AB39C..$AB3A6`, row0 `(any,21)` | 0; Map68 `(57,25)`, Down3 |
| Map68 `$BDA5E..$BDA90`, row3 `(any,0)` | Up `$11` (17); Map73 `(255,62)`, Up1 |
| Map73 `$C2F6A..$C2F84`, row0 `(29,52)` | 0; Map6 `(20,30)`, Up1 |
| Map6 `$9A3F4..$9A40E`, row0 `(any,0)` | 0; Map7 `(11,22)`, Up1 |

`exploration.asm:WarpIfSetAtPoint` divides supplied positions by384, scans
rows in order with negative trigger bytes as wildcards, and writes event1,
the five parameter bytes/trailing byte and WARP_SFX. The ordinary movement
caller needs layout attribute `$1000` after movement/collision admission.
None of these rows tests F760/F761/F802/F805 or guarantees passable terrain.
The single pending request must survive until `ProcessMapEvent` clears it
before dispatch; it is not a queue.

The no-scroll `explorationfunctions_2.asm:ProcessMapEventType1_Warp` plays and
clears WARP_SFX, idles physical0, unwinds exploration to MainLoop, samples
current tile coordinates, replaces nonnegative requested coordinate low bytes,
and returns map/facing/d4=0. Ordinary sampled high bytes are a premise; these
byte writes do not normalize arbitrary state. None of Map68/73/6/7 is in
`flagswitchedmaps.asm`. Map73/6/7 have no battle-coordinate row, but Map68's
sole row19 has wildcard triggers and uses F419 admission. F519 set clears419
**after selection**, still returning Battle19 for that call. The continued
mine-to-town route therefore assumes no Battle19 intercept (normally419 clear);
otherwise stop at that admission without reopening the battle. The rectangle
`(5,19,48,32)` is returned battle geometry, not a containment test. Persistent
story flags and PLAYER_TYPE are not cleared by these warp paths.

Map68's north border instead stays in exploration: it sets CURRENT_MAP=73,
calls `mapload.asm:ProcessMapTransition`, retains physical0 X because the
requested X byte is negative, writes Y current/destination as62*384 (the Y
register is explicitly cleared first), samples current facing and calls
DeclareRaftEntity. It does not apply the row's Up facing byte, run
SwitchMap/CheckBattle, rebuild residents/followers, heal, clear temporary flags
or run Map73 init. Tiles/blocks/layout transition and mode-dependent raft/
follower positioning are distinct from fresh admission. Map68's default zone
calls CheckRandomBattle19; Map73's default simply returns. Those are independent
of the warp rows and require the movement zone seam. Mode1 Caravan control
lacks walking's zone producer; carried mode0/1/2 and raft state must not be
silently normalized. Other exits and zone requests can interrupt any proposed
route; natural reach remains Unknown. No boarding/landing is performed here.

Ordinary ExplorationLoop admission clears pending, applies living/immortal
healing with the accepted HP/status limits, sets current map/not-in-battle,
loads tilesets, selects and rebuilds entities/sprites, clears flags256–383,
sets F80, loads map/base VInts and runs selected init before control. Main
control still depends on the ensuing pending request and player mode. Map68
has default-only empty residents and return-only init. Map73 also has empty
residents, but its fresh init can run the earlier voyage when F700 is clear,
or construction when F700/F508 are set and F705 clear; otherwise it returns.
The selected scrolling edge bypasses those checks. Earlier completed lineage
normally supplies F700/F508/F705/F707/F701/F702 and Peter's F67; those are
explicit history premises, not tests imposed by the return warp or audience.

## Town and castle population and init

**Confirmed:** `mapsetups.asm` and GetCurrentMapSetup use the last set selector.
Town Map6 selects default, then F701, then F702. F802/F805 do not select its
population. The [town owner](../eastern-parmecia/new-granseal-town-through-peter-introduction.md#setup-ordinary-population-and-init)
retains the complete default population; the two later lists are complete below.
References128–141 are NPC lookup references, not physical slots. A walking
entry's home equals its declared position and its range is1. `S` means
eas_InitSlow; `RL` means eas_RightLeftLoop; all other entries below are fixed.

| Ref / sprite role | F701 list: tile, facing, action | F702 list: tile, facing, action |
| --- | --- | --- |
| 128 merchant | `(20,23) D S` | `(16,9) D S` |
| 129 girl | `(18,8) D walking1` | `(21,13) D walking1` |
| 130 woman | `(16,23) D S` | `(7,9) D walking1` |
| 131 woman | `(15,25) R S` | `(3,17) D walking1` |
| 132 priest | `(15,24) R S` | `(11,20) D S` |
| 133 merchant | `(18,23) D S` | `(28,2) D S` |
| 134 old man | `(17,23) D S` | `(21,18) D walking1` |
| 135 worker | `(14,26) R S` | `(28,24) U walking1` |
| 136 boy | `(19,24) L S` | `(30,21) R S` |
| 137 boy | `(20,24) L S` | `(12,25) D RL` |
| 138 worker | `(63,63) D S` | `(30,9) L S` |
| 139 guard | `(13,24) U S` | `(18,28) U S` |
| 140 boy | `(20,25) D S` | `(4,18) D walking1` |
| 141 dwarf | `(14,27) R S` | `(24,7) D S` |

All town setups share `ms_map6_InitFunction` ($54E54; retained primary body10,
eight operations). F802 set runs cs_54E70, `setPos132,(11,18),Down`, and returns
without checking F706. With F802 clear/F706 set, cs_54E78 places138 `(63,63)`
Down and139 `(15,23)` Down; both clear returns. These are current lookup writes,
not persistent population changes or HideEntity. They do not run the audience.

Map7 selects default, F701, F702→default, F805 in that order. F805 wins over
both older flags; F702 wins over F701 when F805 is clear. All three pointer
tables share the zone table and **normal** `ms_map7_InitFunction`, not a
separate805 init. Default has12 NPCs;805 has its first11, omitting Astral139.
F701 has five ally declarations before12 NPCs. Complete declarations follow;
`I`=eas_Init, `S`=eas_InitSlow, `RLM`/`LRM`=eas_RightLeftMoveLoop/
eas_LeftRightMoveLoop. Walking home equals position.

| Ref / sprite role | Default (and805 for128–138) | F701 |
| --- | --- | --- |
| Peter7 | none | `(11,6) L I` |
| Sarah1 / Chester2 / Jaha3 / Kazin4 | none | `(12,8) U I` / `(10,8) U I` / `(13,8) U I` / `(9,8) U I` |
| 128 minister | `(12,5) D S` | `(12,5) D I` |
| 129 King Granseal | `(11,4) D S` | `(11,4) D I` |
| 130 guard | `(3,7) D walking2` | `(3,7) D S` |
| 131 guard | `(4,14) D walking1` | `(4,14) D S` |
| 132 guard | `(8,10) D S` | `(8,9) U S` |
| 133 soldier | `(9,20) D RLM` | `(9,20) D S` |
| 134 soldier | `(19,9) D walking1` | `(19,9) D S` |
| 135 soldier | `(14,10) D S` | `(14,9) U S` |
| 136 soldier | `(13,20) D LRM` | `(13,20) D S` |
| 137 maid | `(8,7) R S` | `(8,7) U S` |
| 138 maid | `(14,7) L S` | `(14,7) U S` |
| 139 Astral | `(10,5) D S`; absent in805 | `(10,6) R I` |

InitializeMapEntities clears/rebuilds mappings, allocates flag-selected followers
first, then residents; an already mapped ally skips its fixed declaration.
Default castle therefore needs supplied Peter mapping (normally F67), whereas
F701 can supply a fixed Peter if not already mapped. Main is physical0 at the
admitted tuple. No declaration proves force membership, HP, actual allocation
or an already completed init action.

Shared castle init ($55816; primary body11, seven operations) checks F707.
Set means immediate return, preserving supplied EGRESS_MAP and F411. Clear
means assign EGRESS_MAP=6, run cs_55832, then set707 and411 after return. The
[year-later owner](../eastern-parmecia/new-granseal-castle-year-later-control.md#reload-caller-flags-and-first-control-read)
owns that earlier scene and its explicit default resident reload. Thus missing
F707 can replace even an805-selected population with default residents; F805
does not independently suppress init. F411 is not an immediate battle request.
This slice assumes F707 set for its ordinary return population and does not
silently treat altered history as the same scene entry.

## Complete castle zone admission

**Confirmed:** `s3_zoneevents.asm` `$5579E..$557EE` has exactly five explicit
rows, X9,10,11,12,13 at Y8, all targeting Map7_ZoneEvent0, then the default
Map7_DefaultZoneEvent. The callback is `$557B6..$557EE`,16 operations; its final
shared rts is also the default program `$557EC..$557EE`. No coordinate wildcard
appears in these five rows. Ordinary movement must first submit event6 through
the layout `$1400` zone seam; matching coordinates alone are not invocation.
ProcessMapEventType6_ZoneEvent applies Init to physical0, reads the request's
X/Y words and calls RunMapSetupZoneEvent. This does not validate resident actors.

| Callback inputs | Ordered effect, conditional on scene/service return |
| --- | --- |
| F802 set, F805 clear | cs_55BEE → set805 → set70 → shared rts; no701/702 test |
| F802 set, F805 set | shared rts; no scene and no repair of missing70 |
| F802 clear, F701 set, F702 clear | earlier cs_55A36 → set702 → set67 → default rts;805 is not tested on this branch |
| F802 clear, F701 clear or F702 set | default rts |

F707, F70, F761 and Cannon possession are absent from this callback. In
particular, supplied805 with802 clear does not suppress the older701/702
branch. The earlier Peter audience remains with its accepted owner; it is
not an additional return-scene path. The wrapper reselects the setup's zone
pointer; a flag change does not itself rebuild the current entity cohort.

## Complete return scene and action premises

**Confirmed:** cs_55BEE occupies `$55BEE..$55EF4`, source lines128–319,
134 top-level records ending at csc_end before csub_55EF4. The following uses
one-based command numbers from retained `programCorpus.programs`. M=128,
K=129, A=139, P=Peter7, B=Bowie0. Text IDs are detailed below. A wait suffix
joins only the named actor's action pointer to eas_Idle, not the whole cast.

| Commands / source lines | Ordered program |
| --- | --- |
| 1–17 /128–144 | Cursor1249; P Init/wait; M welcome; setDest word32768 `(11,7)`; P setDest `(10,8)`; camera `(6,2)`; B/P Up; texts1250–1255 with M Left before1256; P1257. |
| 18–35 /145–171 | K/A/M each custom speed48/48, jump idle, ac_end and wait; M Down; stop K/A/M animation; wait60; SFX_BATTLEFIELD_DEATH; K jump, A jump, M jump/wait; repeat those three; start K/A/M animation. |
| 36–47 /172–203 | K custom56/56; K actions D2,L1,D1/wait; P jump/wait; texts1258–1261; K/A/M custom32/32; K U1,R1,U2/wait; K Down. |
| 48–64 /204–229 | P1 palette call; P custom48/48; P U1/wait; B nod; P jump; M nod; P Right; B jump; A nod; P U1 without wait; B headshake; P jump/wait; P R2/wait; P Up; M nod; A jump/wait; B nod. |
| 65–79 /230–253 | Narration1262; P L1,U1/wait; P nod; A Right; M Left; K jump/wait; K nod; B L1,U1/wait; A Down; B nod; B D1,R1/wait; B Up; narration1263–1265. |
| 80–93 /254–271 | P D1,L1,D1 without wait; M Down; waitIdle P; P Up; P0 palette call; wait10; M1266; M Left; texts1267–1269; A Right;1270 and K1271. |
| 94–107 /272–292 | M custom40/40; M L1/wait; M Up;1272; wait50; A Up;1273; M jump/wait; M Left; texts1274–1276; M R1/wait; M Left. |
| 108–124 /293–309 | A1277–1278; M shiver;1279; A Right; narration1280; A Up;1281; K1282; M1283–1284; A Right; A nod;1285; A Down;1286–1287. |
| 125–134 /310–319 | MUSIC_JOIN; narration1288; j_FadeOut_WaitForP1Input; hideText; A1289; B nod; follow P→B direction2; addNewFollower A; setF70; csc_end. |

All nine custom bodies are complete `ac_setSpeed x,x; ac_jump eas_Idle;
ac_end`. The eleven movement bodies above contain20 movement operands and
eleven endActions terminators. Inline bodies are additional bytes, not extra
top-level records. Their exact order includes the unawaited P U1 at command57:
the later jump can replace its action pointer; a universal final position must
not be derived simply by summing all movement operands.

GetEntityAddressFromCharacter masks the selector to a byte, subtracts96 for
NPC references128+, loads ENTITY_INDEX_LIST and multiplies the physical index
by32 (`ENTITYDEF_SIZE_BITS=5`). M/K/A consequently use lookup offsets32/33/43.
P uses7 and B uses0.
The word32768 (`$8000`) at command4 resolves to B0 after masking; bit15 is a
**no-wait control** retained by csc29, not actor32768 or a new entity.
csc29 writes destination tiles*384, travel and signed velocity32 per axis.
P's unflagged word7 also writes these fields but waits current/destination
equality. Neither setDest tests named-ally HP. Camera command6 disables target
following, sets `(6,2)` and waits view scrolling, not all actor movement.

ClearEntities leaves absent ordinary lookup bytes0: absent P or139 can alias
physical0 rather than safely no-op. A hidden actor's lookup can instead be-1;
the resolver has no missing/hidden guard. Duplicate references/physical aliases
can overwrite each other's action, facing, destination, portrait or following.
Resident139 and follower alias31 are independent lookups even when both use
the Astral sprite. All ordinary actor identities above require actual intended
mapping and an active serviceable record; the scene does not validate them.
For example, with only F67 among ordinary follower flags and default residents,
P is physical1, M2, K3 and A13; B31 is absent/zero. Other enabled followers shift
resident allocations. This example is not a required party-size or history rule.

The selected immediate consumers in mapscriptengine_1/2 and entityscriptengine_2
retain these limits:

- Init sets acceleration factors0/0, speed32/32, acceleration/deceleration
  off, normal obstruction/collision/autofacing, Up, immersion off, size24,
  sprite update and idle. InitSlow instead uses speed16/16, factors1/1 and
  acceleration/deceleration `-2,ON`. Custom speed changes only the packed
  X/Y speed bytes (esc10's word copy), not those inherited settings or position.
  csc14/15 seed the action timer with the physical index and optionally poll
  eas_Idle. The ac_end `$8080` marks inline scan end; ac_jump is the executing tail.
- csc2D clears flagsA bits5/6, compiles actions into RAM, appends an idle jump
  and optionally waits that actor's pointer. Relative moves scale tiles by384
  through esc04 and retain collision/destination conflicts. Unawaited actions
  and other actors may continue during text, gestures and service waits.
- eas_Jump disables autofacing, moves relative `(0,-1)`, waits5, `(0,1)`,
  waits5, `(0,0)`, enables autofacing and jumps idle. stop/startEntity writes
  animation counter-1/0; it does not stop/start movement. K/A/M speed changes
  do not grant noclip. The two K sequences cancel only if both execute normally.
- csc23 facing uses a byte actor and skips HP0 named allies below30; thus
  B/P facing can skip while their actions continue. stop/start uses the full
  word actor; NPC128/129/139 bypass the ally guard. Init, custom/inline actions,
  setDest, nod/headshake/shiver and addNewFollower have no such HP guard.
- Nod stops animation, sleeps10, transforms/DMA, sleeps20, updates, sleeps10,
  then counter0. Headshake performs seven paired transform/update cycles with
  six VInt waits each, then counter0. Shiver saves size/animation, uses size21
  and three resize-on/off update pairs with Sleep5 per half, then restores
  size/animation while leaving resize cleared. These are source requests, not
  measured gestures. The wink is narration, not a separate wink opcode.
- ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry; debug/P2 Start can latch
  it. csWait60/10/50 call Sleep with their low-byte operand only while clear.
  These are not measured durations; direct service sleeps remain separate.

## Text identity and service sequence

**Confirmed:** cursor1249 (`$04E1`) advances normally through1289 (`$0509`),
41 sites. `S`=nextSingleText, `T`=nextText. The table gives paraphrased content
and the exact ordered control tokens; it is not a reconstructed transcript.
M/A/P use modifier$0, K uses$C0. Narrator N uses `$FF,ENTITY_NONE` for S and
`$FF,255` for T, both encoded `$FFFF`.

| ID / form / speaker | Content | Ordered controls |
| --- | --- | --- |
| 1249 / S / M | Welcome LEADER | `{LEADER}{W1}` |
| 1250 / S / A | Trading allies | `{N}{W1}` |
| 1251 / T / M | New friends | `{N}{W2}` |
| 1252 / S / M | Praise | `{W1}` |
| 1253 / T / A | Report from Bedoe | `{N}{W2}` |
| 1254 / T / A | Zeon's revival | `{N}{W1}` |
| 1255 / S / A | Volcanon's anger | `{N}{W1}` |
| 1256 / S / M | Elis's fate | `{W1}` |
| 1257 / S / P | Says she lives | `{W1}` |
| 1258 / S / K | Asks whether she lives | `{N}{W1}` |
| 1259 / S / P | Hesitant yes | `{W1}` |
| 1260 / S / A | Asks confirmation | `{N}{W1}` |
| 1261 / S / P | Confirms | `{W1}` |
| 1262 / S / N | LEADER and Peter explain | `{LEADER}{NAME;7}{N}{W1}` |
| 1263 / T / N | Ancient ruins and Creed | `{N}{W2}` |
| 1264 / T / N | Zeon's message through Evil Spilit (source spelling) | `{N}{W2}` |
| 1265 / S / N | Explanation complete | `{W1}` |
| 1266 / S / M | Amazement at LEADER/Peter | `{N}{LEADER}{NAME;7}{W1}` |
| 1267 / S / M | Their experiences | `{N}{W1}` |
| 1268 / S / A | Ponders | `{W1}` |
| 1269 / S / M | Addresses Astral | `{W1}` |
| 1270 / S / A | Asks next action | `{W1}` |
| 1271 / S / K | Defeat Zeon and save Elis | `{N}{W1}` |
| 1272 / S / M | Objects | `{W1}` |
| 1273 / S / A | Agrees with King | `{W1}` |
| 1274 / S / M | Surprise | `{W1}` |
| 1275 / T / K | Thanks Astral | `{W2}` |
| 1276 / S / K | Asks first step | `{W1}` |
| 1277 / T / A | North Parmecia | `{N}{W2}` |
| 1278 / S / A | Tristan storytellers | `{N}{W1}` |
| 1279 / S / M | Exclamation | `{W1}` |
| 1280 / S / N | Astral winks and smiles | `{N}{W1}` |
| 1281 / S / A | Requests to accompany LEADER | `{N}{N}{LEADER}{W1}` |
| 1282 / S / K | Consents and asks for Elis | `{N}{W1}` |
| 1283 / T / M | Caution | `{W2}` |
| 1284 / S / M | Danger | `{W1}` |
| 1285 / S / A | Assent | `{W1}` |
| 1286 / T / A | Addresses LEADER | `{LEADER}{W2}` |
| 1287 / S / A | Accompanies as adviser | `{N}{W1}` |
| 1288 / T / N | Adviser joins force announcement | `{N}`; no W1/W2 |
| 1289 / S / A | Rocks, northern passage and an explosive | `{N}{N}{W2}{N}{N}{W1}` |

All41 source comments match the exact four-hex-digit `gamescript.txt` entries;
IDs/forms/modifiers/speakers and controls were joined separately. LEADER uses
UpdateForce/first active name, not necessarily fixed actor0. NAME7 is the
current name for Peter; it does not establish that7 is present or alive.
Speech/portrait resolution reads the mapped entity's current mapsprite.
Minister205 has no portrait/bleep5; King203 portrait32/bleep6; Astral209
portrait31/bleep6; Peter base/promoted uses the corresponding portrait and
bleep4/5. Changed class/sprite/alias changes those results. K's modifier$C0
sets both portrait option bits15/14; it does not change actor129. If a portrait
window is already open, csc1D returns before opening another. Narrator$FFFF
sets speech0 and bypasses portrait resolution; it is not lookup255.

csc00/02 wait view scrolling, display and increment the cursor. S additionally
closes portrait/text and sleeps10; T leaves windows open. Normal final cursor
is1290. The debug skip latch instead consumes each text operand without display
or increment, so later displayed text need not retain the nominal site-to-ID
mapping. N is newline; W1/W2 wait input, with W2 continuation/validation-sound
behavior. Internal W2 in single1289 and terminal W1 in non-single1254 remain
meaningful. There is no yes/no opcode or player decision in this scene.

Command125 submits MUSIC_JOIN=19; command126's announcement has no input-wait
token. Command127 calls the pinned jump alias j_FadeOut_WaitForP1Input, whose
effective target in `battlefunctions_0.asm` preserves d0, requests previous
music `$FB` via PlayMusicAfterCurrentOne, calls WaitForPlayerInput, and returns.
`tech/sound/music.asm` submits WAIT_MUSIC_END then the requested command and
polls WAIT_FOR_MUSIC_END with Sleep3 until clear. This is the immediate command
service boundary, not proof the jingle ended audibly. `tech/input.asm` accepts
any currently held direction/A/B/C/Start in CURRENT_PLAYER_INPUT, otherwise
waits VInt and retries; it does not demand a fresh edge or A/C specifically.
Despite the helper name, no palette fade is executed there. Only after return
does hideText close windows, then1289 displays, B nods and following is changed.
Skipped narration does not bypass this direct subroutine/input call.

## Palette remap and restore

**Confirmed:** command48 calls csub_55EF4 (P1), and84 calls sub_55F82 (P0),
with the retrospective actions/text between them and csWait10 after P0.
These are the exact shared helpers already owned by the
[consultation palette analysis](../southern-travel-creed/evil-spirit-consultation-and-zone-return.md#special-graphics-palettes-and-presentation-services).
No additional P1/P0 call or palette load occurs in this scene.

P1 traverses64 CURRENT words ($FFD000), writing BASE ($FFD080). It computes
`s=((c & $E00)>>9)+(c & $E)+((c & $E0)>>3)` and `q=floor(s/7)` plus1 if the
remainder is at least4. Its table bytes are T0=`14,12,10,8,8,6,4,2`,
T1=`14,12,12,10,8,6,4,2`, T2=`0,2,2,4,6,8,10,10`; the output is
`(T2[q]<<8)|(T0[7-q]<<4)|T1[7-q]`. This names a color remap, not measured
grayscale/sepia presentation. It clears FADING_TIMER_BYTE and tail-calls
UpdateBasePalettesAndBackupCurrent with BASE. P0 clears the same byte and
passes COPY ($FFDF2A).

The consumer copies128 bytes from the supplied source to BASE, then CURRENT
to COPY, and writes timer32. Thus P0 restores the selected COPY snapshot to
BASE **before replacing COPY** with then-current colors; it does not reload
an immutable room palette. UpdateFadingPalette decrements the timer, takes
`t=timer>>2`, interpolates each component `(COPY*t+BASE*(8-t))>>3`, writes64
CURRENT words and requests VInt CRAM DMA. The cleared timer byte suppresses
an automatic reverse transition. Neither helper waits for the32 updates to
finish. Script pauses and later text do not prove rendered completion or a
hardware-exact appearance; interruption and other palette services can matter.

## Following, caller return and changed state

**Confirmed:** command131's csc2C reads the follower word's **high byte** for
the HP guard. Peter word`$0007` therefore checks Bowie's HP0 before resolving
P/B, not Peter's HP. If admitted, direction2 supplies `(-24,0)`, and AddFollower
copies42 bytes of eas_Follower1 into action RAM, installs the physical leader/
offsets and appends the physical follower if absent from EXPLORATION_ENTITIES.
Already-listed P is not reordered. This changes no joined/active/AI flag or F67.

Command132's csc56 resolves current139 and scans EXPLORATION_ENTITIES to its
terminator, selecting its **actual last physical entry**, or0 for an empty
list, then AddFollower with `(-24,0)`. It has no HP guard and no forced Peter
target. If P was newly appended, it is last; if already present with other
followers after it, A follows that other last entry. Repeating the script with
A already last can install A following itself; duplicate suppression occurs
only after action allocation/installation and does not validate the leader.
No bounded-capacity/list-validity check is supplied by these callers.

Current A remains the resident139 mapping and sprite; no rename to B31 or
fresh entity reload occurs. Command133 sets F70 **inside** the script, then
csc_end conditionally waits scrolling if a dialogue window is open, clears
VIEW_SCROLLING_SPEED and restores registers. Only after script return does
Map7_ZoneEvent0 set805 then70 again. RunMapSetupZoneEvent then closes portrait/
text, waits one VInt, and calls WaitForEntityToStopMoving with **physical0**,
before restoring registers and returning. It does not join all followers or
prove the next control read. No save transaction makes these stages atomic.

On a later ordinary rebuild, F70 selects the `table_Followers` row
`70,FOLLOWER_B,MAPSPRITE_ASTRAL,1`, alias31, before residents. Its allocated
physical index/action depends on all earlier enabled rows. The complete
declaration order is `66 Sarah,67 Peter,72 Kazin,73 Slade,74 B/Astral,66 Chester,
68 B/Oddler,75 Luke,69 C/Fairy,70 B/Astral,74 A/King,71 Lemon,74 D/Minister,
76 Zynk,77 A/OldMan,82 Frayja,83 A/King,83 D/Minister,84 Rohde`; Caravan65/A
precedes this only for the overworld selection. Multiple B declarations are
not deduplicated: later70 overwrites the lookup after earlier74/68 allocations,
which can remain in the follower list. Map7 uses the ordinary table, not the
Caravan overworld prefix. F805-selected residents omit139 independently of70.

| Changed premise | Consequence at this boundary |
| --- | --- |
| F805 set, F70 clear, F707 set on fresh entry | Eleven castle residents, no139 or70-derived B;802-set zone returns without repairing70. Other follower flags remain independent. |
| F70 set, F805 clear on fresh entry | B/Astral follower plus default/F701 resident139; eligible802 scene still runs and may append resident139 as another Astral. |
| F707 clear, including805 set | Earlier shared init runs and explicitly reloads default residents; preserve that different cohort/history rather than assuming805's list survived. |
| F701 only, F802 set | Return scene has priority despite older audience not being completed; fixed actor positions/settings differ from default. |
| F802 clear | No return/adviser scene through this callback, regardless of Cannon/F761; older701/702 path remains possible. |
| Bowie HP0 | B-facing and P→B follow can skip, while custom/inline actions, gestures, A addition and both later70 writes can still run; P HP0 separately skips its facing, not this high-byte follow guard. |
| Absent, hidden or aliased M/K/A/P | Actual lookup/service effects replace nominal identities; no safe absence handling or automatic reconstruction. |
| Interrupt before133 / after133 / after caller805 | Respectively no new70 yet /70 may be set while805 remains clear /805 can be set before the caller's repeated70 and wrapper finish; no rollback or atomic completed-scene fact. |
| Direct cs_55BEE call or forced repeat | No802/805 test inside the script, no805 write there; actions/text/list allocation may repeat. Normal802+805 zone reentry skips it. |

The narrator's retrospective does not re-execute Creed/ruins/Zeon scenes or
prove prior natural traversal. Elis being alive is dialogue, not a rescue
mutation. The adviser announcement does not call joinForce/joinForceAi,
joinBattleParty, a naming service or a party-capacity test. No item transfer,
warp, battle request, F761/F804/F806 write or yes/no occurs in cs_55BEE.

Next boundaries are identified only: Map6 zone rows `(19,29)` and `(20,29)`
call Map6_ZoneEvent3, which requires805 set/806 clear before the Janet scene
and sets806 after it returns. The town inventor is141 in the later lists;
Map6_EntityEvent13 is defined in `s2_entityevents_701.asm` and reused by the702
event table. It first gives802 priority over earlier702 dialogue, then checks
804 before805; on802 set/804 clear/805 set it searches actual ITEM_CANNON
inventory via j_GetItemInventoryLocation. These are distinct later admission
and item conditions, not prerequisites for Astral's audience. Their scene,
gift transaction and the North Cave blast remain outside this owner.

**Unknown:** natural route/zone reach; actual flags, mode, HP, class, population,
lookups and follower order; completed collision/action/text/portrait/input/
camera/palette/DMA/audio services; visible retrospective/adviser presentation,
usable control, and save/load persistence. These are retained limits, not an
automatic runtime or acquisition queue.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same PowerShell process as each
controlled command. Reuse the [retained pair recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture`, and `_cutscene_facts`.
Keep both payloads and their completed comparison receipts intact.

1. Join the four selected warp rows, map-substitution absence, Map68's Battle19
   admission and the other selected maps' battle-row absence,
   scroll/no-scroll consumers, complete selected population declarations and
   setup pointers. Select Map6/7/68/73 `routeJoins` and `primarySourceBodies`
   from retained map-init; join source operations in exact order. Map73's
   alternate fresh init is a boundary, not executed by the scroll route.
2. Select Map7's complete zone records from
   `tests/fixtures/h2/map-events/routing-setup.json`, and both target contexts
   from `zone-programs.json`; join exact line/opcode/operand order, including
   the shared default rts. Use the entity fixture only for the inventor's
   identified prefix; do not expand its transaction into this slice.
3. Select cs_55BEE from retained
   `local/derived/map-script-engine-static.json:programCorpus.programs`.
   Join all134 top-level records, all nine custom bodies and eleven movement
   bodies through their terminators. Check the encoded interval independently
   using macro widths; join all41 literal/ID/form/modifier/speaker/control sites
   to gamescript.txt, and verify the exact three subroutine targets/consumers.
4. Review actual actor resolution, byte/word/high-byte HP guards, Init/Jump,
   movement/gesture/camera services, palette calls, music/input alias, current
   following, ordered flag writes, fresh follower declarations and zone wrapper.
   Changed-state cases above are part of the review, not optional happy-path prose.
5. Check the complete document/diff, local links/anchors, UTF-8, sole-path scope
   and private/generated boundary; run `git diff --check`. On a clean committed
   head run `uv run sf2 verify plan --base origin/main --head HEAD` and record
   actual exact-head CI in the Issue handoff. No blanket local normal/full/
   engine/Godot/H3/H4 suite follows solely from this documentation change.

Preserve [#961 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179316611),
[rebase handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961#issuecomment-6179363538)
and [PR964 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/964#issuecomment-6179370463),
including the unchanged pair's0.081s receipt, encoded-size correction and
source/shape/truncation diagnostics. Their linked #957/#953/#949/#945/#941/#937
results, failures and Unknowns remain intact, as does
[#954's callback correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171).
Retain [#962's Fairy correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179318374)
and [PR963 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/963#issuecomment-6179327445).
No completed failure is reset or reclassified as interrupted. Freeze a clean
pushed Draft PR for independent review; no merge, cleanup, archive or new Issue.
