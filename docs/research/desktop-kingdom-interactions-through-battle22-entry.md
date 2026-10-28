# Desktop Kingdom conversations through Battle22 entry

Evidence date: 2026-10-14. **Confirmed:** King Ponpei's prebattle conversation
sets F777 only after its final permission text. The sole Map22 zone checks
F777 set and F522 clear, writes EGRESS_MAP=22, and runs all eight commands of
`cs_5994E`. Conditional completion sets F422 and requests `(22,0,0,Right)`;
the surviving request can admit Battle22. Guard136's permission dialogue uses
F212/F258 independently of F777. **Inferred:** these conversations frame the
chess match as the King's condition for leaving. **Unknown:** natural approach,
supplied state, service completion, visible performance and usable control.

## Agreement and provenance

Standard rigor under [Issue #904](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
Complete callback, speaker, scene and admission joins are the uncertainty;
this one English research document supports later story design. Starting
accepted base is `e120a9b6890527eb018e081f1c5b1907c21155bc`, without stacking.
No shared index, registry, contract, schema, fixture, code or remake change.

Begin at the accepted [first-arrival conditional return](./desktop-kingdom-first-arrival.md),
with F522 clear and the actual selected actor mapping. F777 may be clear or
set; F776 is not an ordinary conversation gate. Reuse the accepted
[magic-door admission](./creed-magic-door-and-desktop-transfer.md),
[interaction-state owner](./map-event-interaction-state.md),
[request-consumption owner](./map-event-request-consumption.md) and
[common-menu services](./common-menus.md). Stop at the conditional local
return/bypass or `BattleLoop` admission, before opening, roster, combat,
postbattle, Cotton Balloon acquisition/use or Floor World. No runtime
acquisition, general menu/renderer/terrain audit or new verification machinery.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only checkout is clean at that pin. Retained evidence binds USA ROM
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This is static source/retained-evidence review, not a new extraction or
playthrough. Short setup filenames mean `data/maps/entries/map22/mapsetups/`;
map-script consumers mean `code/common/scripting/map/`. Fnnn names flag nnn;
command indices are zero-based. Coordinates and waits are source operands.

The unchanged map-init/map-script-engine pair totals 18,285,979 bytes. One
linear comparison, bounded source/fixture reads and small prose/receipts are
budgeted below 512 MiB and one minute for comparisons. The pair and existing
cutscene-source comparison passed in 0.080 seconds. No new payload/environment
or capture is needed; reassess before material resource or scope expansion.

## Actual activation, event selection and return

**Confirmed:** `data/maps/mapsetups.asm` selects default `ms_map22`, or
`ms_map22_flag522` under F522. Both pointer tables share EntityEvents,
ZoneEvents, AreaDescriptions, Section5 and InitFunction; only population
differs. `GetCurrentMapSetup` resolves supplied flags again at each accessor.
F776 neither selects these event tables nor appears in the selected callbacks.
The first-arrival owner supplies the complete baseline population/allocation.

`code/gameflow/exploration/explorationvints.asm:ProcessPlayerAction` first
idles physical 0 and waits for its movement and view scrolling. A takes the
field-menu path. C can reach `GetActivatedEntity` after the debug and
physical-1 overlapping-Caravan alternatives; NPC128's local service callback
is distinct from that Caravan shortcut. In ordinary camera-target-0 state,
`explorationfunctions_0.asm:GetActivatedEntity` offsets the candidate using
the player's facing, traverses desk/counter blocks (`$2800` after `$3C00`
masking), then selects the first eligible physical entry within Manhattan
distance less than MAP_TILE_SIZE=384. It skips the viewed entity and current
followers. It returns the actor's saved facing in d1 and player facing in d2.
A negative view target returns without selecting an actor. This is a caller
relation, not a verified walking itinerary or a claim about arbitrary view state.

