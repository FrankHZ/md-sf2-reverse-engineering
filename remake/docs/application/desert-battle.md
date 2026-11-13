# Battle14: Southeast Desert

From the [Hassan Continue](kraken-hassan.md), the party can sail across the berth east of town and walk into the marked desert on Map70. This encounter uses the carried party, equipment, resources and seeds. It does not gate Kraken, the East Shrine warp or taking the sword. The dependency on this particular approach is geographical.

## Where the encounter starts

**Confirmed** in the running remake: from Hassan (8,9) Up, walk D3 R4 U12 to Map69 (26,54). R D R boards the raft at (28,55); R lands at (29,55). Walk R11 U3 R3 U2 R2 U2 R5 U R13 to scroll to Map70 (1,47). Walk R14 to (15,47); one more R triggers Battle14. Scrolling itself does not select a battle.

The [original encounter and outcomes](../../../docs/research/chapters/southern-travel-creed/battle14-desert-encounter-and-outcomes.md) own the USA retail and pinned SF2DISASM evidence. On a marked cell, X16–19 selects the random check; other cells need not request 14. With F514 clear, the check sets F414 and the step counter to 30000. The map checks Battle10 before Battle14. Holding F410 still selects 10 even when F414 is set. Battle14 has no local before/start/after story or enemy leader. The Map70 crop starts at (15,26); its twelve allied formation places, fifteen starting enemies, two quadrilateral regions and one triangular region come from the pinned placement. Battle scenes select backgrounds by terrain.

## Win, retreat and retry

**Confirmed** with ordinary Godot keys from the Hassan party after eight paid Raises at the priest (980 gold; all eleven alive; 8,943 gold). F10 field Save retained the previous Egress register, New Granseal. Each independent attempt used a new copy of that slot. The observations below describe the remake, not a recording of the original.

| Player action | Observed result |
| --- | --- |
| Win by clearing the enemy force | In round 27 May dealt 6 to the last Golem and received 37 EXP; the last kill yielded 240 gold. The party returned to Map70 (16,52) Down with 11,983 gold, F464/F514 set, F414 clear and 10,000 steps. The return was Bowie's final local position (1,26) plus the crop origin. The earlier loss of Peter did not prevent his normal field recovery. Bowie, Peter and May had 12, 34 and 24 HP; eight other members remained down. No Power Stick was awarded: the original base Master Mage carries an Iron Rod. |
| Lose Bowie | One attempt ended in defeat after 99 player decisions. The party retained its earned rewards before the half-gold charge and returned to Map6 (11,22) Up with 5,261 gold; F514 remained clear. |
| Cast EGRESS, then re-enter in the same session | Bowie spent 8 MP. Gold stayed at 8,943. Map6 (11,22) Up, F414/F464 set, F514 clear. F64 reset the raft to the default berth, Map69 (42,15). Walking via Maps73 and 72, boarding that raft and entering the marked desert led to fresh player control. A live Hassan register would select Hassan (8,9) Up and Map69 (28,55); it is not forced by this encounter. |

## Save and the East Shrine handoff

**Confirmed:** field Save and a fresh title Continue restored the victory position, roster, flags, berth and cooldown. From (16,52), `RRRRRRRRRRRRRRURURURURRURRURURURURRRUUU` reached Map62 (6,18) Up in 39 ordinary inputs. There was no intervening battle. This provides the [shrine continuation](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1804) a reproducible start; the subsequent conversation and Battle15 belong there.

On completed visits, a marked step with nonzero counter declines. Accepted walking steps decrement a positive counter; entering the exploration loop subtracts 20000 and clamps a negative signed result to zero. Scrolls do not subtract. At zero the range-8 draw requests on zero; otherwise a range-4 draw yields a 2–5 step delay. From the shrine one Down returned to Map70 (43,41) with counter zero. Walking west and south into the marked zone selected a completed Battle14 with F514/F464 retained, F414 cleared and first control on Peter. Bowie's level was 1. The remake admits the base formation for such repeats. At unpromoted Bowie level 24 or above, a completed encounter requires source enemy substitution; the current importer has no authenticated substitute formations and stops with `repeat-enemy-upgrade`. Promoted classes, substituted equipment/drops and exact original presentation are outside this chapter.

## Reproduction

Use the registered private ROM and pinned source with the [private input setup](../../../docs/operations/local-private-inputs.md). The [campaign selection](../../content/private/pre-voyage-opening.json) and [encounter reader](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) own the playable row and its source pins. Produce the encounter, action scenes and field poses into new ignored worktree-local destinations:

```powershell
uv run python -m sf2tool.remake_exploration_battles --battle 14 --rom $rom --upstream $upstream --output local/desert/inputs/battle14
uv run python -m sf2tool.remake_battle_scene_content --battle 14 --rom $rom --upstream $upstream --output local/desert/scene
uv run python -m sf2tool.remake_battle_scene_content --field-death-base local/desert/scene/battle-scenes.json --rom $rom --upstream $upstream --output local/desert/field
```

Bind these three operands and the field scene as an additional private encounter in the ordinary [campaign package](kraken-hassan.md), mounting its existing command-menu, battlefield-menu and identity icons. Regenerate the world with the campaign selection and reuse the unchanged chapter audio and presentation assets. The prepared reviewer checkout has `./local/issue1809/run.ps1 win <new-sample>`, `restore <same-sample>` and `egress <another-sample>`. Its ignored handoff includes the reproducible route, read-only post-victory slot and observed failure logs. No controlled battle start, map edit, flag edit or screenshot is required.
