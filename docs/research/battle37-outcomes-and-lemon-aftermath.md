# Battle37 outcomes, Lemon aftermath and exploration return

Evidence date: 2026-10-21. **Confirmed (static):** with Bowie alive,
Red Baron128's death can queue the remaining living enemies for removal.
Both terminal checks give defeat precedence. Battle37 has no enemy-drop
record; Red Baron's equipped Dark Sword is not an awarded victory item.
Clear537 runs the complete Lemon scene; held537 skips it. Scene Lemon28
is an ally-character alias, distinct from defeated enemy128. Neither the
scene nor its after-join tail recruits Lemon. The scene does not replace
the earlier return seed: exploration rebuilds Map77 at the first active
member's actual position plus crop16,32, with that member's aliased facing.
Natural reach, completed services, usable control, presentation and
persistence remain **Unknown**. Lowering is not playable acceptance.

## Agreement, provenance and method

Standard rigor for [Issue #1140](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1140)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
connect accepted [initialization](./battle37-initialization-and-opening.md)
and [activation/dispatch](./battle37-activation-and-first-turn-dispatch.md)
at accepted base `dbaf00a57854c5a93d82c6e5e9964c39a1972471` to both
outcomes, the complete after scene and exploration return. Own this new
document and navigation only in activation/dispatch. No shared index,
registry, schema, fixture, tool, design or engine changes, competing
writer or stacked dependency. Stop at Map77's adjacent exit interfaces;
town content, later Lemon recruitment, Battle38 initialization, Red Baron
Desoul/action execution and whole-battle simulation remain outside scope.

Reuse [Battle36 outcomes](./battle36-outcomes-and-galam-return.md),
[Battle35 shared outcome/drop/return](./battle35-outcomes-and-roft-return.md),
[corrected death accounting](./battle34-outcomes-and-sky-orb-handoff.md),
[Map77 connections/admission](./roft-to-galam-and-battle36-admission.md),
[battle loop](./battle-loop.md), [cutscenes](./battle-cutscenes.md),
[scripting](./common-scripting.md), [maps](./common-maps.md),
[request production](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[lifecycle](../design/contracts/battle-control-lifecycle.md) and
[routing](../design/contracts/battle-cutscene-routing.md).
Those accepted owners retain common semantics; this owner checks selected37
operands, complete source joins, bounded changed states and actual consumers.

Original paths are relative to clean read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`; `map/` and `entity/`
abbreviate children of `code/common/scripting/`. sf2const.asm,
sf2enums.asm and source macros own widths, constants and encodings.
Fnnn means story flag nnn; ordinals are zero-based, intervals half-open.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No new source/ROM equality, decoded terrain or runtime observation is claimed.

Minimum evidence is selected tables/scene/helpers, accepted immutable
18,285,979-byte init/script pair with `local/issue1005/pair.txt` provenance,
one existing-compiler lowering and actual production readers/rules/runner/
adapter. Selected reads and one pair load are linear in these bounded inputs;
small ignored receipts belong in `local/issue1140/`. Each direct command
targets one minute/512MiB; peak memory is unmeasured. No acquisition, full
map import/export, new parser/platform/fixtures, verifier tests, normal/full/
old comparison suites, SDK/Godot/emulator/H3/H4 or screenshots. Reassess
material contradictions or changed prerequisites before expanding.

## Terminal selection, leader cleanup and no Dark Sword drop

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` returns from
ExecuteIndividualTurn, optionally runs debug kill-all, then calls
ExecuteBattleCutscene_Defeated, ProcessKilledCombatants and
CountRemainingCombatants. Zero allies selects BattleLoop_Defeat before
zero enemies selects BattleLoop_Victory. A nonterminal pass reads the
actual turn-buffer actor, applies ProcessAfterTurnEffects, processes
deaths and counts again with the same priority. There is no second
defeated hook. Only a second nonterminal result increments the byte turn
cursor by2. An admitted retreat unwinds before this suffix.

| Complete table / selected37 use | Selected value |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, base `$47BE8`|slot37/byte74 at `$47C32` → ms_Empty, sole FFFF word.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, base `$47C8E`|byte37 at `$47CB3` → -1, nonzero leader cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, base `$47CF4`|slot37/byte74 at `$47D3E` → abcs_battle37 `$4DB94`.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, base `$47D6A`|byte37 at `$47D8F` →0, passed to JoinForce.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5 has a row; no37 post-leader position override.|

`battle/cutscenes/battleendcutscenesstart.asm` requires Bowie0 HP.w!=0
and enemy128 HP.w=0. Here128 is combat Red Baron94, not ally Lemon28,
the opening scene's Red Baron129 or its Geshp128. Clear537 interprets
ms_Empty; held537 skips that interpreter call. Both reach
`battleendcutscenesend.asm:loc_47C48`, reread CURRENT_BATTLE.b and use
its leader byte. Unchanged37 scans all32 enemies128–159, appending every
nonzero-HP ID to the existing death list, updating its word length and
FF sentinel. It does not clear the list or test X/Y, membership,
neutral/activation state or the eighteen-row declared roster.

ProcessKilledCombatants requests death presentation/waits. A listed ally
increments its own defeat count; only the enemy branch temporarily uses
BATTLESCENE_FIRST_ALLY for kill accounting, then restores the enemy ID.
The listed combatant receives X/Y=-1, status clear and stat refresh;
its aliased physical entity moves to$7000,$7000. The helper does not set
HP0, clear list length or call DropEnemyItem. Living enemies appended
by leader cleanup can disappear from counts through X without an action
kill/drop. Stale/duplicate lists and aliases remain caller inputs.

CountRemainingCombatants scans all30 allies/32 enemies, testing signed
X.b>=0 and HP.w!=0; it ignores Y, joined/active membership, neutral and
activation bits, and declared enemy count. High-bit nonzero HP counts.
Bowie HP0 finally forces ally count0. ApplyPositionsAfterEnemyLeaderDies
uses the same Bowie/enemy128 gate but a separate word-battle lookup;
the after-battlescene listener misses37 and does no repositioning.

| Supplied state at the named seam | Consequence |
| --- | --- |
|Unchanged eighteen positioned/nonzero-HP STARTING enemies|All eighteen count even with active bits clear.|
|Bowie alive;128 HP0; other seventeen nonzero HP|Leader sweep can append those seventeen; returning cleanup removes their X from the count.|
|128 alive/positioned; all seventeen others gone|128 keeps enemy count nonzero. LEADER AI is not the terminal test.|
|128 alive but every enemy fails X/HP count|Victory can occur without the defeated hook's death gate.|
|Bowie HP0; another living ally; zero enemies|Wrapper skips and forced ally0 selects defeat. Both counts0 also selects defeat.|
|Bowie alive but every ally off-map|Zero allies still selects defeat.|
|128 becomes HP0 only during after-turn effects|No second defeated hook; surviving positioned enemies can keep the battle running.|
|537 held|Empty hook skips, but gated leader cleanup and both count priorities remain.|

`data/battles/global/enemyitemdrops.asm` is the complete30-record,
four-byte table plus word terminator (`$BE52..$BECC`). There is **no37
record**, no selected enemy/item/drop-bit tuple. The three action-side
DropEnemyItem calls reuse the [accepted live battle/target/item/drop
admission](./battle35-outcomes-and-roft-return.md#heat-axe-declaration-actual-inventory-and-deals);
an unchanged live37 fails the table match before removal/award/Deals.
Red Baron's actual equipped DARK_SWORD has index70; definition inventory,
spell-use properties and enemy death do not add a table row. Victory,
leader cleanup and abcs_battle37 are not independent drop callers.
The selected outcome supplies neither a Dark Sword award nor Lemon recruitment. Individual
action/Desoul execution and changed selectors at other drop seams remain
outside scope; existing independent item/drop state is not reset by537.

## Victory seed, repeat skip and defeat/Egress selection

**Confirmed:** `battle/battleloop_2.asm:BattleLoop_Victory` first calls
HealLivingAndImmortalAllies. All30 slots are visited; living allies and
Peter7/Lemon28 even at HP0 get maxHP/maxMP, retain STUN/POISON/CURSE
and refresh stats. Other dead allies are skipped, regardless of membership.
An immortal whose maxHP is0 can still be HP0 afterward. Battle44's timer
special case is not37.

Before the after wrapper, CURRENT_MAP.b goes to MAP_EVENT_PARAM_2.
UpdateForceAndGetFirstBattlePartyMemberIndex rebuilds force lists and
reads the **first active-list byte**. Its combatant X/Y receive byte
BATTLE_AREA_X/Y additions into PARAM3/4; its actual physical alias supplies
facing to PARAM5. PARAM1=0 is written without posting event TYPE.
The [shared seed owner](./battle35-outcomes-and-roft-return.md#victory-seed-empty-after-wrapper-and-completion-tail)
retains empty/stale-list, unchecked alias and word/longword boundaries.
No alive/positioned check validates this selected member or alias.

Unchanged map77/crop16,32 therefore saves77 and wrapped memberX+16,
memberY+32, with that member's facing. Supplied member19,28 gives35,60;
250,250 gives10,26. These arithmetic examples are not measured endpoint
positions. The member need not be Bowie or the last actor. This seed is
neither view31,54 nor mainEntity36,57 nor Lemon's declared35,59 or later
movement destination. Alias0 can read physical0; FF can shift to a read
at ENTITY_DATA+$1FE0+$10, without LoadBattle's fallback/3F mask.

`battle/cutscenes/afterbattlecutscenesstart.asm` tests
500+CURRENT_BATTLE: clear537 selects abcs_battle37; held537 skips the
**entire scene**, including text/population/priorities/motion. Both join
`EndAfterBattleCutscene`, reread the live selector and call JoinForce
with table byte0. This sets Bowie's joined0, rebuilds lists, and sets
active32 only below twelve active members. An already held32 survives
capacity; a newly set32 is absent from the just-built list snapshot.
This tail joins neither Lemon28 nor Zynk26 and does not run a join scene.

After wrapper return, victory rereads CURRENT_BATTLE, clears400+index
and sets500+index: unchanged37 clears437, sets537. Intro487 is untouched.
It clears d0–d3 **words**, copies PARAM2–5 bytes, returns d4=1. Upper
longword halves are not established by clr.w. PARAM1=0 differs from d4=1.
Each helper/table uses its own live selector read: a changed selector
between completion test, scene dispatch, join and completion tail can
select different entries. Reached healing/seed/script/join/clear/set
prefixes are not transactional, and537 is not proof of finished services.

Defeat first calls UpdateBattleUnlockedFlag: clear437 only when537 is
already held. It clears dialogue name1, requests sad music/txt363/close,
restores Bowie HP=max, halves current gold by longword logical shift,
selects egress and returns d4=-1. It newly sets neither537 nor487 and
does not execute after/join/victory tail. Battle4's losable branch is not37.

Reuse [admitted Egress/Angel Wing resource and unwind semantics](./battle35-outcomes-and-roft-return.md#defeat-egress-and-angel-wing-actual-destination-state):
MP or item removal/refresh precedes its text/sound/flash; the admitted
retreat updates unlock, unwinds the action frame, selects egress and
returns clr.w d4. It bypasses terminal suffix, defeat gold recovery,
after wrapper and completion. No full action simulation follows here.
`battle/battleloop/getegresspositionforbattle.asm` has special battle
IDs16/17/13/31/32 and unlock clears38/39, **no37 case**. It reads live
EGRESS_MAP.b then GetSavepointForMap. Held399 scans23 rows: map53 or77
has no row and retains its map byte with1,1/Up; supplied Roft38 uses
21,27/Right. Clear399 instead selects initial3/56,3/Down. Held64 also
updates raft state; the shared savepoint owner retains that branch.
No actual last save, visited town or EGRESS_MAP value is established.

## Complete Lemon scene and identity boundaries

**Confirmed:** `data/battles/entries/battle37/cs_afterbattle.asm`
spans `$4DB94..$4DDA4`. abcs_battle37 starts at$4DB94; the only
flag branch joins cs_4DBEE at$4DBEE. The complete ce_4DD8C table is:
mainEntity36,57,Down; Peter33,59,Right/eas_Init;
Lemon35,59,Down/eas_Init; cscEntitiesEnd. Its24 bytes comprise a
six-byte main header, two eight-byte records and word terminator.
The scene contains no joinForce/joinBattleParty/addFollower/followEntity,
set/clear flag, battle-start, item delivery or warp command.

Here B=Bowie0, P=Peter7, L=Lemon28, A=FOLLOWER_A30 and
S=FOLLOWER_B31. S is a **scene reference**, not an unconditional Astral
identity; ordinary follower flags choose its physical occupant. Text IDs
refer to the next section. The complete source command order is:

| Ordered block | Complete selected sequence |
| --- | --- |
|Entry|textCursor2972; loadMapFadeIn77,31,54; loadMapEntities ce_4DD8C.|
|Setup|setActscriptWait B,Init; P,Init; setPos P33,59,Right; setActscriptWait S,Init; setPos S34,57,Down; setActscriptWait A,Init; setPos A37,59,Left.|
|Only branch|jumpIfFlagClear76→cs_4DBEE. Held76: setActscriptWait Zynk26,Init; setPos26,63,62,Down; fall through.|
|Lying pose|stopEntity L; customActscriptWait L: ac_motion OFF, ac_orientLeft, ac_updateSprite, ac_jump eas_Idle, ac_end delimiter. setPriority S,0; L,FFFF; fadeInB.|
|First concern|entityActionsWait P:Right1/endActions; csWait50; setFacing P,Up; single2972 P,2973 S.|
|Mask|entityActionsWait S:Right1,Down1/endActions; csWait30; setFacing P,Right; single2974 S,2975 no speaker/FF,2976 L/C0.|
|Recognition|setActscript P,Jump; setActscriptWait S,Jump; repeat those two commands; single2977 S.|
|Inspection|entityActionsWait B:Down2/endActions; setFacing B,Left; shiver P; single2978 P; shiver L; stopEntity L; entityFlashWhite L,57.|
|Awakening|setActscript P,Jump; B,Jump; setActscriptWait S,Jump; single2979 P,2980 S,2981 L/C0.|
|Recovery pose|setActscriptWait L,Init; startEntity L; setFacing L,Down; single2982 L/C0; csWait5; setActscript L,2xRightLeft; csWait120; setFacing L,Up.|
|Recollection|nextText2983 L/C0; single2984 L/C0,2985 S; setActscriptWait L,Jump; single2986 L/C0.|
|Explanation|nextText2987 S; nextText2988 S; single2989 S; setFacing L,Down; single2990 L/C0.|
|Distress|entityActionsWait L:Down2/endActions; shiver L; setFacing P,Down; B,Down; single2991 L/C0; setFacing L,Down; single2992 L/C0.|
|Reassurance|entityActionsWait S:Down1/endActions; nextText2993 S; single2994 S,2995 L/C0; shiver L; csWait50; shiver L; setFacing L,Up; single2996 L/C0.|
|Departure|setActscript P,Jump; setActscriptWait B,Jump; single2997 L/C0; setFacing L,Down; csWait30; entityActionsWait L:Down3/endActions; single2998 S.|
|Closing|setFacing P,Right; nextText2999 P; single3000 P,3001 S; setFacing S,Right; B,Left; single3002 S; csc_end.|

Unmarked dialogue flags are0. Init/Jump/2xRightLeft name eas_Init,
eas_Jump/eas_2xRightLeft. `single` means nextSingleText; `nextText`
retains its open-window behavior. No HP/class/alias condition in this
table branches to a recruitment path. csc_end returns to the wrapper.

### Population, HP, class and alias reads

`map/mapscriptengine_1.asm:csc37/csc48` requests black fade, detaches
view, loads tilesets/map77 with packed camera31,54 and VInt/display
services. It does not invoke fresh exploration setup/init/CheckBattle.
csc42 separately clears/rebuilds entities and loads their sprites before
reactivating display/VInts. `map/mapfunctions.asm` creates followers
before explicit records and main physical0. An existing nonzero ally
alias suppresses its explicit duplicate: F71 can make Lemon's explicit
35,59 record skip, just as an existing Peter alias can skip33,59.
Peter is then explicitly positioned; Lemon has no setPos to repair that
changed starting state. Table coordinates alone do not fix every run.

The complete `data/scripting/entity/followers.asm` has separate S31
choices through F74/F68/F70 and A30 choices through F74/F77/F83;
F71 selects Lemon28 and F76 selects Zynk26, independently of joined,
active or HP flags. On overworld77, held65 prepends Caravan/A30 before
the ordinary table. Later duplicate reference assignments can overwrite
its alias while earlier physical entities remain. Missing reference
aliases are0 after ClearEntities; unguarded commands can then address
physical0. No fixed S/A/26 physical slot is assumed.

Lemon's table entry has sprite operand ALLY_LEMON=28, below ally count30:
it declares/resolves **ally reference28**, not a new enemy128 and not
a conversion or resurrection of Red Baron128. GetAllyMapsprite reads
live class; table_AllyMapsprites[28] is LEMON_SPECIAL57. With unchanged
start class RDBN31 (>=BDBT23), it keeps57. Other classes take their
actual class branches. While CURRENT_BATTLE remains37 the nonbattle
HP0→BLUE_FLAME branch is bypassed; only current battleFF enables it.
Neither the scene nor its entity declaration changes class, HP, joined28,
active60 or follower71. Victory's earlier immortal healing is separate.

GetEntityAddressFromCharacter masks the character word to FF, subtracts96
for enemy-bit IDs, then reads the physical alias without zero/FF/capacity
validation. It does not infer a valid actor from a name. setPos/setFacing
read a **byte ID** for AdjustScriptPointerByCharacterAliveStatus: ordinary
ally HP0 skips; pseudo30/31 bypass. Zynk can be declared by76 and get
Init through unguarded csc15 even when HP0 skips its setPos. Flag76 is
not joined26/active58. startEntity/stopEntity instead read a **word ID**
before the same helper, so selected28 checks Lemon's own HP, not Bowie's.
If still HP0 they skip; custom/action scripts, priority, flash and shiver
have no corresponding ally-HP precheck. They can still act on its alias.
Portrait/speech properties follow the actual aliased mapsprite; C0/FF
modifiers do not prove a correct Lemon/Astral portrait resource binding.

### Action and presentation service semantics

csc1C stopEntity writes ANIMCOUNTER.b=FF; csc1B startEntity writes0.
Neither stops travel, clears actions, hides/kills/revives an entity or
recruits anyone. Inline customActscriptWait uses csc14: installs its
pointer, sets the read timer from the physical slot and waits for the
action pointer to equal eas_Idle, then scans through8080 delimiter.
Its ac_motion OFF independently writes counterFF; ac_orientLeft writes
orientation bits0–1 in flagsB, not FACING. Sprite refresh and idle jump
follow. The later Init resets orientation Up, while startEntity restores
counter0. These are animation/pose operations, not HP writes.

Reuse [accepted action semantics](./battle36-initialization-and-opening.md#action-hp-alias-and-wait-semantics):
Init sets acceleration0, speed32, acceleration/deceleration off,
obstruction on, flagsA bits5/6 clear (disabling entity/map collision
checks at their actual consumers), auto-facing on, orientation Up,
immersed off, size24, sprite refresh, then idle. Jump disables auto-facing,
relative0,-1/wait5, relative0,1/wait5, relative0,0, enables auto-facing,
then idle. 2xRightLeft sets Right/Left/Right/Left with refresh/wait30
after each, then idle. Nonwaiting installs can overlap; waiting on S/B
does not prove the other actor's Jump finished. csWait120 is a request,
not an observation of completion.

csc2D entityActions reads byte identity, stores slot timer, ANDs the
moving entity's flagsA with9F, lowers signed step counts to relative
movement, appends eas_Idle and waits when requested. No HP precheck is
added. With unchanged aliases, declared L35,59 and completed movements,
Down2 then Down3 gives35,64; it is not a warp, hide, combatant HP change
or saved return destination. Changed71/followers/service states can alter
that premise. `entity/entityscriptengine_2.asm:esc04_moveToRelativeDest`
adds signed scaled deltas to current position, calls the obstruction helper
and installs destination/travel/velocity without a0–63 clamp. csc2D's cleared
bit5 bypasses that helper's entity-obstruction scan. No automatic village
transfer is declared. Scene A/S/26 receive no new follow commands; Init/action scripts
can replace their previous following behavior until fresh population.

csc53 resolves a physical alias, writes byte_FFAFB0[slot]=0 for priority
operand0,1 for **any nonzero word**, so FFFF is true. The actual
entity/entityscriptengine_1.asm:sub_4E24 builds sprite links for nonzero
entries before zero entries. This is a sprite ordering bucket, not a
VDP tile-priority bit or a story flag; it is not reset at scene end.
csc2A shiver saves counter/global SPRITE_SIZE, sets counterFF/size21,
toggles flagsB bit3 on/off with sprite refresh and Sleep5 three times,
then restores counter/size; bit3 ends clear, not its prior value.
csc18 flash-white reads duration57, shifts right2 and DBF runs15
iterations: bit2 on/refresh/two VInts, off/refresh/two VInts,60 requested
VInts total, ending bit2 clear. No HP or position change follows from
either effect. Actual VInt scheduling, DMA, overlap and visible timing
remain unobserved.

## Text identity and complete control tokens

**Confirmed:** `data/scripting/text/gamescript.txt` supplies31 successive
requests2972–3002. Only authored short paraphrases and complete ordered
tokens are public. P/S use flags0; L usesC0 (right-side/mirrored portrait
modifiers);2975 usesFF/ENTITY_NONE and no speaker. Portrait and speech
lookups still use the actual mapsprite. `nextSingleText` closes portrait/
text and requests Sleep10; nextText leaves them open. Text skip can bypass
display and cursor increment; these IDs describe unchanged unskipped flow.

| ID / speaker / authored role | Complete ordered tokens |
| --- | --- |
|2972 P / notices tears|`["{W1}"]`|
|2973 S / questions the cause|`["{W1}"]`|
|2974 S / recognizes fighting style|`["{N}","{N}","{W1}"]`|
|2975 none / mask removal narration|`["{W1}"]`|
|2976 L / brief reaction|`["{W1}"]`|
|2977 S / identifies Lemon|`["{NAME;28}","{W1}"]`|
|2978 P / asks to inspect|`["{W1}"]`|
|2979 P / notices life|`["{W1}"]`|
|2980 S / calls Lemon twice|`["{NAME;28}","{NAME;28}","{W1}"]`|
|2981 L / startled location question|`["{W1}"]`|
|2982 L / startled response|`["{W1}"]`|
|2983 L / recalls nightmare|`["{N}","{W2}"]`|
|2984 L / recalls controlled killings|`["{N}","{W1}"]`|
|2985 S / rejects dream account|`["{W1}"]`|
|2986 L / reacts to explanation|`["{W1}"]`|
|2987 S / describes devil control|`["{N}","{W2}"]`|
|2988 S / describes forced violence|`["{N}","{W2}"]`|
|2989 S / explains restored senses|`["{N}","{W1}"]`|
|2990 L / disbelief and self-identification|`["{N}","{NAME;28}","{W1}"]`|
|2991 L / seeks atonement|`["{N}","{W1}"]`|
|2992 L / asks Astral for advice|`["{W1}"]`|
|2993 S / first reassurance|`["{W1}"]`|
|2994 S / assigns blame to devils|`["{N}","{W1}"]`|
|2995 L / hesitation|`["{W1}"]`|
|2996 L / declares fatal penance|`["{N}","{W2}"]`|
|2997 L / repeats intent|`["{W1}"]`|
|2998 S / pities departing Lemon|`["{NAME;28}","{W1}"]`|
|2999 P / reports departure|`["{NAME;28}","{N}","{W2}"]`|
|3000 P / questions nonintervention|`["{W1}"]`|
|3001 S / describes vampire immortality|`["{NAME;28}","{N}","{N}","{W1}"]`|
|3002 S / anticipates reunion and departure|`["{N}","{LEADER}","{W1}"]`|
|363 / defeat exhaustion|`["{LEADER}","{W1}"]`|
|274 / admitted Egress spell/level|`["{NAME}","{N}","{SPELL}","{#}"]`|
|275 / admitted retreat item|`["{NAME}","{N}","{ITEM}"]`|

NAME;28 names ally28 without joining it. LEADER rebuilds the active list
and need not name hardcoded Bowie. Speaker dialogue about death/immortality
does not itself implement those mechanics. Requests prove neither display,
acknowledgment, input ownership, timing nor finished text services.

## Saved seed, exploration handoff and immediate Map77 exits

**Confirmed:** neither scene load/population helper nor the complete
after scene writes PARAM2–5; there is no warp/reset-return command to
replace the seed. ExecuteMapScript's return conditionally waits for view
when dialogue is open and clears view scrolling speed. The join/437/537
tail returns the saved bytes through d0–d3, not the actors' last scene
coordinates. Shared request state remains mutable; an intervening request
can replace bytes, so this is the uninterrupted source path.

`code/gameflow/mainloop.asm:MainLoop` returns from j_BattleLoop,
runs SwitchMap, then directly calls ExplorationLoop without another
CheckBattle. The complete switched-map table has no77 entry. d4=1/-1/0
is the victory/defeat/retreat return distinction; this ExplorationLoop
does not choose its population path from d4.
`explorationfunctions_2.asm:ExplorationLoop` clears TYPE.w, adjusts/clamps
the step counter, heals living/immortal allies again and fades out. A
non-FF destination byte sets CURRENT_MAP and battleFF, obtains that
map's setup, clears/rebuilds population at returned d1–d3, loads sprites,
clears setup-temp flags and sets80. It then loads map/view, installs base
VInts, runs setup init, conditionally requests music/fade and reaches
WaitForEvent/controlled-action dispatch. An FF destination instead preserves
population and updates main/raft; unchanged victory returns77, notFF.
Declared scene Lemon is therefore removed by ordinary fresh77 population;
only retained follower conditions such as71 can create it again. No
scene recruitment flag is synthesized. Map77 has zero fixed NPC records
and an RTS init. Terrain/material/input/service completion remains Unknown.

The [accepted Map77 zone/scan owner](./roft-to-galam-and-battle36-admission.md#default-zone-request-widths-and-independent-battle37-admission)
retains distinct invoked callbacks and admission. With537 held, the
default zone returns without posting reload or clearing a preheld437.
With537 clear it sets437 and posts longword000100FF: TYPE.w=1,
PARAM1=0, PARAM2=FF/CURRENT, leaving coordinate/facing/sound bytes.
An explicit later77/CURRENT CheckBattle scan needs437; if537 is held
it clears437 while admitting37 **on that scan**. Normal victory already
cleared437; entering exploration alone is not another admission call.

All five outgoing rows in `data/maps/entries/map77/6-warp-events.asm`
(`$C5D78..$C5DA2`, including terminator) are:

| Ordinal / source X,Y | Connection / destination entry interface |
| --- | --- |
|0 /40,42|No-scroll0 →29 Dwarven Village exterior,15,20,Up1.|
|1 /255,33|No-scroll0 →53 Prism Flowers battlefield,19,1,Down3.|
|2 /255,34|No-scroll0 →53,10,30,Up1.|
|3 /255,63|Down-scroll13 hex →66 Granseal overworld,255,1,Down3.|
|4 /63,255|Right-scroll10 hex →76 Roft overworld,1,255,Right0.|

`exploration.asm:WarpIfSetAtPoint` scans in order, treating signed-negative
source coordinate bytes as wildcards, posts the first match and leaves
later rows untested. At63,63 row3 wins over row4. Destination255 retains
the corresponding current axis at its consumer; scroll uses the accepted
preserve-population connection path. These rows contain no537/536/900,
party, HP or recruitment predicate. F900's separate layout marker and
movement/collision admission do not make row0 naturally traversable by
declaration alone. No village, neighbouring exploration or Battle38 body
is entered by this research.

## One lowering and actual current consumers

**Confirmed about tooling:** unchanged
[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py) registered
the selected source and compiled abcs_battle37 in0.1068s, error=None:
17 root instructions and223 in cs_4DBEE, plus Init/Jump/2xRightLeft
actions14/9/14. The inline lying pose has five typed actions including
terminal jump/idle. No native-call occurs in either program or actions.
The76 branch, fallthrough, text modifiers,31 requests, scene map/population,
priorities, counters, flash60, shiver and movement/waits are representable.
This is lowering only; no Content import, map conversion, runner, adapter
or original service executed. Unsupported operations would remain native
prefixes; this selection happened to contain none.

**Confirmed about accepted production code, by direct read:** production
consumes prepared Content. [Capability status](../../remake/docs/capability-status.md)
owns runnable claims; successful lowering does not bind selected37.

| Actual consumer | Existing expression and selected37 gap |
| --- | --- |
|[Preparation](../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Outcome hooks, join0, battleOrigin and configured egress have shapes. Pin admits1/2/3, not37. Private outcome admission requires EnemyLeaderPresent=false and End/Return-only defeated programs;37's empty hook satisfies the latter, nonzero leader byte fails the former. No bound37 terrain/definitions/world/after materials follow.|
|[BattleOutcomeRules](../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleDeathBatch](../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs), [BattleActionCommitter](../../remake/src/Sf2.Remake.Application/Runtime/Battles/BattleActionCommitter.cs)|Defeat priority and corrected dead-ally/enemy-FIRST_ALLY accounting exist. Batch admits distinct positiveHP→0 changes and excludes nondead/off-map; it cannot reproduce arbitrary native death lists or living-enemy leader sweep. No equipped70 reward is supplied by outcome handling.|
|[BattleOutcome](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Pre-scene anchor, completion skip, join→unlock→complete and return exist. Active.First()/bound positioned actor/validated anchor differ from native unchecked list/alias/byte wrapping. Nonzero status can refuse return-status-refresh. Egress is configured route, not arbitrary live EGRESS_MAP; absent399 or held64/640 refuses. Policy validation precedes published tail, unlike native interrupted prefixes.|
|[Compiler](../../src/sf2tool/remake_exploration_programs.py), [SceneEntities](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEntityAllocator](../../remake/src/Sf2.Remake.Domain/Maps/MapEntityAllocation.cs)|Ally28 identity, follower duplicate/alias-zero allocation and two explicit records are expressible. Population reads ordinary followers, omitting65's overworld Caravan prefix; prepared start-class appearance differs from live class/current-battle/HP lookup. Capacity guards differ from native unchecked accesses. No admitted71/76/28/60 state follows.|
|[ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs), [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Typed motion, animation counter, orientation flags, priority, positions, branch and presentation have consumers; shiver saves counter/size. Position/facing/animation operations lack the selected native HP prechecks. Scene load keeps actors/program until separate population reload/sprite readiness; text/display requires bound black/palette/closed-window context. Return uses OutcomeReturn; Apply can check admission for NewBattle, unlike native immediate return.|
|[ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [BattleMapViewport](../../remake/game/src/Battles/BattleMapViewport.cs)|Gesture/flash/text/entity/sprite/fade projections exist. Generic shiver offset/30-tick completion and flash duration60 are not original flags/VInt parity. Sprite Priority is ordered separately from tile highPriority. No selected37 material binding, finished scene, exploration control, save/load or presentation observation exists.|

In particular EntityActionRunner's MoveEntityRelative computes the next
fixed-point position and throws `entity-motion-boundary` above24192
(63×384) on either axis. For declared Lemon35,59 after completed Down2,
the final Down3 targets y64=24576 and is **rejected**, even though both
programs lowered without native-call. That is a direct guard deduction,
not an observed runner failure or proof that every changed71/alias state
reaches the same operand. No engine fix is included.

Selected integration still needs admitted37 content/material/outcome,
leader sweep and exact HP/class/alias/return behavior plus completed
service/control observations. These gaps authorize neither implementation
nor an engine-ready label or automatic runtime acquisition queue.

## Reproduction, retained failures and Unknowns

The single lowering is retained in `local/issue1140/observe-01.json` and
is not repeated for a new head identity. The ignored direct recipe reuses
accepted shared instruction checks and joins selected tables, complete
scene/order/tokens, macros and actual handlers/consumers. It also checks
bounded terminal/crop/wildcard examples, exact two-path/nav-only scope,
UTF8/LF/links and private publication. Tracked source/prose own findings;
ignored recipes/receipts are coordination, not a second source of truth.
Load current selections in the same PowerShell process; use a new receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1140/review.py <configured-source-disasm-root> --receipt local/issue1140/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument contains sf2enums.asm, code/ and data/. Apply ADR0004's
semantic/provenance/polarity/width/selector/order/private checklist; no
maintained parser/schema/fixture/helper tests or H3 callbacks changed.
Normal `uv run sf2 verify` is **SKIPPED** by explicit documentation scope;
full/old/runtime gates remain excluded. Clean committed planner selection
and actual exact-head scoped CI are recorded in the frozen handoff, not
inferred from a proposed command or lowering receipt.

Preserve [#1136 initialization results](./battle37-initialization-and-opening.md#reproduction-preserved-results-and-unknowns)
and its single opening lowering, plus
[#1138 activation results](./battle37-activation-and-first-turn-dispatch.md#reproduction-retained-evidence-and-unknowns).
The [Map53/77 admission owner](./roft-to-galam-and-battle36-admission.md#actual-lowering-and-current-consumer-boundary)
retains Map53's event-enabled import UnboundLocalError/zero partial
programs. Linked predecessors retain twelve completed old private
comparison failures and corrected alias/collision/follower/statistics
evidence. No completed failure is erased, called interrupted or rerun.

Freeze a clean committed/pushed Draft PR with exact head/tree, selected
commands/results/CI, residual gaps and ignored handoff. Main-gate owns
independent acceptance/merge; stop writer/processes after handoff. No
merge, cleanup/archive, next Issue or private upload follows.

**Unknown:** natural outcomes/retreats/zone/warp reach; actual selectors,
flags, party/HP/MP/maxima/status/class/equipment, inventory/drop/Deals,
gold/death-list/F88/RNG, first active member, coordinates/facing/aliases,
transport/follower state and EGRESS_MAP; interruption/interleaving and
finished death/sprite/DMA/scene/action/view/text/portrait/audio/fade/load/
setup/input services, VInt/hardware timing, usable exploration and save
persistence. Source/ROM equality beyond accepted evidence, terrain/material
bindings and broader malformed states remain open. Individual Red Baron
Desoul/actions, full battle simulation, later Lemon recruitment, village
content and Battle38 initialization remain beyond this slice.