`code/gameflow/battle/battlefunctions/battlefunctions_0.asm:GetEntityEventIndex`
reverse-maps the physical actor through ENTITY_INDEX_LIST, changing the logical
counter to 128 after the ally segment. No match falls back to logical 0.
`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` stores d2 in
EVENT_RELATIVE_POSITION, finds the first matching four-byte event row, or uses
the `$FD` default. It obtains portrait/speech from the mapped entity, then
resolves the logical identity back to a physical actor for facing updates.
These are lookup identities, not battle-roster positions.

The second `msEntityEvent` operand is an action bitfield, despite the macro's
facing comment. RIGHT=0 does neither facing operation. UP=1 turns the actor
to `(playerFacing+2)&3` after a VInt without the bit-1 restoration. DOWN=3
turns then restores its saved facing after the callback. These are not required
approach directions or the actor's initial facing. The wrapper clears the
entity-update VInt flag before calling the callback; after a returning callback
it conditionally restores facing, closes portrait/text and reactivates entity
updates. It does not roll back service mutations or prove that services returned.

### Complete event table and selected handlers

**Confirmed:** `s2_entityevents.asm:5–32` contains 27 explicit rows and one
default, selecting 17 distinct named handlers plus the default. Event N below
means `Map22_EntityEventN`. Physical P is only the predecessor's example with
F67/F68 followers, no preexisting Eric/Karna alias and no raft overwrite.
Changing those premises changes physical indices; re-resolve the actual mapping.

| Logical actor; baseline sprite | Example P | Row byte / handler | Selected prebattle work, in order |
| --- | --- | --- | --- |
| Eric14; ally sprite lookup | 3 | UP 1 / 0 | 1772, 1773, return |
| Karna15; ally sprite lookup | 4 | UP 1 / 1 | 1774, 1775, 1776, return |
| NPC128; CARAVAN62 | 5 | RIGHT 0 / 2 | `j_CaravanMenu`, then return |
| NPC129; YOUNG_MAN196 | 6 | DOWN 3 / 3 | 1766, 1767, return |
| NPC130; YOUNG_WOMAN197 | 7 | DOWN 3 / 4 | 1768, 1769, return |
| NPC131; OLD_MAN198 | 8 | UP 1 / 5 | 1770, 1771, return |
| NPC132; PRIEST192 | 9 | UP 1 / 6 | `j_ChurchMenu`, then return |
| NPC133; MAID208 | 10 | UP 1 / 7 | F522 clear: 1777, return |
| NPC134; KING_PONPEI188 | 11 | DOWN 3 / 8 | F522/F777 branches below |
| NPC135; GUARD206 | 12 | DOWN 3 / 9 | F777 clear: 1783, 1784; set: 1782; return |
| NPC136; GUARD206 | 13 | DOWN 3 / 10 | F212 clear: 1802, return; excluded set arm below |
| NPC137; KING153 | 14 | RIGHT 0 / 11 | 1790, 1791, return |
| NPC138; QUEEN135 | 15 | RIGHT 0 / 12 | 1789, return |
| NPC139,140; BISHOP144 | 16,17 | RIGHT 0 / 13 | 1788, return |
| NPC141,142; KNIGHT78 | 18,19 | RIGHT 0 / 15 | 1787, return |
| NPC143,144; ROOK125 | 20,21 | RIGHT 0 / 17 | 1785, return |
| NPC145,146; QUEEN135 | 22,23 | RIGHT 0 / 12 | 1789, return |
| NPC147–152; PAWN77 | 24–29 | RIGHT 0 / 21 | 1786, return |
| Unmatched logical ID | Mapping-dependent | Default 0 | `Map22_DefaultEntityEvent`: return only |

The table covers every explicit row, including all three Queen rows sharing Event12,
two Bishops, two Knights, two Rooks and six Pawns. ORB153–155 have no explicit
event row. Eric/Karna callbacks contain text and return only: neither recruits,
changes party membership nor proves an earlier conversation occurred. Except
for the King permission arm below, selected text-only arms leave story/request
state unwritten. Wrapper facing, portrait, speech, event-relative-position and
update state still change.

If no actor is activated, the ordinary caller instead invokes CheckArea and
may fall through to FieldMenu. `s4_descriptions.asm:ms_map22_AreaDescriptions`
is only `rts`; the area wrapper starts d7=0, so this callback contributes no
handled description. This is not an audit of all search rewards. Section5's
item table points to a separate future exit: its item handler and Cotton
Balloon use are outside this outcome.

