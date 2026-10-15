# North Cliff Entry through Battle25 Admission

Opening North Cliff's passage and recruiting Rohde do not immediately start
another battle. The stored entrance route leads from Map72 `(27,33)` to
Map48 `(55,29)`, facing Up. If that request reaches ordinary admission with
unlock F425 set, it selects the North Cave encounter, Battle25. The opening,
the journey to the entrance and the battle selector have separate conditions.

**Confirmed static:** the request, ordered selection, fresh initialization,
initial combatants and map/camera configuration described here. **Inferred:**
this stages the North Cave encounter after Rohde opens the passage.
**Unknown:** natural traversal, supplied state, completed services, rendered
darkness, usable control and save persistence. The endpoint is the fresh
start-dispatcher return immediately before `ActivateEnemies`; no combat,
outcome, later cave crossing or Ketto arrival is included.

Continue with [Battle25 outcomes through Ketto entry](battle25-outcomes-through-ketto-entry.md)
for actual termination, member-derived returns, separate cave departures and
conditional town initialization. That guide retains the later admission,
mutable-state, service and natural-route limits through the controller request.

## Evidence and Reading Boundary

Continue from [North Cliff opening and Rohde recruitment](north-cliff-opening-and-rohde-recruitment.md).
The accepted Layer A owner is
[North Cliff entry through Battle25 admission](../../research/north-cliff-entry-through-battle25-admission.md),
accepted through [#989 / PR992](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/992).
It pins USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Source paths are relative to `disasm/`; Fnnn denotes flag nnn. Indices are
zero-based. The roadmap owns the
[Standard scope agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
This English synthesis consumes accepted evidence without adding an original
finding or remake behavior. Concurrent Battle25-outcome research and UI work
are independent and supply no evidence for this guide.

The [map-entry routing contract](../contracts/map-entry-routing-state.md)
owns the bounded selectors, while the
[battle encounter contract](../contracts/battle-encounter-definition.md)
keeps map, battle, placement, combatant and script-route identities distinct.
Their aggregate boundaries are supplemented by #989's selected source review;
neither contract alone establishes this encounter or a natural campaign route.

## From the Opened Passage to a Battle Request

**Confirmed:** the opening scene's live layout copy, unlock F425 and later
caller F750 are separate writes. Fresh Map72 uses F750 to reconstruct the opened
layout. Neither live nor reconstructed opening proves that the party reaches
an entrance tile or produces a warp event.

The first Map72 warp row stores `(27,33)` → Map48 `(55,29)`, Up, without
scrolling. The row adds no F750, F425, F524, Rohde membership, item, HP or class
predicate. Supplied F750 clear/F425 set state is not rejected or repaired by it;
physical access remains a separate question. The walking controller first
requires a warp-kind layout attribute. `WarpIfSetAtPoint` then converts
internal coordinates to map tiles and scans the rows in order. This selected
row has exact source coordinates, although the scanner supports negative-byte
wildcards in other rows.

The producer writes the pending type 1 request, destination tuple, facing and
warp sound. That slot is mutable: another producer can replace it before
consumption. `ProcessMapEvent` clears the type before dispatch, retaining
other parameter bytes. For this no-scroll warp the consumer submits/clears
the sound request, idles physical entity 0, unwinds exploration and samples
main's position. It replaces destination coordinate low bytes and applies
Up before returning to `MainLoop`. Ordinary sampled high bytes are a premise
of the nominal `(55,29)` words; arbitrary upper bytes are not normalized.
Admission tests low bytes, while later exploration placement uses words.
The warp itself does not clear player mode.

`MainLoop` runs `SwitchMap` and then `CheckBattle`. The accepted switch table
has no replacement for maps 48 or 72. Map48's complete candidate set, in scan order,
is:

| Battle | Battle-area crop X/Y/width/height | Exact trigger X/Y | Unlock / completion |
| --- | --- | --- | --- |
|11, CAVE_OF_DARKNESS|`(0,0,27,32)`|`(15,26)`|411 /511|
|25, NORTH_CAVE|`(28,0,32,32)`|`(55,29)`|425 /525|

Each candidate tests map, unlock and trigger coordinates. The area rectangle
is output geometry, not a containment test. At `(55,29)` Battle11 fails its
trigger even with F411 set; Battle25 qualifies only with F425 set. At `(15,26)`
Battle11 can qualify, whereas Battle25 cannot. At `(52,12)`, `(3,11)` or another
nonmatching point, neither qualifies merely because the point lies inside a
cave rectangle. A changed effective map can select another battle or none.

On a match, the selector stores all four area bytes before checking
completion. With F425 and F525 both set, it clears F425 **and still returns 25 on
that same call**. It does not resume scanning. With F425 clear, F525 alone admits
nothing. Retaining F425 with F525 clear permits another supplied admission;
clearing F425 before consumption removes it. No F475, F750, scene-completion,
party, HP, class or RNG predicate is added. `MainLoop` passes the selected
index to `BattleLoop` without first running Map48 exploration population/init.

Other stored Map72 cave routes request Map48 `(52,12)` from `(25,28)`,
`(15,26)` from `(58,49)`, and `(3,11)` from `(56,45)`, all Up. They explain why
a different entrance request is not interchangeable with the selected one.
They do not prove passage between the two cave areas.

## When No Battle Qualifies

**Confirmed:** explicit ordinary Map48 exploration entry still has shared
setup behavior. It clears pending state, reduces the step counter by 20000
with signed clamp, heals living/immortal allies, sets current map 48 and
battle 255, loads tiles, rebuilds entities/mapsprites, clears temporary flags 256–383
and sets F80 before `LoadMap` and the init hook. Persistent story, party and
battle flags survive.

There is no Map48 setup-table row. The actual fallback is `ms_Void`, a
`$FFFF` word returned by `GetCurrentMapSetup`. Its entity-list accessor
returns that sentinel: initialization clears old actors, builds flag-selected
followers, skips fixed NPC rows on the first `$FF` byte, then declares main entity 0
at the supplied coordinates/facing. The init dispatcher sees `$FFFF` and
skips the function call. Absence of a dedicated setup does not remove shared
population or loading.

Map48 is an ordinary follower map, without the overworld Caravan prefix
from F65. Peter (F67), Astral (F70) and Rohde (F84) are independent conditions applied in
source order. Rohde's joined flag 11/active flag 43 is not follower flag 84; the old live follow
chain is not carried forward as a fixed cast. Supplied aliases, HP/class and
mode can change mappings and sprites. Loading Map48 does not call Battle11's
before-scene, and Battle25 has no separate scene population.

A CURRENT255 request heals first but retains current map/battle, updates main
properties and raft, and omits explicit population/temp reset. A scroll
request stays inside exploration, bypassing `MainLoop`/`CheckBattle` and the
ordinary entry. A direct script `LoadMap` is also a different lifecycle.
These routes do not substitute for the selected explicit no-scroll request.
The no-battle branch retains its supplied player-mode premise; fresh
`BattleLoop` clears player mode. A return label alone proves no usable control.

Map48 has four immediate stored returns, all no-scroll, facing Down and
requesting Map72:

| Map48 match | Requested Map72 destination |
| --- | --- |
|`(55,30)`|`(27,34)`|
|`(52,13)`|`(25,29)`|
|`(15,27)`|`(58,50)`|
|`(3,12)`|`(56,46)`|

The first is beside the selected entrance; it is not proof of a reachable
escape tile or Battle25 Egress. Ordinary return checks Map72's wildcard
battles 8/24/26 and unlocks 408/424/426 in order, including their completion
handling, and can select a battle instead of fresh exploration. No later trip or broad optional cave
exploration is implied.

## Fresh Entry and the Empty Introduction Hooks

**Confirmed:** `BattleLoop` clears player mode and sets F399 before testing F88.
With F88 clear, fresh entry clears seconds, saves the incoming map/battle,
stores current map 48/battle 25, installs base VInts and invokes the before hook. After
its return the loop restores/reassigns map/battle, clears region flags 90–105,
heals allies, places allies then enemies, clears AI memory, loads the battle
for combatant 0 and invokes the start hook. That order matters: health and the
rebuilt active list determine placement before physical entities and the
camera are selected. Exploration `(55,29)`, Up is not a forced battle-main
position/facing. Interruptions retain the reached prefix without rollback.

Both 48-word before/start routing tables resolve slot 25 to `ms_Empty`, the
single terminator at `$47A4E..$47A50`. These are actual resolved targets,
not a deduction from a missing battle directory or a table's entry count.
Both wrappers test intro flag F475. Before dispatches when clear without
setting it; start sets F475 **before** dispatch. Already-set F475 skips both
hooks but preserves fresh healing, placement and load.

The empty interpreter clears its skip latch, consumes the terminator,
conditionally waits for view scrolling if a dialogue window remains, clears
scroll speed and returns. Neither hook adds Battle25 dialogue, a camera
scene, an entity list, a join or movement, or Battle11's `csub_458E` darkness
refresh. Shared loading still configures the secondary layer.

With F88 set, continuation restores saved seconds, clears F88 and AI memory,
sets view target 0, loads using supplied current state and jumps to the
individual-turn loop. Incoming map 48/battle 25 do not replace that saved state. It skips
the fresh hooks, region reset, healing and placement; the following jump to
`@Start` is unreachable. This is not the fresh endpoint or evidence of the
initial formation described next.

## Who Actually Enters the Encounter

**Confirmed:** Battle25's spriteset supplies twelve potential ally placements,
fifteen starting enemies, three regions and zero AI points. Placement indices
are not ally identities. The twelve battle-local ally coordinates are:

| Placement | X/Y | Placement | X/Y |
| --- | --- | --- | --- |
|0|`(25,26)`|6|`(27,25)`|
|1|`(26,26)`|7|`(26,25)`|
|2|`(27,26)`|8|`(28,26)`|
|3|`(26,27)`|9|`(28,25)`|
|4|`(27,27)`|10|`(27,28)`|
|5|`(28,27)`|11|`(28,24)`|

Before placement, healing visits all 30 allies regardless of membership.
Living allies and the immortals Peter (7)/Lemon (28) receive max HP/MP, retain only
existing STUN/POISON/CURSE status bits and update derived stats. Other dead
allies are skipped; this is not general revival or a membership change.

Placement resets every ally X to −1, rebuilds the joined-and-active list with
`UpdateForce`, and traverses it in ascending ally-ID order. At most twelve
members are processed. A zero-word-HP member consumes a member/loop slot but
**does not advance the coordinate pointer**; the next living member receives
that unused placement. Surplus members leave the active party and get X=−1,
without an immediate rebuild of the cached list/count. The empty-list case
has no guard before its first read; no safe empty party is inferred.

Only X/Y is copied from an ally row. Its HEALER1, NOTHING, NONE orders and
15/15 regions do not replace the member's inventory or AI. Rohde's joined flag 11,
active flag 43, follower flag 84, health, class, equipment and actual placement are
separate facts. The earlier cannon scene guarantees neither an active healthy
Rohde nor a slot or equipped weapon here. Capacity and the rebuilt list still
apply. A living joined/active Bowie (0) normally takes placement 0; the later
camera example explicitly depends on that premise.

### The Fifteen Enemies and Their Relationships

The actual assignment consumer gives the following combatant IDs 128–142.
They are not physical map-entity indices. All rows are STARTING, with no
hidden/spawning identity substitution. All secondary orders are NONE;
regions show primary/secondary, with 15 the sentinel.

| Combatant ID | Enemy type (definition ID) | Local X/Y | AI role | Primary order | Regions | Authored extra item |
| --- | --- | --- | --- | --- | --- | --- |
|128|DARK_SOLDIER (18)|`(24,7)`|ATTACKER1|NONE|0/15|NOTHING|
|129|DARK_BISHOP (81)|`(21,10)`|HEALER3|FOLLOW_ENEMY0|0/15|NOTHING|
|130|LESSER_DEMON (45)|`(19,9)`|HEALER3|FOLLOW_ENEMY0|0/15|NOTHING|
|131|DARK_BISHOP (81)|`(6,6)`|HEALER3|FOLLOW_ENEMY4|1/1|NOTHING|
|132|DARK_SNIPER (62)|`(7,9)`|ATTACKER3|FOLLOW_ENEMY5|1/1|NOTHING|
|133|SKELETON (17)|`(5,10)`|ATTACKER1|NONE|1/15|MEDICAL_HERB `$6000`|
|134|LESSER_DEMON (45)|`(14,12)`|ATTACKER1|NONE|1/15|NOTHING|
|135|SKELETON (17)|`(10,16)`|ATTACKER1|NONE|2/15|MEDICAL_HERB `$2000`|
|136|DARK_SNIPER (62)|`(8,17)`|ATTACKER1|NONE|2/15|NOTHING|
|137|LESSER_DEMON (45)|`(6,28)`|ATTACKER1|NONE|2/15|NOTHING|
|138|DARK_SNIPER (62)|`(13,28)`|ATTACKER3|FOLLOW_ENEMY11|15/15|NOTHING|
|139|SKELETON (17)|`(14,27)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|140|SKELETON (17)|`(15,19)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$2000`|
|141|DARK_SNIPER (62)|`(18,21)`|ATTACKER3|FOLLOW_ENEMY14|15/15|NOTHING|
|142|SKELETON (17)|`(20,22)`|ATTACKER1|NONE|15/15|MEDICAL_HERB `$6000`|

FOLLOW_ENEMY0/4/5/11/14 refer respectively to combatants 128/132/133/139/142.
Thus the first Bishop and Demon follow the Soldier; the western Bishop
follows Sniper 132, which follows Skeleton 133; the two later following Snipers
refer to Skeletons 139 and 142. These are stored relationships, not movements
or attacks already performed.

All three regions are four-vertex type 4 shapes, in battle-local coordinates:

| Region | Vertices in stored order |
| --- | --- |
|0|`(0,0),(0,14),(31,14),(31,0)`|
|1|`(0,0),(0,31),(7,31),(31,7)`|
|2|`(0,0),(0,31),(18,31),(18,0)`|

The shared trigger consumer checks triangles 1/2/4 and 3/2/4. Region 15 is not
a fourth polygon. There are no AI points or MOVE_TO point orders. Clearing
region flags alone fires no tactic, and source-named AI roles do not establish
natural activation or a simulated battle strategy.

### Initial Definitions, Equipment and Difficulty

Enemy initialization copies the selected definition, restores HP/MP to maxima,
sets row coordinates, combines definition movement with row AI, and stores
index, trigger regions and orders. It adds the extra row item, sets activation,
adjusts base ATT for difficulty and updates current stats. The five selected
definitions are:

| Type | Level | Max HP/MP | Source base ATT/DEF/AGI/MOV | Movement | Equipped slot 0 | Four spell slots |
| --- | --- | --- | --- | --- | --- | --- |
|DARK_SOLDIER (18)|23|35/17|31/46/34/4|REGULAR|LARGE_AXE|ATTACK, NOTHING, NOTHING, NOTHING|
|DARK_BISHOP (81)|23|39/38|28/38/27/5|HEALER|FLAIL|AURA, BLAST LV3, NOTHING, NOTHING|
|LESSER_DEMON (45)|22|40/21|49/37/32/6|FLYING|NOTHING|BLAZE LV3, BOOST LV2, NOTHING, NOTHING|
|DARK_SNIPER (62)|24|38/0|11/37/35/5|STEALTH_ARCHER|HYPER_CANNON|four NOTHING|
|SKELETON (17)|21|30/0|26/40/30/6|REGULAR|BROAD_SWORD|four NOTHING|

All have ENHANCED spell power, initial status NONE and otherwise empty item
slots; the Demon has no equipped item. Soldier/Bishop/Sniper resistance is 0;
Demon has STATUS_MINOR; Skeleton has WIND_WEAKNESS, LIGHTNING_MINOR,
ICE_MAJOR, FIRE_WEAKNESS and STATUS_IMMUNITY. Soldier/Demon prowess is
CRITICAL150_1IN32, DOUBLE_1IN32, COUNTER_1IN16. Bishop changes counter to 1IN32,
Sniper changes double to 1IN16, and Skeleton changes critical to 1IN8. These
are copied configuration, not proved damage, accuracy or spell execution.

The five Skeleton herbs illustrate why authored item flags need their actual
consumer. `AddItem` chooses the first empty slot and masks the input by
`$807F`: it strips both `$2000` USABLE_BY_AI and `$4000` UNUSED_ITEM_DROP.
Both authored herb words therefore become stored `$0000` in slot 1 after the
equipped sword. They are not equipped; the source flags alone establish
neither stored AI use nor drops. No Bishop receives Battle24's extra Blizzard.

Every difficulty applies floor(source base ATT ×5/4); SUPER (2) applies it a
second time. Initialized base ATT is therefore:

| Type | Ordinary difficulties | SUPER |
| --- | --- | --- |
|Soldier|38|47|
|Bishop|35|43|
|Demon|61|76|
|Sniper|13|16|
|Skeleton|32|40|

These adjusted bases still pass through equipment and other current-stat
rules. They are neither final ATT nor damage values. Battle25 is absent from
the eleven-entry random-battle list, so the upgrade consumer returns the
original enemy type before completion, class/level or upgrade RNG checks.
Setting F525, promoting/levelling Bowie or changing RNG does not enable that
upgrade here. Later combat is not thereby RNG-free. F88 continuation skips
this fresh definition initialization altogether.

STARTING retains only definition activation bits `$F000`: Bishops get `$3000`,
the other four types `$2000`. Definition BIT4/BIT5 are discarded; active and
neutral bits are clear at this endpoint. Slots 143–159 instead get activation 0,
max/current HP 0 and X=−1; the absent path's trigger-byte read is not a valid
sixteenth enemy row. No Battle32 Jaro exception or NORTH_CAVE neutral-table
entry adds another initial participant.

## The Cropped Cave and the Actual View Target

**Confirmed:** battle loading takes Map48 source `(28,0)`, size 32×32, and
copies it to local origin 0. Roster coordinates are already local: `(x,y)`
corresponds to original Map48 `(x+28,y)`. Ordinary Bowie at local `(25,26)`
therefore occupies original `(53,26)`, not warp destination `(55,29)`.
The crop neither triggers admission nor chooses the camera descriptor.

`PositionBattleEntities` clears exploration actors, scans placed allies 0–29
then enemies in combatant order, skips neutral/negative entries and builds
fresh logical-to-physical mappings. Physical indices depend on the actual
party and admitted enemies; they are not 128–142. The selected enemy sprites
are ordinary, below the special-sprite threshold. Positions are masked by
`$3F`, scaled by 384 and initialized facing Down with `eas_Standing`. Rohde's
prior live following is replaced. `LoadBattle` selects combatant 0's mapping
for the view. A missing combatant0 is not repaired into a valid party; negative targets
fall back to physical 0 when sampling coordinates.

Map48 has two descriptors with different main bounds but identical remaining
configuration:

| Descriptor | Main start/end | Foreground start | Background start | Parallax pairs | Main / secondary autoscroll bytes | Layer / music |
| --- | --- | --- | --- | --- | --- | --- |
|0|`(0,0)..(26,31)`|`(0,0)`|`(28,33)`|256/256 each|0/0;128/128|255 / BATTLE_THEME_1|
|1|`(28,0)..(59,31)`|`(0,0)`|`(28,33)`|256/256 each|0/0;128/128|255 / BATTLE_THEME_1|

After fading out, loading tilesets, waiting, clearing VInts and positioning
entities, `LoadBattle` initializes sprites and calls `LoadMap` with −1.
Descriptor selection tests the view target's current internal coordinates
against scaled descriptor bounds. It **does not add crop X28** to that local
target. Ordinary local Bowie `(25,26)` therefore selects descriptor 0 despite
this being Battle25. At local X28 with valid Y it can select 1; X27 misses both
and falls back to 0 at the sentinel. No-battle main at world `(55,29)` selects 1
with the uncut layout. Changed party/camera state must retain this distinction.

`LoadMapArea` overrides battle main bounds to local 0–31 on both axes while
still consuming the selected descriptor's other fields. Because those fields
agree here, descriptor choice does not change the secondary-layer parameters.
Layer 255 is nonzero and takes the map-on-foreground branch. The start fields
are scaled into RAM/plane coordinates; parallax multiplies then shifts by 8.
The secondary autoscroll fields are **bytes**, and 128 is a follow sentinel,
not a signed constant velocity.

Shared `LoadMap` calls `sub_38C0` for initial plane-B coordinates when secondary
autoscroll is nonzero. For a nonnegative target and this layer type it uses
the background start and the displacement between the view target and
plane A; a negative target retains current plane B. The VInt scrolling consumer treats
128 specially: it recomputes that relation, writes plane-B destination and
speed 16 per axis, and sets scrolling bits. Plane-update consumers rebuild the
planes and request VRAM DMA. These are actual load/follow consumers; they do
not require Battle11's explicit darkness-refresh scene to be called.

Loading then restores base VInts, loads battle terrain, requests map music
and fade-in and returns to the empty start wrapper. The wrapper sets F475 if
needed and returns. **This guide stops before `ActivateEnemies`.**

**Inferred:** the secondary plane supports cave darkness relative to the
chosen actor. **Unknown:** composed mask, lit radius, occlusion/palette,
settled camera, VInt/DMA/frame timing, completed fade or usable control.
Battle11's explicit refresh is not a Battle25 rendered observation. See the
[Layer A map consumers](../../research/north-cliff-entry-through-battle25-admission.md#actual-map-crop-area-selection-and-secondary-layer)
for encoding/formula detail; no rendered-fidelity claim follows from it.

## Changed or Interrupted State

The ordinary path depends on the state actually consumed. These alternatives
keep that boundary visible rather than promising one campaign result:

| Changed premise | Consequence |
| --- | --- |
|750 and 425 differ|Layout reconstruction and battle unlock remain independent; the warp adds no repair.|
|Request overwritten /425 cleared|Inspect the consumed request; the original producer no longer guarantees25.|
|48 `(55,29)` with 411/425 set|11 fails its coordinate;25 qualifies.|
|48 `(15,26)` with F411 set|11 can qualify; do not substitute25's encounter.|
|48 other nonmatching coordinates|No cave battle solely from area containment.|
|425 clear /525 set|Completion alone does not unlock25.|
|425/525 both set|Clear F425 after selection, enter25 on that call with original definitions.|
|475 set|Skip empty hooks while fresh healing/placement/load still occur.|
|88 set|Use saved current state; skip fresh initialization and this endpoint.|
|Dead/reserve/unjoined Rohde or changed capacity|Join/follower facts do not guarantee placement, health or equipment.|
|Dead ordinary ally /dead Peter or Lemon|Ordinary dead skip healing; immortals heal before placement.|
|Empty active list /absent combatant 0|No guarded empty-party or valid-camera repair is inferred.|
|Changed Bowie class/level/RNG, still25|Ally stats/sprites can differ; no random enemy upgrade here.|
|SUPER /other difficulty|Two /one base-ATT scaling passes before current-stat rules.|
|Local view X25 /27 /28 with valid Y|Descriptor0 /fallback0 /descriptor 1; battle main bounds remain32×32.|
|CURRENT /scroll /explicit48 /direct LoadMap|Admission, population, mode and flag-reset lifetimes differ.|
|Interrupted request/heal/placement/load/start|Retain reached writes; no rollback, completed service, playable or saved-state guarantee.|

**Unknown:** collision-free natural entrance/return reach; supplied flags,
request, roster, HP/class/equipment, mode and mappings; completion of sound,
text/window tail, load, fade, scrolling and VInt/DMA services; darkness,
control readiness, uninterrupted progression and save/load persistence.
Supplied suspension state is not validated by the fresh chain. Outcomes,
later cave crossing/Ketto and actual combat remain separate scope. These
limits do not create an automatic acquisition or runtime queue.

## Evidence Review and Retained Results

The complete accepted #989 research and the #990 opening guide were reviewed,
including changed/interrupted states and the resolved byte-width discrepancy.
Selected public fixtures were checked against their schemas and actual
maintained consumers, with the following limits:

| Evidence relation | Checked public owners and boundary |
| --- | --- |
|Request and ordered admission|`sf2-map-event-request-consumption-static-v1`, `sf2-common-maps-static-v1`, `sf2-map-data-static-v1`, `sf2-battle-global-data-static-v1`; request consumption and selector order are distinct from private route rows.|
|No-battle setup/init|`sf2-map-setup-static-v1`, `sf2-map-init-static-v1`; setup/init structure does not alone prove the complete no-Map48 sentinel and follower path.|
|Empty hooks|`sf2-battle-cutscenes-static-v1`, `sf2-battle-routing-data-static-v1`, `sf2-map-script-engine-static-v1`; intro wrapper order is public, actual slot 25 target resolution consumes #989.|
|Encounter and healing|`sf2-battle-spriteset-data-static-v1`, `sf2-battle-loop-static-v1`; spriteset25 address/extent and shared healing are retained, complete roster/definitions/assignment consume accepted selected source review.|
|Crop and view|Common-maps fixture and map-data inventory retain crop/load structure, not composed darkness or every selected descriptor/byte consumer.|

The [map selector consumer](../../../src/sf2tool/h2/maps.py),
[request consumer](../../../src/sf2tool/h2/map_event_request_consumption.py),
[cutscene consumer](../../../src/sf2tool/h2/battle_cutscenes.py),
[spriteset verifier](../../../src/sf2tool/h2/battle_spriteset_data.py) and
[battle-loop consumer](../../../src/sf2tool/h2/battle_loop.py) were inspected.
The spriteset fixture explicitly says `rowContentTracked=false`; aggregate
counts, addresses and schema success do not independently prove these rows,
actual membership, natural activation or rendered services. Older aggregate
runtime questions remain general inventory limits, not a demand to repeat
research for this accepted static seam. No new in-scope contradiction was
found; material contradictions return to research.

Owning commands remain `uv run sf2 h2` with `map-event-request-consumption`,
`common-maps`, `map-data`, `battle-global-data`, `map-setup`, `map-init`,
`battle-cutscenes`, `battle-routing-data`, `map-script-engine`,
`battle-spriteset-data` and `battle-loop`. Layer A links its retained-pair
recipe. Source/pair absence in this design environment is accepted; the
public checks and prose review are **not a fresh comparison**. No acquisition,
regeneration, runtime, installation or new maintained tooling was performed.

[#989 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/989#issuecomment-6181049459)
and [PR992 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/992#issuecomment-6181060429)
retain head `b714f581`, tree `9955d447`, base `5dbcf49a`, successful
CI38656388954 (scope21s/research36s) and author's 0.107s unchanged
18,285,979-byte pair comparison. Original `18fee1d6`/CI38655534808 and reviewed
`7efb8771`/CI38655682054 remain retained. The earlier Battle11 note called
128/128 secondary autoscroll words; accepted macro `dc.b`, adjacent RAM bytes
and byte consumers established bytes, and research corrected that one line.
The ignored no-Map48 assertion was corrected to compare each msMap row's
first operand rather than its whole argument; manual absence evidence was
already correct. Guessed paths were resolved to actual consumers. These
corrections do not become new synthesis findings or rendered observations.

[#990 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/990#issuecomment-6180902294)
and [PR991 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/991#issuecomment-6180918917)
retain `428f3b8f`/tree `e5144030`, CI38655297200 (scope18s), 532 links and bounded
field/CLI discovery diagnostics. The opening guide's
[retained review](north-cliff-opening-and-rohde-recruitment.md#evidence-review-and-retained-results)
preserves #985/#986's identities, pair/CI receipts, LF-only failure, field/path
and hidden-X corrections, removed scratch length cap and linked #981/#982/#977
failures/Unknowns. Completed results are not reset or repeated for new receipts.

The #994 Issue/PR handoff records full prose/diff/navigation/link/anchor,
UTF-8/private/exact-five-path checks, `git diff --check`, configured clean
committed planning and actual exact-head CI. Documentation-only scope selects
no blanket normal/full/Python/.NET/Godot/H3/H4/translation suite. Independent
acceptance and integration remain with main-gate.
