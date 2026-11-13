# Hassan to the East Shrine and the Wooden Panel

The chapter starts at the [Hassan Continue](kraken-hassan.md) with eleven joined members, three alive, 9,923 gold and the raft parked on Map69 at (28,55). Acceptance requires the same party to visit the old man on Map62, enter Battle15 through the shrine, recover from a failed or escaped attempt, win, collect the Wooden Panel and retain it across Save and Continue. [Chapter agreement](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1804)

## Route from Hassan

**Confirmed** through ordinary English and simplified Chinese title Continue and movement on the Hassan package.

1. From the church (8,9), walk Down 3, Right 4 and Up 12. The exit lands at Map69 (26,54).
2. Right, Down, Right boards the raft. Right lands east of the berth at (29,55).
3. Walk Right 11, Up 3, Right 3, Up 2, Right 2, Up 2, Right 5, Up 1 and Right 13. Enter Map70 at (1,47).
4. Walk Right 14 to (15,47). The next Right requests Battle14 at (16,47) with F414/F510/F516 held and F514 clear. The Hassan package stops there with `battle-entry-unavailable`. [Desert encounter dependency](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1809)

## Shrine content and engine boundaries

| Location | Confirmed in the prepared private field package |
| --- | --- |
| East Shrine, Map62 | Approach the moving old man (logical 129) and talk twice. The first talk sets F728. The second leaves F77 and F729 held. The party receives a field follower. |
| Shrine exterior, Map61 | With the old man following, walking to (5,9) runs the opening. Its layout change opens (5,6), its story command sets F415, and the zone caller sets F730. The Map69 entrance is (43,31), arriving at (5,15). |
| Hall, Map50 | The first victory's after-program ends at (27,11) facing Up, clears F77 and sets F732. In a controlled field run with the battle already cleared, the explicit return and a walk to (14,3) reached the chest at (14,2). |
| Wooden Panel | Search from the south facing Up grants item 112 to the first member with a free slot and records F152. Bowie received it in the English and Chinese field observations. A second English Search returned empty and retained the original item. |

The Map61 opening and Battle15 departure use the source follow command. It checks the high byte of the follower operand before resolving either field entity. For the selected Peter and follower operands this checks Bowie's HP. A zero-HP Bowie skips the follow and the script proceeds to its subsequent flag writes. The public engine tests vary HP and entity availability independently.

The Battle15 formation holds twelve ally positions and sixteen enemy rows. Enemy 143 is a hidden Golem at (2,8), with both trigger regions absent. Ordinary round population keeps it off the board with zero maximum HP. Its bare Power Water is carried inventory when a reset succeeds. The F151 battle trap, its immediate attack and the associated enemy drop are separate from the F152 Panel in field exploration. The Battle15 battle-search action has no private binding on this base. [Original encounter and outcome](../../../docs/research/chapters/southern-travel-creed/battle15-shrine-approach-and-outcomes.md) · [Original Panel custody](../../../docs/research/chapters/southern-travel-creed/ribble-tree-opening-and-achilles-sword.md)

**Unknown** for the end-to-end remake route until Battle14 and Battle15 enter the active campaign: the actual arrival at Map62 from this save, Battle15 entry and victory with that party, retreat or defeat and re-entry, and post-Panel Save/Continue. The separate field runs supply their own starting maps and flags.

## Reproduction

In the prepared checkout, use a new sample name for each command. The field checks enter through the localization observation fixture. The Hassan route enters through the title.

```powershell
./local/issue1804/run.ps1 en review
./local/issue1804/run.ps1 zh-CN review
./local/issue1804/direct.ps1 elder review en
./local/issue1804/direct.ps1 exterior review en
./local/issue1804/direct.ps1 after review zh-CN
./local/issue1804/direct.ps1 panel review en
```

The direct after-program check requires a field fixture with F415 clear. A prior run with F415 still set reached the unavailable battle on the final warp. The ordinary victory caller owns the F415/F515 writes and must be exercised from battle. The engine behavior checks are `ShrineFollowerTests` and `HiddenSpawnTests`. The selected private inputs, saves, generated battle scenes and observations stay local.