### Speaker, portrait and bounded service handoffs

**Confirmed:** `GetEntityPortaitAndSpeechSfx` in
`code/common/scripting/entity/getentityportaitandspeechsfx.asm` resolves the
actual entity, reads its MAPSPRITE byte and searches the complete
`data/spritedialogproperties.asm` table. `GetEntityAddressFromCharacter`
in mapscriptengine_1 uses ally lookup slots directly; logical 128+ subtracts
ENTITY_ENEMY_INDEX_DIFFERENCE=96. A zero lookup resolves physical 0, without
a separate absent-actor rejection. Aliases and supplied mappings therefore
matter to both facing and speaker identity.

| Actual sprite in the baseline mapping | Portrait result; speech request |
| --- | --- |
| Eric base/promoted/special (33/34/35) | Eric14; bleep5 |
| Karna base/promoted/special (36/37/38) | Karna15; bleep5 |
| PRIEST192 | Priest34; bleep5 |
| YOUNG_MAN196 / YOUNG_WOMAN197 / OLD_MAN198 | NONE; bleep2 / bleep3 / bleep6 |
| MAID208 / KING_PONPEI188 / GUARD206 | NONE; bleep3 / bleep6 / bleep5 |
| CARAVAN62 and selected KING/QUEEN/BISHOP/KNIGHT/ROOK/PAWN sprites | No matching table entry: DEFAULT; bleep6 |

NONE=255 is sign-extended to -1; DEFAULT=$FFFF is also -1. Neither requests a
new portrait window. Do not substitute the KING_GRANSEAL portrait for Ponpei
or the chess KING. The prior scene's narrative Goliath attribution does not
change NPC128's Caravan sprite or these local service calls. Eric/Karna's
ordinary class variants share their respective portrait. `GetAllyMapsprite`
selects BLUE_FLAME for zero HP during exploration; class and alias changes can
select another actual sprite. The conversation/portrait handlers have no
separate ally-alive skip, so a changed sprite is not proof the callback is absent.

Event2 directly calls `j_CaravanMenu` (`s05_jumpinterface.asm`, `$20010` →
`CaravanMenu` `$21FD2`, `code/common/menus/caravan/caravanactions_1.asm`).
There is no local F65, joined-member, vehicle-mode or movement prerequisite
inside this callback. CaravanMenu saves registers/allocates its local frame,
delivers its opening message, and enters MENU_CARAVAN. Its -1 selection takes
ExitCaravan, delivers the exit message, releases the frame/restores registers
and returns; non-exit actions return to the menu loop. Its immediate
`caravanactions_2.asm:DisplayCaravanMessageWithPortrait` selects hardcoded
Rohde portrait 11 when F70 is clear or Astral 31 when set. Thus the service's
portrait need not be NPC128's dispatcher fallback. Do not import the separate
C-button shortcut's shrink/music/view-update work into this direct callback.

Event6 directly calls `j_ChurchMenu` (`$20004` → `ChurchMenu` `$20A02`,
`code/common/menus/church/churchactions_1.asm`). It uses CURRENT_PORTRAIT for
its conditional opening, displays its welcome, closes windows and enters
MENU_CHURCH. Selection -1 takes ExitMenu: conditional portrait, farewell,
window closure, local-frame/register restoration, return. Other selections
belong to the existing common-menu owner. The Map22 callback then returns to
the shared entity wrapper only if the selected service returns. No new local
EGRESS/savepoint write, completed transaction, save, healing, promotion,
storage/party result or post-service control is established by these call sites.

## Permission and independent guard flags

**Confirmed:** `CheckFlag` in `code/common/stats/gameflags.asm` returns zero
for clear and nonzero for set; SetFlag ORs the selected bit. Event8 first tests
F522. Its set arm selects postbattle text 1797 and returns without reaching
the F777 write; that arm is excluded content, not a second permission grant.
With F522 clear it tests F777:

