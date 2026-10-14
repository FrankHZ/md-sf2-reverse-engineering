# Evil Spirit Shrine entry and scene admission

Evidence date: 2026-10-14. **Confirmed:** Map33's `(9,5)` exit requests Map34
at `(26,28)`, Left. Fresh Map34 exploration selects 21 baseline residents when
F784 is clear, or an empty resident list when it is set. Its init only returns.
A separately produced zone request whose Y low byte is 13 reaches a callback
that calls `cs_5B6C0` only while F784 is clear. **Inferred:** these declarations
prepare the basement consultation anticipated by Creed's departure dialogue.
**Unknown:** natural traversal, actual supplied state, service completion,
presentation, usable control and persistence. This document stops at the scene
call; it does not establish a completed consultation or its consequences.

## Agreement and evidence boundary

Standard rigor under [Issue #945](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945)
and [content epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514):
one static route/setup/request join, whose uncertainty is the distinction
between declared residents, actual allocation and scene admission. Sole owned
path is this document. Accepted starting base is
`2eddb2c6d00a748315d9a4671dca8429a8a1ea67`, including corrected #941/#937.
No shared index, registry, schema, fixture, design, tooling or remake writes;
concurrent #946 synthesis is disjoint and is not an evidence dependency.

Consume accepted [restoration and main-room departure](./creed-restoration-and-basement-departure.md),
[Creed entry](./creed-mansion-entry-and-first-goliath-scene.md),
[magic-door transfer](./creed-magic-door-and-desktop-transfer.md),
[Battle23 return](./battle23-outcomes-and-burrow-return.md),
[setup routing](./map-event-flag-route-selection.md) and
[request consumption](./map-event-request-consumption.md). The restoration
owner's conditional F783/Hassan writes and final actor state are not evidence
that the player walks to the exit or reaches this scene. No runtime,
acquisition, new environment, generalized renderer, scene internals, special
subroutine/explosion analysis, postscene return, recruitment or Oddler departure
is included.

All source paths below are relative to `disasm/` at
[SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
The assigned read-only checkout is clean at that pin. Retained evidence binds
USA retail ROM SHA-256
`9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
Setup filenames mean `data/maps/entries/map34/mapsetups/`; map/entity consumers
mean `code/common/scripting/map/` and `code/common/scripting/entity/`.
Fnnn means flag nnn; resident indices are zero-based. Numeric constants resolve
through `sf2enums.asm`, not narrative names. Address intervals are half-open.

Reuse the unchanged 18,285,979-byte map-init/map-script-engine pair. Linear
comparison budget is under one minute/512 MiB, plus small selected records and
prose/receipts; no network acquisition or model processing of entire payloads.
The retained comparison recipe completed in 0.084s with pair size/mtime
unchanged. Reassess missing evidence or material consumer expansion. Aggregate
fixture equality is supporting evidence, not proof of this composed prose.

## Mansion exit and fresh Map34 admission

**Confirmed:** the complete `data/maps/entries/map33/6-warp-events.asm`,
`$AC722..$AC734`, is these two eight-byte rows followed by endWord:

| Order / trigger X,Y | Type / destination map | Destination X,Y / facing |
| --- | --- | --- |
| 0 / `(9,5)` | no-scroll0 / EVIL_SPIRIT_SHRINE34 | `(26,28)` / LEFT2 |
| 1 / `(255,31)` | no-scroll0 / OVERWORLD_DEVILS_TAIL67 | `(26,23)` / UP1 |

`code/gameflow/exploration/exploration.asm:WarpIfSetAtPoint` sign-extends
candidate fixed-point X/Y, divides each by MAP_TILE_SIZE=384, and scans the
current map's warp table in order. The `$FFFF` word ends it. Negative trigger
bytes wildcard that axis; other trigger bytes compare only the quotient's low
byte. First match writes word MAP_EVENT_TYPE=MAP_EVENT_WARP1, copies the four
bytes type/map/X/Y to PARAM_1..4 and the facing word to PARAM_5..6, and sets
WARP_SFX. It does not check F783, F784, F523 or restoration completion.

The ordinary walking caller is `entityscriptengine_2.asm:esc02_controlCharacter`:
input and area bounds select movement offsets, slope handling can adjust them,
and `loc_51A8` forms the candidate from current position plus those offsets.
With entity obstruction enabled, an overlapping current/destination entity can
exit before map-event inspection. `loc_5220` converts the candidate into the
actual working-layout offset and masks the block with `$3C00`; `loc_528A`
calls WarpIfSetAtPoint for type `$1000`. The converter uses actor layer/current
battle state, applicable secondary/background origins and the map-offset hash;
a coordinate table alone proves neither that block type nor a successful walk.
The later collision/movement tail is not proof that an already written request
has been consumed. No pending-request guard exists at this writer.

`code/gameflow/exploration/explorationfunctions_2.asm:WaitForEvent` prioritizes
nonzero MAP_EVENT_TYPE. ProcessMapEvent clears that word before dispatch.
For type1 with PARAM_1=0, ProcessMapEventType1_Warp plays/clears WARP_SFX, idles
physical0, removes the exploration return address, and returns the tuple to
MainLoop. UpdatePlayerPosFromMapEvent first derives signed tile coordinates
from physical0's current position. Explicit nonnegative destination bytes then
replace only the low bytes of d1/d2. Thus `(26,28)` assumes their inherited
high bytes are zero; the explicit destination does not repair arbitrary supplied
high bytes. Facing is byte LEFT2 in cleared d3; d4 is zero.

`code/gameflow/mainloop.asm:MainLoop` calls SwitchMap then CheckBattle on this
ordinary post-warp return. The complete `data/maps/global/flagswitchedmaps.asm`
has no Map34 substitution; `data/battles/global/battlemapcoords.asm` has no
Map34 row. `code/common/maps/getbattle.asm:CheckBattle` returns d7=-1 and
restores the tuple, admitting exploration rather than a battle.

Explicit map34 exploration clears pending MAP_EVENT_TYPE, reduces/clamps
STEP_COUNTER, heals living/immortal allies, starts fade-out and disables view
following. It sets CURRENT_MAP=34/CURRENT_BATTLE=255, loads tilesets, waits for
fading, clears VInts, resolves the entity list, rebuilds actors and loads
mapsprites; then clears temporary flags256–383 and sets F80. F783/F784/F802
survive that reset. Common work initializes sprite-frame state, targets
physical0, calls LoadMap with d0=-1/d1=34, installs base VInts and invokes init.
The accepted full-map loader avoids battle cropping; arrival and resident
coordinates are not offsets from Battle23's cropped region. Palette comparison
then controls music/fade, followed by SetMoveSfx and WaitForEvent. With no
pending event it installs camera target0 and the actual mode's controller.
All of this is ordered conditional source flow, not a usable-arrival observation.

## Setup pointers and all resident identities

**Confirmed:** `data/maps/mapsetups.asm:113–115` contains only Map34 default
`ms_map34` and F784 override `ms_map34_flag784`, then msMapEnd. GetCurrentMapSetup
starts with the default and replaces it for each set flag; every accessor
reselects. F783, F523 and F802 do not participate in this Map34 selector.

| Pointer table / address | Entities pointer | Shared remaining five pointers, in order |
| --- | --- | --- |
| ms_map34 / `$5B5B6` | ms_map34_Entities `$5B5E6` | EntityEvents `$5B690`; ZoneEvents `$5B696`; AreaDescriptions `$5B6B4`; Section5 `$5B6B6`; InitFunction `$5B6BE` |
| ms_map34_flag784 / `$5B5CE` | ms_map34_flag784_Entities `$5B68E` | The same five pointers |

`sf2mapsetupmacros.asm:msFixedEntity` encodes X/Y/facing/mapsprite bytes plus
a long action pointer, eight bytes per row. `s1_entities.asm:5–25` is exactly
21 rows (`$5B5E6..$5B68E`) with **no terminator in that file**.
`data/maps/mapsetupsstorage.asm:426–427` includes it immediately before
`s1_entities_784.asm`, whose only record is msEntitiesEnd (`$FFFF`) at `$5B68E`.
That word terminates both the baseline scan after row20 and the alternate scan
at its first byte. The alternate means no residents, not no main/followers/raft.

The table resolves every row. I=`eas_Init`; F=`eas_InitFixedSprite` (complete
bodies below). All face UP1. N is the number of selected ordinary follower
records allocated before residents; the example column uses only F67/F68,
so N=2. The physical formulas describe allocation before any later overwrite.

| Row / source line | Logical ID / lookup offset | Mapsprite name = number | X,Y | Action | Physical slot / N=2 example |
| --- | --- | --- | --- | --- | --- |
| 0 / 5 | 128 / 32 | CREED=212 | 7,10 | I | N+1 / 3 |
| 1 / 6 | 129 / 33 | FAIRY=219 | 6,10 | I | N+2 / 4 |
| 2 / 7 | 130 / 34 | EVIL_SPIRIT=252 | 63,63 | I | 47 / 47 |
| 3 / 8 | 131 / 35 | BEASTMAN2=222 | 63,63 | I | N+3 / 5 |
| 4 / 9 | 132 / 36 | BOY=200 | 63,63 | I | N+4 / 6 |
| 5 / 10 | 133 / 37 | EFFECT1=63 | 63,63 | F | N+5 / 7 |
| 6 / 11 | 134 / 38 | EFFECT1=63 | 63,63 | F | N+6 / 8 |
| 7 / 12 | 135 / 39 | EFFECT1=63 | 63,63 | F | N+7 / 9 |
| 8 / 13 | 136 / 40 | EFFECT1=63 | 63,63 | F | N+8 / 10 |
| 9 / 14 | 137 / 41 | EFFECT1=63 | 63,63 | F | N+9 / 11 |
| 10 / 15 | 138 / 42 | EFFECT1=63 | 63,63 | F | N+10 / 12 |
| 11 / 16 | 139 / 43 | EFFECT1=63 | 63,63 | F | N+11 / 13 |
| 12 / 17 | 140 / 44 | EFFECT1=63 | 63,63 | F | N+12 / 14 |
| 13 / 18 | 141 / 45 | EFFECT1=63 | 63,63 | F | N+13 / 15 |
| 14 / 19 | 142 / 46 | EFFECT1=63 | 63,63 | F | N+14 / 16 |
| 15 / 20 | 143 / 47 | EFFECT1=63 | 63,63 | F | N+15 / 17 |
| 16 / 21 | 144 / 48 | EFFECT1=63 | 63,63 | F | N+16 / 18 |
| 17 / 22 | 145 / 49 | EFFECT1=63 | 63,63 | F | N+17 / 19 |
| 18 / 23 | 146 / 50 | EFFECT1=63 | 63,63 | F | N+18 / 20 |
| 19 / 24 | 147 / 51 | EFFECT1=63 | 63,63 | F | N+19 / 21 |
| 20 / 25 | 148 / 52 | EFFECT1=63 | 63,63 | F | N+20 / 22 |

`mapfunctions.asm:InitializeMapEntities/loc_440E2..loc_44180` clears actors
and the 64-byte lookup table, starts followers at physical1, and consumes
residents with coordinates masked `$3F` and multiplied by384. All these sprite
values are at least COMBATANT_ALLIES_NUMBER30, so none takes the fixed-ally
lookup/deduplication path. Ordinary rows append a lookup entry at offset32
onward and advance the physical counter. The sprite252 row instead satisfies
MAPSPRITES_SPECIALS_START240: it appends ENTITY_SPECIAL_SPRITE47, calls
DeclareNewEntity at physical47 with d6=ENTITY_ENEMY_START32, and restores the
ordinary counter without incrementing it. It therefore consumes a logical
lookup entry but not the next ordinary physical slot. Its stored ENTNUM32 is
not its lookup logical130 or its physical47. EFFECT1=63 follows the ordinary
branch, despite the effect name. These sixteen rows are distinct allocations.

`entityfunctions_1.asm:DeclareNewEntity` writes current/destination coordinates,
facing, sprite, action pointer, ENTNUM and layer, and clears travel/velocity.
Here d6's high word is zero (layer0); ordinary residents' ENTNUM equals their
physical slot. Main/Bowie is declared afterward at physical0, using the supplied
arrival coordinates/facing, GetAllyMapsprite and eas_Idle. Shared SPRITE_SIZE is
set to16. A `(63,63)` declaration remains allocated; it is not HideEntity or a
claim of visible offscreen staging. No effect animation, summoning or explosion
is proven by these names, coordinates or action pointers.

`data/scripting/entity/eas_actions.asm:168–180` defines I completely: acceleration
factors0/0, speed32/32, acceleration OFF/OFF, deceleration OFF/OFF, entity
obstructable ON, entity uncollidable OFF, map uncollidable OFF, auto-facing ON,
orientation Up, immersed OFF, size24, updateSprite, jump eas_Idle.
Lines194–209 define F with the same prefix through immersed OFF, then
auto-facing OFF, motion OFF, size24, updateSprite, jump eas_Idle.
`data/scripting/entity/eas_main.asm:eas_Idle` waits1 and branches back.
`sf2cutscenemacros.asm` encodes the actions; the entity interpreter's
esc0B_setSpriteSize writes the shared SPRITE_SIZE word. The installed pointers
are asynchronous programs, not evidence that they ran, settled or rendered.
The size write does not establish a persistent per-actor physical/body scale.

`followersfunctions_1.asm:InitializeFollowerEntities` and the complete
`data/scripting/entity/followers.asm` retain independent flag-selected rows.
Map34 is absent from `data/maps/global/overworldmaps.asm`, so F65 adds no Caravan
prefix here. In the N=2 example Peter7 is physical1, B31/Oddler physical2, with
initial supplied arrival coordinates/facing and successive follower actions.
Changing flags changes N and action-pointer assignment. Repeated B rows
(F74 Astral, F68 Oddler, F70 Astral) allocate independently and later overwrite
the same B lookup. Missing Peter/B leaves zero lookup, potentially aliasing
physical0. F69's Fairy is FOLLOWER_C158/offset62, separate from resident129.
F74 and F83 can likewise supply additional A/D rows; party membership does not
replace follower flags. There is no local repair for arbitrary crowded or
inconsistent supplied populations.

The accepted GetAllyMapsprite and exploration-healing consumers remain relevant
to main/followers: actual class and HP choose ally sprites; zero-HP allies outside
battle use the blue flame. Healing includes living/immortal allies, not general
revival. All 21 resident sprite declarations are independent of ally HP/joined
flags. `mapscriptengine_1.asm:GetEntityAddressFromCharacter` masks logical IDs
with `$FF`, subtracts96 for high-bit IDs and reads the lookup. It does not reject
missing/aliased mappings. A fresh F784 empty list leaves resident lookups zero;
clearing F784 later cannot manufacture the missing residents.

PLAYER_TYPE is not reset by this route or init; walking0 is the coherent
predecessor premise. `followersfunctions_2.asm:sub_44404` can add a raft at
physical31 with lookup offset63 when F64 is set and RAFT_MAP=34, using saved
coordinates masked `$7F`. Mode2 instead changes main's sprite to RAFT, stands
followers and declares the separate raft at `(64,64)` without that saved-map
gate. This later declaration can overwrite physical31 under an excessive
supplied ordinary population; the table's formulas do not certify collision-free
allocation for all flag combinations. No boarding or saved-raft relocation is
performed by the Map34 init. SetControlledEntityActScript uses actual mode:
walking0, source-named eas_Raft for Caravan1, otherwise source-named eas_Caravan
for raft control. It does not validate arbitrary mode values here.

## Return-only local surfaces and the three warps

**Confirmed:** both setups point to the following complete local surfaces.
These are local absence claims; shared wrappers can still perform work.

| Owner / interval | Complete local content and immediate meaning |
| --- | --- |
| s2_entityevents.asm / `$5B690..$5B696` | Default record only, target Map34_DefaultEntityEvent `$5B694`, whose body is rts. No local resident-specific dialogue or branch. |
| s3_zoneevents.asm / `$5B696..$5B6B4` | `(255,13)` specific row, then default; callback and gate below. |
| s4_descriptions.asm / `$5B6B4..$5B6B6` | ms_map34_AreaDescriptions is rts, not a description-record array. RunMapSetupAreaDescription clears d7 before calling it, so it supplies no handled result. |
| s5_itemevents.asm / `$5B6B6..$5B6BE` | Default item record only, target Map34_DefaultItemEvent0 `$5B6BC`, body rts. No item-specific local script. |
| s6_initfunction.asm / `$5B6BE..$5B6C0` | ms_map34_InitFunction is rts. No scene, flag, positioning, pending request or EGRESS_MAP assignment in this init. |
| Map34 3-flag-events / `$AD492..$AD494` | endWord only. |
| Map34 4-step-events / `$AD494..$AD496` | endWord only. |
| Map34 5-roof-events / `$AD496..$AD498` | endWord only. |
| Map34 7-chest-items / `$AD4B2..$AD4B4` | endWord only. |
| Map34 8-other-items / `$AD4B4..$AD4B6` | endWord only. |

The entity wrapper still resolves portrait/speech state, controls entity VInts,
invokes the default and closes windows; default modifier0 selects neither of
its optional facing/property-update branches. The item wrapper still selects
its default, closes windows and waits for VInt/physical0 coordinate equality.
Generic search/item behavior is not globally absent. RunMapSetupInitFunction
saves/restores d0–a1 around selection/call and supplies no zone-style closure
or coordinate wait. Its return permits the conditional exploration tail
already described; it does not automatically invoke the zone.

The complete `data/maps/entries/map34/6-warp-events.asm`, `$AD498..$AD4B2`,
is three eight-byte rows followed by endWord. Each uses the same immediate
candidate/layout matcher and no-scroll caller as the mansion exit:

| Order / trigger | Destination map / position / facing | Immediate admission |
| --- | --- | --- |
| 0 / `(26,28)` | CREED_MANSION33 / `(9,5)` / RIGHT0 | SwitchMap has no Map33 substitution; CheckBattle has no Map33 row. Explicit33 rebuilds the population and runs the current flag-selected Map33 init, owned by the accepted Creed documents. No particular postscene return state is assumed here. |
| 1 / `(22,29)` | CURRENT255 / `(24,2)` / LEFT2 | SwitchMap leaves255; CheckBattle resolves the current map for lookup, finds no Map34 battle, and restores255. Exploration preserves current map/population rather than declaring residents again. |
| 2 / `(24,2)` | CURRENT255 / `(22,29)` / RIGHT0 | The same retained-population path with the opposite destination/facing. |

There is no F784/F783/F802 condition in these warp records or their immediate
matcher. All explicit X/Y replacements retain the high-byte premise above.
For CURRENT255, ExplorationLoop's MapIndexNotProvided branch waits fading,
uses UpdateMainEntityProperties to update main's current/destination coordinates,
facing and ally sprite, and calls DeclareRaftEntity. That helper can reposition
existing followers to the supplied destination (walking mode), skip that loop
for Caravan1, or return early for raft2; its F64/saved-map raft branch remains
independent. It does not rebuild resident lookups, clear map-setup temporary
flags or set F80 through the fresh-map branch. Common LoadMap/init/control work
still runs, using current map34 and the current setup's return-only init.

The two area records in `data/maps/entries/map34/2-areas.asm`, `$AD454..$AD492`,
declare main-layer rectangles `(0,0)..(26,25)` and `(19,25)..(29,34)`. The two
CURRENT warps join coordinates in those different declared regions; this is a
data relation, not proof of a walked stairway, active area, loaded collision
block or automatic reverse warp. Likewise arriving at `(26,28)` is not itself
proof that the return warp's candidate/layout writer executes. Fresh reentry
reselects/rebuilds; a current-map transfer retains the actual existing residents,
even if F784 has changed since their creation.

## Actual zone request and scene-call boundary

**Confirmed:** `entityscriptengine_2.asm:esc02_controlCharacter/loc_5220` reads
the candidate's actual working-layout word after the input/bounds/slope and
entity-obstruction path described above. At `loc_5294`, masked type `$1400`
writes MAP_EVENT_ZONE_EVENT6. It derives PARAM_1 X as signed word
`(XDEST + d4) / 384` and PARAM_3 Y as signed word `(YDEST + d5) / 384`, with
word additions before sign extension/division. The layout candidate used current
position plus offsets; the request uses destination plus offsets. They agree
under the usual settled-position premise, not under arbitrary current versus
destination state. A subsequent collision check can suppress movement after
the event was written; writing type6 alone is not proof of completed movement.
This writer neither reads F784 nor validates that a setup row will match.

WaitForEvent returns pending type6; ProcessMapEvent clears MAP_EVENT_TYPE before
selection. ProcessMapEventType6_ZoneEvent clears d0, calls j_ApplyInitActscript,
then loads word PARAM_1 into d1 and PARAM_3 into d2 before j_RunMapSetupZoneEvent.
`entityfunctions_2.asm:ApplyInitActscript` uses **physical0** directly and only
replaces its action pointer with eas_Init. It has no ally-HP guard, no immediate
coordinate snap and no wait for that action to execute. The call therefore
does not establish that main is already stopped when the callback begins.

`mapsetupsfunctions_1.asm:RunMapSetupZoneEvent` saves d0–a1, calls the current
setup selector, loads its ZoneEvents pointer and scans four-byte rows from
d7=0. The exact first byte `$FF` wildcards X; exact `$FF` in Y would wildcard Y.
Otherwise comparisons use the request's low bytes. `$FD` selects the default;
relative callback offsets are added to the table base. This is narrower than
the warp matcher's any-negative-byte wildcard rule.

The complete Map34 zone table is:

| Address / source | Selector and target |
| --- | --- |
| `$5B696`, s3_zoneevents.asm5 | X255 wildcard, Y13; offset8 to Map34_ZoneEvent0 `$5B69E` |
| `$5B69A`, line6 | Default marker `$FD,0`; offset28 to Map34_DefaultZoneEvent `$5B6B2` |

Both setup pointers supply this same table. With request Y low byte13, X is
irrelevant and the callback receives the supplied d1/d2 words, d7=0 and a0
pointing to Map34_ZoneEvent0. With another Y, d7=4 selects the default. A nonzero
request Y high byte does not prevent low-byte13 from matching. In particular,
arrival `(26,28)` and the two CURRENT destinations `(24,2)`/`(22,29)` do not
by themselves match Y13 or generate type6. No region-wide trigger at every X
is proven without the actual `$1400` candidate and request producer.

The complete callback is six operations, verified against the retained
`tests/fixtures/h2/map-events/zone-programs.json` Map34_ZoneEvent0 record:

| PC / source line | Operation / boundary |
| --- | --- |
| `$5B69E` / 14 | chkFlg784 |
| `$5B6A2` / 15 | bne.s Map34_DefaultZoneEvent: set F784 skips the scene and both writes |
| `$5B6A4` / 16 | script cs_5B6C0: clear F784 admits the call; **stop at this call** |
| `$5B6AA` / 17 | setFlg784, reachable only after the selected script returns |
| `$5B6AE` / 18 | setFlg802, after the preceding flag write |
| `$5B6B2` / 21 | rts, shared default target |

`sf2macros.asm:script` expands to `lea cs_5B6C0(pc),a0` and trap MAPSCRIPT6.
`code/common/tech/interrupts/trap6_mapscript.asm` saves d0–a6, activates
VInt_UpdateEntities and calls j_ExecuteMapScript; the jump-interface alias in
`code/common/tech/jumpinterfaces/s07_jumpinterface.asm` resolves to
ExecuteMapScript. The same interface resolves j_GetMapSetupEntities to
GetMapSetupEntityList and the init/zone aliases to their named wrappers.
This establishes a0's selected target and the actual call boundary, not the
contents or outcome of cs_5B6C0. No commands inside it are reviewed here.

F784 independently controls setup selection and this callback gate. F783 is
not checked here; F802 is not checked either. An externally supplied F784 set
can select an empty population and suppress the call without proving the scene
ran. Conversely F784 clear admits the call even with F783 clear, F802 already
set or a retained empty/mismapped population. There is no population, follower,
HP, restoration-completion or EGRESS_MAP precondition in the callback.

The caller's two flag writes are conditional future effects only. Interruption
before return can leave neither caller write; interruption between them can
leave F784 set/F802 unchanged. Since scene internals are outside scope, no claim
is made that these are the scene's only potential writes or effects. The writes
are not an atomic transaction and prove no save/load persistence.

If the callback returns (including the F784-set/default path), the zone wrapper
closes portrait/text, waits one VInt, then calls WaitForEntityToStopMoving on
physical0. That helper waits for both current coordinates to equal destination
coordinates, not for every actor action or service to finish. It then restores
the wrapper's saved registers. The MAPSCRIPT trap likewise restores its saved
registers only if the script call returns. ApplyInitActscript runs before the
F784 test even when the callback will be skipped. These wrapper boundaries do
not establish postscene player control; pending state, actual motion and service
completion still govern whether the caller can proceed. Analysis ends without
following a postscene route, recruitment or departure.

## Changed-premise checks and reproduction

The following are static discriminators, not executed save-state scenarios:

| Changed premise | Consequence supported at this boundary |
| --- | --- |
| Fresh explicit34, F784 clear | Declare all21 residents plus selected followers/main/possible raft; return-only init. No automatic scene. |
| Fresh explicit34, F784 set | Empty resident list; same local init/events. Y13 callback skips call and both writes. This is not evidence of past completion. |
| Repeat explicit34, still F784 clear | Rebuild baseline even if an earlier attempt failed or was interrupted; no scene-completion inference. |
| CURRENT255 transfer on34 | Retain actual resident allocation/lookup state while relocating main and eligible followers; changing flags does not rebuild it. |
| F784 set after baseline creation | Later selector/gate changes; existing residents are not retroactively removed. |
| F784 cleared after empty creation | Call can be admitted with zero resident lookups; callback creates no missing actors. |
| F783/F523/F802 changed, F784 unchanged | No change to Map34 setup or local callback polarity. Upstream Map33 init/history can differ independently. |
| Different follower flags/class/HP | Physical slots, aliases and ally sprites can differ; fixed resident IDs/sprites and callback gate stay independent. |
| Different PLAYER_TYPE/raft state | Allocation/control/retained-transfer behavior can differ; no local normalization to walking. |
| Table coordinate supplied without layout producer | No naturally produced request is proven. Directly supplied type6/Y13 still matches the wrapper independently of layout. |
| Candidate uses current position, destination differs | Layout classification and destination-derived request can refer to different tiles. Do not substitute one for the other. |
| Y low byte not13 | Default rts; no call or caller flag writes. Y high byte alone does not discriminate. |
| Pending event/intervening request writer | WaitForEvent prioritizes it; producer/consumer atomicity or an unmodified request tuple is not established. |
| Scene or later service does not return | Do not report completed caller writes, wrapper wait or usable control. |

Load `. ./local/private-inputs.ps1` in the same process as controlled commands;
verify source HEAD/clean status. Reuse the exact
[retained comparison recipe](./battle06-to-battle07-content-route.md#reproduction-and-review-boundary):
map-init equality, `_verify_map_script_engine_fixture` and `_cutscene_facts`,
without regeneration. The retained map-init selections are routeJoins79/80,
pointerTableJoins80/81 and primarySourceBodies50 for ms_map34_InitFunction.
Those joins bind the two pointers to the one rts body; they do not prove arrival.

For direct reproduction, check the named source operands and immediate consumers:

- Read the complete two-row Map33 and three-row Map34 warp tables, macros,
  candidate/layout caller, matcher, no-scroll request consumer, MainLoop,
  SwitchMap and full battle-coordinate table. Distinguish byte replacement,
  full-map loading and retained-population entry.
- Join Map34's complete selector and both six-pointer tables to all21 exact
  resident records and the adjacent alternate terminator. Resolve constants,
  logical lookup, special47 versus ordinary allocation, ENTNUM/layer and all
  sixteen effect rows; read both full initial action bodies and follower/raft
  premises. No generic renderer or scene-body proof substitutes for this join.
- Read each complete local surface in the table above and its shared wrapper.
  In `tests/fixtures/h2/map-events/routing-setup.json`, select
  `expected.categories.zoneEvents.tables` by symbol ms_map34_ZoneEvents and
  inspect both exact records and their targets. In `zone-programs.json`, select
  canonicalSymbol Map34_ZoneEvent0 and Map34_DefaultZoneEvent; compare every
  sourceLine/opcode/operand/address with the callback table above.
- Trace current-position candidate classification separately from
  destination-derived PARAM_1/PARAM_3, low-byte Y13 matching, physical0 init,
  F784 polarity, script macro/trap/alias, conditional flag order and wrapper
  coordinate wait. Stop at the selected program call, not its contents.

Review the whole document and exact owned diff, local links, provenance,
private/generated boundary and `git diff --check`. On the clean committed head,
run configured `uv run sf2 verify plan --base origin/main --head HEAD`, and
record actual exact-head CI, Git identities and inspection diagnostics on #945.
No blanket local normal/full/engine/Godot/H3/H4 suite is selected solely for
prose. No new executable verification machinery is required.

Preserve [#941 handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)
and [PR944 acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802),
including the corrected1892 `{N}{W1}` text control, followEntity high-byte/Bowie-HP
reasoning, all inspection diagnostics and linked predecessor results.
Preserve [#937 original handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252),
[PR939 review](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395),
[corrected handoff](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)
and [acceptance](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382).
Completed failures and retained inputs/results are not restarted or overwritten.

**Unknown:** natural route and active layout/collision, supplied flags, actual
population/HP/class/lookup/mode/raft state, request timing and intervening writes,
service/action completion, rendered sprites/effects/camera/text/audio, usable
control and persistence. **Inferred:** the room and preparation serve the
anticipated Evil Spirit consultation; that narrative does not add an admission
flag. These are conditional limits under
[ADR0014](../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md)
and [ADR0016](../decisions/0016-remake-start-evidence-deferral.md), not an automatic
runtime queue. The bounded static entry/setup/call admission is the deliverable.
