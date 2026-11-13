# From the Polca berth through Kraken to Hassan

Continue the previous chapter's eleven-member party from Polca, defeat the Kraken, land the raft and save in Hassan. This chapter uses the ordinary title, field and battle inputs. The High-rigor boundary ends after a fresh Continue in Hassan accepts movement. [Chapter agreement](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1796)

## Play

In the prepared checkout run `./local/issue1799/play.ps1 en` or `./local/issue1799/play.ps1 zh-CN`. Choose **Continue → slot 1**. Each language has its own disposable save. The first launch copies the existing Polca berth save. Further launches reuse your progress. Supply a new sample name as the second argument to start again.

1. From Map72 (43,47), go Up into Polca. From (15,30), go Right, Up, Right twice, Up four times. Face the priest across the counter. Raise Jaha for 140 gold, decline the other raises and cure Peter's poison for 10. Return with Down four times, Left twice, Down twice to the bank.
2. Board with Down and follow the [river directions](south-parmecia-raft.md#play-from-the-berth). The final Left starts the Map49 Kraken scene. Advance the dialogue and use the battle menu. Peter and Luke can fly north of the deck and approach the head from above and below. Keep Bowie alive. Defeating the head ends the fight even with living limbs.
3. After victory the party is back on the water at (29,51) facing Left. Go Left, Down four times, Left twice. The first of those last two Left inputs parks the raft at (28,55) and lands the party at (27,55). The next enters Hassan at (14,1). Down moves Bowie to (14,2).
4. Open the system menu with F10, choose Save and confirm a slot. Restart and choose Continue. The Hassan savepoint places Bowie at (8,9) facing Up. Down moves to (8,10).

## Observed through ordinary input

**Confirmed** in English and simplified Chinese, from separate copies of the same unedited berth save.

| Boundary | Observation |
| --- | --- |
| Continue and Polca church | Eleven joined, four living, 9,753 gold. The paid services left five living and 9,603 gold. The same party boarded the raft. |
| First encounter | Map49 used F416 for admission. The introduction ran once and F466 was set at battle input. The program uses a reserved physical head slot, its two 48×48 poses, the cloned limb sprite slots, quake and rumble. Lines 2513–2519 across the encounter scenes were read in each selected locale. |
| Victory | Peter and Luke attacked the head. It reached zero in round six with ten limbs still alive. The leader outcome awarded 320 gold, cleared F416 and retained F466 alongside F516. The after-scene and ordinary return restored raft mode at Map69 (29,51). The former encounter tile at (28,51) was navigable. |
| Landing and first town input | The raft stayed on Map69 (28,55). Bowie and the two field followers resumed walking. Map9's default cast and conditional initialization ran. Bowie, Peter and Luke were alive after the battle. All eleven remained joined, with 9,923 gold. |
| Save and Continue | Both languages displayed the slot's Hassan destination. At the restored input, the full party records, joined/active/reserve lists, item custody, flags, gold, walking mode and raft berth matched the state saved in town. |

**Confirmed** on separate English runs from the original berth. Casting Egress spent Bowie's eight MP before the return. Polca field recovery restored his MP to eight. Gold stayed at 9,753. A leader defeat instead returned with 4,876 gold. Both used the fixed Map13 (18,25) Up return, kept F416/F466 set and F516 clear, and restored the raft berth at Map72 (43,48). Walking back, boarding and re-entering reached Battle16 movement again. The retry did not require another first introduction. The item-based retreat comparison belongs to the battle outcome rules. [Battle16 comparison](kraken-battle.md)

## Coverage and reproduction

The [original entry](../../../docs/design/synthesis/chapters/southern-travel-creed/south-parmecia-through-kraken-battle16-entry.md) and [outcome/landing contract](../../../docs/design/synthesis/chapters/southern-travel-creed/battle16-outcomes-through-hassan-entry.md) own the source flag, location, population and recovery rules. The package preserves Map69/70 conditional battle rows. Battles14/17 remain unavailable, and the Battle15 branch is outside the route. Map9 selects the F518 cast when applicable. Focused restore cases cover the merchant, Rohde, Rick and an unusable player alias. Hassan recruitment, shops, Search, side quests, Taros and Caravan progression have no chapter acceptance. **Unknown** includes natural original-game timing and audiovisual parity.

From the prepared checkout run the following commands with a fresh sample name per journey. The native observer sends mapped keys to the running game and checks the resulting state.

```powershell
./local/issue1799/observe.ps1 Win en review1
./local/issue1799/observe.ps1 Restore en review1
./local/issue1799/observe.ps1 Win zh-CN review1
./local/issue1799/observe.ps1 Restore zh-CN review1
./local/issue1799/observe.ps1 Retreat en retry1
```

The prepared checkout's `local/issue1799/HANDOFF.md` identifies private preparation inputs, retained observations and failure logs. Focused engine behaviors include `MapEntityLifecycleTests`, `OpeningRestoreTests` and `RaftTravelTests`. The full party, graphics, text, audio, saves and traces stay private.
