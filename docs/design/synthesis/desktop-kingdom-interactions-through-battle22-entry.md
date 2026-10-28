# Desktop Kingdom: Conversations and the Chessboard Challenge

**Confirmed:** King Ponpei gives permission to approach the chessboard after his final
prebattle line returns. A separate zone checks that permission and starts the encounter scene;
its surviving warp can then admit Battle22. **Inferred:** the King makes defeating his army the
condition for leaving. **Unknown:** the natural itinerary, actual supplied state, completed
services, presentation and useful control. The dialogue alone establishes none of those results.

This English guide continues [Desktop Kingdom first arrival](desktop-kingdom-first-arrival.md)
from its conditional local return. It consumes only
[accepted conversation/admission research](../../research/desktop-kingdom-interactions-through-battle22-entry.md)
under [Issue #907](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/907) and the
[current Standard agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
The selected premise is F522 clear, with actual actor mappings re-resolved for each interaction.
F777 may already be set; F776 does not gate these ordinary conversations.

The endpoint is **entry to BattleLoop before battle opening or roster work**, or the conditional
zone bypass/local return. Postbattle scripts, Cotton Balloon acquisition/use, Floor World,
combat, rewards and exits remain outside this guide. The King's mention of a tunnel is retained
as dialogue without importing that later content. Continue from the admitted BattleLoop boundary in
[Chessboard initialization and initial roster](battle22-initialization-and-roster.md), which consumes
accepted #908 research and keeps fresh activation and resumed turns outside its own boundary.

## Who Can Be Talked To

**Confirmed:** default `ms_map22` and F522-selected `ms_map22_flag522` share the same entity
and zone event pointers. Their population differs. Setup accessors resolve supplied flags again;
neither F776 nor completion of the first-arrival scene is an extra local conversation gate.
The [arrival owner](desktop-kingdom-first-arrival.md) supplies population, follower allocation,
HP/class, alias and raft-overwrite limits. An event row does not guarantee an actor exists or is
naturally reachable in the current layout.

The ordinary action caller idles physical actor 0 and waits for its movement and view scrolling.
A selects the field menu. C can select a nearby actor after the debug and overlapping-Caravan
alternatives. NPC128's callback is separate from that Caravan shortcut. With camera target 0,
activation offsets the candidate by the player's facing, traverses desk/counter blocks, and
chooses the first eligible physical actor within Manhattan distance less than one tile
(MAP_TILE_SIZE=384). It skips the viewed actor and current followers. A negative view target
returns without selecting an actor. These are activation conditions, not a walking itinerary.

The selected physical actor is reverse-mapped through ENTITY_INDEX_LIST, switching to logical
IDs 128 onward after the ally segment. The first reverse match wins; no match falls back to
logical 0. The event wrapper records player facing as EVENT_RELATIVE_POSITION, selects the
first matching four-byte row or its default, and obtains portrait/speech from the mapped actor.
It resolves that logical identity back to a physical actor for its facing operations. Logical
NPC IDs and physical slots are lookup identities, not battle-roster positions.

The row's second operand is an **action bitfield**, despite its facing-like macro names:

| Row value | Operation around a returning callback |
| --- | --- |
| RIGHT=0 | Neither turn nor restore facing. |
| UP=1 | After a VInt, turn the actor to `(playerFacing+2)&3`; no bit-1 restoration. |
| DOWN=3 | Turn as above, then restore the actor's saved facing after return. |

These values do not require approaching from a particular direction and do not describe initial
facing. The wrapper disables entity updates before the callback. After return it conditionally
restores facing, closes portrait/text and enables updates again. That cleanup neither rolls back
service mutations nor proves that a service, text or callback returned.

## Villagers, Eric and Karna

**Confirmed:** the complete table contains 27 explicit rows and one default, selecting 17 named
handlers plus the default. Event N below means `Map22_EntityEventN`. Example physical slots apply
only with the predecessor's F67/F68 followers, no earlier Eric/Karna aliases and no raft overwrite.
Different supplied followers or mappings require a fresh lookup.

| Logical actor; baseline sprite | Example physical slot | Row / Event | Selected prebattle work |
| --- | --- | --- | --- |
| Eric14; ally sprite lookup | 3 | UP1 / 0 | Texts 1772,1773; return. |
| Karna15; ally sprite lookup | 4 | UP1 / 1 | Texts 1774,1775,1776; return. |
| NPC129; YOUNG_MAN196 | 6 | DOWN3 / 3 | Texts 1766,1767; return. |
| NPC130; YOUNG_WOMAN197 | 7 | DOWN3 / 4 | Texts 1768,1769; return. |
| NPC131; OLD_MAN198 | 8 | UP1 / 5 | Texts 1770,1771; return. |

The young man credits Creed with saving them in a natural disaster. The young woman describes a
supposed rule against devils using magic to affect human lives. The old man describes a kingdom
without time, aging or death and asks what life means there. **Inferred:** these accounts explain
the residents' predicament. They do not establish universal rules for magic, immortality or time.

Eric says he was searching for the legendary sword during the disaster and hates being here.
Karna asks about the visitors, accepts the explanation of a mistake, and recommends King Ponpei.
**Confirmed:** both callbacks only deliver their selected text and return. Neither recruits,
writes membership, grants an item nor proves an earlier natural conversation occurred.

## Caravan and Church Services

| Logical actor; baseline sprite | Example physical slot | Row / Event | Immediate handoff |
| --- | --- | --- | --- |
| NPC128; CARAVAN62 | 5 | RIGHT0 / 2 | `j_CaravanMenu`, then return if the service returns. |
| NPC132; PRIEST192 | 9 | UP1 / 6 | `j_ChurchMenu`, then return if the service returns. |

**Confirmed:** Event2 calls CaravanMenu directly. It adds no local F65, party-membership,
vehicle-mode or movement prerequisite. The service saves registers and allocates its local frame,
delivers its opening message, then enters MENU_CARAVAN. Selection -1 takes ExitCaravan, delivers
the exit message, releases the frame, restores registers and returns. Other actions return to
the service menu loop. The separate C-button shortcut's shrinking, music and view-update work
does not belong to this direct callback.

The Caravan message routine chooses **Rohde portrait 11 with F70 clear, Astral portrait 31 with F70 set**.
That service portrait differs from NPC128's dispatcher fallback. The earlier scene's narrative
Goliath attribution neither changes the CARAVAN sprite nor turns this callback into the mansion's
WORKER interaction.

Event6 calls ChurchMenu directly. It uses CURRENT_PORTRAIT for its conditional opening, displays
the welcome, closes windows and enters MENU_CHURCH. Selection -1 takes ExitMenu: conditional
portrait, farewell, window closure, frame/register restoration and return. Other selections
remain with the [common-menu research owner](../../research/common-menus.md).

The local callbacks establish these bounded handoffs and possible returns. They establish no
completed save, healing, promotion, storage/party transaction or post-service player control,
and add no local EGRESS/savepoint write. A returning service reaches the entity wrapper; a
nonreturning service does not. Service mutations are not undone by wrapper cleanup.

## King Ponpei and the Guards

| Logical actor; baseline sprite | Example physical slot | Row / Event | Selected work |
| --- | --- | --- | --- |
| NPC133; MAID208 | 10 | UP1 / 7 | F522 clear: text 1777; return. |
| NPC134; KING_PONPEI188 | 11 | DOWN3 / 8 | Test F522 before F777; permission branch below. |
| NPC135; GUARD206 | 12 | DOWN3 / 9 | F777 clear: 1783,1784; set: 1782; return. |
| NPC136; GUARD206 | 13 | DOWN3 / 10 | F212 clear: 1802; return regardless of F258. |

The maid recommends the King. **Confirmed:** the King's Event8 first checks F522. Its set arm
selects excluded postbattle text 1797 and returns before the permission write. With F522 clear,
F777 chooses the introduction or its repeat:

| F777 when the King callback runs | Conversation order | State change after text return |
| --- | --- | --- |
| Clear | 1778–1780, then shared 1781 | Set F777 after 1781 returns; return. |
| Set | 1781 only | Set F777 again after 1781 returns; return. |

The first conversation asks why the party could not see Creed, mentions a tunnel in Floor World,
and offers departure if the party wins against the King's army. Both arms end with permission to
go to the chessboard. **Inferred:** this frames the next challenge as a condition for leaving.
**Confirmed:** the callback writes only the permission flag at this boundary. It writes no F422,
warp, recruitment, reward or F522 completion and contains no Yes/No branch. Talking alone does
not unlock or start the battle. If the final text does not return, its subsequent write remains
unestablished. A supplied F777 can also select repeat text without proving a prior conversation.

Guard135 tests F777 only. Before permission he calls the army strong and good training; afterward
he says it will not attack until the party reaches the board's center. This is dialogue. The
actual trigger uses wildcard X at Y16, followed by a separate admission check; the line supplies
no universal center or battle-area rule.

Guard136's permission line uses **F212, independently of F777**. F212 clear selects 1802 even
with F777 set and regardless of F258. The identified excluded F212-set continuation selects
1803, then tests F258: set returns; clear calls `cs_599A4` and sets F258 only after its return.
No body of that script, Cotton Balloon custody/use or later exit is consumed here. F212/F258
do not gate the chessboard zone.

The maid's excluded F522-set continuation tests F256: clear selects 1798–1799 then sets F256;
set selects 1800–1801. These and King1797 are identified branch alternatives, not selected
prebattle content. The init owner's F522/F778 dispatch to `cs_5996E` likewise remains outside
this guide. F522 is not a blanket prohibition on every other conversation.

## The Chess Pieces

| Logical actors; baseline sprite | Example physical slots | Row / Event | Selected prebattle work |
| --- | --- | --- | --- |
| NPC137; KING153 | 14 | RIGHT0 / 11 | 1790,1791; return. |
| NPC138,145,146; QUEEN135 | 15,22,23 | RIGHT0 / 12 | 1789; return. |
| NPC139,140; BISHOP144 | 16,17 | RIGHT0 / 13 | 1788; return. |
| NPC141,142; KNIGHT78 | 18,19 | RIGHT0 / 15 | 1787; return. |
| NPC143,144; ROOK125 | 20,21 | RIGHT0 / 17 | 1785; return. |
| NPC147–152; PAWN77 | 24–29 | RIGHT0 / 21 | 1786; return. |
| Unmatched logical ID | Mapping-dependent | Default0 | `Map22_DefaultEntityEvent`: return only. |

This completes all 27 explicit rows: three Queens, two Bishops, two Knights, two Rooks, six
Pawns, the chess King and the eleven actors above. ORB153–155 have no explicit event row.
**Confirmed:** the selected chess callbacks are text and return. Except for the King's
permission branch, the selected text-only callbacks write no story/request state; wrapper
facing, portrait/speech, event-relative-position and update state still change.

**Inferred:** the pieces' speeches characterize a proud, varied army. The Rooks' armor, Pawns'
attack boast, Knights' readiness, Bishops' healing joke, Queens' leadership and King's claim of
balance do not define combat statistics, capabilities or the later battle roster.

If activation finds no actor, the ordinary caller instead checks the area and may fall through
to FieldMenu. Map22's AreaDescriptions callback is only `rts`; with wrapper d7 initially zero,
it contributes no handled description. This does not audit all search rewards. Section5's
item handler is a separate future exit and is outside this outcome.

## Actual Speaker and the Complete Selected Text

**Confirmed:** portrait/speech lookup uses the mapped actor's actual MAPSPRITE byte. Ally lookup
slots are used directly; logical 128+ subtracts ENTITY_ENEMY_INDEX_DIFFERENCE=96. A zero script
lookup resolves physical 0 without a separate absent-actor rejection. Aliases and supplied
mappings can therefore change the actor used for facing and speech.

| Actual sprite under the baseline mapping | Portrait; speech request |
| --- | --- |
| Eric33/34/35; Karna36/37/38 | Eric14; Karna15 respectively, each bleep5. |
| PRIEST192 | Priest34; bleep5. |
| YOUNG_MAN196 / YOUNG_WOMAN197 / OLD_MAN198 | NONE; bleep2 / bleep3 / bleep6. |
| MAID208 / KING_PONPEI188 / GUARD206 | NONE; bleep3 / bleep6 / bleep5. |
| CARAVAN62; the selected chess sprites | No matching entry: DEFAULT; bleep6. |

NONE=255 sign-extends to -1; DEFAULT=$FFFF is also -1. Neither requests a new portrait window.
Do not give Ponpei or the chess King KING_GRANSEAL's portrait. Eric/Karna class variants share
their respective portraits, but zero HP in exploration selects BLUE_FLAME and aliases/class
changes can alter the actual sprite. Their callbacks add no separate alive test. A changed
sprite does not itself prove the conversation cannot run. The Caravan service override remains
separate from this dispatcher result.

The selected callback set is exactly 1766–1791 plus 1802, derived from the complete selected arms;
1792–1793 add the two encounter lines, for **29 distinct literal/control joins**. Plain `txt`
uses the TEXTBOX trap and ID with the dispatcher speaker state. No selected literal substitutes
a dynamic name. `{N}` is a line-break control; `{W1}`/`{W2}` remain explicit controls, without a
measured wait or proven input advance. The accepted owner's wording, including text 1788, is preserved:

| ID | Speaker / condition | Exact literal |
| --- | --- | --- |
| 1766 | Young man129 | `Creed is a fine gentleman.{W2}` |
| 1767 | Young man129 | `His magic saved us a long time{N}ago during a natural disaster.{W1}` |
| 1768 | Young woman130 | `Devils cannot use their magic{N}to affect human lives.{W2}` |
| 1769 | Young woman130 | `Supposedly, it's one of their{N}rules.{W1}` |
| 1770 | Old man131 | `This Kingdom was made by{N}Creed.  There's no concept{N}of TIME here.{W2}` |
| 1771 | Old man131 | `We never get old.{N}We never die.{N}What is the meaning of life?{W1}` |
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

## Requesting the Encounter

**Confirmed:** the sole zone row is `(255,16)` → Map22_ZoneEvent0, followed by a default `rts`.
Byte 255 is an X wildcard; Y must be 16. Any supplied X at Y16 matches the wrapper row. This
does not prove every physical tile on that row can naturally request the event. The ordinary
control actscript recognizes destination block type `$1400`, writes MAP_EVENT_TYPE=6 and
derives X/Y word parameters from destination plus attempted offsets, divided by 384. Layout,
movement and reach still matter; no local facing or F776 gate is added by the row.

WaitForEvent prioritizes pending events over A/C. ProcessMapEvent clears the type before
dispatching type 6. The type 6 consumer installs the init actscript on physical 0 and passes the
X/Y words from PARAM1/PARAM3 to the zone wrapper. The callback first tests F777; clear returns.
It then tests F522; set returns. Only **F777 set/F522 clear** writes EGRESS_MAP=22 and calls
`cs_5994E`.

EGRESS_MAP supplies only a map selector. The byte write neither supplies egress coordinates or
facing nor changes map-event parameters. The later warp supplies its own destination tuple.
This is not a completed egress, savepoint or service transaction; the map byte may already be
changed when a later scene command fails to finish.

### The Complete Eight-Command Scene

**Confirmed:** `cs_5994E` has eight commands, 32 encoded bytes. Indices are zero-based. The
sequence asks whether the party is ready, announces the start, then writes the unlock and warp:

| Index | Exact command | Immediate effect and completion boundary |
| --- | --- | --- |
| 0 | `textCursor 1792` | Set CUTSCENE_DIALOG_INDEX=1792. |
| 1 | `csWait 50` | Consume encoded negative wait; Sleep uses low byte 50 when text skipping is off. |
| 2 | `setCamDest 10,0` | Set VIEW_TARGET_ENTITY=-1, request camera destination and wait for scrolling. |
| 3 | `nextText $0,134` | Resolve NPC134's actual portrait/speech, wait for scrolling, display 1792, advance cursor. |
| 4 | `nextSingleText $0,134` | Display 1793, advance cursor, close portrait/text, then Sleep 10. |
| 5 | `setStoryFlag 22` | Add BATTLE_UNLOCKED_FLAGS_START=400 and set F422; neither F22 nor F522. |
| 6 | `warp MAP_DESKTOP_KINGDOM,0,0,RIGHT` | Write pending type 1 and PARAM1–5 bytes `0,22,0,0,0`. |
| 7 | `csc_end` | End script; conditional scroll wait, clear VIEW_SCROLLING_SPEED, restore caller registers. |

Camera `(10,0)` is a requested border, scaled by 384, not actor placement or the warp destination.
The scroll consumer polls with VInt waits and rechecks, without a timeout. Text handlers also
wait for scrolling. Baseline KING_PONPEI requests no new portrait and both lines use bleep6.
Modifier$0 adds neither right-side nor mirror bit; an already open portrait is not reopened.
The scene does not turn or move NPC134 or recruit anyone.

There is **no Yes/No command or answer branch**, despite “Are you ready?” Ordinary non-skipping
execution reaches F422 and the warp only after preceding wait/camera/text calls return. The
debug P2-Start text-skip path can skip the encoded wait and two text calls, but does not skip
camera, flag or warp. Source waits are not measured presentation duration or proof of completion.

After script return, the zone wrapper closes windows, waits one VInt, and waits until physical 0's
current X/Y equal its destination. It does not wait for every entity/action. The pending warp
already exists at this point; an unfinished wrapper can leave it unconsumed. EGRESS_MAP, F422,
pending warp and wrapper completion are separate partial-state boundaries without rollback.

## Consuming the Warp and Admitting Battle22

**Confirmed:** a surviving request takes priority over camera-target 0/current-mode control
reinstallation. ProcessMapEvent clears the pending type and takes type 1's no-scroll path because
PARAM1=0. It consumes/clears WARP_SFX, idles physical 0 and unwinds exploration to MainLoop.
UpdatePlayerPosFromMapEvent initially reads physical 0's tile; explicit nonnegative PARAM3/PARAM4
override it with `(0,0)`, and PARAM5 supplies Right=0. The requested tuple is therefore
`(22,0,0,Right)`, independently of camera `(10,0)`. Useful local control between these stages is not
established.

MainLoop calls SwitchMap then CheckBattle. The complete switched-map table contains no Map22
entry. The sole Map22 battle-coordinate row is zero-based Battle22, tuple
`22,9,0,30,30,255,255`. CheckBattle tests **map, F422, then the two 255 wildcard trigger bytes**.
It does not test the player against the battle-area rectangle. On selection it writes area
`(9,0,30,30)`, then checks F522. F522 set clears F422 but does not revoke this call's selection.

| Actual state at consumption | Conditional result |
| --- | --- |
| Surviving Map22 warp; F422 set, F522 clear | Wildcards match even at `(0,0)` outside the area. Write area, retain F422, return 22; MainLoop calls BattleLoop with 22. |
| F422 independently cleared before CheckBattle | Skip the row before area/completion check; return -1 and enter fresh explicit-Map22 exploration. Its later init/control is not established here. |
| F522 independently set after zone admission; F422 still set | Write area, clear F422, still return 22 and admit BattleLoop this time. |
| Request replaced, incomplete or destination changed | Consume actual surviving fields; the named Map22/Battle22 result is not guaranteed. |

The F777-clear/F522-set zone bypass writes neither EGRESS_MAP, F422 nor warp. After wrapper
return, zero pending type lets WaitForEvent target physical 0 and install current-mode control;
a pending replacement takes priority. This bypass itself never calls CheckBattle, even with
F422 supplied set. Conversely CheckBattle does not consult F777/F776/F212/F258; those flags do
not veto an independently admitted Map22 request with F422.

**Stop at BattleLoop admission before opening/roster, or that conditional local bypass/return.**
No F522 completion write, reward, postbattle grant, item acquisition or exit follows from this
boundary. Neither an invoked scene nor the King's invitation proves a completed battle start.

## Evidence and Review Boundary

**Confirmed provenance:** accepted main `ccf4a57360fea5b62160fe959a5d2dbca6dd8736` contains
[PR906](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/906). Its
[research owner](../../research/desktop-kingdom-interactions-through-battle22-entry.md#agreement-and-provenance)
pins [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)
and USA retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This synthesis reviews accepted joins, without raw-source reinvestigation, extraction or runtime.

The bounded executable review uses the existing
[map-init fixture](../../../tests/fixtures/h2/map-init-static-v1.json),
[schema](../../../schemas/h2-map-init-static-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_init.py), plus the
[map-script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json),
[schema](../../../schemas/h2/map-script-engine-fixture.schema.json) and
[verifier](../../../src/sf2tool/h2/map_script_engine.py). The retained recipe uses map-init
equality, `_verify_map_script_engine_fixture` and `_cutscene_facts` source equality; this guide
does not rerun or regenerate the unchanged 18,285,979-byte pair/0.080s receipt.

The [entity-program shard](../../../tests/fixtures/h2/map-events/entity-programs.json) contains
18 selected Map22 programs/76 operations; the
[zone-program shard](../../../tests/fixtures/h2/map-events/zone-programs.json) contains two/eight.
Their [schema owner](../../../schemas/README.md) and
[fixture loader](../../../src/sf2tool/h2/map_events_fixture.py) define the closed shard boundary;
the [verifier](../../../src/sf2tool/h2/map_events.py) retains complete equality.
The selected `programCorpus.programs` entry joins all eight scene commands and the terminator.
Map-init dispatcher/pointer order and map-script aggregates prove their named static boundaries.
Complete accepted direct-source joins supply local callback, speaker, service and admission
details that those aggregates do not independently guard. No aggregate proves natural reach,
presentation or composed runtime success. No conflicting accepted owner was found at this boundary.

Preserve [#904's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174520666):
head `eba9181e8505e38daa261311281b3aab6164af3f`, tree `6ea40ea61af5063d14422570283a55d8b78d1800`,
base `e120a9b6890527eb018e081f1c5b1907c21155bc`,
[CI38608369060](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608369060).
Its [final handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/904#issuecomment-6174585157)
retains the byte-identical rebase head `ee2bf3ff5d00da75e6913ba5b2813c5ee9b49c29`,
tree `7933d0b8dd61953fb9fb40a1584d11cf7478a86e`,
[CI38608730439](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608730439),
and final EGRESS_MAP clarification head `4b6595ea854670a9f12a985d3583750b46c217d9`,
tree `edb7ff22fd26365d8432baa32ff978d7883fd831`,
[CI38608904864](https://github.com/FrankHZ/md-sf2-reverse-engineering/actions/runs/38608904864).
Rebased/final base is `affc5679674521f8ce26649763fd749a2e178095`. All three CI runs succeeded
in scope/research-public; engine, adapter and H4 jobs were skipped by scope. The
[independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/906#issuecomment-6174618486)
applies to the final identity. Original truncated reads, HTTPS private-path false positive,
table-range/King-write corrections and CRLF equality checks remain with those receipts.

Preserve [#903's original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174446531),
[correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/903#issuecomment-6174502720)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/905#issuecomment-6174512539).
Those handoffs retain the original/corrected identities, CI38607861678/38608377291,
inspection failures and resolved corrections: negative-d1 reload reuses existing blocks/layout;
RightLeft changes facing. The arrival guide's
[evidence section](desktop-kingdom-first-arrival.md#evidence-review-and-changed-state-limits)
also preserves #900 original/corrected heads, CI38605731097/38606392847, unchanged pair/0.081s,
resolved csc2D FLAGS_A5/6 destination-scan bypass and all #899/#896/#895 linked failures and
corrections. No completed result is replaced or replayed for this synthesis.

Direct acceptance checks the complete owned prose/diff, all actor/event and 29 literal/control
joins, branch/write order, eight commands, links/fragments, English/private boundary and
whitespace. Run only the configured clean committed dependency planner and actual final-head CI;
documentation alone requires no local normal/full/engine/Godot/H3/H4/translation suite.

**Unknown:** natural local activation/zone reach/itinerary; supplied flags, HP/class, party,
aliases/mappings, mode and pending state; completed service/camera/text/wrapper work; rendered
or heard performance, useful control and save persistence. Changed-state examples above are
static counterexamples, not executed scenarios or an automatic runtime queue under
[ADR0014](../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) and
[ADR0016](../../decisions/0016-remake-start-evidence-deferral.md). This bounded guide completes
neither the campaign nor [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