| F777 at King callback | Text order | Subsequent mutation |
| --- | --- | --- |
| Clear | 1778, 1779, 1780, then shared 1781 | Set F777 only after 1781 returns; return |
| Set | 1781 only | Set F777 again after that text returns; return |

Talking alone writes neither F422 nor a warp/request. No Yes/No prompt,
forced combat, recruitment, reward or F522 completion occurs in this callback.
If text never returns, the later flag write is not established. The repeated
conversation omits the introduction, but is not evidence that an earlier
natural conversation succeeded: F777 can be supplied independently.

Event9 (guard135) checks only F777: clear selects 1783–1784; set selects 1782.
Its statement about reaching the board's center is dialogue. The actual
trigger below is a wildcard-X zone at Y16, followed by a separate battle check;
the text does not define a universal center/rectangle admission rule.

Event10 (guard136) checks F212, not F777. Clear selects 1802 and returns
regardless of F258. The identified excluded F212-set continuation selects
1803, then checks F258: set returns, clear calls `cs_599A4` and sets F258
only after its return. This document does not inspect that script or infer
Cotton Balloon custody/use from the flag. Even F777 set leaves this guard's
1802 line selected when F212 is clear. F212/F258 do not gate the chessboard zone.

Event7's excluded F522-set arm checks F256: clear selects 1798–1799 then sets
F256; set selects 1800–1801. These and King 1797 are postbattle alternatives,
not part of the selected prebattle literal set. The init owner's F522/F778
continuation to `cs_5996E` is likewise excluded. No postbattle script is needed
to explain the prebattle branch polarity.

### Exact selected text and controls

**Confirmed:** all selected callback IDs are exactly 1766–1791 plus 1802
(27 distinct literals); the encounter adds 1792–1793. This set was derived
from the complete selected arms, not assumed from a contiguous range.
`data/scripting/text/gamescript.txt` keys 06E6–0701 and 070A match the source
comments below. The original wording of 1788 is retained. Plain `txt` expands
to the TEXTBOX trap plus its ID; it does not select a new named speaker.
The mapped actor/dispatcher above supplies that state. No selected literal
contains a dynamic name substitution. `{N}`, `{W1}`, `{W2}` remain explicit
text controls, not measured waits or proven button advances.

| ID | Actor / selected arm | Exact literal |
| --- | --- | --- |
| 1766 | NPC129 | `Creed is a fine gentleman.{W2}` |
| 1767 | NPC129 | `His magic saved us a long time{N}ago during a natural disaster.{W1}` |
| 1768 | NPC130 | `Devils cannot use their magic{N}to affect human lives.{W2}` |
| 1769 | NPC130 | `Supposedly, it's one of their{N}rules.{W1}` |
| 1770 | NPC131 | `This Kingdom was made by{N}Creed.  There's no concept{N}of TIME here.{W2}` |
| 1771 | NPC131 | `We never get old.{N}We never die.{N}What is the meaning of life?{W1}` |
| 1772 | Eric14 | `I was there during the{N}disaster.{W2}` |
| 1773 | Eric14 | `I was looking for the{N}legendary sword.{N}I hate it here!{W1}` |
| 1774 | Karna15 | `Are you from the outside?{N}Why did you come here?{W2}` |
| 1775 | Karna15 | `Ah, a mistake.  I see.{W2}` |
| 1776 | Karna15 | `I suggest you see King{N}Ponpei.{N}He's a good man.{W1}` |
| 1777 | Maid133; F522 clear | `I'm sure King Ponpei will{N}help you.{W1}` |
| 1778 | Ponpei134; first permission | `So, you were not able to see{N}Creed?{W2}` |
| 1779 | Ponpei134; first permission | `Somewhere in Floor World,{N}there's a tunnel that leads{N}to Creed.{W2}` |
| 1780 | Ponpei134; first permission | `OK, I'll let you go if you{N}win a game against my army.{W2}` |
| 1781 | Ponpei134; both prebattle arms | `You may go to the chessboard{N}now.{W1}` |
| 1782 | Guard135; F777 set | `The army won't attack until{N}you reach the center of the{N}board.{W1}` |
| 1783 | Guard135; F777 clear | `The Chess Army is very{N}strong.{W2}` |
| 1784 | Guard135; F777 clear | `It will be good training for{N}you.{W1}` |
| 1785 | Rooks143,144 | `Mr. Creed gave me this solid,{N}armored body!{W1}` |
| 1786 | Pawns147–152 | `My attack power is the best{N}in the army!{W1}` |
| 1787 | Knights141,142 | `We knights are always ready{N}to fight!{W1}` |
| 1788 | Bishops139,140 | `Is healing is my only ability?{N}Ha, ha!  You have a lot to{N}learn!{W1}` |
| 1789 | Queens138,145,146 | `I lead the army with the King.{W1}` |
| 1790 | Chess King137 | `The Chess Army of Desktop{N}Kingdom is a well balanced{N}force!{W2}` |
| 1791 | Chess King137 | `If it is King Ponpei's wish,{N}we will accept your{N}challenge!{W1}` |
| 1792 | Ponpei134; nextText $0 | `Are you ready?{W2}` |
| 1793 | Ponpei134; nextSingleText $0 | `Now, start the battle!{N}Do your best!{W1}` |
| 1802 | Guard136; F212 clear | `Halt!  I can't let you{N}pass without the King's{N}permission.{W1}` |

