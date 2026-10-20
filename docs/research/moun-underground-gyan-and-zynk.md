# Moun underground: Gyan joining and Zynk following

Continue with [Moun town, the painter and the Arm of Golem](./moun-town-painter-and-golem-arm.md).

Evidence date: 2026-10-20. **Confirmed (static):** Map39 has two setups selected
by flag830. The initial zone scene formally joins Gyan24; the separate Zynk26
entity interaction installs following and then sets76/836, without joining
Zynk to the force. Changing830 changes subsequent setup queries, not the
population already loaded. Both return warps are unconditional declarations.
**Unknown:** natural walking/interaction reach, completed services, actual
incoming state, presentation, usable control and saved persistence. This is
content research, not a playable underground implementation.

## Agreement and source route

Standard rigor for [Issue #1094](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1094)
under [#1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
describe the complete underground content and state boundaries for later
integration. Standard rigor fits the coupled setup, loaded-population,
caller, membership and inventory authorities. Accepted base
`d28248ed53353b0b09289f9c16f2226c0e045616` includes
[Battle33 outcomes and the two underground entrances](./battle33-outcomes-and-zynk-handoff.md).
Own this document and a navigation-only addition there. No stacked dependency,
shared registry/index/schema/fixture, design or implementation change.

Begin with the two Map31→39 requests, cover both setups, inhabitants, zone and
entity handlers, all selected scene/action/text dependencies, two optional
items and both Map39→31 returns. Include the immediate changed Map31 setup
consequence; stop before town shops, painter, Arm of Golem, later travel or
another encounter. Source review controls over narrative comments.

Original paths below are relative to `disasm/` at clean, read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
USA retail identity is SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new ROM/H1/runtime equality is claimed. Constants come from `sf2enums.asm`
and `sf2const.asm`. Fnnn means flag nnn, ordinals are zero-based and address
intervals half-open. `map39/` abbreviates `data/maps/entries/map39/`,
`map/` and `entity/` abbreviate `code/common/scripting/map/` and `entity/`.

Reuse [common maps](./common-maps.md), [common scripting](./common-scripting.md),
[request production](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[field search](./field-search-control.md) and its
[inventory/rollback detail](./elven-village-conversations-and-local-exploration.md).
The unchanged 18,285,979-byte init/script pair and `local/issue1005/pair.txt`
provenance supply existing structured joins. Direct bounded linear source,
table, selected-script/action/text and consumer checks plus selected in-memory
lowerings budget less than one minute/512MiB per run and small ignored
`local/issue1094/` receipts. No regeneration, new corpus/platform, runtime,
SDK/Godot/emulator, screenshot, old comparison or blanket suite. Reassess a
material contradiction or supporting-tool expansion before continuing.

## Arrival, map profile and setup selection

**Confirmed:** unchanged predecessor requests enter39 at `(3,21)` Right0 from
Map31 `(16,8)`, or `(14,20)` Left2 from `(27,7)`. Their declarations do not
test830/833/76 or membership. The predecessor owns rock/layout admission and
request sampling; neither declaration proves natural reach.

MainLoop's explicit-map path calls SwitchMap then CheckBattle. The full
flag-switched-map table has no39 substitution and the battle-coordinate table
has no Map39 row. An intact explicit39 request therefore selects exploration,
not an encounter. `explorationfunctions_2.asm:ExplorationLoop` clears pending,
subtracts20000 from STEP_COUNTER with signed BGE (N==V, else zero), heals
living/immortal allies, fades, sets CURRENT_MAP39/CURRENT_BATTLE255, loads
tilesets and reconstructs entities/mapsprites. It clears temporary256–383,
sets80, loads the map, installs base VInts and runs the separately selected
init. These are ordered calls, not proof of their observed completion.

| Map39 owner / interval | Complete selected declaration |
| --- | --- |
| `00-tilesets.asm` `$B0736..$B073C` | Palette0; tilesets0,8,48,37,58. |
| `2-areas.asm` `$B0764..$B0784` | One area: main0,0 through25,26; secondary foreground/background0,0; both parallax256,256; both autoscroll0,0; type255; MUSIC_TOWN; terminator. |
| `3-flag-events.asm` `$B0784..$B0786` | Terminator only; no flag layout copies. |
| `4-step-events.asm` `$B0786..$B0790` | One door copy, trigger4,9: source26,0 size1×1 →4,9; terminator. |
| `5-roof-events.asm` `$B0790..$B0792` | Terminator only. |
| `6-warp-events.asm` `$B0792..$B07A4` | Two eight-byte no-scroll rows and terminator; returns below. |
| `7-chest-items.asm` `$B07A4..$B07A6` | Terminator only. |
| `8-other-items.asm` `$B07A6..$B07B0` | Two four-byte ground-item rows and terminator; grants below. |

Door copying requires the ordinary control consumer's `$0400` layout kind;
after the copy it rereads the resulting kind. Warp and zone checks require
`$1000` and `$1400` respectively. Collision/obstruction and layout meaning are
separate from event coordinates. No selected block rendering or collision
decode is supplied. Empty flag/roof/chest tables do not imply an empty map.

`data/maps/mapsetups.asm` declares default ms_map39 and one override,
830→ms_map39_flag830. `GetCurrentMapSetup` reads live CURRENT_MAP and flags on
each call. The two six-long tables are `$5DF84..$5DF9C` and
`$5DF9C..$5DFB4`:

| Pointer / offset | 830 clear | 830 held |
| --- | --- | --- |
| Entities /0 | ms_map39_Entities | ms_map39_flag830_Entities |
| Entity events /4 | ms_map39_EntityEvents | Same |
| Zone events /8 | ms_map39_ZoneEvents | ms_map39_flag830_ZoneEvents |
| Area descriptions /12 | ms_map39_AreaDescriptions | Same |
| Item events /16 | ms_map39_Section5 | Same |
| Init /20 | ms_map39_InitFunction | ms_map39_flag830_InitFunction |

Only these three pointers differ. Neither Gyan's joined24/active56 nor
Zynk's joined26/active58/follower76/836 selects a setup. Population, init and
later event dispatch can query different setups if830 changes between them.
No flag write here automatically reloads entities, layout or scripts.

## Loaded inhabitants and conditional initialization

**Confirmed:** `s1_entities.asm` (`$5DFB4..$5DFFE`) declares Gyan24 `(22,7)`
Down3 and Zynk26 `(63,63)` Down3, both eas_Init, followed by the seven rows
below. `s1_entities_830.asm` (`$5DFFE..$5E038`) contains just those seven rows.
Both end with msEntitiesEnd. The ally IDs do not consume the nonally aliases:

| Event identity | Mapsprite / initial X,Y / facing | Action |
| --- | --- | --- |
|128|YOUNG_MAN /3,6 /Right0|eas_Init|
|129|MERCHANT /9,7 /Down3|Walking origin9,7 radius1|
|130|WORKER /11,10 /Down3|Walking origin11,10 radius1|
|131|BOY /12,6 /Right0|eas_Init|
|132|GIRL /13,6 /Left2|eas_Init|
|133|YOUNG_WOMAN /15,8 /Down3|Walking origin15,8 radius2|
|134|YOUNG_WOMAN /17,8 /Left2|eas_Init|

`InitializeMapEntities` clears/rebuilds aliases and population, declares
flag-admitted followers before these rows, then main physical entity0.
An ally row is skipped when its alias is already nonzero; ordinary followers
can therefore suppress a fixed Gyan/Zynk declaration. Nonallies128..134 map
to physical slots after admitted followers/allies, not necessarily128..134
physical slots. Ally mapsprites come from current HP/class: dead exploration
allies use BLUE_FLAME, so an ally label does not guarantee its usual portrait
or appearance. Joined/active flags do not themselves create ordinary followers.
The shared follower table admits Peter with67, Frayja with82, Zynk with76;
FOLLOWER_B31 has the independent74/68/70 entries. Gyan has no ordinary
follower-table row. Map39 uses the town follower table, not the overworld one.

The selected fixed/new/zone-init dependency eas_Init (`$460CE`) sets
acceleration factors0,0, speeds32,32, acceleration/deceleration OFF,
entityObstructable ON, entityUncollidable/mapUncollidable OFF, autoFacing ON,
orientUp, immersed OFF, size24, sprite update, then jumps eas_Idle. Idle waits1
and branches back; it is not an unparsed fall-through into the next action.
The three msWalkingEntity rows patch the50-byte eas_Walking template's
origin/radius at22/24/26: wait30, speed0,0, acceleration factors1,1,
acceleration/deceleration−2,ON, entityObstructable/mapUncollidable/
entityUncollidable ON, randomWalk, waitDest, wait20, branch to randomWalk.
The random consumer bounds candidates by origin±radius, draws cardinal
directions with range4, checks layout/other destinations, retries through
its DOWN=3 DBF counter and can yield without moving. Submitted destinations
still require entity/motion services; no particular random path is claimed.

Base init (`$5E0DE..$5E0E0`) is RTS. Held830 init (`$5E0E0..$5E0EE`) checks836:
held returns; clear executes cs_5E0EE (`$5E0EE..$5E0F8`), exactly
`newEntity ALLY_ZYNK,4,12,UP,NOTHING_BYTE; csc_end` (two commands,10 bytes).
It does not set836 or76. This is independent of whether Zynk is joined/dead
or a follower already. With supplied76 and clear836, an ordinary follower can
already exist before this new allocation replaces its ally26 alias.

`csc2B_initializeNewEntity` reads a word identity, four coordinate/facing/sprite
bytes and supplies eas_Init. `InitializeNewEntity` replaces the supplied255
sprite with GetAllyMapsprite for nonnegative ally26, scans63 **signed alias
bytes** for the maximum positive slot (not a free-slot search), allocates
max+1 and overwrites alias26. It converts tiles to384-unit coordinates,
declares and requests properties/sprite work; it neither removes a previous
physical26 nor repairs the exploration follower list. No capacity, already
present, membership or HP admission guard exists in that original helper.
Changed aliases/large allocations and incomplete services remain relevant.

## Event callers and ordinary conversations

**Confirmed:** Gyan's trigger is the initial setup's sole zone row `(18,7)`.
Ordinary walking `esc02_controlCharacter` produces event6 only for a candidate
layout kind `$1400`, after obstruction and the door/warp checks. It writes
type6 before PARAM1/PARAM3 coordinate words derived from destination plus
direction offsets by signed division384. ProcessMapEvent clears the stored
type before dispatch; type6 installs main eas_Init and calls
RunMapSetupZoneEvent with the current payload. That dispatcher reselects the
live setup and matches X/Y, then default. It closes portrait/text and always
waits one VInt then main physical arrival. A zone row alone proves neither
walking admission nor an automatic trigger at map load.

Map39_ZoneEvent0 tests830: held returns, clear executes cs_5E0F8 and only
**after its return** sets830. Held830's zone table (`$5E0CE..$5E0D4`) has only
its RTS default. Base default is also RTS. Thus a repeated/changed request
uses the then-live setup/flag, while the physical population stays loaded.

Zynk's handler is the **entity event**, not a second zone or warp callback.
The real ordinary caller is `ProcessPlayerAction` (`$25AD6..$25BFC`): idle
main, wait for main movement and view, A→field menu; ordinary C bypasses
the Caravan/debug branches, calls GetActivatedEntity, then GetEntityEventIndex
and j_RunMapSetupEntityEvent for a nonnegative result. No entity selects
CheckArea and possibly the menu. GetActivatedEntity scans48 physical entities,
skips the view target and entries in EXPLORATION_ENTITIES, and selects the
first within Manhattan distance <384 of the faced sample; `$2800` counters
extend the sample. Preserve its actual nonzero-view address arithmetic rather
than assuming main0 in arbitrary supplied state.

Comment-free caller audit resolves both jump aliases to RunMapSetupEntityEvent:
one external call to j_RunMapSetupEntityEvent, zero to its `_0` alias, and one
direct internal call from `sub_474EE`, which explicitly substitutes Map1
GALAM_INTERIORS and event0. The two interface JMP definitions are not gameplay
callers. GetEntityEventIndex has one direct ordinary call here and zero
j_GetEntityEventIndex calls; its reverse alias scan returns the first match
(with a fallback0), so duplicate/overwritten aliases matter. No ordinary
automatic exit-interception caller is established. A blocking actor, a
player C interaction and the comment about stopping departure are distinct.

`s2_entityevents.asm` (`$5E038..$5E0B2`) is shared by both setups:

| Identity / packed event byte | Handler / complete ordinary text selection |
| --- | --- |
|26 /Right0|Event0: clear830→RTS; held830→cs_5E20A, then set76, then set836. No836 guard.|
|128 /Up1|Event1:2043, resentment toward Pacalon.|
|129 /Up1|Event2:2044, deaths in the attack.|
|130 /Up1|Event3:2045 then2046, devils defeated and relief at going outside.|
|131 /Down3|Event4:2047, claims courage.|
|132 /Down3|Event5:2048, contradicts the boy.|
|133 /Up1|Event6:2049, Zynk hid the survivors.|
|134 /Up1|Event7: clear830→2050 then2051, Gyan behind her/guarded west gate; held830→2052, his fighting there.|
|Default /0|Map39_DefaultEntityEvent is RTS, including unmapped Gyan24 interaction.|

These direction-named operands are **packed event control bits**, not facing
requirements. RunMapSetupEntityEvent opens the portrait/sets speech from the
reverse-resolved actor, bit0 faces it opposite caller-facing after a VInt,
bit1 restores its old facing at return. It clears entity-update VInt before
handler dispatch; script Trap6 reactivates it. It closes windows and restores
entity services afterward. Zynk's byte0 applies neither face/restore bit;
128/129/130/133/134 use bit0,131/132 use both. Even an RTS default can have
dispatcher presentation/closure effects.

Descriptions (`$5E0D4..$5E0D6`) are RTS; no handled description is installed.
Section5 (`$5E0D6..$5E0DE`) has only default item-event RTS. There are no
selected map-specific item-use effects, prompt/choice or merchant/shop call;
the merchant mapsprite here has ordinary dialogue2044.

## Gyan's complete zone scene and formal recruitment

**Confirmed:** cs_5E0F8 occupies `$5E0F8..$5E20A`:56 top commands encode242
bytes, plus32 inline movement bytes. The eight streams each contain one
move and endActions. The following ranges cover every selected command:

| Ordinals | Ordered effects |
| --- | --- |
|0–7|Cursor2053; camera→Gyan; Bowie destination19,7; Peter Down1/wait; FOLLOWER_B position19,8 Right; Peter18,8 Right; Bowie Right; Gyan Left.|
|8–16|Nine single texts2053–2061, alternating Gyan/FOLLOWER_B, ending with Gyan.|
|17–25|B Down1, Bowie Down1, Gyan Left4, Bowie Up1, B Up1 (all wait); Bowie Left, Peter Up, Gyan Right; wait10.|
|26–34|Gyan2062, Peter2063; Gyan Down; Gyan2064; Bowie Right1/wait, Bowie Left; B Up1/wait, B Left; Gyan Right.|
|35–41|B continued2065 then single2066; Gyan Left then Right; Gyan single2067; Gyan Up; Gyan single2068.|
|42–49|B shiver, B2069; Gyan Right/2070, Left/2071, Right/2072 (all single).|
|50–55|join Gyan; Peter follows Bowie, B follows Peter, Gyan follows B, each relative-place2; Zynk position4,12 Up; end.|

All identity/position operands are source selectors, not guaranteed actors.
csc29 setDest reads a word identity (bit15 suppresses its wait), writes
destinations and absolute travel with signed ±32 velocities, then waits for
physical X/Y==destination for Bowie0's clear bit15. This differs from waiting
for an action pointer. csc2D's inline movement installs a generated script,
sets slot-based timer, clears collision bits5/6 and waits for eas_Idle.
It has no ally-HP skip. Each generated move uses relative384-unit movement
and a destination wait before the Idle tail; obstruction/alias/action services
can still affect execution. No movement finish follows solely from installation.

csc19 setPos and csc23 facing inspect the **byte identity** through
AdjustScriptPointerByCharacterAliveStatus: ally currentHP0 skips their4/2
operand bytes, respectively. FOLLOWER_B31/nonally identities bypass the
ally check. Dead Peter/Gyan/Zynk can therefore skip position/facing while
unconditional camera, destination, motion, shiver or join operations proceed.
Shiver is csc2A: resolves the live alias, saves sprite size/animation counter,
sets size21/counter255, toggles flagsB bit3 on/off for three5+5 Sleep cycles,
restores counter/size. It has no HP guard or membership write.

Command50 invokes csc08_joinForce with24 (not128's paired Sarah/Chester case
and no sad bit15): speech0, view wait, MUSIC_JOIN, JoinForce24, GetClass,
dialogue name1=24/name2=current class, text446, finite-music/input wait,
close and Sleep10. It does **not** consume the cutscene cursor:2072 is the
last ordinary scene message and446 is an additional service message.

JoinForce sets joined flag24, calls UpdateForce (rebuilds ordered joined,
active and reserve counted prefixes), tests refreshed active count against12,
and sets active56 only when below12. It does not clear a preexisting56 when
full and does not heal/reinitialize Gyan. Crucially **no second UpdateForce**
follows the new active write: flags can include56 while counted active/reserve
lists retain their pre-write classification until a later rebuild. Repetition
still runs music/text and the capacity test; a prejoined24 or active56 alone
does not skip the scene when830 is clear. Existing HP/class/items remain.

Each csc2C follow reads a word actor, but its HP precheck reads the **high
byte** of that word: all three ordinary selectors7/31/24 have high byte00,
so Bowie0 HP guards all three. Clear Bowie HP skips all6 operand bytes;
it does not individually test Gyan/Peter's HP. Admitted calls resolve live
physical aliases, use table_FollowerPositions index2→`(-24,0)` and AddFollower.
Each copies42 bytes of eas_Follower1 into writable action scratch, patches
leader/offsets at1E/20/22 and appends the actor to EXPLORATION_ENTITIES unless
already listed (three calls still allocate126 bytes). They install the
Peter→Bowie, B→Peter, Gyan→B chain; they set no ordinary follower flags.
The copied template sets speed32,32/factors0,0, acceleration/deceleration
OFF, entityObstructable OFF, mapUncollidable/entityUncollidable ON, then
follow, waitDest and branch back to follow. esc03_follow compares the maximum
absolute coordinate separation with `$1E0` (480 internal units), only plans
movement when greater, rotates patched offsets (shifted4) by leader facing
around the leader's destination, tests layout words againstC000 with fallback
to a reduced offset then leader destination, and derives signed travel/speeds.
It is not a teleport or a guarantee of a visible trailing formation.
Neither roster capacity nor Gyan being in reserve prevents this scene chain.
The final Zynk setPos is independently guarded by Zynk HP.830 is set by the
zone handler only after the scene returns, after these effects.

## Zynk's entity scene and later reconstruction

**Confirmed:** cs_5E20A (`$5E20A..$5E22C`) has nine commands/34 bytes:
cursor2073; single Zynk2073; single Zynk2074; single actor128/2075;
Zynk facing Down; Zynk shiver; single Zynk2076; addNewFollower26; end.
Actor128 is the young man, not a narrator-only slot or Zynk alias. Ordinary
Zynk mapsprite55 uses portrait26/bleep8; Gyan53 uses portrait24/bleep6;
young man196 has no portrait/bleep2. Actual HP/class/mapsprite and aliases
can select different dialogue resources.

csc56 addNewFollower resolves26, initializes prospective leader0, scans
EXPLORATION_ENTITIES throughFF, retaining its last physical ID, then
AddFollower26 with offset`(-24,0)`. An empty list selects physical0; otherwise
it follows the list's last actor. This does not require Gyan or any particular
chain. It has no HP, already-following, roster or836 guard. AddFollower still
allocates42 scratch bytes on repetition, though an already listed actor is
not appended twice. With actor26 already last, a directly repeated handler
can install self-following. Ordinary C targeting excludes listed followers,
so direct handler repetition and ordinary post-follow interaction differ.

After script return the handler writes76 then836, separately from the action
installation. There is no JoinForce26, joined26/active58 write, or membership
message446. The prose about taking the ancient soldier along is **Inferred**
narrative meaning; following is the actual static operation. Preheld836 does
not suppress this handler when830 is held. Clear830 suppresses the script
and both writes regardless of836/76/membership.

| Supplied/reached boundary | Consequence under the selected code |
| --- | --- |
|Initial load830 clear|Gyan and hidden Zynk declarations, subject to follower alias suppression; base init RTS.|
|Scene joins24 but stops before handler set830|Membership/earlier motions can remain; next live setup still base. No atomic rollback.|
|830 becomes held without population reload|Seven residents and the current Gyan/Zynk physical actors remain; zone selection switches to default and resident134 uses2052.|
|Reload830 held,836 clear|Seven resident rows; init allocates Zynk at4,12, even if supplied76 already declared another physical actor.|
|Reload830 held,836 held,76 clear|No fixed/init Zynk;26/58 alone does not create an ordinary follower.|
|Completed Zynk handler76/836 held|Later reconstruction admits Zynk from follower76; init skips. Gyan's scene-only chain is not reconstructed.|
|Installed Zynk follow, interrupted before76/836|Current action/list can differ from later reconstruction;76 clear loses ordinary following on reload.|
|76 set, interruption before836|Follower is admitted on reload; clear836 still permits the extra newEntity allocation.|

Script entry clears text-skip; debug P2 Start can latch it. Skipped nextText/
nextSingleText consumes its operand without display or cursor increment.
It does not skip all movements/join/follow/flag operations. Thus final830 or
836 proves neither every message displayed nor player acceptance; interrupting
services can leave any reached prefix. Fresh map construction resets scratch,
positions and scene action/list state, then applies the current ordinary
follower flags and selected init. It is not a save/persistence observation.

## Complete keyed text dependencies

**Confirmed:** the resident keys2043–2052 (`07FB..0804`), Gyan2053–2072
(`0805..0818`), Zynk2073–2076 (`0819..081C`) and join446 (`01BE`) are distinct.
The following content is paraphrased; token order is complete. N is line break;
W1/W2 retain different text/input consumers. All scene modifiers are00; no
selected YesNo prompt, LEADER token or narrative-derived join is present.

| Key / speaker | Content / tokens |
| --- | --- |
|2043 /128|Pacalon abandoned them; W1.|
|2044 /129|Many died; N,W1.|
|2045 /130|Asks if devils were defeated; N,W2.|
|2046 /130|Can go outside; W1.|
|2047 /131|Claims he wanted to fight; N,N,W1.|
|2048 /132|Says Tom was frightened; N,N,W1.|
|2049 /133|Zynk hid them; NAME26,N,W1.|
|2050 /134,830 clear|Gyan of Ribble is behind her; NAME24,N,W2.|
|2051 /134,830 clear|He guarded the west gate; N,W1.|
|2052 /134,830 held|He fought at the west gate; NAME24,N,W1.|
|2053 /Gyan|Asks whether visitors are from Pacalon; N,W1.|
|2054 /B|Granseal origin, addresses Gyan; N,NAME24,W1.|
|2055 /Gyan|Does not know Granseal; N,W1.|
|2056 /B|Near Ribble in South Parmecia; N,W1.|
|2057 /Gyan|Asks if near home; N,W1.|
|2058 /B|Settlement just over a year ago; N,W1.|
|2059 /Gyan|Asks if the tunnel reopened; N,W1.|
|2060 /B|Can return home; N,W1.|
|2061 /Gyan|Has waited for this day; N,W1.|
|2062 /Gyan|Asks why North Parmecia; N,W1.|
|2063 /Peter|To defeat devils; W1.|
|2064 /Gyan|Asks confirmation; W1.|
|2065 /B,continued|Avoid another Moun tragedy; N,W2.|
|2066 /B|Plans Grans and defeating the king; N,N,W1.|
|2067 /Gyan|Reflects; W2.|
|2068 /Gyan|Cannot return now; W1.|
|2069 /B|Raises his family; W1.|
|2070 /Gyan|Will miss them; W2.|
|2071 /Gyan|Believes they will understand; N,W2.|
|2072 /Gyan|Asks to accompany them; W1.|
|446 /join service|Names member and class joining; NAME,CLASS,N.|
|2073 /Zynk|Overheard Gyan conversation; N,NAME24,W2.|
|2074 /Zynk|Wants to reach Grans; W1.|
|2075 /128|Taking the ancient soldier; N,NAME26,N,W1.|
|2076 /Zynk|Urges haste; W1.|

All scene rows other than2065 use nextSingleText, including rows endingW2:
csc00 still closes portrait/text and sleeps10 after DisplayText. csc02's
continued2065 leaves windows; both increment the cursor only on the ordinary
display path. Packed speaker decoding uses the actual actor mapsprite.
NAME24/26 resolve those combatants' names; they do not change joined flags
or rebuild force lists. Join446's NAME/CLASS use csc08's dialogue parameters,
not an implicit Zynk or first-active selection. Window/view/text/input/audio
completion is not established by a key/token join.

## Optional ground items and failed awards

**Confirmed:** both setups share map-level rows `(10,10,183,RUNNING_PIMENTO12)`
and `(19,6,184,ANGEL_WING4)`. No chest row, shop purchase, zone award, scene
award or Section5 grant is involved.183/184 are collection flags, not the
temporary map flags cleared at entry, and neither830 nor836 selects them.

The shared CheckArea callers are field-menu search (`d6=0`) and ordinary C
after no entity (`d6=1`). Negative view target returns. Faced coordinates
use signed division384 and the64-wide layout. Dispatch is chest1800,
vase2C00, barrel3000, bookshelf3400, searchable area1C00, then description.
The four non-chest kinds use CheckNonChestItem. These rows alone establish
no particular container kind at either coordinate. Description RTS leaves
unhandled: C can return0 to the menu; menu search falls back423/412.

Matched clear183/184 yields its item and **sets the flag before inventory
award**. Held flag substitutes ITEM_NOTHING127 and sets the flag again;
the kind's empty line is used. No match yields no flag mutation. For a
nonempty item, request413, test Bowie0's four inventory slots first, AddItem
if room, then item music/415 and finite-music/input wait. With Bowie full,
UpdateForce rebuilds the joined list, and OTHER_FORCE_MEMBERS_LIST=TARGETS_LIST+1
is scanned in index order using TARGETS_LIST_LENGTH−2 and DBF. It includes
reserve/dead joined members, not just active or visible actors; missing-Bowie
state still skips the list's first entry. First room receives the item via
AddItem, name3, music/416 and the same wait. This rebuild can refresh Gyan's
previously stale list classification, but is not required for his scene flag.

AddItem writes the first masked127 slot, returning failure if none; the caller
does not test that return after its earlier capacity check. Stable inventory
across that seam is required for the stated successful grant. All-full or
negative initial loop counter requests417, restores candidate X/Y, calls
CloseChest then RefillNonChestItem. No selected chest row matches, so only
the latter clears the matching183/184; no item is added and a later admitted
search can retry. Interruption after flag set but before award/refill is not
an atomic transaction. Common closure returns−1. Item use/equip effects,
natural collection, rendered containers and saved persistence are outside
this award boundary.

## Both return requests and immediate Map31 consequence

**Confirmed:** both Map39 warp rows have mode0/padding0 and face Down3:

| Source X,Y | Destination |
| --- | --- |
|3,21|Map31 MOUN `(16,8)` Down3|
|14,20|Map31 MOUN `(27,7)` Down3|

Neither tests830/836/76, Gyan24/56, Zynk26/58 or item flags. Walking must
admit the `$1000` marker and reach the first matching row. WarpIfSetAtPoint
testsFFFF terminator before coordinate matching, writes type1 before the
payload longword and facing/padding word, then WARP_SFX. WaitForEvent checks
pending before A/C; ProcessMapEvent clears type before consumption. No-scroll
type1 submits/clears WARP_SFX, idles main, unwinds to MainLoop and samples
physical main coordinates. Nonnegative destination bytes replace each sampled
word's **low byte**; high bytes are not universally cleared. Under intact
small state the two returns are31 `(16,8)`/`(27,7)` Down3,d4=0. Changed payload,
PARAM1, main state or partial requests can change the result. These are
requests, not observations of a successfully usable return.

The subsequent explicit31 path calls SwitchMap/CheckBattle, unlike the
predecessor's immediate battle-return path. No31 map substitution exists.
Battle33's row requires433: clear433 enters exploration; held433 admits33
even with533 held (and then clears433 in the completed-match branch).
Ordinary reached Battle33 victory had cleared433/set533, but underground
entry alone does not prove those incoming flags. Stop at this admission
boundary if433 is supplied held; do not execute another battle here.

For admitted exploration31 the
[accepted predecessor's setup/reload section](./battle33-outcomes-and-zynk-handoff.md#returned-map31-reload-conditions-and-outgoing-requests)
applies with the **new live flags**:830 held selects eleven town rows,
not the empty base list, with no fixed Zynk. Held832 init moves nonally136
to30,6 Left; held834 with clear835 clears834. F76 independently reconstructs
Zynk following; Gyan's joined24/active56 does not reconstruct the scene
chain. Flag-layout copies833→16,8 and831→26,32 recur independently;830 does
not imply either. Clear830 still selects base RTS/empty fixed list, even
with supplied Gyan membership or76. Population and init again query live
flags separately. Temporary256–383 clear,80 sets and other entry services
follow the explicit-map lifecycle. Stop at the input/event handoff and these
direct setup effects; the full ground-town catalogue is a later outcome.

## Actual Content, Domain, Application and Godot boundary

**Confirmed about current repository code:** production consumes prepared
content; it does not execute these source files on demand. The
[capability owner](../../remake/docs/capability-status.md) controls actual runtime
claims. This slice prepares no selected package and runs no session.

| Actual consumer | Expressible / transformed / missing boundary |
| --- | --- |
| [OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py), [join lowering](../../src/sf2tool/remake_exploration_joins.py) | Selected scene queries yield161 Gyan instructions,29 Zynk instructions,2 init instructions. Gyan's source command2→instruction2 native setDest is its earliest selected unsupported operation; Zynk command7→instruction27 native addNewFollower; init command0→instruction0 native newEntity. Ally26 fails the lowerer's newEntity128..159 condition. |
| Selected nested actions | All eight inline movements/Idle tails, eas_Init and all three patched walking templates lower without nested native calls. The original dynamically patched42-byte follower template and its aliases/list are separately audited; it does not acquire implementation through a successful inline-action query. No hidden nested refusal is omitted when identifying the three per-entry frontiers. |
| [Map preparation](../../src/sf2tool/remake_exploration_maps.py), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) | Setup variants, population, ordered entity/source-zone events, init and marker/door/warp shapes exist when selected as eventMaps with actual resources. Setup variants must be bound; isolated compiler dictionaries are not an admitted world. Native becomes UnsupportedInstruction/UnsupportedEntityAction, not a service placeholder. |
| [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs) | Runners refuse native with program-opcode/entity-action. Motion/facing/position/follow/camera/flags/text exist; scene follow/position shapes do not carry the original byte-versus-high-byte HP skip. Guarded aliases/capacity differ from unchecked original state. Prior missing resources/state/services can fail before a listed native; these are conditional entry frontiers, not a universal first runtime failure. |
| [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs) and ProgramRunner.JoinResources | Join preserves the pre-active-write counted lists. With encounter starts, joining needs one admitted member deployment/start binding and introduces resources once. Gyan's selected bindings are absent here; initial starts are not proof of original live stats. Zynk dialogue must not be lowered as JoinPartyMember. |
| [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs) | Setup/population/follower/entry flags, doors, source-zone caller and positive warp shapes exist. Add guards alias presence/capacity and takes explicit sprite; it does not implement the ally26 native above. Ordinary Interact uses adjacent tile/facing and prepared events, not the original counter-extended physical reverse-alias scan/follower exclusion. Live handler selection versus loaded population and changed/partial mutable requests need explicit binding. |
| Field-search content/runtime | Existing static field-search contracts express the source algorithm; current exploration programs/commands provide no selected CheckArea183/184 inventory/refill service. Two source item rows and a working door/warp do not implement search. |
| [ExplorationTextRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationTextRunner.cs), [Godot presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs) | Text/name/class, portrait/bleep, waits, join music, camera and shiver gesture have consumers. Selected texts, portraits, sprites, layout/palette/audio and observed completion/timing/input are unbound here. A presentation cue or successful lowering is not original sprite/DMA/audio parity or Godot acceptance. |

The direct selected query includes all entity handlers/defaults, both zone
handlers/defaults and both init functions, preserving their branch/flag order.
For initial zone execution the Gyan native precedes recruitment/830; on a
held830/clear836 reload the init native is encountered before field input;
for an independently admitted Zynk interaction its native is after the selected
dialogue/shiver but before76/836. Bypassing one gap would not establish the
others or provide the selected world/material/search bindings.

## Reproduction, adversarial review and remaining Unknowns

Load the current ignored selections in the same process. Direct audit uses
the configured source root and the unchanged pair, followed on clean HEAD by
scope planning:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1094/review.py <configured-source-disasm-root> --committed
git diff --check origin/main HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Review all selected tables,56/9/2 commands,8 inline streams, actual caller
aliases and control widths/polarity/order, text tokens and item/return joins;
inspect the actual consumers and recursive native results. Exercise the
reasoning against dead/unjoined/full/empty-party, preheld24/56/26/58/76/830/836,
duplicate/missing/overwritten aliases, changed loaded population, repeated
handler versus C targeting, stale lists, skipped text and interrupted writes/
services. No new parser/schema/test is introduced; ADR0004's detailed
acceptance review applies proportionally to the direct documentation audit.

Check exact two-path accepted-base→HEAD ownership, navigation-only predecessor,
UTF8/LF/links/table shape/private boundary, staged complete diff and clean
committed state. Retain exact base/head/tree, commands and scoped CI in the
PR/local handoff. Planner suggestions do not override this documentation-only
scope: normal/full suites are not run or relabeled PASS. Preserve1092's
author01/02/04 failures, author03's dirty passing receipt followed by the
independent egress-constant correction, and later receipts. Preserve1088/1090
corrections and twelve old completed comparison failures through their owning
handoffs. New receipts have distinct paths; none is overwritten or rerun as
an automatic queue.

**Unknown:** natural entrance/zone/C/search/return reach; live flags, HP/class/
stats/inventory, party/list contents, physical aliases/population, layout kinds,
view/player mode and mutable request state; completed loading, movement,
following, portraits/text/input, camera, sprite/DMA/audio and return services;
interruptions, visual/collision meaning, timing, usable control and persistence.
These do not authorize runtime acquisition. Done is a clean committed/pushed
frozen Draft PR with stopped writer/process and successful report to independent
main-gate; integration acceptance/merge remains there. No merge, next Issue,
cleanup or archive in this slice.
