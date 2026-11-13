# Taros Shrine and the first Caravan

Continue from the Hassan agreement with Rohde. Take the western exit, win Battle17 and follow the road into Map12. The first shrine battle has fourteen enemies. Taros stands on the impassable shrine tile at (12,8) and carries the Taros Sword. Its item action casts BOLT 2 without using MP. Select Equipment on Bowie's turn and equip the Achilles Sword before attacking Taros. Other allies can fight the ordinary enemies.

## Play in the prepared checkout

Run `./local/issue1807/play.ps1 en Shrine` or `./local/issue1807/play.ps1 zh-CN Shrine` and choose **Continue → slot 1**. Use `Hassan` for the original [Rohde checkpoint](ribble-sword-rohde.md) or `Caravan` for the shrine immediately after acquisition. A distinct final sample name starts a fresh save namespace. The [western road](hassan-taros-road.md) gives the route from Hassan.

## Observed in the running remake

| Boundary | Observation |
| --- | --- |
| Natural route | **Confirmed.** Hassan Continue started with 11,463 gold, Bowie level 1 and the unequipped sword in slot 3. Ordinary Battle17 attempts, priest services and successful combat retained the party and earned levels. Equipment changed the sword word from 61 to 189 and initially raised attack from 9 to 25. A Battle17 victory save brought level 22 Bowie and four other living members to Map12. |
| Shrine and combat | **Confirmed.** English and Chinese played introduction texts 2520–2527. Taros stood at (12,8) with 49 HP. A separate motionless Caravan appeared at (12,5). Taros used his equipped sword to cast BOLT 2 with zero MP. Selecting Bowie's old weapon produced the ineffective message and left Taros at 49 HP. With the Achilles Sword, Bowie dealt damage and killed Taros on round 33 while Sarah used HEAL 3. |
| Loss and EGRESS | **Confirmed.** Leader defeat returned to the retained Map6 savepoint, halved gold and kept the shrine pending. On a separate natural run EGRESS spent 8 MP, retained 5,424 gold and returned to Map6 (11,22) Up. Both kept F418 and F468, left F518 and F65 clear, and parked the raft on Map69 (42,15). After EGRESS the player crossed Map73 and Map72, boarded the raft on Map69, landed at (27,55) and re-entered Battle18 with F468 retained. |
| First victory and Continue | **Confirmed.** All fifteen after-battle texts (2528–2542) played in both languages. At field control, Map12 (11,8) Down, gold was 9,174. F65, F468, F518 and F725 were set. F84, F418 and Rohde's force flag F11 were clear. The eleven joined members and equipped sword remained. Fresh English and Chinese Continue restored the same walking position, moved Taros off-map, retained the raft on Map69 (28,55) and accepted a step south without repeating the scene. |
| Exit | **Confirmed.** Leaving the shrine reached Map68 (55,44) on foot with F65 set and the same raft berth. The [Caravan travel route](caravan-travel.md) continues from this exit through overworld boarding, the riverbed crossing and the far-bank save. |

The [entry contract](../../../docs/design/synthesis/chapters/southern-travel-creed/southwest-parmecia-through-taros-battle18-entry.md) owns the original formation, neutral and entry rules. The [outcome contract](../../../docs/design/synthesis/chapters/southern-travel-creed/battle18-outcomes-and-caravan-acquisition.md) owns the return position, follower flags and first-victory program.

## Private content and reproduction

Use `sf2tool.remake_exploration_battles.prepare_encounter` with battle 18 and the pinned source/verified ROM. Build its battle scene with `sf2tool.remake_battle_scene_content.build` and `add_field_death`. Bind the scene and formation in the campaign encounter list alongside Battle17. Generate the pre-voyage world and current Chinese catalog. The scene uses the source Taros field poses and Caravan sprite, and the earlier Parmecia ally scenes require the Achilles Sword selector when equipped.

The falling cue first failed at instruction 86 of the compiled after-program. The verified source music context is command 35, Timer B 189. The finite controlled export of command 88 retained 34,908 stereo frames at 44,100 Hz. Add its `SFX_FALLING_TIMER_BD` candidate to the private world inputs alongside the existing timer variants. Both languages subsequently passed this instruction. The local operator package and saves stay private.

The completed campaign registered 1,509 Chinese messages and 131 distinct name identities. The earlier candidate repeated spell identity 13 and the reader rejected that overlay. Fresh Chinese playback verified the corrected dialogue and Taros Sword item name. The rare Taros Sword drop was not observed on this route.
