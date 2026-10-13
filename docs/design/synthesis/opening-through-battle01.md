# Opening through Battle 01: Locations, People and Return

- Scope: an English content guide to the selected opening, castle/tower route, Battle 01 and usable
  field return, under [Issue #690](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/690).
- Evidence baseline: accepted repository `56eef43667f84fd8afb21c5bc08a5c7425bd74d3`; original source
  `ShiningForceCentral/SF2DISASM` at `c834c652b6862bc5679fd7f69a38a7093206efc6`, USA ROM SHA-256
  `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`.
- Audience: readers following this story-content segment and authors locating its accepted evidence.
  Full dialogue, layouts, rosters, assets and runtime records remain with their private/local owners.

The opening connects a house and school area with a messenger exchange, a castle visit, a return to
an adviser, a tower guard and the first selected encounter. The encounter ends with an after-battle
program and a return to ordinary field control. **Inferred content explanation:** these stages make
one useful reading segment because the selected programs leave conditions used by the next stage.
The sequence below is grounded in the accepted route; motives, a complete plot synopsis and all
possible playthroughs remain **Unknown**.

Read the [Gameplay Overview](gameplay-overview.md) for the systems involved and
[Story Progression](story-progression.md) for the general state-routing rules. The
[continuous scenario contract](../contracts/map3-battle01-continuous-scenario.md) owns the selected
route and comparison boundaries; [readiness](map3-battle01-readiness.md#accepted-current-milestone)
owns the accepted private remake milestone. This guide organizes their content without creating
another campaign model or changing their acceptance.

## How to Read the Evidence

A source map number is an identity, not a chapter number. The guide follows the selected content
flow, including revisits to Maps 19 and 20. Names such as `MAP_ANCIENT_TOWER_ENTRANCE` and
`INSIDE_ANCIENT_TOWER` remain source labels; their different wording is not a reason to rename a
location or infer an unseen transition.

| Evidence kind | What it establishes here | Limit |
| --- | --- | --- |
| **Confirmed static** | Pinned source/H1/ROM selection, program structure, text references, flag predicates, entity identities and legal route topology. | Structure does not establish natural execution, dialogue delivery or visible completion. |
| **Confirmed controlled runtime** | R1 admission; early handler/route seams; R2d's explicit Map 21 bridge and seeded battle-ready result. | Harness returns, state normalization and bridge state remain controls. |
| **Confirmed bounded original continuity** | Accepted acquisition from controlled R1 through the selected story route, natural encounter, winning actions and stable return; a separate ordinary-input extension. | Savestate-linked original continuity, not uninterrupted wall time or natural title/New/load input. |
| **Confirmed accepted remake composition** | The current private default-keyboard-A milestone, including actual reached content consumers, completion and input readiness. | Modern clock/explicit deviations apply; no hardware equality, full-game claim or distribution right. |

| Map identity | Supported source label or role | Selected content phase and evidence |
| --- | --- | --- |
| 3 / `map03` | House/school opening role; this guide assigns no canonical town name. | Controlled default admission, then the observed opening and messenger route. |
| 19 / `map19` | `MAP_GRANSEAL_CASTLE_2F` is the retained royal-return warp target label. | Static castle entry, royal/tower links and adviser phase; selected execution belongs to the later original lineage. |
| 20 / `map20` | Royal-entry and tower-transit roles in the selected graph. | Conditional royal init, return to Map 19 and a later tower stair connection; no canonical room name is inferred. |
| 21 / `map21` | Tower-guard interaction role. | Static guard predicates/unlock program and later selected original execution. |
| 40 / `map40` | Passage role between the selected exits. | Static empty selected entity list, constrained warp terminals and later traversal. |
| 57 / `map57` | `MAP_ANCIENT_TOWER_ENTRANCE`; battle label `INSIDE_ANCIENT_TOWER` is separate. | Static encounter definition, observed battle/after-program and field-return endpoint. |
“Mandatory” below means required by the selected accepted route or observed prefix, with its scope
stated. It does not mean every original playthrough must use that interaction. “Additional” content
means a static interaction belonging to the selected location; its original optionality and reachable
outcome stay **Unknown** unless an owner proves them. A conditional branch is not necessarily a
player choice, and a portrait/sprite lookup is not sufficient evidence of who speaks.

## 1. Map 3: Leaving the House and Meeting the School Entities

**Confirmed controlled admission:** R1 reaches the default `ms_map3` setup and its first original
`WaitForEvent`, with the MainLoop handoff at Map 3 `(56,3)`. Only ally 0 is joined and active in that
controlled snapshot. The emitted raw player coordinates are a separate projection from those handoff
arguments; time is explicitly normalized, status is omitted and four stored item bytes are not four
complete item slots. Use [admitted-start](../../research/map3-admitted-start.md#admitted-snapshot-and-setup-join)
for exact state rather than filling gaps from the later party or remake.

The default setup separates entity, entity-event, zone-event, description, item-event and init
owners. Its alternative flags are scanned in source order `609 → 506 → 543`; the last set variant
wins. R1 observes the guarded flags clear and the default init returning without a requested program.
Those alternatives are **Confirmed static**, not additional opening phases proven by R1.

**Confirmed bounded opening route:** ordinary original input leaves the house, opens the house and
school doors, interacts with Sarah, uses the two school stair warps, reaches the two Zone 7 phases,
interacts with entity 142 and reaches the messenger program entry. The internal Map 3 warps connect
`(54,3) → (3,3)` and `(46,7) ↔ (59,12)`; these remain transitions within Map 3. The
[natural-route owner](../../research/map3-battle01-natural-route.md#reached-opening-route) stops before
`cs_5149A`'s body. Later continuity comes from the acquisition owner, not an expanded R2 fixture.

The important dialogue and entity connections are:

| Trigger and source identity | Dialogue/program connection | State or continuation; evidence boundary |
| --- | --- | --- |
| House-exit route, entity operand 128 in `cs_5145C` | Cursor 510 produces 510/511, then cursor 483 produces 483. | Movement/init waits surround the text. [Opening dialogue evidence](../../research/map3-messenger-acceptance.md#opening-dialogue-return-and-subsequent-actions) owns the three-command structure and reached returns; IDs alone do not prove unshimmed acknowledgement. |
| `ALLY_SARAH` / actor 1, `Map3_EntityEvent0` | Selected clear-F603/F602/F256 branch uses 512, 480, 481 and `cs_513D6`. | F256 follows the movement program. Other flag branches use different text/programs; the [optional-interactions fixture](../../../tests/fixtures/h2/map3-optional-interactions-static-v1.json) preserves them. |
| `Map3_ZoneEvent7`, before and after F602 | First introduction uses 513; the later F602-set/F603-clear/F260-clear phase uses 514–516 and `cs_5148C`. | Later positioning precedes F260. [Source zone binding](../contracts/dialogue-system.md#source-zone-caller-through-return) separates caller, text, program and wrapper return. |
| Entity alias 142, `Map3_EntityEvent15` | Text 500/501, with F261 branch and F261/F602 writes. | The selected opening alias resolves to physical slot 17; [entity 142 evidence](../../research/map3-entity142-interactable-reference.md) owns that live identity, not a permanent slot across reloads. Stair reload also reaches `cs_513A0`. |

Sarah's named ally identity is distinct from a numeric NPC alias. “Astral introduction” is the
accepted owner's description of the Zone 7 content; it does not identify every entity 142 use or
speaker solely from its sprite. The
[dialogue contract](../contracts/dialogue-system.md#confirmed-static-contract) preserves cursor,
packed modifier/entity words, portrait helper and entity dialogue-property lookup separately.

### Additional Map 3 interactions

**Confirmed static:** the default surface has 19 entity definitions, 17 entity-event routes including
the default, 17 area descriptions and two item placements. Only `Map3_EntityEvent0` and
`Map3_EntityEvent15` are classified `mandatory-observed-opening` by that inventory. All other route
relevance is **Unknown**; the name “optional interactions” does not settle it.

The following examples explain what else belongs to the location without requiring it in this route.
The full structural mapping remains in the linked fixture, and exact entity/area/item placements stay
private rather than being duplicated here.

| Additional trigger | Source-confirmed content | Question preserved |
| --- | --- | --- |
| `ALLY_CHESTER`, `Map3_EntityEvent1` | F603/F602 branches reference 482, 503/504 or 507. | Which branch is naturally reachable for a separate interaction, and what the player sees. |
| Alias 128, `Map3_EntityEvent2` | F602 selects the 483/505 text alternatives. | This handler is not interchangeable with the house-exit script's reference to 483. |
| Aliases 138/139, `Map3_EntityEvent10/11` | F604-dependent references include 497 and 508/509. | Gate motion does not prove a later conversation or its optionality. |
| Alias 141, `Map3_EntityEvent13` | Direct `j_ChurchMenu → ChurchMenu` call. | Normal admission, choices, transaction and caller return in this segment are not established by the static call. |
| Other default NPC routes, shared text 494 target and default RTS | Source-owned dialogue references, target sharing and empty fallback. | Sprite appearance does not assign character names; an empty fallback is not an invented conversation. |
| Area descriptions and chest/other items | Existing description and item include owners; `Map03s7_ChestItems` and `Map03s8_OtherItems` are distinct. | Reachability, pickups and route relevance remain **Unknown** here. |

See [Map 3 optional interactions](../../research/map3-optional-interactions.md#confirmed-static-contract)
and [map exploration](../contracts/map-exploration.md#areas-events-and-items) for selection and item
rules. The selected reference route excludes optional interactions and FieldMenu; this does not
exclude them from the original game's location content or admit them into the remake milestone.

## 2. Messenger Acceptance: The Party and Castle Route Become Connected

**Confirmed bounded runtime:** Zone 8 reaches `cs_5149A` with F602 set and F603 clear. Its accepted
prompt path selects the F89-set branch `cs_51614`. Seventeen reached map-script text commands use
517–531 and 535–536; the join handler also emits text 447. The raw speaker operands include 142,
143, named allies and Sarah's packed `0xC001` form. The latter combines modifier/entity information;
it must not be displayed as a separate character ID.

**Inferred content summary:** the exchange coordinates the school group and messenger, then the
accepted response leads to party/follower changes. This describes observed command consequences,
not a reconstruction of the characters' motives or the private script prose.

The observed ordering is F600, F66, `join 128`, the Sarah (1) and Chester (2) joins and party updates,
then Sarah→Bowie and Chester→Sarah follower links at distance 2. The `join 128` selector is a
source command operand, not a fourth persistent party member. Only after the whole program returns
does `Map3_ZoneEvent8` commit F603. The follower-ready endpoint is Map 3 `(43,10)`, Down, with the
tracked script/text/prompt/close consumers returned. The
[messenger owner](../../research/map3-messenger-acceptance.md#confirmed-source-and-runtime-facts)
and [R2a fixture](../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json) own these facts.

**Confirmed static alternative; runtime Unknown:** the decline/re-prompt branch references 532–533.
It is not an observed alternate playthrough and must not be spliced into the accepted Yes lineage.
The [party contract](../contracts/party-roster-state.md) keeps membership, active party and followers
separate; neither this route nor its joins prove general capacity or durable save behavior.

## 3. Castle Visit: Gate, Royal Area and Return to the Adviser

The next connection returns from the school to the castle gate. **Confirmed static and separately
observed on the accepted original route:** `Map3_ZoneEvent4` invokes `cs_51652`; its six text references
537–542 lie between guard-action groups for aliases 138/139. The caller sets F604 **after** the script
returns. The gate program itself does not own that flag write, and only entity 139's action group is
awaited. This separates dialogue, motion completion and the condition used after the gate.

The selected Map 3 north exit uses the wildcard-X/y=1 warp, with the selected terminal `(28,1)`
leading to Map 19 `(26,30)`, Up. A wildcard predicate is not proof that every tile on that row can
trigger it: geometry, area, collision and event markers still apply. The accepted original lineage
also observes subsequent movement to `(26,29)`; the earlier Map 19 movement-acceptance seam alone
stopped before that displacement.

**Confirmed static content:** Map 19's selected default population contains 13 records and several
fixed/walking/looping occupancy domains. Alias140 is phase-dependent: the initial init hides it,
the palace-return phase exposes it and the accepted adviser program removes it from the selected
route. Map 19 init guards inspect F605 and F608 around `cs_53104`; do not infer presence from the
entity table without the selected init/program state.

The royal visit crosses Map 19 `(23,3) → Map 20 (23,37)`, Down. The selected royal entry's init
runs `cs_53996`: cursor 2176 belongs to this program; source effects put Bowie at `(23,39)`, move
alias 131 and hide alias 130, then its caller commits F605. These are the program's structural content
and the selected original royal-return stage, not proof that every entry to Map 20 runs that scene.
The [castle static owner](../../research/map3-castle-battle-unlock.md#confirmed-static-derivation)
guards the entry selector, programs and post-program occupancy.

Return uses Map 20 `(23,37) → Map 19 (23,3)`. **Confirmed static:** the destination-facing field is
LEFT, sourced from `warpFacing LEFT`. It is a warp annotation, not an unconditional claim about
orientation after destination init or later motion. The same Maps 19/20 are revisited for the tower
route; map identity alone does not distinguish these content phases.

At the adviser interaction, Map 19 alias 140 / `Map19_EntityEvent12` at the selected player position
`(16,6)` routes clear F607 to `cs_52F0C`, cursor 575 and Yes/No. F89 set selects `cs_52F40`, cursor 578;
its source effects move alias 140 away and set F608. The enclosing entity handler sets F607 after
its script returns. **Confirmed static conditional:** F607 already set routes to `cs_52F24` instead;
F607 and F608 therefore express different effects and must not be collapsed into “accepted quest.”
The accepted original route reaches the royal/guard stages and F607/F608; unobserved prompt branches
and repeat interactions retain **Unknown** runtime consequences.

Other castle conversations belong to their local event tables. For example, Map 19 event 0 references
543/544 under F256, and Map 20 event 2 references 570/571 under F256. These are **Confirmed static**
alternatives in the [entity-program corpus](../../../tests/fixtures/h2/map-events/entity-programs.json),
not extra mandatory steps. Entity IDs and speakers are local to the selected setup; Map 19 alias 140
must not be equated with Map 3 alias 140. The broader
[map-event dialogue analysis](../../research/map-event-dialogue-state.md#runtime-boundary) proves
state/reference structure rather than natural displayed line order or character identity.

## 4. Tower Guard and Passage: Unlocking the Encounter

The tower approach selects a different part of the same maps: Map 19 `(6,2) → Map 20 (6,37)`,
Right, then Map 20 `(3,36) → Map 21 (3,16)`, Right. These selected stair/warp connections come from
R2's predicates and R2b's legal graph; the later original acquisition proves the selected continuous
castle/tower execution. The graph's shortest path is a reproducible legal route, not a rule that
players must take exactly its 110 logical inputs.

**Confirmed static conditional:** the default Map 21 entity table points alias 128 to
`Map21_EntityEvent0`, physically defined in `s2_entityevents_506.asm`. The filename does not require
selection of flag 506. Its actual F608/F256 conditions choose:

| Entry F608 / F256 | Content and continuation, assuming called services return |
| --- | --- |
| clear / clear | text 568, set F256, text 569; no unlock program. |
| clear / set | text 569; no unlock program or flag write. |
| set / set | text 579; no unlock program or flag write. |
| set / clear | text 579, `cs_53EF4`, then F256 after the whole script returns. |

In the last branch, `cs_53EF4` first awaits alias 128 moving right, then applies
`setFacing 135,DOWN`, then `setStoryFlag 1` sets F401. The guard moves from `(5,16)` to `(6,16)`;
the selected player terminal is `(5,15)`, Down. **Confirmed conditional static:** missing logical
alias 135 does not itself reject the original command: under the default initialized-zero lookup
condition it resolves physical slot 0. That does not authorize skipping the facing command, its return
or the preceding action wait. The
[lookup/continuation owner](../../research/map3-castle-battle-unlock.md#flag-predicates-and-return-boundaries)
owns the four branches and this limit. F256 is reused in local handlers; it is not one universal
campaign-stage name.

From Map 21 the selected exit `(9,1) → Map 40 (4,30)`, Up, reaches a passage setup whose selected
entity list is empty. Map 40's wildcard-X/y=12 warp leads to Map 57 `(8,18)`, Up. The
[admission owner](../../research/map3-battle01-admission.md#confirmed-static-findings) identifies only
two reachable warp-event terminals, `(14,12)` and `(15,12)`, and selects the former; ordinary walkable
cells elsewhere on that row are not warp terminals. The 46-input extension is a selected legal route,
not a proof of every valid route or an observed dialogue scene in Map 40.

## 5. Map 57: Battle 01 Admission and Its Actors

**Confirmed static encounter identity:** source battle index **1** is the selected “Battle 01” in
this guide. It is row 1, the second record of the zero-based battle-map table; this is not a claim
that an original UI displays “01.” Its map is 57, source `MAP_ANCIENT_TOWER_ENTRANCE`, while the
battle label is `INSIDE_ANCIENT_TOWER`. The selected area is `(0,0,16,20)`, with wildcard trigger
coordinates. `CheckBattle` requires F401 set and F501 clear; the completion flag uses the +100
relation. Battle identity, map identity and source name remain distinct.

For the new-battle clear-F88 branch, before program `bbcs_01` precedes loading and battle-start
routing. Its cursor 2292, text commands with entity operand 135 and the later `ce_49694` scene
entity load belong to the before program; script `loadMapFadeIn` is not another player warp. Start row 1 targets `ms_Empty`, with F451 checked/set before that empty
program. F90–F105 are cleared for the encounter. `LoadBattle` establishes Map 57, positions and
loads battle entities/sprites, loads `BattleTerrain01`, selects music and completes its load/fade
sequence. Detailed program/caller order remains in the
[admission fixture](../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json).

**Confirmed static and bounded initialized participation:** `BattleSpriteset01` supplies three ally
slots, six enemies, three AI regions and no AI points. The enemies reference `GIZMO`; all nine
entries are `STARTING`. The selected runtime actors are allies 0/1/2 (Bowie/Sarah/Chester) and enemies
128–133. These enemy combatant IDs are not the Map 3 NPC aliases with the same numbers. Initial
placement, terrain, activation polygons and enemy definitions are linked from
[Battle 01 placement](../../research/battle01-placement.md#confirmed-placement-facts),
[battle spritesets](../../research/battle-spriteset-data.md) and
[encounter definition](../contracts/battle-encounter-definition.md), rather than copied into a roster.

Activation, region-cutscene and spawning work precede turn generation. There is no Battle 01 region
cutscene row. Placed/living filters, randomized agility and descending sort determine order; presence
in a starting spriteset is distinct from activation and later ability to act.

**Confirmed controlled R2d seam:** an explicit non-natural bridge seeds the Map 21 terminal and
RNG `0x1234`, traverses the original extension, and stops at actor **1** before the battle input read.
**Confirmed selected natural acquisition:** the accepted savestate-linked route instead reaches
first actor **2**. Neither result makes a fixed actor or seed a legality rule. R2d's fixture never
proves the omitted castle segment or the selected natural order. The
[original frontier](map3-battle01-readiness.md#accepted-original-frontier) locates the later result.

## 6. Winning Battle and the After-Program

**Confirmed bounded original continuity:** the selected winning lineage uses movement, STAY,
physical attacks, HEAL 1 and Medical Herb, with reached player/AI actions, scenes, consumed resource
and HP effects, deaths, rewards and turn handoffs. It reaches victory at round 14/turn 103; these are
reference observations, not encounter rules. Losing attempts and their completed failures remain in
the acquisition owner and are not inserted into this successful input stream. Read
[Tactical Battle Loop](tactical-battle-loop.md) for the general rule connections.

**Confirmed static outcome rule:** Battle 01 uses the ordinary placed/living enemy-count victory
condition. Its leader-present table entry is not a separate leader-kill victory rule. At the winning
return, `BattleLoop_Victory` routes party recovery and `ExecuteAfterBattleCutscene` to
`abcs_battle01`, then clears F401 before setting F501 and returns D4=1. The after-battle join row is
zero; the earlier messenger joins must not be attributed to this row.

The after-program is content that must complete before field return. Its cursor 2305, battle-stat
reset, Map 57 scene replacement, `ce_497F6` entity load, entity 129 text commands, sprite changes,
white-palette effects, movement and removals form the source-owned sequence. Its temporary
entities 128/129 are scene identities, not surviving enemy combatants inferred from their numbers.
**Inferred content summary:** the scene changes the post-encounter situation, then removes its
scene subjects before exploration resumes. Who speaks and the fiction's exact explanation cannot
be supplied from those numeric operands or `MAPSPRITE_ASTRAL`/`MAPSPRITE_MIST_DEMON` alone.

The [victory-return static fixture](../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json)
contains **80 source operations**, including embedded action/data records. The accepted original
final segment records **67 reached operation entry/return pairs**, then shared-tail/enclosing return,
flag updates, BattleLoop return, SwitchMap and ExplorationLoop. These are different denominators,
not seventeen missing runtime commands. The
[final acquisition](../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness)
owns the observation; the [static victory owner](../../research/map3-battle01-victory-return.md#static-boundary)
stops at the unentered ExplorationLoop call and cannot independently establish usable return.

## 7. Returning to the Field and Stopping the Segment

**Confirmed bounded original endpoint:** Map 57 selects `ms_Void`; player `(5,12)`, Down, remains
settled over two neutral completed frames, with battle sentinel 255, F401 clear, F501 set and roster
`[0,1,2]`. There is no blocking battle/script/modal/transfer/consumer; the generic window byte 1 alone
is not a blocking dialogue. The [exact endpoint](../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint)
owns gold, levels, complete slots, status, RNG/time and the rest of this particular winning history.

The accepted PR #504 terminal is nonresumable and contains neutral polls. A separate accepted
PR #526 compatible acquisition observes one ordinary Down input and settled displacement to
`(5,13)`. That closes its named input/effect boundary without changing the first terminal or proving
what comes next in the campaign. Battle 02 admission is outside this segment; separately scoped
[#691](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/691) investigates that relation,
and its unmerged conclusions are not dependencies here.

**Confirmed accepted remake boundary:** the current composition accepts the continuous private
Map 3 → Battle 01 victory → usable 5B return, with reached actual content use, completion,
acknowledgement and input readiness under default keyboard A and the modern deterministic clock.
C is supplemental; B/D are excluded. Original actor/order, 103-turn history, gold 420 and raw frame
values are original observations rather than unconditional modern outcomes. Preserve matched-state
rules and mandatory content/causal effects through the
[modern applicability contract](../contracts/map3-battle01-continuous-scenario.md#modern-clock-applicability).

Historical A10/default/matrix reports remain Unavailable with `milestonePass=false`, as do scoped
milestone fields. No new full executable report or corrected whole-A trajectory follows from this
guide. The [composition owner](../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review)
retains historical seed, HEAL, provenance and review failures. Terminal internal completion remains
**Inferred**, its delay **Unknown**. These limits do not reopen the accepted milestone or create an
automatic emulator queue. Usable return also does not establish user save/load, full-game fidelity,
hardware output equality or public-content rights.

## Source-to-Content Matrix and Evidence Review

The pre-synthesis review inspected these accepted payloads and the code that limits their claims.
Commands below identify existing reproduction owners; they were not rerun to refresh this prose.
The work adds no fixture, extraction, capture or new observation.

| Content connection | Exact accepted payload / fixture ID and verifier | Review disposition and remaining boundary |
| --- | --- | --- |
| Map 3 admission/default setup | [R1](../../../tests/fixtures/h3/map3-admitted-start-v1.json), `sf2-map3-admitted-start-runtime-v1`: `static.map3`, `harness`, `expectedObservation`; [verifier](../../../src/sf2tool/h3/map3_admitted_start.py), `sf2 h3 map3-admitted-start` | Controlled menu/service returns and time normalization remain explicit; status/full slots come from later accepted readback, not invented R1 fields. |
| Ordered setups, areas, warps and content | [map data](../../../tests/fixtures/h2/map-data-static-v1.json), `sf2-map-data-static-v1`; [map content](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [map-data verifier](../../../src/sf2tool/h2/map_data.py), [content verifier](../../../src/sf2tool/h2/map_content.py), `sf2 h2 map-data` / `sf2 h2 map-content` | Last-set setup versus first-match event/warp rules remain separate. Exact private content tables are not published here. [Inventory](../../research/map-data-inventory.md#setup-selection-and-rom-parity) and [content](../../research/map-content.md#source-form-sections) own them. |
| Opening movement and program entries | [R2](../../../tests/fixtures/h3/map3-battle01-natural-route-v1.json), `sf2-map3-battle01-natural-route-runtime-v1`: `static.route.warps`, `static.route.runtimeOpening`, `expectedObservation`; [verifier](../../../src/sf2tool/h3/map3_battle01_natural_route.py), `sf2 h3 map3-battle01-natural-route` | Its named “Battle 01” rail reaches only messenger entry; no body or castle observation is manufactured from that name. |
| Additional default Map 3 content | [optional fixture](../../../tests/fixtures/h2/map3-optional-interactions-static-v1.json), `sf2-map3-optional-interactions-static-v1`: `pointerSetup`, `entityEventRoutes`, aggregates; [verifier](../../../src/sf2tool/h2/map3_optional_interactions.py), `sf2 h2 map3-optional-interactions` | Exact schema plus fresh-parse equality guard structure/order; 15 routes, all descriptions/items retain Unknown route relevance. |
| Messenger choice, joins and followers | [R2a](../../../tests/fixtures/h3/map3-messenger-acceptance-v1.json), `sf2-map3-messenger-acceptance-runtime-v1`: `static.stream`, `static.text`, `expectedObservation`; [verifier](../../../src/sf2tool/h3/map3_messenger_acceptance.py), `sf2 h3 map3-messenger-acceptance` | Speaker operands remain packed/source-compatible; accepted Yes and whole return are distinct from static decline and rendered/acknowledged text. |
| Castle/royal/guard topology | [R2b](../../../tests/fixtures/h2/map3-castle-battle-unlock-static-v1.json), `sf2-map3-castle-battle-unlock-static-v1`: `static.programs`, `static.occupancy`, `static.routeGraph`, `static.warps`; [verifier](../../../src/sf2tool/h2/map3_castle_battle_unlock.py), `sf2 h2 map3-castle-battle-unlock` | Its program `operations` arrays are guarded structural selections, not full transcripts. F604/F605/F607/F256 caller writes and F608/F401 program effects remain distinct. Natural reach uses the later acquisition. |
| Castle branch and dialogue identities | [entity programs](../../../tests/fixtures/h2/map-events/entity-programs.json), section of `sf2-map-events-static-v1`: `Map19_EntityEvent12`, `Map21_EntityEvent0` and additional local targets; [dialogue-state fixture](../../../tests/fixtures/h2/map-event-dialogue-state-static-v1.json), `sf2-map-event-dialogue-state-static-v1`; [dialogue-state verifier](../../../src/sf2tool/h2/map_event_dialogue_state.py), `sf2 h2 map-event-dialogue-state` | Exact target/branch/operand order is static evidence. Reaching portrait/SFX definitions do not prove natural lookup input/result or a visible speaker; the generic dialogue/roster matrices do not supply those joins. |
| Encounter and first-control seam | [admission](../../../tests/fixtures/h2/map3-battle01-admission-static-v1.json), `sf2-map3-battle01-admission-static-v1`: `static.admission`, `static.cutscenes`, `static.loadAndTurnOrder`; [R2d](../../../tests/fixtures/h3/map3-battle01-player-ready-v1.json), `sf2-map3-battle01-player-ready-runtime-v1`: `static.bridge`, `expectedObservation`; [H2](../../../src/sf2tool/h2/map3_battle01_admission.py) / [H3](../../../src/sf2tool/h3/map3_battle01_player_ready.py), `sf2 h2 map3-battle01-admission` / `sf2 h3 map3-battle01-player-ready` | Source/ROM and exact fixture/observation comparisons do not erase the declared bridge. Seeded actor 1, debug-test order and natural actor 2 retain separate provenance. |
| Victory program and return edge | [R4a](../../../tests/fixtures/h2/map3-battle01-victory-return-static-v1.json), `sf2-map3-battle01-victory-return-static-v1`: `victoryReturnSpine`; [verifier](../../../src/sf2tool/h2/map3_battle01_victory_return.py), `sf2 h2 map3-battle01-victory-return` | Verifier closes retained-owner identities and exact source/H1/ROM structure; natural execution and stable/input-ready return use later original results. |
| Original continuity and usable input | [final lineage](../../research/map3-messenger-acceptance.md#native-victory-and-stable-field-readiness), acquisition source `9c3ea03ac5f5b467ee744f1ac624870da2408443`; [Down extension](../../research/map3-messenger-acceptance.md#native-post-victory-ordinary-input-result-issue-515); [continuous contract](../contracts/map3-battle01-continuous-scenario.md#exact-observed-endpoint) | Existing private records/18 verified parent pairs and owning read-only audit recipes establish bounded results. Early owner queues are retained historical boundaries, not new missing-milestone claims. |
| Remake delivery | [accepted composition](../../../remake/docs/evidence/retained-comparisons.md#accepted-composition-review) and [current readiness](map3-battle01-readiness.md#accepted-current-milestone) | Consume the accepted composition without relabeling historical reports, restarting comparisons or using remake results as original evidence. |

One concrete prose discrepancy remains visible for owner follow-up: the
[admission research](../../research/map3-battle01-admission.md#confirmed-static-findings) writes
`BattleSpriteset01` as hex `0x1B3282` beside decimal `1782498`, while its fixture, verifier and the
[spriteset fixture](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json) use decimal
`1782498` (`0x1B32E2`). The placement owner agrees with the latter. This guide does not publish an
address conclusion from the conflicting prose or edit Layer A; main-gate receives the exact mismatch
for the research owner. The counts/identity used above agree across those executable owners.

The other adversarial checks found boundary differences rather than competing results: R1 handoff
versus raw coordinates and incomplete slots; source graph versus later original continuity;
R2d seeded versus natural first actor; script return versus caller flag write; text ID versus
unshimmed delivery; 80 static operations versus 67 reached pairs; and independent composition versus
retained false milestone fields. Each uses the stricter named boundary above. Wider plot meaning,
unobserved alternatives, speaker/presentation joins, persistence and the next encounter remain
**Unknown**, with no automatic research or implementation dispatch.
