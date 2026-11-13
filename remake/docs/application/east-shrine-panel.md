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

The Battle15 formation holds twelve ally positions and sixteen enemy rows. Enemy 143 is a hidden Golem at (2,8), with both trigger regions absent. Ordinary round population keeps it off the board with zero maximum HP. The chest tile has impassable terrain. Fresh initialization leaves its inventory empty until a successful reset supplies its Power Water. The F151 battle trap, its immediate attack and the associated enemy drop are separate from the F152 Panel in field exploration. The Battle15 battle-search action has no private binding on this base. [Original encounter and outcome](../../../docs/research/chapters/southern-travel-creed/battle15-shrine-approach-and-outcomes.md) · [Original Panel custody](../../../docs/research/chapters/southern-travel-creed/ribble-tree-opening-and-achilles-sword.md)

**Unknown** includes exact original audiovisual timing and the natural battle trap outcome. The accepted chapter ends with a usable hall Continue. The Panel’s subsequent use at the Ribble tree belongs to the next chapter.

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