**Inferred:** villagers' accounts explain the kingdom and the chess pieces'
boasts characterize the challenge. Their claims about time, death, devil
magic, armor, attack strength or healing are dialogue, not verified universal
mechanics, stats, immortality or combat capability contracts.

## Zone request and the complete encounter scene

**Confirmed:** `s3_zoneevents.asm` contains only `(255,16)` → Map22_ZoneEvent0,
then a default `rts`. In `RunMapSetupZoneEvent`, byte 255 is an X wildcard;
Y must equal 16. This means any supplied X at Y16 matches this row, not that
every physical tile on that row can naturally dispatch it. The ordinary
`code/common/scripting/entity/entityscriptengine_2.asm:esc02_controlCharacter`
path at loc_5294 recognizes block type `$1400`, writes MAP_EVENT_TYPE=6,
and derives X/Y word parameters from destination plus attempted offsets,
divided by MAP_TILE_SIZE. Layout, motion and actual reach remain conditions.

WaitForEvent prioritizes pending events over A/C. ProcessMapEvent clears the
type before dispatching type 6; `ProcessMapEventType6_ZoneEvent` applies the
init actscript to physical 0, reads X/Y words from PARAM1/PARAM3 and calls the
zone wrapper. No direct player-facing or F776 gate is added by the zone row.

Map22_ZoneEvent0 tests F777 first: clear returns. It then tests F522: set
returns. Only F777 set/F522 clear writes the byte EGRESS_MAP=22, then calls
`cs_5994E`. That operation writes only EGRESS_MAP: it supplies no egress
X/Y/facing tuple and leaves the map-event parameters untouched. The later warp
supplies its own destination coordinates/facing. This does not complete an
egress, service or savepoint transaction; an unfinished scene can leave the
map byte already changed.

The complete `scripts.asm:4–11` program is 32 encoded bytes, eight commands:

| Index | Exact source command | Immediate consumer and conditional effect |
| --- | --- | --- |
| 0 | `textCursor 1792` | csc04 writes CUTSCENE_DIALOG_INDEX=1792. |
| 1 | `csWait 50` | ExecuteMapScript consumes the negative wait word and calls Sleep with its low byte 50 when text skipping is off. |
| 2 | `setCamDest 10,0` | csc32 writes VIEW_TARGET_ENTITY=-1, requests camera destination and waits for view scrolling to finish. |
| 3 | `nextText $0,134` | csc02 resolves actual NPC134 portrait/speech, waits for scrolling, displays 1792 and advances the cursor. |
| 4 | `nextSingleText $0,134` | csc00 displays 1793 and advances the cursor, closes portrait/text, then sleeps 10. |
| 5 | `setStoryFlag 22` | csc13 adds BATTLE_UNLOCKED_FLAGS_START=400 and calls SetFlag: F422, not F22 or F522. |
| 6 | `warp MAP_DESKTOP_KINGDOM,0,0,RIGHT` | csc07 writes pending type 1 and bytes `0,22,0,0,0` for PARAM1–5. |
| 7 | `csc_end` | Terminates the script; conditional scroll wait, clears VIEW_SCROLLING_SPEED, restores caller registers. |

