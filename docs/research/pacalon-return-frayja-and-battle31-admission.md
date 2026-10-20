# Pacalon return, Frayja following and Battle31 admission

Evidence date: 2026-10-20. **Confirmed:** a separately reached south scroll
from Map74 requests71 and substitutes78 when F530 is held. Pacalon town2
and castle36 then have independent admission and live setup reads. The
restored-rain castle callback runs Frayja's offer, requests following and
sets battle unlocks431/433/434; only after script return does it set811,
then follower82. It never joins22 or sets active54. The castle south exit
can subsequently admit Battle31 on Map2. **Unknown:** natural travel,
actual caller state, completed presentation/control and saved persistence.

## Agreement and provenance

Standard rigor for [Issue #1074](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1074)
under [epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053):
map selection, actor identity, scene completion, follower/member lifetimes
and encounter admission require one connected account. Accepted base is
`2433fe5e27d4f6aa12d3bcab80947cc63205da3e`. Owned writes are this document,
one navigation link in the [Mitula return owner](./mitula-return-and-skreech-recruitment.md),
and replacement of deferred Frayja pointers in the
[castle first-visit owner](./pacalon-castle-entry-and-first-visit.md).
There are no shared writes or stacked dependencies. UI692 supplies no evidence.

Reuse the accepted [Map71/78 and town entry](./battle26-return-through-pacalon-entry.md),
[town interactions and exits](./pacalon-interactions-and-castle-handoff.md),
[castle resources and CURRENT stairs](./pacalon-castle-entry-and-first-visit.md),
[Map74 callback and battle ordering](./pacalon-departure-to-battle27-admission.md),
[request state](./map-event-request-state.md),
[request consumption](./map-event-request-consumption.md),
[interaction](./map-event-interaction-state.md),
[dialogue](./map-event-dialogue-state.md),
[common maps](./common-maps.md) and [common scripting](./common-scripting.md).
Do not re-audit their full corpora. Stop at the BattleLoop/exploration
handoff for town2; Battle31/32/33/34 bodies, Moun/Nazca/ship surveys,
runtime acquisition, new extraction/schema/fixture, engine implementation,
design-guide and translation work are excluded.

Source paths are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The selected source is clean and read-only at that pin. USA ROM identity is
SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
`sf2enums.asm`/`sf2const.asm` own constants; Fnnn denotes flag nnn.
Indices are zero-based, address ranges half-open. Short `map/`, `entity/`
and `text/` paths below mean `code/common/scripting/`; castle files are
under `data/maps/entries/map36/mapsetups/` unless qualified otherwise.

Minimum evidence is complete selected source/scene/text plus existing
fixtures/consumers and the unchanged18,285,979-byte map-init/script pair.
Reuse its accepted0.083s comparison and provenance, with no regeneration.
Linear bounded reads/joins are budgeted below one minute/512MiB and small
ignored receipts under `local/issue1074/`; network acquisition, runtime
and new maintained machinery are unnecessary. Reassess contradictions or
material supporting-tool growth before expanding.

## Return route and separate admission reads

**Confirmed:** the Mitula owner ends at Tristan's independent south request
to Map74 `(52,21)` Down3. It does not prove exploration74 was admitted:
CheckBattle orders27 before32 by unlock427/432. This section starts from
an actually admitted Map74 exploration state and a separately reached warp.
Neither Skreech recruitment nor a dialogue instruction transports the party
to Pacalon.

`data/maps/entries/map74/6-warp-events.asm`, `$C3682..$C36C4`, has eight
eight-byte rows plus endWord. Rows5/6 at `$C36AA/$C36B2` are exact duplicates:
source `(255,63)`, scroll DOWN `$13`, map71, destination `(255,1)`, recorded
Down3/padding0. First-match scanning selects5;6 is shadowed for that same
input. Earlier exits lead to Moun, Tristan and Pangoat Valley Bridge; the
later west scroll leads to Nazca region. These side routes are excluded.

The accepted ordinary walking caller needs loaded layout kind `$1000`
before WarpIfSetAtPoint; movement/obstruction/caller state remains separate.
The producer sign-extends/divides candidate coordinates by384, checks the
`$FFFF` sentinel first and treats **any negative source byte** as wildcard.
It writes event word1 before PARAM1..4 longword, then facing/padding word
and WARP_SFX. One mutable slot has no transaction or immutable route snapshot;
no match preserves previous pending state. WaitForEvent checks pending before
A/C, and ProcessMapEvent clears stored type before dispatching the read value.

`explorationfunctions_2.asm:ProcessMapEventType1_Warp`'s nonzero-PARAM1 branch:

1. Idles physical0, reads map71, checks F530 **then**, selects78 if held,
   and writes CURRENT_MAP before ProcessMapTransition. This is its explicit
   substitution, separate from MainLoop's SwitchMap table. No reverse78→71
   repair occurs if530 later changes.
2. ProcessMapTransition loads selected tiles and blocks/layout, reads/scales
   the first area rectangle and dispatches current PARAM1 &3; intact `$13`
   selects `overworldMapTransition_Down`. Plane/layout/sprite projection,
   DMA and VInt waits are requests to actual services, not observed display.
   It does not call LoadMapArea; old area/music/animation state remains live.
   Supplied non255 CURRENT_BATTLE can affect its battle-crop copy.
3. Reads X into post-transition d0.b: byte255 (signed-1) preserves actual main X
   and XDEST independently of the sampled trigger coordinate. If changed to
   nonnegative, it multiplies the whole inherited d0.w by384, retaining its
   high byte. It **clears d0.w** before reading Y1, then writes1*384 to actual
   Y/YDEST; changed negative Y preserves both. These are separate byte tests.
4. Samples actual main coordinates by signed word division and actual facing,
   calls DeclareRaftEntity, then returns to the existing exploration loop.
   PARAM5's Down is not applied. Numeric mode2 returns from raft declaration;
   mode1 skips follower relocation; other modes relocate listed followers'
   actual/destination coordinates to the sampled main tile, then use64/
   RAFT_MAP for the separate physical raft31. Scripts/lookups remain live.

This scroll does not enter MainLoop, scan a battle, heal, adjust STEP_COUNTER,
reset CURRENT_BATTLE/temp flags, reconstruct actors/walking scratch or run
setup init. It does not submit/clear WARP_SFX through the no-scroll branch.
Newly held82 does not allocate a follower during scrolling.
Interrupted services retain reached writes; changed parameters can change
later dispatch/coordinates. Control return depends on later pending/mode state.

Reuse Map71/78 metadata: distinct blocks/layouts, palette0, tilesets101–105,
one `(0,0)..(63,63)` area, both other origins0,0, parallax256/256 on both
layers, zero autoscroll, type254, MUSIC_MAIN_THEME. Both flag/step tables
are empty. Neither has a MapSetups record or a BattleMapCoordinates row;
GetCurrentMapSetup returns ms_Void, not a populated default. Shared population
construction still applies on a fresh entry, unlike this scroll. Matching
metadata does not establish identical collision terrain or a natural path.

The existing no-scroll route then supplies these ordered alternatives:

| Source table / rows | Trigger | Request / nominal seed / facing |
| --- | --- | --- |
|71 or78 /0,1|21,15 or22,15|2 /21,30 /Up1|
|2 /0|19,0|36 /20,30 /Up1|
|2 /1|23,0|36 /22,30 /Up1|
|2 /2|negative X wildcard,0|36 /preserve X,30 /Up1|

None tests530,811,82,22,54, shopping or item possession. The later town
south row requests71 `(22,16)` Down; Map71/78 north/south/west exits remain
separate routes under their accepted owner. No collision/path survey is added.
No-scroll submits/clears WARP_SFX, idles main and unwinds to MainLoop. It
clears d0–d4 words, reads the map, samples actual main coordinate words by
signed division, replaces only nonnegative destination **low bytes**, reads
facing and returns d4=0. Negative destination bytes preserve whole sampled
words; positive replacements do not erase their high byte. These consumption
samples need not equal the earlier trigger. Replacement/interruption matters.

MainLoop calls SwitchMap **then CheckBattle**. No2/36 switch exists; a later
explicit71 request separately reads530 for71→78. Map2's first scan can already
divert to Battle31 if431 was supplied held; it is not gated by this castle
visit. Clear431 permits town exploration; Map36 has no battle row at all.
Do not infer the castle is reachable merely because a warp requested it.

## Fresh town/castle lifetime and restored population

**Confirmed:** when explicit2/36 admission reaches ExplorationLoop, it clears
pending, subtracts20000 from STEP_COUNTER as a word and retains the result
only on BGE (**N==V**); otherwise it clears the word. The overflow-aware
condition is not a test of the wrapped sign bit alone. It heals living/
immortal allies, fades, writes CURRENT_MAP/CURRENT_BATTLE255, loads tilesets,
clears VInts and rebuilds entities/mappings/walking scratch. It then clears
temporary256–383, sets80, loads map/base VInts and invokes the currently
selected init. Music/fade and later control admission remain conditional.
Dead Frayja22 stays dead under the common heal; joined22 is not that predicate.

Reuse town2's complete F530 alternatives,20 puddle copies, changed well text,
shops, church, two item flags and RTS init from the accepted entry/interaction
owners. They do not write530 or create the Frayja offer. Castle36 retains
palette10/tilesets90,52,50,92,58, its two areas, five step records, nine roofs,
descriptions, one chest and two other-item rows from the first-visit owner.
No new asset/layout extraction or pickup/service semantics are implied.

`data/maps/mapsetups.asm` selects ms_map36 or ms_map36_flag530. Pair routeJoins
sourceOrder81/82 and pointerTableJoins82/83 bind `$5D756..$5D76E` /
`$5D76E..$5D786`; only entity-list and entity-event pointers change. Zone,
description, item and init pointers stay shared. GetCurrentMapSetup rechecks
CURRENT_MAP/530 on each call. List construction, interaction selection and
init do not share one frozen flag sample; reselection does not reconstruct
or relabel existing actors.

`s1_entities_530.asm`, `$5D7F0..$5D84A`, contains11 eight-byte records plus
sentinel. This is the complete restored list and interaction mapping. M means
CENTAUR_MALE233; king231/priest192. Every named event mask is UP1.

| Source row / logical / lookup | X,Y / facing / sprite | Action | Event suffix / work |
| --- | --- | --- | --- |
|0 /22 /22|27,8 /Left2 /ally22|eas_Init|212_0:811 check and scene|
|1 /128 /32|25,24 /Left2 /M|eas_Init|212_1:3308|
|2 /129 /33|12,25 /Down3 /M|walk origin12,25 range1|212_2:3309→3310|
|3 /130 /34|21,16 /Down3 /M|walk origin21,16 range1|212_3:3311→3312|
|4 /131 /35|8,3 /Down3 /M|eas_Init|212_4:3313|
|5 /132 /36|5,22 /Left2 /M|eas_Init|212_5:257 branch|
|6 /133 /37|40,17 /Down3 /M|eas_Init|212_6:3316→3317|
|7 /134 /38|29,3 /Down3 /king|eas_Init|212_7:3318→3319|
|8 /135 /39|31,5 /Left2 /M|eas_Init|shared Event6:3291→3292→3293|
|9 /136 /40|31,11 /Left2 /M|eas_Init|212_9:3320→3321→3322|
|10 /137 /41|40,4 /Down3 /priest|eas_Init|shared Event12:j_ChurchMenu|

`212_n` means Map36_212_EntityEventn; shared names mean Map36_EntityEventn.
Frayja22 is an ally declaration, not NPC128. Ordinary flag-followers are
allocated first, independently of force membership, using the full accepted
follower-row order. Castle is neither overworld nor follower-excluded, so65
adds no Caravan prefix. Enabled82 declares ally22; a nonzero lookup22 then
deduplicates the fixed ally row, consuming its action longword without a new
physical allocation. Neither case checks joined22.

For n successful follower declarations and a=1 if the fixed22 row allocates,
else0, NPC128+j maps lookup32+j to physical `n+1+a+j`, j0–9. Frayja is at
its follower mapping or physical n+1. With no followers,22 is1 and NPCs2–11;
with only82,22 is follower1 and NPCs again2–11. Logical131 is therefore
physical5 in both examples, a male centaur at declared8,3 in this setup,
**not Frayja**. With other followers/mappings the physical identity changes.
Duplicate follower aliases allocate separately and later lookup writes win;
there is no arbitrary-population capacity guarantee or safe missing-actor guard.

The two walkers copy50 bytes each into walking scratch. eas_Init initializes
speed32/32, flags/orientation/animation and Idle; it is not ally-stat setup.
Walking origin/range is a declaration subject to real motion/layout, not an
observed route. Ally class/HP owns22/Bowie sprites (dead outside-battle ally
uses blue flame); fixed centaurs/king/priest do not inherit ally HP checks.

The shared `s6_initfunction.asm:ms_map36_InitFunction` at `$5DA16..$5DA28`
checks **joined22**, not82/811. Held22 resolves current22 and MoveEntityOutOfMap
sets both actual and destination X/Y to `$7000`. It does not delete it,
clear lookup/list, stop its script, heal or join it. VInt entity updates skip
X≥`$7000` until repositioned. On retained/missing/aliased mappings it can move
physical0 or another actor;0 is not safe absence. Thus joined22 can displace
even a flag-declared Frayja follower. No scene replay or membership repair
is performed by init.

## Complete restored interactions and keyed dialogue

**Confirmed:** `s2_entityevents_530.asm`, `$5D924..$5D9C0`, has11 named
four-byte event rows and `$FD` default (12 routes). Signed table-relative
offsets include shared Event6 and Event12 and default Map36_DefaultEntityEvent/
RTS mask0. No unlisted Event8 body is needed: row135 reuses Event6.
Existing routing-setup/entity-programs fixtures retain table/body data.

Reuse GetActivatedEntity and RunMapSetupEntityEvent: physical activation
excludes viewed/follower actors, uses the first reverse-lookup alias and
current setup, and can be affected by counters/desks/approach. UP1 faces
the actor toward the player after a VInt without restoring its old facing;
it is not a required approach direction. Wrapper portrait/bleep uses the
actual mapped sprite, then clears VInt_UpdateEntities before the callback,
closes windows/reactivates updates on return. Default RTS retains that work.
F82 can thus affect both actor allocation and eligibility for ordinary contact.

All15 alternate keyed texts3308–3322 (`0CEC..0CFA`) are joined to their
source comments and actual gamescript keys. Below are paraphrases, with
their complete control/substitution distinctions; shared texts/services
remain in the first-visit owner.

| Text / callback | Meaning / terminal control |
| --- | --- |
|3308 /212_1|Rain has restored the speaker's health; W1.|
|3309 /212_2|Devils fled on seeing the soldiers; W2.|
|3310 /212_2|Mocks them as cowards; W1.|
|3311 /212_3|Vicar NAME22 wants to rescue Moun's people; W2.|
|3312 /212_3|Opening its gate could admit devils to Parmecia; W1.|
|3313 /212_4|Thirst is gone, health is still incomplete; W1.|
|3314 /212_5|Dismisses the need for practice; W2.|
|3315 /212_5|Cannot lift the spear; W1.|
|3316 /212_6|Saw a warrior who had come from the south a year earlier; W2.|
|3317 /212_6|A blocked tunnel surprised him and he went to Moun; W1.|
|3318 /212_7|King thanks the strangers for saving them; W2.|
|3319 /212_7|Did not expect human rescuers; W1.|
|3320 /212_9|King is somewhat upset; W2.|
|3321 /212_9|Centaurs had considered themselves the best warriors; W2.|
|3322 /212_9|Acknowledges that belief was wrong; W1.|

`txt` uses trap5/DisplayText and resets substitution pointer for each call;
NAME22 is explicit GetCombatantName22, not activated NPC or active-party head.
W1/W2 retain the dialogue owner's distinct waits/input/cleanup; consecutive
txt calls do not necessarily close between them. There is no yes/no branch.
Event5 checks257; clear requests3314, **then sets257**, then requests3315.
Held257 goes directly to3315. Interruption can leave3314 displayed with257
clear, or257 held before3315 completes. Fresh explicit exploration clears257;
CURRENT stairs and scroll retain it. It is independent of810/811/530.

Shared row135 still describes the Moun invasion/abandonment in3291–3293;
priest137 calls the same church service, with conditional raise/cure/promote/
save effects. Contact alone does not save/heal. Shared descriptions, chest
Mithril176 and other items Fairy Powder174/Pegasus Wing175 retain their
accepted layout/flag-before-pickup/capacity/recovery boundaries. Zone/item
defaults are RTS. No restored callback besides212_0 unlocks a battle or
joins an ally; the king's thanks is dialogue, not an extra state gate.
**Inferred:** these conversations express recovery, pride and a Moun rescue
proposal; their claims do not independently prove a gate/key/tunnel route.

If530 changes after construction, the callback table can address old actors:
default131 is a male centaur at4,22, restored131 at8,3; restored priest route137
can address default female-centaur137, while default priest139 is absent
from the restored list and may reach default. The original callback never
validates the actor's narrative identity. No live flag change repairs that.

## Frayja callback, complete scene and actual handlers

**Confirmed:** Map36_212_EntityEvent0 checks811 first; held811 returns
immediately. Clear811 calls `script cs_5DA28`, then sets811, then82 and RTS.
There is no530/22/54/82 recheck inside this body, or success
test on follow or flag writes. F530 selected the table elsewhere; it does
not prove the actual player passed Battle30. `script` uses trap6, activating
entity updates before ExecuteMapScript and resetting action scratch/skip-text
state. The selected scene at `$5DA28..$5DA7A` has19 top-level records:
18 opcode commands plus terminator,82 encoded bytes, no inline payload,
sleep, choice, branch, join, new population, movement destination or warp.

| Index | Complete operation |
| --- | --- |
|0|textCursor3323|
|1|faceEntity Peter7,131|
|2|faceEntity FOLLOWER_B31,131|
|3|nextText00,Frayja22 →3323|
|4|nextText00,Frayja22 →3324|
|5|nextSingleText00,Frayja22 →3325|
|6|nextSingleText00,FOLLOWER_B31 →3326|
|7|nextText00,Frayja22 →3327|
|8|nextSingleText00,Frayja22 →3328|
|9|nextText00,Peter7 →3329|
|10|nextSingleText00,Peter7 →3330|
|11|nextSingleText00,Frayja22 →3331|
|12|nextSingleTextFF,ENTITY_NONE255 →3332|
|13|nextSingleText00,Frayja22 →3333|
|14|followEntity22,31,2|
|15|setStoryFlag31|
|16|setStoryFlag33|
|17|setStoryFlag34|
|18|csc_end|

Normal cursor progression joins all11 gamescript keys3323–3333
(`0CFB..0D05`) and source comments, with four continued (C) and seven
single (S) requests. Paraphrases and substitution/control boundaries:

| Text / command / mode | Speaker selector | Meaning / terminal control |
| --- | --- | --- |
|3323 /3 /C|22|Credits Zalbard's defeat and Mitula's rescue; W2.|
|3324 /4 /C|22|Trusts these soldiers and asks a favor; W2.|
|3325 /5 /S|22|Requests company to Moun; W1.|
|3326 /6 /S|31|Asks the intended purpose there; W1.|
|3327 /7 /C|22|Believes Moun still has survivors; W2.|
|3328 /8 /S|22|Wants to rescue them; W1.|
|3329 /9 /C|7|Says Moun lies on the way to Nazka (source spelling); W2.|
|3330 /10 /S|7|Urges LEADER to go together; W1.|
|3331 /11 /S|22|Agrees; W1.|
|3332 /12 /S|none|Narration says LEADER takes NAME22 along; W1.|
|3333 /13 /S|22|Expresses thanks; W1.|

The actual handlers preserve these distinctions:

- `sf2cutscenemacros.asm:faceEntity` emits opcode52 and two **word** IDs.
  csc52_faceEntity resolves target131 before actor7/31, reads their live
  **destination** X/Y words and chooses direction from word distances:
  subtract words, OR-test the wrapped sign and negate negative differences,
  then compare magnitudes unsigned; ties choose vertical;
  equal chosen coordinate chooses Right/Down. It updates the acting sprite
  and waits one VInt per call. It does not HP-check, move, synchronize
  destinations with actual coordinates, or face toward logical22.131 is the
  male-centaur identity above;31 is the current FOLLOWER_B alias, only
  conditionally Astral. Missing/aliased7/31/131 can resolve physical0 or another
  actor; supplied destinations can change either facing. These are commands,
  not an observed gaze direction or intended narrative correction.
- nextText/nextSingleText emit a modifier byte and entity byte as one packed
  portrait operand. Here00/22,00/31,00/7 use actual mapped sprite portrait/
  bleep, with no right/mirror modifier. csc1D tests packed bits15/14 and
  bypasses opening when a portrait already exists. The **FFFF** at command12
  is recognized by csc00 before lookup: it sets speech SFX0 and skips portrait
  selection; it does not resolve entity255 or itself close an existing portrait.
  The single-text tail still closes portrait/text and sleeps10 after display.
  C waits for view scroll, displays and increments cursor without that tail;
  S also closes/sleeps. NAME22 and LEADER are separate text substitutions,
  not proof of roster change. Sprite/class/HP/aliases own actual portraits.
- Debug/P2 Start may skip text display and cursor increments while consuming
  operands. It does not skip the two faces, follow or story-flag commands.
  Normal text3332 is therefore neither a yes/no decision nor a success check.
  No completed text/input/audio/rendering duration follows from these requests.
- `followEntity` emits three **words**0016,001F,0002. csc2C first probes
  `(a6).b`, so its HP helper sees the **high byte00/Bowie0**, not Frayja22.
  GetCurrentHp masks the character to its low byte. Bowie HP0 advances A6
  by six operand bytes and unwinds this command; it still proceeds to the
  three unlocks. Frayja HP0 alone does not suppress following, and no leader
  HP check exists. After admission, full-word22/31 resolve current physical
  slots; table_FollowerPositions row2 gives(-24,0) **pixel offsets**.
- AddFollower installs/copies42-byte eas_Follower1 and advances walking scratch
  even when the physical follower already occurs in EXPLORATION_ENTITIES.
  It patches physical leader/offsets and appends the physical follower only
  if absent, preserving existing order. No last-list-tail selection or
  addNewFollower is used. It replaces the action pointer without moving22
  to a fixed tile. Missing/aliased slots can create self-follow or alter main/
  another actor. No bounds safety or completed following is established.
- `setStoryFlag` emits opcode13 plus a **word** battle index; csc13 reads
  that word, adds BATTLE_UNLOCKED_FLAGS_START400 with `addi.w`, then calls
  j_SetFlag→SetFlag. The actual order31,33,34 writes **431,433,434**, not
  F31/33/34, completion531/533/534 or join flags. GetFlag masks with FLAG_MASK,
  divides by8 and uses `$80 >> remainder`; SetFlag ORs the mask into its byte.
  FLAG_MASK is1023. These selected small indices do not wrap. Repeats preserve
  already-held bits.
- csc_end conditionally waits for view scroll if a dialogue window is present,
  clears scroll speed and restores interpreter registers. It does not roll
  back portraits/actors/list/flags or establish usable control. Only after
  this return does the callback set811, then82. Wrapper cleanup/reactivation
  follows; later pending zero installs mode-selected control/view0, whereas
  pending nonzero dispatches first. These are separate completion boundaries.

## Independent state lifetimes and interrupted prefixes

**Confirmed:** no join22, JoinForce, JoinBattleParty, UpdateForce,
InitializeAllyStats, item initialization or join ceremony occurs in this
scene/callback. Joined22 and active32+22=54 remain supplied state;82 means
a future flag-driven follower declaration, not force membership. Current
AddFollower is yet another immediate list/script mutation. Flags811/82
are outside temporary256–383 and survive ordinary fresh exploration clears.

| Supplied state / interruption | Resulting boundary |
| --- | --- |
|811 held,82/431/433/434 absent or inconsistent|Callback returns without replay or repair; it does not restore any missing later effect.|
|811 clear,82 already held|Fresh construction can dedup22 and exclude it as a follower from ordinary activation; a supplied callback still runs, reinstalls following if admitted, and rewrites flags.|
|22 held, either82/811|Shared init displaces mapped22; scene has no unjoin, direct reposition or heal. Active54 is independently preserved; later follower services have their own effects.|
|Bowie HP0 or changed HP; Frayja/leader dead or missing|Only this follow's high-byte Bowie precheck skips; faces/text/three unlocks have their separate consumers. No success propagation.|
|Stop during text/before14|Reached face/text/cursor effects can remain; no newly reached follow/unlocks/811/82.|
|Stop after14/before15|Follow/list/scratch may change while all later flags remain as supplied.|
|Stop after15 or16|Only reached unlock prefix431 or431/433 is newly held; no atomic group or rollback.|
|Stop after17/before script return|All three unlocks can be held with811/82 still clear; repeats are possible if callback can be reached again.|
|Stop after811/before82|811 held suppresses later replay even with82 clear. No repair path in this callback.|
|Follow skipped but callback returns|431/433/434,811,82 can all be held without an immediate successful following operation or membership22.|
|530 changes between population/event/init reads|Source list, current callback, sprite identity and shared displacement can differ; no implicit reconstruction.|

The four internal castle stair requests remain CURRENT255 under the accepted
owner: `(2,4)↔(52,3)` and `(55,11)↔(5,12)` with their recorded facings.
They still clear pending/adjust counter/heal/fade, update main/raft, reload
area/view and rerun current init, but retain actors/lookups/scripts/scratch,
CURRENT_BATTLE and temporary257. LoadMap's CURRENT branch retains blocks/layout
instead of reloading them; low-byte-sign tests govern coordinate updates.
Mode-dependent follower relocation precedes the shared22 displacement.
Fresh explicit36 rebuilds the population and clears257; F82 can now recreate
22 through the flag table even if the immediate follow skipped. Such a new
declaration does not prove that the previous scene completed motion or joined22.

## Castle south return and Battle31 stopping boundary

**Confirmed:** Map36 warp row0 at `$AE050` is `(255,31)`, no-scroll,
Map2 `(255,1)`, Down3. It does not test811/82/22/54/530/unlocks or scene
completion. Its accepted consumer preserves sampled X, replaces only Y's
low byte with1 and returns the explicit2 request to MainLoop. Pending
replacement, high bytes, actual mode and ordinary layout admission remain
live premises; no scene command posts this warp.

`code/common/maps/getbattle.asm:CheckBattle` scans45 seven-byte rows in
order. The only Map2 record is index31, crop `(0,0,48,32)`, trigger `(255,255)`;
wildcards do not require entry coordinate containment within that crop.
SwitchMap leaves2 unchanged. The scan tests unlock400+31=431, then trigger
bytes, writes crop, tests completion531, and if held clears431 **while still
returning31 for this invocation**.

| Live town-admission flags | MainLoop disposition |
| --- | --- |
|431 clear, either531|d7=-1; explicit2 goes to fresh exploration lifetime.|
|431 held,531 clear|d7=31; crop0,0,48,32; call BattleLoop.|
|431 held,531 held|Clear431 and still return31/call BattleLoop; a later scan without rearming can explore.|

F530/811/82/22/54 and unlock433/434 are not additional Map2 admission tests.
Preheld431 can divert the inbound town route before any castle visit. A stale
431 plus531 still admits once; a replay with811 clear can rearm431 even with
531 held. Held811 by itself cannot rearm it. Unmatched scans do not write
a new battle crop. A supplied CURRENT255 request resolves actual CURRENT_MAP
only for the scan, restoring the original operand on no match; intact castle
south uses explicit2. Stop **before** BattleLoop initialization/body or the
exploration body of this final handoff; this is not a victory/egress contract.

Return to Map74 is a **separate later route** via town south and Map71/78
north scroll, not a scene warp. Under the accepted
[Map74 owner](./pacalon-departure-to-battle27-admission.md#callback-order-and-partial-reload-overwrite),
zone `(255,61)` first checks527: clear sets427/posts reload/sets428–430;
held527 requires811 held and532 clear to set432/post reload. A subsequent
MainLoop scan orders27 before32; preheld427 can still win. Neither811 nor
this castle offer immediately enters Battle32. No Battle31/32/33/34 body,
Moun gate operation or Nazca ship acquisition is asserted here.

## Actual preparation, Content, Application and Godot consumption

**Confirmed about repository consumers:** existing source records and closed
fixtures express evidence, while prepared Content and reached execution are
separate boundaries. The actual OriginalPrograms compiler was run read-only
on selected scene/callback into ignored `local/issue1074/lowering.json`.
No registered world/start/actor/resource package or runtime is created.
The [capability owner](../../remake/docs/capability-status.md) controls current
supported runtime scope; these findings do not make this route engine-ready.

| Boundary / actual owner | Expressible shapes and current refusal/gap |
| --- | --- |
|[Preparer](../../src/sf2tool/remake_exploration_content.py), [maps](../../src/sf2tool/remake_exploration_maps.py)|Registered canonical identity/pin and explicit selection drive preparation. Entities, walking, setups, callbacks, zones, text and simple no-scroll warps have representations. Scrolling, retained-coordinate or unselected-destination warps emit warp-frontier/native refusal; the south scroll and preserved-X return are not directly runnable simple transfers. No new export is required for this research.|
|[OriginalPrograms](../../src/sf2tool/remake_exploration_programs.py)|cs_5DA28 produces62 lowered instructions: cursor; two **native-call faceEntity** at indices1/2; text/window/wait operations; follow22→31(-24,0); set-flag431/433/434; end-map-script. The first refusal is before any scene text/follow/unlock. There is no join-party. Callback branch811, activating call, write811, write82 and jump to the shared return preserve their representable order; that return is a separate one-instruction program.|
|[Content reader](../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [ProgramRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs)|native-call becomes UnsupportedInstruction; reached execution reports program-opcode/Unsupported. A successful lowering is not a completed scene. WriteFlag/BranchFlag/CallProgram/text/follow can be consumed only with a bound admitted world/program. Missing content and explicit semantic refusal are different gaps.|
|Follow and identity / [FollowerMotion](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/FollowerMotion.cs)|ProgramRunner resolves slots and installs follower motion. Compiler omits the original high-byte Bowie-HP precheck; modern install has no original list append/dedup or42-byte scratch accounting. Original missing-slot/alias and HP behavior require separate binding/review. Member22/active54 resources are absent and not acquired by this no-join scene.|
|Setup and request / [MapTransfer](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [MapSetupSelection](../../remake/src/Sf2.Remake.Domain/Maps/MapSetupSelection.cs), [MapEventDispatcher](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs)|Transfer selects prepared setup/population; later movement/interaction uses the loaded world.Definition.Events. Original live530 callback reselection versus retained population is not established by that shape. CURRENT lifetime, mutable pending replacement, signed/retained coordinate words, shared22 displacement and stale/rearmed encounter admission need selected bindings and assessment.|
|Battle/private binding / [PrivateExplorationReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateExplorationReader.cs), [PrivateBattleEncounterReader](../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs)|Production consumes provenance-bound prepared documents, not research fixtures/disassembly on demand. Private encounter pins remain1–3, not31/32/33/34; no selected2/36/71/74/78 world, Frayja deployment, town/castle resources or battle-start binding is delivered.|
|Presentation / [ExplorationPresentation](../../remake/game/src/Exploration/ExplorationPresentation.cs), [ExplorationViewRunner](../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|Logical text/window/sprite/view consumers exist, with field-view-profile refusal for unsupported profiles. Full selected text/portrait/bleep/map/material/audio binding, reached Godot rendering, word/alias fidelity, waits/control and original timing remain unassessed. Shared pickup/services retain their accepted owners and gaps.|

## Reproduction, self-review and remaining Unknowns

Load the ignored input/tool selection in the same process as each verification
launch. Reuse the pair without modifying size/mtime or its existing receipt:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1074/review.py <configured-source-disasm-root>
git diff --check
```

The direct recipe joins the complete19-record/82-byte scene, all26 selected
text/comment/control references,11 entity declarations/12 interaction routes,
shared callbacks, init, route rows and selected map metadata; verifies actual
handlers/macros/constants/branch/width/write order and actual lowering/current
consumer guards; reviews changed states, UTF-8, links/anchors, private boundary
and exact three owned paths. These are direct source/prose checks, not tests
of an audit program or runtime proof. No parser/schema/extractor is changed;
their new-code mutation obligations do not add a new test suite. ADR0004's
adversarial branch/alias/width/provenance review still applies.

Self-review separates131/22, destination/actual coordinates, byte HP probe/
word follower,431/31/531, follower82/member22/active54,257/811 lifetimes,
scroll/fresh/CURRENT, scene-return/callback-return/control and lowering/
execution. Preserve actual results and exact-head CI in the Issue/PR handoff.
On a clean committed head run
`uv run sf2 verify plan --base origin/main --head HEAD`; documentation scope
does not launch blanket normal/full Python/.NET/Godot/H3/H4 suites.

Preserve #1072 author0.203s/main-gate0.202s, CI39477634522 and its two accepted
independent reviews. #1070's original audit/CI39464142557 missed STEP_COUNTER
prose; its accepted N==V correction/CI39466738861 remains explicit, as do
#1068's high-byte HP and #1066's projection corrections. Earlier diagnostics,
Unknowns and twelve completed old private-comparison failures retain their
owners; none is rerun/relabelled here. The unchanged pair is not fresh proof
of ROM/H1 equality or naturally completed travel.

**Unknown:** actual natural collision/route/contact reach; supplied flags,
HP/class/aliases/population/capacity/mode/pending requests; completed motion,
dialogue and services; visible/audible result and precise timing; usable control,
save/load persistence and later encounter execution. These are boundaries,
not an automatic capture queue. Research provenance stays distinct from
future design decisions and remake acceptance.
