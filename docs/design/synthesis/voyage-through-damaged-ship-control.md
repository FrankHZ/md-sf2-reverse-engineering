# Voyage and Landing through Damaged-ship Control

The departure scene sends the ship toward the mainland. **Inferred narrative connection:** the
voyage reaches Parmecia, a rough landing prompts the refugees to salvage timber, and Jaha's attempt
to help leads into a request for Bowie to bring a plank above deck. **Confirmed static content:**
Map73's first-arrival init contains this whole sequence, changes the current map to Map14 inside
the script, then writes F700 on return. **Unknown:** natural arrival, completed services, actual
actor state and whether the following event read permits player control.

Read this after [Earthquake-town exit through docks departure](earthquake-town-through-docks-departure.md).
The accepted [research owner](../../research/voyage-through-damaged-ship-control.md) supplies the
pinned-source, dialogue, inline-action and caller review. This guide supports content/design planning
and stops at the first post-init control/event read on Map14. Continue with
[Damaged-ship plank choices through Battle08 admission](damaged-ship-plank-through-battle08.md)
for the next conditional segment; town-building remains separate. It adds no remake capability. The
[roadmap](../documentation-roadmap.md#current-baseline-and-near-term-synthesis) owns the Standard-rigor
agreement for [#731](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/731).

`Fnnn` means flag nnn. Source references use `disasm/` at
[`ShiningForceCentral/SF2DISASM` commit `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm).
Research and fixtures bind the USA retail ROM in the [root baseline](../../../README.md#pinned-original-baseline).
Command indices below are zero-based script records, not frames or measured durations. Dialogue is
summarized from accepted research; its wording does not create a gameplay predicate.

## 1. Arrival Selects the First Scene before Offering Control

**Confirmed conditional connection:** if the predecessor's request survives and its no-scroll
consumer runs, it hands the main loop Map73 `(26,13)`, Right, after F74/F640 were cleared. Map73
has no substitution row and no encounter row naming 73 in the first-column map field. Battle08's
row names Map72. This request therefore selects exploration rather than Battle08, regardless of
encounter unlock flags; it does not prove a naturally completed voyage.

Explicit entry clears the pending event, heals living/immortal allies and constructs the selected
population before loading the map and calling setup init. It also clears setup temporary flags
256–383 and sets F80. Map73 has one default setup, an empty fixed-entity list and terminator-only
flag/step-copy tables. Empty fixed content still permits followers: reconstruction initializes
them first. F74 clear removes that flag's royal follower declarations, while other follower flags
and `PLAYER_TYPE` remain inputs. No player-control handoff precedes this init.

The init selector separates first arrival from later visits:

| Input | Confirmed selected content and return |
| --- | --- |
| F700 clear, with any F508/F705 | Run `cs_503A6`, then set F700 and return. The first scene does not fall through into the later selector. |
| F700 set, F508 clear | Return without either scene; this entry remains on Map73. |
| F700 set, F508 set, F705 set | Return without either scene; this entry remains on Map73. |
| F700 set, F508 set, F705 clear | Select later `cs_50806`, then set F705 and return. Its town-building content is outside this guide. |

F700 selects the scene; it is not proof that control was installed or that every service completed.
F74, F640, party readiness and completed Battle08 are not additional predicates on the F700-clear
branch. The [arrival/selector review](../../research/voyage-through-damaged-ship-control.md#map73-admission-and-init-selection)
owns these distinctions.

## 2. The Voyage Resets Stats and Keeps Kiwi Content Conditional

**Confirmed ordered content:** `cs_503A6` first resets ally battle stats, reloads the current Map73
with view operands `(0,0)`, rebuilds the voyage actors from `ce_506DE`, awaits Bowie's initialization,
copies a `22x10` block region and requests Town music/fade-in (0–8). Reloading the map here neither
changes the main actor's authored position nor dispatches another setup init.

`resetForceBattleStats` processes ally indices 0–29 without joined, active or alive filtering. It
assigns maximum HP/MP, retains only existing STUN/POISON/CURSE bits in an intermediate status mask,
then updates combatant stats. It does not add those bits; the update derives curse from equipped
items, so the mask alone does not establish final curse status. It can restore a zero-HP ally.
This differs from entry's living/immortal healing. It recruits nobody and changes no battle-party
membership. Handler-local reset evidence does not prove this voyage naturally ran.

Astral, Bowie, Sarah, the Minister and Jaha discuss escape and the mainland; narration sends the
ship east, and Sarah feels sick (9–20). F6 then selects optional Kiwi dialogue:

| Branch | Content consequence |
| --- | --- |
| F6 set | Kiwi attracts Sarah's attention and spots land; Sarah tells Astral, who addresses the King (21–28). |
| F6 clear | Jump to `cs_50426` at 29, skipping all eight lines, including the King exchange. |

The initial cursor 2239 is replaced with 2213 before the first text. Both paths reset it to 2229
at 29; later it resets to 2239 before arrival narration. Omitting Kiwi does not shift subsequent
dialogue identities. Neither branch joins Kiwi or writes F6. His optional recruitment belongs to
the [earlier Granseal guide](battle06-outcomes-through-battle07.md).

Astral and the Minister tell the captain to land despite damage and discuss timber reuse (29–43).
Scroll assignments, waits and quake requests stage the approach. The ship's speed/idle action is
awaited, its next Right-1 movement starts without a join, then Slade, Sarah and the captain react
to impact. `waitIdle 132` joins that movement before `stopEntity 132`, further waits and arrival
narration (44–55). A speed, scroll or wait operand does not measure observed displacement or duration;
the awaited steps still depend on entity/service updates completing.

## 3. Landing Replaces the Actors and Gives the Crowd Instructions

**Confirmed scene transition:** command 56 loads `MAP_SHIP_DAMAGED` (Map14) with view operands
`(18,25)` inside Map73's ongoing init. Command 57 rebuilds actors from `ce_5072E`; Bowie's init is
awaited, followed by explicit fade-in. The Minister calls for attention, proposes building a new
Granseal and orders ship timber moved ashore (56–120). **Inferred narrative meaning:** this is the
landing and salvage briefing, rather than a completed construction episode.

Twelve townspeople, references 129–140, receive staggered unjoined Right/Left action lists. After
the Minister's turns/waits, the script assigns the crowd's facing and installs repeated random-walk
actions without joins, then waits 80. Those actions and the wait do not prove timber delivery,
arrival at a destination or completion of the new town.

The three list loads establish separate reference contexts, not one permanent cast:

| Context | Authored main placement and relevant logical references |
| --- | --- |
| Voyage: `ce_506DE`, command 4, Map73 | Main `(63,63)`, Up. NPCs 128–131 are Astral, Minister, King and merchant/captain; 132 is the ship at `(5,5)`, Right. Sarah, Jaha, Kiwi and Slade use ally references. |
| Crowd: `ce_5072E`, command 57, Map14 | Main `(0,0)`, Up. 128 is the Minister at `(23,30)`, Down; 129–140 are townspeople. Reference 132 now identifies a townsperson. |
| Ship interior: `ce_5079E`, command 122, Map14 | Main `(40,8)`, Left. Fixed ally records name Sarah, Chester, Jaha, Kazin, Slade and Kiwi. NPCs 128/129 are workers, 130 a young man; 131–133 are three `MAPSPRITE_OBJECT2` records. Reference 132 is the middle object at `(45,6)`, Down. |

These are declarations, not observed final positions or force membership. `loadMapEntities`
clears entities, initializes followers, declares fixed records and declares main entity 0 before
sprite reload. Logical references resolve through the entity-index mapping. An already-mapped ally
is skipped, so F66/F72/F73 can make Sarah/Chester, Kazin or Slade use a follower placement instead
of their fixed record. F65 adds the overworld follower prefix on Map73; Map14 uses the ordinary
follower list. Clearing F74 does not normalize these inputs, and the script does not normalize
`PLAYER_TYPE`. Declaring Kiwi's sprite does not recruit him or set F6.

The [entity/reset review](../../research/voyage-through-damaged-ship-control.md#force-reset-and-entity-list-semantics)
owns list identities and follower counterexamples. Required actor mappings, natural motion and
the actual force/party state remain **Unknown** at this caller.

## 4. Jaha's Plank Scene Changes Layout and Presentation

**Confirmed ordered content:** the script loads Map14 again with view operands `(45,2)`, rebuilds
the ship-interior list and awaits Bowie's init (121–123). This explicit load reloads the layout
before the following temporary copies; camera/view operands are separate from main placement.

| Phase | Scripted content and bounded state effect |
| --- | --- |
| Prepare the scene (124–131) | Save block `(51,7)` to `(63,63)` and copy `(51,6)` to `(51,7)`, both `1x1`; stop animations of objects 131–133. F6 set jumps to `cs_50624` at 131; F6 clear hides Kiwi at 130 first. Both paths fade in. |
| Ask Jaha to help (132–147) | Await Jaha Down 1; he objects to dismantling the ship and Sarah asks him to help with a plank. Await Down 2, face Down, use a pose sprite and wait. After effort dialogue, await disabling auto-facing and a custom speed `(64,64)`/idle action. |
| Stage the mishap (148–157) | Restore the saved block to `(51,7)`, request falling SFX, await Jaha Up 4, request quake 5/door-open SFX, wait and hide Jaha. Copy `(56,0)` size `1x2` to `(51,3)`, stop quake and wait. |
| Direct Bowie onward (158–168) | Slade comments and moves Right 2 with a wait; Sarah comments. Camera destination becomes `(34,4)`; Kazin asks Bowie to bring the plank above deck. Set Bowie's priority operand to 0 and reference 132's to `$FFFF`, then end. |

**Inferred staging meaning:** the dialogue, object placement and block/sprite changes depict plank
handling and Jaha's mishap. The operations themselves have narrower meanings:

- `stopEntity` writes animation counter -1; it is not a general motion-completion barrier.
- `hide` moves an entity/destination off-map, invalidates matching entity-index mappings and removes
  a matching exploration follower entry. Hiding Jaha changes no combat HP, force membership or
  battle-party membership; it does not prove death or departure from the force.
- Block copies mutate the working map and mark view planes for update. `(63,63)` is a temporary
  layout cell, not an inventory slot. No item acquisition or transfer is supplied by these copies.
- `setPriority` resolves the current entity and writes 0 for a zero operand, 1 for any nonzero
  operand. The final pair therefore concerns Bowie and the middle interior object, not the voyage
  ship or crowd actor that previously used 132. Source layer names do not prove rendered overlap.

No player-controlled carrying, warp request, story-flag write or second init follows in this script.
Bowie has no move after his authored `(40,8)`, Left placement. Actual actor positions, service/wait
termination, object interpretation and visible/audio results remain bounded by the
[complete-script review](../../research/voyage-through-damaged-ship-control.md#complete-first-arrival-script).

## 5. Map14 Is Current When the Map73 Caller Returns

**Confirmed load boundary:** both Map14 loads occur inside the original Map73 init call.
`loadMapFadeIn` loads map resources/properties and restores display/interrupt processing; the
following commands separately construct entities and request fade-in. These loads perform no
MainLoop substitution/encounter scan, setup-population selection, setup temporary-flag clear or
`RunMapSetupInitFunction` call. Map14's flag/step-copy tables contain only terminators.

Map14's default init has an F703-controlled `cs_58FA4` call and later EGRESS_MAP write. Arrival
through this script does **not** run that init or those effects, and no ordinary Map14 population
replaces `ce_5079E` on return. Being on Map14 is not proof of ordinary Map14 entry.

At script end, cleanup conditionally waits for view scrolling if a dialogue window remains, clears
scrolling speed and restores registers. The Map73 init then sets F700 and returns. Register
restoration does not restore the former current map or actors: exploration continues on Map14.
Its palette-conditional music/fade path, `SetMoveSfx` and `WaitForEvent` follow. Town music and
the two named SFX are requests, not proof of final audible state or completed services.

The first `WaitForEvent` read of `MAP_EVENT_TYPE` is the stopping boundary:

| Pending word at that read | Confirmed conditional consumer |
| --- | --- |
| Zero | Target the camera at main entity 0 and install its controlled action. `PLAYER_TYPE=0` selects character control; other types select vehicle branches. The secondary loop reads pending events again before A/C input. |
| Nonzero | Skip camera/control installation and enter the secondary read. A surviving event returns for `ProcessMapEvent`, which clears the word and dispatches it. No movement-ready frame is established. |

The earlier entry clear and lack of an explicit warp in `cs_503A6` do not prove this read zero:
VInt-driven actors/services can run during waits. F700 is written **after the script, before this
conditional control consumer**. Its source comment cannot certify restored player control.

Subsequent event lookup uses current Map14 data. `GetCurrentMapSetup` selects its default unless
F458 selects `ms_map14_flag458`; those setups have distinct entity/zone pointers and shared
area-description/item pointers. This identifies future event owners without invoking a new init
or consuming their player-triggered content. The
[caller/control review](../../research/voyage-through-damaged-ship-control.md#in-script-loading-caller-return-and-first-control-read)
retains this boundary. Onward walking, plank choice and Battle08 admission are outside this guide.

## Evidence Review and Remaining Limits

The pre-synthesis review read accepted #728, the departure predecessor/guide, relevant contracts,
selected tracked fixtures and their schema/verifier boundaries. Research owns pinned-source/text,
complete inline-script/list review and retained comparison results. This guide adds no source
investigation, extraction, comparison rerun or runtime observation. The commands below identify
existing reproduction owners; they are not newly executed checks.

| Relation | Reviewed executable owner and reproduction route | Disposition |
| --- | --- | --- |
| Arrival admission and pending/control | [common-maps fixture](../../../tests/fixtures/h2/common-maps-static-v1.json), `sf2-common-maps-static-v1`; [battle-global](../../../tests/fixtures/h2/battle-global-data-static-v1.json), `sf2-battle-global-data-static-v1`; [request consumption](../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json), `sf2-map-event-request-consumption-static-v1`. Commands `uv run sf2 h2 common-maps`, `battle-global-data`, `map-event-request-consumption`. | Separate table/consumer facts agree with accepted Map73 admission and conditional event reads. They do not prove request survival, arrival or a zero pending word. |
| Init, setup and actors | [init fixture](../../../tests/fixtures/h2/map-init-static-v1.json), `sf2-map-init-static-v1`, primary body 79; [verifier](../../../src/sf2tool/h2/map_init.py), [schema](../../../schemas/h2-map-init-static-fixture.schema.json); [setup](../../../tests/fixtures/h2/map-setup-static-v1.json), `sf2-map-setup-static-v1`; [entities](../../../tests/fixtures/h2/map-entities-static-v1.json), `sf2-map-entities-static-v1`. Commands `map-init`, `map-setup`, `map-entities` under `uv run sf2 h2`. | Init's two calls and F700/F508/F705 branches agree. Generic setup/entity facts do not establish this caller's follower mappings, party state or completed waits. Inline lists use accepted research. |
| Script operations and their order | [script fixture](../../../tests/fixtures/h2/map-script-engine-static-v1.json), `sf2-map-script-engine-static-v1`; [verifier](../../../src/sf2tool/h2/map_script_engine.py), [schema owner](../../../src/sf2tool/h2/map_script_schema.py). Command `uv run sf2 h2 map-script-engine`. | Selected `cs_503A6` sites retain reset 1, reload 2, list loads 4/57/122, Map14 loads 56/121, block copies 6/124/125/148/155 and lifecycle/priority sites. Public command-family facts are narrower than the retained complete program: F6 targets, cursor resets and inline action order use accepted research's complete review. No caller-composed observation is added. |
| Current-map event routing | [map-content](../../../tests/fixtures/h2/map-content-static-v1.json), `sf2-map-content-static-v1`; [routing shard](../../../tests/fixtures/h2/map-events/routing-setup.json), `sf2-map-events-static-v1`; [shard/schema owner](../../../src/sf2tool/h2/map_events_fixture.py). Commands `uv run sf2 h2 map-content`, `map-events`. | Map14 data/setup lookup identifies future owners, not natural activation, a fresh init or an executed plank choice. |

The [map contract](../contracts/map-exploration.md#load-and-selection-order) describes load/selection
order; its conceptual init step applies to the surrounding ordinary exploration path. It must not
be read as an init call made by every `LoadMap` service. Accepted research's explicit in-script
consumer is the narrower boundary here. The
[setup-data contract](../contracts/map-setup-data.md#contract-boundary) preserves static pointer
identity, while the [party/script contract](../contracts/party-roster-state.md#handler-boundary)
keeps reset, recruitment and actor operations distinct. No contract rewrite or evidence conflict
requiring a broader claim follows from this synthesis.

**Unknown, preserved:** natural departure/arrival lineage, legal walking, required follower/actor
mappings and party state; actual positions and completed waits/motion; fade, quake, scroll, sprite
overlap and audio results/timing; pending-event value/overwrite at the handoff; and save/load
persistence. A future composed observation would need its named caller, F700/F6/follower/player-type
inputs, selected script branch, current map/entity mappings, F700 write and first pending/control
consumer. F700-set, F6-clear, conflicting followers, nonzero pending and alternate player type are
useful controls, not an automatic runtime queue. Original facts, inferred narrative meaning and
remake choices remain separate; this segment completes neither the campaign nor the design epic.
