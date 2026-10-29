# Creed restoration and main-room departure

Evidence date: 2026-10-14. **Confirmed:** the normal post-Willard Map33 return
selects seven resident declarations and runs the complete `cs_5B016` when F523
is set and F783 is clear. After that script returns, init sets F783, writes
EGRESS_MAP=Hassan and executes its remaining placement branches. **Inferred:**
the scene depicts Creed restoring the party and leaving with the Fairy to
consult the Evil Spirit. **Unknown:** natural traversal, supplied state,
completed concurrent actions/services, visible restoration, usable control
and persistence. Leaving the main room is not a completed basement visit.

## Agreement and provenance

Standard rigor under [Issue #941](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514).
The uncertainty is the complete scene/population/consumer/init-return join.
Accepted starting base is `5c2eb99da1c056b7547b54c13ec506b2bf9c25ba`, including
corrected #937/PR939. This sole owned document supports the next English guide;
no shared index, registry, schema, fixture, tooling, design or remake change.
The concurrent #938 design outcome is disjoint, not an evidence dependency.

Consume accepted [Battle23 outcomes and burrow return](./battle23-outcomes-and-burrow-return.md),
[Creed entry](./creed-mansion-entry-and-first-goliath-scene.md),
[magic door](./creed-magic-door-and-desktop-transfer.md),
[setup selection](../../world/map-event-flag-route-selection.md) and
[request consumption](../../world/map-event-request-consumption.md).
Begin at admitted Map33 exploration from the Map27 exit request and stop when
this init returns through the conditional exploration handoff. No basement
zone/conversation, Evil Spirit scene, recruit selection, Oddler departure,
later battle, runtime/comparator work or payload regeneration is included.

Source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Setup filenames mean `data/maps/entries/map33/mapsetups/`; map/entity consumer
filenames mean `code/common/scripting/map/` and `code/common/scripting/entity/`.
Fnnn means flag nnn. Command and resident indices are zero-based. Coordinates,
speeds and waits are source operands, not measured motion or elapsed time.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair and small
selected inventories. Linear comparison budget is below one minute/512 MiB,
plus small prose/receipts; the pair/cutscene comparison completed in 0.080s
with size/mtime unchanged. No acquisition, environment or payload was created.
Reassess material consumer/resource expansion. Aggregate equality alone does
not prove the new prose: direct identity joins below cover the selected source.

## Admission, setup selection and population

**Confirmed:** the accepted complete Map27 warp table has two Map33 exits:
`(255,0)` and `(21,255)`, both no-scroll requests to `(15,9)`, Right0. Negative
trigger bytes are wildcards, and the first matching row wins. Neither exit
tests F523. Their admitted consumer returns through normal MainLoop's SwitchMap
then CheckBattle; no Map33 substitution or battle-coordinate row exists.
The tuple assumes the predecessor's low-byte coordinate limitation is satisfied;
its explicit destination bytes do not repair arbitrary inherited high bytes.
Neither a table match nor F523 proves natural traversal of the burrow.

`code/gameflow/exploration/explorationfunctions_2.asm:ExplorationLoop` clears
MAP_EVENT_TYPE, adjusts STEP_COUNTER, heals living/immortal allies, starts
fade-out and disables view following. Explicit map33 sets CURRENT_MAP=33 and
CURRENT_BATTLE=255, loads tilesets, waits fading, clears VInts, resolves the
entity setup, rebuilds actors and loads mapsprites. It clears temporary flags
256–383 and sets F80; F523/F774/F783/F784/F786/F22 survive that reset. Common
work initializes sprite frames, targets physical0, calls LoadMap with d0=-1,
d1=33, installs base VInts and calls the selected init. Full-map loading avoids
the battle crop. A supplied MAP_CURRENT=255 entry instead preserves population,
updates main/raft properties and uses current layout; it is not either explicit
Map27-to-Map33 exit and does not instantiate the rows below anew.

`mapsetupsfunctions_1.asm:GetCurrentMapSetup` scans every override, replacing
the pointer for each set flag; last-set wins. The complete Map33 entry in
`data/maps/mapsetups.asm:106–112` and all four pointer tables resolve:

| Order / guard | Selected setup | Entities / init |
| --- | --- | --- |
| Default | ms_map33 | Six baseline residents / ms_map33_InitFunction |
| F523 | ms_map33_flag523 | Seven return residents / same init |
| F783 | ms_map33 | Baseline residents / same init |
| F784 | ms_map33_flag784 | Seven later residents / same init |
| F786 | ms_map33_flag786 | Three later residents / alternate return-only init |
| F22, last | ms_map33 | Baseline residents / normal init, overriding F786 too |

Every pointer table shares EntityEvents, ZoneEvents, AreaDescriptions and
Section5. The normal return premise is F523 set, F783/F784/F786/F22 clear.
F774 does not select a setup or gate this scene. Each accessor reselects from
current flags: `j_GetMapSetupEntities` resolves to GetMapSetupEntityList, and
`j_RunMapSetupInitFunction` to RunMapSetupInitFunction in
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm`. Population and init
selection need not agree if supplied flags change between calls.

The complete `s1_entities_523.asm`, `$5A6D8..$5A712`, is seven fixed rows then
msEntitiesEnd. Exact row identity, not just count, is significant:

| Row | Logical actor | Sprite source | Initial X/Y, facing, action |
| --- | --- | --- | --- |
| 0 | 128, Goliath in dialogue | WORKER202 | `(63,63)`, Up, eas_Init |
| 1 | 129, Creed | CREED212 | `(19,6)`, Left, eas_Init |
| 2 | 130, Fairy | FAIRY219 | `(63,63)`, Up, eas_InitSlow |
| 3 | Eric14 | Ally/class/HP lookup | `(63,63)`, Down, eas_Init |
| 4 | Randolf16 | Ally/class/HP lookup | `(63,63)`, Up, eas_Init |
| 5 | Tyrin17 | Ally/class/HP lookup | `(63,63)`, Down, eas_Init |
| 6 | Karna15 | Ally/class/HP lookup | `(63,63)`, Up, eas_Init |

`mapfunctions.asm:InitializeMapEntities` clears actors and lookups, allocates
followers from physical1, masks resident coordinates with `$3F`, scales them
by MAP_TILE_SIZE=384 and declares the rows. Ordinary NPC lookup entries begin
at offset32, addressed by logical128–130 after subtracting96. Ally rows use
ally lookup indices; an existing nonzero lookup skips the entire fixed row.
They have no joined/active-membership guard. Bowie is declared at physical0
with the supplied arrival coordinates/facing and eas_Idle. Parked rows remain
allocated; they are not a claim that four recruits appear visibly or join.

The complete `data/scripting/entity/followers.asm` and
`followersfunctions_1.asm:InitializeFollowerEntities` retain independent
flag-selected followers. Map33 is absent from `data/maps/global/overworldmaps.asm`,
so F65 does not add the overworld Caravan prefix here. With only F67/F68 among
ordinary follower selectors, Peter7 is physical1 and B31/Oddler physical2;
Goliath/Creed/Fairy are physical3/4/5, and Eric/Randolf/Tyrin/Karna physical6/7/8/9.
This is a conditional allocation example, not fixed physical identities.
Other flags consume slots in table order. Repeated B rows (F74 Astral, F68
Oddler, F70 Astral) are not deduplicated; later rows overwrite the B lookup.
Missing Peter/B leaves zero lookup, aliasing physical0. F69's FOLLOWER_C Fairy
is separate from resident130, even though both can use the Fairy sprite.

`getallymapsprite.asm:GetAllyMapsprite` uses actual class/HP and selects the
blue flame for zero-HP allies outside battle. Exploration healing includes
living allies and Peter/Lemon, not every dead ally. No fixed Peter row or
newEntity fallback in this scene repairs missing follower selection. B31 is
outside the ally HP test. GetEntityAddressFromCharacter masks the logical
operand, resolves the lookup and returns the physical actor; logical names
are not safe substitutes for actual mapping. PLAYER_TYPE is not changed by
this scene/init; the coherent Battle23 predecessor supplied walking0. Existing
mode/raft consumers and their supplied-state exceptions remain as documented
in the accepted Creed entry owner.

Changed-premise setups are also explicit. Baseline rows contain Goliath128,
Creed129, Eric14, Randolf16, Tyrin17, Karna15, all parked at `(63,63)`; there is
no resident130. F784's seven rows have Goliath `(17,14)` Up, Creed `(17,5)` Down,
Fairy `(17,10)` Up, then the same four parked allies. F786's three rows are
walking WORKER `(22,4)` Up with origin `(22,4)`/range1, CREED `(17,5)` Down and
ODDLER `(17,7)` Up. Its pointer targets the source-named
`ms_map33_flag312_InitFunction` in `s6_initfunction_786.asm`, `$5AC56..$5AC58`:
a single rts, despite that misleading label suffix. It bypasses the scene and
all normal init writes. F22 can override it. A normal-init selection with
F523 set/F783 clear still runs the scene under F784 or F22; it does not validate
that the return population or expected positions were supplied. A baseline
missing130 can alias main; successful later hide130 could then hide that actor.

## Complete scene and embedded actions

**Confirmed:** `scripts.asm:247–535`, address `$5B016`, is the entire selected
program, ending at its own csc_end before `cs_5B466` at `$5B466`. All 212 retained
commands were joined to exact source line, macro and operands. All 24 inline
movement bodies and six custom bodies were read through their terminators.
There is no branch into the adjacent program.

In the table, P=Peter7, B=FOLLOWER_B31, G=128, C=129, F=130. Directions U/D/L/R
mean Up/Down/Left/Right. `W(n)` is csWait n. A movement is entityActionsWait
unless explicitly marked unawaited; each listed sequence ends in endActions.
`speed actor,n` is the complete waited custom body `ac_setSpeed n,n;
ac_jump eas_Idle; ac_end`. Init, InitSlow, Jump and DeactivateAutoFacing are
waited setActscriptWait calls. RightLeft/UpDown and TwirlGrow are unawaited
setActscript calls. Text IDs refer to the separate exact-order table below.
These conventions retain the actual wait distinction for every action.

| Commands | Complete ordered sequence |
| --- | --- |
| 0–7 | textCursor1840; reloadMap `(12,2)`; set Bowie/P/B `(63,63)` Down in that order; fadeInB; W40; text1840. |
| 8–18 | C D1,L2; face C Down; texts1841–1842; set G `(17,14)` Up; setCamDest `(12,4)`; G U2; stepEvent `(17,11)`; G U3; texts1843–1844. |
| 19–27 | W10; shiver G; texts1845–1846; headshake G; text1847; speed C,48; C D1; text1848. |
| 28–39 | quake3; shiver G; quake0; shiver G; texts1849–1852; quake3; shiver G; quake0; shiver G. |
| 40–51 | W20; face C Up; W30; speed C,16; C U1; W20; face C Down; text1853; nod G; text1854; speed G,48; G D5. |
| 52–66 | Face C Up; W120; face C Down; text1855; W5; C RightLeft; W120; C D1,L1; W20; text1856; W5; C RightLeft; text1857; W30; C L1. |
| 67–79 | Text1858; face C Down; W20; C Init; C Jump; texts1859–1866. |
| 80–88 | Speed C,16; C DeactivateAutoFacing; C U1; W40; shiver C; W30; shiver C; W20; text1867. |
| 89–97 | White flash4; W40; flash4; W30; flash4; W20; flash4; W10; flash30. |
| 98–111 | Set Bowie `(16,9)` Down then its TwirlGrow; set P `(15,9)` Down then its TwirlGrow; set B `(17,9)` Down then its TwirlGrow; W60; W5; Bowie RightLeft; W5; P UpDown; W5; B RightLeft; W120. |
| 112–124 | Face P Right; text1868; nod Bowie twice; face Bowie Right; nod B twice; text1869; face Bowie/P/B Up in order; texts1870–1871. |
| 125–135 | Set F `(16,0)` Down; F D7; texts1872–1873; F D1; text1874; nod Bowie; text1875; W5; F RightLeft; text1876. |
| 136–147 | C Init; C D1; face F Down/C Right; text1877; F DeactivateAutoFacing; install F R1 unawaited; C R1 waited; face C Down; text1878; nod Bowie; text1879. |
| 148–156 | Speed C,16; C U1; text1880; face C Down; G U4,R1,U3; face G Left; G Jump twice; text1881. |
| 157–165 | Face F Up/C Right; text1882; face C Down; G L1; text1883; face C Right; shiver C; text1884. |
| 166–177 | Quake4; shiver G; quake0; G Jump twice; G R1,D2; W20; F InitSlow; F U1; face F Left; text1885; face C Right. |
| 178–190 | Texts1886–1892; face G Up; shiver G; G DeactivateAutoFacing; speed G,16; install G D1 unawaited; text1893. |
| 191–200 | Face C Down; text1894; quake4; G Init; G L1; face G Up; quake0; text1895; face C Left; text1896. |
| 201–211 | C Init; C L3,U2,L4; hide C; F InitSlow; F L1,D1; text1897; F U1,L3,U2,L4; hide F; followEntity P,Bowie,2; followEntity B,P,2; csc_end. |

Moves are relative to actual current actor coordinates, not to a presumed
previous story endpoint. Under the seven-row premise and completed earlier
moves, Creed's `(19,6)` seed reaches `(17,7)` after command8 and `(15,8)` before
Peter's first text; his Up1 before the spell reaches `(15,7)`. After the later
Down1/Right1/Up1 sequence he is `(16,7)`, so his departure path ends `(9,5)`
before hide. Fairy starts its explicit `(16,0)` approach, reaches `(16,8)`,
then its Right1 and Up1 would reach `(17,7)`; the final two sequences likewise
lead to `(9,5)` before hide. The unawaited steps are premises, not joined facts.
Goliath's final Down1 is likewise unawaited and can be replaced by Init; the
normal caller's final absolute `(17,10)` placement is independent of its
completed displacement. No complete basement route is inferred from these paths.

## Text identities and immediate consumers

**Confirmed:** all 58 source-comment literals match `data/scripting/text/gamescript.txt`
keys `0730`–`0769` exactly, including spaces and controls, in cursor order
1840–1897. The table paraphrases content instead of republishing the transcript.
S=nextSingleText; T=nextText. Speaker notation C80 means `$80,129`, C0 means
`$0,129`; G/P/F use modifier0, and none means `$FF,ENTITY_NONE` (`$FFFF`).

| ID / command | Form / speaker | Paraphrased content | Ordered controls |
| --- | --- | --- | --- |
| 1840 / 7 | S/C80 | Calls Goliath | `{W1}` |
| 1841 / 10 | S/C80 | Calls louder | `{W1}` |
| 1842 / 11 | S/G | Answers that he is coming | `{W1}` |
| 1843 / 17 | S/G | Addresses Creed | `{W1}` |
| 1844 / 18 | S/C80 | Remarks on the noisy collection | `{N}{W1}` |
| 1845 / 21 | S/G | Hesitates | `{W1}` |
| 1846 / 22 | S/C80 | Asks what is wrong | `{W1}` |
| 1847 / 24 | S/G | Admits adding to the collection | `{N}{W1}` |
| 1848 / 27 | S/C80 | Reacts angrily | `{W1}` |
| 1849 / 32 | T/G | Apologizes | `{W2}` |
| 1850 / 33 | S/G | Says they jumped through the magic door | `{N}{W1}` |
| 1851 / 34 | T/C80 | Accuses him of leaving it unlocked | `{N}{W2}` |
| 1852 / 35 | S/C80 | Repeats the locking instruction | `{N}{W1}` |
| 1853 / 47 | S/C80 | Demands to see the additions | `{W1}` |
| 1854 / 49 | S/G | Agrees to fetch them | `{W1}` |
| 1855 / 55 | S/C80 | Reacts to a sound | `{W1}` |
| 1856 / 61 | S/none | Unattributed interjection | `{W1}` |
| 1857 / 64 | S/C80 | Reports hearing something | `{W1}` |
| 1858 / 67 | S/P | Asks whether this is Creed | `{W1}` |
| 1859 / 72 | T/C80 | Hears someone in his collection | `{N}{W2}` |
| 1860 / 73 | S/C80 | Identifies them as Goliath's additions | `{N}{W1}` |
| 1861 / 74 | T/P | Says they sought him | `{W2}` |
| 1862 / 75 | S/P | Requests original size | `{N}{W1}` |
| 1863 / 76 | T/C80 | Repeats that they sought him | `{W2}` |
| 1864 / 77 | S/C80 | Asks why | `{W1}` |
| 1865 / 78 | S/P | Requests enlargement, complaining of neck strain | `{N}{W1}` |
| 1866 / 79 | S/C80 | Apologizes | `{W1}` |
| 1867 / 88 | S/C80 | Speaks the restoration incantation | `{W1}` |
| 1868 / 113 | S/P | Says they are normal again and recalls being small | `{N}{N}{W1}` |
| 1869 / 119 | S/C80 | Asks their purpose | `{N}{W1}` |
| 1870 / 123 | S/P | Asks about Zeon | `{N}{W1}` |
| 1871 / 124 | S/F | Reacts to Zeon's name before placement | `{W1}` |
| 1872 / 127 | S/F | Repeats the question after approach | `{W1}` |
| 1873 / 128 | S/C80 | Identifies the devil king sealed in Arc Valley | `{N}{W1}` |
| 1874 / 130 | S/F | Asks whether Zeon has revived | `{N}{W1}` |
| 1875 / 132 | S/none | Narrates the leader's nod | `{LEADER}{W1}` |
| 1876 / 135 | S/F | Expresses disbelief | `{N}{W1}` |
| 1877 / 140 | S/C80 | Silences the Fairy | `{N}{W1}` |
| 1878 / 145 | S/C80 | Asks whether the report is true | `{W1}` |
| 1879 / 147 | S/none | Narrates a second leader nod | `{LEADER}{W1}` |
| 1880 / 150 | S/C80 | Predicts Zeon's destructive return | `{N}{W1}` |
| 1881 / 156 | S/G | Reports the additions missing | `{N}{W1}` |
| 1882 / 159 | S/C0 | Says he knows | `{W1}` |
| 1883 / 162 | S/G | Asks how | `{W1}` |
| 1884 / 165 | S/C0 | Dismisses Goliath | `{N}{W1}` |
| 1885 / 176 | S/F | Reacts to the crisis | `{N}{W1}` |
| 1886 / 178 | T/C0 | Reports a dark cloud over Grans | `{N}{W2}` |
| 1887 / 179 | T/C0 | Reports drought in North Parmecia | `{N}{W2}` |
| 1888 / 180 | S/C0 | Contrasts South Parmecia | `{W1}` |
| 1889 / 181 | T/F | Credits Volcanon | `{W2}` |
| 1890 / 182 | S/F | Fears for Mitula | `{W1}` |
| 1891 / 183 | T/C0 | Calls Zeon the exception to their invulnerability | `{N}{N}{W2}` |
| 1892 / 184 | S/C0 | Emphasizes Zeon's power | `{N}{W1}` |
| 1893 / 190 | S/G | Reacts fearfully to Zeon | `{W1}` |
| 1894 / 192 | S/C0 | Challenges Goliath's continued presence | `{N}{W1}` |
| 1895 / 198 | T/C80 | Defers scolding him | `{N}{W2}` |
| 1896 / 200 | S/C80 | Proposes consulting the Evil Spirit | `{N}{W1}` |
| 1897 / 206 | S/F | Tells the group to follow Creed | `{N}{W1}` |

`sf2cutscenemacros.asm` encodes those modifier/speaker bytes and textCursor;
`mapscriptengine_2.asm:csc04` sets the cursor; csc00/csc02 consume it, wait
for view scrolling, call DisplayText and increment it. S closes portrait/text and
sleeps10; T leaves the continuation open. W1/W2 are text-input waits, not
choice branches; W2 also uses the continuation arrow/validation-sound path.
`code/common/scripting/text/textfunctions_1.asm` supplies line breaks and
LEADER substitution through UpdateForce and the first battle-party member's
name. The two narrated nod commands address Bowie, while their narration uses that
runtime leader name; arbitrary supplied membership need not identify them.

`mapscriptengine_1.asm:csc1D` and `code/common/menus/portraitwindow.asm:OpenPortraitWindow`
interpret modifier `$80` as right-side, unmirrored portrait; modifier0 sets
neither bit. Existing portrait state or a missing portrait result can suppress
opening. `getentityportaitandspeechsfx.asm` resolves the actual mapped sprite
through its dialogue-property table for portrait/SFX; logical129 does not
independently prove a Creed portrait. `$FFFF` bypasses lookup and writes speech
SFX0. Text1856 remains unattributed; no actor is selected for that interjection.

ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry, but debug mode with P2
Start can latch it. The dispatcher then skips csWait sleeps; csc00/csc02 also
skip text and cursor advancement. The nominal sequential table assumes that
skip is not latched. It does not prove all text appeared or that time remained
available for concurrent growth/look/movement actions.

**Inferred:** the dialogue and staging express restoration and a plan to seek
information downstairs. The lines about Zeon, weather, Volcanon and Mitula are
statements by scene speakers, not independent proof of world state or a new
implemented rule. B is intended as Oddler under the F68 premise, but has no text
in this selected program. No consent menu or recruitment command appears.

## Reload, movement, growth and presentation boundaries

**Confirmed:** `mapscriptengine_1.asm:csc46_reloadMap` disables view following,
deactivates VInts, packs `(12,2)` as `(12<<8 | 2)*3` for d0 and supplies d1=-1
to LoadMap. `code/common/maps/mapload.asm:LoadMap` interprets these as view
selection and current-map reload: it reuses the working layout and actors,
rather than running InitializeMapEntities or recursively invoking map init.
It reloads display/area state, then csc46 enables display/interrupts, activates
VInts and waits one VInt. It does not make the party small or repair aliases.
The subsequent three setPos calls park actual actors at `(63,63)`, Down.
That is separate from inheriting the earlier shrinking action across fresh maps.

csc39 calls FadeInFromBlack; `code/common/tech/interrupts/fadingcommands.asm`
owns its fading service. csc32's setCamDest `(12,4)` disables view following,
passes tile coordinates through SetCameraDestination (scaling by384) to
SetViewDestination and waits for view scrolling. This is a camera destination,
not an actor coordinate. No claim of exact displayed camera position/timing
follows without the view consumers completing in actual state.

The sole csc47 step event scales `(17,11)` by384 and calls OpenDoor in
`code/gameflow/exploration/exploration.asm`. With CURRENT_BATTLE255 it selects
row1 of the complete `data/maps/entries/map33/4-step-events.asm`: source `(26,0)`,
size1x1, destination `(17,11)`. The positive-source branch copies one live layout
word at FF0000 with a 64-word row stride, requests SFX_DOOR_OPEN and marks the
plane update selected by MAP_AREA_LAYER_TYPE. An in-battle call returns early.
No persistence flag is written. The complete `3-flag-events.asm` has only F22's
25x32 copy from `(28,0)` to `(0,0)`, not an F783 replay of this opening. A copy
request proves neither visible opening nor collision/traversal/persistence.

csc19 sets current and destination X/Y together from unsigned byte operands
scaled by384, writes facing and requests sprite update. csc23 writes facing
and requests the same update. Both use AdjustScriptPointerByCharacterAliveStatus:
logical ally IDs below30 with HP0 skip the command; NPC128–130 and B31 bypass
that guard. followEntity has a different encoding boundary: its follower is a
word, but csc2C first reads that word's **high byte** for the HP guard. Both
selected follower words7 and31 supply0, so both relations test **Bowie's HP**,
not Peter's or B's. If admitted, the full words then resolve the actual actors.
In contrast, csc14/csc15/csc2D action installation and nod/headshake/shiver have
no such HP skip. HP0 can suppress the applicable placement/facing or both
relations without suppressing the action or gesture. This follows the encoded
use site, not a general assumption that all commands test their named actor.

csc14/csc15 install the selected action pointer and seed its wait timer with
the resolved physical index. Their waited forms poll that actor's pointer for
eas_Idle, without a timeout. csc2D compiles signed relative moves into action
work RAM, appends an idle jump and optionally polls only its selected actor.
`entityscriptengine_2.asm:esc04_moveToRelativeDest` scales signed deltas by384,
adds actual current coordinates and checks destination conflict before advancing.
`esc00_wait` consumes the per-entity byte timer; literal wait counts and speed
bytes are not independently measured frame durations/pixel rates. Replacement
of the same actor's pointer can cancel its prior action. Waiting for Creed's
Right1 does not join Fairy's Right1; later Goliath Init need not join his Down1.

The named helpers are complete in `data/scripting/entity/eas_actions.asm`:

- eas_Init sets acceleration factors0/0, speed32/32, acceleration/deceleration
  off, obstructability on, entity/map uncollidable off, autofacing on,
  orientation Up, immersion off, size24, sprite update and idle. It does not
  reset position. Init later replaces Goliath/Creed's custom speeds.
- eas_InitSlow uses speed16/16, factors1/1, acceleration and deceleration
  `-2,ON`, then the same obstruction/collision/autofacing/orientation/immersion/
  size/update/idle tail. It is not simply a measured half-speed animation.
- eas_DeactivateAutoFacing clears autofacing and jumps idle; the following
  unrelated action bytes are unreachable from that jump.
- eas_Jump clears autofacing, moves relative `(0,-1)`, waits5, moves `(0,1)`,
  waits5, moves `(0,0)`, restores autofacing and jumps idle. RightLeft and
  UpDown perform their named four facings, each with update/wait30, then idle.

The complete eas_TwirlGrow, lines325–368, first enables the entity's resize bit.
Each pair below is facing, shared-size write, sprite update, wait5, in order:

| Step | Facing / size |
| --- | --- |
| 1 | Left / 6 |
| 2 | Down / 8 |
| 3 | Right / 10 |
| 4 | Up / 12 |
| 5 | Left / 14 |
| 6 | Down / 16 |
| 7 | Right / 18 |
| 8 | Up / 20 |
| 9 | Left / 22 |
| 10 | Down / 24 |

Its tail disables resize, writes shared size18, and jumps **eas_Init**, not
straight to idle. A completed Init therefore writes size24 again and requests
an ordinary sprite update before idle. The intermediate18 is not a lasting
actor size. `esc0B` writes global SPRITE_SIZE; `esc1F` changes bit3 of this
physical actor's flags B. `esc0A` can defer work for sprite-load capacity.
ChangeEntityMapsprite bypasses special sprites/entity32; otherwise its resize
branch passes the current shared size to `entityfunctions_4.asm:ResizeSprite`,
which transforms decompressed mapsprite pixels for both walking frames.
These writes are not combatant body scale, a general collision-size mechanic,
party-stat restoration or persistent per-actor size. Concurrent actors use the
same size scratch word, so their installation alone does not prove identical
rendered growth.

Commands99/101/103 do not wait for any of those three actions. The map script
waits60 then5 before replacing Bowie's action, another5 before Peter's, another5
before B's, followed by120. Those sleeps are not an all-actor completion join;
load pressure, aliases, debug skip or stalled actions can change the result.
The later nods and text1868 do not prove that each growth tail reached Init.
No explicit removeShadow command or independent shadow-restoration observation
exists in this selected scene/action.

csc41 shifts each flash operand right3 and uses DBF: each4 requests one
LaunchFading, and30 requests four. The selected sequence is four single-launch
calls and one four-launch call. LaunchFading selects all palettes `$F`, maximum
counter1 and FLASH_QUICKLY_2, waits fading plus another VInt and restores the
saved counter maximum; csc41 then duplicates palettes. Operands4/30 are not
four/thirty-frame presentation guarantees. Plain quake3/4/0 use csc33's direct
QUAKE_AMPLITUDE assignment, not its high-bit ramp branches.

csc2A shiver saves shared size/animation counter, uses size21, stops animation,
and toggles resize/update with Sleep5 on/off three times, then restores the
saved size/counter. It does not restore an originally set resize bit: each
iteration clears it. csc26 nod stops animation, sleeps10, transforms/mapsprite
DMAs the nod, sleeps20, restores via sprite update, sleeps10 and sets animation
counter0. csc27 headshake runs seven alternating sprite-transformation/update
cycles with VInt waits, then sets counter0. These synchronous presentation
services can wait or alter sprite state while other entity actions continue;
they do not globally freeze motion or certify completed concurrent actions.

## Hide, follower tail, init return and stop

**Confirmed:** csc2E resolves the actor and calls
`entityfunctions_2.asm:HideEntity`. It writes current and destination coordinates
`$7000,$7000`, marks every matching ENTITY_INDEX_LIST entry -1 and removes that
physical actor from EXPLORATION_ENTITIES if present. It does not write a story
flag, change force membership or warp. This differs from setPos `(63,63)`,
which leaves the lookup/slot allocated. Creed's and Fairy's hide calls occur
only after their respective waited departure paths; Fairy speaks1897 after
Creed is hidden and before her final path/hide.

csc2C resolves both actors, maps direction index2 through table_FollowerPositions
to `(-24,0)`, and calls AddFollower for P→Bowie then B→P. AddFollower allocates
a copied 42-byte follower action template, sets its target/offset fields and
adds the actor to EXPLORATION_ENTITIES only if absent. It replaces the actor's
action and establishes a following relation, not a new recruit or immediate
teleport. Both calls use the mappings still present at that moment. The copied
eas_Follower1 in `data/scripting/entity/eas_main.asm` resets speed32/32 and
movement flags when executed, then follows through destination waits. Neither
call waits for settling; missing/aliased mappings can produce self-follow or
replace another relation. No persistent follower flag is set.

`mapscriptengine_2.asm:loc_47234` handles csc_end: if DIALOGUE_WINDOW_INDEX is
nonzero, wait for view scrolling; then clear VIEW_SCROLLING_SPEED, restore
caller registers and return. It does not poll all actor actions. The normal
init at `$5AB7C..$5AC56` then performs this complete ordered tail:

| Ordered guard / source label | Effect if selected |
| --- | --- |
| F523 set and F783 clear | Execute cs_5B016. **Only after return**, set F783, then write byte MAP_HASSAN=9 to EGRESS_MAP. |
| F22 set | cs_5ABA8: set G `(14,14)` Up; bypass the next three placement alternatives. |
| Else F784 set | Skip those placement alternatives. |
| Else F783 set | cs_5ABCC: set G `(17,10)` Down. |
| Else F774 set | cs_5ABE6: set G `(17,12)` Down. |
| Tail F784 set, independently of the earlier F22 branch | For each clear joined flag, in order: F14→cs_5AC04 Eric `(15,14)` Down; F16→cs_5AC1C Randolf `(16,15)` Down; F17→cs_5AC34 Tyrin `(18,15)` Down; F15→cs_5AC4C Karna `(19,14)` Down. |
| return_5AC54 | rts. Each tiny placement script has its own csc_end. |

The ordinary return completes the first row, skips F22/F784, now sees F783 set
and places Goliath `(17,10)` Down; F784 clear skips all four ally placements.
The completion flag does not automatically rebuild the six-row baseline
population: the seven-row actors already existed, and Creed/Fairy were hidden
explicitly. Future setup accessors can now select baseline, but that is not a
retroactive replacement of the running scene or its caller. The F784 tail's
joined flags and csc19's HP guard are distinct; clear joined alone does not
guarantee a dead ally is placed.

F783 already set on a repeat normal init skips **both** the scene and Hassan
assignment, then applies actual placement guards. A fresh repeat with ordinary
later flags clear rebuilds the baseline parked residents before placing Goliath;
it does not reproduce the first return's actor population. Merely supplying
F783 set neither proves this scene ran nor repairs a stale EGRESS_MAP. F523
clear also skips the scene/assignment even if F783 is clear; with other later
flags clear, F774 alone decides the earlier Goliath placement. Selected F786
init bypasses this whole tail. These are source discriminators, not executed
save-state scenarios.

RunMapSetupInitFunction restores its saved registers and returns directly.
Unlike the zone wrapper, it adds no portrait/text closure or physical0
coordinate-equality wait after this init. ExplorationLoop then compares base
and current palette data, conditionally calls PlayMapMusic/FadeInFromBlack,
sets movement sound and enters WaitForEvent. If MAP_EVENT_TYPE is zero,
WaitForEvent selects camera physical0 and installs the controller for the
actual PLAYER_TYPE. Pending work has priority and bypasses that installation.
The selected scene has no warp or pending-event write, but no atomic protection
against a different writer or interruption is established here.

With coherent mappings, living Bowie/Peter, completed services, no intervening
displacement and no pending event, the explicit restoration placements leave
Bowie `(16,9)`, Peter
`(15,9)`, B `(17,9)` and the caller places Goliath `(17,10)` Down; Creed/Fairy
are hidden and following relations are installed. The final explicit party
facings are Up, but resumed follower actions are live behavior. Usable player
control is conditional on the caller and controller running successfully, not
a property implied by F783 alone. EGRESS_MAP=9 selects Hassan for later Egress
logic; it neither transports the party there nor records a new position tuple.

An interrupted scene retains completed reload/placement/copy/presentation/hide
prefixes without rollback and may never reach F783 or EGRESS_MAP. Interruption
between the two caller writes can leave F783 set with the old selector; failure
in a later placement/service can leave both writes completed without reaching
exploration. No save/persistence or basement-completion claim follows. Stop
here: do not invoke a later zone, conversation, recruit choice or departure.

## Reproduction and review boundary

Load `. ./local/private-inputs.ps1` in the same process as controlled commands
and verify the assigned source HEAD/clean status. Reuse the
[retained comparison recipe](../grans-opening/battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`,
without regenerating payloads. Select `programCorpus.programs` by
`id == "cs_5B016"`.

Direct reproduction additionally checks:

- The complete Map33 selector, four pointer tables, exact seven F523 resident
  rows and changed-premise row identities; actual follower allocation, duplicate
  and class/HP consumers; jump aliases and normal/786 init entry points.
- Every retained command's sourceLine/macro/arguments against scripts.asm247–535;
  all 24 movement and six custom bodies through endActions/ac_end; the next
  source line begins cs_5B466. Review the complete named helper bodies too.
- The sequential text cursor, all 58 exact comment-to-gamescript literal/control
  joins and the table's command/speaker/modifier identities. Review actual
  text/portrait/name/debug consumers, not only comment content or totals.
- Every selected mutation/wait/hide/follow consumer, complete step/flag tables,
  TwirlGrow's Init tail, shared sprite size versus per-entity state, full caller
  placement order, write timing, and the conditional exploration return.

Review the whole document and exact owned diff, links, provenance/private
boundary and `git diff --check`. On a clean committed head run configured
`uv run sf2 verify plan --base origin/main --head HEAD` and record actual
final-head CI, exact Git identities and inspection diagnostics in #941's
handoff. No local blanket normal/full/engine/Godot/H3/H4 suite is selected
solely for this prose; no new verification machinery is introduced.

Preserve [#937 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252),
[PR939 P2 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
and [independent acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382).
Their original/corrected commits, CI receipts, unchanged-pair result and linked
predecessor failures remain retained; this slice does not overwrite them or
reinterpret completed failures as interrupted suites.

**Unknown:** natural exit reach, actual flags/population/HP/class/mappings,
mode/raft/layout/pending state, service and concurrent-action completion,
rendered growth/shadows/camera/text/audio/gestures, usable exploration control
and persistence. These limits remain conditional evidence gaps under
[ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md), not an automatic
runtime queue. The complete bounded static scene/init return is the deliverable.
