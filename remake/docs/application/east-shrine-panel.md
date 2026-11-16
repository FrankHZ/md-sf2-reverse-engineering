# Hassan to the East Shrine and the Wooden Panel

The chapter starts at the [Hassan Continue](kraken-hassan.md) with eleven joined members, three alive, 9,923 gold and the raft parked on Map69 at (28,55). Acceptance requires the same party to visit the old man on Map62, enter Battle15 through the shrine, recover from a failed or escaped attempt, win, collect the Wooden Panel and retain it across Save and Continue. [Chapter agreement](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1804)

## Route from Hassan

**Confirmed** through ordinary English and simplified Chinese title Continue and movement on the Hassan package.

1. From the church (8,9), walk Down 3, Right 4 and Up 12. The exit lands at Map69 (26,54).
2. Right, Down, Right boards the raft. Right lands east of the berth at (29,55).
3. Walk Right 11, Up 3, Right 3, Up 2, Right 2, Up 2, Right 5, Up 1 and Right 13. Enter Map70 at (1,47).
4. Walk Right 14 to (15,47). The next Right requests Battle14 at (16,47) with F414/F510/F516 held and F514 clear. The earlier Hassan-only package stopped there with `battle-entry-unavailable`. [Desert encounter dependency](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1809)

## The ordinary post-desert return

**Confirmed** with the untouched field save from the [accepted desert encounter](desert-battle.md). Title Continue placed the eleven-member party on Map70 (16,52) facing Down with 11,983 gold. Walking 39 steps reached Map62 (6,18). Two conversations with the moving old man set F728, F77 and F729. The system menu saved in the East Shrine. A fresh Continue restored the same roster, HP, inventory, gold, names, item custody, Egress register and raft berth.

From this saved anchor, Down exits to Map70 (43,41). The return crossed to Map69, boarded the parked raft at (28,55), landed south of the shrine and reached Map61 in 131 inputs. The observed crossing did not roll another Battle14. In a separate run from the same saved anchor, walking back and forth at Map70 (17,47) triggered completed Battle14 on input 64 with F77, F729 and F514 held. Walking to (5,9) opened the gate, set F415 and F730, and copied the warp word 0x1004 to (5,6). Three Up inputs from the gate requested Battle15. The intermediate field-only package returned the ordinary unavailable-encounter receipt at (5,7). The synchronous result identifies `battle-15`.

To prepare the same party for the next battle, another branch of the East Shrine save walked back to Hassan and used the ordinary church Raise service. Eight offers cost 1,050 gold. Its town save retains the old man, all eleven living allies, 10,933 gold, the original berth and Egress Map6.

## Battle15, escape and the Hall save

**Confirmed** on the ordinary campaign with English battle input and a fresh Chinese Continue. With the church-prepared party, walk from Hassan to the original raft, sail north, land below (43,31) and enter Map61. Face the gate from (5,9) and walk Up three times. The Map50 introduction leads into the 32×24 battlefield. Fifteen enemies are on the field. The sixteenth slot starts off-board, uninitialized, with zero maximum HP and four empty item slots.

In the observed winning battle, the party advanced toward the northern Arrow Launcher. Peter defeated it at (14,5) in round three. Other attack plans ended in ordinary leader defeats and returned to Map6. Victory played 2506–2512, cleared F415 and F77, set F515 and F732 and returned field control at (27,11) facing Up. Walk to (14,3), face the chest to the north, and choose Search. Bowie received the Wooden Panel in slot three. Save from the system menu and choose Continue after restarting. The hall save retained 11,463 gold, eleven party records (ten living), Bowie's Panel and the raft on Map69 (43,33). A second Search in Chinese was empty with F152 still set. The savepoint covers the shrine side of Map50; the separate Ribble-side area has its own progression.

**Confirmed** on a separate Chinese branch from the ordinary East Shrine save. Bowie cast Egress for 8 MP. The party returned to New Granseal at (11,22) with the same 11,983 gold and F415 held. Walk out through Maps73 and 72, board the reset raft on Map69 at (42,15), then enter Map61 (136 inputs in the observed return). The opened gate admitted Battle15 again, skipped its completed introduction and presented the next player movement. The adapter selected the battle music from Map50's bound battle area.

## Shrine content and engine boundaries

| Location | Confirmed in the prepared private field package |
| --- | --- |
| East Shrine, Map62 | Approach the moving old man (logical 129) and talk twice. The first talk sets F728. The second leaves F77 and F729 held. The party receives a field follower. |
| Shrine exterior, Map61 | With the old man following, walking to (5,9) runs the opening. Its layout change opens (5,6), its story command sets F415, and the zone caller sets F730. The Map69 entrance is (43,31), arriving at (5,15). |
| Hall, Map50 | The first victory's after-program ends at (27,11) facing Up, clears F77 and sets F732. In a controlled field run with the battle already cleared, the explicit return and a walk to (14,3) reached the chest at (14,2). |
| Wooden Panel | Search from the south facing Up grants item 112 to the first member with a free slot and records F152. Bowie received it in the English and Chinese field observations. A second English Search returned empty and retained the original item. |

**Confirmed** in the prepared Chinese field package: the Map61 gate displayed rows 3642–3644, the Battle15 introduction displayed 2503–2505, and the first-victory departure displayed 2506–2512. Each row resolved in Chinese with no display fallback. The next field Search named Bowie and the Wooden Panel. The encounter was unavailable in this preparation, so its three additional enemy name rows were excluded. Adding an unknown enemy name makes the runtime reject the whole overlay.