`sf2cutscenemacros.asm` defines these encodings. SetCameraDestination in
battlefunctions_0 multiplies the tile operands by 384 and calls
SetViewDestination; `(10,0)` denotes the camera's requested border, not actor
placement. `code/common/maps/camerafunctions.asm:WaitForViewScrollEnd` polls
scroll state with VInt waits/rechecks, without a timeout. The text handlers
also call that wait; csc1D opens no new portrait for baseline KING_PONPEI's
NONE result. Modifier $0 sets neither right-side nor mirrored-portrait bit;
an already open portrait is not reopened. Both lines request bleep6 for the
baseline actor. The script does not turn/move NPC134 or recruit anyone.

There is no csc11 Yes/No command or conditional answer branch, despite 1792's
question. In ordinary non-skipping execution, later F422/warp require prior
wait/camera/text handlers to return. ExecuteMapScript's debug P2-Start text
skip path can skip the encoded wait and these two text calls; it does not
skip the camera, flag or warp commands. No source wait establishes measured
presentation duration or successful service completion.

After csc_end, the callback returns. The zone wrapper closes portrait/text,
waits one VInt and waits for physical 0's current X/Y to equal its destination.
This is not an all-entity/action barrier. It runs after the pending warp has
been written; failure to finish the wrapper can therefore leave that request
unconsumed. No rollback or atomic multi-write transaction is supplied.

## Pending warp, Battle22 admission and stop

**Confirmed:** on a surviving warp, WaitForEvent sees the pending type before
reinstalling camera-target0/current-mode control. ProcessMapEvent clears the
type, then takes type 1's no-scroll path because PARAM1=0. It consumes/clears
WARP_SFX, idles physical 0 and unwinds exploration to MainLoop. The current
physical 0 tile position is initially read by UpdatePlayerPosFromMapEvent;
explicit nonnegative PARAM3/PARAM4 override it with `(0,0)`, and PARAM5 gives
Right=0. The camera destination `(10,0)` is not the player's warp destination.
No useful local control between these operations is promised.

MainLoop calls SwitchMap then CheckBattle, as in the accepted magic-door
owner. The complete flag-switched-map table has no Map22 entry. The sole
Map22 battle row is zero-based index 22 in
`data/battles/global/battlemapcoords.asm`, tuple `22,9,0,30,30,255,255`.
`code/common/maps/getbattle.asm:CheckBattle` checks map, then F422, then
trigger X/Y (both 255 wildcards). It does not test the player against the area
rectangle. On selection it writes area `(9,0,30,30)`, then checks F522. F522
set clears F422 but does not revoke this invocation's selected index 22.

| State at the actual CheckBattle call | Result |
| --- | --- |
| Map22, F422 set, F522 clear; ordinary surviving scene result | Wildcard triggers match even at `(0,0)` outside the area. Write area, retain F422, return 22; MainLoop passes 22 to BattleLoop. |
| Map22, F422 cleared independently before consumption | Skip the row before area/completion check; return -1 and enter fresh explicit-Map22 exploration. This does not prove its later population/init/control completed. |
| Map22, F422 set, F522 set independently after zone admission | Write area, clear F422, still return 22 and admit BattleLoop this time. A later check depends on later supplied state. |
| Pending request replaced, incomplete, or destination changed | Consume actual surviving request fields; the named Map22/Battle22 result is not guaranteed. |

The zone's F777-clear/F522-set bypass writes neither EGRESS_MAP nor F422 nor
warp. After wrapper return, zero pending type allows WaitForEvent to target
physical 0 and install current-mode control; a pending replacement takes
priority. This local bypass itself does not call CheckBattle, even if F422
was supplied set. Conversely CheckBattle does not consult F777/F776/F212/F258:
those flags cannot override an independently admitted Map22 request with F422.

