# Creed return, Fairy following and recruitment choices

Evidence date: 2026-10-14. **Confirmed:** the shrine exit can request a fresh
Creed Mansion entry; F784 normally selects seven residents and permits init to
place the four unjoined recruits. Fairy's interaction requests a current follow
relation and only afterward sets F762 then F785. The four recruit callbacks
choose initial offers, refusal text or F22 alternatives; a Yes joins the selected
ally before F787 and the following request. **Inferred:** these form the room's
post-consultation help/companion choices. **Unknown:** natural return/talk reach,
actual supplied state, completed presentation/following, usable control and save
persistence. Neither a flag nor a narrated decision proves those outcomes.

## Agreement and provenance

Standard rigor under [Issue #953](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953)
and [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one complete room/choice outcome, with ordered state and service boundaries.
Accepted starting base is `9de649eb1da588065f513b5d0f32819636bb8933`.
This document alone is owned. Consume accepted
[consultation and zone return](./evil-spirit-consultation-and-zone-return.md),
[shrine entry/return routing](./evil-spirit-shrine-entry-and-scene-admission.md),
[restoration and Map33 init](./creed-restoration-and-basement-departure.md), and
[Rick's join/follow consumers](./hassan-rick-recruitment.md).
No shared index, registry, fixture, schema, design, tooling, engine, translation,
runtime, acquisition or environment changes. The independent consultation
synthesis consumes accepted evidence and is not a dependency. Stop before
`cs_5B466`; only its admission boundary is identified here.

All original-source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Setup filenames refer to `data/maps/entries/map33/mapsetups/`; map/entity-engine
filenames refer to `code/common/scripting/map/` and
`code/common/scripting/entity/`. Fnnn means flag nnn; address intervals are
half-open. Source lines and program indices are exact, not runtime observations.
Constants resolve through `sf2enums.asm`, RAM symbols through `sf2const.asm`.

Minimum evidence is pinned static source plus selected existing fixtures and
the unchanged 18,285,979-byte map-init/map-script-engine pair. Linear comparison
budget: under one minute/512 MiB, plus small selected prose/receipts. The reused
comparison completed in 0.081s with size/mtime unchanged. No regeneration,
network acquisition, full transcript publication or new framework is needed.
Reassess contradictions or material consumer expansion before scaling. Counts
support the exact source/fixture joins below; they do not replace them.

## Return, population and init

**Confirmed:** the accepted Map34 warp table at `$AD498..$AD4B2` has a first
row `(26,28)`, no-scroll0, destination CREED_MANSION33 `(9,5)`, Right0.
It tests no consultation flag. Its other two rows are the internal CURRENT255
pair `(22,29)` to `(24,2)` Left and the reverse Right; they are not mansion
entries. A working-layout warp block, admitted movement and request consumption
remain premises. The inherited low-byte destination rule does not repair
arbitrary high bytes. The accepted no-scroll consumer clears MAP_EVENT_TYPE,
returns through MainLoop's SwitchMap/CheckBattle and has no Map33 replacement
or battle-coordinate row. F784 after the consultation's return is compatible
with this route, but neither F784 nor the table proves the walk occurred.

Explicit Map33 entry rebuilds actors, loads mapsprites/layout and clears only
temporary flags256–383 and sets F80 before selected init. It preserves
F22/F67–69/F759/F762/F774/F783–787 and the joined/active flags. CURRENT255 reuse
retains the existing population and current layout; it is not the explicit
shrine exit. Each setup accessor reselects from current flags, so changed flags
between population, init and interaction need not give a coherent narrative
state.

The complete `data/maps/mapsetups.asm:106–112` selector is last-set-wins:
default `ms_map33`; F523 → `ms_map33_flag523`; F783 → baseline `ms_map33`;
F784 → `ms_map33_flag784`; F786 → `ms_map33_flag786`; F22 → baseline, last.
All share this document's entity-event table. The accepted restoration owner
contains the complete alternate populations and init control flow. Their
consequences for this return are:

| Selected setup | Resident/init consequence |
| --- | --- |
| F784, without F786/F22 overriding | Seven rows below; normal init. F783 set skips restoration and its Hassan EGRESS_MAP write. |
| F786, F22 clear | WORKER128 walking at `(22,4)` Up, origin `(22,4)`/range1; CREED129 `(17,5)` Down; ODDLER130 `(17,7)` Up. No four recruit rows. `ms_map33_flag312_InitFunction` in s6_initfunction_786.asm is only rts at `$5AC56`; the source label does not mean F312 selects it. |
| F22 set | Six baseline rows: WORKER128 Up, CREED129 Left, Eric Down, Randolf Up, Tyrin Down, Karna Up, all initially `(63,63)`; no resident130. Normal init places128 `(14,14)` Up. F22 does not bypass the independent F784 recruit-placement tail. |
| F784 clear, no later override | Default/F783 selects baseline; F523 alone selects the seven restoration rows. Normal init's F523-set/F783-clear scene remains eligible independently of F784. |

The complete `s1_entities_784.asm`, `$5A712..$5A74C`, is seven fixed rows plus
msEntitiesEnd. Allocation and normal F784 init must be read together:

| Row / logical identity | Declared sprite / initial tile, facing, action | Normal init if this joined flag is clear |
| --- | --- | --- |
| 0 / 128, Goliath dialogue | WORKER202 / `(17,14)` Up, eas_Init | No F784 placement; remains at declared position absent other work |
| 1 / 129, Creed | CREED212 / `(17,5)` Down, eas_Init | No later placement here |
| 2 / 130, Fairy | FAIRY219 / `(17,10)` Up, eas_InitSlow | No later placement here |
| 3 / Eric14 | Ally/class/HP / `(63,63)` Down, eas_Init | F14 clear: cs_5AC04 → `(15,14)` Down |
| 4 / Randolf16 | Ally/class/HP / `(63,63)` Up, eas_Init | F16 clear: cs_5AC1C → `(16,15)` Down |
| 5 / Tyrin17 | Ally/class/HP / `(63,63)` Down, eas_Init | F17 clear: cs_5AC34 → `(18,15)` Down |
| 6 / Karna15 | Ally/class/HP / `(63,63)` Up, eas_Init | F15 clear: cs_5AC4C → `(19,14)` Down |

`ms_map33_InitFunction` (`$5AB7C..$5AC56`) first runs restoration only for
F523 set/F783 clear; after its return it sets F783 and EGRESS_MAP=Hassan9.
Then F22 places128 as above; otherwise F784 skips Goliath alternatives,
otherwise F783 places128 `(17,10)` Down, otherwise F774 places128 `(17,12)`
Down. Finally F784 independently gates the four joined-flag tests in table
order. These `setPos` operations skip zero-HP allies; a clear joined flag is
not sufficient to place one. F787 is not tested by init. F784 supplied with
F523 set/F783 clear can still run restoration on the selected population;
do not interpret F784 as proof of completed restoration or consultation.

`mapfunctions.asm:InitializeMapEntities` allocates flag-selected followers
first, then fixed residents. Resident coordinates are masked by `$3F` and
scaled by MAP_TILE_SIZE=384. NPC128–130 use lookup offsets32–34; ally rows use
indices14/16/17/15 and are skipped if an existing nonzero lookup already exists,
without a joined/active test. With only F67/F68 ordinary followers, Peter7 and
B31/Oddler occupy physical1/2; Goliath/Creed/Fairy are physical3/4/5 and
Eric/Randolf/Tyrin/Karna physical6/7/8/9. This is a conditional allocation,
not intrinsic identity. Bowie is physical0 at the supplied arrival tuple.

The complete follower table has no Eric/Randolf/Tyrin/Karna rows. F69 adds
FOLLOWER_C158 (lookup offset62)/Fairy, a different logical actor from
resident130. F74/F68/F70 can all allocate B31 with Astral/Oddler/Astral sprites;
later allocations overwrite that lookup without deduplication. Map33 is not an
overworld Caravan map. Missing ordinary mappings start at zero and may alias
physical0; hidden mappings are -1 and have no local validity repair. HP/class
determines ally mapsprites (zero-HP outside battle can be blue flame);
exploration healing covers living/immortal allies, not every dead recruit. Mode,
raft, excessive population and alias limits remain those of the accepted entry
owner.

No local interaction changes EGRESS_MAP or PLAYER_TYPE. Repeated normal init
with F783 set skips the Hassan write as well as restoration. Init return does
not add the zone wrapper's physical0 coordinate wait. Exploration's palette,
music and WaitForEvent tail must still return; a pending MAP_EVENT_TYPE takes
priority over controller installation. These are inherited conditional-control
limits, not evidence of a new Egress destination or usable input after entry.

## Complete interaction table and callbacks

**Confirmed:** `s2_entityevents.asm`, `$5A766..$5AA8A`, contains seven specific
four-byte rows and one default, eight callback bodies and seventeen embedded
script programs. All four setup pointers and six route selectors share it.
The source's UP/DOWN operands are wrapper action flags, not admission-facing
requirements: UP1 turns the mapped actor toward the player and does not restore
its old facing; DOWN3 also restores the saved facing after return. Default0
does neither. The exact table and complete callback decisions are:

| Row PC / selector, flags | Target / half-open callback range | Ordered branch result |
| --- | --- | --- |
| `$5A766` / 128, DOWN3 | Map33_EntityEvent0 `$5A788..$5A7D2` | F22 set:1952–1955; else F786 set:1950–1951; else F784 set:1905–1906; else F783 set:1904; else1827. Return; no explicit flag writes. |
| `$5A76A` / 129, UP1 | Map33_EntityEvent1 `$5A7D2..$5A7F0` | F786 set:1946–1947; otherwise1898–1900. Return; no F22 test or explicit flag writes. |
| `$5A76E` / 130, UP1 | Map33_EntityEvent2 `$5A7F0..$5A828` | F786 set:1948–1949 then return. Otherwise F759 clear:1919, set:1901–1902; both close text, run cs_5A828, then set762, set785, return. |
| `$5A772` / Eric14, UP1 | Map33_EntityEvent3 `$5A842..$5A864` | F22 set:cs_5A90E; otherwise F787 clear:cs_5A8CA, set:txt1909; return. |
| `$5A776` / Randolf16, UP1 | Map33_EntityEvent4 `$5A864..$5A886` | F22 set:cs_5A97E; otherwise F787 clear:cs_5A93A, set:txt1912; return. |
| `$5A77A` / Tyrin17, UP1 | Map33_EntityEvent5 `$5A886..$5A8A8` | F22 set:cs_5A9EE; otherwise F787 clear:cs_5A9AA, set:txt1915; return. |
| `$5A77E` / Karna15, UP1 | Map33_EntityEvent6 `$5A8A8..$5A8CA` | F22 set:cs_5AA5E; otherwise F787 clear:cs_5AA1A, set:txt1918; return. |
| `$5A782` / default `$FD`,0 | Map33_DefaultEntityEvent `$5A786..$5A788` | rts only. |

`mapsetupsfunctions_1.asm:RunMapSetupEntityEvent` scans logical IDs, saves
player facing in EVENT_RELATIVE_POSITION, selects mapped sprite portrait/speech,
turns/restores according to those bits, disables entity updates around the
callback, then closes portrait/text and activates updates. Trap6's `script`
path reactivates entity updates before ExecuteMapScript, so action waits have
an updating path. The wrapper preserves a physical index for restoration;
it does not re-decide logical identity after the script.

Ordinary activation still uses ProcessPlayerAction/GetActivatedEntity and
GetEntityEventIndex: current view target/facing, layout/counter reach, first
qualifying nearby physical actor, exclusion of the view target and actors in
EXPLORATION_ENTITIES, then inverse lookup order. No handler call proves those
preconditions. Missing/aliased mappings can select another logical row before
the intended one. Directly invoking a callback is a different premise.

The actual actor matters. Logical130 is Fairy under F784, Oddler under F786,
and absent in a fresh F22 baseline; F786 selects Oddler's words even if flags
changed over a retained Fairy. Baseline WORKER has no portrait/bleep7, CREED
portrait/bleep8, FAIRY no portrait/bleep1, ODDLER portrait/bleep4. Eric/Karna's
base/promoted/special sprites select their portraits/bleep5; Randolf/Tyrin's
select theirs/bleep6 (`data/spritedialogproperties.asm`). Different actual
sprites/mappings can change those results. Script text uses modifier0 for each
recruit; Fairy narration is `$FF,ENTITY_NONE` ($FFFF), bypassing actor lookup
and setting speech SFX0. Requested portrait state is not rendered evidence.

The callbacks do not enforce F784, F783, F523, a completed visit, roster
membership, HP, party capacity or a narrative chronology before offering a
recruit. F22 bypasses F787, even when F787 is already set. The F22/F786 words
about regained memory, disaster and freedom are supplied branches, not proof
of those later events. No room callback buys, transfers equipment, heals a
dwarf, grants a sword or implements the characters' other promises.

## Fairy: live block copy and current following

**Confirmed:** `cs_5A828`, source lines123–127, `$5A828..$5A842`, is exactly:

| Record | Command / immediate effect |
| --- | --- |
| 0 | setBlocks17,13,1,1,9,5 |
| 1 | textCursor1903 |
| 2 | nextSingleText `$FF,ENTITY_NONE`: narrated decision to take Fairy |
| 3 | followEntity130,FOLLOWER_B,2 |
| 4 | csc_end |

csc34 reads packed words `$110D,$0101,$0905` and calls
`code/gameflow/exploration/exploration.asm:CopyMapBlocks`. It copies the current
layout word at `(17,13)` to `(9,5)`, using a 64-word/128-byte row stride in
FF0000 working RAM, then requests both view-plane update bits. This is a live
copy of tile/collision bits, not a fixed literal tile, flag write or warp-table
edit. Earlier mutations (including F22's declared `(28,0)` size25×32 copy to
`(0,0)` in `3-flag-events.asm`) can change the copied word. No selected source
proves the actual image/block or completed redraw. The complete Map33 flag-copy
table has only F22; there is no F762/F785 replay of this one-cell copy on fresh
load. Retained layout and a fresh load must not be conflated.

The narration has no yesNo, F89 decision or join. In `sf2cutscenemacros.asm`,
followEntity encodes follower130 as word `$0082`, target31 as `$001F`, direction2
as a word. csc2C first reads **the high byte** of the follower word for its
AdjustScriptPointerByCharacterAliveStatus test: here0, so Bowie HP0 skips the
six argument bytes and the follow operation. It does not test Fairy HP, or B's
HP/population. Prior block copy and narration still happened, and a normal
script return still permits caller762 then785. This preserves the corrected
high-byte guard from the restoration owner.

Otherwise GetEntityAddressFromCharacter resolves resident130 and B31; direction2
indexes bytes `(-24,0)` in table_FollowerPositions. AddFollower installs a
copied 42-byte eas_Follower1 block on the resolved Fairy actor, patches target
and pixel offsets and advances ENTITY_WALKING_PARAMETERS. It appends the
physical follower before the EXPLORATION_ENTITIES `$FF` sentinel only if absent;
a repeat still allocates/replaces the action even when no append occurs.
Missing B can target physical0; aliases can self-follow or replace another
actor's action. No local check creates an absent Fairy/B or repairs an invalid
lookup. No instruction places Fairy immediately behind Oddler.

The action in `data/scripting/entity/eas_main.asm` sets speed32/32, disables
acceleration/deceleration and entity obstruction, enables map/entity
uncollidability, and loops follow/wait-destination. esc03_follow uses actual
target coordinates/destination/facing, moves only beyond maximum-axis separation
`$1E0`, and checks layout with fallback offsets/target destination. Installation
is not completion; the scene adds no settle wait. csc_end conditionally waits
view scrolling if a dialogue window remains, clears VIEW_SCROLLING_SPEED and
returns, without joining all entity actions.

Caller F762 and F785 are ordered, non-atomic writes after scene return. F759
only selects the preceding speech. There is no F762/F785 repeat guard, no F69
write, no F68 clear and no force membership change here. An ordinarily listed
Fairy is excluded from activation, but a supplied direct callback can repeat;
if the HP guard skipped following, ordinary exclusion has not been established.
Fresh Map33 discards this current action/list allocation and reselects residents;
F762/F785 do not suppress the F784 Fairy row. F69, if independently supplied,
creates the separate FOLLOWER_C Fairy. Persistence across the temporary-flag
reset is different from permanent following or a verified save/load result.

## Eight offers and ordered successful branches

**Confirmed:** each of the eight offer programs has seven records: textCursor,
nextText, nextText, yesNo, jumpIfFlagSet89 to the paired success label,
nextSingleText refusal, csc_end. There are no other guards in these programs.
The first two texts stay in the cutscene text stream even when a literal ends
W1; the refusal is a single-text command that closes portrait/text and sleeps10.
Each row below gives exact source ranges and text IDs in that order.

| Actor / form | Offer label / lines | Prompt IDs → No ID | Yes target / lines |
| --- | --- | --- | --- |
| Eric / initial | cs_5A8CA / 240–246 | 1907,1908 →1909 | cs_5A8E6 / 247–253 |
| Eric / F22 | cs_5A90E / 254–260 | 1920,1921 →1922 | cs_5A92A / 261–264 |
| Randolf / initial | cs_5A93A / 265–271 | 1910,1911 →1912 | cs_5A956 / 272–278 |
| Randolf / F22 | cs_5A97E / 279–285 | 1923,1924 →1925 | cs_5A99A / 286–289 |
| Tyrin / initial | cs_5A9AA / 290–296 | 1913,1914 →1915 | cs_5A9C6 / 297–303 |
| Tyrin / F22 | cs_5A9EE / 304–310 | 1926,1927 →1928 | cs_5AA0A / 311–314 |
| Karna / initial | cs_5AA1A / 315–321 | 1916,1917 →1918 | cs_5AA36 / 322–328 |
| Karna / F22 | cs_5AA5E / 329–335 | 1929,1930 →1931 | cs_5AA7A / 336–339 |

`mapscriptengine_2.asm:csc11_promptYesNoForStoryFlow` calls j_YesNoPrompt,
sets F89 for result0 and clears it for nonzero, then sleeps10. In
`code/common/menus/yesnoprompt.asm`, default/Left is Yes0, Right is No-1;
A/C accept the current choice, B chooses No. An initially held PLAYER_1_INPUT
causes a release wait before the loop; window movement has its own waits.
csc0C checks F89 and jumps only when set. Thus Yes takes the success label;
No (including B) displays the refusal and returns without joining, F787 or
shrinks/following. The menu overwrites stale F89 when it returns. This is a
real choice command, unlike a W1/W2 dialogue wait or Fairy narration.

Each initial Yes label is seven records: join selected ally; setF787;
setActscript first unselected actor; setActscript second; setActscriptWait
third; addNewFollower selected; csc_end. Exact shrink order is:

| Selected | First asynchronous | Second asynchronous | Third, waited |
| --- | --- | --- | --- |
| Eric14 | Randolf16 | Tyrin17 | Karna15 |
| Randolf16 | Eric14 | Tyrin17 | Karna15 |
| Tyrin17 | Eric14 | Randolf16 | Karna15 |
| Karna15 | Eric14 | Randolf16 | Tyrin17 |

All twelve installations select eas_TwirlShrinkDisappear. The unselected actors
are named operands, not a filtered list of unjoined allies: there is no joined
or active-membership guard before shrinking them. Every F22 Yes label instead
has only four records: join selected; setF787; addNewFollower selected; csc_end.
There are no shrinks in those alternatives. Callback return follows script
return, with no new callback flag or extra choice. F787 suppresses subsequent
initial offers but does not suppress any F22 offer.

### Join and capacity are separate from following

**Confirmed:** join uses csc08 with word14/16/17/15, no sad-join bit15 and no
special128 Sarah/Chester selector. It clears speech SFX, waits view scrolling,
requests MUSIC_JOIN and calls `j_JoinForce → code/common/stats/battleparty.asm:JoinForce`.
It then reads current class via j_GetClass, fills dialogue name/class indices,
and requests generic text446. j_FadeOut_WaitForP1Input schedules previous music
and waits for a nonzero standard direction/button input; it is not a claim of
a screen fade or new input edge. Finally csc08 closes text and sleeps10.
Only its return reaches setF787. This acknowledgement can leave membership
changed while F787 is still clear if interrupted or stalled.

FORCEMEMBER_JOINED_FLAGS_START=0, FORCEMEMBER_ACTIVE_FLAGS_START=32 and
FORCE_MAX_SIZE=12 bind the exact state writes:

| Recruit | Joined flag always set | Active flag conditionally set |
| --- | --- | --- |
| Eric14 | F14 | F46 |
| Randolf16 | F16 | F48 |
| Tyrin17 | F17 | F49 |
| Karna15 | F15 | F47 |

JoinForce sets the joined flag even if already joined, then UpdateForce rebuilds
joined/active/reserve lists and counts from separate flags without HP filtering.
If the rebuilt active count is below12, JoinBattleParty sets that ally's active
flag. At12 or above it skips this write; it never clears an existing active flag.
There is no already-active rejection, replacement decision or capacity failure
branch. UpdateForce precedes the active write and no second rebuild occurs in
the join tail: a newly activated ally can remain in the prior reserve snapshot
until another consumer rebuilds it. Already-active membership is counted once
and the flag may simply be set again. Full party and HP0 do not prevent joining,
acknowledgement, F787, initial shrinks or the later addNewFollower call.

No selected command heals/revives, resets battle stats, grants AI, changes class,
grants equipment or reselects the actor's mapsprite. The class/name text reads
current data; the initial occupational claims are speech, not mutations.

`mapscriptengine_1.asm:csc56_addFollower` has no HP guard. It resolves the selected
ally, scans EXPLORATION_ENTITIES to `$FF`, chooses the last listed physical actor
(or0 for an empty list), and calls AddFollower with offsets `(-24,0)`. It creates
no missing actor and sets no persistent follower selector. It does not explicitly
choose Fairy, Peter or Oddler. Fairy-first and recruit-first interactions can
therefore select different targets. An existing last-listed selected actor can
be made to follow itself; deduplication prevents a second list entry, not the
new action allocation. Following is independent of active-party admission.

### Complete shrink action and completion boundary

**Confirmed:** `data/scripting/entity/eas_actions.asm:369–411` is the complete
eas_TwirlShrinkDisappear body. Start with ac_wait5 and ac_resizable ON. Then,
in exact order, set facing/size and updateSprite, followed by ac_wait5 each time:

| Step | Facing | Size operand |
| --- | --- | --- |
| 0 | Down | 22 |
| 1 | Right | 20 |
| 2 | Up | 18 |
| 3 | Left | 16 |
| 4 | Down | 14 |
| 5 | Right | 12 |
| 6 | Up | 10 |
| 7 | Left | 8 |
| 8 | Down | 6 |

The full tail is ac_setPos63,63; ac_resizable OFF; ac_setSize18;
ac_jump eas_Idle. It does not omit this reset, call eas_Init, hideEntity or
LeaveForce. esc1F toggles the actor's resize flag; esc0B writes the shared
SPRITE_SIZE word; esc0A requests ChangeEntityMapsprite and can defer at the
sprite-load limit. These are source operands and service requests, not measured
pixel sizes, frame durations or completed shrinking on screen.

esc0C_setPosition multiplies signed word coordinates by384, writes current and
destination X/Y to `$5E80,$5E80`, clears the long XTRAVEL/YTRAVEL pair, then
continues. It does not clear velocity, delete lookup entries, remove a follower
list entry, clear membership or free a slot. VInt_UpdateEntities skips actors
at X >= `$7000`; `$5E80` remains below that threshold, so the remaining tail
can run. This differs from HideEntity's `$7000` parking, lookup invalidation
and list removal, whose retained action pointer is dormant. The final shared
size write18 is not followed by a new updateSprite in this body.

`sf2cutscenemacros.asm` gives asynchronous csc15 wait byte0 and waited byte$FF.
`csc15_setEntityActscript` has no HP guard; it resolves the logical actor,
initializes its action-read timer from the returned physical index, installs
the pointer and, for the waited variant, polls only that actor's pointer for
exact eas_Idle. The first two installations return immediately. Different
physical indices, prior state, sprite service pressure, disabled updates or
aliasing affect progression; waiting on the third is not a proof that all three
finished. If aliases share a physical slot, later installations replace earlier
ones; no distinct-actor assertion rejects this. A previously hidden/invalid
mapping is not repaired. Interrupted or stalled waits do not roll back join/F787.

## Exact text joins and presentation limits

**Confirmed:** all 49 text sites in the complete entity-event source match the
literal at the corresponding four-digit hexadecimal key in
`data/scripting/text/gamescript.txt`: 45 distinct room IDs, plus generic join446
in csc08. The four initial refusal IDs also occur as direct callback txt calls.
The tables paraphrase the dialogue; ordered control sequences are exact.
D means callback txt using the wrapper's actual speaker. T means nextText;
S means nextSingleText. Script recruit speakers are the actor named in the
offer table, modifier0;1903 is narration. Source line references below identify
all callback/narration sites; offer lines are identified in the preceding table.

| ID / line / form | Subject | Ordered controls |
| --- | --- | --- |
| 1827 / 59 / D | Goliath rejects the visit | `{N}{W1}` |
| 1898 / 80 / D | Creed suspects a jewel secret | `{N}{W2}` |
| 1899 / 81 / D | Suggests Tristan storytellers | `{N}{W2}` |
| 1900 / 82 / D | Locates Tristan | `{W1}` |
| 1901 / 109 / D | Fairy asks about a sick dwarf | `{N}{W2}` |
| 1902 / 110 / D | Offers help and asks to be taken there | `{N}{W1}` |
| 1903 / 125 / S | Narrated decision to take Fairy | `{LEADER}{N}{W1}` |
| 1904 / 55 / D | Goliath reacts to Zeon's awakening | `{N}{W2}{N}{W1}` |
| 1905 / 48 / D | Goliath says his master wants to help | `{N}{LEADER}{W2}` |
| 1906 / 49 / D | Asks for one choice | `{W1}` |
| 1919 / 105 / D | Fairy announces she is going east with the party | `{N}{N}{W1}` |
| 1946 / 75 / D | Creed says devils cannot help | `{W2}` |
| 1947 / 76 / D | Says people must seal Zeon | `{N}{N}{W1}` |
| 1948 / 98 / D | Logical130 greets the leader | `{LEADER}{N}{W2}` |
| 1949 / 99 / D | Reports a headache | `{N}{W1}` |
| 1950 / 41 / D | Goliath asks about recovered memory | `{N}{W2}` |
| 1951 / 42 / D | Reports a severe headache | `{N}{W1}` |
| 1952 / 32 / D | Goliath sobs and greets the leader | `{N}{N}{LEADER}{W2}` |
| 1953 / 33 / D | Claims Oddler regained memory and went berserk | `{N}{W2}` |
| 1954 / 34 / D | Claims Creed understood Oddler's fearful face | `{N}{N}{W2}` |
| 1955 / 35 / D | Claims a fall into the sea; asks whether Creed lives | `{N}{N}{W1}` |

Each offer row below is T/T/S, with IDs and control groups in the same order.
The last initial ID also has D form at callback lines143/171/199/227 respectively.

| Actor / form | IDs | Speech summary | Ordered controls, separated by text ID |
| --- | --- | --- | --- |
| Eric initial | 1907/1908/1909 | Knight searching for a sword; asks to be chosen; wishes the party happiness if refused | `{NAME;14}{N}{N}{W2}` / `{N}{W1}` / `{N}{W1}` |
| Eric F22 | 1920/1921/1922 | Wants a legendary sword; asks to join; proposes searching alone if refused | `{N}{W2}` / `{N}{W2}{N}{W1}` / `{N}{N}{W1}` |
| Randolf initial | 1910/1911/1912 | Names himself; claims exceptional warrior strength; calls life here comfortable if refused | `{N}{NAME;16}{W2}` / `{N}{W1}` / `{N}{W1}` |
| Randolf F22 | 1923/1924/1925 | Says original size returned; considers going with the party; will stay if refused | `{N}{N}{W2}` / `{N}{N}{W1}` / `{N}{W1}` |
| Tyrin initial | 1913/1914/1915 | Mage trained in Ponpei; offers help; says his magic could help if refused | `{NAME;17}{N}{W2}` / `{N}{N}{W1}` / `{N}{W1}` |
| Tyrin F22 | 1926/1927/1928 | Says he is free; asks to join; warns of regret if refused | `{N}{W2}` / `{W1}` / `{W1}` |
| Karna initial | 1916/1917/1918 | Young priest; asks to leave with the party; insults a refusal | `{NAME;15}{N}{N}{W2}` / `{N}{N}{W1}` / `{W1}` |
| Karna F22 | 1929/1930/1931 | Celebrates freedom; asks to come along; calls refusal rude | `{N}{W2}` / `{N}{W1}` / `{W1}` |

Join446 (`01BE`) uses `{NAME}{CLASS}{N}` in that order and has no W1/W2 token;
the separate csc08 acknowledgement supplies the input wait. Explicit NAME;14/
16/17/15 expands the corresponding stored name; it is not a fixed literal name
or renaming prompt. LEADER calls UpdateForce and uses the first active member's
name, which need not be Bowie under arbitrary supplied membership. The follow
HP guard nevertheless reads Bowie0. Generic NAME/CLASS uses csc08's current
indices. `textfunctions_1.asm` treats N as newline and W1/W2 as input waits;
W2 uses continuation-arrow/validation-sound behavior. None is a yes/no menu.

csc04 sets CUTSCENE_DIALOG_INDEX; csc00/02 display then increment it. S closes
portrait/text and sleeps10; T does not. Normal No cursor endpoints are1910,
1913,1916,1919 for initial offers and1923,1926,1929,1932 for F22 offers; Yes
skips the refusal with cursor at its ID. Fairy's nominal cursor ends1904.
ExecuteMapScript clears SKIP_CUTSCENE_TEXT at entry, but debug/P2 Start may
latch it; csc00/02 then omit display and advancement. Other commands, including
yesNo and join, are not thereby removed. Nominal IDs/controls are not a claim
that every line displayed or enough time passed for asynchronous actors.

## Changed premises, return and next boundary

**Confirmed:** the source distinguishes these supplied-state cases without
inventing one common recruitment-eligibility predicate:

| Premise | Consequence / limit |
| --- | --- |
| Fresh F784 set, F786/F22 clear, F783 set | Declares seven rows; places living unjoined recruits; skips restoration/Egress write. F784 alone proves no prior event. |
| F787 set on fresh F784 entry | Unjoined recruits can be placed again; initial callbacks give refusal text without shrinking them. Init has no F787 suppression. |
| F22 and F787 both set | F22 offer wins for all four; Yes has no other-actor shrinks. F22 may independently change setup/layout. |
| Already joined but supplied/reachable actor, F22 set or F787 clear | Offer can repeat; join reasserts flags and acknowledgement. Normal fresh init may instead leave that ally parked, and current follower exclusion can prevent ordinary talk. |
| Already active or full active party | Membership and counts follow the ordered JoinForce rules; no scene-level rejection, displacement or capacity rollback. |
| HP0 selected recruit | Init placement can skip, but if callback executes, offer/join/csc56 still have no HP guard. Shrink targets likewise have no HP guard. |
| Bowie HP0 at Fairy follow | Block copy and narration can precede a skipped follow; caller762/785 can still be set. |
| Retained population with changed F786/F22/F784 | Callback text/setup selection changes without rebuilding actors; a Fairy sprite may speak the Oddler branch, or a missing actor may alias main. |
| Fairy already following / direct repeat | Ordinary activation excludes listed actors; direct repeated call has no762/785 guard, allocates another action block and need not append another list entry. |
| Missing/aliased recruit or target, empty follower list | Lookup/list rules still execute; no creation, validity, distinctness or self-follow repair. Artificial direct calls do not prove natural reach. |
| Interruption before Fairy script end | Copy/narration/follow may be partial; caller flags not reached. Between caller writes,762 may be set with785 unchanged. |
| Interruption during join acknowledgement | Joined/possibly active flags can be set with787 still clear. No atomic rollback. |
| Interruption after F787 or during shrink wait | Initial offers can be suppressed while shrinks or selected following remain unfinished. |
| Callback and wrapper return | Windows close and entity updates reactivate; no all-follower settle, saved-state verification or guaranteed usable-controller observation. |

At `(15,21)`, `s3_zoneevents.asm:Map33_ZoneEvent0` first checks F774. Clear
selects the earlier Goliath scene and ends that callback; it does not fall
through. F774 set, F785 set and F786 clear admit `cs_5B466`. Only after
that later script's return does the zone caller set F786, set F69 and clear
F68, in that order. This identifies why current resident130 following and
F762/F785 are not already the later Fairy/Oddler handoff. No internals or
completion claim for cs_5B466 are included. In particular this admission tests
neither F787 nor the four joined flags: a recruitment choice is not its local
prerequisite. Natural zone admission still needs the request/layout consumer.

**Inferred:** the initial offers represent choosing one helper, and the F22
alternatives express later freedom to recruit the others. **Confirmed:** the
executable boundary is F22/F787 branching and the ordered operations; there is
no universal one-recruit rule. Narrative freedom, restored size, promises and
future destinations are speech unless a separate accepted owner proves them.

**Unknown:** actual return and ordinary talk/zone reach; supplied flags, HP,
class, membership/list snapshots, population, mappings, mode and layout; all
service/actor completion, appearance/audio, stable control and save/load
persistence. Static survival of non-temporary flags is not an observed save.
These are retained limits, not an automatic emulator or acquisition queue.

## Reproduction and review boundary

Load ignored `local/private-inputs.ps1` in the same PowerShell process as every
controlled command. Reuse the
[retained pair recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`.
No normal/full/engine/Godot/H3/H4 suite is selected solely for this prose.

The selected exact joins, in addition to semantic consumer review, are:

1. In `tests/fixtures/h2/map-events/routing-setup.json`, select
   `categories.entityEvents.tables[symbol=ms_map33_EntityEvents]`: all eight
   rows, entity IDs, flags, PCs and relative targets above. Check all six Map33
   entity route joins, not just the F784 setup.
2. In `tests/fixtures/h2/map-events/entity-programs.json`, select
   `entityTargetPrograms` for Map33_DefaultEntityEvent and Event0–6. Check all 89
   operations against source lines/mnemonics/operands and PCs; preserve the
   complete flag-test and branch polarity/order, every text/script call, both
   Fairy caller writes and every return. Source comments are excluded as code.
3. In the retained map-script output, select all 17 `programCorpus.programs`
   with sourcePath `data/maps/entries/map33/mapsetups/s2_entityevents.asm`.
   Check all 105 records against exact source lines, macros and argument order:
   Fairy5, eight offers7 each, four initial successes7 each and four F22
   successes4 each. Public `map-script-engine-static-v1.json` holds summary and
   consumer contracts; it is not the private full program corpus.
4. Join all 49 room text sites by cursor/explicit ID to `gamescript.txt` and the
   controls/forms/speakers here; separately join csc08's446. Check all 43 lines
   of the complete shrink body, its nine facing/size pairs, ten waits and full
   four-command tail against the selected consumers. Review encoding, actual
   HP guard, AddFollower allocation/list semantics, JoinForce count/write order,
   live CopyMapBlocks source and actor lookup/position versus hiding directly.
5. Consume the accepted Map34 warp and Map33 setup/init joins; select the
   retained map-init source body, embedded placement scripts and route/pointer
   joins for ms_map33_InitFunction and the F786 return-only alternate. Cross-check
   exact F784 population rows, source s6 order, normal versus retained entry,
   and downstream admission only. Preserve predecessor evidence/Unknowns.
6. Review the complete document and cached diff, local links, UTF-8, exact sole
   path and private/generated boundary; run `git diff --check`. On a clean
   committed head run `uv run sf2 verify plan --base origin/main --head HEAD`,
   record selected partitions and actual exact-head CI, then freeze the clean
   pushed Draft PR for independent main-gate acceptance.

Preserve [#949's handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949#issuecomment-6178414274)
and [PR952 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/952#issuecomment-6178451561),
including the corrected dormant hidden-slot semantics,0.079s retained comparison,
CI38636725332 and inspection parse/path/truncation diagnostics. Their linked
#945/#941/#937 handoffs retain corrected high-byte following, text controls,
completed failures/results, newline/index diagnostics and unresolved boundaries.
This slice neither resets those receipts nor replays completed suites.
