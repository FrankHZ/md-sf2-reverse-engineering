# East Shrine Return through Battle11 Cave Admission

The cave encounter can follow Battle10's direct overworld return or an optional East Shrine visit.
The selected connections lead west from Map70, north from Map69 and into the Dark Caves from Map72.
If the cave request admits Battle11 and its intro has not played, Peter describes the passage,
reacts to a voice and meets the hobgoblin who welcomes and threatens the visitors. Battle preparation
then replaces this scene's cast with the actual combatants.

**Confirmed static content and control flow:** the accepted
[research owner](../../research/east-shrine-return-through-battle11-admission.md) supports the
connections, admission and opening below. **Inferred:** these form a candidate content sequence from
the Battle10/optional shrine frontier toward the cave encounter. **Unknown:** natural traversal,
supplied actor/story/party state, completed services and visible darkness. This is neither a mandatory
shrine route nor an observed playable entry or remake readiness claim.

Begin with [Battle10 outcomes and the optional shrine visit](battle10-outcomes-through-east-shrine-visit.md).
Stop at the new-battle start-dispatcher return immediately before `ActivateEnemies`. Combat, outcomes,
crossing the cave and later destinations are outside this guide. Fnnn means flag nnn; map and battle
IDs retain source numbering.

## Returning to the Overworld and Selecting the Cave Connection

**Confirmed conditional connections:** skipping the shrine leaves Battle10's immediate victory
return on Map70 at a battle-derived position and facing. Visiting the shrine supplies a different
starting point: its south exit, wildcard X and Y19, requests Map70 `(43,41)`, Down. That exit tests
neither the grandfather's invitation nor F77/F728/F729. An intact request and completed consumers
remain necessary in either case; a source connection alone does not establish legal walking.

The selected continuation is:

| Connection | Consequence |
| --- | --- |
| Map70 west edge, X0 / wildcard Y | Scroll Left to Map69, set main X to 62 and retain Y. This branch does not assign the row's Left facing to main. |
| Map69 fixed northern point `(41,15)` | Ordinary request to Map72 `(40,61)`, Up. |
| Map72 cave point `(58,49)` | Ordinary request to Map48 `(15,26)`, Up. |

The west scroll continues the existing exploration loop. It carries the population and temporary
state instead of invoking ordinary entity/map initialization, healing or `CheckBattle`. The fixed
northern connection does return through main's map/battle selector. Map69's separate north scroll
has a different destination and bypasses that ordinary-entry boundary; it cannot substitute for the
selected fixed point. Map70's north scroll goes to a different overworld map altogether.

The shrine exit also passes through battle selection, unlike Battle10's immediate victory return.
Supplied F410 can select Battle10 there, or F414 can select Battle14 when F410 is clear. A matched
completion flag clears the unlock but still selects the battle on that same call. Ordinary Map70
entry occurs only if no battle qualifies. It rebuilds entities/followers, clears pending and temporary
flags 256–383, heals living/immortal allies and runs its empty fixed list/return-only init. These
resets do not clear persistent story or party flags, and restored input still depends on services
returning with pending zero.

