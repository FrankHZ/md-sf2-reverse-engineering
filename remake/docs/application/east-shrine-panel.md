# Hassan to the East Shrine and the Wooden Panel

The chapter starts at the [Hassan Continue](kraken-hassan.md) with eleven joined members, three alive, 9,923 gold and the raft parked on Map69 at (28,55). Acceptance requires the same party to visit the old man on Map62, enter Battle15 through the shrine, recover from a failed or escaped attempt, win, collect the Wooden Panel and retain it across Save and Continue. [Chapter agreement](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1804)

## Route from Hassan

**Confirmed** through ordinary English and simplified Chinese title Continue and movement on the Hassan package.

1. From the church (8,9), walk Down 3, Right 4 and Up 12. The exit lands at Map69 (26,54).
2. Right, Down, Right boards the raft. Right lands east of the berth at (29,55).
3. Walk Right 11, Up 3, Right 3, Up 2, Right 2, Up 2, Right 5, Up 1 and Right 13. Enter Map70 at (1,47).
4. Walk Right 14 to (15,47). The next Right requests Battle14 at (16,47) with F414/F510/F516 held and F514 clear. The Hassan package stops there with `battle-entry-unavailable`. [Desert encounter dependency](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1809)

## The ordinary post-desert return

**Confirmed** with the untouched field save from the [accepted desert encounter](desert-battle.md). Title Continue placed the eleven-member party on Map70 (16,52) facing Down with 11,983 gold. Walking 39 steps reached Map62 (6,18). Two conversations with the moving old man set F728, F77 and F729. The system menu saved in the East Shrine. A fresh Continue restored the same roster, HP, inventory, gold, names, item custody, Egress register and raft berth.

From this saved anchor, Down exits to Map70 (43,41). The return crossed to Map69, boarded the parked raft at (28,55), landed south of the shrine and reached Map61 in 131 inputs. The observed crossing did not roll another Battle14. In a separate run from the same saved anchor, walking back and forth at Map70 (17,47) triggered completed Battle14 on input 64 with F77, F729 and F514 held. Walking to (5,9) opened the gate, set F415 and F730, and copied the warp word 0x1004 to (5,6). Three Up inputs from the gate requested Battle15. The field-only package returned the ordinary unavailable-encounter receipt at (5,7). The synchronous result identifies `battle-15`.

To prepare the same party for the next battle, another branch of the East Shrine save walked back to Hassan and used the ordinary church Raise service. Eight offers cost 1,050 gold. Its town save retains the old man, all eleven living allies, 10,933 gold, the original berth and Egress Map6.

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

**Unknown** for the end-to-end remake route: Battle15 deployment and victory with this party, retreat or defeat and re-entry, and post-Panel Save/Continue. The separate before/after field runs supply their own starting maps and flags.

## Reproduction

In the prepared checkout, use a new sample name for each command. The field checks enter through the localization observation fixture. The Hassan route enters through the title.

```powershell
./local/issue1804/run.ps1 en review
./local/issue1804/run.ps1 zh-CN review
./local/issue1804/campaign.ps1 elder review en -Package shrine14 -Catalog shrine-cat03
./local/issue1804/campaign.ps1 gate review en -Package shrine14 -Catalog shrine-cat03
./local/issue1804/direct.ps1 elder review en
./local/issue1804/direct.ps1 exterior review zh-CN
./local/issue1804/direct.ps1 before review zh-CN
./local/issue1804/direct.ps1 after review zh-CN
./local/issue1804/direct.ps1 panel review en
```

The direct after-program check requires a field fixture with F415 clear. A prior run with F415 still set reached the unavailable battle on the final warp. The ordinary victory caller owns the F415/F515 writes and must be exercised from battle. The engine behavior checks are `ShrineFollowerTests` and `HiddenSpawnTests`. The selected private inputs, saves, generated battle scenes and observations stay local.
