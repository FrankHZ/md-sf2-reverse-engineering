# Battle13 and Polca’s raft

This chapter continues the [Dark Caves and Bedoe campaign](dark-caves-bedoe.md) from the same party’s eastern Polca save. High rigor applies to the state carried across battle return, story choices, map entry and Save/Continue. The playable boundary is the first ordinary water step after boarding.

## Play

In the prepared checkout, run one of these commands from the repository root, choose **Continue**, then **slot 1**. Use `en` for English or `zh-CN` for Chinese.

| Command | Starting position and controls |
| --- | --- |
| `./local/issue1738/play.ps1 zh-CN Battle13` | Map13 (30,6), facing west. Walk six cells west and advance the confrontation. Defeat the enemy leader while keeping Bowie alive. |
| `./local/issue1738/play.ps1 en Return` | Map13 (16,12), after the victory. Enter the mayor’s house and speak to him four times. Leave south and answer Peter. Refusing the first request leads to Oddler’s own appeal farther south. |
| `./local/issue1738/play.ps1 zh-CN Raft` | Map72 (43,47), after Luke joins. Press Down once and let the party finish boarding. Press Down again to reach (43,49). |

Each language and entry uses a separate player namespace. Its first launch copies a real field save. Later launches resume the player’s slot. For a fresh independent check, run `./local/issue1738/check-route.ps1 en Raft` or `./local/issue1738/check-route.ps1 zh-CN Battle13`. The former reads the main scene before input, after boarding and after the first water step. Both observers use ordinary Continue and keyboard events.

## Observed in the remake

- **Confirmed** — The starting party carried 7,413 gold, with Chester and Slade dead. Defeat restored Bowie and returned to Bedoe (27,35) Up with 4,761 gold after earnings and halving. F413 remained set, with F513, F75, F716 and F64 clear.
- **Confirmed** — The winning party returned to Polca (16,12) Down with 9,753 gold. F413 cleared, F513 and Luke’s follower flag F75 were set, and neither the raft berth nor F64 or Luke’s membership was present. The first three conversations with mayor137 advanced his temporary flags. Returning from the fourth conversation set F716, F64 and the stored berth on Map72 (43,48).
- **Confirmed** — The Chinese route accepted the first Oddler request. An English replay refused the first and second requests. The second refusal played the longer appeal before the shared conclusion. At the first refusal F75 remained set. On departure F68, F717 and F718 were set and F75 was clear. Luke joined during Map72 init. His join committed F10 and F42 before the final exit line, movement, hide and F416. The immediate join projection still listed Luke in reserve. Save/Continue rebuilds the force lists from the stored membership and active flags. The save retains the eleven actor records including Luke’s level, HP, equipped middle sword and the party’s existing casualties.
- **Confirmed** — Luke’s scene returned control at (45,47). From (44,47), Down toward a different water cell left the player on foot. From (43,47), Down aligned with physical slot 31 at (43,48). After boarding, the player displayed raft sprite 61 and mode 2. The next Down reached (43,49) at player input with the same party resources, gold and step counter. The stored berth stayed at (43,48). The separate raft actor and listed followers moved off-map. Mode 1 belongs to the Caravan.
- **Unknown** — Hardware-exact animation, sound timing and human play feel. The field save offered at the berth covers walking before boarding. Disembarkation, cross-map navigation, Battle14, Kraken and later Caravan travel are outside this chapter.

## Source and preparation

The original predicates and script operands come from the [Battle13 outcome and mayor](../../../docs/research/chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md), [Oddler choices](../../../docs/research/chapters/eastern-parmecia/polca-oddler-choice-and-following.md), [Luke’s departure](../../../docs/research/chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md) and [raft controller](../../../docs/research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md) owners. The private world uses the pinned US ROM and disassembly identified there. The campaign selection admits the F513 setup, Battle13 aftermath and Bedoe egress, Luke’s starting growth, and the source mayor and departure programs. The field controller compares adjacent physical destinations before running the boarding actions. Riding uses the active area, current working-layout water mask and current physical obstructions.

Reuse the predecessor’s encounter inputs and reviewed audio, then run the chapter’s local preparation and English/Chinese review commands in `local/issue1738/HANDOFF.md`. The three private save files originate from the ordinary game save UI. Regenerate the selected world and catalogs with the tracked producers and campaign selection. Private saves, images, audio, source text, generated packages and input receipts stay local.