There are other possible interruptions along this selected connection. Map70 and Map69 retain
their specific/default zone requests for encounters; a specific match does not fall through merely
because its helper declines. The fixed Map69→72 request can select the first supplied unlock among
Battle8, Battle24 and Battle26 before ordinary Map72 entry, regardless of its battle-area output.
If ordinary entry occurs, Map72's F716-set/F10-clear init invokes a later scene rather than returning
immediately. Its zones can also request encounters or later content. The research's
[interruption review](../../research/east-shrine-return-through-battle11-admission.md#interruptions-before-the-cave-request)
owns their complete predicates and request lineage. No supplied tuple or completed shrine dialogue
proves an interruption-free route. Warp and zone production also require their respective layout
attributes; the mutable request may be changed before consumption.

## Cave Arrival and Battle Admission Are Separate

**Confirmed admission:** F411 originates in the accepted
[New Granseal castle initialization](new-granseal-castle-year-later-control.md), after its first
address/dismissal script returns. With F707 clear, the caller writes the egress map, runs that script,
then sets F707 and F411. An F707-set/F411-clear supplied state is not repaired by repeat init.
Shrine completion, May recruitment and Peter's membership/following are independent of this producer.
Neither the selected cave warp nor Battle11 admission adds those predicates.

At Map48, the battle selector tests the exact trigger coordinate as well as the unlock. Battle11's
`(0,0,27,32)` area is output for the selected battle, not an admission rectangle:

| Intact request / supplied flags | Selection on this call |
| --- | --- |
| `(15,26)`, F411 set / F511 clear | Battle11. |
| `(15,26)`, F411 and F511 both set | Clear F411 and still select Battle11. |
| `(15,26)`, F411 clear | No battle, even if Battle25's F425 is set. |
| `(55,29)`, F425 set | The separate Battle25 row can qualify; Battle11's trigger fails. |
| Other points, including `(52,12)` and `(3,11)` | Neither cave row qualifies merely because the point lies inside its area. |

The request `(15,26)` differs from both the opening's main position `(15,24)` and view origin `(8,19)`.
F511 completion and F461 intro state are separate; neither establishes the other in arbitrary
supplied states. Battle25 content and alternative cave routes remain outside this guide.

**Confirmed no-battle alternative:** Map48 has no normal map setup. The missing-setup sentinel skips
init and fixed NPC population rather than selecting an invented empty init. Ordinary exploration
still performs shared loading/healing/temporary reset, clears and rebuilds entities with main and
applicable persistent followers. Its zone dispatcher skips the absent handler but retains the VInt
and physical-main stop waits. Pending zero after services permits control; loading the cave does
not by itself run the Battle11 opening. Collision, visibility and usable cave traversal remain
**Unknown**.

## The Opening Uses a Scene Cast, Not a Party Assertion

**Confirmed new-battle selection:** F88 clear takes new-battle initialization. With F461 clear,
the before wrapper dispatches `bbcs_11` without setting F461; the later start wrapper sets it before
dispatching the empty `ms_Empty` row. F461 set skips both scripts but still prepares a new battle.
F88 set instead restores saved battle state, clears F88/AI memory and bypasses the wrappers and new
placement. That resume branch reaches individual turns and is outside this guide's endpoint.

The before scene loads Map48 with view origin `(8,19)`, then explicitly loads `ce_4ACA0`. This
population request supplies the cast despite the cave's absent normal setup:

| Authored actor | Scene role and boundary |
| --- | --- |
| Bowie/main `(15,24)`, Up | Physical main slot 0; the script installs and waits for `eas_Init`. |
| Peter `(15,25)`, Up | Character 7 with `eas_Init`, declared from this row only if not already mapped. |
| Hobgoblin `(15,3)`, Down | Event/character 128, the speaking host. |
| Vampire bat `(16,4)`, Down; witch `(14,4)`, Down | Event/characters 129 and 130, part of the authored scene population. |

Entity loading clears the old population and rebuilds persistent followers before reading these
rows. F67 can already map Peter, so the explicit Peter row—including its position and action pointer—is
then skipped. Later movement acts on that rebuilt follower instead. Authored Peter placement is
therefore not a universal scene origin, and relative actions do not prove an absolute endpoint.

F67 does not test joined/active/HP state. With no earlier mapping, the explicit Peter row can likewise
declare an unjoined, inactive or dead actor. Because current battle is already 11, the out-of-battle
zero-HP blue-flame sprite alternative is bypassed. Seeing a Peter actor or selecting that sprite
cannot establish living Peter, F7/F39, party membership or player/AI control. Healing and actual
placement occur after the scene. There is no join, force-AI or `followEntity` command in this opening.

F77 can rebuild the grandfather under `FOLLOWER_A=30`; it does not preserve the shrine's transient
129→`FOLLOWER_B=31` relation. Other supplied follower flags can change ordering or aliases. These
identities are not fixed physical slots, and following does not make the grandfather a combatant.
May has no persistent follower row; her F8/F40 flags alone do not place her in this scene.

## Peter's Exchange with the Hobgoblin

**Confirmed authored sequence:** after the population load, the script targets Bowie with the
camera, refreshes plane B through `csub_458E`, requests `fadeInB` and a sound fade-out. The load and
fade command names describe requests and handler services, not an observed completed presentation.

Peter first moves Left one tile and Up one tile, with action waits, and describes the route through
the cave. Peter faces Right and Bowie Left for his mention of a hobgoblin. After a requested 20-frame
sleep, speaker 128 answers and the script requests `MUSIC_ENEMY_ATTACK`.

Peter moves Up once more. After a five-frame sleep, Bowie starts `eas_2xRightLeft`; after another
five, Peter starts `eas_2xUpDown`. Those two oscillation commands do not themselves wait for completion.
A 120-frame sleep follows. Bowie faces Up, Peter moves Up again with a wait and asks who spoke;
128 identifies himself. The camera then receives speed `$20` and targets 128. The hobgoblin welcomes
the visitors, moves Down one tile with a wait, delivers the threat, and the script requests another
sound fade-out before terminating.

These beats paraphrase seven authored text requests, IDs 2483–2489 (`09B3..09B9`), with Peter and
128 as speakers. They contain no scene choice, conditional jump, story write, warp or combat decision.
The four explicit sleeps request 150 frames in total, not the scene's elapsed duration. Text adds
its own scrolling/window/sleep services, and debug skipping can omit text, cursor advances and sleeps.
The full record/text composition remains with the
[opening research](../../research/east-shrine-return-through-battle11-admission.md#opening-records-guards-and-services).

Commands also have different state requirements. Peter/Bowie facing skips an ally whose **word** HP
is zero; HP256 is nonzero. Action installation/movement and camera targeting have no equivalent HP
guard, and camera targeting alone does not wait for scrolling. A missing character mapping resolves
slot 0 rather than safely skipping. Actor names and dialogue thus cannot prove successful movement,
correct mapping, speech, camera arrival or service completion. The script terminator conditionally
waits for view scrolling and clears scrolling speed; no composed runtime observation is claimed.

## Darkness Presentation and the Handoff to Battle

**Confirmed plane service:** `csub_458E` runs after the second Bowie camera request. It uses the
existing camera/layer helper to write plane-B coordinate and destination words, constructs its ring
counters and sets the plane-B update bit. Existing VInt/plane consumers use those fields for layout
and VRAM work. Its inputs depend on actual entity, camera and layer state; a fixed opening coordinate
cannot replace them. It sets no visibility radius, party/story flag or entity population and does
not wait for rendered output. The research's
[helper review](../../research/east-shrine-return-through-battle11-admission.md#plane-b-helper-and-darkness-evidence-boundary)
owns the arithmetic, descriptor and consumer chain.

**Inferred:** the refresh aligns the cave's secondary plane relative to the chosen actor for
darkness-related presentation. **Unknown:** the visible mask, lit region, occlusion, palette and
hardware timing on natural entry. Later fades, movements, camera changes and battle loading add
services and reload state; the helper writes do not prove their composed result or persistence.

**Confirmed battle handoff:** after before-return, the loop restores map/battle, clears region
flags, heals living/immortal allies, initializes ally then enemy positions, clears AI memory and
loads the battle for combatant 0. Ally placement consumes the actual active list and word HP,
skipping dead members; twelve authored ally positions do not guarantee twelve deployed allies.
Ten enemy rows supply battle placement instead of the three scene enemies.

`LoadBattle` clears the scripted entities through `PositionBattleEntities`, prepares map/sprite/
terrain resources, selects the combatant's mapped camera target, plays map music and requests
fade-in. It does not preserve the scene cast or guarantee the helper's exact plane words survive.
The start wrapper then sets F461 if clear and dispatches `ms_Empty`, whose normal termination services
still apply. This guide stops at that dispatcher's return, immediately before `ActivateEnemies`.
It establishes a source handoff, not an observed player-ready turn, activation/AI rule or combat outcome.

## Evidence Review and Remaining Limits

This guide consumes research accepted in PR #770, with the preceding English guide accepted in
PR #769. The research owns pinned SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`, USA retail
identity, source composition and reproduction. Direct bounded review found no Layer A correction
needed. Existing tracked fixtures were read in memory and validated against their owning schemas;
no optional private payload was generated/copied and no source reinvestigation or runtime run occurred.

| Reviewed relation | Evidence and discriminating boundary |
| --- | --- |
| Connections, missing setup and init gates | [sf2-map-data-static-v1](../../../tests/fixtures/h2/map-data-static-v1.json), [sf2-map-setup-static-v1](../../../tests/fixtures/h2/map-setup-static-v1.json), [sf2-map-init-static-v1](../../../tests/fixtures/h2/map-init-static-v1.json) bodies 11/76/77/78 and [map/exploration contract](../contracts/map-exploration.md). Map-data is an inventory; exact warp rows and consumer composition come from accepted research. No Map48 init-route join exists. |
| Admission and before/start selection | [sf2-common-maps-static-v1](../../../tests/fixtures/h2/common-maps-static-v1.json), [sf2-battle-global-data-static-v1](../../../tests/fixtures/h2/battle-global-data-static-v1.json), [sf2-battle-cutscenes-static-v1](../../../tests/fixtures/h2/battle-cutscenes-static-v1.json), [sf2-battle-routing-data-static-v1](../../../tests/fixtures/h2/battle-routing-data-static-v1.json) and [cutscene contract](../contracts/battle-cutscene-routing.md). Battle-area output precedes completion clearing; `routeTargetsParsed=false` prevents deriving exact row-11 targets from aggregate occupancy. |
| Scene commands, mapping and waits | [sf2-map-script-engine-static-v1](../../../tests/fixtures/h2/map-script-engine-static-v1.json), its `bbcs_11` transition/population/camera selections and placement/action/dialogue/control handler facts. It retains family sites and guards, not a complete public scene transcript. Accepted research supplies the full ordered records, population, text and follower composition. |
| Plane refresh and final population | Common-map plane-update facts and [sf2-battle-spriteset-data-static-v1](../../../tests/fixtures/h2/battle-spriteset-data-static-v1.json). `rowContentTracked=false`; exact Battle11 placement and helper/load order come from accepted research. Neither aggregate fixture validates visible darkness or natural deployment. |

The [map-init verifier](../../../src/sf2tool/h2/map_init.py) and
[schema](../../../schemas/h2-map-init-static-fixture.schema.json) preserve body/operation order.
The [script schema composition](../../../src/sf2tool/h2/map_script_schema.py) and
[verifier](../../../src/sf2tool/h2/map_script_engine.py) preserve handler, operand and family-site
boundaries. Narrow owners are the existing `uv run sf2 h2 map-data`, `map-setup`, `map-init`,
`map-script-engine`, `common-maps`, `battle-global-data`, `battle-cutscenes`, `battle-routing-data`
and `battle-spriteset-data` commands; naming them is traceability, not a claim of a fresh extraction.
The research's [reproduction boundary](../../research/east-shrine-return-through-battle11-admission.md#existing-evidence-reproduction-and-unknowns)
retains the exact selected comparison route. Broad suites are not selected solely for this prose.

**Unknown:** collision-free and uninterrupted travel; actual actors/mappings/flags/HP/party/AI state;
request survival and completed walking/text/input/camera/fade services; composed darkness and timing;
and continuous playable Battle11 entry. These limits do not authorize collection or new machinery.
This guide selects no product/engine decision and does not complete the campaign.
