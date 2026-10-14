# Creed departure, dwarf recovery and the Cannon gift

Evidence date: 2026-10-14. **Confirmed:** selected table routes connect Creed
Mansion33 through Devil's Tail67 and Southwest Parmecia68 to mine interior30.
Map30 zone0 requires F69 set and F760 clear, runs the complete Fairy recovery
scene, calls the Cannon helper, then sets F760 and clears F69 after return.
The helper checks Bowie's four item slots; its admitted gift branch does not
check AddItem's result or F761 before narration, F761 and the digging scene.
**Inferred:** the staged recovery and gratitude express Fairy helping the dwarves
and the dwarves thanking the party. **Unknown:** natural traversal, supplied
state, completed presentation, usable control and save persistence.

## Agreement and provenance

Standard rigor under [Issue #961](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/961)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one coherent departure/recovery/gift outcome with route, population, actor,
item, flag and immediate-return boundaries. This document alone is owned.
Consume accepted [Oddler stay and Fairy handoff](./oddler-stays-and-fairy-handoff.md),
[Creed return and recruitment](./creed-return-fairy-and-recruitment-choices.md),
[Fairy Cave and Devil's Tail](./fairy-cave-exploration-and-devils-tail-return.md),
[Caravan and Southwest Parmecia](./caravan-boarding-and-southwest-parmecia-travel.md),
[map-event requests](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md) and
[item transactions](./map-event-item-transactions.md). No dependency on an
unmerged conclusion. Stop after the immediate zone/entity/helper return and
relevant fresh-entry consequences; subsequent travel, Cannon use, North Cliff,
Pacalon/Grans campaign, remake/UI, translation and shared files are excluded.

All source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The read-only source checkout is clean at that pin. Existing evidence binds USA
retail ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Unqualified setup files below mean `data/maps/entries/map30/mapsetups/`;
map/entity engines mean `code/common/scripting/map/` and
`code/common/scripting/entity/`. Constants come from `sf2enums.asm`, RAM symbols
from `sf2const.asm`. Fnnn denotes flag nnn; address intervals are half-open.

Minimum evidence is pinned source, selected existing fixtures and the retained
18,285,979-byte map-init/map-script-engine pair. Linear comparison budget is
under one minute/512 MiB, plus small local inspection receipts and this prose.
No regeneration, new framework, acquisition, runtime, installation or cleanup.
Replan a material contradiction or broader consumer dependency rather than
silently expanding the outcome. Exact joins and ordered consumers establish
static claims; neither counts nor narrative establish execution.

## Selected departure and mine-entry route

**Confirmed:** the following ordered rows in `data/maps/entries/mapN/6-warp-events.asm`
are independent table alternatives. R/U/L/D are facing0/1/2/3; any is byte255.
They do not test F759/F760/F761/F762/F785/F786 or prove terrain reach.

| Source table / selected row | Request payload: scroll, map, X/Y, facing |
| --- | --- |
| Map33 `$AC722..$AC734`, row1 `(any,31)` | no-scroll0; Map67 `(26,23)`, Up1 |
| Map67 `$BD262..$BD28C`, row4 `(63,any)` | scroll Right `$10` (16); Map68 `(1,255)`, Right0 |
| Map68 `$BDA5E..$BDA90`, row1 `(57,24)` | no-scroll0; Map30 `(8,20)`, Up1 |
| Map30 `$AB39C..$AB3A6`, sole row `(any,21)` | no-scroll0; Map68 `(57,25)`, Down3; identified only as the local return boundary |

Map33 row0 instead returns to shrine34. Map67's other rows enter mansion33,
Fairy Cave32 at either entrance, or Harpy Pond51; they are not mandatory steps
in this selected route. Map68's other rows enter shrine12, Elven Village13 or
adjacent overworlds. Map30 is `MAP_DWARVEN_VILLAGE_INTERIOR=30`, the mine reached
**directly from68**; Map29's village exterior is a distinct map, not an implied
intermediate stop. No later Grans route is inferred from these names or speech.

`exploration.asm:WarpIfSetAtPoint` divides the supplied position by384, scans
rows in order with negative coordinate bytes as wildcards, then writes event1,
parameters1–4, facing/padding at parameter5 and WARP_SFX. Actual working-layout
warp kind, controller/movement admission and the later event consumption remain
premises. `ProcessMapEvent` clears pending before dispatch. The no-scroll branch
in `explorationfunctions_2.asm:ProcessMapEventType1_Warp` submits/clears WARP_SFX,
idles physical0, unwinds exploration to MainLoop, samples current tile position,
replaces requested nonnegative coordinate **low bytes**, and returns the map,
facing and d4=0. Ordinary in-range sampled high bytes are a premise, not repaired
by the table. Neither warp path clears PLAYER_TYPE.

The Map67 border uses the different nonzero-scroll branch. It writes
CURRENT_MAP=68, calls `mapload.asm:ProcessMapTransition`, applies requested
nonnegative X/Y to physical0 current/destination and calls DeclareRaftEntity
from the resulting position/facing. It returns inside exploration, without the
no-scroll MainLoop SwitchMap/CheckBattle, fresh entity construction, temporary
flag reset or selected init. ProcessMapTransition loads blocks/layout/selected
tilesets and calls the direction-selected overworld transition; it does not
rebuild the residents/follower table. The row's Y255 retains Y and its Right
facing byte is not applied by this branch; current main facing is sampled.
After ProcessMapTransition, requested X replaces only d0's low byte before a
word multiply by384; no clear removes the returned upper byte. The ordinary X1
derivation assumes that upper byte is zero. Y's register is explicitly cleared before
its request byte is tested. The ordinary tuple is conditional, not an
arbitrary-state normalization.
Map67 and68 both have empty fixed residents and return-only init, but retained
followers and changed flags still differ from a fresh rebuild. The caller's
DeclareRaftEntity has its own mode-dependent follower/raft positioning.

No selected map33/67/68/30 appears in `data/maps/global/flagswitchedmaps.asm`.
The scroll branch's special drought-map substitution is also inapplicable.
For no-scroll admission, `getbattle.asm:CheckBattle` has no Map30 battle row;
Map67's sole row21 has wildcard triggers and F421 admission. F521 set clears
F421 **after selection**, without canceling that call's Battle21. Map68's row19
similarly uses F419/F519; the border scroll does not itself call CheckBattle.
Stop at any admitted battle rather than assuming the mine journey continues.

Later walking `$1400` zones remain independent of warp rows: Map67's passage
and call-for-help branches are owned by the accepted Fairy Cave/pond work;
its default calls CheckRandomBattle21. Map68's default calls CheckRandomBattle19.
Reuse the accepted completed-flag/counter/RNG/request consumers, including their
packed CURRENT255 reload and residual-coordinate limits. Neither a table arrival
nor this route proves a zone ran. Mode1 Caravan control lacks walking's zone
producer; the established numeric mode0/1/2 controller distinctions, F65
population selection and separate raft state still apply. No boarding/landing
or battle internals are reopened here.

On an explicit no-battle Map30 entry, ExplorationLoop clears pending, performs
its step-counter and living/immortal-ally healing, sets CURRENT_MAP30 and
CURRENT_BATTLE255, loads layout and rebuilds actors, clears only temporary
flags256–383, sets F80, then invokes selected init. F759–762/F785 and follower
flags survive that reset. CURRENT255 reuse retains population instead; accessors
reselect tables from current flags without retroactively rebuilding actors.
Init/music/fade/WaitForEvent service and pending-zero priority still separate
entry from usable control. No local EGRESS_MAP or PLAYER_TYPE write is added.

## Complete Map30 population and init

**Confirmed:** `data/maps/mapsetups.asm:99–101` is last-set-wins: default
`ms_map30`, then F785 → `ms_map30_flag785`, then F761 → `ms_map30_flag761`.
F761 wins when both are set. F69, F760 and F762 do not select the population.
All three pointer tables (`$5A176..$5A1BE`) share entity-event, zone-event,
description, item-event and init pointers. Their resident-list pointers differ:

| Setup / source interval | Logical resident, sprite226/Dwarf, initial tile/facing/action |
| --- | --- |
| Baseline / `s1_entities.asm` `$5A1BE..$5A1E0` | 128 `(6,10)` Down, walking origin `(6,10)` range1; 129 `(7,4)` Up Init; 130 `(15,12)` Down Init; 131 `(15,11)` Down Init; end. No132. |
| F785 / `s1_entities_785.asm` `$5A1E0..$5A20A` | 128 `(11,11)` Down Init; 129 `(16,12)` Left Init; 130 `(15,12)` Down Init; 131 `(15,11)` Down Init; 132 `(15,13)` Up Init; end. |
| F761 / `s1_entities_761.asm` `$5A20A..$5A20C` | end only: no residents. |

InitializeMapEntities clears actors/lookups, allocates selected followers first,
then these residents, masks resident coordinates by `$3F` and scales by384.
Residents128–132 use lookup offsets32–36; they are NPC mapsprites, not force
members or combat-HP patients. Main/Bowie is physical0. Map30 is not in the
overworld list, so F65 alone adds no Caravan here. Normal follower declarations
select Peter7 for F67, B31/Oddler for F68 and **C158/Fairy, lookup62**, for F69.
C is not resident130 (the sick dwarf). Class/HP affect ally mapsprites; HP0
outside battle can be blue flame. Follower allocation is not a joined/active
membership test or a guarantee of alive actors.

With only F67/F69 and F785, Peter is physical1, C physical2, and residents128–132
physical3–7. This matches the accepted handoff's F69-set/F68-clear selectors;
**B31 is absent and its zero lookup resolves to physical0**, not a surviving
Oddler. Thus the scene's B-follow request can act on Bowie, append physical0
to EXPLORATION_ENTITIES, and its B placement/facing can also target Bowie.
The later explicit Bowie placement follows and can overwrite B's placement.
With F68 also set, Peter/B/C are physical1/2/3 and residents4–8: B is distinct.
These examples require no other follower selectors and ordinary mode. F74/F70
can allocate B/Astral; later duplicate IDs overwrite lookups without deduplicating
physical actors. Missing selectors may alias0; hidden mappings become -1 with
no local validity repair. Actual mappings, mode, raft and list state matter.

`s6_initfunction.asm:ms_map30_InitFunction` (`$5A2F2..$5A30C`) first checks F761.
If set, run cs_5A30C; after return independently check F760. If F760 clear,
run cs_5A31E, otherwise return. This is not an if/else narrative progression:
F761 set/F760 clear runs **both**, even on the empty F761 population.

| Program / complete ordered body | Immediate meaning |
| --- | --- |
| cs_5A30C `$5A30C..$5A31E` | setBlocks `(21,6,2,2,7,5)`; setBlocks `(21,0,4,4,6,0)`; csc_end. Two layout copies. |
| cs_5A31E `$5A31E..$5A33A` | customActscriptWait130: ac_motion OFF, ac_orientRight, ac_updateSprite, ac_jump eas_Idle, ac_end; stopEntity130; csc_end. |
| cs_5A256 `$5A256..$5A25C` | shiver130; csc_end. This is the interaction's repeated shiver program, not init. |

`ac_motion OFF` is esc14's ANIMCOUNTER=-1, not a global update/movement pause.
Orientation Right is the orientation operand3, distinct from facing. The manual
action waits for130's idle pointer; stopEntity again writes animation counter-1.
Its full-word130 HP guard bypasses named allies. With F761 set/F760 clear,
missing130 can therefore redirect this work to physical0. With F760 set on a
fresh baseline/F785 population, init skips illness setup; it does not relocate
or replay the recovery. Changing F761 live selects no new actors until a rebuild.

## Six entity rows and the two-row zone table

**Confirmed:** `s2_entityevents.asm` (`$5A20C..$5A2C2`) has five specific rows
plus default, six total. UP1/DOWN3/RIGHT0 are wrapper action bits, not required
player facings. Bit0 turns the actor toward the player; bit1 restores its saved
facing after callback. Thus128 turns without restoration;129/131/132 turn and
restore;130/default do neither. All three setups share the table.

| Row PC / selector,bits | Callback range / complete branch order |
| --- | --- |
| `$5A20C` / 128,UP1 | Map30_EntityEvent0 `$5A224..$5A236`: F760 clear → txt1705, return; set → bsr.w sub_5A278, return. No F761 guard. |
| `$5A210` / 129,DOWN3 | Map30_EntityEvent1 `$5A236..$5A240`: txt1706; set759; return. |
| `$5A214` / 130,RIGHT0 | Map30_EntityEvent2 `$5A240..$5A256`: script cs_5A256; txt1707; script cs_5A256; set759; return. |
| `$5A218` / 131,DOWN3 | Map30_EntityEvent3 `$5A25C..$5A272`: F761 clear → txt1708, set759, return; set → txt1710, return. |
| `$5A21C` / 132,DOWN3 | Map30_EntityEvent4 `$5A272..$5A278`: txt1709; shared default rts. |
| `$5A220` / default253,0 | Map30_DefaultEntityEvent `$5A276..$5A278`: rts. Its byte range overlaps the preceding callback's tail. |

These callbacks do not gate illness on F760: a direct post-recovery130 call
still shivers and speaks cold;129 still reports sickness;131 chooses by761 only.
Baseline has no132 and fresh761 has no resident rows, despite retained callback
addresses. A logical callback's existence is not a natural interaction proof.
The ordinary activation path depends on facing/view target, range/layout,
EXPLORATION_ENTITIES exclusions and inverse lookup order. The entity wrapper
resolves actual sprite portrait/speech, disables entity updates around callback,
then restores facing where requested, closes windows and reactivates updates.
Trap6's script entry reactivates updates, permitting the action services.

The shared `s4_descriptions.asm` is only rts (`$5A2E8..$5A2EA`).
`s5_itemevents.asm` (`$5A2EA..$5A2F2`) has only a default row and return-only
Map30_DefaultItemEvent0. Neither is another recovery/gift trigger; item-use
and generic search services retain their existing separate boundaries.

`s3_zoneevents.asm` has `(7,13)` at `$5A2C2` → Map30_ZoneEvent0 `$5A2CA`, and
default `$FD,0` at `$5A2C6` → `$5A2E6` rts. The complete zone0 body is:
chkFlg69; beq default; chkFlg760; bne default; script cs_5A33A;
bsr.s sub_5A278; setFlg760; clrFlg69; shared rts at `$5A2E6`.
There is **no759/762/785/761, HP, inventory or population guard**. F785 supplies
the usual five-resident arrangement but does not admit the scene. Direct script
invocation omits caller guards/helper/flag writes.

Walking's zone producer uses working-layout `$1400` and destination plus movement
offsets/384; type6 idles main via Init, passes coordinate words, and the wrapper
compares low bytes/first matching row after selecting current setup. After any
callback return it closes portrait/text, waits one VInt and physical0 position/
destination equality. This is not an all-actor/action barrier or a control grant.
An unreturned script/helper prevents later caller flags and wrapper completion.

## Complete recovery scene

**Confirmed:** `scripts.asm:4–125`, cs_5A33A `$5A33A..$5A4FE`, contains90 top-level
records. Below C=FOLLOWER_C158, B=FOLLOWER_B31, P=Peter7, W=Bowie0.
D/U/L/Rn means relative movement by n tiles; S=nextSingleText, T=nextText.
Every listed inline movement body ends with endActions. Wt denotes the waited
entityActionsWait form; an explicitly unawaited body uses entityActions.

| Source lines / indices | Complete ordered commands and embedded actions |
| --- | --- |
| 4–13 / 0–6 | cursor1711; camera destination `(0,7)`; C InitSlow waited; follow B→P direction2; C custom waited body ac_setSpeed0,0; ac_jump eas_Idle; ac_end; C destination `(8,11)`; C:1711 S. |
| 14–29 / 7–18 | sleep5; C 2xRightLeft unawaited; sleep50; camera `(5,7)`;128 Wt L1; C Right;128:1712 S;128 Jump waited twice;128 Wt R3;128 Left;128:1713 S. |
| 30–45 / 19–30 | camera follows C; C Wt R4,D1,R2; camera `(10,7)`;128 Right; sleep60; C:1714 T; C:1715 S; sleep50; C shiver; sleep30; C shiver; sleep20. |
| 46–62 / 31–45 | C flash120; fade out white; C flash100; fade in white; sleep50; sleep5;129 2xRightLeft unawaited; sleep100;129 Wt D1;129 Left;129:1716 S; sleep15;132 headshake;132:1717 S; sleep60. |
| 63–79 / 46–60 |130 Init waited;130 Jump waited; startEntity130;130:1718 S; sleep5;130 2xRightLeft unawaited; sleep100;130 Up;130:1719 S;129 Wt U1;129 Left;130 Right;129:1720 S;130:1721 S; sleep20. |
| 80–96 / 61–75 |128 Wt R1;128 Down;129 Left;130 Left;131 Left;132 Left; C Up;128:1722 S;130 nod; C:1723 T; C:1724 S; sleep15; B `(7,11)` Right; P `(7,10)` Right; W `(8,11)` Right. |
| 97–112 / 76–83 | camera follows C; C Wt L2,U1,L3; C:1725 S; camera neutral; C Wt U2,L3,D3; W Down; P Down; B Down. |
| 113–125 / 84–89 | C Wt R1,D5; hide C; camera `(4,7)`; W unawaited R2;128 Wt L3; csc_end. |

With the F785 residents and ordinary distinct/serviced mappings,128's declared
`(11,11)` becomes `(10,11)`, `(13,11)`, `(14,11)`, then `(11,11)`;
129 moves `(16,12)`→`(16,13)`→`(16,12)`. C's explicit `(8,11)` destination
then relative bodies give `(14,12)`→`(9,11)`→`(6,12)`→`(7,17)` before hide.
These derive requested paths only. The scene ends after128's last waited body,
not W's unawaited R2 pointer; Bowie's later helper Init can replace that pointer.
The zone wrapper's eventual physical0 coordinate wait is also not that pointer's
completion. Neither recovery text nor startEntity writes combatant HP, revives
an ally or changes force membership.

## Cannon helper and complete digging scene

**Confirmed:** `s2_entityevents.asm:sub_5A278` (`$5A278..$5A2C2`) is called by
zone0 immediately after recovery, or by128 when F760 is set. Complete order:

1. txt1726; txt1727. Set d0=0 and d1=0; call
   j_GetItemBySlotAndHeldItemsNumber; compare **word d2 with4**.
2. Equality: txt1729; branch to return `$5A2C0`. No item call, music,761 or
   digging scene. The zone caller still sets760/clears69 after this return.
3. Inequality: d0=0, d1=ITEM_CANNON `$72`; call j_AddItem. There is no result
   test. Request MUSIC_ITEM27; txt1728; Sleep100; clsTxt; set761;
   request SOUND_COMMAND_PLAY_PREVIOUS_MUSIC; script cs_5A4FE; return.

The pinned s02 jump interfaces resolve both j_ names to the implementations in
`code/common/stats/itemstats.asm`. GetItemBySlotAndHeldItemsNumber reads slot0's
entry into d1 and counts all four non-NOTHING item indices (masked) into d2;
slot0 is not required to be free. AddItem scans slots in order for the first
masked NOTHING, stores the item with index/broken-bit mask and returns d2=0;
if none is free it returns1. Both use combatant0/Bowie, not whoever LEADER text
names, the active party's first member, an NPC lookup or a party-wide capacity.
Under unchanged valid inventory a count below4 supplies a free slot, but the
caller does not verify AddItem's returned result or roll back subsequent work.
It checks neither existing Cannon custody nor HP/class/membership nor F761.

Consequently a repeated direct helper or live128/F760 callback can give another
Cannon if Bowie has space, even with761 set; if full it gives1729 again. Fresh761
entry removes residents through table selection, which limits ordinary access
without being a universal one-time transaction guard. A failure/interruption
after AddItem can leave the item before761; after761 it can leave partial digging
before the zone's760/69 writes. There is no atomic item/scene/flag transaction.

`scripts.asm:126–217`, cs_5A4FE `$5A4FE..$5A646`, contains46 top-level records.
All movement bodies below include their endActions terminator; unmarked action
installs are explicitly unawaited. It neither warps to Grans nor hides dwarves.

| Source lines / indices | Complete ordered commands and embedded actions |
| --- | --- |
| 126–137 / 0–9 | cursor1730;129 Left;130 Left;131 Left;132 Left; camera `(6,7)`;128:1730 S;128 Wt R1;128:1731 S;128:1732 S. |
| 138–150 / 10–20 | Install Jump on129,130,131 then wait132 Jump; repeat that four-install sequence; wait W Init; W Wt L1; P destination `(8,10)`. |
| 151–168 / 21–28 |128 unawaited L2;129 unawaited L1;130 unawaited L1;131 unawaited L1;132 Wt L2; W Right; P Right; B Right. |
| 169–199 / 29–33 |128 unawaited U2,L1,U3;131 unawaited L3,U2,L1,U3;132 unawaited U2,L3,U2,L1,U3;129 unawaited U1,L5,U2,L1,U3;130 Wt eaWait40,U1,L4,U1. |
| 200–210 / 34–38 | sleep30;130 Wt D1;130 Left;130 nod;130 Wt U2,L1,U3. |
| 211–217 / 39–45 | place128 `(7,3)` Up;129 `(8,3)` Up;130 `(7,4)` Up;131 `(8,4)` Up;132 `(7,5)` Up; setBlocks `(21,6,2,2,7,5)`; csc_end. |

Waiting132 after each group of jumps/moves does not wait129/130/131/128.
Only130's last path is explicitly waited before the five placements. Those
placements set current and destination but do not clear other action pointers;
remaining earlier actions could still run under changed timing/mappings.
The final copy affects current layout only. Fresh761 init repeats that2x2 copy
**and adds** the4x4 copy `(21,0)`→`(6,0)`; the live scene does not issue the latter.
No later route, collision aperture or visibly completed excavation is proved.

## Exact text and immediate services

**Confirmed:** source literals join `data/scripting/text/gamescript.txt`
hex keys06A9–06C4 to IDs1705–1732. The table paraphrases content; controls are
exact and ordered. T/S are cursor forms above; txt uses an explicit ID.
R=resident under the selected mapping; helper speech inherits context.

| ID / form / actual logical speaker | Content | Controls |
| --- | --- | --- |
| 1705 / txt / R128 | Came from Grans for Mithril | `{N}{W1}` |
| 1706 / txt / R129 | Earthquakes blocked tunnel; sick friend needed | `{N}{W2}{N}{N}{N}{W1}` |
| 1707 / txt / R130 | Shivering and cold | `{N}{W1}` |
| 1708 / txt / R131 | One dwarf went to seek Fairy | `{N}{W2}{N}{W1}` |
| 1709 / txt / R132 | Digging song toward Grans | `{N}{W2}{N}{N}{W1}` |
| 1710 / txt / R131 | Invitation to Dwarven Village on Grans | `{N}{N}{W1}` |
| 1711 / S / C | Asks for sick dwarf | `{W1}` |
| 1712 / S / R128 | Welcomes Fairy | `{N}{W1}` |
| 1713 / S / R128 | Asks for examination | `{W1}` |
| 1714 / T / C | Hopes arrival is timely | `{W2}` |
| 1715 / S / C | Begins examination | `{W1}` |
| 1716 / S / R129 | Asks how patient is | `{W1}` |
| 1717 / S / R132 | Does not know yet | `{W1}` |
| 1718 / S / R130 | Exclamation | `{W1}` |
| 1719 / S / R130 | Question | `{W1}` |
| 1720 / S / R129 | Asks whether patient is OK | `{W1}` |
| 1721 / S / R130 | Says he feels fine | `{N}{W1}` |
| 1722 / S / R128 | Asks how to thank Fairy | `{W1}` |
| 1723 / T / C | Declines personal reward | `{W2}` |
| 1724 / S / C | Asks them to help LEADER | `{N}{LEADER}{W1}` |
| 1725 / S / C | Farewell and safe-journey wish | `{LEADER}{W2}{N}{N}{W1}` |
| 1726 / txt / helper | Credits recovery to addressee | `{W2}` |
| 1727 / txt / helper | Offers gratitude and gift | `{N}{W1}` |
| 1728 / txt / helper | Narrates LEADER receiving Cannon | `{LEADER}{N}{W1}` |
| 1729 / txt / helper | Full hands, deferred gift | `{N}{W1}` |
| 1730 / S / R128 | Cannon destroys large rocks | `{N}{W1}` |
| 1731 / S / R128 | Calls dwarves back to Grans | `{N}{N}{W2}` |
| 1732 / S / R128 | Orders digging | `{W1}` |

All scene T/S operands use modifier0. Actual Fairy mapsprite219 has no portrait
and bleep1; Dwarf226 has no portrait and bleep6 (`spritedialogproperties.asm`).
An entity callback's initial txt uses the wrapper-resolved speaker. The helper
has no explicit portrait/speech resolver: the128 interaction supplies its
context, whereas the ordinary unskipped zone chain follows C's last1725 text,
so its first direct txts inherit that remaining speech context, not a new128 attribution. The
later cs_5A4FE explicitly selects128. No named speaker is encoded in helper
1728's LEADER substitution. LEADER uses UpdateForce/first active name, which
can differ from fixed recipient Bowie. Changed sprites/aliases change speech.

csc04 sets cursor; normal csc00/02 display then increment (nominal final1726 or
1733 respectively). csc00 closes portrait/text and sleeps10; csc02 leaves it
open. Both wait scrolling. ExecuteMapScript clears SKIP_CUTSCENE_TEXT on entry;
debug/P2 Start can latch it so skipped cursor commands consume operands without
display or increment; the same latch skips encoded csWait sleeps. Literal txt
calls and the helper's direct Sleep100 are separate paths, not csc00/02 or csWait.
N is newline and W1/W2 input wait; W2 has continuation/validation-sound behavior.
The internal W2 in single1725 and terminal W2 in single1731 remain meaningful;
there is no yes/no choice. Input, rendered content and elapsed time are unproved.

## Action, layout and return semantics

**Confirmed:** the selected map-script consumers and complete action bodies
preserve the following distinctions:

- csc14/custom and csc15/action-pointer installs seed the physical-index wait
  timer and optionally poll only that actor's pointer for eas_Idle. Custom
  bodies end with ac_end `$8080`; their ac_jump eas_Idle is the executing tail.
  InitSlow supplies speed16/16, acceleration factors1/1 and acceleration/
  deceleration operands-2,ON, normal collision/autofacing, Up orientation,
  immersion off, size24/update/idle. Init supplies factors0/0, speed32/32,
  acceleration/deceleration off and the same remaining settings. Neither resets
  position or removes a prior EXPLORATION_ENTITIES entry. C's custom speed0/0
  replaces the action pointer; its following setDest writes travel/velocity32
  and waits current/destination equality because the actor word has no bit15.
  Peter's gift-scene setDest behaves likewise, without a named-ally HP check.
- eas_Jump is complete: autofacing off; ac_moveRel `(0,-1)`; wait5; `(0,1)`;
  wait5; `(0,0)`; autofacing on; jump idle. eas_2xRightLeft sets Right/update/
  wait30, Left/update/wait30, then repeats and jumps idle. Unawaited installation
  plus a fixed csWait does not guarantee completion before a replacing action.
- csc2D compiles inline bodies into action RAM, clears flagsA bits5/6, appends
  an idle jump and optionally waits for that one pointer. Its move operands
  scale relative tiles by384 through esc04 and retain collision/destination
  conflict behavior; eaWait40 is an action wait, not40 movement tiles.
  csc19/setPos and csc23/facing use **byte** actors and skip HP0 named allies
  below30. Init/custom/inline actions, setDest, flash, gestures and hide have
  no corresponding named-actor HP guard. startEntity/stopEntity use full-word
  actors and write animation counter0/-1; for NPC130 they do not test Bowie.
- csc2C reads the follower **word's high byte** for its HP guard. B31 is encoded
  `$001F`, hence it tests Bowie0 before decoding B/P, not B's HP or membership.
  On admitted follow, direction2 selects `(-24,0)` and AddFollower copies its
  42-byte action, installs the physical leader/offsets and appends the follower
  if absent from EXPLORATION_ENTITIES. It sets no F68/F69. With B's zero lookup,
  this can be physical0 following Peter; HP0 Bowie skips the relation, but does
  not skip the rest of the scene or caller flags after a normal return.
- csc2E/HideEntity puts C current/destination at `$7000,$7000`, invalidates all
  matching lookups to-1 and removes the physical actor from the exploration
  list. It does not erase the action record or change force membership/flags.
  Ordinary entity service skips current X at least `$7000`, leaving a dormant
  slot. F69 is cleared only later by the zone caller; hiding alone is not a
  persistent follower-table edit.
- csc18 flash uses duration shifted right2 as a DBF counter:120 requests31
  cycles,100 requests26. Each cycle sets flagsB bit2/update, waits two VInts,
  clears bit2/update and waits two VInts; no initial-bit restoration. These
  are124/104 WaitForVInt calls, not proven wall time or rendered flashes.
  csc3F/40 call LaunchFading with palette mask1, counter1 and OUT_TO_WHITE/
  IN_FROM_WHITE; it waits FADING_SETTING zero plus one VInt, restores the prior
  counter maximum and sets palette mask15. This joins fading service, not actors.
- Shiver saves size/animation, uses size21 and three resize-on/update/Sleep5,
  resize-off/update/Sleep5 cycles, then restores size/animation (not an original
  resize bit). Headshake132 stops animation, performs seven paired transform/
  DMA/update cycles with six VInt waits each, then counter0. Nod130 stops
  animation, Sleep10, transform/DMA, Sleep20, update, Sleep10, counter0.
  These are requests/services, not medical state changes or measured gestures.
- csc32 disables camera following, sets tile destination and waits scrolling;
  csc24 selects C or CAMERA_NEUTRAL. They do not teleport player or join all
  actor motion. The last camera destinations are `(4,7)` for recovery and
  `(6,7)` for digging. csc34/CopyMapBlocks copies the requested rectangles within
  the current 64-wide working layout and marks both view-plane update bits.
  It does not itself rebuild actors, set story flags or prove the next rendered
  frame/collision route. Live copy and two fresh-init copies remain distinct.

csc_end conditionally waits view scrolling when the dialogue-window index is
nonzero, clears VIEW_SCROLLING_SPEED, restores registers and returns to its
caller, not directly to an acknowledged controller state.
Recovery→helper→760→clear69 is the zone order. A full inventory returns before
761/digging, yet still reaches760/clear69; a later128 interaction can retry the
gift with F760 set and room residents retained. A successful admitted helper
sets761 **before** digging; it does not clear69 or set760 itself. An entity-call
helper returns through the entity wrapper, which has no zone-style physical0
coordinate wait. An existing761 does not short-circuit either helper or a
69-set/760-clear zone. Missing residents under that supplied combination are
not repaired and may alias main. The zone/helper do not write F759/F762/F785;
the separate illness entity callbacks above do set F759.

**Inferred:** recovery is expressed by130's changed orientation/animation/actions
and self-report, followed by gratitude and a gift narrative. **Confirmed:** no
combat-HP restoration, elapsed-time requirement, universal one-time award,
party-capacity fallback, automatic Grans journey or save transaction follows
from that narrative. Full/available inventory, direct repeat, supplied761,
69/760 mismatch, baseline/F785/empty populations, absent B/C/residents, dead
Bowie/Peter, changed membership/class/mode, aliases and interrupted service all
retain the actual branches above rather than being normalized to a happy path.

**Unknown:** natural route/zone/interaction reach; actual flags/layout/mode/HP/
class/inventory/membership/population/lookups/list; actual AddItem result;
collision/action/text/portrait/view/fade/audio completion; visible recovery,
usability of control and save/load persistence. These limits are not an emulator
or acquisition queue. Subsequent travel and Cannon use remain outside this owner.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same PowerShell process as every
controlled command. Reuse the [retained pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`.
The comparison completed in0.081s with all18,285,979 bytes and mtimes unchanged.
The source and existing records can reproduce this document without generating
new schemas, fixtures, source dumps or runtime observations:

1. Select Map30's three routes in retained map-init, all source bodies in its
   setup directory, all three population lists, complete follower declarations
   and both Map30 routing tables. Join exact lines/operands/order to source.
   Inspect the four selected warp rows and scroll/no-scroll consumers separately.
2. Select Map30's six entity contexts and two zone contexts in
   `tests/fixtures/h2/map-events/entity-programs.json`, `zone-programs.json`
   and `routing-setup.json`. Preserve shared rts overlap and exact branch polarity.
   Read the complete helper directly through its return, since table-target
   fixture coverage does not make it a separately selected callback.
3. Select cs_5A256/30C/31E/33A/4FE from retained
   `local/derived/map-script-engine-static.json:programCorpus.programs`.
   Join every top-level record and every inline/custom body through its
   terminator, encoding and immediate consumer. The two large scenes have90/46
   records; all five selected programs total144 records, plus25 inline bodies
   (50 action operands and25 terminators) and two complete custom bodies.
   Inline bodies are additional bytes, not additional top-level records.
4. Join all28 text sites1705–1732 to exact source literal, ID, form, speaker
   operands and ordered controls. Resolve inherited helper speech separately
   from explicit scene selectors and LEADER separately from recipient0.
   Inspect item capacity/AddItem result handling and all selected consumers above.
5. Review the complete document/diff, links/anchors, UTF-8 and sole-path/private
   boundary; run `git diff --check`. On a clean committed candidate run
   `uv run sf2 verify plan --base origin/main --head HEAD`; record the plan and
   actual exact-head CI in the Issue handoff. Documentation-only local acceptance
   does not launch blanket normal/full/engine/Godot/H3/H4 suites.

Retain [#957's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/957#issuecomment-6178978855)
and [PR959 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/959#issuecomment-6179038849),
including exact Git/CI identities, pair receipt, author corrections and inspection
diagnostics. Their linked #953/#949/#945/#941/#937 results, completed failures
and Unknowns, plus [#954's callback correction](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171),
remain intact. No completed failing suite is reclassified as interrupted or rerun.
Freeze a clean pushed Draft PR for independent main-gate acceptance; no merge,
archive, cleanup or new Issue is part of this slice.
