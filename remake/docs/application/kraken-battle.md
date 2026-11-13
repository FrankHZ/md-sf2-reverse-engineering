# Battle16 on the Kraken raft

Battle16 uses the ordinary battle loader, map viewport, initiative, player commands and outcome return. The private content selects the retail Map49 ship and river, twelve ally formation positions, and eleven enemies. The preceding [river route](south-parmecia-raft.md) reaches the encounter trigger. The independent battle comparison starts on Map49 at (15,16) Up, immediately before battle selection.

## Play in the prepared checkout

Run `./local/issue1798/play.ps1` from the repository root. Use the arrow keys to move and Enter to accept a position. Use Attack and select a target, or use Magic to heal or cast Egress. The four living members of the Polca berth party start together. Ground troops can cross the gangways and decks. Peter and Luke can fly over water. Kraken limbs can move through water and attack allies on the deck from within their own range.

One winning route with this party keeps Bowie at (15,15). Sarah moves to (16,15) and heals herself as needed. Peter flies east along row 18, then north along column 26. Luke flies north from the deployment area and approaches along row 7. Peter attacks from (26,10) and Luke from (26,8). Party resources, enemy movement and initiative determine later choices.

## Independent observations

**Confirmed** in the running remake with normal keyboard commands and private source graphics. The controlled map-entry comparison reads the saved members, equipment, spells, conditions, counters, seeds and gold from the [Polca berth save](polca-first-raft.md). It supplies ordinary difficulty, a fresh roster-only battle and a three-map world. The comparison omits the introduction and uses a short post-victory warp.

| Observation | Result |
| --- | --- |
| First player input | Luke at (13,17). Bowie 12 HP, Sarah 30, poisoned Peter 32 and Luke 28. Seven other retained members had zero HP. The head had 35 HP, zero MP and two entries in the first initiative queue. Its two 48×48 field poses were mounted from selector 254. |
| Head kill with the saved party | Peter dealt 12 and 13 on his fourth-round attack. The head used AQUA for 11 damage. Luke dealt 1 and Peter dealt 13 in round five. The head reached zero with both arms and all eight legs at full HP. The common leader cleanup removed all eleven positions while retaining the surviving limbs’ HP. Field input resumed on Map69 (29,51) Left with 10,073 gold, F466 and F516 set and F416 clear. The comparison returned in walking mode. |
| Limb kill with a controlled party | Bowie, Peter and Luke had 100 HP and 80 defense, and both flyers had 80 attack. A normal attack killed one leg in round two. Subsequent player turns continued with the head alive, including three more leg kills before Luke killed the head. |
| Bowie defeated with the saved party | The defeat dialogue completed. Field input resumed on Map13 (18,25) Up with 4,876 of the initial 9,753 gold. F416 and F466 remained set, F516 remained clear and the recorded raft berth was Map72 (43,48). |
| Egress with the saved party | Bowie used Magic on his first turn. The return reached the same Polca position and berth with all 9,753 gold, F416 and F466 set and F516 clear. |

Run `./local/issue1798/observe.ps1 -Outcome Win`, `Limbs`, `Defeat` or `Retreat` for a fresh headless observation. The ignored handoff identifies inputs, prepared outputs and the retained failed runs. The live checks read the HUD, actor resources, turn queue, spell delivery, death cleanup and field input. AQUA uses the remake’s functional bubble effect. Original presentation timing and full campaign travel are outside this comparison.

## Encounter resources and campaign binding

The [initialization and first-control research](../../../docs/research/chapters/southern-travel-creed/battle16-initialization-and-first-control.md) owns the ordering, per-slot SWARM thresholds, movement costs, innate ranges and turn rules. The [outcome research](../../../docs/research/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md) owns the Polca and South Parmecia returns. Counter eligibility follows the [observed counter helper](../../../docs/research/battle/runtime-rng-and-battle-math.md). The source baseline is US retail and SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`. The scene producer uses `specialsprites.asm` for the field head and the encounter’s fixed RIVER backdrop for physical attacks.

Use new ignored destinations under the current worktree. In the prepared environment, `$rom` selects the verified ROM and `$upstream` the pinned source checkout.

```powershell
uv run python -m sf2tool.remake_battle_scene_content --battle 16 --rom $rom --upstream $upstream --output local/kraken-scene
uv run python -m sf2tool.remake_battle_scene_content --rom $rom --upstream $upstream --field-death-base local/kraken-scene/battle-scenes.json --output local/kraken-field
```

The [connected Polca-to-Hassan chapter](kraken-hassan.md) consumes these encounter inputs. To reproduce the campaign binding:

1. Extend the existing encounter producer's supported selection and CLI choices with 16. Its source code is `VERSUS_KRAKEN` and its header is `[12, 11, 0, 0]`. Use the existing producer for placement, scene metadata and compressed terrain. The exact outputs match [the registered Battle16 pins](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs).
2. Append an ordinary private world encounter row. Use the three Battle16 operands, the existing authenticated static/enemy/gold inputs, and the Battle16 scene with the shared command, battlefield and identity icons. Paths are relative to the packaged world. Baselines come from the existing party growth selection. The private controlled party is only an input to the independent comparison.
3. Supply Map49 and the ordinary field sprites for Luke, the legs (74) and arms (123). Skip special selector 254 in the ordinary three-direction sprite extractor. The field head's two poses belong to the Battle16 scene. The prepared product reader admitted the campaign's eleven ally definitions and eleven enemies.
4. Move 16 from the unavailable encounters into the playable campaign selection. Bind the source before/start/after/defeated hooks, F416/F466/F516, fixed Polca egress and paid retreat. Use the actual incoming party and the common battle loader. Verify the first victory restores riding at Map69 (29,51) Left, then continue to the landing and Hassan. These steps belong to the [campaign integration issue](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1799).