**The selected endpoint is entry to BattleLoop before opening/roster, or the
conditional bypass/local return above.** No F522 write, Battle22 completion,
postbattle grant, item acquisition or Floor World transfer is established.

## Counterexamples and reproduction boundary

| Changed premise | Source discriminator and limit |
| --- | --- |
| F776 clear/set independently | Does not gate these conversations or this zone. It controls the predecessor's init scene; no natural arrival completion follows from a supplied bit. |
| F777 clear/set, F522 clear | Changes King introduction and guard135 text; only set admits the zone scene. Repeated King talk still writes F777 after 1781. |
| F522 set independently | Selects alternate population and excluded maid/King arms; zone bypasses. It is not a general prohibition on every other callback or a post-selection battle veto. |
| F212 clear, F777 set, F258 either | Guard136 still says 1802. Its F212-set continuation is independent, with F258 controlling the excluded script. |
| Missing/duplicate mapping or different followers | Actual physical actor, first reverse match, facing and speech can change; zero script lookup resolves physical 0. A table row alone does not prove an activatable actor. |
| Changed HP/class or prior service result | Eric/Karna's actual sprite can differ; no local recruitment or blanket alive test is added by their text callbacks. Service mutations are not rolled back by wrapper cleanup. |
| Supplied X at Y16 versus natural movement | Wrapper wildcard match is confirmed; actual block request, approach and intended itinerary are Unknown. |
| Stalled camera/text, debug text skipping, incomplete wrapper | EGRESS_MAP may precede an unfinished scene; F422 precedes warp; warp precedes wrapper completion. These are distinct partial-state boundaries. |
| Changed F422/F522 or replaced pending request | Battle selection uses actual destination/flags at consumption, in the order above; talking/scene invocation alone proves no battle start. |

Load `. ./local/private-inputs.ps1` in the same process as controlled commands
and verify the clean pinned checkout. Reuse the unchanged
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`
source equality. Do not regenerate the pair. The retained
`programCorpus.programs` entry `cs_5994E` joins all eight source lines,
macros, arguments and terminator. The existing public
`tests/fixtures/h2/map-events/entity-programs.json` has 18 selected Map22
programs/76 operations; `zone-programs.json` has two/eight. Compare each
operation's source line, opcode/operands and all complete handler boundaries,
including the branch conditions that exclude continuation content. No excluded
postbattle/guard script body needs to be traversed.

Join all 27 table rows/default to the actual population and handler, the
29 selected gamescript literals to comments/document, both setup pointers,
flag polarity/write order, macro encodings and immediate consumers above.
Fixtures do not independently guard every new prose claim: complete bounded
source/prose review, whole-owned-diff, links, English/private boundary and
`git diff --check` supplement them. Run the configured clean committed
`uv run sf2 verify plan --base origin/main --head HEAD`; record actual
final-head CI and exact identities in the Issue handoff. Documentation scope
selects no local blanket normal/full/engine/Godot/H3/H4 suite.

Preserve [#900's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174161863),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/900#issuecomment-6174261324),
[P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174194697)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/902#issuecomment-6174272544).
Keep original/corrected identities and CI 38605731097/38606392847 separate,
along with the unchanged 18,285,979-byte pair/0.081s predecessor receipt and
all linked failures. Its resolved P2 remains: csc2D clears FLAGS_A bits5/6,
so unmodified inline moves bypass the competing-destination scan; ordinary
collision is not an explanation for the coincident trio/ORB targets. This
slice introduces no new claim about those movements. Also retain
[#899's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/899#issuecomment-6174047117)
and [PR901 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/901#issuecomment-6174065903).

**Unknown:** natural local activation/zone reach/itinerary, actual supplied
flags, HP/class/party/mapping/mode/pending state, service/camera/text/wrapper
completion, heard/rendered performance, useful control and save persistence.
Static counterexamples are not executed scenarios or an automatic runtime
queue under [ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md).
