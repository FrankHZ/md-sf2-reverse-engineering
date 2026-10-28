# Granseal epilogue interactions and Castle2F admission

Evidence date: 2026-10-22. **Confirmed (static):** ordinary Granseal/F543
provides fifteen entity-event rows, one west-side zone, a repeatable resident
scene and Astral's Yes/No invitation. Yes sets F981 before the escort requests
Castle2F19,26,30/Up. Castle init handles three optional allies and calls
`cs_62D06`; F981/F982 then select the royal-scene call or town music/fade.
These are source interfaces, not observed natural movement or a playable ending.

## Agreement, provenance and method

Standard rigor for [Issue #1186](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1186)
and [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
one coherent town-interaction → invitation → escort → castle-entry account.
Accepted base `f711cc8db5be5bffc4e79102cc057c078d327f41`, tree
`62bfd4ebcf50690f30b509ea0a28fe362e34eca4`; no stacked dependency.
Own this document and predecessor navigation only. Acceptance requires all
fifteen routes, defaults/search/zone/warps, dialogue IDs and ordered tokens,
complete two town scripts, command consumers, changed-state cases, and the
castle entry through its first subsequent scene call. Royal-scene execution,
indoor exploration, kiss, credits, extra battles, translation, implementation
and runtime collection remain outside this slice.

The entry owner is [Zeon aftermath and Granseal return](./zeon-aftermath-and-granseal-return.md#ordinary-granseal-entry-and-the-stopping-boundary).
Reuse [common scripting](./common-scripting.md), [common maps](./common-maps.md),
[interaction state](./map-event-interaction-state.md), [request consumption](./map-event-request-consumption.md),
[follow semantics](./roft-town-interactions-and-zynk-recruitment.md#actual-join-and-field-follow-effects)
and [search/award semantics](./dojo-exploration-and-sheela-recruitment.md).
Source paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`town/` means `data/maps/entries/map03/mapsetups/`; `castle/` means the
Map19 equivalent; `map/` means `code/common/scripting/map/`. Addresses and
packed words are hexadecimal, intervals half-open; IDs/coordinates are decimal.
The registered USA ROM identity remains SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
This source audit makes no fresh source/ROM equivalence claim.

Linear selected source and consumer inspection is sufficient: fifteen entity
routes, one zone, seventeen descriptions, nine warps and two short town scripts.
The unchanged init/script pair is18,285,979 bytes; provenance remains
`local/issue1005/pair.txt`. Its corpus has `cs_6283E`2 commands/8 encoded bytes,
`cs_628C8`14/68, `cs_6290C`17/106. The last sum excludes its inline action
payloads: address interval `$6290C..$6299A` is142 physical bytes. The two-script
file `$628C8..$6299A` is210 physical bytes; neither106 nor174 is its size.

One selected lowering answers the concrete consumer question: do the native
YesNo callback and destination/follow/warp sequence become executable typed
content? Its retained result is described below; no map/world preparation or
publication occurred. No pair regeneration, full-world work, seed, frame or
renderer collection. Direct recipes target<60s/<512MiB, peak **unmeasured**;
small evidence<2MiB and all new ignored output<4MiB under `local/issue1186/`.
These are replan signals, not permission to omit required facts. Work and memory
are linear in the existing pair plus selected sources; no model/API batch or
network acquisition is needed beyond repository coordination and CI.

## Ordinary population and interaction admission

**Confirmed:** the predecessor posts Map3,56,3/Down. Ordinary MainLoop checks
SwitchMap first: held640 substitutes earthquake Map4. Stable clear640 is the
Map3 condition used here. Map3's setup order in `data/maps/mapsetups.asm` is
default →609→506→543. `GetCurrentMapSetup` replaces its candidate for every held
flag, so stable543 wins over609/506. Population and init are separate reads;
flag changes between them do not form a transactional snapshot.

`town/pointertable_543.asm:$626A6..$626BE` selects the six sections in order:
Entities, EntityEvents, ZoneEvents, AreaDescriptions, Section5, InitFunction.
Population `$626BE..$62738` has the following complete declarations; all fixed
rows use eas_Init. `W(x,y,r)` denotes the walking origin and radius.
The event suffix is `Map3_21F_EntityEvent<n>` in
`s2_entityevents_543.asm:$62738..$62820`. Event-byte directions are retained
as source values, not predicates requiring Bowie to face that way.

| Logical actor | Population x,y/facing; sprite or walking | Event n / byte | Callback requests |
| --- | --- | --- | --- |
|128 Woman|4,10/R; Woman|0 /DOWN3|3919–3920|
|129 Worker|5,10/L; Worker|1 /DOWN3|3921–3922|
|130 Young Man|19,14/D; W(19,14,3)|2 /UP1|3923|
|131 Worker|10,23/D; Worker|3 /UP1|3924|
|132 Girl|20,21/D; W(20,21,1)|4 /UP1|3925–3926|
|133 Girl|26,9/U; Girl|5 /DOWN3|3927|
|134 Centaur Female|45,18/U; Centaur Female|6 /DOWN3|3928–3929|
|135 Woman|26,27/D; Woman|7 /UP1|script cs_628C8;3915–3918|
|136 Man|23,27/D; Man|8 /UP1|3930–3931|
|137 Guard|1,26/R; Guard|9 /UP1|3910–3911|
|138 Astral|42,9/D; Astral|10 /UP1|3932–3933; choice4033 or4034|
|ally1 Sarah|44,10/R; live ally sprite|11 /UP1|3934–3935|
|ally2 Chester|43,11/L; live ally sprite|12 /UP1|3936–3937|
|ally3 Jaha|40,11/U; live ally sprite|13 /UP1|3938–3939|
|ally4 Kazin|40,10/D; live ally sprite|14 /UP1|3940–3941|

`InitializeMapEntities` and `InitializeFollowerEntities` allocate persistent
followers before fixed rows. An ally already aliased to a follower consumes its
row tail without repositioning or replacing that follower script. Bowie is
physical0; ordinary nonallies and allies resolve through ENTITY_INDEX_LIST,
not logical-ID-as-slot. With **no** persistent followers,128–138 occupy1–11,
Sarah/Chester/Jaha/Kazin12–15; Woman135→8, Man136→9, Guard137→10, Astral138→11.
Extra followers shift slots and can cause ally-row reuse. These are supplied
allocation examples, not an observed party. Ordinary Man136 is distinct from
Map59 scene Woman136; ordinary Astral138 is distinct from that scene's133.

`RunMapSetupEntityEvent` stores the interaction-relative byte, reselects setup,
finds the first ID match or `$FD` default and obtains portrait/speech properties.
The event byte's bit0 controls turning toward the player; bit1 controls the
post-callback property restore. Thus UP1 and DOWN3 both turn, while only3 takes
that restore branch. It disables entity-update VInt before the callback;
`script` execution activates its own entity services. Return closes portrait/text
and reactivates entity update. DefaultEntityEvent is rts, with no town-specific
fallback dialogue. The selected init `$628C6..$628C8` is rts only.
These wrappers/services must actually complete; none was run here.

## What the residents say

**Confirmed:** the plain resident/guard callbacks (events0–6,8–9,11–14) return after their listed `txt` requests.
They do not revive Elis, award an item, select a kisser, recruit an ally or set
a story flag. Their reports about recovery, reconstruction or peace are dialogue
claims. The tables contain author paraphrases, not transcribed retail text.
For `txt`, the numeric ID is explicit. `cs_628C8` uses cursor3915 and four
successive requests: T=nextText, S=nextSingleText, modifier0 throughout.
Control tokens appear below in exact source order, including repeats.

| ID | Request / speaker | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|3910|txt/137|Guard questions his duty.|`{W2}`|
|3911|txt/137|Guard says enemies are gone.|`{W1}`|
|3912|txt/zone|Guard calls Bowie back.|`{W2}`|
|3913|txt/zone|Guard cites Astral's order against leaving today.|`{N}{W2}`|
|3914|txt/zone|Guard insists Bowie remain.|`{W1}`|
|3915|T/135|Woman envies Bowie and comments on her age.|`{N}{W2}`|
|3916|S/135|Woman laments a lack of kisses.|`{W1}`|
|3917|T/136|Man contradicts her.|`{W2}`|
|3918|S/136|Man offers a kiss.|`{W1}`|
|3919|txt/128|Woman asks whether Bowie heard the news.|`{LEADER}{W2}`|
|3920|txt/128|Woman reports color returning to Elis's face.|`{N}{W1}`|
|3921|txt/129|Worker greets Bowie happily.|`{LEADER}{N}{W2}`|
|3922|txt/129|Worker reports that Elis may awaken today.|`{N}{W1}`|
|3923|txt/130|Young Man credits Galam and Pacalon with reconstruction.|`{N}{N}{W1}`|
|3924|txt/131|Worker reports a Lemon memorial statue in Galam.|`{N}{NAME;28}{W1}`|
|3925|txt/132|Girl asks whether Bowie will kiss Elis.|`{LEADER}{N}{W2}`|
|3926|txt/132|Girl expresses jealousy.|`{N}{W1}`|
|3927|txt/133|Girl welcomes seeing Elis again.|`{N}{W1}`|
|3928|txt/134|Centaur warns against Chester kissing Elis.|`{NAME;2}{N}{W2}`|
|3929|txt/134|Centaur threatens a kicking response.|`{N}{W1}`|
|3930|txt/136|Man repeats a rumor and asks Bowie about the kiss.|`{N}{W2}`|
|3931|txt/136|Man wishes for youth.|`{N}{W1}`|
|3932|txt/138|Astral greets Bowie.|`{LEADER}{W2}`|
|3933|txt/138|Astral asks whether Bowie is ready for the castle.|`{N}{W1}`|
|3934|txt/ally1|Sarah acknowledges the required kiss.|`{N}{W2}`|
|3935|txt/ally1|Sarah worries about the chosen person.|`{N}{N}{W1}`|
|3936|txt/ally2|Chester doubts selection because he is a centaur.|`{N}{W2}`|
|3937|txt/ally2|Chester wonders who Astral will select.|`{N}{W1}`|
|3938|txt/ally3|Jaha offers to do anything for Elis.|`{N}{W2}`|
|3939|txt/ally3|Jaha hopes Astral will choose him.|`{N}{W1}`|
|3940|txt/ally4|Kazin says poison is gone but sleep remains.|`{N}{W2}`|
|3941|txt/ally4|Kazin connects Astral's summons with the kiss.|`{N}{N}{W1}`|
|4033|txt/138 Yes|Astral proposes visiting the king.|`{W1}`|
|4034|txt/138 No|Astral objects to delay.|`{N}{W1}`|

Debug text skipping does not advance script text cursors as ordinary dialogue
would; these are the unskipped linear IDs, not a debug-mode observation.

## Search descriptions, items and defaults

**Confirmed:** `town/s4_descriptions_543.asm:$62846..$628BE` loads base
`$FC3`=4035, selects byte_62856 and jumps DisplayAreaDescription. Every msDesc
has zero condition/function bytes. Exact coordinate match emits investigation
`423+kind`, then `4035+offset`; it closes windows and returns d7=−1.
No match returns0. Investigation kinds2/3/4/5/6 select425 sign,426 wash basin,
427 bookshelves,428 well,429 paper; each has ordered tokens
`{NAME}{N}{W2}{CLEAR}`. These do not mutate town story state.

| Coordinate | Kind / offset / text ID | Author paraphrase | Ordered tokens |
| --- | --- | --- | --- |
|9,22|2/0/4035|Weapons and items sign.|`{N}{W1}`|
|7,4|4/1/4036|Book about soldiers' conduct.|`{N}{W1}`|
|7,9|3/2/4037|Unpleasant soapy water.|`{N}{N}{W1}`|
|26,13|5/3/4038|A deep well.|`{W1}`|
|40,6|4/4/4039|Book about healing magic.|`{N}{W1}`|
|41,6|4/5/4040|Book about beginner swordsmanship.|`{N}{W1}`|
|42,6|6/6/4041|Paper about overcoming fear.|`{N}{N}{W1}`|
|43,6|6/7/4042|Paper urges caution before acting.|`{N}{W1}`|
|44,6|6/8/4043|Paper questions a maxim about pain and gain.|`{N}{N}{W1}`|
|53,13|4/9/4044|Book about Max's legend.|`{N}{W1}`|
|54,13|4/10/4045|Book about good conduct.|`{N}{W1}`|
|55,13|4/11/4046|Book about small dinosaurs.|`{N}{W1}`|
|56,13|4/12/4047|SEGA guide and approving reaction.|`{N}{N}{W1}`|
|56,15|4/13/4048|Book title prompts an Astral joke.|`{N}{N}{W1}`|
|57,15|4/14/4049|Castle guide.|`{N}{W1}`|
|58,15|4/15/4050|Study of Yogurt.|`{N}{W1}`|
|59,15|4/16/4051|Introductory science book.|`{N}{W1}`|

Ordinary C interaction first tries an entity; FieldMenu search can enter
CheckArea independently. Its faced-coordinate and working-layout-kind dispatch
precedes descriptions: chest`$1800`, vase`$2C00`, barrel`$3000`, bookshelf`$3400`
and area`$1C00` first use their item paths. A coordinate is not proof of a reached
search. Reuse the [accepted admission and item fallback](./elven-village-conversations-and-local-exploration.md#admission-before-the-coordinate-lookup).
Shared Map3 `7-chest-items.asm:$9793A..$97940` declares6,18/F220/NOTHING;
`8-other-items.asm:$97940..$97946` declares59,17/F128/MEDICAL_HERB.
Live collected flags and inventory capacity govern the common award/repair
path; no new automatic herb or empty-chest transaction follows from F543.
`s5_itemevents_543.asm:$628BE..$628C6` contains only the `$FD` default callback
and rts. RunMapSetupItemEvent starts d6=0; this callback leaves it0 for ordinary
item-use fallback, not success, consumption or a special epilogue item action.
No town event row invokes a shop/church service; the welcome sign is description
content. General menus/services and their successful natural use remain outside
this event audit.

## West-side restriction and all nine warp rows

**Confirmed:** `town/s3_zoneevents_543.asm:$62820..$62846` has
X2/Y255→Map3_21F_ZoneEvent0 and rts default.255 is a wildcard here.
There is no F981, F982 or extra flag guard. The callback synchronously runs
`cs_6283E:$6283E..$62846`, then requests3912,3913,3914 and returns.

`map/mapscriptengine_1.asm:csc28_moveEntityNextToPlayer` reads Bowie's
**destination**, resolves Guard137 through the physical alias table, and
subtracts MAP_TILE_SIZE384 from X for LEFT. It writes Guard X/Y destinations,
absolute travel distances and signed velocities48; waits until Guard position
matches destination, faces him RIGHT and refreshes his sprite, then waits for
Bowie's motion to stop. It does not teleport either actor, push Bowie back,
change the warp table or set a lasting exit-lock flag. There is no collision
query in csc28's destination assignment. Live scripts/services still control
whether its waits finish. At supplied Bowie destination2,y the target is1,y,
not an unconditional return to Guard's initial1,26.

The entity movement consumer `entityscriptengine_2.asm:loc_528A/loc_5294`
distinguishes working-layout mask`$3C00`: warp marker`$1000` invokes
WarpIfSetAtPoint; zone marker`$1400` posts event6 and computed candidate X/Y.
The later solidity/debug/collision checks are separate. ProcessMapEvent clears
the event word before dispatch; type6 installs Bowie's init script, calls the
current setup zone handler with posted coordinates, and the wrapper closes
windows/waits for Bowie. A source zone row alone does not create that layout
marker or prove approach geometry. In particular X2 zone and X0 west warp
are separate admissions. The guard's words and movement do not prove that
all routes are sealed or that a player can evade the restriction.

Map3 `6-warp-events.asm:$978F0..$9793A` is shared by all these setups.
WarpIfSetAtPoint matches the first coordinate row with signed-negative wildcard
bytes. All nine rows are no-scroll, with explicit destination/facing and **no
story-flag predicate in the warp table**. Each still needs its actual movement/
layout admission; none has been naturally walked in this slice.

| Trigger x,y | Destination / facing | State and selection boundary |
| --- | --- | --- |
|255,1|Castle2F19,26,30/U|Ordinary rebuild; no F981 prerequisite here. Castle setup/981/982 determine subsequent init.|
|0,255|Overworld Granseal66,29,32/D|Ordinary rebuild; X2 zone is separate. Destination battle/routing remains caller-dependent and outside this slice.|
|50,23|Docks44,1,25/R|Ordinary rebuild; no F543/F981 gate in this row. Docks exploration excluded.|
|50,24|Docks44,1,25/R|Same destination and boundary.|
|50,25|Docks44,1,25/R|Same destination and boundary.|
|46,7|CURRENT255→Map3,59,12/L|School stairs; negative-map ordinary path retains population/working layout.|
|59,12|CURRENT255→Map3,46,7/D|Reverse school stairs; same preserve boundary.|
|3,3|CURRENT255→Map3,54,3/R|Bowie's house stairs; same preserve boundary.|
|54,3|CURRENT255→Map3,3,3/R|Reverse house stairs; same preserve boundary.|

These rows do not write981/982 or clear collected flags. Common entry still
heals living/immortal allies and performs its normal prelude; CURRENT255 keeps
population, scratch/temporary state and working-layout changes, whereas a named
map rebuild initializes population, clears256–383 and sets80. Setup init is
reselected in either path. F543 and F981/F982 are outside that temporary clear;
there is no save or persistence observation. Preserve semantics and generic
healing limits retain their [accepted owner](./roft-town-interactions-and-zynk-recruitment.md).

## Woman135, Man136 and the complete resident scene

**Confirmed:** EntityEvent7 calls `cs_628C8` then rts; no flag test or write
makes the scene one-time. EntityEvent8 independently emits3930/3931: speaking
to Man136 does not call the woman's scene. Full `cs_628C8:$628C8..$6290C` order:

1. Cursor3915; Woman135 T3915 then S3916, modifier0.
2. setDest Bowie25,28; setFacing Bowie UP.
3. setDest Man13625,27; setFacing136 RIGHT; setFacing135 LEFT.
4. Man136 T3917 then S3918, modifier0; csWait10.
5. setActscriptWait135,eas_Jump twice; end.

`csc29_setEntityDest` resolves the logical actor, multiplies each coordinate
by384, writes destination/travel/signed velocity32 and waits unless operand
bit15 is set. Both actual operands have that bit clear. It changes neither
current position immediately nor collision flags. setFacing follows completed
motion and refreshes mapsprite state; it is not part of an atomic relocation.
`eas_Jump` disables autofacing, sets relative0,−1, waits5, sets0,+1, waits5,
sets0,0, reenables autofacing and jumps eas_Idle. The relative moves use current
position; there is no waitDest between them and no inferred exact jump height
or frame duration. Woman's installed eas_Init has obstruction bit7 set and
collision-check bits5/6 clear; Jump changes autofacing, not those collision bits.
The source names containing “Uncollidable” are not an independent collision
interpretation. Neither this script nor the man callback records a kiss outcome.

csc15 and csc2D install the resolved **physical slot byte** in
ACTSCRIPTWAITTIMER. In the no-follower example the two Woman scripts each
install8, not0; Astral's escort blocks each install11, not138. A later executed
`ac_jump eas_Idle` passes through esc_clearTimerGoToNextCommand and clears the
timer. Installation is distinct from that later execution. Waiting predicates
observe the installed script pointer reaching eas_Idle; the scheduler and
initial timer state preclude deriving a frame count from these instructions.
This preserves the [accepted timer correction](./zeon-aftermath-and-granseal-return.md#entrances-actor-continuity-and-command-semantics).

## Astral's choice and complete escort

**Confirmed:** EntityEvent10 emits3932,3933, calls j_YesNoPrompt, tests D0.w,
and takes `bne.s byte_627F2` on nonzero. j_YesNoPrompt is the jump-interface
alias of YesNoPrompt in `code/common/menus/yesnoprompt.asm:$15284..$154A2`.
The menu initializes choice0/Yes; left selects0, rightFF, B cancels toFF,
A/C accept the selected byte. Its return sign-extends that byte: Yes0,
No/cancel−1. After window movement/deletion it returns D0; no menu-local
F981 write exists. This direct D0 callback is not map-script `yesNo`/F89.

| Returned D0 / existing F981 | Callback suffix if services return |
| --- | --- |
|0 /clear|4033 →setFlg981 →script cs_6290C →return.|
|0 /held|Same4033, same set981 and same script; no one-time guard.|
|−1 /clear|4034 →return; no flag write or escort.|
|−1 /held|4034 →return;981 remains held. No clear or rollback.|
|Other nonzero supplied value|Same nonzero branch; not an additional natural menu result.|

F981 is written **before** the script. Interruption at later services can leave
it held without any castle arrival. The callback does not inspect982 or543;
selection/admission supplied its setup. All five setDest requests below have
bit15 clear and are sequential waits, including each ally request to the same
coordinate. They are not five simultaneous setPos operations.

Full `cs_6290C:$6290C..$6299A` order:

| Stage | Exact ordered requests | Completion boundary |
| --- | --- | --- |
|Close/start|hideText; entityActionsWait138: down1|Closes portrait/text; waits Astral script idle.|
|Gather|setDest Bowie42,10; Kazin42,10; Sarah42,10; Jaha42,10; Chester42,10|Each destination write and motion wait completes before the next request.|
|Install follows|Bowie→138 offset2; Kazin→138 offset5; Sarah→138 offset6; Jaha→Kazin offset2; Chester→Sarah offset2|Five separate installations; live Bowie HP prefix guard applies to each.|
|First leg|entityActionsWait138: down1,left1,down1,down1,down1|Relative destination/wait per move, then script idle.|
|Second leg|entityActionsWait138: down3,left6,eaWait10|Waits this Astral script only.|
|Third leg|entityActionsWait138: up8,left6,up2,eaWait10,up6|Waits this Astral script only.|
|Post departure|warp Castle2F19,26,30/Up; end|Posts event1; does not synchronously initialize Castle2F.|

csc2C reads the **first byte of the follower word** for its HP precheck;
all five small nonnegative logical IDs have high byte0. Thus each call tests
live Bowie0, even Jaha/Chester/Kazin/Sarah, before resolving the full follower
and leader words. Bowie HP0 skips each six-byte follow operand group; positive
HP allows alias resolution and AddFollower. This is not a per-follower HP test.
No constant HP/party state is assumed; the earlier ordinary healing prelude
does not justify ignoring this boundary in a supplied or interrupted state.

The follower-position table is signed **pixels**:2=(−24,0),5=(−24,−24),
6=(−24,+24). AddFollower copies42 bytes of eas_Follower1 into walking scratch,
patches leader physical slot and signed offsets at `$1E/$20/$22`, installs that
script and adds the follower's slot to EXPLORATION_ENTITIES if absent. It does
not set persistent follower flags66–84 or a join flag. `esc03_follow` uses
current separation, leader destination/facing, rotated offsets and layout
fallback; offsets are not fixed world-coordinate displacements. Follower
script flags clear obstruction7, set collision bits5/6, and configure speed32;
their source names do not promise a collision-free walk.

Each entityActionsWait installation seeds the physical timer and clears bits5/6
with AND9F. Relative moves wait for their destination; eaWait copies a timed
action. End jumps idle. Only Astral is waited; trailing followers are not joined
into an all-party completion barrier. If every source movement finishes from
the declared42,9 start, Astral's requested route ends29,1; that arithmetic is a
conditional destination calculation, not observed geometry or proof that all
followers arrived. The later warp posts Bowie26,30 in a different map.

`mapscriptengine_2.asm:csc07_warp` writes MAP_EVENT_TYPE.w=1, param1.b=0,
map19 and X26/Y30/facingUP bytes. Script/interaction return precedes ordinary
WaitForEvent consumption. ProcessMapEvent first clears the event word;
type1/no-scroll makes Bowie idle, unwinds the exploration return, supplies
map/coordinate/facing and returns to MainLoop. SwitchMap/CheckBattle precede
ordinary19 exploration. There is no Map19 switch row or battle-map trigger;
this script does not itself perform the subsequent init or save. The rebuild
reallocates actors; temporary follow scripts/physical slots are not a durable
castle entourage. Persistent follower flags, if supplied, still participate.

## Castle2F entry and the next scene boundary

**Confirmed:** `MAP_GRANSEAL_CASTLE_2F` is19. Its setup sequence is
default →501→609→506→507→543→982; last held wins. Stable543 with clear982
selects543; held982 overrides543, even in a supplied state with543 clear.
Population and init selection remain separate reads. `castle/pointertable_543`
`$6299A..$629B2` selects the ordinary543 six sections. `pointertable_982`
`$629B2..$629CA` changes Entities/EntityEvents but reuses543 zone, descriptions,
item-event and **init** pointers. This is not a new982-only init implementation.

The543 population `$629CA..$62A6C` declares20 rows: Slade, Peter, May, Rohde,
Luke, Higins, Sheela, Chaz, Zynk; hidden-at63,63 Sarah/Chester/Jaha/Kazin;
King128 at29,6/D, Astral129 at63,63/D, Old Woman130 at24,16/R;
walking Guards131/132 at23,26 and34,26 radius2; fixed Guards133/134 at14,25
and17,25/D with eas_InitFixedSprite. The first thirteen are ally identities,
not nonally128 onwards.982's18-row population `$62A6C..$62AFE` omits Sarah
and Kazin, repositions remaining principal actors, and starts Astral129 at29,9/D;
these tables describe fresh declarations, with the same follower-reuse caveat.
No town logical138 continuity is assumed.

`castle/s6_initfunction_543.asm:ms_map19_flag543_InitFunction`
`$62C9E..$62D06` checks in this exact order:

| Live flag | Held | Clear |
| --- | --- | --- |
|25 /Sheela joined|Skip removal.|D0=Sheela25; MoveEntityOutOfMap.|
|8 /May joined|Skip removal.|D0=May8; MoveEntityOutOfMap.|
|27 /Chaz joined|Skip removal.|D0=Chaz27; MoveEntityOutOfMap.|

The service resolves the ally's physical alias and calls SetEntityPosition
with X/Y`$7000`. It moves the actor off-map; it does not delete that alias,
clear a joined flag or recruit another character. All combinations of these
three flags flow to the **first subsequent scene call**, `script cs_62D06`.
Its entire helper `$62D06..$62D0E` is setSprite Rohde→MAPSPRITE_NPC_ROHDE,
then end. This sprite request does not set/clear Rohde's join state.

The remaining init branch topology identifies the next owner without entering
the royal script: clear981→MUSIC_TOWN/FadeInFromBlack/rts; held981/clear982→
`script cs_62D0E`, then set982, write EXPLORATION_ENTITIES first byteFF and
return; held981/held982→MUSIC_TOWN/FadeInFromBlack/rts. The982 write occurs
**after** cs_62D0E returns, not at castle admission. A retained/supplied982
still selects the982 population with clear981 and takes the no-royal branch.
The royal script starting `$62D0E`, its text, movement, kiss, credits,
indoor zone/entity events and any later ending transitions belong to the next
slice. No claim of their completion or one-time save semantics is made here.

## Actual content, importer and runtime consumers

**Confirmed:** the selected producer run returned without exception, with
no prepared world. It registers just the town entity-event, zone and scripts_2
files, allows script destinations3/19, and compiles the seven named entries
in the local recipe. One shared return program appears through normal calls:
eight total compiled programs are retained. Compilation is not execution or
proof of published scene content.

| Actual owner | Support, refusal and publication boundary |
| --- | --- |
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|cs_6283E lowers to5 instructions: move-next-to-player, wait Guard motion, face RIGHT, wait Bowie motion, end-map-script. Zone callback is8 instructions with call, three text-cursor/show-text pairs and end. Source table admission/layout marker still needs map content.|
|Same producer|cs_628C8 has28 instructions; typed10 and12 are native setDest (source3/5). Text prefixes are representable; these destination requests stop execution before later facing/dialogue/jumps. eas_Jump is one cached9-operation action stream.|
|Same producer|Astral callback has12 instructions; typed4/5/6 are native jsr/tst.w/bne.s (source2/3/4). It emits no typed choice for this raw subroutine. Later flag/call instructions cannot bypass the first native stop. byte_627F2 separately lowers to3 instructions; that does not connect the raw D0 branch.|
|Same producer|Escort has18 instructions: close portrait/text, four motion installations, five native setDest at typed3–7, five typed follows, queue-warp19/26/30/U, end. Follow lowering resolves offsets but does not reproduce csc2C's live Bowie-byte HP skip. No legitimate reached escort execution follows from these typed suffixes.|
|[Map preparation](../../src/sf2tool/remake_exploration_maps.py), [final preparation](../../src/sf2tool/remake_exploration_content.py)|Selection/setup resources bind populations and optional eventMaps. Each warp is retained: selected supported no-scroll destinations become warp entries, CURRENT255 becomes preserve; unadmitted destinations become explicit warp-frontier programs. Interactions/zones come from selected setup references and default rows. Search/item service content is not supplied by these event loops. This run did not invoke compile_maps or final preparation, admit an epilogue package or publish map variants.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [story types](../../remake/src/Sf2.Remake.Application/Content/Scenarios/StoryProgram.cs)|Typed follow/motion/choice/queued-warp exist. native-call maps to UnsupportedInstruction; native actions map to UnsupportedEntityAction. ChooseYesNo writes a result flag, unlike this direct D0 callback. Reader support is not imported543 content or exact branch semantics.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|UnsupportedInstruction throws program-opcode before Next/commit; earlier prefixes remain committed. MoveEntityNextToPlayer writes destination/travel/signed48 velocities, followed by typed waits/face. Motion SlotTimer and SlotTimerClearCollision seed entity.Slot; the latter also AND9F. QueueMapWarp stores ScriptWarp; null-cursor handling begins transfer. This is separate publication per instruction, not source-transaction rollback.|
|[Choice runner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationChoiceRunner.cs), [dispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs)|Detailed choice phases support entry-held/release, left/right/cancel/confirm, close and return delay with result-flag publication; simple ChooseDialogue requires ChoiceWait with Work:null. Neither executes arbitrary j_YesNoPrompt/tst/branch assembly. Astral needs a deliberate callback conversion, preserving0/nonzero polarity and write order.|
|[FollowerMotion](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FollowerMotion.cs), [EntityMotion](../../remake/src/Sf2.Remake.Domain/Maps/EntityMotion.cs)|Install binds physical leader slot and signed pixel offsets; it clears obstruction, sets collision bits and initializes motion/timer. Domain Follow uses separation, rotated offsets, passability fallback and current live motion. These mechanisms do not supply the omitted original HP precheck or exact original scheduler.|
|[Allocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Allocator supports follower-before-fixed alias reuse. Ordinary transfer rebuilds from selected content/current flags; preserve retains the working world. Producer recognizes MoveEntityOutOfMap as hide/removeAliases:false and Rohde's NPC setSprite as typed sprite. Availability of these entry services does not establish the unimported castle/royal scene.|
|[Adapter view](../../remake/game/src/Exploration/ExplorationSessionView.cs), [presentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [capability owner](../../remake/docs/capability-status.md)|The adapter projects choice state and sends held/release/PollChoice input; movement/follower/render projections are reusable. No epilogue instance, selected materials, real input, service completion or natural player-ready castle was observed. Earlier accepted Map3/Battle01–03 capabilities are not F543 ending support.|

**Inferred:** the generic mechanisms can be reused after selected content and
the raw-choice, blocking-destination and original HP-prefix differences are
resolved by an explicitly scoped integration task. No new engine/importer,
allowlist, silent native skip, or callback repair is authorized by this account.

## Reproduction, retained failures and open boundary

Ignored `local/issue1186/observe.py` is the sole selected inventory/lowering;
its fresh observe-01.json retains source/pair identities, full selected output,
action stream, native locations and any partial/error state. Do not rerun it
during ordinary review. `review.py` directly checks pinned source/control order,
complete rows/tokens, corpus/source agreement, supplied-state examples, physical
slot/timer/follow semantics, actual consumers and owned prose/links/privacy.
It reuses the existing receipt without lowering, snapshots or pair regeneration.
The complete local recipes are review aids, not new maintained parsers/tests or
durable evidence owners. This tracked account, named source and exact Git
objects own the facts. Independent review uses a fresh nonexistent receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1186/review.py <configured-source-disasm-root> --receipt local/issue1186/reproduction-01.json --committed
git diff --check origin/main...HEAD
uv run sf2 verify plan --base origin/main --head HEAD
```

Every uv command loads the worktree's configuration in that same process.
Detailed invocation, durations/output sizes, exact base/head/tree, planner,
actual exact-head scoped CI, failures and stopped-process state belong in the
frozen Issue/PR and ignored handoff. No ROM blocker or fresh ROM observation
was claimed. Normal/full/SDK/Godot/emulator/H3/H4, screenshots, old suites and
old comparison reruns are **SKIPPED**, not passed, under this documentation
acceptance. ADR0004's relevant provenance/date/order/width/alias/changed-state/
actual-consumer/private checks apply; new schemas, maintained-parser mutation
suites and H3 callback closure are N/A.

Preserve [all predecessor failures and Unknowns](./zeon-aftermath-and-granseal-return.md#reproduction-failures-and-open-boundary),
including #1184's initial corpus-program error, spacing/snapshot-review failures,
original author/main PASS and CI that missed physical-slot installation timer
and equipment-refreshed CURSE prose, independent P2 findings and focused fixes.
Those results are retained in `local/issue1184/handoff.md`; no old recipe,
comparison or lowering is rerun or replaced with a green result. Earlier
partial/native/failed import and twelve completed old comparison failures
retain their accepted owners. Current orientation wildcard/nonexistent-file
read errors and the rejected oversized write command caused no source writes;
these remain in the local handoff. Direct review01 failed because its local
guard placed the Y destination write after the first velocity write; the source
writes both destinations first. Review02 failed on byte comparison of Windows
CRLF working code against LF Git objects; normalize only that comparison.
Both failed receipts remain. Neither changed original source or lowering output.
Self-review also corrected the Centaur event byte to DOWN3, conditional escort
endpoint to29,1 and an overbroad royal-script range before acceptance. These
are candidate prose corrections, not new runtime evidence.

**Unknown:** natural zone/warp/interaction reach, actual geometry and bypasses,
live flags/aliases/followers/party/HP/status/inventory/debug input, service
completion and interruption prefixes, exact scheduler/follow positions/timing,
source-ROM equivalence, rendered/audio/material/portrait results, input
restoration, save/load persistence, and royal-scene/kiss/credits/ending or
original/remake playability. Static completeness creates no runtime-work queue.
The next bounded owner starts with castle's royal-scene admission above;
it must not assume this document executed it. Freeze clean pushed Draft PR for
independent main-gate acceptance; same task owns corrections. No integration,
cleanup, archival, ref deletion or next dispatch is part of this slice.

Continue with [Granseal royal council and chamber admission](./granseal-royal-council-and-chamber-admission.md) for the complete royal scene, companion departures, post-council interactions and ordinary Map20 entry.