The Map61 opening and Battle15 departure use the source follow command. It checks the high byte of the follower operand before resolving either field entity. For the selected Peter and follower operands this checks Bowie's HP. A zero-HP Bowie skips the follow and the script proceeds to its subsequent flag writes. The public engine tests vary HP and entity availability independently.

## Optional battle chest

**Confirmed** in the remake. During Battle15, finish movement at (2,9) facing Up. Select Search
in the battle menu and confirm. Peter opened the chest in round five on the ordinary Hassan route.
The map changed its chest block from 1 to 2 and set F151. The Golem in enemy slot 143 started
offscreen with zero maximum HP and empty hands. Search reset it on its deployment tile (2,8)
with 24 HP and Power Water (item 9). The scene showed the Golem attacking Peter at (2,9);
his HP went from 24 to 17. After-turn effects use the opener's queue entry.

This entrance is an intentional remake correction. The US battle menu's five-byte indexing of
seven-byte records cannot match the F151 row from Battle15. The remake uses the admitted Map50
placement and chest marker. The original trap handler's exact activation word, region checks,
deployment reset, attacker/target and enemy drop table govern the downstream behavior.
[USA ROM offsets and original caller](../../../docs/research/chapters/southern-travel-creed/battle15-shrine-approach-and-outcomes.md#chest-triggered-spawn-and-power-water-drop)

**Confirmed** through Save, fresh Continue and ordinary battle actions. May shot the Golem
from (2,10). In round six Peter attacked from (1,8), killed it and received Power Water.
The enemy's inventory emptied, Peter's first slot held item 9 and the drop register gained
bit 29. After saving and continuing again, May searched the open chest. It was empty and
Peter kept the single item. F152 stayed clear throughout this battle.

| Directed condition on a separate copy of the battle save | Result under ordinary input |
| --- | --- |
| Golem still occupies the faced chest | Search reports the occupant and returns to the command menu. |
| Golem at 1 HP, May has space | May kills it, receives item 9, and bit 29 is set. |
| Same attack with May's four slots filled | Bit 29 is set, the Golem loses item 9 and Deals stays empty. |
| Same attack with bit 29 already set | No second receipt or removal. |
| Arrow Launcher at 1 HP in May's range, Golem alive | Leader defeat clears the enemies without a Golem item receipt. The shrine after-program returns to the field. Search at (14,2) grants the Wooden Panel and sets F152 alongside F151. Chinese Continue then finds the Panel chest empty. |

A changed activation word, an assigned region or an already initialized Golem skips the trap
predicate. When the enemy slot exceeds the battle's enemy count, reset leaves the enemy offscreen. The
remake retains the opened receipt and reports the attempted offscreen attack as unsupported. **Unknown** includes original presentation timing and the outcome of
attempting to play that invalid original scene. The remake uses the ordinary result panel and
physical scene. The trap-specific original music and spawn choreography are outside this binding.

### Replay the trap

Use the assigned checkout's ignored `local/issue1814` samples. The runner copies a seed into a
fresh namespace beneath the registered disposable save root. The retained keyboard driver writes each sample’s inputs, battle decisions, native state and effects. Use distinct sample names on each replay.

```powershell
. ./local/issue1759/env.ps1
$trap = Join-Path $repo 'local/issue1814'
$slots = Join-Path $env:SF2_SAVE_ROOT 'issue1814'
$env:TRAP_TRIAL = ''
& "$trap/run.ps1" win review-open en -Seed "$trap/priest.json"
$env:TRAP_TRIAL = 'pursuit'
& "$trap/run.ps1" trial review-kill en -Seed "$slots/issue1814-campaign-en-review-open/sf2-opening/slot1.json"
$env:TRAP_TRIAL = 'natural-empty'
& "$trap/run.ps1" trial review-empty en -Seed "$slots/issue1814-campaign-en-review-kill/sf2-opening/slot1.json"
```

`seeds.py` derives the directed cases from the retained post-trap save. Use
`TRAP_TRIAL=kill`, `full`, `marked`, `occupied` or `sweep` with the matching seed and the
same `trial` command. The ordinary approach required north-facing movement: ending on
(2,9) facing west offered Stay. Earlier samples also reached Bowie's ordinary defeat.
The host refused the initial in-checkout save root. Completed native samples retained their
Godot ObjectDB shutdown warnings.

## Reproduction

Use the retained local inputs in the assigned checkout. Each new sample has a separate disposable save namespace. The elder and Egress commands use the ordinary post-desert and East Shrine slots. The raise command follows the actual walking route to the priest. The win and Egress commands start from copies of those saved games.

```powershell
. ./local/issue1759/env.ps1
$root = Join-Path $env:SF2_SAVE_ROOT 'issue1804-campaign-en'
./local/issue1804/campaign.ps1 elder review en
./local/issue1804/campaign.ps1 raise review-party en -Seed "$root-review/sf2-opening/slot1.json"
./local/issue1804/campaign.ps1 win review-win en -Seed "$root-review-party/sf2-opening/slot1.json"
./local/issue1804/campaign.ps1 panel-restore review-panel zh-CN -Seed "$root-review-win/sf2-opening/slot1.json"
./local/issue1804/campaign.ps1 egress review-egress zh-CN -Seed "$root-review/sf2-opening/slot1.json"
```

The private operator handoff retains the generated package, catalogs, the winning slot and original failed runs. The field-only before/after fixtures remain available for checking individual lines. Native shutdown sometimes reported leaked ObjectDB instances and the earlier detached gate observer sometimes failed during finalization after the gameplay receipt. The accepted win, fresh Continue, and paid Egress/re-entry each exited successfully. Engine checks cover the source follow guard, hidden population and hall save selection in `ShrineFollowerTests`, `HiddenSpawnTests` and `ShrineSaveTests`.
