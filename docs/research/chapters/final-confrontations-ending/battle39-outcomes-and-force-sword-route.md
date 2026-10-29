# Battle39 outcomes, Force Sword and Devils Head entry

Next: [Devils Head labyrinth and Battle40 admission](./devils-head-labyrinth-and-battle40-admission.md).

Evidence date: 2026-10-21. **Confirmed (static):** Battle39 can end through
the gated Hydra128 leader sweep or the ordinary zero-enemy count; defeat
has priority. Evil Ring and Holy Thunder are separate conditional
action-side drops. Victory returns to Map66 with no authored aftermath,
clears439 and sets539. Two independent Map41 entrances connect the
shrine and Devils Head exterior. Map42 separates the999/901 attraction
scene from902 sword extraction. Map41 uses the selected Force Sword at
6,8/Up to open the head, then conditionally recruits Lemon28 and sets71.
The final declared warp targets Map28 at16,30/Up. Natural traversal,
completed services and playable remake support remain **Unknown**.

## Agreement, evidence and method

Standard rigor for [Issue #1154](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1154)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053),
continuing accepted base `074ec970344e43347de9920c49abf0044859603a`.
This supplies future content/design decisions about state, items, map
connections and actual consumers. Own this document and navigation only
in [Battle39 first control](./battle39-opening-and-first-control.md).
No shared index/schema/fixture/tool/engine changes or stacked dependency.
Stop at Map28's entry interface; its interior, Battle40, optional towns,
dwarven village and forging remain outside. Map54's lack of an independent
setup file does not prove a missing map; its global route is deferred.

Reuse [Battle38 outcomes](../moun-nazca-grans/battle38-outcomes-and-geshp-aftermath.md),
[Map66](../moun-nazca-grans/granseal-overworld-return-and-battle38-admission.md),
[Battle37 Lemon aftermath](../moun-nazca-grans/battle37-outcomes-and-lemon-aftermath.md),
[item transactions](../../world/map-event-item-transactions.md),
[request production](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[maps](../../world/common-maps.md) and [scripting](../../world/common-scripting.md).
The previously deferred Lemon recruitment is covered only at this entrance.
Research completion does not establish engine readiness.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` abbreviates `code/gameflow/battle/`, `map/` abbreviates
`code/common/scripting/map/`; Fnnn means story flag nnn. sf2const.asm,
sf2enums.asm, sf2macros.asm, sf2mapsetupmacros.asm and
sf2cutscenemacros.asm own constants and encodings. Ordinals are zero-based,
address intervals half-open. USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`;
no new source/ROM equality or original-runtime observation is claimed.

Minimum evidence is these two small setups/scenes, selected battle/global
tables, shared callers and current production consumers. Reuse the
18,285,979-byte init/script pair and `local/issue1005/pair.txt` provenance
read-only. One selected existing-compiler observation is sufficient; no
full import or new comparator. Work is linear in selected source/consumers
and one pair load, with small fixed flag/item cases. Estimate receipts
under2MiB, seconds per direct command, peak-memory target512MiB
**unmeasured**. No runtime/network acquisition cost or model processing
of the full pair. Reassess any new cross-system mechanism, large output
or dynamic acquisition before expanding. Ignored recipes/results live
under `local/issue1154/`; all older receipts remain intact.

## Terminal detection and selected39 hooks

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` calls the defeated
hook after ExecuteIndividualTurn/debug handling, then death cleanup and
CountRemainingCombatants. It tests ally count0 before enemy count0. Only
a nonterminal result runs that queued actor's after-turn effects, cleanup
and the same two checks again. There is no second defeated hook; only a
second nonterminal result advances the byte cursor by2. Admitted retreat
unwinds before this suffix.

| Complete source table / selected39 use | Exact selected row |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words at$47BE8|slot39/byte78 at$47C36 → ms_Empty, the wordFFFF.|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes at$47C8E|byte39 at$47CB5 →−1, enables leader cleanup.|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words at$47CF4|slot39/byte78 at$47D42 → ms_Empty.|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes at$47D6A|byte39 at$47D91 →0, still passed to JoinForce.|
|`data/battles/global/afterbattlepositions.asm`|Only battle5 has a row; no39 relocation.|

`battle/cutscenes/battleendcutscenesstart.asm` requires Bowie0 HP.w!=0
and enemy128 HP.w=0. From `data/battles/spritesets/spriteset39.asm`,
128 is combat **Hydra33**, first of14 STARTING enemies, at8,24;
LEADER14 AI is a separate declaration. Clear539 interprets ms_Empty;
held539 skips the interpreter. Both reach `battleendcutscenesend.asm`,
reread CURRENT_BATTLE and test that selector's leader byte. Unchanged39's
−1 scans all32 enemy IDs128–159, appends every nonzero-HP ID to the
existing death list, writes FF sentinel and increments its word length.
It ignores position, declared roster, activation and neutral state.

ProcessKilledCombatants performs requested spins/sound/waits; each listed
ally receives its own defeat, whereas an enemy credits BATTLESCENE_FIRST_ALLY
and then restores the enemy ID. It writes X/Y=−1, status0, refreshes
stats and moves the aliased physical entity to$7000,$7000. It neither
sets HP0, clears list length nor calls DropEnemyItem. Thus living swept
enemies can disappear from counting without producing a drop.

CountRemainingCombatants scans all30 allies and32 enemies. Signed X.b>=0
and HP.w!=0 are required; Y, membership, activation, neutral status and
declared roster size are ignored. Bowie HP0 finally forces ally count0.
ApplyPositionsAfterEnemyLeaderDies has the same Bowie/128 HP gates but
its separate battle5-only table gives39 no position override.

| Supplied seam state | Result, assuming the named calls return |
| --- | --- |
|All14 fresh enemies positioned/living|All14 count; zero regions do not make them absent.|
|Bowie alive;128 HP0; other13 alive|The13 can be swept, then fail X admission; no guaranteed ring/thunder award.|
|Same with a living undeclared142–159 slot|It is also appended, even if already off-map. Existing/stale list entries are not deduplicated.|
|128 HP!=0 but X<0; all other enemies fail X/HP|Ordinary victory can occur without the HP0 hook.|
|128 positioned/living; others gone|Enemy count remains nonzero; there is no “all others defeated” victory shortcut.|
|Bowie HP0, another living ally and zero enemies / simultaneous terminal states|Defeat takes precedence; defeated hook skips. All allies off-map also loses with Bowie alive.|
|128 dies only in after-turn effects|No second sweep that turn; remaining positioned/living enemies keep the battle running.|
|539 held|Empty hook skips, but the gated leader sweep and both terminal priorities remain.|

These are complete branch distinctions at the named seams, not a
whole-battle simulation or proof that every supplied state occurs naturally.

## Two conditional drops and actual inventory

**Confirmed:** `data/battles/global/enemyitemdrops.asm` has30 four-byte
records plus word terminator, `$BE52..$BECC`. `enemyEntity` adds128:

| Record / address | Match and independent dropped bit |
| --- | --- |
|15 /$BE8E|TO_ANCIENT_SHRINE39, enemyEntity2=**130 Evil Bishop**,9,23; EVIL_RING25; droppedFlag15 →$FFF7A5 mask80 hex.|
|16 /$BE92|39, enemyEntity8=**136 Minotaur**,8,19; HOLY_THUNDER18; droppedFlag16 →$FFF7A6 mask01 hex.|

These bytes belong to ENEMY_ITEM_DROPPED_FLAGS at$FFF7A4, independent
of story F15/F16,439/489/539 and the placement UNUSED_ITEM_DROP bit.
The [accepted fresh holdings](./battle39-opening-and-first-control.md#placement-bits-real-holdings-equipment-and-current-stats)
are130 `[00DF,0019,007F,007F]` and136 `[00A9,0012,007F,007F]`.
InitializeEnemyItems/AddItem mask807F strips equipment/AI/drop bits from
the placement expressions6099/6012. The ring is unequipped, adds no
ATT15 or curse on that path; copied rod/axe equipment is separate.
Matching a drop row still requires the item to remain in the target's
actual inventory. Combat Minotaur139 is not136; a held copy elsewhere
does not satisfy this row.

`battle/battleactions/battleactionsengine_1.asm` has three direct BSR
sites to `dropenemyitem.asm:battlesceneScript_DropEnemyItem`. At each:

1. Actor byte must have enemy bit clear, target byte enemy bit set, and
   targetDies.b!=0. Live battle byte and full target byte must match.
2. GetItemSlotContainingIndex masks the index and scans all four actual
   target slots, first match or−1. Equipped/broken copies match; a missing
   item returns without changing dropped flags. No placement-bit test.
3. Neither selected item is Taros Sword, Iron Ball or Counter Sword;
   neither makes the special range32 RNG draw. The later repeat-battle
   chance code is unreachable, not a repeat entitlement.
4. BSET tests/sets15 or16. Held bit returns before removal/award. First
   admission removes that target slot, arranges items and refreshes stats.
5. The current allied actor's HP.w is checked. A living actor receives
   an AddItem attempt in its own four slots; no force-wide/Caravan search
   or active/class/equip eligibility test. The bare index is passed,
   so the awarded word is0019 or0012, without target equipment/broken bits.
6. Success requests messages396 then395. Actor HP0 or full inventory
   instead tests RARE and calls AddItemToDeals without those messages.

Both definitions in `data/stats/items/itemdefs.asm` are RARE. Evil Ring
is WIZ/SORC/VICR, range0–0, price5000, RING|RARE|CURSED|BREAKABLE,
BOLT2, ATT+15 then two NONE0 effects. Holy Thunder has no equip class,
range0–1, price1500, RARE|CONSUMABLE, THUNDR1 and three NONE0 effects.
Deals uses one nibble per item; AddItemToDeals skips addition at15.
No victory/empty scene/recruitment call supplies missing drops.

| Changed state / prefix | Consequence |
| --- | --- |
|Correct ally kill, target item, clear dropped bit, actor alive/room|One bare selected item can be awarded. Ring/thunder cases use independent flags.|
|Actor full or HP0; Deals below15 / at15|Rare addition / discarded addition, after dropped bit and target removal.|
|Wrong target, missing item, enemy actor, targetDies0|No selected drop. HP0 alone is insufficient; later victory does not retry it.|
|Dropped bit already held, even on fresh reconstruction|No removal/award. Changing489/539 does not clear that bit here.|
|Hydra sweep removes living130/136|No action-side drop; holding both placements is not receiving them.|
|Interrupted after bit/removal/award but before text or539|Reached item/bit state can precede completion; rollback/save persistence is unproved.|

## Victory, defeat, Egress and Map66 return

**Confirmed:** `battle/battleloop_2.asm:BattleLoop_Victory` heals living
allies and immortal Peter7/Lemon28 even at HP0, to maxima; preserves
STUN/POISON/CURSE and refreshes stats. Other dead allies skip. Then it
saves CURRENT_MAP.b to PARAM2, rebuilds force/active lists and reads the
first active-list byte. Its actual X/Y receive byte additions of crop
17,20 into PARAM3/4; its physical alias supplies facing to PARAM5.
PARAM1=0 is written without posting event TYPE. This unchecked native
selection need not be Bowie, the last actor, alive or positioned; an empty
list/stale alias is not given a managed fallback.

For unchanged39, saved return is Map66, `(memberX+17)&255`,
`(memberY+20)&255`, aliased facing. Supplied8,9→25,29;
250,250→11,14. Camera3,22, Hydra8,24 and shrine entry are not this seed.
The empty after wrapper tests539, runs ms_Empty only when clear, then
always passes join byte0 to JoinForce. This rejoins Bowie: joined0,
rebuild lists, active32 only if fewer than12 active. It recruits neither
Lemon28 nor any follower. A preexisting active32 survives full capacity;
a newly written32 is absent from the just-built snapshot.

After wrapper return victory rereads the selector, clears400+index and
sets500+index: unchanged39 clears439/sets539, leaves489, drop bits,
901/902/930/71 unchanged. Words d0–d3 are cleared and PARAM2–5 copied
back; d4=1. Upper longword halves are not established by clr.w. Selector
and pending request bytes can change between independent reads; the
heal/seed/join/clear/set sequence is not transactional.

Defeat first UpdateBattleUnlockedFlag (clear439 only when539 held),
clears dialogue-name1, requests sad music/text363/close, restores Bowie
HP=max and halves gold with logical longword shift. GetEgressPositionForBattle
then **unconditionally clears439 for selected39**, even when539 clear,
and selects live EGRESS_MAP; no fixed-town case39. Returns d4=−1, without
after/join/new539. The losable battle4 exception does not apply.

Admitted Egress or Angel Wing pays MP/item first, requests its text,
sound/flash and updates unlock, unwinds the action frame, calls the same
egress helper and returns clr.w d4. It bypasses defeat gold/Bowie recovery
and victory completion. Denied retreat is a different path. F489 and
539 are not newly set; both defeat and retreat clear439 when the selected
helper is reached, regardless of539.

`code/common/maps/egressinit.asm:GetSavepointForMap`: held399 scans23
savepoint rows;66 has no row, so retains66 with1,1/Up. Supplied Granseal3
selects32,13/Up; clear399 selects initial3/56,3/Down. Held64 additionally
resets raft state through the shared table. Ordinary BattleLoop sets399;
actual EGRESS_MAP/church/raft/caller state remains an input.

MainLoop sends the returned registers into ExplorationLoop. Its fresh
return sets battleFF, rebuilds full Map66 layout/area and ordinary
population, unlike the retained battle crop. The
[accepted Map66 owner](../moun-nazca-grans/granseal-overworld-return-and-battle38-admission.md#complete-map66-local-surfaces-and-immediate-exits)
retains zero fixed NPCs, F508/F611 copies, Mithril213 at30,44, empty chest/
step/roof tables and all ten exits. At invoked y28, Map66_ZoneEvent2
requires538 held and539 clear to set439/post reload. Held539 returns
without clearing a supplied439; it is not the owner of victory's clear.
Other zones/default random producer and ordered CheckBattle remain
independent. Return/coordinate overlap does not itself invoke a zone,
warp or battle; actual movement marker selects the relevant path.

## Map connections and complete local data

**Confirmed:** `data/maps/mapsetups.asm` has one default each for41/42,
with no conditional override. CheckBattle/global setup selection is not
an implicit prerequisite embedded in these warp records. All rows below
are no-scroll0; Right0/Up1/Left2/Down3 use sf2enums directions.

| Source / ordinal / candidate | Exact destination |
| --- | --- |
|Map66 /7 /35,28|Map41 DEVILS_HEAD6,14/Up.|
|Map66 /8 /33,43|Map41 DEVILS_HEAD23,14/Up, the shrine-exterior area.|
|Map41 /0 and1 /23,15 and24,15|Map66 OVERWORLD_GRANSEAL_KINGDOM33,44/Down.|
|Map41 /2 /6,15|Map66 36,28/Down.|
|Map41 /3 /23,4|Map42 FORCE_SWORD_SHRINE7,14/Up.|
|Map41 /4 /6,7|Map28 DEVILS_HEAD_LABYRINTH16,30/Up: stopping interface.|
|Map42 /0 /7,15|Map41 23,5/Down.|
|Map42 /1 /26,18|MAP_CURRENT255 → current42,39,8/Up.|
|Map42 /2 /39,9|Current42,26,19/Down.|
|Map42 /3 /19,16|Current42,2,8/Up.|
|Map42 /4 /2,9|Current42,19,17/Up.|

Warp tables contain no539/901/902/930/item/Lemon predicate. Layout
markers, collision, area membership, actual approach, map request
consumption and completed transfer are separate premises. “Reverse” exits
are intentionally asymmetric coordinates; no straight walking route is
inferred. MAP_CURRENT resolution is not a new map ID255.

| File / source interval | Complete non-setup data |
| --- | --- |
|Map41 `2-areas` $B14BE..$B14FC|Two areas0,0..12,15 and17,0..29,15. Both secondary FG0,16/BG0,0; both planes parallax256,256/autoscroll0,0/type0. Music WITCH then SHRINE.|
|41 `3-flag-events` $B14FC..$B14FE|Only endWord; no flag-copy rows.930 replay is setup init below.|
|41 `4-step-events` $B14FE..$B1508|One candidate23,4: copy30,0 size1×1→23,4; endWord.|
|41 `5-roof-events` $B1508..$B150A|Only endWord.|
|41 `6-warp-events` $B150A..$B1534|Five eight-byte rows above plus word terminator.|
|41 `7-chest-items` $B1534..$B1536|No chest rows.|
|41 `8-other-items` $B1536..$B153C|6,5 /flag199 /EVIL_LANCE; endWord.|
|Map42 `2-areas` $B1A90..$B1B0A|Four areas0,0..14,15;15,0..32,12;16,14..32,26;34,0..44,9. First FG0,16/BG0,0/type0, others FG/BG0,0/type255. All planes parallax256,256/autoscroll0,0. Music INTRO for first three, SHRINE for fourth.|
|42 `3-flag-events` $B1B0A..$B1B24|In order:901 copy0,32 size7×6→0,0;901 copy33,1 size1×1→26,18;902 copy33,2 size1×1→39,2; endWord.|
|42 `4-step`/`5-roof` $B1B24..$B1B28|Each only endWord.|
|42 `6-warp-events` $B1B28..$B1B52|Five rows above plus word terminator.|
|42 `7-chest-items` $B1B52..$B1B5C|19,24 /133 /POWER_WATER;4,3 /198 /EVIL_AXE; endWord.|
|42 `8-other-items` $B1B5C..$B1B5E|Only endWord; Force Sword is not a mapItem/chest record.|

`code/common/maps/mapload.asm:LoadMapBlocksAndLayout` tests held flags
and copies in order, then applies held chest flags. Setting a story flag
alone does not recopy the live map. The Map41 step-copy/warp at23,4
is distinct from F930's head at6,7. No decoded layout/marker/collision
join or walking-route calculation is claimed here.

Generic CheckArea tests the block in front of VIEW_TARGET_ENTITY and
its marker before setup descriptions:1800 chest;2C00 vase;3000 barrel;
3400 shelves;1C00 ground item. OpenChest/CheckNonChestItem test/set the
record flag; held flag yields NOTHING. A found non-gold item first tries
Bowie0, then the joined force list after its first byte; no HP/active
predicate. Success uses413 then415 or416, music/input waits. All-full
uses417, CloseChest and RefillNonChestItem to clear the candidate flags
and restore a chest, permitting a retry. These are not the mandatory
sword or action-drop transaction. Item declarations alone do not prove
a suitable marker or successful award.

## Setups, population and investigation dispatch

**Confirmed:** the six-pointer tables are Map41 `$5F3E8..$5F400`
and Map42 `$5FDE4..$5FDFC`. Their complete surfaces:

| Surface | Map41 | Map42 |
| --- | --- | --- |
|s1 entities|$5F400..$5F402: msEntitiesEnd, zero fixed NPCs.|$5FDFC..$5FDFE: same.|
|s2 entity events|$5F402..$5F408: default RTS.|$5FDFE..$5FE04: default RTS.|
|s3 zones|$5F408..$5F40E: default RTS.|$5FE04..$5FE24:19,21→Map42_ZoneEvent0, then default RTS.|
|s4 descriptions|$5F40E..$5F46C: target6,7, two function rows.|$5FE24..$5FE90: target39,2, two function rows.|
|s5 items|$5F46C..$5F4A0:6,8/Up/Force Sword→Map41_ItemEvent0, then default RTS.|$5FE90..$5FE98: default RTS.|
|s6 init|$5F4A0..$5F4B8: held930 runs cs_5F4AE, else RTS.|$5FE98..$5FE9A: RTS.|

`InitializeFollowerEntities` uses ordinary `data/scripting/entity/followers.asm`
on41/42; they are not overworlds, so no65 Caravan prefix. It selects by
follower flags, independent of joined/active membership and HP. Relevant
rows in full-table order are74→FOLLOWER_B31/Astral,68→31/Oddler,
70→31/Astral,71→Lemon28; other held rows can add other companions.
Later present31 rows overwrite its alias while prior physical entities
can remain. Thus “Astral” is the authored dialogue role, not a guaranteed
31 identity. F71 is not joined28 or active60. Missing aliases initialize0;
GetEntityAddressFromCharacter masks toFF, adjusts enemy IDs by96 and
reads the physical alias without presence/capacity validation.

`map/mapsetupsfunctions_1.asm:DisplayAreaDescription` scans six-byte
rows in order using the **front target coordinates** packed X/Y. Both
maps put msDescFunctionD6 with byte2=FF before msDescFunction with
byte2=0. The first matches only when d6.w=0; the second has no d6
restriction. `main/mainactions.asm:FieldMenu` Search clears d6, while
`explorationvints.asm` sets d6=1 for shortcut CheckArea after no entity
event. This is a call-type distinction; no row tests facing or actor ID.
CheckArea computes the target from the current view entity/facing and
tries generic markers first. A target description is not a movement zone.
Matched functions return handled d7=−1 through the shared portrait/text
close. Original service reach/alias validity remains unobserved.

| Map / source function / supplied flags | Complete result |
| --- | --- |
|41 DescFunc0,930 clear; no Force Sword in joined force|txt3511, generic head warning.|
|41 DescFunc0,930 clear; inventory lookup finds67|txt3512, identifies mouth slit. GetItemInventoryLocation rebuilds joined force, scans members in order and held slots descending, masks item index; no HP/active/equipped predicate. Unjoined inventory is excluded.|
|41 DescFunc1,930 clear|txt3511, no inventory lookup, even if sword held.|
|41 either,930 held|txt423 then412; no sword-possession test or reopening.|
|42 DescFunc0,902 clear|Runs full extraction cs_5FF06 then sets902. No999/901/inventory/Lemon/539 check.|
|42 DescFunc1,902 clear|DisplayText3334 ($D06), sword remains embedded; no grant/902 write.|
|42 either,902 held|Clear dialogue-name1, DisplayText424 then434; no repeated extraction, even if no sword remains in inventory.|

## Map42 attraction, extraction and mandatory inventory

**Confirmed:** Map42_ZoneEvent0 requires999 held and901 clear. It runs
cs_5FE9A then sets901. Other combinations return unchanged;902 is not
tested. F999's earlier producer belongs to the accepted
[Nazca shootdown](../moun-nazca-grans/nazca-ship-shootdown-and-crash-handoff.md).
The coordinate19,21 needs actual zone invocation/marker1400; it does not
automatically move or award anything. Here B=Bowie0, S=FOLLOWER_B31;
`single`/`next` name nextSingleText/nextText, all text flagsFF/narrator
unless stated. All listed relative movement streams end endActions and
wait. Named Init means eas_Init, Anim2 means eas_AnimSpeedx2.

| Complete ordered scene / source interval | Operations |
| --- | --- |
|cs_5FE9A at$5FE9A, before cs_5FF06 at$5FF06|textCursor3337; next3337 narrator; single3338 narrator; wait-script B Init; move B Down1,Down1,Right1,Right1,Right1,Right1; move B Right1,Right1,Right1,Up1,Up1; move B Up1,Up1; wait60; single3339 narrator; hideText; wait40; flashScreenWhite20; sound BATTLEFIELD_DEATH; quake2; copy33,1 size1×1→26,18; wait20; quake0; wait60; single3340 S/flags0; csc_end.|
|cs_5FF06 at$5FF06, before csub at$5FF56|textCursor3335; hideText; entityFlashWhite B,60; copy33,2 size1×1→39,2; newEntity128 at39,2/Down/OBJECT3=180; async script128 Anim2; executeSubroutine csub_5FF56; hide128; quake2; copy0,32 size7×6→0,0; wait60; sound BATTLEFIELD_DEATH; wait20; quake0; single3335 narrator; single3336 S/flags0; csc_end.|
|csub_5FF56 `$5FF56..$5FF62`|d0.w=Force Sword67; d1=1; tail JMP ReceiveMandatoryItem. No result test by cs_5FF06 or DescFunc0 before902.|

The three motion groups sum+7,−2: supplied starting Bowie19,21 would
end26,19 if the services complete. The scene does not set that starting
position or prove the zone caller arrived there. Sword extraction uses
an effect entity128, not Hydra combat128. InitializeNewEntity selects
max signed physical alias+1 over63 entries, overwrites reference128,
uses eas_Init and has no ordinary managed capacity guard; hide128 is
a separate physical/alias operation. Creating an effect does not grant67.

ReceiveMandatoryItem in **included** `code/common/stats/iteminventory.asm`
at$4F48A (not the similarly named older split file) preserves d1–d5/a0,
rebuilds all joined force members, scans them ascending for held count<4
without HP/active filters, adds bare67 and requests music/text213 plus
fade/input/close before returning0. It does not equip it. Force Sword's
definition is HERO,1–1,10000, WEAPON|RARE|UNSELLABLE, useSpell NOTHING,
ATT+46/NONE0/NONE0. No jewel inventory check is performed by this helper.

All-full with d1=1 requests214 and enters DiscardItem. Cancel, declined
confirmation, UNSELLABLE selection (text37) and equipped cursed item
(text30) loop; confirmation uses44. After actual removal, a RARE item
is added to Deals **and loops back to selection**; this source does not
return immediately after that rare discard. Only the nonrare removal
branch returns member/slot/item to ReceiveMandatoryItem. That helper
adds67 to the returned member, requests215/music/fade/input/close and
returns2, without testing AddItem's result. d1=0 would return1 on full,
but is not the selected call. Empty joined lists, exhausted discardable
choices, interrupted UI or changed inventory remain unchecked caller/
service boundaries;902 is not independently proof of receipt.

| Supplied901/902 | Fresh/revisit layout and callbacks |
| --- | --- |
|0/0|No flag copies.999 can admit attraction; active Search can independently extract at39,2.|
|1/0|Both901 copies run on load, including7×6→0,0; attraction skips; extraction still allowed. The first attraction scene itself only copies the1×1 portal.|
|0/1|Only sword1×1 replacement runs on load. Extraction skips;999 can still admit attraction because it does not test902. No inferred flag implication.|
|1/1|All three rows run in source order; both one-shot scenes skip. Losing/using the item does not reset either flag here.|

Extraction's live7×6 copy precedes902, whereas its replay row is gated
by901. Preserve that asymmetry; do not rewrite it into a single monotonic
“sword obtained” layout state. Flags are written after their scripts;
partial copies/received inventory may precede901/902 with no rollback.

## Map41 opening and Lemon recruitment

**Confirmed:** FieldMenu's Item→Use selects a member/held slot.
Force Sword is absent from `data/stats/items/usableoutsidebattleitems.asm`,
so IsItemUsableOnField returns−1 and routes to setup item events.
GetPlayerEntityPosition supplies first-entity X/Y/facing, separately from
the selected item's holder. RunMapSetupItemEvent masks d4 to007F and
matches byte X,Y,facing,item; the sole row requires6,8/Up/67. There is
no Bowie-holder, HERO-class, equipped,999/901/902/539/71 predicate.
The ordinary caller supplies possession through selection; the callback
does not rescan inventory. Wrong coordinate/facing/item takes RTS default
and the caller's73/422 “use/no effect” messages. No item is removed by
this event; it is not the later consumable-item path.

| Supplied930 / joined28 | Complete Map41_ItemEvent0 sequence |
| --- | --- |
|0 /0|Run cs_5F4B8; set930; run cs_5F594; set71; d6.w=−1; return.|
|0 /1|Run opening; set930; skip recruitment and71 write; d6=−1; return.|
|1 /either|Immediate return; shared dispatcher initialized d6=0, so caller can request73/422. Does not repair missing28/71.|

Neither opening nor recruitment requires71 clear; setting930 before the
join creates a distinct interrupted state. F930-held init runs only
cs_5F4AE: copy13,0 size3×5→5,4 then csc_end. It restores the open head
on reentry; it does not replay the sound/quake scene or recruit Lemon.
F930 clear runs no init scene, even if a previous interrupted live copy
had opened the map. The Map28 warp itself contains no930 check.

Full opening `scripts.asm:cs_5F4B8` at$5F4B8, with B=Bowie0,
P=Peter7, S=FOLLOWER_B31, RL=eas_2xRightLeft, Jump=eas_Jump:

| Ordered phase | Complete operations |
| --- | --- |
|Preparation|wait-script B Init,P Init,S Init; wait40; rumble; quake3; wait40; rumble; wait5.|
|Look around|async B RL; wait5; async P RL; wait5; async S RL; rumble; wait40; rumble; wait40; rumble; wait40; rumble.|
|First head copy|copy13,6 size3×5→5,4; BLAST_SPELL; flashScreenWhite4; rumble; face B/P/S Up; wait60.|
|Open and settle|copy13,0 size3×5→5,4; BLAST_SPELL; flashScreenWhite4; rumble; quake$4003; wait40.|
|Reaction and follow|async B Jump,P Jump, wait-script S Jump; repeat that three-call group once; wait50; follow P→B offset2; follow S→P offset2; csc_end.|

Full Lemon scene `cs_5F594` at$5F594 through$5F630:

| Ordered phase | Complete operations |
| --- | --- |
|Arrival|newEntity Lemon28 at6,15/Up/spriteFF; textCursor3513; single3513 Lemon/0; setDest S6,10; face B/P/S Down; setCamDest1,6; single3514 Lemon/0; wait-move Lemon Up4; single3515 Lemon/0.|
|Appeal|wait30; face S Up; wait30; single3516 S/80; face S Down; next3517 Lemon/0; next3518 Lemon/0; single3519 Lemon/0; wait40; face S Up; single3520 S/80.|
|Agreement|nod P; nod B; face S Down; wait20; nod S; single3521 S/0; join Lemon28; wait-script Lemon Jump twice; single3522 Lemon/0; face S Up; follow Lemon→S offset2; csc_end.|

newEntity28 takes live ally appearance through GetAllyMapsprite, despite
the declared FF sprite; it is not a fixed Lemon-special sprite or a new
combatant-stat reset. A preexisting71 follower can leave an earlier
physical Lemon while newEntity allocates/rebinds28; no deduplication
predicate is present. setDest's word31 has bit15 clear, so its native
handler waits for travel completion; it is not an asynchronous teleport.
join28 uses normal join music, JoinForce and text446 with current class,
fade/input/close/sleep10. It sets joined28, rebuilds lists, conditionally
sets active60 below12, preserving a preheld60. A newly written60 is
absent from that list snapshot. It does not clear71; the caller sets71
after the whole scene returns. No separate choice prompt or refusal
branch exists in this authored recruitment sequence.

## Text, actions and service boundaries

**Confirmed:** `data/scripting/text/gamescript.txt` owns the following
IDs and complete token sequences. Prose is an author summary, not copied
dialogue. Text order above includes all source requests; shared inventory/
drop/join requests can occur inside their calls.

| ID / speaker role | Author summary | Ordered tokens |
| --- | --- | --- |
|3334 /description|Sword embedded in rock.|`{N}{W1}`|
|3335 /narratorFF|A distant opening is heard.|`{N}{W1}`|
|3336 /S0|Sword acquired; seek tower door.|`{N}{N}{W1}`|
|3337 /narratorFF, next|Jewel starts glowing.|`{N}{W2}`|
|3338 /narratorFF|Light draws the leader.|`{N}{LEADER}{D3}`|
|3339 /narratorFF|Glow intensifies.|`{N}{W1}`|
|3340 /S0|Recognizes the new opening.|`{N}{W1}`|
|3511 /description|Warning about devil statue.|`{N}{W1}`|
|3512 /description|Identifies mouth slit.|`{N}{N}{W1}`|
|3513 /Lemon0|Calls to Astral and leader.|`{LEADER}{W1}`|
|3514 /Lemon0|Asks leader to wait.|`{LEADER}{W1}`|
|3515 /Lemon0|Requests membership.|`{W1}`|
|3516 /S80|Hesitation.|`{W1}`|
|3517 /Lemon0, next|Offers fighting ability.|`{N}{W2}`|
|3518 /Lemon0, next|States wish for death.|`{W2}`|
|3519 /Lemon0|Offers to fight for their cause.|`{N}{W1}`|
|3520 /S80|Recommends him as Galam's hero.|`{N}{W1}`|
|3521 /S0|Invites Lemon by name.|`{NAME;28}{N}{N}{W1}`|
|3522 /Lemon0|Thanks them and declares resolve.|`{N}{W1}`|
|423 /head investigated;412 /empty|Head's post-open fallback.|`{NAME}{N}{W2}{CLEAR}`;`{W1}`|
|424 /investigated;434 /ordinary|Shrine's902-held fallback.|`{NAME}{W2}{CLEAR}`;`{W1}`|
|213 /receive;214 /full;215 /discard+receive|Mandatory inventory service.|`{NAME}{N}{ITEM}`;`{ITEM}{N}{N}{W1}`;`{NAME}{N}{ITEM}{N}{ITEM}`|
|446 /join|Current member and class join message.|`{NAME}{CLASS}{N}`|
|396 /drop;395 /receive|Action-side messages.|`{NAME}{N}{ITEM}{D1}`;`{NAME}{N}{ITEM}`|

FF/ENTITY_NONE requests skip portrait lookup, not an unconditional close
of existing text/portrait windows; hideText and single-text returns close
through their own helpers. S80 changes portrait placement, not identity.
S's actual sprite/portrait follows its current alias. Lemon dialogue
likewise uses live ally appearance. Ordinary alias/HP/service boundaries
from the preceding opening owner remain: setActscriptWait waits for Idle;
RL installs right/left alternation with30 waits. eas_Jump disables
auto-facing with ac_autoFacing OFF (opcode15 hex), requests relative
(0,−1), waits5, requests(0,+1), waits5, requests(0,0), restores
auto-facing with ac_autoFacing ON and jumps to eas_Idle. ac_motion is
the separate opcode14 hex; Anim2 sets animation-speed state.
setFacing/csc23 checks ordinary-ally0–29 HP and can skip at HP0;
pseudo31 bypasses that test. csc2D/entityActions, csc14 inline scripts,
csc15/setActscript and csc29/setDest have no HP gate. Supplied Lemon28
HP0 therefore does not suppress installation or execution of moveUp4
or the later two Jump scripts; valid aliases and progressing entity
services remain premises. `csc2C_followEntity` tests the high byte
of the word operand (0 here) before reading the whole identity: its
alive precheck is Bowie0, even for Peter/Lemon follow. Do not infer the
three follow installations from dialogue completion alone.

csc34_setBlocks calls CopyMapBlocks and requests both view-plane updates;
it does not write a story flag. csc33's$4003 masks amplitude3 and ramps
2→1→0 with Sleep40 each, rather than storing16387 as amplitude.
csc41 shifts duration right3 and DBF includes its initial iteration:
flash4 requests one quick-white fade, flash20 requests three, then palette
duplication. csc18 entityFlashWhite60 runs16 groups of four VInt waits
(64 requested VInts), toggling flagsB bit2 and refreshing sprites. These
are source instruction counts, not measured elapsed/rendered timings.
View/camera, motion, fade, text/input, sound, layout/DMA and UI completion
are required services; no script or flag alone establishes them.

## One lowering and actual consumers

**Confirmed about tooling:** unchanged
[OriginalPrograms](../../../../src/sf2tool/remake_exploration_programs.py) registered
the two selected setups and compiled seven requested callback roots once.
The0.1279s observation completed with all root errors=None; retained26
programs and actions Init14/RL14/Jump9/Anim2=3. The four full scenes have
45/88/38/28 lowered instructions respectively; cs_5F4AE has2.
Complete results/partial native operations are in
`local/issue1154/observe-01.json` (72,608 bytes). No retry or runner pass.

| Program / zero-based lowered indices | All23 program native occurrences |
| --- | --- |
|cs_5F4B8|21/29 setBlocks;23/31 flashScreenWhite;33 setQuake.|
|cs_5F594|0 newEntity ally28;8 setDest.|
|cs_5F4AE|0 setBlocks.|
|Map41_DescFunc0|1 jsr inventory lookup;2 cmpi.w;3 bne.s.|
|cs_5FE9A|24 flashScreenWhite;27 setBlocks.|
|loc_5FE7A|0 clr.w;1/2 jsr DisplayText.|
|cs_5FF06|4/10 setBlocks.|
|csub_5FF56|0 jmp ReceiveMandatoryItem.|
|Map42_DescFunc1|1 jsr DisplayText.|
|loc_5FE52|0 clr.w;1/2 jsr DisplayText.|

The sole named action native is eas_AnimSpeedx2[0] ac_animSpeedX2;
its inline occurrence is in cs_5FF06[6]. Native inventory comparison/
branch does not recursively compile byte_5F446 (3512): error=None does
not prove complete typed control flow. The raw source branch remains
documented and directly checked. The mandatory tail retains only native
jmp, not a grant or success status. Later typed join-party28/follow/text
operations do not make earlier newEntity/setDest executable. No substitute
blocks/flash or edited compiler was used.

**Confirmed about current production, by direct reads:** prepared Content
is the runtime input. The [capability owner](../../../../remake/docs/capability-status.md)
explicitly marks item acquisition, area descriptions and setup-event
selection Unsupported. These are current boundaries, not requests to
repair them in this slice.

| Actual consumer | Expressible / refused / unbound boundary |
| --- | --- |
|[prepare_encounter/compile_routes](../../../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs)|Outcome hook/join0/origin/egress shapes exist; encounter pins1/2/3 exclude39. Private outcome requires EnemyLeaderPresent=false and End/Return-only defeated program: empty39 hook fits latter, −1 leader fails former. No admitted39 route/world results.|
|[BattleOutcomeRules](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [BattleDeathBatch](../../../../remake/src/Sf2.Remake.Domain/Battles/Rules/BattleDeathBatch.cs), [BattleRewards](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleRewards.cs)|Defeat priority, ordinary death accounting and EXP/gold exist; positiveHP→0 distinct death batches do not reproduce arbitrary/living native sweep. No selected130/136 drop, bits15/16 or Deals producer. Item definition/loadout representation does not grant an action drop.|
|[PrivateBattleScenarioReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|Previous39 refusal remains: placement-item-refresh, source-commandset, ENHANCED/source-ai-word and source-mover for BRASS_GUNNER and MAGE. REGULAR/HEALER/CENTAUR/HOVERING/STEALTH/FLYING have Mover cases. PRIMARY active is bit0/mask0001; no regression to the earlier corrected wording.|
|[BattleOutcome](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs), [Sf2OutcomeReturnPolicy](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [MapPartyMembership](../../../../remake/src/Sf2.Remake.Domain/Maps/MapPartyMembership.cs)|Pre-scene first-active anchor, completed skip, join→unlock→complete and12-capacity snapshot exist. Managed positioned member/coordinate validation differs from unchecked list/alias/byte wrapping; nonzero status refuses return-status-refresh. Egress is configured and refuses absent399/held64/640; generic completed-only unlock finish does not supply39's unconditional egress clear. Native interrupted prefixes differ from validation-before-publication.|
|[prepare](../../../../src/sf2tool/remake_exploration_content.py), [compile_maps](../../../../src/sf2tool/remake_exploration_maps.py)|Selects entity/zone/init/scripts; no ordinary s4 description or s5 item-event registration/dispatch. Area, ordered flag/step copies and simple selected warps have shapes. Chest/ground-item tables are not turned into inventory transactions here. Scene lowering above used explicit registration, not a production import. Unselected destinations/scroll retain frontier; same-map255 can use preserve mode.|
|[SceneEntities](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/SceneEntities.cs), [MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs)|Ordinary follower flags/aliases, marker1400 source-zone and1000 warp, step-copy and selected map setup/transfer services exist. Guarded49-slot allocation/prepared ally sprites differ from native live appearance/unchecked aliases. No Search/type-d6 or selected-item Force Sword use entry, mandatory discard UI,902 receipt or930→Lemon route is bound. Map66 reload/scroll gaps persist.|
|[ProgramRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [EntityActionRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)|Typed flags/calls/text/ordinary quake/motion/effect entity128/join28/follows have consumers. Native-call becomes UnsupportedInstruction and throws program-opcode; action native throws entity-action. If prior bindings/services succeed, first scene boundaries are opening[21], Lemon[0], attraction[24], extraction[4], replay[0]. Relative motion rejects outside0..63 with entity-motion-boundary; it does not clamp. Source HP skips and alias timing require their own bindings.|
|[ExplorationPresentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs)|Generic entity-white, gesture, text/audio/camera/quake projection exists; a separate fixed white-four-pulses cue is not these one/three-pulse source opcodes. No selected materials, layout copies, native services, original timings or usable input observed.|

**Inferred:** these source joins give a coherent future content contract
from Battle39 outcomes to the Map28 entrance, with independently stored
items, story flags and membership. The existing typed pieces could be
reused in a separately scoped implementation. They do not establish a
currently runnable41/42/39 route or natural state implications.

## Reproduction, preservation and stopping boundary

The ignored direct recipe reads the immutable pair and sole lowering
receipt, checks selected table identities/widths/rows, complete callbacks/
scene operations/token sequences, shared branch polarity/order, caller
and alias targets, consumers, changed-state distinctions, exact two-path
scope/navigation/UTF8/LF/links and private publication boundary. It is a
transparent local check, not maintained parser/schema/fixture machinery
or a test suite for a verification program. Use a new receipt name:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1154/review.py <configured-source-disasm-root> --receipt local/issue1154/direct-reproduction-01.json --committed
git diff --check
uv run sf2 verify plan --base origin/main --head HEAD
```

The source argument contains sf2enums.asm, code/ and data/. Applicable
ADR0004 provenance/date/selector/width/order/branch/adversarial/consumer
checks apply; maintained parser/schema/fixture/helper tests and H3 callbacks
are N/A. Normal verify is **SKIPPED** under explicit Issue documentation
scope, not PASS. No full/old-comparison/SDK/Godot/emulator/H3/H4/screenshot
run. Exact committed planner, actual scoped CI, resources, scratch failures
and frozen head/tree belong in the handoff; independent main-gate acceptance
and merge remain pending.

Preserve #1152 original passing direct/CI and missed P2s, its focused
PRIMARY bit0/mask0001 and BRASS_GUNNER/MAGE corrections; prior no-clamp
correction/eleven aftermath natives; Map53 UnboundLocalError/zero partial
import; twelve completed old comparisons; F88 caller/queue limits,
Battle37 y64 and Map66 reload/scroll gaps. None is relabeled interrupted,
automatically repaired or rerun. New native refusals above remain findings.

**Unknown:** natural39 outcome/drop/return, shrine/door/Lemon/Map28 reach;
live32 enemy/all30 ally slots, selectors/HP/maxima/status/items/gold/
drop/Deals/death lists/FIRST_ALLY, joined/active/follower/transport/story
flags, F88/queue/RNG, aliases/positions/facing/savepoint; source/ROM
equality beyond accepted evidence; decoded layout/markers/collision and
materials; interrupted/interleaved callbacks and request headroom; completed
movement/camera/map/entity/view/palette/DMA/VInt/text/window/portrait/
audio/input/discard services, hardware timing, usable control and save
persistence. These are explicit limits, not an acquisition queue. No
Map28 internal exploration, Battle40 initialization/action or later story
claim follows from its destination row. Freeze the pushed Draft candidate
and stop writing; no merge, cleanup or next Issue.
