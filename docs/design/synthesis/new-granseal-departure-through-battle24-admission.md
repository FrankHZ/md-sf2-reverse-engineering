# New Granseal Departure through Battle24 Admission

Leaving New Granseal opens another stretch of overworld travel. The selected
south exit enters Map73; its east border scrolls into Map72 while retaining
the current exploration actors. A later zone callback can request Battle24,
the approach to North Parmecia. **Inferred:** the formation stages that approach;
it does not establish a compulsory journey or the enemies' executed tactics.

**Confirmed static:** travel, request production, ordered battle selection and
fresh encounter initialization are separate steps. Neither Janet's recruitment
nor Cannon/Dynamite possession is a prerequisite for this selected route or
Battle24 admission. **Unknown:** natural travel, supplied state, completed
services, visible encounter, usable control and save/load persistence. This
guide ends after the fresh start dispatcher returns, before `ActivateEnemies`.

## Evidence and Reading Boundary

The accepted Layer A owner is
[New Granseal departure through Battle24 admission](../../research/new-granseal-departure-through-battle24-admission.md),
corrected and accepted through [#977 / PR979](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/979).
It pins USA retail SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`
and [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6).
Source paths below are relative to `disasm/`; Fnnn means flag nnn. The roadmap
owns the [Standard scope agreement](../documentation-roadmap.md#current-baseline-and-near-term-synthesis).
This explanation introduces no original-game finding or remake behavior.

The [Janet guide](new-granseal-janet-recruitment-and-departure.md) supplies the
preceding town encounter, not an admission requirement. The accepted
[request-state](../../research/map-event-request-state.md),
[request-consumption](../../research/map-event-request-consumption.md) and
[Battle23 roster](../../research/battle23-initialization-and-roster.md) owners
supply shared mechanisms. The [encounter-definition](../contracts/battle-encounter-definition.md)
and [cutscene-routing](../contracts/battle-cutscene-routing.md) contracts retain
their structural boundaries; #977 supplies this selected route and roster's
deeper review. Combat, Battle24 outcomes/Egress, North Cave preparation/blast,
Rohde joining and Battle25 belong to separate outcomes.

## Leaving Town and Retaining the Overworld

**Confirmed static:** the selected warp rows in
`data/maps/entries/mapNN/6-warp-events.asm` request:

| Step | Match | Destination and facing input |
| --- | --- | --- |
| Map6 south exit, row 1 | any X, Y31 | fresh Map73 `(29,53)`, Down; no scroll |
| Map73 east edge, row 1 | X63, any Y | scroll Right into Map72, X1 and retained Y; Right in row |

Map6 row 2 `(31,14)` is a separate row with the same destination; it does not
prove traversable terrain. Map73 `(29,52)` returns to town `(20,30)`, Up.
Its south edge, row 2 `(any,63)`, scrolls to **Map68**: the source symbol is
`MAP_OVERWORLD_SOUTHWEST_PARMECIA=68`. At `(63,63)`, east row 1 wins first.
This preserves the accepted symbol-to-number correction. None of these
selected rows tests Janet, joined/active membership, items, F805/F806 or F523.

A real warp still requires the character controller's masked `$1000` layout
kind, a matching scan and survival of the single mutable pending slot.
`WarpIfSetAtPoint` divides internal coordinates by 384 and writes event1,
parameters1–4, facing/padding at parameter5 and WARP_SFX. Negative coordinate
bytes are wildcards. Coordinates in this guide do not prove a walk or layout
reach. Dispatch clears the type before handling, not every parameter.

For the town's no-scroll request, `ProcessMapEventType1_Warp` submits/clears
WARP_SFX, idles physical 0, unwinds exploration and samples its tile position.
It replaces only the low bytes for nonnegative requested coordinates, takes
facing from parameter5 and returns through MainLoop's SwitchMap/CheckBattle.
Ordinary sampled high bytes are a premise, not table normalization. Map73 has
no battle-map row or switched replacement, so an intact request can reach
ordinary exploration. Neither warp branch clears PLAYER_TYPE.

Fresh explicit Map73 entry clears pending, reduces STEP_COUNTER by 20000 as a
word and clamps a negative result to 0, heals living/immortal allies, sets the
map and CURRENT_BATTLE255, loads tiles, rebuilds entities, clears temporary
F256–383 and sets F80 before load/init. Persistent story/party flags survive.
Its default setup has no fixed residents. Enabled followers still populate:
F65 selects the Caravan prefix before ordinary followers; otherwise only the
ordinary table applies. Peter/F67, Luke/F75, Fairy/F69 and the alternative
F74/F68/F70 FOLLOWER_B declarations can affect order and mapping. Later logical
mapping writes do not remove earlier physical actors. No row declares Janet;
joined18/active50/F806 do not reconstruct her town following chain. F805 alone
does not recreate Astral. Force membership and map followers remain distinct.

Map73 init preserves supplied history: F700 clear runs `cs_503A6`, then sets700;
with700 set, F508 clear returns, while F508 set/F705 clear runs `cs_50806` and
then sets705. With700/508/705 set it returns. These earlier scenes remain with their
owners. Map73's default zone callback itself returns immediately.

The east scroll follows another path. It sets CURRENT_MAP72 and runs
`ProcessMapTransition`, loads the selected blocks/layout/tilesets and performs
the overworld scroll. It replaces physical 0's current/destination X, retains Y
for `$FF`, and samples current facing: the row's Right byte is **not applied**.
X1 assumes a zero returned upper byte, because replacement affects only the
low byte before word multiplication by 384; the Y register is explicitly cleared.
This path returns inside exploration. It does not call MainLoop/CheckBattle,
rebuild residents/followers, heal, clear temporary flags or run Map72 init.

Raft/mode state also survives. Fresh population's raft-player mode 2 changes
main's sprite, stands followers and declares a separate raft at `(64,64)`.
Otherwise F64 and matching RAFT_MAP permit its stored masked position at
physical31, lookup offset63, facing Left; FOLLOWER_B lookup31 is separate.
The scroll/current-map `DeclareRaftEntity` returns for mode 2; mode 1 skips
ordinary follower relocation, while other modes relocate the current follower
list before the F64/map gate. A mismatch hides the separate raft. These calls
do not prove boarding, landing or completed following.

Both maps use default setup rows and empty resident lists. A later fresh
explicit Map72 entry rebuilds and resets state; a CURRENT255 no-battle reload retains
actors/temporary flags but still loads and runs init. Map72 init returns if
F716 is clear or Luke10 already joined; otherwise it calls the accepted Luke
scene. East scrolling skips that init. Fresh, current and suspended histories
cannot be substituted for one another.

## A Zone Can Produce a Request

**Confirmed static:** Map72's zone dispatcher selects one first match. A
specific callback that declines does not fall through to the default.

| Ordered zone rows | Predicate | Selected boundary |
| --- | --- | --- |
| 0–2 | X32,33,34; any Y | helper26 |
| 3–8 | X25–30, Y37 | cave callback |
| default row 9 | `$FD` marker after no specific match | F523 clear selects helper8; set selects helper24 |

The cave row remains a separate boundary even when it returns immediately.
Its F750/F805/F256 and item branches do not become Battle24 prerequisites.
This guide does not explain cave preparation or its scene.

The continuation, [Battle24 outcomes through North Cliff preparation](battle24-outcomes-through-north-cliff-preparation.md),
explains actual end counting, victory versus Egress returns and the later
missing-item warning through its local retry/control boundary. Actual removal,
blast/Rohde, cave interior and Battle25 remain separate scope.

Walking's masked `$1400` path writes event6 and two coordinate words at
parameters1/3 **before** the later raw blocking check. A blocked tail can leave
a zone event without an admitted step. Admitted movement sets destination and
travel and decrements STEP_COUNTER only when signed-positive. Mode1 instead
selects `eas_Raft`/opcode7, and mode 2 `eas_Caravan`/opcode8; neither uses this
walking producer. These source controller names differ from the numeric
Caravan1/raft2 mode constants. A coordinate does not call default every frame.

The zone handler installs `eas_Init` on physical 0 and passes the coordinate
words to the selected setup/table. It then closes portrait/text, waits a VInt
and physical 0 position=destination. It does not join all followers or zero
existing travel. WaitForEvent restores camera0 and the mode-selected controller
only if pending is zero. Working layout, mode, request survival and service
completion remain premises.

Within default, F523 chooses exactly one helper. F524 is the selected Battle24
completion flag inside `CheckRandomBattle`, not the default selector:

| Battle24 helper state | Result |
| --- | --- |
| F524 clear, any counter | request immediately; no RNG |
| F524 set, nonzero counter, including negative | decline; leave counter; no RNG |
| F524 set, zero counter; range8 result0 | request; no second draw |
| F524 set, zero counter; range8 result nonzero | decline; range4 draw+2 sets counter2–5 |

The RNG updates its 16-bit seed by multiply13/add7 and scales to the range;
these are arithmetic bounds, not observed independent probabilities. Battle8
and26 use the same helper with their own flags.

The request branch sets F424, writes `$000100FF` at MAP_EVENT_TYPE and sets
counter 30000 before waiting for scrolling, submitting SFX_BOOST and requesting
the flash. Flash operand 30 becomes four LaunchFading calls through shift/DBF,
then DuplicatePalettes; fading waits depend on VInts. No F524 write or direct
CheckBattle call occurs here. An interrupted service can leave earlier writes.

That longword writes type word1, parameter1=0 and parameter2=CURRENT255.
Parameters3–5 and WARP_SFX survive. Just after a zone producer, parameter3/4
contain Y's high/low bytes, while parameter5 is older state. Thus an ordinary
small Y can yield requested X0 and Y=zone Y; it is not an all255 coordinate/
facing tuple. Other writers can leave other residuals. The no-scroll consumer
samples main, applies surviving bytes and returns to MainLoop. This partial
request carries no immutable battle identity or guarantee of completed reload.

## Ordered Admission and Fresh Entry

**Confirmed static:** `CheckBattle` resolves CURRENT255 and scans map/unlock/
trigger rows in order. Map72 has three candidates:

| Battle | Crop origin and size | Trigger | Unlock / completion |
| --- | --- | --- | --- |
| 8 | `(1,32)`,32x30 | `(255,255)` | F408 / F508 |
| 24 | `(1,32)`,32x30 | `(255,255)` | F424 / F524 |
| 26 | `(28,0)`,36x32 | `(255,255)` | F426 / F526 |

The first unlocked row wins; wildcard triggers do not test rectangle
containment. F408 can intercept helper24's request. With408 clear/424 set,
24 wins even if426 is set. With both clear,426 can select26; no unlock permits
ordinary exploration. A selected completion flag clears that row's unlock
**after selection** and still returns its battle on this call. It does not
continue scanning. Supplied424 can independently admit24 without F523 or the
narrative route. Changed map, overwritten pending or cleared424 can sever the
producer-to-admission connection.

Battle24's crop is Map72 `(1,32)`,32x30. All formation coordinates below are
battle-local: `(x,y)` corresponds to Map72 `(x+1,y+32)`. The crop and wildcard
trigger do not themselves position the main actor. MainLoop passes the
selected map72/battle24 to BattleLoop.

BattleLoop first clears PLAYER_TYPE and sets399. With F88 clear it clears
seconds, saves incoming map/battle, assigns current values, sets base VInts
and invokes before. After restoring/reassigning map/battle it clears region
flags90–105, heals allies, initializes allies then enemies, clears AI memory,
loads battle for combatant 0, and invokes start. No exploration population/init
intervenes and no later failure rolls back the completed prefix.

Both before/start slot 24 entries, at byte offset 48 in their separate 48-word
tables, select `ms_Empty`, the `$FFFF` terminator at `$47A4E..$47A50`.
Before dispatches only with intro F474 clear and does not set it. Start sets474
**before** dispatch; already-set474 skips both hooks while fresh healing,
placement and load still run. The interpreter clears its skip latch, consumes
the terminator and conditionally waits for scrolling if a dialogue window
remains, then clears VIEW_SCROLLING_SPEED. There is no Battle24-local opening
dialogue, join or scripted movement.

F88 set selects supplied suspended state: restore saved seconds, clear88,
clear AI memory, target view0 and LoadBattle, then jump to individual turns.
It skips incoming map/battle assignment, before, region reset, healing,
placement and start; the following fresh @Start branch is unreachable. The
fresh formation below and this guide's start-return endpoint do not describe
an arbitrary suspension.

## The Initial Encounter

**Confirmed static:** spriteset24 is 378 bytes at `$1B4D2C..$1B4EA6`: four header
bytes, twelve ally and sixteen enemy records of 12 bytes, three regions of12
bytes and one point of 2 bytes. Subsections begin at byte offsets 4/148/340/376.
The actual subsection consumer selects by CURRENT_BATTLE*4 and preceding
counts/strides. Source row numbers are not physical actor numbers.

### Party placement follows membership and health

All thirty allies first get X=-1. UpdateForce rebuilds joined-and-active
members in ascending ally-ID order. The first twelve **processed members**
bound placement, rather than twelve guaranteed living heroes. A zero-HP member
consumes a loop slot without advancing the placement pointer; the next living
member uses that unconsumed coordinate. Surplus members LeaveBattleParty,
clearing active and leaving X=-1, without immediately rebuilding cached lists.
There is no initial empty-list guard before the first member read.

Healing already visited all 30 independently of membership. Living allies and
Peter7/Lemon28 get HP/MP maxima and status masked to existing STUN/POISON/CURSE,
then stats/equipment rebuilding; other dead allies skip. This is not blanket
revival. Janet can be reserve, dead or differently ordered; joining does not
guarantee active membership, health or a slot. Exploration followers do not enlist.

Placement indexes 0–5 are `(4,18),(3,17),(3,18),(2,18),(3,19),(2,17)`;
6–11 are `(2,19),(2,16),(3,16),(1,16),(1,17),(1,18)`. Only X/Y is copied.
The rows' HEALER1, NOTHING, NONE orders and15/15 regions do not replace allies'
identity, inventory or AI. The Layer A
[complete slot/consumer account](../../research/new-granseal-departure-through-battle24-admission.md#complete-ally-slots-and-enemy-configuration)
retains the exact fields and exceptional states.

### Sixteen enemies and their authored orders

The northern group centers on Dark Bishop128 at `(25,7)`, also point0.
Dark Sniper129 and Skeleton130 follow128. Another group centers on Skeleton134,
followed by Bishop135 and Skeleton136; Sniper141 follows Skeleton140 farther
west. The remaining demons, snipers and skeletons carry the orders below.
These are stored instructions and regions, not observed movement or a tactical
plan. FOLLOW_TARGET0 uses combatant operand0; it is not a fixed map destination.

| Combatant / row | Type (definition ID) | Local X/Y | AI | Primary order | Regions primary/secondary | Authored extra item |
| --- | --- | --- | --- | --- | --- | --- |
|128 /0|Dark Bishop81|`(25,7)`|LEADER|MOVE_TO point0|1/15|NOTHING `$007F`|
|129 /1|Dark Sniper62|`(25,5)`|ATTACKER3|follow128|1/15|NOTHING `$007F`|
|130 /2|Skeleton17|`(24,8)`|ATTACKER3|follow128|1/15|Medical Herb `$2000`|
|131 /3|Lesser Demon45|`(28,10)`|ATTACKER1|NONE|1/15|NOTHING `$007F`|
|132 /4|Dark Sniper62|`(18,13)`|ATTACKER1|NONE|1/0|NOTHING `$007F`|
|133 /5|Skeleton17|`(21,14)`|ATTACKER1|NONE|1/0|Medical Herb `$6000`|
|134 /6|Skeleton17|`(17,18)`|ATTACKER1|NONE|15/15|Medical Herb `$2000`|
|135 /7|Dark Bishop81|`(18,19)`|HEALER3|follow134|15/15|Blizzard `$6011`|
|136 /8|Skeleton17|`(18,20)`|HEALER3|follow134|15/15|Medical Herb `$2000`|
|137 /9|Lesser Demon45|`(22,25)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|138 /10|Lesser Demon45|`(21,26)`|ATTACKER1|NONE|0/1|NOTHING `$007F`|
|139 /11|Skeleton17|`(11,21)`|ATTACKER1|NONE|15/15|Medical Herb `$2000`|
|140 /12|Skeleton17|`(9,25)`|ATTACKER1|NONE|15/15|Medical Herb `$6000`|
|141 /13|Dark Sniper62|`(8,24)`|ATTACKER3|follow140|15/15|NOTHING `$007F`|
|142 /14|Lesser Demon45|`(14,3)`|ATTACKER1|NONE|1/2|NOTHING `$007F`|
|143 /15|Lesser Demon45|`(8,8)`|ATTACKER2|FOLLOW_TARGET0|1/2|NOTHING `$007F`|

All secondary orders are NONE, filler0 and spawn STARTING0. The source's
FOLLOW_ENEMY0/6/12 become actual combatants128/134/140. LEADER14,
ATTACKER1/2/3=6/7/9 and HEALER3=2 are AI inputs. Enemy slots144–159 instead get
activation0, HP0 and X=-1; their absent path's trigger read is not another
valid source row. Battle32's Jaro exception does not apply.

The three type4 regions retain ordered local vertices:

| Region | Vertices |
| --- | --- |
|0|`(9,18),(13,29),(31,29),(31,18)`|
|1|`(19,0),(10,18),(31,18),(31,0)`|
|2|`(0,0),(0,10),(10,17),(19,0)`|

Their second byte and trailing bytes are0; no geometry meaning is assigned to
those bytes. Shared consumers distinguish polygon tests, region15 sentinel,
point0 `(25,7)` and combatant-follow orders. Region tests split type4 into
triangles1/2/4 and3/2/4; MOVE_TO's bit6/low nibble selects the point. No such
order or trigger has executed at this endpoint.

### Copied definitions, items and repeated encounters

With F524 clear, each enemy copies its selected 56-byte definition, HP/MP maxima,
row position, movetype/AI, index, orders and regions before items/activation,
difficulty and final stat rebuilding. Fresh definitions supply:

| Type | HP/MP | Base ATT/DEF/AGI/MOV before difficulty | Equipped weapon | Four spell slots |
| --- | --- | --- | --- | --- |
|Dark Bishop|39/38|28/38/27/5|Flail|AURA, BLAST LV3, NOTHING, NOTHING|
|Dark Sniper|38/0|11/37/35/5|Hyper Cannon|four NOTHING|
|Skeleton|30/0|26/40/30/6|Broad Sword|four NOTHING|
|Lesser Demon|40/21|49/37/32/6|none|BLAZE LV3, BOOST LV2, NOTHING, NOTHING|

Definitions begin with status NONE and other item slots empty; their own
level/resistance/prowess/spell-power fields survive. This is configuration,
not spell execution or a claim that zero-MP enemies cast. Non-NOTHING row
items enter the first empty slot through AddItem's `$807F` mask. Herbs become
`$0000`, Blizzard `$0011`: authored `$2000` USABLE_BY_AI and `$4000`
UNUSED_ITEM_DROP bits are stripped. Fresh copied weapons remain in slot0;
these additions use slot1 and are not equipped. Authored flags do not prove
stored AI-use or drop behavior.

STARTING0 retains only the definition's `$F000`: fresh Bishops get `$3000`,
other types `$2000`; definition BIT4/BIT5 are discarded and active/neutral bits
are clear before ordinary activation. Difficulty applies floor(baseATT*5/4)
on all settings and a second time for SUPER2: Bishop35/43, Sniper13/16,
Skeleton32/40 and Demon61/76, ordinary/SUPER. These are initialized **base ATT**;
equipped/current-stat effects follow. They are not final attack or damage.

Battle24 belongs to `list_RandomBattles`. Before copying, the upgrade helper
checks membership and completion F524. Clear524 retains the original type
without an upgrade draw. Set524 permits Bowie's effective level/class and RNG
to choose an upgrade, even on direct supplied424 admission. Classes beyond
the last unpromoted add 20; subtracting 24 and quotient by 10 gate further work.
Nonpositive or zero quotient returns the original. Higher values use movetype
ranges/exclusions with retry/default branches. The airborne table's empty
exclusion list enters a subtract 1 scan without a zero guard. No successful,
uniform upgrade or single repeated roster/definition/equipment is guaranteed.
Coordinates, AI and item inputs remain the spriteset's; copied definitions and
resulting initialization can differ. See Layer A's
[repeat-state review](../../research/new-granseal-departure-through-battle24-admission.md#fresh-definitions-and-repeat-state-distinction).

### Physical actors and the final boundary

LoadBattle clears/rebuilds map entities. It scans allies0–29, admitting
nonnegative positions, then regular enemies in combatant order, skipping
neutral/negative positions. Logical-to-physical lookups map the actors actually
admitted to contiguous normal physical slots. Combatant128 is not physical128.
Fresh enemy mapsprites145/126/81/109 use the normal path; Battle24 has no neutral
entry. Local coordinates are masked by `$3F`, scaled by 384, facing Down with
`eas_Standing`. Exploration followers and Janet's temporary actor are replaced.

LoadBattle targets combatant 0's mapping, initializes sprites, loads map,
mapsprites and terrain, restores base VInts, requests music/fade-in and returns;
it does not repair an absent combatant 0. The empty start wrapper then returns.
**Stop before `BattleLoop:@Start` calls `ActivateEnemies`.** Cleared region
flags, stored orders and STARTING0 do not prove activation, turn generation,
combat, presentation completion or playable control.

## Changed State and Interrupted Prefixes

| Change | Conditional consequence |
| --- | --- |
|No Janet/items/805/806|Selected route and later admission still depend on their actual predicates, not those histories.|
|Earlier Map73 history flags|700/508/705 can select an earlier init scene before travel.|
|East scroll with battle unlocks already set|Scroll still selects no battle; a later MainLoop scan can.|
|Specific26/cave match declines|No default24 fallthrough, even with523 set.|
|523 clear; separately supplied424|Default chooses8; independent ordered admission can still choose24.|
|524 set, negative/nonzero counter|Helper24 declines; walking only decrements positive counters.|
|424 request with408 held|Battle8 wins the next Map72 scan;424 can remain.|
|424/524 set,408 clear|24 is selected and424 cleared on that call; upgrade logic can differ.|
|No unlocks, CURRENT255 reload|Actors/temp flags remain but Map72 init can select Luke through716/10.|
|474 set,88 clear|Skip hooks; fresh heal/placement/load still run.|
|88 set|Supplied suspended state; no common fresh roster or start-return guarantee.|
|Dead/reserve Janet, altered active list, >12 active|Membership order, HP and processed-member limit govern placement.|
|Request/flag write before stalled flash/wrapper|Earlier pending/424 can exist without MainLoop admission.|
|474 set before stalled interpreter tail|Set intro flag does not prove completed control or persistence.|

**Unknown:** natural route/layout and request production; supplied flags,
mode/raft/follower state, party, HP, class, level, RNG, mapping/capacity validity;
request survival and completed scroll/follower/flash/fade/DMA/music/input;
actual upgraded repeats, visible encounter, usable control and save/load
persistence. These limits remain explicit and do not authorize runtime work.

## Evidence Review and Retained Results

This synthesis reviews the complete corrected #977 Layer A against selected
existing public fixtures, schemas and actual consumers. The
[map-events zone/routing owner](../../../tests/fixtures/h2/map-events-static-v1.json),
[map-init](../../../tests/fixtures/h2/map-init-static-v1.json) and
[random-battle state](../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)
retain zone choice, callback branch/order, init history and helper writes/services.
The [spriteset fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json)
retains addresses/counts with `rowContentTracked=false`; the
[routing fixture](../../../tests/fixtures/h2/battle-routing-data-static-v1.json)
retains route occupancy with `routeTargetsParsed=false`. Neither aggregate
proves private detailed roster rows or exact slot24 targets. Those selected
facts and actual placement/stat/upgrade/load consumers come from accepted #977.
The structural contracts' wider Unknowns do not undo that bounded finding or
justify expanding this guide to all battles.

Owning commands are `uv run sf2 h2 map-events`, `uv run sf2 h2 map-init`,
`uv run sf2 h2 map-event-random-battle-state`, `uv run sf2 h2 battle-spriteset-data`
and `uv run sf2 h2 battle-routing-data`, plus Layer A's retained-pair/consumer
recipe. Source/pair are absent in this synthesis checkout. Public schema and
consumer checks are not a fresh pinned-source or pair comparison; nothing is
acquired or regenerated. No selected contradiction was found. Material
contradictions return to the research owner rather than being repaired here.

[#977 corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977#issuecomment-6180317175)
and [PR979 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/979#issuecomment-6180326745)
retain final1a58b934/tree583dfb93/base27bedb07 and CI38650800988 (scope12s,
research29s), original1b4b1409/CI38650285331 and intermediate32f9c13d/
CI38650658600 (scope21s/research40s). Both distinct0.080s unchanged-pair
receipts remain retained. Main-gate's P2 corrected the south-scroll numeric
meaning to Map68 by joining `MAP_OVERWORLD_SOUTHWEST_PARMECIA` through the
pinned enums; the earlier failing check and corrected direct check remain.
The [original #977 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/977#issuecomment-6180256540)
retains the masked relative-pointer expectation,34 statements versus35 anchors
including the sound operand, region-row counting, CRLF byte failure and failed
PowerShell edit, all corrected through narrow direct checks. Its initial
nonexistent `battlecoords.asm` lookup was corrected to the actual
`data/battles/global/battlemapcoords.asm`; truncated reads were narrowed.
Fixture goldens and completed suites were not changed to erase those results.

[#978 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/978#issuecomment-6180262572)
and [PR980 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/980#issuecomment-6180278753)
retain dce4622f/treebecd9b9f/base2c8d441e, CI38650369688 (scope18s),514 links,
independent review and corrected inspector category key, schema path and
program-local ordinal comparison. The Janet guide's
[retained review](new-granseal-janet-recruitment-and-departure.md#evidence-review-and-retained-results)
preserves #973's45records/246bytes, author0.093s/independent0.096s pair receipts,
CI38648632852 and alias/private-regex corrections, #974's original/final CI and
KeyError correction, #969/#970/#965 results, Battle19/32-byte-stride, #954
callback and #962 Fairy corrections, linked failures and Unknowns. These
completed results are not reset, reclassified or repeated to replace receipts.

The Issue handoff records full prose/diff/navigation/links/anchors, UTF-8,
private/exact-ownership and `git diff --check`, the same-process configured
clean committed planner and actual exact-head CI. Documentation-only work
uses direct checks without blanket Python/.NET/Godot/H3/H4/translation suites.
Independent acceptance and integration remain with main-gate.
