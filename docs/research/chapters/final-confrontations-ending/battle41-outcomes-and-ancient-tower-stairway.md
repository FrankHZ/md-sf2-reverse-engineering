# Battle41 outcomes and Ancient Tower stairway

Continue with [Battle42 initialization and opening](./battle42-initialization-and-opening.md).

Evidence date: 2026-10-22. **Confirmed (static):** Bowie defeat wins
terminal priority. Reaper128's death can sweep other living enemies;
that sweep does not award Blue Shaman129's Holy Staff. Victory has an
empty after-hook, rejoins member0, clears441 and sets541, then rebuilds
ordinary Map56 from a preseeded party-member return. Map60 is a seven-area,
23-warp stairway with two F999 layout copies and no authored setup or
battle row. Its exits declare57/58; held999 switches the58 request to59
before encounter selection. Natural route
geometry, original completion and playable remake support are **Unknown**.

## Agreement, provenance and method

Standard rigor for [Issue #1168](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1168)
under [Epic #1053](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1053).
Accepted base `8783570ffee3502939a6ed73b0fd2e08e0720692`, tree
`e9d7586493883d2c8fd0218fb94f188e95f59267`. Continue
[Battle41 opening/first control](./battle41-opening-activation-and-first-control.md)
through outcomes, selected drop, Map56 reconstruction and the complete
Map60 local exploration tables. Stop at Map57/58 receiving setup and
CheckBattle interfaces, including58's flag-selected59 replacement.
Their complete content, Battle42/43 content, original
runtime acquisition, engine changes and world import are outside scope.

Own this document and predecessor successor-navigation only. No shared
index/schema/fixture/extractor/engine or aggregate change, competing
research writer or stacked dependency. Reuse the accepted
[Battle40 outcomes](./battle40-outcomes-and-odd-eye-aftermath.md),
[Map56/Creed admission](./ancient-tower-ascent-creed-and-battle41-admission.md),
[item transactions](../../world/map-event-item-transactions.md),
[request production](../../world/map-event-request-state.md),
[request consumption](../../world/map-event-request-consumption.md),
[maps](../../world/common-maps.md) and [scripting](../../world/common-scripting.md).
Their general rules remain owners; selected41/60 relationships are checked here.

Original paths below are relative to read-only
[SF2DISASM c834c652b6862bc5679fd7f69a38a7093206efc6/disasm](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
`battle/` means `code/gameflow/battle/`; `map/` and `entity/` mean
children of `code/common/scripting/`. sf2const.asm, sf2enums.asm,
sf2macros.asm, sf2mapmacros.asm and sf2cutscenemacros.asm own constants
and encodings. Fnnn is a story flag; dropped bits are a separate bank.
Ordinals are zero-based, intervals half-open, addresses hexadecimal.
USA ROM SHA-256 remains
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
No fresh source/ROM equality or original-runtime evidence is claimed.

Minimum evidence is selected global rows/callers, all eight local Map60
authored ASM data files and actual final producers/consumers. Reuse the
immutable18,285,979-byte static pair and `local/issue1005/pair.txt`
provenance read-only; do not regenerate it. No authored41 after-scene
needs lowering. Direct selected source/consumer checks determine the
final output path, so no synthetic producer run, prior opening/Map56
observation, layout/terrain export or full import is repeated. Work is
linear in selected text/table size with fixed small branch checks;
seconds per direct command, selected receipts below2MiB, total including
CI/coordination below4MiB, peak-memory target512MiB **unmeasured**.
Ignored recipes/receipts belong to `local/issue1168/`; keep earlier
results intact. Reassess before runtime work, cross-system expansion
or material budget growth. No new model batch or asset acquisition.

## Terminal detection and living-enemy sweep

**Confirmed:** `battle/battleloop_1.asm:BattleLoop` calls the defeated
hook after individual-turn/debug processing, then ProcessKilledCombatants,
then CountRemainingCombatants. Ally count0 branches to defeat before
enemy count0 branches to victory. Only a nonterminal result reaches
that actor's after-turn effects, cleanup and the same priority checks.
There is no second defeated hook there. Only another nonterminal result
advances the byte cursor by2. Admitted retreat unwinds before this suffix.

| Complete global owner / selected41 slot | Exact selection |
| --- | --- |
|`data/battles/cutscenes/battleendcutscenes.asm`,48 relative words, base$47BE8|word offset82 at$47C3A → ms_Empty (FFFF)|
|`data/battles/global/enemyleaderpresence.asm`,45 bytes, base$47C8E|byte41 at$47CB7 →−1|
|`data/battles/cutscenes/afterbattlecutscenes.asm`,48 relative words, base$47CF4|word offset82 at$47D46 → ms_Empty (FFFF)|
|`data/battles/cutscenes/afterbattlejoins.asm`,52 bytes, base$47D6A|byte41 at$47D93 →0|
|`data/battles/global/afterbattlepositions.asm`|Only battle5 has a row; no41 position override|

`battle/cutscenes/battleendcutscenesstart.asm:ExecuteBattleCutscene_Defeated`
requires Bowie0 HP.w!=0 and combatant128 HP.w=0. spriteset41's first
enemy is Reaper35,16,8, not the opening's scene128 alias. LEADER14 AI
is separate from this table predicate. Clear541 dispatches ms_Empty;
held541 skips the interpreter. Both reach battleendcutscenesend.asm,
reread CURRENT_BATTLE and test the leader byte. Selected−1 is nonzero:
scan all32 IDs128–159, append every HP.w!=0 ID to the existing death
list, write FF sentinel after each append and increment its word length.
No X/Y, declared roster, neutral, activation or duplicate check applies.
The list begins$FFB58A and length.w is at$FFB402; this append has no
capacity guard. For a supplied initial length0 and nineteen living
subordinates it writes nineteen IDs through$FFB59C and sentinel$FFB59D.
BATTLESCENE_DATA_END is$FFB59A, sixteen bytes after list start; that named
marker is not enforced as a runtime bound. Retained entries append at
their supplied word offset instead of starting anew. These are address/
write extents, not observed memory corruption or proof every such state
is naturally reached.

ProcessKilledCombatants (`$24518..$24642`) runs spin/effect/sound/waits.
Allied IDs receive defeat credit; enemy IDs credit BATTLESCENE_FIRST_ALLY
then restore the enemy identity. It writes X/Y=−1, status0, refreshes
stats and parks the physical entity at$7000,$7000. It neither writes
HP0, calls DropEnemyItem nor clears list length. The sweep removes
counting eligibility while a swept enemy can still have nonzero HP.

CountRemainingCombatants (`$23C58..$23CBA`) scans30 allies0–29 and32
enemies128–159. Signed X.b>=0 and HP.w!=0 are its only per-slot gates;
Y, party membership, neutral, PRIMARY, roster length and signed HP are
not. Bowie HP0 finally forces ally count0. ApplyPositionsAfterEnemyLeaderDies
uses Bowie/128 HP gates but finds no41 row.

| Supplied valid/changed seam state | Consequence if services return |
| --- | --- |
|Twenty fresh enemies living/positioned|All20 count, including inactive145–147.|
|Bowie alive;128 HP0; other19 HP!=0|All19 can be swept, including129; Holy Staff is not awarded by that sweep.|
|An additional living148–159 or already off-map enemy|Sweep includes it; retained list entries are not deduplicated.|
|128 HP!=0 but X<0; no other countable enemy|Ordinary victory can occur without the defeated hook.|
|128 living/positioned; subordinates absent|Enemy count remains nonzero.|
|Bowie HP0 and no enemies, or both counts0|Defeat takes priority and defeated hook skips.|
|All allies fail X/HP with Bowie HP!=0|Ally count0 still defeats. Y<0 alone does not exclude an otherwise countable slot.|
|HP word8000, X byte7F versus80|Nonzero HP counts at7F; signed-negative80 excludes. Neutral and activation bits do not change these gates.|
|128 dies only during after-turn effects|No second sweep that turn; surviving countable enemies can keep the battle nonterminal.|
|541 held|Empty defeated interpreter is skipped, but leader sweep and terminal priority still apply.|

These distinctions cover the common rule beyond the fresh roster;
they are not a whole-battle simulation or natural reach proof.

## Holy Staff transaction and limits

**Confirmed:** `data/battles/global/enemyitemdrops.asm` at
`$BE52..$BECC` contains30 four-byte records and word terminator. The
sole41 match is record18 at$BE9A: battle OUTSIDE_ANCIENT_TOWER41,
enemyEntity1→combatant129, HOLY_STAFF97, droppedFlag18. Authored order
is preserved: preceding record17 is Battle40's Fairy Tear, following19
is Battle43's Healing Rain. No second41 row supplies a leader drop.

spriteset41 identifies129 as Blue Shaman85 at15,5. Its placement item
is NOTHING; enemydefs.asm supplies `[00E1,007F,007F,007F]`, Holy Staff
equipped plus three empty slots. Definition equipment and an actual
held inventory are different claims. Item97's definition is VICR,
range1–1, price9000, WEAPON|RARE, use NOTHING; effects ATT+29,
HP_RECOVERY3, NONE0. Class/equip legality does not gate the award.

`battle/battleactions/battleactionsengine_1.asm` has three direct BSR
sites to `dropenemyitem.asm:battlesceneScript_DropEnemyItem`
(`$BD24..$BE52`): per-target effect, admitted second attack and admitted
counterattack. Each resets targetDies before its effect and calls the
drop helper afterward; actor/target pointers can change during counter
processing. The reached helper, rather than an assumed attack label,
checks actual allied actor/enemy target/targetDies bytes.

The complete selected transaction is:

1. Actor enemy bit must be clear, target enemy bit set, targetDies.b
   nonzero. Live CURRENT_BATTLE.b and complete target byte must match
   the row. HP0 alone, an unrelated holder or end-of-battle cleanup
   does not admit it.
2. GetItemSlotContainingIndex masks the item index and scans four live
   target slots, returning the first match or−1. Equipped/broken copies
   match. Missing staff returns before dropped-bank mutation.
3. Only Taros Sword, Iron Ball and Counter Sword branch to special RNG.
   Holy Staff bypasses all three comparisons and goes directly to
   @DropItem: **no random draw in this selected drop path**. The later
   repeat-battle chance code is bypassed by an unconditional branch;
   it does not add a41 probability. Other action RNG is outside scope.
4. Read droppedFlag18, DIVU8, add quotient2 to ENEMY_ITEM_DROPPED_FLAGS
   base$FFF7A4, SWAP remainder2, BSET bit2 at$FFF7A6 (mask04).
   BSET sets before testing its previous state. Previously held returns
   without removal/award; this bank is independent of story F18/441/491/541.
5. First admission removes the actual target slot through RemoveItemBySlot,
   arranges remaining items and refreshes target stats, including losing
   equipped-staff effects. Then read the allied actor's live HP.w.
   Nonzero HP attempts AddItem into that actor's four slots; bare97 is
   passed, yielding0061, without equipped/broken bits. No Caravan or
   other-party search occurs. Success requests dropped396 then received395.
6. Actor HP0 or full inventory instead tests item RARE **after** bit
   and removal. Holy Staff is rare: AddItemToDeals increments its nibble
   unless already15. Item97 is odd, so DEALS_ITEMS$FFF604 +floor(97/2)
   =$FFF634, low nibble, increment1. At15 the fallback is lost. It does
   not request those two pickup messages. Bank/item changes are not rolled
   back because the actor cannot receive it.

| Changed inputs at the reached helper | Selected consequence |
| --- | --- |
|Correct ally kill, actual staff, clear bit18, actor alive/room|One bare staff; set18 and remove original equipped item before award/messages.|
|Same but actor HP0 or four occupied slots|Set18/remove; Deals+1 below15, otherwise lost.|
|Bit18 held, even after target inventory reconstruction|No removal or award; no selected staff RNG.|
|Wrong battle/target, enemy actor, targetDies0, missing actual item|No selected transaction.|
|Reaper death sweeps living129|No DropEnemyItem call from sweep; victory does not retry a staff award.|
|129 killed earlier and staff received/Deals/lost|Later victory, defeat or retreat does not undo that reached prefix here.|
|Interruption after BSET or removal|Bit can exist without removal/receipt, or removal without award/messages; persistence/rollback is unproved.|

## Empty aftermath, return seed and state ownership

**Confirmed:** BattleLoop_Victory first calls HealLivingAndImmortalAllies.
Living allies and immortal Peter7/Lemon28 heal to maxima; other dead allies
skip. STUN/POISON/CURSE survive the native mask and stats refresh. Then:

- Copy CURRENT_MAP.b to PARAM2 at$FFA84D.
- Update force/active lists; take the first active-list byte, not the
  last actor or necessarily Bowie. Add BATTLE_AREA_X/Y to that member's
  X/Y with **byte wrapping**, storing PARAM3/4 at$FFA84E/$FFA84F.
- Resolve that combatant's physical entity alias and copy its facing
  byte to PARAM5 at$FFA850. Write PARAM1=0 at$FFA84C; no event type is
  posted by this seed.
- Run ExecuteAfterBattleCutscene; reread battle, clear400+41=441, then
  set500+41=541. Clear d0–d3 **words**, copy PARAM2–5 into low bytes,
  set d4=1 and return. Upper longword halves are not established by clr.w.

Unchanged41 uses Map56 and crop0,0/32×36. Therefore its seed is
`56, firstActiveX, firstActiveY, aliasedFacing`; no41 position override
replaces it. The active member need not be alive or positioned. Empty
active list/stale alias has no managed native fallback. Changed crop
or selectors use live reads; no clamp, snapshot or transaction is added.

ExecuteAfterBattleCutscene checks541: clear dispatches ms_Empty, held
skips it. ExecuteMapScript on FFFF still resets dword_FFB1A4 to
FF9004_LOADING_SPACE, clears SKIP_CUTSCENE_TEXT, and at termination
conditionally waits for view scrolling if a dialogue window is open,
then clears view speed. Debug P2 Start can reassert text-skip after its
initial clear; these are ordered writes, not a guaranteed final clear.
There is no authored41 scene, loadMap, layout
copy, actor population, dialogue, F999 write or narrative transition.
Skipping the wrapper body bypasses those interpreter effects.

Both after paths reach EndAfterBattleCutscene, reread CURRENT_BATTLE
and call JoinForce with selected byte0. This sets joined0, rebuilds
lists, and joins active32 only when active count<12; an already-held32
survives full capacity. A new32 is not in the just-built list snapshot.
It rejoins Bowie, not Creed/Reaper or a new character. The return seed
precedes this call and is not recomputed from the new membership.

Defeat first runs UpdateBattleUnlockedFlag: clear441 only if541 is
already held. It clears dialogue-name1, requests sad music/text363/close,
restores Bowie HP=max, logically halves the gold longword, selects
egress and returns d4=−1. Battle4's losable exception does not apply41.
No after-hook/join/new541 occurs.

Admitted Angel Wing removes the selected slot first. Admitted Egress
gets spell cost and decreases MP first. Both then request text/flash/
sound, update unlock, unwind the action frame, call the egress helper
and return d4.w=0, bypassing the ordinary turn suffix, defeat recovery/
gold loss and victory completion. Denial and pre-admission legality
remain separate; reached payment can precede transfer completion.

GetEgressPositionForBattle (`$23E50..$23EB0`) clears unlock specially
for38/39 and uses fixed towns only for16/17/13/31/32. **41 matches none**:
live EGRESS_MAP.b at$FFF719 feeds GetSavepointForMap. It is not hardcoded56.
That helper (`$75EC..$764E`) checks399: clear returns initial Map3,56,3/Down;
held searches four-byte savepoint rows, default supplied map,1,1/Up,
then applies F64's raft-reset interface. BattleLoop set399 on entry,
but a changed value at this later read selects its own branch.

The [Map56 Church/save owner](./ancient-tower-ascent-creed-and-battle41-admission.md#church-and-the-actual-savepoint-connection)
already proves the selected candidate: accepted Church Save copies
CURRENT_MAP to EGRESS_MAP and sets399 before SaveGame; mere arrival,
talking or declining Save does not. With live EGRESS_MAP56 and held399,
row18 selects42,38/Down. Other saved/church state can select elsewhere;
Creed's42,39 and warp arrival57,24 are not this savepoint. This slice
does not replay the accepted Save seam or establish save durability.

| State | Owner / selected effects |
| --- | --- |
|441 unlock|Creed's904-clear zone sets it; victory clears it; defeat/admitted retreat clear only if old541 held. It is a CheckBattle gate, not intro/completion.|
|491 intro|Start wrapper sets before empty start; before/start wrappers test it independently. Outcomes here neither clear nor newly set it.|
|541 completed|Guards defeated/after interpreters; victory sets after join and clear441. Held541 alone does not suppress leader sweep.|
|903 Creed conversation|Talk/zone set after conversation returns. No outcome mutation.|
|904 Creed zone done|Zone tests first and sets after441. Held904 with441 clear is not repaired. No outcome mutation.|
|18 dropped bank|Independent equipment transaction; outcome/Creed flags do not substitute for it.|
|999 layout state|Read by Map60 flag-copy rows; no selected41 hook/outcome writes it. Its earlier story producer is outside scope.|

| Old541 / outcome | Selected441,491,541,903,904 after ordinary completion |
| --- | --- |
|clear / victory|clear, unchanged, set, unchanged, unchanged|
|held / victory|clear, unchanged, held, unchanged, unchanged; skip after interpreter, still join0|
|clear / defeat or admitted retreat|441 retained,491 unchanged,541 clear,903/904 unchanged|
|held / defeat or admitted retreat|441 clear,491 unchanged,541 held,903/904 unchanged|

Intermediate effects precede final flags/transfer: a joined0 prefix can
exist before clear441; clear441 can precede set541. No atomicity, save
commit or audiovisual/input completion follows from source order.

## Map56 reconstruction and real transfer to the stairway

**Confirmed:** battle return enters `code/gameflow/mainloop.asm:alt_MainLoopEntry`,
runs SwitchMap then ExplorationLoop, bypassing immediate CheckBattle.
Unchanged victory supplies explicit56, so exploration clears event type,
adjusts/clamps STEP_COUNTER, heals/fades/detaches view, sets CURRENT_MAP56
and CURRENT_BATTLE=FF, loads tilesets, obtains Map56 setup entities,
clears/rebuilds ordinary entities and followers, loads sprites, clears
temporary flags256–383 and sets80. It initializes sprite counters,
targets physical0, loads fresh map/layout, installs base services and
runs the RTS setup init. Conditional palette/music/fade precedes events/input.

The predecessor owns Map56's complete setup, Creed NPC, ordinary19-row
follower table, three areas and fourteen warps. These are rebuilt from
live flags and the **saved return**, rather than retaining scene Reaper,
combat enemies/aliases or battle crop. Creed is logical128 after allocated
followers, not combat128 or a fixed physical slot. No outcome clears903/904.
Old scene action scripts/layout are not an exploration snapshot.
Defeat/retreat use this same ordinary destination interface for their
dynamic egress result; Map56 reconstruction is conditional on selecting56.

Map56 warp row1 (`6-warp-events.asm`, base$B7D30, row$B7D38) is
X255 wildcard,Y5, no-scroll, explicit Map60,57,24/Up. It contains no541,
903 or904 condition. The actual movement caller `entity/entityscriptengine_2.asm:loc_528A`
first requires candidate working-layout `(word & 3C00)==1000` before
WarpIfSetAtPoint. Zone marker1400 is a separate alternative; arrival
alone does not execute the Creed zone.

WarpIfSetAtPoint divides coordinates by384, scans authored order, stops
at FFFF and treats signed-negative trigger bytes as wildcards. A match
posts MAP_EVENT_WARP1; copies scroll/map/X/Y bytes to PARAM1–4 and facing/
padding to PARAM5–6; sets WARP_SFX. WaitForEvent/ProcessMapEvent clears
event type **before** dispatch. Scroll0 idles physical0, removes the
exploration return frame and returns map/X/Y/facing with d4=0 to MainLoop.
Negative destination bytes would retain individual coordinates; none of
the selected60 or direct return rows uses them.

MainLoop then runs SwitchMap and CheckBattle. No coordinate row names60,
so no battle is selected on that map; explicit60 rebuilds population,
temporary flags and base layout. This is a request/consumption/marker
contract, not proof a walk from victory return can reach Y5, that the
decoded tile has the marker, or that stairs/collision naturally connect.

## All eight Map60 files, setup and area selection

**Confirmed:** `data/maps/entries/map60/` has exactly these eight authored
ASM files; no mapsetups folder or msMap60 route. GetCurrentMapSetup in
mapsetupsfunctions_1.asm scans the complete map route table and falls
back to ms_Void=FFFF. GetMapSetupEntityList returns that terminator;
InitializeMapEntities still clears ordinary records, allocates ordinary
followers and Bowie0, but no fixed local NPC. RunMapSetupInitFunction
skips a FFFF setup, so there is no authored local init/callback scene.
It does not disable ordinary movement/warps or clear persistent flags.

| File | ROM interval / complete shape |
| --- | --- |
|00-tilesets.asm|$B9E38..$B9E3E: palette12; tilesets94,98,99,255,255|
|2-areas.asm|$B9E66..$B9F3A:7×30-byte records + FFFF word,212 bytes|
|3-flag-events.asm|$B9F3A..$B9F4C:2×8-byte copies + FFFF word,18 bytes|
|4-step-events.asm|$B9F4C..$B9F4E: endWord only|
|5-roof-events.asm|$B9F4E..$B9F50: endWord only|
|6-warp-events.asm|$B9F50..$BA00A:23×8-byte records + FFFF word,186 bytes|
|7-chest-items.asm|$BA00A..$BA00C: endWord only|
|8-other-items.asm|$BA00C..$BA00E: endWord only|

Every area has main/secondary parallax256,256 (unity), both autoscroll
pairs0,0, background start0,0 and MUSIC_BATTLE_THEME_1=34.
The complete ordered areas are:

| Row | Inclusive main start→end | Secondary foreground start | mainLayerType |
| --- | --- | --- | --- |
|0|52,0→63,11|0,0|255 foreground|
|1|0,0→15,14|17,0|0 background|
|2|0,16→15,30|17,0|0 background|
|3|34,0→47,12|0,0|255 foreground|
|4|0,32→23,55|25,0|0 background|
|5|34,15→50,29|0,0|255 foreground|
|6|52,10→61,26|0,0|255 foreground|

LoadMap scans source order and compares inclusive bounds×384 with the
selection point: d0=−1 uses tracked entity (physical0 when no view target);
the alternative encoded point uses its own shifts. First match wins;
terminator falls back to area0. Thus52..61,Y10..11 is in both0 and6,
but0 wins;52,12 and57,24 select6,61,10 selects0,13,2 selects1,
13,29 selects2,38,3 selects3,21,35 selects4,39,26 selects5. An uncovered
point selects0 natively, not a newly inferred connecting corridor.
The table specifies planes/music requests, not decoded geometry or audio.

`LoadMapBlocksAndLayout` loads blocks, decodes base layout, scans flag
copies in authored order, then chest-open writes and optional battle crop.
CURRENT_BATTLE=FF omits the crop in ordinary Map60. Each eight-byte flag
record is flag.w, source X/Y bytes, width/height bytes, destination X/Y
bytes; CopyMapBlocks operates words in the64-wide working layout.

| Order / guard | Source / size | Destination | Encoded copy operands |
| --- | --- | --- | --- |
|0 / F999|12,57 /4×4|12,52|0C39,0404,0C34|
|1 / F999|1,57 /4×3|1,52|0139,0403,0134|

Held999 applies16 words/32 bytes then12 words/24 bytes,28 words/56 bytes
of destination content in disjoint rectangles; record storage is16 bytes,
not56. Clear999 omits both. Each row independently reads the live flag;
a change between reads can apply only one. No selected outcome sets999.
Its comments identify an earlier shipdown state; that earlier scene is
not investigated here. Empty step/roof/chest/other tables supply no local
event/item records and do not prove collision-free movement.

## All23 warps and direct receiving interfaces

**Confirmed:** every row uses warpNoScroll0; fields are trigger X/Y,
scroll, target map, destination X/Y, facing and zero padding, all bytes.
255 is CURRENT only in target-map position; in trigger position it is
the authored wildcard. U/D/L/R mean Up/Down/Left/Right.

| Row | Trigger X,Y | Target / destination | Facing |
| --- | --- | --- | --- |
|0|53,1|57 /7,4|D|
|1|255,51|58 /13,36|U|
|2|61,10|CURRENT /13,2|D|
|3|62,10|CURRENT /14,2|D|
|4|13,1|CURRENT /61,9|U|
|5|14,1|CURRENT /61,9|U|
|6|1,7|CURRENT /1,29|U|
|7|2,7|CURRENT /2,29|U|
|8|1,30|CURRENT /1,8|D|
|9|2,30|CURRENT /2,8|D|
|10|13,30|CURRENT /38,3|D|
|11|14,30|CURRENT /38,3|D|
|12|38,2|CURRENT /13,29|U|
|13|39,2|CURRENT /13,29|U|
|14|45,8|CURRENT /21,35|D|
|15|46,8|CURRENT /22,35|D|
|16|21,34|CURRENT /45,7|U|
|17|22,34|CURRENT /45,7|U|
|18|3,53|CURRENT /39,26|L|
|19|39,26|CURRENT /3,53|R|
|20|47,19|CURRENT /53,17|R|
|21|53,17|CURRENT /47,19|L|
|22|255,26|56 /15,6|D|

Twenty internal CURRENT rows retain CURRENT_MAP through CheckBattle,
which finds no60 encounter. ExplorationLoop's−1 branch updates the main
entity/followers and retained layout, reruns services and skipped setup
init, but does not rebuild NPCs/aliases, clear temporary flags or reapply
F999 copies. A changed999 during an internal warp therefore does not
itself rebuild layout. Explicit57/58/56 rows instead request fresh maps;
even an explicit same-number target would use rebuild rather than CURRENT.

Dual entrances are not perfect inverses: rows2/3 arrive at13/14,2,
but4/5 both return61,9;10/11 both arrive38,3 and12/13 both return13,29;
14/15 arrive21/22,35 while16/17 both return45,7. Rows6/7 send X1/2,Y7
to the same X,Y29;8/9 send X1/2,Y30 to the same X,Y8.
Rows18/19 and20/21 are exact opposing
coordinate requests, subject to markers and the next movement event.
These asymmetries must not be normalized into a symmetric staircase graph.
At39,26 row19 precedes wildcard22 and wins **if WarpIfSetAtPoint is
called there**; Y26 alone does not guarantee return56. Adjacent overlapping
areas and warps do not prove connected or marked decoded tiles.

| Connection | Direct counterpart and receiver boundary |
| --- | --- |
|56 Y5→60,57,24/U|60 row22 Y26 wildcard→56,15,6/D, except earlier exact39,26. Explicit56 reconstructs Creed/followers/setup. Arrival Y6 misses41's Y24 trigger.|
|60 row0→57,7,4/D|Map57 `6-warp-events.asm` $B8612..$B8624 row0: Y4 wildcard→60,53,1/R. Return facing differs from entry; Map57's other exit is outside this receiver slice.|
|60 row1→58,13,36/U|Map58 `6-warp-events.asm` $B8A26..$B8A30 row0: Y37 wildcard→60,8,52/R. It does not return to the same60 departure/arrival point.|

For57/58, MainLoop calls **SwitchMap before CheckBattle**.
`data/maps/global/flagswitchedmaps.asm` ($7988..$799C) has three six-byte
records and a word terminator. Its selected third row is58,F999→59
(MAP_ZEON_BATTLEFIELD). SwitchMap changes d0 only;13,36/Up is retained.
Thus the declared58 target is not always the map consumed by CheckBattle:

| Supplied request / live999 | Effective receiver and CheckBattle rows |
| --- | --- |
|57,7,4/D / either|No switch. Old row1 `57,0,0,16,20,255,255`; held401 admits1.|
|58,13,36/U / clear|No switch. Old row7 `58,0,0,28,38,255,255`; held407 admits7.|
|58,13,36/U / held|Switch to59. Rows42 then43 both `59,0,0,28,38,255,255`; first held442/443 wins,42 before43.|

Each admitted row reads its own completed501/507/542/543: held completion
clears that selected unlock yet still returns the battle **on this call**.
Clear unlock skips without writing its crop; no matching unlocked row
returns−1. The table proves conditional42/43 interfaces, not which flags
are natural or that either fight is the next reached encounter. No battle
hook/initialization/content investigation follows here.

If no encounter, effective57/58 has no msMap setup: ms_Void fallback,
ordinary population/layout and no authored setup init. Effective59 instead
selects ms_map59, or ms_map59_flag543 when543 is held, through its existing
setup route. Stop at this receiving setup interface; its scene/content is
outside scope. Map58's authored return row is a direct counterpart only
when58 actually remains the receiver; it does not prove a59 return path.
The same999 can affect both Map60 copies and58→59 switching, but each
consumer reads its own live flag. No automatic999 write follows41.

## Actual preparation, final output and runtime consumers

**Confirmed about current code:** source lowering is not the final
producer. [prepare](../../../../src/sf2tool/remake_exploration_content.py) verifies
registered canonical identity and source pin, compiles requested programs
and battle routes, calls compile_maps, optionally prepares selected setup
variants, then serializes those maps and compiler.programs. No actual60
production import/output or scene execution is claimed in this slice.

The complete final [map compiler](../../../../src/sf2tool/remake_exploration_maps.py)
path for selected60 **outside eventMaps** is source-determined:

1. Missing setup route gives no fixed NPC records; ordinary population
   is attached only when selection includes outcomeEgressMap. Setup=null,
   input=null (no entities/step rows), no battle route from source60.
2. Create native `map-60-setup` frontier. Two flag records produce
   `map-60-flag-layout`: branch if999 clear past native-copy0; branch
   if999 clear past native-copy1; jump to setup. onLoad names that wrapper.
3. The final route-absent pass overwrites **setup's instructions** with
   end; it does not replace the wrapper or onLoad. Both clear reads
   reach end. Unchanged held999 stops at native flag-layout-copy0;
   copy1 also remains in the serialized wrapper. This is the corrected Map54 final-overwrite
   lesson applied to the actual producer, not a claim every absent setup
   is an unconditional unsupported frontier or every output is executable.
4. eventMaps would select a different callback/layout branch requiring
   setup references; absent60 has no setup to bind. Do not assert that
   adding eventMaps60 admits it. Its typed flag-copy capability elsewhere
   does not resolve this route's missing binding automatically.

| Actual owner | Existing capability / selected gap |
| --- | --- |
|[canonical importer](../../../../src/sf2tool/h2/map_import.py), [map compiler](../../../../src/sf2tool/remake_exploration_maps.py)|Transport area/flag/warp data and preserve authored warp order. Admitted destination in selection, scroll0, coordinate bounds allow typed warp with preserve for CURRENT/rebuild otherwise; missing destinations become warp-frontiers. Actual60 layout/materials/selected output are not supplied here. Native F999 wrapper remains a reached stop despite final setup=end. Selected compiler/MapTransfer use the declared destination directly; no native58,F999→59 SwitchMap binding is present.|
|[ExplorationContentReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs), [MapTransfer](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs), [WorkingMapLayout](../../../../remake/src/Sf2.Remake.Domain/Maps/WorkingMapLayout.cs)|Read load modes and typed layoutEvents; Build applies held flag copies in order, preserve keeps current world/layout, rebuild applies entry flags and reconstructs. Typed word copies exist, but this selected non-event output supplies native-copy stops rather than typed60 layoutEvents. Actual population/sprites, palette, destination traversal and input must still be bound/admitted.|
|[MapEventDispatcher](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/MapEventDispatcher.cs), [ExplorationViewRunner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ExplorationViewRunner.cs)|Marker/first-match event and ordered inclusive area selection exist. These unity/zero-scroll profiles fit supported layer parameters. Uncovered-area initialization throws field-view-area instead of native first-area fallback. Music/presentation resources and completion need their actual bindings; table transport is not audible/rendered acceptance.|
|[battle preparation/routes](../../../../src/sf2tool/remake_exploration_battles.py), [PrivateBattleEncounterReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs), [PrivateBattleScenarioReader](../../../../remake/src/Sf2.Remake.Content/Scenarios/PrivateBattleScenarioReader.cs)|prepare_encounter admits2/3; private reader pins/region admission1/2/3. Route compilation rejects41's coordinate Y24 before hooks/outcome binding. Generic after/defeated/join and configured outcomeEgressMap exist; neither complete41 nor dynamic native EGRESS_MAP/Church selection is admitted. Existing1/7 table relationships do not grant playable57/58 encounters.|
|[BattleOutcomeRules](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/BattleOutcomeRules.cs), [Sf2OutcomeReturnPolicy](../../../../remake/src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs), [BattleOutcome](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs)|Represented living/positioned actors, leader-defeat priority, guarded after-program, join/clear/set and return stages exist. Active membership uses First/Single and rejects missing positions; it does not perform unchecked native byte/alias reads/wrapping. Defeat selects configured egress, refuses399-clear/F64/F640 variants; nonzero status refresh refuses. Arbitrary RAM slots, retained death list and interrupted native prefix parity are not established.|
|[physical action](../../../../remake/src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2PhysicalAction.cs), [program runner](../../../../remake/src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs), [capability owner](../../../../remake/docs/capability-status.md)|Action/reward/death cleanup and typed programs exist. No actual41 enemy-drop table, dropped18, staff removal/award or Deals fallback is bound by these consumers; retained equipment/AI limits from first-control still apply. Reached UnsupportedInstruction refuses before advancement; FFFF native interpreter housekeeping is not proved by a typed end. No full import failure ordering or41 retreat implementation is asserted.|
|[adapter presentation](../../../../remake/game/src/Exploration/ExplorationPresentation.cs), [session view](../../../../remake/game/src/Exploration/ExplorationSessionView.cs)|Generic admitted map/entity/text/battle projection exists. Actual56/60/57/58 content, marker geometry, presentation completion and usable input have not been observed. The accepted runnable through-Battle03 entry boundary remains separate.|

**Inferred:** future explicit content work can reuse typed maps, ordered
copies/areas/warps, population and outcome stages after resolving selected
admission, drop/Deals, dynamic egress, empty-wrapper semantics and actual
materials. These are integration boundaries, not silently added engine
work or a claim that static research makes this route engine-ready.

## Reproduction, preserved results and stop

Ignored `local/issue1168/review.py` retains readable selected-source and
consumer snapshots, complete small tables, ordered operand/branch/call
checks, flag/item/address derivations, link/date/private/owned-diff checks.
No new maintained checker or tests of verification programs. Use a fresh
receipt name and load private selections in the launching process:

```powershell
. ./local/private-inputs.ps1
uv run --locked python -X utf8 local/issue1168/review.py <configured-source-disasm-root> --receipt local/issue1168/direct-reproduction-01.json --committed --snapshots local/issue1168/direct-03.json
git diff --check origin/main
uv run sf2 verify plan --base origin/main --head HEAD
```

ADR0004 adversarial review applies provenance, complete data/widths,
selector polarity, mutation order, changed states, aliases, final producer,
actual consumers and public/private boundary. New maintained parser/
schema/fixture/mutation-test and H3-callback items are N/A. Normal/full
Python verification, old comparisons, SDK/Godot/emulator/H3/H4/screenshots
are **SKIPPED** under this explicit documentation-only scope. Committed
planner, actual scoped CI, resources, failures and frozen candidate/process
identity belong to the Issue/PR handoff; main-gate independently accepts/merges.

Preserve #1162's followEntity high-byte/Bowie-HP missed-finding PASS and
accepted correction, including author/direct-main-gate/CI39725455473
passes and corrected narrow/CI39726160754. Preserve #1160 geometry/local
failures/direct02 boundary, Map54 final-overwrite correction, Map53
UnboundLocalError with zero partial output, twelve completed old comparison
failures, and #1166's completed recipe/read failures and accepted direct/
CI39730133738 results. Their owners/handoffs retain exact details. None
is rerun, erased, called interrupted, upgraded or made an automatic repair queue.

**Unknown:** natural incoming HP/membership/inventories/aliases/flags/RNG,
actual kills/staff receipt/Deals stock, interleaved services and partial
prefixes, decoded layouts/markers/collision/terrain, source/ROM equality
beyond accepted evidence, natural stairs and old-encounter reach, camera/
fade/text/audio/render/DMA/VInt/input completion, hardware timing, save
persistence and playable41/56/60/57/58 support. No runtime queue follows.
Freeze a clean pushed Draft PR, wait for actual scoped CI/process terminal
state and notify main-gate once. No self-merge, cleanup, archive or next Issue.
